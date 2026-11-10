Imports System.Drawing
Imports System.Windows.Forms

' Second-person authorization; SQL validates credentials independently.
Public Class PartialPalletAuthorizationForm
    Inherits Form


    Public ReadOnly Property AuthorizedUserId As Integer
        Get
            Return _authorizedUserId
        End Get
    End Property
    Private _authorizedUserId As Integer

    Public ReadOnly Property AuthorizedBarcode As String
        Get
            Return barcodeInput.Text.Trim().ToUpperInvariant()
        End Get
    End Property
    Public ReadOnly Property AuthorizedPassword As String
        Get
            Return passwordInput.Text
        End Get
    End Property
    Public ReadOnly Property Reason As String
        Get
            Return reasonInput.Text.Trim()
        End Get
    End Property

    Public Sub New()
        InitializeComponent()
        AddHandler confirm.Click, AddressOf Authorize
    End Sub

    Public Sub New(code As String, actual As Integer, required As Integer,
                   Optional cancelEmpty As Boolean = False)
        Me.New()
        Text = If(cancelEmpty, "PackAndShip | Autorización para cancelar pallet vacío",
                  "PackAndShip | Autorización de pallet parcial")
        heading.Text = If(cancelEmpty, "CANCELAR PALLET VACÍO — ", "PALLET PARCIAL — ") & code
        info.Text = If(cancelEmpty,
            "No hay piezas registradas. Al cancelar se reserva este código de contenedor permanentemente.",
            "Registradas: " & actual & " / " & required & " piezas.") &
            Environment.NewLine & "Debe autorizar otra persona que no tenga rol de Operador."
        reasonLabel.Text = If(cancelEmpty, "Motivo de cancelación (mínimo 10 caracteres)",
                              "Motivo de cierre parcial (mínimo 10 caracteres)")
        confirm.Text = If(cancelEmpty, "Autorizar y cancelar pallet", "Autorizar y cerrar parcial")
    End Sub

    Private Sub Authorize(sender As Object, e As EventArgs)
        If barcodeInput.Text.Trim() = "" OrElse passwordInput.Text = "" OrElse
           reasonInput.Text.Trim().Length < 10 Then
            MessageBox.Show("Ingresa las credenciales de autorización y un motivo de al menos 10 caracteres.",
                            "Obligatorio", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Try
            Dim user = PackAndShipData.Authenticate(
                barcodeInput.Text.Trim().ToUpperInvariant(), passwordInput.Text)
            If user Is Nothing Then
                Throw New UnauthorizedAccessException("Credenciales de autorización incorrectas o usuario inactivo.")
            End If
            If user.Id = SessionContext.UserId Then
                Throw New UnauthorizedAccessException("Una segunda persona debe autorizar el pallet parcial.")
            End If
            Dim role = user.RoleName.Trim()
            If role.IndexOf("Operator", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               role.IndexOf("Operador", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Throw New UnauthorizedAccessException("Los operadores no pueden autorizar pallets parciales.")
            End If
            _authorizedUserId = user.Id
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Autorización denegada",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
