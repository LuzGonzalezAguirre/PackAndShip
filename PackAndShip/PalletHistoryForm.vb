Imports System.Data
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms

' Pallet history, detailed traceability and CSV reports.
Public Class PalletHistoryForm
    Inherits Form

    Private ReadOnly searchCode As New TextBox With {.Width = 150}
    Private ReadOnly searchModel As New TextBox With {.Width = 130}
    Private ReadOnly statusPicker As New ComboBox With {.Width = 115, .DropDownStyle = ComboBoxStyle.DropDownList}
    Private ReadOnly fromPicker As New DateTimePicker With {.Width = 135, .Format = DateTimePickerFormat.Short}
    Private ReadOnly toPicker As New DateTimePicker With {.Width = 135, .Format = DateTimePickerFormat.Short}
    Private ReadOnly grid As New DataGridView()
    Private ReadOnly pieces As New DataGridView()
    Private ReadOnly levels As New DataGridView()
    Private ReadOnly audit As New DataGridView()
    Private ReadOnly packingReport As New DataGridView()
    Private ReadOnly transfers As New DataGridView()
    Private ReadOnly summary As New Label With {.AutoSize = True}
    Private ReadOnly removeButton As New Button With {.Text = "Remove selected piece", .AutoSize = True}
    Private ReadOnly cancelButton As New Button With {.Text = "Cancel pallet", .AutoSize = True}
    Private ReadOnly tabs As New TabControl With {.Dock = DockStyle.Fill}
    Private selectedId As Integer

    Public Sub New()
        Text = "PackAndShip | Pallet History"
        Size = New Size(1250, 800)
        MinimumSize = New Size(900, 600)
        StartPosition = FormStartPosition.CenterParent
        BackColor = PaSTheme.Background
        Font = New Font("Segoe UI", 10)
        BuildLayout()
        AddHandler Shown, Sub() RefreshHistory()
    End Sub

    Private Shared Sub ConfigureGrid(g As DataGridView)
        g.Dock = DockStyle.Fill
        g.ReadOnly = True
        g.AllowUserToAddRows = False
        g.AllowUserToDeleteRows = False
        g.MultiSelect = False
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
        g.BackgroundColor = Color.White
        g.RowHeadersVisible = False
    End Sub

    Private Sub BuildLayout()
        statusPicker.Items.AddRange(New Object() {"All", "OPEN", "CLOSED", "PARTIAL", "TRANSFERRED", "CANCELLED"})
        statusPicker.SelectedIndex = 0
        fromPicker.Value = DateTime.Today.AddMonths(-3)
        toPicker.Value = DateTime.Today

        Dim filters As New FlowLayoutPanel With {
            .Dock = DockStyle.Top, .Height = 90,
            .Padding = New Padding(10), .AutoScroll = True,
            .WrapContents = True
        }
        filters.Controls.Add(New Label With {.Text = "Pallet", .AutoSize = True, .Margin = New Padding(5, 11, 3, 3)})
        filters.Controls.Add(searchCode)
        filters.Controls.Add(New Label With {.Text = "Model", .AutoSize = True, .Margin = New Padding(9, 11, 3, 3)})
        filters.Controls.Add(searchModel)
        filters.Controls.Add(New Label With {.Text = "Status", .AutoSize = True, .Margin = New Padding(9, 11, 3, 3)})
        filters.Controls.Add(statusPicker)
        filters.Controls.Add(New Label With {.Text = "From", .AutoSize = True, .Margin = New Padding(9, 11, 3, 3)})
        filters.Controls.Add(fromPicker)
        filters.Controls.Add(New Label With {.Text = "To", .AutoSize = True, .Margin = New Padding(9, 11, 3, 3)})
        filters.Controls.Add(toPicker)
        Dim refresh As New Button With {.Text = "Search", .AutoSize = True}
        AddHandler refresh.Click, Sub() RefreshHistory()
        filters.Controls.Add(refresh)
        Dim exportSummary As New Button With {.Text = "Export pallet report CSV", .AutoSize = True}
        AddHandler exportSummary.Click, Sub() ExportGrid(grid, "pallet_report")
        filters.Controls.Add(exportSummary)

        Dim actions As New FlowLayoutPanel With {
            .Dock = DockStyle.Top, .Height = 48, .Padding = New Padding(9)
        }
        actions.Controls.Add(summary)
        actions.Controls.Add(removeButton)
        actions.Controls.Add(cancelButton)
        AddHandler removeButton.Click, AddressOf RemoveSelectedPiece
        AddHandler cancelButton.Click, AddressOf CancelSelectedPallet
        removeButton.Visible = SessionContext.CanModifySetup
        cancelButton.Visible = SessionContext.UserId > 0

        ConfigureGrid(grid)
        ConfigureGrid(pieces)
        ConfigureGrid(levels)
        ConfigureGrid(audit)
        ConfigureGrid(packingReport)
        ConfigureGrid(transfers)
        AddHandler grid.SelectionChanged, AddressOf SelectedPalletChanged

        Dim upper As New GroupBox With {.Text = "Pallets", .Dock = DockStyle.Fill, .Padding = New Padding(8)}
        upper.Controls.Add(grid)

        Dim names = New String() {"Pieces and Q-Wall Inspection IDs", "Levels", "Audit history", "Packing report by model/operator/day", "Pallet transfers"}
        Dim detailGrids = New DataGridView() {pieces, levels, audit, packingReport, transfers}
        For index As Integer = 0 To names.Length - 1
            Dim tab As New TabPage(names(index))
            Dim target As DataGridView = detailGrids(index)
            tab.Controls.Add(target)
            Dim bar As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 40}
            Dim export As New Button With {.Text = "Export selected tab CSV", .AutoSize = True}
            Dim filename As String = names(index).Replace(" ", "_")
            AddHandler export.Click, Sub() ExportGrid(target, filename)
            bar.Controls.Add(export)
            tab.Controls.Add(bar)
            tabs.TabPages.Add(tab)
        Next

        Dim split As New SplitContainer With {
            .Dock = DockStyle.Fill, .Orientation = Orientation.Horizontal,
            .SplitterDistance = 275, .Panel1MinSize = 150, .Panel2MinSize = 150
        }
        split.Panel1.Controls.Add(upper)
        split.Panel2.Controls.Add(tabs)
        Controls.Add(split)
        Controls.Add(actions)
        Controls.Add(filters)
    End Sub

    Private Sub RefreshHistory()
        Try
            If fromPicker.Value.Date > toPicker.Value.Date Then
                Throw New InvalidOperationException("From date must be before To date.")
            End If
            Dim status = If(statusPicker.SelectedIndex <= 0, "", statusPicker.Text)
            packingReport.DataSource = PaSHistoryData.PackingReport(fromPicker.Value, toPicker.Value)
            transfers.DataSource = PaSHistoryData.ReadTransfers()
            Dim data = PaSHistoryData.SearchContainers(
                searchCode.Text, searchModel.Text, status, fromPicker.Value, toPicker.Value)
            grid.DataSource = data
            If grid.Columns.Contains("ContainerId") Then grid.Columns("ContainerId").Visible = False
            Dim packed As Long = 0
            For Each r As DataRow In data.Rows
                packed += Convert.ToInt64(r("Packed"))
            Next
            summary.Text = "Pallets: " & data.Rows.Count & "  |  Current packed pieces: " & packed & "     "
            If grid.Rows.Count = 0 Then
                selectedId = 0
                pieces.DataSource = Nothing
                levels.DataSource = Nothing
                audit.DataSource = Nothing
            Else
                SelectedPalletChanged(Nothing, EventArgs.Empty)
            End If
            RefreshActions()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "History loading error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub SelectedPalletChanged(sender As Object, e As EventArgs)
        If grid.SelectedRows.Count = 0 OrElse grid.SelectedRows(0).Cells("ContainerId").Value Is Nothing Then
            Return
        End If
        Try
            selectedId = Convert.ToInt32(grid.SelectedRows(0).Cells("ContainerId").Value)
            pieces.DataSource = PaSHistoryData.ReadPieces(selectedId)
            levels.DataSource = PaSHistoryData.ReadLevels(selectedId)
            audit.DataSource = PaSHistoryData.ReadAudit(selectedId)
            RefreshActions()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Pallet details error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RefreshActions()
        Dim authorized = SessionContext.UserId > 0 AndAlso SessionContext.CanModifySetup
        Dim openState = grid.SelectedRows.Count > 0 AndAlso
            Convert.ToString(grid.SelectedRows(0).Cells("Status").Value) = "OPEN"
        removeButton.Enabled = authorized AndAlso openState AndAlso pieces.Rows.Count > 0
        cancelButton.Enabled = SessionContext.UserId > 0 AndAlso openState AndAlso
            pieces.Rows.Count = 0 AndAlso
            Not levels.Rows.Cast(Of DataGridViewRow)().Any(
                Function(row) Convert.ToString(row.Cells("Status").Value) = "CLOSED")
    End Sub

    Private Function ReasonDialog(action As String) As String
        Using dialog As New Form With {
            .Text = action & " | Reason required", .Size = New Size(550, 260),
            .FormBorderStyle = FormBorderStyle.FixedDialog, .StartPosition = FormStartPosition.CenterParent,
            .MinimizeBox = False, .MaximizeBox = False
        }
            Dim caption As New Label With {
                .Text = "Reason (at least 10 characters). Saved permanently in audit.",
                .Dock = DockStyle.Top, .Height = 40
            }
            Dim reason As New TextBox With {.Multiline = True, .Dock = DockStyle.Fill, .MaxLength = 500}
            Dim footer As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 52, .FlowDirection = FlowDirection.RightToLeft}
            Dim ok As New Button With {.Text = "Confirm", .DialogResult = DialogResult.OK}
            Dim cancel As New Button With {.Text = "Back", .DialogResult = DialogResult.Cancel}
            footer.Controls.Add(ok)
            footer.Controls.Add(cancel)
            dialog.Controls.Add(reason)
            dialog.Controls.Add(footer)
            dialog.Controls.Add(caption)
            dialog.AcceptButton = ok
            dialog.CancelButton = cancel
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return Nothing
            If reason.Text.Trim().Length < 10 Then
                MessageBox.Show("Reason must be at least 10 characters.", "Required")
                Return Nothing
            End If
            Return reason.Text.Trim()
        End Using
    End Function

    Private Sub RemoveSelectedPiece(sender As Object, e As EventArgs)
        If selectedId <= 0 OrElse pieces.SelectedRows.Count = 0 Then Return
        Dim productId = Convert.ToInt32(pieces.SelectedRows(0).Cells("ProductId").Value)
        Dim reason = ReasonDialog("Remove product " & productId)
        If reason Is Nothing Then Return
        If MessageBox.Show("Remove product " & productId & " from this OPEN level? " &
                           "The action will be audited.", "Confirm correction",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
        Try
            PaSHistoryData.RemovePiece(selectedId, productId, reason)
            RefreshHistory()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Correction blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub CancelSelectedPallet(sender As Object, e As EventArgs)
        If selectedId <= 0 OrElse Not cancelButton.Enabled OrElse
           grid.SelectedRows.Count = 0 Then Return
        Dim code = Convert.ToString(grid.SelectedRows(0).Cells("Pallet").Value)
        Dim required = Convert.ToInt32(grid.SelectedRows(0).Cells("Required").Value)
        Using auth As New PartialPalletAuthorizationForm(code, 0, required, True)
            If auth.ShowDialog(Me) <> DialogResult.OK Then Return
            If MessageBox.Show(
                "Cancel empty pallet " & code & "? The code cannot be reused.",
                "Confirm cancellation", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
            Try
                PaSPackingData.CancelEmptyPallet(
                    selectedId, auth.AuthorizedUserId, auth.AuthorizedBarcode,
                    auth.AuthorizedPassword, auth.Reason)
                RefreshHistory()
            Catch ex As Exception
                MessageBox.Show(ex.Message, "Cancellation blocked",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Using
    End Sub

    Private Shared Function CsvField(value As Object) As String
        Dim raw = If(value Is Nothing OrElse value Is DBNull.Value, "", Convert.ToString(value))
        ' Prevent spreadsheet formula injection when opening exported CSV.
        If raw.Length > 0 AndAlso ("=+-@" & ChrW(9) & ChrW(13)).Contains(raw(0)) Then
            raw = "'" & raw
        End If
        Return """" & raw.Replace("""", """""") & """"
    End Function

    Private Sub ExportGrid(source As DataGridView, name As String)
        If source.DataSource Is Nothing Then Return
        Using dialog As New SaveFileDialog With {
            .Filter = "CSV (UTF-8)|*.csv", .FileName = name & "_" &
            DateTime.Now.ToString("yyyyMMdd_HHmm") & ".csv"
        }
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                Dim table = DirectCast(source.DataSource, DataTable)
                Using writer As New StreamWriter(dialog.FileName, False, New UTF8Encoding(True))
                    writer.WriteLine(String.Join(",", table.Columns.Cast(Of DataColumn)().
                                                Select(Function(col) CsvField(col.ColumnName))))
                    For Each r As DataRow In table.Rows
                        writer.WriteLine(String.Join(",", table.Columns.Cast(Of DataColumn)().
                                                    Select(Function(col) CsvField(r(col)))))
                    Next
                End Using
                MessageBox.Show("Report exported.", "PackAndShip")
            Catch ex As Exception
                MessageBox.Show(ex.Message, "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub
End Class
