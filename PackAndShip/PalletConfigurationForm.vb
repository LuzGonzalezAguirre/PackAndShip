Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

Public Class PalletConfigurationForm
    Inherits Form

    Private ReadOnly navy As Color = Color.FromArgb(31, 78, 121)
    Private ReadOnly searchBox As New TextBox()
    Private ReadOnly buPicker As New ComboBox()
    Private ReadOnly partsList As New ListBox()
    Private ReadOnly levelsPanel As New FlowLayoutPanel()
    Private ReadOnly titleLabel As New Label()
    Private ReadOnly totalLabel As New Label()
    Private ReadOnly countLabel As New Label()
    Private ReadOnly statusLabel As New Label()
    Private ReadOnly activeCheck As New CheckBox()
    Private ReadOnly addLevelButton As New Button()
    Private ReadOnly saveButton As New Button()
    Private ReadOnly refreshButton As New Button()
    Private ReadOnly quantityInputs As New List(Of NumericUpDown)()
    Private ReadOnly addPanel As New Panel()
    Private parts As New List(Of PaSPart)()
    Private selectedPart As PaSPart
    Private isLoading As Boolean

    Public Sub New()
        Text = "Pack and Ship | Setup - Pallet Configuration"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(1150, 780)
        MinimumSize = New Size(900, 630)
        BackColor = Color.FromArgb(244, 247, 250)
        Font = New Font("Segoe UI", 10)
        BuildLayout()
        AddHandler Shown, Sub() LoadParts()
    End Sub

    Private Sub BuildLayout()
        Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 88, .BackColor = navy}
        header.Controls.Add(New Label With {
            .Text = "Pallet Configuration", .Font = New Font("Segoe UI", 23, FontStyle.Bold),
            .ForeColor = Color.White, .AutoSize = True, .Location = New Point(24, 12)
        })
        header.Controls.Add(New Label With {
            .Text = "One pallet setup per part number", .ForeColor = Color.White,
            .AutoSize = True, .Location = New Point(28, 58)
        })

        Dim browser As New Panel With {
            .Dock = DockStyle.Left, .Width = 330, .Padding = New Padding(14, 14, 10, 14),
            .BackColor = Color.White
        }
        Dim browserHeading As New Label With {
            .Text = "Part numbers", .Dock = DockStyle.Top,
            .Height = 35, .Font = New Font("Segoe UI", 15, FontStyle.Bold),
            .ForeColor = navy
        }
        Dim filterArea As New TableLayoutPanel With {
            .Dock = DockStyle.Top, .Height = 117, .ColumnCount = 1, .RowCount = 4
        }
        filterArea.RowStyles.Add(New RowStyle(SizeType.Absolute, 22))
        filterArea.RowStyles.Add(New RowStyle(SizeType.Absolute, 35))
        filterArea.RowStyles.Add(New RowStyle(SizeType.Absolute, 23))
        filterArea.RowStyles.Add(New RowStyle(SizeType.Absolute, 35))
        filterArea.Controls.Add(New Label With {.Text = "Search part number", .Dock = DockStyle.Fill}, 0, 0)
        searchBox.Dock = DockStyle.Fill
        filterArea.Controls.Add(searchBox, 0, 1)
        filterArea.Controls.Add(New Label With {.Text = "Customer (filter only)", .Dock = DockStyle.Fill}, 0, 2)
        buPicker.Dock = DockStyle.Fill
        buPicker.DropDownStyle = ComboBoxStyle.DropDownList
        filterArea.Controls.Add(buPicker, 0, 3)
        AddHandler searchBox.TextChanged, Sub() FilterParts()
        AddHandler buPicker.SelectedIndexChanged, Sub() FilterParts()
        partsList.Dock = DockStyle.Fill
        partsList.IntegralHeight = False
        partsList.DisplayMember = "Number"
        AddHandler partsList.SelectedIndexChanged, AddressOf PartSelected
        refreshButton.Text = "Refresh catalog"
        refreshButton.Dock = DockStyle.Bottom
        refreshButton.Height = 40
        StyleButton(refreshButton)
        AddHandler refreshButton.Click, Sub() LoadParts()
        browser.Controls.Add(partsList)
        browser.Controls.Add(filterArea)
        browser.Controls.Add(browserHeading)
        browser.Controls.Add(refreshButton)

        Dim editor As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(20, 16, 20, 14)}
        Dim editorTop As New Panel With {.Dock = DockStyle.Top, .Height = 132}
        titleLabel.Text = "Select a part number"
        titleLabel.Font = New Font("Segoe UI", 19, FontStyle.Bold)
        titleLabel.ForeColor = navy
        titleLabel.SetBounds(0, 0, 680, 44)
        titleLabel.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        statusLabel.Text = "Choose a part from the catalog to configure its pallet."
        statusLabel.SetBounds(0, 47, 680, 25)
        activeCheck.Text = "Configuration active"
        activeCheck.SetBounds(0, 90, 240, 30)
        activeCheck.Checked = True
        editorTop.Controls.AddRange(New Control() {titleLabel, statusLabel, activeCheck})

        Dim footer As New Panel With {.Dock = DockStyle.Bottom, .Height = 112}
        countLabel.Text = "0 levels"
        countLabel.SetBounds(0, 7, 220, 32)
        countLabel.Font = New Font("Segoe UI", 13, FontStyle.Bold)
        totalLabel.Text = "Total: 0 pieces"
        totalLabel.SetBounds(230, 7, 340, 34)
        totalLabel.Font = New Font("Segoe UI", 17, FontStyle.Bold)
        totalLabel.ForeColor = navy
        addLevelButton.Text = "+ Add level"
        addLevelButton.SetBounds(0, 54, 155, 41)
        StyleButton(addLevelButton)
        AddHandler addLevelButton.Click, Sub() AddLevel(1)
        saveButton.Text = "Save configuration"
        saveButton.SetBounds(185, 54, 225, 41)
        StyleButton(saveButton)
        AddHandler saveButton.Click, AddressOf SaveConfiguration
        footer.Controls.AddRange(New Control() {countLabel, totalLabel, addLevelButton, saveButton})

        levelsPanel.Dock = DockStyle.Fill
        levelsPanel.FlowDirection = FlowDirection.TopDown
        levelsPanel.WrapContents = False
        levelsPanel.AutoScroll = True
        levelsPanel.Padding = New Padding(0, 0, 12, 0)
        AddHandler levelsPanel.Resize, Sub() ResizeRows()
        editor.Controls.Add(levelsPanel)
        editor.Controls.Add(footer)
        editor.Controls.Add(editorTop)

        Controls.Add(editor)
        Controls.Add(browser)
        Controls.Add(header)
        EnableEditor(False)
    End Sub

    Private Sub StyleButton(button As Button)
        button.FlatStyle = FlatStyle.Flat
        button.BackColor = navy
        button.ForeColor = Color.White
        button.FlatAppearance.BorderSize = 0
    End Sub

    Private Sub LoadParts()
        Try
            Dim selectedId = If(selectedPart Is Nothing, 0, selectedPart.Id)
            isLoading = True
            parts = PalletConfigurationData.GetParts()
            Dim customers = parts.
                Where(Function(p) p.BusinessUnitId.HasValue).
                GroupBy(Function(p) p.BusinessUnitId.Value).
                Select(Function(g) New BusinessUnit With {.Id = g.Key, .Name = g.First().BusinessUnit}).
                OrderBy(Function(b) b.Name).ToList()
            customers.Insert(0, New BusinessUnit With {.Id = 0, .Name = "All customers"})
            buPicker.DataSource = customers
            buPicker.DisplayMember = "Name"
            buPicker.ValueMember = "Id"
            isLoading = False
            FilterParts(selectedId)
        Catch ex As Exception
            isLoading = False
            MessageBox.Show(ex.Message, "Cannot load part catalog",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FilterParts(Optional preferredPartId As Integer = 0)
        If isLoading Then Return
        Dim query = searchBox.Text.Trim()
        Dim selectedBu = TryCast(buPicker.SelectedItem, BusinessUnit)
        Dim buId = If(selectedBu Is Nothing, 0, selectedBu.Id)
        Dim options = parts.Where(Function(p) (buId = 0 OrElse p.BusinessUnitId = buId) AndAlso
            (query = "" OrElse p.Number.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)).
            ToList()
        isLoading = True
        partsList.DataSource = Nothing
        partsList.DataSource = options
        partsList.DisplayMember = "Number"
        partsList.ClearSelected()
        If preferredPartId > 0 Then
            Dim index = options.FindIndex(Function(p) p.Id = preferredPartId)
            If index >= 0 Then partsList.SelectedIndex = index
        End If
        isLoading = False
        If partsList.SelectedItem IsNot Nothing Then
            LoadSelectedPart()
        Else
            selectedPart = Nothing
            titleLabel.Text = "Select a part number"
            statusLabel.Text = options.Count & " matching parts"
            ClearLevels()
            EnableEditor(False)
        End If
    End Sub

    Private Sub PartSelected(sender As Object, e As EventArgs)
        If Not isLoading Then LoadSelectedPart()
    End Sub

    Private Sub LoadSelectedPart()
        selectedPart = TryCast(partsList.SelectedItem, PaSPart)
        If selectedPart Is Nothing Then
            EnableEditor(False)
            Return
        End If
        Try
            Dim config = PalletConfigurationData.GetConfiguration(selectedPart.Id)
            ClearLevels()
            titleLabel.Text = selectedPart.Number & "   |   " & selectedPart.BusinessUnit
            If config Is Nothing Then
                statusLabel.Text = "Not configured • New configuration"
                activeCheck.Checked = True
                AddLevel(1)
            Else
                statusLabel.Text = "Existing configuration"
                activeCheck.Checked = config.IsActive
                For Each quantity In config.Levels
                    AddLevel(quantity)
                Next
                If config.Levels.Count = 0 Then
                    statusLabel.Text = "Existing configuration has no levels; correct before saving."
                End If
                If config.Levels.Sum(Function(x) CLng(x)) <> CLng(config.TotalPieces) Then
                    statusLabel.Text = "Warning: stored total does not match the levels."
                End If
            End If
            EnableEditor(True)
        Catch ex As Exception
            EnableEditor(False)
            MessageBox.Show(ex.Message, "Cannot load pallet configuration",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub EnableEditor(enabled As Boolean)
        addLevelButton.Enabled = enabled
        saveButton.Enabled = enabled
        activeCheck.Enabled = enabled
        levelsPanel.Enabled = enabled
    End Sub

    Private Sub ClearLevels()
        For Each control As Control In levelsPanel.Controls
            control.Dispose()
        Next
        levelsPanel.Controls.Clear()
        quantityInputs.Clear()
        UpdateTotals()
    End Sub

    Private Sub AddLevel(amount As Integer)
        Dim row As New Panel With {
            .Height = 68, .Width = Math.Max(400, levelsPanel.ClientSize.Width - 28),
            .BackColor = Color.White, .Margin = New Padding(0, 0, 0, 8)
        }
        Dim levelTitle As New Label With {
            .Text = "Level " & (quantityInputs.Count + 1),
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .ForeColor = navy, .Location = New Point(15, 17), .Size = New Size(120, 35)
        }
        Dim countText As New Label With {
            .Text = "Required pieces:", .Location = New Point(145, 23),
            .AutoSize = True
        }
        Dim input As New NumericUpDown With {
            .Minimum = 1, .Maximum = Integer.MaxValue, .Value = amount,
            .TextAlign = HorizontalAlignment.Center,
            .Location = New Point(290, 17), .Width = 115
        }
        AddHandler input.ValueChanged, Sub() UpdateTotals()
        Dim remove As New Button With {
            .Text = "Remove", .Size = New Size(100, 34),
            .Location = New Point(430, 16), .Tag = input
        }
        AddHandler remove.Click, Sub()
                                     If quantityInputs.Count <= 1 Then
                                         MessageBox.Show("A pallet needs at least one level.")
                                         Return
                                     End If
                                     quantityInputs.Remove(input)
                                     levelsPanel.Controls.Remove(row)
                                     row.Dispose()
                                     RenumberLevels()
                                     UpdateTotals()
                                 End Sub
        row.Controls.AddRange(New Control() {levelTitle, countText, input, remove})
        quantityInputs.Add(input)
        levelsPanel.Controls.Add(row)
        ResizeRows()
        UpdateTotals()
    End Sub

    Private Sub RenumberLevels()
        For i = 0 To levelsPanel.Controls.Count - 1
            Dim row = DirectCast(levelsPanel.Controls(i), Panel)
            DirectCast(row.Controls(0), Label).Text = "Level " & (i + 1)
        Next
    End Sub

    Private Sub ResizeRows()
        For Each row As Control In levelsPanel.Controls
            row.Width = Math.Max(540, levelsPanel.ClientSize.Width - 28)
        Next
    End Sub

    Private Sub UpdateTotals()
        countLabel.Text = quantityInputs.Count & " levels"
        Dim pieces As Long = quantityInputs.Sum(Function(n) CLng(n.Value))
        totalLabel.Text = "Total: " & pieces & " pieces"
        saveButton.Enabled = selectedPart IsNot Nothing AndAlso
                             quantityInputs.Count > 0 AndAlso pieces <= Integer.MaxValue
    End Sub

    Private Sub SaveConfiguration(sender As Object, e As EventArgs)
        If selectedPart Is Nothing Then
            MessageBox.Show("Select a part number first.")
            Return
        End If
        Dim quantities = quantityInputs.Select(Function(input) CInt(input.Value)).ToList()
        Try
            PalletConfigurationData.SaveConfiguration(selectedPart.Id, quantities, activeCheck.Checked)
            MessageBox.Show("Pallet configuration saved successfully.",
                            "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Dim currentPartId = selectedPart.Id
            LoadParts()
            FilterParts(currentPartId)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Cannot save configuration",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
