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
        Text = If(cancelEmpty, "PackAndShip | Cancel Empty Pallet Authorization",
                  "PackAndShip | Partial Pallet Authorization")
        heading.Text = If(cancelEmpty, "CANCEL EMPTY PALLET — ", "PARTIAL PALLET — ") & code
        info.Text = If(cancelEmpty,
            "No pieces recorded. Cancelling reserves this container code permanently.",
            "Registered: " & actual & " / " & required & " pieces.") &
            Environment.NewLine & "A different, non-Operator user must authorize."
        reasonLabel.Text = If(cancelEmpty, "Cancellation reason (minimum 10 characters)",
                              "Reason for partial closure (minimum 10 characters)")
        confirm.Text = If(cancelEmpty, "Authorize & Cancel Pallet", "Authorize & Close Partial")
    End Sub

    Private Sub Authorize(sender As Object, e As EventArgs)
        If barcodeInput.Text.Trim() = "" OrElse passwordInput.Text = "" OrElse
           reasonInput.Text.Trim().Length < 10 Then
            MessageBox.Show("Enter authorizer credentials and a reason of at least 10 characters.",
                            "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Try
            Dim user = PackAndShipData.Authenticate(
                barcodeInput.Text.Trim().ToUpperInvariant(), passwordInput.Text)
            If user Is Nothing Then
                Throw New UnauthorizedAccessException("Invalid or inactive authorizer credentials.")
            End If
            If user.Id = SessionContext.UserId Then
                Throw New UnauthorizedAccessException("A second user must authorize the partial pallet.")
            End If
            Dim role = user.RoleName.Trim()
            If role.IndexOf("Operator", StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               role.IndexOf("Operador", StringComparison.OrdinalIgnoreCase) >= 0 Then
                Throw New UnauthorizedAccessException("Operators cannot authorize partial pallets.")
            End If
            _authorizedUserId = user.Id
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Authorization denied",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
