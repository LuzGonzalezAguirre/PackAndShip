<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ScanRejectionForm
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
        Me.alertBand = New System.Windows.Forms.Panel()
        Me.bigTitle = New System.Windows.Forms.Label()
        Me.containerLabel = New System.Windows.Forms.Label()
        Me.reasonLabel = New System.Windows.Forms.Label()
        Me.serialLabel = New System.Windows.Forms.Label()
        Me.pendingLabel = New System.Windows.Forms.Label()
        Me.approvalsGrid = New System.Windows.Forms.DataGridView()
        Me.departmentLabel = New System.Windows.Forms.Label()
        Me.departmentPicker = New System.Windows.Forms.ComboBox()
        Me.barcodeLabel = New System.Windows.Forms.Label()
        Me.barcodeInput = New System.Windows.Forms.TextBox()
        Me.passwordLabel = New System.Windows.Forms.Label()
        Me.passwordInput = New System.Windows.Forms.TextBox()
        Me.approveButton = New System.Windows.Forms.Button()
        Me.refreshButton = New System.Windows.Forms.Button()
        Me.exitButton = New System.Windows.Forms.Button()
        Me.alertBand.SuspendLayout()
        CType(Me.approvalsGrid, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        ' alertBand
        '
        Me.alertBand.BackColor = System.Drawing.Color.FromArgb(176, 27, 40)
        Me.alertBand.Location = New System.Drawing.Point(0, 0)
        Me.alertBand.Size = New System.Drawing.Size(1050, 155)
        Me.alertBand.Controls.Add(Me.bigTitle)
        Me.alertBand.Controls.Add(Me.containerLabel)
        '
        ' bigTitle
        '
        Me.bigTitle.Text = "PIEZA RECHAZADA — ALTO"
        Me.bigTitle.ForeColor = System.Drawing.Color.White
        Me.bigTitle.Font = New System.Drawing.Font("Segoe UI", 32.0!, System.Drawing.FontStyle.Bold)
        Me.bigTitle.Location = New System.Drawing.Point(30, 22)
        Me.bigTitle.Size = New System.Drawing.Size(980, 72)
        Me.bigTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        ' containerLabel
        '
        Me.containerLabel.Text = "CONTENEDOR:"
        Me.containerLabel.ForeColor = System.Drawing.Color.White
        Me.containerLabel.Font = New System.Drawing.Font("Segoe UI", 16.0!, System.Drawing.FontStyle.Bold)
        Me.containerLabel.Location = New System.Drawing.Point(35, 99)
        Me.containerLabel.Size = New System.Drawing.Size(970, 39)
        Me.containerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        ' reasonLabel
        '
        Me.reasonLabel.Text = "Motivo de rechazo"
        Me.reasonLabel.Font = New System.Drawing.Font("Segoe UI", 20.0!, System.Drawing.FontStyle.Bold)
        Me.reasonLabel.ForeColor = System.Drawing.Color.DarkRed
        Me.reasonLabel.Location = New System.Drawing.Point(35, 172)
        Me.reasonLabel.Size = New System.Drawing.Size(975, 104)
        '
        ' serialLabel
        '
        Me.serialLabel.Text = "Serie SSI / Serie cliente"
        Me.serialLabel.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.serialLabel.Location = New System.Drawing.Point(35, 280)
        Me.serialLabel.Size = New System.Drawing.Size(975, 60)
        '
        ' pendingLabel
        '
        Me.pendingLabel.Text = "ESCÁNER BLOQUEADO — REQUIERE AUTORIZACIÓN"
        Me.pendingLabel.Font = New System.Drawing.Font("Segoe UI", 17.0!, System.Drawing.FontStyle.Bold)
        Me.pendingLabel.ForeColor = System.Drawing.Color.DarkRed
        Me.pendingLabel.Location = New System.Drawing.Point(35, 348)
        Me.pendingLabel.Size = New System.Drawing.Size(975, 40)
        '
        ' approvalsGrid
        '
        Me.approvalsGrid.Location = New System.Drawing.Point(35, 397)
        Me.approvalsGrid.Size = New System.Drawing.Size(975, 170)
        Me.approvalsGrid.ReadOnly = True
        Me.approvalsGrid.AllowUserToAddRows = False
        Me.approvalsGrid.AllowUserToDeleteRows = False
        Me.approvalsGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.approvalsGrid.RowHeadersVisible = False
        '
        ' departmentLabel
        '
        Me.departmentLabel.Text = "Departamento que autoriza"
        Me.departmentLabel.Location = New System.Drawing.Point(35, 587)
        Me.departmentLabel.Size = New System.Drawing.Size(300, 24)
        '
        ' departmentPicker
        '
        Me.departmentPicker.Location = New System.Drawing.Point(35, 615)
        Me.departmentPicker.Size = New System.Drawing.Size(304, 30)
        Me.departmentPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        '
        ' barcodeLabel
        '
        Me.barcodeLabel.Text = "Credencial / Gafete"
        Me.barcodeLabel.Location = New System.Drawing.Point(356, 587)
        Me.barcodeLabel.Size = New System.Drawing.Size(300, 24)
        '
        ' barcodeInput
        '
        Me.barcodeInput.Location = New System.Drawing.Point(356, 615)
        Me.barcodeInput.Size = New System.Drawing.Size(303, 30)
        Me.barcodeInput.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.barcodeInput.MaxLength = 50
        '
        ' passwordLabel
        '
        Me.passwordLabel.Text = "Contraseña"
        Me.passwordLabel.Location = New System.Drawing.Point(675, 587)
        Me.passwordLabel.Size = New System.Drawing.Size(300, 24)
        '
        ' passwordInput
        '
        Me.passwordInput.Location = New System.Drawing.Point(675, 615)
        Me.passwordInput.Size = New System.Drawing.Size(335, 30)
        Me.passwordInput.UseSystemPasswordChar = True
        Me.passwordInput.MaxLength = 50
        '
        ' approveButton
        '
        Me.approveButton.Text = "AUTORIZAR DEPARTAMENTO"
        Me.approveButton.BackColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.approveButton.ForeColor = System.Drawing.Color.White
        Me.approveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.approveButton.Location = New System.Drawing.Point(610, 670)
        Me.approveButton.Size = New System.Drawing.Size(400, 52)
        '
        ' refreshButton
        '
        Me.refreshButton.Text = "Actualizar autorizaciones"
        Me.refreshButton.Location = New System.Drawing.Point(275, 670)
        Me.refreshButton.Size = New System.Drawing.Size(320, 52)
        '
        ' exitButton
        '
        Me.exitButton.Text = "Cerrar (permanece bloqueado)"
        Me.exitButton.Location = New System.Drawing.Point(35, 670)
        Me.exitButton.Size = New System.Drawing.Size(225, 52)
        Me.exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        '
        ' ScanRejectionForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(1050, 748)
        Me.MinimumSize = New System.Drawing.Size(800, 680)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "PackAndShip | Rechazo de pieza — aprobación requerida"
        Me.AcceptButton = Me.approveButton
        Me.CancelButton = Me.exitButton
        Me.Controls.Add(Me.alertBand)
        Me.Controls.Add(Me.reasonLabel)
        Me.Controls.Add(Me.serialLabel)
        Me.Controls.Add(Me.pendingLabel)
        Me.Controls.Add(Me.approvalsGrid)
        Me.Controls.Add(Me.departmentLabel)
        Me.Controls.Add(Me.departmentPicker)
        Me.Controls.Add(Me.barcodeLabel)
        Me.Controls.Add(Me.barcodeInput)
        Me.Controls.Add(Me.passwordLabel)
        Me.Controls.Add(Me.passwordInput)
        Me.Controls.Add(Me.approveButton)
        Me.Controls.Add(Me.refreshButton)
        Me.Controls.Add(Me.exitButton)
        Me.alertBand.ResumeLayout(False)
        CType(Me.approvalsGrid, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents alertBand As System.Windows.Forms.Panel
    Friend WithEvents bigTitle As System.Windows.Forms.Label
    Friend WithEvents containerLabel As System.Windows.Forms.Label
    Friend WithEvents reasonLabel As System.Windows.Forms.Label
    Friend WithEvents serialLabel As System.Windows.Forms.Label
    Friend WithEvents pendingLabel As System.Windows.Forms.Label
    Friend WithEvents approvalsGrid As System.Windows.Forms.DataGridView
    Friend WithEvents departmentLabel As System.Windows.Forms.Label
    Friend WithEvents departmentPicker As System.Windows.Forms.ComboBox
    Friend WithEvents barcodeLabel As System.Windows.Forms.Label
    Friend WithEvents barcodeInput As System.Windows.Forms.TextBox
    Friend WithEvents passwordLabel As System.Windows.Forms.Label
    Friend WithEvents passwordInput As System.Windows.Forms.TextBox
    Friend WithEvents approveButton As System.Windows.Forms.Button
    Friend WithEvents refreshButton As System.Windows.Forms.Button
    Friend WithEvents exitButton As System.Windows.Forms.Button
End Class
