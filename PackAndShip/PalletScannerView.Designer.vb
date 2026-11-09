<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PalletScannerView
    Inherits System.Windows.Forms.UserControl

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
        Me.SuspendLayout()
        Me.mainLayout = New System.Windows.Forms.Panel()
        Me.headerPage = New System.Windows.Forms.Panel()
        Me.headerLayout = New System.Windows.Forms.Panel()
        Me.activeStrip = New System.Windows.Forms.Panel()
        Me.activeCaption = New System.Windows.Forms.Label()
        Me.headerContainerCell = New System.Windows.Forms.Panel()
        Me.headerModelCell = New System.Windows.Forms.Panel()
        Me.headerOperatorCell = New System.Windows.Forms.Panel()
        Me.headerCustomerCell = New System.Windows.Forms.Panel()
        Me.headerTimeCell = New System.Windows.Forms.Panel()
        Me.captionContainer = New System.Windows.Forms.Label()
        Me.captionModel = New System.Windows.Forms.Label()
        Me.captionOperator = New System.Windows.Forms.Label()
        Me.captionCustomer = New System.Windows.Forms.Label()
        Me.captionTime = New System.Windows.Forms.Label()
        Me.containerInput = New System.Windows.Forms.TextBox()
        Me.partPicker = New System.Windows.Forms.ComboBox()
        Me.operatorValue = New System.Windows.Forms.Label()
        Me.customerValue = New System.Windows.Forms.Label()
        Me.clockValue = New System.Windows.Forms.Label()
        Me.activePartTitle = New System.Windows.Forms.Label()
        Me.buildState = New System.Windows.Forms.Label()
        Me.bodyLayout = New System.Windows.Forms.Panel()
        Me.leftLayout = New System.Windows.Forms.Panel()
        Me.scanArea = New System.Windows.Forms.Panel()
        Me.piecesArea = New System.Windows.Forms.Panel()
        Me.progressArea = New System.Windows.Forms.Panel()
        Me.scanTitle = New System.Windows.Forms.Label()
        Me.piecesTitle = New System.Windows.Forms.Label()
        Me.progressTitle = New System.Windows.Forms.Label()
        Me.scanHelp = New System.Windows.Forms.Label()
        Me.ssiCaption = New System.Windows.Forms.Label()
        Me.clientScanLabel = New System.Windows.Forms.Label()
        Me.firstQr = New System.Windows.Forms.TextBox()
        Me.secondQr = New System.Windows.Forms.TextBox()
        Me.scanStatus = New System.Windows.Forms.Label()
        Me.scannedList = New System.Windows.Forms.ListBox()
        Me.progressIcon = New System.Windows.Forms.Label()
        Me.progressPercent = New System.Windows.Forms.Label()
        Me.progressState = New System.Windows.Forms.Label()
        Me.progressBar = New System.Windows.Forms.ProgressBar()
        Me.progressText = New System.Windows.Forms.Label()
        Me.levelTiles = New System.Windows.Forms.FlowLayoutPanel()
        Me.confirmAgain = New System.Windows.Forms.Button()
        Me.ruleSummary = New System.Windows.Forms.Label()
        Me.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.mainLayout.AutoScroll = True
        Me.mainLayout.AutoScrollMinSize = New System.Drawing.Size(1150, 730)
        Me.mainLayout.Padding = New System.Windows.Forms.Padding(12, 10, 12, 10)
        Me.mainLayout.BackColor = System.Drawing.Color.FromArgb(245, 248, 252)
        Me.headerPage.Dock = System.Windows.Forms.DockStyle.None
        Me.headerPage.Location = New System.Drawing.Point(12, 10)
        Me.headerPage.Size = New System.Drawing.Size(1138, 136)
        Me.headerPage.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        Me.headerPage.Margin = New System.Windows.Forms.Padding(0, 0, 0, 6)
        Me.headerLayout.Dock = System.Windows.Forms.DockStyle.None
        Me.headerLayout.Location = New System.Drawing.Point(0, 0)
        Me.headerLayout.Size = New System.Drawing.Size(1138, 82)
        Me.headerLayout.BackColor = System.Drawing.Color.FromArgb(224, 232, 241)
        Me.headerContainerCell.Dock = System.Windows.Forms.DockStyle.None
        Me.headerContainerCell.Location = New System.Drawing.Point(0, 0)
        Me.headerContainerCell.Size = New System.Drawing.Size(190, 82)
        Me.headerContainerCell.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        Me.headerContainerCell.Margin = New System.Windows.Forms.Padding(1, 1, 1, 1)
        Me.captionContainer.Text = "CONTAINER"
        Me.captionContainer.Location = New System.Drawing.Point(9, 6)
        Me.captionContainer.Size = New System.Drawing.Size(154, 18)
        Me.captionContainer.AutoEllipsis = True
        Me.captionContainer.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Bold)
        Me.captionContainer.ForeColor = System.Drawing.Color.FromArgb(108, 128, 155)
        Me.containerInput.Location = New System.Drawing.Point(9, 30)
        Me.containerInput.Size = New System.Drawing.Size(154, 33)
        Me.containerInput.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left
        Me.headerContainerCell.Controls.Add(Me.captionContainer)
        Me.headerContainerCell.Controls.Add(Me.containerInput)
        Me.headerModelCell.Dock = System.Windows.Forms.DockStyle.None
        Me.headerModelCell.Location = New System.Drawing.Point(190, 0)
        Me.headerModelCell.Size = New System.Drawing.Size(300, 82)
        Me.headerModelCell.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        Me.headerModelCell.Margin = New System.Windows.Forms.Padding(1, 1, 1, 1)
        Me.captionModel.Text = "PART NUMBER / MODEL"
        Me.captionModel.Location = New System.Drawing.Point(9, 6)
        Me.captionModel.Size = New System.Drawing.Size(240, 18)
        Me.captionModel.AutoEllipsis = True
        Me.captionModel.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Bold)
        Me.captionModel.ForeColor = System.Drawing.Color.FromArgb(108, 128, 155)
        Me.partPicker.Location = New System.Drawing.Point(9, 30)
        Me.partPicker.Size = New System.Drawing.Size(240, 33)
        Me.partPicker.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left
        Me.headerModelCell.Controls.Add(Me.captionModel)
        Me.headerModelCell.Controls.Add(Me.partPicker)
        Me.headerOperatorCell.Dock = System.Windows.Forms.DockStyle.None
        Me.headerOperatorCell.Location = New System.Drawing.Point(490, 0)
        Me.headerOperatorCell.Size = New System.Drawing.Size(245, 82)
        Me.headerOperatorCell.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        Me.headerOperatorCell.Margin = New System.Windows.Forms.Padding(1, 1, 1, 1)
        Me.captionOperator.Text = "OPERATOR"
        Me.captionOperator.Location = New System.Drawing.Point(9, 6)
        Me.captionOperator.Size = New System.Drawing.Size(190, 18)
        Me.captionOperator.AutoEllipsis = True
        Me.captionOperator.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Bold)
        Me.captionOperator.ForeColor = System.Drawing.Color.FromArgb(108, 128, 155)
        Me.operatorValue.Location = New System.Drawing.Point(9, 30)
        Me.operatorValue.Size = New System.Drawing.Size(190, 33)
        Me.operatorValue.AutoEllipsis = True
        Me.operatorValue.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left
        Me.headerOperatorCell.Controls.Add(Me.captionOperator)
        Me.headerOperatorCell.Controls.Add(Me.operatorValue)
        Me.headerCustomerCell.Dock = System.Windows.Forms.DockStyle.None
        Me.headerCustomerCell.Location = New System.Drawing.Point(735, 0)
        Me.headerCustomerCell.Size = New System.Drawing.Size(160, 82)
        Me.headerCustomerCell.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        Me.headerCustomerCell.Margin = New System.Windows.Forms.Padding(1, 1, 1, 1)
        Me.captionCustomer.Text = "CUSTOMER"
        Me.captionCustomer.Location = New System.Drawing.Point(9, 6)
        Me.captionCustomer.Size = New System.Drawing.Size(116, 18)
        Me.captionCustomer.AutoEllipsis = True
        Me.captionCustomer.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Bold)
        Me.captionCustomer.ForeColor = System.Drawing.Color.FromArgb(108, 128, 155)
        Me.customerValue.Location = New System.Drawing.Point(9, 30)
        Me.customerValue.Size = New System.Drawing.Size(116, 33)
        Me.customerValue.AutoEllipsis = True
        Me.customerValue.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left
        Me.headerCustomerCell.Controls.Add(Me.captionCustomer)
        Me.headerCustomerCell.Controls.Add(Me.customerValue)
        Me.headerTimeCell.Dock = System.Windows.Forms.DockStyle.None
        Me.headerTimeCell.Location = New System.Drawing.Point(895, 0)
        Me.headerTimeCell.Size = New System.Drawing.Size(243, 82)
        Me.headerTimeCell.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        Me.headerTimeCell.Margin = New System.Windows.Forms.Padding(1, 1, 1, 1)
        Me.captionTime.Text = "DATE / TIME"
        Me.captionTime.Location = New System.Drawing.Point(9, 6)
        Me.captionTime.Size = New System.Drawing.Size(163, 18)
        Me.captionTime.AutoEllipsis = True
        Me.captionTime.Font = New System.Drawing.Font("Segoe UI", 8.5!, System.Drawing.FontStyle.Bold)
        Me.captionTime.ForeColor = System.Drawing.Color.FromArgb(108, 128, 155)
        Me.clockValue.Location = New System.Drawing.Point(9, 30)
        Me.clockValue.Size = New System.Drawing.Size(163, 33)
        Me.clockValue.AutoEllipsis = True
        Me.clockValue.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left
        Me.headerTimeCell.Controls.Add(Me.captionTime)
        Me.headerTimeCell.Controls.Add(Me.clockValue)
        Me.containerInput.Font = New System.Drawing.Font("Segoe UI", 12!, System.Drawing.FontStyle.Bold)
        Me.containerInput.MaxLength = 18
        Me.containerInput.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.partPicker.Font = New System.Drawing.Font("Segoe UI", 10!)
        Me.partPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.partPicker.DisplayMember = "DisplayName"
        Me.partPicker.ValueMember = "Id"
        Me.operatorValue.Font = New System.Drawing.Font("Segoe UI", 13!, System.Drawing.FontStyle.Bold)
        Me.operatorValue.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.customerValue.Font = New System.Drawing.Font("Segoe UI", 12!, System.Drawing.FontStyle.Bold)
        Me.customerValue.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.clockValue.Font = New System.Drawing.Font("Segoe UI", 10!, System.Drawing.FontStyle.Bold)
        Me.headerLayout.Controls.Add(Me.headerContainerCell)
        Me.headerLayout.Controls.Add(Me.headerModelCell)
        Me.headerLayout.Controls.Add(Me.headerOperatorCell)
        Me.headerLayout.Controls.Add(Me.headerCustomerCell)
        Me.headerLayout.Controls.Add(Me.headerTimeCell)
        Me.activeStrip.Dock = System.Windows.Forms.DockStyle.None
        Me.activeStrip.Location = New System.Drawing.Point(0, 82)
        Me.activeStrip.Size = New System.Drawing.Size(1138, 54)
                Me.activeStrip.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        Me.activeCaption.Location = New System.Drawing.Point(12, 8)
        Me.activeCaption.Size = New System.Drawing.Size(114, 18)
        Me.activeCaption.Font = New System.Drawing.Font("Segoe UI", 8!, System.Drawing.FontStyle.Bold)
        Me.activeCaption.Text = "ACTIVE BUILD"
        Me.activeCaption.ForeColor = System.Drawing.Color.FromArgb(108, 128, 155)
        Me.activePartTitle.Location = New System.Drawing.Point(130, 4)
        Me.activePartTitle.Size = New System.Drawing.Size(680, 26)
        Me.activePartTitle.Font = New System.Drawing.Font("Segoe UI", 12!, System.Drawing.FontStyle.Bold)
        Me.activePartTitle.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.activePartTitle.Text = "Select a model"
        Me.activePartTitle.AutoEllipsis = True
        Me.buildState.Location = New System.Drawing.Point(130, 29)
        Me.buildState.Size = New System.Drawing.Size(770, 18)
        Me.buildState.Font = New System.Drawing.Font("Segoe UI", 9!)
        Me.buildState.Text = "Scan a container barcode and press Enter"
        Me.buildState.ForeColor = System.Drawing.Color.FromArgb(108, 128, 155)
        Me.activeStrip.Controls.Add(Me.activeCaption)
        Me.activeStrip.Controls.Add(Me.activePartTitle)
        Me.activeStrip.Controls.Add(Me.buildState)
        Me.headerPage.Controls.Add(Me.activeStrip)
        Me.headerPage.Controls.Add(Me.headerLayout)
        Me.bodyLayout.Dock = System.Windows.Forms.DockStyle.None
        Me.bodyLayout.Location = New System.Drawing.Point(12, 157)
        Me.bodyLayout.Size = New System.Drawing.Size(1138, 560)
        Me.bodyLayout.BackColor = System.Drawing.Color.FromArgb(245, 248, 252)
        Me.leftLayout.Dock = System.Windows.Forms.DockStyle.None
        Me.leftLayout.Location = New System.Drawing.Point(0, 0)
        Me.leftLayout.Size = New System.Drawing.Size(450, 560)
        Me.scanArea.Dock = System.Windows.Forms.DockStyle.None
        Me.scanArea.Location = New System.Drawing.Point(0, 0)
        Me.scanArea.Size = New System.Drawing.Size(450, 310)
        Me.scanArea.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        Me.scanArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.scanArea.Margin = New System.Windows.Forms.Padding(4, 4, 5, 4)
        Me.piecesArea.Dock = System.Windows.Forms.DockStyle.None
        Me.piecesArea.Location = New System.Drawing.Point(0, 322)
        Me.piecesArea.Size = New System.Drawing.Size(450, 238)
        Me.piecesArea.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        Me.piecesArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.piecesArea.Margin = New System.Windows.Forms.Padding(4, 4, 5, 4)
        Me.progressArea.Dock = System.Windows.Forms.DockStyle.None
        Me.progressArea.Location = New System.Drawing.Point(465, 0)
        Me.progressArea.Size = New System.Drawing.Size(673, 560)
        Me.progressArea.BackColor = System.Drawing.Color.FromArgb(255, 255, 255)
        Me.progressArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.progressArea.Margin = New System.Windows.Forms.Padding(4, 4, 5, 4)
        Me.scanTitle.Text = "SCAN LABELS"
        Me.scanTitle.Location = New System.Drawing.Point(18, 15)
        Me.scanTitle.Size = New System.Drawing.Size(360, 30)
        Me.scanTitle.Font = New System.Drawing.Font("Segoe UI", 13!, System.Drawing.FontStyle.Bold)
        Me.scanTitle.ForeColor = System.Drawing.Color.FromArgb(27, 39, 58)
        Me.piecesTitle.Text = "RECENTLY SCANNED"
        Me.piecesTitle.Location = New System.Drawing.Point(18, 15)
        Me.piecesTitle.Size = New System.Drawing.Size(360, 30)
        Me.piecesTitle.Font = New System.Drawing.Font("Segoe UI", 13!, System.Drawing.FontStyle.Bold)
        Me.piecesTitle.ForeColor = System.Drawing.Color.FromArgb(27, 39, 58)
        Me.progressTitle.Text = "PALLET PROGRESS"
        Me.progressTitle.Location = New System.Drawing.Point(18, 15)
        Me.progressTitle.Size = New System.Drawing.Size(360, 30)
        Me.progressTitle.Font = New System.Drawing.Font("Segoe UI", 13!, System.Drawing.FontStyle.Bold)
        Me.progressTitle.ForeColor = System.Drawing.Color.FromArgb(27, 39, 58)
        Me.scanHelp.Text = "Scanner ready. Each QR sends Enter automatically."
        Me.scanHelp.Location = New System.Drawing.Point(18, 52)
        Me.scanHelp.Size = New System.Drawing.Size(370, 32)
        Me.scanHelp.Font = New System.Drawing.Font("Segoe UI", 9!)
        Me.scanHelp.ForeColor = System.Drawing.Color.FromArgb(108, 128, 155)
        Me.ssiCaption.Text = "01  SSI SERIAL"
        Me.ssiCaption.Location = New System.Drawing.Point(18, 95)
        Me.ssiCaption.Size = New System.Drawing.Size(250, 22)
        Me.ssiCaption.Font = New System.Drawing.Font("Segoe UI", 9!, System.Drawing.FontStyle.Bold)
        Me.ssiCaption.ForeColor = System.Drawing.Color.FromArgb(108, 128, 155)
        Me.firstQr.Location = New System.Drawing.Point(18, 120)
        Me.firstQr.Size = New System.Drawing.Size(380, 43)
        Me.firstQr.Font = New System.Drawing.Font("Segoe UI", 13!, System.Drawing.FontStyle.Bold)
        Me.firstQr.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left
        Me.clientScanLabel.Text = "02  CLIENT SERIAL"
        Me.clientScanLabel.Location = New System.Drawing.Point(18, 171)
        Me.clientScanLabel.Size = New System.Drawing.Size(300, 22)
        Me.clientScanLabel.Font = New System.Drawing.Font("Segoe UI", 9!, System.Drawing.FontStyle.Bold)
        Me.clientScanLabel.ForeColor = System.Drawing.Color.FromArgb(108, 128, 155)
        Me.secondQr.Location = New System.Drawing.Point(18, 197)
        Me.secondQr.Size = New System.Drawing.Size(380, 43)
        Me.secondQr.Font = New System.Drawing.Font("Segoe UI", 13!, System.Drawing.FontStyle.Bold)
        Me.secondQr.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left
        Me.scanStatus.Location = New System.Drawing.Point(18, 248)
        Me.scanStatus.Size = New System.Drawing.Size(380, 48)
        Me.scanStatus.Font = New System.Drawing.Font("Segoe UI", 9!, System.Drawing.FontStyle.Bold)
        Me.scanStatus.ForeColor = System.Drawing.Color.FromArgb(108, 128, 155)
        Me.scanStatus.Text = "Select a model, then scan the container."
        Me.scanArea.Controls.Add(Me.scanTitle)
        Me.scanArea.Controls.Add(Me.scanHelp)
        Me.scanArea.Controls.Add(Me.ssiCaption)
        Me.scanArea.Controls.Add(Me.firstQr)
        Me.scanArea.Controls.Add(Me.clientScanLabel)
        Me.scanArea.Controls.Add(Me.secondQr)
        Me.scanArea.Controls.Add(Me.scanStatus)
        Me.piecesTitle.Location = New System.Drawing.Point(18, 15)
        Me.scannedList.Location = New System.Drawing.Point(18, 49)
        Me.scannedList.Size = New System.Drawing.Size(380, 145)
        Me.scannedList.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left
        Me.scannedList.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.scannedList.Enabled = False
        Me.scannedList.BackColor = System.Drawing.Color.FromArgb(248, 250, 253)
        Me.piecesArea.Controls.Add(Me.piecesTitle)
        Me.piecesArea.Controls.Add(Me.scannedList)
        Me.progressIcon.Location = New System.Drawing.Point(24, 58)
        Me.progressIcon.Size = New System.Drawing.Size(72, 72)
        Me.progressIcon.Font = New System.Drawing.Font("Segoe UI", 36!, System.Drawing.FontStyle.Bold)
        Me.progressIcon.Text = "▣"
        Me.progressIcon.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.progressPercent.Location = New System.Drawing.Point(106, 53)
        Me.progressPercent.Size = New System.Drawing.Size(420, 72)
        Me.progressPercent.Font = New System.Drawing.Font("Segoe UI", 32!, System.Drawing.FontStyle.Bold)
        Me.progressPercent.Text = "0%"
        Me.progressPercent.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.progressState.Location = New System.Drawing.Point(26, 144)
        Me.progressState.Size = New System.Drawing.Size(520, 38)
        Me.progressState.Font = New System.Drawing.Font("Segoe UI", 12!, System.Drawing.FontStyle.Bold)
        Me.progressState.Text = "WAITING FOR PALLET"
        Me.progressState.ForeColor = System.Drawing.Color.FromArgb(28, 113, 83)
        Me.progressBar.Location = New System.Drawing.Point(25, 202)
        Me.progressBar.Size = New System.Drawing.Size(540, 22)
        Me.progressBar.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left
        Me.progressText.Location = New System.Drawing.Point(25, 243)
        Me.progressText.Size = New System.Drawing.Size(540, 93)
        Me.progressText.Font = New System.Drawing.Font("Segoe UI", 16!, System.Drawing.FontStyle.Bold)
        Me.progressText.Text = "0 / 0 pieces"
        Me.levelTiles.Location = New System.Drawing.Point(25, 343)
        Me.levelTiles.Size = New System.Drawing.Size(540, 62)
        Me.levelTiles.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left
        Me.levelTiles.AutoScroll = True
        Me.confirmAgain.Location = New System.Drawing.Point(25, 408)
        Me.confirmAgain.Size = New System.Drawing.Size(540, 50)
        Me.confirmAgain.Anchor = System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left
        Me.confirmAgain.Text = "Review packed level"
        Me.confirmAgain.Enabled = False
        Me.confirmAgain.BackColor = System.Drawing.Color.FromArgb(245, 248, 252)
        Me.progressArea.Controls.Add(Me.progressTitle)
        Me.progressArea.Controls.Add(Me.progressIcon)
        Me.progressArea.Controls.Add(Me.progressPercent)
        Me.progressArea.Controls.Add(Me.progressState)
        Me.progressArea.Controls.Add(Me.progressBar)
        Me.progressArea.Controls.Add(Me.progressText)
        Me.progressArea.Controls.Add(Me.levelTiles)
        Me.progressArea.Controls.Add(Me.confirmAgain)
        Me.progressArea.Controls.Add(Me.ruleSummary)
        Me.ruleSummary.Visible = False
        Me.leftLayout.Controls.Add(Me.scanArea)
        Me.leftLayout.Controls.Add(Me.piecesArea)
        Me.bodyLayout.Controls.Add(Me.leftLayout)
        Me.bodyLayout.Controls.Add(Me.progressArea)
        Me.mainLayout.Controls.Add(Me.headerPage)
        Me.mainLayout.Controls.Add(Me.bodyLayout)
        Me.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill
        Me.Controls.Add(Me.mainLayout)
        Me.Me.BackColor = System.Drawing.Color.FromArgb(245, 248, 252)
        Me.Me.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.Me.MinimumSize = New System.Drawing.Size(990, 630)
        Me.Me.Size = New System.Drawing.Size(1280, 760)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents mainLayout As System.Windows.Forms.Panel
    Friend WithEvents headerPage As System.Windows.Forms.Panel
    Friend WithEvents headerLayout As System.Windows.Forms.Panel
    Friend WithEvents activeStrip As System.Windows.Forms.Panel
    Friend WithEvents activeCaption As System.Windows.Forms.Label
    Friend WithEvents headerContainerCell As System.Windows.Forms.Panel
    Friend WithEvents headerModelCell As System.Windows.Forms.Panel
    Friend WithEvents headerOperatorCell As System.Windows.Forms.Panel
    Friend WithEvents headerCustomerCell As System.Windows.Forms.Panel
    Friend WithEvents headerTimeCell As System.Windows.Forms.Panel
    Friend WithEvents captionContainer As System.Windows.Forms.Label
    Friend WithEvents captionModel As System.Windows.Forms.Label
    Friend WithEvents captionOperator As System.Windows.Forms.Label
    Friend WithEvents captionCustomer As System.Windows.Forms.Label
    Friend WithEvents captionTime As System.Windows.Forms.Label
    Friend WithEvents containerInput As System.Windows.Forms.TextBox
    Friend WithEvents partPicker As System.Windows.Forms.ComboBox
    Friend WithEvents operatorValue As System.Windows.Forms.Label
    Friend WithEvents customerValue As System.Windows.Forms.Label
    Friend WithEvents clockValue As System.Windows.Forms.Label
    Friend WithEvents activePartTitle As System.Windows.Forms.Label
    Friend WithEvents buildState As System.Windows.Forms.Label
    Friend WithEvents bodyLayout As System.Windows.Forms.Panel
    Friend WithEvents leftLayout As System.Windows.Forms.Panel
    Friend WithEvents scanArea As System.Windows.Forms.Panel
    Friend WithEvents piecesArea As System.Windows.Forms.Panel
    Friend WithEvents progressArea As System.Windows.Forms.Panel
    Friend WithEvents scanTitle As System.Windows.Forms.Label
    Friend WithEvents piecesTitle As System.Windows.Forms.Label
    Friend WithEvents progressTitle As System.Windows.Forms.Label
    Friend WithEvents scanHelp As System.Windows.Forms.Label
    Friend WithEvents ssiCaption As System.Windows.Forms.Label
    Friend WithEvents clientScanLabel As System.Windows.Forms.Label
    Friend WithEvents firstQr As System.Windows.Forms.TextBox
    Friend WithEvents secondQr As System.Windows.Forms.TextBox
    Friend WithEvents scanStatus As System.Windows.Forms.Label
    Friend WithEvents scannedList As System.Windows.Forms.ListBox
    Friend WithEvents progressIcon As System.Windows.Forms.Label
    Friend WithEvents progressPercent As System.Windows.Forms.Label
    Friend WithEvents progressState As System.Windows.Forms.Label
    Friend WithEvents progressBar As System.Windows.Forms.ProgressBar
    Friend WithEvents progressText As System.Windows.Forms.Label
    Friend WithEvents levelTiles As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents confirmAgain As System.Windows.Forms.Button
    Friend WithEvents ruleSummary As System.Windows.Forms.Label
End Class
