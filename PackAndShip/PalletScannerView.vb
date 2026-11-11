Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

' Visual controls and positions are defined in PalletScannerView.Designer.vb.
' This file contains only scan, database, validation and navigation logic.
Public Partial Class PalletScannerView
    Inherits UserControl

    Private ReadOnly navy As Color = PaSTheme.Navy
    Private activeContainer As PaSPackedContainer
    Private verifiedSsi As String = ""
    Private verifiedClient As String = ""
    Private ReadOnly clockTimer As New Timer With {.Interval = 1000}
    Private parts As New List(Of PaSPart)()
    Private selectedConfiguration As PalletConfiguration
    Private selectedRule As ScanRuleRecord
    Private selectedFields As New List(Of ScanFieldRecord)()
    Private palletFormat As PalletIdSettings
    Private autoConfirmVisible As Boolean
    Private autoScanBusy As Boolean
    Private autoOpenBusy As Boolean
    Private rejectionStorageFailed As Boolean

    ' These hidden state guards preserve the existing SQL write workflow.
    ' They are not visual controls and do not need Designer positions.
    Private ReadOnly openButton As New Button()
    Private ReadOnly verifyButton As New Button()
    Private ReadOnly recordButton As New Button()
    Private ReadOnly levelButton As New Button()
    Private ReadOnly closePallet As New Button()
    ' State-only controls, not placed on the UserControl surface.
    Private ReadOnly partialButton As New Button()
    Private ReadOnly cancelEmptyButton As New Button()
    Private ReadOnly pendingButton As New Button()

    Public Sub New()
        InitializeComponent()
        BindScannerEvents()
        AddHandler clockTimer.Tick, Sub() clockValue.Text = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss")
        AddHandler Disposed, Sub() clockTimer.Stop()
        clockTimer.Start()
    End Sub

    ' Compatibilidad con eventos Click creados por Visual Studio Designer.
    ' Esta etiqueta es informativa y no debe ejecutar operaciones SQL.
    Private Sub lblPalletPiecesCaption_Click(sender As Object, e As EventArgs) Handles lblPalletPiecesCaption.Click
    End Sub

    Public ReadOnly Property CanClosePartial As Boolean
        Get
            Return partialButton.Enabled
        End Get
    End Property

    Public ReadOnly Property CanCancelEmpty As Boolean
        Get
            Return cancelEmptyButton.Enabled
        End Get
    End Property

    Public ReadOnly Property HasPendingSqlAction As Boolean
        Get
            Return ConnectionRecovery.Pending() IsNot Nothing
        End Get
    End Property

    Public Sub RequestPartialPallet()
        If Not partialButton.Enabled Then
            MessageBox.Show("El cierre parcial requiere un pallet ABIERTO con al menos una pieza y menos del total configurado.",
                            "Pallet parcial", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        ClosePartialPallet(Me, EventArgs.Empty)
    End Sub

    Public Sub RequestCancelEmptyPallet()
        If Not cancelEmptyButton.Enabled Then
            MessageBox.Show("Solo se puede cancelar un pallet ABIERTO sin piezas ni niveles cerrados.",
                            "Cancelar pallet vacío", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If
        CancelEmptyPallet(Me, EventArgs.Empty)
    End Sub

    Public Sub RequestPendingSqlReview()
        ReviewPendingSql(Me, EventArgs.Empty)
    End Sub

    Private Sub BindScannerEvents()
        AddHandler openButton.Click, AddressOf OpenOrLoadContainer
        AddHandler verifyButton.Click, Sub() VerifyQrFormat()
        AddHandler recordButton.Click, AddressOf RecordPiece
        AddHandler levelButton.Click, AddressOf CloseLevel
        AddHandler closePallet.Click, AddressOf CompletePallet
        AddHandler confirmAgain.Click, Sub() PromptCompletedLevel()
        AddHandler partPicker.SelectedIndexChanged, AddressOf PartChanged
        AddHandler containerInput.KeyDown, AddressOf ContainerEnter
        AddHandler firstQr.KeyDown, AddressOf FirstLabelEnter
        AddHandler secondQr.KeyDown, AddressOf SecondLabelEnter
        AddHandler containerInput.TextChanged, Sub()
                                                   If activeContainer IsNot Nothing AndAlso
                                                      activeContainer.Status <> "OPEN" AndAlso
                                                      Not String.Equals(activeContainer.Code, containerInput.Text.Trim(),
                                                                        StringComparison.OrdinalIgnoreCase) Then
                                                       activeContainer = Nothing
                                                   End If
                                                   ResetVerification()
                                                   UpdateScannerAvailability()
                                               End Sub
        AddHandler firstQr.TextChanged, Sub()
                                            ResetVerification()
                                            If selectedRule IsNot Nothing AndAlso selectedRule.ScanCount = 1 Then
                                                secondQr.Clear()
                                            End If
                                        End Sub
        AddHandler secondQr.TextChanged, Sub() ResetVerification()
    End Sub

    Private Sub ContainerEnter(sender As Object, e As KeyEventArgs)
        If e.KeyCode <> Keys.Enter Then Return
        e.SuppressKeyPress = True
        If autoOpenBusy OrElse activeContainer IsNot Nothing AndAlso
           activeContainer.Status = "OPEN" Then Return
        If Not openButton.Enabled Then
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "Selecciona un modelo configurado o resuelve la operación SQL pendiente."
            Return
        End If
        autoOpenBusy = True
        Try
            OpenOrLoadContainer(sender, EventArgs.Empty)
            If activeContainer IsNot Nothing AndAlso activeContainer.Status = "OPEN" Then
                scanStatus.Text = "Contenedor listo. Escanea la serie SSI."
                firstQr.Focus()
            End If
        Finally
            autoOpenBusy = False
        End Try
    End Sub

    Private Sub FirstLabelEnter(sender As Object, e As KeyEventArgs)
        If e.KeyCode <> Keys.Enter Then Return
        e.SuppressKeyPress = True
        If autoScanBusy Then Return
        If selectedRule IsNot Nothing AndAlso selectedRule.ScanCount = 1 Then
            VerifyAndRecord()
        Else
            secondQr.Focus()
        End If
    End Sub

    Private Sub SecondLabelEnter(sender As Object, e As KeyEventArgs)
        If e.KeyCode <> Keys.Enter Then Return
        e.SuppressKeyPress = True
        VerifyAndRecord()
    End Sub

    ' La liberacion siempre se comprueba en CCS, nunca solo en memoria local.
    Private Function RejectionAllowsScan() As Boolean
        If rejectionStorageFailed Then
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "ESTACIÓN DETENIDA: no se pudo registrar el rechazo local. Solicita TI."
            Return False
        End If
        Try
            Dim localHold = ScanRejectionHold.Pending()
            If localHold IsNot Nothing Then
                Dim registered = RejectionAuthorizationData.Register(
                    localHold.ContainerId, localHold.RejectionType, localHold.SsiCode,
                    localHold.ClientCode, localHold.Reason)
                If registered Is Nothing Then
                    Throw New InvalidOperationException("CCS no confirmó el rechazo pendiente.")
                End If
                ScanRejectionHold.ClearIfRegistered(localHold.ContainerId)
            End If
            If activeContainer Is Nothing OrElse activeContainer.Status <> "OPEN" Then Return True
            Dim blocked = RejectionAuthorizationData.Pending(activeContainer.Id)
            If blocked Is Nothing Then Return True
            ResetVerification()
            Using dialog As New ScanRejectionForm(blocked, activeContainer.Code)
                dialog.ShowDialog(FindForm())
            End Using
            Dim stillBlocked = RejectionAuthorizationData.Pending(activeContainer.Id)
            If stillBlocked IsNot Nothing Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "ESCANEO BLOQUEADO: faltan aprobaciones del rechazo."
                Return False
            End If
            firstQr.Clear()
            secondQr.Clear()
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Rechazo autorizado. Retira la pieza rechazada y escanea otra pieza válida."
            Return False  ' Nunca registrar automaticamente la pieza rechazada.
        Catch ex As Exception
            ResetVerification()
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "Sin confirmación de autorizaciones en CCS: " & ex.Message
            MessageBox.Show(ex.Message, "Escáner detenido: autorización no verificable",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Sub RejectPiece(kind As String, detail As String)
        ResetVerification()
        scanStatus.ForeColor = Color.DarkRed
        scanStatus.Text = "RECHAZADO: " & detail
        If activeContainer Is Nothing OrElse activeContainer.Status <> "OPEN" Then
            MessageBox.Show(detail, "PIEZA RECHAZADA", MessageBoxButtons.OK, MessageBoxIcon.Stop)
            Return
        End If
        Try
            ' Guardar primero el incidente en disco para no perderlo si CCS falla.
            ScanRejectionHold.Save(New UnregisteredScanRejection With {
                .ContainerId = activeContainer.Id,
                .RejectionType = kind,
                .SsiCode = firstQr.Text.Trim(),
                .ClientCode = secondQr.Text.Trim(),
                .Reason = detail
            })
            Dim info = RejectionAuthorizationData.Register(
                activeContainer.Id, kind, firstQr.Text.Trim(), secondQr.Text.Trim(), detail)
            If info Is Nothing Then Throw New InvalidOperationException("No se confirmó el bloqueo del pallet.")
            ScanRejectionHold.ClearIfRegistered(activeContainer.Id)
            Using dialog As New ScanRejectionForm(info, activeContainer.Code)
                dialog.ShowDialog(FindForm())
            End Using
            firstQr.Clear()
            secondQr.Clear()
        Catch ex As Exception
            ' Si falla el almacenamiento local, el proceso en memoria se detiene.
            Try
                If ScanRejectionHold.Pending() Is Nothing Then rejectionStorageFailed = True
            Catch
                rejectionStorageFailed = True
            End Try
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "RECHAZO — intervención de TI requerida: " & ex.Message
            MessageBox.Show(detail & Environment.NewLine & Environment.NewLine &
                            "No se pudo confirmar el bloqueo en CCS: " & ex.Message &
                            Environment.NewLine & "Detén el escaneo y notifica a TI.",
                            "PIEZA RECHAZADA — DETENER ESTACIÓN",
                            MessageBoxButtons.OK, MessageBoxIcon.Stop)
            ' No continuar mientras falle la comprobacion del bloqueo en CCS.
        End Try
    End Sub

    Private Sub VerifyAndRecord()
        If autoScanBusy Then Return
        autoScanBusy = True
        Try
            If activeContainer Is Nothing OrElse activeContainer.Status <> "OPEN" Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "Abre un pallet antes de escanear piezas."
                Return
            End If
            If activeContainer.CurrentLevel Is Nothing OrElse
               activeContainer.CurrentLevel.ScannedPieces >= activeContainer.CurrentLevel.RequiredPieces Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "Confirma el nivel completo antes de escanear más piezas."
                Return
            End If
            If Not RejectionAllowsScan() Then Return
            VerifyQrFormat()
            If recordButton.Enabled Then
                RecordPiece(Me, EventArgs.Empty)
            Else
                firstQr.Focus()
            End If
        Finally
            autoScanBusy = False
        End Try
    End Sub

    Public Sub LoadStation()
        operatorValue.Text = If(SessionContext.FullName, "")
        customerValue.Text = If(SessionContext.BusinessUnitName, "")
        clockValue.Text = DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss")
        Try
            palletFormat = PalletIdSettings.Load()
            parts = PalletConfigurationData.GetParts().
                Where(Function(p) p.BusinessUnitId.HasValue AndAlso
                       p.BusinessUnitId.Value = SessionContext.BusinessUnitId).
                OrderBy(Function(p) p.Number).ToList()
            partPicker.DataSource = Nothing
            partPicker.DataSource = parts
            partPicker.SelectedIndex = -1
            selectedConfiguration = Nothing
            selectedRule = Nothing
            selectedFields.Clear()
            activePartTitle.Text = "Selecciona un número de parte"
            buildState.Text = "Esperando contenedor y modelo"
            ShowConfiguredPalletMetrics()
            activeContainer = Nothing
            lbl_lot.Text = "P######"
            DataGridView1.DataSource = Nothing
            ResetVerification()
            scannedList.Items.Clear()
            scannedList.Items.Add("Abre o recupera un contenedor para ver sus piezas.")
            ruleSummary.Text = "Ejemplo de contenedor: " & palletFormat.Example() &
                               Environment.NewLine & "Q-Wall PASS: Verificar al escanear" &
                               Environment.NewLine & "Prevención de duplicados: SQL UNIQUE(product_id)"
            containerInput.Clear()
            firstQr.Clear()
            secondQr.Clear()
            ConfigureScanMode()
            UpdateScannerAvailability()

            ' Restore the workstation's active pallet after logout/restart.
            Dim lockedCode = StationSettings.ReadLockedPallet()
            If lockedCode <> "" Then
                Dim saved = PaSPackingData.FindContainer(lockedCode)
                If saved Is Nothing Then
                    Throw New InvalidOperationException(
                        "La estación está vinculada al pallet " & lockedCode &
                        ", pero CCS no lo encuentra. Se requiere revisión de TI.")
                End If
                If saved.Status = "OPEN" Then
                    Dim model = parts.FirstOrDefault(Function(x) x.Id = saved.PartId)
                    If model Is Nothing Then
                        Throw New InvalidOperationException(
                            "El pallet abierto " & lockedCode &
                            " pertenece a otro cliente/modelo. Termínalo antes de cambiar la configuración.")
                    End If
                    partPicker.SelectedItem = model
                    containerInput.Text = lockedCode
                    activeContainer = saved
                    RefreshContainer()
                    scanStatus.Text = "Se recuperó el pallet bloqueado " & lockedCode &
                                      ". Termina o autoriza el cierre parcial antes de cambiar."
                Else
                    StationSettings.UnlockPallet(lockedCode)
                End If
            End If
            containerInput.Focus()
        Catch ex As Exception
            verifyButton.Enabled = False
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "No se pudo cargar la configuración del escáner: " & ex.Message
        End Try
    End Sub

    Private Sub PartChanged(sender As Object, e As EventArgs)
        activeContainer = Nothing
        ResetVerification()
        selectedConfiguration = Nothing
        ShowConfiguredPalletMetrics()
        selectedRule = Nothing
        selectedFields.Clear()
        Dim part = TryCast(partPicker.SelectedItem, PaSPart)
        If part Is Nothing Then
            ConfigureScanMode()
            UpdateScannerAvailability()
            Return
        End If
        Try
            selectedConfiguration = PalletConfigurationData.GetConfiguration(part.Id)
            selectedRule = ScanRulesData.ReadRules().FirstOrDefault(
                Function(r) r.PartId = part.Id AndAlso
                    r.CustomerId = SessionContext.BusinessUnitId AndAlso r.IsActive)
            If selectedRule IsNot Nothing Then
                selectedFields = ScanRulesData.ReadFields(selectedRule.Id)
            End If
            If selectedConfiguration Is Nothing OrElse Not selectedConfiguration.IsActive Then
                activePartTitle.Text = part.DisplayName
                buildState.Text = "Configuración de pallet inexistente o inactiva"
                ShowConfiguredPalletMetrics()
            Else
                activePartTitle.Text = part.DisplayName
                buildState.Text = "Contenedor: " & containerInput.Text.Trim().ToUpperInvariant() &
                    "   |   " & selectedConfiguration.Levels.Count & " niveles configurados"
                ShowConfiguredPalletMetrics()
            End If
            ruleSummary.Text = If(selectedRule Is Nothing,
                "Regla de escaneo: Inactiva / no disponible", "Escaneos configurados: " & selectedRule.ScanCount &
                Environment.NewLine & "Requiere coincidencia: " & If(selectedRule.RequiresMatch, "Yes", "No")) &
                Environment.NewLine & "Q-Wall PASS: Verificar al escanear" &
                Environment.NewLine & "Prevención de duplicados: SQL UNIQUE(product_id)" &
                Environment.NewLine & "Formato de contenedor: Configuración general"
        Catch ex As Exception
            buildState.Text = "No se pudo leer la configuración."
            ruleSummary.Text = ex.Message
        End Try
        firstQr.Clear()
        secondQr.Clear()
        ConfigureScanMode()
        UpdateScannerAvailability()
    End Sub

    Private Sub ConfigureScanMode()
        Dim singleScan = selectedRule IsNot Nothing AndAlso selectedRule.ScanCount = 1
        scanHelp.Text = If(singleScan,
            "Escanea SSI una vez. La serie del cliente se genera según el modelo.",
            "Escanea primero SSI y luego cliente. Presiona Enter después de cada código.")
        clientScanLabel.Text = If(singleScan,
            "SERIE DEL CLIENTE — VISTA AUTOMÁTICA", "ESCANEO 2 — SERIE DEL CLIENTE")
        secondQr.ReadOnly = singleScan
        secondQr.TabStop = Not singleScan
        secondQr.BackColor = If(singleScan, Color.FromArgb(242, 247, 252), Color.White)
        secondQr.Clear()
    End Sub

    Private Sub UpdateScannerAvailability()
        Dim part = TryCast(partPicker.SelectedItem, PaSPart)
        Dim validContainer = palletFormat IsNot Nothing AndAlso palletFormat.IsValid(containerInput.Text)
        verifyButton.Enabled = validContainer AndAlso part IsNot Nothing AndAlso
                               selectedConfiguration IsNot Nothing AndAlso selectedConfiguration.IsActive AndAlso
                               selectedRule IsNot Nothing AndAlso
                               (selectedRule.ScanCount = 1 OrElse selectedRule.ScanCount = 2)
        If Not validContainer Then
            scanStatus.Text = "Ingresa un ID de contenedor conforme a Configuración general."
        ElseIf selectedRule Is Nothing OrElse
               (selectedRule.ScanCount <> 1 AndAlso selectedRule.ScanCount <> 2) Then
            scanStatus.Text = "Se requiere una regla activa de uno o dos escaneos para esta parte."
        ElseIf selectedConfiguration Is Nothing OrElse Not selectedConfiguration.IsActive Then
            scanStatus.Text = "La configuración de pallet no existe o está inactiva."
        Else
            scanStatus.Text = If(selectedRule.ScanCount = 1,
                "Listo para escanear SSI; la serie del cliente se generará tras validar el formato.",
                "Listo para escanear ambas etiquetas.")
            scanStatus.ForeColor = navy
        End If
        UpdatePackingButtons()
    End Sub

    Private Sub SetQWallStatus(value As String)
        Dim lines = ruleSummary.Text.Split(New String() {Environment.NewLine},
                                          StringSplitOptions.None)
        For index As Integer = 0 To lines.Length - 1
            If lines(index).StartsWith("Q-Wall PASS:", StringComparison.Ordinal) Then
                lines(index) = "Q-Wall PASS: " & value
            End If
        Next
        ruleSummary.Text = String.Join(Environment.NewLine, lines)
    End Sub

    Private Sub ResetVerification()
        verifiedSsi = ""
        verifiedClient = ""
        recordButton.Enabled = False
    End Sub

    Private Sub UpdatePackingButtons()
        Dim openNow = activeContainer IsNot Nothing AndAlso activeContainer.Status = "OPEN"
        Dim current As PaSPackedLevel = Nothing
        If openNow Then current = activeContainer.CurrentLevel
        Dim pendingOp = ConnectionRecovery.Pending()
        Dim pending = pendingOp IsNot Nothing
        pendingButton.Enabled = pending
        If pending Then
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "El empaque está bloqueado por una operación SQL pendiente: " &
                pendingOp.Kind & " (ID de contenedor " & pendingOp.ContainerId & "). " &
                "Usa Acciones de pallet > Revisar operación SQL pendiente."
        End If
        Dim lockedCode = StationSettings.ReadLockedPallet()
        Dim locked = openNow OrElse lockedCode <> ""
        containerInput.ReadOnly = locked
        partPicker.Enabled = Not locked
        openButton.Enabled = Not pending AndAlso Not openNow AndAlso lockedCode = ""
        Dim part = TryCast(partPicker.SelectedItem, PaSPart)
        verifyButton.Enabled = Not pending AndAlso
            palletFormat IsNot Nothing AndAlso palletFormat.IsValid(containerInput.Text) AndAlso
            part IsNot Nothing AndAlso selectedConfiguration IsNot Nothing AndAlso
            selectedConfiguration.IsActive AndAlso selectedRule IsNot Nothing AndAlso
            (selectedRule.ScanCount = 1 OrElse selectedRule.ScanCount = 2)
        recordButton.Enabled = Not pending AndAlso openNow AndAlso current IsNot Nothing AndAlso
            verifiedSsi <> "" AndAlso verifiedClient <> "" AndAlso
            firstQr.Text.Trim() = verifiedSsi AndAlso
            selectedRule IsNot Nothing AndAlso
            (selectedRule.ScanCount = 1 OrElse secondQr.Text.Trim() = verifiedClient)
        levelButton.Enabled = Not pending AndAlso openNow AndAlso current IsNot Nothing AndAlso
            current.ScannedPieces = current.RequiredPieces
        closePallet.Enabled = Not pending AndAlso openNow AndAlso current Is Nothing AndAlso
            activeContainer.Levels.Count > 0 AndAlso
            activeContainer.Levels.All(Function(l) l.Status = "CLOSED" AndAlso
                                      l.ScannedPieces = l.RequiredPieces)
        partialButton.Enabled = Not pending AndAlso openNow AndAlso
            activeContainer.PieceTotal > 0 AndAlso
            activeContainer.PieceTotal < activeContainer.RequiredTotal
        cancelEmptyButton.Enabled = Not pending AndAlso openNow AndAlso
            activeContainer.PieceTotal = 0 AndAlso
            activeContainer.ClosedLevels = 0
        confirmAgain.Enabled = levelButton.Enabled
        confirmAgain.Visible = levelButton.Enabled
        confirmAgain.Text = If(current IsNot Nothing AndAlso
                               current.LevelNumber = activeContainer.Levels.Count,
                               "Confirmar cierre final del pallet",
                               "Confirmar nivel empacado")
    End Sub

    Private Sub ReviewPendingSql(sender As Object, e As EventArgs)
        Try
            Dim p = ConnectionRecovery.Pending()
            If p Is Nothing Then
                AfterRecovery()
                Return
            End If
            Dim description = ConnectionRecovery.DescribePending()
            Dim result = ConnectionRecovery.CheckAndRecover()
            If ConnectionRecovery.Pending() Is Nothing Then
                AfterRecovery(result)
                MessageBox.Show(
                    description & Environment.NewLine & Environment.NewLine &
                    "CCS result: " & result & Environment.NewLine &
                    "No quedan operaciones pendientes. Los controles están disponibles.",
                    "Operación SQL conciliada", MessageBoxButtons.OK,
                    MessageBoxIcon.Information)
                Return
            End If
            Dim current = ConnectionRecovery.Pending()
            If current IsNot Nothing AndAlso current.ReviewRequired Then
                If MessageBox.Show(
                    description & Environment.NewLine & Environment.NewLine &
                    "CCS no contiene la operación rechazada. " &
                    "¿Descartar SOLO este intento rechazado? No se eliminarán piezas de CCS." &
                    Environment.NewLine &
                    "Si aún falta empacar la pieza, vuelve a escanearla y verificarla.",
                    "El rechazo de SQL requiere confirmación",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) = DialogResult.Yes Then
                    ConnectionRecovery.DismissRejectedAfterCheck()
                    AfterRecovery()
                    MessageBox.Show(
                        "Intento rechazado descartado después de verificar CCS. " &
                        "No se modificaron registros de empaque en SQL.",
                        "Escáner desbloqueado", MessageBoxButtons.OK,
                        MessageBoxIcon.Information)
                End If
            Else
                MessageBox.Show(
                    description & Environment.NewLine &
                    "La operación sigue sin resolverse. El escáner permanece bloqueado.",
                    "Esperando SQL", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        Catch ex As Exception
            MessageBox.Show(
                "No se pudo conciliar la operación SQL. El escáner permanece bloqueado." &
                Environment.NewLine & ex.Message,
                "Error al conciliar SQL", MessageBoxButtons.OK,
                MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub OpenOrLoadContainer(sender As Object, e As EventArgs)
        Try
            Dim part = TryCast(partPicker.SelectedItem, PaSPart)
            If part Is Nothing Then Throw New InvalidOperationException("Selecciona un modelo.")
            Dim code = containerInput.Text.Trim().ToUpperInvariant()
            Dim lockedCode = StationSettings.ReadLockedPallet()
            If lockedCode <> "" AndAlso Not String.Equals(code, lockedCode,
                                                          StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException(
                    "Termina el pallet " & lockedCode & " antes de abrir otro pallet.")
            End If
            If activeContainer IsNot Nothing AndAlso activeContainer.Status = "OPEN" AndAlso
               Not String.Equals(activeContainer.Code, code, StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException(
                    "Completa o cierra parcialmente el pallet actual antes de cambiar.")
            End If
            If Not PalletIdSettings.Load().IsValid(code) Then
                Throw New InvalidOperationException("El formato del contenedor no coincide con Configuración general.")
            End If

            Dim existing = PaSPackingData.FindContainer(code)
            If existing Is Nothing Then
                If selectedConfiguration Is Nothing OrElse Not selectedConfiguration.IsActive Then
                    Throw New InvalidOperationException("Se requiere una configuración de pallet activa.")
                End If
                ' Container scan + Enter is the operator's explicit open action.
                ' No additional message box interrupts barcode scanning.
                Dim createdId = PaSPackingData.OpenContainer(code, part.Id)
                activeContainer = PaSPackingData.ReadContainer(createdId)
            Else
                If existing.PartId <> part.Id Then
                    Throw New InvalidOperationException("El contenedor pertenece a otro modelo.")
                End If
                activeContainer = existing
            End If
            If activeContainer.Status = "OPEN" Then
                StationSettings.LockPallet(code)
            End If
            ResetVerification()
            RefreshContainer()
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Listo para escanear la serie SSI."
            firstQr.Focus()
        Catch ex As Exception
            ShowPackingError(ex)
        End Try
    End Sub

    Public Sub RefreshAfterHistory()
        ' A correction may have changed the active pallet. Preserve selection,
        ' reload authoritative SQL state, and invalidate any pre-verified QR.
        ResetVerification()
        If activeContainer IsNot Nothing Then
            RefreshContainer()
            If activeContainer.Status <> "OPEN" Then
                ResetAfterPalletClosed(activeContainer.Code)
            End If
        End If
    End Sub

    ' Indicadores sin pallet abierto: muestran la configuración del modelo seleccionado.
    Private Sub ShowConfiguredPalletMetrics()
        Dim configuration = selectedConfiguration
        If configuration Is Nothing OrElse Not configuration.IsActive Then
            lblLevelsCount.Text = "0 / 0"
            lblLevelPiecesCount.Text = "0 / 0"
            lblPalletPiecesCount.Text = "0 / 0"
        Else
            lblLevelsCount.Text = "0 / " & configuration.Levels.Count
            lblLevelPiecesCount.Text = If(configuration.Levels.Count > 0,
                "0 / " & configuration.Levels(0), "0 / 0")
            lblPalletPiecesCount.Text = "0 / " & configuration.TotalPieces
        End If
    End Sub

    Private Shared Function EstadoPalletVisible(status As String) As String
        Select Case If(status, "").ToUpperInvariant()
            Case "OPEN"
                Return "ABIERTO"
            Case "CLOSED"
                Return "CERRADO"
            Case "PARTIAL"
                Return "PARCIAL"
            Case "TRANSFERRED"
                Return "TRANSFERIDO"
            Case "CANCELLED"
                Return "CANCELADO"
            Case Else
                Return status
        End Select
    End Function

    Private Sub RefreshContainer()
        If activeContainer Is Nothing Then Return
        activeContainer = PaSPackingData.ReadContainer(activeContainer.Id)
        If activeContainer Is Nothing Then
            Throw New InvalidOperationException("No se encontró el contenedor.")
        End If
        lbl_lot.Text = activeContainer.Code
        RefreshLevelScanGrid()
        Dim current = activeContainer.CurrentLevel
        Dim actual = activeContainer.PieceTotal
        Dim required = activeContainer.RequiredTotal
        lblLevelsCount.Text = activeContainer.ClosedLevels & " / " & activeContainer.Levels.Count
        Dim visibleLevel = If(current, activeContainer.Levels.
            OrderByDescending(Function(level) level.LevelNumber).FirstOrDefault())
        lblLevelPiecesCount.Text = If(visibleLevel Is Nothing, "0 / 0",
            visibleLevel.ScannedPieces & " / " & visibleLevel.RequiredPieces)
        lblPalletPiecesCount.Text = actual & " / " & required
        Dim percentage = If(required > 0,
            Math.Min(100, CInt(Math.Truncate(CDbl(actual) * 100.0 / required))), 0)

        levelTiles.SuspendLayout()
        Try
            levelTiles.Controls.Clear()
            For Each level In activeContainer.Levels.OrderBy(Function(x) x.LevelNumber)
                Dim isClosed = level.Status = "CLOSED"
                Dim isFull = level.ScannedPieces = level.RequiredPieces
                Dim tileColor As System.Drawing.Color = If(isClosed, System.Drawing.Color.FromArgb(28, 113, 83),
                               If(isFull, System.Drawing.Color.FromArgb(196, 128, 40), PaSTheme.Navy))
                Dim tile As New Label With {
                    .Text = "L" & level.LevelNumber & "  " &
                            level.ScannedPieces & "/" & level.RequiredPieces &
                            If(isClosed, "  ✓", ""),
                    .TextAlign = ContentAlignment.MiddleCenter,
                    .Size = New Size(118, 38),
                    .Margin = New Padding(0, 0, 9, 4),
                    .BackColor = tileColor,
                    .ForeColor = Color.White,
                    .Font = New Font("Segoe UI", 10, FontStyle.Bold)
                }
                levelTiles.Controls.Add(tile)
            Next
        Finally
            levelTiles.ResumeLayout()
        End Try

        buildState.Text = "Contenedor: " & activeContainer.Code & "  |  " & EstadoPalletVisible(activeContainer.Status)
        If activeContainer.Status <> "OPEN" Then
            StationSettings.UnlockPallet(activeContainer.Code)
        End If
        scannedList.Items.Clear()
        If activeContainer.Pieces.Count = 0 Then
            scannedList.Items.Add("No hay piezas registradas.")
        Else
            For Each piece In activeContainer.Pieces
                scannedList.Items.Add("Nivel " & piece.LevelNumber & " | " &
                    piece.SsiCode & " | " & piece.ClientCode)
            Next
        End If
        UpdatePackingButtons()
    End Sub

    ' Lista todas las piezas confirmadas del pallet, sin reiniciarse entre niveles.
    ' La información proviene del estado real actualizado de SQL Server CCS.
    Private Sub RefreshLevelScanGrid()
        Dim rows As New System.Data.DataTable()
        rows.Columns.Add("ContainerCode", GetType(String))
        rows.Columns.Add("SsiModel", GetType(String))
        rows.Columns.Add("LevelNumber", GetType(Integer))
        rows.Columns.Add("PieceNumber", GetType(Integer))
        rows.Columns.Add("SsiSerial", GetType(String))
        rows.Columns.Add("ClientSerial", GetType(String))
        rows.Columns.Add("ScanTime", GetType(String))

        If activeContainer IsNot Nothing Then
            Dim selectedPart = parts.FirstOrDefault(Function(p) p.Id = activeContainer.PartId)
            Dim modelNumber = If(selectedPart Is Nothing, "", selectedPart.Number)
            Dim pieceNumber As Integer = 0
            For Each piece In activeContainer.Pieces.
                OrderBy(Function(p) p.ScannedAt).ThenBy(Function(p) p.ProductId)
                pieceNumber += 1
                rows.Rows.Add(activeContainer.Code, modelNumber, piece.LevelNumber,
                              pieceNumber, piece.SsiCode, piece.ClientCode,
                              piece.ScannedAt.ToString("dd/MM/yyyy HH:mm:ss"))
            Next
        End If

        DataGridView1.DataSource = rows
        DataGridView1.ClearSelection()
    End Sub

    Private Sub ResetAfterPalletClosed(closedCode As String)
        ' The source has already reached a terminal SQL status; unlock the
        ' station and clear the visible Container field for the next pallet.
        StationSettings.UnlockPallet(closedCode)
        activeContainer = Nothing
        ResetVerification()
        firstQr.Clear()
        secondQr.Clear()
        containerInput.Clear()
        scannedList.Items.Clear()
        scannedList.Items.Add("No hay pallet activo. Ingresa o escanea el siguiente contenedor.")
        lbl_lot.Text = "P######"
        DataGridView1.DataSource = Nothing
        levelTiles.Controls.Clear()
        ShowConfiguredPalletMetrics()
        buildState.Text = "Listo para el siguiente pallet"
        SetQWallStatus("Verificar al escanear")
        UpdateScannerAvailability()
        containerInput.Focus()
    End Sub

    Public Sub LoadTransferredPallet(destinationCode As String)
        If ConnectionRecovery.Pending() IsNot Nothing Then
            Throw New InvalidOperationException("Resuelve la operación SQL pendiente antes de cargar una transferencia.")
        End If
        Dim locked = StationSettings.ReadLockedPallet()
        If locked <> "" AndAlso Not String.Equals(locked, destinationCode,
                                                   StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidOperationException(
                "Termina el pallet " & locked & " antes de cargar el pallet transferido.")
        End If
        Dim pallet = PaSPackingData.FindContainer(destinationCode)
        If pallet Is Nothing OrElse pallet.Status <> "OPEN" Then
            Throw New InvalidOperationException("El pallet de destino transferido no está ABIERTO.")
        End If
        Dim part = parts.FirstOrDefault(Function(x) x.Id = pallet.PartId)
        If part Is Nothing Then
            Throw New InvalidOperationException(
                "El modelo del pallet de destino no está disponible para el cliente de esta estación.")
        End If
        partPicker.SelectedItem = part
        containerInput.Text = pallet.Code
        activeContainer = pallet
        StationSettings.LockPallet(pallet.Code)
        ResetVerification()
        RefreshContainer()
        scanStatus.ForeColor = Color.DarkGreen
        scanStatus.Text = "Transferencia cargada. Continúa escaneando las piezas restantes."
    End Sub

    Public Sub AfterRecovery(Optional recoveryResult As String = "")
        ResetVerification()
        Try
            If activeContainer IsNot Nothing Then
                RefreshContainer()
                If activeContainer.Status <> "OPEN" Then
                    ResetAfterPalletClosed(activeContainer.Code)
                End If
            End If
        Catch ex As Exception
            scanStatus.Text = "Se restableció la conexión, pero falló la actualización del pallet: " & ex.Message
            Return
        End Try
        UpdateScannerAvailability()
        If recoveryResult.StartsWith("NOT CANCELLED", StringComparison.OrdinalIgnoreCase) Then
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "No se canceló el pallet vacío. Solicita nuevamente autorización."
        ElseIf recoveryResult.StartsWith("NOT CLOSED", StringComparison.OrdinalIgnoreCase) Then
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "No se cerró el pallet parcial. Solicita nuevamente autorización."
        Else
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Conexión restablecida. Operación SQL pendiente conciliada."
        End If
    End Sub

    Private Sub RecordPiece(sender As Object, e As EventArgs)
        If Not recordButton.Enabled OrElse activeContainer Is Nothing Then Return
        If Not RejectionAllowsScan() Then Return
        Try
            Dim current = activeContainer.CurrentLevel
            If current Is Nothing Then Throw New InvalidOperationException("No hay nivel abierto.")
            ' Durable write-ahead intent; SQL remains the only authority for PASS.
            ConnectionRecovery.SaveBeforeSend("ADD_PIECE", activeContainer.Id,
                current.LevelNumber, verifiedSsi, verifiedClient, SessionContext.UserId)
            PaSPackingData.AddPiece(activeContainer.Id, current.LevelNumber, verifiedSsi, verifiedClient)
            ConnectionRecovery.MarkDelivered()
            ResetVerification()
            firstQr.Clear()
            secondQr.Clear()
            RefreshContainer()
            SetQWallStatus("Verificar al escanear")
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Pieza registrada en el nivel " & current.LevelNumber & "."
            firstQr.Focus()
            If levelButton.Enabled Then PromptCompletedLevel()
        Catch ex As Exception
            ResetVerification()
            Dim sqlError = TryCast(ex, Microsoft.Data.SqlClient.SqlException)
            If sqlError IsNot Nothing AndAlso
               (sqlError.Number = 51106 OrElse sqlError.Number = 51107 OrElse
                sqlError.Number = 51108 OrElse sqlError.Number = 51109 OrElse
                sqlError.Number = 2601 OrElse sqlError.Number = 2627) Then
                ' El servidor rechazo transaccionalmente esta insercion.
                ' Verificar en CCS antes de descartar el intento durable local.
                Try
                    ConnectionRecovery.FlagReview(ex.Message)
                    ConnectionRecovery.DismissRejectedAfterCheck()
                    If sqlError.Number = 51106 OrElse sqlError.Number = 51108 OrElse
                       sqlError.Number = 51109 Then
                        RejectPiece("QWALL_REJECT", "Q-Wall rechazó la pieza: " & ex.Message)
                    Else
                        Dim part = TryCast(partPicker.SelectedItem, PaSPart)
                        Dim location = If(part Is Nothing, "",
                            RejectionAuthorizationData.FindPackedSerial(
                                part.Id, firstQr.Text.Trim(), secondQr.Text.Trim()))
                        Dim same = activeContainer IsNot Nothing AndAlso
                            String.Equals(location, activeContainer.Code,
                                          StringComparison.OrdinalIgnoreCase)
                        RejectPiece(If(same, "DUPLICATE_SAME", "DUPLICATE_OTHER"),
                            If(location = "", "Pieza duplicada: ya se encuentra empacada.",
                               If(same, "La pieza ya esta en ESTE CONTENEDOR: ",
                                        "La pieza ya esta en OTRO CONTENEDOR: ") & location))
                    End If
                    Return
                Catch recoveryEx As Exception
                    ShowPackingError(recoveryEx)
                    Return
                End Try
            End If
            ShowPackingError(ex)
        End Try
    End Sub

    Private Sub PromptCompletedLevel()
        If autoConfirmVisible OrElse Not levelButton.Enabled OrElse
           activeContainer Is Nothing Then Return
        autoConfirmVisible = True
        Try
            ' Refresh SQL counts before showing physical packing confirmation.
            RefreshContainer()
            If Not levelButton.Enabled Then Return
            Dim current = activeContainer.CurrentLevel
            If current Is Nothing Then Return
            Dim isLast = current.LevelNumber = activeContainer.Levels.Count
            Using dialog As New PackingConfirmationForm(
                activeContainer.Code, current.LevelNumber, activeContainer.Levels.Count,
                current.RequiredPieces, isLast)
                If dialog.ShowDialog(FindForm()) <> DialogResult.OK Then
                    scanStatus.ForeColor = Color.FromArgb(149, 94, 13)
                    scanStatus.Text = "El nivel está completo. Confirma el empaque físico para continuar."
                    Return
                End If
            End Using
            CloseLevel(Me, EventArgs.Empty)
            If ConnectionRecovery.Pending() IsNot Nothing Then Return
            If isLast AndAlso closePallet.Enabled Then
                CompletePallet(Me, EventArgs.Empty)
            ElseIf activeContainer IsNot Nothing AndAlso
                   activeContainer.Status = "OPEN" Then
                scanStatus.ForeColor = Color.DarkGreen
                scanStatus.Text = "Nivel confirmado. Escanea el siguiente nivel."
                firstQr.Focus()
            End If
        Catch ex As Exception
            ShowPackingError(ex)
        Finally
            autoConfirmVisible = False
        End Try
    End Sub

    Private Sub CloseLevel(sender As Object, e As EventArgs)
        If Not levelButton.Enabled OrElse activeContainer Is Nothing Then Return
        Try
            Dim current = activeContainer.CurrentLevel
            If current Is Nothing Then Return
            ConnectionRecovery.SaveBeforeSend("CLOSE_LEVEL", activeContainer.Id,
                current.LevelNumber, "", "", SessionContext.UserId)
            PaSPackingData.CloseLevel(activeContainer.Id, current.LevelNumber)
            ConnectionRecovery.MarkDelivered()
            ResetVerification()
            RefreshContainer()
            scanStatus.Text = "Nivel " & current.LevelNumber & " cerrado."
        Catch ex As Exception
            ShowPackingError(ex)
        End Try
    End Sub

    Private Sub CompletePallet(sender As Object, e As EventArgs)
        If Not closePallet.Enabled OrElse activeContainer Is Nothing Then Return
        Try
            ConnectionRecovery.SaveBeforeSend("CLOSE_PALLET", activeContainer.Id,
                0, "", "", SessionContext.UserId)
            PaSPackingData.CloseContainer(activeContainer.Id)
            ConnectionRecovery.MarkDelivered()
            ResetVerification()
            RefreshContainer()
            Dim closedCode = activeContainer.Code
            ResetAfterPalletClosed(closedCode)
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Pallet " & closedCode & " completado. Listo para el siguiente contenedor."
        Catch ex As Exception
            ShowPackingError(ex)
        End Try
    End Sub

    Private Sub ClosePartialPallet(sender As Object, e As EventArgs)
        If Not partialButton.Enabled OrElse activeContainer Is Nothing Then Return
        Try
            ' Refresh from CCS immediately before requesting approval.
            RefreshContainer()
            If activeContainer.Status <> "OPEN" OrElse activeContainer.PieceTotal <= 0 OrElse
               activeContainer.PieceTotal >= activeContainer.RequiredTotal Then
                Throw New InvalidOperationException("El cierre parcial requiere un pallet ABIERTO, incompleto y con piezas.")
            End If
            Using authorization As New PartialPalletAuthorizationForm(
                activeContainer.Code, activeContainer.PieceTotal, activeContainer.RequiredTotal)
                If authorization.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                If MessageBox.Show(
                    "¿Cerrar pallet " & activeContainer.Code & " como PARCIAL con " &
                    activeContainer.PieceTotal & " of " & activeContainer.RequiredTotal &
                    " piezas? No se podrá volver a abrir.", "Confirmar pallet parcial",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return

                ' Only the operation identity is persisted; credentials are
                ' never stored locally or replayed after a network outage.
                ConnectionRecovery.SaveBeforeSend(
                    "CLOSE_PARTIAL", activeContainer.Id, 0, "", "", SessionContext.UserId)
                Try
                    PaSPackingData.ClosePartialContainer(
                        activeContainer.Id, authorization.AuthorizedUserId,
                        authorization.AuthorizedBarcode, authorization.AuthorizedPassword,
                        authorization.Reason)
                    ConnectionRecovery.MarkDelivered()
                Catch ex As Microsoft.Data.SqlClient.SqlException
                    If ex.Number >= 50000 OrElse ex.Number = 2601 OrElse ex.Number = 2627 Then
                        ' SQL rejected it transactionally: credentials/reason/etc.
                        ' Retrying requires a fresh explicit approval.
                        ConnectionRecovery.MarkDelivered()
                    End If
                    Throw
                End Try
            End Using
            ResetVerification()
            RefreshContainer()
            Dim closedCode = activeContainer.Code
            ResetAfterPalletClosed(closedCode)
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Pallet parcial " & closedCode & " cerrado. Listo para el siguiente contenedor."
        Catch ex As Exception
            ShowPackingError(ex)
        End Try
    End Sub

    Private Sub CancelEmptyPallet(sender As Object, e As EventArgs)
        If Not cancelEmptyButton.Enabled OrElse activeContainer Is Nothing Then Return
        Try
            ' Always refresh the authoritative CCS counts before authorization.
            RefreshContainer()
            If activeContainer.Status <> "OPEN" OrElse
               activeContainer.PieceTotal <> 0 OrElse
               activeContainer.ClosedLevels <> 0 Then
                Throw New InvalidOperationException(
                    "Solo se puede cancelar un pallet ABIERTO sin piezas ni niveles cerrados.")
            End If
            Using authorization As New PartialPalletAuthorizationForm(
                activeContainer.Code, 0, activeContainer.RequiredTotal, True)
                If authorization.ShowDialog(FindForm()) <> DialogResult.OK Then Return
                If MessageBox.Show(
                    "¿Cancelar pallet vacío " & activeContainer.Code & "?" &
                    Environment.NewLine &
                    "El código del contenedor quedará reservado y no podrá reutilizarse.",
                    "Confirmar cancelación de pallet vacío",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then Return

                ' Retain the operation identity across network loss, but never
                ' persist a second user's credentials on the workstation.
                ConnectionRecovery.SaveBeforeSend(
                    "CANCEL_EMPTY", activeContainer.Id, 0, "", "", SessionContext.UserId)
                Try
                    PaSPackingData.CancelEmptyPallet(
                        activeContainer.Id, authorization.AuthorizedUserId,
                        authorization.AuthorizedBarcode, authorization.AuthorizedPassword,
                        authorization.Reason)
                    ConnectionRecovery.MarkDelivered()
                Catch ex As Microsoft.Data.SqlClient.SqlException
                    If ex.Number >= 50000 OrElse ex.Number = 2601 OrElse ex.Number = 2627 Then
                        ' Explicit business rejection: SQL rolled back, so a fresh
                        ' authorization can be used. A timeout remains pending.
                        ConnectionRecovery.MarkDelivered()
                    End If
                    Throw
                End Try
            End Using
            ResetVerification()
            RefreshContainer()
            Dim cancelledCode = activeContainer.Code
            ResetAfterPalletClosed(cancelledCode)
            scanStatus.ForeColor = Color.DarkGreen
            scanStatus.Text = "Pallet vacío " & cancelledCode &
                              " cancelado. Listo para el siguiente contenedor."
        Catch ex As Exception
            ShowPackingError(ex)
        End Try
    End Sub

    Private Sub ShowPackingError(ex As Exception)
        ' A SQL business rejection cannot be blindly retried. Keep its intent
        ' for human review; a connection timeout remains automatically recoverable.
        Dim sqlError = TryCast(ex, Microsoft.Data.SqlClient.SqlException)
        If sqlError IsNot Nothing AndAlso (sqlError.Number >= 50000 OrElse
                                           sqlError.Number = 2601 OrElse sqlError.Number = 2627) Then
            ConnectionRecovery.FlagReview(ex.Message)
        End If
        If ConnectionRecovery.Pending() IsNot Nothing Then
            ResetVerification()
            recordButton.Enabled = False
            levelButton.Enabled = False
            closePallet.Enabled = False
            cancelEmptyButton.Enabled = False
            openButton.Enabled = False
            scanStatus.Text = "SQL action pending verification. Wait for connection recovery."
        End If
        scanStatus.ForeColor = Color.DarkRed
        scanStatus.Text = ex.Message
        MessageBox.Show(ex.Message, "PackAndShip", MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Private Sub VerifyQrFormat()
        ResetVerification()
        SetQWallStatus("NOT VERIFIED")
        If Not verifyButton.Enabled Then
            UpdateScannerAvailability()
            Return
        End If
        Try
            ' Re-read global format immediately before every verification.
            Dim currentFormat = PalletIdSettings.Load()
            If Not currentFormat.IsValid(containerInput.Text) Then
                scanStatus.Text = "El formato del contenedor cambió en Configuración general."
                scanStatus.ForeColor = Color.DarkRed
                Return
            End If
            Dim selectedPart = TryCast(partPicker.SelectedItem, PaSPart)
            Dim result = ScanFormatValidator.CheckScans(
                firstQr.Text, secondQr.Text, selectedRule, selectedFields, selectedPart)
            If selectedRule IsNot Nothing AndAlso selectedRule.ScanCount = 1 Then
                secondQr.Text = If(result.Valid, result.GeneratedClientCode, "")
            End If
            If Not result.Valid Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = result.Message
                Return
            End If

            ' Check the exact full SSI and Client strings saved by Q-Wall,
            ' not only the extracted numeric portion. This is read-only.
            Dim fullClientCode As String = If(selectedRule.ScanCount = 1,
                                              result.GeneratedClientCode, secondQr.Text.Trim())
            Dim qwall = QWallPassValidator.Verify(selectedPart.Id, firstQr.Text.Trim(), fullClientCode)
            If Not qwall.Verified Then
                SetQWallStatus("RECHAZADO")
                RejectPiece("QWALL_REJECT", qwall.Message)
                Return
            End If

            ' El producto puede estar ya empacado en este mismo pallet o en otro.
            ' La restriccion UNIQUE(product_id) en SQL sigue siendo la ultima garantia.
            Dim alreadyIn = RejectionAuthorizationData.FindPackedProduct(qwall.ProductId)
            If alreadyIn <> "" Then
                Dim sameContainer = String.Equals(alreadyIn, activeContainer?.Code,
                                                  StringComparison.OrdinalIgnoreCase)
                RejectPiece(If(sameContainer, "DUPLICATE_SAME", "DUPLICATE_OTHER"),
                            If(sameContainer,
                               "La pieza ya esta registrada en ESTE MISMO CONTENEDOR: " & alreadyIn,
                               "La pieza ya esta registrada en OTRO CONTENEDOR: " & alreadyIn))
                Return
            End If

            SetQWallStatus("VERIFIED")
            verifiedSsi = firstQr.Text.Trim()
            verifiedClient = fullClientCode
            UpdatePackingButtons()
            If activeContainer Is Nothing Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "Q-Wall PASS confirmado. Abre o recupera el contenedor antes de registrar."
            ElseIf activeContainer.Status <> "OPEN" Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "Q-Wall PASS confirmado. Este contenedor está " & activeContainer.Status & "."
            ElseIf activeContainer.CurrentLevel Is Nothing Then
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "Q-Wall PASS confirmado. No hay nivel abierto disponible."
            ElseIf activeContainer.CurrentLevel.ScannedPieces >= activeContainer.CurrentLevel.RequiredPieces Then
                scanStatus.ForeColor = Color.FromArgb(145, 89, 10)
                scanStatus.Text = "Q-Wall PASS confirmado. Cierra el nivel completo antes de continuar."
            ElseIf recordButton.Enabled Then
                scanStatus.ForeColor = Color.DarkGreen
                scanStatus.Text = "Q-Wall PASS verificado. Registrando pieza en CCS."
            Else
                scanStatus.ForeColor = Color.DarkRed
                scanStatus.Text = "Q-Wall PASS confirmado, pero el registro no está disponible. Vuelve a cargar el contenedor."
            End If
        Catch ex As Exception
            scanStatus.ForeColor = Color.DarkRed
            scanStatus.Text = "Verification unavailable: " & ex.Message
        End Try
    End Sub


End Class
