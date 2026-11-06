Imports System.Drawing
Imports System.Windows.Forms

' Second-person authorization; SQL validates credentials independently.
Public Class PartialPalletAuthorizationForm
    Inherits Form

    Private ReadOnly barcodeInput As New TextBox()
    Private ReadOnly passwordInput As New TextBox()
    Private ReadOnly reasonInput As New TextBox()
    Private ReadOnly info As New Label()

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

    Public Sub New(code As String, actual As Integer, required As Integer)
        Text = "PackAndShip | Partial Pallet Authorization"
        ClientSize = New Size(530, 405)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        StartPosition = FormStartPosition.CenterParent
        Font = New Font("Segoe UI", 10)
        BackColor = PaSTheme.Background

        Dim heading As New Label With {
            .Text = "PARTIAL PALLET — " & code,
            .Font = New Font("Segoe UI", 16, FontStyle.Bold),
            .ForeColor = PaSTheme.Navy, .Location = New Point(20, 12),
            .Size = New Size(485, 38)
        }
        info.Text = "Registered: " & actual & " / " & required &
            " pieces. A different, non-Operator user must authorize."
        info.SetBounds(22, 56, 485, 45)
        info.ForeColor = Color.DarkRed

        Dim barcodeLabel As New Label With {.Text = "Authorizer Barcode ID", .Location = New Point(22, 114), .AutoSize = True}
        barcodeInput.SetBounds(22, 137, 480, 29)
        barcodeInput.CharacterCasing = CharacterCasing.Upper
        barcodeInput.MaxLength = 50

        Dim passwordLabel As New Label With {.Text = "Authorizer Password", .Location = New Point(22, 178), .AutoSize = True}
        passwordInput.SetBounds(22, 201, 480, 29)
        passwordInput.MaxLength = 50
        passwordInput.UseSystemPasswordChar = True

        Dim reasonLabel As New Label With {.Text = "Reason for partial closure (minimum 10 characters)", .Location = New Point(22, 242), .AutoSize = True}
        reasonInput.SetBounds(22, 266, 480, 69)
        reasonInput.Multiline = True
        reasonInput.MaxLength = 500

        Dim confirm As New Button With {.Text = "Authorize & Close Partial", .Size = New Size(235, 38),
                                        .Location = New Point(267, 351)}
        Dim cancel As New Button With {.Text = "Cancel", .Size = New Size(118, 38),
                                       .Location = New Point(135, 351), .DialogResult = DialogResult.Cancel}
        AddHandler confirm.Click, AddressOf Authorize

        Controls.AddRange(New Control() {
            heading, info, barcodeLabel, barcodeInput, passwordLabel,
            passwordInput, reasonLabel, reasonInput, confirm, cancel
        })
        AcceptButton = confirm
        CancelButton = cancel
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
