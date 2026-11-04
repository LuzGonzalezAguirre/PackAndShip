Imports System.ComponentModel
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

' Shared rules are edited as raw database fields so existing Q-Wall values
' remain unchanged unless the operator deliberately modifies them.
Public Class ScanRuleEditorForm
    Inherits Form

    Private ReadOnly original As ScanRuleRecord
    Private ReadOnly navy As Color = Color.FromArgb(31, 78, 121)
    Private ReadOnly partPicker As New ComboBox()
    Private ReadOnly scanCount As New NumericUpDown()
    Private ReadOnly matchCheck As New CheckBox()
    Private ReadOnly activeCheck As New CheckBox()
    Private ReadOnly notesBox As New TextBox()
    Private ReadOnly fieldsGrid As New DataGridView()
    Private ReadOnly fields As New BindingList(Of ScanFieldRecord)()
    Private ReadOnly choices As List(Of PaSPart)

    Public Sub New(existing As ScanRuleRecord, availableParts As List(Of PaSPart))
        original = existing
        choices = availableParts
        Text = If(existing Is Nothing, "Add Scan Rule", "Edit Scan Rule")
        Size = New Size(1190, 765)
        MinimumSize = New Size(1020, 620)
        StartPosition = FormStartPosition.CenterParent
        BackColor = Color.FromArgb(244, 247, 250)
        Font = New Font("Segoe UI", 10)
        BuildLayout()
        LoadValues()
    End Sub

    Private Sub BuildLayout()
        Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 72, .BackColor = navy}
        header.Controls.Add(New Label With {
            .Text = If(original Is Nothing, "Add Scan Rule", "Edit Scan Rule #" & original.Id),
            .Font = New Font("Segoe UI", 22, FontStyle.Bold), .ForeColor = Color.White,
            .AutoSize = True, .Location = New Point(22, 15)
        })
        Dim upper As New Panel With {.Dock = DockStyle.Top, .Height = 191, .Padding = New Padding(18)}
        upper.Controls.Add(New Label With {.Text = "Part number (SSI / Volvo)", .Location = New Point(20, 12), .AutoSize = True})
        partPicker.SetBounds(20, 38, 590, 33)
        partPicker.DropDownStyle = ComboBoxStyle.DropDownList
        partPicker.DataSource = choices
        partPicker.DisplayMember = "DisplayName"
        partPicker.ValueMember = "Id"
        upper.Controls.Add(partPicker)

        upper.Controls.Add(New Label With {.Text = "Scan count", .Location = New Point(632, 12), .AutoSize = True})
        scanCount.SetBounds(632, 38, 110, 33)
        scanCount.Minimum = 1
        scanCount.Maximum = 100
        upper.Controls.Add(scanCount)

        matchCheck.Text = "Requires match"
        matchCheck.SetBounds(770, 36, 160, 35)
        activeCheck.Text = "Active"
        activeCheck.SetBounds(950, 36, 130, 35)
        upper.Controls.Add(matchCheck)
        upper.Controls.Add(activeCheck)

        upper.Controls.Add(New Label With {.Text = "Notes", .Location = New Point(20, 89), .AutoSize = True})
        notesBox.Multiline = True
        notesBox.MaxLength = 4000
        notesBox.SetBounds(20, 112, 1050, 62)
        notesBox.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Right
        upper.Controls.Add(notesBox)

        Dim toolbar As New FlowLayoutPanel With {
            .Dock = DockStyle.Top, .Height = 64, .Padding = New Padding(16, 10, 10, 7)
        }
        Dim addField As New Button With {.Text = "Add field", .Size = New Size(125, 34)}
        Dim removeField As New Button With {.Text = "Remove field", .Size = New Size(130, 34)}
        StyleButton(addField)
        StyleButton(removeField)
        toolbar.Controls.Add(addField)
        toolbar.Controls.Add(removeField)
        toolbar.Controls.Add(New Label With {
            .Text = "Scan index starts at 0. Existing extraction values are preserved.",
            .AutoSize = True, .Margin = New Padding(14, 8, 0, 0)
        })
        AddHandler addField.Click, Sub()
                                       fields.Add(New ScanFieldRecord With {
                                           .ScanIndex = 0, .SequenceOrder = fields.Count,
                                           .ExtractionMode = "por_separador", .FieldTarget = "",
                                           .Separator = "espacio", .ValuePosition = "despues"
                                       })
                                   End Sub
        AddHandler removeField.Click, Sub()
                                          If fieldsGrid.CurrentRow Is Nothing Then
                                              MessageBox.Show("Select a field first.")
                                              Return
                                          End If
                                          Dim field = TryCast(fieldsGrid.CurrentRow.DataBoundItem, ScanFieldRecord)
                                          If field IsNot Nothing Then fields.Remove(field)
                                      End Sub

        fieldsGrid.Dock = DockStyle.Fill
        fieldsGrid.AutoGenerateColumns = False
        fieldsGrid.DataSource = fields
        fieldsGrid.AllowUserToAddRows = False
        fieldsGrid.AllowUserToDeleteRows = False
        fieldsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        fieldsGrid.MultiSelect = False
        fieldsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        fieldsGrid.BackgroundColor = Color.White
        fieldsGrid.RowHeadersVisible = False
        AddColumn("Scan", "ScanIndex", 35)
        AddColumn("Mode", "ExtractionMode", 85)
        AddColumn("Target", "FieldTarget", 85)
        AddColumn("Separator", "Separator", 68)
        AddColumn("Custom separator", "SeparatorCustom", 72)
        AddColumn("Position", "ValuePosition", 70)
        AddColumn("Segment", "SegmentIndex", 48)
        AddColumn("Length", "FixedLength", 48)
        AddColumn("Prefix", "PrefixValue", 110)
        AddColumn("Label", "DisplayLabel", 90)
        AddColumn("Order", "SequenceOrder", 40)
        AddHandler fieldsGrid.DataError, Sub(sender, e)
                                             e.ThrowException = False
                                             MessageBox.Show("Invalid field value. Check numeric columns.",
                                                             "Scan field", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                                         End Sub

        Dim footer As New Panel With {.Dock = DockStyle.Bottom, .Height = 73}
        Dim save As New Button With {.Text = "Save rule", .Size = New Size(158, 39),
                                    .Anchor = AnchorStyles.Top Or AnchorStyles.Right}
        StyleButton(save)
        Dim cancel As New Button With {.Text = "Cancel", .DialogResult = DialogResult.Cancel,
                                      .Size = New Size(130, 39),
                                      .Anchor = AnchorStyles.Top Or AnchorStyles.Right}
        footer.Controls.Add(save)
        footer.Controls.Add(cancel)
        AddHandler footer.Resize, Sub()
                                      save.Left = footer.ClientSize.Width - 184
                                      cancel.Left = save.Left - 145
                                  End Sub
        save.Top = 14
        cancel.Top = 14
        AddHandler save.Click, AddressOf SaveRule
        CancelButton = cancel

        Controls.Add(fieldsGrid)
        Controls.Add(footer)
        Controls.Add(toolbar)
        Controls.Add(upper)
        Controls.Add(header)
    End Sub

    Private Sub AddColumn(title As String, propertyName As String, weight As Single)
        fieldsGrid.Columns.Add(New DataGridViewTextBoxColumn With {
            .HeaderText = title, .DataPropertyName = propertyName, .FillWeight = weight
        })
    End Sub

    Private Sub StyleButton(button As Button)
        button.FlatStyle = FlatStyle.Flat
        button.BackColor = navy
        button.ForeColor = Color.White
    End Sub

    Private Sub LoadValues()
        If original Is Nothing Then
            scanCount.Value = 2
            activeCheck.Checked = True
            Dim customerId = SessionContext.BusinessUnitId
            Dim candidate = choices.FirstOrDefault(Function(p) p.BusinessUnitId.HasValue AndAlso p.BusinessUnitId.Value = customerId)
            If candidate IsNot Nothing Then partPicker.SelectedItem = candidate
            Return
        End If
        partPicker.SelectedValue = original.PartId
        scanCount.Value = Math.Max(1, Math.Min(100, original.ScanCount))
        matchCheck.Checked = original.RequiresMatch
        activeCheck.Checked = original.IsActive
        notesBox.Text = original.Notes
        For Each field In ScanRulesData.ReadFields(original.Id)
            fields.Add(field)
        Next
    End Sub

    Private Sub SaveRule(sender As Object, e As EventArgs)
        If Not fieldsGrid.EndEdit() Then
            MessageBox.Show("Finish editing the current field before saving.")
            Return
        End If
        Dim part = TryCast(partPicker.SelectedItem, PaSPart)
        If part Is Nothing Then
            MessageBox.Show("Select a part number.")
            Return
        End If
        Dim rule As New ScanRuleRecord With {
            .Id = If(original Is Nothing, 0, original.Id),
            .PartId = part.Id,
            .ScanCount = CInt(scanCount.Value),
            .RequiresMatch = matchCheck.Checked,
            .IsActive = activeCheck.Checked,
            .Notes = notesBox.Text,
            .UpdatedAt = If(original Is Nothing, DateTime.MinValue, original.UpdatedAt)
        }
        If MessageBox.Show("Save this shared scan rule? Changes will affect Q-Wall as well.",
                           "Confirm shared rule update", MessageBoxButtons.YesNo,
                           MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
        Try
            ScanRulesData.SaveRule(rule, fields.ToList())
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Cannot save scan rule", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
