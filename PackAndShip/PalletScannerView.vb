Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

' Visual controls and positions are defined in PalletScannerView.Designer.vb.
' This file contains only scan, database, validation and navigation logic.
Public Partial Class PalletScannerView
    Inherits UserControl

    Private ReadOnly navy As Color = PaSTheme.Navy
    Private activeContainer As PaSPackedContainer
    Private verifiedSsi As String = ""
    Private verifiedClient As String = ""
    Private ReadOnly clockTimer As New Timer With {.Interval = 1000}
    Private parts As New List(Of PaSPart)()
    Private selectedConfiguration As PalletConfiguration
    Private selectedRule As ScanRuleRecord
    Private selectedFields As New List(Of ScanFieldRecord)()
    Private palletFormat As PalletIdSettings
    Private autoConfirmVisible As Boolean
    Private autoScanBusy As Boolean
    Private autoOpenBusy As Boolean

    ' These hidden state guards preserve the existing SQL write workflow.
    ' They are not visual controls and do not need Designer positions.
    Private ReadOnly openButton As New Button()
    Private ReadOnly verifyButton As New Button()
    Private ReadOnly recordButton As New Button()
    Private ReadOnly levelButton As New Button()
    Private ReadOnly closePallet As New Button()

    Public Sub New()
        InitializeComponent()
        BindScannerEvents()
        AddHandler clockTimer.Tick, Sub() clockValue.Text = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss")
        AddHandler Disposed, Sub() clockTimer.Stop()
        clockTimer.Start()
    End Sub

    Private Sub BindScannerEvents()
        AddHandler openButton.Click, AddressOf OpenOrLoadContainer
        AddHandler verifyButton.Click, Sub() VerifyQrFormat()
        AddHandler recordButton.Click, AddressOf RecordPiece
        AddHandler levelButton.Click, AddressOf CloseLevel
        AddHandler closePallet.Click, AddressOf CompletePallet
        AddHandler partialButton.Click, AddressOf ClosePartialPallet
        AddHandler cancelEmptyButton.Click, AddressOf CancelEmptyPallet
        AddHandler pendingButton.Click, AddressOf ReviewPendingSql
        AddHandler confirmAgain.Click, Sub() PromptCompletedLevel()
        AddHandler partPicker.SelectedIndexChanged, AddressOf PartChanged
        AddHandler containerInput.KeyDown, AddressOf ContainerEnter
        AddHandler firstQr.KeyDown, AddressOf FirstLabelEnter
        AddHandler secondQr.KeyDown, AddressOf SecondLabelEnter
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
        AddHandler firstQr.TextChanged, Sub()
            ResetVerification()
            If selectedRule IsNot Nothing AndAlso selectedRule.ScanCount = 1 Then
                secondQr.Clear()
            End If
        End Sub
        AddHandler secondQr.TextChanged, Sub() ResetVerification()
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
