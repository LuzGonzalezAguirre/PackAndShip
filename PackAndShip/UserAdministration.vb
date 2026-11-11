Imports System.Data
Imports Microsoft.Data.SqlClient

Public NotInheritable Class PaSRole
    Public Property Id As Integer
    Public Property Name As String = ""
    Public Property CanModifySetup As Boolean
End Class

Public NotInheritable Class PaSUser
    Public Property Id As Integer
    Public Property FullName As String = ""
    Public Property Barcode As String = ""
    Public Property RoleId As Integer
    Public Property RoleName As String = ""
    Public Property IsActive As Boolean
    Public Property CreatedAt As DateTime
End Class

' Data access for the Users & Roles setup section.
' All mutations re-check the active actor's role in SQL Server.
Public NotInheritable Class UserAdministration
    Private Sub New()
    End Sub

    Public Shared Function ListUsers() As List(Of PaSUser)
        Dim result As New List(Of PaSUser)()
        Using cn = PackAndShipData.CreateConnection()
            Const sql As String =
                "SELECT u.user_id, u.name, u.barcode_id, u.role_id, r.role_name, u.is_active, u.created_at " &
                "FROM dbo.ssi_PaS_Users u JOIN dbo.ssi_PaS_Roles r ON r.role_id=u.role_id " &
                "ORDER BY u.name"
            Using cmd As New SqlCommand(sql, cn)
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        result.Add(New PaSUser With {
                            .Id = rd.GetInt32(0),
                            .FullName = rd.GetString(1),
                            .Barcode = rd.GetString(2),
                            .RoleId = rd.GetInt32(3),
                            .RoleName = rd.GetString(4),
                            .IsActive = rd.GetBoolean(5),
                            .CreatedAt = rd.GetDateTime(6)
                        })
                    End While
                End Using
            End Using
        End Using
        Return result
    End Function

    Public Shared Function ListRoles() As List(Of PaSRole)
        Dim result As New List(Of PaSRole)()
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(
                "SELECT role_id, role_name, can_modify_setup FROM dbo.ssi_PaS_Roles ORDER BY role_id", cn)
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        result.Add(New PaSRole With {
                            .Id = rd.GetInt32(0),
                            .Name = rd.GetString(1),
                            .CanModifySetup = rd.GetBoolean(2)
                        })
                    End While
                End Using
            End Using
        End Using
        Return result
    End Function

    ' Read from database instead of relying only on the cached UI permission.
    Private Shared Sub RequireAuthorizedActor(cn As SqlConnection)
        If SessionContext.UserId <= 0 Then Throw New UnauthorizedAccessException("No active session.")
        Const sql As String =
            "SELECT COUNT(1) FROM dbo.ssi_PaS_Users u " &
            "JOIN dbo.ssi_PaS_Roles r ON r.role_id=u.role_id " &
            "WHERE u.user_id=@id AND u.is_active=1 AND r.can_modify_setup=1"
        Using cmd As New SqlCommand(sql, cn)
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = SessionContext.UserId
            If Convert.ToInt32(cmd.ExecuteScalar()) <> 1 Then
                Throw New UnauthorizedAccessException("Current user is not authorized to modify Setup.")
            End If
        End Using
    End Sub

    Private Shared Function NormalizeBarcode(value As String) As String
        Dim barcode = If(value, "").Trim().ToUpperInvariant()
        If barcode.Length = 0 OrElse barcode.Length > 50 Then
            Throw New ArgumentException("Barcode ID must contain 1 to 50 characters.")
        End If
        Return barcode
    End Function

    Private Shared Function NormalizeName(value As String) As String
        Dim fullName = If(value, "").Trim()
        If fullName.Length = 0 OrElse fullName.Length > 100 Then
            Throw New ArgumentException("Name must contain 1 to 100 characters.")
        End If
        Return fullName
    End Function

    Private Shared Function NormalizeRoleName(value As String) As String
        Dim roleName = If(value, "").Trim()
        If roleName.Length = 0 OrElse roleName.Length > 50 Then
            Throw New ArgumentException("El nombre del rol debe contener entre 1 y 50 caracteres.")
        End If
        Return roleName
    End Function

    ' El permiso se valida en CCS: no basta con habilitar botones en la interfaz.
    Public Shared Sub CreateRole(name As String, canModifySetup As Boolean)
        Dim normalized = NormalizeRoleName(name)
        Using cn = PackAndShipData.CreateConnection()
            RequireAuthorizedActor(cn)
            Using cmd As New SqlCommand(
                "INSERT INTO dbo.ssi_PaS_Roles(role_name,can_modify_setup) " &
                "SELECT @name,@setup WHERE NOT EXISTS " &
                "(SELECT 1 FROM dbo.ssi_PaS_Roles WITH (UPDLOCK,HOLDLOCK) " &
                "WHERE UPPER(LTRIM(RTRIM(role_name)))=UPPER(@name))", cn)
                cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = normalized
                cmd.Parameters.Add("@setup", SqlDbType.Bit).Value = canModifySetup
                If cmd.ExecuteNonQuery() <> 1 Then
                    Throw New InvalidOperationException("Ya existe un rol con ese nombre.")
                End If
            End Using
        End Using
    End Sub

    Public Shared Sub UpdateRole(roleId As Integer, name As String, canModifySetup As Boolean)
        If roleId <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(roleId))
        Dim normalized = NormalizeRoleName(name)
        Using cn = PackAndShipData.CreateConnection()
            RequireAuthorizedActor(cn)
            ' Mantener el permiso actual del usuario autenticado durante su sesión:
            ' no permitir revocar el propio acceso ni dejar la estación sin administrador.
            Using check As New SqlCommand(
                "SELECT COUNT(1) FROM dbo.ssi_PaS_Users " &
                "WHERE user_id=@actor AND role_id=@role AND is_active=1", cn)
                check.Parameters.Add("@actor", SqlDbType.Int).Value = SessionContext.UserId
                check.Parameters.Add("@role", SqlDbType.Int).Value = roleId
                If Convert.ToInt32(check.ExecuteScalar()) > 0 Then
                    Throw New InvalidOperationException(
                        "No puedes editar el rol de tu propia sesión. Usa otro usuario autorizado.")
                End If
            End Using
            Using cmd As New SqlCommand(
                "UPDATE dbo.ssi_PaS_Roles SET role_name=@name,can_modify_setup=@setup " &
                "WHERE role_id=@id AND NOT EXISTS " &
                "(SELECT 1 FROM dbo.ssi_PaS_Roles WITH (UPDLOCK,HOLDLOCK) " &
                "WHERE role_id<>@id AND UPPER(LTRIM(RTRIM(role_name)))=UPPER(@name))", cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = roleId
                cmd.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = normalized
                cmd.Parameters.Add("@setup", SqlDbType.Bit).Value = canModifySetup
                If cmd.ExecuteNonQuery() <> 1 Then
                    Throw New InvalidOperationException(
                        "No se actualizó el rol: puede no existir o el nombre ya está en uso.")
                End If
            End Using
        End Using
    End Sub

    ' Initial password equals Barcode ID, as requested for this station.
    Public Shared Sub CreateUser(fullName As String, barcodeId As String, roleId As Integer, active As Boolean)
        Dim name = NormalizeName(fullName)
        Dim barcode = NormalizeBarcode(barcodeId)
        Using cn = PackAndShipData.CreateConnection()
            RequireAuthorizedActor(cn)
            Const sql As String =
                "INSERT INTO dbo.ssi_PaS_Users(name, barcode_id, password_hash, role_id, is_active) " &
                "VALUES (@name, @barcode, HASHBYTES('SHA2_256', CONVERT(VARCHAR(50), @password)), @role, @active)"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = name
                cmd.Parameters.Add("@barcode", SqlDbType.VarChar, 50).Value = barcode
                cmd.Parameters.Add("@password", SqlDbType.VarChar, 50).Value = barcode
                cmd.Parameters.Add("@role", SqlDbType.Int).Value = roleId
                cmd.Parameters.Add("@active", SqlDbType.Bit).Value = active
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Public Shared Sub UpdateUser(userId As Integer, fullName As String, barcodeId As String,
                                 roleId As Integer, active As Boolean)
        Dim name = NormalizeName(fullName)
        Dim barcode = NormalizeBarcode(barcodeId)
        If userId = SessionContext.UserId Then
            Throw New InvalidOperationException("You cannot edit your own account from this session.")
        End If
        Using cn = PackAndShipData.CreateConnection()
            RequireAuthorizedActor(cn)
            Const sql As String =
                "UPDATE dbo.ssi_PaS_Users SET name=@name, barcode_id=@barcode, role_id=@role, is_active=@active " &
                "WHERE user_id=@id"
            Using cmd As New SqlCommand(sql, cn)
                cmd.Parameters.Add("@name", SqlDbType.VarChar, 100).Value = name
                cmd.Parameters.Add("@barcode", SqlDbType.VarChar, 50).Value = barcode
                cmd.Parameters.Add("@role", SqlDbType.Int).Value = roleId
                cmd.Parameters.Add("@active", SqlDbType.Bit).Value = active
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = userId
                If cmd.ExecuteNonQuery() <> 1 Then Throw New InvalidOperationException("User no longer exists.")
            End Using
        End Using
    End Sub

    Public Shared Sub SetUserActive(userId As Integer, active As Boolean)
        If userId = SessionContext.UserId Then
            Throw New InvalidOperationException("You cannot deactivate your own account.")
        End If
        Using cn = PackAndShipData.CreateConnection()
            RequireAuthorizedActor(cn)
            Using cmd As New SqlCommand(
                "UPDATE dbo.ssi_PaS_Users SET is_active=@active WHERE user_id=@id", cn)
                cmd.Parameters.Add("@active", SqlDbType.Bit).Value = active
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = userId
                If cmd.ExecuteNonQuery() <> 1 Then Throw New InvalidOperationException("User no longer exists.")
            End Using
        End Using
    End Sub

    Public Shared Sub ResetPasswordToBarcode(userId As Integer)
        Using cn = PackAndShipData.CreateConnection()
            RequireAuthorizedActor(cn)
            Using cmd As New SqlCommand(
                "UPDATE dbo.ssi_PaS_Users " &
                "SET password_hash=HASHBYTES('SHA2_256', CONVERT(VARCHAR(50), barcode_id)) " &
                "WHERE user_id=@id", cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = userId
                If cmd.ExecuteNonQuery() <> 1 Then Throw New InvalidOperationException("User no longer exists.")
            End Using
        End Using
    End Sub
End Class
