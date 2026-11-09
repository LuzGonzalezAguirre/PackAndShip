<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PackingConfirmationForm
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
        Me.band = New System.Windows.Forms.Panel()
        Me.symbol = New System.Windows.Forms.Label()
        Me.heading = New System.Windows.Forms.Label()
        Me.codeLabel = New System.Windows.Forms.Label()
        Me.number = New System.Windows.Forms.Label()
        Me.message = New System.Windows.Forms.Label()
        Me.noButton = New System.Windows.Forms.Button()
        Me.yesButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        Me.band.BackColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.band.Dock = System.Windows.Forms.DockStyle.Top
        Me.band.Height = 104
        Me.symbol.Text = "▣"
        Me.symbol.Font = New System.Drawing.Font("Segoe UI Symbol", 35.0!, System.Drawing.FontStyle.Bold)
        Me.symbol.ForeColor = System.Drawing.Color.White
        Me.symbol.Location = New System.Drawing.Point(28, 12)
        Me.symbol.Size = New System.Drawing.Size(76, 70)
        Me.symbol.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.heading.Text = "LEVEL READY TO CLOSE"
        Me.heading.Font = New System.Drawing.Font("Segoe UI", 19.0!, System.Drawing.FontStyle.Bold)
        Me.heading.ForeColor = System.Drawing.Color.White
        Me.heading.Location = New System.Drawing.Point(108, 22)
        Me.heading.Size = New System.Drawing.Size(470, 54)
        Me.heading.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        Me.band.Controls.Add(Me.symbol)
        Me.band.Controls.Add(Me.heading)
        Me.codeLabel.Text = "CONTAINER"
        Me.codeLabel.Location = New System.Drawing.Point(30, 124)
        Me.codeLabel.Size = New System.Drawing.Size(540, 31)
        Me.codeLabel.Font = New System.Drawing.Font("Segoe UI", 13.0!, System.Drawing.FontStyle.Bold)
        Me.codeLabel.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.number.Text = "0 / 0"
        Me.number.Location = New System.Drawing.Point(28, 163)
        Me.number.Size = New System.Drawing.Size(540, 66)
        Me.number.Font = New System.Drawing.Font("Segoe UI", 36.0!, System.Drawing.FontStyle.Bold)
        Me.number.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.number.ForeColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.message.Text = "Confirm physically packed pieces."
        Me.message.Location = New System.Drawing.Point(35, 238)
        Me.message.Size = New System.Drawing.Size(540, 66)
        Me.message.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        Me.noButton.Text = "NOT YET"
        Me.noButton.Location = New System.Drawing.Point(30, 328)
        Me.noButton.Size = New System.Drawing.Size(220, 53)
        Me.noButton.BackColor = System.Drawing.Color.White
        Me.noButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.noButton.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.yesButton.Text = "YES — NEXT LEVEL"
        Me.yesButton.Location = New System.Drawing.Point(268, 328)
        Me.yesButton.Size = New System.Drawing.Size(310, 53)
        Me.yesButton.ForeColor = System.Drawing.Color.White
        Me.yesButton.BackColor = System.Drawing.Color.FromArgb(31, 78, 121)
        Me.yesButton.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.yesButton.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.BackColor = System.Drawing.Color.FromArgb(244, 248, 252)
        Me.ClientSize = New System.Drawing.Size(610, 410)
        Me.Font = New System.Drawing.Font("Segoe UI", 11.0!)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.ShowInTaskbar = False
        Me.Controls.Add(Me.band)
        Me.Controls.Add(Me.codeLabel)
        Me.Controls.Add(Me.number)
        Me.Controls.Add(Me.message)
        Me.Controls.Add(Me.noButton)
        Me.Controls.Add(Me.yesButton)
        Me.AcceptButton = Me.yesButton
        Me.CancelButton = Me.noButton
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents band As System.Windows.Forms.Panel
    Friend WithEvents symbol As System.Windows.Forms.Label
    Friend WithEvents heading As System.Windows.Forms.Label
    Friend WithEvents codeLabel As System.Windows.Forms.Label
    Friend WithEvents number As System.Windows.Forms.Label
    Friend WithEvents message As System.Windows.Forms.Label
    Friend WithEvents noButton As System.Windows.Forms.Button
    Friend WithEvents yesButton As System.Windows.Forms.Button
End Class
