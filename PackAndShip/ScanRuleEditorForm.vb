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
    Private ReadOnly customerPicker As New ComboBox()
    Private ReadOnly partPicker As New ComboBox()
    Private isBinding As Boolean
    Private ReadOnly scanCount As New NumericUpDown()
    Private ReadOnly matchCheck As New CheckBox()
    Private ReadOnly activeCheck As New CheckBox()
    Private ReadOnly notesBox As New TextBox()
    Private ReadOnly scanContainer As New FlowLayoutPanel()
    Private ReadOnly scanGroups As New Dictionary(Of Integer, FlowLayoutPanel)()
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
        AddHandler scanCount.ValueChanged, AddressOf ScanCountChanged
    End Sub

    Private Sub BuildLayout()
        Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 72, .BackColor = navy}
        header.Controls.Add(New Label With {
            .Text = If(original Is Nothing, "Add Scan Rule", "Edit Scan Rule #" & original.Id),
            .Font = New Font("Segoe UI", 22, FontStyle.Bold), .ForeColor = Color.White,
            .AutoSize = True, .Location = New Point(22, 15)
        })
        Dim upper As New Panel With {.Dock = DockStyle.Top, .Height = 234, .Padding = New Padding(18)}
        upper.Controls.Add(New Label With {.Text = "Customer", .Location = New Point(20, 12), .AutoSize = True})
        customerPicker.SetBounds(20, 38, 280, 33)
        customerPicker.DropDownStyle = ComboBoxStyle.DropDownList
        upper.Controls.Add(customerPicker)
        AddHandler customerPicker.SelectedIndexChanged, AddressOf CustomerChanged

        upper.Controls.Add(New Label With {.Text = "Part number (SSI / Client)", .Location = New Point(320, 12), .AutoSize = True})
        partPicker.SetBounds(320, 38, 520, 33)
        partPicker.DropDownStyle = ComboBoxStyle.DropDownList
        partPicker.DisplayMember = "DisplayName"
        partPicker.ValueMember = "Id"
        upper.Controls.Add(partPicker)

        upper.Controls.Add(New Label With {.Text = "Scan count", .Location = New Point(20, 91), .AutoSize = True})
        scanCount.SetBounds(20, 116, 110, 33)
        scanCount.Minimum = 1
        scanCount.Maximum = 100
        upper.Controls.Add(scanCount)

        matchCheck.Text = "Requires match"
        matchCheck.SetBounds(163, 114, 160, 35)
        activeCheck.Text = "Active"
        activeCheck.SetBounds(340, 114, 130, 35)
        upper.Controls.Add(matchCheck)
        upper.Controls.Add(activeCheck)

        upper.Controls.Add(New Label With {.Text = "Notes", .Location = New Point(20, 165), .AutoSize = True})
        notesBox.Multiline = True
        notesBox.MaxLength = 4000
        notesBox.SetBounds(20, 187, 1050, 37)
        notesBox.Anchor = AnchorStyles.Left Or AnchorStyles.Top Or AnchorStyles.Right
        upper.Controls.Add(notesBox)

        scanContainer.Dock = DockStyle.Fill
        scanContainer.FlowDirection = FlowDirection.TopDown
        scanContainer.WrapContents = False
        scanContainer.AutoScroll = True
        scanContainer.Padding = New Padding(12, 10, 12, 10)
        AddHandler scanContainer.Resize, Sub() ResizeGroups()

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

        Controls.Add(scanContainer)
        Controls.Add(footer)
        Controls.Add(upper)
        Controls.Add(header)
    End Sub

    Private Sub StyleButton(button As Button)
        button.FlatStyle = FlatStyle.Flat
        button.BackColor = navy
        button.ForeColor = Color.White
    End Sub

    Private Sub LoadValues()
        ' In Edit preserve the stored part even if it belongs to a different customer.
        Dim initialCustomerId As Integer = SessionContext.BusinessUnitId
        If original IsNot Nothing Then
            Dim originalPart = choices.FirstOrDefault(Function(p) p.Id = original.PartId)
            If originalPart IsNot Nothing AndAlso originalPart.BusinessUnitId.HasValue Then
                initialCustomerId = originalPart.BusinessUnitId.Value
            Else
                initialCustomerId = original.CustomerId
            End If
        End If

        Dim customers = choices.Where(Function(p) p.BusinessUnitId.HasValue).
            GroupBy(Function(p) p.BusinessUnitId.Value).
            Select(Function(g) New BusinessUnit With {.Id = g.Key, .Name = g.First().BusinessUnit}).
            OrderBy(Function(b) b.Name).ToList()
        customers.Insert(0, New BusinessUnit With {.Id = 0, .Name = "All customers"})

        isBinding = True
        customerPicker.DisplayMember = "Name"
        customerPicker.ValueMember = "Id"
        customerPicker.DataSource = customers
        Dim index = customers.FindIndex(Function(b) b.Id = initialCustomerId)
        customerPicker.SelectedIndex = If(index >= 0, index, 0)
        isBinding = False
        RefreshParts(If(original Is Nothing, 0, original.PartId))

        If original Is Nothing Then
            scanCount.Value = 2
            activeCheck.Checked = False ' New shared rules require deliberate activation.
            BuildScanGroups(New List(Of ScanFieldRecord)())
            Return
        End If
        scanCount.Value = Math.Max(1, Math.Min(100, original.ScanCount))
        matchCheck.Checked = original.RequiresMatch
        activeCheck.Checked = original.IsActive
        notesBox.Text = original.Notes
        BuildScanGroups(ScanRulesData.ReadFields(original.Id))
    End Sub

    Private Sub CustomerChanged(sender As Object, e As EventArgs)
        If isBinding Then Return
        RefreshParts(0)
    End Sub

    Private Sub RefreshParts(preferredPartId As Integer)
        Dim customer = TryCast(customerPicker.SelectedItem, BusinessUnit)
        Dim selectedId As Integer = If(customer Is Nothing, 0, customer.Id)
        Dim filtered = choices.Where(Function(p) selectedId = 0 OrElse
            (p.BusinessUnitId.HasValue AndAlso p.BusinessUnitId.Value = selectedId)).
            OrderBy(Function(p) p.Number).ThenBy(Function(p) p.Id).ToList()

        isBinding = True
        partPicker.DataSource = Nothing
        partPicker.DisplayMember = "DisplayName"
        partPicker.ValueMember = "Id"
        partPicker.DataSource = filtered
        partPicker.SelectedIndex = -1
        If preferredPartId > 0 Then
            Dim index = filtered.FindIndex(Function(p) p.Id = preferredPartId)
            If index >= 0 Then partPicker.SelectedIndex = index
        End If
        isBinding = False
    End Sub

    Private Sub ScanCountChanged(sender As Object, e As EventArgs)
        Dim saved As List(Of ScanFieldRecord)
        Try
            saved = GetFieldModels()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Scan fields", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End Try
        If saved.Any(Function(f) f.ScanIndex >= CInt(scanCount.Value)) Then
            If MessageBox.Show("Reducing scan count will remove fields from higher scans. Continue?",
                               "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
                RemoveHandler scanCount.ValueChanged, AddressOf ScanCountChanged
                scanCount.Value = saved.Max(Function(f) f.ScanIndex) + 1
                AddHandler scanCount.ValueChanged, AddressOf ScanCountChanged
                Return
            End If
        End If
        BuildScanGroups(saved.Where(Function(f) f.ScanIndex < CInt(scanCount.Value)).ToList())
    End Sub

    Private Sub BuildScanGroups(models As List(Of ScanFieldRecord))
        scanContainer.SuspendLayout()
        Try
            While scanContainer.Controls.Count > 0
                Dim item = scanContainer.Controls(0)
                scanContainer.Controls.RemoveAt(0)
                item.Dispose()
            End While
            scanGroups.Clear()
            For index As Integer = 0 To CInt(scanCount.Value) - 1
                Dim currentIndex = index
                Dim group As New FlowLayoutPanel With {
                    .FlowDirection = FlowDirection.TopDown, .WrapContents = False,
                    .Width = Math.Max(900, scanContainer.ClientSize.Width - 32),
                    .AutoScroll = False, .BackColor = Color.White,
                    .BorderStyle = BorderStyle.FixedSingle,
                    .Padding = New Padding(10, 8, 10, 8),
                    .Margin = New Padding(0, 0, 0, 14)
                }
                Dim heading As New Label With {
                    .Text = "SCAN " & (index + 1),
                    .ForeColor = Color.FromArgb(31, 78, 121),
                    .Font = New Font("Segoe UI", 12, FontStyle.Bold),
                    .Height = 33, .Width = group.Width - 36
                }
                group.Controls.Add(heading)
                scanGroups.Add(index, group)
                Dim scanFields = models.Where(Function(f) f.ScanIndex = currentIndex).
                                        OrderBy(Function(f) f.SequenceOrder).ToList()
                If scanFields.Count = 0 Then
                    scanFields.Add(New ScanFieldRecord With {
                        .ScanIndex = currentIndex, .SequenceOrder = 0,
                        .ExtractionMode = "por_separador", .Separator = "espacio",
                        .ValuePosition = "despues",
                        .FieldTarget = If(currentIndex = 0, "frameSN", "volvoSerialNumber"),
                        .DisplayLabel = (currentIndex + 1).ToString()
                    })
                End If
                For Each field In scanFields
                    InsertFieldCard(currentIndex, field)
                Next
                Dim add As New Button With {
                    .Text = "+ Add field", .Width = 130, .Height = 31,
                    .ForeColor = Color.FromArgb(31, 78, 121),
                    .FlatStyle = FlatStyle.Flat, .Tag = currentIndex
                }
                AddHandler add.Click, Sub()
                                          Dim groupFields = group.Controls.OfType(Of ScanFieldCard)().Count()
                                          Dim model As New ScanFieldRecord With {
                                              .ScanIndex = currentIndex, .SequenceOrder = groupFields,
                                              .ExtractionMode = "por_separador", .Separator = "espacio",
                                              .ValuePosition = "despues",
                                              .FieldTarget = If(currentIndex = 0, "frameSN", "volvoSerialNumber"),
                                              .DisplayLabel = (currentIndex + 1).ToString()
                                          }
                                          InsertFieldCard(currentIndex, model)
                                          group.Controls.SetChildIndex(group.Controls.OfType(Of ScanFieldCard)().Last(), group.Controls.Count - 2)
                                          UpdateGroupHeight(group)
                                      End Sub
                group.Controls.Add(add)
                UpdateGroupHeight(group)
                scanContainer.Controls.Add(group)
            Next
            ResizeGroups()
        Finally
            scanContainer.ResumeLayout()
        End Try
    End Sub

    Private Sub InsertFieldCard(index As Integer, model As ScanFieldRecord)
        Dim group = scanGroups(index)
        Dim card As New ScanFieldCard(index, model)
        card.Width = Math.Max(795, group.Width - 35)
        AddHandler card.RemoveRequested, Sub(chosen)
                                             group.Controls.Remove(chosen)
                                             chosen.Dispose()
                                             UpdateGroupHeight(group)
                                         End Sub
        group.Controls.Add(card)
        UpdateGroupHeight(group)
    End Sub

    Private Sub UpdateGroupHeight(group As FlowLayoutPanel)
        group.Height = 65 + group.Controls.OfType(Of ScanFieldCard)().Count() * 225 + 43
    End Sub

    Private Sub ResizeGroups()
        For Each group In scanGroups.Values
            group.Width = Math.Max(900, scanContainer.ClientSize.Width - 32)
            For Each card In group.Controls.OfType(Of ScanFieldCard)()
                card.Width = Math.Max(795, group.Width - 35)
            Next
        Next
    End Sub

    Private Function GetFieldModels() As List(Of ScanFieldRecord)
        Dim result As New List(Of ScanFieldRecord)()
        For Each group In scanGroups.OrderBy(Function(g) g.Key)
            For Each card In group.Value.Controls.OfType(Of ScanFieldCard)()
                result.Add(card.GetField())
            Next
        Next
        Return result
    End Function

    Private Sub SaveRule(sender As Object, e As EventArgs)
        Dim fieldModels As List(Of ScanFieldRecord)
        Try
            fieldModels = GetFieldModels()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Invalid scan field", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End Try
        For index As Integer = 0 To CInt(scanCount.Value) - 1
            If Not fieldModels.Any(Function(f) f.ScanIndex = index) Then
                MessageBox.Show("Scan " & (index + 1) & " needs at least one field.")
                Return
            End If
        Next
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
            ScanRulesData.SaveRule(rule, fieldModels)
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Cannot save scan rule", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
