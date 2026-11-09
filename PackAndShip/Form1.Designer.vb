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
        mainMenu = New MenuStrip()
        menuSetup = New ToolStripMenuItem()
        setupCustomerItem = New ToolStripMenuItem()
        setupUsersItem = New ToolStripMenuItem()
        setupPalletItem = New ToolStripMenuItem()
        setupRulesItem = New ToolStripMenuItem()
        setupGeneralItem = New ToolStripMenuItem()
        menuPalletActions = New ToolStripMenuItem()
        menuPartialPallet = New ToolStripMenuItem()
        menuCancelEmptyPallet = New ToolStripMenuItem()
        menuReviewPendingSql = New ToolStripMenuItem()
        menuHistory = New ToolStripMenuItem()
        menuTransfer = New ToolStripMenuItem()
        menuHelp = New ToolStripMenuItem()
        helpAboutItem = New ToolStripMenuItem()
        menuLogout = New ToolStripMenuItem()
        appHeader = New Panel()
        headerTitle = New Label()
        signedInPanel = New Panel()
        scannerView = New PalletScannerView()
        connectionStatus = New StatusStrip()
        connectionLabel = New ToolStripStatusLabel()
        mainMenu.SuspendLayout()
        appHeader.SuspendLayout()
        signedInPanel.SuspendLayout()
        connectionStatus.SuspendLayout()
        SuspendLayout()
        ' 
        ' mainMenu
        ' 
        mainMenu.AutoSize = False
        mainMenu.BackColor = Color.White
        mainMenu.Font = New Font("Segoe UI", 10F)
        mainMenu.Items.AddRange(New ToolStripItem() {menuSetup, menuPalletActions, menuHistory, menuTransfer, menuHelp, menuLogout})
        mainMenu.Location = New Point(0, 0)
        mainMenu.Name = "mainMenu"
        mainMenu.Size = New Size(1384, 34)
        mainMenu.TabIndex = 3
        ' 
        ' menuSetup
        ' 
        menuSetup.DropDownItems.AddRange(New ToolStripItem() {setupCustomerItem, setupUsersItem, setupPalletItem, setupRulesItem, setupGeneralItem})
        menuSetup.Name = "menuSetup"
        menuSetup.Size = New Size(56, 30)
        menuSetup.Text = "Setup"
        ' 
        ' setupCustomerItem
        ' 
        setupCustomerItem.Name = "setupCustomerItem"
        setupCustomerItem.Size = New Size(226, 24)
        setupCustomerItem.Text = "Customer Configuration"
        ' 
        ' setupUsersItem
        ' 
        setupUsersItem.Name = "setupUsersItem"
        setupUsersItem.Size = New Size(226, 24)
        setupUsersItem.Text = "Users & Roles"
        ' 
        ' setupPalletItem
        ' 
        setupPalletItem.Name = "setupPalletItem"
        setupPalletItem.Size = New Size(226, 24)
        setupPalletItem.Text = "Pallet Configuration"
        ' 
        ' setupRulesItem
        ' 
        setupRulesItem.Name = "setupRulesItem"
        setupRulesItem.Size = New Size(226, 24)
        setupRulesItem.Text = "Scan Rules"
        ' 
        ' setupGeneralItem
        ' 
        setupGeneralItem.Name = "setupGeneralItem"
        setupGeneralItem.Size = New Size(226, 24)
        setupGeneralItem.Text = "General Settings"
        ' 
        ' menuPalletActions
        ' 
        menuPalletActions.DropDownItems.AddRange(New ToolStripItem() {menuPartialPallet, menuCancelEmptyPallet, menuReviewPendingSql})
        menuPalletActions.Name = "menuPalletActions"
        menuPalletActions.Size = New Size(103, 30)
        menuPalletActions.Text = "Pallet Actions"
        ' 
        ' menuPartialPallet
        ' 
        menuPartialPallet.Name = "menuPartialPallet"
        menuPartialPallet.Size = New Size(291, 24)
        menuPartialPallet.Text = "Partial pallet (authorization)"
        ' 
        ' menuCancelEmptyPallet
        ' 
        menuCancelEmptyPallet.Name = "menuCancelEmptyPallet"
        menuCancelEmptyPallet.Size = New Size(291, 24)
        menuCancelEmptyPallet.Text = "Cancel empty pallet (authorization)"
        ' 
        ' menuReviewPendingSql
        ' 
        menuReviewPendingSql.Name = "menuReviewPendingSql"
        menuReviewPendingSql.Size = New Size(291, 24)
        menuReviewPendingSql.Text = "Review pending SQL"
        ' 
        ' menuHistory
        ' 
        menuHistory.Name = "menuHistory"
        menuHistory.Size = New Size(102, 30)
        menuHistory.Text = "Pallet History"
        ' 
        ' menuTransfer
        ' 
        menuTransfer.Name = "menuTransfer"
        menuTransfer.Size = New Size(112, 30)
        menuTransfer.Text = "Pallet Transfers"
        ' 
        ' menuHelp
        ' 
        menuHelp.DropDownItems.AddRange(New ToolStripItem() {helpAboutItem})
        menuHelp.Name = "menuHelp"
        menuHelp.Size = New Size(60, 30)
        menuHelp.Text = "Ayuda"
        ' 
        ' helpAboutItem
        ' 
        helpAboutItem.Name = "helpAboutItem"
        helpAboutItem.Size = New Size(199, 24)
        helpAboutItem.Text = "About PackAndShip"
        ' 
        ' menuLogout
        ' 
        menuLogout.Name = "menuLogout"
        menuLogout.Size = New Size(101, 30)
        menuLogout.Text = "Cerrar sesión"
        ' 
        ' appHeader
        ' 
        appHeader.BackColor = Color.FromArgb(CByte(31), CByte(78), CByte(121))
        appHeader.Controls.Add(headerTitle)
        appHeader.Dock = DockStyle.Top
        appHeader.Location = New Point(0, 34)
        appHeader.Name = "appHeader"
        appHeader.Size = New Size(1384, 84)
        appHeader.TabIndex = 1
        ' 
        ' headerTitle
        ' 
        headerTitle.AutoSize = True
        headerTitle.Font = New Font("Segoe UI", 23.0F, FontStyle.Bold)
        headerTitle.ForeColor = Color.White
        headerTitle.Location = New Point(24, 19)
        headerTitle.Name = "headerTitle"
        headerTitle.Size = New Size(253, 42)
        headerTitle.TabIndex = 0
        headerTitle.Text = "PACK AND SHIP"
        ' 
        ' signedInPanel
        ' 
        signedInPanel.Controls.Add(scannerView)
        signedInPanel.Dock = DockStyle.Fill
        signedInPanel.Location = New Point(0, 118)
        signedInPanel.Name = "signedInPanel"
        signedInPanel.Size = New Size(1384, 620)
        signedInPanel.TabIndex = 0
        ' 
        ' scannerView
        ' 
        scannerView.BackColor = Color.FromArgb(CByte(245), CByte(248), CByte(252))
        scannerView.Dock = DockStyle.Fill
        scannerView.Font = New Font("Segoe UI", 10.0F)
        scannerView.Location = New Point(0, 0)
        scannerView.MinimumSize = New Size(990, 830)
        scannerView.Name = "scannerView"
        scannerView.Size = New Size(1384, 830)
        scannerView.TabIndex = 0
        ' 
        ' connectionStatus
        ' 
        connectionStatus.Items.AddRange(New ToolStripItem() {connectionLabel})
        connectionStatus.Location = New Point(0, 738)
        connectionStatus.Name = "connectionStatus"
        connectionStatus.Size = New Size(1384, 22)
        connectionStatus.SizingGrip = False
        connectionStatus.TabIndex = 2
        ' 
        ' connectionLabel
        ' 
        connectionLabel.Name = "connectionLabel"
        connectionLabel.Size = New Size(146, 17)
        connectionLabel.Text = "CCS: checking connection"
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(8.0F, 20.0F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(244), CByte(247), CByte(250))
        ClientSize = New Size(1384, 760)
        Controls.Add(signedInPanel)
        Controls.Add(appHeader)
        Controls.Add(connectionStatus)
        Controls.Add(mainMenu)
        Font = New Font("Segoe UI", 11F)
        MainMenuStrip = mainMenu
        Name = "Form1"
        StartPosition = FormStartPosition.CenterScreen
        Text = "Pack and Ship | SSI"
        mainMenu.ResumeLayout(False)
        mainMenu.PerformLayout()
        appHeader.ResumeLayout(False)
        appHeader.PerformLayout()
        signedInPanel.ResumeLayout(False)
        connectionStatus.ResumeLayout(False)
        connectionStatus.PerformLayout()
        ResumeLayout(False)
        PerformLayout()
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
