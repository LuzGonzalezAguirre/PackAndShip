Imports System.IO
Imports System.Linq
Imports System.Text.Json

' Crash-resistant local outbox. Each operation is written to disk before the
' UI reports it as saved. Does not claim Q-Wall PASS while disconnected.
' LocalApplicationData is scoped to the Windows account on this workstation.
Public NotInheritable Class OfflinePartSnapshot
    Public Property Part As New PaSPart()
    Public Property Configuration As New PalletConfiguration()
    Public Property Rule As New ScanRuleRecord()
    Public Property Fields As New List(Of ScanFieldRecord)()
End Class

Public NotInheritable Class OfflinePiece
    Public Property SsiCode As String = ""
    Public Property ClientCode As String = ""
    Public Property LevelNumber As Integer
    Public Property ActorUserId As Integer
    Public Property RecordedAtUtc As DateTime = DateTime.UtcNow
    Public Property Uploaded As Boolean
End Class

Public NotInheritable Class OfflinePallet
    Public Property LocalId As String = Guid.NewGuid().ToString("N")
    Public Property Code As String = ""
    Public Property PartId As Integer
    Public Property Model As String = ""
    Public Property ActorUserId As Integer
    Public Property Levels As New List(Of Integer)()
    Public Property Pieces As New List(Of OfflinePiece)()
    Public Property ClosedLevels As Integer
    Public Property Completed As Boolean
    Public Property CreatedAtUtc As DateTime = DateTime.UtcNow
    Public Property ServerId As Integer
    Public Property SyncStatus As String = "PENDING"
    Public Property SyncError As String = ""
End Class

Public NotInheritable Class OfflineDatabase
    Public Property Format As New PalletIdSettings()
    Public Property Models As New List(Of OfflinePartSnapshot)()
    Public Property Pallets As New List(Of OfflinePallet)()
End Class

Public NotInheritable Class OfflineStore
    Private Shared ReadOnly gate As New Object()
    Private Shared ReadOnly root As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                     "SSI", "PackAndShip")
    Private Shared ReadOnly filename As String = Path.Combine(root, "offline-queue.json")
    Private Sub New()
    End Sub

    Private Shared Function ReadUnsafe() As OfflineDatabase
        If Not File.Exists(filename) Then Return New OfflineDatabase()
        Dim data = JsonSerializer.Deserialize(Of OfflineDatabase)(File.ReadAllText(filename))
        If data Is Nothing Then Throw New InvalidDataException("Offline queue file cannot be read.")
        Return data
    End Function

    Private Shared Sub WriteUnsafe(data As OfflineDatabase)
        Directory.CreateDirectory(root)
        Dim tempFile = filename & ".tmp"
        Dim bytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data))
        Using stream As New FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None)
            stream.Write(bytes, 0, bytes.Length)
            stream.Flush(True)
        End Using
        ' Same-volume atomic rename: never overwrite good data with a partial file.
        If File.Exists(filename) Then
            File.Replace(tempFile, filename, Nothing)
        Else
            File.Move(tempFile, filename)
        End If
    End Sub

    Public Shared Function Snapshot() As OfflineDatabase
        SyncLock gate
            Return ReadUnsafe()
        End SyncLock
    End Function

    Public Shared Sub CacheFormat(value As PalletIdSettings)
        SyncLock gate
            Dim data = ReadUnsafe()
            data.Format = value
            WriteUnsafe(data)
        End SyncLock
    End Sub

    Public Shared Sub CacheModel(part As PaSPart, config As PalletConfiguration,
                                 rule As ScanRuleRecord, fields As List(Of ScanFieldRecord))
        If part Is Nothing OrElse config Is Nothing OrElse Not config.IsActive OrElse
           rule Is Nothing OrElse Not rule.IsActive OrElse fields Is Nothing OrElse
           fields.Count = 0 Then Return
        SyncLock gate
            Dim data = ReadUnsafe()
            data.Models.RemoveAll(Function(x) x.Part.Id = part.Id)
            data.Models.Add(New OfflinePartSnapshot With {
                .Part = part, .Configuration = config, .Rule = rule,
                .Fields = fields.ToList()
            })
            WriteUnsafe(data)
        End SyncLock
    End Sub

    ' Snapshot a live OPEN pallet after a successful SQL read. Allows an
    ' interrupted pallet to be completed offline without creating a new code.
    ' Never overwrite locally pending work with an older SQL snapshot.
    Public Shared Sub CaptureOnlineContainer(container As PaSPackedContainer,
                                             actorId As Integer)
        If container Is Nothing OrElse container.Code = "" OrElse actorId <= 0 Then Return
        SyncLock gate
            Dim db = ReadUnsafe()
            Dim known = db.Pallets.FirstOrDefault(
                Function(x) String.Equals(x.Code, container.Code, StringComparison.OrdinalIgnoreCase))
            If known IsNot Nothing Then
                If known.Pieces.Any(Function(x) Not x.Uploaded) OrElse
                   known.Completed AndAlso known.SyncStatus <> "SYNCED" Then
                    Return
                End If
            End If
            If container.Status = "CLOSED" Then
                If known IsNot Nothing AndAlso known.ServerId = container.Id Then
                    known.Completed = True
                    known.ClosedLevels = known.Levels.Count
                    known.SyncStatus = "SYNCED"
                    known.SyncError = ""
                    WriteUnsafe(db)
                End If
                Return
            End If
            If container.Status <> "OPEN" Then Return
            Dim levels = container.Levels.OrderBy(Function(x) x.LevelNumber).ToList()
            If levels.Count = 0 OrElse
               levels.Select(Function(x) x.LevelNumber).SequenceEqual(
                   Enumerable.Range(1, levels.Count)) = False Then Return
            Dim closed = levels.TakeWhile(Function(x) x.Status = "CLOSED").Count()
            If levels.Skip(closed).Any(Function(x) x.Status <> "OPEN") Then Return
            Dim p As OfflinePallet = known
            If p Is Nothing Then
                p = New OfflinePallet With {
                    .Code = container.Code,
                    .PartId = container.PartId,
                    .Model = db.Models.Where(Function(x) x.Part.Id = container.PartId).
                        Select(Function(x) x.Part.Number).FirstOrDefault(),
                    .ActorUserId = actorId
                }
                db.Pallets.Add(p)
            ElseIf p.ServerId <> 0 AndAlso p.ServerId <> container.Id Then
                Return
            End If
            p.ServerId = container.Id
            p.Levels = levels.Select(Function(x) x.RequiredPieces).ToList()
            p.ClosedLevels = closed
            p.Pieces = container.Pieces.Select(Function(x) New OfflinePiece With {
                .SsiCode = x.SsiCode, .ClientCode = x.ClientCode,
                .LevelNumber = x.LevelNumber, .ActorUserId = actorId,
                .RecordedAtUtc = x.ScannedAt, .Uploaded = True
            }).ToList()
            p.Completed = False
            p.SyncStatus = "PENDING"
            p.SyncError = ""
            WriteUnsafe(db)
        End SyncLock
    End Sub

    Public Shared Function CreatePallet(code As String, model As OfflinePartSnapshot,
                                        actor As Integer) As OfflinePallet
        If actor <= 0 Then Throw New UnauthorizedAccessException("Sign in before offline work.")
        SyncLock gate
            Dim data = ReadUnsafe()
            If Not data.Format.IsValid(code) Then Throw New InvalidOperationException("Invalid pallet ID.")
            If data.Pallets.Any(Function(p) String.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase)) Then
                Throw New InvalidOperationException("This pallet ID is already in the offline history.")
            End If
            If model.Configuration.Levels.Count = 0 OrElse
               model.Configuration.Levels.Any(Function(x) x <= 0) OrElse
               model.Configuration.Levels.Sum() <> model.Configuration.TotalPieces Then
                Throw New InvalidOperationException("Cached pallet configuration is inconsistent.")
            End If
            Dim p As New OfflinePallet With {
                .Code = code.Trim().ToUpperInvariant(),
                .PartId = model.Part.Id, .Model = model.Part.Number,
                .ActorUserId = actor, .Levels = model.Configuration.Levels.ToList()
            }
            data.Pallets.Add(p)
            WriteUnsafe(data)
            Return p
        End SyncLock
    End Function

    Private Shared Function RequirePallet(db As OfflineDatabase, id As String) As OfflinePallet
        Dim p = db.Pallets.FirstOrDefault(Function(x) x.LocalId = id)
        If p Is Nothing Then Throw New InvalidOperationException("Offline pallet was not found.")
        Return p
    End Function

    Public Shared Sub AddPiece(id As String, ssi As String, client As String, actor As Integer)
        SyncLock gate
            Dim db = ReadUnsafe()
            Dim p = RequirePallet(db, id)
            If p.Completed Then Throw New InvalidOperationException("Offline pallet is already complete.")
            If p.ClosedLevels >= p.Levels.Count Then Throw New InvalidOperationException("All levels are closed.")
            If db.Pallets.Any(Function(other) other.Pieces.Any(
                Function(piece) String.Equals(piece.SsiCode, ssi, StringComparison.OrdinalIgnoreCase) OrElse
                                String.Equals(piece.ClientCode, client, StringComparison.OrdinalIgnoreCase))) Then
                Throw New InvalidOperationException("This serial is already present in the local offline queue.")
            End If
            Dim level = p.ClosedLevels + 1
            If p.Pieces.Count(Function(x) x.LevelNumber = level) >= p.Levels(level - 1) Then
                Throw New InvalidOperationException("Close the full level before adding another piece.")
            End If
            p.Pieces.Add(New OfflinePiece With {
                .SsiCode = ssi, .ClientCode = client,
                .LevelNumber = level, .ActorUserId = actor
            })
            p.SyncStatus = "PENDING"
            WriteUnsafe(db)
        End SyncLock
    End Sub

    Public Shared Sub CloseLevel(id As String)
        SyncLock gate
            Dim db = ReadUnsafe()
            Dim p = RequirePallet(db, id)
            If p.Completed OrElse p.ClosedLevels >= p.Levels.Count Then
                Throw New InvalidOperationException("There is no open level.")
            End If
            Dim level = p.ClosedLevels + 1
            If p.Pieces.Count(Function(x) x.LevelNumber = level) <> p.Levels(level - 1) Then
                Throw New InvalidOperationException("Level must have its exact required quantity.")
            End If
            p.ClosedLevels += 1
            WriteUnsafe(db)
        End SyncLock
    End Sub

    Public Shared Sub Complete(id As String)
        SyncLock gate
            Dim db = ReadUnsafe()
            Dim p = RequirePallet(db, id)
            If p.Completed OrElse p.ClosedLevels <> p.Levels.Count OrElse
               p.Pieces.Count <> p.Levels.Sum() Then
                Throw New InvalidOperationException("All levels must be complete and closed first.")
            End If
            p.Completed = True
            WriteUnsafe(db)
        End SyncLock
    End Sub

    Public Shared Sub UpdateSync(id As String, apply As Action(Of OfflinePallet))
        SyncLock gate
            Dim db = ReadUnsafe()
            Dim p = RequirePallet(db, id)
            apply(p)
            WriteUnsafe(db)
        End SyncLock
    End Sub
End Class
