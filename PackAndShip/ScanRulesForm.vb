Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

Public Class ScanRulesForm
    Inherits Form

    Private ReadOnly navy As Color = Color.FromArgb(31, 78, 121)
    Private ReadOnly searchBox As New TextBox()
    Private ReadOnly customerPicker As New ComboBox()
    Private ReadOnly rulesGrid As New DataGridView()
    Private ReadOnly fieldsGrid As New DataGridView()
    Private ReadOnly summaryLabel As New Label()
    Private ReadOnly notesLabel As New TextBox()
    Private rules As New List(Of ScanRuleRecord)()
    Private availableParts As New List(Of PaSPart)()
    Private loading As Boolean

    Public Sub New()
        Text = "Pack and Ship | Setup - Scan Rules"
        Size = New Size(1200, 790)
        MinimumSize = New Size(950, 600)
        StartPosition = FormStartPosition.CenterParent
        Font = New Font("Segoe UI", 10)
        BackColor = Color.FromArgb(244, 247, 250)
        BuildLayout()
        AddHandler Shown, Sub() ReloadRules()
    End Sub

    Private Sub BuildLayout()
        Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 86, .BackColor = navy}
        header.Controls.Add(New Label With {
            .Text = "Scan Rules", .AutoSize = True, .Location = New Point(22, 12),
            .ForeColor = Color.White, .Font = New Font("Segoe UI", 22, FontStyle.Bold)
        })
        header.Controls.Add(New Label With {
            .Text = "Shared Q-Wall rules • Changes also affect Q-Wall",
            .AutoSize = True, .Location = New Point(25, 59), .ForeColor = Color.White
        })

        Dim actions As New FlowLayoutPanel With {
            .Dock = DockStyle.Top, .Height = 54, .Padding = New Padding(12, 9, 12, 4),
            .WrapContents = False
        }
        Dim addRule As New Button With {.Text = "Add rule", .Size = New Size(130, 34)}
        Dim editRule As New Button With {.Text = "Edit selected", .Size = New Size(145, 34)}
        Dim toggleRule As New Button With {.Text = "Activate / Deactivate", .Size = New Size(185, 34)}
        For Each btn In {addRule, editRule, toggleRule}
            btn.BackColor = navy
            btn.ForeColor = Color.White
            btn.FlatStyle = FlatStyle.Flat
            actions.Controls.Add(btn)
        Next
        AddHandler addRule.Click, Sub() OpenRuleEditor(Nothing)
        AddHandler editRule.Click, Sub()
                                       Dim selected = SelectedRule()
                                       If selected Is Nothing Then
                                           MessageBox.Show("Select a rule first.")
                                           Return
                                       End If
                                       OpenRuleEditor(selected)
                                   End Sub
        AddHandler toggleRule.Click, AddressOf ToggleSelectedRule

        Dim filters As New FlowLayoutPanel With {
            .Dock = DockStyle.Top, .Height = 58, .Padding = New Padding(12, 11, 12, 6),
            .WrapContents = False
        }
        filters.Controls.Add(New Label With {.Text = "Search", .AutoSize = True, .Margin = New Padding(5, 7, 10, 0)})
        searchBox.Width = 240
        filters.Controls.Add(searchBox)
        filters.Controls.Add(New Label With {.Text = "Customer", .AutoSize = True, .Margin = New Padding(18, 7, 10, 0)})
        customerPicker.Width = 225
        customerPicker.DropDownStyle = ComboBoxStyle.DropDownList
        filters.Controls.Add(customerPicker)
        Dim refresh As New Button With {.Text = "Refresh", .Size = New Size(95, 33), .BackColor = navy, .ForeColor = Color.White}
        filters.Controls.Add(refresh)
        summaryLabel.AutoSize = True
        summaryLabel.Margin = New Padding(18, 7, 0, 0)
        filters.Controls.Add(summaryLabel)
        AddHandler searchBox.TextChanged, Sub() FilterRules()
        AddHandler customerPicker.SelectedIndexChanged, Sub() FilterRules()
        AddHandler refresh.Click, Sub() ReloadRules()

        ConfigureGrid(rulesGrid)
        rulesGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "Id", .HeaderText = "ID", .DataPropertyName = "Id", .FillWeight = 28})
        rulesGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "SsiPart", .HeaderText = "SSI Part", .DataPropertyName = "SsiPart", .FillWeight = 90})
        rulesGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "Customer", .HeaderText = "Customer", .DataPropertyName = "Customer", .FillWeight = 100})
        rulesGrid.Columns.Add(New DataGridViewTextBoxColumn With {.Name = "ScanCount", .HeaderText = "Scans", .DataPropertyName = "ScanCount", .FillWeight = 36})
        rulesGrid.Columns.Add(New DataGridViewCheckBoxColumn With {.Name = "RequiresMatch", .HeaderText = "Match", .DataPropertyName = "RequiresMatch", .FillWeight = 36})
        rulesGrid.Columns.Add(New DataGridViewCheckBoxColumn With {.Name = "IsActive", .HeaderText = "Active", .DataPropertyName = "IsActive", .FillWeight = 35})
        AddHandler rulesGrid.SelectionChanged, AddressOf ShowRuleDetails

        ConfigureGrid(fieldsGrid)
        fieldsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Scan", .DataPropertyName = "ScanIndex", .FillWeight = 30})
        fieldsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Mode", .DataPropertyName = "ExtractionMode", .FillWeight = 85})
        fieldsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Target", .DataPropertyName = "FieldTarget", .FillWeight = 75})
        fieldsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Label", .DataPropertyName = "DisplayLabel", .FillWeight = 110})
        fieldsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Separator", .DataPropertyName = "Separator", .FillWeight = 60})
        fieldsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Custom sep.", .DataPropertyName = "SeparatorCustom", .FillWeight = 60})
        fieldsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Position", .DataPropertyName = "ValuePosition", .FillWeight = 65})
        fieldsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Segment", .DataPropertyName = "SegmentIndex", .FillWeight = 45})
        fieldsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Length", .DataPropertyName = "FixedLength", .FillWeight = 45})
        fieldsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Prefix", .DataPropertyName = "PrefixValue", .FillWeight = 65})
        fieldsGrid.Columns.Add(New DataGridViewTextBoxColumn With {.HeaderText = "Order", .DataPropertyName = "SequenceOrder", .FillWeight = 40})

        Dim bottom As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(14, 7, 14, 14)}
        Dim detailLayout As New TableLayoutPanel With {.Dock = DockStyle.Fill, .RowCount = 3, .ColumnCount = 1}
        detailLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 32))
        detailLayout.RowStyles.Add(New RowStyle(SizeType.Percent, 100))
        detailLayout.RowStyles.Add(New RowStyle(SizeType.Absolute, 62))
        detailLayout.Controls.Add(New Label With {
            .Text = "Fields for selected rule", .Font = New Font("Segoe UI", 13, FontStyle.Bold),
            .ForeColor = navy, .Dock = DockStyle.Fill
        }, 0, 0)
        detailLayout.Controls.Add(fieldsGrid, 0, 1)
        notesLabel.Dock = DockStyle.Fill
        notesLabel.Multiline = True
        notesLabel.ReadOnly = True
        notesLabel.BackColor = Color.White
        detailLayout.Controls.Add(notesLabel, 0, 2)
        bottom.Controls.Add(detailLayout)

        Dim topGrid As New Panel With {.Dock = DockStyle.Top, .Height = 255, .Padding = New Padding(14, 2, 14, 0)}
        rulesGrid.Dock = DockStyle.Fill
        topGrid.Controls.Add(rulesGrid)
        Controls.Add(bottom)
        Controls.Add(topGrid)
        Controls.Add(filters)
        Controls.Add(actions)
        Controls.Add(header)
    End Sub

    Private Sub ConfigureGrid(grid As DataGridView)
        grid.Dock = DockStyle.Fill
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AutoGenerateColumns = False
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.BackgroundColor = Color.White
        grid.RowHeadersVisible = False
    End Sub

    Private Sub ReloadRules()
        Try
            availableParts = ScanRulesData.ReadPartOptions()
            rules = ScanRulesData.ReadRules()
            Dim previousId = 0
            Dim previous = TryCast(customerPicker.SelectedItem, BusinessUnit)
            If previous IsNot Nothing Then previousId = previous.Id
            If previousId = 0 Then previousId = SessionContext.BusinessUnitId
            Dim options = rules.GroupBy(Function(r) r.CustomerId).
                Select(Function(g) New BusinessUnit With {.Id = g.Key, .Name = g.First().Customer}).
                OrderBy(Function(x) x.Name).ToList()
            options.Insert(0, New BusinessUnit With {.Id = 0, .Name = "All customers"})
            loading = True
            customerPicker.DataSource = options
            customerPicker.DisplayMember = "Name"
            customerPicker.ValueMember = "Id"
            Dim selectedIndex = options.FindIndex(Function(c) c.Id = previousId)
            customerPicker.SelectedIndex = If(selectedIndex >= 0, selectedIndex, 0)
            loading = False
            FilterRules()
        Catch ex As Exception
            loading = False
            MessageBox.Show(ex.Message, "Cannot read shared scan rules",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FilterRules()
        If loading Then Return
        Dim term = searchBox.Text.Trim()
        Dim selected = TryCast(customerPicker.SelectedItem, BusinessUnit)
        Dim customerId = If(selected Is Nothing, 0, selected.Id)
        Dim subset = rules.Where(Function(r) (customerId = 0 OrElse r.CustomerId = customerId) AndAlso
            (term = "" OrElse r.SsiPart.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
             r.Customer.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
             r.Id.ToString().Contains(term))).ToList()
        rulesGrid.DataSource = Nothing
        rulesGrid.DataSource = subset
        rulesGrid.ClearSelection()
        fieldsGrid.DataSource = Nothing
        notesLabel.Clear()
        summaryLabel.Text = subset.Count & " of " & rules.Count & " rules"
    End Sub

    Private Function SelectedRule() As ScanRuleRecord
        If rulesGrid.SelectedRows.Count = 0 Then Return Nothing
        Return TryCast(rulesGrid.SelectedRows(0).DataBoundItem, ScanRuleRecord)
    End Function

    Private Sub OpenRuleEditor(rule As ScanRuleRecord)
        Try
            Using editor As New ScanRuleEditorForm(rule, availableParts)
                If editor.ShowDialog(Me) = DialogResult.OK Then ReloadRules()
            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Scan rule editor", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ToggleSelectedRule(sender As Object, e As EventArgs)
        Dim rule = SelectedRule()
        If rule Is Nothing Then
            MessageBox.Show("Select a rule first.")
            Return
        End If
        Dim action = If(rule.IsActive, "deactivate", "activate")
        If MessageBox.Show("Do you want to " & action & " this rule for " &
                           rule.SsiPart & "? This affects Q-Wall.", "Confirm shared rule change",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
        Try
            ScanRulesData.SetActive(rule, Not rule.IsActive)
            ReloadRules()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Cannot update rule", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ShowRuleDetails(sender As Object, e As EventArgs)
        If loading OrElse rulesGrid.SelectedRows.Count = 0 Then Return
        Dim rule = TryCast(rulesGrid.SelectedRows(0).DataBoundItem, ScanRuleRecord)
        If rule Is Nothing Then Return
        Try
            fieldsGrid.DataSource = ScanRulesData.ReadFields(rule.Id)
            notesLabel.Text = "Notes: " & rule.Notes
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Cannot read scan fields",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
