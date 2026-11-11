<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CustomerConfigurationForm
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
        Me.headerPanel = New System.Windows.Forms.Panel()
        Me.titleLabel = New System.Windows.Forms.Label()
        Me.subtitleLabel = New System.Windows.Forms.Label()
        Me.customerCaption = New System.Windows.Forms.Label()
        Me.customerPicker = New System.Windows.Forms.ComboBox()
        Me.helpLabel = New System.Windows.Forms.Label()
        Me.saveButton = New System.Windows.Forms.Button()
        Me.dismissButton = New System.Windows.Forms.Button()
        Me.headerPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        ' headerPanel
        '
        Me.headerPanel.Name = "headerPanel"
        Me.headerPanel.BackColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.headerPanel.Location = New System.Drawing.Point(0, 0)
        Me.headerPanel.Size = New System.Drawing.Size(550, 82)
        Me.headerPanel.Controls.Add(Me.titleLabel)
        Me.headerPanel.Controls.Add(Me.subtitleLabel)
        '
        ' titleLabel
        '
        Me.titleLabel.Name = "titleLabel"
        Me.titleLabel.Text = "Configuración de cliente"
        Me.titleLabel.Font = New System.Drawing.Font("Segoe UI", 19.0!, System.Drawing.FontStyle.Bold)
        Me.titleLabel.ForeColor = System.Drawing.Color.White
        Me.titleLabel.Location = New System.Drawing.Point(23, 12)
        Me.titleLabel.Size = New System.Drawing.Size(495, 37)
        '
        ' subtitleLabel
        '
        Me.subtitleLabel.Name = "subtitleLabel"
        Me.subtitleLabel.Text = "Cliente predeterminado de esta estación"
        Me.subtitleLabel.ForeColor = System.Drawing.Color.FromArgb(220, 233, 247)
        Me.subtitleLabel.Location = New System.Drawing.Point(25, 54)
        Me.subtitleLabel.Size = New System.Drawing.Size(490, 23)
        '
        ' customerCaption
        '
        Me.customerCaption.Name = "customerCaption"
        Me.customerCaption.Text = "Cliente"
        Me.customerCaption.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold)
        Me.customerCaption.ForeColor = System.Drawing.Color.FromArgb(27, 39, 58)
        Me.customerCaption.Location = New System.Drawing.Point(24, 111)
        Me.customerCaption.Size = New System.Drawing.Size(475, 25)
        '
        ' customerPicker
        '
        Me.customerPicker.Name = "customerPicker"
        Me.customerPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.customerPicker.Font = New System.Drawing.Font("Segoe UI", 11.0!)
        Me.customerPicker.Location = New System.Drawing.Point(24, 141)
        Me.customerPicker.Size = New System.Drawing.Size(476, 33)
        Me.customerPicker.TabIndex = 0
        '
        ' helpLabel
        '
        Me.helpLabel.Name = "helpLabel"
        Me.helpLabel.Text = "Este cliente se guarda para la computadora actual."
        Me.helpLabel.ForeColor = System.Drawing.Color.FromArgb(93, 108, 126)
        Me.helpLabel.Location = New System.Drawing.Point(24, 192)
        Me.helpLabel.Size = New System.Drawing.Size(476, 44)
        '
        ' saveButton
        '
        Me.saveButton.Name = "saveButton"
        Me.saveButton.Text = "Guardar"
        Me.saveButton.BackColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.saveButton.ForeColor = System.Drawing.Color.White
        Me.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.saveButton.Location = New System.Drawing.Point(365, 260)
        Me.saveButton.Size = New System.Drawing.Size(145, 40)
        Me.saveButton.TabIndex = 1
        '
        ' dismissButton
        '
        Me.dismissButton.Name = "dismissButton"
        Me.dismissButton.Text = "Cancelar"
        Me.dismissButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.dismissButton.Location = New System.Drawing.Point(215, 260)
        Me.dismissButton.Size = New System.Drawing.Size(135, 40)
        Me.dismissButton.TabIndex = 2
        '
        ' CustomerConfigurationForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(244, 247, 250)
        Me.ClientSize = New System.Drawing.Size(550, 330)
        Me.MinimumSize = New System.Drawing.Size(0, 0)
        Me.MaximumSize = New System.Drawing.Size(0, 0)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.Text = "PackAndShip | Configuración de cliente"
        Me.AcceptButton = Me.saveButton
        Me.CancelButton = Me.dismissButton
        Me.Controls.Add(Me.headerPanel)
        Me.Controls.Add(Me.customerCaption)
        Me.Controls.Add(Me.customerPicker)
        Me.Controls.Add(Me.helpLabel)
        Me.Controls.Add(Me.dismissButton)
        Me.Controls.Add(Me.saveButton)
        Me.headerPanel.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents headerPanel As System.Windows.Forms.Panel
    Friend WithEvents titleLabel As System.Windows.Forms.Label
    Friend WithEvents subtitleLabel As System.Windows.Forms.Label
    Friend WithEvents customerCaption As System.Windows.Forms.Label
    Friend WithEvents customerPicker As System.Windows.Forms.ComboBox
    Friend WithEvents helpLabel As System.Windows.Forms.Label
    Friend WithEvents saveButton As System.Windows.Forms.Button
    Friend WithEvents dismissButton As System.Windows.Forms.Button
End Class
