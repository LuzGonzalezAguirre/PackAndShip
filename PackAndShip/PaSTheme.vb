Imports System.Drawing
Imports System.Windows.Forms

' Shared visual language for PackAndShip Windows Forms.
' Keep business logic out of this class.
Public NotInheritable Class PaSTheme
    Private Sub New()
    End Sub

    Public Shared ReadOnly Navy As Color = Color.FromArgb(31, 78, 121)
    Public Shared ReadOnly NavyHover As Color = Color.FromArgb(25, 65, 106)
    Public Shared ReadOnly Background As Color = Color.FromArgb(245, 248, 252)
    Public Shared ReadOnly Surface As Color = Color.White
    Public Shared ReadOnly TextPrimary As Color = Color.FromArgb(27, 39, 58)
    Public Shared ReadOnly TextSecondary As Color = Color.FromArgb(108, 128, 155)
    Public Shared ReadOnly Border As Color = Color.FromArgb(222, 230, 240)

    Public Shared Function Heading(value As String, Optional size As Single = 22.0F) As Label
        Return New Label With {
            .Text = value,
            .AutoSize = True,
            .ForeColor = TextPrimary,
            .Font = New Font("Segoe UI", size, FontStyle.Bold)
        }
    End Function

    Public Shared Function Subtitle(value As String) As Label
        Return New Label With {
            .Text = value,
            .AutoSize = True,
            .ForeColor = TextSecondary,
            .Font = New Font("Segoe UI", 10.0F)
        }
    End Function

    Public Shared Function ActionButton(caption As String, Optional primary As Boolean = True) As Button
        Dim button As New Button With {
            .Text = caption,
            .Size = New Size(150, 40),
            .FlatStyle = FlatStyle.Flat,
            .Cursor = Cursors.Hand,
            .Font = New Font("Segoe UI", 10, FontStyle.Bold),
            .BackColor = If(primary, Navy, Surface),
            .ForeColor = If(primary, Color.White, Navy)
        }
        button.FlatAppearance.BorderSize = If(primary, 0, 1)
        button.FlatAppearance.BorderColor = Border
        AddHandler button.MouseEnter, Sub()
                                          If button.Enabled Then button.BackColor = If(primary, NavyHover, Background)
                                      End Sub
        AddHandler button.MouseLeave, Sub()
                                          button.BackColor = If(primary, Navy, Surface)
                                      End Sub
        Return button
    End Function

    Public Shared Sub ConfigureForm(form As Form)
        form.BackColor = Background
        form.Font = New Font("Segoe UI", 10)
        form.ForeColor = TextPrimary
    End Sub
End Class
