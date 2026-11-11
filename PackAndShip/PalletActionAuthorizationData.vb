Imports System.Data
Imports Microsoft.Data.SqlClient

Public NotInheritable Class PalletActionAuthorizationData
    Private Sub New()
    End Sub

    Public Shared Function BeginAction(containerId As Integer, kind As String,
                                       reason As String) As Long
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand("dbo.ssi_PaS_BeginPalletAction", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@container_id", SqlDbType.Int).Value = containerId
                cmd.Parameters.Add("@action_type", SqlDbType.VarChar, 20).Value = kind
                cmd.Parameters.Add("@packing_user_id", SqlDbType.Int).Value = SessionContext.UserId
                cmd.Parameters.Add("@reason", SqlDbType.NVarChar, 500).Value = reason.Trim()
                Return Convert.ToInt64(cmd.ExecuteScalar())
            End Using
        End Using
    End Function

    Public Shared Function GetPending(containerId As Integer, kind As String) As DataRow
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(
                "SELECT request_id,reason,requested_by_user_id FROM dbo.ssi_PaS_PalletActionRequests " &
                "WHERE container_id=@container AND action_type=@kind AND status='PENDING'", cn)
                cmd.Parameters.Add("@container", SqlDbType.Int).Value = containerId
                cmd.Parameters.Add("@kind", SqlDbType.VarChar, 20).Value = kind
                Dim t As New DataTable()
                Using a As New SqlDataAdapter(cmd)
                    a.Fill(t)
                End Using
                Return If(t.Rows.Count = 0, Nothing, t.Rows(0))
            End Using
        End Using
    End Function

    Public Shared Function ReadApprovals(requestId As Long) As DataTable
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(
                "SELECT d.department_id AS DepartmentId,d.department_name AS Departamento," &
                "CASE WHEN a.approval_id IS NULL THEN 'PENDIENTE' ELSE 'APROBADO' END AS Estado," &
                "u.name AS Autoriza,a.approved_at AS FechaUTC " &
                "FROM dbo.ssi_PaS_PalletActionRequirements rr " &
                "JOIN dbo.ssi_PaS_RejectionDepartments d ON d.department_id=rr.department_id " &
                "LEFT JOIN dbo.ssi_PaS_PalletActionApprovals a " &
                "ON a.request_id=rr.request_id AND a.department_id=rr.department_id " &
                "LEFT JOIN dbo.ssi_PaS_Users u ON u.user_id=a.authorized_by_user_id " &
                "WHERE rr.request_id=@id ORDER BY d.department_id", cn)
                cmd.Parameters.Add("@id", SqlDbType.BigInt).Value = requestId
                Dim t As New DataTable()
                Using a As New SqlDataAdapter(cmd)
                    a.Fill(t)
                End Using
                Return t
            End Using
        End Using
    End Function

    Public Shared Sub Approve(requestId As Long, departmentId As Integer,
                              barcode As String, password As String)
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand("dbo.ssi_PaS_ApprovePalletAction", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@request_id", SqlDbType.BigInt).Value = requestId
                cmd.Parameters.Add("@department_id", SqlDbType.Int).Value = departmentId
                cmd.Parameters.Add("@barcode", SqlDbType.VarChar, 50).Value = barcode.Trim().ToUpperInvariant()
                cmd.Parameters.Add("@password", SqlDbType.VarChar, 50).Value = password
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Shared Sub Complete(requestId As Long)
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand("dbo.ssi_PaS_CompletePalletAction", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@request_id", SqlDbType.BigInt).Value = requestId
                cmd.Parameters.Add("@packing_user_id", SqlDbType.Int).Value = SessionContext.UserId
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub
End Class
