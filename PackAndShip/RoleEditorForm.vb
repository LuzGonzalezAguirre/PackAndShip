Imports System.Drawing
Imports System.Windows.Forms

' Formulario para agregar o editar roles. Todos los controles se crean en Designer.vb.
Public Partial Class RoleEditorForm
    Inherits Form

    Private ReadOnly originalRole As PaSRole

    Public Sub New()
        InitializeComponent()
        AddHandler saveButton.Click, AddressOf SaveRole
    End Sub

    Public Sub New(role As PaSRole)
        Me.New()
        originalRole = role
        If role IsNot Nothing Then
            Text = "PackAndShip | Editar rol"
            titleLabel.Text = "Editar rol"
            nameInput.Text = role.Name
            setupCheckbox.Checked = role.CanModifySetup
        End If
    End Sub

    Private Sub SaveRole(sender As Object, e As EventArgs)
        Try
            If originalRole Is Nothing Then
                UserAdministration.CreateRole(nameInput.Text, setupCheckbox.Checked)
            Else
                UserAdministration.UpdateRole(originalRole.Id, nameInput.Text,
                                              setupCheckbox.Checked)
            End If
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "No se pudo guardar el rol",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
