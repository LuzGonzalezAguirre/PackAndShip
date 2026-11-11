Imports System.Linq
Imports System.Text.RegularExpressions

' Format-only validation. Q-Wall PASS and global uniqueness are separate,
' mandatory checks that must succeed before a piece can be recorded.
Public NotInheritable Class ScanFormatResult
    Public Property Valid As Boolean
    Public Property Message As String = ""
    Public Property InternalSerial As String = ""
    Public Property ClientSerial As String = ""
    Public Property GeneratedClientCode As String = ""
End Class

Public NotInheritable Class ScanFormatValidator
    Private Sub New()
    End Sub

    Public Shared Function CheckScans(firstRaw As String, secondRaw As String,
                                      rule As ScanRuleRecord,
                                      fields As List(Of ScanFieldRecord),
                                      part As PaSPart) As ScanFormatResult
        If rule Is Nothing OrElse Not rule.IsActive Then
            Return Fail("No active scan rule for this part and customer.")
        End If
        If rule.ScanCount <> 1 AndAlso rule.ScanCount <> 2 Then
            Return Fail("Only one- or two-label scan formats are supported.")
        End If
        If fields Is Nothing Then Return Fail("No scan fields were found.")
        If part Is Nothing OrElse part.Id <> rule.PartId Then
            Return Fail("The scan rule does not belong to the selected model.")
        End If

        Dim extracted As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        If fields.Any(Function(f) f.ScanIndex < 0 OrElse f.ScanIndex >= rule.ScanCount) Then
            Return Fail("Scan fields do not match the configured scan count.")
        End If
        For scanIndex As Integer = 0 To rule.ScanCount - 1
            Dim currentScanIndex = scanIndex
            Dim raw = If(scanIndex = 0, firstRaw, secondRaw)
            If String.IsNullOrWhiteSpace(raw) Then
                Return Fail("Scan " & (scanIndex + 1) & " is required.")
            End If
            Dim mappings = fields.Where(Function(f) f.ScanIndex = currentScanIndex).OrderBy(Function(f) f.SequenceOrder).ToList()
            If mappings.Count = 0 Then Return Fail("Scan " & (scanIndex + 1) & " has no field configuration.")
            For Each mapping In mappings
                Dim value As String = Nothing
                Dim errorText As String = Nothing
                Dim expectedPartPrefix As String = ""
                Select Case mapping.FieldTarget
                    Case "frameSN"
                        expectedPartPrefix = part.Number
                    Case "volvoSerialNumber"
                        expectedPartPrefix = part.ClientNumber
                End Select
                If Not TryExtract(raw.Trim(), mapping, expectedPartPrefix, value, errorText) Then
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
        If rule.ScanCount = 1 Then
            ' Q-Wall special Volvo models use a single SSI scan and build the
            ' client serial from the catalog Client PN + space + extracted digits.
            ' Never invent a serial when the SSI mapping or Client PN is missing.
            If fields.Any(Function(f) f.FieldTarget = "volvoSerialNumber") Then
                Return Fail("Single-scan auto-generation requires a frameSN mapping, not a second client mapping.")
            End If
            If Not extracted.ContainsKey("frameSN") Then
                Return Fail("Single-scan rule must extract frameSN.")
            End If
            If String.IsNullOrWhiteSpace(part.ClientNumber) Then
                Return Fail("Client part number is missing; automatic serial cannot be generated.")
            End If
            Dim serial = extracted("frameSN")
            Return New ScanFormatResult With {
                .Valid = True,
                .InternalSerial = serial,
                .ClientSerial = serial,
                .GeneratedClientCode = part.ClientNumber.Trim() & " " & serial,
                .Message = "QR format valid. Client serial generated. Q-Wall PASS and unique-container verification are still required."
            }
        End If
        If Not extracted.ContainsKey("frameSN") OrElse
           Not extracted.ContainsKey("volvoSerialNumber") Then
            Return Fail("Both internal and client serial fields must be configured.")
        End If
        Dim internalSerial = extracted("frameSN")
        Dim clientSerial = extracted("volvoSerialNumber")
        ' Two-label PackAndShip flow: both extracted numeric serials must match,
        ' regardless of the legacy requires_match flag in the shared Q-Wall rule.
        If Not String.Equals(internalSerial, clientSerial, StringComparison.Ordinal) Then
            Return Fail("Serial mismatch: SSI (" & internalSerial &
                        ") and Client (" & clientSerial & ") must be identical.")
        End If
        Return New ScanFormatResult With {
            .Valid = True,
            .Message = "QR format valid. Q-Wall PASS and unique-container verification are still required.",
            .InternalSerial = internalSerial,
            .ClientSerial = clientSerial,
            .GeneratedClientCode = part.ClientNumber.Trim() & " " & clientSerial
        }
    End Function

    Private Shared Function TryExtract(raw As String, field As ScanFieldRecord,
                                       expectedPartPrefix As String,
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
                If String.IsNullOrEmpty(separatorValue) Then
                    errorText = "Custom separator is empty."
                    Return False
                End If
            Case "ninguno"
                ' Some Q-Wall models print SSI PN directly beside the serial,
                ' e.g. 43291.6 + 11 digits, without whitespace.
                separatorValue = ""
            Case Else
                errorText = "Separator '" & field.Separator & "' is not supported in split mode."
                Return False
        End Select

        Dim prefix = If(field.PrefixValue, "").Trim()
        Dim catalogPrefix = If(expectedPartPrefix, "").Trim()
        If catalogPrefix = "" Then
            errorText = "The selected model has no SSI/Client part prefix in the catalog."
            Return False
        End If
        ' Existing Q-Wall rules may leave prefix_value empty.
        ' Only in that case, derive it from the selected model's catalog.
        ' A populated rule prefix must match that same catalog part number.
        If prefix = "" Then
            prefix = catalogPrefix
        ElseIf Not String.Equals(prefix, catalogPrefix, StringComparison.OrdinalIgnoreCase) Then
            errorText = "The configured prefix does not match the selected model's part number."
            Return False
        End If
        Dim expectedStart = prefix & separatorValue
        If Not raw.StartsWith(expectedStart, StringComparison.OrdinalIgnoreCase) Then
            errorText = If(field.Separator = "ninguno",
                "The code must begin with the configured prefix (without a separator).",
                "The code must begin with the configured prefix followed by its separator.")
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
