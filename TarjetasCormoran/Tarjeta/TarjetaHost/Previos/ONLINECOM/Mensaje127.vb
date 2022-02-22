Public Class Mensaje127
    Inherits mensajeGenerico
    Public TipoCambioAplicado As Integer
    Public Region As String
    Public TipoCambioComprador As Decimal
    Public TipoCambioVendedor As Decimal
    Public TipocambioApesos As Decimal
    Public TipoCambioAdolares As Decimal


    Public Sub New(ByVal pImplementacion As E_Implementaciones, Optional ByVal pValorInicial As String = "")
        MyBase.New(pImplementacion, pValorInicial)
    End Sub

    
    Public Overrides Sub Fromstring(ByVal s As String)
        Dim resultado As String = "OK"
        Try
            Region = s.Substring(0, 4)
            TipoCambioComprador = CDec(s.Substring(4, 8)) / 1000
            TipoCambioVendedor = CDec(s.Substring(12, 8)) / 1000
            TipocambioApesos = CDec(s.Substring(20, 8)) / 1000
            TipoCambioAdolares = CDec(s.Substring(28, 8)) / 1000
        Catch ex As Exception
            resultado = "Error Parseando campo 127 :" + s + "   > " + ex.Message
        End Try
        LogMessageFromString(Logstate(s))
    End Sub
    Public Overrides Function Tostring() As String
        Dim s As String = _
        TipoCambioAplicado.ToString("0") + _
         (TipoCambioComprador * 1000).ToString("00000000") + _
         (TipoCambioVendedor * 1000).ToString("00000000") + _
         (TipocambioApesos * 1000).ToString("00000000") + _
         (TipoCambioAdolares * 1000).ToString("00000000") + _
            Space(10)
        If TipoCambioComprador <> 0 Then LogMessageToString(Logstate(s))
        Return s
    End Function

    Public Function ToStringDescriptivo() As String
        Return ""
    End Function

    Public Overrides Function Logstate(ByVal s As String) As String
        Return _
         " TipoCambio Vendedor :" + TipoCambioVendedor.ToString + vbCrLf + _
         " TipoCambio Comprador:" + TipoCambioComprador.ToString + vbCrLf + _
         " TipoCambio A Pesos  :" + TipocambioApesos.ToString + vbCrLf + _
         " TipoCambio A Dolares:" + TipoCambioAdolares.ToString + vbCrLf + _
         "127          :" + s

    End Function



End Class
