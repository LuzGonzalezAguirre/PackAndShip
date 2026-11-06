Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

' SQL-backed scanner: Q-Wall PASS and global duplicate checks execute before recording.
' Production rollout still requires end-to-end and concurrency validation.
Public Class PalletScannerView
    Inherits UserControl

    Private ReadOnly navy As Color = PaSTheme.Navy
    Private ReadOnly containerInput As New TextBox()
    Private ReadOnly partPicker As New ComboBox()
    Private ReadOnly customerValue As New Label()
    Private ReadOnly operatorValue As New Label()
    Private ReadOnly clockValue As New Label()
    Private ReadOnly firstQr As New TextBox()
    Private ReadOnly secondQr As New TextBox()
    Private ReadOnly scanHelp As New Label()
    Private ReadOnly clientScanLabel As New Label()
    Private ReadOnly scanStatus As New Label()
    Private ReadOnly palletSummary As New Label()
    Private ReadOnly ruleSummary As New Label()
    Private ReadOnly verifyButton As Button = PaSTheme.ActionButton("Verify QR format")
    Private ReadOnly openButton As Button = PaSTheme.ActionButton("Open / Load", False)
    Private ReadOnly pendingButton As New ToolStripButton("Review pending SQL")
    Private ReadOnly recordButton As Button = PaSTheme.ActionButton("Record piece")
    Private ReadOnly levelButton As Button = PaSTheme.ActionButton("Close level", False)
    Private ReadOnly partialButton As New ToolStripButton("Partial pallet")
    Private ReadOnly cancelEmptyButton As New ToolStripButton("Cancel pallet")
    Private activeContainer As PaSPackedContainer
    Private verifiedSsi As String = ""
    Private verifiedClient As String = ""
    Private ReadOnly clockTimer As New Timer With {.Interval = 1000}
    Private parts As New List(Of PaSPart)()
    Private selectedConfiguration As PalletConfiguration
    Private selectedRule As ScanRuleRecord
    Private selectedFields As New List(Of ScanFieldRecord)()
    Private palletFormat As PalletIdSettings

    Public Sub New()
        Dock = DockStyle.Fill
        BackColor = PaSTheme.Background
        AutoScroll = True
        Font = New Font("Segoe UI", 10)
        BuildLayout()
        AddHandler clockTimer.Tick, Sub() clockValue.Text = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss")
        clockTimer.Start()
        AddHandler Disposed, Sub() clockTimer.Stop()
    End Sub

    Private Function FieldCaption(value As String, x As Integer, y As Integer) As Label
        Return New Label With {
            .Text = value, .Location = New Point(x, y),
            .Size = New Size(310, 22),
            .ForeColor = PaSTheme.TextSecondary,
            .Font = New Font("Segoe UI", 9, FontStyle.Bold)
        }
    End Function

    Private ReadOnly activePartTitle As New Label()
    Private ReadOnly buildState As New Label()
    Private ReadOnly progressText As New Label()
    Private ReadOnly progressBar As New ProgressBar()
    Private ReadOnly scannedList As New ListBox()
    Private ReadOnly closePallet As Button = PaSTheme.ActionButton("Complete pallet")
    Private ReadOnly headerCards As New List(Of Panel)()
    Private ReadOnly packingTools As New ToolStrip()
    Private ReadOnly progressIcon As New Label()
    Private ReadOnly progressPercent As New Label()
    Private ReadOnly progressState As New Label()
    Private ReadOnly levelTiles As New FlowLayoutPanel()
    Private ReadOnly confirmAgain As Button = PaSTheme.ActionButton("Confirm packed level", False)
    Private autoConfirmVisible As Boolean
    Private autoScanBusy As Boolean
    Private autoOpenBusy As Boolean
    Private ReadOnly headerPage As New Panel()
    Private ReadOnly bodyLeft As New Panel()
    Private ReadOnly bodyRight As New Panel()
    Private scanArea As New Panel()
    Private buildArea As New Panel()
    Private piecesArea As New Panel()
    Private progressArea As New Panel()
    Private validationArea As New Panel()
    Private ReadOnly mainPage As New Panel()

    Private Function Card(title As String, parent As Control, x As Integer,
                          y As Integer, width As Integer, height As Integer) As Panel
        Dim panel As New Panel With {
            .Location = New Point(x, y),
            .Size = New Size(width, height),
            .BackColor = Color.White,
            .BorderStyle = BorderStyle.FixedSingle
        }
        parent.Controls.Add(panel)
        Dim label = PaSTheme.Heading(title, 13)
        label.Location = New Point(18, 14)
        panel.Controls.Add(label)
        Return panel
    End Function

    Private Function HeaderCell(title As String, value As Control,
                                portion As Single) As Panel
        Dim cell As New Panel With {
            .Dock = DockStyle.Fill,
            .BackColor = Color.White,
            .Margin = New Padding(2, 1, 2, 1),
            .Padding = New Padding(10)
        }
        Dim caption = FieldCaption(title, 9, 7)
        caption.Font = New Font("Segoe UI", 8.5F, FontStyle.Bold)
        caption.Size = New Size(220, 22)
        cell.Controls.Add(caption)
        value.Location = New Point(9, 31)
        value.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Right
        value.Width = 210
        cell.Controls.Add(value)
        headerCards.Add(cell)
        Return cell
    End Function

    Private Sub BuildLayout()
        mainPage.Dock = DockStyle.Fill
        mainPage.AutoScroll = True
        mainPage.AutoScrollMinSize = New Size(1060, 700)
        mainPage.BackColor = PaSTheme.Background
        Controls.Add(mainPage)

        ' Compact information header with active build integrated below.
        headerPage.Location = New Point(16, 12)
        headerPage.Size = New Size(1028, 136)
        headerPage.BackColor = PaSTheme.Border
        mainPage.Controls.Add(headerPage)
        Dim strip As New TableLayoutPanel With {
            .Dock = DockStyle.Top, .Height = 85, .ColumnCount = 5,
            .RowCount = 1, .BackColor = PaSTheme.Border, .Padding = New Padding(1)
        }
        strip.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        For Each weight As Single In {17.0F, 27.0F, 22.0F, 14.0F, 20.0F}
            strip.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, weight))
        Next
        headerPage.Controls.Add(strip)

        containerInput.CharacterCasing = CharacterCasing.Upper
        containerInput.MaxLength = 18
        containerInput.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        partPicker.DropDownStyle = ComboBoxStyle.DropDownList
        partPicker.DisplayMember = "DisplayName"
        partPicker.ValueMember = "Id"
        partPicker.Font = New Font("Segoe UI", 10)
        operatorValue.Font = New Font("Segoe UI", 13, FontStyle.Bold)
        operatorValue.ForeColor = navy
        customerValue.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        customerValue.ForeColor = navy
        clockValue.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        clockValue.TextAlign = ContentAlignment.MiddleLeft
        strip.Controls.Add(HeaderCell("CONTAINER", containerInput, 17), 0, 0)
        strip.Controls.Add(HeaderCell("PART NUMBER / MODEL", partPicker, 27), 1, 0)
        strip.Controls.Add(HeaderCell("OPERATOR", operatorValue, 22), 2, 0)
        strip.Controls.Add(HeaderCell("CUSTOMER", customerValue, 14), 3, 0)
        strip.Controls.Add(HeaderCell("DATE / TIME", clockValue, 20), 4, 0)

        Dim activeStrip As New Panel With {
            .Dock = DockStyle.Bottom, .Height = 50, .BackColor = Color.White
        }
        headerPage.Controls.Add(activeStrip)
        Dim activeCaption As New Label With {
            .Text = "ACTIVE BUILD", .ForeColor = PaSTheme.TextSecondary,
            .Font = New Font("Segoe UI", 8, FontStyle.Bold),
            .Location = New Point(12, 7), .Size = New Size(110, 17)
        }
        activePartTitle.Location = New Point(130, 4)
        activePartTitle.Size = New Size(460, 25)
        activePartTitle.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        activePartTitle.ForeColor = PaSTheme.Navy
        activePartTitle.AutoEllipsis = True
        buildState.Location = New Point(130, 29)
        buildState.Size = New Size(720, 18)
        buildState.Font = New Font("Segoe UI", 9)
        buildState.ForeColor = PaSTheme.TextSecondary
        activeStrip.Controls.AddRange(New Control() {activeCaption, activePartTitle, buildState})

        ' Always-visible operational ToolStrip, not packing progress buttons.
        packingTools.Location = New Point(16, 154)
        packingTools.AutoSize = False
        packingTools.Size = New Size(1028, 38)
        packingTools.GripStyle = ToolStripGripStyle.Hidden
        packingTools.RenderMode = ToolStripRenderMode.System
        packingTools.BackColor = Color.White
        packingTools.Padding = New Padding(8, 2, 8, 2)
        packingTools.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        partialButton.Text = "◧  Partial pallet"
        cancelEmptyButton.Text = "✕  Cancel empty pallet"
        partialButton.DisplayStyle = ToolStripItemDisplayStyle.Text
        cancelEmptyButton.DisplayStyle = ToolStripItemDisplayStyle.Text
        pendingButton.DisplayStyle = ToolStripItemDisplayStyle.Text
        pendingButton.Visible = False
        AddHandler partialButton.Click, AddressOf ClosePartialPallet
        AddHandler cancelEmptyButton.Click, AddressOf CancelEmptyPallet
        AddHandler pendingButton.Click, AddressOf ReviewPendingSql
        packingTools.Items.Add(partialButton)
        packingTools.Items.Add(New ToolStripSeparator())
        packingTools.Items.Add(cancelEmptyButton)
        packingTools.Items.Add(New ToolStripSeparator())
        packingTools.Items.Add(pendingButton)
        mainPage.Controls.Add(packingTools)

        ' The legacy button objects are kept as nonvisual state guards.
        AddHandler openButton.Click, AddressOf OpenOrLoadContainer
        AddHandler verifyButton.Click, Sub() VerifyQrFormat()
        AddHandler recordButton.Click, AddressOf RecordPiece
        AddHandler levelButton.Click, AddressOf CloseLevel
        AddHandler closePallet.Click, AddressOf CompletePallet

        AddHandler partPicker.SelectedIndexChanged, AddressOf PartChanged
        AddHandler containerInput.TextChanged, Sub()
            If activeContainer IsNot Nothing AndAlso
               activeContainer.Status <> "OPEN" AndAlso
               Not String.Equals(activeContainer.Code, containerInput.Text.Trim(),
                                 StringComparison.OrdinalIgnoreCase) Then
                activeContainer = Nothing
            End If
            ResetVerification()
            UpdateScannerAvailability()
        End Sub
        AddHandler containerInput.KeyDown, AddressOf ContainerEnter
        AddHandler firstQr.TextChanged, Sub()
            ResetVerification()
            If selectedRule IsNot Nothing AndAlso selectedRule.ScanCount = 1 Then
                secondQr.Clear()
            End If
        End Sub
        AddHandler secondQr.TextChanged, Sub() ResetVerification()
        AddHandler firstQr.KeyDown, AddressOf FirstLabelEnter
        AddHandler secondQr.KeyDown, AddressOf SecondLabelEnter

        bodyLeft.Location = New Point(16, 204)
        bodyLeft.Size = New Size(420, 490)
        bodyRight.Location = New Point(448, 204)
        bodyRight.Size = New Size(596, 490)
        mainPage.Controls.Add(bodyLeft)
        mainPage.Controls.Add(bodyRight)

        scanArea = Card("SCAN LABELS", bodyLeft, 0, 0, 420, 306)
        scanHelp.Text = "Scanner ready. Each QR sends Enter automatically."
        scanHelp.SetBounds(18, 49, 375, 35)
        scanHelp.ForeColor = PaSTheme.TextSecondary
        scanArea.Controls.Add(scanHelp)
        scanArea.Controls.Add(FieldCaption("01  SSI SERIAL", 18, 93))
        firstQr.SetBounds(18, 118, 382, 43)
        firstQr.Font = New Font("Consolas", 13, FontStyle.Bold)
        scanArea.Controls.Add(firstQr)
        clientScanLabel.Text = "02  CLIENT SERIAL"
        clientScanLabel.SetBounds(18, 175, 340, 23)
        clientScanLabel.ForeColor = PaSTheme.TextSecondary
        clientScanLabel.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        scanArea.Controls.Add(clientScanLabel)
        secondQr.SetBounds(18, 202, 382, 43)
        secondQr.Font = New Font("Consolas", 13, FontStyle.Bold)
        scanArea.Controls.Add(secondQr)
        scanStatus.SetBounds(18, 255, 382, 46)
        scanStatus.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        scanStatus.ForeColor = PaSTheme.TextSecondary
        scanArea.Controls.Add(scanStatus)

        piecesArea = Card("RECENTLY SCANNED", bodyLeft, 0, 320, 420, 170)
        scannedList.SetBounds(18, 48, 382, 103)
        scannedList.Font = New Font("Consolas", 9.5F)
        scannedList.BorderStyle = BorderStyle.None
        scannedList.BackColor = Color.FromArgb(248, 250, 253)
        scannedList.Enabled = False
        piecesArea.Controls.Add(scannedList)

        progressArea = Card("PALLET PROGRESS", bodyRight, 0, 0, 596, 490)
        progressArea.BackColor = Color.White
        progressIcon.Text = "▣"
        progressIcon.SetBounds(24, 57, 72, 72)
        progressIcon.ForeColor = PaSTheme.Navy
        progressIcon.Font = New Font("Segoe UI Symbol", 43, FontStyle.Bold)
        progressIcon.TextAlign = ContentAlignment.MiddleCenter
        progressArea.Controls.Add(progressIcon)
        progressPercent.SetBounds(104, 59, 390, 72)
        progressPercent.Font = New Font("Segoe UI", 33, FontStyle.Bold)
        progressPercent.ForeColor = PaSTheme.Navy
        progressArea.Controls.Add(progressPercent)
        progressState.SetBounds(25, 148, 530, 38)
        progressState.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        progressState.ForeColor = Color.FromArgb(28, 113, 83)
        progressArea.Controls.Add(progressState)
        progressBar.SetBounds(25, 202, 536, 22)
        progressBar.Minimum = 0
        progressBar.Maximum = 100
        progressArea.Controls.Add(progressBar)
        progressText.SetBounds(25, 244, 530, 96)
        progressText.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        progressText.ForeColor = PaSTheme.TextPrimary
        progressArea.Controls.Add(progressText)
        levelTiles.SetBounds(25, 344, 536, 62)
        levelTiles.AutoScroll = True
        levelTiles.WrapContents = True
        levelTiles.BackColor = Color.White
        progressArea.Controls.Add(levelTiles)
        confirmAgain.Text = "Review packed level"
        confirmAgain.SetBounds(25, 396, 536, 51)
        AddHandler confirmAgain.Click, Sub() PromptCompletedLevel()
        progressArea.Controls.Add(confirmAgain)
        confirmAgain.Enabled = False

        AddHandler mainPage.Resize, Sub() ResizeScannerLayout()
        ResizeScannerLayout()
        activePartTitle.Text = "Select a model"
        buildState.Text = "Scan a container barcode and press Enter"
        progressText.Text = "0 / 0 pieces" & Environment.NewLine & "0 levels complete"
        progressPercent.Text = "0%"
        progressState.Text = "WAITING FOR PALLET"
        scanStatus.Text = "Select a model, then scan the container."
    End Sub

    Private Sub ResizeScannerLayout()
        Dim totalWidth = Math.Max(1028, mainPage.ClientSize.Width - 32)
        Dim leftWidth = Math.Max(410, CInt(totalWidth * 0.40))
        Dim rightWidth = totalWidth - leftWidth - 12
        headerPage.Width = totalWidth
        packingTools.Width = totalWidth
        bodyLeft.Width = leftWidth
        bodyRight.Left = bodyLeft.Left + leftWidth + 12
        bodyRight.Width = rightWidth
        For Each cell In headerCards
            For Each ctl As Control In cell.Controls
                If TypeOf ctl Is TextBox OrElse TypeOf ctl Is ComboBox OrElse
                   (TypeOf ctl Is Label AndAlso ctl.Location.Y >= 30) Then
                    ctl.Width = Math.Max(55, cell.ClientSize.Width - 18)
                End If
            Next
        Next
        Dim available = Math.Max(105, mainPage.ClientSize.Height - 219)
        bodyLeft.Height = available
        bodyRight.Height = available
        scanArea.Width = leftWidth
        piecesArea.Width = leftWidth
        piecesArea.Height = Math.Max(160, available - 320)
        scannedList.Width = leftWidth - 36
        scannedList.Height = piecesArea.Height - 67
        firstQr.Width = leftWidth - 36
        secondQr.Width = leftWidth - 36
        scanHelp.Width = leftWidth - 36
        scanStatus.Width = leftWidth - 36
        progressArea.Width = rightWidth
        progressArea.Height = Math.Max(490, available)
        progressText.Width = rightWidth - 50
        progressPercent.Width = rightWidth - 128
        progressState.Width = rightWidth - 50
        progressBar.Width = rightWidth - 50
        levelTiles.Width = rightWidth - 50
        confirmAgain.Width = rightWidth - 50
        confirmAgain.Top = progressArea.Height - 84
    End Sub

    Private Sub ContainerEnter(sender As Object, e As KeyEventArgs)
        If e.KeyCode <> Keys.Enter Then Return
        e.SuppressKeyPress = True
        If autoOpenBusy OrElse activeContainer IsNot Nothing AndAlso
           activeContainer.Status = "OPEN" Then Return
        If Not openButton.Enabled Then
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "Select a configured model or resolve the pending SQL action."
            Return
        End If
        autoOpenBusy = True
        Try
            OpenOrLoadContainer(sender, EventArgs.Empty)
            If activeContainer IsNot Nothing AndAlso activeContainer.Status = "OPEN" Then
                scanStatus.Text = "Container ready — scan SSI serial."
                firstQr.Focus()
            End If
        Finally
            autoOpenBusy = False
        End Try
    End Sub

    Private Sub FirstLabelEnter(sender As Object, e As KeyEventArgs)
        If e.KeyCode <> Keys.Enter Then Return
        e.SuppressKeyPress = True
        If autoScanBusy Then Return
        If selectedRule IsNot Nothing AndAlso selectedRule.ScanCount = 1 Then
            VerifyAndRecord()
        Else
            secondQr.Focus()
        End If
    End Sub

    Private Sub SecondLabelEnter(sender As Object, e As KeyEventArgs)
        If e.KeyCode <> Keys.Enter Then Return
        e.SuppressKeyPress = True
        VerifyAndRecord()
    End Sub

    Private Sub VerifyAndRecord()
        If autoScanBusy Then Return
        autoScanBusy = True
        Try
            If activeContainer Is Nothing OrElse activeContainer.Status <> "OPEN" Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "Open a pallet before scanning pieces."
                Return
            End If
            If activeContainer.CurrentLevel Is Nothing OrElse
               activeContainer.CurrentLevel.ScannedPieces >= activeContainer.CurrentLevel.RequiredPieces Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "Confirm the full level before scanning more pieces."
                Return
            End If
            VerifyQrFormat()
            If recordButton.Enabled Then
                RecordPiece(Me, EventArgs.Empty)
            Else
                firstQr.Focus()
            End If
        Finally
            autoScanBusy = False
        End Try
    End Sub

    Public Sub LoadStation()
        operatorValue.Text = If(SessionContext.FullName, "")
        customerValue.Text = If(SessionContext.BusinessUnitName, "")
        clockValue.Text = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss")
        Try
            palletFormat = PalletIdSettings.Load()
            parts = PalletConfigurationData.GetParts().
                Where(Function(p) p.BusinessUnitId.HasValue AndAlso
                       p.BusinessUnitId.Value = SessionContext.BusinessUnitId).
                OrderBy(Function(p) p.Number).ToList()
            partPicker.DataSource = Nothing
            partPicker.DataSource = parts
            partPicker.SelectedIndex = -1
            selectedConfiguration = Nothing
            selectedRule = Nothing
            selectedFields.Clear()
            activePartTitle.Text = "Select a part number"
            buildState.Text = "Waiting for a container and model"
            progressText.Text = "0 / 0 pieces" & Environment.NewLine & "0 levels complete"
            progressBar.Value = 0
            activeContainer = Nothing
            ResetVerification()
            scannedList.Items.Clear()
            scannedList.Items.Add("Open or load a container to view its pieces.")
            ruleSummary.Text = "Container example: " & palletFormat.Example() &
                               Environment.NewLine & "Q-Wall PASS: Check on scan" &
                               Environment.NewLine & "Duplicate prevention: SQL UNIQUE(product_id)"
            containerInput.Clear()
            firstQr.Clear()
            secondQr.Clear()
            ConfigureScanMode()
            UpdateScannerAvailability()

            ' Restore the workstation's active pallet after logout/restart.
            Dim lockedCode = StationSettings.ReadLockedPallet()
            If lockedCode <> "" Then
                Dim saved = PaSPackingData.FindContainer(lockedCode)
                If saved Is Nothing Then
                    Throw New InvalidOperationException(
                        "Workstation is locked to pallet " & lockedCode &
                        ", but CCS cannot find it. IT review is required.")
                End If
                If saved.Status = "OPEN" Then
                    Dim model = parts.FirstOrDefault(Function(x) x.Id = saved.PartId)
                    If model Is Nothing Then
                        Throw New InvalidOperationException(
                            "Open pallet " & lockedCode &
                            " belongs to another customer/model. Finish it before changing station setup.")
                    End If
                    partPicker.SelectedItem = model
                    containerInput.Text = lockedCode
                    activeContainer = saved
                    RefreshContainer()
                    scanStatus.Text = "Resumed locked pallet " & lockedCode &
                                      ". Finish or authorize partial closure before switching."
                Else
                    StationSettings.UnlockPallet(lockedCode)
                End If
            End If
            containerInput.Focus()
        Catch ex As Exception
            verifyButton.Enabled = False
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "Cannot load scanner setup: " & ex.Message
        End Try
    End Sub

    Private Sub PartChanged(sender As Object, e As EventArgs)
        activeContainer = Nothing
        ResetVerification()
        selectedConfiguration = Nothing
        selectedRule = Nothing
        selectedFields.Clear()
        Dim part = TryCast(partPicker.SelectedItem, PaSPart)
        If part Is Nothing Then
            ConfigureScanMode()
            UpdateScannerAvailability()
            Return
        End If
        Try
            selectedConfiguration = PalletConfigurationData.GetConfiguration(part.Id)
            selectedRule = ScanRulesData.ReadRules().FirstOrDefault(
                Function(r) r.PartId = part.Id AndAlso
                    r.CustomerId = SessionContext.BusinessUnitId AndAlso r.IsActive)
            If selectedRule IsNot Nothing Then
                selectedFields = ScanRulesData.ReadFields(selectedRule.Id)
            End If
            If selectedConfiguration Is Nothing OrElse Not selectedConfiguration.IsActive Then
                activePartTitle.Text = part.DisplayName
                buildState.Text = "Pallet configuration missing or inactive"
                progressText.Text = "0 / 0 pieces" & Environment.NewLine & "0 levels complete"
            Else
                activePartTitle.Text = part.DisplayName
                buildState.Text = "Container: " & containerInput.Text.Trim().ToUpperInvariant() &
                    "   |   " & selectedConfiguration.Levels.Count & " configured levels"
                progressText.Text = "0 / " & selectedConfiguration.TotalPieces &
                    " pieces" & Environment.NewLine &
                    "0 / " & selectedConfiguration.Levels.Count & " levels complete" &
                    Environment.NewLine & "Pieces per level: " &
                    String.Join(" / ", selectedConfiguration.Levels)
                progressBar.Value = 0
            End If
            ruleSummary.Text = If(selectedRule Is Nothing,
                "Scan rule: Inactive / missing", "Configured scans: " & selectedRule.ScanCount &
                Environment.NewLine & "Match required: " & If(selectedRule.RequiresMatch, "Yes", "No")) &
                Environment.NewLine & "Q-Wall PASS: Check on scan" &
                Environment.NewLine & "Duplicate prevention: SQL UNIQUE(product_id)" &
                Environment.NewLine & "Container format: General Settings"
        Catch ex As Exception
            buildState.Text = "Unable to read configuration."
            ruleSummary.Text = ex.Message
        End Try
        firstQr.Clear()
        secondQr.Clear()
        ConfigureScanMode()
        UpdateScannerAvailability()
    End Sub

    Private Sub ConfigureScanMode()
        Dim singleScan = selectedRule IsNot Nothing AndAlso selectedRule.ScanCount = 1
        scanHelp.Text = If(singleScan,
            "Scan SSI once. Client serial is generated from the selected model.",
            "Scan SSI first, then Client. Press Enter after each QR.")
        clientScanLabel.Text = If(singleScan,
            "CLIENT SERIAL — AUTOMATIC PREVIEW", "SCAN 2 — CLIENT SERIAL")
        secondQr.ReadOnly = singleScan
        secondQr.TabStop = Not singleScan
        secondQr.BackColor = If(singleScan, Color.FromArgb(242, 247, 252), Color.White)
        secondQr.Clear()
    End Sub

    Private Sub UpdateScannerAvailability()
        Dim part = TryCast(partPicker.SelectedItem, PaSPart)
        Dim validContainer = palletFormat IsNot Nothing AndAlso palletFormat.IsValid(containerInput.Text)
        verifyButton.Enabled = validContainer AndAlso part IsNot Nothing AndAlso
                               selectedConfiguration IsNot Nothing AndAlso selectedConfiguration.IsActive AndAlso
                               selectedRule IsNot Nothing AndAlso
                               (selectedRule.ScanCount = 1 OrElse selectedRule.ScanCount = 2)
        If Not validContainer Then
            scanStatus.Text = "Enter a container ID matching General Settings."
        ElseIf selectedRule Is Nothing OrElse
               (selectedRule.ScanCount <> 1 AndAlso selectedRule.ScanCount <> 2) Then
            scanStatus.Text = "An active one- or two-scan rule is required for this part."
        ElseIf selectedConfiguration Is Nothing OrElse Not selectedConfiguration.IsActive Then
            scanStatus.Text = "Pallet configuration is missing or inactive."
        Else
            scanStatus.Text = If(selectedRule.ScanCount = 1,
                "Ready for SSI scan; Client serial generated after format validation.",
                "Ready for both label scans.")
            scanStatus.ForeColor = navy
        End If
        UpdatePackingButtons()
    End Sub

    Private Sub SetQWallStatus(value As String)
        Dim lines = ruleSummary.Text.Split(New String() {Environment.NewLine},
                                          StringSplitOptions.None)
        For index As Integer = 0 To lines.Length - 1
            If lines(index).StartsWith("Q-Wall PASS:", StringComparison.Ordinal) Then
                lines(index) = "Q-Wall PASS: " & value
            End If
        Next
        ruleSummary.Text = String.Join(Environment.NewLine, lines)
    End Sub

    Private Sub ResetVerification()
        verifiedSsi = ""
        verifiedClient = ""
        recordButton.Enabled = False
    End Sub

    Private Sub UpdatePackingButtons()
        Dim openNow = activeContainer IsNot Nothing AndAlso activeContainer.Status = "OPEN"
        Dim current As PaSPackedLevel = Nothing
        If openNow Then current = activeContainer.CurrentLevel
        Dim pendingOp = ConnectionRecovery.Pending()
        Dim pending = pendingOp IsNot Nothing
        pendingButton.Visible = pending
        If pending Then
            pendingButton.Text = If(pendingOp.ReviewRequired,
                                    "Review blocked SQL action", "Check pending SQL action")
            pendingButton.Enabled = True
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "Packing is blocked by pending SQL action: " &
                pendingOp.Kind & " (Container ID " & pendingOp.ContainerId & "). " &
                "Click Review pending SQL."
        End If
        Dim lockedCode = StationSettings.ReadLockedPallet()
        Dim locked = openNow OrElse lockedCode <> ""
        containerInput.ReadOnly = locked
        partPicker.Enabled = Not locked
        openButton.Enabled = Not pending AndAlso Not openNow AndAlso lockedCode = ""
        Dim part = TryCast(partPicker.SelectedItem, PaSPart)
        verifyButton.Enabled = Not pending AndAlso
            palletFormat IsNot Nothing AndAlso palletFormat.IsValid(containerInput.Text) AndAlso
            part IsNot Nothing AndAlso selectedConfiguration IsNot Nothing AndAlso
            selectedConfiguration.IsActive AndAlso selectedRule IsNot Nothing AndAlso
            (selectedRule.ScanCount = 1 OrElse selectedRule.ScanCount = 2)
        recordButton.Enabled = Not pending AndAlso openNow AndAlso current IsNot Nothing AndAlso
            verifiedSsi <> "" AndAlso verifiedClient <> "" AndAlso
            firstQr.Text.Trim() = verifiedSsi AndAlso
            selectedRule IsNot Nothing AndAlso
            (selectedRule.ScanCount = 1 OrElse secondQr.Text.Trim() = verifiedClient)
        levelButton.Enabled = Not pending AndAlso openNow AndAlso current IsNot Nothing AndAlso
            current.ScannedPieces = current.RequiredPieces
        closePallet.Enabled = Not pending AndAlso openNow AndAlso current Is Nothing AndAlso
            activeContainer.Levels.Count > 0 AndAlso
            activeContainer.Levels.All(Function(l) l.Status = "CLOSED" AndAlso
                                      l.ScannedPieces = l.RequiredPieces)
        partialButton.Enabled = Not pending AndAlso openNow AndAlso
            activeContainer.PieceTotal > 0 AndAlso
            activeContainer.PieceTotal < activeContainer.RequiredTotal
        cancelEmptyButton.Enabled = Not pending AndAlso openNow AndAlso
            activeContainer.PieceTotal = 0 AndAlso
            activeContainer.ClosedLevels = 0
        confirmAgain.Enabled = levelButton.Enabled
        confirmAgain.Visible = levelButton.Enabled
        confirmAgain.Text = If(current IsNot Nothing AndAlso
                               current.LevelNumber = activeContainer.Levels.Count,
                               "Review final pallet completion",
                               "Review packed level")
    End Sub

    Private Sub ReviewPendingSql(sender As Object, e As EventArgs)
        Try
            Dim p = ConnectionRecovery.Pending()
            If p Is Nothing Then
                AfterRecovery()
                Return
            End If
            Dim description = ConnectionRecovery.DescribePending()
            Dim result = ConnectionRecovery.CheckAndRecover()
            If ConnectionRecovery.Pending() Is Nothing Then
                AfterRecovery(result)
                MessageBox.Show(
                    description & Environment.NewLine & Environment.NewLine &
                    "CCS result: " & result & Environment.NewLine &
                    "No pending action remains. Packing controls are available again.",
                    "SQL action reconciled", MessageBoxButtons.OK,
                    MessageBoxIcon.Information)
                Return
            End If
            Dim current = ConnectionRecovery.Pending()
            If current IsNot Nothing AndAlso current.ReviewRequired Then
                If MessageBox.Show(
                    description & Environment.NewLine & Environment.NewLine &
                    "CCS does not contain this rejected operation. " &
                    "Discard ONLY this rejected attempt (no pieces in CCS will be deleted)?" &
                    Environment.NewLine &
                    "If the piece still needs packing, scan and verify it again.",
                    "SQL rejection requires acknowledgement",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                    ConnectionRecovery.DismissRejectedAfterCheck()
                    AfterRecovery()
                    MessageBox.Show(
                        "Rejected attempt cleared after CCS verification. " &
                        "No SQL packing records were changed.",
                        "Scanner unlocked", MessageBoxButtons.OK,
                        MessageBoxIcon.Information)
                End If
            Else
                MessageBox.Show(
                    description & Environment.NewLine &
                    "The operation is unresolved. The scanner stays locked.",
                    "Waiting for SQL", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            MessageBox.Show(
                "Unable to reconcile the SQL operation. The scanner remains locked." &
                Environment.NewLine & ex.Message,
                "SQL reconciliation failed", MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub OpenOrLoadContainer(sender As Object, e As EventArgs)
        Try
            Dim part = TryCast(partPicker.SelectedItem, PaSPart)
            If part Is Nothing Then Throw New InvalidOperationException("Select a model.")
            Dim code = containerInput.Text.Trim().ToUpperInvariant()
            Dim lockedCode = StationSettings.ReadLockedPallet()
            If lockedCode <> "" AndAlso Not String.Equals(code, lockedCode,
                                                          StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException(
                    "Finish pallet " & lockedCode & " before opening a different pallet.")
            End If
            If activeContainer IsNot Nothing AndAlso activeContainer.Status = "OPEN" AndAlso
               Not String.Equals(activeContainer.Code, code, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException(
                    "Complete or partially close the current pallet before switching.")
            End If
            If Not PalletIdSettings.Load().IsValid(code) Then
                Throw New InvalidOperationException("Container format does not match General Settings.")
            End If

            Dim existing = PaSPackingData.FindContainer(code)
            If existing Is Nothing Then
                If selectedConfiguration Is Nothing OrElse Not selectedConfiguration.IsActive Then
                    Throw New InvalidOperationException("An active pallet configuration is required.")
                End If
                ' Container scan + Enter is the operator's explicit open action.
                ' No additional message box interrupts barcode scanning.
                Dim createdId = PaSPackingData.OpenContainer(code, part.Id)
                activeContainer = PaSPackingData.ReadContainer(createdId)
            Else
                If existing.PartId <> part.Id Then
                    Throw New InvalidOperationException("Container belongs to another model.")
                End If
                activeContainer = existing
            End If
            If activeContainer.Status = "OPEN" Then
                StationSettings.LockPallet(code)
            End If
            ResetVerification()
            RefreshContainer()
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Ready to scan SSI serial."
            firstQr.Focus()
        Catch ex As Exception
            ShowPackingError(ex)
        End Try
    End Sub

    Public Sub RefreshAfterHistory()
        ' A correction may have changed the active pallet. Preserve selection,
        ' reload authoritative SQL state, and invalidate any pre-verified QR.
        ResetVerification()
        If activeContainer IsNot Nothing Then
            RefreshContainer()
            If activeContainer.Status <> "OPEN" Then
                ResetAfterPalletClosed(activeContainer.Code)
            End If
        End If
    End Sub

    Private Sub RefreshContainer()
        If activeContainer Is Nothing Then Return
        activeContainer = PaSPackingData.ReadContainer(activeContainer.Id)
        If activeContainer Is Nothing Then
            Throw New InvalidOperationException("Container was not found.")
        End If
        Dim current = activeContainer.CurrentLevel
        Dim actual = activeContainer.PieceTotal
        Dim required = activeContainer.RequiredTotal
        progressText.Text = actual & " / " & required & " pieces" & Environment.NewLine &
            activeContainer.ClosedLevels & " / " & activeContainer.Levels.Count &
            " levels closed" & Environment.NewLine &
            If(current Is Nothing, "No open level",
               "Level " & current.LevelNumber & ": " & current.ScannedPieces &
               " / " & current.RequiredPieces)
        Dim percentage = If(required > 0,
            Math.Min(100, CInt(Math.Truncate(CDbl(actual) * 100.0 / required))), 0)
        progressBar.Value = percentage
        progressPercent.Text = percentage.ToString() & "%"
        progressState.Text = If(activeContainer.Status <> "OPEN",
                                activeContainer.Status,
                                If(current Is Nothing, "READY FOR PALLET CLOSURE",
                                   If(current.ScannedPieces = current.RequiredPieces,
                                      "LEVEL FULL — CONFIRM TO CONTINUE",
                                      "PACKING IN PROGRESS")))
        progressState.ForeColor = If(current IsNot Nothing AndAlso
                                     current.ScannedPieces = current.RequiredPieces,
                                     Color.FromArgb(28, 113, 83), PaSTheme.Navy)
        levelTiles.SuspendLayout()
        Try
            levelTiles.Controls.Clear()
            For Each level In activeContainer.Levels.OrderBy(Function(x) x.LevelNumber)
                Dim isClosed = level.Status = "CLOSED"
                Dim isFull = level.ScannedPieces = level.RequiredPieces
                Dim tileColor As System.Drawing.Color = If(isClosed, System.Drawing.Color.FromArgb(28, 113, 83),
                               If(isFull, System.Drawing.Color.FromArgb(196, 128, 40), PaSTheme.Navy))
                Dim tile As New Label With {
                    .Text = "L" & level.LevelNumber & "  " &
                            level.ScannedPieces & "/" & level.RequiredPieces &
                            If(isClosed, "  ✓", ""),
                    .TextAlign = ContentAlignment.MiddleCenter,
                    .Size = New Size(118, 38),
                    .Margin = New Padding(0, 0, 9, 4),
                    .BackColor = tileColor,
                    .ForeColor = Color.White,
                    .Font = New Font("Segoe UI", 10, FontStyle.Bold)
                }
                levelTiles.Controls.Add(tile)
            Next
        Finally
            levelTiles.ResumeLayout()
        End Try
        progressIcon.Text = If(activeContainer.Status = "OPEN", "▣", "✓")
        buildState.Text = "Container: " & activeContainer.Code & "  |  " & activeContainer.Status
        If activeContainer.Status <> "OPEN" Then
            StationSettings.UnlockPallet(activeContainer.Code)
        End If
        scannedList.Items.Clear()
        If activeContainer.Pieces.Count = 0 Then
            scannedList.Items.Add("No pieces recorded.")
        Else
            For Each piece In activeContainer.Pieces
                scannedList.Items.Add("L" & piece.LevelNumber & " | " &
                    piece.SsiCode & " | " & piece.ClientCode)
            Next
        End If
        UpdatePackingButtons()
    End Sub

    Private Sub ResetAfterPalletClosed(closedCode As String)
        ' The source has already reached a terminal SQL status; unlock the
        ' station and clear the visible Container field for the next pallet.
        StationSettings.UnlockPallet(closedCode)
        activeContainer = Nothing
        ResetVerification()
        firstQr.Clear()
        secondQr.Clear()
        containerInput.Clear()
        scannedList.Items.Clear()
        scannedList.Items.Add("No active pallet. Enter or scan the next container.")
        progressBar.Value = 0
        progressPercent.Text = "0%"
        progressState.Text = "READY FOR NEXT PALLET"
        progressIcon.Text = "▣"
        levelTiles.Controls.Clear()
        If selectedConfiguration IsNot Nothing Then
            progressText.Text = "0 / " & selectedConfiguration.TotalPieces &
                " pieces" & Environment.NewLine & "0 levels complete"
        Else
            progressText.Text = "0 / 0 pieces" & Environment.NewLine & "0 levels complete"
        End If
        buildState.Text = "Ready for next pallet"
        SetQWallStatus("Check on scan")
        UpdateScannerAvailability()
        containerInput.Focus()
    End Sub

    Public Sub LoadTransferredPallet(destinationCode As String)
        If ConnectionRecovery.Pending() IsNot Nothing Then
            Throw New InvalidOperationException("Resolve the pending SQL operation before loading a transfer.")
        End If
        Dim locked = StationSettings.ReadLockedPallet()
        If locked <> "" AndAlso Not String.Equals(locked, destinationCode,
                                                   StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException(
                "Finish pallet " & locked & " before loading the transferred pallet.")
        End If
        Dim pallet = PaSPackingData.FindContainer(destinationCode)
        If pallet Is Nothing OrElse pallet.Status <> "OPEN" Then
            Throw New InvalidOperationException("Transferred destination is not an OPEN pallet.")
        End If
        Dim part = parts.FirstOrDefault(Function(x) x.Id = pallet.PartId)
        If part Is Nothing Then
            Throw New InvalidOperationException(
                "Destination pallet model is not available for this station's customer.")
        End If
        partPicker.SelectedItem = part
        containerInput.Text = pallet.Code
        activeContainer = pallet
        StationSettings.LockPallet(pallet.Code)
        ResetVerification()
        RefreshContainer()
        scanStatus.ForeColor = Color.DarkGreen
        scanStatus.Text = "Pallet transfer loaded. Continue scanning remaining pieces."
    End Sub

    Public Sub AfterRecovery(Optional recoveryResult As String = "")
        ResetVerification()
        Try
            If activeContainer IsNot Nothing Then
                RefreshContainer()
                If activeContainer.Status <> "OPEN" Then
                    ResetAfterPalletClosed(activeContainer.Code)
                End If
            End If
        Catch ex As Exception
            scanStatus.Text = "Reconnected, but pallet refresh failed: " & ex.Message
            Return
        End Try
        UpdateScannerAvailability()
        If recoveryResult.StartsWith("NOT CANCELLED", StringComparison.OrdinalIgnoreCase) Then
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "Empty pallet was not cancelled. Request authorization again."
        ElseIf recoveryResult.StartsWith("NOT CLOSED", StringComparison.OrdinalIgnoreCase) Then
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "Partial pallet was not closed. Request authorization again."
        Else
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Connection restored. Pending SQL action reconciled."
        End If
    End Sub

    Private Sub RecordPiece(sender As Object, e As EventArgs)
        If Not recordButton.Enabled OrElse activeContainer Is Nothing Then Return
        Try
            Dim current = activeContainer.CurrentLevel
            If current Is Nothing Then Throw New InvalidOperationException("No open level.")
            ' Durable write-ahead intent; SQL remains the only authority for PASS.
            ConnectionRecovery.SaveBeforeSend("ADD_PIECE", activeContainer.Id,
                current.LevelNumber, verifiedSsi, verifiedClient, SessionContext.UserId)
            PaSPackingData.AddPiece(activeContainer.Id, current.LevelNumber, verifiedSsi, verifiedClient)
            ConnectionRecovery.MarkDelivered()
            ResetVerification()
            firstQr.Clear()
            secondQr.Clear()
            RefreshContainer()
            SetQWallStatus("Check on scan")
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Piece recorded in level " & current.LevelNumber & "."
            firstQr.Focus()
            If levelButton.Enabled Then PromptCompletedLevel()
        Catch ex As Exception
            ResetVerification()
            ShowPackingError(ex)
        End Try
    End Sub

    Private Sub PromptCompletedLevel()
        If autoConfirmVisible OrElse Not levelButton.Enabled OrElse
           activeContainer Is Nothing Then Return
        autoConfirmVisible = True
        Try
            ' Refresh SQL counts before showing physical packing confirmation.
            RefreshContainer()
            If Not levelButton.Enabled Then Return
            Dim current = activeContainer.CurrentLevel
            If current Is Nothing Then Return
            Dim isLast = current.LevelNumber = activeContainer.Levels.Count
            Using dialog As New PackingConfirmationForm(
                activeContainer.Code, current.LevelNumber, activeContainer.Levels.Count,
                current.RequiredPieces, isLast)
                If dialog.ShowDialog(FindForm()) <> DialogResult.OK Then
                    scanStatus.ForeColor = Color.FromArgb(149, 94, 13)
                    scanStatus.Text = "Level is full. Confirm physical packing to continue."
                    Return
                End If
            End Using
            CloseLevel(Me, EventArgs.Empty)
            If ConnectionRecovery.Pending() IsNot Nothing Then Return
            If isLast AndAlso closePallet.Enabled Then
                CompletePallet(Me, EventArgs.Empty)
            ElseIf activeContainer IsNot Nothing AndAlso
                   activeContainer.Status = "OPEN" Then
                scanStatus.ForeColor = Color.DarkGreen
                scanStatus.Text = "Level confirmed. Scan the next level."
                firstQr.Focus()
            End If
        Catch ex As Exception
            ShowPackingError(ex)
        Finally
            autoConfirmVisible = False
        End Try
    End Sub

    Private Sub CloseLevel(sender As Object, e As EventArgs)
        If Not levelButton.Enabled OrElse activeContainer Is Nothing Then Return
        Try
            Dim current = activeContainer.CurrentLevel
            If current Is Nothing Then Return
            ConnectionRecovery.SaveBeforeSend("CLOSE_LEVEL", activeContainer.Id,
                current.LevelNumber, "", "", SessionContext.UserId)
            PaSPackingData.CloseLevel(activeContainer.Id, current.LevelNumber)
            ConnectionRecovery.MarkDelivered()
            ResetVerification()
            RefreshContainer()
            scanStatus.Text = "Level " & current.LevelNumber & " closed."
        Catch ex As Exception
            ShowPackingError(ex)
        End Try
    End Sub

    Private Sub CompletePallet(sender As Object, e As EventArgs)
        If Not closePallet.Enabled OrElse activeContainer Is Nothing Then Return
        Try
            ConnectionRecovery.SaveBeforeSend("CLOSE_PALLET", activeContainer.Id,
                0, "", "", SessionContext.UserId)
            PaSPackingData.CloseContainer(activeContainer.Id)
            ConnectionRecovery.MarkDelivered()
            ResetVerification()
            RefreshContainer()
            Dim closedCode = activeContainer.Code
            ResetAfterPalletClosed(closedCode)
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Pallet " & closedCode & " completed. Ready for next container."
        Catch ex As Exception
            ShowPackingError(ex)
        End Try
    End Sub

    Private Sub ClosePartialPallet(sender As Object, e As EventArgs)
        If Not partialButton.Enabled OrElse activeContainer Is Nothing Then Return
        Try
            ' Refresh from CCS immediately before requesting approval.
            RefreshContainer()
            If activeContainer.Status <> "OPEN" OrElse activeContainer.PieceTotal <= 0 OrElse
               activeContainer.PieceTotal >= activeContainer.RequiredTotal Then
                Throw New InvalidOperationException("Partial close requires a nonempty, incomplete OPEN pallet.")
            End If
            Using authorization As New PartialPalletAuthorizationForm(
                activeContainer.Code, activeContainer.PieceTotal, activeContainer.RequiredTotal)
                If authorization.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                If MessageBox.Show(
                    "Close pallet " & activeContainer.Code & " as PARTIAL with " &
                    activeContainer.PieceTotal & " of " & activeContainer.RequiredTotal &
                    " pieces? This cannot be reopened.", "Confirm partial pallet",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return

                ' Only the operation identity is persisted; credentials are
                ' never stored locally or replayed after a network outage.
                ConnectionRecovery.SaveBeforeSend(
                    "CLOSE_PARTIAL", activeContainer.Id, 0, "", "", SessionContext.UserId)
                Try
                    PaSPackingData.ClosePartialContainer(
                        activeContainer.Id, authorization.AuthorizedUserId,
                        authorization.AuthorizedBarcode, authorization.AuthorizedPassword,
                        authorization.Reason)
                    ConnectionRecovery.MarkDelivered()
                Catch ex As Microsoft.Data.SqlClient.SqlException
                    If ex.Number >= 50000 OrElse ex.Number = 2601 OrElse ex.Number = 2627 Then
                        ' SQL rejected it transactionally: credentials/reason/etc.
                        ' Retrying requires a fresh explicit approval.
                        ConnectionRecovery.MarkDelivered()
                    End If
                    Throw
                End Try
            End Using
            ResetVerification()
            RefreshContainer()
            Dim closedCode = activeContainer.Code
            ResetAfterPalletClosed(closedCode)
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Partial pallet " & closedCode & " closed. Ready for next container."
        Catch ex As Exception
            ShowPackingError(ex)
        End Try
    End Sub

    Private Sub CancelEmptyPallet(sender As Object, e As EventArgs)
        If Not cancelEmptyButton.Enabled OrElse activeContainer Is Nothing Then Return
        Try
            ' Always refresh the authoritative CCS counts before authorization.
            RefreshContainer()
            If activeContainer.Status <> "OPEN" OrElse
               activeContainer.PieceTotal <> 0 OrElse
               activeContainer.ClosedLevels <> 0 Then
                Throw New InvalidOperationException(
                    "Only an OPEN pallet without pieces or closed levels may be cancelled.")
            End If
            Using authorization As New PartialPalletAuthorizationForm(
                activeContainer.Code, 0, activeContainer.RequiredTotal, True)
                If authorization.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                If MessageBox.Show(
                    "Cancel empty pallet " & activeContainer.Code & "?" &
                    Environment.NewLine &
                    "The container code will remain reserved and cannot be reused.",
                    "Confirm empty pallet cancellation",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return

                ' Retain the operation identity across network loss, but never
                ' persist a second user's credentials on the workstation.
                ConnectionRecovery.SaveBeforeSend(
                    "CANCEL_EMPTY", activeContainer.Id, 0, "", "", SessionContext.UserId)
                Try
                    PaSPackingData.CancelEmptyPallet(
                        activeContainer.Id, authorization.AuthorizedUserId,
                        authorization.AuthorizedBarcode, authorization.AuthorizedPassword,
                        authorization.Reason)
                    ConnectionRecovery.MarkDelivered()
                Catch ex As Microsoft.Data.SqlClient.SqlException
                    If ex.Number >= 50000 OrElse ex.Number = 2601 OrElse ex.Number = 2627 Then
                        ' Explicit business rejection: SQL rolled back, so a fresh
                        ' authorization can be used. A timeout remains pending.
                        ConnectionRecovery.MarkDelivered()
                    End If
                    Throw
                End Try
            End Using
            ResetVerification()
            RefreshContainer()
            Dim cancelledCode = activeContainer.Code
            ResetAfterPalletClosed(cancelledCode)
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Empty pallet " & cancelledCode &
                              " cancelled. Ready for the next container."
        Catch ex As Exception
            ShowPackingError(ex)
        End Try
    End Sub

    Private Sub ShowPackingError(ex As Exception)
        ' A SQL business rejection cannot be blindly retried. Keep its intent
        ' for human review; a connection timeout remains automatically recoverable.
        Dim sqlError = TryCast(ex, Microsoft.Data.SqlClient.SqlException)
        If sqlError IsNot Nothing AndAlso (sqlError.Number >= 50000 OrElse
                                           sqlError.Number = 2601 OrElse sqlError.Number = 2627) Then
            ConnectionRecovery.FlagReview(ex.Message)
        End If
        If ConnectionRecovery.Pending() IsNot Nothing Then
            ResetVerification()
            recordButton.Enabled = False
            levelButton.Enabled = False
            closePallet.Enabled = False
            cancelEmptyButton.Enabled = False
            openButton.Enabled = False
            scanStatus.Text = "SQL action pending verification. Wait for connection recovery."
        End If
        scanStatus.ForeColor = Color.DarkRed
        scanStatus.Text = ex.Message
        MessageBox.Show(ex.Message, "PackAndShip", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Private Sub VerifyQrFormat()
        ResetVerification()
        SetQWallStatus("NOT VERIFIED")
        If Not verifyButton.Enabled Then
            UpdateScannerAvailability()
            Return
        End If
        Try
            ' Re-read global format immediately before every verification.
            Dim currentFormat = PalletIdSettings.Load()
            If Not currentFormat.IsValid(containerInput.Text) Then
                scanStatus.Text = "The container format changed in General Settings."
                scanStatus.ForeColor = Color.DarkRed
                Return
            End If
            Dim selectedPart = TryCast(partPicker.SelectedItem, PaSPart)
            Dim result = ScanFormatValidator.CheckScans(
                firstQr.Text, secondQr.Text, selectedRule, selectedFields, selectedPart)
            If selectedRule IsNot Nothing AndAlso selectedRule.ScanCount = 1 Then
                secondQr.Text = If(result.Valid, result.GeneratedClientCode, "")
            End If
            If Not result.Valid Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = result.Message
                Return
            End If

            ' Check the exact full SSI and Client strings saved by Q-Wall,
            ' not only the extracted numeric portion. This is read-only.
            Dim fullClientCode As String = If(selectedRule.ScanCount = 1,
                                              result.GeneratedClientCode, secondQr.Text.Trim())
            Dim qwall = QWallPassValidator.Verify(selectedPart.Id, firstQr.Text.Trim(), fullClientCode)
            If Not qwall.Verified Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "QR format valid; " & qwall.Message
                SetQWallStatus("NOT VERIFIED")
                Return
            End If

            SetQWallStatus("VERIFIED")
            verifiedSsi = firstQr.Text.Trim()
            verifiedClient = fullClientCode
            UpdatePackingButtons()
            If activeContainer Is Nothing Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "Q-Wall PASS confirmed. Open / Load the container before recording."
            ElseIf activeContainer.Status <> "OPEN" Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "Q-Wall PASS confirmed. This container is " & activeContainer.Status & "."
            ElseIf activeContainer.CurrentLevel Is Nothing Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "Q-Wall PASS confirmed. No open level is available."
            ElseIf activeContainer.CurrentLevel.ScannedPieces >= activeContainer.CurrentLevel.RequiredPieces Then
                scanStatus.ForeColor = Color.FromArgb(145, 89, 10)
                scanStatus.Text = "Q-Wall PASS confirmed. Close the completed level before continuing."
            ElseIf recordButton.Enabled Then
                scanStatus.ForeColor = Color.DarkGreen
                scanStatus.Text = "Q-Wall PASS verified — recording piece in CCS."
            Else
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "Q-Wall PASS confirmed, but recording is unavailable. Reload the container."
            End If
        Catch ex As Exception
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "Verification unavailable: " & ex.Message
        End Try
    End Sub
End Class
