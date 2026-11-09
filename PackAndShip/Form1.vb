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
    Private units As New List(Of BusinessUnit)()

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "Pack and Ship | SSI"
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.MinimumSize = New Size(1, 1)
        Me.ClientSize = New Size(422, 340)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = backgroundColor
        Me.Font = New Font("Segoe UI", 11.0F)
        BindViewEvents()
        AddHandler connectionTimer.Tick, AddressOf ConnectionTimerTick
        ReloadCustomers()
        ResetLogin()
    End Sub

    Private Sub BindViewEvents()
        AddHandler setupCustomerItem.Click, AddressOf ChangeDefaultCustomer
        AddHandler setupUsersItem.Click, AddressOf OpenUsersSetup
        AddHandler setupPalletItem.Click, AddressOf OpenPalletConfiguration
        AddHandler setupRulesItem.Click, AddressOf OpenScanRules
        AddHandler setupGeneralItem.Click, AddressOf OpenGeneralSettings
        AddHandler helpAboutItem.Click, Sub()
            MessageBox.Show("PackAndShip" & Environment.NewLine &
                            "Manufacturing pallet scanning and configuration.",
                            "About", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Sub
        AddHandler menuHistory.Click, AddressOf OpenPalletHistory
        AddHandler menuTransfer.Click, AddressOf OpenPalletTransfers
        AddHandler menuLogout.Click, AddressOf LogoutFromMenu
        AddHandler buttonLogin.Click, AddressOf PerformLogin
    End Sub

    Private Sub AddLabel(parent As Control, value As String, x As Integer, y As Integer,
                         width As Integer, height As Integer, bold As Boolean)
        Dim item As New Label With {
            .Text = value, .Location = New Point(x, y), .Size = New Size(width, height),
            .ForeColor = brandBlue
        }
        If bold Then item.Font = New Font("Segoe UI", 17, FontStyle.Bold)
        parent.Controls.Add(item)
    End Sub

    Private Sub CenterCard(parent As Control, child As Control)
        child.Location = New Point(
            Math.Max(0, (parent.ClientSize.Width - child.Width) \ 2),
            Math.Max(18, (parent.ClientSize.Height - child.Height) \ 2))
    End Sub

    Private Sub ReloadCustomers()
        Try
            units = PackAndShipData.GetBusinessUnits()
            RefreshCustomer()
        Catch ex As Exception
            units.Clear()
            labelCustomer.Text = "Customer unavailable"
            MessageBox.Show(ex.Message, "SQL Server connection", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RefreshCustomer()
        Dim saved = StationSettings.ReadCustomerId()
        Dim selected As BusinessUnit = Nothing
        If saved.HasValue Then selected = units.Find(Function(u) u.Id = saved.Value)
        SessionContext.BusinessUnitId = If(selected Is Nothing, 0, selected.Id)
        SessionContext.BusinessUnitName = If(selected Is Nothing, "", selected.Name)
        labelCustomer.Text = If(selected Is Nothing, "Not configured", selected.Name)
    End Sub

    Private Sub PerformLogin(sender As Object, e As EventArgs)
        Dim barcode = inputBarcode.Text.Trim().ToUpperInvariant()
        Dim password = inputPassword.Text
        If barcode = "" OrElse password = "" Then
            MessageBox.Show("Enter Barcode ID and Password.", "Required fields",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Dim user = PackAndShipData.Authenticate(barcode, password)
            If user Is Nothing Then
                MessageBox.Show("Invalid credentials or inactive user.", "Access denied",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning)
                inputPassword.Clear()
                Return
            End If

            SessionContext.UserId = user.Id
            SessionContext.FullName = user.Name
            SessionContext.BarcodeId = user.Barcode
            SessionContext.RoleId = user.RoleId
            SessionContext.RoleName = user.RoleName
            SessionContext.CanModifySetup = user.CanModifySetup

            SetAuthenticatedLayout()
            scannerView.LoadStation()
            connectionFailed = False
            connectionChecked = False
            failureCount = 0
            connectionTimer.Interval = 5000
            connectionTimer.Start()
            menuSetup.Enabled = SessionContext.CanModifySetup
            menuLogout.Enabled = True
            labelWelcome.Text = "Operator: " & user.Name & "  |  Role: " & user.RoleName &
                                Environment.NewLine & "Customer: " &
                                If(SessionContext.BusinessUnitId = 0, "Not configured",
                                   SessionContext.BusinessUnitName)
            If SessionContext.BusinessUnitId = 0 Then
                labelStatus.Text = "Configure this station's customer from Setup before scanning."
            Else
                labelStatus.Text = "Pallet scanning rules are pending definition."
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Login error", MessageBoxButtons.OK, MessageBoxIcon.Error)
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
            connectionLabel.Text = "CCS: CONNECTED | " & result
            If result.StartsWith("RECOVERED") OrElse result.StartsWith("NOT CLOSED") OrElse
               result.StartsWith("NOT CANCELLED") Then
                scannerView.AfterRecovery(result)
            ElseIf result = "CONNECTED" AndAlso restored Then
                scannerView.AfterRecovery()
            End If
            If restored Then
                MessageBox.Show("SQL Server connection restored." & Environment.NewLine &
                                result, "PackAndShip connection",
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
                connectionLabel.Text = "CCS: CONNECTED | Last action requires review: " & ex.Message
                connectionTimer.Interval = 30000
                Return
            End If
            connectionChecked = True
            connectionFailed = True
            failureCount += 1
            Dim seconds = CInt(Math.Min(300, 5 * Math.Pow(2, Math.Min(6, failureCount - 1))))
            connectionTimer.Interval = seconds * 1000
            connectionLabel.ForeColor = Color.DarkRed
            connectionLabel.Text = "CCS: DISCONNECTED | Retry in " & seconds &
                                   "s | " & If(ConnectionRecovery.Pending() Is Nothing,
                                               "No pending SQL action", "Last action saved locally")
        Finally
            connectionBusy = False
            If SessionContext.UserId > 0 Then connectionTimer.Start()
        End Try
    End Sub

    Private Sub OpenPalletTransfers(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 Then
            MessageBox.Show("Sign in to transfer a partial pallet.", "Access denied")
            Return
        End If
        If ConnectionRecovery.Pending() IsNot Nothing Then
            MessageBox.Show("Resolve the pending SQL operation before transferring pallets.",
                            "Pending operation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Dim locked = StationSettings.ReadLockedPallet()
        If locked <> "" Then
            MessageBox.Show("Finish current pallet " & locked &
                            " before transferring another pallet.",
                            "Active pallet", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Using dialog As New PalletTransferForm()
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                ' Transfer has already committed in SQL. If local loading fails,
                ' the new pallet can be restored with Open / Load.
                scannerView.LoadTransferredPallet(dialog.DestinationCode)
            Catch ex As Exception
                MessageBox.Show("Transfer saved in SQL. New pallet: " &
                                dialog.DestinationCode & Environment.NewLine &
                                "Automatic loading failed: " & ex.Message &
                                Environment.NewLine &
                                "Load the new pallet from the scanner manually.",
                                "Pallet transferred", MessageBoxButtons.OK,
                                MessageBoxIcon.Warning)
            End Try
        End Using
    End Sub

    Private Sub OpenPalletHistory(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 Then
            MessageBox.Show("Sign in to view pallet history.", "Access denied")
            Return
        End If
        Using dialog As New PalletHistoryForm()
            dialog.ShowDialog(Me)
        End Using
        ' Corrections may have changed the active pallet; do not clear model selection.
        Try
            scannerView.RefreshAfterHistory()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Pallet refresh", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub

    Private Sub OpenScanRules(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Your role cannot access Setup.", "Access denied",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Using dialog As New ScanRulesForm()
            dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenPalletConfiguration(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Your role cannot modify Setup.", "Access denied",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Using dialog As New PalletConfigurationForm()
            dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenUsersSetup(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Your role cannot modify Setup.", "Access denied",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        Using dialog As New UsersSetupForm()
            dialog.ShowDialog(Me)
        End Using
    End Sub

    Private Sub OpenGeneralSettings(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Your role cannot modify Setup.", "Access denied",
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
            MessageBox.Show("Finish pallet " & openCode &
                            " (complete or authorized partial) before changing this station's customer.",
                            "Active pallet", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        If SessionContext.UserId <= 0 OrElse Not SessionContext.CanModifySetup Then
            MessageBox.Show("Your role cannot change Setup.", "Access denied")
            Return
        End If
        If units.Count = 0 Then
            ReloadCustomers()
            If units.Count = 0 Then Return
        End If

        Using dialog As New Form With {
            .Text = "PackAndShip | Customer Configuration",
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
                .Text = "Customer Configuration",
                .Location = New Point(23, 12), .AutoSize = True,
                .Font = New Font("Segoe UI", 19, FontStyle.Bold),
                .ForeColor = Color.White
            })
            topPanel.Controls.Add(New Label With {
                .Text = "Default customer for this workstation",
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
            Dim save = PaSTheme.ActionButton("Save", True)
            save.Location = New Point(365, 18)
            save.Size = New Size(145, 40)
            save.DialogResult = DialogResult.OK
            Dim cancel = PaSTheme.ActionButton("Cancel", False)
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
                MessageBox.Show(ex.Message, "Station configuration error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    Private Sub SetLoginLayout()
        SuspendLayout()
        Try
            mainMenu.Visible = False
            appHeader.Visible = False
            signedInPanel.Visible = False
            loginPanel.Visible = True
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimumSize = New Size(1, 1)
            ClientSize = New Size(422, 340)
            Text = "Pack and Ship | Login"
            CenterToScreen()
        Finally
            ResumeLayout(True)
        End Try
    End Sub

    Private Sub SetAuthenticatedLayout()
        SuspendLayout()
        Try
            loginPanel.Visible = False
            signedInPanel.Visible = True
            FormBorderStyle = FormBorderStyle.Sizable
            MaximizeBox = True
            MinimumSize = New Size(800, 570)
            ClientSize = New Size(1100, 720)
            appHeader.Visible = False
            mainMenu.Visible = True
            Text = "Pack and Ship | SSI"
            CenterToScreen()
        Finally
            ResumeLayout(True)
        End Try
    End Sub

    Private Sub LogoutFromMenu(sender As Object, e As EventArgs)
        If SessionContext.UserId <= 0 Then Return
        ResetLogin()
    End Sub

    Private Sub ResetLogin()
        connectionTimer.Stop()
        SessionContext.Clear()
        connectionLabel.Text = "CCS: signed out"
        connectionLabel.ForeColor = Color.DimGray
        RefreshCustomer()
        inputBarcode.Clear()
        inputPassword.Clear()
        SetLoginLayout()
        menuSetup.Enabled = False
        menuLogout.Enabled = False
        Me.AcceptButton = buttonLogin
        inputBarcode.Focus()
    End Sub
End Class
