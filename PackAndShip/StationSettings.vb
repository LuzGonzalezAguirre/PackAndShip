Imports System.IO

' Configuración compartida por los usuarios Windows de la misma computadora.
Public NotInheritable Class StationSettings
    Private Sub New()
    End Sub

    Private Shared ReadOnly ConfigDirectory As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PackAndShip")

    Private Shared ReadOnly ConfigFile As String =
        Path.Combine(ConfigDirectory, "station_config.txt")

    Private Shared ReadOnly PalletLockFile As String =
        Path.Combine(ConfigDirectory, "active_pallet.txt")

    ' One workstation may have only one active OPEN pallet. Retain the lock
    ' across logout and app restart; do not clear it on connection failure.
    Public Shared Function ReadLockedPallet() As String
        If Not File.Exists(PalletLockFile) Then Return ""
        Return File.ReadAllText(PalletLockFile).Trim().ToUpperInvariant()
    End Function

    Public Shared Sub LockPallet(code As String)
        Dim normalized = If(code, "").Trim().ToUpperInvariant()
        If normalized = "" Then Throw New ArgumentException("Pallet code is required.")
        Dim existing = ReadLockedPallet()
        If existing <> "" AndAlso Not String.Equals(existing, normalized,
                                                      StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException(
                "Finish pallet " & existing & " (complete or authorized partial) before switching.")
        End If
        Directory.CreateDirectory(ConfigDirectory)
        Dim temp = PalletLockFile & ".tmp"
        File.WriteAllText(temp, normalized)
        If File.Exists(PalletLockFile) Then
            File.Replace(temp, PalletLockFile, Nothing)
        Else
            File.Move(temp, PalletLockFile)
        End If
    End Sub

    Public Shared Sub UnlockPallet(code As String)
        Dim saved = ReadLockedPallet()
        If saved <> "" AndAlso String.Equals(saved, If(code, "").Trim(),
                                             StringComparison.OrdinalIgnoreCase) Then
            File.Delete(PalletLockFile)
        End If
    End Sub

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
