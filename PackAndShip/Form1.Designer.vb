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
        Me.menuPalletActions = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuPartialPallet = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuCancelEmptyPallet = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuReviewPendingSql = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuHistory = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuTransfer = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuHelp = New System.Windows.Forms.ToolStripMenuItem()
        Me.helpAboutItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.menuLogout = New System.Windows.Forms.ToolStripMenuItem()
        Me.appHeader = New System.Windows.Forms.Panel()
        Me.headerTitle = New System.Windows.Forms.Label()
        Me.signedInPanel = New System.Windows.Forms.Panel()
        Me.scannerView = New PalletScannerView()
        Me.connectionStatus = New System.Windows.Forms.StatusStrip()
        Me.connectionLabel = New System.Windows.Forms.ToolStripStatusLabel()
        Me.mainMenu.SuspendLayout()
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
            Me.menuSetup, Me.menuPalletActions, Me.menuHistory, Me.menuTransfer, Me.menuHelp, Me.menuLogout})
        Me.menuSetup.Text = "Setup"
        Me.menuSetup.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {
            Me.setupCustomerItem, Me.setupUsersItem, Me.setupPalletItem,
            Me.setupRulesItem, Me.setupGeneralItem})
        Me.setupCustomerItem.Text = "Customer Configuration"
        Me.setupUsersItem.Text = "Users & Roles"
        Me.setupPalletItem.Text = "Pallet Configuration"
        Me.setupRulesItem.Text = "Scan Rules"
        Me.setupGeneralItem.Text = "General Settings"
        Me.menuPalletActions.Text = "Pallet Actions"
        Me.menuPalletActions.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {
            Me.menuPartialPallet, Me.menuCancelEmptyPallet, Me.menuReviewPendingSql})
        Me.menuPartialPallet.Text = "Partial pallet (authorization)"
        Me.menuCancelEmptyPallet.Text = "Cancel empty pallet (authorization)"
        Me.menuReviewPendingSql.Text = "Review pending SQL"
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
        Me.ClientSize = New System.Drawing.Size(1200, 760)
        Me.Font = New System.Drawing.Font("Segoe UI", 11.0!)
        Me.MainMenuStrip = Me.mainMenu
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Pack and Ship | SSI"
        Me.Controls.Add(Me.signedInPanel)
        Me.Controls.Add(Me.appHeader)
        Me.Controls.Add(Me.connectionStatus)
        Me.Controls.Add(Me.mainMenu)
        Me.mainMenu.ResumeLayout(False)
        Me.signedInPanel.ResumeLayout(False)
        Me.connectionStatus.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents mainMenu As System.Windows.Forms.MenuStrip
    Friend WithEvents menuSetup As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents setupCustomerItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents setupUsersItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents setupPalletItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents setupRulesItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents setupGeneralItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuPalletActions As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuPartialPallet As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuCancelEmptyPallet As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuReviewPendingSql As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuHistory As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuTransfer As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuHelp As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents helpAboutItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents menuLogout As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents appHeader As System.Windows.Forms.Panel
    Friend WithEvents headerTitle As System.Windows.Forms.Label
    Friend WithEvents signedInPanel As System.Windows.Forms.Panel
    Friend WithEvents scannerView As PalletScannerView
    Friend WithEvents connectionStatus As System.Windows.Forms.StatusStrip
    Friend WithEvents connectionLabel As System.Windows.Forms.ToolStripStatusLabel
End Class
