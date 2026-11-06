Imports System.Drawing
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms

' Dedicated offline workbench. Unlike live packing it never claims Q-Wall PASS.
' The model and format rules were cached from CCS during a connected session.
Public Class OfflinePackingForm
    Inherits Form

    Private ReadOnly modelPicker As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 240}
    Private ReadOnly containerInput As New TextBox With {.Width = 125}
    Private ReadOnly palletPicker As New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Width = 230}
    Private ReadOnly firstQr As New TextBox With {.Width = 460}
    Private ReadOnly secondQr As New TextBox With {.Width = 460}
    Private ReadOnly scanHelp As New Label With {.AutoSize = True}
    Private ReadOnly status As New Label With {.AutoSize = True, .ForeColor = Color.DarkRed}
    Private ReadOnly pieceList As New ListBox With {.Dock = DockStyle.Fill}
    Private ReadOnly queueList As New ListBox With {.Dock = DockStyle.Fill}
    Private ReadOnly progress As New Label With {.AutoSize = True}
    Private ReadOnly syncButton As New Button With {.Text = "Sync now", .AutoSize = True}
    Private ReadOnly timer As New Timer With {.Interval = 30000}
    Private ReadOnly cached As OfflineDatabase = OfflineStore.Snapshot()
    Private active As OfflinePallet
    Private syncing As Boolean

    Public Sub New()
        Text = "PackAndShip | Offline Packing"
        Size = New Size(1120, 780)
        MinimumSize = New Size(850, 600)
        StartPosition = FormStartPosition.CenterParent
        Font = New Font("Segoe UI", 10)
        BackColor = PaSTheme.Background
        BuildLayout()
        AddHandler Shown, Sub()
            ReloadQueue()
            timer.Start()
        End Sub
        AddHandler timer.Tick, Async Sub() Await SynchronizeAsync(False)
        AddHandler FormClosed, Sub() timer.Stop()
    End Sub

    Private Shared Function LabelOf(text As String) As Label
        Return New Label With {.Text = text, .AutoSize = True, .Margin = New Padding(7, 12, 4, 3)}
    End Function

    Private Sub BuildLayout()
        Dim toolbar As New FlowLayoutPanel With {
            .Dock = DockStyle.Top, .Height = 74, .WrapContents = True,
            .Padding = New Padding(10), .AutoScroll = True
        }
        toolbar.Controls.Add(LabelOf("Model"))
        Dim modelChoices = cached.Models.Where(Function(x) x.Part.BusinessUnitId.HasValue AndAlso
            x.Part.BusinessUnitId.Value = SessionContext.BusinessUnitId).
            OrderBy(Function(x) x.Part.Number).ToList()
        modelPicker.DataSource = modelChoices
        modelPicker.DisplayMember = "Model"
        AddHandler modelPicker.Format, Sub(sender, e)
            Dim item = TryCast(e.ListItem, OfflinePartSnapshot)
            If item IsNot Nothing Then e.Value = item.Part.DisplayName
        End Sub
        toolbar.Controls.Add(modelPicker)
        toolbar.Controls.Add(LabelOf("New pallet"))
        toolbar.Controls.Add(containerInput)
        Dim create As New Button With {.Text = "Create offline pallet", .AutoSize = True}
        AddHandler create.Click, AddressOf CreatePallet
        toolbar.Controls.Add(create)
        toolbar.Controls.Add(LabelOf("Select pallet"))
        toolbar.Controls.Add(palletPicker)
        AddHandler palletPicker.SelectedIndexChanged, Sub()
            active = TryCast(palletPicker.SelectedItem, OfflinePallet)
            RefreshActive()
        End Sub

        Dim scanBar As New FlowLayoutPanel With {
            .Dock = DockStyle.Top, .Height = 160, .WrapContents = True,
            .Padding = New Padding(10), .AutoScroll = True
        }
        scanBar.Controls.Add(LabelOf("SSI QR"))
        scanBar.Controls.Add(firstQr)
        scanBar.SetFlowBreak(firstQr, True)
        scanBar.Controls.Add(LabelOf("Client QR"))
        scanBar.Controls.Add(secondQr)
        scanBar.SetFlowBreak(secondQr, True)
        Dim add As New Button With {.Text = "Save piece locally", .AutoSize = True}
        AddHandler add.Click, AddressOf SavePiece
        scanBar.Controls.Add(add)
        scanBar.Controls.Add(scanHelp)
        AddHandler firstQr.KeyDown, Sub(sender, e)
            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True
                Dim m = CurrentModel()
                If m IsNot Nothing AndAlso m.Rule.ScanCount = 1 Then
                    SavePiece(sender, EventArgs.Empty)
                Else
                    secondQr.Focus()
                End If
            End If
        End Sub
        AddHandler secondQr.KeyDown, Sub(sender, e)
            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True
                SavePiece(sender, EventArgs.Empty)
            End If
        End Sub

        Dim actionBar As New FlowLayoutPanel With {
            .Dock = DockStyle.Top, .Height = 78, .Padding = New Padding(10)
        }
        Dim closeLevel As New Button With {.Text = "Close level locally", .AutoSize = True}
        Dim complete As New Button With {.Text = "Complete pallet locally", .AutoSize = True}
        AddHandler closeLevel.Click, Sub() ApplyLocal(Sub() OfflineStore.CloseLevel(active.LocalId))
        AddHandler complete.Click, Sub() ApplyLocal(Sub() OfflineStore.Complete(active.LocalId))
        AddHandler syncButton.Click, Async Sub() Await SynchronizeAsync(True)
        actionBar.Controls.Add(closeLevel)
        actionBar.Controls.Add(complete)
        actionBar.Controls.Add(syncButton)
        actionBar.Controls.Add(progress)

        Dim columns As New SplitContainer With {
            .Dock = DockStyle.Fill,
            .SplitterDistance = 520
        }
        Dim left As New GroupBox With {.Text = "Pieces saved on this workstation", .Dock = DockStyle.Fill}
        left.Controls.Add(pieceList)
        Dim right As New GroupBox With {.Text = "Offline queue / synchronization", .Dock = DockStyle.Fill}
        right.Controls.Add(queueList)
        columns.Panel1.Controls.Add(left)
        columns.Panel2.Controls.Add(right)

        Dim footer As New Panel With {.Dock = DockStyle.Bottom, .Height = 76}
        status.Location = New Point(14, 9)
        status.MaximumSize = New Size(1040, 60)
        status.Text = "OFFLINE: labels are format-checked only. Q-Wall PASS is checked on SQL upload."
        footer.Controls.Add(status)

        Controls.Add(columns)
        Controls.Add(footer)
        Controls.Add(actionBar)
        Controls.Add(scanBar)
        Controls.Add(toolbar)
    End Sub

    Private Function CurrentModel() As OfflinePartSnapshot
        If active Is Nothing Then Return TryCast(modelPicker.SelectedItem, OfflinePartSnapshot)
        Return cached.Models.FirstOrDefault(Function(x) x.Part.Id = active.PartId)
    End Function

    Private Sub CreatePallet(sender As Object, e As EventArgs)
        Try
            Dim m = TryCast(modelPicker.SelectedItem, OfflinePartSnapshot)
            If m Is Nothing Then Throw New InvalidOperationException(
                "No cached model. Connect to CCS once and select the model in the main scanner first.")
            Dim code = containerInput.Text.Trim().ToUpperInvariant()
            ' When connected, detect collision before making a new local pallet.
            ' Offline mode never assumes SQL availability.
            Dim created = OfflineStore.CreatePallet(code, m, SessionContext.UserId)
            ReloadQueue(created.LocalId)
            status.Text = "Offline pallet created locally. NOT Q-Wall verified."
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    Private Sub SavePiece(sender As Object, e As EventArgs)
        Try
            If active Is Nothing Then Throw New InvalidOperationException("Select or create an offline pallet.")
            Dim m = CurrentModel()
            If m Is Nothing Then Throw New InvalidOperationException("Cached scan rule is missing.")
            Dim result = ScanFormatValidator.CheckScans(firstQr.Text, secondQr.Text,
                                                        m.Rule, m.Fields, m.Part)
            If Not result.Valid Then Throw New InvalidOperationException(result.Message)
            Dim client = If(m.Rule.ScanCount = 1, result.GeneratedClientCode, secondQr.Text.Trim())
            OfflineStore.AddPiece(active.LocalId, firstQr.Text.Trim(), client, SessionContext.UserId)
            firstQr.Clear()
            secondQr.Clear()
            ReloadQueue(active.LocalId)
            status.Text = "Piece saved LOCALLY — Q-Wall approval and duplicate check pending upload."
            firstQr.Focus()
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    Private Sub ApplyLocal(op As Action)
        Try
            If active Is Nothing Then Throw New InvalidOperationException("Select an offline pallet.")
            Dim selected = active.LocalId
            op()
            ReloadQueue(selected)
            status.Text = "Saved locally; pending server validation."
        Catch ex As Exception
            ShowError(ex)
        End Try
    End Sub

    Private Sub ReloadQueue(Optional selectedId As String = Nothing)
        If selectedId Is Nothing AndAlso active IsNot Nothing Then selectedId = active.LocalId
        Dim data = OfflineStore.Snapshot()
        Dim models = data.Pallets.OrderByDescending(Function(p) p.CreatedAtUtc).ToList()
        palletPicker.DataSource = Nothing
        palletPicker.DataSource = models
        palletPicker.DisplayMember = "Code"
        If selectedId IsNot Nothing Then
            For index As Integer = 0 To models.Count - 1
                If models(index).LocalId = selectedId Then
                    palletPicker.SelectedIndex = index
                    Exit For
                End If
            Next
        End If
        active = TryCast(palletPicker.SelectedItem, OfflinePallet)
        RefreshActive()
        queueList.Items.Clear()
        For Each p In models
            queueList.Items.Add(p.Code & " | " & p.Model & " | " &
                p.Pieces.Count & "/" & p.Levels.Sum() & " | " &
                If(p.Completed, "COMPLETE", "IN PROGRESS") & " | " & p.SyncStatus)
            If p.SyncError <> "" Then queueList.Items.Add("  REVIEW: " & p.SyncError)
        Next
    End Sub

    Private Sub RefreshActive()
        pieceList.Items.Clear()
        If active Is Nothing Then Return
        Dim m = CurrentModel()
        If m IsNot Nothing Then
            scanHelp.Text = If(m.Rule.ScanCount = 1, "One SSI scan; Client QR generated.",
                "Two label scans required.")
            secondQr.ReadOnly = m.Rule.ScanCount = 1
        End If
        For Each piece In active.Pieces
            pieceList.Items.Add("L" & piece.LevelNumber & " | " & piece.SsiCode &
                " | " & If(piece.Uploaded, "UPLOADED", "LOCAL"))
        Next
        progress.Text = active.Pieces.Count & "/" & active.Levels.Sum() &
            " pieces; " & active.ClosedLevels & "/" & active.Levels.Count & " levels closed"
    End Sub

    Private Async Function SynchronizeAsync(showMessage As Boolean) As Task
        If syncing Then Return
        syncing = True
        syncButton.Enabled = False
        Try
            Dim result = Await Task.Run(Function() OfflineSynchronizer.SyncAll())
            ReloadQueue()
            If showMessage Then MessageBox.Show(result, "Offline synchronization")
        Catch ex As Exception
            If showMessage Then ShowError(ex)
        Finally
            syncButton.Enabled = True
            syncing = False
        End Try
    End Function

    Private Sub ShowError(ex As Exception)
        status.ForeColor = Color.DarkRed
        status.Text = ex.Message
        MessageBox.Show(ex.Message, "Offline packing", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub
End Class
