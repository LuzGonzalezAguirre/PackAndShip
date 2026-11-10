Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

Public Class UsersSetupForm
    Inherits Form

    Private ReadOnly navy As Color = Color.FromArgb(31, 78, 121)









    Private allUsers As New List(Of PaSUser)()
    Private roles As New List(Of PaSRole)()

    Public Sub New()
        InitializeComponent()
        BindEvents()
        AddHandler Shown, Sub() LoadData()
    End Sub

    Private Sub BindEvents()
        AddHandler newButton.Click, Sub() OpenEditor(Nothing)
        AddHandler editButton.Click, AddressOf EditSelectedUser
        AddHandler statusButton.Click, AddressOf ToggleStatus
        AddHandler resetButton.Click, AddressOf ResetPassword
        AddHandler reloadButton.Click, Sub() LoadData()
        AddHandler searchBox.TextChanged, Sub() RenderRows()
        AddHandler roleFilter.SelectedIndexChanged, Sub() RenderRows()
        AddHandler usersGrid.CellDoubleClick, Sub(sender, e)
            If e.RowIndex >= 0 Then
                Dim selectedUser = TryCast(usersGrid.Rows(e.RowIndex).DataBoundItem, PaSUser)
                If selectedUser IsNot Nothing Then OpenEditor(selectedUser)
            End If
        End Sub
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
            Dim previousRole As Integer = 0
            Dim previouslySelected = TryCast(roleFilter.SelectedItem, PaSRole)
            If previouslySelected IsNot Nothing Then previousRole = previouslySelected.Id

            Dim filterOptions As New List(Of PaSRole) From {
                New PaSRole With {.Id = 0, .Name = "Todos los roles"}
            }
            filterOptions.AddRange(roles)
            roleFilter.DataSource = filterOptions
            roleFilter.SelectedValue = previousRole
            If roleFilter.SelectedIndex < 0 Then roleFilter.SelectedIndex = 0
            RenderRows()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "No se pudieron cargar los usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RenderRows()
        Dim term = searchBox.Text.Trim()
        Dim chosenRole = TryCast(roleFilter.SelectedItem, PaSRole)
        Dim roleId As Integer = If(chosenRole Is Nothing, 0, chosenRole.Id)
        Dim filtered = allUsers.Where(Function(u) (roleId = 0 OrElse u.RoleId = roleId) AndAlso
            (term = "" OrElse
             u.FullName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
             u.Barcode.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
             u.RoleName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)).ToList()
        usersGrid.DataSource = Nothing
        usersGrid.DataSource = filtered
        countLabel.Text = filtered.Count.ToString() & " of " & allUsers.Count.ToString() & " usuarios"
        usersGrid.ClearSelection()
    End Sub

    Private Function SelectedUser() As PaSUser
        If usersGrid.SelectedRows.Count = 0 Then Return Nothing
        Return TryCast(usersGrid.SelectedRows(0).DataBoundItem, PaSUser)
    End Function

    Private Sub EditSelectedUser(sender As Object, e As EventArgs)
        Dim user = SelectedUser()
        If user Is Nothing Then
            MessageBox.Show("Select a user first.", "Editar usuario",
                            MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        OpenEditor(user)
    End Sub

    Private Sub OpenEditor(user As PaSUser)
        Try
            Using dialog As New UserEditorDialog(user, roles)
                If dialog.ShowDialog(Me) = DialogResult.OK Then LoadData()
            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Editor de usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
        If MessageBox.Show("¿Deseas " & action & " " & user.FullName & "?",
                           "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        Try
            UserAdministration.SetUserActive(user.Id, desired)
            LoadData()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "No se pudo cambiar el estado", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ResetPassword(sender As Object, e As EventArgs)
        Dim user = SelectedUser()
        If user Is Nothing Then
            MessageBox.Show("Select a user first.")
            Return
        End If
        If MessageBox.Show("¿Restablecer contraseña de " & user.FullName & " a su código de barras?",
                           "Confirmar restablecimiento de contraseña", MessageBoxButtons.YesNo,
                           MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
        Try
            UserAdministration.ResetPasswordToBarcode(user.Id)
            MessageBox.Show("Contraseña restablecida correctamente.")
        Catch ex As Exception
            MessageBox.Show(ex.Message, "No se pudo restablecer la contraseña", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
