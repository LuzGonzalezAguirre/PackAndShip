Imports System.Drawing
Imports System.Windows.Forms

Public Class Form1
    Private ReadOnly brandBlue As Color = Color.FromArgb(31, 78, 121)
    Private ReadOnly backgroundColor As Color = Color.FromArgb(244, 247, 250)
    Private ReadOnly connectionTimer As New Timer With {.Interval = 5000}
    Private connectionBusy As Boolean
    Private connectionFailed As Boolean
    Private connectionChecked As Boolean
    Private failureCount As Integer
    ' Remember the Design-time window size before displaying the compact login.
    Private signedInDesignerSize As Size
    Private units As New List(Of BusinessUnit)()

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        signedInDesignerSize = Me.ClientSize
        Me.Text = "PackAndShip | Escáner"
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.MinimumSize = New Size(1, 1)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = backgroundColor
        Me.Font = New Font("Segoe UI", 11.0F)
        BindViewEvents()
        AddHandler connectionTimer.Tick, AddressOf ConnectionTimerTick
        ReloadCustomers()
        menuSetup.Enabled = False
        menuPalletActions.Enabled = False
        menuLogout.Enabled = False
        ' LoginForm already authenticated the user before Form1 was created.
        ActivateAuthenticatedStation()
    End Sub

    Private Sub BindViewEvents()
        AddHandler setupCustomerItem.Click, AddressOf ChangeDefaultCustomer
        AddHandler setupUsersItem.Click, AddressOf OpenUsersSetup
        AddHandler setupPalletItem.Click, AddressOf OpenPalletConfiguration
        AddHandler setupRulesItem.Click, AddressOf OpenScanRules
        AddHandler setupGeneralItem.Click, AddressOf OpenGeneralSettings
        AddHandler helpAboutItem.Click, Sub()
                                            MessageBox.Show("PackAndShip" & Environment.NewLine &
                            "Escaneo y configuración de pallets para manufactura.",
                            "About", MessageBoxButtons.OK, MessageBoxIcon.Information)
                                        End Sub
        AddHandler menuPalletActions.DropDownOpening, AddressOf UpdatePalletActionsMenu
        AddHandler menuPartialPallet.Click, Sub() scannerView.RequestPartialPallet()
        AddHandler menuCancelEmptyPallet.Click, Sub() scannerView.RequestCancelEmptyPallet()
        AddHandler menuReviewPendingSql.Click, Sub() scannerView.RequestPendingSqlReview()
        AddHandler menuHistory.Click, AddressOf OpenPalletHistory
        AddHandler menuTransfer.Click, AddressOf OpenPalletTransfers
        AddHandler menuLogout.Click, AddressOf LogoutFromMenu
    End Sub

    Private Sub UpdatePalletActionsMenu(sender As Object, e As EventArgs)
        Dim signedIn = SessionContext.UserId > 0
        menuPartialPallet.Enabled = signedIn AndAlso scannerView.CanClosePartial
        menuCancelEmptyPallet.Enabled = signedIn AndAlso scannerView.CanCancelEmpty
        menuReviewPendingSql.Enabled = signedIn AndAlso scannerView.HasPendingSqlAction
    End Sub

    Private Sub ReloadCustomers()
        Try
            units = PackAndShipData.GetBusinessUnits()
            RefreshCustomer()
        Catch ex As Exception
            units.Clear()
            MessageBox.Show(ex.Message, "Conexión con SQL Server", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RefreshCustomer()
        Dim saved = StationSettings.ReadCustomerId()
        Dim selected As BusinessUnit = Nothing
        If saved.HasValue Then selected = units.Find(Function(u) u.Id = saved.Value)
        SessionContext.BusinessUnitId = If(selected Is Nothing, 0, selected.Id)
        SessionContext.BusinessUnitName = If(selected Is Nothing, "", selected.Name)
    End Sub

    Private Sub ActivateAuthenticatedStation()
        If SessionContext.UserId <= 0 Then
            Close()
            Return
        End If
        SetAuthenticatedLayout()
        scannerView.LoadStation()
        connectionFailed = False
        connectionChecked = False
        failureCount = 0
        connectionTimer.Interval = 5000
        connectionTimer.Start()
        menuSetup.Enabled = SessionContext.CanModifySetup
        menuPalletActions.Enabled = True
        menuLogout.Enabled = True
    End Sub

    Private Sub ShowLoginDialog()
        ' Used after logout. Keep the scanner invisible while credentials are entered.
        Hide()
        Try
            Using login As New LoginForm(SessionContext.BusinessUnitName)
                If login.ShowDialog() <> DialogResult.OK Then
                    Close()
                    Return
                End If
            End Using
            ActivateAuthenticatedStation()
        Finally
            If Not IsDisposed AndAlso SessionContext.UserId > 0 Then Show()
        End Try
    End Sub

    ' Bounded exponential backoff: 5, 10, 20, 40, 80, 160, 300 s.
    ' Probes run off the UI thread, never overlap, and only query CCS.
    Private Async Sub ConnectionTimerTick(sender As Object, e As EventArgs)
        If connectionBusy OrElse SessionContext.UserId <= 0 Then Return
        connectionBusy = True
        connectionTimer.Stop()
        Try
            Dim result = Await System.Threading.Tasks.Task.Run(
                Function() ConnectionRecovery.CheckAndRecover())
            Dim restored = connectionChecked AndAlso connectionFailed
            connectionChecked = True
            connectionFailed = False
            failureCount = 0
            connectionLabel.ForeColor = If(result.StartsWith("REVIEW REQUIRED"),
                                           Color.DarkRed, Color.DarkGreen)
            connectionLabel.Text = "CCS: CONECTADO | " & result
            If result.StartsWith("RECOVERED") OrElse result.StartsWith("NOT CLOSED") OrElse
               result.StartsWith("NOT CANCELLED") Then
                scannerView.AfterRecovery(result)
            ElseIf result = "CONNECTED" AndAlso restored Then
                scannerView.AfterRecovery()
            End If
            If restored Then
                MessageBox.Show("Conexión con SQL Server restablecida." & Environment.NewLine &
                                result, "Conexión PackAndShip",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
            connectionTimer.Interval = 30000
        Catch ex As Exception
            Dim sqlError = TryCast(ex, Microsoft.Data.SqlClient.SqlException)
            If sqlError IsNot Nothing AndAlso
               (sqlError.Number >= 50000 OrElse sqlError.Number = 2601 OrElse
                sqlError.Number = 2627) Then
                ConnectionRecovery.FlagReview(ex.Message)
                connectionChecked = True
                connectionFailed = False
                failureCount = 0
                connectionLabel.ForeColor = Color.DarkRed
                connectionLabel.Text = "CCS: CONECTADO | La última operación requiere revisión: " & ex.Message
                connectionTimer.Interval = 30000
                Return
            End If
            connectionChecked = True
            connectionFailed = True
            failureCount += 1
            Dim seconds = CInt(Math.Min(300, 5 * Math.Pow(2, Math.Min(6, failureCount - 1))))
            connectionTimer.Interval = seconds * 1000
            connectionLabel.ForeColor = Color.DarkRed
            connectionLabel.Text = "CCS: DESCONECTADO | Nuevo intento en " & seconds &
                                   "s | " & If(ConnectionRecovery.Pending() Is Nothing,
                                               "Sin operaciones SQL pendientes", "Última operación guardada localmente")
        Finally
            connectionBusy = False
            If SessionContext.UserId > 0 Then connectionTimer.Start()
        End Try
    End Sub

    Private Sub OpenPalletTransfers(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 Then
            MessageBox.Show("Inicia sesión para transferir un pallet parcial.", "Acceso denegado")
            Return
        End If
        If ConnectionRecovery.Pending() IsNot Nothing Then
            MessageBox.Show("Resuelve la operación SQL pendiente antes de transferir pallets.",
                            "Operación pendiente", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Dim locked = StationSettings.ReadLockedPallet()
        If locked <> "" Then
            MessageBox.Show("Termina el pallet actual " & locked &
                            " antes de transferir otro pallet.",
                            "Pallet activo", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Using dialog As New PalletTransferForm()
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                ' Transfer has already committed in SQL. If local loading fails,
                ' the new pallet can be restored with Open / Load.
                scannerView.LoadTransferredPallet(dialog.DestinationCode)
            Catch ex As Exception
                MessageBox.Show("Transferencia guardada en SQL. Pallet nuevo: " &
                                dialog.DestinationCode & Environment.NewLine &
                                "Falló la carga automática: " & ex.Message &
                                Environment.NewLine &
                                "Carga el nuevo pallet manualmente desde el escáner.",
                                "Pallet transferido", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
            End Try
        End Using
    End Sub

    Private Sub OpenPalletHistory(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 Then
            MessageBox.Show("Inicia sesión para ver el historial de pallets.", "Acceso denegado")
            Return
        End If
        Using dialog As New PalletHistoryForm()
            dialog.ShowDialog(Me)
        End Using
        ' Corrections may have changed the active pallet; do not clear model selection.
        Try
            scannerView.RefreshAfterHistory()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Actualización de pallet", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub OpenScanRules(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Tu rol no puede acceder a Configuración.", "Acceso denegado",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Using dialog As New ScanRulesForm()
            dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenPalletConfiguration(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Tu rol no puede modificar Configuración.", "Acceso denegado",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Using dialog As New PalletConfigurationForm()
            dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenUsersSetup(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Tu rol no puede modificar Configuración.", "Acceso denegado",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Using dialog As New UsersSetupForm()
            dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenGeneralSettings(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Tu rol no puede modificar Configuración.", "Acceso denegado",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Using dialog As New GeneralSettingsForm()
            dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Sub ChangeDefaultCustomer(sender As Object, e As EventArgs)
        Dim openCode = StationSettings.ReadLockedPallet()
        If openCode <> "" Then
            MessageBox.Show("Termina el pallet " & openCode &
                            " (completo o parcial autorizado) antes de cambiar el cliente de la estación.",
                            "Pallet activo", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Tu rol no puede cambiar Configuración.", "Acceso denegado")
            Return
        End If
        If units.Count = 0 Then
            ReloadCustomers()
            If units.Count = 0 Then Return
        End If

        Using dialog As New Form With {
            .Text = "PackAndShip | Configuración de cliente",
            .Size = New Size(550, 385),
            .MinimumSize = New Size(550, 385),
            .MaximumSize = New Size(550, 385),
            .StartPosition = FormStartPosition.CenterParent,
            .FormBorderStyle = FormBorderStyle.FixedDialog,
            .MaximizeBox = False, .MinimizeBox = False
        }
            PaSTheme.ConfigureForm(dialog)

            Dim topPanel As New Panel With {
                .Dock = DockStyle.Top, .Height = 82,
                .BackColor = PaSTheme.Navy
            }
            topPanel.Controls.Add(New Label With {
                .Text = "Configuración de cliente",
                .Location = New Point(23, 12), .AutoSize = True,
                .Font = New Font("Segoe UI", 19, FontStyle.Bold),
                .ForeColor = Color.White
            })
            topPanel.Controls.Add(New Label With {
                .Text = "Cliente predeterminado de esta estación",
                .Location = New Point(25, 53), .AutoSize = True,
                .ForeColor = Color.FromArgb(220, 233, 247)
            })
            dialog.Controls.Add(topPanel)

            Dim contentPanel As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = PaSTheme.Background,
                .Padding = New Padding(24)
            }
            Dim caption As New Label With {
                .Text = "Customer",
                .Location = New Point(24, 79),
                .Size = New Size(460, 25),
                .ForeColor = PaSTheme.TextPrimary,
                .Font = New Font("Segoe UI", 10, FontStyle.Bold)
            }
            Dim picker As New ComboBox With {
                .Location = New Point(24, 108),
                .Size = New Size(476, 33),
                .DropDownStyle = ComboBoxStyle.DropDownList,
                .Font = New Font("Segoe UI", 11)
            }
            contentPanel.Controls.AddRange(New Control() {caption, picker})
            dialog.Controls.Add(contentPanel)

            picker.DisplayMember = "Name"
            picker.ValueMember = "Id"
            picker.DataSource = New List(Of BusinessUnit)(units)
            If SessionContext.BusinessUnitId > 0 Then
                Dim savedCustomer = units.Find(Function(u) u.Id = SessionContext.BusinessUnitId)
                If savedCustomer IsNot Nothing Then picker.SelectedValue = savedCustomer.Id
            End If

            Dim footer As New Panel With {
                .Dock = DockStyle.Bottom, .Height = 78,
                .BackColor = PaSTheme.Surface
            }
            Dim save = PaSTheme.ActionButton("Guardar", True)
            save.Location = New Point(365, 18)
            save.Size = New Size(145, 40)
            save.DialogResult = DialogResult.OK
            Dim cancel = PaSTheme.ActionButton("Cancelar", False)
            cancel.Location = New Point(215, 18)
            cancel.Size = New Size(135, 40)
            cancel.DialogResult = DialogResult.Cancel
            footer.Controls.AddRange(New Control() {cancel, save})
            dialog.Controls.Add(footer)
            footer.BringToFront()
            topPanel.BringToFront()
            dialog.AcceptButton = save
            dialog.CancelButton = cancel

            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Dim selected = TryCast(picker.SelectedItem, BusinessUnit)
            If selected Is Nothing Then Return
            Try
                StationSettings.SaveCustomerId(selected.Id)
                RefreshCustomer()
                scannerView.LoadStation()
            Catch ex As Exception
                MessageBox.Show(ex.Message, "Error de configuración de estación",
                                MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    Private Sub SetAuthenticatedLayout()
        SuspendLayout()
        Try
            signedInPanel.Visible = True
            FormBorderStyle = FormBorderStyle.Sizable
            MaximizeBox = True
            ' The authenticated size is editable from Form1 Designer's
            ' ClientSize property, rather than hardcoded here.
            ClientSize = New Size(Math.Max(1080, signedInDesignerSize.Width),
                                  Math.Max(720, signedInDesignerSize.Height))
            appHeader.Visible = False
            mainMenu.Visible = True
            Text = "PackAndShip | Escáner"
            CenterToScreen()
        Finally
            ResumeLayout(True)
        End Try
    End Sub

    Private Sub LogoutFromMenu(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 Then Return
        connectionTimer.Stop()
        SessionContext.Clear()
        connectionLabel.Text = "CCS: sesión cerrada"
        connectionLabel.ForeColor = Color.DimGray
        menuSetup.Enabled = False
        menuPalletActions.Enabled = False
        menuLogout.Enabled = False
        RefreshCustomer()
        ShowLoginDialog()
    End Sub
End Class
