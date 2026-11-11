Imports System.Data
Imports System.Drawing
Imports System.Windows.Forms

' Controles y dimensiones definidos en PalletActionAuthorizationForm.Designer.vb.
Public Partial Class PalletActionAuthorizationForm
    Inherits Form

    Private ReadOnly containerId As Integer
    Private ReadOnly actionKind As String
    Private requestId As Long
    Private completed As Boolean

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(container As PaSPackedContainer, cancelEmpty As Boolean)
        Me.New()
        containerId = container.Id
        actionKind = If(cancelEmpty, "CANCEL_EMPTY", "CLOSE_PARTIAL")
        titleLabel.Text = If(cancelEmpty, "CANCELAR PALLET VACÍO", "CERRAR PALLET PARCIAL")
        palletLabel.Text = "CONTENEDOR " & container.Code & "  |  " &
            container.PieceTotal & " / " & container.RequiredTotal & " piezas"
        AddHandler startButton.Click, AddressOf StartRequest
        AddHandler approveButton.Click, AddressOf ApproveRequest
        AddHandler completeButton.Click, AddressOf CompleteRequest
        AddHandler refreshButton.Click, Sub() RefreshApprovals()
        AddHandler Shown, Sub() RestoreRequest()
    End Sub

    Private Sub RestoreRequest()
        Try
            Dim previous = PalletActionAuthorizationData.GetPending(containerId, actionKind)
            If previous Is Nothing Then Return
            If Convert.ToInt32(previous("requested_by_user_id")) <> SessionContext.UserId Then
                Throw New UnauthorizedAccessException(
                    "Existe una solicitud iniciada por otro operador. Solicita su revisión.")
            End If
            requestId = Convert.ToInt64(previous("request_id"))
            reasonInput.Text = Convert.ToString(previous("reason"))
            reasonInput.ReadOnly = True
            startButton.Enabled = False
            RefreshApprovals()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Solicitud pendiente",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            startButton.Enabled = False
        End Try
    End Sub

    Private Sub StartRequest(sender As Object, e As EventArgs)
        Try
            If reasonInput.Text.Trim().Length < 10 Then
                Throw New ArgumentException("El motivo debe tener al menos 10 caracteres.")
            End If
            requestId = PalletActionAuthorizationData.BeginAction(
                containerId, actionKind, reasonInput.Text)
            reasonInput.ReadOnly = True
            startButton.Enabled = False
            RefreshApprovals()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "No se pudo iniciar la autorización",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub RefreshApprovals()
        If requestId <= 0 OrElse completed Then Return
        Try
            Dim rows = PalletActionAuthorizationData.ReadApprovals(requestId)
            approvalsGrid.DataSource = rows
            If approvalsGrid.Columns.Contains("DepartmentId") Then
                approvalsGrid.Columns("DepartmentId").Visible = False
            End If
            Dim pendingRows = rows.Select("Estado = 'PENDIENTE'")
            Dim choices As New DataTable()
            choices.Columns.Add("DepartmentId", GetType(Integer))
            choices.Columns.Add("Departamento", GetType(String))
            For Each row As DataRow In pendingRows
                choices.Rows.Add(Convert.ToInt32(row("DepartmentId")),
                                 Convert.ToString(row("Departamento")))
            Next
            departmentPicker.DataSource = Nothing
            departmentPicker.DisplayMember = "Departamento"
            departmentPicker.ValueMember = "DepartmentId"
            departmentPicker.DataSource = choices
            approveButton.Enabled = choices.Rows.Count > 0
            completeButton.Enabled = rows.Rows.Count > 0 AndAlso pendingRows.Length = 0
            progressLabel.Text = If(rows.Rows.Count = 0,
                "Sin departamentos configurados; solicitud bloqueada.",
                "Aprobaciones: " & (rows.Rows.Count - pendingRows.Length) &
                " / " & rows.Rows.Count)
            progressLabel.ForeColor = If(completeButton.Enabled, Color.DarkGreen, Color.DarkRed)
        Catch ex As Exception
            completeButton.Enabled = False
            approveButton.Enabled = False
            progressLabel.Text = "Sin confirmación de CCS. Operación bloqueada."
            MessageBox.Show(ex.Message, "Error al consultar aprobaciones",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ApproveRequest(sender As Object, e As EventArgs)
        If requestId <= 0 OrElse departmentPicker.SelectedValue Is Nothing Then Return
        Try
            PalletActionAuthorizationData.Approve(
                requestId, Convert.ToInt32(departmentPicker.SelectedValue),
                barcodeInput.Text, passwordInput.Text)
            barcodeInput.Clear()
            passwordInput.Clear()
            RefreshApprovals()
        Catch ex As Exception
            passwordInput.Clear()
            MessageBox.Show(ex.Message, "Autorización denegada",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub CompleteRequest(sender As Object, e As EventArgs)
        If Not completeButton.Enabled OrElse requestId <= 0 Then Return
        If MessageBox.Show(
            If(actionKind = "CANCEL_EMPTY",
                "¿Cancelar definitivamente este contenedor vacío?",
                "¿Cerrar definitivamente este pallet como PARCIAL?") &
            Environment.NewLine & "Esta operación no se puede deshacer.",
            "Confirmar operación de pallet",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return
        completeButton.Enabled = False
        Try
            ' SQL verifica las aprobaciones de nuevo, en la transacción final.
            PalletActionAuthorizationData.Complete(requestId)
            completed = True
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            MessageBox.Show(
                "No se pudo confirmar la operación en CCS. Verifica el estado del pallet " &
                "antes de volver a intentarlo." & Environment.NewLine & ex.Message,
                "Operación no confirmada", MessageBoxButtons.OK, MessageBoxIcon.Error)
            RefreshApprovals()
        End Try
    End Sub
End Class
