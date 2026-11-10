Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

Public Class ScanRulesForm
    Inherits Form

    Private ReadOnly navy As Color = Color.FromArgb(31, 78, 121)






    Private rules As New List(Of ScanRuleRecord)()
    Private availableParts As New List(Of PaSPart)()
    Private loading As Boolean

    Public Sub New()
        InitializeComponent()
        ConfigureGrid(rulesGrid)
        ConfigureGrid(fieldsGrid)
        BindEvents()
        AddHandler Shown, Sub() ReloadRules()
    End Sub

    Private Sub BindEvents()
        AddHandler addRule.Click, Sub() OpenRuleEditor(Nothing)
        AddHandler editRule.Click, Sub()
            Dim selected = SelectedRule()
            If selected Is Nothing Then
                MessageBox.Show("Primero selecciona una regla.")
                Return
            End If
            OpenRuleEditor(selected)
        End Sub
        AddHandler toggleRule.Click, AddressOf ToggleSelectedRule
        AddHandler refreshButton.Click, Sub() ReloadRules()
        AddHandler searchBox.TextChanged, Sub() FilterRules()
        AddHandler customerPicker.SelectedIndexChanged, Sub() FilterRules()
        AddHandler rulesGrid.SelectionChanged, AddressOf ShowRuleDetails
    End Sub

    Private Sub ConfigureGrid(grid As DataGridView)
        grid.ReadOnly = True
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AutoGenerateColumns = False
        grid.MultiSelect = False
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grid.BackgroundColor = Color.White
        grid.RowHeadersVisible = False
    End Sub

    Private Sub ReloadRules()
        Try
            availableParts = ScanRulesData.ReadPartOptions()
            rules = ScanRulesData.ReadRules()
            Dim previousId = 0
            Dim previous = TryCast(customerPicker.SelectedItem, BusinessUnit)
            If previous IsNot Nothing Then previousId = previous.Id
            If previousId = 0 Then previousId = SessionContext.BusinessUnitId
            Dim options = availableParts.Where(Function(p) p.BusinessUnitId.HasValue).
                GroupBy(Function(p) p.BusinessUnitId.Value).
                Select(Function(g) New BusinessUnit With {.Id = g.Key, .Name = g.First().BusinessUnit}).
                OrderBy(Function(x) x.Name).ToList()
            options.Insert(0, New BusinessUnit With {.Id = 0, .Name = "Todos los clientes"})
            loading = True
            customerPicker.DataSource = options
            customerPicker.DisplayMember = "Name"
            customerPicker.ValueMember = "Id"
            Dim selectedIndex = options.FindIndex(Function(c) c.Id = previousId)
            customerPicker.SelectedIndex = If(selectedIndex >= 0, selectedIndex, 0)
            loading = False
            FilterRules()
        Catch ex As Exception
            loading = False
            MessageBox.Show(ex.Message, "No se pudieron consultar las reglas de escaneo compartidas",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FilterRules()
        If loading Then Return
        Dim term = searchBox.Text.Trim()
        Dim selected = TryCast(customerPicker.SelectedItem, BusinessUnit)
        Dim customerId = If(selected Is Nothing, 0, selected.Id)
        Dim subset = rules.Where(Function(r) (customerId = 0 OrElse r.CustomerId = customerId) AndAlso
            (term = "" OrElse r.SsiPart.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
             r.Customer.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
             r.Id.ToString().Contains(term))).ToList()
        rulesGrid.DataSource = Nothing
        rulesGrid.DataSource = subset
        rulesGrid.ClearSelection()
        fieldsGrid.DataSource = Nothing
        notesLabel.Clear()
        summaryLabel.Text = subset.Count & " of " & rules.Count & " reglas"
    End Sub

    Private Function SelectedRule() As ScanRuleRecord
        If rulesGrid.SelectedRows.Count = 0 Then Return Nothing
        Return TryCast(rulesGrid.SelectedRows(0).DataBoundItem, ScanRuleRecord)
    End Function

    Private Sub OpenRuleEditor(rule As ScanRuleRecord)
        Try
            Using editor As New ScanRuleEditorForm(rule, availableParts)
                If editor.ShowDialog(Me) = DialogResult.OK Then ReloadRules()
            End Using
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Editor de reglas de escaneo", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ToggleSelectedRule(sender As Object, e As EventArgs)
        Dim rule = SelectedRule()
        If rule Is Nothing Then
            MessageBox.Show("Primero selecciona una regla.")
            Return
        End If
        Dim action = If(rule.IsActive, "deactivate", "activate")
        If MessageBox.Show("¿Deseas " & action & " esta regla para " &
                           rule.SsiPart & "? Esto también afectará a Q-Wall.", "Confirmar cambio de regla compartida",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
        Try
            ScanRulesData.SetActive(rule, Not rule.IsActive)
            ReloadRules()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "No se pudo actualizar la regla", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ShowRuleDetails(sender As Object, e As EventArgs)
        If loading OrElse rulesGrid.SelectedRows.Count = 0 Then Return
        Dim rule = TryCast(rulesGrid.SelectedRows(0).DataBoundItem, ScanRuleRecord)
        If rule Is Nothing Then Return
        Try
            fieldsGrid.DataSource = ScanRulesData.ReadFields(rule.Id)
            notesLabel.Text = "Notas: " & rule.Notes
        Catch ex As Exception
            MessageBox.Show(ex.Message, "No se pudieron consultar los campos de escaneo",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
