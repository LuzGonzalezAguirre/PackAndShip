Imports System.IO

' Configuración compartida por los usuarios Windows de la misma computadora.
Public NotInheritable Class StationSettings
    Private Sub New()
    End Sub

    Private Shared ReadOnly ConfigDirectory As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PackAndShip")

    Private Shared ReadOnly ConfigFile As String =
        Path.Combine(ConfigDirectory, "station_config.txt")

    Public Shared Function ReadCustomerId() As Integer?
        If Not File.Exists(ConfigFile) Then Return Nothing
        Dim value As Integer
        If Integer.TryParse(File.ReadAllText(ConfigFile).Trim(), value) AndAlso value > 0 Then
            Return value
        End If
        Return Nothing
    End Function

    Public Shared Sub SaveCustomerId(buId As Integer)
        If buId <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(buId))
        Directory.CreateDirectory(ConfigDirectory)
        File.WriteAllText(ConfigFile, buId.ToString(Globalization.CultureInfo.InvariantCulture))
    End Sub
End Class
