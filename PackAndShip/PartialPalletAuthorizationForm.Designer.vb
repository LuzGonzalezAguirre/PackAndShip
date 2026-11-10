<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PartialPalletAuthorizationForm
    Inherits System.Windows.Forms.Form
    Private components As System.ComponentModel.IContainer
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.barcodeInput = New System.Windows.Forms.TextBox()
        Me.passwordInput = New System.Windows.Forms.TextBox()
        Me.reasonInput = New System.Windows.Forms.TextBox()
        Me.info = New System.Windows.Forms.Label()
        Me.heading = New System.Windows.Forms.Label()
        Me.barcodeLabel = New System.Windows.Forms.Label()
        Me.passwordLabel = New System.Windows.Forms.Label()
        Me.reasonLabel = New System.Windows.Forms.Label()
        Me.confirm = New System.Windows.Forms.Button()
        Me.cancel = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        Me.heading.Text = "PALLET PARCIAL"
        Me.heading.Location = New System.Drawing.Point(20, 12)
        Me.heading.Size = New System.Drawing.Size(485, 38)
        Me.info.Text = "Las piezas registradas requieren autorización adicional."
        Me.info.Location = New System.Drawing.Point(22, 56)
        Me.info.Size = New System.Drawing.Size(485, 45)
        Me.barcodeLabel.Text = "Credencial de quien autoriza"
        Me.barcodeLabel.Location = New System.Drawing.Point(22, 114)
        Me.barcodeLabel.Size = New System.Drawing.Size(380, 23)
        Me.barcodeInput.Location = New System.Drawing.Point(22, 137)
        Me.barcodeInput.Size = New System.Drawing.Size(480, 29)
        Me.passwordLabel.Text = "Contraseña de quien autoriza"
        Me.passwordLabel.Location = New System.Drawing.Point(22, 178)
        Me.passwordLabel.Size = New System.Drawing.Size(380, 23)
        Me.passwordInput.Location = New System.Drawing.Point(22, 201)
        Me.passwordInput.Size = New System.Drawing.Size(480, 29)
        Me.reasonLabel.Text = "Motivo del cierre parcial (mínimo 10 caracteres)"
        Me.reasonLabel.Location = New System.Drawing.Point(22, 242)
        Me.reasonLabel.Size = New System.Drawing.Size(480, 24)
        Me.reasonInput.Location = New System.Drawing.Point(22, 266)
        Me.reasonInput.Size = New System.Drawing.Size(480, 69)
        Me.confirm.Text = "Autorizar y cerrar parcial"
        Me.confirm.Location = New System.Drawing.Point(267, 351)
        Me.confirm.Size = New System.Drawing.Size(235, 38)
        Me.cancel.Text = "Cancelar"
        Me.cancel.Location = New System.Drawing.Point(135, 351)
        Me.cancel.Size = New System.Drawing.Size(118, 38)
        Me.heading.Font = New System.Drawing.Font("Segoe UI", 16.0!, System.Drawing.FontStyle.Bold)
        Me.heading.ForeColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.info.ForeColor = System.Drawing.Color.DarkRed
        Me.barcodeInput.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.barcodeInput.MaxLength = 50
        Me.passwordInput.MaxLength = 50
        Me.passwordInput.UseSystemPasswordChar = True
        Me.reasonInput.MaxLength = 500
        Me.reasonInput.Multiline = True
        Me.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.AcceptButton = Me.confirm
        Me.CancelButton = Me.cancel
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(530, 405)
        Me.MinimumSize = New System.Drawing.Size(0,0)
        Me.MaximumSize = New System.Drawing.Size(0,0)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.BackColor = System.Drawing.Color.FromArgb(244,247,250)
        Me.Text = "PackAndShip | Autorización de pallet"
        Me.Controls.Add(Me.barcodeInput)
        Me.Controls.Add(Me.passwordInput)
        Me.Controls.Add(Me.reasonInput)
        Me.Controls.Add(Me.info)
        Me.Controls.Add(Me.heading)
        Me.Controls.Add(Me.barcodeLabel)
        Me.Controls.Add(Me.passwordLabel)
        Me.Controls.Add(Me.reasonLabel)
        Me.Controls.Add(Me.confirm)
        Me.Controls.Add(Me.cancel)
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub
    Friend WithEvents barcodeInput As System.Windows.Forms.TextBox
    Friend WithEvents passwordInput As System.Windows.Forms.TextBox
    Friend WithEvents reasonInput As System.Windows.Forms.TextBox
    Friend WithEvents info As System.Windows.Forms.Label
    Friend WithEvents heading As System.Windows.Forms.Label
    Friend WithEvents barcodeLabel As System.Windows.Forms.Label
    Friend WithEvents passwordLabel As System.Windows.Forms.Label
    Friend WithEvents reasonLabel As System.Windows.Forms.Label
    Friend WithEvents confirm As System.Windows.Forms.Button
    Friend WithEvents cancel As System.Windows.Forms.Button
End Class
