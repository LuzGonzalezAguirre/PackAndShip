Imports System.Data
Imports Microsoft.Data.SqlClient

Public NotInheritable Class BusinessUnit
    Public Property Id As Integer
    Public Property Name As String = ""
    Public Overrides Function ToString() As String
        Return Name
    End Function
End Class

Public NotInheritable Class SignedInUser
    Public Property Id As Integer
    Public Property Name As String = ""
    Public Property Barcode As String = ""
    Public Property RoleId As Integer
    Public Property RoleName As String = ""
    Public Property CanModifySetup As Boolean
End Class

Public NotInheritable Class PackAndShipData
    Private Sub New()
    End Sub

    Friend Shared Function CreateConnection() As SqlConnection
        ' Cadena definida directamente en código, como en Q-Wall.
        ' Reemplazar SERVIDOR_SQL y BASE_DE_DATOS en el entorno local.
        ' No publicar usuario/contraseña reales en el repositorio.
        Const connectionString As String =
            "Server=SERVIDOR_SQL;Database=BASE_DE_DATOS;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;"
        Dim connection = New SqlConnection(connectionString)
        connection.Open()
        Return connection
    End Function

    Public Shared Function GetBusinessUnits() As List(Of BusinessUnit)
        Dim result As New List(Of BusinessUnit)()
        Using connection = CreateConnection()
            Using command As New SqlCommand(
                "SELECT bu_id, bu_name FROM dbo.ssi_BusinessUnits ORDER BY bu_name", connection)
                Using reader = command.ExecuteReader()
                    While reader.Read()
                        result.Add(New BusinessUnit With {
                            .Id = reader.GetInt32(0),
                            .Name = reader.GetString(1)
                        })
                    End While
                End Using
            End Using
        End Using
        Return result
    End Function

    Public Shared Function Authenticate(barcode As String, password As String) As SignedInUser
        If String.IsNullOrWhiteSpace(barcode) OrElse String.IsNullOrWhiteSpace(password) Then Return Nothing
        If barcode.Length > 50 OrElse password.Length > 50 Then Return Nothing

        Using connection = CreateConnection()
            ' Compatible con el esquema SHA2_256 de Q-Wall (temporal).
            ' Migrar a hash con salt y KDF antes del despliegue productivo.
            Const sql As String =
                "SELECT u.user_id, u.name, u.barcode_id, r.role_id, r.role_name, r.can_modify_setup " &
                "FROM dbo.ssi_PaS_Users AS u " &
                "INNER JOIN dbo.ssi_PaS_Roles AS r ON r.role_id = u.role_id " &
                "WHERE u.barcode_id = @barcode " &
                "AND u.password_hash = HASHBYTES('SHA2_256', CAST(@password AS VARCHAR(50))) " &
                "AND u.is_active = 1"
            Using command As New SqlCommand(sql, connection)
                command.Parameters.Add("@barcode", SqlDbType.VarChar, 50).Value = barcode
                command.Parameters.Add("@password", SqlDbType.VarChar, 50).Value = password
                Using reader = command.ExecuteReader()
                    If Not reader.Read() Then Return Nothing
                    Return New SignedInUser With {
                        .Id = reader.GetInt32(0),
                        .Name = reader.GetString(1),
                        .Barcode = reader.GetString(2),
                        .RoleId = reader.GetInt32(3),
                        .RoleName = reader.GetString(4),
                        .CanModifySetup = reader.GetBoolean(5)
                    }
                End Using
            End Using
        End Using
    End Function
End Class
