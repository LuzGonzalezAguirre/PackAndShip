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
    Private ReadOnly buttonSetup As New Button()
    Private ReadOnly setupMenu As New ContextMenuStrip()
    Private ReadOnly loginPanel As New Panel()
    Private ReadOnly signedInPanel As New Panel()
    Private units As New List(Of BusinessUnit)()

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Pack and Ship | SSI"
        Me.MinimumSize = New Size(740, 510)
        Me.Size = New Size(1000, 650)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = backgroundColor
        Me.Font = New Font("Segoe UI", 11.0F)
        CreateLayout()
        ReloadCustomers()
        ResetLogin()
    End Sub

    Private Sub CreateLayout()
        Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 84, .BackColor = brandBlue}
        Dim title As New Label With {
            .Text = "PACK AND SHIP", .Font = New Font("Segoe UI", 23, FontStyle.Bold),
            .ForeColor = Color.White, .AutoSize = True, .Location = New Point(24, 19)
        }
        header.Controls.Add(title)

        buttonSetup.Text = "Setup  ▾"
        buttonSetup.Size = New Size(135, 40)
        buttonSetup.Anchor = AnchorStyles.Right Or AnchorStyles.Top
        buttonSetup.BackColor = Color.White
        buttonSetup.ForeColor = brandBlue
        buttonSetup.FlatStyle = FlatStyle.Flat
        buttonSetup.Location = New Point(Width - 190, 23)
        AddHandler header.Resize, Sub()
                                      buttonSetup.Left = header.ClientSize.Width - buttonSetup.Width - 24
                                  End Sub
        AddHandler buttonSetup.Click, Sub() setupMenu.Show(buttonSetup, 0, buttonSetup.Height)
        header.Controls.Add(buttonSetup)

        Dim changeCustomer = setupMenu.Items.Add("Change default customer")
        AddHandler changeCustomer.Click, AddressOf ChangeDefaultCustomer
        Dim palletEntry = setupMenu.Items.Add("Pallet Configuration")
        AddHandler palletEntry.Click, AddressOf OpenPalletConfiguration
        Dim scanEntry = setupMenu.Items.Add("Scan Rules")
        AddHandler scanEntry.Click, AddressOf OpenScanRules
        Dim usersEntry = setupMenu.Items.Add("Users & Roles")
        AddHandler usersEntry.Click, AddressOf OpenUsersSetup
        Dim generalEntry = setupMenu.Items.Add("General (pending)")
        generalEntry.Enabled = False

        loginPanel.Dock = DockStyle.Fill
        signedInPanel.Dock = DockStyle.Fill
        CreateLoginContent()
        CreateSignedInContent()

        Me.Controls.Add(loginPanel)
        Me.Controls.Add(signedInPanel)
        Me.Controls.Add(header)
    End Sub

    Private Sub CreateLoginContent()
        Dim card As New Panel With {
            .Width = 480, .Height = 375, .BackColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle
        }
        AddHandler loginPanel.Resize, Sub() CenterCard(loginPanel, card)
        loginPanel.Controls.Add(card)
        CenterCard(loginPanel, card)

        AddLabel(card, "Operator Login", 25, 20, 400, 32, True)
        AddLabel(card, "Default customer", 25, 72, 410, 22, False)
        labelCustomer.Location = New Point(25, 100)
        labelCustomer.Size = New Size(420, 33)
        labelCustomer.ForeColor = brandBlue
        labelCustomer.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        card.Controls.Add(labelCustomer)

        AddLabel(card, "Barcode ID", 25, 145, 410, 25, False)
        inputBarcode.Location = New Point(25, 175)
        inputBarcode.Width = 420
        inputBarcode.CharacterCasing = CharacterCasing.Upper
        card.Controls.Add(inputBarcode)

        AddLabel(card, "Password", 25, 220, 410, 25, False)
        inputPassword.Location = New Point(25, 250)
        inputPassword.Width = 420
        inputPassword.UseSystemPasswordChar = True
        card.Controls.Add(inputPassword)

        buttonLogin.Text = "Log In"
        buttonLogin.BackColor = brandBlue
        buttonLogin.ForeColor = Color.White
        buttonLogin.FlatStyle = FlatStyle.Flat
        buttonLogin.Location = New Point(25, 305)
        buttonLogin.Size = New Size(420, 42)
        AddHandler buttonLogin.Click, AddressOf PerformLogin
        card.Controls.Add(buttonLogin)
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

            loginPanel.Visible = False
            signedInPanel.Visible = True
            buttonSetup.Enabled = SessionContext.CanModifySetup
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
            .Text = "Station customer configuration",
            .Size = New Size(500, 230),
            .StartPosition = FormStartPosition.CenterParent,
            .FormBorderStyle = FormBorderStyle.FixedDialog,
            .MaximizeBox = False, .MinimizeBox = False
        }
            Dim caption As New Label With {
                .Text = "Default customer for this computer",
                .Location = New Point(22, 22), .Size = New Size(435, 30)
            }
            Dim picker As New ComboBox With {
                .Location = New Point(22, 62), .Width = 435,
                .DropDownStyle = ComboBoxStyle.DropDownList
            }
            ' Bind only after the ComboBox has a handle. This avoids a zero-count
            ' Items collection when SelectedIndex is set before Controls.Add.
            dialog.Controls.AddRange(New Control() {caption, picker})
            picker.DisplayMember = "Name"
            picker.ValueMember = "Id"
            picker.DataSource = New List(Of BusinessUnit)(units)
            If SessionContext.BusinessUnitId > 0 Then
                Dim savedCustomer = units.Find(Function(u) u.Id = SessionContext.BusinessUnitId)
                If savedCustomer IsNot Nothing Then picker.SelectedValue = savedCustomer.Id
            End If
            Dim save As New Button With {
                .Text = "Save", .DialogResult = DialogResult.OK,
                .Location = New Point(327, 115), .Size = New Size(130, 35)
            }
            dialog.Controls.Add(save)
            dialog.AcceptButton = save

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

    Private Sub ResetLogin()
        SessionContext.Clear()
        RefreshCustomer()
        inputBarcode.Clear()
        inputPassword.Clear()
        signedInPanel.Visible = False
        loginPanel.Visible = True
        buttonSetup.Enabled = False
        Me.AcceptButton = buttonLogin
        inputBarcode.Focus()
    End Sub
End Class
