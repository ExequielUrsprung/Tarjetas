Imports System.Net.Sockets
Imports TjComun.IdaLib
Imports Trx.Messaging
Imports Trx.Messaging.Iso8583
Imports Trx.Messaging.Channels
Imports Trx.Messaging.FlowControl
Imports IsoLib
Imports System.Timers
Imports TjServer

Public Class HostTCP
    Inherits ClientPeer
    Property Tipo As TipoHost
    Dim srv As ServerTar
    Dim timeOutCierre As Integer = 30000
    Dim timeOutAut As Integer = 25000
    Dim WithEvents TimerRspuesta As New System.Timers.Timer
    Dim tiempoEnvio As Integer = 4
    Dim horaEnvio As DateTime = Now
    Dim respuesta_echo As Boolean = True
    Dim emv As Boolean = False
    Dim direccion As String
    Sub New(hst As TjComun.TipoHost, ip As String, port As Integer, server As ServerTar)
        MyBase.New(hst.ToString, ObtenerTcpChannel(hst, ip, port, Configuracion.TipoConfiguracion, Configuracion.Ciudad), New BasicMessageIdentifierPOS())
        direccion = ip
        Tipo = hst
        srv = server
        TimerRspuesta.Interval = 1000
        TimerRspuesta.Enabled = True
        TimerRspuesta.Start()
    End Sub

    Private Sub Control_Echo(sender As Object, e As ElapsedEventArgs) Handles TimerRspuesta.Elapsed
        Dim Edad = Now.Subtract(horaEnvio).Minutes
        If Edad >= tiempoEnvio Then
            SendEchoTest()
        End If
    End Sub

    Public Function Logger() As log4net.ILog
        Return log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    End Function
#Region "Mensajes"
    Public Function Generar0800(ByVal Implementacion As E_Implementaciones) As Iso8583messagePOS
        StringLog = ""
        declaraciones.Logger(vbNewLine & "       ***************** Enviando 0800 EchoTest ***************************")
        Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(800, Implementacion)
        agregar3_codProc(echoMsg, E_ProcCode.Echotest)
        Agregar7_FechaHora(echoMsg)

        agregar24_IDRed(echoMsg, Implementacion)
        If Implementacion = E_Implementaciones.Visa Or Implementacion = E_Implementaciones.VisaHomol Then
            If Configuracion.TipoConfiguracion = Configuracion.Produccion Then
                'CODIGO DE COMERCIO VISA PRODUCCION Y TERMINAL CAJA 15 PRODUCCION
                Incrementar_Trace(74604015, Implementacion)
                agregar11_trace(echoMsg, Ultimo_Trace(Implementacion, "74604015"))
                agregar41_IdTerminal(echoMsg, "74604015") ' estaba 74601001
                agregar42_IdComercio(echoMsg, "04605366")
            Else
                'DATOS DE PRUEBA
                Incrementar_Trace(72009901, Implementacion)
                agregar11_trace(echoMsg, Ultimo_Trace(Implementacion, "72009901"))
                agregar41_IdTerminal(echoMsg, "72009901") 'TERMINAL DE PRUEBA
                agregar42_IdComercio(echoMsg, "03659307") 'ID COMERCIO PRUEBA
            End If
        Else
            If Configuracion.TipoConfiguracion = Configuracion.Produccion Then
                agregar11_trace(echoMsg, Ultimo_Trace(Implementacion, "75804015"))
                agregar41_IdTerminal(echoMsg, "75804015")
                agregar42_IdComercio(echoMsg, "06473369")
            Else
                'agregar11_trace(echoMsg, Ultimo_Trace(Implementacion, "06000099"))
                agregar41_IdTerminal(echoMsg, "06000099")
                'agregar42_IdComercio(echoMsg, "00000013")
            End If
        End If
        Logger.Info(StringLog)
        Return echoMsg
    End Function

    Public Function Generar0800_Sincronizacion(ByVal req As Req) As Iso8583messagePOS
        StringLog = ""
        declaraciones.Logger(vbNewLine & "       ***************** Enviando 0800 Sincronizacion ***************************")
        Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(800, req.nrohost)
        agregar3_codProc(echoMsg, E_ProcCode.Sincronizacion)
        Agregar7_FechaHora(echoMsg)

        agregar24_IDRed(echoMsg, req.nrohost)
        If req.nrohost = E_Implementaciones.Visa Or req.nrohost = E_Implementaciones.VisaHomol Then
            If Configuracion.TipoConfiguracion = Configuracion.Produccion Then
                'CODIGO DE COMERCIO VISA PRODUCCION Y TERMINAL CAJA 15 PRODUCCION
                Incrementar_Trace("74604015", req.nrohost)
                agregar11_trace(echoMsg, Ultimo_Trace(req.nrohost, "74604015"))
                agregar41_IdTerminal(echoMsg, "74604015") ' estaba 74601001
                'agregar42_IdComercio(echoMsg, "04605366")
            Else
                'DATOS DE PRUEBA
                Incrementar_Trace("72009901", req.nrohost)
                agregar11_trace(echoMsg, Ultimo_Trace(req.nrohost, "72009901"))
                agregar41_IdTerminal(echoMsg, "72009901") 'TERMINAL DE PRUEBA
                'agregar42_IdComercio(echoMsg, "03659307") 'ID COMERCIO PRUEBA
            End If

        Else

            'If Configuracion.TipoConfiguracion = Configuracion.Produccion Then
            '    Incrementar_Trace("06000099", req.nrohost)
            '    agregar11_trace(echoMsg, Ultimo_Trace(req.nrohost, "75804015"))
            '    agregar41_IdTerminal(echoMsg, "75804015")
            '    agregar62_IDEncripcion(echoMsg, "0254677000014978")
            '    'agregar42_IdComercio(echoMsg, "06473369")
            'Else
            Incrementar_Trace(req.terminal, req.nrohost)
            agregar11_trace(echoMsg, Ultimo_Trace(req.nrohost, req.terminal))
            agregar41_IdTerminal(echoMsg, req.terminal)
            agregar62_IDEncripcion(echoMsg, req.idEncripcion)

            'agregar42_IdComercio(echoMsg, "00000013")
            'End If
        End If
        Logger.Info(StringLog)
        Return echoMsg
    End Function

    Private Function vencimiento(s As String) As Date
        Try
            Return CDate("01/" + Mid(s, 3, 2) + "/" + Mid(s, 1, 2))
            Logger.Error("Fecha vencimiento " + "01/" + Mid(s, 3, 2) + "/" + Mid(s, 1, 2))
        Catch ex As Exception
            'Ver que hacer cuando viene fecha vacia, o da una excepcion aca.
            Return CDate("01/01/01")
            Logger.Error("Fecha vencimiento erronea")
        End Try
    End Function

    ''' <summary>
    ''' Genera mensaje para REVERSO
    ''' </summary>
    ''' <param name="req"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Generar0400_3DES(ByVal req As Req) As Iso8583messagePOS
        StringLog = ""
        declaraciones.Logger(vbNewLine & "       ******************* Enviando REVERSO 3DES*******************")

        Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(400, req.nrohost)
        req.tipoMensaje = "0400"
        agregar3_codProc(echoMsg, req.operacion)
        agregar4_Monto(echoMsg, req.importe)
        Agregar7_FechaHora(echoMsg, req.msjIda.fechaTransaccion)
        agregar11_trace(echoMsg, req.nroTrace)
        If req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Chip And
            req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Contactless Then Agregar12y13_HoraFecha(echoMsg, req.msjIda.fechaTransaccion)         ' ACA VER PORQUE VA LA HORA DE LA TRANSACCION 
        'agregar17_CaptureDate(echoMsg, req.FechaHoraEnvioMsg)
        If (req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog) And
            (req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless) Then
            agregar19_codigopais(echoMsg) 'ver segun emisor
        End If
        agregar22_modoIngreso3DES(echoMsg, req.msjIda.tipoIngreso, req.nrohost)      ' 0-BANDA      1-MANUAL 
        If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless Then If req.msjIda.secuencia.Trim <> "" Then agregar23_cardsec(echoMsg, req.msjIda.secuencia)
        agregar24_IDRed(echoMsg, req.nrohost)
        agregar25_condicionlapos(echoMsg)
        If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
            agregar37_RetrefNumber(echoMsg, req.retrefnumber) ' va solo en el caso de ser alguna anulacion.
        End If
        'If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
        '    agregar38_codautorizacion(echoMsg, req.msjRespuesta.Autorizacion)
        'End If
        agregar41_IdTerminal(echoMsg, Trim(req.terminal))
        agregar42_IdComercio(echoMsg, req.idComercio)
        agregar47_BloqueEncriptado(echoMsg, req.msjIda.datosEncriptados, req.nrohost)

        If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion Then
            If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
                '--- CUANDO ES MASTERCAR HAY QUE PASAR 001, 002 Y NO 101 Y 102  (HAY QUE PONERLO COMO STRING)
                If req.planemisor > 100 And req.planemisor < 118 Then
                    agregar48_cuotas(echoMsg, Format(req.planemisor - 100, "000"), req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
                Else
                    agregar48_cuotas(echoMsg, req.planemisor, req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
                End If
            Else
                agregar48_cuotas(echoMsg, req.planemisor, req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
            End If
        Else
            If req.esMaestro Then
                agregar48_cuotas(echoMsg, "001")
            Else
                If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
                    '--- CUANDO ES MASTERCAR HAY QUE PASAR 001, 002 Y NO 101 Y 102  (HAY QUE PONERLO COMO STRING)
                    If req.planemisor > 100 And req.planemisor < 118 Then
                        agregar48_cuotas(echoMsg, Format(req.planemisor - 100, "000"))
                    Else
                        agregar48_cuotas(echoMsg, req.planemisor)
                    End If
                Else
                    agregar48_cuotas(echoMsg, req.planemisor)
                End If
            End If
        End If

        If req.msjIda.moneda = TransmisorTCP.Moneda.Pesos Then
            agregar49_moneda(echoMsg, E_Monedas.PESOS)      'req.moneda)
        ElseIf req.msjIda.moneda = TransmisorTCP.Moneda.Dolares Then
            agregar49_moneda(echoMsg, E_Monedas.DOLARES)      'req.moneda)
        End If
        'If req.esMaestro AndAlso req.pinBlock IsNot Nothing AndAlso req.pinBlock.Trim <> "" Then agregar52_Pinblock(echoMsg, req.pinBlock)
        If req.esCashback Then agregar54_Cashback(echoMsg, req.msjIda.importeCashback)
        If req.FALLBACK Then
        Else
            If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless Then agregar55_CodSeg3DES(echoMsg, req.msjIda.criptograma)
        End If
        If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
            agregar59_InfAdicional3DES(echoMsg, req.msjIda.idEncripcion, req.nrohost, req.FALLBACK, req.msjIda.datosemisor)
            agregar60_VersionSoft(echoMsg, req.msjIda.versionAPP)
        Else
            agregar59_InfAdicional3DES(echoMsg, req.msjIda.idEncripcion, req.nrohost, False, "", req.msjIda.versionAPP)
            agregar60_VersionSoft(echoMsg)
        End If
        agregar62_NroTicket(echoMsg, req.nroTicket)
        Logger.Info(StringLog)

        Return echoMsg

    End Function





    ''' <summary>
    ''' Genera mensaje para ADVICE
    ''' </summary>
    ''' <param name="req"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Generar0220_3DES(ByVal req As Req) As Iso8583messagePOS
        StringLog = ""
        declaraciones.Logger(vbNewLine & "       ******************* Enviando ADVICE 3DES*******************")

        Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(220, req.nrohost)
        req.tipoMensaje = "0220"
        agregar3_codProc(echoMsg, req.operacion)
        agregar4_Monto(echoMsg, req.importe)
        Agregar7_FechaHora(echoMsg, req.msjIda.fechaTransaccion)
        agregar11_trace(echoMsg, req.nroTrace)               '--- agregar11_trace(echoMsg, req.nroTraceAdv)
        If req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Chip And
            req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Contactless Then Agregar12y13_HoraFecha(echoMsg, req.msjIda.fechaTransaccion)         ' ACA VER PORQUE VA LA HORA DE LA TRANSACCION 

        agregar17_CaptureDate(echoMsg, req.FechaHoraEnvioMsg)

        If (req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog) And
            (req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless) Then
            agregar19_codigopais(echoMsg) 'ver segun emisor
        End If
        agregar22_modoIngreso3DES(echoMsg, req.msjIda.tipoIngreso, req.nrohost)      ' 0-BANDA      1-MANUAL 
        If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless Then If req.msjIda.secuencia.Trim <> "" Then agregar23_cardsec(echoMsg, req.msjIda.secuencia)
        agregar24_IDRed(echoMsg, req.nrohost)
        agregar25_condicionlapos(echoMsg)
        If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
            agregar37_RetrefNumber(echoMsg, req.retrefnumber) ' va solo en el caso de ser alguna anulacion.
        End If
        'If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
        agregar38_codautorizacion(echoMsg, req.msjIda.CodAutorizaAdv)     '  req.msjRespuesta.Autorizacion)
        'End If
        agregar41_IdTerminal(echoMsg, Trim(req.terminal))
        agregar42_IdComercio(echoMsg, req.idComercio)
        agregar47_BloqueEncriptado(echoMsg, req.msjIda.datosEncriptados, req.nrohost)

        If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion Then
            If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
                '--- CUANDO ES MASTERCAR HAY QUE PASAR 001, 002 Y NO 101 Y 102  (HAY QUE PONERLO COMO STRING)
                If req.planemisor > 100 And req.planemisor < 118 Then
                    agregar48_cuotas(echoMsg, Format(req.planemisor - 100, "000"), req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
                Else
                    agregar48_cuotas(echoMsg, req.planemisor, req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
                End If
            Else
                agregar48_cuotas(echoMsg, req.planemisor, req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
            End If
        Else
            If req.esMaestro Then
                agregar48_cuotas(echoMsg, "001")
            Else
                If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
                    '--- CUANDO ES MASTERCAR HAY QUE PASAR 001, 002 Y NO 101 Y 102  (HAY QUE PONERLO COMO STRING)
                    If req.planemisor > 100 And req.planemisor < 118 Then
                        agregar48_cuotas(echoMsg, Format(req.planemisor - 100, "000"))
                    Else
                        agregar48_cuotas(echoMsg, req.planemisor)
                    End If
                Else
                    agregar48_cuotas(echoMsg, req.planemisor)
                End If
            End If
        End If

        If req.msjIda.moneda = TransmisorTCP.Moneda.Pesos Then
            agregar49_moneda(echoMsg, E_Monedas.PESOS)      'req.moneda)
        ElseIf req.msjIda.moneda = TransmisorTCP.Moneda.Dolares Then
            agregar49_moneda(echoMsg, E_Monedas.DOLARES)      'req.moneda)
        End If
        'If req.esMaestro AndAlso req.pinBlock IsNot Nothing AndAlso req.pinBlock.Trim <> "" Then agregar52_Pinblock(echoMsg, req.pinBlock)
        If req.esCashback Then agregar54_Cashback(echoMsg, req.msjIda.importeCashback)
        If req.FALLBACK Then
        Else
            If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless Then agregar55_CodSeg3DES(echoMsg, req.msjIda.criptograma)
        End If
        If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
            agregar59_InfAdicional3DES(echoMsg, req.msjIda.idEncripcion, req.nrohost, req.FALLBACK, req.msjIda.datosemisor)
            agregar60_VersionSoft(echoMsg, req.msjIda.versionAPP)
        Else
            agregar59_InfAdicional3DES(echoMsg, req.msjIda.idEncripcion, req.nrohost, False, "", req.msjIda.versionAPP)
            agregar60_VersionSoft(echoMsg)
        End If
        agregar62_NroTicket(echoMsg, req.nroTicket)
        Logger.Info(StringLog)

        Return echoMsg

    End Function




    ''' <summary>
    ''' Genera mensaje para ADVICE
    ''' </summary>
    ''' <param name="req"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Generar0220_Y1_3DES(ByVal req As Req) As Iso8583messagePOS
        Try
            StringLog = ""
            declaraciones.Logger(vbNewLine & "       ******************* Enviando ADVICE 3DES*******************")

            Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(220, req.nrohost)
            req.tipoMensaje = "0220"
            agregar3_codProc(echoMsg, req.operacion)
            agregar4_Monto(echoMsg, req.importe)
            Agregar7_FechaHora(echoMsg, req.msjIda.fechaTransaccion)
            agregar11_trace(echoMsg, req.nroTrace)               '--- agregar11_trace(echoMsg, req.nroTraceAdv)
            If req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Chip And
            req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Contactless Then Agregar12y13_HoraFecha(echoMsg, req.msjIda.fechaTransaccion)         ' ACA VER PORQUE VA LA HORA DE LA TRANSACCION 

            agregar17_CaptureDate(echoMsg, req.FechaHoraEnvioMsg)

            If (req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog) And
            (req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless) Then
                agregar19_codigopais(echoMsg) 'ver segun emisor
            End If
            agregar22_modoIngreso3DES(echoMsg, req.msjIda.tipoIngreso, req.nrohost)      ' 0-BANDA      1-MANUAL 
            If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless Then If req.msjIda.secuencia.Trim <> "" Then agregar23_cardsec(echoMsg, req.msjIda.secuencia)
            agregar24_IDRed(echoMsg, req.nrohost)
            agregar25_condicionlapos(echoMsg)
            If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
                agregar37_RetrefNumber(echoMsg, req.retrefnumber) ' va solo en el caso de ser alguna anulacion.
            End If
            'If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
            agregar38_codautorizacion(echoMsg, req.msjIda.CodAutorizaAdv)     '  req.msjRespuesta.Autorizacion)
            'End If
            agregar41_IdTerminal(echoMsg, Trim(req.terminal))
            agregar42_IdComercio(echoMsg, req.idComercio)
            agregar47_BloqueEncriptado(echoMsg, req.msjIda.datosEncriptados, req.nrohost)

            If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion Then
                If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
                    '--- CUANDO ES MASTERCAR HAY QUE PASAR 001, 002 Y NO 101 Y 102  (HAY QUE PONERLO COMO STRING)
                    If req.planemisor > 100 And req.planemisor < 118 Then
                        agregar48_cuotas(echoMsg, Format(req.planemisor - 100, "000"), req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
                    Else
                        agregar48_cuotas(echoMsg, req.planemisor, req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
                    End If
                Else
                    agregar48_cuotas(echoMsg, req.planemisor, req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
                End If
            Else
                If req.esMaestro Then
                    agregar48_cuotas(echoMsg, "001")
                Else
                    If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
                        '--- CUANDO ES MASTERCAR HAY QUE PASAR 001, 002 Y NO 101 Y 102  (HAY QUE PONERLO COMO STRING)
                        If req.planemisor > 100 And req.planemisor < 118 Then
                            agregar48_cuotas(echoMsg, Format(req.planemisor - 100, "000"))
                        Else
                            agregar48_cuotas(echoMsg, req.planemisor)
                        End If
                    Else
                        agregar48_cuotas(echoMsg, req.planemisor)
                    End If
                End If
            End If

            If req.msjIda.moneda = TransmisorTCP.Moneda.Pesos Then
                agregar49_moneda(echoMsg, E_Monedas.PESOS)      'req.moneda)
            ElseIf req.msjIda.moneda = TransmisorTCP.Moneda.Dolares Then
                agregar49_moneda(echoMsg, E_Monedas.DOLARES)      'req.moneda)
            End If
            'If req.esMaestro AndAlso req.pinBlock IsNot Nothing AndAlso req.pinBlock.Trim <> "" Then agregar52_Pinblock(echoMsg, req.pinBlock)
            If req.esCashback Then agregar54_Cashback(echoMsg, req.msjIda.importeCashback)
            If req.FALLBACK Then
            Else
                If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless Then agregar55_CodSeg3DES(echoMsg, req.msjIda.criptograma)
            End If
            If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
                agregar59_InfAdicional3DES(echoMsg, req.msjIda.idEncripcion, req.nrohost, req.FALLBACK, req.msjIda.datosemisor)
                agregar60_VersionSoft(echoMsg, req.msjIda.versionAPP)
            Else
                agregar59_InfAdicional3DES(echoMsg, req.msjIda.idEncripcion, req.nrohost, False, "", req.msjIda.versionAPP)
                agregar60_VersionSoft(echoMsg)
            End If
            agregar62_NroTicket(echoMsg, req.nroTicket)
            Logger.Info(StringLog)

            Return echoMsg

        Catch

        End Try
    End Function





    ''' <summary>
    ''' Genera mensaje para ADVICE
    ''' </summary>
    ''' <param name="req"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Generar0220_Z1_3DES(ByVal req As Req) As Iso8583messagePOS
        Try
            StringLog = ""
            declaraciones.Logger(vbNewLine & "       ******************* Enviando ADVICE 3DES*******************")

            Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(220, req.nrohost)
            req.tipoMensaje = "0220"
            agregar3_codProc(echoMsg, req.operacion)
            agregar4_Monto(echoMsg, req.importe)
            Agregar7_FechaHora(echoMsg, req.msjIda.fechaTransaccion)
            agregar11_trace(echoMsg, req.nroTrace)               '--- agregar11_trace(echoMsg, req.nroTraceAdv)
            If req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Chip And
            req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Contactless Then Agregar12y13_HoraFecha(echoMsg, req.msjIda.fechaTransaccion)         ' ACA VER PORQUE VA LA HORA DE LA TRANSACCION 

            agregar17_CaptureDate(echoMsg, req.FechaHoraEnvioMsg)

            If (req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog) And
            (req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless) Then
                agregar19_codigopais(echoMsg) 'ver segun emisor
            End If
            agregar22_modoIngreso3DES(echoMsg, req.msjIda.tipoIngreso, req.nrohost)      ' 0-BANDA      1-MANUAL 
            If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless Then If req.msjIda.secuencia.Trim <> "" Then agregar23_cardsec(echoMsg, req.msjIda.secuencia)
            agregar24_IDRed(echoMsg, req.nrohost)
            agregar25_condicionlapos(echoMsg)
            If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
                agregar37_RetrefNumber(echoMsg, req.retrefnumber) ' va solo en el caso de ser alguna anulacion.
            End If
            'If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
            agregar38_codautorizacion(echoMsg, req.msjIda.CodAutorizaAdv)     '  req.msjRespuesta.Autorizacion)
            'End If
            agregar41_IdTerminal(echoMsg, Trim(req.terminal))
            agregar42_IdComercio(echoMsg, req.idComercio)
            agregar47_BloqueEncriptado(echoMsg, req.msjIda.datosEncriptados, req.nrohost)

            If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion Then
                If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
                    '--- CUANDO ES MASTERCAR HAY QUE PASAR 001, 002 Y NO 101 Y 102  (HAY QUE PONERLO COMO STRING)
                    If req.planemisor > 100 And req.planemisor < 118 Then
                        agregar48_cuotas(echoMsg, Format(req.planemisor - 100, "000"), req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
                    Else
                        agregar48_cuotas(echoMsg, req.planemisor, req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
                    End If
                Else
                    agregar48_cuotas(echoMsg, req.planemisor, req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
                End If
            Else
                If req.esMaestro Then
                    agregar48_cuotas(echoMsg, "001")
                Else
                    If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
                        '--- CUANDO ES MASTERCAR HAY QUE PASAR 001, 002 Y NO 101 Y 102  (HAY QUE PONERLO COMO STRING)
                        If req.planemisor > 100 And req.planemisor < 118 Then
                            agregar48_cuotas(echoMsg, Format(req.planemisor - 100, "000"))
                        Else
                            agregar48_cuotas(echoMsg, req.planemisor)
                        End If
                    Else
                        agregar48_cuotas(echoMsg, req.planemisor)
                    End If
                End If
            End If

            If req.msjIda.moneda = TransmisorTCP.Moneda.Pesos Then
                agregar49_moneda(echoMsg, E_Monedas.PESOS)      'req.moneda)
            ElseIf req.msjIda.moneda = TransmisorTCP.Moneda.Dolares Then
                agregar49_moneda(echoMsg, E_Monedas.DOLARES)      'req.moneda)
            End If
            'If req.esMaestro AndAlso req.pinBlock IsNot Nothing AndAlso req.pinBlock.Trim <> "" Then agregar52_Pinblock(echoMsg, req.pinBlock)
            If req.esCashback Then agregar54_Cashback(echoMsg, req.msjIda.importeCashback)
            If req.FALLBACK Then
            Else
                If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless Then agregar55_CodSeg3DES(echoMsg, req.msjIda.criptograma)
            End If
            If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
                agregar59_InfAdicional3DES(echoMsg, req.msjIda.idEncripcion, req.nrohost, req.FALLBACK, req.msjIda.datosemisor)
                agregar60_VersionSoft(echoMsg, req.msjIda.versionAPP)
            Else
                agregar59_InfAdicional3DES(echoMsg, req.msjIda.idEncripcion, req.nrohost, False, "", req.msjIda.versionAPP)
                agregar60_VersionSoft(echoMsg)
            End If
            agregar62_NroTicket(echoMsg, req.nroTicket)
            Logger.Info(StringLog)

            Return echoMsg
        Catch

        End Try

    End Function













    Public Function Generar0400PEI(ByVal req As Req) As Iso8583messagePOS
        StringLog = ""
        declaraciones.Logger(vbNewLine & "       ******************* Enviando REVERSO PEI *******************")

        Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(400, req.nrohost)
        req.tipoMensaje = "0400"

        If req.ida.MANUAL = E_ModoIngreso.Manual Then agregar2_tarjeta(echoMsg, req.ida.TARJ)

        agregar3_codProc(echoMsg, req.operacion)

        agregar4_Monto(echoMsg, req.importe)
        Agregar7_FechaHora(echoMsg)
        agregar11_trace(echoMsg, req.nroTrace)
        Agregar12y13_HoraFecha(echoMsg, DateTime.FromOADate(req.ida.HORA))         ' ACA VER PORQUE VA LA HORA DE LA TRANSACCION 

        If req.ida.MANUAL = E_ModoIngreso.Manual Then Agregar14_expdate(echoMsg, vencimiento(req.ida.EXPDATE))

        agregar22_modoIngreso(echoMsg, req.ida.MANUAL, req.esMaestro)      ' 0-BANDA      1-MANUAL 
        agregar24_IDRed(echoMsg, req.nrohost)
        agregar25_condicionlapos(echoMsg)
        If req.ida.MANUAL <> E_ModoIngreso.Manual Then
            agregar35_Track2(echoMsg, req.ida.TRACK2) ' req.ida.TARJ, req.ida.EXPDATE)
        End If

        If req.ida.oper = 2 Or req.ida.oper = 3 Then
            agregar37_RetrefNumber(echoMsg, req.retrefnumber) ' va solo en el caso de ser alguna anulacion.
        End If

        agregar41_IdTerminal(echoMsg, Trim(req.terminal))

        'agregar42_IdComercio(echoMsg, "03659307")
        agregar42_IdComercio(echoMsg, req.idComercio)
        If req.ida.MANUAL <> E_ModoIngreso.Manual And req.ida.TRACK1.Trim <> "" Then
            agregar45_track1(echoMsg, req.ida.TRACK1)
        End If
        If req.ida.MANUAL <> E_ModoIngreso.Manual And req.ida.TRACK1.Trim = "" Then
            agregar46_Track1NoLeido(echoMsg, "1")       ' PREGUNTAR A GUSTAVO QUE HACEMOS, VOY A TENER QUE GUARDARLO TRACK 1 NO LEIDO ?????????? NOSOTROS LO CONTROLAMOS EN REG3000 NO HABRIA QUE MANDARLO.
        End If

        If req.ida.oper = 1 Then
            agregar48_cuotas(echoMsg, req.planemisor, req.ida.TKTORI, req.ida.FECORI)
        Else
            agregar48_cuotas(echoMsg, req.planemisor)
        End If
        '        agregar48_cuotas(echoMsg, req.cuotas) ' VER SI REVERSA LAS MISMAS CUOTAS DEL ORIGINAL O PUEDE REVERSAR OTRA, VER MANUAL POSNET
        agregar49_moneda(echoMsg, E_Monedas.PESOS)

        If req.esMaestro Then agregar52_Pinblock(echoMsg, req.pinBlock)
        If req.esCashback Then agregar54_Cashback(echoMsg, req.ida.CASHBACK)

        'If req.nrohost = TipoHost.POSNET Then agregar59_InfAdicional(echoMsg)
        If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then agregar59_InfAdicional(echoMsg)

        agregar60_VersionSoft(echoMsg)
        agregar62_NroTicket(echoMsg, req.nroTicket)
        Logger.Info(StringLog)

        Return echoMsg
    End Function



    ''' <summary>
    ''' Genera mensaje para REVERSO
    ''' </summary>
    ''' <param name="req"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Generar0400(ByVal req As Req) As Iso8583messagePOS
        StringLog = ""
        declaraciones.Logger(vbNewLine & "       ******************* Enviando REVERSO *******************")

        Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(400, req.nrohost)
        req.tipoMensaje = "0400"

        If req.ida.MANUAL = E_ModoIngreso.Manual Then agregar2_tarjeta(echoMsg, req.ida.TARJ)

        agregar3_codProc(echoMsg, req.operacion)

        agregar4_Monto(echoMsg, req.importe)
        Agregar7_FechaHora(echoMsg)
        agregar11_trace(echoMsg, req.nroTrace)
        Agregar12y13_HoraFecha(echoMsg, DateTime.FromOADate(req.ida.HORA))         ' ACA VER PORQUE VA LA HORA DE LA TRANSACCION 

        If req.ida.MANUAL = E_ModoIngreso.Manual Then Agregar14_expdate(echoMsg, vencimiento(req.ida.EXPDATE))

        agregar22_modoIngreso(echoMsg, req.ida.MANUAL, req.esMaestro)      ' 0-BANDA      1-MANUAL 
        agregar24_IDRed(echoMsg, req.nrohost)
        agregar25_condicionlapos(echoMsg)
        If req.ida.MANUAL <> E_ModoIngreso.Manual Then
            agregar35_Track2(echoMsg, req.ida.TRACK2) ' req.ida.TARJ, req.ida.EXPDATE)
        End If

        If req.ida.oper = 2 Or req.ida.oper = 3 Then
            agregar37_RetrefNumber(echoMsg, req.retrefnumber) ' va solo en el caso de ser alguna anulacion.
        End If


        agregar41_IdTerminal(echoMsg, Trim(req.terminal))

        'agregar42_IdComercio(echoMsg, "03659307")
        agregar42_IdComercio(echoMsg, req.idComercio)
        If req.ida.MANUAL <> E_ModoIngreso.Manual And req.ida.TRACK1.Trim <> "" Then
            agregar45_track1(echoMsg, req.ida.TRACK1)
        End If
        If req.ida.MANUAL <> E_ModoIngreso.Manual And req.ida.TRACK1.Trim = "" Then
            agregar46_Track1NoLeido(echoMsg, "1")       ' PREGUNTAR A GUSTAVO QUE HACEMOS, VOY A TENER QUE GUARDARLO TRACK 1 NO LEIDO ?????????? NOSOTROS LO CONTROLAMOS EN REG3000 NO HABRIA QUE MANDARLO.
        End If

        If req.ida.oper = 1 Then
            agregar48_cuotas(echoMsg, req.planemisor, req.ida.TKTORI, req.ida.FECORI)
        Else
            agregar48_cuotas(echoMsg, req.planemisor)
        End If
        '        agregar48_cuotas(echoMsg, req.cuotas) ' VER SI REVERSA LAS MISMAS CUOTAS DEL ORIGINAL O PUEDE REVERSAR OTRA, VER MANUAL POSNET
        agregar49_moneda(echoMsg, E_Monedas.PESOS)

        If req.esMaestro Then agregar52_Pinblock(echoMsg, req.pinBlock)
        If req.esCashback Then agregar54_Cashback(echoMsg, req.ida.CASHBACK)

        'If req.nrohost = TipoHost.POSNET Then agregar59_InfAdicional(echoMsg)
        If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then agregar59_InfAdicional(echoMsg)


        agregar60_VersionSoft(echoMsg)
        agregar62_NroTicket(echoMsg, req.nroTicket)
        Logger.Info(StringLog)

        Return echoMsg

    End Function
    ''' <summary>
    ''' Genera mensaje para Cierre de lote
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Generar0500(req As Req) As Iso8583messagePOS
        StringLog = ""
        declaraciones.Logger(vbNewLine & "       **************** Enviando CIERRE *************************")

        Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(500, req.nrohost) ' poner host

        agregar3_codProc(echoMsg, E_ProcCode.Cierre_Lote)

        Agregar7_FechaHora(echoMsg)
        agregar11_trace(echoMsg, req.nroTrace)
        agregar15_FechaCierre(echoMsg)
        agregar24_IDRed(echoMsg, req.nrohost)
        agregar41_IdTerminal(echoMsg, Trim(req.terminal))
        agregar42_IdComercio(echoMsg, req.idComercio)
        agregar60_VersionSoft(echoMsg, BuscarVersionSoftPP(req.terminal, req.nrohost))
        'agregar60_VersionSoft(echoMsg)
        agregar63_InfoCierre(echoMsg, req.nroLote,
                                      req.CierreCantCompras, req.CierreMontoCompras,
                                      req.CierreCantDevoluciones, req.CierreMontoDevoluciones,
                                      req.CierreCantAnulaciones, req.CierreMontoAnulaciones)
        Logger.Info(StringLog)
        Return echoMsg
    End Function


    ''' <summary>
    ''' Genera mensaje 0200 para Compra, Anulacion y Devolucion (CON ENCRIPTACION 3DES).
    ''' </summary>
    ''' <param name="req"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Generar0200_3DES(ByVal req As Req) As Iso8583messagePOS

        '--------------------------------------------------------------------------------------------------------------------------
        '--- ACA VA CONCATENANDO Y FORMANDO EL REQ PARA MANDARSE DE ACUERDO AL MANUAL  *** EMV PLATAFORMA SISTEMAS PROPIOS ***     
        '--- PAGINA 14                                                                                                             
        '--------------------------------------------------------------------------------------------------------------------------
        StringLog = ""
        declaraciones.Logger(vbNewLine & "       ***************** Enviando 0200  3DES *******************************")

        Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(200, req.nrohost)
        req.tipoMensaje = "0200"
        agregar3_codProc(echoMsg, req.operacion)
        agregar4_Monto(echoMsg, req.importe)      '--- ya viene multiplicado con el coeficiente.
        Agregar7_FechaHora(echoMsg, req.msjIda.fechaTransaccion)
        agregar11_trace(echoMsg, req.nroTrace)
        If req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Chip And
            req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Contactless Then Agregar12y13_HoraFecha(echoMsg, req.msjIda.fechaTransaccion)    '--- ACA VER PORQUE VA LA HORA DE LA TRANSACCION 

        If (req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog) And
            (req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless) Then
            agregar19_codigopais(echoMsg)    '--- ver segun emisor
        End If
        agregar22_modoIngreso3DES(echoMsg, req.msjIda.tipoIngreso, req.nrohost)      ' 0-BANDA      1-MANUAL          5-CHIP
        If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless Then If req.msjIda.secuencia.Trim <> "" Then agregar23_cardsec(echoMsg, req.msjIda.secuencia)
        agregar24_IDRed(echoMsg, req.nrohost)
        agregar25_condicionlapos(echoMsg)
        If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then agregar37_RetrefNumber(echoMsg, req.retrefnumber) ' va solo en el caso de ser alguna anulacion.
        agregar41_IdTerminal(echoMsg, Trim(req.terminal))
        agregar42_IdComercio(echoMsg, req.idComercio)
        agregar47_BloqueEncriptado(echoMsg, req.msjIda.datosEncriptados, req.nrohost)

        If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion Then
            If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
                '--- CUANDO ES MASTERCAR HAY QUE PASAR 001, 002 Y NO 101 Y 102  (HAY QUE PONERLO COMO STRING)
                If req.planemisor > 100 And req.planemisor < 118 Then
                    agregar48_cuotas(echoMsg, Format(req.planemisor - 100, "000"), req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
                Else
                    agregar48_cuotas(echoMsg, req.planemisor, req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
                End If
            Else
                    agregar48_cuotas(echoMsg, req.planemisor, req.msjIda.ticketOriginal, req.msjIda.fechaOperacionOriginal.ToString("ddMMyy"))
            End If
        Else
            If req.esMaestro Then
                agregar48_cuotas(echoMsg, "001")
            Else
                If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then

                    '--- CUANDO ES MASTERCAR HAY QUE PASAR 001, 002 Y NO 101 Y 102  (HAY QUE PONERLO COMO STRING)
                    If IsNumeric(req.planemisor) = True Then
                        If req.planemisor > 100 And req.planemisor < 118 Then
                            agregar48_cuotas(echoMsg, Format(req.planemisor - 100, "000"))
                        Else
                            agregar48_cuotas(echoMsg, req.planemisor)
                        End If
                    Else
                        agregar48_cuotas(echoMsg, req.planemisor)
                    End If

                Else
                    agregar48_cuotas(echoMsg, req.planemisor)
                End If
            End If
        End If
        'TODO: en el manual de visa nuevo no esta
        If req.msjIda.moneda = TransmisorTCP.Moneda.Pesos Then
            agregar49_moneda(echoMsg, E_Monedas.PESOS)      'req.moneda)
        ElseIf req.msjIda.moneda = TransmisorTCP.Moneda.Dolares Then
            agregar49_moneda(echoMsg, E_Monedas.DOLARES)      'req.moneda)
        End If

        '--- SERVICE CODE:  LO TRAE DEL TRACK 2 (SON LOS 3 DIGITOS QUE ESTAN DESPUES DE LA FECHA DE VENCIMIENTO)     
        '---                CUANDO EL TERCER DIGITO DE ESOS 3 DIGITOS ES UN 0, 3, 5, 6 ó 7 TIENE QUE SOLICITAR PIN   
        '---                MAESTRO LO TRAE SIEMPRE                                                                  
        If req.esMaestro AndAlso req.pinBlock IsNot Nothing AndAlso req.pinBlock.Trim <> "" Then
            agregar52_Pinblock(echoMsg, req.pinBlock)
        ElseIf req.pinBlock IsNot Nothing AndAlso req.pinBlock.Trim <> "" Then
            agregar52_Pinblock(echoMsg, req.pinBlock)
        End If

        If req.esCashback Then agregar54_Cashback(echoMsg, req.msjIda.importeCashback)

        If req.FALLBACK Then

        Else
            If req.msjIda.tipoIngreso = E_ModoIngreso.Chip Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Contactless Then agregar55_CodSeg(echoMsg, Trim(req.msjIda.criptograma))
        End If
        If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
            agregar59_InfAdicional3DES(echoMsg, req.msjIda.idEncripcion, req.nrohost, req.FALLBACK, req.msjIda.datosemisor)
            agregar60_VersionSoft(echoMsg, req.msjIda.versionAPP)
        Else ' VISA
            agregar59_InfAdicional3DES(echoMsg, req.msjIda.idEncripcion, req.nrohost, False, "", req.msjIda.versionAPP)
            agregar60_VersionSoft(echoMsg)
        End If
        agregar62_NroTicket(echoMsg, req.nroTicket)
        Logger.Info(StringLog)
        Return echoMsg
    End Function

    '''' <summary>
    '''' Genera mensaje 0200 para Compra, Anulacion y Devolucion.
    '''' </summary>
    '''' <param name="req"></param>
    '''' <returns></returns>
    '''' <remarks></remarks>
    'Public Function Generar0200Sin3DES(ByVal req As Req) As Iso8583messagePOS
    '    StringLog = ""
    '    declaraciones.Logger(vbNewLine & "       ***************** Enviando 0200 *******************************")

    '    Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(200, req.nrohost)
    '    req.tipoMensaje = "0200"

    '    If req.ida.MANUAL = E_ModoIngreso.Manual Then agregar2_tarjeta(echoMsg, req.msjIda.nroTarjeta) ' ver en manual dice que si el ingreso de la tarjeta fue manual va el nro de tarjeta aca.

    '    agregar3_codProc(echoMsg, req.operacion)
    '    agregar4_Monto(echoMsg, req.importe) ' ya viene multiplicado con el coeficiente.

    '    If req.ida.MANUAL <> E_ModoIngreso.Chip Then
    '        Agregar7_FechaHora(echoMsg)
    '    Else
    '        If Not req.esCashback Then Agregar7_FechaHora(echoMsg)
    '    End If

    '    agregar11_trace(echoMsg, req.nroTrace)

    '    If req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Chip Then
    '        Agregar12y13_HoraFecha(echoMsg, req.msjIda.fechaTransaccion)         ' ACA VER PORQUE VA LA HORA DE LA TRANSACCION 
    '    End If

    '    If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Manual Then Agregar14_expdate(echoMsg, vencimiento(req.msjIda.fechaExpiracion))

    '    If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Then agregar19_codigopais(echoMsg)
    '    agregar22_modoIngreso(echoMsg, req.msjIda.tipoIngreso, req.esMaestro)      ' 0-BANDA      1-MANUAL          5-CHIP
    '    'If req.ida.MANUAL = E_ModoIngreso.Chip Then
    '    '    If req.ida.CARDSEQ.Trim <> "" Then
    '    '        agregar23_cardsec(echoMsg, req.ida.CARDSEQ)
    '    '    End If
    '    'End If
    '    agregar24_IDRed(echoMsg, req.nrohost)
    '    agregar25_condicionlapos(echoMsg)

    '    If req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Banda Or req.msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Chip Then
    '        agregar35_Track2(echoMsg, req.msjIda.track2)
    '    End If

    '    If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
    '        agregar37_RetrefNumber(echoMsg, req.retrefnumber) ' va solo en el caso de ser alguna anulacion.
    '    End If

    '    agregar41_IdTerminal(echoMsg, Trim(req.terminal))
    '    agregar42_IdComercio(echoMsg, req.idComercio)

    '    If (req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Banda Or req.ida.MANUAL = E_ModoIngreso.Chip) And req.ida.TRACK1.Trim <> "" Then
    '        agregar45_track1(echoMsg, req.ida.TRACK1)
    '    End If
    '    If (req.ida.MANUAL = E_ModoIngreso.Banda Or req.ida.MANUAL = E_ModoIngreso.Chip) And req.ida.TRACK1.Trim = "" Then
    '        'TODO: ver porque en el manual nuevo de visa no esta este campo
    '        agregar46_Track1NoLeido(echoMsg, "1")       'TRACK 1 NO LEIDO ?????????? NOSOTROS LO CONTROLAMOS EN REG3000 NO HABRIA QUE MANDARLO.
    '    End If

    '    If req.ida.oper = 1 Then
    '        agregar48_cuotas(echoMsg, req.planemisor, req.ida.TKTORI, req.ida.FECORI)
    '    Else
    '        If req.esMaestro Then
    '            agregar48_cuotas(echoMsg, "001")
    '        Else
    '            agregar48_cuotas(echoMsg, req.planemisor)
    '        End If

    '    End If


    '    'TODO: en el manual de visa nuevo no esta
    '    agregar49_moneda(echoMsg, E_Monedas.PESOS)      'req.moneda)

    '    If req.esMaestro Then agregar52_Pinblock(echoMsg, req.pinBlock)
    '    If req.esCashback Then agregar54_Cashback(echoMsg, req.ida.CASHBACK)


    '    'If req.ida.MANUAL = E_ModoIngreso.Chip Then
    '    'TODO: esto es para chip, ver
    '    'agregar55_CodSeg(echoMsg, Trim(req.ida.CRIPTO))
    '    'Else
    '    If req.obligarCodSeg = True Then
    '        If req.ida.CODSEG.Trim = "" Then
    '            req.InvalidarReq("Falta Cod. de Seguridad")
    '        Else
    '            agregar55_CodSeg(echoMsg, Trim(req.ida.CODSEG))
    '        End If
    '    End If
    '    'End If

    '    If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
    '        agregar59_InfAdicional(echoMsg)
    '    End If


    '    If req.ida.MANUAL = E_ModoIngreso.Chip Then
    '        'TODO: version para chip
    '        'agregar60_VersionSoft(echoMsg, req.ida.APPVERSION)
    '        If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
    '            agregar60_VersionSoft(echoMsg, "A0600")
    '        Else
    '            agregar60_VersionSoft(echoMsg)
    '        End If

    '    Else
    '        agregar60_VersionSoft(echoMsg)
    '    End If

    '    agregar62_NroTicket(echoMsg, req.nroTicket)
    '    Logger.Info(StringLog)
    '    Return echoMsg
    'End Function



    ''' <summary>
    ''' Genera mensaje 0200 para Compra, Anulacion y Devolucion.
    ''' </summary>
    ''' <param name="req"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Generar0200(ByVal req As Req) As Iso8583messagePOS
        StringLog = ""
        declaraciones.Logger(vbNewLine & "       ***************** Enviando 0200 *******************************")

        Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(200, req.nrohost)
        req.tipoMensaje = "0200"

        If req.ida.MANUAL = E_ModoIngreso.Manual Then agregar2_tarjeta(echoMsg, req.ida.TARJ) ' ver en manual dice que si el ingreso de la tarjeta fue manual va el nro de tarjeta aca.

        agregar3_codProc(echoMsg, req.operacion)
        agregar4_Monto(echoMsg, req.importe) ' ya viene multiplicado con el coeficiente.

        If req.ida.MANUAL <> E_ModoIngreso.Chip Then
            Agregar7_FechaHora(echoMsg)
        Else
            If Not req.esCashback Then Agregar7_FechaHora(echoMsg)
        End If

        agregar11_trace(echoMsg, req.nroTrace)

        If req.ida.MANUAL <> E_ModoIngreso.Chip Then
            Agregar12y13_HoraFecha(echoMsg, DateTime.FromOADate(req.ida.HORA))         ' ACA VER PORQUE VA LA HORA DE LA TRANSACCION 
        End If

        If req.ida.MANUAL = E_ModoIngreso.Manual Then Agregar14_expdate(echoMsg, vencimiento(req.ida.EXPDATE))

        If req.ida.MANUAL = E_ModoIngreso.Chip Then agregar19_codigopais(echoMsg)
        agregar22_modoIngreso(echoMsg, req.ida.MANUAL, req.esMaestro)      ' 0-BANDA      1-MANUAL          5-CHIP
        'If req.ida.MANUAL = E_ModoIngreso.Chip Then
        '    If req.ida.CARDSEQ.Trim <> "" Then
        '        agregar23_cardsec(echoMsg, req.ida.CARDSEQ)
        '    End If
        'End If
        agregar24_IDRed(echoMsg, req.nrohost)
        agregar25_condicionlapos(echoMsg)

        If req.ida.MANUAL = E_ModoIngreso.Banda Or req.ida.MANUAL = E_ModoIngreso.Chip Then
            agregar35_Track2(echoMsg, req.ida.TRACK2)
        End If

        If req.ida.oper = 2 Or req.ida.oper = 3 Then
            agregar37_RetrefNumber(echoMsg, req.retrefnumber) ' va solo en el caso de ser alguna anulacion.
        End If

        agregar41_IdTerminal(echoMsg, Trim(req.terminal))
        agregar42_IdComercio(echoMsg, req.idComercio)

        If (req.ida.MANUAL = E_ModoIngreso.Banda Or req.ida.MANUAL = E_ModoIngreso.Chip) And req.ida.TRACK1.Trim <> "" Then
            agregar45_track1(echoMsg, req.ida.TRACK1)
        End If
        If (req.ida.MANUAL = E_ModoIngreso.Banda Or req.ida.MANUAL = E_ModoIngreso.Chip) And req.ida.TRACK1.Trim = "" Then
            'TODO: ver porque en el manual nuevo de visa no esta este campo
            agregar46_Track1NoLeido(echoMsg, "1")       'TRACK 1 NO LEIDO ?????????? NOSOTROS LO CONTROLAMOS EN REG3000 NO HABRIA QUE MANDARLO.
        End If

        If req.ida.oper = 1 Then
            agregar48_cuotas(echoMsg, req.planemisor, req.ida.TKTORI, req.ida.FECORI)
        Else
            If req.esMaestro Then
                agregar48_cuotas(echoMsg, "001")
            Else
                agregar48_cuotas(echoMsg, req.planemisor)
            End If

        End If


        'TODO: en el manual de visa nuevo no esta
        agregar49_moneda(echoMsg, E_Monedas.PESOS)      'req.moneda)

        If req.esMaestro Then agregar52_Pinblock(echoMsg, req.pinBlock)
        If req.esCashback Then agregar54_Cashback(echoMsg, req.ida.CASHBACK)


        'If req.ida.MANUAL = E_ModoIngreso.Chip Then
        'TODO: esto es para chip, ver
        'agregar55_CodSeg(echoMsg, Trim(req.ida.CRIPTO))
        'Else
        If req.obligarCodSeg = True Then
            If req.ida.CODSEG.Trim = "" Then
                req.InvalidarReq("Falta Cod. de Seguridad")
            Else
                agregar55_CodSeg(echoMsg, Trim(req.ida.CODSEG))
            End If
        End If
        'End If

        If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
            agregar59_InfAdicional(echoMsg)
        End If


        If req.ida.MANUAL = E_ModoIngreso.Chip Then
            'TODO: version para chip
            'agregar60_VersionSoft(echoMsg, req.ida.APPVERSION)
            If req.nrohost = TipoHost.POSNET Or req.nrohost = TipoHost.Posnet_homolog Then
                agregar60_VersionSoft(echoMsg, "A0600")
            Else
                agregar60_VersionSoft(echoMsg)
            End If

        Else
            agregar60_VersionSoft(echoMsg)
        End If

        agregar62_NroTicket(echoMsg, req.nroTicket)
        Logger.Info(StringLog)
        Return echoMsg
    End Function

#End Region


    Public Sub Cerrar(req As Req)
        If IsConnected Then
            Dim msg = Generar0500(req)
            Me.MandarCierreyEsperar(req, msg)
        Else
            'Logger.Error(String.Format("No se pudo realizar el cierre de la terminal {0}, Host: {1} DESCONECTADO", Trim(req.terminal), Name))
            CulminacionCierreNOSatisfactoria(req)
        End If
    End Sub

    Sub enviarReverso3DES(req As Req)
        If IsConnected Then
            Dim msj = Me.Generar0400_3DES(req)
            srv.AgregarAPendientes(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)
            Logger.Info(String.Format("Enviando REVERSO {0}/{1}/{2} ...", Me.Name, req.nroTrace, Trim(req.terminal)))
            'DisminuirNroTicket(req.terminal.Trim, req.nrohost)
            If Not Me.MandarMensajeSinEsperar3DES(msj, req) Then
                Logger.Error(String.Format("No se pudo reversar {0}/{1}/{2}", req.nombreHost, req.nroTrace, Trim(req.terminal)))
            End If
        Else
            Logger.Error(String.Format("Host {0} desconectado", Me.Name))
        End If
    End Sub


    Sub enviarAdvice3DES(req As Req)
        If IsConnected Then
            'If req.msjIda.CodAutorizaAdv.Trim = "Y1" Then
            '    Dim msj = Me.Generar0220_Y1_3DES(req)
            'ElseIf req.msjIda.CodAutorizaAdv.Trim = "Z1" Then
            '    Dim msg = Me.Generar0220_Z1_3DES(req)
            'Else
            Dim msj = Me.Generar0220_3DES(req)
            'End If
            srv.AgregarAPendientes(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)
            Logger.Info(String.Format("Enviando ADVICE {0}/{1}/{2} ...", Me.Name, req.nroTrace, Trim(req.terminal)))
            If Not Me.MandarMensajeSinEsperar3DES(msj, req) Then
                Logger.Error(String.Format("No se pudo reversar {0}/{1}/{2}", req.nombreHost, req.nroTrace, Trim(req.terminal)))
            End If
        Else
            Logger.Error(String.Format("Host {0} desconectado", Me.Name))
        End If
    End Sub


    Sub enviarReversoPEI(req As Req)
        If IsConnected Then
            Dim msj = Me.Generar0400PEI(req)
            srv.AgregarAPendientes(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)
            Logger.Info(String.Format("Enviando REVERSO {0}/{1}/{2} ...", Me.Name, req.nroTrace, Trim(req.terminal)))
            'DisminuirNroTicket(req.terminal.Trim, req.nrohost)
            If Not Me.MandarMensajeSinEsperar(msj, req) Then
                Logger.Error(String.Format("No se pudo reversar {0}/{1}/{2}", req.nombreHost, req.nroTrace, Trim(req.terminal)))
            End If
        Else
            Logger.Error(String.Format("Host {0} desconectado", Me.Name))
        End If
    End Sub


    Sub enviarReverso(req As Req)
        If IsConnected Then
            Dim msj = Me.Generar0400(req)
            srv.AgregarAPendientes(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)
            Logger.Info(String.Format("Enviando REVERSO {0}/{1}/{2} ...", Me.Name, req.nroTrace, Trim(req.terminal)))
            'DisminuirNroTicket(req.terminal.Trim, req.nrohost)
            If Not Me.MandarMensajeSinEsperar(msj, req) Then
                Logger.Error(String.Format("No se pudo reversar {0}/{1}/{2}", req.nombreHost, req.nroTrace, Trim(req.terminal)))
            End If
        Else
            Logger.Error(String.Format("Host {0} desconectado", Me.Name))
        End If
    End Sub

    Sub enviaryesperar(req As Req)
        horaEnvio = Now
        If IsConnected And respuesta_echo Then
            Logger.Info(String.Format("Host {0} conectado", Me.Name))
            Select Case req.operacion
                Case E_ProcCode.Compra, E_ProcCode.Devolucion, E_ProcCode.AnulacionCompra, E_ProcCode.AnulacionDevolucion,
                     E_ProcCode.Compra_maestro_CajAhorroD, E_ProcCode.Compra_maestro_CajAhorroP, E_ProcCode.Compra_maestro_CtaCteD,
                     E_ProcCode.Compra_maestro_CtaCteP, E_ProcCode.compra_cashback,
                     E_ProcCode.Devolucion_Compra_maestro_CajAhorroD,
                     E_ProcCode.Devolucion_Compra_maestro_CajAhorroP,
                     E_ProcCode.Devolucion_Compra_maestro_CtaCteD,
                     E_ProcCode.Devolucion_Compra_maestro_CtaCteP,
                     E_ProcCode.Anulacion_Compra_maestro_CajAhorroD,
                     E_ProcCode.Anulacion_Compra_maestro_CajAhorroP,
                     E_ProcCode.Anulacion_Compra_maestro_CtaCteD,
                     E_ProcCode.Anulacion_Compra_maestro_CtaCteP,
                     E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroD,
                     E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroP,
                     E_ProcCode.Anulacion_Compra_maestro_CtaCteD,
                     E_ProcCode.Anulacion_Compra_maestro_CtaCteP,
                     E_ProcCode.anulacion_compra_cashback
                    Dim msj = Me.Generar0200(req)
                    If req.RespInvalida = False Then


                        srv.AgregarAPendientes(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)

                        If Not Me.MandarMensajeSinEsperar(msj, req) Then

                            srv.SacarPendiente(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)

                            Logger.Error(String.Format("Host {0} desconectado", Me.Name))
                            req.InvalidarReq("INVALIDA: Error en envío, reintente mas tarde.")

                            srv.ResponderInvalidaACaja(req)
                        End If
                    Else
                        srv.ResponderInvalidaACaja(req)
                    End If
                Case Else
                    req.InvalidarReq("Cod. Procesamiento erróneo.")
                    srv.ResponderInvalidaACaja(req)
            End Select
        Else
            If Not respuesta_echo Then
                Logger.Error(String.Format("Host {0} no responde", Me.Name))
                req.InvalidarReq("INVALIDA: HOST no responde reintente mas tarde.")
                srv.ResponderInvalidaACaja(req)
            Else
                Logger.Error(String.Format("Host {0} desconectado", Me.Name))
                req.InvalidarReq("INVALIDA: HOST desconectado, reintente mas tarde.")
                srv.ResponderInvalidaACaja(req)
            End If
        End If
    End Sub


    Sub enviar_sincro(req As Req)
        horaEnvio = Now

        Logger.Info(String.Format("Host {0} conectado", Me.Name))

        Dim msj = Me.Generar0800_Sincronizacion(req)

        srv.AgregarAPendientes(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)

        If Not Me.Mandar_Sincronizacion_SinEsperar(msj) Then

            srv.SacarPendiente(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)

            Logger.Error(String.Format("Host {0} desconectado", Me.Name))
            req.InvalidarReq("INVALIDA: Error en envío, reintente mas tarde.")

            srv.ResponderInvalidaACaja(req)
        End If



    End Sub


    Sub enviar3DES(req As Req)
        horaEnvio = Now
        If IsConnected And respuesta_echo Then
            Logger.Info(String.Format("Host {0} conectado", Me.Name))
            Select Case req.msjIda.tipoMensaje
                Case TransmisorTCP.TipoMensaje.Compra, TransmisorTCP.TipoMensaje.AnulacionDevolucion, TransmisorTCP.TipoMensaje.Anulacion,
                     TransmisorTCP.TipoMensaje.Devolucion, TransmisorTCP.TipoMensaje.CompraCashback
                    Dim msj = Me.Generar0200_3DES(req)
                    If req.RespInvalida = False Then
                        '--------------------------------------------------------------
                        '--- LO AGREGA EN LA COLA ANTES DE MANDARLO A FIST DATA        
                        '--------------------------------------------------------------
                        srv.AgregarAPendientes(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)

                        '----------------------------------
                        '--- LO MANDA A FIRST DATA         
                        '----------------------------------
                        If Not Me.MandarMensajeSinEsperar3DES(msj, req) Then
                            '----------------------------------------------------------------
                            '--- SI DA NO APROBADO LO SACA DE LA COLA DE PENDIENTES          
                            '----------------------------------------------------------------
                            srv.SacarPendiente(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)

                            Logger.Error(String.Format("Host {0} desconectado", Me.Name))
                            req.InvalidarReq("INVALIDA: Error en envío, reintente mas tarde.")

                            srv.ResponderInvalidaACajaTCP(req)
                        End If
                    Else
                        srv.ResponderInvalidaACajaTCP(req)
                    End If

                Case TransmisorTCP.TipoMensaje.Sincronizacion
                    If req.RespInvalida = False Then
                        Dim msj = Me.Generar0800_Sincronizacion(req)

                        srv.AgregarAPendientes(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)

                        If Not Me.MandarMensajeSinEsperar3DES(msj, req) Then

                            srv.SacarPendiente(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)

                            Logger.Error(String.Format("Host {0} desconectado", Me.Name))
                            req.InvalidarReq("INVALIDA: Error en envío, reintente mas tarde.")

                            srv.ResponderInvalidaACajaTCP(req)
                        End If
                    Else
                        srv.ResponderInvalidaACajaTCP(req)
                    End If
                Case Else
                    req.InvalidarReq("Cod. Procesamiento erróneo.")
                    srv.ResponderInvalidaACajaTCP(req)
            End Select

        Else

            If Not respuesta_echo Then
                Logger.Error(String.Format("Host {0} no responde", Me.Name))
                req.InvalidarReq("INVALIDA: HOST no responde reintente mas tarde.")
                srv.ResponderInvalidaACajaTCP(req)
            Else
                Logger.Error(String.Format("Host {0} desconectado", Me.Name))
                req.InvalidarReq("INVALIDA: HOST desconectado, reintente mas tarde.")
                srv.ResponderInvalidaACajaTCP(req)
            End If
        End If
    End Sub



    Sub enviar(req As Req)
        horaEnvio = Now
        If IsConnected And respuesta_echo Then
            Logger.Info(String.Format("Host {0} conectado", Me.Name))
            Select Case req.operacion
                Case E_ProcCode.Compra, E_ProcCode.Devolucion, E_ProcCode.AnulacionCompra, E_ProcCode.AnulacionDevolucion,
                     E_ProcCode.Compra_maestro_CajAhorroD, E_ProcCode.Compra_maestro_CajAhorroP, E_ProcCode.Compra_maestro_CtaCteD,
                     E_ProcCode.Compra_maestro_CtaCteP, E_ProcCode.compra_cashback,
                     E_ProcCode.Devolucion_Compra_maestro_CajAhorroD,
                     E_ProcCode.Devolucion_Compra_maestro_CajAhorroP,
                     E_ProcCode.Devolucion_Compra_maestro_CtaCteD,
                     E_ProcCode.Devolucion_Compra_maestro_CtaCteP,
                     E_ProcCode.Anulacion_Compra_maestro_CajAhorroD,
                     E_ProcCode.Anulacion_Compra_maestro_CajAhorroP,
                     E_ProcCode.Anulacion_Compra_maestro_CtaCteD,
                     E_ProcCode.Anulacion_Compra_maestro_CtaCteP,
                     E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroD,
                     E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroP,
                     E_ProcCode.Anulacion_Compra_maestro_CtaCteD,
                     E_ProcCode.Anulacion_Compra_maestro_CtaCteP,
                     E_ProcCode.anulacion_compra_cashback
                    Dim msj = Me.Generar0200(req)
                    If req.RespInvalida = False Then

                        srv.AgregarAPendientes(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)

                        If Not Me.MandarMensajeSinEsperar(msj, req) Then

                            srv.SacarPendiente(ServerTar.ClaveIDA(Me, Trim(req.terminal), msj), req)

                            Logger.Error(String.Format("Host {0} desconectado", Me.Name))
                            req.InvalidarReq("INVALIDA: Error en envío, reintente mas tarde.")

                            srv.ResponderInvalidaACaja(req)
                        End If
                    Else
                        srv.ResponderInvalidaACaja(req)
                    End If
                Case Else
                    req.InvalidarReq("Cod. Procesamiento erróneo.")
                    srv.ResponderInvalidaACaja(req)
            End Select

        Else

            If Not respuesta_echo Then
                Logger.Error(String.Format("Host {0} no responde", Me.Name))
                req.InvalidarReq("INVALIDA: HOST no responde reintente mas tarde.")
                srv.ResponderInvalidaACaja(req)
            Else
                Logger.Error(String.Format("Host {0} desconectado", Me.Name))
                req.InvalidarReq("INVALIDA: HOST desconectado, reintente mas tarde.")
                srv.ResponderInvalidaACaja(req)
            End If
        End If
    End Sub


    Sub enviar_PEI(req As Req)
        horaEnvio = Now

        srv.AgregarAPendientes(ServerTar.ClaveIDA(Me.Name, Trim(req.terminal), req.nroTrace), req)

        If Not Me.MandarMensajePEI(req) Then
            srv.SacarPendiente(ServerTar.ClaveIDA(Me.Name, Trim(req.terminal), req.nroTrace), req)

            Logger.Error(String.Format("Host {0} desconectado", Me.Name))
            req.InvalidarReq("INVALIDA: Error en envío PEI, reintente mas tarde.")

            srv.ResponderInvalidaACaja(req)
        End If
    End Sub

    Private Function MandarMensajePEI(req As Req) As Boolean
        Try
            AgregarMovimientoPEI(req)

            Dim starinfo As New ProcessStartInfo
            starinfo.FileName = Configuracion.DirLocal & "\AgentePEI\AgentePEI.exe"     '--- DirLocal es c:\tarjetas  
            starinfo.UseShellExecute = False
            starinfo.WindowStyle = ProcessWindowStyle.Hidden
            starinfo.CreateNoWindow = True

            If req.OperacionPEI = lib_PEI.Concepto.COMPRA_DE_BIENES.ToString Then
                starinfo.Arguments = "c@" & Me.GenerarMensajePEI(req).ToString & "@" & req.terminal & "@" & Me.direccion & "@" & req.idComercio & "@" & req.nroTrace & "@" & req.pinguino & "@" & req.nroCaja.ToString("00")
            ElseIf req.OperacionPEI = lib_PEI.Concepto.DEVOLUCION.ToString Then
                If req.ida.oper = 4 Then 'importe = 0 Then 'DEVOLUCION TOTAL, LO HICE ASI PORQUE HAY QUE MANDAR IMPORTE 0.
                    starinfo.Arguments = "dt@" & Me.GenerarMensajePEIDevTotal(req, "").ToString & "@" & req.terminal & "@" & Me.direccion & "@" & req.idComercio & "@" & req.nroTrace
                Else
                    starinfo.Arguments = "dp@" & Me.GenerarMensajePEIDevParcial(req).ToString & "@" & req.terminal & "@" & Me.direccion & "@" & req.idComercio & "@" & req.nroTrace
                End If
            End If

            If starinfo.Arguments <> "" Then
                System.Diagnostics.Process.Start(starinfo)
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function GenerarMensajePEIDevParcial(req As Req) As lib_PEI.Operacion_Devolucion_Parcial
        Dim trx As New lib_PEI.Operacion_Devolucion_Parcial With {
                .codigoSeguridad = req.ida.CODSEG.Trim,
                .track1 = req.ida.TRACK1.Trim,
                .track2 = req.ida.TRACK2.Trim,
                .ultimos = req.ida.TARJ.Substring(req.ida.TARJ.Trim.Length - 4).Trim,
                .Documentotitular = req.ida.documento.Trim,
                .idTerminal = req.terminal,                               '"PING01", 'buscarTerPEI(nrocaja), 
                .idReferenciaOperacionComercio = "DEVCORMORAN",
                .traceTrxComercio = req.nroTracePEI,                      'req.nroCaja.ToString.Substring(0, 1) + "_" + req.nroCaja.ToString.Substring(1) + "_" + Format(Now, "yyyyMMdd") + "_" + Format(Now, "hhmmss"),
                .idCanal = "PEIBANDA",
                .idComercio = req.idComercio,                             '"385",
                .tipoOperacion = lib_PEI.Concepto.DEVOLUCION,
                .idPagoOriginal = req.ida.TKTORI,
                .importe = CDec(req.ida.IMPO) * 100,
                .moneda = lib_PEI.Moneda.ARS
        }

        If req.emisor = 6 Then trx.codigoSeguridad = ""
        Return trx
    End Function

    Private Function GenerarMensajePEIDevTotal(req As Req, NroIdOperacionOriginalCompra As String) As lib_PEI.Operacion_Devolucion
        Dim trx As New lib_PEI.Operacion_Devolucion With {
                .codigoSeguridad = req.ida.CODSEG.Trim,
                .track1 = req.ida.TRACK1.Trim,
                .track2 = req.ida.TRACK2.Trim,
                .ultimos = req.ida.TARJ.Substring(req.ida.TARJ.Trim.Length - 4).Trim,
                .Documentotitular = req.ida.documento.Trim,
                .idTerminal = req.terminal,                             '"PING01", 'buscarTerPEI(nrocaja), 
                .idReferenciaOperacionComercio = "DEVCORMORAN",
                .traceTrxComercio = req.nroTracePEI,                    'req.nroCaja.ToString.Substring(0, 1) + "_" + req.nroCaja.ToString.Substring(1) + "_" + Format(Now, "yyyyMMdd") + "_" + Format(Now, "hhmmss"),
                .idCanal = "PEIBANDA",
                .idComercio = req.idComercio,                           '"385",
                .tipoOperacion = lib_PEI.Concepto.DEVOLUCION,
                .idPagoOriginal = NroIdOperacionOriginalCompra          '  req.ida.TKTORI
        }

        If req.emisor = 6 Then trx.codigoSeguridad = ""
        Return trx
    End Function

    Private Function GenerarMensajePEI(req As Req) As lib_PEI.Operacion_Compra
        Dim trx As New lib_PEI.Operacion_Compra With {
                .importe = CDec(req.ida.IMPO) * 100,
                .codigoSeguridad = req.ida.CODSEG.Trim,
                .moneda = lib_PEI.Moneda.ARS,
                .track1 = req.ida.TRACK1.Trim,
                .track2 = req.ida.TRACK2.Trim,
                .ultimos = req.ida.TARJ.Substring(req.ida.TARJ.Trim.Length - 4).Trim,
                .Documentotitular = req.ida.documento.Trim,
                .idTerminal = req.terminal,  '"PING01", 'buscarTerPEI(nrocaja), 
                .idReferenciaOperacionComercio = "PAGOCORMORAN",
                .traceTrxComercio = req.nroTracePEI, 'req.nroTrace, 'req.nroCaja.ToString.Substring(0, 1) + "_" + req.nroCaja.ToString.Substring(1) + "_" + Format(Now, "yyyyMMdd") + "_" + Format(Now, "hhmmss"),
                .idCanal = "PEIBANDA",
                .idComercio = req.idComercio, '"385",
                .tipoOperacion = lib_PEI.Concepto.COMPRA_DE_BIENES
        }

        If req.emisor = 6 Then trx.codigoSeguridad = ""
        Return trx
    End Function

    Public Sub Desconectar()
        '        If IsConnected Then
        Close()

        'End If
    End Sub


    Sub SendtestVenta(req As Req)
        If IsConnected Then
            Logger.Info(String.Format("Host {0} conectado", Me.Name))
            Dim msj = Me.Generar0200(req)

            Me.MandarMensajeyEsperar(msj)
            'Debug.Assert(req.vta.VtaOk = 0, "No me dio el OK")
        Else
            Logger.Info(String.Format("Host {0} desconectado", Me.Name))
        End If

    End Sub

    Sub Sincronizar(host As String)
        'Me.Mandar_Sincronizacion_SinEsperar(Me.Generar0800_Sincronizacion(TipoHost.Posnet_homolog))
    End Sub

    'Dim homologacion As Boolean = True
    Sub SendEchoTest()
        horaEnvio = Now
        If Me.Name.Contains("PEI") Then Exit Sub
        If IsConnected Then
            Logger.Info(String.Format("Host {0} conectado", Me.Name))
            'Dim msj As Iso8583messagePOS
            Logger.Info(String.Format("Enviando ECHO a host {0}", Me.Name))
            'If Me.Name = TipoHost.VISA.ToString Then
            If Me.Name = TipoHost.VISA.ToString Then
                Me.Mandar_Echo_Esperar(Me.Generar0800(TipoHost.VISA))
                'msj = Me.Generar0800(TipoHost.VISA)
            ElseIf Me.Name = TipoHost.Visa_homolog.ToString Then
                Me.Mandar_Echo_Esperar(Me.Generar0800(TipoHost.Visa_homolog))
                'msj = Me.Generar0800(TipoHost.Visa_homolog)
            ElseIf Me.Name = TipoHost.POSNET.ToString Then
                Me.Mandar_Echo_Esperar(Me.Generar0800(TipoHost.POSNET))
                'msj = Me.Generar0800(TipoHost.POSNET)
            ElseIf Me.Name = TipoHost.Posnet_homolog.ToString Then
                Me.Mandar_Echo_Esperar(Me.Generar0800(TipoHost.Posnet_homolog))
                'msj = Me.Generar0800(TipoHost.Posnet_homolog)
            Else
                Logger.Warn(String.Format("No se envió echotest, Host {0} DESCONOCIDO", Me.Name))
            End If
            'Me.Mandar_Echo_Esperar(msj)
        Else
            Logger.Warn(String.Format("Host {0} desconectado", Me.Name))
        End If
    End Sub

    Public Shared Function StrToByteArray(ByVal str As String) As Byte()
        Dim encoding As New System.Text.ASCIIEncoding()
        Return encoding.GetBytes(str)
    End Function 'StrToByteArray



    Private Sub MandarCierreyEsperar(req As Req, msg As Iso8583messagePOS)
        horaEnvio = Now
        Dim m As Iso8583Message
        Dim request As PeerRequestPOS
        request = New PeerRequestPOS(Me, msg)

        Logger.Info("Antes de enviar")
        request.Send()
        request.WaitResponse(timeOutCierre)
        Logger.Info("Despues de enviar")

        m = request.ResponseMessage
        If m IsNot Nothing Then
            srv.ProcesarCierre(Me, m, req)
        Else
            Logger.Warn(String.Format("Dio timeout. No se realizó el Cierre de la terminal {0}/{1}", req.nombreHost, Trim(req.terminal)))
            CulminacionCierreNOSatisfactoria(req)
        End If
    End Sub

    Public Function Mandar_Sincronizacion_SinEsperar(ByVal msg As Iso8583messagePOS) As Boolean
        Dim request As PeerRequestPOS
        request = New PeerRequestPOS(Me, msg)
        Try
            Logger.Info("Solicitando Sincronizacion ...")
            request.Send()
        Catch ex As Exception
            Logger.Error("No se envió el mensaje. " + vbNewLine + ex.Message)
            Return False
        End Try
        Return True ' si se mando
    End Function


    Public Sub Mandar_Echo_Esperar(ByVal msg As Iso8583messagePOS)
        Dim m As Iso8583Message
        Dim request As PeerRequestPOS
        request = New PeerRequestPOS(Me, msg)
        Logger.Info("Enviando echo...")
        Try
            request.Send()
            request.WaitResponse(timeOutAut)
            Logger.Info("Fin de envío echo...")
        Catch ex As Exception
            Logger.Error("No se pudo enviar echo. Verificar.")
        End Try
        m = request.ResponseMessage
        If m IsNot Nothing Then
            tiempoEnvio = 10
            respuesta_echo = True
            srv.ProcesarMensajeEntrante(Me, m)
        Else
            respuesta_echo = False
            tiempoEnvio = 1
            Logger.Warn(String.Format("El host {0} no respondió al EchoTest.", Me.Name))
        End If
    End Sub

    Public Sub MandarMensajeyEsperar(ByVal msg As Iso8583messagePOS)
        'Logger.Info("Enviando " + msg.ToString)
        Dim m As Iso8583Message
        Dim request As PeerRequestPOS
        request = New PeerRequestPOS(Me, msg)

        Logger.Info("Antes de enviar")
        request.Send()
        request.WaitResponse(timeOutAut)
        Logger.Info("Despues de enviar")

        m = request.ResponseMessage
        If m IsNot Nothing Then
            srv.ProcesarMensajeEntrante(Me, m)
        Else
            Logger.Info("Dio timeout")
        End If
    End Sub

    Public Function MandarMensajeSinEsperar3DES(ByVal msg As Iso8583messagePOS, req As Req) As Boolean
        Dim request As PeerRequestPOS
        request = New PeerRequestPOS(Me, msg)
        Try
            If req.msjIda.tipoMensaje <> TransmisorTCP.TipoMensaje.Sincronizacion Then
                AgregarMovimiento3DES(req)
            End If
            request.Send()   '--- ACA MANDA A FIRST DATA
        Catch ex As Exception
            Logger.Error("No se envió el mensaje. " + vbNewLine + ex.Message)
            Return False
        End Try
        Return True ' si se mando
    End Function

    Public Function MandarMensajeSinEsperar(ByVal msg As Iso8583messagePOS, req As Req) As Boolean
        Dim request As PeerRequestPOS
        request = New PeerRequestPOS(Me, msg)
        Try
            AgregarMovimiento(req)
            request.Send()
        Catch ex As Exception
            Logger.Error("No se envió el mensaje. " + vbNewLine + ex.Message)
            Return False
        End Try
        Return True      '--- si se mando
    End Function


    Private Sub HostTCP_RequestDone(sender As Object, e As PeerRequestDoneEventArgs) Handles Me.RequestDone
        '--------------------------------------------------------
        '--- ACA SE RECIBE TODO LO QUE RESPONDE EL HOST          
        '--------------------------------------------------------
        Logger.Info("Llegó un mensaje " + CType(e.Request.ResponseMessage, Iso8583Message).MessageTypeIdentifier.ToString)
        If CType(e.Request.ResponseMessage, Iso8583Message).MessageTypeIdentifier <> 510 And CType(e.Request.ResponseMessage, Iso8583Message).MessageTypeIdentifier <> 810 Then
            'srv.ProcesarMensajeEntrante(sender, e.Request.ResponseMessage)
            srv.RecuperarRequerimiento(sender, e.Request.ResponseMessage)
        Else
            'else agregado para lo del 3DES
            '-----------------------------------------------
            '--- 800 ES SINCRONIZAR ó ECO SI NO SINCRONIZA  
            '--- 500 ES CIERRE                              
            '-----------------------------------------------
            If CType(e.Request.ResponseMessage, Iso8583Message).MessageTypeIdentifier = 810 And e.Request.ResponseMessage.Fields(3).ToString = CStr(E_ProcCode.Sincronizacion) Then
                srv.RecuperarRequerimiento(sender, e.Request.ResponseMessage)
            End If

        End If

    End Sub

    Private Sub Controlar_Desconexion() Handles Me.Disconnected
        Try
            Logger.Info(String.Format("Host {0} desconectado", Me.Name))
            srv.Desconexion(Me.Name)
        Catch
            Logger.Warn("No se puede borrar archivo de estado de conexión.")
        End Try
    End Sub
    Private Sub Controlar_Conexion() Handles Me.Connected
        Try
            srv.Conexion(Me.Name)
        Catch
            Logger.Warn("No se puede crear archivo de estado de conexión.")
        End Try
    End Sub
End Class
