Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

' First scanner UI iteration. No accepted-piece writes or closure operations
' until Q-Wall PASS schema and container uniqueness are verified.
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
    Private ReadOnly scanStatus As New Label()
    Private ReadOnly palletSummary As New Label()
    Private ReadOnly ruleSummary As New Label()
    Private ReadOnly verifyButton As Button = PaSTheme.ActionButton("Verify QR format")
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

        AddHandler partPicker.SelectedIndexChanged, AddressOf PartChanged
        AddHandler containerInput.TextChanged, Sub() UpdateScannerAvailability()

        bodyLeft.Location = New Point(20, 136)
        bodyLeft.Size = New Size(630, 686)
        mainPage.Controls.Add(bodyLeft)
        bodyRight.Location = New Point(668, 136)
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
        Dim helpText = PaSTheme.Subtitle("Scan SSI first, then Client. Press Enter after each QR.")
        helpText.Location = New Point(19, 46)
        scanArea.Controls.Add(helpText)
        scanArea.Controls.Add(FieldCaption("SCAN 1 — SSI SERIAL", 19, 77))
        firstQr.SetBounds(19, 101, 590, 38)
        firstQr.Font = New Font("Consolas", 12)
        scanArea.Controls.Add(firstQr)
        scanArea.Controls.Add(FieldCaption("SCAN 2 — CLIENT SERIAL", 19, 149))
        secondQr.SetBounds(19, 173, 590, 38)
        secondQr.Font = New Font("Consolas", 12)
        scanArea.Controls.Add(secondQr)

        AddHandler firstQr.KeyDown, Sub(sender, e)
                                        If e.KeyCode = Keys.Enter Then
                                            e.SuppressKeyPress = True
                                            secondQr.Focus()
                                        End If
                                    End Sub
        AddHandler secondQr.KeyDown, Sub(sender, e)
                                         If e.KeyCode = Keys.Enter Then
                                             e.SuppressKeyPress = True
                                             VerifyQrFormat()
                                         End If
                                     End Sub
        verifyButton.SetBounds(19, 235, 178, 39)
        verifyButton.Text = "Verify QR format"
        AddHandler verifyButton.Click, Sub() VerifyQrFormat()
        scanArea.Controls.Add(verifyButton)
        scanStatus.SetBounds(207, 226, 397, 80)
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
        closePallet.SetBounds(18, 199, 334, 46)
        closePallet.Text = "Complete pallet"
        closePallet.Enabled = False
        progressArea.Controls.Add(closePallet)

        validationArea = Card("VALIDATION RULES", bodyRight, 0, 294, 372, 391)
        ruleSummary.SetBounds(18, 54, 334, 163)
        ruleSummary.Font = New Font("Segoe UI", 10)
        validationArea.Controls.Add(ruleSummary)
        Dim warning As New Label With {
            .Text = "PACKING LOCKED" & Environment.NewLine &
                    "Q-Wall PASS, global duplicate checks, and exact level completion are not connected yet.",
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
                           "Q-Wall PASS: Not connected" & Environment.NewLine &
                           "Duplicate prevention: Not connected" & Environment.NewLine &
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
        scanStatus.Width = Math.Max(180, leftWidth - 220)
        scannedList.Width = leftWidth - 38
        progressText.Width = rightWidth - 40
        progressBar.Width = rightWidth - 38
        closePallet.Width = rightWidth - 38
        ruleSummary.Width = rightWidth - 38
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
            ruleSummary.Text = "Container example: " & palletFormat.Example() &
                               Environment.NewLine & "Q-Wall PASS: Not connected" &
                               Environment.NewLine & "Duplicate prevention: Not connected"
            containerInput.Clear()
            firstQr.Clear()
            secondQr.Clear()
            UpdateScannerAvailability()
            containerInput.Focus()
        Catch ex As Exception
            verifyButton.Enabled = False
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "Cannot load scanner setup: " & ex.Message
        End Try
    End Sub

    Private Sub PartChanged(sender As Object, e As EventArgs)
        selectedConfiguration = Nothing
        selectedRule = Nothing
        selectedFields.Clear()
        Dim part = TryCast(partPicker.SelectedItem, PaSPart)
        If part Is Nothing Then
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
                Environment.NewLine & "Q-Wall PASS: Not connected" &
                Environment.NewLine & "Duplicate prevention: Not connected" &
                Environment.NewLine & "Container format: General Settings"
        Catch ex As Exception
            buildState.Text = "Unable to read configuration."
            ruleSummary.Text = ex.Message
        End Try
        firstQr.Clear()
        secondQr.Clear()
        UpdateScannerAvailability()
    End Sub

    Private Sub UpdateScannerAvailability()
        Dim part = TryCast(partPicker.SelectedItem, PaSPart)
        Dim validContainer = palletFormat IsNot Nothing AndAlso palletFormat.IsValid(containerInput.Text)
        verifyButton.Enabled = validContainer AndAlso part IsNot Nothing AndAlso
                               selectedConfiguration IsNot Nothing AndAlso selectedConfiguration.IsActive AndAlso
                               selectedRule IsNot Nothing AndAlso selectedRule.ScanCount = 2
        If Not validContainer Then
            scanStatus.Text = "Enter a container ID matching General Settings."
        ElseIf selectedRule Is Nothing OrElse selectedRule.ScanCount <> 2 Then
            scanStatus.Text = "Two-label rule is not available for this part."
        ElseIf selectedConfiguration Is Nothing OrElse Not selectedConfiguration.IsActive Then
            scanStatus.Text = "Pallet configuration is missing or inactive."
        Else
            scanStatus.Text = "Ready for both label scans (format validation only)."
            scanStatus.ForeColor = navy
        End If
    End Sub

    Private Sub VerifyQrFormat()
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
            Dim result = ScanFormatValidator.CheckTwoLabels(
                firstQr.Text, secondQr.Text, selectedRule, selectedFields)
            scanStatus.ForeColor = If(result.Valid, Color.FromArgb(145, 89, 10), Color.DarkRed)
            scanStatus.Text = result.Message
            ' Intentionally do NOT clear scans, increase counts, save a piece,
            ' or enable closure. PASS and global uniqueness checks are not wired.
        Catch ex As Exception
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "Verification unavailable: " & ex.Message
        End Try
    End Sub
End Class
