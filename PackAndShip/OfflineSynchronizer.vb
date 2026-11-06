Imports System.Data
Imports System.Linq
Imports Microsoft.Data.SqlClient

' Replay is idempotent across connection failures: a server-side commit may
' have succeeded even if the response was lost. Never mark local data uploaded
' until the server state has been re-read or SQL accepted the operation.
Public NotInheritable Class OfflineSynchronizer
    Private Shared ReadOnly gate As New Object()
    Private Sub New()
    End Sub

    Public Shared Function SyncAll() As String
        If Not Threading.Monitor.TryEnter(gate) Then Return "Synchronization is already running."
        Try
            Dim ok As Integer = 0
            Dim failed As Integer = 0
            Dim outstanding = OfflineStore.Snapshot().Pallets.
                Where(Function(p) p.SyncStatus <> "SYNCED").ToList()
            For Each pending In outstanding
                If Not pending.Completed Then Continue For
                Try
                    SyncOne(pending.LocalId)
                    ok += 1
                Catch ex As Exception
                    failed += 1
                    Try
                        OfflineStore.UpdateSync(pending.LocalId, Sub(p)
                            p.SyncStatus = "REVIEW"
                            p.SyncError = ex.Message
                        End Sub)
                    Catch
                        Throw New InvalidOperationException(
                            "Could not persist synchronization failure to local disk.", ex)
                    End Try
                End Try
            Next
            Return "Synchronized: " & ok & "  |  Pending/review: " & failed
        Finally
            Threading.Monitor.Exit(gate)
        End Try
    End Function

    Private Shared Sub SyncOne(id As String)
        Dim db = OfflineStore.Snapshot()
        Dim p = db.Pallets.First(Function(x) x.LocalId = id)
        ' Avoid uploading open pallets before the operator finishes packing.
        If Not p.Completed Then Return

        Dim containerId As Integer = p.ServerId
        If containerId = 0 Then
            ' Atomic SQL mapping by GUID makes retry safe after response loss.
            ' A pre-existing unrelated pallet code is rejected by SQL.
            containerId = OpenForActor(p)
            OfflineStore.UpdateSync(id, Sub(x)
                x.ServerId = containerId
                x.SyncStatus = "UPLOADING"
                x.SyncError = ""
            End Sub)
        End If

        ' Cached pallet layouts are authoritative for offline scans; if Setup
        ' changed before upload, block instead of misplacing any pieces.
        Dim initial = PaSPackingData.ReadContainer(containerId)
        If initial Is Nothing OrElse initial.PartId <> p.PartId OrElse initial.Code <> p.Code OrElse
           initial.Levels.Count <> p.Levels.Count OrElse
           Not initial.Levels.OrderBy(Function(x) x.LevelNumber).
               Select(Function(x) x.RequiredPieces).SequenceEqual(p.Levels) Then
            Throw New InvalidOperationException(
                "Pallet model or levels differ from offline snapshot. Manual review required.")
        End If

        For i As Integer = 0 To p.Pieces.Count - 1
            Dim piece = p.Pieces(i)
            If piece.Uploaded Then Continue For
            Dim current = PaSPackingData.ReadContainer(containerId)
            If current Is Nothing OrElse current.PartId <> p.PartId OrElse current.Code <> p.Code Then
                Throw New InvalidOperationException("Server pallet identity mismatch.")
            End If
            Dim identical = current.Pieces.Any(Function(x) x.LevelNumber = piece.LevelNumber AndAlso
                String.Equals(x.SsiCode, piece.SsiCode, StringComparison.Ordinal) AndAlso
                String.Equals(x.ClientCode, piece.ClientCode, StringComparison.Ordinal))
            If Not identical Then
                If current.Status <> "OPEN" Then
                    Throw New InvalidOperationException("Server pallet is no longer OPEN.")
                End If
                Dim lev = current.Levels.FirstOrDefault(Function(x) x.LevelNumber = piece.LevelNumber)
                If lev Is Nothing OrElse lev.Status <> "OPEN" Then
                    Throw New InvalidOperationException("Required server pallet level is not OPEN.")
                End If
                AddForActor(containerId, piece)
            End If
            Dim pieceIndex = i
            OfflineStore.UpdateSync(id, Sub(x)
                x.Pieces(pieceIndex).Uploaded = True
                x.SyncStatus = "UPLOADING"
                x.SyncError = ""
            End Sub)
        Next

        For levelNo As Integer = 1 To p.ClosedLevels
            Dim current = PaSPackingData.ReadContainer(containerId)
            Dim level = current.Levels.FirstOrDefault(Function(x) x.LevelNumber = levelNo)
            If level Is Nothing Then Throw New InvalidOperationException("Server level missing.")
            If level.ScannedPieces <> p.Levels(levelNo - 1) Then
                Throw New InvalidOperationException("Server level count differs from offline snapshot.")
            End If
            If level.Status = "OPEN" Then
                CloseLevelForActor(containerId, levelNo, p.ActorUserId)
            ElseIf level.Status <> "CLOSED" Then
                Throw New InvalidOperationException("Server level has unexpected status.")
            End If
        Next

        Dim completed = PaSPackingData.ReadContainer(containerId)
        If completed.Status = "OPEN" Then
            CloseForActor(containerId, p.ActorUserId)
        ElseIf completed.Status <> "CLOSED" Then
            Throw New InvalidOperationException("Server pallet is cancelled or invalid.")
        End If
        OfflineStore.UpdateSync(id, Sub(x)
            x.SyncStatus = "SYNCED"
            x.SyncError = ""
        End Sub)
    End Sub

    Private Shared Function OpenForActor(p As OfflinePallet) As Integer
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand("dbo.ssi_PaS_OfflineOpen", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@local_id", SqlDbType.Char, 32).Value = p.LocalId
                cmd.Parameters.Add("@container_code", SqlDbType.VarChar, 18).Value = p.Code
                cmd.Parameters.Add("@pn_id", SqlDbType.Int).Value = p.PartId
                cmd.Parameters.Add("@actor_user_id", SqlDbType.Int).Value = p.ActorUserId
                Return Convert.ToInt32(cmd.ExecuteScalar())
            End Using
        End Using
    End Function

    Private Shared Sub AddForActor(containerId As Integer, piece As OfflinePiece)
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand("dbo.ssi_PaS_AddPiece", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@container_id", SqlDbType.Int).Value = containerId
                cmd.Parameters.Add("@level_number", SqlDbType.Int).Value = piece.LevelNumber
                cmd.Parameters.Add("@ssi_full_code", SqlDbType.NVarChar, 200).Value = piece.SsiCode
                cmd.Parameters.Add("@client_full_code", SqlDbType.NVarChar, 200).Value = piece.ClientCode
                cmd.Parameters.Add("@actor_user_id", SqlDbType.Int).Value = piece.ActorUserId
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Private Shared Sub CloseLevelForActor(id As Integer, level As Integer, actor As Integer)
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand("dbo.ssi_PaS_CloseLevel", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@container_id", SqlDbType.Int).Value = id
                cmd.Parameters.Add("@level_number", SqlDbType.Int).Value = level
                cmd.Parameters.Add("@actor_user_id", SqlDbType.Int).Value = actor
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Private Shared Sub CloseForActor(id As Integer, actor As Integer)
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand("dbo.ssi_PaS_CloseContainer", cn)
                cmd.CommandType = CommandType.StoredProcedure
                cmd.Parameters.Add("@container_id", SqlDbType.Int).Value = id
                cmd.Parameters.Add("@actor_user_id", SqlDbType.Int).Value = actor
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub
End Class
