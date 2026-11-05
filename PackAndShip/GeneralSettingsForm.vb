Imports System.Drawing
Imports System.Windows.Forms

Public Class GeneralSettingsForm
    Inherits Form

    Private ReadOnly navy As Color = Color.FromArgb(31, 78, 121)
    Private ReadOnly prefixInput As New TextBox()
    Private ReadOnly digitsInput As New NumericUpDown()
    Private ReadOnly exampleLabel As New Label()
    Private ReadOnly testInput As New TextBox()
    Private ReadOnly testResult As New Label()
    Private ReadOnly saveButton As New Button()
    Private saved As PalletIdSettings

    Public Sub New()
        Text = "Pack and Ship | Setup - General"
        Size = New Size(750, 560)
        MinimumSize = New Size(650, 510)
        FormBorderStyle = FormBorderStyle.Sizable
        StartPosition = FormStartPosition.CenterParent
        BackColor = Color.FromArgb(244, 247, 250)
        Font = New Font("Segoe UI", 10)
        BuildLayout()
        AddHandler Shown, Sub() LoadCurrentSettings()
    End Sub

    Private Sub BuildLayout()
        Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 87, .BackColor = navy}
        header.Controls.Add(New Label With {
            .Text = "General Settings", .AutoSize = True,
            .Font = New Font("Segoe UI", 22, FontStyle.Bold),
            .ForeColor = Color.White, .Location = New Point(22, 11)
        })
        header.Controls.Add(New Label With {
            .Text = "Global pallet ID format • Shared by all stations",
            .AutoSize = True, .ForeColor = Color.White, .Location = New Point(25, 59)
        })
        Controls.Add(header)

        Dim footer As New Panel With {.Dock = DockStyle.Bottom, .Height = 77}
        saveButton.Text = "Save changes"
        saveButton.Size = New Size(150, 40)
        saveButton.Location = New Point(535, 15)
        saveButton.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        saveButton.BackColor = navy
        saveButton.ForeColor = Color.White
        saveButton.FlatStyle = FlatStyle.Flat
        AddHandler saveButton.Click, AddressOf SaveChanges
        Dim cancelButton As New Button With {
            .Text = "Close", .Location = New Point(390, 15),
            .Size = New Size(130, 40), .Anchor = AnchorStyles.Top Or AnchorStyles.Right
        }
        AddHandler cancelButton.Click, Sub() Close()
        AddHandler footer.Resize, Sub()
                                      saveButton.Left = footer.ClientSize.Width - saveButton.Width - 25
                                      cancelButton.Left = saveButton.Left - cancelButton.Width - 15
                                  End Sub
        footer.Controls.Add(saveButton)
        footer.Controls.Add(cancelButton)
        Controls.Add(footer)

        Dim content As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(24)}
        Controls.Add(content)
        content.BringToFront()
        header.BringToFront()
        footer.BringToFront()

        Dim section As New Panel With {.Dock = DockStyle.Fill, .BackColor = Color.White}
        content.Controls.Add(section)

        section.Controls.Add(New Label With {
            .Text = "PALLET IDENTIFIER", .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .ForeColor = navy, .Location = New Point(24, 21), .AutoSize = True
        })
        section.Controls.Add(New Label With {
            .Text = "Prefix", .Location = New Point(24, 75), .AutoSize = True
        })
        prefixInput.SetBounds(24, 101, 155, 33)
        prefixInput.MaxLength = 5
        prefixInput.CharacterCasing = CharacterCasing.Upper
        section.Controls.Add(prefixInput)

        section.Controls.Add(New Label With {
            .Text = "Numeric digits", .Location = New Point(207, 75), .AutoSize = True
        })
        digitsInput.SetBounds(207, 101, 155, 33)
        digitsInput.Minimum = 1
        digitsInput.Maximum = 12
        section.Controls.Add(digitsInput)

        exampleLabel.SetBounds(24, 160, 590, 37)
        exampleLabel.Font = New Font("Consolas", 15, FontStyle.Bold)
        exampleLabel.ForeColor = navy
        section.Controls.Add(exampleLabel)

        section.Controls.Add(New Label With {
            .Text = "Test a scanned pallet ID", .Location = New Point(24, 220), .AutoSize = True
        })
        testInput.SetBounds(24, 247, 330, 33)
        section.Controls.Add(testInput)
        testResult.SetBounds(368, 252, 230, 32)
        section.Controls.Add(testResult)

        section.Controls.Add(New Label With {
            .Text = "This setting validates the pallet ID format only. It does not generate " &
                    "pallet numbers or change existing records." & Environment.NewLine &
                    "Saved in SQL Server and shared by all PackAndShip stations.",
            .Location = New Point(24, 316), .Size = New Size(595, 55),
            .ForeColor = Color.FromArgb(80, 85, 95)
        })

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
