Imports System.Drawing
Imports System.Windows.Forms

' Large touch-friendly confirmation. SQL counts have already been reloaded.
Public NotInheritable Class PackingConfirmationForm
    Inherits Form

    Public Sub New(code As String, level As Integer, levels As Integer,
                   count As Integer, isFinal As Boolean)
        Text = If(isFinal, "Finish pallet", "Confirm completed level")
        ClientSize = New Size(610, 410)
        FormBorderStyle = FormBorderStyle.FixedDialog
        StartPosition = FormStartPosition.CenterParent
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        BackColor = Color.FromArgb(244, 248, 252)
        Font = New Font("Segoe UI", 11)

        Dim accent = If(isFinal, Color.FromArgb(28, 113, 83), PaSTheme.Navy)
        Dim band As New Panel With {.Dock = DockStyle.Top, .Height = 104, .BackColor = accent}
        Dim symbol As New Label With {
            .Text = If(isFinal, "✓", "▣"),
            .Font = New Font("Segoe UI Symbol", 35, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(28, 12), .Size = New Size(76, 70),
            .TextAlign = ContentAlignment.MiddleCenter
        }
        Dim heading As New Label With {
            .Text = If(isFinal, "PALLET READY TO CLOSE", "LEVEL READY TO CLOSE"),
            .Font = New Font("Segoe UI", 19, FontStyle.Bold),
            .ForeColor = Color.White,
            .Location = New Point(108, 22), .Size = New Size(470, 54),
            .TextAlign = ContentAlignment.MiddleLeft
        }
        band.Controls.Add(symbol)
        band.Controls.Add(heading)

        Dim codeLabel As New Label With {
            .Text = "CONTAINER  " & code,
            .Location = New Point(30, 124), .Size = New Size(540, 31),
            .Font = New Font("Segoe UI", 13, FontStyle.Bold),
            .ForeColor = PaSTheme.Navy
        }
        Dim number As New Label With {
            .Text = count.ToString() & " / " & count.ToString(),
            .Location = New Point(28, 163), .Size = New Size(540, 66),
            .Font = New Font("Segoe UI", 36, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleCenter,
            .ForeColor = accent
        }
        Dim body As New Label With {
            .Text = If(isFinal,
                       "Last level " & level & " of " & levels & " is full." & Environment.NewLine &
                       "Are all pieces physically packed? Confirm to close the level AND pallet.",
                       "Level " & level & " of " & levels & " is full." & Environment.NewLine &
                       "Are all pieces physically packed? Confirm to open the next level."),
            .Location = New Point(35, 238), .Size = New Size(540, 66),
            .Font = New Font("Segoe UI", 12),
            .TextAlign = ContentAlignment.MiddleCenter,
            .ForeColor = PaSTheme.TextPrimary
        }
        Dim no As New Button With {
            .Text = "NOT YET", .Location = New Point(30, 328),
            .Size = New Size(220, 53), .FlatStyle = FlatStyle.Flat,
            .BackColor = Color.White, .ForeColor = PaSTheme.Navy,
            .DialogResult = DialogResult.Cancel,
            .Font = New Font("Segoe UI", 12, FontStyle.Bold)
        }
        Dim yes As New Button With {
            .Text = If(isFinal, "YES — COMPLETE PALLET", "YES — NEXT LEVEL"),
            .Location = New Point(268, 328), .Size = New Size(310, 53),
            .FlatStyle = FlatStyle.Flat, .BackColor = accent,
            .ForeColor = Color.White, .DialogResult = DialogResult.OK,
            .Font = New Font("Segoe UI", 12, FontStyle.Bold)
        }
        no.FlatAppearance.BorderColor = PaSTheme.Border
        yes.FlatAppearance.BorderSize = 0
        Controls.Add(band)
        Controls.Add(codeLabel)
        Controls.Add(number)
        Controls.Add(body)
        Controls.Add(no)
        Controls.Add(yes)
        AcceptButton = yes
        CancelButton = no
    End Sub
End Class
