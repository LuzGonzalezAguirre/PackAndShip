Imports System.Data
Imports System.Linq
Imports Microsoft.Data.SqlClient

Public NotInheritable Class PaSPackedPiece
    Public Property ProductId As Integer
    Public Property SsiCode As String = ""
    Public Property ClientCode As String = ""
    Public Property LevelNumber As Integer
    Public Property ScannedAt As DateTime
End Class

Public NotInheritable Class PaSPackedLevel
    Public Property LevelNumber As Integer
    Public Property RequiredPieces As Integer
    Public Property ScannedPieces As Integer
    Public Property Status As String = ""
End Class

Public NotInheritable Class PaSPackedContainer
    Public Property Id As Integer
    Public Property Code As String = ""
    Public Property PartId As Integer
    Public Property Status As String = ""
    Public Property Levels As New List(Of PaSPackedLevel)()
    Public Property Pieces As New List(Of PaSPackedPiece)()
    Public ReadOnly Property RequiredTotal As Integer
        Get
            Return Levels.Sum(Function(l) l.RequiredPieces)
        End Get
    End Property
    Public ReadOnly Property PieceTotal As Integer
        Get
            Return Levels.Sum(Function(l) l.ScannedPieces)
        End Get
    End Property
    Public ReadOnly Property ClosedLevels As Integer
        Get
            Return Levels.Count(Function(l) l.Status = "CLOSED")
        End Get
    End Property
    Public ReadOnly Property CurrentLevel As PaSPackedLevel
        Get
            Return Levels.OrderBy(Function(l) l.LevelNumber).
                FirstOrDefault(Function(l) l.Status = "OPEN")
        End Get
    End Property
End Class

' All changes go through transactional SQL procedures, never direct INSERTs
' from WinForms. Container state is reloaded from SQL after each action.
Public NotInheritable Class PaSPackingData
    Private Sub New()
    End Sub

    Public Shared Function OpenContainer(code As String, partId As Integer) As Integer
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand("dbo.ssi_PaS_OpenContainer", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@container_code", SqlDbType.VarChar, 18).Value = code.Trim()
                cmd.Parameters.Add("@pn_id", SqlDbType.Int).Value = partId
                cmd.Parameters.Add("@actor_user_id", SqlDbType.Int).Value = SessionContext.UserId
                Using reader = cmd.ExecuteReader()
                    If Not reader.Read() Then Throw New InvalidOperationException("OpenContainer returned no container.")
                    Return reader.GetInt32(reader.GetOrdinal("container_id"))
                End Using
            End Using
        End Using
    End Function

    Public Shared Function FindContainer(code As String) As PaSPackedContainer
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(
                "SELECT c.container_id FROM dbo.ssi_PaS_Containers c WHERE c.container_code=@code", cn)
                cmd.Parameters.Add("@code", SqlDbType.VarChar, 18).Value = code.Trim()
                Dim found = cmd.ExecuteScalar()
                If found Is Nothing OrElse found Is DBNull.Value Then Return Nothing
                Return ReadContainer(Convert.ToInt32(found), cn)
            End Using
        End Using
    End Function

    Public Shared Function ReadContainer(id As Integer) As PaSPackedContainer
        Using cn = PackAndShipData.CreateConnection()
            Return ReadContainer(id, cn)
        End Using
    End Function

    Private Shared Function ReadContainer(id As Integer, cn As SqlConnection) As PaSPackedContainer
        Dim container As PaSPackedContainer = Nothing
        Using cmd As New SqlCommand(
            "SELECT c.container_id,c.container_code,c.status,cfg.pn_id " &
            "FROM dbo.ssi_PaS_Containers c " &
            "INNER JOIN dbo.ssi_PaS_PalletConfigurations cfg ON cfg.configuration_id=c.configuration_id " &
            "WHERE c.container_id=@id", cn)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
            Using reader = cmd.ExecuteReader()
                If reader.Read() Then
                    container = New PaSPackedContainer With {
                        .Id = reader.GetInt32(0),
                        .Code = reader.GetString(1),
                        .Status = reader.GetString(2),
                        .PartId = reader.GetInt32(3)
                    }
                End If
            End Using
        End Using
        If container Is Nothing Then Return Nothing

        Using cmd As New SqlCommand(
            "SELECT l.level_number,l.required_pieces,l.status,COUNT(p.container_piece_id) AS actual " &
            "FROM dbo.ssi_PaS_ContainerLevels l " &
            "LEFT JOIN dbo.ssi_PaS_ContainerPieces p ON p.container_level_id=l.container_level_id " &
            "WHERE l.container_id=@id " &
            "GROUP BY l.level_number,l.required_pieces,l.status ORDER BY l.level_number", cn)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    container.Levels.Add(New PaSPackedLevel With {
                        .LevelNumber = reader.GetInt32(0),
                        .RequiredPieces = reader.GetInt32(1),
                        .Status = reader.GetString(2),
                        .ScannedPieces = reader.GetInt32(3)
                    })
                End While
            End Using
        End Using
        Using cmd As New SqlCommand(
            "SELECT p.product_id,l.level_number,q.frameSN,q.fpcaSN,p.scanned_at " &
            "FROM dbo.ssi_PaS_ContainerPieces p " &
            "INNER JOIN dbo.ssi_PaS_ContainerLevels l ON l.container_level_id=p.container_level_id " &
            "INNER JOIN dbo.ssi_Products q ON q.product_id=p.product_id " &
            "WHERE l.container_id=@id ORDER BY p.scanned_at DESC,p.container_piece_id DESC", cn)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id
            Using reader = cmd.ExecuteReader()
                While reader.Read()
                    container.Pieces.Add(New PaSPackedPiece With {
                        .ProductId = reader.GetInt32(0),
                        .LevelNumber = reader.GetInt32(1),
                        .SsiCode = reader.GetString(2),
                        .ClientCode = reader.GetString(3),
                        .ScannedAt = reader.GetDateTime(4)
                    })
                End While
            End Using
        End Using
        Return container
    End Function

    Public Shared Sub AddPiece(containerId As Integer, levelNumber As Integer,
                               ssiCode As String, clientCode As String)
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand("dbo.ssi_PaS_AddPiece", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@container_id", SqlDbType.Int).Value = containerId
                cmd.Parameters.Add("@level_number", SqlDbType.Int).Value = levelNumber
                cmd.Parameters.Add("@ssi_full_code", SqlDbType.NVarChar, 200).Value = ssiCode.Trim()
                cmd.Parameters.Add("@client_full_code", SqlDbType.NVarChar, 200).Value = clientCode.Trim()
                cmd.Parameters.Add("@actor_user_id", SqlDbType.Int).Value = SessionContext.UserId
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Shared Sub CloseLevel(containerId As Integer, levelNumber As Integer)
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand("dbo.ssi_PaS_CloseLevel", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@container_id", SqlDbType.Int).Value = containerId
                cmd.Parameters.Add("@level_number", SqlDbType.Int).Value = levelNumber
                cmd.Parameters.Add("@actor_user_id", SqlDbType.Int).Value = SessionContext.UserId
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Shared Sub CloseContainer(containerId As Integer)
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand("dbo.ssi_PaS_CloseContainer", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@container_id", SqlDbType.Int).Value = containerId
                cmd.Parameters.Add("@actor_user_id", SqlDbType.Int).Value = SessionContext.UserId
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub
End Class
