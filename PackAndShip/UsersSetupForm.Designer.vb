<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UsersSetupForm
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
        Me.top = New System.Windows.Forms.Panel()
        Me.heading = New System.Windows.Forms.Label()
        Me.subheading = New System.Windows.Forms.Label()
        Me.actions = New System.Windows.Forms.Panel()
        Me.newButton = New System.Windows.Forms.Button()
        Me.editButton = New System.Windows.Forms.Button()
        Me.statusButton = New System.Windows.Forms.Button()
        Me.resetButton = New System.Windows.Forms.Button()
        Me.reloadButton = New System.Windows.Forms.Button()
        Me.departmentsButton = New System.Windows.Forms.Button()
        Me.searchPanel = New System.Windows.Forms.Panel()
        Me.searchLabel = New System.Windows.Forms.Label()
        Me.searchBox = New System.Windows.Forms.TextBox()
        Me.roleLabel = New System.Windows.Forms.Label()
        Me.roleFilter = New System.Windows.Forms.ComboBox()
        Me.countLabel = New System.Windows.Forms.Label()
        Me.usersGrid = New System.Windows.Forms.DataGridView()
        Me.columnId = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.columnFullName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.columnBarcode = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.columnRoleName = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.columnIsActive = New System.Windows.Forms.DataGridViewCheckBoxColumn()
        Me.SuspendLayout()
        Me.top.Name = "top"
        Me.top.Location = New System.Drawing.Point(0,0)
        Me.top.Size = New System.Drawing.Size(1060,92)
        Me.heading.Name = "heading"
        Me.heading.Location = New System.Drawing.Point(22,13)
        Me.heading.Size = New System.Drawing.Size(600,37)
        Me.subheading.Name = "subheading"
        Me.subheading.Location = New System.Drawing.Point(25,59)
        Me.subheading.Size = New System.Drawing.Size(680,26)
        Me.actions.Name = "actions"
        Me.actions.Location = New System.Drawing.Point(0,92)
        Me.actions.Size = New System.Drawing.Size(1060,57)
        Me.newButton.Name = "newButton"
        Me.newButton.Location = New System.Drawing.Point(15,10)
        Me.newButton.Size = New System.Drawing.Size(115,34)
        Me.editButton.Name = "editButton"
        Me.editButton.Location = New System.Drawing.Point(142,10)
        Me.editButton.Size = New System.Drawing.Size(90,34)
        Me.statusButton.Name = "statusButton"
        Me.statusButton.Location = New System.Drawing.Point(243,10)
        Me.statusButton.Size = New System.Drawing.Size(180,34)
        Me.resetButton.Name = "resetButton"
        Me.resetButton.Location = New System.Drawing.Point(435,10)
        Me.resetButton.Size = New System.Drawing.Size(145,34)
        Me.reloadButton.Name = "reloadButton"
        Me.reloadButton.Location = New System.Drawing.Point(592,10)
        Me.reloadButton.Size = New System.Drawing.Size(95,34)
        Me.departmentsButton.Name = "departmentsButton"
        Me.departmentsButton.Text = "Autorizaciones"
        Me.departmentsButton.Location = New System.Drawing.Point(700,10)
        Me.departmentsButton.Size = New System.Drawing.Size(155,34)
        Me.departmentsButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.departmentsButton.ForeColor = System.Drawing.Color.White
        Me.departmentsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.searchPanel.Name = "searchPanel"
        Me.searchPanel.Location = New System.Drawing.Point(0,149)
        Me.searchPanel.Size = New System.Drawing.Size(1060,52)
        Me.searchLabel.Name = "searchLabel"
        Me.searchLabel.Location = New System.Drawing.Point(20,15)
        Me.searchLabel.Size = New System.Drawing.Size(70,24)
        Me.searchBox.Name = "searchBox"
        Me.searchBox.Location = New System.Drawing.Point(95,11)
        Me.searchBox.Size = New System.Drawing.Size(245,30)
        Me.roleLabel.Name = "roleLabel"
        Me.roleLabel.Location = New System.Drawing.Point(360,15)
        Me.roleLabel.Size = New System.Drawing.Size(48,24)
        Me.roleFilter.Name = "roleFilter"
        Me.roleFilter.Location = New System.Drawing.Point(407,11)
        Me.roleFilter.Size = New System.Drawing.Size(230,30)
        Me.countLabel.Name = "countLabel"
        Me.countLabel.Location = New System.Drawing.Point(660,15)
        Me.countLabel.Size = New System.Drawing.Size(240,24)
        Me.usersGrid.Name = "usersGrid"
        Me.usersGrid.Location = New System.Drawing.Point(18,217)
        Me.usersGrid.Size = New System.Drawing.Size(1020,400)
        Me.heading.Text = "Usuarios y roles"
        Me.subheading.Text = "Administración de usuarios de PackAndShip"
        Me.newButton.Text = "Agregar usuario"
        Me.editButton.Text = "Editar"
        Me.statusButton.Text = "Activar / Desactivar"
        Me.resetButton.Text = "Restablecer contraseña"
        Me.reloadButton.Text = "Actualizar"
        Me.searchLabel.Text = "Buscar"
        Me.roleLabel.Text = "Rol"
        Me.top.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.heading.ForeColor = System.Drawing.Color.White
        Me.subheading.ForeColor = System.Drawing.Color.White
        Me.heading.Font = New System.Drawing.Font("Segoe UI",22.0!,System.Drawing.FontStyle.Bold)
        Me.roleFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.roleFilter.DisplayMember = "Name"
        Me.roleFilter.ValueMember = "Id"
        Me.usersGrid.ReadOnly = True
        Me.usersGrid.AllowUserToAddRows = False
        Me.usersGrid.AllowUserToDeleteRows = False
        Me.usersGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect
        Me.usersGrid.MultiSelect = False
        Me.usersGrid.AutoGenerateColumns = False
        Me.usersGrid.BackgroundColor = System.Drawing.Color.White
        Me.usersGrid.RowHeadersVisible = False
        Me.usersGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.columnId.Name = "Id"
        Me.columnId.HeaderText = "ID"
        Me.columnId.DataPropertyName = "Id"
        Me.columnId.FillWeight = 30!
        Me.usersGrid.Columns.Add(Me.columnId)
        Me.columnFullName.Name = "FullName"
        Me.columnFullName.HeaderText = "Empleado"
        Me.columnFullName.DataPropertyName = "FullName"
        Me.columnFullName.FillWeight = 140!
        Me.usersGrid.Columns.Add(Me.columnFullName)
        Me.columnBarcode.Name = "Barcode"
        Me.columnBarcode.HeaderText = "Código de barras"
        Me.columnBarcode.DataPropertyName = "Barcode"
        Me.columnBarcode.FillWeight = 80!
        Me.usersGrid.Columns.Add(Me.columnBarcode)
        Me.columnRoleName.Name = "RoleName"
        Me.columnRoleName.HeaderText = "Rol"
        Me.columnRoleName.DataPropertyName = "RoleName"
        Me.columnRoleName.FillWeight = 90!
        Me.usersGrid.Columns.Add(Me.columnRoleName)
        Me.columnIsActive.Name = "IsActive"
        Me.columnIsActive.HeaderText = "Activo"
        Me.columnIsActive.DataPropertyName = "IsActive"
        Me.columnIsActive.FillWeight = 35!
        Me.usersGrid.Columns.Add(Me.columnIsActive)
        Me.newButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.newButton.ForeColor = System.Drawing.Color.White
        Me.newButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.editButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.editButton.ForeColor = System.Drawing.Color.White
        Me.editButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.statusButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.statusButton.ForeColor = System.Drawing.Color.White
        Me.statusButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.resetButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.resetButton.ForeColor = System.Drawing.Color.White
        Me.resetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.reloadButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.reloadButton.ForeColor = System.Drawing.Color.White
        Me.reloadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.top.Controls.Add(Me.heading)
        Me.top.Controls.Add(Me.subheading)
        Me.actions.Controls.Add(Me.newButton)
        Me.actions.Controls.Add(Me.editButton)
        Me.actions.Controls.Add(Me.statusButton)
        Me.actions.Controls.Add(Me.resetButton)
        Me.actions.Controls.Add(Me.reloadButton)
        Me.actions.Controls.Add(Me.departmentsButton)
        Me.searchPanel.Controls.Add(Me.searchLabel)
        Me.searchPanel.Controls.Add(Me.searchBox)
        Me.searchPanel.Controls.Add(Me.roleLabel)
        Me.searchPanel.Controls.Add(Me.roleFilter)
        Me.searchPanel.Controls.Add(Me.countLabel)
        Me.Controls.Add(Me.top)
        Me.Controls.Add(Me.actions)
        Me.Controls.Add(Me.searchPanel)
        Me.Controls.Add(Me.usersGrid)
        Me.ClientSize = New System.Drawing.Size(1060,680)
        Me.MinimumSize = New System.Drawing.Size(0,0)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "PackAndShip | Usuarios y roles"
        Me.BackColor = System.Drawing.Color.FromArgb(244,247,250)
        Me.Font = New System.Drawing.Font("Segoe UI",10.0!)
        Me.ResumeLayout(False)
    End Sub
    Friend WithEvents top As System.Windows.Forms.Panel
    Friend WithEvents heading As System.Windows.Forms.Label
    Friend WithEvents subheading As System.Windows.Forms.Label
    Friend WithEvents actions As System.Windows.Forms.Panel
    Friend WithEvents newButton As System.Windows.Forms.Button
    Friend WithEvents editButton As System.Windows.Forms.Button
    Friend WithEvents statusButton As System.Windows.Forms.Button
    Friend WithEvents resetButton As System.Windows.Forms.Button
    Friend WithEvents reloadButton As System.Windows.Forms.Button
    Friend WithEvents departmentsButton As System.Windows.Forms.Button
    Friend WithEvents searchPanel As System.Windows.Forms.Panel
    Friend WithEvents searchLabel As System.Windows.Forms.Label
    Friend WithEvents searchBox As System.Windows.Forms.TextBox
    Friend WithEvents roleLabel As System.Windows.Forms.Label
    Friend WithEvents roleFilter As System.Windows.Forms.ComboBox
    Friend WithEvents countLabel As System.Windows.Forms.Label
    Friend WithEvents usersGrid As System.Windows.Forms.DataGridView
    Friend WithEvents columnId As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents columnFullName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents columnBarcode As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents columnRoleName As System.Windows.Forms.DataGridViewTextBoxColumn
    Friend WithEvents columnIsActive As System.Windows.Forms.DataGridViewCheckBoxColumn
End Class
