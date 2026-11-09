Imports System.Drawing
Imports System.Windows.Forms

Public Class GeneralSettingsForm
    Inherits Form

    Private ReadOnly navy As Color = Color.FromArgb(31, 78, 121)






    Private saved As PalletIdSettings

    Public Sub New()
        InitializeComponent()
        BindEvents()
        AddHandler Shown, Sub() LoadCurrentSettings()
    End Sub

    Private Sub BindEvents()
        AddHandler saveButton.Click, AddressOf SaveChanges
        AddHandler cancelButton.Click, Sub() Close()
        AddHandler prefixInput.TextChanged, Sub() UpdatePreview()
        AddHandler digitsInput.ValueChanged, Sub() UpdatePreview()
        AddHandler testInput.TextChanged, Sub() UpdatePreview()
    End Sub

    Private Sub LoadCurrentSettings()
        Try
            saved = PalletIdSettings.Load()
            prefixInput.Text = saved.Prefix
            digitsInput.Value = saved.DigitCount
            testInput.Clear()
            UpdatePreview()
        Catch ex As Exception
            saveButton.Enabled = False
            MessageBox.Show(ex.Message & Environment.NewLine &
                            "The existing format was not overwritten.",
                            "Cannot load general settings", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function BuildFormat() As PalletIdSettings
        Dim prefix = prefixInput.Text.Trim().ToUpperInvariant()
        Dim digits = CInt(digitsInput.Value)
        PalletIdSettings.ValidateFormat(prefix, digits)
        Return New PalletIdSettings With {.Prefix = prefix, .DigitCount = digits}
    End Function

    Private Sub UpdatePreview()
        Try
            Dim format = BuildFormat()
            exampleLabel.Text = "Example: " & format.Example()
            If String.IsNullOrWhiteSpace(testInput.Text) Then
                testResult.Text = ""
            Else
                Dim valid = format.IsValid(testInput.Text)
                testResult.Text = If(valid, "Valid format", "Invalid format")
                testResult.ForeColor = If(valid, Color.DarkGreen, Color.DarkRed)
            End If
            saveButton.Enabled = True
        Catch
            exampleLabel.Text = "Enter a valid prefix (A-Z)"
            testResult.Text = ""
            saveButton.Enabled = False
        End Try
    End Sub

    Private Sub SaveChanges(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Your role cannot modify Setup.", "Access denied",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Try
            Dim format = BuildFormat()
            PalletIdSettings.Save(format.Prefix, format.DigitCount)
            saved = format
            MessageBox.Show("Global pallet ID format saved in SQL Server.", "Saved",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Cannot save general settings",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
