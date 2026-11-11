<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class RoleEditorForm
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
        Me.titleLabel = New System.Windows.Forms.Label()
        Me.nameCaption = New System.Windows.Forms.Label()
        Me.nameInput = New System.Windows.Forms.TextBox()
        Me.setupCheckbox = New System.Windows.Forms.CheckBox()
        Me.permissionHelp = New System.Windows.Forms.Label()
        Me.saveButton = New System.Windows.Forms.Button()
        Me.cancelButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        ' titleLabel
        '
        Me.titleLabel.Name = "titleLabel"
        Me.titleLabel.Text = "Agregar rol"
        Me.titleLabel.Font = New System.Drawing.Font("Segoe UI", 18.0!, System.Drawing.FontStyle.Bold)
        Me.titleLabel.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.titleLabel.Location = New System.Drawing.Point(22, 18)
        Me.titleLabel.Size = New System.Drawing.Size(410, 43)
        '
        ' nameCaption
        '
        Me.nameCaption.Name = "nameCaption"
        Me.nameCaption.Text = "Nombre del rol"
        Me.nameCaption.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.nameCaption.Location = New System.Drawing.Point(25, 84)
        Me.nameCaption.Size = New System.Drawing.Size(398, 25)
        '
        ' nameInput
        '
        Me.nameInput.Name = "nameInput"
        Me.nameInput.Location = New System.Drawing.Point(25, 112)
        Me.nameInput.Size = New System.Drawing.Size(398, 30)
        Me.nameInput.MaxLength = 50
        Me.nameInput.TabIndex = 0
        '
        ' setupCheckbox
        '
        Me.setupCheckbox.Name = "setupCheckbox"
        Me.setupCheckbox.Text = "Puede modificar Configuración"
        Me.setupCheckbox.AutoSize = True
        Me.setupCheckbox.Location = New System.Drawing.Point(25, 170)
        Me.setupCheckbox.TabIndex = 1
        '
        ' permissionHelp
        '
        Me.permissionHelp.Name = "permissionHelp"
        Me.permissionHelp.Text = "Habilita la administración de usuarios, reglas, pallets y otros ajustes."
        Me.permissionHelp.Location = New System.Drawing.Point(25, 201)
        Me.permissionHelp.Size = New System.Drawing.Size(399, 54)
        Me.permissionHelp.ForeColor = System.Drawing.Color.FromArgb(93, 108, 126)
        '
        ' saveButton
        '
        Me.saveButton.Name = "saveButton"
        Me.saveButton.Text = "Guardar"
        Me.saveButton.BackColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.saveButton.ForeColor = System.Drawing.Color.White
        Me.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.saveButton.Location = New System.Drawing.Point(281, 273)
        Me.saveButton.Size = New System.Drawing.Size(140, 40)
        Me.saveButton.TabIndex = 2
        '
        ' cancelButton
        '
        Me.cancelButton.Name = "cancelButton"
        Me.cancelButton.Text = "Cancelar"
        Me.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.cancelButton.Location = New System.Drawing.Point(128, 273)
        Me.cancelButton.Size = New System.Drawing.Size(140, 40)
        Me.cancelButton.TabIndex = 3
        '
        ' RoleEditorForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(450, 340)
        Me.MinimumSize = New System.Drawing.Size(0, 0)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "PackAndShip | Agregar rol"
        Me.AcceptButton = Me.saveButton
        Me.CancelButton = Me.cancelButton
        Me.Controls.Add(Me.titleLabel)
        Me.Controls.Add(Me.nameCaption)
        Me.Controls.Add(Me.nameInput)
        Me.Controls.Add(Me.setupCheckbox)
        Me.Controls.Add(Me.permissionHelp)
        Me.Controls.Add(Me.saveButton)
        Me.Controls.Add(Me.cancelButton)
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents titleLabel As System.Windows.Forms.Label
    Friend WithEvents nameCaption As System.Windows.Forms.Label
    Friend WithEvents nameInput As System.Windows.Forms.TextBox
    Friend WithEvents setupCheckbox As System.Windows.Forms.CheckBox
    Friend WithEvents permissionHelp As System.Windows.Forms.Label
    Friend WithEvents saveButton As System.Windows.Forms.Button
    Friend WithEvents cancelButton As System.Windows.Forms.Button
End Class
