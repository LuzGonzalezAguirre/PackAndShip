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


    Private isBinding As Boolean





    Private ReadOnly scanGroups As New Dictionary(Of Integer, FlowLayoutPanel)()
    Private ReadOnly choices As List(Of PaSPart)

    Public Sub New()
        InitializeComponent()
        choices = New List(Of PaSPart)()
        original = Nothing
        BindEvents()
        AddHandler scanCount.ValueChanged, AddressOf ScanCountChanged
    End Sub

    Public Sub New(existing As ScanRuleRecord, availableParts As List(Of PaSPart))
        Me.New()
        original = existing
        choices = availableParts
        Text = If(existing Is Nothing, "Agregar regla de escaneo", "Editar regla de escaneo")
        headerTitle.Text = If(existing Is Nothing, "Agregar regla de escaneo", "Editar regla de escaneo n.º " & existing.Id)
        LoadValues()
    End Sub

    Private Sub BindEvents()
        AddHandler customerPicker.SelectedIndexChanged, AddressOf CustomerChanged
        AddHandler scanContainer.Resize, Sub() ResizeGroups()
        AddHandler saveButton.Click, AddressOf SaveRule
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
        customers.Insert(0, New BusinessUnit With {.Id = 0, .Name = "Todos los clientes"})

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
            MessageBox.Show(ex.Message, "Campos de escaneo", MessageBoxButtons.OK, MessageBoxIcon.Warning)
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
                    .Text = "ESCANEO " & (index + 1),
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
                    .Text = "+ Agregar campo", .Width = 130, .Height = 31,
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
            MessageBox.Show(ex.Message, "Campo de escaneo inválido", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End Try
        For index As Integer = 0 To CInt(scanCount.Value) - 1
            If Not fieldModels.Any(Function(f) f.ScanIndex = index) Then
                MessageBox.Show("Escaneo " & (index + 1) & " necesita al menos un campo.")
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
        If MessageBox.Show("¿Guardar esta regla compartida? Los cambios también afectarán a Q-Wall.",
                           "Confirm shared rule update", MessageBoxButtons.YesNo,
                           MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
        Try
            ScanRulesData.SaveRule(rule, fieldModels)
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "No se pudo guardar la regla de escaneo", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
