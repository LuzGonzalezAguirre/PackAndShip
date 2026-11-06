Imports System.Data
Imports Microsoft.Data.SqlClient

' Read-only history and reports. All corrections execute stored procedures.
Public NotInheritable Class PaSHistoryData
    Private Sub New()
    End Sub

    Public Shared Function SearchContainers(code As String, part As String,
                                            status As String, fromDate As DateTime,
                                            toDate As DateTime) As DataTable
        Const sql As String =
            "SELECT c.container_id AS ContainerId, c.container_code AS Pallet, " &
            "pn.ssiPN AS Model, c.status AS Status, " &
            "COALESCE(SUM(l.required_pieces),0) AS Required, " &
            "(SELECT COUNT(*) FROM dbo.ssi_PaS_ContainerPieces cp " &
            " JOIN dbo.ssi_PaS_ContainerLevels cl ON cl.container_level_id=cp.container_level_id " &
            " WHERE cl.container_id=c.container_id) AS Packed, " &
            "creator.name AS CreatedBy, c.created_at AS CreatedAt, " &
            "closer.name AS ClosedBy, c.closed_at AS ClosedAt " &
            "FROM dbo.ssi_PaS_Containers c " &
            "JOIN dbo.ssi_PaS_PalletConfigurations cfg ON cfg.configuration_id=c.configuration_id " &
            "JOIN dbo.ssi_PartNumbers pn ON pn.pn_id=cfg.pn_id " &
            "JOIN dbo.ssi_PaS_Users creator ON creator.user_id=c.created_by_user_id " &
            "LEFT JOIN dbo.ssi_PaS_Users closer ON closer.user_id=c.closed_by_user_id " &
            "JOIN dbo.ssi_PaS_ContainerLevels l ON l.container_id=c.container_id " &
            "WHERE (@code='' OR c.container_code LIKE '%' + @code + '%') " &
            "AND (@part='' OR pn.ssiPN LIKE '%' + @part + '%') " &
            "AND (@status='' OR c.status=@status) " &
            "AND c.created_at>=@fromDate AND c.created_at<@toDate " &
            "GROUP BY c.container_id,c.container_code,pn.ssiPN,c.status, " &
            "creator.name,c.created_at,closer.name,c.closed_at " &
            "ORDER BY c.created_at DESC, c.container_id DESC"
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@code", SqlDbType.VarChar, 18).Value = code.Trim()
                cmd.Parameters.Add("@part", SqlDbType.VarChar, 50).Value = part.Trim()
                cmd.Parameters.Add("@status", SqlDbType.VarChar, 12).Value = status
                cmd.Parameters.Add("@fromDate", SqlDbType.DateTime2).Value = fromDate.Date
                cmd.Parameters.Add("@toDate", SqlDbType.DateTime2).Value = toDate.Date.AddDays(1)
                Return ReadTable(cmd)
            End Using
        End Using
    End Function

    ' Piece-level production report grouped by actual scan day, model and operator.
    ' Uses UTC dates because scanned_at is stored with SYSUTCDATETIME().
    Public Shared Function PackingReport(fromDate As DateTime, toDate As DateTime) As DataTable
        Const sql As String =
            "SELECT CONVERT(date,p.scanned_at) AS ScanDateUTC, " &
            "pn.ssiPN AS Model,u.name AS Operator, " &
            "COUNT(*) AS PackedPieces,COUNT(DISTINCT c.container_id) AS Pallets " &
            "FROM dbo.ssi_PaS_ContainerPieces p " &
            "JOIN dbo.ssi_PaS_ContainerLevels l ON l.container_level_id=p.container_level_id " &
            "JOIN dbo.ssi_PaS_Containers c ON c.container_id=l.container_id " &
            "JOIN dbo.ssi_PaS_PalletConfigurations cfg ON cfg.configuration_id=c.configuration_id " &
            "JOIN dbo.ssi_PartNumbers pn ON pn.pn_id=cfg.pn_id " &
            "JOIN dbo.ssi_PaS_Users u ON u.user_id=p.scanned_by_user_id " &
            "WHERE p.scanned_at>=@fromDate AND p.scanned_at<@toDate " &
            "GROUP BY CONVERT(date,p.scanned_at),pn.ssiPN,u.name " &
            "ORDER BY ScanDateUTC DESC,Model,Operator"
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@fromDate", SqlDbType.DateTime2).Value = fromDate.Date
                cmd.Parameters.Add("@toDate", SqlDbType.DateTime2).Value = toDate.Date.AddDays(1)
                Return ReadTable(cmd)
            End Using
        End Using
    End Function

    Public Shared Function ReadPieces(containerId As Integer) As DataTable
        Const sql As String =
            "SELECT cp.product_id AS ProductId, l.level_number AS Level, " &
            "q.frameSN AS ScanSSI, q.fpcaSN AS ScanClient, " &
            "cp.verified_inspection_id AS InspectionId, u.name AS ScannedBy, " &
            "cp.scanned_at AS ScannedAt " &
            "FROM dbo.ssi_PaS_ContainerPieces cp " &
            "JOIN dbo.ssi_PaS_ContainerLevels l ON l.container_level_id=cp.container_level_id " &
            "JOIN dbo.ssi_Products q ON q.product_id=cp.product_id " &
            "JOIN dbo.ssi_PaS_Users u ON u.user_id=cp.scanned_by_user_id " &
            "WHERE l.container_id=@id ORDER BY l.level_number,cp.scanned_at"
        Return ByContainer(sql, containerId)
    End Function

    Public Shared Function ReadLevels(containerId As Integer) As DataTable
        Const sql As String =
            "SELECT l.level_number AS Level,l.required_pieces AS Required, " &
            "COUNT(cp.container_piece_id) AS Packed,l.status AS Status, " &
            "u.name AS ClosedBy,l.closed_at AS ClosedAt " &
            "FROM dbo.ssi_PaS_ContainerLevels l " &
            "LEFT JOIN dbo.ssi_PaS_ContainerPieces cp ON cp.container_level_id=l.container_level_id " &
            "LEFT JOIN dbo.ssi_PaS_Users u ON u.user_id=l.closed_by_user_id " &
            "WHERE l.container_id=@id " &
            "GROUP BY l.level_number,l.required_pieces,l.status,u.name,l.closed_at " &
            "ORDER BY l.level_number"
        Return ByContainer(sql, containerId)
    End Function

    Public Shared Function ReadAudit(containerId As Integer) As DataTable
        Const sql As String =
            "SELECT a.audit_id AS AuditId,a.event_at AS EventAt," &
            "a.event_type AS Action,a.product_id AS ProductId," &
            "a.inspection_id AS InspectionId,a.container_level_id AS LevelId," &
            "u.name AS Actor,a.reason AS Reason " &
            "FROM dbo.ssi_PaS_PackingAudit a " &
            "JOIN dbo.ssi_PaS_Users u ON u.user_id=a.actor_user_id " &
            "WHERE a.container_id=@id ORDER BY a.event_at DESC,a.audit_id DESC"
        Return ByContainer(sql, containerId)
    End Function

    Private Shared Function ByContainer(sql As String, id As Integer) As DataTable
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
                Return ReadTable(cmd)
            End Using
        End Using
    End Function

    Private Shared Function ReadTable(cmd As SqlCommand) As DataTable
        Dim result As New DataTable()
        Using adapter As New SqlDataAdapter(cmd)
            adapter.Fill(result)
        End Using
        Return result
    End Function

    Public Shared Sub RemovePiece(containerId As Integer, productId As Integer, reason As String)
        ExecuteCorrection("dbo.ssi_PaS_RemovePiece", containerId, productId, reason)
    End Sub

    Public Shared Sub CancelContainer(containerId As Integer, reason As String)
        ExecuteCorrection("dbo.ssi_PaS_CancelContainer", containerId, Nothing, reason)
    End Sub

    Private Shared Sub ExecuteCorrection(procedure As String, containerId As Integer,
                                         productId As Integer?, reason As String)
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(procedure, cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@container_id", SqlDbType.Int).Value = containerId
                If productId.HasValue Then
                    cmd.Parameters.Add("@product_id", SqlDbType.Int).Value = productId.Value
                End If
                cmd.Parameters.Add("@actor_user_id", SqlDbType.Int).Value = SessionContext.UserId
                cmd.Parameters.Add("@reason", SqlDbType.NVarChar, 500).Value = reason.Trim()
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub
End Class
