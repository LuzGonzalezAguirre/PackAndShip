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

    Private Sub BuildLayout()
        Dim page As New Panel With {
            .Dock = DockStyle.Fill, .AutoScroll = True,
            .Padding = New Padding(22), .BackColor = PaSTheme.Background
        }
        Controls.Add(page)

        Dim top As New Panel With {
            .Dock = DockStyle.Top, .Height = 232,
            .BackColor = PaSTheme.Surface,
            .Padding = New Padding(18)
        }
        page.Controls.Add(top)
        Dim title = PaSTheme.Heading("Pallet Scanner", 21)
        title.Location = New Point(20, 11)
        top.Controls.Add(title)

        top.Controls.Add(FieldCaption("Operator", 22, 62))
        operatorValue.SetBounds(22, 85, 490, 40)
        operatorValue.Font = New Font("Segoe UI", 19, FontStyle.Bold)
        operatorValue.ForeColor = navy
        top.Controls.Add(operatorValue)

        top.Controls.Add(FieldCaption("Date and time", 570, 60))
        clockValue.SetBounds(570, 86, 300, 31)
        clockValue.Font = New Font("Segoe UI", 13, FontStyle.Bold)
        top.Controls.Add(clockValue)

        top.Controls.Add(FieldCaption("Customer", 570, 124))
        customerValue.SetBounds(570, 149, 345, 32)
        customerValue.Font = New Font("Segoe UI", 15, FontStyle.Bold)
        customerValue.ForeColor = navy
        top.Controls.Add(customerValue)

        top.Controls.Add(FieldCaption("Pallet / container ID", 22, 143))
        containerInput.SetBounds(22, 170, 225, 33)
        containerInput.Font = New Font("Segoe UI", 13, FontStyle.Bold)
        containerInput.CharacterCasing = CharacterCasing.Upper
        containerInput.MaxLength = 18
        top.Controls.Add(containerInput)

        top.Controls.Add(FieldCaption("Part number (SSI / Client)", 264, 143))
        partPicker.SetBounds(264, 170, 285, 33)
        partPicker.DropDownStyle = ComboBoxStyle.DropDownList
        partPicker.DisplayMember = "DisplayName"
        partPicker.ValueMember = "Id"
        top.Controls.Add(partPicker)
        AddHandler partPicker.SelectedIndexChanged, AddressOf PartChanged
        AddHandler containerInput.TextChanged, Sub() UpdateScannerAvailability()

        Dim section As New Panel With {.Dock = DockStyle.Top, .Height = 345,
                                       .BackColor = PaSTheme.Background}
        page.Controls.Add(section)
        section.BringToFront()
        Dim scanCard As New Panel With {.BackColor = Color.White, .Location = New Point(0, 12),
                                       .Size = New Size(600, 325)}
        section.Controls.Add(scanCard)

        Dim scanTitle = PaSTheme.Heading("Scan both label QR codes", 15)
        scanTitle.Location = New Point(19, 17)
        scanCard.Controls.Add(scanTitle)
        Dim subtitle = PaSTheme.Subtitle("The labels are checked against the selected model's scan rule.")
        subtitle.Location = New Point(21, 51)
        scanCard.Controls.Add(subtitle)

        scanCard.Controls.Add(FieldCaption("Scan 1 — SSI / internal serial", 20, 88))
        firstQr.SetBounds(20, 113, 540, 38)
        firstQr.Font = New Font("Consolas", 13)
        scanCard.Controls.Add(firstQr)

        scanCard.Controls.Add(FieldCaption("Scan 2 — Client serial", 20, 168))
        secondQr.SetBounds(20, 193, 540, 38)
        secondQr.Font = New Font("Consolas", 13)
        scanCard.Controls.Add(secondQr)
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

        verifyButton.SetBounds(20, 256, 180, 42)
        AddHandler verifyButton.Click, Sub() VerifyQrFormat()
        scanCard.Controls.Add(verifyButton)
        scanStatus.SetBounds(215, 254, 355, 55)
        scanStatus.Font = New Font("Segoe UI", 9)
        scanStatus.ForeColor = Color.DarkRed
        scanCard.Controls.Add(scanStatus)

        Dim side As New Panel With {.BackColor = Color.White, .Location = New Point(617, 12),
                                   .Size = New Size(350, 325)}
        section.Controls.Add(side)
        Dim summaryHeading = PaSTheme.Heading("Pallet configuration", 14)
        summaryHeading.Location = New Point(18, 15)
        side.Controls.Add(summaryHeading)
        palletSummary.SetBounds(18, 52, 316, 112)
        palletSummary.Font = New Font("Segoe UI", 10)
        side.Controls.Add(palletSummary)

        Dim ruleHeading = PaSTheme.Heading("Validation", 13)
        ruleHeading.Location = New Point(18, 173)
        side.Controls.Add(ruleHeading)
        ruleSummary.SetBounds(18, 208, 316, 95)
        ruleSummary.Font = New Font("Segoe UI", 9)
        side.Controls.Add(ruleSummary)

        Dim footer As New Panel With {.Dock = DockStyle.Bottom, .Height = 90,
                                      .BackColor = PaSTheme.Surface}
        Dim notice As New Label With {
            .Text = "PACKING LOCKED — Q-Wall PASS and cross-container duplicate checks must be integrated before recording pieces or closing levels.",
            .Dock = DockStyle.Fill, .Padding = New Padding(20, 18, 20, 12),
            .ForeColor = Color.DarkRed,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold)
        }
        footer.Controls.Add(notice)
        page.Controls.Add(footer)

        AddHandler page.Resize, Sub()
                                    Dim available = Math.Max(900, page.ClientSize.Width - 44)
                                    top.Width = available
                                    section.Width = available
                                    scanCard.Width = Math.Max(500, CInt(available * 0.61))
                                    side.Left = scanCard.Right + 16
                                    side.Width = Math.Max(280, available - side.Left)
                                    firstQr.Width = scanCard.Width - 40
                                    secondQr.Width = scanCard.Width - 40
                                    scanStatus.Width = Math.Max(160, scanCard.Width - 225)
                                End Sub
        scanStatus.Text = "Select a container and an active part rule."
        palletSummary.Text = "Select a part number to load its pallet levels."
        ruleSummary.Text = "Format check only. No production records are written."
        verifyButton.Enabled = False
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
            palletSummary.Text = "Select a part number to load its pallet levels."
            ruleSummary.Text = "Expected container: " & palletFormat.Example()
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
                palletSummary.Text = "No active pallet configuration for " & part.Number
            Else
                palletSummary.Text = "Model: " & part.Number & Environment.NewLine &
                    "Total pieces: " & selectedConfiguration.TotalPieces & Environment.NewLine &
                    "Levels: " & selectedConfiguration.Levels.Count & Environment.NewLine &
                    "Per level: " & String.Join(" / ", selectedConfiguration.Levels)
            End If
            ruleSummary.Text = If(selectedRule Is Nothing,
                "No active scan rule.", "Configured scans: " & selectedRule.ScanCount &
                Environment.NewLine & "Match required: " & If(selectedRule.RequiresMatch, "Yes", "No"))
        Catch ex As Exception
            palletSummary.Text = "Unable to read configuration."
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
