<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ScanRuleEditorForm
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
        Me.headerTitle = New System.Windows.Forms.Label()
        Me.upper = New System.Windows.Forms.Panel()
        Me.customerCaption = New System.Windows.Forms.Label()
        Me.customerPicker = New System.Windows.Forms.ComboBox()
        Me.partCaption = New System.Windows.Forms.Label()
        Me.partPicker = New System.Windows.Forms.ComboBox()
        Me.countCaption = New System.Windows.Forms.Label()
        Me.scanCount = New System.Windows.Forms.NumericUpDown()
        Me.matchCheck = New System.Windows.Forms.CheckBox()
        Me.activeCheck = New System.Windows.Forms.CheckBox()
        Me.notesCaption = New System.Windows.Forms.Label()
        Me.notesBox = New System.Windows.Forms.TextBox()
        Me.scanContainer = New System.Windows.Forms.FlowLayoutPanel()
        Me.saveButton = New System.Windows.Forms.Button()
        Me.cancelButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        Me.header.Name = "header"
        Me.header.Location = New System.Drawing.Point(0,0)
        Me.header.Size = New System.Drawing.Size(1190,72)
        Me.headerTitle.Name = "headerTitle"
        Me.headerTitle.Location = New System.Drawing.Point(22,15)
        Me.headerTitle.Size = New System.Drawing.Size(800,42)
        Me.upper.Name = "upper"
        Me.upper.Location = New System.Drawing.Point(0,72)
        Me.upper.Size = New System.Drawing.Size(1190,234)
        Me.customerCaption.Name = "customerCaption"
        Me.customerCaption.Location = New System.Drawing.Point(20,12)
        Me.customerCaption.Size = New System.Drawing.Size(190,24)
        Me.customerPicker.Name = "customerPicker"
        Me.customerPicker.Location = New System.Drawing.Point(20,38)
        Me.customerPicker.Size = New System.Drawing.Size(280,33)
        Me.partCaption.Name = "partCaption"
        Me.partCaption.Location = New System.Drawing.Point(320,12)
        Me.partCaption.Size = New System.Drawing.Size(400,24)
        Me.partPicker.Name = "partPicker"
        Me.partPicker.Location = New System.Drawing.Point(320,38)
        Me.partPicker.Size = New System.Drawing.Size(520,33)
        Me.countCaption.Name = "countCaption"
        Me.countCaption.Location = New System.Drawing.Point(20,91)
        Me.countCaption.Size = New System.Drawing.Size(140,24)
        Me.scanCount.Name = "scanCount"
        Me.scanCount.Location = New System.Drawing.Point(20,116)
        Me.scanCount.Size = New System.Drawing.Size(110,33)
        Me.matchCheck.Name = "matchCheck"
        Me.matchCheck.Location = New System.Drawing.Point(163,114)
        Me.matchCheck.Size = New System.Drawing.Size(160,35)
        Me.activeCheck.Name = "activeCheck"
        Me.activeCheck.Location = New System.Drawing.Point(340,114)
        Me.activeCheck.Size = New System.Drawing.Size(130,35)
        Me.notesCaption.Name = "notesCaption"
        Me.notesCaption.Location = New System.Drawing.Point(20,165)
        Me.notesCaption.Size = New System.Drawing.Size(100,24)
        Me.notesBox.Name = "notesBox"
        Me.notesBox.Location = New System.Drawing.Point(20,187)
        Me.notesBox.Size = New System.Drawing.Size(1050,37)
        Me.scanContainer.Name = "scanContainer"
        Me.scanContainer.Location = New System.Drawing.Point(15,322)
        Me.scanContainer.Size = New System.Drawing.Size(1150,345)
        Me.saveButton.Name = "saveButton"
        Me.saveButton.Location = New System.Drawing.Point(960,677)
        Me.saveButton.Size = New System.Drawing.Size(158,39)
        Me.cancelButton.Name = "cancelButton"
        Me.cancelButton.Location = New System.Drawing.Point(805,677)
        Me.cancelButton.Size = New System.Drawing.Size(130,39)
        Me.headerTitle.Text = "Add Scan Rule"
        Me.customerCaption.Text = "Customer"
        Me.partCaption.Text = "Part number (SSI / Client)"
        Me.countCaption.Text = "Scan count"
        Me.matchCheck.Text = "Requires match"
        Me.activeCheck.Text = "Active"
        Me.notesCaption.Text = "Notes"
        Me.saveButton.Text = "Save rule"
        Me.cancelButton.Text = "Cancel"
        Me.header.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.headerTitle.Font = New System.Drawing.Font("Segoe UI",22.0!,System.Drawing.FontStyle.Bold)
        Me.headerTitle.ForeColor = System.Drawing.Color.White
        Me.customerPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.partPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.partPicker.DisplayMember = "DisplayName"
        Me.partPicker.ValueMember = "Id"
        Me.scanCount.Minimum = 1D
        Me.scanCount.Maximum = 100D
        Me.notesBox.Multiline = True
        Me.notesBox.MaxLength = 4000
        Me.scanContainer.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.scanContainer.WrapContents = False
        Me.scanContainer.AutoScroll = True
        Me.saveButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.saveButton.ForeColor = System.Drawing.Color.White
        Me.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.CancelButton = Me.cancelButton
        Me.header.Controls.Add(Me.headerTitle)
        Me.upper.Controls.Add(Me.customerCaption)
        Me.upper.Controls.Add(Me.customerPicker)
        Me.upper.Controls.Add(Me.partCaption)
        Me.upper.Controls.Add(Me.partPicker)
        Me.upper.Controls.Add(Me.countCaption)
        Me.upper.Controls.Add(Me.scanCount)
        Me.upper.Controls.Add(Me.matchCheck)
        Me.upper.Controls.Add(Me.activeCheck)
        Me.upper.Controls.Add(Me.notesCaption)
        Me.upper.Controls.Add(Me.notesBox)
        Me.Controls.Add(Me.header)
        Me.Controls.Add(Me.upper)
        Me.Controls.Add(Me.scanContainer)
        Me.Controls.Add(Me.saveButton)
        Me.Controls.Add(Me.cancelButton)
        Me.ClientSize = New System.Drawing.Size(1190,765)
        Me.MinimumSize = New System.Drawing.Size(0,0)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Add Scan Rule"
        Me.BackColor = System.Drawing.Color.FromArgb(244,247,250)
        Me.Font = New System.Drawing.Font("Segoe UI",10.0!)
        Me.ResumeLayout(False)
    End Sub
    Friend WithEvents header As System.Windows.Forms.Panel
    Friend WithEvents headerTitle As System.Windows.Forms.Label
    Friend WithEvents upper As System.Windows.Forms.Panel
    Friend WithEvents customerCaption As System.Windows.Forms.Label
    Friend WithEvents customerPicker As System.Windows.Forms.ComboBox
    Friend WithEvents partCaption As System.Windows.Forms.Label
    Friend WithEvents partPicker As System.Windows.Forms.ComboBox
    Friend WithEvents countCaption As System.Windows.Forms.Label
    Friend WithEvents scanCount As System.Windows.Forms.NumericUpDown
    Friend WithEvents matchCheck As System.Windows.Forms.CheckBox
    Friend WithEvents activeCheck As System.Windows.Forms.CheckBox
    Friend WithEvents notesCaption As System.Windows.Forms.Label
    Friend WithEvents notesBox As System.Windows.Forms.TextBox
    Friend WithEvents scanContainer As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents saveButton As System.Windows.Forms.Button
    Friend WithEvents cancelButton As System.Windows.Forms.Button
End Class
