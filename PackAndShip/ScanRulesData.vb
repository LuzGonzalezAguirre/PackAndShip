Imports System.Data
Imports System.Linq
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
    Public Property UpdatedAt As DateTime
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
                "SELECT id,pn_id,ssi_pn,bu_id,bu_name,scan_count,requires_match,notes,is_active,updated_at " &
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
                            .IsActive = rd.GetBoolean(8),
                            .UpdatedAt = rd.GetDateTime(9)
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

    Public Shared Function ReadPartOptions() As List(Of PaSPart)
        Return PalletConfigurationData.GetParts()
    End Function

    Private Shared Sub RequireAuthorized(cn As SqlConnection, tx As SqlTransaction)
        If SessionContext.UserId <= 0 Then Throw New UnauthorizedAccessException("Sign in first.")
        Using cmd As New SqlCommand(
            "SELECT COUNT(*) FROM dbo.ssi_PaS_Users u JOIN dbo.ssi_PaS_Roles r ON r.role_id=u.role_id " &
            "WHERE u.user_id=@id AND u.is_active=1 AND r.can_modify_setup=1", cn, tx)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = SessionContext.UserId
            If Convert.ToInt32(cmd.ExecuteScalar()) <> 1 Then
                Throw New UnauthorizedAccessException("Your role cannot modify shared scan rules.")
            End If
        End Using
    End Sub

    Private Shared Sub Validate(rule As ScanRuleRecord, fields As List(Of ScanFieldRecord))
        If rule Is Nothing Then Throw New ArgumentException("Select a rule.")
        If rule.PartId <= 0 Then Throw New ArgumentException("Select a part number.")
        If rule.ScanCount < 1 OrElse rule.ScanCount > 100 Then Throw New ArgumentException("Scan count must be from 1 to 100.")
        If fields Is Nothing OrElse fields.Count = 0 Then Throw New ArgumentException("Add at least one scan field.")
        If fields.Count > 300 Then Throw New ArgumentException("Too many fields.")
        If rule.Notes.Length > 0 AndAlso rule.Notes.Length > 4000 Then Throw New ArgumentException("Notes too long.")
        For Each field In fields
            If field.ScanIndex < 0 OrElse field.ScanIndex >= rule.ScanCount Then
                Throw New ArgumentException("Scan index must be between 0 and scan count - 1.")
            End If
            If field.SequenceOrder < 0 Then Throw New ArgumentException("Sequence order cannot be negative.")
            If field.ExtractionMode.Trim() = "" OrElse field.ExtractionMode.Length > 40 OrElse
               field.FieldTarget.Trim() = "" OrElse field.FieldTarget.Length > 60 Then
                Throw New ArgumentException("Extraction mode and field target are required.")
            End If
            If field.Separator.Length > 40 OrElse field.SeparatorCustom.Length > 20 OrElse
               field.ValuePosition.Length > 20 OrElse field.PrefixValue.Length > 100 OrElse
               field.DisplayLabel.Length > 200 Then
                Throw New ArgumentException("One of the field values exceeds its maximum length.")
            End If
            If field.FixedLength.HasValue AndAlso field.FixedLength.Value <= 0 Then
                Throw New ArgumentException("Fixed length must be positive.")
            End If
            If field.SegmentIndex.HasValue AndAlso field.SegmentIndex.Value < 0 Then
                Throw New ArgumentException("Segment index cannot be negative.")
            End If
        Next
        If fields.GroupBy(Function(x) x.ScanIndex.ToString() & ":" & x.SequenceOrder.ToString()).
            Any(Function(g) g.Count() > 1) Then
            Throw New ArgumentException("Each scan index and sequence order combination must be unique.")
        End If
    End Sub

    ' Rule plus fields are saved atomically. No hard deletion of rules.
    Public Shared Sub SaveRule(rule As ScanRuleRecord, fields As List(Of ScanFieldRecord))
        Validate(rule, fields)
        Using cn = PackAndShipData.CreateConnection()
            Using tx = cn.BeginTransaction(IsolationLevel.Serializable)
                Try
                    RequireAuthorized(cn, tx)
                    Dim number As String = ""
                    Dim customer As String = ""
                    Dim buId As Integer
                    Using cmd As New SqlCommand(
                        "SELECT p.ssiPN, p.bu_id, b.bu_name FROM dbo.ssi_PartNumbers p " &
                        "INNER JOIN dbo.ssi_BusinessUnits b ON b.bu_id=p.bu_id WHERE p.pn_id=@id", cn, tx)
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = rule.PartId
                        Using rd = cmd.ExecuteReader()
                            If Not rd.Read() Then Throw New ArgumentException("Part must belong to a valid customer.")
                            number = rd.GetString(0)
                            buId = rd.GetInt32(1)
                            customer = rd.GetString(2)
                        End Using
                    End Using
                    If rule.IsActive AndAlso fields.Any(Function(field) field.ExtractionMode = "prefijo_11_opcional") Then
                        Throw New InvalidOperationException(
                            "Unsupported optional-separator extraction mode. Keep this rule inactive.")
                    End If
                    If rule.IsActive AndAlso rule.Id = 0 AndAlso
                       String.Equals(customer, "Cummins", StringComparison.OrdinalIgnoreCase) Then
                        Throw New InvalidOperationException(
                            "Cummins rules require a validated prefix/optional-separator parser before activation.")
                    End If

                    Dim id As Integer = rule.Id
                    If id > 0 Then
                        Using cmd As New SqlCommand(
                            "UPDATE dbo.quality_pn_scan_rules SET pn_id=@part,ssi_pn=@pn,bu_id=@bu,bu_name=@name," &
                            "scan_count=@count,requires_match=@match,notes=@notes,is_active=@active," &
                            "updated_at=SYSDATETIME() " &
                            "WHERE id=@id AND updated_at=@updated", cn, tx)
                            AddRuleParameters(cmd, rule, number, buId, customer)
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                            cmd.Parameters.Add("@updated", SqlDbType.DateTime2).Value = rule.UpdatedAt
                            If cmd.ExecuteNonQuery() <> 1 Then
                                Throw New InvalidOperationException("Rule was modified by another session. Refresh and try again.")
                            End If
                        End Using
                        Using cmd As New SqlCommand("DELETE FROM dbo.quality_scan_fields WHERE rule_id=@id", cn, tx)
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                            cmd.ExecuteNonQuery()
                        End Using
                    Else
                        Using cmd As New SqlCommand(
                            "INSERT INTO dbo.quality_pn_scan_rules " &
                            "(pn_id,ssi_pn,bu_id,bu_name,scan_count,requires_match,notes,is_active," &
                            "created_by_id,created_at,updated_by_id,updated_at) " &
                            "OUTPUT INSERTED.id VALUES (@part,@pn,@bu,@name,@count,@match,@notes,@active," &
                            "NULL,SYSDATETIME(),NULL,SYSDATETIME())", cn, tx)
                            AddRuleParameters(cmd, rule, number, buId, customer)
                            id = Convert.ToInt32(cmd.ExecuteScalar())
                        End Using
                    End If
                    For Each field In fields
                        Using cmd As New SqlCommand(
                            "INSERT INTO dbo.quality_scan_fields " &
                            "(rule_id,scan_index,extraction_mode,field_target,separator,separator_custom," &
                            "value_position,segment_index,fixed_length,prefix_value,display_label,sequence_order) " &
                            "VALUES (@rule,@scan,@mode,@target,@sep,@custom,@position,@segment,@length," &
                            "@prefix,@label,@sort)", cn, tx)
                            cmd.Parameters.Add("@rule", SqlDbType.Int).Value = id
                            cmd.Parameters.Add("@scan", SqlDbType.Int).Value = field.ScanIndex
                            cmd.Parameters.Add("@mode", SqlDbType.NVarChar, 40).Value = field.ExtractionMode.Trim()
                            cmd.Parameters.Add("@target", SqlDbType.NVarChar, 60).Value = field.FieldTarget.Trim()
                            cmd.Parameters.Add("@sep", SqlDbType.NVarChar, 40).Value = field.Separator
                            cmd.Parameters.Add("@custom", SqlDbType.NVarChar, 20).Value = field.SeparatorCustom
                            cmd.Parameters.Add("@position", SqlDbType.NVarChar, 20).Value = field.ValuePosition
                            cmd.Parameters.Add("@segment", SqlDbType.Int).Value = If(field.SegmentIndex.HasValue, CType(field.SegmentIndex.Value, Object), DBNull.Value)
                            cmd.Parameters.Add("@length", SqlDbType.Int).Value = If(field.FixedLength.HasValue, CType(field.FixedLength.Value, Object), DBNull.Value)
                            cmd.Parameters.Add("@prefix", SqlDbType.NVarChar, 100).Value = field.PrefixValue
                            cmd.Parameters.Add("@label", SqlDbType.NVarChar, 200).Value = field.DisplayLabel
                            cmd.Parameters.Add("@sort", SqlDbType.Int).Value = field.SequenceOrder
                            cmd.ExecuteNonQuery()
                        End Using
                    Next
                    tx.Commit()
                Catch
                    tx.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Sub

    Private Shared Sub AddRuleParameters(cmd As SqlCommand, rule As ScanRuleRecord,
                                         partNumber As String, buId As Integer, customer As String)
        cmd.Parameters.Add("@part", SqlDbType.Int).Value = rule.PartId
        cmd.Parameters.Add("@pn", SqlDbType.NVarChar, 40).Value = partNumber
        cmd.Parameters.Add("@bu", SqlDbType.Int).Value = buId
        cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = customer
        cmd.Parameters.Add("@count", SqlDbType.Int).Value = rule.ScanCount
        cmd.Parameters.Add("@match", SqlDbType.Bit).Value = rule.RequiresMatch
        cmd.Parameters.Add("@notes", SqlDbType.NVarChar, -1).Value = rule.Notes
        cmd.Parameters.Add("@active", SqlDbType.Bit).Value = rule.IsActive
    End Sub

    Public Shared Sub SetActive(rule As ScanRuleRecord, active As Boolean)
        Using cn = PackAndShipData.CreateConnection()
            Using tx = cn.BeginTransaction()
                Try
                    RequireAuthorized(cn, tx)
                    If active Then
                        Using validation As New SqlCommand(
                            "SELECT COUNT(1) FROM dbo.quality_pn_scan_rules r " &
                            "WHERE r.id=@id AND UPPER(r.bu_name)=N'CUMMINS' " &
                            "AND r.notes LIKE N'%Parser pending%'", cn, tx)
                            validation.Parameters.Add("@id", SqlDbType.Int).Value = rule.Id
                            If Convert.ToInt32(validation.ExecuteScalar()) > 0 Then
                                Throw New InvalidOperationException(
                                    "Cummins optional-separator parser is not implemented. Cannot activate this rule.")
                            End If
                        End Using
                    End If
                    Using cmd As New SqlCommand(
                        "UPDATE dbo.quality_pn_scan_rules SET is_active=@active," &
                        "updated_at=SYSDATETIME() " &
                        "WHERE id=@id AND updated_at=@updated", cn, tx)
                        cmd.Parameters.Add("@active", SqlDbType.Bit).Value = active
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = rule.Id
                        cmd.Parameters.Add("@updated", SqlDbType.DateTime2).Value = rule.UpdatedAt
                        If cmd.ExecuteNonQuery() <> 1 Then
                            Throw New InvalidOperationException("Rule changed since it was loaded. Refresh.")
                        End If
                    End Using
                    tx.Commit()
                Catch
                    tx.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Sub
End Class
