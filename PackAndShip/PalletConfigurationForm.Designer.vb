<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PalletConfigurationForm
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
        Me.browser = New System.Windows.Forms.Panel()
        Me.browserHeading = New System.Windows.Forms.Label()
        Me.searchCaption = New System.Windows.Forms.Label()
        Me.searchBox = New System.Windows.Forms.TextBox()
        Me.customerCaption = New System.Windows.Forms.Label()
        Me.buPicker = New System.Windows.Forms.ComboBox()
        Me.partsList = New System.Windows.Forms.ListBox()
        Me.refreshButton = New System.Windows.Forms.Button()
        Me.editor = New System.Windows.Forms.Panel()
        Me.titleLabel = New System.Windows.Forms.Label()
        Me.statusLabel = New System.Windows.Forms.Label()
        Me.activeCheck = New System.Windows.Forms.CheckBox()
        Me.distribution = New System.Windows.Forms.Panel()
        Me.distributionTitle = New System.Windows.Forms.Label()
        Me.levelCaption = New System.Windows.Forms.Label()
        Me.levelCountInput = New System.Windows.Forms.NumericUpDown()
        Me.piecesCaption = New System.Windows.Forms.Label()
        Me.piecesPerLevelInput = New System.Windows.Forms.NumericUpDown()
        Me.distributeButton = New System.Windows.Forms.Button()
        Me.helperNote = New System.Windows.Forms.Label()
        Me.levelsPanel = New System.Windows.Forms.FlowLayoutPanel()
        Me.countLabel = New System.Windows.Forms.Label()
        Me.totalLabel = New System.Windows.Forms.Label()
        Me.addLevelButton = New System.Windows.Forms.Button()
        Me.saveButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        Me.header.Name = "header"
        Me.header.Location = New System.Drawing.Point(0,0)
        Me.header.Size = New System.Drawing.Size(1150,88)
        Me.heading.Name = "heading"
        Me.heading.Location = New System.Drawing.Point(24,12)
        Me.heading.Size = New System.Drawing.Size(650,42)
        Me.subtitle.Name = "subtitle"
        Me.subtitle.Location = New System.Drawing.Point(28,58)
        Me.subtitle.Size = New System.Drawing.Size(680,24)
        Me.browser.Name = "browser"
        Me.browser.Location = New System.Drawing.Point(0,100)
        Me.browser.Size = New System.Drawing.Size(398,625)
        Me.browserHeading.Name = "browserHeading"
        Me.browserHeading.Location = New System.Drawing.Point(15,14)
        Me.browserHeading.Size = New System.Drawing.Size(360,35)
        Me.searchCaption.Name = "searchCaption"
        Me.searchCaption.Location = New System.Drawing.Point(15,60)
        Me.searchCaption.Size = New System.Drawing.Size(350,21)
        Me.searchBox.Name = "searchBox"
        Me.searchBox.Location = New System.Drawing.Point(15,84)
        Me.searchBox.Size = New System.Drawing.Size(355,32)
        Me.customerCaption.Name = "customerCaption"
        Me.customerCaption.Location = New System.Drawing.Point(15,127)
        Me.customerCaption.Size = New System.Drawing.Size(350,23)
        Me.buPicker.Name = "buPicker"
        Me.buPicker.Location = New System.Drawing.Point(15,155)
        Me.buPicker.Size = New System.Drawing.Size(355,32)
        Me.partsList.Name = "partsList"
        Me.partsList.Location = New System.Drawing.Point(15,204)
        Me.partsList.Size = New System.Drawing.Size(355,338)
        Me.refreshButton.Name = "refreshButton"
        Me.refreshButton.Location = New System.Drawing.Point(15,552)
        Me.refreshButton.Size = New System.Drawing.Size(355,40)
        Me.editor.Name = "editor"
        Me.editor.Location = New System.Drawing.Point(414,100)
        Me.editor.Size = New System.Drawing.Size(715,625)
        Me.titleLabel.Name = "titleLabel"
        Me.titleLabel.Location = New System.Drawing.Point(20,10)
        Me.titleLabel.Size = New System.Drawing.Size(670,44)
        Me.statusLabel.Name = "statusLabel"
        Me.statusLabel.Location = New System.Drawing.Point(20,60)
        Me.statusLabel.Size = New System.Drawing.Size(670,25)
        Me.activeCheck.Name = "activeCheck"
        Me.activeCheck.Location = New System.Drawing.Point(20,95)
        Me.activeCheck.Size = New System.Drawing.Size(240,30)
        Me.distribution.Name = "distribution"
        Me.distribution.Location = New System.Drawing.Point(20,145)
        Me.distribution.Size = New System.Drawing.Size(675,115)
        Me.distributionTitle.Name = "distributionTitle"
        Me.distributionTitle.Location = New System.Drawing.Point(10,7)
        Me.distributionTitle.Size = New System.Drawing.Size(390,27)
        Me.levelCaption.Name = "levelCaption"
        Me.levelCaption.Location = New System.Drawing.Point(10,41)
        Me.levelCaption.Size = New System.Drawing.Size(95,23)
        Me.levelCountInput.Name = "levelCountInput"
        Me.levelCountInput.Location = New System.Drawing.Point(105,38)
        Me.levelCountInput.Size = New System.Drawing.Size(75,30)
        Me.piecesCaption.Name = "piecesCaption"
        Me.piecesCaption.Location = New System.Drawing.Point(196,41)
        Me.piecesCaption.Size = New System.Drawing.Size(115,23)
        Me.piecesPerLevelInput.Name = "piecesPerLevelInput"
        Me.piecesPerLevelInput.Location = New System.Drawing.Point(310,38)
        Me.piecesPerLevelInput.Size = New System.Drawing.Size(90,30)
        Me.distributeButton.Name = "distributeButton"
        Me.distributeButton.Location = New System.Drawing.Point(416,36)
        Me.distributeButton.Size = New System.Drawing.Size(160,36)
        Me.helperNote.Name = "helperNote"
        Me.helperNote.Location = New System.Drawing.Point(10,80)
        Me.helperNote.Size = New System.Drawing.Size(615,27)
        Me.levelsPanel.Name = "levelsPanel"
        Me.levelsPanel.Location = New System.Drawing.Point(20,279)
        Me.levelsPanel.Size = New System.Drawing.Size(675,214)
        Me.countLabel.Name = "countLabel"
        Me.countLabel.Location = New System.Drawing.Point(20,505)
        Me.countLabel.Size = New System.Drawing.Size(220,32)
        Me.totalLabel.Name = "totalLabel"
        Me.totalLabel.Location = New System.Drawing.Point(252,505)
        Me.totalLabel.Size = New System.Drawing.Size(340,34)
        Me.addLevelButton.Name = "addLevelButton"
        Me.addLevelButton.Location = New System.Drawing.Point(20,558)
        Me.addLevelButton.Size = New System.Drawing.Size(155,41)
        Me.saveButton.Name = "saveButton"
        Me.saveButton.Location = New System.Drawing.Point(205,558)
        Me.saveButton.Size = New System.Drawing.Size(225,41)
        Me.heading.Text = "Pallet Configuration"
        Me.subtitle.Text = "One pallet setup per part number"
        Me.browserHeading.Text = "Part numbers"
        Me.searchCaption.Text = "Search part number"
        Me.customerCaption.Text = "Customer (filter only)"
        Me.refreshButton.Text = "Refresh catalog"
        Me.titleLabel.Text = "Select a part number"
        Me.statusLabel.Text = "Choose a part from the catalog to configure its pallet."
        Me.activeCheck.Text = "Configuration active"
        Me.distributionTitle.Text = "Quick level distribution"
        Me.levelCaption.Text = "Total levels"
        Me.piecesCaption.Text = "Pieces per level"
        Me.distributeButton.Text = "Generate levels"
        Me.helperNote.Text = "Generate equal quantities, then adjust individual levels below."
        Me.countLabel.Text = "0 levels"
        Me.totalLabel.Text = "Total: 0 pieces"
        Me.addLevelButton.Text = "+ Add level"
        Me.saveButton.Text = "Save configuration"
        Me.header.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.heading.Font = New System.Drawing.Font("Segoe UI",23.0!,System.Drawing.FontStyle.Bold)
        Me.heading.ForeColor = System.Drawing.Color.White
        Me.subtitle.ForeColor = System.Drawing.Color.White
        Me.browser.BackColor = System.Drawing.Color.White
        Me.distribution.BackColor = System.Drawing.Color.White
        Me.browserHeading.Font = New System.Drawing.Font("Segoe UI",15.0!,System.Drawing.FontStyle.Bold)
        Me.titleLabel.Font = New System.Drawing.Font("Segoe UI",16.0!,System.Drawing.FontStyle.Bold)
        Me.totalLabel.Font = New System.Drawing.Font("Segoe UI",17.0!,System.Drawing.FontStyle.Bold)
        Me.partsList.DisplayMember = "DisplayName"
        Me.partsList.IntegralHeight = False
        Me.buPicker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.activeCheck.Checked = True
        Me.levelCountInput.Minimum = 1D
        Me.levelCountInput.Maximum = 1000D
        Me.levelCountInput.Value = 1D
        Me.piecesPerLevelInput.Minimum = 1D
        Me.piecesPerLevelInput.Maximum = 2147483647D
        Me.piecesPerLevelInput.Value = 1D
        Me.levelsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.levelsPanel.WrapContents = False
        Me.levelsPanel.AutoScroll = True
        Me.refreshButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.refreshButton.ForeColor = System.Drawing.Color.White
        Me.refreshButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.distributeButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.distributeButton.ForeColor = System.Drawing.Color.White
        Me.distributeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.addLevelButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.addLevelButton.ForeColor = System.Drawing.Color.White
        Me.addLevelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.saveButton.BackColor = System.Drawing.Color.FromArgb(31,78,121)
        Me.saveButton.ForeColor = System.Drawing.Color.White
        Me.saveButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.header.Controls.Add(Me.heading)
        Me.header.Controls.Add(Me.subtitle)
        Me.browser.Controls.Add(Me.browserHeading)
        Me.browser.Controls.Add(Me.searchCaption)
        Me.browser.Controls.Add(Me.searchBox)
        Me.browser.Controls.Add(Me.customerCaption)
        Me.browser.Controls.Add(Me.buPicker)
        Me.browser.Controls.Add(Me.partsList)
        Me.browser.Controls.Add(Me.refreshButton)
        Me.distribution.Controls.Add(Me.distributionTitle)
        Me.distribution.Controls.Add(Me.levelCaption)
        Me.distribution.Controls.Add(Me.levelCountInput)
        Me.distribution.Controls.Add(Me.piecesCaption)
        Me.distribution.Controls.Add(Me.piecesPerLevelInput)
        Me.distribution.Controls.Add(Me.distributeButton)
        Me.distribution.Controls.Add(Me.helperNote)
        Me.editor.Controls.Add(Me.titleLabel)
        Me.editor.Controls.Add(Me.statusLabel)
        Me.editor.Controls.Add(Me.activeCheck)
        Me.editor.Controls.Add(Me.distribution)
        Me.editor.Controls.Add(Me.levelsPanel)
        Me.editor.Controls.Add(Me.countLabel)
        Me.editor.Controls.Add(Me.totalLabel)
        Me.editor.Controls.Add(Me.addLevelButton)
        Me.editor.Controls.Add(Me.saveButton)
        Me.Controls.Add(Me.header)
        Me.Controls.Add(Me.browser)
        Me.Controls.Add(Me.editor)
        Me.ClientSize = New System.Drawing.Size(1150,780)
        Me.MinimumSize = New System.Drawing.Size(0,0)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Pack and Ship | Setup - Pallet Configuration"
        Me.BackColor = System.Drawing.Color.FromArgb(244,247,250)
        Me.Font = New System.Drawing.Font("Segoe UI",10.0!)
        Me.ResumeLayout(False)
    End Sub
    Friend WithEvents header As System.Windows.Forms.Panel
    Friend WithEvents heading As System.Windows.Forms.Label
    Friend WithEvents subtitle As System.Windows.Forms.Label
    Friend WithEvents browser As System.Windows.Forms.Panel
    Friend WithEvents browserHeading As System.Windows.Forms.Label
    Friend WithEvents searchCaption As System.Windows.Forms.Label
    Friend WithEvents searchBox As System.Windows.Forms.TextBox
    Friend WithEvents customerCaption As System.Windows.Forms.Label
    Friend WithEvents buPicker As System.Windows.Forms.ComboBox
    Friend WithEvents partsList As System.Windows.Forms.ListBox
    Friend WithEvents refreshButton As System.Windows.Forms.Button
    Friend WithEvents editor As System.Windows.Forms.Panel
    Friend WithEvents titleLabel As System.Windows.Forms.Label
    Friend WithEvents statusLabel As System.Windows.Forms.Label
    Friend WithEvents activeCheck As System.Windows.Forms.CheckBox
    Friend WithEvents distribution As System.Windows.Forms.Panel
    Friend WithEvents distributionTitle As System.Windows.Forms.Label
    Friend WithEvents levelCaption As System.Windows.Forms.Label
    Friend WithEvents levelCountInput As System.Windows.Forms.NumericUpDown
    Friend WithEvents piecesCaption As System.Windows.Forms.Label
    Friend WithEvents piecesPerLevelInput As System.Windows.Forms.NumericUpDown
    Friend WithEvents distributeButton As System.Windows.Forms.Button
    Friend WithEvents helperNote As System.Windows.Forms.Label
    Friend WithEvents levelsPanel As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents countLabel As System.Windows.Forms.Label
    Friend WithEvents totalLabel As System.Windows.Forms.Label
    Friend WithEvents addLevelButton As System.Windows.Forms.Button
    Friend WithEvents saveButton As System.Windows.Forms.Button
End Class
