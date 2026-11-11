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
        AddHandler dismissButton.Click, Sub() Close()
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
                            "No se sobrescribió el formato existente.",
                            "No se pudo cargar la configuración general", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
            exampleLabel.Text = "Ejemplo: " & format.Example()
            If String.IsNullOrWhiteSpace(testInput.Text) Then
                testResult.Text = ""
            Else
                Dim valid = format.IsValid(testInput.Text)
                testResult.Text = If(valid, "Formato válido", "Formato inválido")
                testResult.ForeColor = If(valid, Color.DarkGreen, Color.DarkRed)
            End If
            saveButton.Enabled = True
        Catch
            exampleLabel.Text = "Ingresa un prefijo válido (A-Z)"
            testResult.Text = ""
            saveButton.Enabled = False
        End Try
    End Sub

    Private Sub SaveChanges(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Tu rol no puede modificar Configuración.", "Acceso denegado",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Try
            Dim format = BuildFormat()
            PalletIdSettings.Save(format.Prefix, format.DigitCount)
            saved = format
            MessageBox.Show("Formato global de ID de pallet guardado en SQL Server.", "Guardado",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "No se pudo guardar la configuración general",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
