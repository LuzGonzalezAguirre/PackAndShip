Imports System.Drawing
Imports System.Windows.Forms

Public Class Form1
    Private ReadOnly brandBlue As Color = Color.FromArgb(31, 78, 121)
    Private ReadOnly backgroundColor As Color = Color.FromArgb(244, 247, 250)
    Private ReadOnly inputBarcode As New TextBox()
    Private ReadOnly inputPassword As New TextBox()
    Private ReadOnly labelCustomer As New Label()
    Private ReadOnly labelStatus As New Label()
    Private ReadOnly labelWelcome As New Label()
    Private ReadOnly buttonLogin As New Button()
    Private ReadOnly buttonLogout As New Button()
    Private ReadOnly mainMenu As New MenuStrip()
    Private ReadOnly menuSetup As New ToolStripMenuItem("Setup")
    Private ReadOnly menuHelp As New ToolStripMenuItem("Ayuda")
    Private ReadOnly menuLogout As New ToolStripMenuItem("Cerrar sesión")
    Private ReadOnly appHeader As New Panel()
    Private ReadOnly loginPanel As New Panel()
    Private ReadOnly signedInPanel As New Panel()
    Private units As New List(Of BusinessUnit)()

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Pack and Ship | SSI"
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.MinimumSize = New Size(1, 1)
        Me.ClientSize = New Size(422, 340)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = backgroundColor
        Me.Font = New Font("Segoe UI", 11.0F)
        CreateLayout()
        ReloadCustomers()
        ResetLogin()
    End Sub

    Private Sub CreateLayout()
        appHeader.Dock = DockStyle.Top
        appHeader.Height = 84
        appHeader.BackColor = brandBlue
        Dim title As New Label With {
            .Text = "PACK AND SHIP", .Font = New Font("Segoe UI", 23, FontStyle.Bold),
            .ForeColor = Color.White, .AutoSize = True, .Location = New Point(24, 19)
        }
        appHeader.Controls.Add(title)

        ' Traditional fixed MenuStrip, matching the Q-Wall navigation layout.
        mainMenu.Dock = DockStyle.Top
        mainMenu.AutoSize = False
        mainMenu.Height = 34
        mainMenu.BackColor = PaSTheme.Surface
        mainMenu.ForeColor = PaSTheme.TextPrimary
        mainMenu.Font = New Font("Segoe UI", 10.0F)
        mainMenu.Padding = New Padding(9, 3, 0, 3)

        Dim changeCustomer = menuSetup.DropDownItems.Add("Customer Configuration")
        AddHandler changeCustomer.Click, AddressOf ChangeDefaultCustomer
        Dim usersEntry = menuSetup.DropDownItems.Add("Users & Roles")
        AddHandler usersEntry.Click, AddressOf OpenUsersSetup
        Dim palletEntry = menuSetup.DropDownItems.Add("Pallet Configuration")
        AddHandler palletEntry.Click, AddressOf OpenPalletConfiguration
        Dim scanEntry = menuSetup.DropDownItems.Add("Scan Rules")
        AddHandler scanEntry.Click, AddressOf OpenScanRules
        Dim generalEntry = menuSetup.DropDownItems.Add("General Settings")
        AddHandler generalEntry.Click, AddressOf OpenGeneralSettings

        Dim aboutEntry = menuHelp.DropDownItems.Add("About PackAndShip")
        AddHandler aboutEntry.Click, Sub()
                                         MessageBox.Show("PackAndShip" & Environment.NewLine &
                                                         "Manufacturing pallet scanning and configuration.",
                                                         "About", MessageBoxButtons.OK,
                                                         MessageBoxIcon.Information)
                                     End Sub
        AddHandler menuLogout.Click, AddressOf LogoutFromMenu

        mainMenu.Items.AddRange(New ToolStripItem() {menuSetup, menuHelp, menuLogout})
        MainMenuStrip = mainMenu

        loginPanel.Dock = DockStyle.Fill
        signedInPanel.Dock = DockStyle.Fill
        CreateLoginContent()
        CreateSignedInContent()

        Me.Controls.Add(loginPanel)
        Me.Controls.Add(signedInPanel)
        Me.Controls.Add(appHeader)
        Me.Controls.Add(mainMenu)
    End Sub

    Private Sub CreateLoginContent()
        ' Compact standalone login; the form itself is the card.
        Dim card As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .BorderStyle = BorderStyle.None
        }
        loginPanel.Controls.Add(card)

        Dim cardHeader As New Panel With {
            .Dock = DockStyle.Top, .Height = 50,
            .BackColor = brandBlue
        }
        Dim cardTitle As New Label With {
            .Text = "Pack and Ship",
            .Dock = DockStyle.Fill,
            .TextAlign = ContentAlignment.MiddleCenter,
            .Font = New Font("Segoe UI", 13.0F, FontStyle.Bold),
            .ForeColor = Color.White
        }
        cardHeader.Controls.Add(cardTitle)

        Dim cardFooter As New Panel With {
            .Dock = DockStyle.Bottom, .Height = 50,
            .BackColor = brandBlue
        }
        buttonLogin.Text = "Iniciar sesión"
        buttonLogin.Dock = DockStyle.Fill
        buttonLogin.BackColor = brandBlue
        buttonLogin.ForeColor = Color.White
        buttonLogin.FlatStyle = FlatStyle.Flat
        buttonLogin.FlatAppearance.BorderSize = 0
        buttonLogin.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)
        buttonLogin.Cursor = Cursors.Hand
        AddHandler buttonLogin.Click, AddressOf PerformLogin
        cardFooter.Controls.Add(buttonLogin)

        Dim sectionLogin As New Label With {
            .Text = "Inicio de sesión",
            .Location = New Point(14, 57), .Size = New Size(394, 20),
            .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
            .ForeColor = brandBlue
        }
        Dim barcodeLabel As New Label With {
            .Text = "Credencial (Barcode ID)",
            .Location = New Point(14, 79), .Size = New Size(394, 18),
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.FromArgb(80, 80, 80)
        }
        inputBarcode.Location = New Point(14, 99)
        inputBarcode.Size = New Size(394, 27)
        inputBarcode.Font = New Font("Segoe UI", 11.0F)
        inputBarcode.BackColor = Color.White
        inputBarcode.BorderStyle = BorderStyle.FixedSingle
        inputBarcode.CharacterCasing = CharacterCasing.Upper
        inputBarcode.TabIndex = 0

        Dim passwordLabel As New Label With {
            .Text = "Password",
            .Location = New Point(14, 133), .Size = New Size(394, 18),
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.FromArgb(80, 80, 80)
        }
        inputPassword.Location = New Point(14, 153)
        inputPassword.Size = New Size(394, 27)
        inputPassword.Font = New Font("Segoe UI", 11.0F)
        inputPassword.BackColor = Color.White
        inputPassword.BorderStyle = BorderStyle.FixedSingle
        inputPassword.UseSystemPasswordChar = True
        inputPassword.TabIndex = 1

        Dim sectionStation As New Label With {
            .Text = "Estación de trabajo",
            .Location = New Point(14, 198), .Size = New Size(394, 20),
            .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold),
            .ForeColor = brandBlue
        }
        Dim customerLabel As New Label With {
            .Text = "Cliente predeterminado",
            .Location = New Point(14, 222), .Size = New Size(394, 18),
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.FromArgb(80, 80, 80)
        }
        labelCustomer.Location = New Point(14, 242)
        labelCustomer.Size = New Size(394, 32)
        labelCustomer.ForeColor = brandBlue
        labelCustomer.BackColor = Color.FromArgb(242, 247, 252)
        labelCustomer.BorderStyle = BorderStyle.FixedSingle
        labelCustomer.TextAlign = ContentAlignment.MiddleLeft
        labelCustomer.Font = New Font("Segoe UI", 11.0F, FontStyle.Bold)

        card.Controls.AddRange(New Control() {
            sectionLogin, barcodeLabel, inputBarcode, passwordLabel,
            inputPassword, sectionStation, customerLabel, labelCustomer
        })
        card.Controls.Add(cardFooter)
        card.Controls.Add(cardHeader)
        cardHeader.BringToFront()
        cardFooter.BringToFront()
    End Sub

    Private Sub CreateSignedInContent()
        Dim title As New Label With {
            .Text = "Pallet Scanner", .Font = New Font("Segoe UI", 24, FontStyle.Bold),
            .ForeColor = brandBlue, .AutoSize = True, .Location = New Point(35, 35)
        }
        signedInPanel.Controls.Add(title)
        labelWelcome.Location = New Point(38, 100)
        labelWelcome.Size = New Size(850, 90)
        signedInPanel.Controls.Add(labelWelcome)
        labelStatus.Text = "Pallet scanning rules are pending definition."
        labelStatus.Location = New Point(38, 210)
        labelStatus.Size = New Size(850, 45)
        signedInPanel.Controls.Add(labelStatus)

        buttonLogout.Text = "Log Out"
        buttonLogout.BackColor = brandBlue
        buttonLogout.ForeColor = Color.White
        buttonLogout.FlatStyle = FlatStyle.Flat
        buttonLogout.Location = New Point(38, 280)
        buttonLogout.Size = New Size(160, 42)
        AddHandler buttonLogout.Click, Sub() ResetLogin()
        signedInPanel.Controls.Add(buttonLogout)
    End Sub

    Private Sub AddLabel(parent As Control, value As String, x As Integer, y As Integer,
                         width As Integer, height As Integer, bold As Boolean)
        Dim item As New Label With {
            .Text = value, .Location = New Point(x, y), .Size = New Size(width, height),
            .ForeColor = brandBlue
        }
        If bold Then item.Font = New Font("Segoe UI", 17, FontStyle.Bold)
        parent.Controls.Add(item)
    End Sub

    Private Sub CenterCard(parent As Control, child As Control)
        child.Location = New Point(
            Math.Max(0, (parent.ClientSize.Width - child.Width) \ 2),
            Math.Max(18, (parent.ClientSize.Height - child.Height) \ 2))
    End Sub

    Private Sub ReloadCustomers()
        Try
            units = PackAndShipData.GetBusinessUnits()
            RefreshCustomer()
        Catch ex As Exception
            units.Clear()
            labelCustomer.Text = "Customer unavailable"
            MessageBox.Show(ex.Message, "SQL Server connection", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RefreshCustomer()
        Dim saved = StationSettings.ReadCustomerId()
        Dim selected As BusinessUnit = Nothing
        If saved.HasValue Then selected = units.Find(Function(u) u.Id = saved.Value)
        SessionContext.BusinessUnitId = If(selected Is Nothing, 0, selected.Id)
        SessionContext.BusinessUnitName = If(selected Is Nothing, "", selected.Name)
        labelCustomer.Text = If(selected Is Nothing, "Not configured", selected.Name)
    End Sub

    Private Sub PerformLogin(sender As Object, e As EventArgs)
        Dim barcode = inputBarcode.Text.Trim().ToUpperInvariant()
        Dim password = inputPassword.Text
        If barcode = "" OrElse password = "" Then
            MessageBox.Show("Enter Barcode ID and Password.", "Required fields",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim user = PackAndShipData.Authenticate(barcode, password)
            If user Is Nothing Then
                MessageBox.Show("Invalid credentials or inactive user.", "Access denied",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
                inputPassword.Clear()
                Return
            End If

            SessionContext.UserId = user.Id
            SessionContext.FullName = user.Name
            SessionContext.BarcodeId = user.Barcode
            SessionContext.RoleId = user.RoleId
            SessionContext.RoleName = user.RoleName
            SessionContext.CanModifySetup = user.CanModifySetup

            SetAuthenticatedLayout()
            menuSetup.Enabled = SessionContext.CanModifySetup
            menuLogout.Enabled = True
            labelWelcome.Text = "Operator: " & user.Name & "  |  Role: " & user.RoleName &
                                Environment.NewLine & "Customer: " &
                                If(SessionContext.BusinessUnitId = 0, "Not configured",
                                   SessionContext.BusinessUnitName)
            If SessionContext.BusinessUnitId = 0 Then
                labelStatus.Text = "Configure this station's customer from Setup before scanning."
            Else
                labelStatus.Text = "Pallet scanning rules are pending definition."
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Login error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub OpenScanRules(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Your role cannot access Setup.", "Access denied",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Using dialog As New ScanRulesForm()
            dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenPalletConfiguration(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Your role cannot modify Setup.", "Access denied",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Using dialog As New PalletConfigurationForm()
            dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenUsersSetup(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Your role cannot modify Setup.", "Access denied",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Using dialog As New UsersSetupForm()
            dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenGeneralSettings(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Your role cannot modify Setup.", "Access denied",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Using dialog As New GeneralSettingsForm()
            dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Sub ChangeDefaultCustomer(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Your role cannot change Setup.", "Access denied")
            Return
        End If
        If units.Count = 0 Then
            ReloadCustomers()
            If units.Count = 0 Then Return
        End If

        Using dialog As New Form With {
            .Text = "PackAndShip | Customer Configuration",
            .Size = New Size(550, 385),
            .MinimumSize = New Size(550, 385),
            .MaximumSize = New Size(550, 385),
            .StartPosition = FormStartPosition.CenterParent,
            .FormBorderStyle = FormBorderStyle.FixedDialog,
            .MaximizeBox = False, .MinimizeBox = False
        }
            PaSTheme.ConfigureForm(dialog)

            Dim topPanel As New Panel With {
                .Dock = DockStyle.Top, .Height = 82,
                .BackColor = PaSTheme.Navy
            }
            topPanel.Controls.Add(New Label With {
                .Text = "Customer Configuration",
                .Location = New Point(23, 12), .AutoSize = True,
                .Font = New Font("Segoe UI", 19, FontStyle.Bold),
                .ForeColor = Color.White
            })
            topPanel.Controls.Add(New Label With {
                .Text = "Default customer for this workstation",
                .Location = New Point(25, 53), .AutoSize = True,
                .ForeColor = Color.FromArgb(220, 233, 247)
            })
            dialog.Controls.Add(topPanel)

            Dim contentPanel As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = PaSTheme.Background,
                .Padding = New Padding(24)
            }
            Dim caption As New Label With {
                .Text = "Customer",
                .Location = New Point(24, 79),
                .Size = New Size(460, 25),
                .ForeColor = PaSTheme.TextPrimary,
                .Font = New Font("Segoe UI", 10, FontStyle.Bold)
            }
            Dim picker As New ComboBox With {
                .Location = New Point(24, 108),
                .Size = New Size(476, 33),
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .Font = New Font("Segoe UI", 11)
            }
            contentPanel.Controls.AddRange(New Control() {caption, picker})
            dialog.Controls.Add(contentPanel)

            picker.DisplayMember = "Name"
            picker.ValueMember = "Id"
            picker.DataSource = New List(Of BusinessUnit)(units)
            If SessionContext.BusinessUnitId > 0 Then
                Dim savedCustomer = units.Find(Function(u) u.Id = SessionContext.BusinessUnitId)
                If savedCustomer IsNot Nothing Then picker.SelectedValue = savedCustomer.Id
            End If

            Dim footer As New Panel With {
                .Dock = DockStyle.Bottom, .Height = 78,
                .BackColor = PaSTheme.Surface
            }
            Dim save = PaSTheme.ActionButton("Save", True)
            save.Location = New Point(365, 18)
            save.Size = New Size(145, 40)
            save.DialogResult = DialogResult.OK
            Dim cancel = PaSTheme.ActionButton("Cancel", False)
            cancel.Location = New Point(215, 18)
            cancel.Size = New Size(135, 40)
            cancel.DialogResult = DialogResult.Cancel
            footer.Controls.AddRange(New Control() {cancel, save})
            dialog.Controls.Add(footer)
            footer.BringToFront()
            topPanel.BringToFront()
            dialog.AcceptButton = save
            dialog.CancelButton = cancel

            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim selected = TryCast(picker.SelectedItem, BusinessUnit)
            If selected Is Nothing Then Return
            Try
                StationSettings.SaveCustomerId(selected.Id)
                RefreshCustomer()
                labelWelcome.Text = "Operator: " & SessionContext.FullName &
                    "  |  Role: " & SessionContext.RoleName &
                    Environment.NewLine & "Customer: " & SessionContext.BusinessUnitName
                labelStatus.Text = "Pallet scanning rules are pending definition."
            Catch ex As Exception
                MessageBox.Show(ex.Message, "Station configuration error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    Private Sub SetLoginLayout()
        SuspendLayout()
        Try
            mainMenu.Visible = False
            appHeader.Visible = False
            signedInPanel.Visible = False
            loginPanel.Visible = True
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimumSize = New Size(1, 1)
            ClientSize = New Size(422, 340)
            Text = "Pack and Ship | Login"
            CenterToScreen()
        Finally
            ResumeLayout(True)
        End Try
    End Sub

    Private Sub SetAuthenticatedLayout()
        SuspendLayout()
        Try
            loginPanel.Visible = False
            signedInPanel.Visible = True
            FormBorderStyle = FormBorderStyle.Sizable
            MaximizeBox = True
            MinimumSize = New Size(800, 570)
            ClientSize = New Size(1100, 720)
            appHeader.Visible = True
            mainMenu.Visible = True
            Text = "Pack and Ship | SSI"
            CenterToScreen()
        Finally
            ResumeLayout(True)
        End Try
    End Sub

    Private Sub LogoutFromMenu(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 Then Return
        ResetLogin()
    End Sub

    Private Sub ResetLogin()
        SessionContext.Clear()
        RefreshCustomer()
        inputBarcode.Clear()
        inputPassword.Clear()
        SetLoginLayout()
        menuSetup.Enabled = False
        menuLogout.Enabled = False
        Me.AcceptButton = buttonLogin
        inputBarcode.Focus()
    End Sub
End Class
