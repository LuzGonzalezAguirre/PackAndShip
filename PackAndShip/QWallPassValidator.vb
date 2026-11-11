Imports System.Data
Imports Microsoft.Data.SqlClient

' Read-only Q-Wall verification: no product or inspection is created here.
' Packing authorization is rechecked transactionally by ssi_PaS_AddPiece.
Public NotInheritable Class QWallPassResult
    Public Property Verified As Boolean
    Public Property Message As String = ""
    Public Property ProductId As Integer
    Public Property InspectionId As Integer
End Class

Public NotInheritable Class QWallPassValidator
    Private Sub New()
    End Sub

    Public Shared Function Verify(partId As Integer, ssiFullCode As String,
                                  clientFullCode As String) As QWallPassResult
        If partId <= 0 OrElse String.IsNullOrWhiteSpace(ssiFullCode) OrElse
           String.IsNullOrWhiteSpace(clientFullCode) Then
            Return Refused("Se requiere el modelo y ambos números de serie completos.")
        End If

        Using cn = PackAndShipData.CreateConnection()
            ' Q-Wall GetOrCreateProductID stores the complete scan strings.
            ' Do not use only extracted digits: part+both strings identify the record.
            Dim matched As Integer
            Dim productId As Integer
            Using cmd As New SqlCommand(
                "SELECT product_id FROM dbo.ssi_Products " &
                "WHERE pn_id=@pn AND frameSN=@ssi AND fpcaSN=@client", cn)
                cmd.Parameters.Add("@pn", SqlDbType.Int).Value = partId
                cmd.Parameters.Add("@ssi", SqlDbType.NVarChar, 200).Value = ssiFullCode.Trim()
                cmd.Parameters.Add("@client", SqlDbType.NVarChar, 200).Value = clientFullCode.Trim()
                Using rd = cmd.ExecuteReader()
                    While rd.Read()
                        matched += 1
                        If matched > 1 Then Exit While
                        productId = Convert.ToInt32(rd("product_id"))
                    End While
                End Using
            End Using

            If matched = 0 Then
                Return Refused("La pieza NO SE ENCUENTRA EN Q-Wall para este modelo y estas dos series.")
            End If
            If matched > 1 Then
                Return Refused("Existen varios productos de Q-Wall para estas series. Se requiere revisión de Calidad.")
            End If

            Dim inspectionId As Integer
            Dim passed As Boolean
            Dim scrap As Boolean
            Dim found As Boolean
            Dim finished As Boolean
            Using cmd As New SqlCommand(
                "SELECT TOP (1) inspection_id, overall_result, is_scrap, finished_at " &
                "FROM dbo.ssi_Inspections WHERE product_id=@product " &
                "ORDER BY COALESCE(finished_at, started_at) DESC, inspection_id DESC", cn)
                cmd.Parameters.Add("@product", SqlDbType.Int).Value = productId
                Using rd = cmd.ExecuteReader()
                    If rd.Read() Then
                        found = True
                        inspectionId = Convert.ToInt32(rd("inspection_id"))
                        passed = Not rd.IsDBNull(1) AndAlso Convert.ToBoolean(rd("overall_result"))
                        scrap = Not rd.IsDBNull(2) AndAlso Convert.ToBoolean(rd("is_scrap"))
                        finished = Not rd.IsDBNull(3)
                    End If
                End Using
            End Using
            If Not found Then Return Refused("La pieza NO TIENE INSPECCIÓN registrada en Q-Wall.")
            If Not finished Then Return Refused("La última inspección de Q-Wall está SIN FINALIZAR.")
            If scrap Then Return Refused("La última inspección de Q-Wall marcó la pieza como SCRAP.")
            If Not passed Then Return Refused("La pieza tiene FAIL en su última inspección de Q-Wall.")

            ' Q-Wall authorizes flags independently of the inspection fields.
            ' A past flag does not block a newer completed PASS inspection.
            ' Every flag on the latest inspection must have an explicit
            ' authorization and must belong to this product.
            Dim currentFlagCount As Integer
            Using cmd As New SqlCommand(
                "SELECT COUNT(*) AS total_flags, " &
                "COALESCE(SUM(CASE WHEN product_id<>@product OR " &
                "authorized_by_user_id IS NULL OR authorized_at IS NULL " &
                "THEN 1 ELSE 0 END),0) AS unauthorized_flags " &
                "FROM dbo.ssi_PieceFlagRecords WHERE inspection_id=@id", cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = inspectionId
                cmd.Parameters.Add("@product", SqlDbType.Int).Value = productId
                Using rd = cmd.ExecuteReader()
                    If rd.Read() Then
                        currentFlagCount = Convert.ToInt32(rd("total_flags"))
                        If Convert.ToInt32(rd("unauthorized_flags")) <> 0 Then
                            Return Refused("La última inspección tiene una bandera de Calidad sin autorización válida.")
                        End If
                    End If
                End Using
            End Using

            Return New QWallPassResult With {
                .Verified = True,
                .ProductId = productId,
                .InspectionId = inspectionId,
                .Message = If(currentFlagCount > 0,
                    "Q-Wall PASS con bandera de Calidad autorizada. Se revisarán duplicados.",
                    "Q-Wall PASS confirmado. Se revisarán duplicados.")
            }
        End Using
    End Function

    Private Shared Function Refused(message As String) As QWallPassResult
        Return New QWallPassResult With {.Verified = False, .Message = message}
    End Function
End Class
