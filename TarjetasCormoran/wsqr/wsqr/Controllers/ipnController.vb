Imports System.Net
Imports System.Web.Http

Namespace Controllers



    Public Class ipnController
        Inherits ApiController


        ' GET: api/ipn
        Public Function GetValues() As IEnumerable(Of String)
            Return New String() {"mauro"}
        End Function

        ' GET: api/ipn/5
        Public Function GetValue(ByVal id As Integer) As String
            Return "value" & id
        End Function

        ' POST: api/ipn
        'Public Function PostValue(<FromBody()> ByVal value As String) As IHttpActionResult
        Public Function PostValue(ByVal topic As String, ByVal id As String) As IHttpActionResult
            If topic = "merchant_order" Then
                Dim respIPN As System.IO.StreamWriter = New System.IO.StreamWriter("C:\SistemaQR\RespuestasQR\" & id & ".IPN", False)
                respIPN.Close()
            End If
            Return Ok()
        End Function

        ' PUT: api/ipn/5
        Public Sub PutValue(ByVal id As Integer, <FromBody()> ByVal value As String)

        End Sub

        ' DELETE: api/ipn/5
        Public Sub DeleteValue(ByVal id As Integer)

        End Sub
    End Class
End Namespace