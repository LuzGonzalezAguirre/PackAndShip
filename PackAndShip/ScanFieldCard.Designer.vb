<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ScanFieldCard
    Inherits System.Windows.Forms.UserControl
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
        Me.fieldLabel = New System.Windows.Forms.TextBox()
        Me.target = New System.Windows.Forms.ComboBox()
        Me.mode = New System.Windows.Forms.ComboBox()
        Me.remove = New System.Windows.Forms.Button()
        Me.separator = New System.Windows.Forms.ComboBox()
        Me.position = New System.Windows.Forms.ComboBox()
        Me.prefix = New System.Windows.Forms.TextBox()
        Me.length = New System.Windows.Forms.TextBox()
        Me.customSeparator = New System.Windows.Forms.TextBox()
        Me.segment = New System.Windows.Forms.TextBox()
        Me.order = New System.Windows.Forms.NumericUpDown()
        Me.preview = New System.Windows.Forms.Label()
        Me.fieldCaption = New System.Windows.Forms.Label()
        Me.targetCaption = New System.Windows.Forms.Label()
        Me.modeCaption = New System.Windows.Forms.Label()
        Me.separatorCaption = New System.Windows.Forms.Label()
        Me.positionCaption = New System.Windows.Forms.Label()
        Me.prefixCaption = New System.Windows.Forms.Label()
        Me.lengthCaption = New System.Windows.Forms.Label()
        Me.customCaption = New System.Windows.Forms.Label()
        Me.segmentCaption = New System.Windows.Forms.Label()
        Me.orderCaption = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        Me.fieldLabel.Name = "fieldLabel"
        Me.fieldLabel.Location = New System.Drawing.Point(12,36)
        Me.fieldLabel.Size = New System.Drawing.Size(190,30)
        Me.target.Name = "target"
        Me.target.Location = New System.Drawing.Point(213,36)
        Me.target.Size = New System.Drawing.Size(225,30)
        Me.mode.Name = "mode"
        Me.mode.Location = New System.Drawing.Point(451,36)
        Me.mode.Size = New System.Drawing.Size(270,30)
        Me.remove.Name = "remove"
        Me.remove.Location = New System.Drawing.Point(734,36)
        Me.remove.Size = New System.Drawing.Size(39,30)
        Me.separator.Name = "separator"
        Me.separator.Location = New System.Drawing.Point(12,102)
        Me.separator.Size = New System.Drawing.Size(149,30)
        Me.position.Name = "position"
        Me.position.Location = New System.Drawing.Point(174,102)
        Me.position.Size = New System.Drawing.Size(160,30)
        Me.prefix.Name = "prefix"
        Me.prefix.Location = New System.Drawing.Point(345,102)
        Me.prefix.Size = New System.Drawing.Size(260,30)
        Me.length.Name = "length"
        Me.length.Location = New System.Drawing.Point(617,102)
        Me.length.Size = New System.Drawing.Size(103,30)
        Me.customSeparator.Name = "customSeparator"
        Me.customSeparator.Location = New System.Drawing.Point(12,167)
        Me.customSeparator.Size = New System.Drawing.Size(165,30)
        Me.segment.Name = "segment"
        Me.segment.Location = New System.Drawing.Point(190,167)
        Me.segment.Size = New System.Drawing.Size(125,30)
        Me.order.Name = "order"
        Me.order.Location = New System.Drawing.Point(330,167)
        Me.order.Size = New System.Drawing.Size(85,30)
        Me.preview.Name = "preview"
        Me.preview.Location = New System.Drawing.Point(432,151)
        Me.preview.Size = New System.Drawing.Size(440,44)
        Me.fieldCaption.Name = "fieldCaption"
        Me.fieldCaption.Location = New System.Drawing.Point(12,12)
        Me.fieldCaption.Size = New System.Drawing.Size(160,20)
        Me.targetCaption.Name = "targetCaption"
        Me.targetCaption.Location = New System.Drawing.Point(213,12)
        Me.targetCaption.Size = New System.Drawing.Size(160,20)
        Me.modeCaption.Name = "modeCaption"
        Me.modeCaption.Location = New System.Drawing.Point(451,12)
        Me.modeCaption.Size = New System.Drawing.Size(160,20)
        Me.separatorCaption.Name = "separatorCaption"
        Me.separatorCaption.Location = New System.Drawing.Point(12,79)
        Me.separatorCaption.Size = New System.Drawing.Size(160,20)
        Me.positionCaption.Name = "positionCaption"
        Me.positionCaption.Location = New System.Drawing.Point(174,79)
        Me.positionCaption.Size = New System.Drawing.Size(160,20)
        Me.prefixCaption.Name = "prefixCaption"
        Me.prefixCaption.Location = New System.Drawing.Point(345,79)
        Me.prefixCaption.Size = New System.Drawing.Size(160,20)
        Me.lengthCaption.Name = "lengthCaption"
        Me.lengthCaption.Location = New System.Drawing.Point(617,79)
        Me.lengthCaption.Size = New System.Drawing.Size(160,20)
        Me.customCaption.Name = "customCaption"
        Me.customCaption.Location = New System.Drawing.Point(12,145)
        Me.customCaption.Size = New System.Drawing.Size(160,20)
        Me.segmentCaption.Name = "segmentCaption"
        Me.segmentCaption.Location = New System.Drawing.Point(190,145)
        Me.segmentCaption.Size = New System.Drawing.Size(160,20)
        Me.orderCaption.Name = "orderCaption"
        Me.orderCaption.Location = New System.Drawing.Point(330,145)
        Me.orderCaption.Size = New System.Drawing.Size(160,20)
        Me.remove.Text = "×"
        Me.fieldCaption.Text = "Etiqueta de campo"
        Me.targetCaption.Text = "Campo de destino"
        Me.modeCaption.Text = "Modo de extracción"
        Me.separatorCaption.Text = "Separador"
        Me.positionCaption.Text = "Extraer valor"
        Me.prefixCaption.Text = "Prefijo conocido"
        Me.lengthCaption.Text = "Longitud esperada"
        Me.customCaption.Text = "Separador personalizado"
        Me.segmentCaption.Text = "Índice de segmento"
        Me.orderCaption.Text = "Orden"
        Me.target.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown
        Me.mode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown
        Me.separator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown
        Me.position.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown
        Me.target.Items.AddRange(New Object() {"Internal serial (frameSN)", "Customer serial (volvoSerialNumber)"})
        Me.mode.Items.AddRange(New Object() {"Split by separator"})
        Me.separator.Items.AddRange(New Object() {"Space ( )", "personalizado", "ninguno"})
        Me.position.Items.AddRange(New Object() {"After separator", "Before separator"})
        Me.fieldLabel.MaxLength = 200
        Me.prefix.MaxLength = 100
        Me.customSeparator.MaxLength = 20
        Me.order.Maximum = 2147483647D
        Me.remove.ForeColor = System.Drawing.Color.DarkRed
        Me.preview.ForeColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.preview.Font = New System.Drawing.Font("Consolas",10.0!,System.Drawing.FontStyle.Bold)
        Me.Controls.Add(Me.fieldLabel)
        Me.Controls.Add(Me.target)
        Me.Controls.Add(Me.mode)
        Me.Controls.Add(Me.remove)
        Me.Controls.Add(Me.separator)
        Me.Controls.Add(Me.position)
        Me.Controls.Add(Me.prefix)
        Me.Controls.Add(Me.length)
        Me.Controls.Add(Me.customSeparator)
        Me.Controls.Add(Me.segment)
        Me.Controls.Add(Me.order)
        Me.Controls.Add(Me.preview)
        Me.Controls.Add(Me.fieldCaption)
        Me.Controls.Add(Me.targetCaption)
        Me.Controls.Add(Me.modeCaption)
        Me.Controls.Add(Me.separatorCaption)
        Me.Controls.Add(Me.positionCaption)
        Me.Controls.Add(Me.prefixCaption)
        Me.Controls.Add(Me.lengthCaption)
        Me.Controls.Add(Me.customCaption)
        Me.Controls.Add(Me.segmentCaption)
        Me.Controls.Add(Me.orderCaption)
        Me.BackColor = System.Drawing.Color.FromArgb(249,251,254)
        Me.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.Size = New System.Drawing.Size(875,215)
        Me.Margin = New System.Windows.Forms.Padding(0,0,0,10)
        Me.ResumeLayout(False)
    End Sub
    Friend WithEvents fieldLabel As System.Windows.Forms.TextBox
    Friend WithEvents target As System.Windows.Forms.ComboBox
    Friend WithEvents mode As System.Windows.Forms.ComboBox
    Friend WithEvents remove As System.Windows.Forms.Button
    Friend WithEvents separator As System.Windows.Forms.ComboBox
    Friend WithEvents position As System.Windows.Forms.ComboBox
    Friend WithEvents prefix As System.Windows.Forms.TextBox
    Friend WithEvents length As System.Windows.Forms.TextBox
    Friend WithEvents customSeparator As System.Windows.Forms.TextBox
    Friend WithEvents segment As System.Windows.Forms.TextBox
    Friend WithEvents order As System.Windows.Forms.NumericUpDown
    Friend WithEvents preview As System.Windows.Forms.Label
    Friend WithEvents fieldCaption As System.Windows.Forms.Label
    Friend WithEvents targetCaption As System.Windows.Forms.Label
    Friend WithEvents modeCaption As System.Windows.Forms.Label
    Friend WithEvents separatorCaption As System.Windows.Forms.Label
    Friend WithEvents positionCaption As System.Windows.Forms.Label
    Friend WithEvents prefixCaption As System.Windows.Forms.Label
    Friend WithEvents lengthCaption As System.Windows.Forms.Label
    Friend WithEvents customCaption As System.Windows.Forms.Label
    Friend WithEvents segmentCaption As System.Windows.Forms.Label
    Friend WithEvents orderCaption As System.Windows.Forms.Label
End Class
