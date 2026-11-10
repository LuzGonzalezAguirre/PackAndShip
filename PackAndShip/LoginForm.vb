Imports System.Drawing
Imports System.Windows.Forms

' Independent, freely resizable login window.
' Visual controls are owned by LoginForm.Designer.vb.
Public Partial Class LoginForm
    Inherits Form

    Public Sub New()
        InitializeComponent()
        AddHandler buttonLogin.Click, AddressOf PerformLogin
        AddHandler inputBarcode.KeyDown, AddressOf BarcodeEnter
        AddHandler inputPassword.KeyDown, AddressOf PasswordEnter
        AddHandler Shown, Sub() inputBarcode.Focus()
    End Sub

    Public Sub New(customerName As String)
        Me.New()
        labelCustomer.Text = If(String.IsNullOrWhiteSpace(customerName), "Sin configurar", customerName)
    End Sub

    Private Sub BarcodeEnter(sender As Object, e As KeyEventArgs)
        If e.KeyCode <> Keys.Enter Then Return
        e.SuppressKeyPress = True
        inputPassword.Focus()
    End Sub

    Private Sub PasswordEnter(sender As Object, e As KeyEventArgs)
        If e.KeyCode <> Keys.Enter Then Return
        e.SuppressKeyPress = True
        buttonLogin.Focus()
    End Sub

    Private Sub PerformLogin(sender As Object, e As EventArgs)
        Dim barcode = inputBarcode.Text.Trim().ToUpperInvariant()
        Dim password = inputPassword.Text
        If barcode = "" OrElse password = "" Then
            MessageBox.Show("Ingresa la credencial y contraseña.", "Campos obligatorios",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim user = PackAndShipData.Authenticate(barcode, password)
            If user Is Nothing Then
                MessageBox.Show("Credenciales inválidas o usuario inactivo.", "Acceso denegado",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
                inputPassword.Clear()
                inputPassword.Focus()
                Return
            End If

            SessionContext.UserId = user.Id
            SessionContext.FullName = user.Name
            SessionContext.BarcodeId = user.Barcode
            SessionContext.RoleId = user.RoleId
            SessionContext.RoleName = user.RoleName
            SessionContext.CanModifySetup = user.CanModifySetup
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Error de inicio de sesión", MessageBoxButtons.OK,
                            MessageBoxIcon.Error)
        End Try
    End Sub
End Class
