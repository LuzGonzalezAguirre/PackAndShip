<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
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
        Me.components = New System.ComponentModel.Container()
        Me.mainMenu = New System.Windows.Forms.MenuStrip()
        Me.menuSetup = New System.Windows.Forms.ToolStripMenuItem()
        Me.setupCustomerItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.setupUsersItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.setupPalletItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.setupRulesItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.setupGeneralItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuHistory = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuTransfer = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuHelp = New System.Windows.Forms.ToolStripMenuItem()
        Me.helpAboutItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuLogout = New System.Windows.Forms.ToolStripMenuItem()
        Me.appHeader = New System.Windows.Forms.Panel()
        Me.headerTitle = New System.Windows.Forms.Label()
        Me.loginPanel = New System.Windows.Forms.Panel()
        Me.loginCard = New System.Windows.Forms.Panel()
        Me.loginCardHeader = New System.Windows.Forms.Panel()
        Me.loginTitle = New System.Windows.Forms.Label()
        Me.loginCardFooter = New System.Windows.Forms.Panel()
        Me.buttonLogin = New System.Windows.Forms.Button()
        Me.sectionLogin = New System.Windows.Forms.Label()
        Me.barcodeLabel = New System.Windows.Forms.Label()
        Me.inputBarcode = New System.Windows.Forms.TextBox()
        Me.passwordLabel = New System.Windows.Forms.Label()
        Me.inputPassword = New System.Windows.Forms.TextBox()
        Me.sectionStation = New System.Windows.Forms.Label()
        Me.customerCaption = New System.Windows.Forms.Label()
        Me.labelCustomer = New System.Windows.Forms.Label()
        Me.labelStatus = New System.Windows.Forms.Label()
        Me.labelWelcome = New System.Windows.Forms.Label()
        Me.buttonLogout = New System.Windows.Forms.Button()
        Me.signedInPanel = New System.Windows.Forms.Panel()
        Me.scannerView = New PalletScannerView()
        Me.connectionStatus = New System.Windows.Forms.StatusStrip()
        Me.connectionLabel = New System.Windows.Forms.ToolStripStatusLabel()
        Me.mainMenu.SuspendLayout()
        Me.loginPanel.SuspendLayout()
        Me.loginCard.SuspendLayout()
        Me.loginCardHeader.SuspendLayout()
        Me.loginCardFooter.SuspendLayout()
        Me.signedInPanel.SuspendLayout()
        Me.connectionStatus.SuspendLayout()
        Me.SuspendLayout()

        ' MenuStrip, its menus, and their entries are all editable in Designer.
        Me.mainMenu.AutoSize = False
        Me.mainMenu.Dock = System.Windows.Forms.DockStyle.Top
        Me.mainMenu.Height = 34
        Me.mainMenu.BackColor = System.Drawing.Color.White
        Me.mainMenu.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.mainMenu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.mainMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {
            Me.menuSetup, Me.menuHistory, Me.menuTransfer, Me.menuHelp, Me.menuLogout})
        Me.menuSetup.Text = "Setup"
        Me.menuSetup.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {
            Me.setupCustomerItem, Me.setupUsersItem, Me.setupPalletItem,
            Me.setupRulesItem, Me.setupGeneralItem})
        Me.setupCustomerItem.Text = "Customer Configuration"
        Me.setupUsersItem.Text = "Users & Roles"
        Me.setupPalletItem.Text = "Pallet Configuration"
        Me.setupRulesItem.Text = "Scan Rules"
        Me.setupGeneralItem.Text = "General Settings"
        Me.menuHistory.Text = "Pallet History"
        Me.menuTransfer.Text = "Pallet Transfers"
        Me.menuHelp.Text = "Ayuda"
        Me.menuHelp.DropDownItems.Add(Me.helpAboutItem)
        Me.helpAboutItem.Text = "About PackAndShip"
        Me.menuLogout.Text = "Cerrar sesión"

        Me.appHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.appHeader.Height = 84
        Me.appHeader.BackColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.headerTitle.Text = "PACK AND SHIP"
        Me.headerTitle.AutoSize = True
        Me.headerTitle.Location = New System.Drawing.Point(24, 19)
        Me.headerTitle.Font = New System.Drawing.Font("Segoe UI", 23.0!, System.Drawing.FontStyle.Bold)
        Me.headerTitle.ForeColor = System.Drawing.Color.White
        Me.appHeader.Controls.Add(Me.headerTitle)

        ' Login is a real set of nested designer controls.
        Me.loginPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.loginPanel.BackColor = System.Drawing.Color.White
        Me.loginPanel.Visible = False
        Me.loginCard.Dock = System.Windows.Forms.DockStyle.Fill
        Me.loginCard.BackColor = System.Drawing.Color.White
        Me.loginCardHeader.Dock = System.Windows.Forms.DockStyle.Top
        Me.loginCardHeader.Height = 50
        Me.loginCardHeader.BackColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.loginTitle.Text = "Pack and Ship"
        Me.loginTitle.Dock = System.Windows.Forms.DockStyle.Fill
        Me.loginTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.loginTitle.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold)
        Me.loginTitle.ForeColor = System.Drawing.Color.White
        Me.loginCardHeader.Controls.Add(Me.loginTitle)
        Me.loginCardFooter.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.loginCardFooter.Height = 50
        Me.loginCardFooter.BackColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.buttonLogin.Text = "Iniciar sesión"
        Me.buttonLogin.Dock = System.Windows.Forms.DockStyle.Fill
        Me.buttonLogin.BackColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.buttonLogin.ForeColor = System.Drawing.Color.White
        Me.buttonLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.buttonLogin.Font = New System.Drawing.Font("Segoe UI", 11.0!, System.Drawing.FontStyle.Bold)
        Me.loginCardFooter.Controls.Add(Me.buttonLogin)

        Me.sectionLogin.Text = "Inicio de sesión"
        Me.sectionLogin.Location = New System.Drawing.Point(14, 57)
        Me.sectionLogin.Size = New System.Drawing.Size(394, 20)
        Me.sectionLogin.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.sectionLogin.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.barcodeLabel.Text = "Credencial (Barcode ID)"
        Me.barcodeLabel.Location = New System.Drawing.Point(14, 79)
        Me.barcodeLabel.Size = New System.Drawing.Size(394, 18)
        Me.inputBarcode.Location = New System.Drawing.Point(14, 99)
        Me.inputBarcode.Size = New System.Drawing.Size(394, 27)
        Me.inputBarcode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.inputBarcode.TabIndex = 0
        Me.passwordLabel.Text = "Password"
        Me.passwordLabel.Location = New System.Drawing.Point(14, 133)
        Me.passwordLabel.Size = New System.Drawing.Size(394, 18)
        Me.inputPassword.Location = New System.Drawing.Point(14, 153)
        Me.inputPassword.Size = New System.Drawing.Size(394, 27)
        Me.inputPassword.UseSystemPasswordChar = True
        Me.inputPassword.TabIndex = 1
        Me.sectionStation.Text = "Estación de trabajo"
        Me.sectionStation.Location = New System.Drawing.Point(14, 198)
        Me.sectionStation.Size = New System.Drawing.Size(394, 20)
        Me.sectionStation.Font = New System.Drawing.Font("Segoe UI", 9.0!, System.Drawing.FontStyle.Bold)
        Me.sectionStation.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.customerCaption.Text = "Cliente predeterminado"
        Me.customerCaption.Location = New System.Drawing.Point(14, 222)
        Me.customerCaption.Size = New System.Drawing.Size(394, 18)
        Me.labelCustomer.Location = New System.Drawing.Point(14, 242)
        Me.labelCustomer.Size = New System.Drawing.Size(394, 32)
        Me.labelCustomer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.labelCustomer.BackColor = System.Drawing.Color.FromArgb(242, 247, 252)
        Me.labelCustomer.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.labelCustomer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.labelCustomer.Font = New System.Drawing.Font("Segoe UI", 11.0!, System.Drawing.FontStyle.Bold)
        Me.loginCard.Controls.AddRange(New System.Windows.Forms.Control() {
            Me.sectionLogin, Me.barcodeLabel, Me.inputBarcode, Me.passwordLabel,
            Me.inputPassword, Me.sectionStation, Me.customerCaption, Me.labelCustomer,
            Me.loginCardHeader, Me.loginCardFooter})
        Me.loginPanel.Controls.Add(Me.loginCard)

        ' Scanner is a separate editable UserControl: open its Designer
        ' to edit nested QR fields, progress, header and tiles individually.
        Me.signedInPanel.Dock = System.Windows.Forms.DockStyle.Fill
        Me.signedInPanel.Visible = True
        Me.scannerView.Dock = System.Windows.Forms.DockStyle.Fill
        Me.signedInPanel.Controls.Add(Me.scannerView)

        Me.connectionStatus.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.connectionStatus.SizingGrip = False
        Me.connectionStatus.Items.Add(Me.connectionLabel)
        Me.connectionLabel.Text = "CCS: checking connection"

        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(244, 247, 250)
        Me.ClientSize = New System.Drawing.Size(1100, 720)
        Me.Font = New System.Drawing.Font("Segoe UI", 11.0!)
        Me.MainMenuStrip = Me.mainMenu
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Pack and Ship | SSI"
        Me.Controls.Add(Me.loginPanel)
        Me.Controls.Add(Me.signedInPanel)
        Me.Controls.Add(Me.appHeader)
        Me.Controls.Add(Me.connectionStatus)
        Me.Controls.Add(Me.mainMenu)
        Me.mainMenu.ResumeLayout(False)
        Me.loginPanel.ResumeLayout(False)
        Me.loginCard.ResumeLayout(False)
        Me.loginCardHeader.ResumeLayout(False)
        Me.loginCardFooter.ResumeLayout(False)
        Me.signedInPanel.ResumeLayout(False)
        Me.connectionStatus.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents inputBarcode As System.Windows.Forms.TextBox
    Friend WithEvents inputPassword As System.Windows.Forms.TextBox
    Friend WithEvents labelCustomer As System.Windows.Forms.Label
    Friend WithEvents labelStatus As System.Windows.Forms.Label
    Friend WithEvents labelWelcome As System.Windows.Forms.Label
    Friend WithEvents buttonLogin As System.Windows.Forms.Button
    Friend WithEvents buttonLogout As System.Windows.Forms.Button
    Friend WithEvents mainMenu As System.Windows.Forms.MenuStrip
    Friend WithEvents menuSetup As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents setupCustomerItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents setupUsersItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents setupPalletItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents setupRulesItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents setupGeneralItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuHistory As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuTransfer As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuHelp As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents helpAboutItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuLogout As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents appHeader As System.Windows.Forms.Panel
    Friend WithEvents headerTitle As System.Windows.Forms.Label
    Friend WithEvents loginPanel As System.Windows.Forms.Panel
    Friend WithEvents loginCard As System.Windows.Forms.Panel
    Friend WithEvents loginCardHeader As System.Windows.Forms.Panel
    Friend WithEvents loginTitle As System.Windows.Forms.Label
    Friend WithEvents loginCardFooter As System.Windows.Forms.Panel
    Friend WithEvents sectionLogin As System.Windows.Forms.Label
    Friend WithEvents barcodeLabel As System.Windows.Forms.Label
    Friend WithEvents passwordLabel As System.Windows.Forms.Label
    Friend WithEvents sectionStation As System.Windows.Forms.Label
    Friend WithEvents customerCaption As System.Windows.Forms.Label
    Friend WithEvents signedInPanel As System.Windows.Forms.Panel
    Friend WithEvents scannerView As PalletScannerView
    Friend WithEvents connectionStatus As System.Windows.Forms.StatusStrip
    Friend WithEvents connectionLabel As System.Windows.Forms.ToolStripStatusLabel
End Class
