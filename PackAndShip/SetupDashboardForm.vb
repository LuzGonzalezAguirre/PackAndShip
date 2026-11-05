Imports System.Drawing
Imports System.Windows.Forms

' Visual navigation only: delegates call the existing, tested Setup forms in Form1.
Public Class SetupDashboardForm
    Inherits Form

    Private ReadOnly openCustomer As Action
    Private ReadOnly openUsers As Action
    Private ReadOnly openPallet As Action
    Private ReadOnly openScans As Action
    Private ReadOnly openGeneral As Action

    Public Sub New(customerAction As Action, usersAction As Action,
                   palletAction As Action, scansAction As Action,
                   generalAction As Action)
        openCustomer = customerAction
        openUsers = usersAction
        openPallet = palletAction
        openScans = scansAction
        openGeneral = generalAction

        Text = "Pack and Ship | Setup"
        StartPosition = FormStartPosition.CenterParent
        Size = New Size(1000, 725)
        MinimumSize = New Size(800, 570)
        PaSTheme.ConfigureForm(Me)
        BuildLayout()
    End Sub

    Private Sub BuildLayout()
        Dim banner As New Panel With {
            .Dock = DockStyle.Top, .Height = 83,
            .BackColor = PaSTheme.Navy, .Padding = New Padding(26)
        }
        Dim title As New Label With {
            .Text = "SETUP", .AutoSize = True, .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 23, FontStyle.Bold),
            .Location = New Point(25, 13)
        }
        Dim subtitle As New Label With {
            .Text = "Configuration center", .AutoSize = True, .ForeColor = Color.FromArgb(214, 231, 246),
            .Location = New Point(28, 55)
        }
        banner.Controls.Add(title)
        banner.Controls.Add(subtitle)
        Controls.Add(banner)

        Dim main As New Panel With {
            .Dock = DockStyle.Fill, .AutoScroll = True,
            .Padding = New Padding(28, 25, 28, 25)
        }
        Dim heading = PaSTheme.Heading("Manage your station", 21)
        heading.Location = New Point(10, 6)
        main.Controls.Add(heading)
        Dim detail = PaSTheme.Subtitle("Select a section to review or update its settings.")
        detail.Location = New Point(12, 53)
        main.Controls.Add(detail)

        Dim cards As New TableLayoutPanel With {
            .Location = New Point(10, 98),
            .ColumnCount = 2, .RowCount = 3,
            .Width = 880, .Height = 452,
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right,
            .BackColor = Color.Transparent
        }
        cards.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        cards.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
        For i As Integer = 0 To 2
            cards.RowStyles.Add(New RowStyle(SizeType.Percent, 33.333F))
        Next

        AddCard(cards, 0, 0, "Customer Configuration",
                "Default customer for this workstation", "Open customer", openCustomer)
        AddCard(cards, 1, 0, "Users & Roles",
                "Accounts, access levels and permissions", "Manage users", openUsers)
        AddCard(cards, 0, 1, "Pallet Configuration",
                "Part numbers, pallet levels and piece quantities", "Configure pallets", openPallet)
        AddCard(cards, 1, 1, "Scan Rules",
                "Scans, serial mapping and validation fields", "Manage rules", openScans)
        AddCard(cards, 0, 2, "General Settings",
                "Shared pallet ID format in SQL Server", "Open general", openGeneral)

        main.Controls.Add(cards)
        AddHandler main.Resize, Sub()
                                    cards.Width = Math.Max(700, main.ClientSize.Width - 20)
                                End Sub
        Controls.Add(main)
        main.BringToFront()
        banner.BringToFront()
    End Sub

    Private Sub AddCard(table As TableLayoutPanel, column As Integer, row As Integer,
                        title As String, description As String,
                        buttonLabel As String, navigate As Action)
        Dim wrapper As New Panel With {
            .Dock = DockStyle.Fill, .Margin = New Padding(0, 0, 16, 15),
            .BackColor = PaSTheme.Surface,
            .BorderStyle = BorderStyle.FixedSingle
        }
        Dim heading = PaSTheme.Heading(title, 13)
        heading.Location = New Point(20, 16)
        wrapper.Controls.Add(heading)
        Dim subtitle = PaSTheme.Subtitle(description)
        subtitle.Location = New Point(22, 47)
        subtitle.MaximumSize = New Size(390, 42)
        wrapper.Controls.Add(subtitle)
        Dim actionButton = PaSTheme.ActionButton(buttonLabel, False)
        actionButton.SetBounds(20, 88, 172, 34)
        AddHandler actionButton.Click, Sub()
                                           If navigate IsNot Nothing Then navigate.Invoke()
                                       End Sub
        wrapper.Controls.Add(actionButton)
        table.Controls.Add(wrapper, column, row)
    End Sub
End Class
