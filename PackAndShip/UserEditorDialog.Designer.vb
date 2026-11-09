<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class UserEditorDialog
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
        Me.heading = New System.Windows.Forms.Label()
        Me.nameCaption = New System.Windows.Forms.Label()
        Me.inputName = New System.Windows.Forms.TextBox()
        Me.barcodeCaption = New System.Windows.Forms.Label()
        Me.inputBarcode = New System.Windows.Forms.TextBox()
        Me.roleCaption = New System.Windows.Forms.Label()
        Me.inputRole = New System.Windows.Forms.ComboBox()
        Me.inputActive = New System.Windows.Forms.CheckBox()
        Me.saveButton = New System.Windows.Forms.Button()
        Me.cancelButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        Me.heading.Name = "heading"
        Me.heading.Location = New System.Drawing.Point(24,15)
        Me.heading.Size = New System.Drawing.Size(420,42)
        Me.nameCaption.Name = "nameCaption"
        Me.nameCaption.Location = New System.Drawing.Point(24,77)
        Me.nameCaption.Size = New System.Drawing.Size(425,25)
        Me.inputName.Name = "inputName"
        Me.inputName.Location = New System.Drawing.Point(24,104)
        Me.inputName.Size = New System.Drawing.Size(430,30)
        Me.barcodeCaption.Name = "barcodeCaption"
        Me.barcodeCaption.Location = New System.Drawing.Point(24,146)
        Me.barcodeCaption.Size = New System.Drawing.Size(425,25)
        Me.inputBarcode.Name = "inputBarcode"
        Me.inputBarcode.Location = New System.Drawing.Point(24,173)
        Me.inputBarcode.Size = New System.Drawing.Size(430,30)
        Me.roleCaption.Name = "roleCaption"
        Me.roleCaption.Location = New System.Drawing.Point(24,216)
        Me.roleCaption.Size = New System.Drawing.Size(425,25)
        Me.inputRole.Name = "inputRole"
        Me.inputRole.Location = New System.Drawing.Point(24,243)
        Me.inputRole.Size = New System.Drawing.Size(430,30)
        Me.inputActive.Name = "inputActive"
        Me.inputActive.Location = New System.Drawing.Point(24,292)
        Me.inputActive.Size = New System.Drawing.Size(200,29)
        Me.saveButton.Name = "saveButton"
        Me.saveButton.Location = New System.Drawing.Point(333,334)
        Me.saveButton.Size = New System.Drawing.Size(121,37)
        Me.cancelButton.Name = "cancelButton"
        Me.cancelButton.Location = New System.Drawing.Point(200,334)
        Me.cancelButton.Size = New System.Drawing.Size(120,37)
        Me.heading.Text = "Add employee"
        Me.nameCaption.Text = "Employee name"
        Me.barcodeCaption.Text = "Barcode ID"
        Me.roleCaption.Text = "Role"
        Me.inputActive.Text = "Active user"
        Me.saveButton.Text = "Save"
        Me.cancelButton.Text = "Cancel"
        Me.heading.Font = New System.Drawing.Font("Segoe UI",19.0!,System.Drawing.FontStyle.Bold)
        Me.heading.ForeColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.inputName.MaxLength = 100
        Me.inputBarcode.MaxLength = 50
        Me.inputBarcode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.inputRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.inputActive.Checked = True
        Me.saveButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.saveButton.ForeColor = System.Drawing.Color.White
        Me.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.AcceptButton = Me.saveButton
        Me.CancelButton = Me.cancelButton
        Me.Controls.Add(Me.heading)
        Me.Controls.Add(Me.nameCaption)
        Me.Controls.Add(Me.inputName)
        Me.Controls.Add(Me.barcodeCaption)
        Me.Controls.Add(Me.inputBarcode)
        Me.Controls.Add(Me.roleCaption)
        Me.Controls.Add(Me.inputRole)
        Me.Controls.Add(Me.inputActive)
        Me.Controls.Add(Me.saveButton)
        Me.Controls.Add(Me.cancelButton)
        Me.ClientSize = New System.Drawing.Size(480,390)
        Me.MinimumSize = New System.Drawing.Size(0,0)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "New user"
        Me.BackColor = System.Drawing.Color.White
        Me.Font = New System.Drawing.Font("Segoe UI",11.0!)
        Me.ResumeLayout(False)
    End Sub
    Friend WithEvents heading As System.Windows.Forms.Label
    Friend WithEvents nameCaption As System.Windows.Forms.Label
    Friend WithEvents inputName As System.Windows.Forms.TextBox
    Friend WithEvents barcodeCaption As System.Windows.Forms.Label
    Friend WithEvents inputBarcode As System.Windows.Forms.TextBox
    Friend WithEvents roleCaption As System.Windows.Forms.Label
    Friend WithEvents inputRole As System.Windows.Forms.ComboBox
    Friend WithEvents inputActive As System.Windows.Forms.CheckBox
    Friend WithEvents saveButton As System.Windows.Forms.Button
    Friend WithEvents cancelButton As System.Windows.Forms.Button
End Class
