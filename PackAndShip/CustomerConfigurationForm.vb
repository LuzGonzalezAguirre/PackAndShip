Imports System.Drawing
Imports System.Windows.Forms

' La presentación se mantiene en CustomerConfigurationForm.Designer.vb.
' No se crean controles, tamaños ni posiciones en este archivo.
Public Partial Class CustomerConfigurationForm
    Inherits Form

    Public ReadOnly Property SelectedCustomer As BusinessUnit
        Get
            Return TryCast(customerPicker.SelectedItem, BusinessUnit)
        End Get
    End Property

    Public Sub New()
        InitializeComponent()
        AddHandler saveButton.Click, AddressOf SaveSelection
    End Sub

    Public Sub New(customers As List(Of BusinessUnit), selectedCustomerId As Integer)
        Me.New()
        customerPicker.DisplayMember = "Name"
        customerPicker.ValueMember = "Id"
        customerPicker.DataSource = New List(Of BusinessUnit)(customers)
        If selectedCustomerId > 0 Then
            Dim previous = customers.Find(Function(item) item.Id = selectedCustomerId)
            If previous IsNot Nothing Then customerPicker.SelectedValue = previous.Id
        End If
    End Sub

    Private Sub SaveSelection(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Tu usuario no tiene permisos para cambiar el cliente.",
                            "Acceso denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If StationSettings.ReadLockedPallet() <> "" Then
            MessageBox.Show("Debes finalizar el pallet activo antes de cambiar el cliente de esta estación.",
                            "Pallet activo", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        If SelectedCustomer Is Nothing Then
            MessageBox.Show("Selecciona un cliente antes de guardar.",
                            "Cliente obligatorio", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            customerPicker.Focus()
            Return
        End If

        DialogResult = DialogResult.OK
        Close()
    End Sub
End Class
