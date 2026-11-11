<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DepartmentAuthorizationForm
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
        Me.helpLabel = New System.Windows.Forms.Label()
        Me.departmentTabs = New System.Windows.Forms.TabControl()
        Me.manufacturingTab = New System.Windows.Forms.TabPage()
        Me.qualityTab = New System.Windows.Forms.TabPage()
        Me.productionTab = New System.Windows.Forms.TabPage()
        Me.manufacturingEnabled = New System.Windows.Forms.CheckBox()
        Me.qualityEnabled = New System.Windows.Forms.CheckBox()
        Me.productionEnabled = New System.Windows.Forms.CheckBox()
        Me.manufacturingRoles = New System.Windows.Forms.CheckedListBox()
        Me.qualityRoles = New System.Windows.Forms.CheckedListBox()
        Me.productionRoles = New System.Windows.Forms.CheckedListBox()
        Me.saveManufacturing = New System.Windows.Forms.Button()
        Me.saveQuality = New System.Windows.Forms.Button()
        Me.saveProduction = New System.Windows.Forms.Button()
        Me.reloadButton = New System.Windows.Forms.Button()
        Me.departmentTabs.SuspendLayout()
        Me.manufacturingTab.SuspendLayout()
        Me.qualityTab.SuspendLayout()
        Me.productionTab.SuspendLayout()
        Me.SuspendLayout()
        '
        ' heading
        '
        Me.heading.Name = "heading"
        Me.heading.Text = "Autorizaciones por departamento"
        Me.heading.Font = New System.Drawing.Font("Segoe UI", 19.0!, System.Drawing.FontStyle.Bold)
        Me.heading.ForeColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.heading.Location = New System.Drawing.Point(24,18)
        Me.heading.Size = New System.Drawing.Size(710,43)
        '
        ' helpLabel
        '
        Me.helpLabel.Name = "helpLabel"
        Me.helpLabel.Text = "Activa los departamentos obligatorios y selecciona los roles que pueden aprobar. Cada departamento debe autorizar con una persona distinta."
        Me.helpLabel.Location = New System.Drawing.Point(27,69)
        Me.helpLabel.Size = New System.Drawing.Size(708,56)
        '
        ' departmentTabs
        '
        Me.departmentTabs.Name = "departmentTabs"
        Me.departmentTabs.Location = New System.Drawing.Point(25,137)
        Me.departmentTabs.Size = New System.Drawing.Size(712,453)
        Me.departmentTabs.Controls.Add(Me.manufacturingTab)
        Me.departmentTabs.Controls.Add(Me.qualityTab)
        Me.departmentTabs.Controls.Add(Me.productionTab)
        '
        ' manufacturingTab
        '
        Me.manufacturingTab.Name = "manufacturingTab"
        Me.manufacturingTab.Text = "Manufactura"
        Me.manufacturingTab.UseVisualStyleBackColor = True
        Me.manufacturingTab.Controls.Add(Me.manufacturingEnabled)
        Me.manufacturingTab.Controls.Add(Me.manufacturingRoles)
        Me.manufacturingTab.Controls.Add(Me.saveManufacturing)
        '
        ' qualityTab
        '
        Me.qualityTab.Name = "qualityTab"
        Me.qualityTab.Text = "Calidad"
        Me.qualityTab.UseVisualStyleBackColor = True
        Me.qualityTab.Controls.Add(Me.qualityEnabled)
        Me.qualityTab.Controls.Add(Me.qualityRoles)
        Me.qualityTab.Controls.Add(Me.saveQuality)
        '
        ' productionTab
        '
        Me.productionTab.Name = "productionTab"
        Me.productionTab.Text = "Producción"
        Me.productionTab.UseVisualStyleBackColor = True
        Me.productionTab.Controls.Add(Me.productionEnabled)
        Me.productionTab.Controls.Add(Me.productionRoles)
        Me.productionTab.Controls.Add(Me.saveProduction)
        '
        ' manufacturingEnabled
        '
        Me.manufacturingEnabled.Name = "manufacturingEnabled"
        Me.manufacturingEnabled.Text = "Requerir autorización de Manufactura"
        Me.manufacturingEnabled.Location = New System.Drawing.Point(24,18)
        Me.manufacturingEnabled.Size = New System.Drawing.Size(540,31)
        '
        ' qualityEnabled
        '
        Me.qualityEnabled.Name = "qualityEnabled"
        Me.qualityEnabled.Text = "Requerir autorización de Calidad"
        Me.qualityEnabled.Location = New System.Drawing.Point(24,18)
        Me.qualityEnabled.Size = New System.Drawing.Size(540,31)
        '
        ' productionEnabled
        '
        Me.productionEnabled.Name = "productionEnabled"
        Me.productionEnabled.Text = "Requerir autorización de Producción"
        Me.productionEnabled.Location = New System.Drawing.Point(24,18)
        Me.productionEnabled.Size = New System.Drawing.Size(540,31)
        '
        ' manufacturingRoles
        '
        Me.manufacturingRoles.Name = "manufacturingRoles"
        Me.manufacturingRoles.Location = New System.Drawing.Point(24,64)
        Me.manufacturingRoles.Size = New System.Drawing.Size(648,280)
        Me.manufacturingRoles.CheckOnClick = True
        '
        ' qualityRoles
        '
        Me.qualityRoles.Name = "qualityRoles"
        Me.qualityRoles.Location = New System.Drawing.Point(24,64)
        Me.qualityRoles.Size = New System.Drawing.Size(648,280)
        Me.qualityRoles.CheckOnClick = True
        '
        ' productionRoles
        '
        Me.productionRoles.Name = "productionRoles"
        Me.productionRoles.Location = New System.Drawing.Point(24,64)
        Me.productionRoles.Size = New System.Drawing.Size(648,280)
        Me.productionRoles.CheckOnClick = True
        '
        ' saveManufacturing
        '
        Me.saveManufacturing.Name = "saveManufacturing"
        Me.saveManufacturing.Text = "Guardar Manufactura"
        Me.saveManufacturing.Location = New System.Drawing.Point(433,359)
        Me.saveManufacturing.Size = New System.Drawing.Size(239,42)
        '
        ' saveQuality
        '
        Me.saveQuality.Name = "saveQuality"
        Me.saveQuality.Text = "Guardar Calidad"
        Me.saveQuality.Location = New System.Drawing.Point(433,359)
        Me.saveQuality.Size = New System.Drawing.Size(239,42)
        '
        ' saveProduction
        '
        Me.saveProduction.Name = "saveProduction"
        Me.saveProduction.Text = "Guardar Producción"
        Me.saveProduction.Location = New System.Drawing.Point(433,359)
        Me.saveProduction.Size = New System.Drawing.Size(239,42)
        '
        ' reloadButton
        '
        Me.reloadButton.Name = "reloadButton"
        Me.reloadButton.Text = "Actualizar"
        Me.reloadButton.Location = New System.Drawing.Point(603,604)
        Me.reloadButton.Size = New System.Drawing.Size(134,40)
        '
        ' DepartmentAuthorizationForm
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Font = New System.Drawing.Font("Segoe UI",10.0!)
        Me.BackColor = System.Drawing.Color.White
        Me.ClientSize = New System.Drawing.Size(768,668)
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "PackAndShip | Departamentos y roles autorizadores"
        Me.Controls.Add(Me.heading)
        Me.Controls.Add(Me.helpLabel)
        Me.Controls.Add(Me.departmentTabs)
        Me.Controls.Add(Me.reloadButton)
        Me.departmentTabs.ResumeLayout(False)
        Me.manufacturingTab.ResumeLayout(False)
        Me.qualityTab.ResumeLayout(False)
        Me.productionTab.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents heading As System.Windows.Forms.Label
    Friend WithEvents helpLabel As System.Windows.Forms.Label
    Friend WithEvents departmentTabs As System.Windows.Forms.TabControl
    Friend WithEvents manufacturingTab As System.Windows.Forms.TabPage
    Friend WithEvents qualityTab As System.Windows.Forms.TabPage
    Friend WithEvents productionTab As System.Windows.Forms.TabPage
    Friend WithEvents manufacturingEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents qualityEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents productionEnabled As System.Windows.Forms.CheckBox
    Friend WithEvents manufacturingRoles As System.Windows.Forms.CheckedListBox
    Friend WithEvents qualityRoles As System.Windows.Forms.CheckedListBox
    Friend WithEvents productionRoles As System.Windows.Forms.CheckedListBox
    Friend WithEvents saveManufacturing As System.Windows.Forms.Button
    Friend WithEvents saveQuality As System.Windows.Forms.Button
    Friend WithEvents saveProduction As System.Windows.Forms.Button
    Friend WithEvents reloadButton As System.Windows.Forms.Button
End Class
