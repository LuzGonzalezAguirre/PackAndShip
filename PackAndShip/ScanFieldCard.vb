Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

' Visual editor for one field within a scan. No SQL writes occur here.
Friend Class ScanFieldCard
    Inherits Panel

    Private ReadOnly fieldLabel As New TextBox()
    Private ReadOnly target As New ComboBox()
    Private ReadOnly mode As New ComboBox()
    Private ReadOnly separator As New ComboBox()
    Private ReadOnly position As New ComboBox()
    Private ReadOnly prefix As New TextBox()
    Private ReadOnly length As New TextBox()
    Private ReadOnly customSeparator As New TextBox()
    Private ReadOnly segment As New TextBox()
    Private ReadOnly order As New NumericUpDown()
    Private ReadOnly preview As New Label()
    Private ReadOnly remove As New Button()
    Private ReadOnly scanIndex As Integer
    Public Event RemoveRequested(card As ScanFieldCard)

    Public Sub New(index As Integer, model As ScanFieldRecord)
        scanIndex = index
        BorderStyle = BorderStyle.FixedSingle
        BackColor = Color.FromArgb(249, 251, 254)
        Height = 215
        Width = 875
        Margin = New Padding(0, 0, 0, 10)
        BuildControls(model)
    End Sub

    Private Sub TextAt(caption As String, x As Integer, y As Integer)
        Controls.Add(New Label With {
            .Text = caption, .Location = New Point(x, y),
            .Size = New Size(160, 20), .Font = New Font("Segoe UI", 9),
            .ForeColor = Color.FromArgb(60, 85, 120)
        })
    End Sub

    Private Sub ConfigureChoice(box As ComboBox, values As String(), x As Integer, y As Integer, width As Integer)
        box.SetBounds(x, y, width, 30)
        box.DropDownStyle = ComboBoxStyle.DropDown  ' Existing unknown Q-Wall values remain editable.
        box.Items.AddRange(values)
        Controls.Add(box)
        AddHandler box.TextChanged, Sub() UpdatePreview()
        AddHandler box.SelectedIndexChanged, Sub() UpdatePreview()
    End Sub

    Private Sub BuildControls(model As ScanFieldRecord)
        TextAt("Field label", 12, 12)
        TextAt("Field maps to", 213, 12)
        TextAt("Extraction mode", 451, 12)
        fieldLabel.SetBounds(12, 36, 190, 30)
        fieldLabel.MaxLength = 200
        Controls.Add(fieldLabel)
        ConfigureChoice(target, {"Internal serial (frameSN)", "Customer serial (volvoSerialNumber)"}, 213, 36, 225)
        ConfigureChoice(mode, {"Split by separator"}, 451, 36, 270)
        remove.Text = "×"
        remove.ForeColor = Color.DarkRed
        remove.SetBounds(734, 36, 39, 30)
        Controls.Add(remove)
        AddHandler remove.Click, Sub() RaiseEvent RemoveRequested(Me)

        TextAt("Separator", 12, 79)
        TextAt("Extract value", 174, 79)
        TextAt("Known prefix", 345, 79)
        TextAt("Expected length", 617, 79)
        ConfigureChoice(separator, {"Space ( )", "personalizado", "ninguno"}, 12, 102, 149)
        ConfigureChoice(position, {"After separator", "Before separator"}, 174, 102, 160)
        prefix.SetBounds(345, 102, 260, 30)
        prefix.MaxLength = 100
        Controls.Add(prefix)
        length.SetBounds(617, 102, 103, 30)
        Controls.Add(length)

        TextAt("Custom separator", 12, 145)
        TextAt("Segment index", 190, 145)
        TextAt("Order", 330, 145)
        customSeparator.SetBounds(12, 167, 165, 30)
        customSeparator.MaxLength = 20
        Controls.Add(customSeparator)
        segment.SetBounds(190, 167, 125, 30)
        order.SetBounds(330, 167, 85, 30)
        order.Maximum = Integer.MaxValue
        Controls.Add(segment)
        Controls.Add(order)

        preview.SetBounds(432, 151, 440, 44)
        preview.ForeColor = Color.FromArgb(31, 78, 121)
        preview.Font = New Font("Consolas", 10, FontStyle.Bold)
        Controls.Add(preview)

        fieldLabel.Text = model.DisplayLabel
        target.Text = If(model.FieldTarget = "frameSN", "Internal serial (frameSN)",
                         If(model.FieldTarget = "volvoSerialNumber", "Customer serial (volvoSerialNumber)", model.FieldTarget))
        mode.Text = If(model.ExtractionMode = "por_separador", "Split by separator", model.ExtractionMode)
        separator.Text = If(model.Separator = "espacio", "Space ( )", model.Separator)
        position.Text = If(model.ValuePosition = "despues", "After separator",
                           If(model.ValuePosition = "antes", "Before separator", model.ValuePosition))
        prefix.Text = model.PrefixValue
        length.Text = If(model.FixedLength.HasValue, model.FixedLength.Value.ToString(), "")
        customSeparator.Text = model.SeparatorCustom
        segment.Text = If(model.SegmentIndex.HasValue, model.SegmentIndex.Value.ToString(), "")
        order.Value = Math.Min(order.Maximum, Math.Max(0, model.SequenceOrder))

        For Each box In {prefix, length, customSeparator}
            AddHandler box.TextChanged, Sub() UpdatePreview()
        Next
        UpdatePreview()
    End Sub

    Private Sub UpdatePreview()
        If preview Is Nothing Then Return
        Dim sep = If(separator.Text = "Space ( )", " ", If(separator.Text = "personalizado", customSeparator.Text, ""))
        Dim count As Integer
        If Not Integer.TryParse(length.Text.Trim(), count) OrElse count <= 0 Then count = 11
        Dim digitExample = New String("X"c, Math.Min(count, 16))
        If count > 16 Then digitExample &= "..."
        If mode.Text = "Split by separator" Then
            preview.Text = "Preview: " & prefix.Text & sep & digitExample
        Else
            preview.Text = "Preview: verify extraction mode"
        End If
    End Sub

    Public Function GetField() As ScanFieldRecord
        Dim parsedLength As Integer? = Nothing
        Dim parsedSegment As Integer? = Nothing
        Dim value As Integer
        If length.Text.Trim() <> "" Then
            If Not Integer.TryParse(length.Text.Trim(), value) OrElse value <= 0 Then
                Throw New ArgumentException("Expected length must be a positive number.")
            End If
            parsedLength = value
        End If
        If segment.Text.Trim() <> "" Then
            If Not Integer.TryParse(segment.Text.Trim(), value) OrElse value < 0 Then
                Throw New ArgumentException("Segment index must be a nonnegative number.")
            End If
            parsedSegment = value
        End If
        Return New ScanFieldRecord With {
            .ScanIndex = scanIndex, .DisplayLabel = fieldLabel.Text,
            .FieldTarget = If(target.Text = "Internal serial (frameSN)", "frameSN",
                              If(target.Text = "Customer serial (volvoSerialNumber)", "volvoSerialNumber", target.Text)),
            .ExtractionMode = If(mode.Text = "Split by separator", "por_separador", mode.Text),
            .Separator = If(separator.Text = "Space ( )", "espacio", separator.Text),
            .SeparatorCustom = customSeparator.Text,
            .ValuePosition = If(position.Text = "After separator", "despues",
                                If(position.Text = "Before separator", "antes", position.Text)),
            .PrefixValue = prefix.Text,
            .FixedLength = parsedLength, .SegmentIndex = parsedSegment,
            .SequenceOrder = CInt(order.Value)
        }
    End Function
End Class
