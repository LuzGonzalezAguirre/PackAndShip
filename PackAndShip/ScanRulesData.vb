Imports System.Data
Imports Microsoft.Data.SqlClient

Public NotInheritable Class ScanRuleRecord
    Public Property Id As Integer
    Public Property PartId As Integer
    Public Property SsiPart As String = ""
    Public Property CustomerId As Integer
    Public Property Customer As String = ""
    Public Property ScanCount As Integer
    Public Property RequiresMatch As Boolean
    Public Property IsActive As Boolean
    Public Property Notes As String = ""
End Class

Public NotInheritable Class ScanFieldRecord
    Public Property Id As Integer
    Public Property RuleId As Integer
    Public Property ScanIndex As Integer
    Public Property ExtractionMode As String = ""
    Public Property FieldTarget As String = ""
    Public Property Separator As String = ""
    Public Property SeparatorCustom As String = ""
    Public Property ValuePosition As String = ""
    Public Property SegmentIndex As Integer?
    Public Property FixedLength As Integer?
    Public Property PrefixValue As String = ""
    Public Property DisplayLabel As String = ""
    Public Property SequenceOrder As Integer
End Class

Public NotInheritable Class ScanRulesData
    Private Sub New()
    End Sub

    Public Shared Function ReadRules() As List(Of ScanRuleRecord)
        Dim result As New List(Of ScanRuleRecord)()
        Using cn = PackAndShipData.CreateConnection()
            Const sql As String =
                "SELECT id,pn_id,ssi_pn,bu_id,bu_name,scan_count,requires_match,notes,is_active " &
                "FROM dbo.quality_pn_scan_rules ORDER BY bu_name,ssi_pn,id"
            Using cmd As New SqlCommand(sql, cn)
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        result.Add(New ScanRuleRecord With {
                            .Id = rd.GetInt32(0),
                            .PartId = rd.GetInt32(1),
                            .SsiPart = rd.GetString(2),
                            .CustomerId = rd.GetInt32(3),
                            .Customer = rd.GetString(4),
                            .ScanCount = rd.GetInt32(5),
                            .RequiresMatch = rd.GetBoolean(6),
                            .Notes = If(rd.IsDBNull(7), "", rd.GetString(7)),
                            .IsActive = rd.GetBoolean(8)
                        })
                    End While
                End Using
            End Using
        End Using
        Return result
    End Function

    Public Shared Function ReadFields(ruleId As Integer) As List(Of ScanFieldRecord)
        Dim result As New List(Of ScanFieldRecord)()
        Using cn = PackAndShipData.CreateConnection()
            Const sql As String =
                "SELECT id,rule_id,scan_index,extraction_mode,field_target,separator," &
                "separator_custom,value_position,segment_index,fixed_length,prefix_value," &
                "display_label,sequence_order FROM dbo.quality_scan_fields " &
                "WHERE rule_id=@rule ORDER BY scan_index,sequence_order,id"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@rule", SqlDbType.Int).Value = ruleId
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        result.Add(New ScanFieldRecord With {
                            .Id = rd.GetInt32(0),
                            .RuleId = rd.GetInt32(1),
                            .ScanIndex = rd.GetInt32(2),
                            .ExtractionMode = rd.GetString(3),
                            .FieldTarget = rd.GetString(4),
                            .Separator = rd.GetString(5),
                            .SeparatorCustom = rd.GetString(6),
                            .ValuePosition = rd.GetString(7),
                            .SegmentIndex = If(rd.IsDBNull(8), CType(Nothing, Integer?), rd.GetInt32(8)),
                            .FixedLength = If(rd.IsDBNull(9), CType(Nothing, Integer?), rd.GetInt32(9)),
                            .PrefixValue = rd.GetString(10),
                            .DisplayLabel = rd.GetString(11),
                            .SequenceOrder = rd.GetInt32(12)
                        })
                    End While
                End Using
            End Using
        End Using
        Return result
    End Function
End Class
