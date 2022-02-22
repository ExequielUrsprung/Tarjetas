Public Class Mensaje54Pas
    Inherits mensajeGenerico
    Public CDE_ENTE As Integer ' Código de Ente	9(3)
    Public CDE_CUSTOM As String ' Código de cliente	X(19)
    Public NRO_OPC As Integer ' 	Opción que corresponde a la selección de la deuda.	9(5)

    Public Sub New(ByVal pImplementacion As E_Implementaciones, Optional ByVal pValorInicial As String = "")
        MyBase.New(pImplementacion, pValorInicial)
    End Sub

    Public Sub New(ByVal pImplementacion As E_Implementaciones, ByVal Ente As Integer, ByVal Custom As String, ByVal OPc As Integer)
        MyBase.New(pImplementacion)
        CDE_ENTE = Ente
        CDE_CUSTOM = Custom
        NRO_OPC = OPc
    End Sub
    Public Overrides Sub Fromstring(ByVal s As String)
        Dim resultado As String = "OK"
        Try
            CDE_CUSTOM = (s.Substring(3, 19))
            CDE_ENTE = CInt(s.Substring(0, 3).Replace(" ", "0"))
            NRO_OPC = CInt(s.Substring(22, 1).Replace(" ", "0"))
            LogMessageFromString(Logstate(s))
        Catch ex As Exception
            resultado = "Error Parseando campo 54 PAS:" + s + "   > " + ex.Message
            Logger.Error(resultado)
        End Try
    End Sub
    Public Overrides Function Tostring() As String
        Dim s As String = _
         CDE_ENTE.ToString("000") + _
         CDE_CUSTOM.PadRight(19) + _
         NRO_OPC.ToString("00000") + _
            Space(73)
        LogMessageToString(Logstate(s))
        Return s
    End Function

    Public Function ToStringDescriptivo() As String
        Return CDE_ENTE.ToString("000") + " " + CDE_CUSTOM + " " + NRO_OPC.ToString("00000")
    End Function

    Public Overrides Function Logstate(ByVal s As String) As String
        Try
            Return _
          " CDE Ente   :" + CDE_ENTE.ToString + vbCrLf _
        + " CDE Custom :" + CDE_CUSTOM.ToString + vbCrLf _
        + " Cta/Anio   :" + NRO_OPC.ToString + vbCrLf _
        + " 54         :" + s

        Catch ex As Exception
            Logger.Error("Logstate " + ex.Message + " " + ex.StackTrace)
            Return s
        End Try

    End Function

End Class
