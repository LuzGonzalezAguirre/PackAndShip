Imports System.Linq
Imports System.Text.RegularExpressions

' Format-only validation. Q-Wall PASS and global uniqueness are separate,
' mandatory checks that must succeed before a piece can be recorded.
Public NotInheritable Class ScanFormatResult
    Public Property Valid As Boolean
    Public Property Message As String = ""
    Public Property InternalSerial As String = ""
    Public Property ClientSerial As String = ""
End Class

Public NotInheritable Class ScanFormatValidator
    Private Sub New()
    End Sub

    Public Shared Function CheckTwoLabels(firstRaw As String, secondRaw As String,
                                          rule As ScanRuleRecord,
                                          fields As List(Of ScanFieldRecord)) As ScanFormatResult
        If rule Is Nothing OrElse Not rule.IsActive Then
            Return Fail("No active scan rule for this part and customer.")
        End If
        If rule.ScanCount <> 2 Then
            Return Fail("This model does not use the two-label process. Its specific scan flow is pending.")
        End If
        If fields Is Nothing Then Return Fail("No scan fields were found.")

        Dim extracted As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For scanIndex As Integer = 0 To 1
            Dim raw = If(scanIndex = 0, firstRaw, secondRaw)
            If String.IsNullOrWhiteSpace(raw) Then
                Return Fail("Scan " & (scanIndex + 1) & " is required.")
            End If
            Dim mappings = fields.Where(Function(f) f.ScanIndex = scanIndex).OrderBy(Function(f) f.SequenceOrder).ToList()
            If mappings.Count = 0 Then Return Fail("Scan " & (scanIndex + 1) & " has no field configuration.")
            For Each mapping In mappings
                Dim value As String = Nothing
                Dim errorText As String = Nothing
                If Not TryExtract(raw.Trim(), mapping, value, errorText) Then
                    Return Fail("Scan " & (scanIndex + 1) & ": " & errorText)
                End If
                If mapping.FieldTarget <> "descartado" Then
                    If extracted.ContainsKey(mapping.FieldTarget) Then
                        Return Fail("The scan rule maps the same target more than once.")
                    End If
                    extracted.Add(mapping.FieldTarget, value)
                End If
            Next
        Next
        If Not extracted.ContainsKey("frameSN") OrElse
           Not extracted.ContainsKey("volvoSerialNumber") Then
            Return Fail("Both internal and client serial fields must be configured.")
        End If
        Dim internalSerial = extracted("frameSN")
        Dim clientSerial = extracted("volvoSerialNumber")
        If rule.RequiresMatch AndAlso Not String.Equals(internalSerial, clientSerial, StringComparison.Ordinal) Then
            Return Fail("Internal and client serial values do not match.")
        End If
        Return New ScanFormatResult With {
            .Valid = True,
            .Message = "QR format valid. Q-Wall PASS and unique-container verification are still required.",
            .InternalSerial = internalSerial,
            .ClientSerial = clientSerial
        }
    End Function

    Private Shared Function TryExtract(raw As String, field As ScanFieldRecord,
                                       ByRef result As String, ByRef errorText As String) As Boolean
        ' Fail closed: do not approximate unimplemented parser modes.
        If field.ExtractionMode <> "por_separador" OrElse
           field.ValuePosition <> "despues" Then
            errorText = "Extraction mode '" & field.ExtractionMode & "' is not supported by the scanner yet."
            Return False
        End If
        Dim separatorValue As String
        Select Case field.Separator
            Case "espacio"
                separatorValue = " "
            Case "guion"
                separatorValue = "-"
            Case "apostrofe"
                separatorValue = "'"
            Case "guion_bajo"
                separatorValue = "_"
            Case "pipe"
                separatorValue = "|"
            Case "custom"
                separatorValue = field.SeparatorCustom
            Case Else
                errorText = "Separator '" & field.Separator & "' is not supported in split mode."
                Return False
        End Select
        If String.IsNullOrEmpty(separatorValue) Then
            errorText = "Separator is empty."
            Return False
        End If

        Dim prefix = If(field.PrefixValue, "")
        If prefix = "" Then
            errorText = "Known prefix is missing. This rule cannot be verified safely."
            Return False
        End If
        Dim expectedStart = prefix & separatorValue
        If Not raw.StartsWith(expectedStart, StringComparison.OrdinalIgnoreCase) Then
            errorText = "The code must begin with the configured prefix followed by its separator."
            Return False
        End If

        result = raw.Substring(expectedStart.Length)
        If Not field.FixedLength.HasValue OrElse field.FixedLength.Value <= 0 Then
            errorText = "Expected length must be configured."
            Return False
        End If
        If result.Length <> field.FixedLength.Value OrElse
           Not Regex.IsMatch(result, "^[0-9]+$") Then
            errorText = "Serial must contain exactly " & field.FixedLength.Value & " numeric digits."
            Return False
        End If
        Return True
    End Function

    Private Shared Function Fail(message As String) As ScanFormatResult
        Return New ScanFormatResult With {.Valid = False, .Message = message}
    End Function
End Class
