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
    Private ReadOnly recordButton As Button = PaSTheme.ActionButton("Record piece")
    Private ReadOnly levelButton As Button = PaSTheme.ActionButton("Close level", False)
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
        mainPage.AutoScrollMinSize = New Size(1060, 860)
        mainPage.BackColor = PaSTheme.Background
        Controls.Add(mainPage)

        ' Compact Q-Wall-style horizontal data strip, with the new app palette.
        headerPage.Location = New Point(20, 16)
        headerPage.Size = New Size(1020, 100)
        headerPage.BackColor = PaSTheme.Border
        mainPage.Controls.Add(headerPage)

        Dim strip As New TableLayoutPanel With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 5, .RowCount = 1,
            .BackColor = PaSTheme.Border,
            .Margin = New Padding(0),
            .Padding = New Padding(1)
        }
        strip.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        For Each weight As Single In {18.0F, 26.0F, 24.0F, 13.0F, 19.0F}
            strip.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, weight))
        Next
        headerPage.Controls.Add(strip)

        containerInput.CharacterCasing = CharacterCasing.Upper
        containerInput.MaxLength = 18
        containerInput.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        containerInput.Height = 31
        partPicker.DropDownStyle = ComboBoxStyle.DropDownList
        partPicker.DisplayMember = "DisplayName"
        partPicker.ValueMember = "Id"
        partPicker.Font = New Font("Segoe UI", 10)
        partPicker.Height = 31

        operatorValue.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        operatorValue.ForeColor = navy
        operatorValue.AutoEllipsis = True
        operatorValue.Height = 48
        customerValue.Font = New Font("Segoe UI", 13, FontStyle.Bold)
        customerValue.ForeColor = navy
        customerValue.AutoEllipsis = True
        customerValue.Height = 43
        clockValue.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        clockValue.Height = 43
        clockValue.TextAlign = ContentAlignment.MiddleLeft

        strip.Controls.Add(HeaderCell("CONTAINER", containerInput, 18), 0, 0)
        strip.Controls.Add(HeaderCell("PART NUMBER / MODEL", partPicker, 26), 1, 0)
        strip.Controls.Add(HeaderCell("OPERATOR", operatorValue, 24), 2, 0)
        strip.Controls.Add(HeaderCell("CUSTOMER", customerValue, 13), 3, 0)
        strip.Controls.Add(HeaderCell("DATE / TIME", clockValue, 19), 4, 0)
        openButton.SetBounds(20, 119, 142, 32)
        openButton.Text = "Open / Load"
        AddHandler openButton.Click, AddressOf OpenOrLoadContainer
        mainPage.Controls.Add(openButton)

        AddHandler partPicker.SelectedIndexChanged, AddressOf PartChanged
        AddHandler containerInput.TextChanged, Sub()
                                                  If activeContainer IsNot Nothing AndAlso
                                                     Not String.Equals(activeContainer.Code, containerInput.Text.Trim(),
                                                                       StringComparison.OrdinalIgnoreCase) Then
                                                      activeContainer = Nothing
                                                  End If
                                                  ResetVerification()
                                                  SetQWallStatus("Check on scan")
                                                  UpdateScannerAvailability()
                                              End Sub
        AddHandler firstQr.TextChanged, Sub()
                                            ResetVerification()
                                            SetQWallStatus("Check on scan")
                                        End Sub
        AddHandler secondQr.TextChanged, Sub()
                                             ResetVerification()
                                             SetQWallStatus("Check on scan")
                                         End Sub

        bodyLeft.Location = New Point(20, 158)
        bodyLeft.Size = New Size(630, 686)
        mainPage.Controls.Add(bodyLeft)
        bodyRight.Location = New Point(668, 158)
        bodyRight.Size = New Size(372, 686)
        mainPage.Controls.Add(bodyRight)

        ' Active build summary, separate QR scanner, then recorded pieces.
        buildArea = Card("ACTIVE BUILD", bodyLeft, 0, 0, 630, 145)
        activePartTitle.SetBounds(18, 48, 470, 36)
        activePartTitle.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        activePartTitle.ForeColor = PaSTheme.TextPrimary
        activePartTitle.AutoEllipsis = True
        buildArea.Controls.Add(activePartTitle)
        buildState.SetBounds(18, 99, 580, 31)
        buildState.ForeColor = PaSTheme.TextSecondary
        buildArea.Controls.Add(buildState)

        scanArea = Card("SCAN LABELS", bodyLeft, 0, 161, 630, 323)
        scanHelp.Text = "Scan SSI first, then Client. Press Enter after each QR."
        scanHelp.Location = New Point(19, 46)
        scanHelp.AutoSize = True
        scanHelp.ForeColor = PaSTheme.TextSecondary
        scanArea.Controls.Add(scanHelp)
        scanArea.Controls.Add(FieldCaption("SCAN 1 — SSI SERIAL", 19, 77))
        firstQr.SetBounds(19, 101, 590, 38)
        firstQr.Font = New Font("Consolas", 12)
        scanArea.Controls.Add(firstQr)
        clientScanLabel.Text = "SCAN 2 — CLIENT SERIAL"
        clientScanLabel.Location = New Point(19, 149)
        clientScanLabel.Size = New Size(310, 22)
        clientScanLabel.ForeColor = PaSTheme.TextSecondary
        clientScanLabel.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        scanArea.Controls.Add(clientScanLabel)
        secondQr.SetBounds(19, 173, 590, 38)
        secondQr.Font = New Font("Consolas", 12)
        scanArea.Controls.Add(secondQr)
        AddHandler firstQr.TextChanged, Sub()
                                            If selectedRule IsNot Nothing AndAlso selectedRule.ScanCount = 1 Then
                                                secondQr.Clear()
                                            End If
                                        End Sub

        AddHandler firstQr.KeyDown, Sub(sender, e)
                                        If e.KeyCode = Keys.Enter Then
                                            e.SuppressKeyPress = True
                                            If selectedRule IsNot Nothing AndAlso selectedRule.ScanCount = 1 Then
                                                VerifyQrFormat()
                                            Else
                                                secondQr.Focus()
                                            End If
                                        End If
                                    End Sub
        AddHandler secondQr.KeyDown, Sub(sender, e)
                                         If e.KeyCode = Keys.Enter Then
                                             e.SuppressKeyPress = True
                                             VerifyQrFormat()
                                         End If
                                     End Sub
        verifyButton.SetBounds(19, 235, 152, 39)
        verifyButton.Text = "Verify QR format"
        AddHandler verifyButton.Click, Sub() VerifyQrFormat()
        scanArea.Controls.Add(verifyButton)
        recordButton.SetBounds(182, 235, 148, 39)
        recordButton.Text = "Record piece"
        recordButton.Enabled = False
        AddHandler recordButton.Click, AddressOf RecordPiece
        scanArea.Controls.Add(recordButton)
        scanStatus.SetBounds(342, 224, 265, 88)
        scanStatus.Font = New Font("Segoe UI", 9)
        scanStatus.ForeColor = Color.DarkRed
        scanArea.Controls.Add(scanStatus)

        piecesArea = Card("SCANNED PARTS", bodyLeft, 0, 500, 630, 185)
        scannedList.SetBounds(18, 47, 592, 118)
        scannedList.Font = New Font("Consolas", 10)
        scannedList.BorderStyle = BorderStyle.None
        scannedList.BackColor = Color.FromArgb(248, 250, 253)
        scannedList.Items.Add("No accepted pieces yet — Q-Wall verification pending.")
        scannedList.Enabled = False
        piecesArea.Controls.Add(scannedList)

        progressArea = Card("PALLET PROGRESS", bodyRight, 0, 0, 372, 278)
        progressText.SetBounds(18, 53, 326, 115)
        progressText.Font = New Font("Segoe UI", 11)
        progressText.ForeColor = PaSTheme.TextPrimary
        progressArea.Controls.Add(progressText)
        progressBar.SetBounds(18, 166, 334, 11)
        progressBar.Minimum = 0
        progressBar.Maximum = 100
        progressBar.Value = 0
        progressArea.Controls.Add(progressBar)
        levelButton.SetBounds(18, 189, 334, 37)
        levelButton.Text = "Close level"
        levelButton.Enabled = False
        AddHandler levelButton.Click, AddressOf CloseLevel
        progressArea.Controls.Add(levelButton)
        closePallet.SetBounds(18, 233, 334, 36)
        closePallet.Text = "Complete pallet"
        closePallet.Enabled = False
        AddHandler closePallet.Click, AddressOf CompletePallet
        progressArea.Controls.Add(closePallet)

        validationArea = Card("VALIDATION RULES", bodyRight, 0, 294, 372, 391)
        ruleSummary.SetBounds(18, 54, 334, 163)
        ruleSummary.Font = New Font("Segoe UI", 10)
        validationArea.Controls.Add(ruleSummary)
        Dim warning As New Label With {
            .Text = "SQL VALIDATION ACTIVE" & Environment.NewLine &
                    "Packing actions use transactional SQL validation for PASS, duplicates, level quantities and container closure.",
            .Location = New Point(18, 246),
            .Size = New Size(334, 124),
            .Font = New Font("Segoe UI", 9, FontStyle.Bold),
            .ForeColor = Color.DarkRed,
            .BackColor = Color.FromArgb(255, 246, 245),
            .Padding = New Padding(10)
        }
        validationArea.Controls.Add(warning)

        AddHandler mainPage.Resize, Sub() ResizeScannerLayout()
        ResizeScannerLayout()
        activePartTitle.Text = "Select a part number"
        buildState.Text = "Waiting for a container and model"
        progressText.Text = "0 / 0 pieces" & Environment.NewLine & "0 levels complete"
        ruleSummary.Text = "Scan rule: Pending" & Environment.NewLine &
                           "Q-Wall PASS: Check on scan" & Environment.NewLine &
                           "Duplicate prevention: SQL unique product ID" & Environment.NewLine &
                           "Container format: General Settings"
        scanStatus.Text = "Enter a container and select a model."
        verifyButton.Enabled = False
    End Sub

    Private Sub ResizeScannerLayout()
        Dim totalWidth = Math.Max(1020, mainPage.ClientSize.Width - 40)
        Dim rightWidth = Math.Max(330, CInt(totalWidth * 0.35))
        Dim leftWidth = totalWidth - rightWidth - 18
        headerPage.Width = totalWidth
        bodyLeft.Width = leftWidth
        bodyRight.Left = bodyLeft.Left + leftWidth + 18
        bodyRight.Width = rightWidth
        For i As Integer = 0 To headerCards.Count - 1
            Dim cell = headerCards(i)
            For Each ctl As Control In cell.Controls
                If TypeOf ctl Is TextBox OrElse TypeOf ctl Is ComboBox OrElse
                   (TypeOf ctl Is Label AndAlso ctl.Location.Y >= 30) Then
                    ctl.Width = Math.Max(65, cell.ClientSize.Width - 18)
                End If
            Next
        Next

        For Each panel In {buildArea, scanArea, piecesArea}
            panel.Width = leftWidth
        Next
        For Each panel In {progressArea, validationArea}
            panel.Width = rightWidth
        Next
        activePartTitle.Width = Math.Max(250, leftWidth - 140)
        buildState.Width = leftWidth - 35
        firstQr.Width = leftWidth - 39
        secondQr.Width = leftWidth - 39
        scanStatus.Width = Math.Max(160, leftWidth - 355)
        scannedList.Width = leftWidth - 38
        progressText.Width = rightWidth - 40
        progressBar.Width = rightWidth - 38
        closePallet.Width = rightWidth - 38
        levelButton.Width = rightWidth - 38
        ruleSummary.Width = rightWidth - 38
    End Sub

    Public Sub LoadStation()
        operatorValue.Text = If(SessionContext.FullName, "")
        customerValue.Text = If(SessionContext.BusinessUnitName, "")
        clockValue.Text = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss")
        Try
            palletFormat = PalletIdSettings.Load()
            OfflineStore.CacheFormat(palletFormat)
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
            ' Save the exact approved model/levels/scan rules while connected.
            ' Offline work must use this snapshot, not invented defaults.
            OfflineStore.CacheModel(part, selectedConfiguration, selectedRule, selectedFields)
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
        recordButton.Enabled = openNow AndAlso current IsNot Nothing AndAlso
            verifiedSsi <> "" AndAlso verifiedClient <> "" AndAlso
            firstQr.Text.Trim() = verifiedSsi AndAlso
            selectedRule IsNot Nothing AndAlso
            (selectedRule.ScanCount = 1 OrElse secondQr.Text.Trim() = verifiedClient)
        levelButton.Enabled = openNow AndAlso current IsNot Nothing AndAlso
            current.ScannedPieces = current.RequiredPieces
        closePallet.Enabled = openNow AndAlso current Is Nothing AndAlso
            activeContainer.Levels.Count > 0 AndAlso
            activeContainer.Levels.All(Function(l) l.Status = "CLOSED" AndAlso
                                      l.ScannedPieces = l.RequiredPieces)
    End Sub

    Private Sub OpenOrLoadContainer(sender As Object, e As EventArgs)
        Try
            Dim part = TryCast(partPicker.SelectedItem, PaSPart)
            If part Is Nothing Then Throw New InvalidOperationException("Select a model.")
            Dim code = containerInput.Text.Trim().ToUpperInvariant()
            If Not PalletIdSettings.Load().IsValid(code) Then
                Throw New InvalidOperationException("Container format does not match General Settings.")
            End If

            Dim existing = PaSPackingData.FindContainer(code)
            If existing Is Nothing Then
                If selectedConfiguration Is Nothing OrElse Not selectedConfiguration.IsActive Then
                    Throw New InvalidOperationException("An active pallet configuration is required.")
                End If
                If MessageBox.Show("Create container " & code & " for " & part.Number & "?",
                                   "Open container", MessageBoxButtons.YesNo,
                                   MessageBoxIcon.Question) <> DialogResult.Yes Then Return
                Dim createdId = PaSPackingData.OpenContainer(code, part.Id)
                activeContainer = PaSPackingData.ReadContainer(createdId)
            Else
                If existing.PartId <> part.Id Then
                    Throw New InvalidOperationException("Container belongs to another model.")
                End If
                activeContainer = existing
            End If
            ResetVerification()
            RefreshContainer()
            scanStatus.Text = "Container " & code & " loaded."
        Catch ex As Exception
            ShowPackingError(ex)
        End Try
    End Sub

    Public Sub RefreshAfterHistory()
        ' A correction may have changed the active pallet. Preserve selection,
        ' reload authoritative SQL state, and invalidate any pre-verified QR.
        ResetVerification()
        If activeContainer IsNot Nothing Then RefreshContainer()
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
        progressBar.Value = If(required > 0,
            Math.Min(100, CInt(Math.Truncate(CDbl(actual) * 100.0 / required))), 0)
        buildState.Text = "Container: " & activeContainer.Code & "  |  " & activeContainer.Status
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

    Private Sub RecordPiece(sender As Object, e As EventArgs)
        If Not recordButton.Enabled OrElse activeContainer Is Nothing Then Return
        Try
            Dim current = activeContainer.CurrentLevel
            If current Is Nothing Then Throw New InvalidOperationException("No open level.")
            ' SQL re-verifies PASS, global uniqueness and capacity atomically.
            PaSPackingData.AddPiece(activeContainer.Id, current.LevelNumber, verifiedSsi, verifiedClient)
            ResetVerification()
            firstQr.Clear()
            secondQr.Clear()
            RefreshContainer()
            SetQWallStatus("Check on scan")
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Piece recorded in level " & current.LevelNumber & "."
            firstQr.Focus()
        Catch ex As Exception
            ResetVerification()
            ShowPackingError(ex)
        End Try
    End Sub

    Private Sub CloseLevel(sender As Object, e As EventArgs)
        If Not levelButton.Enabled OrElse activeContainer Is Nothing Then Return
        Try
            Dim current = activeContainer.CurrentLevel
            If current Is Nothing Then Return
            If MessageBox.Show("Close level " & current.LevelNumber & " with " &
                               current.RequiredPieces & " pieces?", "Close level",
                               MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            PaSPackingData.CloseLevel(activeContainer.Id, current.LevelNumber)
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
            If MessageBox.Show("Complete container " & activeContainer.Code & "?",
                               "Complete pallet", MessageBoxButtons.YesNo,
                               MessageBoxIcon.Question) <> DialogResult.Yes Then Return
            PaSPackingData.CloseContainer(activeContainer.Id)
            ResetVerification()
            RefreshContainer()
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Pallet completed."
        Catch ex As Exception
            ShowPackingError(ex)
        End Try
    End Sub

    Private Sub ShowPackingError(ex As Exception)
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
                scanStatus.Text = "Q-Wall PASS confirmed. Click Record piece to pack it."
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
