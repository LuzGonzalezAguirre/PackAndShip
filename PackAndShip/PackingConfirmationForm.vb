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
        Me.Text = If(isFinal, "Finalizar pallet", "Confirmar nivel completo")
        band.BackColor = accent
        number.ForeColor = accent
        heading.Text = If(isFinal, "PALLET LISTO PARA CERRAR", "NIVEL LISTO PARA CERRAR")
        symbol.Text = If(isFinal, "✓", "▣")
        codeLabel.Text = "CONTENEDOR  " & code
        number.Text = count & " / " & count
        message.Text = If(isFinal,
            "Último nivel " & level & " of " & levels & " está completo." & Environment.NewLine &
            "¿Todas las piezas están empacadas físicamente? Confirma para cerrar el nivel Y el pallet.",
            "Nivel " & level & " of " & levels & " está completo." & Environment.NewLine &
            "¿Todas las piezas están empacadas físicamente? Confirma para abrir el siguiente nivel.")
        yesButton.Text = If(isFinal, "SÍ — COMPLETAR PALLET", "SÍ — SIGUIENTE NIVEL")
        yesButton.BackColor = accent
    End Sub
End Class
