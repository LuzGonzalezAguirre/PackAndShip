Imports System.Data
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports Microsoft.Data.SqlClient

' Exactly one last database operation can be pending at a time.
' We never queue offline pallets or bypass Q-Wall verification.
Public NotInheritable Class PendingPackingOperation
    Public Property Id As String = Guid.NewGuid().ToString("N")
    Public Property Kind As String = ""
    Public Property ContainerId As Integer
    Public Property LevelNumber As Integer
    Public Property Ssi As String = ""
    Public Property Client As String = ""
    Public Property ActorUserId As Integer
    Public Property CreatedUtc As DateTime = DateTime.UtcNow
    Public Property ReviewRequired As Boolean
    Public Property LastError As String = ""
End Class

Public NotInheritable Class ConnectionRecovery
    Private Shared ReadOnly gate As New Object()
    Private Shared ReadOnly folder As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "SSI", "PackAndShip")
    Private Shared ReadOnly filePath As String = Path.Combine(folder, "pending-operation.json")

    Private Sub New()
    End Sub

    Private Shared Function ReadPending() As PendingPackingOperation
        If Not File.Exists(filePath) Then Return Nothing
        Dim result = JsonSerializer.Deserialize(Of PendingPackingOperation)(
            File.ReadAllText(filePath, Encoding.UTF8))
        If result Is Nothing OrElse String.IsNullOrWhiteSpace(result.Id) OrElse
           result.ContainerId <= 0 OrElse result.ActorUserId <= 0 OrElse
           Not (result.Kind = "ADD_PIECE" OrElse result.Kind = "CLOSE_LEVEL" OrElse
                result.Kind = "CLOSE_PALLET" OrElse result.Kind = "CLOSE_PARTIAL" OrElse
                result.Kind = "CANCEL_EMPTY") Then
            Throw New InvalidDataException("Pending database operation is corrupt. Do not remove it; contact IT.")
        End If
        Return result
    End Function

    Private Shared Sub SavePending(value As PendingPackingOperation)
        Directory.CreateDirectory(folder)
        Dim tmp = filePath & ".tmp"
        Dim bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value))
        Using stream As New FileStream(tmp, FileMode.Create, FileAccess.Write, FileShare.None)
            stream.Write(bytes, 0, bytes.Length)
            stream.Flush(True)
        End Using
        If File.Exists(filePath) Then
            File.Replace(tmp, filePath, Nothing)
        Else
            File.Move(tmp, filePath)
        End If
    End Sub

    Public Shared Function Pending() As PendingPackingOperation
        SyncLock gate
            Return ReadPending()
        End SyncLock
    End Function

    Public Shared Sub SaveBeforeSend(kind As String, container As Integer, level As Integer,
                                     ssi As String, client As String, actor As Integer)
        SyncLock gate
            If ReadPending() IsNot Nothing Then
                Throw New InvalidOperationException(
                    "A previous SQL operation is pending. Wait for reconnection and review its result.")
            End If
            SavePending(New PendingPackingOperation With {
                .Kind = kind, .ContainerId = container, .LevelNumber = level,
                .Ssi = If(ssi, ""), .Client = If(client, ""), .ActorUserId = actor
            })
        End SyncLock
    End Sub

    Public Shared Sub MarkDelivered()
        SyncLock gate
            If File.Exists(filePath) Then File.Delete(filePath)
        End SyncLock
    End Sub

    Public Shared Sub FlagReview(message As String)
        SyncLock gate
            Dim p = ReadPending()
            If p Is Nothing Then Return
            p.ReviewRequired = True
            p.LastError = message
            SavePending(p)
        End SyncLock
    End Sub

    ' Reach CCS and determine whether the last operation committed.
    ' When the SQL response was lost, first compare exact server state.
    ' Only retry if there is proof that the operation is still absent.
    Public Shared Function CheckAndRecover() As String
        SyncLock gate
            Dim p = ReadPending()
            Using cn = PackAndShipData.CreateConnection()
                If p Is Nothing Then Return "CONNECTED"
                If p.ReviewRequired Then
                    ' A business rejection never auto-replays. However, a dropped
                    ' acknowledgement can leave an operation flagged REVIEW after
                    ' the server actually committed it. Check for that exact
                    ' server-side result before keeping the scanner blocked.
                    Dim confirmed = IsPendingCommitted(p)
                    If confirmed Then
                        MarkDelivered()
                        Return "RECOVERED: server already confirmed " & p.Kind
                    End If
                    Return "REVIEW REQUIRED: " & p.LastError
                End If
                Dim actual = PaSPackingData.ReadContainer(p.ContainerId)
                If actual Is Nothing Then
                    FlagReview("Original pallet is missing in CCS.")
                    Return "REVIEW REQUIRED: pallet not found"
                End If
                If p.Kind = "ADD_PIECE" Then
                    Dim exists = actual.Pieces.Any(Function(x) x.LevelNumber = p.LevelNumber AndAlso
                        String.Equals(x.SsiCode, p.Ssi, StringComparison.Ordinal) AndAlso
                        String.Equals(x.ClientCode, p.Client, StringComparison.Ordinal))
                    If Not exists Then
                        If actual.Status <> "OPEN" Then
                            FlagReview("Pallet is no longer OPEN.")
                            Return "REVIEW REQUIRED: pallet not OPEN"
                        End If
                        Dim level = actual.Levels.FirstOrDefault(Function(x) x.LevelNumber = p.LevelNumber)
                        If level Is Nothing OrElse level.Status <> "OPEN" Then
                            FlagReview("Level is no longer OPEN.")
                            Return "REVIEW REQUIRED: level not OPEN"
                        End If
                        ExecutePending(p, cn)
                    End If
                ElseIf p.Kind = "CLOSE_LEVEL" Then
                    Dim level = actual.Levels.FirstOrDefault(Function(x) x.LevelNumber = p.LevelNumber)
                    If level Is Nothing Then
                        FlagReview("Level is missing in CCS.")
                        Return "REVIEW REQUIRED: level missing"
                    End If
                    If level.Status <> "CLOSED" Then
                        ExecutePending(p, cn)
                    End If
                ElseIf p.Kind = "CLOSE_PALLET" Then
                    If actual.Status <> "CLOSED" Then ExecutePending(p, cn)
                ElseIf p.Kind = "CANCEL_EMPTY" Then
                    ' No authorizer credentials are saved. Check whether CCS
                    ' committed cancellation; otherwise require fresh approval.
                    If actual.Status = "OPEN" Then
                        MarkDelivered()
                        Return "NOT CANCELLED: pallet is still OPEN. Reauthorize cancellation."
                    ElseIf actual.Status <> "CANCELLED" Then
                        FlagReview("Cancellation has unexpected SQL status " & actual.Status)
                        Return "REVIEW REQUIRED: cancellation status mismatch"
                    End If
                ElseIf p.Kind = "CLOSE_PARTIAL" Then
                    ' Authorizer password is never written to disk. Do not replay
                    ' a privileged operation without a fresh second approval.
                    If actual.Status = "OPEN" Then
                        MarkDelivered()
                        Return "NOT CLOSED: partial pallet was not confirmed in SQL. Reauthorize closure."
                    ElseIf actual.Status <> "PARTIAL" Then
                        FlagReview("Partial closure has unexpected SQL status " & actual.Status)
                        Return "REVIEW REQUIRED: unexpected partial closure status"
                    End If
                End If
                MarkDelivered()
                Return "RECOVERED: " & p.Kind & " for " & actual.Code
            End Using
        End SyncLock
    End Function

    Public Shared Function DescribePending() As String
        SyncLock gate
            Dim p = ReadPending()
            If p Is Nothing Then Return "No pending SQL operation."
            Dim detail = "Pending SQL operation: " & p.Kind &
                Environment.NewLine & "Container ID: " & p.ContainerId &
                If(p.LevelNumber > 0, Environment.NewLine & "Level: " & p.LevelNumber, "")
            If p.Kind = "ADD_PIECE" Then
                detail &= Environment.NewLine & "SSI: " & p.Ssi
            End If
            If p.ReviewRequired Then
                detail &= Environment.NewLine & "Review reason: " & p.LastError
            End If
            Return detail
        End SyncLock
    End Function

    Private Shared Function IsPendingCommitted(p As PendingPackingOperation) As Boolean
        Dim state = PaSPackingData.ReadContainer(p.ContainerId)
        If state Is Nothing Then Return False
        Select Case p.Kind
            Case "ADD_PIECE"
                Return state.Pieces.Any(Function(x) x.LevelNumber = p.LevelNumber AndAlso
                    String.Equals(x.SsiCode, p.Ssi, StringComparison.Ordinal) AndAlso
                    String.Equals(x.ClientCode, p.Client, StringComparison.Ordinal))
            Case "CLOSE_LEVEL"
                Return state.Levels.Any(Function(x) x.LevelNumber = p.LevelNumber AndAlso
                    x.Status = "CLOSED")
            Case "CLOSE_PALLET"
                Return state.Status = "CLOSED"
            Case "CLOSE_PARTIAL"
                Return state.Status = "PARTIAL"
            Case "CANCEL_EMPTY"
                Return state.Status = "CANCELLED"
        End Select
        Return False
    End Function

    ' Explicit dismissal is only allowed for a SQL-rejected operation, after
    ' a fresh server-side read proves it did not commit. Never discard an
    ' ambiguous timeout or any operation while CCS is unavailable.
    Public Shared Sub DismissRejectedAfterCheck()
        SyncLock gate
            Dim p = ReadPending()
            If p Is Nothing Then Return
            If Not p.ReviewRequired Then
                Throw New InvalidOperationException(
                    "Pending operation may still be valid. Reconcile it with CCS first.")
            End If
            If IsPendingCommitted(p) Then
                MarkDelivered()
                Return
            End If
            ' A stored procedure rejection is transactionally rolled back.
            ' Requiring explicit acknowledgement prevents silent data loss.
            MarkDelivered()
        End SyncLock
    End Sub

    Private Shared Sub ExecutePending(p As PendingPackingOperation, cn As SqlConnection)
        Dim proc As String
        Select Case p.Kind
            Case "ADD_PIECE"
                proc = "dbo.ssi_PaS_AddPiece"
            Case "CLOSE_LEVEL"
                proc = "dbo.ssi_PaS_CloseLevel"
            Case "CLOSE_PALLET"
                proc = "dbo.ssi_PaS_CloseContainer"
            Case Else
                Throw New InvalidOperationException("Unknown pending action.")
        End Select
        Using cmd As New SqlCommand(proc, cn)
            cmd.CommandType = CommandType.StoredProcedure
            cmd.Parameters.Add("@container_id", SqlDbType.Int).Value = p.ContainerId
            cmd.Parameters.Add("@actor_user_id", SqlDbType.Int).Value = p.ActorUserId
            If p.Kind <> "CLOSE_PALLET" Then
                cmd.Parameters.Add("@level_number", SqlDbType.Int).Value = p.LevelNumber
            End If
            If p.Kind = "ADD_PIECE" Then
                cmd.Parameters.Add("@ssi_full_code", SqlDbType.NVarChar, 200).Value = p.Ssi
                cmd.Parameters.Add("@client_full_code", SqlDbType.NVarChar, 200).Value = p.Client
            End If
            cmd.ExecuteNonQuery()
        End Using
    End Sub
End Class
