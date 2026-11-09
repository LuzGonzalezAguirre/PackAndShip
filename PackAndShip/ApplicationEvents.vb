Imports Microsoft.VisualBasic.ApplicationServices
Imports System.Windows.Forms
Imports System.Linq

Namespace My
    Partial Friend Class MyApplication

        ' Runs before Form1 is created. No Scanner window exists behind LoginForm.
        Protected Overrides Function OnStartup(eventArgs As StartupEventArgs) As Boolean
            If Not MyBase.OnStartup(eventArgs) Then Return False

            Dim customerName As String = ""
            Try
                Dim savedCustomerId = StationSettings.ReadCustomerId()
                If savedCustomerId.HasValue Then
                    Dim selected = PackAndShipData.GetBusinessUnits().
                        FirstOrDefault(Function(item) item.Id = savedCustomerId.Value)
                    If selected IsNot Nothing Then customerName = selected.Name
                End If
            Catch ex As Exception
                ' Authentication can still be attempted. Form1 will surface the
                ' database connection error when station information is loaded.
            End Try

            Using login As New LoginForm(customerName)
                Return login.ShowDialog() = DialogResult.OK
            End Using
        End Function

    End Class
End Namespace
