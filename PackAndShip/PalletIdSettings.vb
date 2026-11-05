Imports System.IO
Imports System.Text.RegularExpressions

' Station-local pallet ID format. Independent of the existing default-customer file.
' This is a validation setting only; it does not generate or reserve pallet IDs.
Public NotInheritable Class PalletIdSettings
    Private Sub New()
    End Sub

    Public Property Prefix As String = "P"
    Public Property DigitCount As Integer = 6

    Private Shared ReadOnly Folder As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "PackAndShip")
    Private Shared ReadOnly SettingsFile As String = Path.Combine(Folder, "pallet_id_format.txt")

    Public Shared Function Load() As PalletIdSettings
        Dim current As New PalletIdSettings()
        If Not File.Exists(SettingsFile) Then Return current

        Dim lines = File.ReadAllLines(SettingsFile)
        If lines.Length <> 2 Then Throw New InvalidDataException("Invalid pallet format configuration.")
        Dim prefix = lines(0).Trim().ToUpperInvariant()
        Dim digits As Integer
        If Not Integer.TryParse(lines(1).Trim(), digits) Then
            Throw New InvalidDataException("Invalid pallet ID digit count.")
        End If
        ValidateFormat(prefix, digits)
        current.Prefix = prefix
        current.DigitCount = digits
        Return current
    End Function

    Public Shared Sub Save(prefix As String, digits As Integer)
        Dim normalized = If(prefix, "").Trim().ToUpperInvariant()
        ValidateFormat(normalized, digits)
        Directory.CreateDirectory(Folder)
        Dim temporaryFile = Path.Combine(Folder, "pallet_id_format.tmp")
        File.WriteAllLines(temporaryFile, {normalized, digits.ToString(Globalization.CultureInfo.InvariantCulture)})
        File.Move(temporaryFile, SettingsFile, True)
    End Sub

    Public Shared Sub ValidateFormat(prefix As String, digits As Integer)
        If Not Regex.IsMatch(If(prefix, ""), "^[A-Z]{1,5}$") Then
            Throw New ArgumentException("Prefix must contain 1 to 5 letters (A-Z).")
        End If
        If digits < 1 OrElse digits > 12 Then
            Throw New ArgumentOutOfRangeException(NameOf(digits), "Number of digits must be from 1 to 12.")
        End If
    End Sub

    Public Function Example() As String
        Return Prefix & New String("0"c, DigitCount - 1) & "1"
    End Function

    Public Function IsValid(value As String) As Boolean
        If value Is Nothing Then Return False
        Return Regex.IsMatch(value.Trim(), "^" & Regex.Escape(Prefix) &
                             "[0-9]{" & DigitCount & "}$", RegexOptions.CultureInvariant)
    End Function
End Class
