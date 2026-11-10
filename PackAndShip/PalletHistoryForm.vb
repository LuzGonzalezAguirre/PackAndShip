Imports System.Data
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Windows.Forms

' Pallet history, detailed traceability and CSV reports.
Public Class PalletHistoryForm
    Inherits Form















    Private selectedId As Integer

    Public Sub New()
        InitializeComponent()
        statusPicker.Items.AddRange(New Object() {"All", "OPEN", "CLOSED", "PARTIAL", "TRANSFERRED", "CANCELLED"})
        statusPicker.SelectedIndex = 0
        fromPicker.Value = DateTime.Today.AddMonths(-3)
        toPicker.Value = DateTime.Today
        removeButton.Visible = SessionContext.CanModifySetup
        cancelButton.Visible = SessionContext.UserId > 0
        BindEvents()
        AddHandler Shown, Sub() RefreshHistory()
    End Sub

    Private Shared Sub ConfigureGrid(g As DataGridView)
        g.ReadOnly = True
        g.AllowUserToAddRows = False
        g.AllowUserToDeleteRows = False
        g.MultiSelect = False
        g.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
        g.BackgroundColor = Color.White
        g.RowHeadersVisible = False
    End Sub

    Private Sub BindEvents()
        AddHandler refreshButton.Click, Sub() RefreshHistory()
        AddHandler exportSummaryButton.Click, Sub() ExportGrid(grid, "pallet_report")
        AddHandler removeButton.Click, AddressOf RemoveSelectedPiece
        AddHandler cancelButton.Click, AddressOf CancelSelectedPallet
        AddHandler grid.SelectionChanged, AddressOf SelectedPalletChanged
        AddHandler exportPieces.Click, Sub() ExportGrid(pieces, "Pieces_and_Q-Wall_Inspection_IDs")
        AddHandler exportLevels.Click, Sub() ExportGrid(levels, "Levels")
        AddHandler exportAudit.Click, Sub() ExportGrid(audit, "Audit_history")
        AddHandler exportPacking.Click, Sub() ExportGrid(packingReport, "Packing_report_by_model_operator_day")
        AddHandler exportTransfers.Click, Sub() ExportGrid(transfers, "Pallet_transfers")
    End Sub

    Private Sub RefreshHistory()
        Try
            If fromPicker.Value.Date > toPicker.Value.Date Then
                Throw New InvalidOperationException("La fecha inicial debe ser anterior a la fecha final.")
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
            summary.Text = "Pallets: " & data.Rows.Count & "  |  Piezas empacadas actuales: " & packed & "     "
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
            MessageBox.Show(ex.Message, "Error al cargar el historial",
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
            MessageBox.Show(ex.Message, "Error al consultar detalles del pallet",
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
            .Text = action & " | Motivo obligatorio", .Size = New Size(550, 260),
            .FormBorderStyle = FormBorderStyle.FixedDialog, .StartPosition = FormStartPosition.CenterParent,
            .MinimizeBox = False, .MaximizeBox = False
        }
            Dim caption As New Label With {
                .Text = "Motivo (mínimo 10 caracteres). Se guardará permanentemente en auditoría.",
                .Dock = DockStyle.Top, .Height = 40
            }
            Dim reason As New TextBox With {.Multiline = True, .Dock = DockStyle.Fill, .MaxLength = 500}
            Dim footer As New FlowLayoutPanel With {.Dock = DockStyle.Bottom, .Height = 52, .FlowDirection = FlowDirection.RightToLeft}
            Dim ok As New Button With {.Text = "Confirmar", .DialogResult = DialogResult.OK}
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
                MessageBox.Show("El motivo debe tener al menos 10 caracteres.", "Obligatorio")
                Return Nothing
            End If
            Return reason.Text.Trim()
        End Using
    End Function

    Private Sub RemoveSelectedPiece(sender As Object, e As EventArgs)
        If selectedId <= 0 OrElse pieces.SelectedRows.Count = 0 Then Return
        Dim productId = Convert.ToInt32(pieces.SelectedRows(0).Cells("ProductId").Value)
        Dim reason = ReasonDialog("Eliminar producto " & productId)
        If reason Is Nothing Then Return
        If MessageBox.Show("Eliminar producto " & productId & " de este nivel ABIERTO? " &
                           "La acción quedará registrada en auditoría.", "Confirmar corrección",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
        Try
            PaSHistoryData.RemovePiece(selectedId, productId, reason)
            RefreshHistory()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Corrección bloqueada", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub CancelSelectedPallet(sender As Object, e As EventArgs)
        If selectedId <= 0 OrElse Not cancelButton.Enabled OrElse
           grid.SelectedRows.Count = 0 Then Return
        Dim code = Convert.ToString(grid.SelectedRows(0).Cells("Pallet").Value)
        Dim required = Convert.ToInt32(grid.SelectedRows(0).Cells("Obligatorio").Value)
        Using auth As New PartialPalletAuthorizationForm(code, 0, required, True)
            If auth.ShowDialog(Me) <> DialogResult.OK Then Return
            If MessageBox.Show(
                "Cancelar pallet vacío " & code & "? El código no podrá reutilizarse.",
                "Confirmar cancelación", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
            Try
                PaSPackingData.CancelEmptyPallet(
                    selectedId, auth.AuthorizedUserId, auth.AuthorizedBarcode,
                    auth.AuthorizedPassword, auth.Reason)
                RefreshHistory()
            Catch ex As Exception
                MessageBox.Show(ex.Message, "Cancelación bloqueada",
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
                MessageBox.Show("Reporte exportado.", "PackAndShip")
            Catch ex As Exception
                MessageBox.Show(ex.Message, "Error al exportar", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub
End Class
