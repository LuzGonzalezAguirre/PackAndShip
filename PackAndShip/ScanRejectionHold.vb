Imports System.IO
Imports System.Text
Imports System.Text.Json

' Intent local durable: si no es posible registrar un rechazo en CCS,
' la estacion NO puede aceptar otro escaneo ni perder la incidencia tras reiniciar.
Public NotInheritable Class UnregisteredScanRejection
    Public Property EventToken As Guid = Guid.NewGuid()
    Public Property ContainerId As Integer
    Public Property RejectionType As String = ""
    Public Property SsiCode As String = ""
    Public Property ClientCode As String = ""
    Public Property Reason As String = ""
End Class

Public NotInheritable Class ScanRejectionHold
    Private Shared ReadOnly gate As New Object()
    Private Shared ReadOnly folder As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "SSI", "PackAndShip")
    Private Shared ReadOnly filePath As String =
        Path.Combine(folder, "unregistered-scan-rejection.json")

    Private Sub New()
    End Sub

    Public Shared Function Pending() As UnregisteredScanRejection
        SyncLock gate
            If Not File.Exists(filePath) Then Return Nothing
            Dim v = JsonSerializer.Deserialize(Of UnregisteredScanRejection)(
                File.ReadAllText(filePath, Encoding.UTF8))
            If v Is Nothing OrElse v.EventToken = Guid.Empty OrElse
               v.ContainerId <= 0 OrElse
               String.IsNullOrWhiteSpace(v.RejectionType) Then
                Throw New InvalidDataException(
                    "El rechazo local no es valido. Deten la estacion y solicita revision de TI.")
            End If
            Return v
        End SyncLock
    End Function

    Public Shared Sub Save(value As UnregisteredScanRejection)
        If value Is Nothing OrElse value.ContainerId <= 0 OrElse
           value.EventToken = Guid.Empty Then
            Throw New ArgumentException("Rechazo sin contenedor valido.")
        End If
        SyncLock gate
            If File.Exists(filePath) Then
                Throw New InvalidOperationException(
                    "Existe otro rechazo sin registrar. No puede reemplazarse.")
            End If
            Directory.CreateDirectory(folder)
            Dim temporary = filePath & ".tmp"
            Dim data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))
            Using fs As New FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None)
                fs.Write(data, 0, data.Length)
                fs.Flush(True)
            End Using
            File.Move(temporary, filePath)
        End SyncLock
    End Sub

    Public Shared Sub ClearIfRegistered(containerId As Integer)
        SyncLock gate
            Dim pending = Pending()
            If pending Is Nothing Then Return
            If pending.ContainerId <> containerId Then
                Throw New InvalidOperationException(
                    "No se puede borrar un rechazo de otro pallet.")
            End If
            ' Solo se debe llamar despues de confirmacion SQL del bloqueo.
            File.Delete(filePath)
        End SyncLock
    End Sub
End Class
