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
            ' Q-Wall may have more than one ssi_Products row after
            ' reinspecting the same physical SSI + Client serials. Those
            ' rows are not extra pieces: inspect their shared history.
            Dim existsInQWall As Boolean
            Using cmd As New SqlCommand(
                "SELECT CASE WHEN EXISTS (" &
                "SELECT 1 FROM dbo.ssi_Products " &
                "WHERE pn_id=@pn AND frameSN=@ssi AND fpcaSN=@client) " &
                "THEN 1 ELSE 0 END", cn)
                cmd.Parameters.Add("@pn", SqlDbType.Int).Value = partId
                cmd.Parameters.Add("@ssi", SqlDbType.NVarChar, 200).Value = ssiFullCode.Trim()
                cmd.Parameters.Add("@client", SqlDbType.NVarChar, 200).Value = clientFullCode.Trim()
                existsInQWall = Convert.ToInt32(cmd.ExecuteScalar()) = 1
            End Using

            If Not existsInQWall Then
                Return Refused("La pieza NO SE ENCUENTRA EN Q-Wall para este modelo y estas dos series.")
            End If

            Dim productId As Integer
            Dim inspectionId As Integer
            Dim passed As Boolean
            Dim scrap As Boolean
            Dim found As Boolean
            Dim finished As Boolean
            ' All Q-Wall product rows with this SAME physical pair of QR codes
            ' form one inspection history. The most recently STARTED inspection
            ' wins, including FAIL or unfinished runs. Never pick an older PASS.
            Using cmd As New SqlCommand(
                "SELECT TOP (1) i.product_id, i.inspection_id, i.overall_result, " &
                "i.is_scrap, i.finished_at " &
                "FROM dbo.ssi_Products AS p " &
                "INNER JOIN dbo.ssi_Inspections AS i ON i.product_id=p.product_id " &
                "WHERE p.pn_id=@pn AND p.frameSN=@ssi AND p.fpcaSN=@client " &
                "ORDER BY COALESCE(i.started_at,i.finished_at) DESC, i.inspection_id DESC", cn)
                cmd.Parameters.Add("@pn", SqlDbType.Int).Value = partId
                cmd.Parameters.Add("@ssi", SqlDbType.NVarChar, 200).Value = ssiFullCode.Trim()
                cmd.Parameters.Add("@client", SqlDbType.NVarChar, 200).Value = clientFullCode.Trim()
                Using rd = cmd.ExecuteReader()
                    If rd.Read() Then
                        found = True
                        productId = Convert.ToInt32(rd("product_id"))
                        inspectionId = Convert.ToInt32(rd("inspection_id"))
                        passed = Not rd.IsDBNull(2) AndAlso Convert.ToBoolean(rd("overall_result"))
                        scrap = Not rd.IsDBNull(3) AndAlso Convert.ToBoolean(rd("is_scrap"))
                        finished = Not rd.IsDBNull(4)
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
