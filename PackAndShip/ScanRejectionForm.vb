Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

' Pantalla de rechazo de gran formato. El diseño reside en Designer.vb.
Public Partial Class ScanRejectionForm
    Inherits Form

    Private ReadOnly blocked As ScanRejectionInfo

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(info As ScanRejectionInfo, containerCode As String)
        Me.New()
        blocked = info
        containerLabel.Text = "CONTENEDOR: " & containerCode
        reasonLabel.Text = info.Reason
        serialLabel.Text = "SSI: " & info.SsiCode &
            Environment.NewLine & "CLIENTE: " & info.ClientCode
        AddHandler refreshButton.Click, Sub() LoadApprovals()
        AddHandler approveButton.Click, AddressOf ApproveDepartment
        AddHandler Shown, Sub() LoadApprovals()
    End Sub

    Private Sub LoadApprovals()
        Try
            Dim rows = RejectionAuthorizationData.RequiredApprovals(blocked.Id)
            approvalsGrid.DataSource = rows
            If approvalsGrid.Columns.Contains("DepartmentId") Then
                approvalsGrid.Columns("DepartmentId").Visible = False
            End If
            Dim pendingCount = rows.Select("Estado = 'PENDIENTE'").Length
            pendingLabel.Text = If(rows.Rows.Count = 0,
                "SIN DEPARTAMENTOS HABILITADOS. CONFIGURAR EN SETUP.",
                If(pendingCount > 0,
                   "BLOQUEADO — " & pendingCount & " APROBACIONES PENDIENTES",
                   "AUTORIZACIÓN COMPLETA — PUEDES CONTINUAR"))
            Dim current = RejectionAuthorizationData.Pending(blocked.ContainerId)
            If current Is Nothing Then
                approveButton.Enabled = False
                barcodeInput.Enabled = False
                passwordInput.Enabled = False
                departmentPicker.Enabled = False
                pendingLabel.ForeColor = Color.DarkGreen
                Me.DialogResult = DialogResult.OK
                Close()
                Return
            End If
            departmentPicker.DataSource = Nothing
            Dim pendingRows = rows.Select("Estado = 'PENDIENTE'")
            Dim choices As New DataTable()
            choices.Columns.Add("DepartmentId", GetType(Integer))
            choices.Columns.Add("Departamento", GetType(String))
            For Each row As DataRow In pendingRows
                choices.Rows.Add(Convert.ToInt32(row("DepartmentId")), Convert.ToString(row("Departamento")))
            Next
            departmentPicker.DisplayMember = "Departamento"
            departmentPicker.ValueMember = "DepartmentId"
            departmentPicker.DataSource = choices
            approveButton.Enabled = choices.Rows.Count > 0
        Catch ex As Exception
            pendingLabel.Text = "ERROR DE CONEXIÓN — ESCÁNER BLOQUEADO"
            approveButton.Enabled = False
            MessageBox.Show(ex.Message, "No se pudieron leer las autorizaciones",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ApproveDepartment(sender As Object, e As EventArgs)
        If departmentPicker.SelectedValue Is Nothing Then Return
        Try
            RejectionAuthorizationData.Approve(blocked.Id,
                Convert.ToInt32(departmentPicker.SelectedValue),
                barcodeInput.Text, passwordInput.Text)
            barcodeInput.Clear()
            passwordInput.Clear()
            LoadApprovals()
            barcodeInput.Focus()
        Catch ex As Exception
            passwordInput.Clear()
            MessageBox.Show(ex.Message, "Autorización rechazada",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
