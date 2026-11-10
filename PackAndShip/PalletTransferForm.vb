Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

' Two scanned pallet codes. The server performs the entire transfer atomically.
Public Class PalletTransferForm
    Inherits Form

    Public ReadOnly Property DestinationCode As String
        Get
            Return _destinationCode
        End Get
    End Property
    Private _destinationCode As String = ""

    Public Sub New()
        InitializeComponent()
        AddHandler sourceInput.Leave, Sub() PreviewSource()
        AddHandler sourceInput.KeyDown, Sub(sender, e)
            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True
                PreviewSource()
                destinationInput.Focus()
            End If
        End Sub
        AddHandler destinationInput.KeyDown, Sub(sender, e)
            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True
                transferButton.Focus()
            End If
        End Sub
        AddHandler transferButton.Click, AddressOf ExecuteTransfer
        AddHandler Shown, Sub() sourceInput.Focus()
    End Sub

    Private Function GetSource() As PaSPackedContainer
        Dim code = sourceInput.Text.Trim().ToUpperInvariant()
        If code = "" Then Throw New InvalidOperationException("Scan the original partial pallet.")
        Dim found = PaSPackingData.FindContainer(code)
        If found Is Nothing Then Throw New InvalidOperationException("No se encontró el pallet de origen en CCS.")
        If found.Status <> "PARTIAL" Then
            Throw New InvalidOperationException(
                "El pallet de origen debe tener estado PARCIAL. Estado actual: " & found.Status)
        End If
        Dim part = PalletConfigurationData.GetParts().
            FirstOrDefault(Function(x) x.Id = found.PartId)
        If part Is Nothing OrElse Not part.BusinessUnitId.HasValue OrElse
           part.BusinessUnitId.Value <> SessionContext.BusinessUnitId Then
            Throw New InvalidOperationException(
                "El modelo de origen pertenece a otro cliente de estación. Transferencia bloqueada.")
        End If
        If found.PieceTotal <= 0 OrElse found.PieceTotal >= found.RequiredTotal Then
            Throw New InvalidOperationException("El pallet original no tiene una cantidad parcial válida.")
        End If
        Return found
    End Function

    Private Sub PreviewSource()
        Try
            Dim pallet = GetSource()
            summary.ForeColor = Color.DarkGreen
            summary.Text = "Origen: " & pallet.Code & " | PARCIAL" &
                Environment.NewLine & "ID de modelo: " & pallet.PartId &
                " | Piezas a transferir: " & pallet.PieceTotal &
                " / " & pallet.RequiredTotal
        Catch ex As Exception
            summary.ForeColor = Color.DarkRed
            summary.Text = ex.Message
        End Try
    End Sub

    Private Sub ExecuteTransfer(sender As Object, e As EventArgs)
        Try
            If ConnectionRecovery.Pending() IsNot Nothing Then
                Throw New InvalidOperationException(
                    "Hay una operación SQL pendiente de conciliación. Debes resolverla antes de continuar.")
            End If
            If StationSettings.ReadLockedPallet() <> "" Then
                Throw New InvalidOperationException(
                    "Completa el pallet ABIERTO antes de transferir otro.")
            End If
            Dim source = GetSource()
            Dim destination = destinationInput.Text.Trim().ToUpperInvariant()
            Dim format = PalletIdSettings.Load()
            If Not format.IsValid(destination) Then
                Throw New InvalidOperationException("El nuevo código de contenedor no coincide con Configuración general.")
            End If
            If String.Equals(source.Code,destination,StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException("El origen y el destino deben ser diferentes.")
            End If
            If PaSPackingData.FindContainer(destination) IsNot Nothing Then
                Throw New InvalidOperationException("El código de destino ya existe en CCS.")
            End If
            If MessageBox.Show(
                "¿Transferir " & source.PieceTotal & " piezas validadas desde " & source.Code &
                " al pallet NUEVO " & destination & "?" & Environment.NewLine &
                "El origen pasará a TRANSFERIDO y el destino quedará ABIERTO.",
                "Confirmar transferencia de pallet", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) <> DialogResult.Yes Then Return

            Dim destinationId = PaSPackingData.TransferPartial(source.Code, destination)
            If destinationId <= 0 Then Throw New InvalidOperationException(
                "SQL no devolvió el ID del nuevo pallet.")
            _destinationCode = destination
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message,"Transferencia de pallet bloqueada",
                            MessageBoxButtons.OK,MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
