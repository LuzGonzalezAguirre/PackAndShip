Imports System.Data
Imports Microsoft.Data.SqlClient

Public NotInheritable Class ScanRejectionInfo
    Public Property Id As Long
    Public Property ContainerId As Integer
    Public Property Reason As String = ""
    Public Property Kind As String = ""
    Public Property SsiCode As String = ""
    Public Property ClientCode As String = ""
    Public Property DetectedAt As DateTime
End Class

Public NotInheritable Class RejectionAuthorizationData
    Private Sub New()
    End Sub

    Public Shared Function Pending(containerId As Integer) As ScanRejectionInfo
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(
                "SELECT rejection_id,container_id,rejection_reason,rejection_type,ssi_scan," &
                "COALESCE(client_scan,N''),detected_at FROM dbo.ssi_PaS_ScanRejections " &
                "WHERE container_id=@id AND status='BLOCKED'", cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = containerId
                Using rd = cmd.ExecuteReader()
                    If Not rd.Read() Then Return Nothing
                    Return New ScanRejectionInfo With {
                        .Id = rd.GetInt64(0), .ContainerId = rd.GetInt32(1),
                        .Reason = rd.GetString(2), .Kind = rd.GetString(3),
                        .SsiCode = rd.GetString(4), .ClientCode = rd.GetString(5),
                        .DetectedAt = rd.GetDateTime(6)
                    }
                End Using
            End Using
        End Using
    End Function

    Public Shared Function Register(containerId As Integer, kind As String,
                                    ssi As String, client As String, reason As String,
                                    eventToken As Guid) As ScanRejectionInfo
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(PaSDirectSql.StartScanRejection, cn)
                cmd.Parameters.Add("@container_id", SqlDbType.Int).Value = containerId
                cmd.Parameters.Add("@actor_user_id", SqlDbType.Int).Value = SessionContext.UserId
                cmd.Parameters.Add("@rejection_type", SqlDbType.VarChar, 30).Value = kind
                cmd.Parameters.Add("@ssi_scan", SqlDbType.NVarChar, 200).Value = If(ssi, "")
                cmd.Parameters.Add("@client_scan", SqlDbType.NVarChar, 200).Value = If(client, "")
                cmd.Parameters.Add("@reason", SqlDbType.NVarChar, 1000).Value = If(reason, "")
                cmd.Parameters.Add("@event_token", SqlDbType.UniqueIdentifier).Value = eventToken
                cmd.ExecuteNonQuery()
            End Using
        End Using
        Return Pending(containerId)
    End Function

    Public Shared Function RequiredApprovals(rejectionId As Long) As DataTable
        Const sql As String =
            "SELECT d.department_id AS DepartmentId,d.department_name AS Departamento," &
            "CASE WHEN a.approval_id IS NULL THEN 'PENDIENTE' ELSE 'APROBADO' END AS Estado," &
            "u.name AS Autoriza, a.approved_at AS FechaUTC " &
            "FROM dbo.ssi_PaS_RejectionRequirements rr " &
            "JOIN dbo.ssi_PaS_RejectionDepartments d ON d.department_id=rr.department_id " &
            "LEFT JOIN dbo.ssi_PaS_RejectionApprovals a ON a.rejection_id=rr.rejection_id " &
            "AND a.department_id=rr.department_id " &
            "LEFT JOIN dbo.ssi_PaS_Users u ON u.user_id=a.authorized_by_user_id " &
            "WHERE rr.rejection_id=@id ORDER BY d.department_id"
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = rejectionId
                Dim result As New DataTable()
                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(result)
                End Using
                Return result
            End Using
        End Using
    End Function

    Public Shared Sub Approve(rejectionId As Long, departmentId As Integer,
                              barcode As String, password As String)
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(PaSDirectSql.ApproveScanRejection, cn)
                cmd.Parameters.Add("@rejection_id", SqlDbType.BigInt).Value = rejectionId
                cmd.Parameters.Add("@department_id", SqlDbType.Int).Value = departmentId
                cmd.Parameters.Add("@barcode", SqlDbType.VarChar, 50).Value = barcode.Trim().ToUpperInvariant()
                cmd.Parameters.Add("@password", SqlDbType.VarChar, 50).Value = password
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Shared Function DepartmentSettings() As DataTable
        Const sql As String =
            "SELECT d.department_id AS DepartmentId,d.department_name AS Departamento," &
            "d.is_required AS Habilitado,r.role_id AS RoleId, " &
            "roles.role_name AS Rol FROM dbo.ssi_PaS_RejectionDepartments d " &
            "LEFT JOIN dbo.ssi_PaS_RejectionDepartmentRoles r ON r.department_id=d.department_id " &
            "LEFT JOIN dbo.ssi_PaS_Roles roles ON roles.role_id=r.role_id " &
            "ORDER BY d.department_id,roles.role_name"
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(sql, cn)
                Dim t As New DataTable()
                Using adapter As New SqlDataAdapter(cmd)
                    adapter.Fill(t)
                End Using
                Return t
            End Using
        End Using
    End Function

    Public Shared Sub SaveDepartment(departmentId As Integer, required As Boolean,
                                     roleIds As IEnumerable(Of Integer))
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(PaSDirectSql.SaveRejectionDepartment, cn)
                cmd.Parameters.Add("@actor_user_id", SqlDbType.Int).Value = SessionContext.UserId
                cmd.Parameters.Add("@department_id", SqlDbType.Int).Value = departmentId
                cmd.Parameters.Add("@is_required", SqlDbType.Bit).Value = required
                cmd.Parameters.Add("@roles_csv", SqlDbType.VarChar, 1000).Value =
                    String.Join(",", roleIds.Distinct().OrderBy(Function(x) x))
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Shared Function FindPackedSerial(partId As Integer, ssi As String,
                                            client As String) As String
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(
                "SELECT TOP(1) c.container_code FROM dbo.ssi_Products q " &
                "JOIN dbo.ssi_PaS_ContainerPieces cp ON cp.product_id=q.product_id " &
                "JOIN dbo.ssi_PaS_ContainerLevels l ON l.container_level_id=cp.container_level_id " &
                "JOIN dbo.ssi_PaS_Containers c ON c.container_id=l.container_id " &
                "WHERE q.pn_id=@part AND q.frameSN=@ssi AND q.fpcaSN=@client", cn)
                cmd.Parameters.Add("@part", SqlDbType.Int).Value = partId
                cmd.Parameters.Add("@ssi", SqlDbType.NVarChar, 200).Value = ssi
                cmd.Parameters.Add("@client", SqlDbType.NVarChar, 200).Value = client
                Dim value = cmd.ExecuteScalar()
                Return If(value Is Nothing OrElse value Is DBNull.Value, "", Convert.ToString(value))
            End Using
        End Using
    End Function

    Public Shared Function FindPackedProduct(productId As Integer) As String
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(
                "SELECT TOP(1) c.container_code FROM dbo.ssi_PaS_ContainerPieces cp " &
                "JOIN dbo.ssi_PaS_ContainerLevels l ON l.container_level_id=cp.container_level_id " &
                "JOIN dbo.ssi_PaS_Containers c ON c.container_id=l.container_id " &
                "WHERE cp.product_id=@id", cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = productId
                Dim value = cmd.ExecuteScalar()
                Return If(value Is Nothing OrElse value Is DBNull.Value, "", Convert.ToString(value))
            End Using
        End Using
    End Function
End Class
