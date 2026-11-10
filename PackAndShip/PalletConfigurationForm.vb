Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

Public Class PalletConfigurationForm
    Inherits Form

    Private ReadOnly navy As Color = Color.FromArgb(31, 78, 121)















    Private ReadOnly quantityInputs As New List(Of NumericUpDown)()
    Private parts As New List(Of PaSPart)()
    Private selectedPart As PaSPart
    Private isLoading As Boolean

    Public Sub New()
        InitializeComponent()
        BindEvents()
        EnableEditor(False)
        AddHandler Shown, Sub() LoadParts()
    End Sub

    Private Sub BindEvents()
        AddHandler searchBox.TextChanged, Sub() FilterParts()
        AddHandler buPicker.SelectedIndexChanged, Sub() FilterParts()
        AddHandler partsList.SelectedIndexChanged, AddressOf PartSelected
        AddHandler refreshButton.Click, Sub() LoadParts()
        AddHandler distributeButton.Click, AddressOf GenerateLevels
        AddHandler addLevelButton.Click, Sub() AddLevel(1)
        AddHandler saveButton.Click, AddressOf SaveConfiguration
        AddHandler levelsPanel.Resize, Sub() ResizeRows()
    End Sub

    Private Sub StyleButton(button As Button)
        button.FlatStyle = FlatStyle.Flat
        button.BackColor = navy
        button.ForeColor = Color.White
        button.FlatAppearance.BorderSize = 0
    End Sub

    Private Sub LoadParts()
        Try
            Dim selectedId = If(selectedPart Is Nothing, 0, selectedPart.Id)
            isLoading = True
            parts = PalletConfigurationData.GetParts()
            Dim customers = parts.
                Where(Function(p) p.BusinessUnitId.HasValue).
                GroupBy(Function(p) p.BusinessUnitId.Value).
                Select(Function(g) New BusinessUnit With {.Id = g.Key, .Name = g.First().BusinessUnit}).
                OrderBy(Function(b) b.Name).ToList()
            customers.Insert(0, New BusinessUnit With {.Id = 0, .Name = "Todos los clientes"})
            Dim selectedCustomerId As Integer = SessionContext.BusinessUnitId
            Dim previousCustomer = TryCast(buPicker.SelectedItem, BusinessUnit)
            If previousCustomer IsNot Nothing Then
                selectedCustomerId = previousCustomer.Id
            End If
            buPicker.DataSource = customers
            buPicker.DisplayMember = "Name"
            buPicker.ValueMember = "Id"
            Dim customerIndex = customers.FindIndex(Function(b) b.Id = selectedCustomerId)
            buPicker.SelectedIndex = If(customerIndex >= 0, customerIndex, 0)
            isLoading = False
            FilterParts(selectedId)
        Catch ex As Exception
            isLoading = False
            MessageBox.Show(ex.Message, "No se pudo cargar el catálogo de partes",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub FilterParts(Optional preferredPartId As Integer = 0)
        If isLoading Then Return
        Dim query = searchBox.Text.Trim()
        Dim selectedBu = TryCast(buPicker.SelectedItem, BusinessUnit)
        Dim buId = If(selectedBu Is Nothing, 0, selectedBu.Id)
        Dim options = parts.Where(Function(p) (buId = 0 OrElse p.BusinessUnitId = buId) AndAlso
            (query = "" OrElse p.Number.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
             p.ClientNumber.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)).
            ToList()
        isLoading = True
        partsList.DataSource = Nothing
        partsList.DataSource = options
        partsList.DisplayMember = "DisplayName"
        partsList.ClearSelected()
        If preferredPartId > 0 Then
            Dim index = options.FindIndex(Function(p) p.Id = preferredPartId)
            If index >= 0 Then partsList.SelectedIndex = index
        End If
        isLoading = False
        If partsList.SelectedItem IsNot Nothing Then
            LoadSelectedPart()
        Else
            selectedPart = Nothing
            titleLabel.Text = "Select a part number"
            statusLabel.Text = options.Count & " partes coincidentes"
            ClearLevels()
            EnableEditor(False)
        End If
    End Sub

    Private Sub PartSelected(sender As Object, e As EventArgs)
        If Not isLoading Then LoadSelectedPart()
    End Sub

    Private Sub LoadSelectedPart()
        selectedPart = TryCast(partsList.SelectedItem, PaSPart)
        If selectedPart Is Nothing Then
            EnableEditor(False)
            Return
        End If
        Try
            Dim config = PalletConfigurationData.GetConfiguration(selectedPart.Id)
            ClearLevels()
            titleLabel.Text = selectedPart.DisplayName
            statusLabel.Text = selectedPart.BusinessUnit
            If config Is Nothing Then
                statusLabel.Text = selectedPart.BusinessUnit & " • Sin configurar • Configuración nueva"
                activeCheck.Checked = True
                AddLevel(1)
            Else
                statusLabel.Text = selectedPart.BusinessUnit & " • Configuración existente"
                activeCheck.Checked = config.IsActive
                For Each quantity In config.Levels
                    AddLevel(quantity)
                Next
                If config.Levels.Count = 0 Then
                    statusLabel.Text = "La configuración existente no tiene niveles; corrige antes de guardar."
                End If
                If config.Levels.Sum(Function(x) CLng(x)) <> CLng(config.TotalPieces) Then
                    statusLabel.Text = "Advertencia: el total guardado no coincide con los niveles."
                End If
            End If
            levelCountInput.Value = Math.Max(1, Math.Min(1000, quantityInputs.Count))
            If quantityInputs.Count > 0 Then piecesPerLevelInput.Value = quantityInputs(0).Value
            EnableEditor(True)
        Catch ex As Exception
            EnableEditor(False)
            MessageBox.Show(ex.Message, "No se pudo cargar la configuración del pallet",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub EnableEditor(enabled As Boolean)
        addLevelButton.Enabled = enabled
        distributeButton.Enabled = enabled
        levelCountInput.Enabled = enabled
        piecesPerLevelInput.Enabled = enabled
        saveButton.Enabled = enabled
        activeCheck.Enabled = enabled
        levelsPanel.Enabled = enabled
    End Sub

    Private Sub GenerateLevels(sender As Object, e As EventArgs)
        If selectedPart Is Nothing Then
            MessageBox.Show("Select a part number first.")
            Return
        End If
        Dim numberOfLevels = CInt(levelCountInput.Value)
        Dim pieces = CInt(piecesPerLevelInput.Value)
        If CLng(numberOfLevels) * CLng(pieces) > Integer.MaxValue Then
            MessageBox.Show("El total supera el máximo permitido.",
                            "Configuración inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If quantityInputs.Count > 0 AndAlso
           MessageBox.Show("¿Reemplazar los niveles mostrados? Se perderán los cambios sin guardar.",
                           "Generar niveles", MessageBoxButtons.YesNo,
                           MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If
        levelsPanel.SuspendLayout()
        Try
            ClearLevels()
            For i As Integer = 1 To numberOfLevels
                AddLevel(pieces)
            Next
        Finally
            levelsPanel.ResumeLayout()
        End Try
        UpdateTotals()
    End Sub

    Private Sub ClearLevels()
        While levelsPanel.Controls.Count > 0
            Dim child = levelsPanel.Controls(0)
            levelsPanel.Controls.RemoveAt(0)
            child.Dispose()
        End While
        quantityInputs.Clear()
        UpdateTotals()
    End Sub

    Private Sub AddLevel(amount As Integer)
        Dim row As New Panel With {
            .Height = 68, .Width = Math.Max(400, levelsPanel.ClientSize.Width - 28),
            .BackColor = Color.White, .Margin = New Padding(0, 0, 0, 8)
        }
        Dim levelTitle As New Label With {
            .Text = "Nivel " & (quantityInputs.Count + 1),
            .Font = New Font("Segoe UI", 12, FontStyle.Bold),
            .ForeColor = navy, .Location = New Point(15, 17), .Size = New Size(120, 35)
        }
        Dim countText As New Label With {
            .Text = "Piezas requeridas:", .Location = New Point(145, 23),
            .AutoSize = True
        }
        Dim input As New NumericUpDown With {
            .Minimum = 1, .Maximum = Integer.MaxValue, .Value = amount,
            .TextAlign = HorizontalAlignment.Center,
            .Location = New Point(290, 17), .Width = 115
        }
        AddHandler input.ValueChanged, Sub() UpdateTotals()
        Dim remove As New Button With {
            .Text = "Remove", .Size = New Size(100, 34),
            .Location = New Point(430, 16), .Tag = input
        }
        AddHandler remove.Click, Sub()
                                     If quantityInputs.Count <= 1 Then
                                         MessageBox.Show("Un pallet necesita al menos un nivel.")
                                         Return
                                     End If
                                     quantityInputs.Remove(input)
                                     levelsPanel.Controls.Remove(row)
                                     row.Dispose()
                                     RenumberLevels()
                                     UpdateTotals()
                                 End Sub
        row.Controls.AddRange(New Control() {levelTitle, countText, input, remove})
        quantityInputs.Add(input)
        levelsPanel.Controls.Add(row)
        ResizeRows()
        UpdateTotals()
    End Sub

    Private Sub RenumberLevels()
        For i = 0 To levelsPanel.Controls.Count - 1
            Dim row = DirectCast(levelsPanel.Controls(i), Panel)
            DirectCast(row.Controls(0), Label).Text = "Nivel " & (i + 1)
        Next
    End Sub

    Private Sub ResizeRows()
        For Each row As Control In levelsPanel.Controls
            row.Width = Math.Max(540, levelsPanel.ClientSize.Width - 28)
        Next
    End Sub

    Private Sub UpdateTotals()
        countLabel.Text = quantityInputs.Count & " niveles"
        Dim pieces As Long = quantityInputs.Sum(Function(n) CLng(n.Value))
        totalLabel.Text = "Total: " & pieces & " piezas"
        saveButton.Enabled = selectedPart IsNot Nothing AndAlso
                             quantityInputs.Count > 0 AndAlso pieces <= Integer.MaxValue
    End Sub

    Private Sub SaveConfiguration(sender As Object, e As EventArgs)
        If selectedPart Is Nothing Then
            MessageBox.Show("Select a part number first.")
            Return
        End If
        Dim quantities = quantityInputs.Select(Function(input) CInt(input.Value)).ToList()
        Try
            PalletConfigurationData.SaveConfiguration(selectedPart.Id, quantities, activeCheck.Checked)
            MessageBox.Show("Configuración de pallet guardada correctamente.",
                            "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Dim currentPartId = selectedPart.Id
            LoadParts()
            FilterParts(currentPartId)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "No se pudo guardar la configuración",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
