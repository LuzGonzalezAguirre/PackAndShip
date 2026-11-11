Imports System.Data
Imports System.Linq
Imports System.Windows.Forms

' Configuración editable de autorizaciones por departamento.
Public Partial Class DepartmentAuthorizationForm
    Inherits Form

    Private ReadOnly roles As New List(Of PaSRole)()

    Public Sub New()
        InitializeComponent()
        AddHandler Shown, Sub() LoadSettings()
        AddHandler saveManufacturing.Click, Sub() SaveDepartment(1, manufacturingEnabled, manufacturingRoles)
        AddHandler saveQuality.Click, Sub() SaveDepartment(2, qualityEnabled, qualityRoles)
        AddHandler saveProduction.Click, Sub() SaveDepartment(3, productionEnabled, productionRoles)
        AddHandler reloadButton.Click, Sub() LoadSettings()
    End Sub

    Private Sub LoadSettings()
        Try
            roles.Clear()
            roles.AddRange(UserAdministration.ListRoles())
            Dim settings = RejectionAuthorizationData.DepartmentSettings()
            Populate(1, manufacturingEnabled, manufacturingRoles, settings)
            Populate(2, qualityEnabled, qualityRoles, settings)
            Populate(3, productionEnabled, productionRoles, settings)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "No se cargaron las autorizaciones",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Populate(departmentId As Integer, enabledBox As CheckBox,
                         picker As CheckedListBox, settings As DataTable)
        Dim rows = settings.Select("DepartmentId = " & departmentId)
        picker.Items.Clear()
        enabledBox.Checked = rows.Length > 0 AndAlso Convert.ToBoolean(rows(0)("Habilitado"))
        Dim selected As New HashSet(Of Integer)(
            rows.Where(Function(r) Not r.IsNull("RoleId")).
                 Select(Function(r) Convert.ToInt32(r("RoleId"))))
        For Each role In roles
            picker.Items.Add(role, selected.Contains(role.Id))
        Next
        picker.DisplayMember = "Name"
    End Sub

    Private Sub SaveDepartment(departmentId As Integer, enabledBox As CheckBox,
                               picker As CheckedListBox)
        Try
            Dim ids As New List(Of Integer)()
            For Each item In picker.CheckedItems
                Dim role = TryCast(item, PaSRole)
                If role IsNot Nothing Then ids.Add(role.Id)
            Next
            RejectionAuthorizationData.SaveDepartment(departmentId, enabledBox.Checked, ids)
            MessageBox.Show("Configuración guardada. Los rechazos existentes conservan " &
                            "sus departamentos originalmente requeridos.",
                            "Autorizaciones", MessageBoxButtons.OK, MessageBoxIcon.Information)
            LoadSettings()
        Catch ex As Exception
            MessageBox.Show(ex.Message, "No se pudo guardar",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End Try
    End Sub
End Class
