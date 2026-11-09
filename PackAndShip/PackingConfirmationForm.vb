Imports System.Drawing
Imports System.Windows.Forms

' Layout belongs to PackingConfirmationForm.Designer.vb.
Public NotInheritable Partial Class PackingConfirmationForm
    Inherits Form

    Public Sub New()
        InitializeComponent()
    End Sub

    Public Sub New(code As String, level As Integer, levels As Integer,
                   count As Integer, isFinal As Boolean)
        Me.New()
        Dim accent As System.Drawing.Color =
            If(isFinal, Color.FromArgb(28, 113, 83), PaSTheme.Navy)
        Me.Text = If(isFinal, "Finish pallet", "Confirm completed level")
        band.BackColor = accent
        number.ForeColor = accent
        heading.Text = If(isFinal, "PALLET READY TO CLOSE", "LEVEL READY TO CLOSE")
        symbol.Text = If(isFinal, "✓", "▣")
        codeLabel.Text = "CONTAINER  " & code
        number.Text = count & " / " & count
        message.Text = If(isFinal,
            "Last level " & level & " of " & levels & " is full." & Environment.NewLine &
            "Are all pieces physically packed? Confirm to close the level AND pallet.",
            "Level " & level & " of " & levels & " is full." & Environment.NewLine &
            "Are all pieces physically packed? Confirm to open the next level.")
        yesButton.Text = If(isFinal, "YES — COMPLETE PALLET", "YES — NEXT LEVEL")
        yesButton.BackColor = accent
    End Sub
End Class
