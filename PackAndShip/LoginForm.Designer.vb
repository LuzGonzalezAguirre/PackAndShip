<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class LoginForm
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
        Me.titleLabel = New System.Windows.Forms.Label()
        Me.sectionLogin = New System.Windows.Forms.Label()
        Me.barcodeLabel = New System.Windows.Forms.Label()
        Me.inputBarcode = New System.Windows.Forms.TextBox()
        Me.passwordLabel = New System.Windows.Forms.Label()
        Me.inputPassword = New System.Windows.Forms.TextBox()
        Me.sectionStation = New System.Windows.Forms.Label()
        Me.customerCaption = New System.Windows.Forms.Label()
        Me.labelCustomer = New System.Windows.Forms.Label()
        Me.buttonLogin = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        ' All elements are placed directly on the Form with Dock=None.
        ' Move and resize them with the mouse in Visual Studio.
        '
        Me.titleLabel.Name = "titleLabel"
        Me.titleLabel.Text = "Empaque y embarque"
        Me.titleLabel.Location = New System.Drawing.Point(22, 16)
        Me.titleLabel.Size = New System.Drawing.Size(394, 40)
        Me.titleLabel.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold)
        Me.titleLabel.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)

        Me.sectionLogin.Name = "sectionLogin"
        Me.sectionLogin.Text = "Inicio de sesión"
        Me.sectionLogin.Location = New System.Drawing.Point(22, 69)
        Me.sectionLogin.Size = New System.Drawing.Size(394, 23)
        Me.sectionLogin.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.sectionLogin.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)

        Me.barcodeLabel.Name = "barcodeLabel"
        Me.barcodeLabel.Text = "Credencial (código de barras)"
        Me.barcodeLabel.Location = New System.Drawing.Point(22, 104)
        Me.barcodeLabel.Size = New System.Drawing.Size(394, 21)

        Me.inputBarcode.Name = "inputBarcode"
        Me.inputBarcode.Location = New System.Drawing.Point(22, 129)
        Me.inputBarcode.Size = New System.Drawing.Size(394, 29)
        Me.inputBarcode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.inputBarcode.TabIndex = 0

        Me.passwordLabel.Name = "passwordLabel"
        Me.passwordLabel.Text = "Contraseña"
        Me.passwordLabel.Location = New System.Drawing.Point(22, 176)
        Me.passwordLabel.Size = New System.Drawing.Size(394, 21)

        Me.inputPassword.Name = "inputPassword"
        Me.inputPassword.Location = New System.Drawing.Point(22, 201)
        Me.inputPassword.Size = New System.Drawing.Size(394, 29)
        Me.inputPassword.UseSystemPasswordChar = True
        Me.inputPassword.TabIndex = 1

        Me.sectionStation.Name = "sectionStation"
        Me.sectionStation.Text = "Estación de trabajo"
        Me.sectionStation.Location = New System.Drawing.Point(22, 251)
        Me.sectionStation.Size = New System.Drawing.Size(394, 22)
        Me.sectionStation.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.sectionStation.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)

        Me.customerCaption.Name = "customerCaption"
        Me.customerCaption.Text = "Cliente predeterminado"
        Me.customerCaption.Location = New System.Drawing.Point(22, 284)
        Me.customerCaption.Size = New System.Drawing.Size(394, 22)

        Me.labelCustomer.Name = "labelCustomer"
        Me.labelCustomer.Text = "Sin configurar"
        Me.labelCustomer.Location = New System.Drawing.Point(22, 309)
        Me.labelCustomer.Size = New System.Drawing.Size(394, 31)
        Me.labelCustomer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.labelCustomer.BackColor = System.Drawing.Color.FromArgb(242, 247, 252)
        Me.labelCustomer.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.labelCustomer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.labelCustomer.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)

        Me.buttonLogin.Name = "buttonLogin"
        Me.buttonLogin.Text = "Iniciar sesión"
        Me.buttonLogin.Location = New System.Drawing.Point(22, 362)
        Me.buttonLogin.Size = New System.Drawing.Size(394, 46)
        Me.buttonLogin.BackColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.buttonLogin.ForeColor = System.Drawing.Color.White
        Me.buttonLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.buttonLogin.Font = New System.Drawing.Font("Segoe UI", 11.0!, System.Drawing.FontStyle.Bold)
        Me.buttonLogin.TabIndex = 2

        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(440, 438)
        Me.MinimumSize = New System.Drawing.Size(0, 0)
        Me.MaximumSize = New System.Drawing.Size(0, 0)
        Me.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.MaximizeBox = True
        Me.MinimizeBox = True
        Me.Text = "PackAndShip | Inicio de sesión"
        Me.AcceptButton = Me.buttonLogin
        Me.Controls.Add(Me.titleLabel)
        Me.Controls.Add(Me.sectionLogin)
        Me.Controls.Add(Me.barcodeLabel)
        Me.Controls.Add(Me.inputBarcode)
        Me.Controls.Add(Me.passwordLabel)
        Me.Controls.Add(Me.inputPassword)
        Me.Controls.Add(Me.sectionStation)
        Me.Controls.Add(Me.customerCaption)
        Me.Controls.Add(Me.labelCustomer)
        Me.Controls.Add(Me.buttonLogin)
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents titleLabel As System.Windows.Forms.Label
    Friend WithEvents sectionLogin As System.Windows.Forms.Label
    Friend WithEvents barcodeLabel As System.Windows.Forms.Label
    Friend WithEvents inputBarcode As System.Windows.Forms.TextBox
    Friend WithEvents passwordLabel As System.Windows.Forms.Label
    Friend WithEvents inputPassword As System.Windows.Forms.TextBox
    Friend WithEvents sectionStation As System.Windows.Forms.Label
    Friend WithEvents customerCaption As System.Windows.Forms.Label
    Friend WithEvents labelCustomer As System.Windows.Forms.Label
    Friend WithEvents buttonLogin As System.Windows.Forms.Button
End Class
