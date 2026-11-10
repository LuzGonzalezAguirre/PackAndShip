Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

' Visual editor for one field within a scan. No SQL writes occur here.
Friend Partial Class ScanFieldCard
    Inherits UserControl












    Private ReadOnly scanIndex As Integer
    Public Event RemoveRequested(card As ScanFieldCard)

    Public Sub New()
        InitializeComponent()
        For Each box In {target, mode, separator, position}
            AddHandler box.TextChanged, Sub() UpdatePreview()
            AddHandler box.SelectedIndexChanged, Sub() UpdatePreview()
        Next
        For Each box In {prefix, length, customSeparator}
            AddHandler box.TextChanged, Sub() UpdatePreview()
        Next
        AddHandler remove.Click, Sub() RaiseEvent RemoveRequested(Me)
    End Sub

    Public Sub New(index As Integer, model As ScanFieldRecord)
        Me.New()
        scanIndex = index
        LoadModel(model)
    End Sub

    Private Sub LoadModel(model As ScanFieldRecord)
        fieldLabel.Text = model.DisplayLabel
        target.Text = If(model.FieldTarget = "frameSN", "Serie interna (frameSN)",
                         If(model.FieldTarget = "volvoSerialNumber", "Serie del cliente (volvoSerialNumber)", model.FieldTarget))
        mode.Text = If(model.ExtractionMode = "por_separador", "Separar por delimitador", model.ExtractionMode)
        separator.Text = If(model.Separator = "espacio", "Espacio ( )", model.Separator)
        position.Text = If(model.ValuePosition = "despues", "Después del separador",
                           If(model.ValuePosition = "antes", "Antes del separador", model.ValuePosition))
        prefix.Text = model.PrefixValue
        length.Text = If(model.FixedLength.HasValue, model.FixedLength.Value.ToString(), "")
        customSeparator.Text = model.SeparatorCustom
        segment.Text = If(model.SegmentIndex.HasValue, model.SegmentIndex.Value.ToString(), "")
        order.Value = Math.Min(order.Maximum, Math.Max(0, model.SequenceOrder))

        UpdatePreview()
    End Sub

    Private Sub UpdatePreview()
        If preview Is Nothing Then Return
        Dim sep = If(separator.Text = "Espacio ( )", " ", If(separator.Text = "personalizado", customSeparator.Text, ""))
        Dim count As Integer
        If Not Integer.TryParse(length.Text.Trim(), count) OrElse count <= 0 Then count = 11
        Dim digitExample = New String("X"c, Math.Min(count, 16))
        If count > 16 Then digitExample &= "..."
        If mode.Text = "Separar por delimitador" Then
            preview.Text = "Vista previa: " & prefix.Text & sep & digitExample
        Else
            preview.Text = "Vista previa: verificar modo de extracción"
        End If
    End Sub

    Public Function GetField() As ScanFieldRecord
        Dim parsedLength As Integer? = Nothing
        Dim parsedSegment As Integer? = Nothing
        Dim value As Integer
        If length.Text.Trim() <> "" Then
            If Not Integer.TryParse(length.Text.Trim(), value) OrElse value <= 0 Then
                Throw New ArgumentException("La longitud esperada debe ser un número positivo.")
            End If
            parsedLength = value
        End If
        If segment.Text.Trim() <> "" Then
            If Not Integer.TryParse(segment.Text.Trim(), value) OrElse value < 0 Then
                Throw New ArgumentException("El índice de segmento no puede ser negativo.")
            End If
            parsedSegment = value
        End If
        Return New ScanFieldRecord With {
            .ScanIndex = scanIndex, .DisplayLabel = fieldLabel.Text,
            .FieldTarget = If(target.Text = "Serie interna (frameSN)", "frameSN",
                              If(target.Text = "Serie del cliente (volvoSerialNumber)", "volvoSerialNumber", target.Text)),
            .ExtractionMode = If(mode.Text = "Separar por delimitador", "por_separador", mode.Text),
            .Separator = If(separator.Text = "Espacio ( )", "espacio", separator.Text),
            .SeparatorCustom = customSeparator.Text,
            .ValuePosition = If(position.Text = "Después del separador", "despues",
                                If(position.Text = "Antes del separador", "antes", position.Text)),
            .PrefixValue = prefix.Text,
            .FixedLength = parsedLength, .SegmentIndex = parsedSegment,
            .SequenceOrder = CInt(order.Value)
        }
    End Function
End Class
