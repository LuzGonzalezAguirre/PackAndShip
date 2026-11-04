Imports System.Data
Imports System.Linq
Imports Microsoft.Data.SqlClient

Public NotInheritable Class PaSPart
    Public Property Id As Integer
    Public Property Number As String = ""
    Public Property VolvoNumber As String = ""
    Public ReadOnly Property DisplayName As String
        Get
            If String.IsNullOrWhiteSpace(VolvoNumber) Then Return "SSI: " & Number
            Return "SSI: " & Number & "   |   Volvo: " & VolvoNumber
        End Get
    End Property
    Public Property BusinessUnitId As Integer?
    Public Property BusinessUnit As String = ""
    Public Property Configured As Boolean
    Public Overrides Function ToString() As String
        Return Number
    End Function
End Class

Public NotInheritable Class PalletConfiguration
    Public Property Id As Integer
    Public Property PartId As Integer
    Public Property TotalPieces As Integer
    Public Property IsActive As Boolean = True
    Public Property Levels As New List(Of Integer)()
End Class

Public NotInheritable Class PalletConfigurationData
    Private Sub New()
    End Sub

    Public Shared Function GetParts() As List(Of PaSPart)
        Dim result As New List(Of PaSPart)()
        Using cn = PackAndShipData.CreateConnection()
            Const sql As String =
                "SELECT p.pn_id, p.ssiPN, p.volvoProductNumber, p.bu_id, ISNULL(b.bu_name, ''), " &
                "CASE WHEN c.configuration_id IS NULL THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END " &
                "FROM dbo.ssi_PartNumbers p " &
                "LEFT JOIN dbo.ssi_BusinessUnits b ON b.bu_id = p.bu_id " &
                "LEFT JOIN dbo.ssi_PaS_PalletConfigurations c ON c.pn_id = p.pn_id " &
                "ORDER BY p.ssiPN, p.pn_id"
            Using cmd As New SqlCommand(sql, cn)
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        result.Add(New PaSPart With {
                            .Id = rd.GetInt32(0),
                            .Number = rd.GetString(1),
                            .VolvoNumber = If(rd.IsDBNull(2), "", rd.GetString(2)),
                            .BusinessUnitId = If(rd.IsDBNull(3), CType(Nothing, Integer?), rd.GetInt32(3)),
                            .BusinessUnit = rd.GetString(4),
                            .Configured = rd.GetBoolean(5)
                        })
                    End While
                End Using
            End Using
        End Using
        Return result
    End Function

    Public Shared Function GetConfiguration(partId As Integer) As PalletConfiguration
        Using cn = PackAndShipData.CreateConnection()
            Dim result As PalletConfiguration = Nothing
            Using cmd As New SqlCommand(
                "SELECT configuration_id, pn_id, total_pieces, is_active " &
                "FROM dbo.ssi_PaS_PalletConfigurations WHERE pn_id=@part", cn)
                cmd.Parameters.Add("@part", SqlDbType.Int).Value = partId
                Using rd = cmd.ExecuteReader()
                    If rd.Read() Then
                        result = New PalletConfiguration With {
                            .Id = rd.GetInt32(0),
                            .PartId = rd.GetInt32(1),
                            .TotalPieces = rd.GetInt32(2),
                            .IsActive = rd.GetBoolean(3)
                        }
                    End If
                End Using
            End Using
            If result Is Nothing Then Return Nothing
            Using cmd As New SqlCommand(
                "SELECT level_number, required_pieces FROM dbo.ssi_PaS_PalletLevels " &
                "WHERE configuration_id=@id ORDER BY level_number", cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = result.Id
                Using rd = cmd.ExecuteReader()
                    Dim expected = 1
                    While rd.Read()
                        If rd.GetInt32(0) <> expected Then
                            Throw New InvalidOperationException("Existing pallet levels are not consecutive.")
                        End If
                        result.Levels.Add(rd.GetInt32(1))
                        expected += 1
                    End While
                End Using
            End Using
            Return result
        End Using
    End Function

    Public Shared Sub SaveConfiguration(partId As Integer, quantities As List(Of Integer), active As Boolean)
        If partId <= 0 Then Throw New ArgumentException("Select a part number.")
        If quantities Is Nothing OrElse quantities.Count = 0 Then
            Throw New ArgumentException("Add at least one pallet level.")
        End If
        If quantities.Any(Function(q) q <= 0) Then
            Throw New ArgumentException("Each level must have at least one piece.")
        End If
        Dim sum As Long = 0
        For Each quantity In quantities
            sum += CLng(quantity)
            If sum > Integer.MaxValue Then
                Throw New ArgumentException("Total pieces exceeds the permitted range.")
            End If
        Next
        Dim total As Integer = CInt(sum)

        Using cn = PackAndShipData.CreateConnection()
            Using tx = cn.BeginTransaction(IsolationLevel.Serializable)
                Try
                    ' Revalidate rights against SQL, not only the enabled button.
                    Using cmd As New SqlCommand(
                        "SELECT COUNT(1) FROM dbo.ssi_PaS_Users u " &
                        "INNER JOIN dbo.ssi_PaS_Roles r ON r.role_id=u.role_id " &
                        "WHERE u.user_id=@actor AND u.is_active=1 AND r.can_modify_setup=1", cn, tx)
                        cmd.Parameters.Add("@actor", SqlDbType.Int).Value = SessionContext.UserId
                        If SessionContext.UserId <= 0 OrElse Convert.ToInt32(cmd.ExecuteScalar()) <> 1 Then
                            Throw New UnauthorizedAccessException("Your account cannot modify Setup.")
                        End If
                    End Using

                    Dim id As Integer = 0
                    Using cmd As New SqlCommand(
                        "SELECT configuration_id FROM dbo.ssi_PaS_PalletConfigurations WITH (UPDLOCK, HOLDLOCK) " &
                        "WHERE pn_id=@part", cn, tx)
                        cmd.Parameters.Add("@part", SqlDbType.Int).Value = partId
                        Dim existing = cmd.ExecuteScalar()
                        If existing IsNot Nothing Then id = Convert.ToInt32(existing)
                    End Using

                    If id = 0 Then
                        Using cmd As New SqlCommand(
                            "INSERT INTO dbo.ssi_PaS_PalletConfigurations " &
                            "(pn_id, total_pieces, is_active) OUTPUT INSERTED.configuration_id " &
                            "VALUES (@part, @total, @active)", cn, tx)
                            cmd.Parameters.Add("@part", SqlDbType.Int).Value = partId
                            cmd.Parameters.Add("@total", SqlDbType.Int).Value = total
                            cmd.Parameters.Add("@active", SqlDbType.Bit).Value = active
                            id = Convert.ToInt32(cmd.ExecuteScalar())
                        End Using
                    Else
                        Using cmd As New SqlCommand(
                            "UPDATE dbo.ssi_PaS_PalletConfigurations " &
                            "SET total_pieces=@total, is_active=@active, updated_at=SYSDATETIME() " &
                            "WHERE configuration_id=@id", cn, tx)
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                            cmd.Parameters.Add("@total", SqlDbType.Int).Value = total
                            cmd.Parameters.Add("@active", SqlDbType.Bit).Value = active
                            cmd.ExecuteNonQuery()
                        End Using
                        Using cmd As New SqlCommand(
                            "DELETE FROM dbo.ssi_PaS_PalletLevels WHERE configuration_id=@id", cn, tx)
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                            cmd.ExecuteNonQuery()
                        End Using
                    End If

                    For index As Integer = 0 To quantities.Count - 1
                        Using cmd As New SqlCommand(
                            "INSERT INTO dbo.ssi_PaS_PalletLevels " &
                            "(configuration_id, level_number, required_pieces) VALUES (@id, @level, @pieces)", cn, tx)
                            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                            cmd.Parameters.Add("@level", SqlDbType.Int).Value = index + 1
                            cmd.Parameters.Add("@pieces", SqlDbType.Int).Value = quantities(index)
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
End Class
