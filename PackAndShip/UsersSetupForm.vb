Imports System.Drawing
Imports System.Windows.Forms

Public Class UsersSetupForm
    Inherits Form

    Private ReadOnly navy As Color = Color.FromArgb(31, 78, 121)
    Private ReadOnly usersGrid As New DataGridView()
    Private ReadOnly searchBox As New TextBox()
    Private ReadOnly countLabel As New Label()
    Private ReadOnly newButton As New Button()
    Private ReadOnly editButton As New Button()
    Private ReadOnly statusButton As New Button()
    Private ReadOnly resetButton As New Button()
    Private ReadOnly reloadButton As New Button()
    Private allUsers As New List(Of PaSUser)()
    Private roles As New List(Of PaSRole)()

    Public Sub New()
        Text = "Pack and Ship | Setup - Users & Roles"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(1060, 680)
        MinimumSize = New Size(850, 550)
        BackColor = Color.FromArgb(244, 247, 250)
        Font = New Font("Segoe UI", 10)
        BuildLayout()
        AddHandler Shown, Sub() LoadData()
    End Sub

    Private Sub BuildLayout()
        Dim top As New Panel With {.Dock = DockStyle.Top, .Height = 92, .BackColor = navy}
        Dim heading As New Label With {
            .Text = "Users & Roles", .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 22, FontStyle.Bold),
            .AutoSize = True, .Location = New Point(22, 13)
        }
        Dim subheading As New Label With {
            .Text = "PackAndShip user administration", .ForeColor = Color.White,
            .AutoSize = True, .Location = New Point(25, 59)
        }
        top.Controls.AddRange(New Control() {heading, subheading})

        Dim actions As New FlowLayoutPanel With {
            .Dock = DockStyle.Top, .Height = 57, .Padding = New Padding(12, 10, 10, 5),
            .WrapContents = False, .AutoScroll = True
        }
        ConfigureButton(newButton, "Add user", 115)
        ConfigureButton(editButton, "Edit", 90)
        ConfigureButton(statusButton, "Activate / Deactivate", 180)
        ConfigureButton(resetButton, "Reset password", 145)
        ConfigureButton(reloadButton, "Refresh", 95)
        actions.Controls.AddRange(New Control() {newButton, editButton, statusButton, resetButton, reloadButton})
        AddHandler newButton.Click, Sub() OpenEditor(Nothing)
        AddHandler editButton.Click, Sub() OpenEditor(SelectedUser())
        AddHandler statusButton.Click, AddressOf ToggleStatus
        AddHandler resetButton.Click, AddressOf ResetPassword
        AddHandler reloadButton.Click, Sub() LoadData()

        Dim searchPanel As New Panel With {.Dock = DockStyle.Top, .Height = 52}
        Dim searchLabel As New Label With {
            .Text = "Search", .Location = New Point(20, 15), .AutoSize = True
        }
        searchBox.Location = New Point(95, 11)
        searchBox.Width = 340
        searchBox.Anchor = AnchorStyles.Left Or AnchorStyles.Top
        AddHandler searchBox.TextChanged, Sub() RenderRows()
        countLabel.AutoSize = True
        countLabel.Location = New Point(460, 15)
        searchPanel.Controls.AddRange(New Control() {searchLabel, searchBox, countLabel})

        usersGrid.Dock = DockStyle.Fill
        usersGrid.ReadOnly = True
        usersGrid.AllowUserToAddRows = False
        usersGrid.AllowUserToDeleteRows = False
        usersGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        usersGrid.MultiSelect = False
        usersGrid.AutoGenerateColumns = False
        usersGrid.BackgroundColor = Color.White
        usersGrid.RowHeadersVisible = False
        usersGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        usersGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "Id", .HeaderText = "ID", .DataPropertyName = "Id", .FillWeight = 30})
        usersGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "FullName", .HeaderText = "Employee", .DataPropertyName = "FullName", .FillWeight = 140})
        usersGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "Barcode", .HeaderText = "Barcode ID", .DataPropertyName = "Barcode", .FillWeight = 80})
        usersGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "RoleName", .HeaderText = "Role", .DataPropertyName = "RoleName", .FillWeight = 90})
        usersGrid.Columns.Add(New DataGridViewCheckBoxColumn With {.Name = "IsActive", .HeaderText = "Active", .DataPropertyName = "IsActive", .FillWeight = 35})
        AddHandler usersGrid.CellDoubleClick, Sub(sender, e)
                                                  If e.RowIndex >= 0 Then OpenEditor(SelectedUser())
                                              End Sub

        Dim gridBox As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(18, 5, 18, 18)}
        gridBox.Controls.Add(usersGrid)
        Controls.Add(gridBox)
        Controls.Add(searchPanel)
        Controls.Add(actions)
        Controls.Add(top)
    End Sub

    Private Sub ConfigureButton(button As Button, caption As String, buttonWidth As Integer)
        button.Text = caption
        button.Width = buttonWidth
        button.Height = 34
        button.FlatStyle = FlatStyle.Flat
        button.BackColor = navy
        button.ForeColor = Color.White
        button.Margin = New Padding(5, 0, 5, 0)
    End Sub

    Private Sub LoadData()
        Try
            roles = UserAdministration.ListRoles()
            allUsers = UserAdministration.ListUsers()
            RenderRows()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Cannot load users", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RenderRows()
        Dim term = searchBox.Text.Trim()
        Dim filtered = allUsers.Where(Function(u) term = "" OrElse
            u.FullName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
            u.Barcode.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
            u.RoleName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0).ToList()
        usersGrid.DataSource = Nothing
        usersGrid.DataSource = filtered
        countLabel.Text = filtered.Count.ToString() & " of " & allUsers.Count.ToString() & " users"
        usersGrid.ClearSelection()
    End Sub

    Private Function SelectedUser() As PaSUser
        If usersGrid.SelectedRows.Count = 0 Then Return Nothing
        Return TryCast(usersGrid.SelectedRows(0).DataBoundItem, PaSUser)
    End Function

    Private Sub OpenEditor(user As PaSUser)
        Try
            Using dialog As New UserEditorDialog(user, roles)
                If dialog.ShowDialog(Me) = DialogResult.OK Then LoadData()
            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message, "User editor", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ToggleStatus(sender As Object, e As EventArgs)
        Dim user = SelectedUser()
        If user Is Nothing Then
            MessageBox.Show("Select a user first.")
            Return
        End If
        Dim desired = Not user.IsActive
        Dim action = If(desired, "activate", "deactivate")
        If MessageBox.Show("Do you want to " & action & " " & user.FullName & "?",
                           "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        Try
            UserAdministration.SetUserActive(user.Id, desired)
            LoadData()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Cannot change status", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ResetPassword(sender As Object, e As EventArgs)
        Dim user = SelectedUser()
        If user Is Nothing Then
            MessageBox.Show("Select a user first.")
            Return
        End If
        If MessageBox.Show("Reset password for " & user.FullName & " to their Barcode ID?",
                           "Confirm password reset", MessageBoxButtons.YesNo,
                           MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
        Try
            UserAdministration.ResetPasswordToBarcode(user.Id)
            MessageBox.Show("Password reset completed.")
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Password reset failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class

Friend Class UserEditorDialog
    Inherits Form

    Private ReadOnly currentUser As PaSUser
    Private ReadOnly inputName As New TextBox()
    Private ReadOnly inputBarcode As New TextBox()
    Private ReadOnly inputRole As New ComboBox()
    Private ReadOnly inputActive As New CheckBox()
    Private ReadOnly saveButton As New Button()

    Public Sub New(user As PaSUser, roles As List(Of PaSRole))
        currentUser = user
        Text = If(user Is Nothing, "New user", "Edit user")
        StartPosition = FormStartPosition.CenterParent
        FormBorderStyle = FormBorderStyle.FixedDialog
        MinimizeBox = False
        MaximizeBox = False
        ClientSize = New Size(480, 390)
        BackColor = Color.White
        Font = New Font("Segoe UI", 11)
        BuildLayout(roles)
    End Sub

    Private Sub BuildLayout(roles As List(Of PaSRole))
        Dim heading As New Label With {
            .Text = If(currentUser Is Nothing, "Add employee", "Edit employee"),
            .Font = New Font("Segoe UI", 19, FontStyle.Bold),
            .ForeColor = Color.FromArgb(31, 78, 121),
            .Location = New Point(24, 15), .Size = New Size(420, 42)
        }
        Controls.Add(heading)

        AddFieldLabel("Employee name", 24, 77)
        inputName.SetBounds(24, 104, 430, 30)
        inputName.MaxLength = 100

        AddFieldLabel("Barcode ID", 24, 146)
        inputBarcode.SetBounds(24, 173, 430, 30)
        inputBarcode.MaxLength = 50
        inputBarcode.CharacterCasing = CharacterCasing.Upper

        AddFieldLabel("Role", 24, 216)
        inputRole.SetBounds(24, 243, 430, 30)
        inputRole.DropDownStyle = ComboBoxStyle.DropDownList
        inputRole.DataSource = roles
        inputRole.DisplayMember = "Name"
        inputRole.ValueMember = "Id"

        inputActive.Text = "Active user"
        inputActive.SetBounds(24, 292, 200, 29)
        inputActive.Checked = True

        saveButton.Text = "Save"
        saveButton.SetBounds(333, 334, 121, 37)
        saveButton.FlatStyle = FlatStyle.Flat
        saveButton.BackColor = Color.FromArgb(31, 78, 121)
        saveButton.ForeColor = Color.White
        AddHandler saveButton.Click, AddressOf SaveUser

        Dim cancel As New Button With {
            .Text = "Cancel", .DialogResult = DialogResult.Cancel,
            .Location = New Point(200, 334), .Size = New Size(120, 37)
        }

        Controls.AddRange(New Control() {inputName, inputBarcode, inputRole, inputActive, saveButton, cancel})
        AcceptButton = saveButton
        CancelButton = cancel

        If currentUser IsNot Nothing Then
            inputName.Text = currentUser.FullName
            inputBarcode.Text = currentUser.Barcode
            inputRole.SelectedValue = currentUser.RoleId
            inputActive.Checked = currentUser.IsActive
        End If
    End Sub

    Private Sub AddFieldLabel(caption As String, x As Integer, y As Integer)
        Controls.Add(New Label With {
            .Text = caption, .Location = New Point(x, y), .Size = New Size(425, 25)
        })
    End Sub

    Private Sub SaveUser(sender As Object, e As EventArgs)
        Dim role = TryCast(inputRole.SelectedItem, PaSRole)
        If role Is Nothing Then
            MessageBox.Show("Select a role.")
            Return
        End If
        Try
            If currentUser Is Nothing Then
                UserAdministration.CreateUser(inputName.Text, inputBarcode.Text, role.Id, inputActive.Checked)
            Else
                UserAdministration.UpdateUser(currentUser.Id, inputName.Text,
                                              inputBarcode.Text, role.Id, inputActive.Checked)
            End If
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Unable to save user", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
