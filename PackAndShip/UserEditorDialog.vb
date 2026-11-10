Imports System.Drawing
Imports System.Windows.Forms

Friend Partial Class UserEditorDialog
    Inherits Form

    Private ReadOnly currentUser As PaSUser

    Public Sub New()
        InitializeComponent()
        AddHandler saveButton.Click, AddressOf SaveUser
    End Sub

    Public Sub New(user As PaSUser, roles As List(Of PaSRole))
        Me.New()
        currentUser = user
        Text = If(user Is Nothing, "Usuario nuevo", "Editar usuario")
        heading.Text = If(user Is Nothing, "Agregar empleado", "Editar empleado")
        inputRole.DataSource = roles
        inputRole.DisplayMember = "Name"
        inputRole.ValueMember = "Id"
        If user IsNot Nothing Then
            inputName.Text = user.FullName
            inputBarcode.Text = user.Barcode
            inputRole.SelectedValue = user.RoleId
            inputActive.Checked = user.IsActive
        End If
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
            MessageBox.Show(ex.Message, "No se pudo guardar el usuario", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
