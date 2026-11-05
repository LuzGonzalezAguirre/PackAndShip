Imports System.Data
Imports Microsoft.Data.SqlClient

' Read-only Q-Wall verification: no product or inspection is created here.
' Production packing remains locked until unique-container persistence is built.
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
            Return Refused("Part number and both complete serial codes are required.")
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
                Return Refused("Q-Wall has no matching product with this part and both complete serial codes.")
            End If
            If matched > 1 Then
                Return Refused("More than one Q-Wall product matches these serials. Quality review required.")
            End If

            Dim inspectionId As Integer
            Dim passed As Boolean
            Dim scrap As Boolean
            Dim found As Boolean
            Using cmd As New SqlCommand(
                "SELECT TOP (1) inspection_id, overall_result, is_scrap " &
                "FROM dbo.ssi_Inspections WHERE product_id=@product " &
                "ORDER BY finished_at DESC, inspection_id DESC", cn)
                cmd.Parameters.Add("@product", SqlDbType.Int).Value = productId
                Using rd = cmd.ExecuteReader()
                    If rd.Read() Then
                        found = True
                        inspectionId = Convert.ToInt32(rd("inspection_id"))
                        passed = Not rd.IsDBNull(1) AndAlso Convert.ToBoolean(rd("overall_result"))
                        scrap = Not rd.IsDBNull(2) AndAlso Convert.ToBoolean(rd("is_scrap"))
                    End If
                End Using
            End Using
            If Not found Then Return Refused("This product has no Q-Wall inspection.")
            If scrap Then Return Refused("The latest Q-Wall inspection marks this piece as scrap.")
            If Not passed Then Return Refused("The latest Q-Wall inspection is not PASS.")

            ' An authorized flag can coexist with PASS in Q-Wall. Hold for Quality
            ' until the release/authorization policy has been defined.
            Using cmd As New SqlCommand(
                "SELECT COUNT(*) FROM dbo.ssi_PieceFlagRecords WHERE inspection_id=@id", cn)
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = inspectionId
                If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                    Return Refused("Latest inspection has a Quality flag. Release criteria must be reviewed.")
                End If
            End Using

            Return New QWallPassResult With {
                .Verified = True,
                .ProductId = productId,
                .InspectionId = inspectionId,
                .Message = "Q-Wall PASS confirmed (read-only). Global duplicate check is still required."
            }
        End Using
    End Function

    Private Shared Function Refused(message As String) As QWallPassResult
        Return New QWallPassResult With {.Verified = False, .Message = message}
    End Function
End Class
