<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ScanRulesForm
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
        Me.header = New System.Windows.Forms.Panel()
        Me.heading = New System.Windows.Forms.Label()
        Me.subtitle = New System.Windows.Forms.Label()
        Me.actions = New System.Windows.Forms.Panel()
        Me.addRule = New System.Windows.Forms.Button()
        Me.editRule = New System.Windows.Forms.Button()
        Me.toggleRule = New System.Windows.Forms.Button()
        Me.filters = New System.Windows.Forms.Panel()
        Me.searchCaption = New System.Windows.Forms.Label()
        Me.searchBox = New System.Windows.Forms.TextBox()
        Me.customerCaption = New System.Windows.Forms.Label()
        Me.customerPicker = New System.Windows.Forms.ComboBox()
        Me.refreshButton = New System.Windows.Forms.Button()
        Me.summaryLabel = New System.Windows.Forms.Label()
        Me.rulesGrid = New System.Windows.Forms.DataGridView()
        Me.fieldsCaption = New System.Windows.Forms.Label()
        Me.fieldsGrid = New System.Windows.Forms.DataGridView()
        Me.notesLabel = New System.Windows.Forms.TextBox()
        Me.col0 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col1 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col2 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col3 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col4 = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.col5 = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.col6 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col7 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col8 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col9 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col10 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col11 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col12 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col13 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col14 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col15 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.col16 = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.SuspendLayout()
        Me.header.Name = "header"
        Me.header.Location = New System.Drawing.Point(0,0)
        Me.header.Size = New System.Drawing.Size(1200,86)
        Me.heading.Name = "heading"
        Me.heading.Location = New System.Drawing.Point(22,12)
        Me.heading.Size = New System.Drawing.Size(590,44)
        Me.subtitle.Name = "subtitle"
        Me.subtitle.Location = New System.Drawing.Point(25,59)
        Me.subtitle.Size = New System.Drawing.Size(740,24)
        Me.actions.Name = "actions"
        Me.actions.Location = New System.Drawing.Point(0,86)
        Me.actions.Size = New System.Drawing.Size(1200,54)
        Me.addRule.Name = "addRule"
        Me.addRule.Location = New System.Drawing.Point(18,10)
        Me.addRule.Size = New System.Drawing.Size(130,34)
        Me.editRule.Name = "editRule"
        Me.editRule.Location = New System.Drawing.Point(163,10)
        Me.editRule.Size = New System.Drawing.Size(145,34)
        Me.toggleRule.Name = "toggleRule"
        Me.toggleRule.Location = New System.Drawing.Point(324,10)
        Me.toggleRule.Size = New System.Drawing.Size(185,34)
        Me.filters.Name = "filters"
        Me.filters.Location = New System.Drawing.Point(0,140)
        Me.filters.Size = New System.Drawing.Size(1200,58)
        Me.searchCaption.Name = "searchCaption"
        Me.searchCaption.Location = New System.Drawing.Point(18,15)
        Me.searchCaption.Size = New System.Drawing.Size(70,25)
        Me.searchBox.Name = "searchBox"
        Me.searchBox.Location = New System.Drawing.Point(95,11)
        Me.searchBox.Size = New System.Drawing.Size(240,33)
        Me.customerCaption.Name = "customerCaption"
        Me.customerCaption.Location = New System.Drawing.Point(355,15)
        Me.customerCaption.Size = New System.Drawing.Size(100,25)
        Me.customerPicker.Name = "customerPicker"
        Me.customerPicker.Location = New System.Drawing.Point(455,11)
        Me.customerPicker.Size = New System.Drawing.Size(225,33)
        Me.refreshButton.Name = "refreshButton"
        Me.refreshButton.Location = New System.Drawing.Point(695,11)
        Me.refreshButton.Size = New System.Drawing.Size(95,33)
        Me.summaryLabel.Name = "summaryLabel"
        Me.summaryLabel.Location = New System.Drawing.Point(810,15)
        Me.summaryLabel.Size = New System.Drawing.Size(300,25)
        Me.rulesGrid.Name = "rulesGrid"
        Me.rulesGrid.Location = New System.Drawing.Point(14,209)
        Me.rulesGrid.Size = New System.Drawing.Size(1170,245)
        Me.fieldsCaption.Name = "fieldsCaption"
        Me.fieldsCaption.Location = New System.Drawing.Point(14,469)
        Me.fieldsCaption.Size = New System.Drawing.Size(580,30)
        Me.fieldsGrid.Name = "fieldsGrid"
        Me.fieldsGrid.Location = New System.Drawing.Point(14,500)
        Me.fieldsGrid.Size = New System.Drawing.Size(1170,168)
        Me.notesLabel.Name = "notesLabel"
        Me.notesLabel.Location = New System.Drawing.Point(14,681)
        Me.notesLabel.Size = New System.Drawing.Size(1170,62)
        Me.heading.Text = "Reglas de escaneo"
        Me.subtitle.Text = "Reglas compartidas con Q-Wall • Los cambios también afectan a Q-Wall"
        Me.addRule.Text = "Agregar regla"
        Me.editRule.Text = "Editar selección"
        Me.toggleRule.Text = "Activar / Desactivar"
        Me.searchCaption.Text = "Buscar"
        Me.customerCaption.Text = "Cliente"
        Me.refreshButton.Text = "Actualizar"
        Me.fieldsCaption.Text = "Campos de la regla seleccionada"
        Me.header.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.heading.ForeColor = System.Drawing.Color.White
        Me.subtitle.ForeColor = System.Drawing.Color.White
        Me.heading.Font = New System.Drawing.Font("Segoe UI",22.0!,System.Drawing.FontStyle.Bold)
        Me.fieldsCaption.Font = New System.Drawing.Font("Segoe UI",13.0!,System.Drawing.FontStyle.Bold)
        Me.customerPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.notesLabel.Multiline = True
        Me.notesLabel.ReadOnly = True
        Me.notesLabel.BackColor = System.Drawing.Color.White
        Me.rulesGrid.ReadOnly = True
        Me.rulesGrid.AllowUserToAddRows = False
        Me.rulesGrid.AllowUserToDeleteRows = False
        Me.rulesGrid.AutoGenerateColumns = False
        Me.rulesGrid.MultiSelect = False
        Me.rulesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.rulesGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.fieldsGrid.ReadOnly = True
        Me.fieldsGrid.AllowUserToAddRows = False
        Me.fieldsGrid.AllowUserToDeleteRows = False
        Me.fieldsGrid.AutoGenerateColumns = False
        Me.fieldsGrid.MultiSelect = False
        Me.fieldsGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.fieldsGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.col0.Name = "Id"
        Me.col0.HeaderText = "ID"
        Me.col0.DataPropertyName = "Id"
        Me.col0.FillWeight = 28!
        Me.rulesGrid.Columns.Add(Me.col0)
        Me.col1.Name = "SsiPart"
        Me.col1.HeaderText = "Parte SSI"
        Me.col1.DataPropertyName = "SsiPart"
        Me.col1.FillWeight = 90!
        Me.rulesGrid.Columns.Add(Me.col1)
        Me.col2.Name = "Customer"
        Me.col2.HeaderText = "Cliente"
        Me.col2.DataPropertyName = "Customer"
        Me.col2.FillWeight = 100!
        Me.rulesGrid.Columns.Add(Me.col2)
        Me.col3.Name = "ScanCount"
        Me.col3.HeaderText = "Escaneos"
        Me.col3.DataPropertyName = "ScanCount"
        Me.col3.FillWeight = 36!
        Me.rulesGrid.Columns.Add(Me.col3)
        Me.col4.Name = "RequiresMatch"
        Me.col4.HeaderText = "Coincidencia"
        Me.col4.DataPropertyName = "RequiresMatch"
        Me.col4.FillWeight = 36!
        Me.rulesGrid.Columns.Add(Me.col4)
        Me.col5.Name = "IsActive"
        Me.col5.HeaderText = "Activo"
        Me.col5.DataPropertyName = "IsActive"
        Me.col5.FillWeight = 35!
        Me.rulesGrid.Columns.Add(Me.col5)
        Me.col6.HeaderText = "Escaneo"
        Me.col6.DataPropertyName = "ScanIndex"
        Me.col6.FillWeight = 30!
        Me.fieldsGrid.Columns.Add(Me.col6)
        Me.col7.HeaderText = "Modo"
        Me.col7.DataPropertyName = "ExtractionMode"
        Me.col7.FillWeight = 85!
        Me.fieldsGrid.Columns.Add(Me.col7)
        Me.col8.HeaderText = "Destino"
        Me.col8.DataPropertyName = "FieldTarget"
        Me.col8.FillWeight = 75!
        Me.fieldsGrid.Columns.Add(Me.col8)
        Me.col9.HeaderText = "Etiqueta"
        Me.col9.DataPropertyName = "DisplayLabel"
        Me.col9.FillWeight = 110!
        Me.fieldsGrid.Columns.Add(Me.col9)
        Me.col10.HeaderText = "Separador"
        Me.col10.DataPropertyName = "Separator"
        Me.col10.FillWeight = 60!
        Me.fieldsGrid.Columns.Add(Me.col10)
        Me.col11.HeaderText = "Separador personalizado"
        Me.col11.DataPropertyName = "SeparatorCustom"
        Me.col11.FillWeight = 60!
        Me.fieldsGrid.Columns.Add(Me.col11)
        Me.col12.HeaderText = "Posición"
        Me.col12.DataPropertyName = "ValuePosition"
        Me.col12.FillWeight = 65!
        Me.fieldsGrid.Columns.Add(Me.col12)
        Me.col13.HeaderText = "Segmento"
        Me.col13.DataPropertyName = "SegmentIndex"
        Me.col13.FillWeight = 45!
        Me.fieldsGrid.Columns.Add(Me.col13)
        Me.col14.HeaderText = "Longitud"
        Me.col14.DataPropertyName = "FixedLength"
        Me.col14.FillWeight = 45!
        Me.fieldsGrid.Columns.Add(Me.col14)
        Me.col15.HeaderText = "Prefijo"
        Me.col15.DataPropertyName = "PrefixValue"
        Me.col15.FillWeight = 65!
        Me.fieldsGrid.Columns.Add(Me.col15)
        Me.col16.HeaderText = "Orden"
        Me.col16.DataPropertyName = "SequenceOrder"
        Me.col16.FillWeight = 40!
        Me.fieldsGrid.Columns.Add(Me.col16)
        Me.addRule.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.addRule.ForeColor = System.Drawing.Color.White
        Me.addRule.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.editRule.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.editRule.ForeColor = System.Drawing.Color.White
        Me.editRule.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.toggleRule.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.toggleRule.ForeColor = System.Drawing.Color.White
        Me.toggleRule.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.refreshButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.refreshButton.ForeColor = System.Drawing.Color.White
        Me.refreshButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.header.Controls.Add(Me.heading)
        Me.header.Controls.Add(Me.subtitle)
        Me.actions.Controls.Add(Me.addRule)
        Me.actions.Controls.Add(Me.editRule)
        Me.actions.Controls.Add(Me.toggleRule)
        Me.filters.Controls.Add(Me.searchCaption)
        Me.filters.Controls.Add(Me.searchBox)
        Me.filters.Controls.Add(Me.customerCaption)
        Me.filters.Controls.Add(Me.customerPicker)
        Me.filters.Controls.Add(Me.refreshButton)
        Me.filters.Controls.Add(Me.summaryLabel)
        Me.Controls.Add(Me.header)
        Me.Controls.Add(Me.actions)
        Me.Controls.Add(Me.filters)
        Me.Controls.Add(Me.rulesGrid)
        Me.Controls.Add(Me.fieldsCaption)
        Me.Controls.Add(Me.fieldsGrid)
        Me.Controls.Add(Me.notesLabel)
        Me.ClientSize = New System.Drawing.Size(1200,790)
        Me.MinimumSize = New System.Drawing.Size(0,0)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "PackAndShip | Reglas de escaneo"
        Me.BackColor = System.Drawing.Color.FromArgb(244,247,250)
        Me.Font = New System.Drawing.Font("Segoe UI",10.0!)
        Me.ResumeLayout(False)
    End Sub
    Friend WithEvents header As System.Windows.Forms.Panel
    Friend WithEvents heading As System.Windows.Forms.Label
    Friend WithEvents subtitle As System.Windows.Forms.Label
    Friend WithEvents actions As System.Windows.Forms.Panel
    Friend WithEvents addRule As System.Windows.Forms.Button
    Friend WithEvents editRule As System.Windows.Forms.Button
    Friend WithEvents toggleRule As System.Windows.Forms.Button
    Friend WithEvents filters As System.Windows.Forms.Panel
    Friend WithEvents searchCaption As System.Windows.Forms.Label
    Friend WithEvents searchBox As System.Windows.Forms.TextBox
    Friend WithEvents customerCaption As System.Windows.Forms.Label
    Friend WithEvents customerPicker As System.Windows.Forms.ComboBox
    Friend WithEvents refreshButton As System.Windows.Forms.Button
    Friend WithEvents summaryLabel As System.Windows.Forms.Label
    Friend WithEvents rulesGrid As System.Windows.Forms.DataGridView
    Friend WithEvents fieldsCaption As System.Windows.Forms.Label
    Friend WithEvents fieldsGrid As System.Windows.Forms.DataGridView
    Friend WithEvents notesLabel As System.Windows.Forms.TextBox
    Friend WithEvents col0 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col1 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col2 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col3 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col4 As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents col5 As System.Windows.Forms.DataGridViewCheckBoxColumn
    Friend WithEvents col6 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col7 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col8 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col9 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col10 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col11 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col12 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col13 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col14 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col15 As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents col16 As System.Windows.Forms.DataGridViewTextBoxColumn
End Class
