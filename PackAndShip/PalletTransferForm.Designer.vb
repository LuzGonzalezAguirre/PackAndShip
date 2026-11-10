<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PalletTransferForm
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
        Me.sourceInput = New System.Windows.Forms.TextBox()
        Me.destinationInput = New System.Windows.Forms.TextBox()
        Me.summary = New System.Windows.Forms.Label()
        Me.transferButton = New System.Windows.Forms.Button()
        Me.scanNext = New System.Windows.Forms.Label()
        Me.heading = New System.Windows.Forms.Label()
        Me.subtitle = New System.Windows.Forms.Label()
        Me.sourceLabel = New System.Windows.Forms.Label()
        Me.targetLabel = New System.Windows.Forms.Label()
        Me.cancel = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        Me.heading.Text = "TRANSFERENCIA DE PALLET"
        Me.heading.Location = New System.Drawing.Point(24, 18)
        Me.heading.Size = New System.Drawing.Size(620, 38)
        Me.subtitle.Text = "Transfiere un pallet parcial cerrado a un contenedor nuevo."
        Me.subtitle.Location = New System.Drawing.Point(25, 62)
        Me.subtitle.Size = New System.Drawing.Size(620, 26)
        Me.sourceLabel.Text = "ESCANEA PALLET ORIGINAL (PARCIAL)"
        Me.sourceLabel.Location = New System.Drawing.Point(25, 107)
        Me.sourceLabel.Size = New System.Drawing.Size(360, 24)
        Me.sourceInput.Location = New System.Drawing.Point(25, 133)
        Me.sourceInput.Size = New System.Drawing.Size(620, 38)
        Me.targetLabel.Text = "ESCANEA CONTENEDOR NUEVO"
        Me.targetLabel.Location = New System.Drawing.Point(25, 188)
        Me.targetLabel.Size = New System.Drawing.Size(360, 24)
        Me.destinationInput.Location = New System.Drawing.Point(25, 216)
        Me.destinationInput.Size = New System.Drawing.Size(620, 38)
        Me.summary.Text = "Escanea el pallet de origen para consultar las cantidades en SQL."
        Me.summary.Location = New System.Drawing.Point(25, 270)
        Me.summary.Size = New System.Drawing.Size(620, 57)
        Me.scanNext.Text = "Las piezas transferidas mantienen su trazabilidad al origen."
        Me.scanNext.Location = New System.Drawing.Point(25, 332)
        Me.scanNext.Size = New System.Drawing.Size(385, 24)
        Me.transferButton.Text = "Transferir y abrir pallet nuevo"
        Me.transferButton.Location = New System.Drawing.Point(390, 365)
        Me.transferButton.Size = New System.Drawing.Size(255, 44)
        Me.cancel.Text = "Cancelar"
        Me.cancel.Location = New System.Drawing.Point(250, 365)
        Me.cancel.Size = New System.Drawing.Size(120, 44)
        Me.heading.Font = New System.Drawing.Font("Segoe UI", 20.0!, System.Drawing.FontStyle.Bold)
        Me.heading.ForeColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.sourceInput.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.destinationInput.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.sourceInput.MaxLength = 18
        Me.destinationInput.MaxLength = 18
        Me.sourceInput.Font = New System.Drawing.Font("Consolas", 14.0!)
        Me.destinationInput.Font = New System.Drawing.Font("Consolas", 14.0!)
        Me.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.CancelButton = Me.cancel
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(690, 440)
        Me.MinimumSize = New System.Drawing.Size(0,0)
        Me.MaximumSize = New System.Drawing.Size(0,0)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Font = New System.Drawing.Font("Segoe UI", 10.0!)
        Me.BackColor = System.Drawing.Color.FromArgb(244,247,250)
        Me.Text = "PalletTransferForm"
        Me.Controls.Add(Me.sourceInput)
        Me.Controls.Add(Me.destinationInput)
        Me.Controls.Add(Me.summary)
        Me.Controls.Add(Me.transferButton)
        Me.Controls.Add(Me.scanNext)
        Me.Controls.Add(Me.heading)
        Me.Controls.Add(Me.subtitle)
        Me.Controls.Add(Me.sourceLabel)
        Me.Controls.Add(Me.targetLabel)
        Me.Controls.Add(Me.cancel)
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub
    Friend WithEvents sourceInput As System.Windows.Forms.TextBox
    Friend WithEvents destinationInput As System.Windows.Forms.TextBox
    Friend WithEvents summary As System.Windows.Forms.Label
    Friend WithEvents transferButton As System.Windows.Forms.Button
    Friend WithEvents scanNext As System.Windows.Forms.Label
    Friend WithEvents heading As System.Windows.Forms.Label
    Friend WithEvents subtitle As System.Windows.Forms.Label
    Friend WithEvents sourceLabel As System.Windows.Forms.Label
    Friend WithEvents targetLabel As System.Windows.Forms.Label
    Friend WithEvents cancel As System.Windows.Forms.Button
End Class
