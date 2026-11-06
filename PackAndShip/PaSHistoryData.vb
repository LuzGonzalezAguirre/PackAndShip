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
            "CASE WHEN c.status='TRANSFERRED' THEN pc.actual_pieces ELSE " &
            "(SELECT COUNT(*) FROM dbo.ssi_PaS_ContainerPieces cp " &
            " JOIN dbo.ssi_PaS_ContainerLevels cl ON cl.container_level_id=cp.container_level_id " &
            " WHERE cl.container_id=c.container_id) END AS Packed, " &
            "creator.name AS CreatedBy, c.created_at AS CreatedAt, " &
            "closer.name AS ClosedBy, c.closed_at AS ClosedAt, " &
            "pc.actual_pieces AS PartialPieces, pc.reason AS PartialReason, " &
            "dst.container_code AS TransferredTo,src.container_code AS TransferredFrom " &
            "FROM dbo.ssi_PaS_Containers c " &
            "JOIN dbo.ssi_PaS_PalletConfigurations cfg ON cfg.configuration_id=c.configuration_id " &
            "JOIN dbo.ssi_PartNumbers pn ON pn.pn_id=cfg.pn_id " &
            "JOIN dbo.ssi_PaS_Users creator ON creator.user_id=c.created_by_user_id " &
            "LEFT JOIN dbo.ssi_PaS_Users closer ON closer.user_id=c.closed_by_user_id " &
            "LEFT JOIN dbo.ssi_PaS_PartialClosures pc ON pc.container_id=c.container_id " &
            "LEFT JOIN dbo.ssi_PaS_PalletTransfers as outx ON outx.source_container_id=c.container_id " &
            "LEFT JOIN dbo.ssi_PaS_Containers dst ON dst.container_id=outx.destination_container_id " &
            "LEFT JOIN dbo.ssi_PaS_PalletTransfers as inx ON inx.destination_container_id=c.container_id " &
            "LEFT JOIN dbo.ssi_PaS_Containers src ON src.container_id=inx.source_container_id " &
            "JOIN dbo.ssi_PaS_ContainerLevels l ON l.container_id=c.container_id " &
            "WHERE (@code='' OR c.container_code LIKE '%' + @code + '%') " &
            "AND (@part='' OR pn.ssiPN LIKE '%' + @part + '%') " &
            "AND (@status='' OR c.status=@status) " &
            "AND c.created_at>=@fromDate AND c.created_at<@toDate " &
            "GROUP BY c.container_id,c.container_code,pn.ssiPN,c.status, " &
            "creator.name,c.created_at,closer.name,c.closed_at,pc.actual_pieces,pc.reason, " &
            "dst.container_code,src.container_code " &
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
        ' For transferred source pallets, retain the original physical
        ' contents in history even after SQL reassigns pieces to destination.
        Const sql As String =
            "SELECT cp.product_id AS ProductId, l.level_number AS Level, " &
            "q.frameSN AS ScanSSI, q.fpcaSN AS ScanClient, " &
            "cp.verified_inspection_id AS InspectionId, u.name AS ScannedBy, " &
            "cp.scanned_at AS ScannedAt, CAST(NULL AS VARCHAR(18)) AS TransferredTo " &
            "FROM dbo.ssi_PaS_ContainerPieces cp " &
            "JOIN dbo.ssi_PaS_ContainerLevels l ON l.container_level_id=cp.container_level_id " &
            "JOIN dbo.ssi_Products q ON q.product_id=cp.product_id " &
            "JOIN dbo.ssi_PaS_Users u ON u.user_id=cp.scanned_by_user_id " &
            "WHERE l.container_id=@id " &
            "UNION ALL " &
            "SELECT cp.product_id, sl.level_number, q.frameSN, q.fpcaSN, " &
            "cp.verified_inspection_id, u.name, cp.scanned_at, dst.container_code " &
            "FROM dbo.ssi_PaS_PalletTransfers t " &
            "JOIN dbo.ssi_PaS_PalletTransferPieces tp ON tp.transfer_id=t.transfer_id " &
            "JOIN dbo.ssi_PaS_ContainerPieces cp ON cp.container_piece_id=tp.container_piece_id " &
            "JOIN dbo.ssi_PaS_ContainerLevels sl ON sl.container_level_id=tp.source_level_id " &
            "JOIN dbo.ssi_PaS_Containers dst ON dst.container_id=t.destination_container_id " &
            "JOIN dbo.ssi_Products q ON q.product_id=tp.product_id " &
            "JOIN dbo.ssi_PaS_Users u ON u.user_id=cp.scanned_by_user_id " &
            "WHERE t.source_container_id=@id " &
            "ORDER BY Level,ScannedAt"
        Return ByContainer(sql, containerId)
    End Function

    Public Shared Function ReadLevels(containerId As Integer) As DataTable
        Const sql As String =
            "SELECT l.level_number AS Level,l.required_pieces AS Required, " &
            "(COUNT(cp.container_piece_id) + " &
            " (SELECT COUNT(*) FROM dbo.ssi_PaS_PalletTransferPieces tp " &
            "  JOIN dbo.ssi_PaS_PalletTransfers tr ON tr.transfer_id=tp.transfer_id " &
            "  WHERE tr.source_container_id=@id AND tp.source_level_id=l.container_level_id)) AS Packed, " &
            "CASE WHEN c.status='TRANSFERRED' THEN 'TRANSFERRED' ELSE l.status END AS Status, " &
            "u.name AS ClosedBy,l.closed_at AS ClosedAt " &
            "FROM dbo.ssi_PaS_ContainerLevels l " &
            "JOIN dbo.ssi_PaS_Containers c ON c.container_id=l.container_id " &
            "LEFT JOIN dbo.ssi_PaS_ContainerPieces cp ON cp.container_level_id=l.container_level_id " &
            "LEFT JOIN dbo.ssi_PaS_Users u ON u.user_id=l.closed_by_user_id " &
            "WHERE l.container_id=@id " &
            "GROUP BY l.container_level_id,l.level_number,l.required_pieces,l.status,c.status,u.name,l.closed_at " &
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

    Public Shared Function ReadTransfers() As DataTable
        Const sql As String =
            "SELECT t.transfer_id AS TransferId, " &
            "src.container_code AS SourcePallet,dst.container_code AS DestinationPallet, " &
            "pn.ssiPN AS Model,t.transferred_pieces AS Pieces, " &
            "u.name AS TransferredBy,t.transferred_at AS TransferredAt " &
            "FROM dbo.ssi_PaS_PalletTransfers t " &
            "JOIN dbo.ssi_PaS_Containers src ON src.container_id=t.source_container_id " &
            "JOIN dbo.ssi_PaS_Containers dst ON dst.container_id=t.destination_container_id " &
            "JOIN dbo.ssi_PaS_PalletConfigurations cfg ON cfg.configuration_id=dst.configuration_id " &
            "JOIN dbo.ssi_PartNumbers pn ON pn.pn_id=cfg.pn_id " &
            "JOIN dbo.ssi_PaS_Users u ON u.user_id=t.transferred_by_user_id " &
            "ORDER BY t.transferred_at DESC,t.transfer_id DESC"
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(sql, cn)
                Return ReadTable(cmd)
            End Using
        End Using
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
