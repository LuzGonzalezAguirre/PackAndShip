Public NotInheritable Class SessionContext
    Private Sub New()
    End Sub

    Public Shared Property UserId As Integer
    Public Shared Property FullName As String = ""
    Public Shared Property BarcodeId As String = ""
    Public Shared Property RoleId As Integer
    Public Shared Property RoleName As String = ""
    Public Shared Property CanModifySetup As Boolean
    Public Shared Property BusinessUnitId As Integer
    Public Shared Property BusinessUnitName As String = ""

    Public Shared Sub Clear()
        UserId = 0
        FullName = ""
        BarcodeId = ""
        RoleId = 0
        RoleName = ""
        CanModifySetup = False
        BusinessUnitId = 0
        BusinessUnitName = ""
    End Sub
End Class
