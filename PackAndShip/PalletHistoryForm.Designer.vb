<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PalletHistoryForm
    Inherits System.Windows.Forms.Form
    Private components As System.ComponentModel.IContainer
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.filters = New System.Windows.Forms.Panel()
        Me.palletCaption = New System.Windows.Forms.Label()
        Me.searchCode = New System.Windows.Forms.TextBox()
        Me.modelCaption = New System.Windows.Forms.Label()
        Me.searchModel = New System.Windows.Forms.TextBox()
        Me.statusCaption = New System.Windows.Forms.Label()
        Me.statusPicker = New System.Windows.Forms.ComboBox()
        Me.fromCaption = New System.Windows.Forms.Label()
        Me.fromPicker = New System.Windows.Forms.DateTimePicker()
        Me.toCaption = New System.Windows.Forms.Label()
        Me.toPicker = New System.Windows.Forms.DateTimePicker()
        Me.refreshButton = New System.Windows.Forms.Button()
        Me.exportSummaryButton = New System.Windows.Forms.Button()
        Me.actions = New System.Windows.Forms.Panel()
        Me.summary = New System.Windows.Forms.Label()
        Me.removeButton = New System.Windows.Forms.Button()
        Me.cancelButton = New System.Windows.Forms.Button()
        Me.palletsTitle = New System.Windows.Forms.Label()
        Me.grid = New System.Windows.Forms.DataGridView()
        Me.tabs = New System.Windows.Forms.TabControl()
        Me.piecesTab = New System.Windows.Forms.TabPage()
        Me.levelsTab = New System.Windows.Forms.TabPage()
        Me.auditTab = New System.Windows.Forms.TabPage()
        Me.packingTab = New System.Windows.Forms.TabPage()
        Me.transfersTab = New System.Windows.Forms.TabPage()
        Me.pieces = New System.Windows.Forms.DataGridView()
        Me.levels = New System.Windows.Forms.DataGridView()
        Me.audit = New System.Windows.Forms.DataGridView()
        Me.packingReport = New System.Windows.Forms.DataGridView()
        Me.transfers = New System.Windows.Forms.DataGridView()
        Me.exportPieces = New System.Windows.Forms.Button()
        Me.exportLevels = New System.Windows.Forms.Button()
        Me.exportAudit = New System.Windows.Forms.Button()
        Me.exportPacking = New System.Windows.Forms.Button()
        Me.exportTransfers = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        Me.filters.Name = "filters"
        Me.filters.Location = New System.Drawing.Point(0,0)
        Me.filters.Size = New System.Drawing.Size(1250,90)
        Me.palletCaption.Name = "palletCaption"
        Me.palletCaption.Location = New System.Drawing.Point(12,16)
        Me.palletCaption.Size = New System.Drawing.Size(45,25)
        Me.searchCode.Name = "searchCode"
        Me.searchCode.Location = New System.Drawing.Point(65,13)
        Me.searchCode.Size = New System.Drawing.Size(150,30)
        Me.modelCaption.Name = "modelCaption"
        Me.modelCaption.Location = New System.Drawing.Point(228,16)
        Me.modelCaption.Size = New System.Drawing.Size(55,25)
        Me.searchModel.Name = "searchModel"
        Me.searchModel.Location = New System.Drawing.Point(288,13)
        Me.searchModel.Size = New System.Drawing.Size(130,30)
        Me.statusCaption.Name = "statusCaption"
        Me.statusCaption.Location = New System.Drawing.Point(430,16)
        Me.statusCaption.Size = New System.Drawing.Size(58,25)
        Me.statusPicker.Name = "statusPicker"
        Me.statusPicker.Location = New System.Drawing.Point(494,13)
        Me.statusPicker.Size = New System.Drawing.Size(115,30)
        Me.fromCaption.Name = "fromCaption"
        Me.fromCaption.Location = New System.Drawing.Point(621,16)
        Me.fromCaption.Size = New System.Drawing.Size(45,25)
        Me.fromPicker.Name = "fromPicker"
        Me.fromPicker.Location = New System.Drawing.Point(670,13)
        Me.fromPicker.Size = New System.Drawing.Size(135,30)
        Me.toCaption.Name = "toCaption"
        Me.toCaption.Location = New System.Drawing.Point(818,16)
        Me.toCaption.Size = New System.Drawing.Size(25,25)
        Me.toPicker.Name = "toPicker"
        Me.toPicker.Location = New System.Drawing.Point(848,13)
        Me.toPicker.Size = New System.Drawing.Size(135,30)
        Me.refreshButton.Name = "refreshButton"
        Me.refreshButton.Location = New System.Drawing.Point(1000,13)
        Me.refreshButton.Size = New System.Drawing.Size(90,31)
        Me.exportSummaryButton.Name = "exportSummaryButton"
        Me.exportSummaryButton.Location = New System.Drawing.Point(12,52)
        Me.exportSummaryButton.Size = New System.Drawing.Size(230,29)
        Me.actions.Name = "actions"
        Me.actions.Location = New System.Drawing.Point(0,90)
        Me.actions.Size = New System.Drawing.Size(1250,48)
        Me.summary.Name = "summary"
        Me.summary.Location = New System.Drawing.Point(15,12)
        Me.summary.Size = New System.Drawing.Size(450,24)
        Me.removeButton.Name = "removeButton"
        Me.removeButton.Location = New System.Drawing.Point(490,7)
        Me.removeButton.Size = New System.Drawing.Size(185,31)
        Me.cancelButton.Name = "cancelButton"
        Me.cancelButton.Location = New System.Drawing.Point(694,7)
        Me.cancelButton.Size = New System.Drawing.Size(130,31)
        Me.palletsTitle.Name = "palletsTitle"
        Me.palletsTitle.Location = New System.Drawing.Point(15,151)
        Me.palletsTitle.Size = New System.Drawing.Size(450,24)
        Me.grid.Name = "grid"
        Me.grid.Location = New System.Drawing.Point(15,180)
        Me.grid.Size = New System.Drawing.Size(1215,250)
        Me.tabs.Name = "tabs"
        Me.tabs.Location = New System.Drawing.Point(15,446)
        Me.tabs.Size = New System.Drawing.Size(1215,309)
        Me.piecesTab.Name = "piecesTab"
        Me.piecesTab.Location = New System.Drawing.Point(0,0)
        Me.piecesTab.Size = New System.Drawing.Size(1200,275)
        Me.levelsTab.Name = "levelsTab"
        Me.levelsTab.Location = New System.Drawing.Point(0,0)
        Me.levelsTab.Size = New System.Drawing.Size(1200,275)
        Me.auditTab.Name = "auditTab"
        Me.auditTab.Location = New System.Drawing.Point(0,0)
        Me.auditTab.Size = New System.Drawing.Size(1200,275)
        Me.packingTab.Name = "packingTab"
        Me.packingTab.Location = New System.Drawing.Point(0,0)
        Me.packingTab.Size = New System.Drawing.Size(1200,275)
        Me.transfersTab.Name = "transfersTab"
        Me.transfersTab.Location = New System.Drawing.Point(0,0)
        Me.transfersTab.Size = New System.Drawing.Size(1200,275)
        Me.pieces.Name = "pieces"
        Me.pieces.Location = New System.Drawing.Point(7,7)
        Me.pieces.Size = New System.Drawing.Size(1175,222)
        Me.levels.Name = "levels"
        Me.levels.Location = New System.Drawing.Point(7,7)
        Me.levels.Size = New System.Drawing.Size(1175,222)
        Me.audit.Name = "audit"
        Me.audit.Location = New System.Drawing.Point(7,7)
        Me.audit.Size = New System.Drawing.Size(1175,222)
        Me.packingReport.Name = "packingReport"
        Me.packingReport.Location = New System.Drawing.Point(7,7)
        Me.packingReport.Size = New System.Drawing.Size(1175,222)
        Me.transfers.Name = "transfers"
        Me.transfers.Location = New System.Drawing.Point(7,7)
        Me.transfers.Size = New System.Drawing.Size(1175,222)
        Me.exportPieces.Name = "exportPieces"
        Me.exportPieces.Location = New System.Drawing.Point(7,236)
        Me.exportPieces.Size = New System.Drawing.Size(182,28)
        Me.exportLevels.Name = "exportLevels"
        Me.exportLevels.Location = New System.Drawing.Point(7,236)
        Me.exportLevels.Size = New System.Drawing.Size(182,28)
        Me.exportAudit.Name = "exportAudit"
        Me.exportAudit.Location = New System.Drawing.Point(7,236)
        Me.exportAudit.Size = New System.Drawing.Size(182,28)
        Me.exportPacking.Name = "exportPacking"
        Me.exportPacking.Location = New System.Drawing.Point(7,236)
        Me.exportPacking.Size = New System.Drawing.Size(182,28)
        Me.exportTransfers.Name = "exportTransfers"
        Me.exportTransfers.Location = New System.Drawing.Point(7,236)
        Me.exportTransfers.Size = New System.Drawing.Size(182,28)
        Me.palletCaption.Text = "Pallet"
        Me.modelCaption.Text = "Modelo"
        Me.statusCaption.Text = "Estado"
        Me.fromCaption.Text = "Desde"
        Me.toCaption.Text = "Hasta"
        Me.refreshButton.Text = "Buscar"
        Me.exportSummaryButton.Text = "Exportar reporte de pallets CSV"
        Me.removeButton.Text = "Eliminar pieza seleccionada"
        Me.cancelButton.Text = "Cancelar pallet"
        Me.palletsTitle.Text = "Pallets"
        Me.piecesTab.Text = "Piezas e ID de inspección Q-Wall"
        Me.levelsTab.Text = "Niveles"
        Me.auditTab.Text = "Historial de auditoría"
        Me.packingTab.Text = "Reporte por modelo/operador/día"
        Me.transfersTab.Text = "Transferencias de pallets"
        Me.exportPieces.Text = "Exportar pestaña CSV"
        Me.exportLevels.Text = "Exportar pestaña CSV"
        Me.exportAudit.Text = "Exportar pestaña CSV"
        Me.exportPacking.Text = "Exportar pestaña CSV"
        Me.exportTransfers.Text = "Exportar pestaña CSV"
        Me.statusPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.fromPicker.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.toPicker.Format = System.Windows.Forms.DateTimePickerFormat.Short
        Me.summary.AutoEllipsis = True
        Me.grid.ReadOnly = True
        Me.grid.AllowUserToAddRows = False
        Me.grid.AllowUserToDeleteRows = False
        Me.grid.MultiSelect = False
        Me.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.grid.RowHeadersVisible = False
        Me.grid.BackgroundColor = System.Drawing.Color.White
        Me.pieces.ReadOnly = True
        Me.pieces.AllowUserToAddRows = False
        Me.pieces.AllowUserToDeleteRows = False
        Me.pieces.MultiSelect = False
        Me.pieces.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.pieces.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.pieces.RowHeadersVisible = False
        Me.pieces.BackgroundColor = System.Drawing.Color.White
        Me.levels.ReadOnly = True
        Me.levels.AllowUserToAddRows = False
        Me.levels.AllowUserToDeleteRows = False
        Me.levels.MultiSelect = False
        Me.levels.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.levels.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.levels.RowHeadersVisible = False
        Me.levels.BackgroundColor = System.Drawing.Color.White
        Me.audit.ReadOnly = True
        Me.audit.AllowUserToAddRows = False
        Me.audit.AllowUserToDeleteRows = False
        Me.audit.MultiSelect = False
        Me.audit.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.audit.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.audit.RowHeadersVisible = False
        Me.audit.BackgroundColor = System.Drawing.Color.White
        Me.packingReport.ReadOnly = True
        Me.packingReport.AllowUserToAddRows = False
        Me.packingReport.AllowUserToDeleteRows = False
        Me.packingReport.MultiSelect = False
        Me.packingReport.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.packingReport.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.packingReport.RowHeadersVisible = False
        Me.packingReport.BackgroundColor = System.Drawing.Color.White
        Me.transfers.ReadOnly = True
        Me.transfers.AllowUserToAddRows = False
        Me.transfers.AllowUserToDeleteRows = False
        Me.transfers.MultiSelect = False
        Me.transfers.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.transfers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
        Me.transfers.RowHeadersVisible = False
        Me.transfers.BackgroundColor = System.Drawing.Color.White
        Me.filters.Controls.Add(Me.palletCaption)
        Me.filters.Controls.Add(Me.searchCode)
        Me.filters.Controls.Add(Me.modelCaption)
        Me.filters.Controls.Add(Me.searchModel)
        Me.filters.Controls.Add(Me.statusCaption)
        Me.filters.Controls.Add(Me.statusPicker)
        Me.filters.Controls.Add(Me.fromCaption)
        Me.filters.Controls.Add(Me.fromPicker)
        Me.filters.Controls.Add(Me.toCaption)
        Me.filters.Controls.Add(Me.toPicker)
        Me.filters.Controls.Add(Me.refreshButton)
        Me.filters.Controls.Add(Me.exportSummaryButton)
        Me.actions.Controls.Add(Me.summary)
        Me.actions.Controls.Add(Me.removeButton)
        Me.actions.Controls.Add(Me.cancelButton)
        Me.piecesTab.Controls.Add(Me.pieces)
        Me.piecesTab.Controls.Add(Me.exportPieces)
        Me.tabs.TabPages.Add(Me.piecesTab)
        Me.levelsTab.Controls.Add(Me.levels)
        Me.levelsTab.Controls.Add(Me.exportLevels)
        Me.tabs.TabPages.Add(Me.levelsTab)
        Me.auditTab.Controls.Add(Me.audit)
        Me.auditTab.Controls.Add(Me.exportAudit)
        Me.tabs.TabPages.Add(Me.auditTab)
        Me.packingTab.Controls.Add(Me.packingReport)
        Me.packingTab.Controls.Add(Me.exportPacking)
        Me.tabs.TabPages.Add(Me.packingTab)
        Me.transfersTab.Controls.Add(Me.transfers)
        Me.transfersTab.Controls.Add(Me.exportTransfers)
        Me.tabs.TabPages.Add(Me.transfersTab)
        Me.Controls.Add(Me.filters)
        Me.Controls.Add(Me.actions)
        Me.Controls.Add(Me.palletsTitle)
        Me.Controls.Add(Me.grid)
        Me.Controls.Add(Me.tabs)
        Me.ClientSize = New System.Drawing.Size(1250,800)
        Me.MinimumSize = New System.Drawing.Size(0,0)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "PackAndShip | Historial de pallets"
        Me.BackColor = System.Drawing.Color.FromArgb(244,247,250)
        Me.Font = New System.Drawing.Font("Segoe UI",10.0!)
        Me.ResumeLayout(False)
    End Sub
    Friend WithEvents filters As System.Windows.Forms.Panel
    Friend WithEvents palletCaption As System.Windows.Forms.Label
    Friend WithEvents searchCode As System.Windows.Forms.TextBox
    Friend WithEvents modelCaption As System.Windows.Forms.Label
    Friend WithEvents searchModel As System.Windows.Forms.TextBox
    Friend WithEvents statusCaption As System.Windows.Forms.Label
    Friend WithEvents statusPicker As System.Windows.Forms.ComboBox
    Friend WithEvents fromCaption As System.Windows.Forms.Label
    Friend WithEvents fromPicker As System.Windows.Forms.DateTimePicker
    Friend WithEvents toCaption As System.Windows.Forms.Label
    Friend WithEvents toPicker As System.Windows.Forms.DateTimePicker
    Friend WithEvents refreshButton As System.Windows.Forms.Button
    Friend WithEvents exportSummaryButton As System.Windows.Forms.Button
    Friend WithEvents actions As System.Windows.Forms.Panel
    Friend WithEvents summary As System.Windows.Forms.Label
    Friend WithEvents removeButton As System.Windows.Forms.Button
    Friend WithEvents cancelButton As System.Windows.Forms.Button
    Friend WithEvents palletsTitle As System.Windows.Forms.Label
    Friend WithEvents grid As System.Windows.Forms.DataGridView
    Friend WithEvents tabs As System.Windows.Forms.TabControl
    Friend WithEvents piecesTab As System.Windows.Forms.TabPage
    Friend WithEvents levelsTab As System.Windows.Forms.TabPage
    Friend WithEvents auditTab As System.Windows.Forms.TabPage
    Friend WithEvents packingTab As System.Windows.Forms.TabPage
    Friend WithEvents transfersTab As System.Windows.Forms.TabPage
    Friend WithEvents pieces As System.Windows.Forms.DataGridView
    Friend WithEvents levels As System.Windows.Forms.DataGridView
    Friend WithEvents audit As System.Windows.Forms.DataGridView
    Friend WithEvents packingReport As System.Windows.Forms.DataGridView
    Friend WithEvents transfers As System.Windows.Forms.DataGridView
    Friend WithEvents exportPieces As System.Windows.Forms.Button
    Friend WithEvents exportLevels As System.Windows.Forms.Button
    Friend WithEvents exportAudit As System.Windows.Forms.Button
    Friend WithEvents exportPacking As System.Windows.Forms.Button
    Friend WithEvents exportTransfers As System.Windows.Forms.Button
End Class
