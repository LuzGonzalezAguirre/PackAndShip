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

    Public Sub New()
        InitializeComponent()
        BindNavigation()
    End Sub

    Public Sub New(customerAction As Action, usersAction As Action,
                   palletAction As Action, scansAction As Action,
                   generalAction As Action)
        Me.New()
        openCustomer = customerAction
        openUsers = usersAction
        openPallet = palletAction
        openScans = scansAction
        openGeneral = generalAction
    End Sub

    Private Sub BindNavigation()
        AddHandler customerButton.Click, Sub()
            If openCustomer IsNot Nothing Then openCustomer.Invoke()
        End Sub
        AddHandler usersButton.Click, Sub()
            If openUsers IsNot Nothing Then openUsers.Invoke()
        End Sub
        AddHandler palletButton.Click, Sub()
            If openPallet IsNot Nothing Then openPallet.Invoke()
        End Sub
        AddHandler scansButton.Click, Sub()
            If openScans IsNot Nothing Then openScans.Invoke()
        End Sub
        AddHandler generalButton.Click, Sub()
            If openGeneral IsNot Nothing Then openGeneral.Invoke()
        End Sub
    End Sub
End Class
