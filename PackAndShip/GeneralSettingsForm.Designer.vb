<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class GeneralSettingsForm
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
        Me.section = New System.Windows.Forms.Panel()
        Me.sectionHeading = New System.Windows.Forms.Label()
        Me.prefixCaption = New System.Windows.Forms.Label()
        Me.prefixInput = New System.Windows.Forms.TextBox()
        Me.digitsCaption = New System.Windows.Forms.Label()
        Me.digitsInput = New System.Windows.Forms.NumericUpDown()
        Me.exampleLabel = New System.Windows.Forms.Label()
        Me.testCaption = New System.Windows.Forms.Label()
        Me.testInput = New System.Windows.Forms.TextBox()
        Me.testResult = New System.Windows.Forms.Label()
        Me.note = New System.Windows.Forms.Label()
        Me.saveButton = New System.Windows.Forms.Button()
        Me.dismissButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        Me.header.Name = "header"
        Me.header.Location = New System.Drawing.Point(0,0)
        Me.header.Size = New System.Drawing.Size(750,87)
        Me.heading.Name = "heading"
        Me.heading.Location = New System.Drawing.Point(22,11)
        Me.heading.Size = New System.Drawing.Size(570,38)
        Me.subtitle.Name = "subtitle"
        Me.subtitle.Location = New System.Drawing.Point(25,59)
        Me.subtitle.Size = New System.Drawing.Size(640,23)
        Me.section.Name = "section"
        Me.section.Location = New System.Drawing.Point(24,100)
        Me.section.Size = New System.Drawing.Size(690,355)
        Me.sectionHeading.Name = "sectionHeading"
        Me.sectionHeading.Location = New System.Drawing.Point(24,21)
        Me.sectionHeading.Size = New System.Drawing.Size(440,30)
        Me.prefixCaption.Name = "prefixCaption"
        Me.prefixCaption.Location = New System.Drawing.Point(24,75)
        Me.prefixCaption.Size = New System.Drawing.Size(160,23)
        Me.prefixInput.Name = "prefixInput"
        Me.prefixInput.Location = New System.Drawing.Point(24,101)
        Me.prefixInput.Size = New System.Drawing.Size(155,33)
        Me.digitsCaption.Name = "digitsCaption"
        Me.digitsCaption.Location = New System.Drawing.Point(207,75)
        Me.digitsCaption.Size = New System.Drawing.Size(160,23)
        Me.digitsInput.Name = "digitsInput"
        Me.digitsInput.Location = New System.Drawing.Point(207,101)
        Me.digitsInput.Size = New System.Drawing.Size(155,33)
        Me.exampleLabel.Name = "exampleLabel"
        Me.exampleLabel.Location = New System.Drawing.Point(24,160)
        Me.exampleLabel.Size = New System.Drawing.Size(590,37)
        Me.testCaption.Name = "testCaption"
        Me.testCaption.Location = New System.Drawing.Point(24,220)
        Me.testCaption.Size = New System.Drawing.Size(300,23)
        Me.testInput.Name = "testInput"
        Me.testInput.Location = New System.Drawing.Point(24,247)
        Me.testInput.Size = New System.Drawing.Size(330,33)
        Me.testResult.Name = "testResult"
        Me.testResult.Location = New System.Drawing.Point(368,252)
        Me.testResult.Size = New System.Drawing.Size(230,32)
        Me.note.Name = "note"
        Me.note.Location = New System.Drawing.Point(24,316)
        Me.note.Size = New System.Drawing.Size(595,55)
        Me.saveButton.Name = "saveButton"
        Me.saveButton.Location = New System.Drawing.Point(535,476)
        Me.saveButton.Size = New System.Drawing.Size(150,40)
        Me.dismissButton.Name = "dismissButton"
        Me.dismissButton.Location = New System.Drawing.Point(390,476)
        Me.dismissButton.Size = New System.Drawing.Size(130,40)
        Me.heading.Text = "Configuración general"
        Me.subtitle.Text = "Formato global de ID de pallet • Compartido por todas las estaciones"
        Me.sectionHeading.Text = "IDENTIFICADOR DE PALLET"
        Me.prefixCaption.Text = "Prefijo"
        Me.digitsCaption.Text = "Dígitos numéricos"
        Me.testCaption.Text = "Probar ID de pallet escaneado"
        Me.note.Text = "Esta opción valida el formato del ID de pallet; no genera números ni modifica registros existentes."
        Me.saveButton.Text = "Guardar cambios"
        Me.dismissButton.Text = "Cerrar"
        Me.header.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.section.BackColor = System.Drawing.Color.White
        Me.heading.Font = New System.Drawing.Font("Segoe UI", 22.0!, System.Drawing.FontStyle.Bold)
        Me.heading.ForeColor = System.Drawing.Color.White
        Me.subtitle.ForeColor = System.Drawing.Color.White
        Me.sectionHeading.Font = New System.Drawing.Font("Segoe UI", 12.0!, System.Drawing.FontStyle.Bold)
        Me.exampleLabel.Font = New System.Drawing.Font("Consolas", 15.0!, System.Drawing.FontStyle.Bold)
        Me.prefixInput.MaxLength = 5
        Me.prefixInput.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.digitsInput.Minimum = 1D
        Me.digitsInput.Maximum = 12D
        Me.saveButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.saveButton.ForeColor = System.Drawing.Color.White
        Me.dismissButton.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.CancelButton = Me.dismissButton
        Me.header.Controls.Add(Me.heading)
        Me.header.Controls.Add(Me.subtitle)
        Me.section.Controls.Add(Me.sectionHeading)
        Me.section.Controls.Add(Me.prefixCaption)
        Me.section.Controls.Add(Me.prefixInput)
        Me.section.Controls.Add(Me.digitsCaption)
        Me.section.Controls.Add(Me.digitsInput)
        Me.section.Controls.Add(Me.exampleLabel)
        Me.section.Controls.Add(Me.testCaption)
        Me.section.Controls.Add(Me.testInput)
        Me.section.Controls.Add(Me.testResult)
        Me.section.Controls.Add(Me.note)
        Me.Controls.Add(Me.header)
        Me.Controls.Add(Me.section)
        Me.Controls.Add(Me.saveButton)
        Me.Controls.Add(Me.dismissButton)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(750,560)
        Me.MinimumSize = New System.Drawing.Size(0,0)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Font = New System.Drawing.Font("Segoe UI",10.0!)
        Me.BackColor = System.Drawing.Color.FromArgb(244,247,250)
        Me.Text = "PackAndShip | Configuración general"
        Me.ResumeLayout(False)
    End Sub
    Friend WithEvents header As System.Windows.Forms.Panel
    Friend WithEvents heading As System.Windows.Forms.Label
    Friend WithEvents subtitle As System.Windows.Forms.Label
    Friend WithEvents section As System.Windows.Forms.Panel
    Friend WithEvents sectionHeading As System.Windows.Forms.Label
    Friend WithEvents prefixCaption As System.Windows.Forms.Label
    Friend WithEvents prefixInput As System.Windows.Forms.TextBox
    Friend WithEvents digitsCaption As System.Windows.Forms.Label
    Friend WithEvents digitsInput As System.Windows.Forms.NumericUpDown
    Friend WithEvents exampleLabel As System.Windows.Forms.Label
    Friend WithEvents testCaption As System.Windows.Forms.Label
    Friend WithEvents testInput As System.Windows.Forms.TextBox
    Friend WithEvents testResult As System.Windows.Forms.Label
    Friend WithEvents note As System.Windows.Forms.Label
    Friend WithEvents saveButton As System.Windows.Forms.Button
    Friend WithEvents dismissButton As System.Windows.Forms.Button
End Class
