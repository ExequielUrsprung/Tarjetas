Public Class AutorizacionesHandler
    Public WithEvents Objaut As TarjetasControl.Autorizaciones
    Public RespuestaError As Integer

    Public Sub New(ByRef pObjAut As TarjetasControl.Autorizaciones)
        Objaut = pObjAut
    End Sub


    Private Sub AsignarResultadoIso(ByVal xEstado As TarjetasControl.TEstadosTransacciones, ByRef ResultadoIso As Integer, ByRef estadoaut As TarjetasControl.Autorizaciones.TEstadosAutorizaciones, ByRef xerrores As TarjetasControl.tErrores) Handles Objaut.AsignarResultadoIso
        'para las autorizaciones previamente rechazadas
        Select Case estadoaut
            Case TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                ResultadoIso = RespuestaError
                If ResultadoIso = 0 Then
                    log.logerror("Status 0 para rechazada!")
                    ResultadoIso = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
                End If
            Case TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Anulada, TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Autorizada
                ResultadoIso = ObtenerCodRespuesta(xEstado, ONLINECOM.E_Implementaciones.Link)
                Select Case xEstado
                    Case TarjetasControl.TEstadosTransacciones.Normal
                        estadoaut = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Autorizada
                    Case Else
                        estadoaut = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                End Select

            Case TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Consulta
                ResultadoIso = ObtenerCodRespuesta(xEstado, ONLINECOM.E_Implementaciones.Link)
            Case TarjetasControl.Autorizaciones.TEstadosAutorizaciones.SAF
                log.logInfo("Transaccion SAF se fuerza deberia tener respuesta " + ObtenerCodRespuesta(xEstado, ONLINECOM.E_Implementaciones.Link).ToString + " para Estado " + xEstado.ToString)
                ResultadoIso = ONLINECOM.E_CodigosRespuesta.OK
                estadoaut = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Autorizada

            Case Else
                log.logerror(" estado incorrecto en asignar resultado")
        End Select

        log.logDebug("asignacion Iso:" + xerrores.ListaErrores)
    End Sub

End Class
