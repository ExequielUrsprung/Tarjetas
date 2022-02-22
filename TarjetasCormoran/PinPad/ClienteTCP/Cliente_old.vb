Imports lib_PP
Imports TransmisorTCP
Imports TjComun
Imports ClienteTCP

Public Enum ResponseDescription
    APROBADA
    NO_APROBADA
    NO_HAY_RESPUESTA
    TIMEOUT_PP
    ERROR_EN_PP
    COMANDO_CANCELADO
    SINCRONIZADO_OK
    NO_SINCRONIZADO
    INICIALIZADO_OK
    NO_INICIALIZADO
    VERSION_NO_SETEADA
End Enum

Public Class Cliente
    Const TIMEOUTREQUEST As Integer = 30           '30
    Public Event Mensaje(msj As MensajeCliente)


    Dim continuar As Boolean

    Dim versionAPP As String = ""
    'Dim puedeOperar As Boolean = False
    Dim sincronizado As Boolean = False
    Dim inicializado As Boolean = False

    Dim Homologacion As Boolean = False

    Dim encripta As Boolean = True
    Dim pinguino As String
    Dim caja As String
    Dim cajera As Integer
    Dim puerto As Integer
    Dim dirip As String
    Dim WithEvents transmisor As TransmisorTCP.ClaseClienteSocket

    Sub New(pEncripta As Boolean, pPinguino As String, pCaja As Integer, pCajera As Integer, pPuerto As Integer, pIP As String)
        encripta = pEncripta
        pinguino = pPinguino
        caja = pCaja
        cajera = pCajera
        puerto = pPuerto
        dirip = pIP
        transmisor = New TransmisorTCP.ClaseClienteSocket(puerto, dirip)
    End Sub

    Public Sub Msg(s As String)
        Dim msj As New MensajeCliente() With {.Fecha = Now, .Texto = s}
        msj.Tipo = TipoMensaje.Informacion
        RaiseEvent Mensaje(msj)
    End Sub

    Public Sub Progreso(s As String)
        Dim msj As New MensajeCliente() With {.Fecha = Now, .Texto = s}
        msj.Tipo = TipoMensaje.Progress
        RaiseEvent Mensaje(msj)
    End Sub

    Public Sub MsgError(s As String)
        Dim msj As New MensajeCliente() With {.Fecha = Now, .Texto = s}
        msj.Tipo = TipoMensaje.Error
        RaiseEvent Mensaje(msj)
    End Sub

    Public Structure ResponseToPOS
        Dim Response As ResponseDescription
        Dim TicketString As String
        Dim DisplayString As String
        Dim TotalAmount As Decimal
        Dim CashbackAmount As Decimal
        Dim Emisor As String
        Dim CodigoAutorizacion As String
    End Structure

#Region "Compra"


    'Public Function Compra(pcuotas As Integer, pimporte As Decimal, pcashback As Decimal, pticket As Integer) As ResponseToPOS
    '    If encripta Then
    '        Return Compra(pcuotas, pimporte, pcashback, pticket)
    '    Else
    '        Return Compra_SinEncripcion(pcuotas, pimporte, pcashback, pinguino, caja, pticket, cajera)
    '    End If
    'End Function

    Private Function Compra_SinEncripcion(pCuotas As Integer, pImporte As Decimal, pCashback As Decimal, pinguino As String, caja As String, ticketcaja As Integer, cajera As Integer) As ResponseToPOS
        Dim Respuesta As New ResponseToPOS


        Using conector As New lib_PP.ConectorPPSinEncripcion
            Dim conCashback As Boolean
            If pCashback > 0 Then
                conCashback = True
            Else
                conCashback = False
            End If
            '********** INICIO DE COMPRA - DESPIERTA EL PINPAD *****************
            If conector.Inicio_Compra(conCashback) = 0 Then

                If conector.RespuestaInicioCompra.modoIngreso = "B" Or conector.RespuestaInicioCompra.modoIngreso = "M" Then

                    Msg("-------- Respuesta a Y01 ----------")
                    Msg("Modo Ing                      : " & conector.RespuestaInicioCompra.modoIngreso)
                    'Msg("Nro Tarjeta                   : " & conector.RespuestaInicioCompra.tarjeta)
                    Dim rango As String = conector.RespuestaInicioCompra.tarjeta
                    Dim tipoTarjeta As Integer = rango.Substring(0, 1)
                    Dim CodigoServicioPIN As Integer = conector.RespuestaInicioCompra.codigoServicio
                    Msg("Cod. Serv.                    : " & conector.RespuestaInicioCompra.codigoServicio)
                    Msg("Cod. Banco                    : " & conector.RespuestaInicioCompra.codigoBanco)
                    Msg("Nro de registro               : " & conector.RespuestaInicioCompra.nroRegistro)
                    Msg("Abreviatura Tarjeta           : " & conector.RespuestaInicioCompra.abrebiaturaTarj)
                    Msg("Nombre Aplicacion CHIP        : " & conector.RespuestaInicioCompra.appChip)


                    '********** CONSULTA A SP QUE DATOS PEDIR EN EL COMANDO Y02 *****************
                    Dim mensajeRespuesta = Consultar_Datos_Adicionales(rango, conector.RespuestaInicioCompra.modoIngreso)

                    If mensajeRespuesta IsNot Nothing Then
                        '********** SOLICITA DATOS ADICIONALES *****************
                        If conector.Datos_adicionales(conector.RespuestaInicioCompra.modoIngreso,
                                                          mensajeRespuesta.u4d,
                                                          mensajeRespuesta.cds,
                                                          mensajeRespuesta.spi,
                                                          mensajeRespuesta.Clave_wkp,
                                                          pImporte,
                                                          pCashback) = 0 Then


                            Select Case conector.RespuestaInicioCompra.modoIngreso
                                Case "M"
                                    Datos_Adicionales_Manual(conector)
                                Case "B"
                                    Datos_Adicionales_Banda(conector)
                                Case Else
                                    Msg("Modo de ingreso no contemplado.")
                            End Select

                            '********** ENVIA TRANSACCION A SP *****************
                            ' mensajeRespuesta = Enviar_Transaccion(conector, pinguino, caja, pCuotas, pImporte, pCashback, ticketcaja, cajera, mensajeRespuesta.Clave_wkp, TransmisorTCP.TipoMensaje.Compra)
                            If mensajeRespuesta IsNot Nothing Then
                                Msg("-------------------------------")
                                Msg("     RESPUESTA DEL SERVIDOR")
                                Msg("-------------------------------")
                                Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)

                                Select Case conector.RespuestaInicioCompra.modoIngreso
                                    Case "M", "B"
                                        If mensajeRespuesta.codRespuesta = "00" Then
                                            Respuesta.Response = ResponseDescription.APROBADA
                                            Respuesta.TotalAmount = mensajeRespuesta.Importe
                                        Else
                                            Respuesta.Response = ResponseDescription.NO_APROBADA
                                        End If
                                        Respuesta.Emisor = mensajeRespuesta.Emisor
                                        Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                        Respuesta.TicketString = mensajeRespuesta.cupon

                                        'Return mensajeRespuesta.codRespuesta
                                        'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR

                                    Case Else
                                        Msg("Modo de ingreso no contemplado.")
                                End Select
                            Else
                                '---NO HUBO RESPUESTA

                                'TODO: Aca habria que reversar. o ver que hacer.
                                Select Case conector.RespuestaInicioCompra.modoIngreso

                                    Case "M", "B"
                                        'TODO: Aca habria que reversar. o ver que hacer.
                                        'Return Nothing


                                    Case Else
                                        Msg("Modo de ingreso no contemplado.")
                                End Select
                                Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                                Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."

                            End If

                        Else
                            'TODO: Error no continua con la transaccion
                            mostrar_error(Respuesta, conector)

                        End If

                    Else
                        'TODO: No hubo respuesta del servidor.
                        Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                        Respuesta.DisplayString = "El SP no respondió a la consulta de datos adicionales."

                    End If
                Else
                    conector.Cancelar()
                    Respuesta.Response = ResponseDescription.COMANDO_CANCELADO
                    Respuesta.DisplayString = "Modo de ingreso no soportado. (CHIP)"
                End If
            Else
                mostrar_error(Respuesta, conector)

            End If

        End Using

        Return Respuesta

    End Function

    Public Function Compra(pCuotas As Integer, pImporte As Decimal, pCashback As Decimal, ticketcaja As Integer) As ResponseToPOS
        Dim Respuesta As New ResponseToPOS
        Dim Reverso As New TransmisorTCP.MensajeIda
        If inicializado And sincronizado Then
            Using conector As New lib_PP.ConectorPP()
                Dim conCashback As Boolean
                If pCashback > 0 Then
                    conCashback = True
                Else
                    conCashback = False
                End If
                '********** INICIO DE COMPRA - DESPIERTA EL PINPAD *****************
                If conector.Inicio_Compra(conCashback) = 0 Then

                    Msg("-------- Respuesta a Y01 ----------")
                    Msg("Modo Ing                      : " & conector.RespuestaInicioCompra.modoIngreso)
                    'Msg("Nro Tarjeta                   : " & conector.RespuestaInicioCompra.tarjeta)
                    Dim rango As String = conector.RespuestaInicioCompra.tarjeta
                    Dim tipoTarjeta As Integer = rango.Substring(0, 1)


                    'MsgBox(conector.RespuestaInicioCompra.codigoServicio)

                    Dim CodigoServicioPIN As Integer = conector.RespuestaInicioCompra.codigoServicio
                    'If Trim(conector.RespuestaInicioCompra.codigoServicio) <> "" Then
                    '    CodigoServicioPIN = conector.RespuestaInicioCompra.codigoServicio
                    'End If

                    Msg("Cod. Serv.                    : " & conector.RespuestaInicioCompra.codigoServicio)
                    Msg("Cod. Banco                    : " & conector.RespuestaInicioCompra.codigoBanco)
                    Msg("Nro de registro               : " & conector.RespuestaInicioCompra.nroRegistro)
                    Msg("Abreviatura Tarjeta           : " & conector.RespuestaInicioCompra.abrebiaturaTarj)
                    Msg("Nombre Aplicacion CHIP        : " & conector.RespuestaInicioCompra.appChip)

                    '********** CONSULTA A SP QUE DATOS PEDIR EN EL COMANDO Y02 *****************
                    Dim mensajeRespuesta = Consultar_Datos_Adicionales(rango, conector.RespuestaInicioCompra.modoIngreso)

                    If mensajeRespuesta IsNot Nothing Then
                        '********** SOLICITA DATOS ADICIONALES *****************
                        If conector.Datos_adicionales(conector.RespuestaInicioCompra.modoIngreso,
                                                      mensajeRespuesta.u4d,
                                                      mensajeRespuesta.cds,
                                                      mensajeRespuesta.spi,
                                                      mensajeRespuesta.Clave_wkd,
                                                      mensajeRespuesta.Clave_wkp,
                                                      mensajeRespuesta.PosicionMK,
                                                      pImporte,
                                                      pCashback,
                                                      tipoTarjeta,
                                                      CodigoServicioPIN) = 0 Then

                            '******** Muestra mensajes segun modo ingreso *********
                            Select Case conector.RespuestaInicioCompra.modoIngreso
                                Case "M"
                                    Datos_Adicionales_Manual(conector)
                                Case "B"
                                    Datos_Adicionales_Banda(conector)
                                Case "C"
                                    Datos_Adicionales_Chip(conector)
                                Case Else
                                    Msg("Modo de ingreso no contemplado.")
                            End Select

                            '********** ENVIA TRANSACCION A SP *****************
                            If pCashback > 0 Then
                                mensajeRespuesta = Enviar_Transaccion(conector, pCuotas, pImporte, pCashback, ticketcaja, mensajeRespuesta.Clave_wkp, TransmisorTCP.TipoMensaje.CompraCashback, Reverso, "")
                            Else
                                mensajeRespuesta = Enviar_Transaccion(conector, pCuotas, pImporte, pCashback, ticketcaja, mensajeRespuesta.Clave_wkp, TransmisorTCP.TipoMensaje.Compra, Reverso, "")
                            End If

                            If mensajeRespuesta IsNot Nothing Then
                                Msg("-------------------------------")
                                Msg("     RESPUESTA DEL SERVIDOR")
                                Msg("-------------------------------")
                                Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)

                                Select Case conector.RespuestaInicioCompra.modoIngreso
                                    Case "M", "B"
                                        If mensajeRespuesta.codRespuesta = "00" Then
                                            Respuesta.Response = ResponseDescription.APROBADA
                                            Respuesta.TotalAmount = mensajeRespuesta.Importe
                                        Else

                                            If mensajeRespuesta.codRespuesta = "55" Then
                                                'TODO: REINGRESO DE PIN
                                            End If


                                            Respuesta.Response = ResponseDescription.NO_APROBADA




                                        End If
                                        Respuesta.Emisor = mensajeRespuesta.Emisor
                                        Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                        Respuesta.TicketString = mensajeRespuesta.cupon

                                        'Return mensajeRespuesta.codRespuesta
                                        'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                                    Case "C"
                                        If conector.Responder_Chip(mensajeRespuesta.Autorizacion, mensajeRespuesta.codRespuesta, mensajeRespuesta.criptograma, True) = 0 Then
                                            Msg("-------- Respuesta a                                                                                                                                                                                                                                                                                              ----------")
                                            Msg("Autorizacion                      : " & conector.RespuestaChip.autorizacion)
                                            Msg("Codigo de respuesta               : " & conector.RespuestaChip.codrespuesta)
                                            Msg("Respuesta                         : " & conector.RespuestaChip.mensajeRepuesta)
                                            Msg("Criptograma                       : " & conector.RespuestaChip.criptograma)


                                            If mensajeRespuesta.codRespuesta = ResponseDescription.APROBADA Then

                                                If conector.RespuestaChip.codrespuesta = "00" Then
                                                    Respuesta.Response = ResponseDescription.APROBADA
                                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                    'Respuesta.TicketString = mensajeRespuesta.cupon
                                                    Respuesta.Emisor = mensajeRespuesta.Emisor
                                                Else


                                                    If mensajeRespuesta.codRespuesta = "55" Then
                                                        'TODO: REINGRESO DE PIN
                                                    End If


                                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                                    '                                                    If mensajeRespuesta.codRespuesta = "00" Then 'si respuesta host aprobada y denegada pp
                                                    Respuesta.DisplayString = String.Format("Denegada por PP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)
                                                    Reversar(Reverso)
                                                    '                                                   Else
                                                    'Respuesta.DisplayString = String.Format("Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)
                                                    '                                                  End If
                                                End If
                                                'Respuesta.TicketString = mensajeRespuesta.cupon

                                            Else
                                                If mensajeRespuesta.codRespuesta = "12" Then
                                                    Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA

                                                Else

                                                    Respuesta.Response = ResponseDescription.NO_APROBADA

                                                    'Respuesta.DisplayString = String.Format("Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)
                                                End If
                                                Respuesta.DisplayString = mensajeRespuesta.mensajeHost

                                            End If

                                            Respuesta.TicketString = mensajeRespuesta.cupon

                                            If mensajeRespuesta.codRespuesta <> "12" Then ' 12 es respuesta invalida
                                                'ENVIA LA RESPUESTA DEL CHIP AL SP.
                                                mensajeRespuesta = Enviar_respuesta_Chip(mensajeRespuesta.terminal, mensajeRespuesta.host, mensajeRespuesta.Ticket, conector)

                                                If mensajeRespuesta Is Nothing Then
                                                    Respuesta.DisplayString = "El SP no respondió al envió de la respuesta del CHIP. No se grabó la respuesta."
                                                    '                                        Else
                                                    '                                            Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                    '                                            Respuesta.TicketString = mensajeRespuesta.cupon
                                                End If

                                            End If
                                        Else
                                            Respuesta.Response = ResponseDescription.ERROR_EN_PP
                                            Respuesta.DisplayString = String.Format("El SP respondió pero el PP no respondió la confirmación del CHIP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)



                                            'TODO: Ver si reversar cuando el pp no responde por algun error.
                                        End If




                                    Case Else
                                        Msg("Modo de ingreso no contemplado.")
                                End Select
                            Else
                                '---NO HUBO RESPUESTA
                                'Respuesta.TicketString = Ticket_SinRepuesta(Reverso)
                                Reversar(Reverso)
                                'TODO: Aca habria que reversar. o ver que hacer.
                                Select Case conector.RespuestaInicioCompra.modoIngreso

                                    Case "M", "B"

                                        'Return Nothing
                                    Case "C"
                                        If conector.Responder_Chip("", "", "", False) = 0 Then

                                        End If

                                    Case Else
                                        Msg("Modo de ingreso no contemplado.")
                                End Select
                                Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                                Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."

                            End If

                        Else
                            'ERROR EN SOLICITUD DE DATOS ADICIONALES
                            'TODO: Error no continua con la transaccion
                            mostrar_error(Respuesta, conector)

                        End If

                    Else
                        'TODO: No hubo respuesta del servidor para la consulta de datos adicionales.
                        Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                        Respuesta.DisplayString = "El SP no respondió a la consulta de datos adicionales. Reintente."
                        conector.Cancelar()

                    End If
                Else
                    'ERROR EN EL DESPERTAR AL PINPAD
                    mostrar_error(Respuesta, conector)

                End If

            End Using
        Else
            Respuesta.Response = ResponseDescription.ERROR_EN_PP
            Respuesta.DisplayString = "No se puede utilizar el PP, falta sincronizar o inicializar."

        End If
        Return Respuesta
    End Function


    Dim revtest As New TransmisorTCP.MensajeIda
    Public Function CompraMM(pCuotas As Integer, pImporte As Decimal, pCashback As Decimal, ticketcaja As Integer, pdocumento As String) As ResponseToPOS
        Dim Respuesta As New ResponseToPOS
        Dim Reverso As New TransmisorTCP.MensajeIda
        Dim DecisionCHIP_Z1 As Boolean = False
        Dim DecisionCHIP_Y1 As Boolean = False
        Dim DecisionCTLS_Z1 As Boolean = False
        Dim DecisionCTLS_Y1 As Boolean = False
        Dim TipoCuentaY07xx As String = ""


        Respuesta.Response = ResponseDescription.NO_APROBADA
        If inicializado And sincronizado Then

            Using conector As New lib_PP.ConectorPP()
                Dim conCashback As Boolean
                If pCashback > 0 Then
                    conCashback = True
                Else
                    conCashback = False
                End If

                '--------------------------------------------------
                '---   INICIO DE COMPRA - DESPIERTA EL PINPAD      
                '--------------------------------------------------
                Dim MensClaves = Consultar_Claves()       '--- consulta al Servidor Tarjetas  
                If MensClaves IsNot Nothing Then

                    '--- ACA HACE EL Y41 (EN MULTIMODO ES Y41) - PAG 40  
                    If conector.Inicio_CompraMM(conCashback, MensClaves.Clave_wkd, MensClaves.Clave_wkp, MensClaves.PosicionMK, pImporte, pCashback) = 0 Then


                        If conector.RespuestaContactless.modoIngreso = "L" Then
                            Msg("-------- Respuesta a Y41 ----------")
                            Msg("Modo Ing                      : " & conector.RespuestaContactless.modoIngreso)
                            'Msg("Nro Tarjeta                   : " & conector.RespuestaContactless.tarjeta)
                            Dim rango As String = conector.RespuestaContactless.tarjeta
                            Dim tipoTarjeta As Integer = rango.Substring(0, 1)
                            Dim CodigoServicioPIN As Integer = conector.RespuestaContactless.codigoServicio
                            Msg("Cod. Serv.                    : " & conector.RespuestaContactless.codigoServicio)
                            Msg("Cod. Banco                    : " & conector.RespuestaContactless.codigoBanco)
                            Msg("Nro de registro               : " & conector.RespuestaContactless.nroRegistro)
                            Msg("Id de encripcion              : " & conector.RespuestaContactless.idEncripcion)
                            Msg("Tipo de encripcion            : " & conector.RespuestaContactless.tipoEncripcion)
                            Msg("Paquete encriptado            : " & conector.RespuestaContactless.paquete)
                            Msg("Autorizacion                  : " & conector.RespuestaContactless.autorizacion)
                            Msg("Respuesta del emisor          : " & conector.RespuestaContactless.respEmisor)
                            Msg("Secuencia PAN                 : " & conector.RespuestaContactless.secuencia)
                            Msg("Tipo de cuenta                : " & conector.RespuestaContactless.tipoCuenta)
                            Msg("PIN encriptado                : " & conector.RespuestaContactless.pin)
                            Msg("Nombre Aplicacion             : " & conector.RespuestaContactless.nombreApp)
                            Msg("ID Aplicacion                 : " & conector.RespuestaContactless.idApp)
                            Msg("Nombre Tarjeta habiente       : " & conector.RespuestaContactless.nombreThab)
                            Msg("Consulta por copia            : " & conector.RespuestaContactless.consultaCopia)
                            Msg("Solicita firma                : " & conector.RespuestaContactless.solicitaFirma)
                            Msg("Datos del emisor              : " & conector.RespuestaContactless.datosEmisor)
                            ' Msg("Codigo de Respuesta           : " & conector.RespuestaContactless.codrespuesta)







                            DecisionCTLS_Z1 = False
                            DecisionCTLS_Y1 = False
                            '--- ACA ES LA PRIMERA Y UNICA DECISION QUE TOMA EL CONTACTLESS   
                            '--- ACA TEORICAMENTE SOLAMENTE SE DAN LOS CASOS Z1 e Y1          

                            'MsgBox(conector.RespuestaContactless.autorizacion.Trim)

                            If conector.RespuestaContactless.autorizacion.Trim = "Z1" Or
                                conector.RespuestaContactless.autorizacion.Trim = "Z3" Or
                                conector.RespuestaContactless.autorizacion.Trim = "Y1" Or
                                conector.RespuestaContactless.autorizacion.Trim = "Y3" Then

                                '--- CUANDO VIENE Z3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER NO APROBADO  
                                '--- CUANDO VIENE Z1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER NO APROBADO  
                                '--- CUANDO VIENE Y3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER APROBADO  
                                '--- CUANDO VIENE Y1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER APROBADO  
                                Reverso.CodAutorizaAdv = conector.RespuestaContactless.autorizacion.Trim + "    "
                                'If My.Computer.Name = "MARCOS-XP" Then
                                ' Reverso.CodAutorizaAdv = "Z1    "
                                'End If

                                If Reverso.CodAutorizaAdv.Trim = "Z1" Then
                                    '--- ADVICE 
                                    Reverso.CodAutorizaAdv = "Z1    "
                                    Dim RespuestaZ1CTLS = Enviar_Transaccion_CL_Adv(conector, pCuotas, pImporte, pCashback, ticketcaja, "", TransmisorTCP.TipoMensaje.CompraCashback, Reverso, pdocumento)

                                    '--- probarlo bien
                                    ' CreartickNOAPROBADO_Z1(revtest, pCuotas, pImporte, ticketcaja, pdocumento)


                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                    'Respuesta.DisplayString = String.Format("CONTACTLESS - Denegada por PP, (Z1)...  Respuesta PP: {0}", Reverso.CodAutorizaAdv.Trim)
                                    Respuesta.DisplayString = String.Format("CONTACTLESS - Denegada por PP, (Z1)...  ")
                                    conector.Cancelar()
                                    Return Respuesta
                                    Exit Function
                                Else
                                    '--- ADVICE 
                                    Reverso.CodAutorizaAdv = "Y1    "
                                    Dim RespuestaY1CTLS = Enviar_Transaccion_CL_Adv(conector, pCuotas, pImporte, pCashback, ticketcaja, "", TransmisorTCP.TipoMensaje.CompraCashback, Reverso, pdocumento)

                                    '--- probarlo bien
                                    'CreartickAPROBADO_Y1(revtest, pCuotas, pImporte, ticketcaja, pdocumento)

                                    'If RespuestaY1CTLS.codRespuesta = "00" Then
                                    Respuesta.Response = ResponseDescription.APROBADA
                                    'Else
                                    ' Respuesta.Response = ResponseDescription.NO_APROBADA
                                    'End If
                                    Respuesta.Emisor = RespuestaY1CTLS.Emisor
                                    Respuesta.DisplayString = RespuestaY1CTLS.Respuesta
                                    Respuesta.TicketString = RespuestaY1CTLS.Ticket

                                    Return Respuesta
                                    Exit Function
                                End If
                            End If








                            Dim mensajeRespuestaxxx = Consultar_Datos_Adicionales(rango, conector.RespuestaContactless.modoIngreso, conector.RespuestaContactless.codigoServicio, "")
                            'Dim posMK = mensajeRespuesta.PosicionMK
                            If mensajeRespuestaxxx IsNot Nothing Then
                                If mensajeRespuestaxxx.codRespuesta = "00" Then
                                    '-------------------------------------------------------------------------------------------------------------
                                    '--- ACA MANDA AL PINPAD EL Y02:  SOLICITA ALGUNOS DATOS MAS, ULTIMOS 4 DIGITOS, COD AUTORIZ, ETC  AL PINPAD  
                                    '-------------------------------------------------------------------------------------------------------------
                                    '--- TIPO CUENTA CTLS 
                                    If Trim(mensajeRespuestaxxx.Emisor) = "MAESTRO" Then
                                        If conector.PideTipoCuenta_CL() = 0 Then
                                            If conector.RespuestaCTLS_TCuenta.CodigoTipoCuenta < 1 Or conector.RespuestaCTLS_TCuenta.CodigoTipoCuenta > 4 Then
                                                Respuesta.DisplayString = "Tipo de Cuenta incorrecto"
                                                Respuesta.Response = ResponseDescription.NO_APROBADA
                                                Return Respuesta
                                            Else
                                                'TipoCuentaY07xx = conector.RespuestaCTLS_TCuenta.CodigoTipoCuenta
                                                If conector.RespuestaCTLS_TCuenta.CodigoTipoCuenta = 1 Then
                                                    TipoCuentaY07xx = 1
                                                ElseIf conector.RespuestaCTLS_TCuenta.CodigoTipoCuenta = 2 Then
                                                    TipoCuentaY07xx = 2
                                                ElseIf conector.RespuestaCTLS_TCuenta.CodigoTipoCuenta = 3 Then
                                                    TipoCuentaY07xx = 8
                                                ElseIf conector.RespuestaCTLS_TCuenta.CodigoTipoCuenta = 4 Then
                                                    TipoCuentaY07xx = 9
                                                End If
                                                conector.Modifica_TipoCuentaY21(TipoCuentaY07xx)
                                            End If
                                        End If
                                    End If
                                End If
                            End If






                            Try
                                '--- TARJETA UNION - ES CABAL, PERO CUANDO ES MUTUAL UNION TIENE QUE PASAR COMO 
                                '--- CABAL Y TIENEN 2 Y 3 CUOTAS SIN INTERES                                    
                                If Mid(conector.RespuestaContactless.tarjeta, 1, 6) = "604341" Then
                                    If pCuotas = 102 Then
                                        pCuotas = 502
                                    End If
                                    If pCuotas = 103 Then
                                        pCuotas = 503
                                    End If
                                End If
                            Catch

                            End Try





                            '********** ENVIA TRANSACCION A AL SERVIDOR DE TARJETAS   -   SP *****************
                            Dim MensajeRespuesta As TransmisorTCP.MensajeRespuesta
                            If pCashback > 0 Then
                                MensajeRespuesta = Enviar_Transaccion_CL(conector, pCuotas, pImporte, pCashback, ticketcaja, "", TransmisorTCP.TipoMensaje.CompraCashback, Reverso, pdocumento, TipoCuentaY07xx)
                            Else
                                MensajeRespuesta = Enviar_Transaccion_CL(conector, pCuotas, pImporte, pCashback, ticketcaja, "", TransmisorTCP.TipoMensaje.Compra, Reverso, pdocumento, TipoCuentaY07xx)
                            End If

                            If MensajeRespuesta IsNot Nothing Then
                                Msg("-------------------------------")
                                Msg("     RESPUESTA DEL SERVIDOR")
                                Msg("-------------------------------")
                                Msg("Codigo de respuesta       : " & MensajeRespuesta.codRespuesta)
                                Msg("Respuesta                 : " & MensajeRespuesta.Respuesta)
                                Msg("Importe trx               : " & MensajeRespuesta.Importe)
                                Msg("Emisor                    : " & MensajeRespuesta.Emisor)
                                Msg("Mensaje Host              : " & MensajeRespuesta.mensajeHost)
                                Msg("Código Autorización       : " & MensajeRespuesta.Autorizacion)





                                '--- CUANDO LA DECISION 1 Y UNICA ES Y1 PUEDE SER TICKET APROBADO   
                                If DecisionCTLS_Y1 = True Then
                                End If

                                '--- CUANDO LA DECISION 1 Y UNICA ES Z1 TIENE QUE SER TICKET RECHAZADO                
                                '--- EN ESTE CASO NO HAY QUE MANDAR UN REVERSO, PERO PARA MANDAR UN ADVICE TENGO QUE  
                                '--- MANDAR LA COMPRA AL SERVIDOR DE TARJETAS, SINO NO PUEDO HACER UN ADVICE, POR ESO 
                                '--- HAGO EL REVERSO                                                                  
                                If DecisionCTLS_Z1 = True Then
                                End If




                                If MensajeRespuesta.codRespuesta = "00" Then
                                    Respuesta.Response = ResponseDescription.APROBADA
                                    Respuesta.TotalAmount = MensajeRespuesta.Importe
                                    Respuesta.CodigoAutorizacion = MensajeRespuesta.Autorizacion
                                Else
                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                End If
                                Respuesta.Emisor = MensajeRespuesta.Emisor
                                Respuesta.DisplayString = MensajeRespuesta.Respuesta
                                Respuesta.TicketString = MensajeRespuesta.cupon

                            Else

                                '---NO HUBO RESPUESTA      
                                'Respuesta.TicketString = Ticket_SinRepuesta(Reverso)
                                Dim respreverso = Reversar(Reverso)
                                If respreverso IsNot Nothing Then
                                    Respuesta.DisplayString = respreverso.Respuesta
                                    'Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."
                                Else
                                    Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."
                                End If
                                Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                            End If

                        Else

                            Msg("-------- Respuesta a Y41 ----------")
                            Msg("Modo Ing                      : " & conector.RespuestaInicioCompra.modoIngreso)
                            'Msg("Nro Tarjeta                   : " & conector.RespuestaInicioCompra.tarjeta)
                            Dim rango As String = conector.RespuestaInicioCompra.tarjeta
                            Dim tipoTarjeta As Integer = rango.Substring(0, 1)
                            Dim CodigoServicioPIN As Integer = conector.RespuestaInicioCompra.codigoServicio
                            Msg("Cod. Serv.                    : " & conector.RespuestaInicioCompra.codigoServicio)
                            Msg("Cod. Banco                    : " & conector.RespuestaInicioCompra.codigoBanco)
                            Msg("Nro de registro               : " & conector.RespuestaInicioCompra.nroRegistro)
                            Msg("Abreviatura Tarjeta           : " & conector.RespuestaInicioCompra.abrebiaturaTarj)
                            Msg("Nombre Aplicacion CHIP        : " & conector.RespuestaInicioCompra.appChip)


                            '--- ACA BUSCA DATOS ADICIONALES QUE NECESITO, PARA HACER EL Y02 (SE HACE MAS ABAJO) - PAG 18  
                            '********** CONSULTA AL SERVIDOR DE TARJETAS PARA OBTENER DATOS ADICIONALES Y EJECUTAR EL COMANDO Y02 *****************
                            Dim mensajeRespuesta = Consultar_Datos_Adicionales(rango, conector.RespuestaInicioCompra.modoIngreso, conector.RespuestaInicioCompra.codigoServicio, conector.RespuestaInicioCompra.appChip)
                            'Dim posMK = mensajeRespuesta.PosicionMK
                            If mensajeRespuesta IsNot Nothing Then
                                If mensajeRespuesta.codRespuesta = "00" Then
                                    If mensajeRespuesta.Emisor.TrimEnd = "VISA DEBITO" Then
                                        If pCashback > 0 Then
                                            pCuotas = "800"
                                        Else
                                            pCuotas = "101"            '--- decia "1" 
                                        End If
                                    End If

                                    If mensajeRespuesta.Emisor.TrimEnd = "MASTERCARD DEBIT" Then
                                        If pCashback > 0 Then
                                            pCuotas = "992"
                                        Else
                                            pCuotas = "101"            '--- decia "1" 
                                        End If
                                    End If

                                    If mensajeRespuesta.Emisor.TrimEnd = "MAESTRO" Then
                                        If pCashback > 0 Then
                                            pCuotas = "992"
                                        Else
                                            pCuotas = "991"
                                        End If
                                    End If

                                    '-------------------------------------------------------------------------------------------------------------
                                    '--- ACA MANDA AL PINPAD EL Y02:  SOLICITA ALGUNOS DATOS MAS, ULTIMOS 4 DIGITOS, COD AUTORIZ, ETC  AL PINPAD  
                                    '-------------------------------------------------------------------------------------------------------------
                                    If conector.Datos_adicionales(conector.RespuestaInicioCompra.modoIngreso,
                                                                  mensajeRespuesta.u4d,
                                                                  mensajeRespuesta.cds,
                                                                  mensajeRespuesta.spi,
                                                                  mensajeRespuesta.Clave_wkd,
                                                                  mensajeRespuesta.Clave_wkp,
                                                                  mensajeRespuesta.PosicionMK,
                                                                  pImporte,
                                                                  pCashback,
                                                                  tipoTarjeta,
                                                                  CodigoServicioPIN) = 0 Then

                                        '******** Muestra mensajes en pantalla segun modo ingreso *********
                                        Select Case conector.RespuestaInicioCompra.modoIngreso
                                            Case "M"
                                                Datos_Adicionales_Manual(conector)
                                            Case "B"
                                                Datos_Adicionales_Banda(conector)
                                            Case "C"
                                                Datos_Adicionales_Chip(conector)
                                            Case Else
                                                Msg("Modo de ingreso no contemplado.")
                                        End Select





                                        DecisionCHIP_Y1 = False
                                        DecisionCHIP_Z1 = False
                                        If conector.RespuestaInicioCompra.modoIngreso = "C" Then
                                            '--- ACA ES LA PRIMERA DECISION QUE TOMA EL CHIP         
                                            '--- ACA TEORICAMENTE SOLAMENTE SE DAN LOS CASOS Z1 e Y1 

                                            'MsgBox(conector.RespuestaDatosAdicionales.autorizacion.Trim)

                                            If conector.RespuestaDatosAdicionales.autorizacion.Trim = "Z1" Or
                                                conector.RespuestaDatosAdicionales.autorizacion.Trim = "Z3" Or
                                                conector.RespuestaDatosAdicionales.autorizacion.Trim = "Y1" Or
                                                conector.RespuestaDatosAdicionales.autorizacion.Trim = "Y3" Then

                                                '--- CUANDO VIENE Z3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER NO APROBADO  
                                                '--- CUANDO VIENE Z1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER NO APROBADO  
                                                '--- CUANDO VIENE Y3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER APROBADO  
                                                '--- CUANDO VIENE Y1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER APROBADO  
                                                Reverso.CodAutorizaAdv = conector.RespuestaDatosAdicionales.autorizacion.Trim + "    "
                                                'If My.Computer.Name = "MARCOS-XP" Then
                                                '    Reverso.CodAutorizaAdv = "Z1    "
                                                'End If

                                                If Reverso.CodAutorizaAdv.Trim = "Z1" Then
                                                    '--- ADVICE 
                                                    Reverso.CodAutorizaAdv = "Z1    "
                                                    Dim mensajeRespuestaZ1Chip = Enviar_TransaccionAdv(conector, pCuotas, pImporte, pCashback, ticketcaja, mensajeRespuesta.Clave_wkp, TransmisorTCP.TipoMensaje.Advice, Reverso, pdocumento)


                                                    '--- probarlo bien
                                                    'CreartickNOAPROBADO_Z1(revtest, pCuotas, pImporte, ticketcaja, pdocumento)

                                                    'End If
                                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                                    Respuesta.DisplayString = String.Format("Denegada por PP (Z1)...  Respuesta PP: {0}", mensajeRespuestaZ1Chip.codRespuesta)
                                                    conector.Cancelar()
                                                    Return Respuesta
                                                    Exit Function
                                                Else
                                                    'If mensajeRespuesta.codRespuesta = "00" Then      '--- APROBADA   
                                                    Respuesta.Response = ResponseDescription.APROBADA
                                                    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                    Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion

                                                    '--- ADVICE 
                                                    Reverso.CodAutorizaAdv = "Y1    "
                                                    Dim mensajeRespuestaY1Chip = Enviar_TransaccionAdv(conector, pCuotas, pImporte, pCashback, ticketcaja, mensajeRespuesta.Clave_wkp, TransmisorTCP.TipoMensaje.Advice, Reverso, pdocumento)


                                                    '--- probarlo bien
                                                    ' CreartickAPROBADO_Y1(revtest, pCuotas, pImporte, ticketcaja, pdocumento)

                                                    'Else
                                                    ' Respuesta.Response = ResponseDescription.NO_APROBADA
                                                    ' End If
                                                    Respuesta.Emisor = mensajeRespuesta.Emisor
                                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                    Respuesta.TicketString = Respuesta.TicketString & mensajeRespuesta.cupon
                                                    Return Respuesta
                                                    Exit Function
                                                End If

                                            End If
                                        End If




                                        Try
                                            '--- TARJETA UNION - ES CABAL, PERO CUANDO ES MUTUAL UNION TIENE QUE PASAR COMO 
                                            '--- CABAL Y TIENEN 2 Y 3 CUOTAS SIN INTERES                                    
                                            If Mid(conector.RespuestaInicioCompra.tarjeta, 1, 6) = "604341" Then
                                                If pCuotas = 102 Then
                                                    pCuotas = 502
                                                End If
                                                If pCuotas = 103 Then
                                                    pCuotas = 503
                                                End If
                                            End If
                                        Catch

                                        End Try




                                        '********** ENVIA TRANSACCION AL SERVIDOR DE TARJETAS *****************
                                        If pCashback > 0 Then
                                            mensajeRespuesta = Enviar_Transaccion(conector, pCuotas, pImporte, pCashback, ticketcaja, mensajeRespuesta.Clave_wkp, TransmisorTCP.TipoMensaje.CompraCashback, Reverso, pdocumento)
                                        Else
                                            mensajeRespuesta = Enviar_Transaccion(conector, pCuotas, pImporte, pCashback, ticketcaja, mensajeRespuesta.Clave_wkp, TransmisorTCP.TipoMensaje.Compra, Reverso, pdocumento)
                                        End If


                                        '-------------------------------------------------------------------------------------------
                                        '--- ACA VUELVE CON UNA RESPUESTA DEL SERVIDOR DE TARJETAS, PUREDE SER APROBADO O NO        
                                        '-------------------------------------------------------------------------------------------
                                        If mensajeRespuesta IsNot Nothing Then
                                            Msg("-------------------------------")
                                            Msg("     RESPUESTA DEL SERVIDOR")
                                            Msg("-------------------------------")
                                            Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                            Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                            Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                            Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                            Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)
                                            Msg("Código Autorización       : " & mensajeRespuesta.Autorizacion)

                                            Select Case conector.RespuestaInicioCompra.modoIngreso
                                                Case "M", "B"
                                                    If mensajeRespuesta.codRespuesta = "00" Then      '--- APROBADA   
                                                        Respuesta.Response = ResponseDescription.APROBADA
                                                        Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                        Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion

                                                    Else

                                                        If mensajeRespuesta.codRespuesta = "55" Then  '--- 55 = error de pin  
                                                            'TODO: REINGRESO DE PIN  
                                                            MensClaves = Consultar_Claves()

                                                            Respuesta.TicketString = mensajeRespuesta.cupon & vbNewLine & "@" & vbNewLine
                                                            If conector.Reingreso_pin(MensClaves.PosicionMK, MensClaves.Clave_wkp, conector.RespuestaInicioCompra.tarjeta) = 0 Then
                                                                Msg("-------- Respuesta a Y04 ----------")
                                                                Msg("PIN Block                     : " & conector.RespuestaDatosAdicionales.PinBlock)

                                                                If pCashback > 0 Then
                                                                    mensajeRespuesta = Enviar_Transaccion(conector, pCuotas, pImporte, pCashback, ticketcaja, mensajeRespuesta.Clave_wkp, TransmisorTCP.TipoMensaje.CompraCashback, Reverso, pdocumento)
                                                                Else
                                                                    mensajeRespuesta = Enviar_Transaccion(conector, pCuotas, pImporte, pCashback, ticketcaja, mensajeRespuesta.Clave_wkp, TransmisorTCP.TipoMensaje.Compra, Reverso, pdocumento)
                                                                End If

                                                                Msg("-----------------------------------------")
                                                                Msg("     RESPUESTA DEL SERVIDOR REINGRESO PIN")
                                                                Msg("-----------------------------------------")
                                                                Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                                                Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                                                Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                                                Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                                                Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)
                                                                Msg("Código Autorización       : " & mensajeRespuesta.Autorizacion)

                                                                If mensajeRespuesta.codRespuesta = "00" Then
                                                                    Respuesta.Response = ResponseDescription.APROBADA
                                                                    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                                    Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion
                                                                Else
                                                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                                                End If
                                                            Else
                                                                Respuesta.Response = ResponseDescription.NO_APROBADA
                                                            End If
                                                        Else
                                                            Respuesta.Response = ResponseDescription.NO_APROBADA
                                                        End If
                                                    End If

                                                    Respuesta.Emisor = mensajeRespuesta.Emisor
                                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                    Respuesta.TicketString = Respuesta.TicketString & mensajeRespuesta.cupon

                                         'Return mensajeRespuesta.codRespuesta        
                                         'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR  
                                                Case "C"

                                                    '-----------------------------------------------------------------------------------------------------------
                                                    '--- CUANDO ES CHIP, UNA VEZ QUE EL HOST DA APROBADO O NO APROBADO, HAY QUE MANDAR UN Y03 AL PINPAD PORQUE  
                                                    '--- EL CHIP TIENE QUE CONTESTAR APROBADO Y NO APROBADO, EL CHIP ES EL QUE DECIDE                           
                                                    '--- ACA ES LA SEGUNDA DECISION QUE TOMA EL CHIP                                                            
                                                    '-----------------------------------------------------------------------------------------------------------
                                                    If conector.Responder_Chip(mensajeRespuesta.Autorizacion, mensajeRespuesta.codRespuesta, mensajeRespuesta.criptograma, True) = 0 Then
                                                        Msg("-------- Respuesta a Y03 ----------")
                                                        Msg("Autorizacion                      : " & conector.RespuestaChip.autorizacion)
                                                        Msg("Codigo de respuesta               : " & conector.RespuestaChip.codrespuesta)
                                                        Msg("Respuesta                         : " & conector.RespuestaChip.mensajeRepuesta)
                                                        Msg("Criptograma                       : " & conector.RespuestaChip.criptograma)

                                                        '--- ACA ESTA LA RESPUESTA DEL SERVIDOR DE TARJETAS   
                                                        If mensajeRespuesta.codRespuesta = ResponseDescription.APROBADA Then   '--- APROBADA POR SP  
                                                            Console.WriteLine(conector.RespuestaChip.autorizacion)


                                                            'Dim pepito As String = "Z3"


                                                            '--- ACA ESTA LA RESPUESTA DEL CHIP  (QUE ES EL QUE DECIDE)  
                                                            If conector.RespuestaChip.autorizacion.Trim <> "Z1" And
                                                                conector.RespuestaChip.autorizacion.Trim <> "Z3" And
                                                                conector.RespuestaChip.autorizacion.Trim <> "Y1" And
                                                                conector.RespuestaChip.autorizacion.Trim <> "Y3" Then
                                                                'pepito <> "Z3" Then

                                                                Respuesta.Response = ResponseDescription.APROBADA
                                                                Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                                Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                                'Respuesta.TicketString = mensajeRespuesta.cupon
                                                                Respuesta.Emisor = mensajeRespuesta.Emisor
                                                                Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion

                                                            Else
                                                                'If My.Computer.Name = "MARCOS-XP" Then
                                                                '    MsgBox(conector.RespuestaChip.autorizacion.Trim)
                                                                'End If

                                                                '--- CUANDO VIENE Z3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER NO APROBADO  
                                                                '--- CUANDO VIENE Z1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER NO APROBADO  
                                                                '--- CUANDO VIENE Y3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER APROBADO  
                                                                '--- CUANDO VIENE Y1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER APROBADO  
                                                                Reverso.CodAutorizaAdv = conector.RespuestaChip.autorizacion.Trim + "    "
                                                                'If My.Computer.Name = "MARCOS-XP" Then
                                                                '    Reverso.CodAutorizaAdv = "Y1    "
                                                                'End If

                                                                If Reverso.CodAutorizaAdv.Trim = "Z3" Then
                                                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                                                    Respuesta.DisplayString = String.Format("Denegada por PP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)

                                                                    '--- PRIMERO HAGO EL REVERSO ANTES QUE EL ADVICE, LIMPIO EL CodAutorizaAdv PORQUE SINO LO TOMA COMO ADVICE 
                                                                    Reverso.CodAutorizaAdv = "      "
                                                                    If Reversar(Reverso) IsNot Nothing Then
                                                                        Respuesta.DisplayString = Respuesta.DisplayString & " - Mov Reversado correctamente."
                                                                    Else
                                                                        Respuesta.DisplayString = Respuesta.DisplayString & " - No se pudo reversar correctamente. Verifique."
                                                                    End If

                                                                    '--- MANDO ADVICE  
                                                                    Reverso.CodAutorizaAdv = "Z3    "
                                                                    MandarAdvice(Reverso)
                                                                    'mensajeRespuesta = Enviar_TransaccionAdv(conector, pCuotas, pImporte, pCashback, ticketcaja, mensajeRespuesta.Clave_wkp, TransmisorTCP.TipoMensaje.Advice, Reverso, pdocumento)


                                                                ElseIf Reverso.CodAutorizaAdv.Trim = "Y3" Then
                                                                    '--- PRIMERO HAGO EL REVERSO ANTES QUE EL ADVICE, LIMPIO EL CodAutorizaAdv PORQUE SINO LO TOMA COMO ADVICE 
                                                                    Reverso.CodAutorizaAdv = "      "
                                                                    If Reversar(Reverso) IsNot Nothing Then
                                                                        Respuesta.DisplayString = Respuesta.DisplayString & " - Mov Reversado correctamente."
                                                                    Else
                                                                        Respuesta.DisplayString = Respuesta.DisplayString & " - No se pudo reversar correctamente. Verifique."
                                                                    End If

                                                                    Respuesta.Response = ResponseDescription.APROBADA
                                                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                                    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                                    Respuesta.Emisor = mensajeRespuesta.Emisor
                                                                    Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion

                                                                    '--- MANDO ADVICE  
                                                                    Reverso.CodAutorizaAdv = "Y3    "
                                                                    MandarAdvice(Reverso)


                                                                ElseIf Reverso.CodAutorizaAdv.Trim = "Z1" Then
                                                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                                                    Respuesta.DisplayString = String.Format("Denegada por PP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)
                                                                    '--- MANDO ADVICE  
                                                                    Reverso.CodAutorizaAdv = "Z1    "
                                                                    MandarAdvice(Reverso)

                                                                Else

                                                                    '--- SI PASA POR ACA ES Y1 
                                                                    Respuesta.Response = ResponseDescription.APROBADA
                                                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                                    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                                    'Respuesta.TicketString = mensajeRespuesta.cupon
                                                                    Respuesta.Emisor = mensajeRespuesta.Emisor
                                                                    Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion

                                                                    '--- MANDO ADVICE  
                                                                    Reverso.CodAutorizaAdv = "Y1    "
                                                                    MandarAdvice(Reverso)

                                                                End If
                                                            End If
                                                            'Respuesta.TicketString = mensajeRespuesta.cupon

                                                        Else    '--- NO APROBADA POR SP  

                                                            If mensajeRespuesta.codRespuesta = "12" Then
                                                                Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                                                            Else
                                                                Respuesta.Response = ResponseDescription.NO_APROBADA
                                                                'Respuesta.DisplayString = String.Format("Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)
                                                            End If
                                                            Respuesta.DisplayString = mensajeRespuesta.mensajeHost




                                                            '--- ACA ESTA LA RESPUESTA DEL CHIP  (QUE ES EL QUE DECIDE)  
                                                            If conector.RespuestaChip.autorizacion.Trim = "Z1" Or
                                                                conector.RespuestaChip.autorizacion.Trim = "Z3" Or
                                                                conector.RespuestaChip.autorizacion.Trim = "Y1" Or
                                                                conector.RespuestaChip.autorizacion.Trim = "Y3" Then

                                                                'If My.Computer.Name = "MARCOS-XP" Then
                                                                '    MsgBox(conector.RespuestaChip.autorizacion.Trim)
                                                                'End If

                                                                '--- CUANDO VIENE Z3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER NO APROBADO  
                                                                '--- CUANDO VIENE Z1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER NO APROBADO  
                                                                '--- CUANDO VIENE Y3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER APROBADO  
                                                                '--- CUANDO VIENE Y1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER APROBADO  
                                                                Reverso.CodAutorizaAdv = conector.RespuestaChip.autorizacion.Trim + "    "
                                                                'If My.Computer.Name = "MARCOS-XP" Then
                                                                '    Reverso.CodAutorizaAdv = "Y1    "
                                                                'End If

                                                                If Reverso.CodAutorizaAdv.Trim = "Z3" Then
                                                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                                                    Respuesta.DisplayString = String.Format("Denegada por PP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)

                                                                    '--- PRIMERO HAGO EL REVERSO ANTES QUE EL ADVICE, LIMPIO EL CodAutorizaAdv PORQUE SINO LO TOMA COMO ADVICE 
                                                                    Reverso.CodAutorizaAdv = "      "
                                                                    If Reversar(Reverso) IsNot Nothing Then
                                                                        Respuesta.DisplayString = Respuesta.DisplayString & " - Mov Reversado correctamente."
                                                                    Else
                                                                        Respuesta.DisplayString = Respuesta.DisplayString & " - No se pudo reversar correctamente. Verifique."
                                                                    End If

                                                                    '--- MANDO ADVICE  
                                                                    Reverso.CodAutorizaAdv = "Z3    "
                                                                    MandarAdvice(Reverso)


                                                                ElseIf Reverso.CodAutorizaAdv.Trim = "Y3" Then
                                                                    '--- PRIMERO HAGO EL REVERSO ANTES QUE EL ADVICE, LIMPIO EL CodAutorizaAdv PORQUE SINO LO TOMA COMO ADVICE 
                                                                    Reverso.CodAutorizaAdv = "      "
                                                                    If Reversar(Reverso) IsNot Nothing Then
                                                                        Respuesta.DisplayString = Respuesta.DisplayString & " - Mov Reversado correctamente."
                                                                    Else
                                                                        Respuesta.DisplayString = Respuesta.DisplayString & " - No se pudo reversar correctamente. Verifique."
                                                                    End If
                                                                    Respuesta.Response = ResponseDescription.APROBADA
                                                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                                    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                                    Respuesta.Emisor = mensajeRespuesta.Emisor
                                                                    Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion

                                                                    '--- MANDO ADVICE  
                                                                    Reverso.CodAutorizaAdv = "Y3    "
                                                                    MandarAdvice(Reverso)


                                                                ElseIf Reverso.CodAutorizaAdv.Trim = "Z1" Then
                                                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                                                    Respuesta.DisplayString = String.Format("Denegada por PP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)
                                                                    '--- MANDO ADVICE  
                                                                    Reverso.CodAutorizaAdv = "Z1    "
                                                                    MandarAdvice(Reverso)

                                                                Else

                                                                    '--- SI PASA POR ACA ES Y1 
                                                                    Respuesta.Response = ResponseDescription.APROBADA
                                                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                                    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                                    'Respuesta.TicketString = mensajeRespuesta.cupon
                                                                    Respuesta.Emisor = mensajeRespuesta.Emisor
                                                                    Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion

                                                                    '--- MANDO ADVICE  
                                                                    Reverso.CodAutorizaAdv = "Y1    "
                                                                    MandarAdvice(Reverso)

                                                                End If
                                                            End If

                                                        End If

                                                        Respuesta.TicketString = mensajeRespuesta.cupon

                                                        If mensajeRespuesta.codRespuesta <> "12" Then       ' 12 es respuesta invalida
                                                            'ENVIA LA RESPUESTA DEL CHIP AL SP.
                                                            mensajeRespuesta = Enviar_respuesta_Chip(mensajeRespuesta.terminal, mensajeRespuesta.host, mensajeRespuesta.Ticket, conector)
                                                            If mensajeRespuesta Is Nothing Then
                                                                Respuesta.DisplayString = "El SP no respondió al envió de la respuesta del CHIP. No se grabó la respuesta."
                                                            End If
                                                        End If
                                                    Else
                                                        Reversar(Reverso)
                                                        Respuesta.Response = ResponseDescription.ERROR_EN_PP
                                                        Respuesta.DisplayString = String.Format("El SP respondió pero el PP no respondió la confirmación del CHIP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)
                                                        'TODO: Ver si reversar cuando el pp no responde por algun error.
                                                    End If

                                                Case Else
                                                    Msg("Modo de ingreso no contemplado.")
                                            End Select
                                        Else
                                            '---NO HUBO RESPUESTA
                                            'Respuesta.TicketString = Ticket_SinRepuesta(Reverso)
                                            'Reversar(Reverso)
                                            'TODO: Aca habria que reversar. o ver que hacer.
                                            Select Case conector.RespuestaInicioCompra.modoIngreso
                                                Case "M", "B"

                                        'Return Nothing
                                                Case "C"
                                                    If conector.Responder_Chip("", "", "", False) = 0 Then

                                                    End If

                                                Case Else
                                                    Msg("Modo de ingreso no contemplado.")
                                            End Select
                                            Dim respreverso = Reversar(Reverso)
                                            If respreverso IsNot Nothing Then
                                                Respuesta.DisplayString = respreverso.Respuesta
                                                'Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."
                                            Else
                                                Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."
                                            End If

                                            Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA

                                        End If

                                    Else

                                        'ERROR EN SOLICITUD DE DATOS ADICIONALES
                                        'TODO: Error no continua con la transaccion
                                        mostrar_error(Respuesta, conector)
                                    End If
                                Else
                                    'TODO: No hubo respuesta del servidor para la consulta de datos adicionales.
                                    Respuesta.Response = ResponseDescription.COMANDO_CANCELADO
                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                    conector.Cancelar()
                                End If
                            Else
                                'TODO: No hubo respuesta del servidor para la consulta de datos adicionales.
                                Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                                Respuesta.DisplayString = "El SP no respondió a la consulta de datos adicionales. Reintente."
                                conector.Cancelar()
                            End If
                        End If
                    Else
                        'ERROR EN EL DESPERTAR AL PINPAD
                        mostrar_error(Respuesta, conector)
                        conector.Cancelar()
                    End If
                Else
                    Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                    Respuesta.DisplayString = "El servidor de tarjetas no responde"
                End If
            End Using
        Else
            Respuesta.Response = ResponseDescription.ERROR_EN_PP
            Respuesta.DisplayString = "No se puede utilizar el PP, falta sincronizar o inicializar... Pase la tarjeta por el Paralelo."
        End If

        '--- ESTO ES LO QUE TOMA LA CAJA CON EL EVENTO CLASECLIENTESOCKET (LEERSOCKET)
        Return Respuesta

    End Function






    Private Sub CreartickNOAPROBADO_Z1(req, pCuotas, pImporte, ticketcaja, pdocumento)
        Try
            Dim nombreCupon As String = req.ping + req.Caja.ToString("00") + "_" + req.ticketcaja.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
            Try
                Dim archivoTick = New System.IO.FileStream("C:\Tarjetas\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
                'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
                'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
                Dim renglonTick = New System.IO.StreamWriter(archivoTick)
                With req
                    'renglonTick.WriteLine("             " + .vta.VtaEmiName)
                    renglonTick.WriteLine("               COMPRA")
                    renglonTick.WriteLine("             PINGUINO " + req.ping)

                    renglonTick.WriteLine("             CAJA: " + .Caja.ToString("00"))
                    'renglonTick.WriteLine("Nro.Com.:" + .idComercio.ToString.PadRight(12) + "  Term:" + .terminal.PadRight(8))
                    'renglonTick.WriteLine("Nro. de Lote:" + .nroLote.ToString.PadRight(3) + "   Nro. de Cupon:" + .nroTicket.ToString.PadRight(4))

                    renglonTick.WriteLine("Importe TOTAL: $" + pImporte.ToString)   ' (Math.Round(CInt(.vta.VtaMontop) / 100, 2)).ToString("###,##0.00") + ".-")
                    renglonTick.WriteLine("Cuotas: " + pCuotas.ToString("00"))          ' req.cuotas.ToString("00"))

                    renglonTick.WriteLine("Nro de comprobante:" + .ticketcaja.ToString)

                    renglonTick.WriteLine("****************************************")
                    renglonTick.WriteLine("       R  E  C  H  A  Z  A  D  O")
                    renglonTick.WriteLine("              ADVICE - Z1")
                    renglonTick.WriteLine("****************************************")

                End With
                renglonTick.Close()

            Catch ex As Exception
                'Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                '             ex.Message)
            End Try

            Try
                'Logger.Info("C:\Tarjetas\Cupones\" + nombreCupon + "-->" + req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
                If System.IO.File.Exists(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT") Then
                    System.IO.File.Delete(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
                End If
                System.IO.File.Copy("C:\Tarjetas\Cupones\" + nombreCupon, req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            Catch ex As Exception
                ' Logger.Error(String.Format("No se pudo renombrar cupón {0}", nombreCupon))
            End Try

            Try
                'Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
                System.IO.File.Copy("C:\Tarjetas\Cupones\" + nombreCupon, "C:\Temp\Cupones\" + nombreCupon)
            Catch ex As Exception
                'Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon))
            End Try
        Catch

        End Try

    End Sub


    Private Sub CreartickAPROBADO_Y1(req, pCuotas, pImporte, ticketcaja, pdocumento)
        Try

            Dim nombreCupon As String = req.ping + req.Caja.ToString("00") + "_" + req.ticketcaja.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
            Try
                Dim archivoTick = New System.IO.FileStream("\\PINGUINO1-1\SYS\TARJETAS\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
                'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
                'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
                Dim renglonTick = New System.IO.StreamWriter(archivoTick)
                With req
                    'renglonTick.WriteLine("             " + .vta.VtaEmiName)
                    renglonTick.WriteLine("               COMPRA")
                    renglonTick.WriteLine("             PINGUINO " + req.ping)

                    renglonTick.WriteLine("             CAJA: " + .Caja.ToString("00"))
                    'renglonTick.WriteLine("Nro.Com.:" + .idComercio.ToString.PadRight(12) + "  Term:" + .terminal.PadRight(8))
                    'renglonTick.WriteLine("Nro. de Lote:" + .nroLote.ToString.PadRight(3) + "   Nro. de Cupon:" + .nroTicket.ToString.PadRight(4))

                    renglonTick.WriteLine("Importe TOTAL: $" + pImporte.ToString)   ' (Math.Round(CInt(.vta.VtaMontop) / 100, 2)).ToString("###,##0.00") + ".-")
                    renglonTick.WriteLine("Cuotas: " + pCuotas.ToString("00"))          ' req.cuotas.ToString("00"))

                    renglonTick.WriteLine("Nro de comprobante:" + .ticketcaja.ToString)

                    renglonTick.WriteLine("****************************************")
                    renglonTick.WriteLine("         A  P  R  O  B  A  D  O")
                    renglonTick.WriteLine("              ADVICE - Y1")
                    renglonTick.WriteLine("****************************************")

                End With
                renglonTick.Close()

            Catch ex As Exception
                'Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                '             ex.Message)
            End Try

            Try
                'Logger.Info("C:\Tarjetas\Cupones\" + nombreCupon + "-->" + req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
                If System.IO.File.Exists(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT") Then
                    System.IO.File.Delete(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
                End If
                System.IO.File.Copy("\\PINGUINO1-1\SYS\TARJETAS\Cupones\" + nombreCupon, req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            Catch ex As Exception
                ' Logger.Error(String.Format("No se pudo renombrar cupón {0}", nombreCupon))
            End Try

            Try
                'Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
                System.IO.File.Copy("C:\Tarjetas\Cupones\" + nombreCupon, "C:\Temp\Cupones\" + nombreCupon)
            Catch ex As Exception
                'Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon))
            End Try
        Catch

        End Try

    End Sub




    Public Function Compra_CL(pCuotas As Integer, pImporte As Decimal, pCashback As Decimal, ticketcaja As Integer) As ResponseToPOS
        Dim Respuesta As New ResponseToPOS
        Dim Reverso As New TransmisorTCP.MensajeIda
        If inicializado And sincronizado Then
            Using conector As New lib_PP.ConectorPP()
                'Dim conCashback As Boolean
                'If pCashback > 0 Then
                '    conCashback = True
                'Else
                '    conCashback = False
                'End If
                Dim MensClaves = Consultar_Claves()
                If MensClaves IsNot Nothing Then

                    '********** INICIO DE COMPRA - DESPIERTA EL PINPAD *****************

                    If conector.Inicio_Compra_CL(pImporte, pCashback, MensClaves.Clave_wkp, MensClaves.Clave_wkd) = 0 Then

                        Msg("-------- Respuesta a Y21 ----------")
                        Msg("Modo Ing                      : " & conector.RespuestaContactless.modoIngreso)
                        'Msg("Nro Tarjeta                   : " & conector.RespuestaContactless.tarjeta)
                        Dim rango As String = conector.RespuestaContactless.tarjeta
                        Dim tipoTarjeta As Integer = rango.Substring(0, 1)
                        Dim CodigoServicioPIN As Integer = conector.RespuestaContactless.codigoServicio
                        Msg("Cod. Serv.                    : " & conector.RespuestaContactless.codigoServicio)
                        Msg("Cod. Banco                    : " & conector.RespuestaContactless.codigoBanco)
                        Msg("Nro de registro               : " & conector.RespuestaContactless.nroRegistro)
                        Msg("Id de encripcion              : " & conector.RespuestaContactless.idEncripcion)
                        Msg("Tipo de encripcion            : " & conector.RespuestaContactless.tipoEncripcion)
                        Msg("Paquete encriptado            : " & conector.RespuestaContactless.paquete)
                        Msg("Autorizacion                  : " & conector.RespuestaContactless.autorizacion)
                        Msg("Respuesta del emisor          : " & conector.RespuestaContactless.respEmisor)
                        Msg("Secuencia PAN                 : " & conector.RespuestaContactless.secuencia)
                        Msg("Tipo de cuenta                : " & conector.RespuestaContactless.tipoCuenta)
                        Msg("PIN encriptado                : " & conector.RespuestaContactless.pin)
                        Msg("Nombre Aplicacion             : " & conector.RespuestaContactless.nombreApp)
                        Msg("ID Aplicacion                 : " & conector.RespuestaContactless.idApp)
                        Msg("Nombre Tarjeta habiente       : " & conector.RespuestaContactless.nombreThab)
                        Msg("Consulta por copia            : " & conector.RespuestaContactless.consultaCopia)
                        Msg("Solicita firma                : " & conector.RespuestaContactless.solicitaFirma)
                        Msg("Datos del emisor              : " & conector.RespuestaContactless.datosEmisor)


                        '********** ENVIA TRANSACCION A SP *****************
                        Dim MensajeRespuesta As TransmisorTCP.MensajeRespuesta
                        If pCashback > 0 Then
                            MensajeRespuesta = Enviar_Transaccion_CL(conector, pCuotas, pImporte, pCashback, ticketcaja, "", TransmisorTCP.TipoMensaje.CompraCashback, Reverso, "", "")
                        Else
                            MensajeRespuesta = Enviar_Transaccion_CL(conector, pCuotas, pImporte, pCashback, ticketcaja, "", TransmisorTCP.TipoMensaje.Compra, Reverso, "", "")
                        End If

                        If MensajeRespuesta IsNot Nothing Then
                            Msg("-------------------------------")
                            Msg("     RESPUESTA DEL SERVIDOR")
                            Msg("-------------------------------")
                            Msg("Codigo de respuesta       : " & MensajeRespuesta.codRespuesta)
                            Msg("Respuesta                 : " & MensajeRespuesta.Respuesta)
                            Msg("Importe trx               : " & MensajeRespuesta.Importe)
                            Msg("Emisor                    : " & MensajeRespuesta.Emisor)
                            Msg("Mensaje Host              : " & MensajeRespuesta.mensajeHost)


                            If MensajeRespuesta.codRespuesta = "00" Then
                                Respuesta.Response = ResponseDescription.APROBADA
                                Respuesta.TotalAmount = MensajeRespuesta.Importe
                            Else
                                Respuesta.Response = ResponseDescription.NO_APROBADA
                            End If
                            Respuesta.Emisor = MensajeRespuesta.Emisor
                            Respuesta.DisplayString = MensajeRespuesta.Respuesta
                            Respuesta.TicketString = MensajeRespuesta.cupon

                            'Return mensajeRespuesta.codRespuesta
                            'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                        Else
                            '---NO HUBO RESPUESTA
                            'Respuesta.TicketString = Ticket_SinRepuesta(Reverso)
                            Reversar(Reverso)
                            Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                            Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."

                        End If
                    Else
                        'ERROR EN EL DESPERTAR AL PINPAD
                        mostrar_error(Respuesta, conector)

                    End If
                Else
                    Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                    Respuesta.DisplayString = "El SP no respondió a la consulta de Claves. Reintente."
                    conector.Cancelar()

                End If
            End Using
        Else
            Respuesta.Response = ResponseDescription.ERROR_EN_PP
            Respuesta.DisplayString = "No se puede utilizar el PP, falta sincronizar o inicializar."

        End If
        Return Respuesta

    End Function

    Private Function Ticket_SinRepuesta(req As TransmisorTCP.MensajeIda) As String
        Dim ticketString As String = ""

        If req.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Then  '"compra_cashback" Then
            ticketString = ticketString & "       COMPRA + EXTRACCION" & vbNewLine

        Else
            ticketString = ticketString & req.tipoMensaje & vbNewLine
        End If
        ticketString = ticketString & "             PINGUINO " & req.ping & vbNewLine
        ticketString = ticketString & "             CAJA: " & req.caja

        ticketString = ticketString & vbNewLine
        ticketString = ticketString & vbNewLine
        ticketString = ticketString & vbNewLine
        ticketString = ticketString & "              NO HAY RESPUESTA" & vbNewLine
        ticketString = ticketString & vbNewLine
        ticketString = ticketString & "             POR FAVOR REINTENTE " & vbNewLine

        Return ticketString
    End Function



    Private Sub Datos_Adicionales_Manual(pConector As lib_PP.ConectorPPSinEncripcion)
        Msg("-------- Respuesta a Y02 (Manual) ----------")
        'Msg("Nro Tarjeta                   : " & pConector.RespuestaDatosAdicionales.tarjeta)
        Msg("Tipo de Encripcion            : " & pConector.RespuestaDatosAdicionales.tipoEncripcion)
        If pConector.RespuestaDatosAdicionales.tipoEncripcion = "N" Then
            Msg("Fecha de vencimiento          : " & pConector.RespuestaDatosAdicionales.fecha_vencimiento)
            Msg("Codigo de seguridad           : " & pConector.RespuestaDatosAdicionales.codigo_seguridad)
        Else
            Msg("ID Encripcion                 : " & pConector.RespuestaDatosAdicionales.idEncription)
            Msg("Paquete Encriptado            : " & pConector.RespuestaDatosAdicionales.Paquete)
        End If
        Msg("Nro Serie Fisico              : " & pConector.RespuestaDatosAdicionales.serieFisico)
        Msg("Tipo de cuenta                : " & pConector.RespuestaDatosAdicionales.tipoCuenta)
        Msg("PIN Block                     : " & pConector.RespuestaDatosAdicionales.PinBlock)

    End Sub

    Private Sub Datos_Adicionales_Banda(pConector As lib_PP.ConectorPPSinEncripcion)

        Msg("-------- Respuesta a Y02 (Banda) ----------")
        'Msg("Nro Tarjeta                   : " & pConector.RespuestaDatosAdicionales.tarjeta)
        Msg("Nro Serie Fisico              : " & pConector.RespuestaDatosAdicionales.serieFisico)
        Msg("Tipo de Encripcion            : " & pConector.RespuestaDatosAdicionales.tipoEncripcion)
        If pConector.RespuestaDatosAdicionales.tipoEncripcion <> "N" Then
            Msg("ID Encripcion                 : " & pConector.RespuestaDatosAdicionales.idEncription)
            Msg("Paquete Encriptado            : " & pConector.RespuestaDatosAdicionales.Paquete)
        Else
            Msg("TRACK 1                       : " & pConector.RespuestaDatosAdicionales.track1)
            Msg("TRACK 2                       : " & pConector.RespuestaDatosAdicionales.track2)
            Msg("TRACK 1 NO LEIDO              : " & pConector.RespuestaDatosAdicionales.track1_no_leido)
            Msg("Fecha de vencimiento          : " & pConector.RespuestaDatosAdicionales.fecha_vencimiento)
            Msg("Codigo de seguridad           : " & pConector.RespuestaDatosAdicionales.codigo_seguridad)

        End If

        Msg("Tipo de cuenta                : " & pConector.RespuestaDatosAdicionales.tipoCuenta)
        Msg("PIN Block                     : " & pConector.RespuestaDatosAdicionales.PinBlock)
        Msg("Nombre Tarjeta Habiente       : " & pConector.RespuestaDatosAdicionales.TarjetaHabiente)


    End Sub
    Private Sub Datos_Adicionales_Chip(pConector As lib_PP.ConectorPPSinEncripcion)

        Msg("-------- Respuesta a Y02 (Chip) ----------")
        'Msg("Nro Tarjeta                   : " & pConector.RespuestaDatosAdicionales.tarjeta)
        Msg("Nro Serie Fisico              : " & pConector.RespuestaDatosAdicionales.serieFisico)
        Msg("ID Encripcion                 : " & pConector.RespuestaDatosAdicionales.idEncription)
        Msg("Tipo de Encripcion            : " & pConector.RespuestaDatosAdicionales.tipoEncripcion)
        Msg("Paquete Encriptado            : " & pConector.RespuestaDatosAdicionales.Paquete)
        Msg("Criptograma                   : " & pConector.RespuestaDatosAdicionales.criptograma)
        Msg("Codigo de autorización        : " & pConector.RespuestaDatosAdicionales.autorizacion)
        Msg("Codigo de respuesta del emisor: " & pConector.RespuestaDatosAdicionales.respEmisor)
        Msg("Secuencia PAN                 : " & pConector.RespuestaDatosAdicionales.secuenciaPAN)
        Msg("Tipo de cuenta                : " & pConector.RespuestaDatosAdicionales.tipoCuenta)
        Msg("PIN Block                     : " & pConector.RespuestaDatosAdicionales.PinBlock)
        Msg("Nombre de aplicacion          : " & pConector.RespuestaDatosAdicionales.appNombre)
        Msg("Identificador de aplicacion   : " & pConector.RespuestaDatosAdicionales.ID_app)
        Msg("Nombre Tarjeta Habiente       : " & pConector.RespuestaDatosAdicionales.TarjetaHabiente)

    End Sub

    Private Sub mostrar_error(ByRef _respuesta As ResponseToPOS, conector As lib_PP.ConectorPPSinEncripcion)
        Msg("-------- Respuesta a Y0E ----------")
        Msg("Comando que generó el error   : " & conector.RespuestaErrores.comandoError)
        Msg("Código de error               : " & conector.RespuestaErrores.codigoError)
        Msg("Descripcion de error          : " & conector.RespuestaErrores.descripcion)
        Msg("Cod. error extendido          : " & conector.RespuestaErrores.extendido)
        _respuesta.Response = ResponseDescription.ERROR_EN_PP
        '_respuesta.DisplayString = String.Format("Cód. Error PP: {0} generado en comando {1}, Descripción: {2}, Cód Extendido: {3}",
        '                                         conector.RespuestaErrores.codigoError,
        '                                         conector.RespuestaErrores.comandoError,
        '                                         conector.RespuestaErrores.descripcion,
        '                                         conector.RespuestaErrores.extendido)

        _respuesta.DisplayString = String.Format("** Error PinPad: Código: {0}   -   {1}  **",
                                                 conector.RespuestaErrores.codigoError,
                                                 conector.RespuestaErrores.descripcion)

    End Sub




    Private Sub Datos_Adicionales_Manual(pConector As lib_PP.ConectorPP)
        Msg("-------- Respuesta a Y02 (Manual) ----------")
        'Msg("Nro Tarjeta                   : " & pConector.RespuestaDatosAdicionales.tarjeta)
        Msg("Tipo de Encripcion            : " & pConector.RespuestaDatosAdicionales.tipoEncripcion)
        If pConector.RespuestaDatosAdicionales.tipoEncripcion = "N" Then
            Msg("Fecha de vencimiento          : " & pConector.RespuestaDatosAdicionales.fecha_vencimiento)
            Msg("Codigo de seguridad           : " & pConector.RespuestaDatosAdicionales.codigo_seguridad)
        Else
            Msg("ID Encripcion                 : " & pConector.RespuestaDatosAdicionales.idEncription)
            Msg("Paquete Encriptado            : " & pConector.RespuestaDatosAdicionales.Paquete)
        End If
        Msg("Nro Serie Fisico              : " & pConector.RespuestaDatosAdicionales.serieFisico)
        Msg("Tipo de cuenta                : " & pConector.RespuestaDatosAdicionales.tipoCuenta)
        Msg("PIN Block                     : " & pConector.RespuestaDatosAdicionales.PinBlock)
    End Sub

    Private Sub Datos_Adicionales_Banda(pConector As lib_PP.ConectorPP)
        Msg("-------- Respuesta a Y02 (Banda) ----------")
        'Msg("Nro Tarjeta                   : " & pConector.RespuestaDatosAdicionales.tarjeta)
        Msg("Nro Serie Fisico              : " & pConector.RespuestaDatosAdicionales.serieFisico)
        Msg("Tipo de Encripcion            : " & pConector.RespuestaDatosAdicionales.tipoEncripcion)
        If pConector.RespuestaDatosAdicionales.tipoEncripcion <> "N" Then
            Msg("ID Encripcion                 : " & pConector.RespuestaDatosAdicionales.idEncription)
            Msg("Paquete Encriptado            : " & pConector.RespuestaDatosAdicionales.Paquete)
        Else
            Msg("TRACK 1                       : " & pConector.RespuestaDatosAdicionales.track1)
            Msg("TRACK 2                       : " & pConector.RespuestaDatosAdicionales.track2)
            Msg("TRACK 1 NO LEIDO              : " & pConector.RespuestaDatosAdicionales.track1_no_leido)
            Msg("Fecha de vencimiento          : " & pConector.RespuestaDatosAdicionales.fecha_vencimiento)
            Msg("Codigo de seguridad           : " & pConector.RespuestaDatosAdicionales.codigo_seguridad)
        End If
        Msg("Tipo de cuenta                : " & pConector.RespuestaDatosAdicionales.tipoCuenta)
        Msg("PIN Block                     : " & pConector.RespuestaDatosAdicionales.PinBlock)
        Msg("Nombre Tarjeta Habiente       : " & pConector.RespuestaDatosAdicionales.TarjetaHabiente)
    End Sub

    Private Sub Datos_Adicionales_Chip(pConector As lib_PP.ConectorPP)

        Msg("-------- Respuesta a Y02 (Chip) ----------")
        'Msg("Nro Tarjeta                   : " & pConector.RespuestaDatosAdicionales.tarjeta)
        Msg("Nro Serie Fisico              : " & pConector.RespuestaDatosAdicionales.serieFisico)
        Msg("ID Encripcion                 : " & pConector.RespuestaDatosAdicionales.idEncription)
        Msg("Tipo de Encripcion            : " & pConector.RespuestaDatosAdicionales.tipoEncripcion)
        Msg("Paquete Encriptado            : " & pConector.RespuestaDatosAdicionales.Paquete)
        Msg("Criptograma                   : " & pConector.RespuestaDatosAdicionales.criptograma)
        Msg("Codigo de autorización        : " & pConector.RespuestaDatosAdicionales.autorizacion)
        Msg("Codigo de respuesta del emisor: " & pConector.RespuestaDatosAdicionales.respEmisor)
        Msg("Secuencia PAN                 : " & pConector.RespuestaDatosAdicionales.secuenciaPAN)
        Msg("Tipo de cuenta                : " & pConector.RespuestaDatosAdicionales.tipoCuenta)
        Msg("PIN Block                     : " & pConector.RespuestaDatosAdicionales.PinBlock)
        Msg("Nombre de aplicacion          : " & pConector.RespuestaDatosAdicionales.appNombre)
        Msg("Identificador de aplicacion   : " & pConector.RespuestaDatosAdicionales.ID_app)
        Msg("Nombre Tarjeta Habiente       : " & pConector.RespuestaDatosAdicionales.TarjetaHabiente)

    End Sub

    Private Sub mostrar_error(ByRef _respuesta As ResponseToPOS, conector As lib_PP.ConectorPP)
        Msg("-------- Respuesta a Y0E ----------")
        Msg("Comando que generó el error   : " & conector.RespuestaErrores.comandoError)
        Msg("Código de error               : " & conector.RespuestaErrores.codigoError)
        Msg("Descripcion de error          : " & conector.RespuestaErrores.descripcion)
        Msg("Cod. error extendido          : " & conector.RespuestaErrores.extendido)
        _respuesta.Response = ResponseDescription.ERROR_EN_PP
        '_respuesta.DisplayString = String.Format("Cód. Error PP: {0} generado en comando {1}, Descripción: {2}, Cód Extendido: {3}",
        '                                         conector.RespuestaErrores.codigoError,
        '                                         conector.RespuestaErrores.comandoError,
        '                                         conector.RespuestaErrores.descripcion,
        '                                         conector.RespuestaErrores.extendido)

        _respuesta.DisplayString = String.Format("*** Error PinPad: Código: {0}   -   {1} ***",
                                                 conector.RespuestaErrores.codigoError,
                                                 conector.RespuestaErrores.descripcion)

    End Sub


    Private Sub Setear_Plan(req As TransmisorTCP.MensajeIda, cuotas As Integer)
        'If cuotas = 1 Then
        '    req.plan = 0
        'ElseIf cuotas > 1 And cuotas <100 Then
        '    req.plan = cuotas + 100
        'Else
        '    req.plan = cuotas
        'End If
        req.plan = cuotas
    End Sub

    Private Function generar_paquete(conector As ConectorPP, clave As String) As String
        Dim s As String = ""
        Dim campos As New List(Of Integer)
        Select Case conector.RespuestaDatosAdicionales.modo
            Case "M"
                s += conector.RespuestaDatosAdicionales.tarjeta.PadRight(20, " ") & conector.RespuestaDatosAdicionales.fecha_vencimiento.PadRight(4, " ")
                campos.Add(1)
                campos.Add(2)
            Case "B"
                s += conector.RespuestaDatosAdicionales.track2
                campos.Add(3)
                If conector.RespuestaDatosAdicionales.track1_no_leido = "Leido" Then
                    s += conector.RespuestaDatosAdicionales.track1
                    campos.Add(4)
                Else
                    s += "1"
                    campos.Add(5)
                End If

            Case "C"
        End Select
        If conector.RespuestaDatosAdicionales.codigo_seguridad <> "" Then
            s += conector.RespuestaDatosAdicionales.codigo_seguridad.PadRight(4, " ")
            campos.Add(6)
        End If
        s = setear_BitMap(campos) + s

        Return DES_Encriptor(s, clave)
        'Return TjComun.crypto.DESEncrypt(s, clave)
    End Function

    Private Function setear_BitMap(s As List(Of Integer)) As String
        Dim bitarray() As String = {0, 0, 0, 0, 0, 0, 0, 0}
        For Each b In s
            bitarray(b - 1) = 1
        Next
        Return Hex(bcd(bitarray))

    End Function

    Private Function bcd(s() As String) As Integer
        Dim peso() As Integer = {128, 64, 32, 16, 8, 4, 2, 1}
        Dim suma As Integer = 0
        Dim h As Integer = 0
        For Each c In s
            suma += c * peso(h)
            h += 1
        Next
        Return suma
    End Function


    ''' <summary>
    ''' Para devolucion.
    ''' </summary>
    ''' <param name="conector"></param>
    ''' <param name="cuotas"></param>
    ''' <param name="importe"></param>
    ''' <param name="cashback"></param>
    ''' <param name="cajera"></param>
    ''' <param name="clave"></param>
    ''' <param name="fechaOri"></param>
    ''' <param name="ticketori"></param>
    ''' <returns></returns>
    Private Function Enviar_TransaccionDev(conector As ConectorPP, cuotas As Integer, importe As Decimal, cashback As Decimal, cajera As Integer, clave As String, fechaOri As String, ticketori As String, ticketcaja As Integer, ByRef rev As TransmisorTCP.MensajeIda) As MensajeRespuesta

        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.cajera = cajera
        Setear_Plan(req, cuotas)
        If fechaOri.Length <> 6 Then
            Dim resp As New MensajeRespuesta
            resp.codRespuesta = "XX"
            resp.Respuesta = "Fecha original mal formada, transacción no enviada. Utilice ddMMyy."
            Return resp
            Exit Function
        End If
        req.fechaOperacionOriginal = CDate(fechaOri.Substring(0, 2) & "/" & fechaOri.Substring(2, 2) & "/" & fechaOri.Substring(4, 2))
        req.ticketOriginal = ticketori

        req.ticketCaja = ticketcaja
        req.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion
        req.nroTarjeta = conector.RespuestaDatosAdicionales.tarjeta
        req.secuencia = conector.RespuestaDatosAdicionales.secuenciaPAN

        If conector.RespuestaDatosAdicionales.tipoEncripcion = "N" Then
            req.datosEncriptados = generar_paquete(conector, clave)
        Else
            req.datosEncriptados = conector.RespuestaDatosAdicionales.Paquete
        End If

        'If tipoTrx = TransmisorTCP.TipoMensaje.Compra Then Setear_Plan(req, cuotas)
        req.moneda = Moneda.Pesos
        req.versionAPP = versionAPP
        req.criptograma = conector.RespuestaDatosAdicionales.criptograma
        req.fechaTransaccion = Now
        req.importeCompra = importe
        req.importeCashback = cashback
        req.idEncripcion = conector.RespuestaDatosAdicionales.idEncription
        req.tipocuenta = conector.RespuestaDatosAdicionales.tipoCuenta
        req.pinBlock = conector.RespuestaDatosAdicionales.PinBlock
        req.nombreTHabiente = conector.RespuestaDatosAdicionales.TarjetaHabiente

        Select Case conector.RespuestaInicioCompra.modoIngreso
            Case "B"
                req.tipoIngreso = TipoIngreso.Banda
            Case "M"
                req.tipoIngreso = TipoIngreso.Manual
            Case "C"
                req.tipoIngreso = TipoIngreso.Chip
            Case "L"
                req.tipoIngreso = TipoIngreso.Contactless
        End Select
        req.datosemisor = ""
        req.appID = conector.RespuestaDatosAdicionales.ID_app
        req.nombreApplicacion = conector.RespuestaDatosAdicionales.appNombre
        req.serviceCode = conector.RespuestaInicioCompra.codigoServicio
        rev = req.Clone
        rev.reversar = True

        revtest = req.Clone
        revtest.reversar = True


        Return Transmitir(req)


    End Function


    ''' <summary>
    ''' Para anulacion.
    ''' </summary>
    ''' <param name="conector"></param>
    ''' <param name="cuotas"></param>
    ''' <param name="importe"></param>
    ''' <param name="cashback"></param>
    ''' <param name="cajera"></param>
    ''' <param name="clave"></param>
    ''' <param name="retrefnumber"></param>
    ''' <param name="ticketori"></param>
    ''' <returns></returns>
    Private Function Enviar_Transaccion(conector As ConectorPP, cuotas As Integer, importe As Decimal, cashback As Decimal, cajera As Integer, clave As String, retrefnumber As String, ticketori As String, tipo As TransmisorTCP.TipoMensaje, ticket As Integer, ByRef rev As TransmisorTCP.MensajeIda, eldoc As String) As MensajeRespuesta

        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.cajera = cajera

        req.referenceNumber = retrefnumber
        req.ticketOriginal = ticketori

        req.ticketCaja = ticket
        req.tipoMensaje = tipo
        'req.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion
        req.nroTarjeta = conector.RespuestaDatosAdicionales.tarjeta
        req.secuencia = conector.RespuestaDatosAdicionales.secuenciaPAN

        If conector.RespuestaDatosAdicionales.tipoEncripcion = "N" Then
            req.datosEncriptados = generar_paquete(conector, clave)
        Else
            req.datosEncriptados = conector.RespuestaDatosAdicionales.Paquete
        End If

        Setear_Plan(req, cuotas)
        req.moneda = Moneda.Pesos
        req.versionAPP = versionAPP
        req.criptograma = conector.RespuestaDatosAdicionales.criptograma
        req.fechaTransaccion = Now
        req.importeCompra = importe
        req.importeCashback = cashback
        req.idEncripcion = conector.RespuestaDatosAdicionales.idEncription
        req.tipocuenta = conector.RespuestaDatosAdicionales.tipoCuenta
        req.pinBlock = conector.RespuestaDatosAdicionales.PinBlock
        req.nombreTHabiente = conector.RespuestaDatosAdicionales.TarjetaHabiente

        Select Case conector.RespuestaInicioCompra.modoIngreso
            Case "B"
                req.tipoIngreso = TipoIngreso.Banda
            Case "M"
                req.tipoIngreso = TipoIngreso.Manual
            Case "C"
                req.tipoIngreso = TipoIngreso.Chip
            Case "L"
                req.tipoIngreso = TipoIngreso.Contactless
        End Select
        req.datosemisor = ""
        req.appID = conector.RespuestaDatosAdicionales.ID_app
        req.nombreApplicacion = conector.RespuestaDatosAdicionales.appNombre
        req.serviceCode = conector.RespuestaInicioCompra.codigoServicio
        rev = req.Clone
        rev.reversar = True

        revtest = req.Clone
        revtest.reversar = True


        Return Transmitir(req)


    End Function









    ''' <summary>
    ''' Para Advice.
    ''' </summary>
    ''' <param name="conector"></param>
    ''' <param name="cuotas"></param>
    ''' <param name="importe"></param>
    ''' <param name="cashback"></param>
    ''' <param name="cajera"></param>
    ''' <param name="clave"></param>
    ''' <param name="retrefnumber"></param>
    ''' <param name="ticketori"></param>
    ''' <returns></returns>
    Private Function Enviar_TransaccionAdvice(conector As ConectorPP, cuotas As Integer, importe As Decimal, cashback As Decimal, cajera As Integer, clave As String, retrefnumber As String, ticketori As String, tipo As TransmisorTCP.TipoMensaje, ticket As Integer, ByRef rev As TransmisorTCP.MensajeIda, eldoc As String) As MensajeRespuesta

        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.cajera = cajera

        req.referenceNumber = retrefnumber
        req.ticketOriginal = "Z3      "     ' ticketori

        req.ticketCaja = ticket
        req.tipoMensaje = tipo
        'req.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion
        req.nroTarjeta = conector.RespuestaDatosAdicionales.tarjeta
        req.secuencia = conector.RespuestaDatosAdicionales.secuenciaPAN

        If conector.RespuestaDatosAdicionales.tipoEncripcion = "N" Then
            req.datosEncriptados = generar_paquete(conector, clave)
        Else
            req.datosEncriptados = conector.RespuestaDatosAdicionales.Paquete
        End If

        Setear_Plan(req, cuotas)
        req.moneda = Moneda.Pesos
        req.versionAPP = versionAPP
        req.criptograma = conector.RespuestaDatosAdicionales.criptograma
        req.fechaTransaccion = Now
        req.importeCompra = importe
        req.importeCashback = cashback
        req.idEncripcion = conector.RespuestaDatosAdicionales.idEncription
        req.tipocuenta = conector.RespuestaDatosAdicionales.tipoCuenta
        req.pinBlock = conector.RespuestaDatosAdicionales.PinBlock
        req.nombreTHabiente = conector.RespuestaDatosAdicionales.TarjetaHabiente

        Select Case conector.RespuestaInicioCompra.modoIngreso
            Case "B"
                req.tipoIngreso = TipoIngreso.Banda
            Case "M"
                req.tipoIngreso = TipoIngreso.Manual
            Case "C"
                req.tipoIngreso = TipoIngreso.Chip
            Case "L"
                req.tipoIngreso = TipoIngreso.Contactless
        End Select
        req.datosemisor = ""
        req.appID = conector.RespuestaDatosAdicionales.ID_app
        req.nombreApplicacion = conector.RespuestaDatosAdicionales.appNombre
        req.serviceCode = conector.RespuestaInicioCompra.codigoServicio
        rev = req.Clone
        rev.reversar = True

        revtest = req.Clone
        revtest.reversar = True


        Return Transmitir(req)


    End Function














    ''' <summary>
    ''' Para compra
    ''' </summary>
    ''' <param name="conector"></param>
    ''' <param name="cuotas"></param>
    ''' <param name="importe"></param>
    ''' <param name="cashback"></param>
    ''' <param name="ticket"></param>
    ''' <param name="clave"></param>
    ''' <param name="tipoTrx"></param>
    ''' <returns></returns>
    Private Function Enviar_Transaccion(conector As ConectorPP, cuotas As Integer, importe As Decimal, cashback As Decimal, ticket As Integer, clave As String, tipoTrx As TransmisorTCP.TipoMensaje, ByRef rev As TransmisorTCP.MensajeIda, eldoc As String) As MensajeRespuesta

        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.cajera = cajera

        req.ticketCaja = ticket
        req.tipoMensaje = tipoTrx
        req.nroTarjeta = conector.RespuestaDatosAdicionales.tarjeta
        req.secuencia = conector.RespuestaDatosAdicionales.secuenciaPAN

        If conector.RespuestaDatosAdicionales.tipoEncripcion = "N" Then
            req.track2 = conector.RespuestaDatosAdicionales.track2
            req.track1 = conector.RespuestaDatosAdicionales.track1
            'req.datosEncriptados = generar_paquete(conector, clave)
        Else
            req.datosEncriptados = conector.RespuestaDatosAdicionales.Paquete
        End If

        If tipoTrx = TransmisorTCP.TipoMensaje.Compra Or tipoTrx = TransmisorTCP.TipoMensaje.CompraCashback Then Setear_Plan(req, cuotas)
        req.moneda = Moneda.Pesos
        req.versionAPP = versionAPP
        req.criptograma = conector.RespuestaDatosAdicionales.criptograma
        req.fechaTransaccion = Now
        req.importeCompra = importe
        req.importeCashback = cashback
        req.idEncripcion = conector.RespuestaDatosAdicionales.idEncription
        req.tipocuenta = conector.RespuestaDatosAdicionales.tipoCuenta
        req.pinBlock = conector.RespuestaDatosAdicionales.PinBlock
        req.nombreTHabiente = conector.RespuestaDatosAdicionales.TarjetaHabiente
        req.reversar = False
        Select Case conector.RespuestaInicioCompra.modoIngreso
            Case "B"
                req.tipoIngreso = TipoIngreso.Banda
            Case "M"
                req.tipoIngreso = TipoIngreso.Manual
            Case "C"
                req.tipoIngreso = TipoIngreso.Chip
            Case "L"
                req.tipoIngreso = TipoIngreso.Contactless
        End Select

        req.serviceCode = conector.RespuestaInicioCompra.codigoServicio
        req.nombreApplicacion = conector.RespuestaDatosAdicionales.appNombre
        req.appID = conector.RespuestaDatosAdicionales.ID_app
        req.datosemisor = ""
        req.NroDocumentoTicket = eldoc
        'If tipoTrx <> TransmisorTCP.TipoMensaje.Reverso Then
        rev = req.Clone
        rev.reversar = True

        revtest = req.Clone
        revtest.reversar = True


        'rev.tipoMensaje = TransmisorTCP.TipoMensaje.Reverso
        'End If

        Return Transmitir(req)


    End Function






    ''' <summary>
    ''' Para compra
    ''' </summary>
    ''' <param name="conector"></param>
    ''' <param name="cuotas"></param>
    ''' <param name="importe"></param>
    ''' <param name="cashback"></param>
    ''' <param name="ticket"></param>
    ''' <param name="clave"></param>
    ''' <param name="tipoTrx"></param>
    ''' <returns></returns>
    Private Function Enviar_TransaccionAdv(conector As ConectorPP, cuotas As Integer, importe As Decimal, cashback As Decimal, ticket As Integer, clave As String, tipoTrx As TransmisorTCP.TipoMensaje, ByRef rev As TransmisorTCP.MensajeIda, eldoc As String) As MensajeRespuesta

        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.cajera = cajera

        req.ticketCaja = ticket
        req.tipoMensaje = tipoTrx
        req.nroTarjeta = conector.RespuestaDatosAdicionales.tarjeta
        req.secuencia = conector.RespuestaDatosAdicionales.secuenciaPAN
        req.CodAutorizaAdv = conector.RespuestaDatosAdicionales.autorizacion

        If conector.RespuestaDatosAdicionales.tipoEncripcion = "N" Then
            req.track2 = conector.RespuestaDatosAdicionales.track2
            req.track1 = conector.RespuestaDatosAdicionales.track1
            'req.datosEncriptados = generar_paquete(conector, clave)
        Else
            req.datosEncriptados = conector.RespuestaDatosAdicionales.Paquete
        End If

        If tipoTrx = TransmisorTCP.TipoMensaje.Compra Or tipoTrx = TransmisorTCP.TipoMensaje.CompraCashback Then Setear_Plan(req, cuotas)
        req.moneda = Moneda.Pesos
        req.versionAPP = versionAPP
        req.criptograma = conector.RespuestaDatosAdicionales.criptograma
        req.fechaTransaccion = Now
        req.importeCompra = importe
        req.importeCashback = cashback
        req.idEncripcion = conector.RespuestaDatosAdicionales.idEncription
        req.tipocuenta = conector.RespuestaDatosAdicionales.tipoCuenta
        req.pinBlock = conector.RespuestaDatosAdicionales.PinBlock
        req.nombreTHabiente = conector.RespuestaDatosAdicionales.TarjetaHabiente
        req.reversar = False
        Select Case conector.RespuestaInicioCompra.modoIngreso
            Case "B"
                req.tipoIngreso = TipoIngreso.Banda
            Case "M"
                req.tipoIngreso = TipoIngreso.Manual
            Case "C"
                req.tipoIngreso = TipoIngreso.Chip
            Case "L"
                req.tipoIngreso = TipoIngreso.Contactless
        End Select

        req.serviceCode = conector.RespuestaInicioCompra.codigoServicio
        req.nombreApplicacion = conector.RespuestaDatosAdicionales.appNombre
        req.appID = conector.RespuestaDatosAdicionales.ID_app
        req.datosemisor = ""
        req.NroDocumentoTicket = eldoc
        'If tipoTrx <> TransmisorTCP.TipoMensaje.Reverso Then
        rev = req.Clone
        rev.reversar = True

        revtest = req.Clone
        revtest.reversar = True


        'rev.tipoMensaje = TransmisorTCP.TipoMensaje.Reverso
        'End If

        Return Transmitir(req)


    End Function



    ''' <summary>
    ''' Para anulacion contactless
    ''' </summary>
    ''' <param name="conector"></param>
    ''' <param name="cuotas"></param>
    ''' <param name="importe"></param>
    ''' <param name="cashback"></param>
    ''' <param name="ticket"></param>
    ''' <param name="clave"></param>
    ''' <param name="tipoTrx"></param>
    ''' <returns></returns>
    Private Function Enviar_Transaccion_CL(conector As ConectorPP, cuotas As Integer, importe As Decimal, cashback As Decimal, ticket As Integer, clave As String, tipoTrx As TransmisorTCP.TipoMensaje, retrefnumber As String, tktori As Integer, ByRef rev As TransmisorTCP.MensajeIda, eldoc As String, tipCueny07 As String) As MensajeRespuesta

        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.cajera = cajera

        req.referenceNumber = retrefnumber
        req.ticketOriginal = tktori


        req.ticketCaja = ticket
        req.tipoMensaje = tipoTrx
        req.nroTarjeta = conector.RespuestaContactless.tarjeta
        req.secuencia = conector.RespuestaContactless.secuencia

        If conector.RespuestaContactless.tipoEncripcion = "N" Then
            req.datosEncriptados = generar_paquete(conector, clave)
        Else
            req.datosEncriptados = conector.RespuestaContactless.paquete
        End If

        If tipoTrx = TransmisorTCP.TipoMensaje.Compra Or tipoTrx = TransmisorTCP.TipoMensaje.CompraCashback Then Setear_Plan(req, cuotas)
        req.moneda = Moneda.Pesos
        req.versionAPP = versionAPP
        req.criptograma = conector.RespuestaContactless.criptograma
        req.fechaTransaccion = Now
        req.importeCompra = importe
        req.importeCashback = cashback
        req.idEncripcion = conector.RespuestaContactless.idEncripcion
        req.tipocuenta = conector.RespuestaContactless.tipoCuenta
        req.pinBlock = conector.RespuestaContactless.pin
        req.nombreTHabiente = conector.RespuestaContactless.nombreThab
        req.reversar = False

        req.tipoIngreso = TipoIngreso.Contactless
        req.serviceCode = conector.RespuestaContactless.codigoServicio
        If conector.RespuestaContactless.datosEmisor = "" Then
            req.datosemisor = "00"
        Else
            req.datosemisor = conector.RespuestaContactless.datosEmisor
        End If
        req.nombreApplicacion = conector.RespuestaContactless.nombreApp
        req.appID = conector.RespuestaContactless.idApp

        req.NroDocumentoTicket = eldoc
        req.TipoCuentaY07 = tipCueny07
        'If tipoTrx <> TransmisorTCP.TipoMensaje.Reverso Then
        rev = req.Clone
        rev.reversar = True

        revtest = req.Clone
        revtest.reversar = True

        'rev.tipoMensaje = TransmisorTCP.TipoMensaje.Reverso
        'End If

        Return Transmitir(req)


    End Function


    ''' <summary>
    ''' Para devolucion contactless
    ''' </summary>
    ''' <param name="conector"></param>
    ''' <param name="cuotas"></param>
    ''' <param name="importe"></param>
    ''' <param name="cashback"></param>
    ''' <param name="clave"></param>
    ''' <param name="tktori"></param>
    ''' <param name="fecori">Formato ddMMyy</param>
    ''' <param name="rev"></param>
    ''' <returns></returns>
    Private Function Enviar_Transaccion_CL(conector As ConectorPP, cuotas As Integer, importe As Decimal, cashback As Decimal, clave As String, tktori As Integer, fecori As String, ticketcaja As Integer, ByRef rev As TransmisorTCP.MensajeIda, eldoc As String, tipCueny07 As String) As MensajeRespuesta

        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.cajera = cajera

        If fecori.Length <> 6 Then
            Dim resp As New MensajeRespuesta
            resp.codRespuesta = "XX"
            resp.Respuesta = "Fecha original mal formada, transacción no enviada. Utilice ddMMyy."
            Return resp
            Exit Function
        End If
        req.fechaOperacionOriginal = CDate(fecori.Substring(0, 2) & "/" & fecori.Substring(2, 2) & "/" & fecori.Substring(4, 2))
        req.ticketOriginal = tktori


        req.ticketCaja = ticketcaja
        req.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion
        req.nroTarjeta = conector.RespuestaContactless.tarjeta
        req.secuencia = conector.RespuestaContactless.secuencia

        If conector.RespuestaContactless.tipoEncripcion = "N" Then
            req.datosEncriptados = generar_paquete(conector, clave)
        Else
            req.datosEncriptados = conector.RespuestaContactless.paquete
        End If

        Setear_Plan(req, cuotas)
        req.moneda = Moneda.Pesos
        req.versionAPP = versionAPP
        req.criptograma = conector.RespuestaContactless.criptograma
        req.fechaTransaccion = Now
        req.importeCompra = importe
        req.importeCashback = cashback
        req.idEncripcion = conector.RespuestaContactless.idEncripcion
        req.tipocuenta = conector.RespuestaContactless.tipoCuenta
        req.pinBlock = conector.RespuestaContactless.pin
        req.nombreTHabiente = conector.RespuestaContactless.nombreThab
        req.reversar = False

        req.tipoIngreso = TipoIngreso.Contactless
        req.serviceCode = conector.RespuestaContactless.codigoServicio
        If conector.RespuestaContactless.datosEmisor = "" Then
            req.datosemisor = "00"
        Else
            req.datosemisor = conector.RespuestaContactless.datosEmisor
        End If
        req.nombreApplicacion = conector.RespuestaContactless.nombreApp
        req.appID = conector.RespuestaContactless.idApp

        req.NroDocumentoTicket = eldoc
        req.TipoCuentaY07 = tipCueny07
        'If tipoTrx <> TransmisorTCP.TipoMensaje.Reverso Then
        rev = req.Clone
        rev.reversar = True

        revtest = req.Clone
        revtest.reversar = True

        'rev.tipoMensaje = TransmisorTCP.TipoMensaje.Reverso
        'End If

        Return Transmitir(req)


    End Function

    ''' <summary>
    ''' Para compra contactless
    ''' </summary>
    ''' <param name="conector"></param>
    ''' <param name="cuotas"></param>
    ''' <param name="importe"></param>
    ''' <param name="cashback"></param>
    ''' <param name="ticket"></param>
    ''' <param name="clave"></param>
    ''' <param name="tipoTrx"></param>
    ''' <returns></returns>
    Private Function Enviar_Transaccion_CL(conector As ConectorPP, cuotas As Integer, importe As Decimal, cashback As Decimal, ticket As Integer, clave As String, tipoTrx As TransmisorTCP.TipoMensaje, ByRef rev As TransmisorTCP.MensajeIda, eldoc As String, tipCueny07 As String) As MensajeRespuesta

        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.cajera = cajera

        req.ticketCaja = ticket
        req.tipoMensaje = tipoTrx
        req.nroTarjeta = conector.RespuestaContactless.tarjeta
        req.secuencia = conector.RespuestaContactless.secuencia

        If conector.RespuestaContactless.tipoEncripcion = "N" Then
            req.datosEncriptados = generar_paquete(conector, clave)
        Else
            req.datosEncriptados = conector.RespuestaContactless.paquete
        End If

        If tipoTrx = TransmisorTCP.TipoMensaje.Compra Or tipoTrx = TransmisorTCP.TipoMensaje.CompraCashback Then Setear_Plan(req, cuotas)
        req.moneda = Moneda.Pesos
        req.versionAPP = versionAPP
        req.criptograma = conector.RespuestaContactless.criptograma
        req.fechaTransaccion = Now
        req.importeCompra = importe
        req.importeCashback = cashback
        req.idEncripcion = conector.RespuestaContactless.idEncripcion


        req.tipocuenta = conector.RespuestaContactless.tipoCuenta


        req.pinBlock = conector.RespuestaContactless.pin
        req.nombreTHabiente = conector.RespuestaContactless.nombreThab
        req.reversar = False

        req.tipoIngreso = TipoIngreso.Contactless
        req.serviceCode = conector.RespuestaContactless.codigoServicio
        If conector.RespuestaContactless.datosEmisor = "" Then
            req.datosemisor = "00"
        Else
            req.datosemisor = conector.RespuestaContactless.datosEmisor
        End If
        req.nombreApplicacion = conector.RespuestaContactless.nombreApp
        req.appID = conector.RespuestaContactless.idApp

        req.NroDocumentoTicket = eldoc
        req.TipoCuentaY07 = tipCueny07


        'If tipoTrx <> TransmisorTCP.TipoMensaje.Reverso Then
        rev = req.Clone
        rev.reversar = True

        revtest = req.Clone
        revtest.reversar = True

        'rev.tipoMensaje = TransmisorTCP.TipoMensaje.Reverso
        'End If

        Return Transmitir(req)


    End Function



    Private Function Enviar_Transaccion_CL_Adv(conector As ConectorPP, cuotas As Integer, importe As Decimal, cashback As Decimal, ticket As Integer, clave As String, tipoTrx As TransmisorTCP.TipoMensaje, ByRef rev As TransmisorTCP.MensajeIda, eldoc As String) As MensajeRespuesta

        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.cajera = cajera

        req.ticketCaja = ticket
        req.tipoMensaje = tipoTrx
        req.nroTarjeta = conector.RespuestaContactless.tarjeta
        req.secuencia = conector.RespuestaContactless.secuencia
        req.CodAutorizaAdv = conector.RespuestaContactless.autorizacion

        If conector.RespuestaContactless.tipoEncripcion = "N" Then
            req.datosEncriptados = generar_paquete(conector, clave)
        Else
            req.datosEncriptados = conector.RespuestaContactless.paquete
        End If

        If tipoTrx = TransmisorTCP.TipoMensaje.Compra Or tipoTrx = TransmisorTCP.TipoMensaje.CompraCashback Then Setear_Plan(req, cuotas)
        req.moneda = Moneda.Pesos
        req.versionAPP = versionAPP
        req.criptograma = conector.RespuestaContactless.criptograma
        req.fechaTransaccion = Now
        req.importeCompra = importe
        req.importeCashback = cashback
        req.idEncripcion = conector.RespuestaContactless.idEncripcion
        req.tipocuenta = conector.RespuestaContactless.tipoCuenta
        req.pinBlock = conector.RespuestaContactless.pin
        req.nombreTHabiente = conector.RespuestaContactless.nombreThab
        req.reversar = False

        req.tipoIngreso = TipoIngreso.Contactless
        req.serviceCode = conector.RespuestaContactless.codigoServicio
        If conector.RespuestaContactless.datosEmisor = "" Then
            req.datosemisor = "00"
        Else
            req.datosemisor = conector.RespuestaContactless.datosEmisor
        End If
        req.nombreApplicacion = conector.RespuestaContactless.nombreApp
        req.appID = conector.RespuestaContactless.idApp

        req.NroDocumentoTicket = eldoc

        'If tipoTrx <> TransmisorTCP.TipoMensaje.Reverso Then
        rev = req.Clone
        rev.reversar = True

        revtest = req.Clone
        revtest.reversar = True

        'rev.tipoMensaje = TransmisorTCP.TipoMensaje.Reverso
        'End If

        Return Transmitir(req)


    End Function





    Private Function Consultar_Datos_Adicionales_Anulacion(tarjeta As String, tickOri As Integer, Optional servCode As String = "", Optional appnombre As String = "") As TransmisorTCP.MensajeRespuesta

        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.ticketOriginal = tickOri
        req.tipoMensaje = TransmisorTCP.TipoMensaje.ConsultarMovimientoAnulacion
        req.nroTarjeta = tarjeta
        req.serviceCode = servCode
        req.nombreApplicacion = appnombre

        Return Transmitir(req)
    End Function


    Private Function Consultar_Datos_Adicionales(tarjeta As String, modoingreso As String, Optional servCode As String = "", Optional appnombre As String = "") As TransmisorTCP.MensajeRespuesta
        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.tipoMensaje = TransmisorTCP.TipoMensaje.PedirInfoAdicional
        req.nroTarjeta = tarjeta
        req.serviceCode = servCode

        Select Case modoingreso
            Case "M"
                req.tipoIngreso = TipoIngreso.Manual
            Case "C"
                req.tipoIngreso = TipoIngreso.Chip
            Case "B"
                req.tipoIngreso = TipoIngreso.Banda
            Case "L"
                req.tipoIngreso = TipoIngreso.Contactless
        End Select

        req.nombreApplicacion = appnombre


        'req.host = TransmisorTCP.TipoHost.Posnet_homolog

        Return Transmitir(req)
    End Function

    Private Function Enviar_respuesta_Chip(terminal As String, host As String, ticket As Integer, conector As ConectorPP) As TransmisorTCP.MensajeRespuesta
        Dim req As New TransmisorTCP.MensajeIda
        req.terminal = terminal
        req.tipoMensaje = TransmisorTCP.TipoMensaje.RespuestaChip
        req.RespChip = conector.RespuestaChip.codrespuesta
        req.ticketOriginal = ticket
        req.host = DirectCast([Enum].Parse(GetType(TipoHost), host), TipoHost)
        Return Transmitir(req)
    End Function

    Private Function Consultar_Claves() As TransmisorTCP.MensajeRespuesta
        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.tipoMensaje = TransmisorTCP.TipoMensaje.PedirClaves
        Return Transmitir(req)              '--- manda al servidor de tarjetas
    End Function

#End Region

#Region "Anulación"

    Public Function AnulacionMM(ticketoriginal As Integer) As ResponseToPOS


        Dim Respuesta As New ResponseToPOS
        Dim Reverso As New TransmisorTCP.MensajeIda
        Respuesta.Response = ResponseDescription.NO_APROBADA
        If inicializado And sincronizado Then
            Using conector As New lib_PP.ConectorPP()
                '********** INICIO DE COMPRA - DESPIERTA EL PINPAD *****************
                Dim MensClaves = Consultar_Claves()
                If MensClaves IsNot Nothing Then
                    If conector.Inicio_AnulacionMM(MensClaves.Clave_wkd, MensClaves.Clave_wkp, MensClaves.PosicionMK) = 0 Then

                        If conector.RespuestaContactless.modoIngreso = "L" Then
                            Msg("-------- Respuesta a Y41 ----------")
                            Msg("Modo Ing                      : " & conector.RespuestaContactless.modoIngreso)
                            'Msg("Nro Tarjeta                   : " & conector.RespuestaContactless.tarjeta)
                            Dim rango As String = conector.RespuestaContactless.tarjeta
                            Dim tipoTarjeta As Integer = rango.Substring(0, 1)
                            Dim CodigoServicioPIN As Integer = conector.RespuestaContactless.codigoServicio
                            Msg("Cod. Serv.                    : " & conector.RespuestaContactless.codigoServicio)
                            Msg("Cod. Banco                    : " & conector.RespuestaContactless.codigoBanco)
                            Msg("Nro de registro               : " & conector.RespuestaContactless.nroRegistro)
                            Msg("Id de encripcion              : " & conector.RespuestaContactless.idEncripcion)
                            Msg("Tipo de encripcion            : " & conector.RespuestaContactless.tipoEncripcion)
                            Msg("Paquete encriptado            : " & conector.RespuestaContactless.paquete)
                            Msg("Autorizacion                  : " & conector.RespuestaContactless.autorizacion)
                            Msg("Respuesta del emisor          : " & conector.RespuestaContactless.respEmisor)
                            Msg("Secuencia PAN                 : " & conector.RespuestaContactless.secuencia)
                            Msg("Tipo de cuenta                : " & conector.RespuestaContactless.tipoCuenta)
                            Msg("PIN encriptado                : " & conector.RespuestaContactless.pin)
                            Msg("Nombre Aplicacion             : " & conector.RespuestaContactless.nombreApp)
                            Msg("ID Aplicacion                 : " & conector.RespuestaContactless.idApp)
                            Msg("Nombre Tarjeta habiente       : " & conector.RespuestaContactless.nombreThab)
                            Msg("Consulta por copia            : " & conector.RespuestaContactless.consultaCopia)
                            Msg("Solicita firma                : " & conector.RespuestaContactless.solicitaFirma)
                            Msg("Datos del emisor              : " & conector.RespuestaContactless.datosEmisor)

                            '********** CONSULTA A SP QUE DATOS PEDIR EN EL COMANDO Y02 *****************
                            Dim mensajeRespuesta = Consultar_Datos_Adicionales_Anulacion(rango, ticketoriginal)


                            If mensajeRespuesta IsNot Nothing Then
                                If mensajeRespuesta.codRespuesta = "00" Then 'ENCONTRO EL MOVIMIENTO
                                    '********** ENVIA TRANSACCION A SP *****************
                                    If CInt(mensajeRespuesta.tipooperacion) < 200000 Then
                                        mensajeRespuesta = Enviar_Transaccion_CL(conector, mensajeRespuesta.nroPlan, mensajeRespuesta.Importe, mensajeRespuesta.CashBack, mensajeRespuesta.Ticket, "", TransmisorTCP.TipoMensaje.Anulacion, mensajeRespuesta.referenceNumber, ticketoriginal, Reverso, "", "")

                                    ElseIf CInt(mensajeRespuesta.tipooperacion) >= 200000 Then
                                        mensajeRespuesta = Enviar_Transaccion_CL(conector, mensajeRespuesta.nroPlan, mensajeRespuesta.Importe, mensajeRespuesta.CashBack, mensajeRespuesta.Ticket, "", TransmisorTCP.TipoMensaje.AnulacionDevolucion, mensajeRespuesta.referenceNumber, ticketoriginal, Reverso, "", "")

                                    Else
                                        mensajeRespuesta.codRespuesta = "99"
                                        mensajeRespuesta.Respuesta = "Operacion no válida para realizar anulacion"

                                    End If
                                    If mensajeRespuesta IsNot Nothing Then
                                        Msg("-------------------------------")
                                        Msg("     RESPUESTA DEL SERVIDOR")
                                        Msg("-------------------------------")
                                        Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                        Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                        Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                        Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                        Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)


                                        If mensajeRespuesta.codRespuesta = "00" Then
                                            Respuesta.Response = ResponseDescription.APROBADA
                                            Respuesta.TotalAmount = mensajeRespuesta.Importe
                                            Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion
                                        Else
                                            Respuesta.Response = ResponseDescription.NO_APROBADA
                                        End If
                                        Respuesta.Emisor = mensajeRespuesta.Emisor
                                        Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                        Respuesta.TicketString = mensajeRespuesta.cupon
                                    Else
                                        '---NO HUBO RESPUESTA
                                        Dim respreverso = Reversar(Reverso)
                                        If respreverso IsNot Nothing Then
                                            Respuesta.DisplayString = respreverso.Respuesta
                                        Else
                                            Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."
                                        End If

                                        Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA


                                    End If
                                Else
                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                    Respuesta.Emisor = mensajeRespuesta.Emisor
                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                    Respuesta.TicketString = mensajeRespuesta.cupon
                                End If
                            End If

                        Else


                            Msg("-------- Respuesta a Y41 ----------")
                            Msg("Modo Ing                      : " & conector.RespuestaInicioCompra.modoIngreso)
                            'Msg("Nro Tarjeta                   : " & conector.RespuestaInicioCompra.tarjeta)
                            Dim rango As String = conector.RespuestaInicioCompra.tarjeta
                            Dim tipoTarjeta As Integer = rango.Substring(0, 1)
                            Dim CodigoServicioPIN As Integer = conector.RespuestaInicioCompra.codigoServicio
                            Msg("Cod. Serv.                    : " & conector.RespuestaInicioCompra.codigoServicio)
                            Msg("Cod. Banco                    : " & conector.RespuestaInicioCompra.codigoBanco)
                            Msg("Nro de registro               : " & conector.RespuestaInicioCompra.nroRegistro)
                            Msg("Abreviatura Tarjeta           : " & conector.RespuestaInicioCompra.abrebiaturaTarj)
                            Msg("Nombre Aplicacion CHIP        : " & conector.RespuestaInicioCompra.appChip)


                            '********** CONSULTA A SP QUE DATOS PEDIR EN EL COMANDO Y02 *****************
                            Dim mensajeRespuesta = Consultar_Datos_Adicionales_Anulacion(rango, ticketoriginal, conector.RespuestaInicioCompra.codigoServicio, conector.RespuestaInicioCompra.appChip)

                            'Dim posMK = mensajeRespuesta.PosicionMK
                            If mensajeRespuesta IsNot Nothing Then
                                If mensajeRespuesta.codRespuesta = "00" Then  'ENCONTRO EL MOVIMIENTO


                                    'If mensajeRespuesta.Emisor.TrimEnd = "MAESTRO" Then
                                    '    If pCashback > 0 Then
                                    '        pCuotas = "992"
                                    '    Else
                                    '        pCuotas = "991"
                                    '    End If

                                    'End If
                                    '********** SOLICITA DATOS ADICIONALES *****************
                                    If conector.Datos_adicionales(conector.RespuestaInicioCompra.modoIngreso,
                                                                  mensajeRespuesta.u4d,
                                                                  mensajeRespuesta.cds,
                                                                  mensajeRespuesta.spi,
                                                                  mensajeRespuesta.Clave_wkd,
                                                                  mensajeRespuesta.Clave_wkp,
                                                                  mensajeRespuesta.PosicionMK,
                                                                  mensajeRespuesta.Importe,
                                                                  mensajeRespuesta.CashBack,
                                                                  tipoTarjeta,
                                                                  CodigoServicioPIN) = 0 Then

                                        '******** Muestra mensajes segun modo ingreso *********
                                        Select Case conector.RespuestaInicioCompra.modoIngreso
                                            Case "M"
                                                Datos_Adicionales_Manual(conector)
                                            Case "B"
                                                Datos_Adicionales_Banda(conector)
                                            Case "C"
                                                Datos_Adicionales_Chip(conector)
                                            Case Else
                                                Msg("Modo de ingreso no contemplado.")
                                        End Select

                                        '********** ENVIA TRANSACCION A SP *****************
                                        If CInt(mensajeRespuesta.tipooperacion) < 200000 Then
                                            mensajeRespuesta = Enviar_Transaccion(conector, mensajeRespuesta.nroPlan, mensajeRespuesta.Importe, mensajeRespuesta.CashBack, cajera, mensajeRespuesta.Clave_wkp, mensajeRespuesta.referenceNumber, ticketoriginal, TransmisorTCP.TipoMensaje.Anulacion, mensajeRespuesta.Ticket, Reverso, "")

                                        ElseIf CInt(mensajeRespuesta.tipooperacion) >= 200000 Then
                                            mensajeRespuesta = Enviar_Transaccion(conector, mensajeRespuesta.nroPlan, mensajeRespuesta.Importe, mensajeRespuesta.CashBack, cajera, mensajeRespuesta.Clave_wkp, mensajeRespuesta.referenceNumber, ticketoriginal, TransmisorTCP.TipoMensaje.AnulacionDevolucion, mensajeRespuesta.Ticket, Reverso, "")
                                        Else
                                            mensajeRespuesta.codRespuesta = "99"
                                            mensajeRespuesta.Respuesta = "Operacion no válida para realizar anulacion"
                                        End If




                                        If mensajeRespuesta IsNot Nothing Then
                                            Msg("-------------------------------")
                                            Msg("     RESPUESTA DEL SERVIDOR")
                                            Msg("-------------------------------")
                                            Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                            Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                            Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                            Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                            Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)

                                            Select Case conector.RespuestaInicioCompra.modoIngreso
                                                Case "M", "B"
                                                    If mensajeRespuesta.codRespuesta = "00" Then
                                                        Respuesta.Response = ResponseDescription.APROBADA
                                                        Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                        Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion
                                                    Else

                                                        If mensajeRespuesta.codRespuesta = "55" Then
                                                            'TODO: REINGRESO DE PIN
                                                            MensClaves = Consultar_Claves()

                                                            Respuesta.TicketString = mensajeRespuesta.cupon & vbNewLine & "@" & vbNewLine
                                                            If conector.Reingreso_pin(MensClaves.PosicionMK, MensClaves.Clave_wkp, conector.RespuestaInicioCompra.tarjeta) = 0 Then
                                                                Msg("-------- Respuesta a Y04 ----------")
                                                                Msg("PIN Block                     : " & conector.RespuestaDatosAdicionales.PinBlock)

                                                                If CInt(mensajeRespuesta.tipooperacion) < 200000 Then
                                                                    mensajeRespuesta = Enviar_Transaccion(conector, mensajeRespuesta.nroPlan, mensajeRespuesta.Importe, mensajeRespuesta.CashBack, cajera, mensajeRespuesta.Clave_wkp, mensajeRespuesta.referenceNumber, ticketoriginal, TransmisorTCP.TipoMensaje.Anulacion, mensajeRespuesta.Ticket, Reverso, "")

                                                                ElseIf CInt(mensajeRespuesta.tipooperacion) >= 200000 Then
                                                                    mensajeRespuesta = Enviar_Transaccion(conector, mensajeRespuesta.nroPlan, mensajeRespuesta.Importe, mensajeRespuesta.CashBack, cajera, mensajeRespuesta.Clave_wkp, mensajeRespuesta.referenceNumber, ticketoriginal, TransmisorTCP.TipoMensaje.AnulacionDevolucion, mensajeRespuesta.Ticket, Reverso, "")

                                                                Else
                                                                    mensajeRespuesta.codRespuesta = "99"
                                                                    mensajeRespuesta.Respuesta = "Operacion no válida para realizar anulacion"

                                                                End If

                                                                'mensajeRespuesta = Enviar_Transaccion(conector, mensajeRespuesta.nroPlan, mensajeRespuesta.Importe, mensajeRespuesta.CashBack, cajera, mensajeRespuesta.Clave_wkp, mensajeRespuesta.referenceNumber, ticketoriginal, Reverso)

                                                                Msg("-----------------------------------------")
                                                                Msg("     RESPUESTA DEL SERVIDOR REINGRESO PIN")
                                                                Msg("-----------------------------------------")
                                                                Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                                                Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                                                Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                                                Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                                                Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)

                                                                If mensajeRespuesta.codRespuesta = "00" Then
                                                                    Respuesta.Response = ResponseDescription.APROBADA
                                                                    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                                    Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion
                                                                Else
                                                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                                                End If


                                                            Else
                                                                Respuesta.Response = ResponseDescription.NO_APROBADA
                                                            End If
                                                        Else
                                                            Respuesta.Response = ResponseDescription.NO_APROBADA
                                                        End If
                                                    End If
                                                    Respuesta.Emisor = mensajeRespuesta.Emisor
                                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                    Respuesta.TicketString = Respuesta.TicketString & mensajeRespuesta.cupon

                                         'Return mensajeRespuesta.codRespuesta
                                         'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                                                Case "C"
                                                    If conector.Responder_Chip(mensajeRespuesta.Autorizacion, mensajeRespuesta.codRespuesta, mensajeRespuesta.criptograma, True) = 0 Then
                                                        Msg("-------- Respuesta a Y03 ----------")
                                                        Msg("Autorizacion                      : " & conector.RespuestaChip.autorizacion)
                                                        Msg("Codigo de respuesta               : " & conector.RespuestaChip.codrespuesta)
                                                        Msg("Respuesta                         : " & conector.RespuestaChip.mensajeRepuesta)
                                                        Msg("Criptograma                       : " & conector.RespuestaChip.criptograma)


                                                        If mensajeRespuesta.codRespuesta = ResponseDescription.APROBADA Then 'APROBADA POR SP
                                                            Console.WriteLine(conector.RespuestaChip.autorizacion)
                                                            If conector.RespuestaChip.autorizacion.Trim <> "Z3" Then
                                                                Respuesta.Response = ResponseDescription.APROBADA
                                                                Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                                Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                                Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion
                                                                'Respuesta.TicketString = mensajeRespuesta.cupon
                                                                Respuesta.Emisor = mensajeRespuesta.Emisor
                                                            Else
                                                                Respuesta.Response = ResponseDescription.NO_APROBADA
                                                                '                                                    If mensajeRespuesta.codRespuesta = "00" Then 'si respuesta host aprobada y denegada pp
                                                                Respuesta.DisplayString = String.Format("Denegada por PP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)
                                                                If Reversar(Reverso) IsNot Nothing Then
                                                                    Respuesta.DisplayString = Respuesta.DisplayString & " - Mov Reversado correctamente."
                                                                Else
                                                                    Respuesta.DisplayString = Respuesta.DisplayString & " - No se pudo reversar correctamente. Verifique."
                                                                End If
                                                            End If

                                                        Else 'NO APROBADA POR SP
                                                            If mensajeRespuesta.codRespuesta = "12" Then
                                                                Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA

                                                            Else
                                                                Respuesta.Response = ResponseDescription.NO_APROBADA
                                                                'Respuesta.DisplayString = String.Format("Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)
                                                            End If
                                                            Respuesta.DisplayString = mensajeRespuesta.mensajeHost
                                                        End If

                                                        Respuesta.TicketString = mensajeRespuesta.cupon

                                                        If mensajeRespuesta.codRespuesta <> "12" Then ' 12 es respuesta invalida
                                                            'ENVIA LA RESPUESTA DEL CHIP AL SP.
                                                            mensajeRespuesta = Enviar_respuesta_Chip(mensajeRespuesta.terminal, mensajeRespuesta.host, mensajeRespuesta.Ticket, conector)

                                                            If mensajeRespuesta Is Nothing Then
                                                                Respuesta.DisplayString = "El SP no respondió al envió de la respuesta del CHIP. No se grabó la respuesta."

                                                            End If

                                                        End If
                                                    Else
                                                        Reversar(Reverso)
                                                        Respuesta.Response = ResponseDescription.ERROR_EN_PP
                                                        Respuesta.DisplayString = String.Format("El SP respondió pero el PP no respondió la confirmación del CHIP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)
                                                        'TODO: Ver si reversar cuando el pp no responde por algun error.
                                                    End If




                                                Case Else
                                                    Msg("Modo de ingreso no contemplado.")
                                            End Select
                                        Else
                                            '---NO HUBO RESPUESTA
                                            'Respuesta.TicketString = Ticket_SinRepuesta(Reverso)
                                            ' Reversar(Reverso)
                                            'TODO: Aca habria que reversar. o ver que hacer.
                                            Select Case conector.RespuestaInicioCompra.modoIngreso

                                                Case "M", "B"

                                        'Return Nothing
                                                Case "C"
                                                    If conector.Responder_Chip("", "", "", False) = 0 Then

                                                    End If

                                                Case Else
                                                    Msg("Modo de ingreso no contemplado.")
                                            End Select
                                            Dim respreverso = Reversar(Reverso)
                                            If respreverso IsNot Nothing Then
                                                Respuesta.DisplayString = respreverso.Respuesta
                                            Else
                                                Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."
                                            End If

                                            Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA


                                        End If

                                    Else
                                        'ERROR EN SOLICITUD DE DATOS ADICIONALES
                                        'TODO: Error no continua con la transaccion
                                        mostrar_error(Respuesta, conector)

                                    End If
                                Else
                                    'TODO: No hubo respuesta del servidor para la consulta de datos adicionales.
                                    Respuesta.Response = ResponseDescription.COMANDO_CANCELADO
                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                    conector.Cancelar()
                                End If
                            Else
                                'TODO: No hubo respuesta del servidor para la consulta de datos adicionales.
                                Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                                Respuesta.DisplayString = "El SP no respondió a la consulta de datos adicionales. Reintente."
                                conector.Cancelar()

                            End If

                        End If

                    Else
                        'ERROR EN EL DESPERTAR AL PINPAD
                        mostrar_error(Respuesta, conector)
                        conector.Cancelar()
                    End If
                Else
                    Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                    Respuesta.DisplayString = "El servidor de tarjetas no responde"
                End If
            End Using
        Else
            Respuesta.Response = ResponseDescription.ERROR_EN_PP
            Respuesta.DisplayString = "No se puede utilizar el PP, falta sincronizar o inicializar."

        End If
        Return Respuesta

    End Function

    Public Function Anulacion(ticketoriginal As Integer, cajera As Integer) As ResponseToPOS
        Dim Respuesta As New ResponseToPOS
        Dim Reverso As New TransmisorTCP.MensajeIda
        If inicializado And sincronizado Then
            Using conector As New lib_PP.ConectorPP()

                '********** INICIO DE ANULACION - DESPIERTA EL PINPAD *****************
                If conector.Inicio_Anulacion() = 0 Then

                    Msg("-------- Respuesta a Y01 ----------")
                    Msg("Modo Ing                      : " & conector.RespuestaInicioCompra.modoIngreso)
                    'Msg("Nro Tarjeta                   : " & conector.RespuestaInicioCompra.tarjeta)
                    Dim rango As String = conector.RespuestaInicioCompra.tarjeta
                    Dim tipoTarjeta As String = rango.Substring(0, 1)
                    Dim CodigoServicioPIN As Integer = conector.RespuestaInicioCompra.codigoServicio
                    Msg("Cod. Serv.                    : " & conector.RespuestaInicioCompra.codigoServicio)
                    Msg("Cod. Banco                    : " & conector.RespuestaInicioCompra.codigoBanco)
                    Msg("Nro de registro               : " & conector.RespuestaInicioCompra.nroRegistro)
                    Msg("Abreviatura Tarjeta           : " & conector.RespuestaInicioCompra.abrebiaturaTarj)
                    Msg("Nombre Aplicacion CHIP        : " & conector.RespuestaInicioCompra.appChip)

                    '********** CONSULTA A SP QUE DATOS PEDIR EN EL COMANDO Y02 *****************
                    Dim mensajeRespuesta = Consultar_Datos_Adicionales_Anulacion(rango, ticketoriginal)

                    If mensajeRespuesta IsNot Nothing Then
                        If mensajeRespuesta.codRespuesta = "00" Then 'ENCONTRO EL MOVIMIENTO

                            '********** SOLICITA DATOS ADICIONALES *****************
                            If conector.Datos_adicionales(conector.RespuestaInicioCompra.modoIngreso,
                                                      mensajeRespuesta.u4d,
                                                      mensajeRespuesta.cds,
                                                      mensajeRespuesta.spi,
                                                      mensajeRespuesta.Clave_wkd,
                                                      mensajeRespuesta.Clave_wkp,
                                                      mensajeRespuesta.PosicionMK,
                                                      mensajeRespuesta.Importe,
                                                      mensajeRespuesta.CashBack,
                                                      tipoTarjeta,
                                                      CodigoServicioPIN) = 0 Then


                                Select Case conector.RespuestaInicioCompra.modoIngreso
                                    Case "M"
                                        Datos_Adicionales_Manual(conector)
                                    Case "B"
                                        Datos_Adicionales_Banda(conector)
                                    Case "C"
                                        Datos_Adicionales_Chip(conector)
                                    Case Else
                                        Msg("Modo de ingreso no contemplado.")
                                End Select

                                '********** ENVIA TRANSACCION A SP *****************
                                If mensajeRespuesta.tipooperacion = "000000" Or mensajeRespuesta.tipooperacion = "001000" Then
                                    mensajeRespuesta = Enviar_Transaccion(conector,
                                                                      mensajeRespuesta.nroPlan,
                                                                      mensajeRespuesta.Importe,
                                                                      mensajeRespuesta.CashBack,
                                                                      cajera,
                                                                      mensajeRespuesta.Clave_wkp,
                                                                      mensajeRespuesta.referenceNumber,
                                                                      ticketoriginal,
                                                                      TransmisorTCP.TipoMensaje.Anulacion,
                                                                      mensajeRespuesta.Ticket,
                                                                      Reverso,
                                                                      "")
                                ElseIf mensajeRespuesta.tipooperacion = "200000" Then
                                    mensajeRespuesta = Enviar_Transaccion(conector,
                                                                     mensajeRespuesta.nroPlan,
                                                                     mensajeRespuesta.Importe,
                                                                     mensajeRespuesta.CashBack,
                                                                     cajera,
                                                                     mensajeRespuesta.Clave_wkp,
                                                                     mensajeRespuesta.referenceNumber,
                                                                     ticketoriginal,
                                                                     TransmisorTCP.TipoMensaje.AnulacionDevolucion,
                                                                     mensajeRespuesta.Ticket,
                                                                     Reverso,
                                                                     "")
                                Else
                                    mensajeRespuesta.codRespuesta = "99"
                                    mensajeRespuesta.Respuesta = "Operacion no válida para realizar anulacion"
                                End If


                                If mensajeRespuesta IsNot Nothing Then
                                    Msg("-------------------------------")
                                    Msg("     RESPUESTA DEL SERVIDOR")
                                    Msg("-------------------------------")
                                    Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                    Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                    Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                    Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                    Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)

                                    Select Case conector.RespuestaInicioCompra.modoIngreso
                                        Case "M", "B"
                                            If mensajeRespuesta.codRespuesta = "00" Then
                                                Respuesta.Response = ResponseDescription.APROBADA
                                                Respuesta.TotalAmount = mensajeRespuesta.Importe
                                            Else
                                                Respuesta.Response = ResponseDescription.NO_APROBADA
                                            End If
                                            Respuesta.Emisor = mensajeRespuesta.Emisor
                                            Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                            Respuesta.TicketString = mensajeRespuesta.cupon

                                        'Return mensajeRespuesta.codRespuesta
                                        'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                                        Case "C"
                                            If conector.Responder_Chip(mensajeRespuesta.Autorizacion, mensajeRespuesta.codRespuesta, mensajeRespuesta.criptograma, True) = 0 Then
                                                Msg("-------- Respuesta a Y03 ----------")
                                                Msg("Autorizacion                      : " & conector.RespuestaChip.autorizacion)
                                                Msg("Codigo de respuesta               : " & conector.RespuestaChip.codrespuesta)
                                                Msg("Respuesta                         : " & conector.RespuestaChip.mensajeRepuesta)
                                                Msg("Criptograma                       : " & conector.RespuestaChip.criptograma)


                                                If conector.RespuestaChip.codrespuesta = "00" Then
                                                    Respuesta.Response = ResponseDescription.APROBADA
                                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                    'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                                                Else
                                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                                    Respuesta.DisplayString = String.Format("Denegada por PP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)

                                                End If
                                                Respuesta.Emisor = mensajeRespuesta.Emisor
                                                Respuesta.TicketString = mensajeRespuesta.cupon
                                            Else
                                                Respuesta.Response = ResponseDescription.ERROR_EN_PP
                                                Respuesta.DisplayString = String.Format("El SP respondió pero el PP no respondió la confirmación del CHIP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)

                                            End If
                                            'mensajeRespuesta = Enviar_respuesta_Chip(pinguino, caja, conector, mensajeRespuesta.clavereverso)
                                            If mensajeRespuesta Is Nothing Then
                                                Respuesta.DisplayString = "El SP no respondió al envió de la respuesta del CHIP, puede que no se haya reversado."
                                            Else
                                                Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                            End If
                                        Case Else
                                            Msg("Modo de ingreso no contemplado.")
                                    End Select
                                Else
                                    'TODO: Aca habria que reversar. o ver que hacer.
                                    Select Case conector.RespuestaInicioCompra.modoIngreso

                                        Case "M", "B"
                                        'TODO: Aca habria que reversar. o ver que hacer.
                                        'Return Nothing
                                        Case "C"
                                            If conector.Responder_Chip("", "", "", False) = 0 Then

                                            End If

                                        Case Else
                                            Msg("Modo de ingreso no contemplado.")
                                    End Select
                                    Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                                    Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."

                                End If

                            Else
                                'TODO: Error no continua con la transaccion
                                mostrar_error(Respuesta, conector)


                            End If
                        Else
                            'TODO: Error no continua con la transaccion
                            Respuesta.Response = ResponseDescription.NO_APROBADA
                            Respuesta.DisplayString = mensajeRespuesta.Respuesta
                            conector.Cancelar()
                        End If
                    Else
                        'TODO: No hubo respuesta del servidor.
                        Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                        Respuesta.DisplayString = "El SP no respondió a la consulta de datos adicionales."

                    End If
                Else
                    mostrar_error(Respuesta, conector)

                End If

            End Using
        Else
            Respuesta.Response = ResponseDescription.ERROR_EN_PP
            Respuesta.DisplayString = "No se puede utilizar el PP, falta sincronizar o inicializar."

        End If
        Return Respuesta
    End Function
#End Region

#Region "Devolución"
    ''' <summary>
    ''' Devolucion MultiModo
    ''' </summary>
    ''' <param name="plan">Plan Original</param>
    ''' <param name="importe">Importe a devolver.</param>
    ''' <param name="cashback">Monto cashback a devolver</param>
    ''' <param name="ticketcaja">Nro de comprobante de la caja, de la operacion original</param>
    ''' <param name="ticketoriginal">Numero de cupón de la transacción original</param>
    ''' <param name="fechaOriginal">Fecha de la transaccón original (Formato ddmmyy)</param>
    ''' <returns></returns>
    Public Function DevolucionMM(plan As Integer, importe As Decimal, cashback As Decimal, ticketcaja As Integer, ticketoriginal As Integer, fechaOriginal As String) As ResponseToPOS
        Dim Respuesta As New ResponseToPOS
        Dim reverso As New TransmisorTCP.MensajeIda
        Respuesta.Response = ResponseDescription.NO_APROBADA
        If inicializado And sincronizado Then
            Using conector As New lib_PP.ConectorPP()
                Dim MensClaves = Consultar_Claves()
                If MensClaves IsNot Nothing Then
                    '********** INICIO DE DEVOLUCION - DESPIERTA EL PINPAD *****************
                    If conector.Inicio_DevolucionMM(MensClaves.Clave_wkd, MensClaves.Clave_wkp, MensClaves.PosicionMK) = 0 Then

                        If conector.RespuestaContactless.modoIngreso = "L" Then
                            Msg("-------- Respuesta a Y41 ----------")
                            Msg("Modo Ing                      : " & conector.RespuestaContactless.modoIngreso)
                            'Msg("Nro Tarjeta                   : " & conector.RespuestaContactless.tarjeta)
                            Dim rango As String = conector.RespuestaContactless.tarjeta
                            Dim tipoTarjeta As Integer = rango.Substring(0, 1)
                            Dim CodigoServicioPIN As Integer = conector.RespuestaContactless.codigoServicio
                            Msg("Cod. Serv.                    : " & conector.RespuestaContactless.codigoServicio)
                            Msg("Cod. Banco                    : " & conector.RespuestaContactless.codigoBanco)
                            Msg("Nro de registro               : " & conector.RespuestaContactless.nroRegistro)
                            Msg("Id de encripcion              : " & conector.RespuestaContactless.idEncripcion)
                            Msg("Tipo de encripcion            : " & conector.RespuestaContactless.tipoEncripcion)
                            Msg("Paquete encriptado            : " & conector.RespuestaContactless.paquete)
                            Msg("Autorizacion                  : " & conector.RespuestaContactless.autorizacion)
                            Msg("Respuesta del emisor          : " & conector.RespuestaContactless.respEmisor)
                            Msg("Secuencia PAN                 : " & conector.RespuestaContactless.secuencia)
                            Msg("Tipo de cuenta                : " & conector.RespuestaContactless.tipoCuenta)
                            Msg("PIN encriptado                : " & conector.RespuestaContactless.pin)
                            Msg("Nombre Aplicacion             : " & conector.RespuestaContactless.nombreApp)
                            Msg("ID Aplicacion                 : " & conector.RespuestaContactless.idApp)
                            Msg("Nombre Tarjeta habiente       : " & conector.RespuestaContactless.nombreThab)
                            Msg("Consulta por copia            : " & conector.RespuestaContactless.consultaCopia)
                            Msg("Solicita firma                : " & conector.RespuestaContactless.solicitaFirma)
                            Msg("Datos del emisor              : " & conector.RespuestaContactless.datosEmisor)

                            '********** CONSULTA A SP QUE DATOS PEDIR EN EL COMANDO Y02 *****************
                            Dim mensajeRespuesta = Consultar_Datos_Adicionales(rango, conector.RespuestaContactless.modoIngreso, conector.RespuestaContactless.codigoServicio, conector.RespuestaContactless.nombreApp)


                            If mensajeRespuesta IsNot Nothing Then
                                If mensajeRespuesta.codRespuesta = "00" Then 'ENCONTRO EL MOVIMIENTO
                                    '********** ENVIA TRANSACCION A SP *****************
                                    mensajeRespuesta = Enviar_Transaccion_CL(conector, plan, importe, cashback, "", ticketoriginal, fechaOriginal, ticketcaja, reverso, "", "")

                                    If mensajeRespuesta IsNot Nothing Then
                                        Msg("-------------------------------")
                                        Msg("     RESPUESTA DEL SERVIDOR")
                                        Msg("-------------------------------")
                                        Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                        Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                        Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                        Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                        Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)


                                        If mensajeRespuesta.codRespuesta = "00" Then
                                            Respuesta.Response = ResponseDescription.APROBADA
                                            Respuesta.TotalAmount = mensajeRespuesta.Importe
                                            Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion
                                        Else
                                            Respuesta.Response = ResponseDescription.NO_APROBADA
                                        End If
                                        Respuesta.Emisor = mensajeRespuesta.Emisor
                                        Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                        Respuesta.TicketString = mensajeRespuesta.cupon
                                    End If
                                End If
                                'Return mensajeRespuesta.codRespuesta
                                'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                            Else
                                '---NO HUBO RESPUESTA
                                'Respuesta.TicketString = Ticket_SinRepuesta(Reverso)


                                Dim respreverso = Reversar(reverso)
                                If respreverso IsNot Nothing Then
                                    Respuesta.DisplayString = respreverso.Respuesta
                                Else
                                    Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."
                                End If
                                Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA


                            End If


                        Else


                            Msg("-------- Respuesta a Y41 ----------")
                            Msg("Modo Ing                      : " & conector.RespuestaInicioCompra.modoIngreso)
                            'Msg("Nro Tarjeta                   : " & conector.RespuestaInicioCompra.tarjeta)
                            Dim rango As String = conector.RespuestaInicioCompra.tarjeta
                            Dim tipoTarjeta As String = rango.Substring(0, 1)
                            Dim CodigoServicioPIN As Integer = conector.RespuestaInicioCompra.codigoServicio
                            Msg("Cod. Serv.                    : " & conector.RespuestaInicioCompra.codigoServicio)
                            Msg("Cod. Banco                    : " & conector.RespuestaInicioCompra.codigoBanco)
                            Msg("Nro de registro               : " & conector.RespuestaInicioCompra.nroRegistro)
                            Msg("Abreviatura Tarjeta           : " & conector.RespuestaInicioCompra.abrebiaturaTarj)
                            Msg("Nombre Aplicacion CHIP        : " & conector.RespuestaInicioCompra.appChip)

                            '********** CONSULTA A SP QUE DATOS PEDIR EN EL COMANDO Y02 *****************
                            Dim mensajeRespuesta = Consultar_Datos_Adicionales(rango, conector.RespuestaInicioCompra.modoIngreso, conector.RespuestaInicioCompra.codigoServicio, conector.RespuestaInicioCompra.appChip)

                            If mensajeRespuesta IsNot Nothing Then
                                If mensajeRespuesta.codRespuesta = "00" Then 'ENCONTRO EL MOVIMIENTO
                                    If mensajeRespuesta.Emisor.TrimEnd = "VISA DEBITO" Then
                                        If cashback > 0 Then
                                            plan = "800"
                                        Else
                                            plan = "101"
                                        End If

                                    End If

                                    If mensajeRespuesta.Emisor.TrimEnd = "MASTERCARD DEBIT" Then
                                        If cashback > 0 Then
                                            plan = "992"
                                        Else
                                            plan = "101"
                                        End If

                                    End If

                                    If mensajeRespuesta.Emisor.TrimEnd = "MAESTRO" Then
                                        If cashback > 0 Then
                                            plan = "992"
                                        Else
                                            plan = "991"
                                        End If

                                    End If
                                    '********** SOLICITA DATOS ADICIONALES *****************
                                    If conector.Datos_adicionales(conector.RespuestaInicioCompra.modoIngreso,
                                                      mensajeRespuesta.u4d,
                                                      mensajeRespuesta.cds,
                                                      mensajeRespuesta.spi,
                                                      mensajeRespuesta.Clave_wkd,
                                                      mensajeRespuesta.Clave_wkp,
                                                      mensajeRespuesta.PosicionMK,
                                                      importe,
                                                      cashback,
                                                      tipoTarjeta,
                                                      CodigoServicioPIN) = 0 Then


                                        Select Case conector.RespuestaInicioCompra.modoIngreso
                                            Case "M"
                                                Datos_Adicionales_Manual(conector)
                                            Case "B"
                                                Datos_Adicionales_Banda(conector)
                                            Case "C"
                                                Datos_Adicionales_Chip(conector)
                                            Case Else
                                                Msg("Modo de ingreso no contemplado.")
                                        End Select

                                        '********** ENVIA TRANSACCION A SP *****************
                                        mensajeRespuesta = Enviar_TransaccionDev(conector, plan, importe, cashback, cajera, mensajeRespuesta.Clave_wkp, fechaOriginal, ticketoriginal, ticketcaja, reverso)
                                        If mensajeRespuesta IsNot Nothing Then
                                            Msg("-------------------------------")
                                            Msg("     RESPUESTA DEL SERVIDOR")
                                            Msg("-------------------------------")
                                            Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                            Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                            Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                            Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                            Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)

                                            Select Case conector.RespuestaInicioCompra.modoIngreso
                                                Case "M", "B"
                                                    If mensajeRespuesta.codRespuesta = "00" Then
                                                        Respuesta.Response = ResponseDescription.APROBADA
                                                        Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                        Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion
                                                    Else
                                                        If mensajeRespuesta.codRespuesta = "55" Then
                                                            'TODO: REINGRESO DE PIN
                                                            MensClaves = Consultar_Claves()

                                                            Respuesta.TicketString = mensajeRespuesta.cupon & vbNewLine & "@" & vbNewLine
                                                            If conector.Reingreso_pin(MensClaves.PosicionMK, MensClaves.Clave_wkp, conector.RespuestaInicioCompra.tarjeta) = 0 Then
                                                                Msg("-------- Respuesta a Y04 ----------")
                                                                Msg("PIN Block                     : " & conector.RespuestaDatosAdicionales.PinBlock)


                                                                mensajeRespuesta = Enviar_TransaccionDev(conector, plan, importe, cashback, cajera, mensajeRespuesta.Clave_wkp, fechaOriginal, ticketoriginal, ticketcaja, reverso)


                                                                Msg("-----------------------------------------")
                                                                Msg("     RESPUESTA DEL SERVIDOR REINGRESO PIN")
                                                                Msg("-----------------------------------------")
                                                                Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                                                Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                                                Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                                                Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                                                Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)

                                                                If mensajeRespuesta.codRespuesta = "00" Then
                                                                    Respuesta.Response = ResponseDescription.APROBADA
                                                                    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                                    Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion
                                                                Else
                                                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                                                End If
                                                            Else
                                                                Respuesta.Response = ResponseDescription.NO_APROBADA
                                                            End If
                                                        Else
                                                            Respuesta.Response = ResponseDescription.NO_APROBADA
                                                        End If

                                                    End If
                                                    Respuesta.Emisor = mensajeRespuesta.Emisor
                                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                    Respuesta.TicketString = mensajeRespuesta.cupon

                                        'Return mensajeRespuesta.codRespuesta
                                        'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                                                Case "C"
                                                    If conector.Responder_Chip(mensajeRespuesta.Autorizacion, mensajeRespuesta.codRespuesta, mensajeRespuesta.criptograma, True) = 0 Then
                                                        Msg("-------- Respuesta a Y03 ----------")
                                                        Msg("Autorizacion                      : " & conector.RespuestaChip.autorizacion)
                                                        Msg("Codigo de respuesta               : " & conector.RespuestaChip.codrespuesta)
                                                        Msg("Respuesta                         : " & conector.RespuestaChip.mensajeRepuesta)
                                                        Msg("Criptograma                       : " & conector.RespuestaChip.criptograma)


                                                        If conector.RespuestaChip.codrespuesta = "00" Then
                                                            Respuesta.Response = ResponseDescription.APROBADA
                                                            Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                            Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                            Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion
                                                            'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                                                        Else
                                                            Respuesta.Response = ResponseDescription.NO_APROBADA
                                                            Respuesta.DisplayString = String.Format("Denegada por PP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)
                                                            If Reversar(reverso) IsNot Nothing Then
                                                                Respuesta.DisplayString = Respuesta.DisplayString & " - Mov Reversado correctamente."
                                                            Else
                                                                Respuesta.DisplayString = Respuesta.DisplayString & " - No se pudo reversar correctamente. Verifique."
                                                            End If

                                                        End If
                                                        Respuesta.Emisor = mensajeRespuesta.Emisor
                                                        Respuesta.TicketString = mensajeRespuesta.cupon
                                                    Else
                                                        Respuesta.Response = ResponseDescription.ERROR_EN_PP
                                                        Respuesta.DisplayString = String.Format("El SP respondió pero el PP no respondió la confirmación del CHIP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)

                                                    End If
                                                    'mensajeRespuesta = Enviar_respuesta_Chip(pinguino, caja, conector, mensajeRespuesta.clavereverso)
                                                    If mensajeRespuesta Is Nothing Then
                                                        Respuesta.DisplayString = "El SP no respondió al envió de la respuesta del CHIP, puede que no se haya reversado."
                                                    Else
                                                        Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                    End If
                                                Case Else
                                                    Msg("Modo de ingreso no contemplado.")
                                            End Select
                                        Else
                                            Select Case conector.RespuestaInicioCompra.modoIngreso

                                                Case "M", "B"
                                        'TODO: Aca habria que reversar. o ver que hacer.
                                        'Return Nothing
                                                Case "C"
                                                    If conector.Responder_Chip("", "", "", False) = 0 Then

                                                    End If

                                                Case Else
                                                    Msg("Modo de ingreso no contemplado.")
                                            End Select

                                            Dim respreverso = Reversar(reverso)
                                            If respreverso IsNot Nothing Then
                                                Respuesta.DisplayString = respreverso.Respuesta
                                            Else
                                                Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."
                                            End If

                                            Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA


                                        End If

                                    Else
                                        'TODO: Error no continua con la transaccion
                                        mostrar_error(Respuesta, conector)

                                    End If
                                Else
                                    'TODO: Error no continua con la transaccion
                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                End If
                            Else
                                'TODO: No hubo respuesta del servidor.
                                Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                                Respuesta.DisplayString = "El SP no respondió a la consulta de datos adicionales."

                            End If
                        End If
                    Else
                        mostrar_error(Respuesta, conector)
                        conector.Cancelar()
                    End If
                Else
                    Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                    Respuesta.DisplayString = "El servidor de tarjetas no responde"
                End If

            End Using
        Else
            Respuesta.Response = ResponseDescription.ERROR_EN_PP
            Respuesta.DisplayString = "No se puede utilizar el PP, falta sincronizar o inicializar."

        End If
        Return Respuesta
    End Function

    Public Function Devolucion(plan As Integer, importe As Decimal, cashback As Decimal, ticketoriginal As Integer, fechaOriginal As String) As ResponseToPOS
        Dim Respuesta As New ResponseToPOS
        If inicializado And sincronizado Then
            Using conector As New lib_PP.ConectorPP()

                '********** INICIO DE ANULACION - DESPIERTA EL PINPAD *****************
                If conector.Inicio_Anulacion() = 0 Then

                    Msg("-------- Respuesta a Y01 ----------")
                    Msg("Modo Ing                      : " & conector.RespuestaInicioCompra.modoIngreso)
                    'Msg("Nro Tarjeta                   : " & conector.RespuestaInicioCompra.tarjeta)
                    Dim rango As String = conector.RespuestaInicioCompra.tarjeta
                    Dim tipoTarjeta As String = rango.Substring(0, 1)
                    Dim CodigoServicioPIN As Integer = conector.RespuestaInicioCompra.codigoServicio
                    Msg("Cod. Serv.                    : " & conector.RespuestaInicioCompra.codigoServicio)
                    Msg("Cod. Banco                    : " & conector.RespuestaInicioCompra.codigoBanco)
                    Msg("Nro de registro               : " & conector.RespuestaInicioCompra.nroRegistro)
                    Msg("Abreviatura Tarjeta           : " & conector.RespuestaInicioCompra.abrebiaturaTarj)
                    Msg("Nombre Aplicacion CHIP        : " & conector.RespuestaInicioCompra.appChip)

                    '********** CONSULTA A SP QUE DATOS PEDIR EN EL COMANDO Y02 *****************
                    Dim mensajeRespuesta = Consultar_Datos_Adicionales(rango, conector.RespuestaInicioCompra.modoIngreso)

                    If mensajeRespuesta IsNot Nothing Then
                        If mensajeRespuesta.codRespuesta = "00" Then 'ENCONTRO EL MOVIMIENTO

                            '********** SOLICITA DATOS ADICIONALES *****************
                            If conector.Datos_adicionales(conector.RespuestaInicioCompra.modoIngreso,
                                                      mensajeRespuesta.u4d,
                                                      mensajeRespuesta.cds,
                                                      mensajeRespuesta.spi,
                                                      mensajeRespuesta.Clave_wkd,
                                                      mensajeRespuesta.Clave_wkp,
                                                      mensajeRespuesta.PosicionMK,
                                                      mensajeRespuesta.Importe,
                                                      mensajeRespuesta.CashBack,
                                                      tipoTarjeta,
                                                      CodigoServicioPIN) = 0 Then


                                Select Case conector.RespuestaInicioCompra.modoIngreso
                                    Case "M"
                                        Datos_Adicionales_Manual(conector)
                                    Case "B"
                                        Datos_Adicionales_Banda(conector)
                                    Case "C"
                                        Datos_Adicionales_Chip(conector)
                                    Case Else
                                        Msg("Modo de ingreso no contemplado.")
                                End Select

                                '********** ENVIA TRANSACCION A SP *****************
                                'mensajeRespuesta = Enviar_Transaccion(conector,
                                '                                      mensajeRespuesta.nroPlan,
                                '                                      mensajeRespuesta.Importe,
                                '                                      mensajeRespuesta.CashBack,
                                '                                      mensajeRespuesta.Ticket,
                                '                                      cajera,
                                '                                      mensajeRespuesta.Clave_wkp,
                                '                                      TransmisorTCP.TipoMensaje.Devolucion,
                                '                                      mensajeRespuesta.referenceNumber,
                                '                                      ticketoriginal)
                                If mensajeRespuesta IsNot Nothing Then
                                    Msg("-------------------------------")
                                    Msg("     RESPUESTA DEL SERVIDOR")
                                    Msg("-------------------------------")
                                    Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                    Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                    Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                    Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                    Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)

                                    Select Case conector.RespuestaInicioCompra.modoIngreso
                                        Case "M", "B"
                                            If mensajeRespuesta.codRespuesta = "00" Then
                                                Respuesta.Response = ResponseDescription.APROBADA
                                                Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion
                                            Else
                                                Respuesta.Response = ResponseDescription.NO_APROBADA
                                            End If
                                            Respuesta.Emisor = mensajeRespuesta.Emisor
                                            Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                            Respuesta.TicketString = mensajeRespuesta.cupon

                                        'Return mensajeRespuesta.codRespuesta
                                        'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                                        Case "C"
                                            If conector.Responder_Chip(mensajeRespuesta.Autorizacion, mensajeRespuesta.codRespuesta, mensajeRespuesta.criptograma, True) = 0 Then
                                                Msg("-------- Respuesta a Y03 ----------")
                                                Msg("Autorizacion                      : " & conector.RespuestaChip.autorizacion)
                                                Msg("Codigo de respuesta               : " & conector.RespuestaChip.codrespuesta)
                                                Msg("Respuesta                         : " & conector.RespuestaChip.mensajeRepuesta)
                                                Msg("Criptograma                       : " & conector.RespuestaChip.criptograma)


                                                If conector.RespuestaChip.codrespuesta = "00" Then
                                                    Respuesta.Response = ResponseDescription.APROBADA
                                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                    Respuesta.CodigoAutorizacion = mensajeRespuesta.Autorizacion
                                                    'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                                                Else
                                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                                    Respuesta.DisplayString = String.Format("Denegada por PP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)

                                                End If
                                                Respuesta.Emisor = mensajeRespuesta.Emisor
                                                Respuesta.TicketString = mensajeRespuesta.cupon
                                            Else
                                                Respuesta.Response = ResponseDescription.ERROR_EN_PP
                                                Respuesta.DisplayString = String.Format("El SP respondió pero el PP no respondió la confirmación del CHIP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)

                                            End If
                                            'mensajeRespuesta = Enviar_respuesta_Chip(pinguino, caja, conector, mensajeRespuesta.clavereverso)
                                            If mensajeRespuesta Is Nothing Then
                                                Respuesta.DisplayString = "El SP no respondió al envió de la respuesta del CHIP, puede que no se haya reversado."
                                            Else
                                                Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                            End If
                                        Case Else
                                            Msg("Modo de ingreso no contemplado.")
                                    End Select
                                Else
                                    'TODO: Aca habria que reversar. o ver que hacer.
                                    Select Case conector.RespuestaInicioCompra.modoIngreso

                                        Case "M", "B"
                                        'TODO: Aca habria que reversar. o ver que hacer.
                                        'Return Nothing
                                        Case "C"
                                            If conector.Responder_Chip("", "", "", False) = 0 Then

                                            End If

                                        Case Else
                                            Msg("Modo de ingreso no contemplado.")
                                    End Select
                                    Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                                    Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."

                                End If

                            Else
                                'TODO: Error no continua con la transaccion
                                mostrar_error(Respuesta, conector)

                            End If
                        Else
                            'TODO: Error no continua con la transaccion
                            Respuesta.Response = ResponseDescription.NO_APROBADA
                            Respuesta.DisplayString = mensajeRespuesta.Respuesta
                        End If
                    Else
                        'TODO: No hubo respuesta del servidor.
                        Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                        Respuesta.DisplayString = "El SP no respondió a la consulta de datos adicionales."

                    End If
                Else
                    mostrar_error(Respuesta, conector)

                End If

            End Using
        Else
            Respuesta.Response = ResponseDescription.ERROR_EN_PP
            Respuesta.DisplayString = "No se puede utilizar el PP, falta sincronizar o inicializar."

        End If
        Return Respuesta
    End Function
#End Region

#Region "Anulación-Devolución"
    Public Function AnulacionDevolucion(plan As Integer, importe As Decimal, cashback As Decimal, ticketoriginal As Integer) As ResponseToPOS
        Dim Respuesta As New ResponseToPOS
        If inicializado And sincronizado Then
            Using conector As New lib_PP.ConectorPP()

                '********** INICIO DE ANULACION - DESPIERTA EL PINPAD *****************
                If conector.Inicio_Anulacion() = 0 Then

                    Msg("-------- Respuesta a Y01 ----------")
                    Msg("Modo Ing                      : " & conector.RespuestaInicioCompra.modoIngreso)
                    'Msg("Nro Tarjeta                   : " & conector.RespuestaInicioCompra.tarjeta)
                    Dim rango As String = conector.RespuestaInicioCompra.tarjeta
                    Dim tipoTarjeta As String = rango.Substring(0, 1)
                    Dim CodigoServicioPIN As Integer = conector.RespuestaInicioCompra.codigoServicio
                    Msg("Cod. Serv.                    : " & conector.RespuestaInicioCompra.codigoServicio)
                    Msg("Cod. Banco                    : " & conector.RespuestaInicioCompra.codigoBanco)
                    Msg("Nro de registro               : " & conector.RespuestaInicioCompra.nroRegistro)
                    Msg("Abreviatura Tarjeta           : " & conector.RespuestaInicioCompra.abrebiaturaTarj)
                    Msg("Nombre Aplicacion CHIP        : " & conector.RespuestaInicioCompra.appChip)

                    '********** CONSULTA A SP QUE DATOS PEDIR EN EL COMANDO Y02 *****************
                    Dim mensajeRespuesta = Consultar_Datos_Adicionales_Anulacion(rango, ticketoriginal)

                    If mensajeRespuesta IsNot Nothing Then
                        If mensajeRespuesta.codRespuesta = "00" Then 'ENCONTRO EL MOVIMIENTO

                            '********** SOLICITA DATOS ADICIONALES *****************
                            If conector.Datos_adicionales(conector.RespuestaInicioCompra.modoIngreso,
                                                      mensajeRespuesta.u4d,
                                                      mensajeRespuesta.cds,
                                                      mensajeRespuesta.spi,
                                                      mensajeRespuesta.Clave_wkd,
                                                      mensajeRespuesta.Clave_wkp,
                                                      mensajeRespuesta.PosicionMK,
                                                      mensajeRespuesta.Importe,
                                                      mensajeRespuesta.CashBack,
                                                      tipoTarjeta,
                                                      CodigoServicioPIN) = 0 Then


                                Select Case conector.RespuestaInicioCompra.modoIngreso
                                    Case "M"
                                        Datos_Adicionales_Manual(conector)
                                    Case "B"
                                        Datos_Adicionales_Banda(conector)
                                    Case "C"
                                        Datos_Adicionales_Chip(conector)
                                    Case Else
                                        Msg("Modo de ingreso no contemplado.")
                                End Select

                                '********** ENVIA TRANSACCION A SP *****************
                                'mensajeRespuesta = Enviar_Transaccion(conector,
                                '                                      mensajeRespuesta.nroPlan,
                                '                                      mensajeRespuesta.Importe,
                                '                                      mensajeRespuesta.CashBack,
                                '                                      mensajeRespuesta.Ticket,
                                '                                      cajera,
                                '                                      mensajeRespuesta.Clave_wkp,
                                '                                      TransmisorTCP.TipoMensaje.Anulacion,
                                '                                      mensajeRespuesta.referenceNumber,
                                '                                      ticketoriginal)
                                If mensajeRespuesta IsNot Nothing Then
                                    Msg("-------------------------------")
                                    Msg("     RESPUESTA DEL SERVIDOR")
                                    Msg("-------------------------------")
                                    Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                    Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                    Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                    Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                    Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)

                                    Select Case conector.RespuestaInicioCompra.modoIngreso
                                        Case "M", "B"
                                            If mensajeRespuesta.codRespuesta = "00" Then
                                                Respuesta.Response = ResponseDescription.APROBADA
                                                Respuesta.TotalAmount = mensajeRespuesta.Importe
                                            Else
                                                Respuesta.Response = ResponseDescription.NO_APROBADA
                                            End If
                                            Respuesta.Emisor = mensajeRespuesta.Emisor
                                            Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                            Respuesta.TicketString = mensajeRespuesta.cupon

                                        'Return mensajeRespuesta.codRespuesta
                                        'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                                        Case "C"
                                            If conector.Responder_Chip(mensajeRespuesta.Autorizacion, mensajeRespuesta.codRespuesta, mensajeRespuesta.criptograma, True) = 0 Then
                                                Msg("-------- Respuesta a Y03 ----------")
                                                Msg("Autorizacion                      : " & conector.RespuestaChip.autorizacion)
                                                Msg("Codigo de respuesta               : " & conector.RespuestaChip.codrespuesta)
                                                Msg("Respuesta                         : " & conector.RespuestaChip.mensajeRepuesta)
                                                Msg("Criptograma                       : " & conector.RespuestaChip.criptograma)


                                                If conector.RespuestaChip.codrespuesta = "00" Then
                                                    Respuesta.Response = ResponseDescription.APROBADA
                                                    Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                                    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                                    'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                                                Else
                                                    Respuesta.Response = ResponseDescription.NO_APROBADA
                                                    Respuesta.DisplayString = String.Format("Denegada por PP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)

                                                End If
                                                Respuesta.Emisor = mensajeRespuesta.Emisor
                                                Respuesta.TicketString = mensajeRespuesta.cupon
                                            Else
                                                Respuesta.Response = ResponseDescription.ERROR_EN_PP
                                                Respuesta.DisplayString = String.Format("El SP respondió pero el PP no respondió la confirmación del CHIP. Respuesta HOST: {0}, Respuesta PP: {1}", mensajeRespuesta.codRespuesta, conector.RespuestaChip.codrespuesta)

                                            End If
                                            'mensajeRespuesta = Enviar_respuesta_Chip(pinguino, caja, conector, mensajeRespuesta.clavereverso)
                                            If mensajeRespuesta Is Nothing Then
                                                Respuesta.DisplayString = "El SP no respondió al envió de la respuesta del CHIP, puede que no se haya reversado."
                                            Else
                                                Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                            End If
                                        Case Else
                                            Msg("Modo de ingreso no contemplado.")
                                    End Select
                                Else
                                    'TODO: Aca habria que reversar. o ver que hacer.
                                    Select Case conector.RespuestaInicioCompra.modoIngreso

                                        Case "M", "B"
                                        'TODO: Aca habria que reversar. o ver que hacer.
                                        'Return Nothing
                                        Case "C"
                                            If conector.Responder_Chip("", "", "", False) = 0 Then

                                            End If

                                        Case Else
                                            Msg("Modo de ingreso no contemplado.")
                                    End Select
                                    Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                                    Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."

                                End If

                            Else
                                'TODO: Error no continua con la transaccion
                                mostrar_error(Respuesta, conector)

                            End If
                        Else
                            'TODO: Error no continua con la transaccion
                            Respuesta.Response = ResponseDescription.NO_APROBADA
                            Respuesta.DisplayString = mensajeRespuesta.Respuesta
                        End If
                    Else
                        'TODO: No hubo respuesta del servidor.
                        Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                        Respuesta.DisplayString = "El SP no respondió a la consulta de datos adicionales."

                    End If
                Else
                    mostrar_error(Respuesta, conector)

                End If

            End Using
        Else
            Respuesta.Response = ResponseDescription.ERROR_EN_PP
            Respuesta.DisplayString = "No se puede utilizar el PP, falta sincronizar o inicializar."

        End If
        Return Respuesta
    End Function
#End Region

#Region "Reverso"
    Public Function ReversoTest() As TransmisorTCP.MensajeRespuesta
        Return Transmitir(revtest)
    End Function

    Public Function Reversar(rev As TransmisorTCP.MensajeIda) As TransmisorTCP.MensajeRespuesta
        Return Transmitir(rev)
    End Function

    Public Function MandarAdvice(rev As TransmisorTCP.MensajeIda) As TransmisorTCP.MensajeRespuesta
        Return Transmitir(rev)
    End Function

    Private Function Enviar_Reverso_test(conector As ConectorPP, cuotas As Integer, importe As Decimal, cashback As Decimal, ticket As Integer, cajera As Integer, clave As String, tipoTrx As TransmisorTCP.TipoMensaje, ByRef rev As TransmisorTCP.MensajeIda) As MensajeRespuesta

        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.cajera = cajera

        req.ticketCaja = ticket
        req.tipoMensaje = tipoTrx
        req.nroTarjeta = conector.RespuestaDatosAdicionales.tarjeta
        req.secuencia = conector.RespuestaDatosAdicionales.secuenciaPAN

        If conector.RespuestaDatosAdicionales.tipoEncripcion = "N" Then
            req.datosEncriptados = generar_paquete(conector, clave)
        Else
            req.datosEncriptados = conector.RespuestaDatosAdicionales.Paquete
        End If

        If tipoTrx = TransmisorTCP.TipoMensaje.Compra Then Setear_Plan(req, cuotas)
        req.moneda = Moneda.Pesos
        req.versionAPP = versionAPP
        req.criptograma = conector.RespuestaDatosAdicionales.criptograma
        req.fechaTransaccion = Now
        req.importeCompra = importe
        req.importeCashback = cashback
        req.idEncripcion = conector.RespuestaDatosAdicionales.idEncription
        req.tipocuenta = conector.RespuestaDatosAdicionales.tipoCuenta
        req.pinBlock = conector.RespuestaDatosAdicionales.PinBlock
        req.nombreTHabiente = conector.RespuestaDatosAdicionales.TarjetaHabiente
        req.reversar = True
        Select Case conector.RespuestaInicioCompra.modoIngreso
            Case "B"
                req.tipoIngreso = TipoIngreso.Banda
            Case "M"
                req.tipoIngreso = TipoIngreso.Manual
            Case "C"
                req.tipoIngreso = TipoIngreso.Chip
            Case "L"
                req.tipoIngreso = TipoIngreso.Contactless
        End Select


        req.serviceCode = conector.RespuestaInicioCompra.codigoServicio

        Return Transmitir(req)


    End Function

    Public Function Reverso_Compra(pCuotas As Integer, pImporte As Decimal, pCashback As Decimal, ticketcaja As Integer) As ResponseToPOS
        Dim Respuesta As New ResponseToPOS
        Dim Reverso As New TransmisorTCP.MensajeIda
        If inicializado And sincronizado Then
            Using conector As New lib_PP.ConectorPP()
                Dim conCashback As Boolean
                If pCashback > 0 Then
                    conCashback = True
                Else
                    conCashback = False
                End If
                '********** INICIO DE REVERSO - DESPIERTA EL PINPAD *****************
                If conector.Inicio_Compra(conCashback) = 0 Then

                    Msg("-------- Respuesta a Y01 ----------")
                    Msg("Modo Ing                      : " & conector.RespuestaInicioCompra.modoIngreso)
                    'Msg("Nro Tarjeta                   : " & conector.RespuestaInicioCompra.tarjeta)
                    Dim rango As String = conector.RespuestaInicioCompra.tarjeta
                    Dim tipoTarjeta As Integer = rango.Substring(0, 1)
                    Dim CodigoServicioPIN As Integer = conector.RespuestaInicioCompra.codigoServicio
                    Msg("Cod. Serv.                    : " & conector.RespuestaInicioCompra.codigoServicio)
                    Msg("Cod. Banco                    : " & conector.RespuestaInicioCompra.codigoBanco)
                    Msg("Nro de registro               : " & conector.RespuestaInicioCompra.nroRegistro)
                    Msg("Abreviatura Tarjeta           : " & conector.RespuestaInicioCompra.abrebiaturaTarj)
                    Msg("Nombre Aplicacion CHIP        : " & conector.RespuestaInicioCompra.appChip)

                    '********** CONSULTA A SP QUE DATOS PEDIR EN EL COMANDO Y02 *****************
                    Dim mensajeRespuesta = Consultar_Datos_Adicionales(rango, conector.RespuestaInicioCompra.modoIngreso)

                    If mensajeRespuesta IsNot Nothing Then
                        '********** SOLICITA DATOS ADICIONALES *****************
                        If conector.Datos_adicionales(conector.RespuestaInicioCompra.modoIngreso,
                                                      mensajeRespuesta.u4d,
                                                      mensajeRespuesta.cds,
                                                      mensajeRespuesta.spi,
                                                      mensajeRespuesta.Clave_wkd,
                                                      mensajeRespuesta.Clave_wkp,
                                                      mensajeRespuesta.PosicionMK,
                                                      pImporte,
                                                      pCashback,
                                                      tipoTarjeta,
                                                      CodigoServicioPIN) = 0 Then

                            '******** Muestra mensajes segun modo ingreso *********
                            Select Case conector.RespuestaInicioCompra.modoIngreso
                                Case "M"
                                    Datos_Adicionales_Manual(conector)
                                Case "B"
                                    Datos_Adicionales_Banda(conector)
                                Case "C"
                                    Datos_Adicionales_Chip(conector)
                                Case Else
                                    Msg("Modo de ingreso no contemplado.")
                            End Select

                            '********** ENVIA TRANSACCION A SP *****************
                            mensajeRespuesta = Enviar_Reverso_test(conector, pCuotas, pImporte, pCashback, ticketcaja, cajera, mensajeRespuesta.Clave_wkp, TransmisorTCP.TipoMensaje.Compra, Nothing)
                            If mensajeRespuesta IsNot Nothing Then
                                Msg("-------------------------------")
                                Msg("     RESPUESTA DEL SERVIDOR")
                                Msg("-------------------------------")
                                Msg("Codigo de respuesta       : " & mensajeRespuesta.codRespuesta)
                                Msg("Respuesta                 : " & mensajeRespuesta.Respuesta)
                                Msg("Importe trx               : " & mensajeRespuesta.Importe)
                                Msg("Emisor                    : " & mensajeRespuesta.Emisor)
                                Msg("Mensaje Host              : " & mensajeRespuesta.mensajeHost)

                                Select Case conector.RespuestaInicioCompra.modoIngreso
                                    Case "M", "B"

                                        If mensajeRespuesta.codRespuesta = "00" Then
                                            Respuesta.Response = ResponseDescription.APROBADA
                                            '    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                        Else
                                            Respuesta.Response = ResponseDescription.NO_APROBADA
                                        End If
                                        'Respuesta.Emisor = mensajeRespuesta.Emisor
                                        Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                        Respuesta.TicketString = mensajeRespuesta.cupon

                                        'Return mensajeRespuesta.codRespuesta
                                        'TODO: SETEAR TICKET Y MENSAJE PARA MOSTRAR
                                    Case "C"
                                        conector.Responder_Chip("", "", "", False)
                                        If mensajeRespuesta.codRespuesta = "00" Then
                                            Respuesta.Response = ResponseDescription.APROBADA
                                            '    Respuesta.TotalAmount = mensajeRespuesta.Importe
                                        Else
                                            Respuesta.Response = ResponseDescription.NO_APROBADA
                                        End If
                                        'Respuesta.Emisor = mensajeRespuesta.Emisor
                                        Respuesta.DisplayString = mensajeRespuesta.Respuesta
                                        Respuesta.TicketString = mensajeRespuesta.cupon
                                    Case Else
                                        Msg("Modo de ingreso no contemplado.")
                                End Select
                            Else
                                '---NO HUBO RESPUESTA
                                Select Case conector.RespuestaInicioCompra.modoIngreso

                                    Case "M", "B"

                                        'Return Nothing
                                    Case "C"
                                        conector.Cancelar()
                                    Case Else
                                        Msg("Modo de ingreso no contemplado.")
                                End Select
                                Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                                Respuesta.DisplayString = "El SP no respondió la transacción. Reintente más tarde."

                            End If

                        Else
                            'ERROR EN SOLICITUD DE DATOS ADICIONALES
                            'TODO: Error no continua con la transaccion
                            mostrar_error(Respuesta, conector)

                        End If

                    Else
                        'TODO: No hubo respuesta del servidor para la consulta de datos adicionales.
                        Respuesta.Response = ResponseDescription.NO_HAY_RESPUESTA
                        Respuesta.DisplayString = "El SP no respondió a la consulta de datos adicionales. Reintente."
                        conector.Cancelar()

                    End If
                Else
                    'ERROR EN EL DESPERTAR AL PINPAD
                    mostrar_error(Respuesta, conector)

                End If

            End Using
        Else
            Respuesta.Response = ResponseDescription.ERROR_EN_PP
            Respuesta.DisplayString = "No se puede utilizar el PP, falta sincronizar o inicializar."

        End If
        Return Respuesta


    End Function

#End Region

#Region "Inicialización"

    Public Sub Arrancar_PP()
        If Sincronizar() = ResponseDescription.SINCRONIZADO_OK Then
            sincronizado = True
            If Inicializar_PinPad() = ResponseDescription.INICIALIZADO_OK Then
                inicializado = True
            Else
                Msg("No se pudo inicializar el pinpad, no puede operar.")
                inicializado = False
            End If
        Else
            sincronizado = False
            Msg("No se pudo sincronizar el pinpad, no puede operar.")
        End If

        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Then
            inicializado = True
            sincronizado = True
        End If

        Msg("Arranca_PP: " + sincronizado.ToString() + ", " + inicializado.ToString())
    End Sub



    Public Function Inicializar_PinPad() As ResponseDescription

        Using conector As New lib_PP.ConectorPP
            If conector.Inicializar() = 0 Then

                Msg("-------- Respuesta a Y00 ----------")
                Msg("Version S. O.                    : " & conector.RespuestaInicializacion.versionSO)
                Msg("Version del soft                 : " & conector.RespuestaInicializacion.versionSOFT)
                Msg("Version del Kernel               : " & conector.RespuestaInicializacion.versionKernel)
                Msg("Version del Kernel Contactless   : " & conector.RespuestaInicializacion.versionKernelCTLS)
                Msg("Soporta Contactless              : " & conector.RespuestaInicializacion.soportaCL)
                Msg("Soporta Impresion                : " & conector.RespuestaInicializacion.soprtaIMP)
                Msg("Clave RSA Presente               : " & conector.RespuestaInicializacion.soportaRSA)
                Msg("Pantalla TOUCH                   : " & conector.RespuestaInicializacion.soportaTOUCH)
                'AGREGADO PARA VX690
                Msg("Nro Serie Fisico                 : " & conector.RespuestaInicializacion.serieFisico)
                versionAPP = conector.RespuestaInicializacion.versionSOFT
                inicializado = True

                '--- en SetearVersion(versionAPP), actualiza en la tabla NUMEROS el numero de serie según el pinguino, caja, hosts 
                '--- va a la clase Sub ProcesarTCCP(...) Handles Transmisor.DatosRecibidos en el ServerTar (proyecto SERVER)       
                Dim respuesta As TransmisorTCP.MensajeRespuesta = SetearVersion(versionAPP)

                If respuesta IsNot Nothing Then
                    Return ResponseDescription.INICIALIZADO_OK
                Else
                    Return ResponseDescription.NO_INICIALIZADO
                End If
            Else
                Dim Mensaje As ResponseToPOS
                mostrar_error(Mensaje, conector)
                inicializado = False
                Return ResponseDescription.NO_INICIALIZADO
            End If
        End Using
    End Function

    Private Function SetearVersion(version As String) As TransmisorTCP.MensajeRespuesta
        Dim tiempo As Date = Now
        Dim req As New TransmisorTCP.MensajeIda
        req.caja = caja
        req.ping = pinguino
        req.versionAPP = version
        req.tipoMensaje = TransmisorTCP.TipoMensaje.ActualizarVersion
        If Homologacion Then
            req.host = TransmisorTCP.TipoHost.Posnet_homolog
        Else
            req.host = TransmisorTCP.TipoHost.POSNET
        End If

        Return Transmitir(req)
    End Function

#End Region

#Region "Sincronizacion"
    ''' <summary>
    ''' Sincroniza el pinpad, para su uso si es necesario. Devuelve 6 si se sincronizo ok.
    ''' </summary>
    ''' <returns></returns>
    Public Function Sincronizar() As ResponseDescription

        Try
            Dim conector As New lib_PP.ConectorPP()
            Using conector
                '--- Busca nro de terminal y serie del pinpad almacenado 
                'Msg("-------- ANTES Consultar_terminal()")
                'MsgBox("1008-------- ANTES Consultar_terminal()")

                Msg("-------- Consultar_Terminal ----------")

                Dim respuesta As TransmisorTCP.MensajeRespuesta = Consultar_terminal()
                'MsgBox("1008-------- DESPUES Consultar_terminal()")
                'Msg("-------- DESPUES Consultar_terminal()")
                If respuesta IsNot Nothing Then
                    '--- Consulta si debe sincronizarce 
                    'MsgBox("1008-------- ANTES Inicio_Sincronizacion()")
                    Msg("-------- Inicio_Sincronizacion() --------------")
                    If conector.Inicio_Sincronizacion(respuesta.terminal, respuesta.serie_PP) = 0 Then    '--- ACA HACE EL S00 
                        Msg("-------- Respuesta a S00 ----------")
                        Msg("Estado         : " & conector.RespuestaInicioSincronizacion.descripcion)
                        Msg("Cod Respuesta  : " & conector.RespuestaInicioSincronizacion.codRespuesta)

                        'TODO: Revisar si existen transacciones sin cerrar. 
                        'TODO: Sino se debe hacer un cierre o imposibilitar transaccionar.
                        If conector.RespuestaInicioSincronizacion.codRespuesta = 2 Then     '--- RESPUESTA 2 ES QUE NO ESTA SINCRONIZADO

                            'Comienza con la sincronizacion.
                            If conector.Solicitar_Sincronizacion(respuesta.terminal) = 0 Then   '--- ACA HACE EL S01
                                Msg("-------- Respuesta a S01 ----------")
                                Dim Cola_vta As New List(Of TransmisorTCP.MensajeRespuesta)

                                '--- RECORRO TODOS LOS HOST QUE TRAJE DEL S01 y LO SINCRONIZO Y LO ENCOLO EN EL VTA
                                For x As Integer = 0 To conector.RespuestaSolicitudSincronizacion.Count - 1
                                    Msg(String.Format(" <<<<<< CLAVE INYECTADA NRO {0} >>>>>>", x + 1))
                                    Msg("Nombre HOST    : " & conector.RespuestaSolicitudSincronizacion(x).NombreHosts)
                                    Msg("Posicion MK    : " & conector.RespuestaSolicitudSincronizacion(x).PrimaryMK)
                                    Msg("ID Encripción  : " & conector.RespuestaSolicitudSincronizacion(x).IDEncripcion)

                                    Dim req As New TransmisorTCP.MensajeIda
                                    req.caja = caja
                                    req.ping = pinguino
                                    req.tipoMensaje = TransmisorTCP.TipoMensaje.Sincronizacion
                                    req.idEncripcion = conector.RespuestaSolicitudSincronizacion(x).IDEncripcion
                                    req.host = Host(conector.RespuestaSolicitudSincronizacion(x).NombreHosts)
                                    req.posMK = conector.RespuestaSolicitudSincronizacion(x).PrimaryMK
                                    '--- LO SINCRONIZO CON SERVIDOR DE TARJETAS, BUSCO LAS CLAVES DE CADA HOSTS EN LA BASE DE DATOS
                                    '--- MANDA EL 0800 A FIRSTDATA (LO HACE EL SERVIDOR DE TARJETAS)
                                    Dim vta = Sincronizar(req)

                                    vta.PosicionMK = conector.RespuestaSolicitudSincronizacion(x).PrimaryMK
                                    Cola_vta.Add(vta)
                                Next

                                Msg("---------- RESPUESTA DEL SERVER -----------")
                                For Each vta In Cola_vta
                                    'confirmar sincro
                                    Msg("HOST sincronizado     : " & vta.host)
                                    Msg("Respuesta Servidor    : " & vta.codRespuesta)
                                    Msg("Clave de datos        : " & vta.Clave_wkd)
                                    Msg("Clave de pines        : " & vta.Clave_wkp)
                                    Msg("Control clave datos   : " & vta.Clave_ctrlwkd)
                                    Msg("Control clave pines   : " & vta.Clave_ctrlwkp)

                                    If vta.Clave_wkd <> "" Then conector.Cargar_claves(vta.host, lib_PP.Tipos_claves.datos, vta.Clave_wkd, vta.Clave_ctrlwkd, vta.PosicionMK)
                                    If vta.Clave_wkp <> "" Then conector.Cargar_claves(vta.host, lib_PP.Tipos_claves.pines, vta.Clave_wkp, vta.Clave_ctrlwkp, vta.PosicionMK)
                                    If vta.Clave_wkd = "" And vta.Clave_wkp = "" Then

                                        Msg("No se obtuvieron claves. No se puede sincronizar.")
                                        Cancelar_PinPad()
                                        sincronizado = False
                                        Return ResponseDescription.NO_SINCRONIZADO
                                    End If
                                Next

                                'Confirma la sincronización al Pinpad.
                                If conector.Confirmar_Sincronizacion(respuesta.terminal) = 0 Then
                                    Msg("-------- Respuesta a S02 ----------")
                                    Msg("Codigo Respuesta     : " & conector.RespuestaConfirmacionSincronizacion.codRespuesta)
                                    Msg("Descripcion          : " & conector.RespuestaConfirmacionSincronizacion.descripcion)
                                    Msg("Serie PP             : " & conector.RespuestaConfirmacionSincronizacion.seriePP)
                                    If conector.RespuestaConfirmacionSincronizacion.codRespuesta = 0 Then
                                        'Finaliza la sincronización, grabando en tabla Numeros el serie del pinpad actual.
                                        If Finalizar_Sincronizacion(respuesta.terminal, conector.RespuestaConfirmacionSincronizacion.seriePP).Sincronizado Then
                                            sincronizado = True
                                            Return ResponseDescription.SINCRONIZADO_OK
                                        Else
                                            sincronizado = False
                                            Return ResponseDescription.NO_SINCRONIZADO
                                        End If
                                    Else
                                        sincronizado = False
                                        Return ResponseDescription.NO_SINCRONIZADO
                                    End If

                                Else
                                    sincronizado = False
                                    Return ResponseDescription.NO_SINCRONIZADO
                                End If
                            Else
                                sincronizado = False
                                Return ResponseDescription.NO_SINCRONIZADO
                            End If
                        ElseIf conector.RespuestaInicioSincronizacion.codRespuesta = 0 Then
                            sincronizado = True
                            Return ResponseDescription.SINCRONIZADO_OK
                        Else
                            sincronizado = False
                            Return ResponseDescription.NO_SINCRONIZADO
                        End If
                    Else
                        sincronizado = False
                        Return ResponseDescription.NO_SINCRONIZADO
                    End If
                Else
                    Msg("No hubo respuesta del servidor")
                    sincronizado = False
                    Return ResponseDescription.NO_SINCRONIZADO
                End If
            End Using
        Catch ex As Exception
            Msg("ERROR DE SINCRONIZACION: " & ex.Message)
            sincronizado = False
            Return ResponseDescription.NO_SINCRONIZADO
        End Try

    End Function



    Private Function Consultar_terminal() As TransmisorTCP.MensajeRespuesta
        Dim tiempo As Date = Now
        Dim req As New TransmisorTCP.MensajeIda          'creo mensaje req para enviar al sistema tarjeta
        req.caja = caja
        req.ping = pinguino
        req.tipoMensaje = TransmisorTCP.TipoMensaje.PedirTerminal
        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Then
            Homologacion = True
            req.host = TransmisorTCP.TipoHost.Posnet_homolog
        Else
            'MsgBox("1008-------- PRODUCCION")
            req.host = TransmisorTCP.TipoHost.POSNET
        End If

        Return Transmitir(req)         '---  ESTO VA AL TRANSMISORTCP QUE ESTA ESCUCHANDO QUE LLEGUE ALGO, ESTá EN EL SERVIDOR DE TARJETAS (MANDA UN PEDIRTERMINAL), EJECUTA EL SUB ProcesarTCP


    End Function


    Private Function Sincronizar(requerimiento As TransmisorTCP.MensajeIda) As TransmisorTCP.MensajeRespuesta
        Return Transmitir(requerimiento)
    End Function

    Private Function Finalizar_Sincronizacion(pTerminal As String, pSeriePP As String) As TransmisorTCP.MensajeRespuesta
        Dim tiempo As Date = Now
        Dim req As New TransmisorTCP.MensajeIda
        req.tipoMensaje = TransmisorTCP.TipoMensaje.ActualizarSerie
        req.terminal = pTerminal
        req.nroSerie = pSeriePP

        If Homologacion Then
            req.host = TipoHost.Posnet_homolog
        Else
            req.host = TipoHost.POSNET
        End If

        Return Transmitir(req)


    End Function

#End Region

#Region "Cancelar"
    Public Function Cancelar_PinPad() As Integer

        Using conector As New lib_PP.ConectorPP
            'If conector.Cancelar() = 0 Then
            Msg("-------- Respuesta a Y06 ----------")
                Msg("Comando Cancelado                   : " & conector.RespuestaCancelacion.cancelado)

                Return 0
            'Else
            '    Dim respuesta As ResponseToPOS
            '    mostrar_error(respuesta, conector)
            '    Return -1
            'End If

        End Using


    End Function
#End Region
#Region "Actualizar"
    Public Function Actualizar_PinPad(fileReader As String) As Integer

        Dim cont As Integer
        If fileReader.Length Mod 4000 > 0 Then
            cont = Int((fileReader.Length / 4000)) + 1
        Else
            cont = fileReader.Length / 4000
        End If

        Using conector As New lib_PP.ConectorPP

            Dim pos As Integer = 0
            For x = 1 To cont
                Dim cadena As String
                If x = cont Then
                    cadena = fileReader.Substring(pos)
                Else
                    cadena = fileReader.Substring(pos, 4000)
                End If

                pos = pos + 4000
                Progreso($"{x}|{cont}")

                If conector.actualizar(cadena, x, "APP") = 0 Then
                    Console.WriteLine("Paso bien")
                Else
                    Dim respuesta As ResponseToPOS
                    mostrar_error(respuesta, conector)
                    Return -1
                End If

            Next
            If conector.actualizar("", 0, "APP") = 0 Then
                Msg("-------- Respuesta a Y31 ----------")
                Msg("Archivo Actualizado               : " & conector.RespuestaActualizacion)
                Return 0
            End If
        End Using


    End Function
#End Region
    Private Function Transmitir(Req As TransmisorTCP.MensajeIda) As TransmisorTCP.MensajeRespuesta
        Dim tiempo As Date = Now
        Try
            '--- ESTO VA AL TransmisorTCP QUE ESTA ESCUCHANDO QUE LLEGUE ALGO, ESTA EN EL SERVIDOR DE TARJETAS, utiliza la ClaseServidorSocket 
            '--- EJECUTA EL SUB ProcesarTCP                                                                                                    
            'MsgBox("1008-------- TRANSMITIR")
            transmisor.Conectar()
            continuar = False
            transmisor.EnviarDatos(Req)

            '--- UNA VEZ QUE TRANSMITIO, ACA TAMBIEN ESPERA QUE LLEGUE ALGO, LA RESPUESTA, ACA utiliza la ClaseClienteSocket       
            '--- EJECUTA   *** Private Sub Respuesta_Recibida() Handles transmisor.DatosRecibidos  *** que pone continuar en true  

            Do
                '--- espera que continuar se ponga en true o supere el tiempo del timeout  
            Loop Until continuar Or Now.Subtract(tiempo).TotalSeconds > TIMEOUTREQUEST

            If continuar Then
                Return transmisor.Respuesta   '--- devuelve la respuesta que viene del servidor (lo lee en la clase CLASECLIENTESOCKET --> leersocket)
            Else
                Return Nothing
            End If
        Catch ex As Exception
            'MOSTRAR ERROR
            Return Nothing
        Finally
            transmisor.Desconectar()
        End Try
    End Function

    Private Function Host(pNombreHost As String) As TransmisorTCP.TipoHost
        Select Case pNombreHost
            Case "POSNET"
                Return TransmisorTCP.TipoHost.POSNET
            Case "VISA"
                Return TransmisorTCP.TipoHost.VISA
        End Select

    End Function




    Private Sub Respuesta_Recibida() Handles transmisor.DatosRecibidos
        continuar = True
    End Sub

End Class
