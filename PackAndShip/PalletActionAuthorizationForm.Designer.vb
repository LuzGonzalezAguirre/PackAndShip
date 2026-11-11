<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PalletActionAuthorizationForm
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
        Me.palletLabel = New System.Windows.Forms.Label()
        Me.reasonLabel = New System.Windows.Forms.Label()
        Me.reasonInput = New System.Windows.Forms.TextBox()
        Me.startButton = New System.Windows.Forms.Button()
        Me.approvalsGrid = New System.Windows.Forms.DataGridView()
        Me.progressLabel = New System.Windows.Forms.Label()
        Me.departmentLabel = New System.Windows.Forms.Label()
        Me.departmentPicker = New System.Windows.Forms.ComboBox()
        Me.barcodeLabel = New System.Windows.Forms.Label()
        Me.barcodeInput = New System.Windows.Forms.TextBox()
        Me.passwordLabel = New System.Windows.Forms.Label()
        Me.passwordInput = New System.Windows.Forms.TextBox()
        Me.approveButton = New System.Windows.Forms.Button()
        Me.completeButton = New System.Windows.Forms.Button()
        Me.refreshButton = New System.Windows.Forms.Button()
        Me.dismissButton = New System.Windows.Forms.Button()
        CType(Me.approvalsGrid, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()

        Me.titleLabel.Name = "titleLabel"
        Me.titleLabel.Text = "AUTORIZACIONES DE PALLET"
        Me.titleLabel.Font = New System.Drawing.Font("Segoe UI",22.0!,System.Drawing.FontStyle.Bold)
        Me.titleLabel.ForeColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.titleLabel.Location = New System.Drawing.Point(22,15)
        Me.titleLabel.Size = New System.Drawing.Size(940,55)

        Me.palletLabel.Name = "palletLabel"
        Me.palletLabel.Location = New System.Drawing.Point(25,75)
        Me.palletLabel.Size = New System.Drawing.Size(925,30)
        Me.palletLabel.Font = New System.Drawing.Font("Segoe UI",13.0!,System.Drawing.FontStyle.Bold)

        Me.reasonLabel.Name = "reasonLabel"
        Me.reasonLabel.Text = "Motivo obligatorio (mínimo 10 caracteres)"
        Me.reasonLabel.Location = New System.Drawing.Point(25,120)
        Me.reasonLabel.Size = New System.Drawing.Size(670,26)

        Me.reasonInput.Name = "reasonInput"
        Me.reasonInput.Multiline = True
        Me.reasonInput.MaxLength = 500
        Me.reasonInput.Location = New System.Drawing.Point(25,151)
        Me.reasonInput.Size = New System.Drawing.Size(730,73)

        Me.startButton.Name = "startButton"
        Me.startButton.Text = "Solicitar autorizaciones"
        Me.startButton.Location = New System.Drawing.Point(775,159)
        Me.startButton.Size = New System.Drawing.Size(185,51)

        Me.progressLabel.Name = "progressLabel"
        Me.progressLabel.Text = "Pendiente de iniciar solicitud"
        Me.progressLabel.Font = New System.Drawing.Font("Segoe UI",16.0!,System.Drawing.FontStyle.Bold)
        Me.progressLabel.ForeColor = System.Drawing.Color.DarkRed
        Me.progressLabel.Location = New System.Drawing.Point(25,247)
        Me.progressLabel.Size = New System.Drawing.Size(924,43)

        Me.approvalsGrid.Name = "approvalsGrid"
        Me.approvalsGrid.Location = New System.Drawing.Point(25,299)
        Me.approvalsGrid.Size = New System.Drawing.Size(933,174)
        Me.approvalsGrid.ReadOnly = True
        Me.approvalsGrid.AllowUserToAddRows = False
        Me.approvalsGrid.AllowUserToDeleteRows = False
        Me.approvalsGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.approvalsGrid.RowHeadersVisible = False

        Me.departmentLabel.Name = "departmentLabel"
        Me.departmentLabel.Text = "Departamento"
        Me.departmentLabel.Location = New System.Drawing.Point(25,490)
        Me.departmentLabel.Size = New System.Drawing.Size(280,24)

        Me.departmentPicker.Name = "departmentPicker"
        Me.departmentPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.departmentPicker.Location = New System.Drawing.Point(25,520)
        Me.departmentPicker.Size = New System.Drawing.Size(290,30)

        Me.barcodeLabel.Name = "barcodeLabel"
        Me.barcodeLabel.Text = "Gafete del autorizador"
        Me.barcodeLabel.Location = New System.Drawing.Point(330,490)
        Me.barcodeLabel.Size = New System.Drawing.Size(280,24)

        Me.barcodeInput.Name = "barcodeInput"
        Me.barcodeInput.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.barcodeInput.MaxLength = 50
        Me.barcodeInput.Location = New System.Drawing.Point(330,520)
        Me.barcodeInput.Size = New System.Drawing.Size(290,30)

        Me.passwordLabel.Name = "passwordLabel"
        Me.passwordLabel.Text = "Contraseña"
        Me.passwordLabel.Location = New System.Drawing.Point(635,490)
        Me.passwordLabel.Size = New System.Drawing.Size(290,24)

        Me.passwordInput.Name = "passwordInput"
        Me.passwordInput.UseSystemPasswordChar = True
        Me.passwordInput.MaxLength = 50
        Me.passwordInput.Location = New System.Drawing.Point(635,520)
        Me.passwordInput.Size = New System.Drawing.Size(323,30)

        Me.approveButton.Name = "approveButton"
        Me.approveButton.Text = "Aprobar departamento"
        Me.approveButton.Location = New System.Drawing.Point(675,571)
        Me.approveButton.Size = New System.Drawing.Size(283,45)
        Me.approveButton.Enabled = False

        Me.completeButton.Name = "completeButton"
        Me.completeButton.Text = "EJECUTAR ACCIÓN AUTORIZADA"
        Me.completeButton.Location = New System.Drawing.Point(610,643)
        Me.completeButton.Size = New System.Drawing.Size(348,55)
        Me.completeButton.Enabled = False

        Me.refreshButton.Name = "refreshButton"
        Me.refreshButton.Text = "Actualizar aprobaciones"
        Me.refreshButton.Location = New System.Drawing.Point(271,643)
        Me.refreshButton.Size = New System.Drawing.Size(318,55)

        Me.dismissButton.Name = "dismissButton"
        Me.dismissButton.Text = "Cerrar sin ejecutar"
        Me.dismissButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.dismissButton.Location = New System.Drawing.Point(25,643)
        Me.dismissButton.Size = New System.Drawing.Size(225,55)

        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(984,727)
        Me.MinimumSize = New System.Drawing.Size(830,720)
        Me.Font = New System.Drawing.Font("Segoe UI",10.0!)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.BackColor = System.Drawing.Color.White
        Me.Text = "PackAndShip | Autorizaciones de pallet"
        Me.CancelButton = Me.dismissButton
        Me.AcceptButton = Me.approveButton
        Me.Controls.Add(Me.titleLabel)
        Me.Controls.Add(Me.palletLabel)
        Me.Controls.Add(Me.reasonLabel)
        Me.Controls.Add(Me.reasonInput)
        Me.Controls.Add(Me.startButton)
        Me.Controls.Add(Me.progressLabel)
        Me.Controls.Add(Me.approvalsGrid)
        Me.Controls.Add(Me.departmentLabel)
        Me.Controls.Add(Me.departmentPicker)
        Me.Controls.Add(Me.barcodeLabel)
        Me.Controls.Add(Me.barcodeInput)
        Me.Controls.Add(Me.passwordLabel)
        Me.Controls.Add(Me.passwordInput)
        Me.Controls.Add(Me.approveButton)
        Me.Controls.Add(Me.completeButton)
        Me.Controls.Add(Me.refreshButton)
        Me.Controls.Add(Me.dismissButton)
        CType(Me.approvalsGrid, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents titleLabel As System.Windows.Forms.Label
    Friend WithEvents palletLabel As System.Windows.Forms.Label
    Friend WithEvents reasonLabel As System.Windows.Forms.Label
    Friend WithEvents reasonInput As System.Windows.Forms.TextBox
    Friend WithEvents startButton As System.Windows.Forms.Button
    Friend WithEvents approvalsGrid As System.Windows.Forms.DataGridView
    Friend WithEvents progressLabel As System.Windows.Forms.Label
    Friend WithEvents departmentLabel As System.Windows.Forms.Label
    Friend WithEvents departmentPicker As System.Windows.Forms.ComboBox
    Friend WithEvents barcodeLabel As System.Windows.Forms.Label
    Friend WithEvents barcodeInput As System.Windows.Forms.TextBox
    Friend WithEvents passwordLabel As System.Windows.Forms.Label
    Friend WithEvents passwordInput As System.Windows.Forms.TextBox
    Friend WithEvents approveButton As System.Windows.Forms.Button
    Friend WithEvents completeButton As System.Windows.Forms.Button
    Friend WithEvents refreshButton As System.Windows.Forms.Button
    Friend WithEvents dismissButton As System.Windows.Forms.Button
End Class
