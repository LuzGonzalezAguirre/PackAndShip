Imports System.Drawing
Imports System.Windows.Forms

' Two scanned pallet codes. The server performs the entire transfer atomically.
Public Class PalletTransferForm
    Inherits Form

    Private ReadOnly sourceInput As New TextBox()
    Private ReadOnly destinationInput As New TextBox()
    Private ReadOnly summary As New Label()
    Private ReadOnly transferButton As New Button()
    Private ReadOnly scanNext As New Label()
    Public ReadOnly Property DestinationCode As String
        Get
            Return _destinationCode
        End Get
    End Property
    Private _destinationCode As String = ""

    Public Sub New()
        Text = "PackAndShip | Pallet Transfers"
        ClientSize = New Size(690, 440)
        MinimumSize = New Size(690, 440)
        MaximumSize = New Size(690, 440)
        FormBorderStyle = FormBorderStyle.FixedDialog
        StartPosition = FormStartPosition.CenterParent
        MaximizeBox = False
        MinimizeBox = False
        Font = New Font("Segoe UI", 10)
        BackColor = PaSTheme.Background

        Dim heading As New Label With {
            .Text = "PALLET TRANSFER",
            .Location = New Point(24, 18), .Size = New Size(620, 38),
            .ForeColor = PaSTheme.Navy,
            .Font = New Font("Segoe UI", 20, FontStyle.Bold)
        }
        Dim subtitle As New Label With {
            .Text = "Transfer a closed PARTIAL pallet into a new container code.",
            .Location = New Point(25, 62), .Size = New Size(620, 26),
            .ForeColor = PaSTheme.TextSecondary
        }
        Dim sourceLabel As New Label With {
            .Text = "SCAN ORIGINAL PALLET (PARTIAL)",
            .Location = New Point(25, 107), .Size = New Size(360, 24)
        }
        sourceInput.SetBounds(25, 133, 620, 38)
        sourceInput.CharacterCasing = CharacterCasing.Upper
        sourceInput.MaxLength = 18
        sourceInput.Font = New Font("Consolas", 14)
        AddHandler sourceInput.Leave, Sub() PreviewSource()
        AddHandler sourceInput.KeyDown, Sub(sender, e)
            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True
                PreviewSource()
                destinationInput.Focus()
            End If
        End Sub

        Dim targetLabel As New Label With {
            .Text = "SCAN NEW CONTAINER",
            .Location = New Point(25, 188), .Size = New Size(360, 24)
        }
        destinationInput.SetBounds(25, 216, 620, 38)
        destinationInput.CharacterCasing = CharacterCasing.Upper
        destinationInput.MaxLength = 18
        destinationInput.Font = New Font("Consolas", 14)
        AddHandler destinationInput.KeyDown, Sub(sender, e)
            If e.KeyCode = Keys.Enter Then
                e.SuppressKeyPress = True
                transferButton.Focus()
            End If
        End Sub

        summary.SetBounds(25, 270, 620, 57)
        summary.Text = "Scan the source pallet to load its current SQL quantities."
        summary.ForeColor = Color.FromArgb(48, 70, 88)
        scanNext.SetBounds(25, 332, 385, 24)
        scanNext.Text = "All transferred pieces remain traceable to the source."
        scanNext.ForeColor = PaSTheme.TextSecondary

        transferButton.Text = "Transfer and open new pallet"
        transferButton.SetBounds(390, 365, 255, 44)
        AddHandler transferButton.Click, AddressOf ExecuteTransfer
        Dim cancel As New Button With {
            .Text = "Cancel", .Location = New Point(250, 365),
            .Size = New Size(120, 44), .DialogResult = DialogResult.Cancel
        }

        Controls.AddRange(New Control() {
            heading,subtitle,sourceLabel,sourceInput,targetLabel,destinationInput,
            summary,scanNext,transferButton,cancel
        })
        CancelButton = cancel
        AddHandler Shown, Sub() sourceInput.Focus()
    End Sub

    Private Function GetSource() As PaSPackedContainer
        Dim code = sourceInput.Text.Trim().ToUpperInvariant()
        If code = "" Then Throw New InvalidOperationException("Scan the original partial pallet.")
        Dim found = PaSPackingData.FindContainer(code)
        If found Is Nothing Then Throw New InvalidOperationException("Source pallet was not found in CCS.")
        If found.Status <> "PARTIAL" Then
            Throw New InvalidOperationException(
                "Source must have PARTIAL status. Actual status: " & found.Status)
        End If
        If found.PieceTotal <= 0 OrElse found.PieceTotal >= found.RequiredTotal Then
            Throw New InvalidOperationException("Original pallet does not have a valid partial quantity.")
        End If
        Return found
    End Function

    Private Sub PreviewSource()
        Try
            Dim pallet = GetSource()
            summary.ForeColor = Color.DarkGreen
            summary.Text = "Source: " & pallet.Code & " | PARTIAL" &
                Environment.NewLine & "Model ID: " & pallet.PartId &
                " | Pieces to transfer: " & pallet.PieceTotal &
                " / " & pallet.RequiredTotal
        Catch ex As Exception
            summary.ForeColor = Color.DarkRed
            summary.Text = ex.Message
        End Try
    End Sub

    Private Sub ExecuteTransfer(sender As Object, e As EventArgs)
        Try
            If ConnectionRecovery.Pending() IsNot Nothing Then
                Throw New InvalidOperationException(
                    "A SQL operation is pending reconciliation. Wait for its resolution.")
            End If
            If StationSettings.ReadLockedPallet() <> "" Then
                Throw New InvalidOperationException(
                    "Complete the current OPEN pallet before transferring another.")
            End If
            Dim source = GetSource()
            Dim destination = destinationInput.Text.Trim().ToUpperInvariant()
            Dim format = PalletIdSettings.Load()
            If Not format.IsValid(destination) Then
                Throw New InvalidOperationException("New container code does not match General Settings.")
            End If
            If String.Equals(source.Code,destination,StringComparison.OrdinalIgnoreCase) Then
                Throw New InvalidOperationException("Source and destination must be different.")
            End If
            If PaSPackingData.FindContainer(destination) IsNot Nothing Then
                Throw New InvalidOperationException("Destination code already exists in CCS.")
            End If
            If MessageBox.Show(
                "Transfer " & source.PieceTotal & " validated pieces from " & source.Code &
                " to NEW pallet " & destination & "?" & Environment.NewLine &
                "Source becomes TRANSFERRED and destination stays OPEN.",
                "Confirm pallet transfer", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) <> DialogResult.Yes Then Return

            Dim destinationId = PaSPackingData.TransferPartial(source.Code, destination)
            If destinationId <= 0 Then Throw New InvalidOperationException(
                "SQL did not return the new pallet ID.")
            _destinationCode = destination
            DialogResult = DialogResult.OK
            Close()
        Catch ex As Exception
            MessageBox.Show(ex.Message,"Pallet transfer blocked",
                            MessageBoxButtons.OK,MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
