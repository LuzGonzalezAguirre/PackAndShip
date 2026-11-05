Imports System.Data
Imports System.Text.RegularExpressions
Imports Microsoft.Data.SqlClient

' Shared pallet identifier format in SQL Server. This only validates
' identifiers; actual pallet number allocation is a separate workflow.
Public NotInheritable Class PalletIdSettings
    Public Property Prefix As String = "P"
    Public Property DigitCount As Integer = 6

    Public Shared Function Load() As PalletIdSettings
        Using cn = PackAndShipData.CreateConnection()
            Using cmd As New SqlCommand(
                "SELECT pallet_prefix, pallet_digits FROM dbo.ssi_PaS_GeneralSettings WHERE settings_id=1", cn)
                Using rd = cmd.ExecuteReader()
                    If Not rd.Read() Then
                        Throw New InvalidOperationException(
                            "General settings are missing. Run sql/005_general_settings.sql on the database.")
                    End If
                    Dim setting As New PalletIdSettings With {
                        .Prefix = rd.GetString(0),
                        .DigitCount = rd.GetInt32(1)
                    }
                    ValidateFormat(setting.Prefix, setting.DigitCount)
                    Return setting
                End Using
            End Using
        End Using
    End Function

    Public Shared Sub Save(prefix As String, digits As Integer)
        Dim normalized = If(prefix, "").Trim().ToUpperInvariant()
        ValidateFormat(normalized, digits)
        If SessionContext.UserId <= 0 Then Throw New UnauthorizedAccessException("Sign in first.")

        Using cn = PackAndShipData.CreateConnection()
            Using tx = cn.BeginTransaction(IsolationLevel.Serializable)
                Try
                    Using permission As New SqlCommand(
                        "SELECT COUNT(*) FROM dbo.ssi_PaS_Users u " &
                        "JOIN dbo.ssi_PaS_Roles r ON r.role_id=u.role_id " &
                        "WHERE u.user_id=@user AND u.is_active=1 AND r.can_modify_setup=1", cn, tx)
                        permission.Parameters.Add("@user", SqlDbType.Int).Value = SessionContext.UserId
                        If Convert.ToInt32(permission.ExecuteScalar()) <> 1 Then
                            Throw New UnauthorizedAccessException("Your account cannot modify general settings.")
                        End If
                    End Using

                    Using cmd As New SqlCommand(
                        "UPDATE dbo.ssi_PaS_GeneralSettings " &
                        "SET pallet_prefix=@prefix, pallet_digits=@digits, " &
                        "updated_by_user_id=@user, updated_at=SYSUTCDATETIME() " &
                        "WHERE settings_id=1", cn, tx)
                        cmd.Parameters.Add("@prefix", SqlDbType.VarChar, 5).Value = normalized
                        cmd.Parameters.Add("@digits", SqlDbType.Int).Value = digits
                        cmd.Parameters.Add("@user", SqlDbType.Int).Value = SessionContext.UserId
                        If cmd.ExecuteNonQuery() <> 1 Then
                            Throw New InvalidOperationException(
                                "General settings are missing. Run sql/005_general_settings.sql.")
                        End If
                    End Using
                    tx.Commit()
                Catch
                    tx.Rollback()
                    Throw
                End Try
            End Using
        End Using
    End Sub

    Public Shared Sub ValidateFormat(prefix As String, digits As Integer)
        If Not Regex.IsMatch(If(prefix, ""), "^[A-Z]{1,5}$") Then
            Throw New ArgumentException("Prefix must contain 1 to 5 letters (A-Z).")
        End If
        If digits < 1 OrElse digits > 12 Then
            Throw New ArgumentOutOfRangeException(NameOf(digits), "Number of digits must be from 1 to 12.")
        End If
    End Sub

    Public Function Example() As String
        Return Prefix & New String("0"c, DigitCount - 1) & "1"
    End Function

    Public Function IsValid(value As String) As Boolean
        If value Is Nothing Then Return False
        Return Regex.IsMatch(value.Trim(), "^" & Regex.Escape(Prefix) &
                             "[0-9]{" & DigitCount & "}$", RegexOptions.CultureInvariant)
    End Function
End Class
