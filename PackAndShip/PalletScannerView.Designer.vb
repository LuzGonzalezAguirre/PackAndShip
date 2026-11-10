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
        mainLayout = New Panel()
        scanArea = New Panel()
        leftLayout = New Panel()
        piecesArea = New Panel()
        piecesTitle = New Label()
        scannedList = New ListBox()
        scanTitle = New Label()
        scanHelp = New Label()
        ssiCaption = New Label()
        firstQr = New TextBox()
        clientScanLabel = New Label()
        secondQr = New TextBox()
        scanStatus = New Label()
        progressArea = New Panel()
        Label1 = New Label()
        lbl_lot = New Label()
        levelTiles = New FlowLayoutPanel()
        progressTitle = New Label()
        progressIcon = New Label()
        progressPercent = New Label()
        progressState = New Label()
        progressBar = New ProgressBar()
        progressText = New Label()
        confirmAgain = New Button()
        ruleSummary = New Label()
        headerPage = New Panel()
        headerLayout = New Panel()
        activeStrip = New Panel()
        activeCaption = New Label()
        activePartTitle = New Label()
        buildState = New Label()
        headerContainerCell = New Panel()
        captionContainer = New Label()
        containerInput = New TextBox()
        headerModelCell = New Panel()
        captionModel = New Label()
        partPicker = New ComboBox()
        headerOperatorCell = New Panel()
        captionOperator = New Label()
        operatorValue = New Label()
        headerCustomerCell = New Panel()
        captionCustomer = New Label()
        customerValue = New Label()
        headerTimeCell = New Panel()
        captionTime = New Label()
        clockValue = New Label()
        DataGridView1 = New DataGridView()
        colContainer = New DataGridViewTextBoxColumn()
        colSsiModel = New DataGridViewTextBoxColumn()
        colLevel = New DataGridViewTextBoxColumn()
        colLevelPiece = New DataGridViewTextBoxColumn()
        colSsiSerial = New DataGridViewTextBoxColumn()
        colClientSerial = New DataGridViewTextBoxColumn()
        colScanTime = New DataGridViewTextBoxColumn()
        mainLayout.SuspendLayout()
        scanArea.SuspendLayout()
        leftLayout.SuspendLayout()
        piecesArea.SuspendLayout()
        progressArea.SuspendLayout()
        headerPage.SuspendLayout()
        headerLayout.SuspendLayout()
        activeStrip.SuspendLayout()
        headerContainerCell.SuspendLayout()
        headerModelCell.SuspendLayout()
        headerOperatorCell.SuspendLayout()
        headerCustomerCell.SuspendLayout()
        headerTimeCell.SuspendLayout()
        CType(DataGridView1, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' mainLayout
        ' 
        mainLayout.AutoScroll = True
        mainLayout.AutoScrollMinSize = New Size(1150, 730)
        mainLayout.BackColor = Color.FromArgb(CByte(245), CByte(248), CByte(252))
        mainLayout.Controls.Add(DataGridView1)
        mainLayout.Controls.Add(scanArea)
        mainLayout.Controls.Add(progressArea)
        mainLayout.Controls.Add(headerPage)
        mainLayout.Dock = DockStyle.Fill
        mainLayout.Location = New Point(0, 0)
        mainLayout.Name = "mainLayout"
        mainLayout.Padding = New Padding(12, 10, 12, 10)
        mainLayout.Size = New Size(1382, 760)
        mainLayout.TabIndex = 0
        ' 
        ' scanArea
        ' 
        scanArea.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        scanArea.BorderStyle = BorderStyle.FixedSingle
        scanArea.Controls.Add(leftLayout)
        scanArea.Controls.Add(scanTitle)
        scanArea.Controls.Add(scanHelp)
        scanArea.Controls.Add(ssiCaption)
        scanArea.Controls.Add(firstQr)
        scanArea.Controls.Add(clientScanLabel)
        scanArea.Controls.Add(secondQr)
        scanArea.Controls.Add(scanStatus)
        scanArea.Location = New Point(12, 97)
        scanArea.Margin = New Padding(4, 4, 5, 4)
        scanArea.Name = "scanArea"
        scanArea.Size = New Size(523, 310)
        scanArea.TabIndex = 0
        ' 
        ' leftLayout
        ' 
        leftLayout.Controls.Add(piecesArea)
        leftLayout.Location = New Point(8, 361)
        leftLayout.Name = "leftLayout"
        leftLayout.Size = New Size(461, 557)
        leftLayout.TabIndex = 0
        ' 
        ' piecesArea
        ' 
        piecesArea.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        piecesArea.BorderStyle = BorderStyle.FixedSingle
        piecesArea.Controls.Add(piecesTitle)
        piecesArea.Controls.Add(scannedList)
        piecesArea.Location = New Point(21, 100)
        piecesArea.Margin = New Padding(4, 4, 5, 4)
        piecesArea.Name = "piecesArea"
        piecesArea.Size = New Size(450, 238)
        piecesArea.TabIndex = 1
        ' 
        ' piecesTitle
        ' 
        piecesTitle.Font = New Font("Segoe UI", 13.0F, FontStyle.Bold)
        piecesTitle.ForeColor = Color.FromArgb(CByte(27), CByte(39), CByte(58))
        piecesTitle.Location = New Point(18, 15)
        piecesTitle.Name = "piecesTitle"
        piecesTitle.Size = New Size(360, 30)
        piecesTitle.TabIndex = 0
        piecesTitle.Text = "RECENTLY SCANNED"
        ' 
        ' scannedList
        ' 
        scannedList.BackColor = Color.FromArgb(CByte(248), CByte(250), CByte(253))
        scannedList.BorderStyle = BorderStyle.None
        scannedList.Enabled = False
        scannedList.Location = New Point(18, 49)
        scannedList.Name = "scannedList"
        scannedList.Size = New Size(380, 136)
        scannedList.TabIndex = 1
        ' 
        ' scanTitle
        ' 
        scanTitle.Font = New Font("Segoe UI", 13.0F, FontStyle.Bold)
        scanTitle.ForeColor = Color.FromArgb(CByte(27), CByte(39), CByte(58))
        scanTitle.Location = New Point(18, 15)
        scanTitle.Name = "scanTitle"
        scanTitle.Size = New Size(360, 30)
        scanTitle.TabIndex = 0
        scanTitle.Text = "SCAN LABELS"
        ' 
        ' scanHelp
        ' 
        scanHelp.Font = New Font("Segoe UI", 9.0F)
        scanHelp.ForeColor = Color.FromArgb(CByte(108), CByte(128), CByte(155))
        scanHelp.Location = New Point(18, 52)
        scanHelp.Name = "scanHelp"
        scanHelp.Size = New Size(370, 32)
        scanHelp.TabIndex = 1
        scanHelp.Text = "Scanner ready. Each QR sends Enter automatically."
        ' 
        ' ssiCaption
        ' 
        ssiCaption.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        ssiCaption.ForeColor = Color.FromArgb(CByte(108), CByte(128), CByte(155))
        ssiCaption.Location = New Point(18, 95)
        ssiCaption.Name = "ssiCaption"
        ssiCaption.Size = New Size(250, 22)
        ssiCaption.TabIndex = 2
        ssiCaption.Text = "01  SSI SERIAL"
        ' 
        ' firstQr
        ' 
        firstQr.Font = New Font("Segoe UI", 13.0F, FontStyle.Bold)
        firstQr.Location = New Point(18, 120)
        firstQr.Name = "firstQr"
        firstQr.Size = New Size(482, 31)
        firstQr.TabIndex = 3
        ' 
        ' clientScanLabel
        ' 
        clientScanLabel.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        clientScanLabel.ForeColor = Color.FromArgb(CByte(108), CByte(128), CByte(155))
        clientScanLabel.Location = New Point(18, 171)
        clientScanLabel.Name = "clientScanLabel"
        clientScanLabel.Size = New Size(300, 22)
        clientScanLabel.TabIndex = 4
        clientScanLabel.Text = "02  CLIENT SERIAL"
        ' 
        ' secondQr
        ' 
        secondQr.Font = New Font("Segoe UI", 13.0F, FontStyle.Bold)
        secondQr.Location = New Point(18, 197)
        secondQr.Name = "secondQr"
        secondQr.Size = New Size(482, 31)
        secondQr.TabIndex = 5
        ' 
        ' scanStatus
        ' 
        scanStatus.Font = New Font("Segoe UI", 9.0F, FontStyle.Bold)
        scanStatus.ForeColor = Color.FromArgb(CByte(108), CByte(128), CByte(155))
        scanStatus.Location = New Point(18, 248)
        scanStatus.Name = "scanStatus"
        scanStatus.Size = New Size(380, 48)
        scanStatus.TabIndex = 6
        scanStatus.Text = "Select a model, then scan the container."
        ' 
        ' progressArea
        ' 
        progressArea.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        progressArea.BorderStyle = BorderStyle.FixedSingle
        progressArea.Controls.Add(Label1)
        progressArea.Controls.Add(lbl_lot)
        progressArea.Controls.Add(levelTiles)
        progressArea.Controls.Add(progressTitle)
        progressArea.Controls.Add(progressIcon)
        progressArea.Controls.Add(progressPercent)
        progressArea.Controls.Add(progressState)
        progressArea.Controls.Add(progressBar)
        progressArea.Controls.Add(progressText)
        progressArea.Controls.Add(confirmAgain)
        progressArea.Controls.Add(ruleSummary)
        progressArea.Location = New Point(539, 97)
        progressArea.Margin = New Padding(4, 4, 5, 4)
        progressArea.Name = "progressArea"
        progressArea.Size = New Size(830, 310)
        progressArea.TabIndex = 1
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI", 18.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        Label1.Location = New Point(53, 125)
        Label1.Name = "Label1"
        Label1.Size = New Size(107, 32)
        Label1.TabIndex = 10
        Label1.Text = "LOT NO:"
        ' 
        ' lbl_lot
        ' 
        lbl_lot.AutoSize = True
        lbl_lot.Font = New Font("Segoe UI", 20.25F, FontStyle.Bold, GraphicsUnit.Point, CByte(0))
        lbl_lot.Location = New Point(40, 157)
        lbl_lot.Name = "lbl_lot"
        lbl_lot.Size = New Size(130, 37)
        lbl_lot.TabIndex = 9
        lbl_lot.Text = "P######" & vbCrLf
        ' 
        ' levelTiles
        ' 
        levelTiles.AutoScroll = True
        levelTiles.Location = New Point(18, 208)
        levelTiles.Name = "levelTiles"
        levelTiles.Size = New Size(704, 97)
        levelTiles.TabIndex = 6
        ' 
        ' progressTitle
        ' 
        progressTitle.Font = New Font("Segoe UI", 13.0F, FontStyle.Bold)
        progressTitle.ForeColor = Color.FromArgb(CByte(27), CByte(39), CByte(58))
        progressTitle.Location = New Point(18, 15)
        progressTitle.Name = "progressTitle"
        progressTitle.Size = New Size(189, 30)
        progressTitle.TabIndex = 0
        progressTitle.Text = "PALLET PROGRESS"
        ' 
        ' progressIcon
        ' 
        progressIcon.Font = New Font("Segoe UI", 36.0F, FontStyle.Bold)
        progressIcon.ForeColor = Color.FromArgb(CByte(31), CByte(78), CByte(121))
        progressIcon.Location = New Point(24, 58)
        progressIcon.Name = "progressIcon"
        progressIcon.Size = New Size(72, 72)
        progressIcon.TabIndex = 1
        progressIcon.Text = "▣"
        ' 
        ' progressPercent
        ' 
        progressPercent.Font = New Font("Segoe UI", 32.0F, FontStyle.Bold)
        progressPercent.ForeColor = Color.FromArgb(CByte(31), CByte(78), CByte(121))
        progressPercent.Location = New Point(106, 53)
        progressPercent.Name = "progressPercent"
        progressPercent.Size = New Size(120, 72)
        progressPercent.TabIndex = 2
        progressPercent.Text = "0%"
        ' 
        ' progressState
        ' 
        progressState.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        progressState.ForeColor = Color.FromArgb(CByte(28), CByte(113), CByte(83))
        progressState.Location = New Point(232, 7)
        progressState.Name = "progressState"
        progressState.Size = New Size(520, 27)
        progressState.TabIndex = 3
        progressState.Text = "WAITING FOR PALLET"
        ' 
        ' progressBar
        ' 
        progressBar.Location = New Point(232, 62)
        progressBar.Name = "progressBar"
        progressBar.Size = New Size(540, 22)
        progressBar.TabIndex = 4
        ' 
        ' progressText
        ' 
        progressText.Font = New Font("Segoe UI", 16.0F, FontStyle.Bold)
        progressText.Location = New Point(232, 86)
        progressText.Name = "progressText"
        progressText.Size = New Size(540, 108)
        progressText.TabIndex = 5
        progressText.Text = "0 / 0 pieces"
        ' 
        ' confirmAgain
        ' 
        confirmAgain.BackColor = Color.FromArgb(CByte(245), CByte(248), CByte(252))
        confirmAgain.Enabled = False
        confirmAgain.Location = New Point(728, 208)
        confirmAgain.Name = "confirmAgain"
        confirmAgain.Size = New Size(97, 97)
        confirmAgain.TabIndex = 7
        confirmAgain.Text = "Review packed level"
        confirmAgain.UseVisualStyleBackColor = False
        ' 
        ' ruleSummary
        ' 
        ruleSummary.Location = New Point(0, 0)
        ruleSummary.Name = "ruleSummary"
        ruleSummary.Size = New Size(100, 23)
        ruleSummary.TabIndex = 8
        ruleSummary.Visible = False
        ' 
        ' headerPage
        ' 
        headerPage.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        headerPage.Controls.Add(headerLayout)
        headerPage.Location = New Point(12, 10)
        headerPage.Margin = New Padding(0, 0, 0, 6)
        headerPage.Name = "headerPage"
        headerPage.Size = New Size(1357, 82)
        headerPage.TabIndex = 0
        ' 
        ' headerLayout
        ' 
        headerLayout.BackColor = Color.FromArgb(CByte(224), CByte(232), CByte(241))
        headerLayout.Controls.Add(activeStrip)
        headerLayout.Controls.Add(headerContainerCell)
        headerLayout.Controls.Add(headerModelCell)
        headerLayout.Controls.Add(headerOperatorCell)
        headerLayout.Controls.Add(headerCustomerCell)
        headerLayout.Controls.Add(headerTimeCell)
        headerLayout.Location = New Point(0, 0)
        headerLayout.Name = "headerLayout"
        headerLayout.Size = New Size(1354, 82)
        headerLayout.TabIndex = 1
        ' 
        ' activeStrip
        ' 
        activeStrip.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        activeStrip.Controls.Add(activeCaption)
        activeStrip.Controls.Add(activePartTitle)
        activeStrip.Controls.Add(buildState)
        activeStrip.Location = New Point(1085, 0)
        activeStrip.Name = "activeStrip"
        activeStrip.Size = New Size(282, 82)
        activeStrip.TabIndex = 0
        ' 
        ' activeCaption
        ' 
        activeCaption.Font = New Font("Segoe UI", 8.0F, FontStyle.Bold)
        activeCaption.ForeColor = Color.FromArgb(CByte(108), CByte(128), CByte(155))
        activeCaption.Location = New Point(12, 8)
        activeCaption.Name = "activeCaption"
        activeCaption.Size = New Size(114, 18)
        activeCaption.TabIndex = 0
        activeCaption.Text = "ACTIVE BUILD"
        ' 
        ' activePartTitle
        ' 
        activePartTitle.AutoEllipsis = True
        activePartTitle.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        activePartTitle.ForeColor = Color.FromArgb(CByte(31), CByte(78), CByte(121))
        activePartTitle.Location = New Point(5, 33)
        activePartTitle.Name = "activePartTitle"
        activePartTitle.Size = New Size(680, 26)
        activePartTitle.TabIndex = 1
        activePartTitle.Text = "Select a model"
        ' 
        ' buildState
        ' 
        buildState.Font = New Font("Segoe UI", 9.0F)
        buildState.ForeColor = Color.FromArgb(CByte(108), CByte(128), CByte(155))
        buildState.Location = New Point(5, 59)
        buildState.Name = "buildState"
        buildState.Size = New Size(225, 18)
        buildState.TabIndex = 2
        buildState.Text = "Scan a container barcode and press Enter"
        ' 
        ' headerContainerCell
        ' 
        headerContainerCell.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        headerContainerCell.Controls.Add(captionContainer)
        headerContainerCell.Controls.Add(containerInput)
        headerContainerCell.Location = New Point(298, 0)
        headerContainerCell.Margin = New Padding(1)
        headerContainerCell.Name = "headerContainerCell"
        headerContainerCell.Size = New Size(225, 82)
        headerContainerCell.TabIndex = 0
        ' 
        ' captionContainer
        ' 
        captionContainer.AutoEllipsis = True
        captionContainer.Font = New Font("Segoe UI", 8.5F, FontStyle.Bold)
        captionContainer.ForeColor = Color.FromArgb(CByte(108), CByte(128), CByte(155))
        captionContainer.Location = New Point(9, 6)
        captionContainer.Name = "captionContainer"
        captionContainer.Size = New Size(154, 18)
        captionContainer.TabIndex = 0
        captionContainer.Text = "CONTAINER"
        ' 
        ' containerInput
        ' 
        containerInput.CharacterCasing = CharacterCasing.Upper
        containerInput.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        containerInput.Location = New Point(9, 30)
        containerInput.MaxLength = 18
        containerInput.Name = "containerInput"
        containerInput.Size = New Size(213, 29)
        containerInput.TabIndex = 1
        ' 
        ' headerModelCell
        ' 
        headerModelCell.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        headerModelCell.Controls.Add(captionModel)
        headerModelCell.Controls.Add(partPicker)
        headerModelCell.Location = New Point(1, 0)
        headerModelCell.Margin = New Padding(1)
        headerModelCell.Name = "headerModelCell"
        headerModelCell.Size = New Size(297, 82)
        headerModelCell.TabIndex = 1
        ' 
        ' captionModel
        ' 
        captionModel.AutoEllipsis = True
        captionModel.Font = New Font("Segoe UI", 8.5F, FontStyle.Bold)
        captionModel.ForeColor = Color.FromArgb(CByte(108), CByte(128), CByte(155))
        captionModel.Location = New Point(3, 9)
        captionModel.Name = "captionModel"
        captionModel.Size = New Size(240, 18)
        captionModel.TabIndex = 0
        captionModel.Text = "PART NUMBER / MODEL"
        ' 
        ' partPicker
        ' 
        partPicker.DisplayMember = "DisplayName"
        partPicker.DropDownStyle = ComboBoxStyle.DropDownList
        partPicker.Font = New Font("Segoe UI", 10.0F)
        partPicker.Location = New Point(8, 29)
        partPicker.Name = "partPicker"
        partPicker.Size = New Size(277, 25)
        partPicker.TabIndex = 1
        partPicker.ValueMember = "Id"
        ' 
        ' headerOperatorCell
        ' 
        headerOperatorCell.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        headerOperatorCell.Controls.Add(captionOperator)
        headerOperatorCell.Controls.Add(operatorValue)
        headerOperatorCell.Location = New Point(514, 0)
        headerOperatorCell.Margin = New Padding(1)
        headerOperatorCell.Name = "headerOperatorCell"
        headerOperatorCell.Size = New Size(221, 82)
        headerOperatorCell.TabIndex = 2
        ' 
        ' captionOperator
        ' 
        captionOperator.AutoEllipsis = True
        captionOperator.Font = New Font("Segoe UI", 8.5F, FontStyle.Bold)
        captionOperator.ForeColor = Color.FromArgb(CByte(108), CByte(128), CByte(155))
        captionOperator.Location = New Point(13, 6)
        captionOperator.Name = "captionOperator"
        captionOperator.Size = New Size(190, 18)
        captionOperator.TabIndex = 0
        captionOperator.Text = "OPERATOR"
        ' 
        ' operatorValue
        ' 
        operatorValue.AutoEllipsis = True
        operatorValue.Font = New Font("Segoe UI", 13.0F, FontStyle.Bold)
        operatorValue.ForeColor = Color.FromArgb(CByte(31), CByte(78), CByte(121))
        operatorValue.Location = New Point(13, 29)
        operatorValue.Name = "operatorValue"
        operatorValue.Size = New Size(190, 33)
        operatorValue.TabIndex = 1
        ' 
        ' headerCustomerCell
        ' 
        headerCustomerCell.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        headerCustomerCell.Controls.Add(captionCustomer)
        headerCustomerCell.Controls.Add(customerValue)
        headerCustomerCell.Location = New Point(735, 0)
        headerCustomerCell.Margin = New Padding(1)
        headerCustomerCell.Name = "headerCustomerCell"
        headerCustomerCell.Size = New Size(160, 82)
        headerCustomerCell.TabIndex = 3
        ' 
        ' captionCustomer
        ' 
        captionCustomer.AutoEllipsis = True
        captionCustomer.Font = New Font("Segoe UI", 8.5F, FontStyle.Bold)
        captionCustomer.ForeColor = Color.FromArgb(CByte(108), CByte(128), CByte(155))
        captionCustomer.Location = New Point(9, 6)
        captionCustomer.Name = "captionCustomer"
        captionCustomer.Size = New Size(116, 18)
        captionCustomer.TabIndex = 0
        captionCustomer.Text = "CUSTOMER"
        ' 
        ' customerValue
        ' 
        customerValue.AutoEllipsis = True
        customerValue.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        customerValue.ForeColor = Color.FromArgb(CByte(31), CByte(78), CByte(121))
        customerValue.Location = New Point(9, 30)
        customerValue.Name = "customerValue"
        customerValue.Size = New Size(116, 33)
        customerValue.TabIndex = 1
        ' 
        ' headerTimeCell
        ' 
        headerTimeCell.BackColor = Color.FromArgb(CByte(255), CByte(255), CByte(255))
        headerTimeCell.Controls.Add(captionTime)
        headerTimeCell.Controls.Add(clockValue)
        headerTimeCell.Location = New Point(895, 0)
        headerTimeCell.Margin = New Padding(1)
        headerTimeCell.Name = "headerTimeCell"
        headerTimeCell.Size = New Size(191, 82)
        headerTimeCell.TabIndex = 4
        ' 
        ' captionTime
        ' 
        captionTime.AutoEllipsis = True
        captionTime.Font = New Font("Segoe UI", 8.5F, FontStyle.Bold)
        captionTime.ForeColor = Color.FromArgb(CByte(108), CByte(128), CByte(155))
        captionTime.Location = New Point(9, 6)
        captionTime.Name = "captionTime"
        captionTime.Size = New Size(163, 18)
        captionTime.TabIndex = 0
        captionTime.Text = "DATE / TIME"
        ' 
        ' clockValue
        ' 
        clockValue.AutoEllipsis = True
        clockValue.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        clockValue.Location = New Point(9, 30)
        clockValue.Name = "clockValue"
        clockValue.Size = New Size(163, 33)
        clockValue.TabIndex = 1
        ' 
        ' DataGridView1
        ' 
        DataGridView1.AutoGenerateColumns = False
        DataGridView1.AllowUserToAddRows = False
        DataGridView1.AllowUserToDeleteRows = False
        DataGridView1.ReadOnly = True
        DataGridView1.MultiSelect = False
        DataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        DataGridView1.RowHeadersVisible = False
        DataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        colContainer.Name = "colContainer"
        colContainer.HeaderText = "Contenedor"
        colContainer.DataPropertyName = "ContainerCode"
        colContainer.FillWeight = 85.0F
        colSsiModel.Name = "colSsiModel"
        colSsiModel.HeaderText = "Modelo SSI"
        colSsiModel.DataPropertyName = "SsiModel"
        colSsiModel.FillWeight = 85.0F
        colLevel.Name = "colLevel"
        colLevel.HeaderText = "Nivel"
        colLevel.DataPropertyName = "LevelNumber"
        colLevel.FillWeight = 40.0F
        colLevelPiece.Name = "colLevelPiece"
        colLevelPiece.HeaderText = "Pieza"
        colLevelPiece.DataPropertyName = "PieceNumber"
        colLevelPiece.FillWeight = 40.0F
        colSsiSerial.Name = "colSsiSerial"
        colSsiSerial.HeaderText = "Serie SSI"
        colSsiSerial.DataPropertyName = "SsiSerial"
        colSsiSerial.FillWeight = 105.0F
        colClientSerial.Name = "colClientSerial"
        colClientSerial.HeaderText = "Serie del cliente"
        colClientSerial.DataPropertyName = "ClientSerial"
        colClientSerial.FillWeight = 105.0F
        colScanTime.Name = "colScanTime"
        colScanTime.HeaderText = "Fecha y hora de escaneo"
        colScanTime.DataPropertyName = "ScanTime"
        colScanTime.FillWeight = 90.0F
        DataGridView1.Columns.AddRange(New DataGridViewColumn() {colContainer, colSsiModel, colLevel, colLevelPiece, colSsiSerial, colClientSerial, colScanTime})
        DataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridView1.Location = New Point(12, 414)
        DataGridView1.Name = "DataGridView1"
        DataGridView1.Size = New Size(1357, 331)
        DataGridView1.TabIndex = 2
        ' 
        ' PalletScannerView
        ' 
        BackColor = Color.FromArgb(CByte(245), CByte(248), CByte(252))
        Controls.Add(mainLayout)
        Font = New Font("Segoe UI", 10.0F)
        MinimumSize = New Size(990, 630)
        Name = "PalletScannerView"
        Size = New Size(1382, 760)
        mainLayout.ResumeLayout(False)
        scanArea.ResumeLayout(False)
        scanArea.PerformLayout()
        leftLayout.ResumeLayout(False)
        piecesArea.ResumeLayout(False)
        progressArea.ResumeLayout(False)
        progressArea.PerformLayout()
        headerPage.ResumeLayout(False)
        headerLayout.ResumeLayout(False)
        activeStrip.ResumeLayout(False)
        headerContainerCell.ResumeLayout(False)
        headerContainerCell.PerformLayout()
        headerModelCell.ResumeLayout(False)
        headerOperatorCell.ResumeLayout(False)
        headerCustomerCell.ResumeLayout(False)
        headerTimeCell.ResumeLayout(False)
        CType(DataGridView1, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
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
    Friend WithEvents scanArea As System.Windows.Forms.Panel
    Friend WithEvents progressArea As System.Windows.Forms.Panel
    Friend WithEvents scanTitle As System.Windows.Forms.Label
    Friend WithEvents progressTitle As System.Windows.Forms.Label
    Friend WithEvents scanHelp As System.Windows.Forms.Label
    Friend WithEvents ssiCaption As System.Windows.Forms.Label
    Friend WithEvents clientScanLabel As System.Windows.Forms.Label
    Friend WithEvents firstQr As System.Windows.Forms.TextBox
    Friend WithEvents secondQr As System.Windows.Forms.TextBox
    Friend WithEvents scanStatus As System.Windows.Forms.Label
    Friend WithEvents progressIcon As System.Windows.Forms.Label
    Friend WithEvents progressPercent As System.Windows.Forms.Label
    Friend WithEvents progressState As System.Windows.Forms.Label
    Friend WithEvents progressBar As System.Windows.Forms.ProgressBar
    Friend WithEvents progressText As System.Windows.Forms.Label
    Friend WithEvents levelTiles As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents confirmAgain As System.Windows.Forms.Button
    Friend WithEvents ruleSummary As System.Windows.Forms.Label
    Friend WithEvents leftLayout As Panel
    Friend WithEvents piecesArea As Panel
    Friend WithEvents piecesTitle As Label
    Friend WithEvents scannedList As ListBox
    Friend WithEvents lbl_lot As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents colContainer As DataGridViewTextBoxColumn
    Friend WithEvents colSsiModel As DataGridViewTextBoxColumn
    Friend WithEvents colLevel As DataGridViewTextBoxColumn
    Friend WithEvents colLevelPiece As DataGridViewTextBoxColumn
    Friend WithEvents colSsiSerial As DataGridViewTextBoxColumn
    Friend WithEvents colClientSerial As DataGridViewTextBoxColumn
    Friend WithEvents colScanTime As DataGridViewTextBoxColumn
End Class
