Option Strict On
Imports TjComun.IdaLib
Imports IsoLib

' IDA del reg3000
'Dim VERSION As String          '* 1       ' NRO DE VERSION               
'Dim TARJ As String             '* 20      ' NRO DE TARJETA               
'Dim EXPDATE As String          '* 4       ' FECHA DE EXPIRACION          
'Dim IMPO As Decimal            'CURRENCY         ' IMPORTE DE LA TRANSACCION    
'Dim MANUAL As Short            ' MODO DE INGRESO 0-MANUAL 1-AU
'Dim PLANINT As Short           ' COD.PLAN                     
'Dim CODSEG As String           '* 30      ' COD. SEGURIDAD            
'Dim TICCAJ As Integer          ' NRO DE TICKET DE LA CAJA     
'Dim CAJERA As Short            ' COD DE CAJERO (OPERADOR)     
'Dim HORA As Single             ' FECHA/HORA                   
'Dim TRACK2 As String           '* 37      '                              
'Dim TRACK1 As String           '* 77      '                              
'Dim CodAut As String           '* 6       ' Codigo de autorizacion pa/anu
'Dim TKTORI As Integer          '                              
'Dim FECORI As String           '* 6       '                              
'Dim PLANINTori As Short        ' COD.PLAN                     
'Dim oper As Short              ' !! operacion                 
'Dim cmd As Short               '+8 offline +16 anular                  
'Dim cajadir As String          '* 26      '                          
'Dim TKID As String             '* 4
'Dim CASHBACK As Decimal        '                          
'Dim CHECK As String            '* 2      ' HACER = CHECKID        

'------------------------------------------------------------------------
'   VALOR DE LA VARIABLE OPER QUE HAY QUE PASAR A GUSTAVO:               
'    0 = COMPRA CON PASADA DE TARJETA                                    
'    0 = COMPRA CON ENTRADA DE DATOS EN FORMA MANUAL                     
'    1 = DEVOLUCION CON ENTRADA DE DATOS EN FORMA MANUAL                 
'    2 = ANULACION DE COMPRA CON ENTRADA DE DATOS EN FORMA MANUAL        
'    3 = ANULACION DE DEVOLUCION CON ENTRADA DE DATOS EN FORMA MANUAL    
'    4 = SOLICITUD DE SALDO EN CUENTA TARJETA MAESTRO                    
'------------------------------------------------------------------------


Public Class Req

#Region "Atributos"
    Property ida As IdaTypeInternal

    Public vta As VtaType


    Public emisor As Integer
    Public descEmisor As String
    Public nrohost As Byte
    Public idComercio As String

    Public obligarCodSeg As Boolean
    Public obligar4Dig As Boolean
    Public obligarFechaVenc As Boolean
    Public nroPlan As Integer
    Public cuotas As Integer
    Public moneda As String
    Public descPlan As String
    Public coeficiente As Decimal
    Public planemisor As String
    Public nroTrace As Integer
    'Public nroTraceAdv As Integer
    Public nroTracePEI As String
    Public nroTicket As Integer
    Public nroLote As Integer
    Public pinBlock As String
    Public nroCaja As Integer
    Public terminal As String
    Public idred As Integer
    Public FALLBACK As Boolean = False

    'Public cashback As Decimal
    Public operacion As E_ProcCode
    Public nombreHost As String
    Public pinguino As String
    Public autorizacion As String
    Public cliente As String
    Public esMaestro As Boolean = False
    Public esCabal As Boolean = False 'CABAL DEBITO ES 604201
    Public importe As Decimal

    Public esVisaDebito As Boolean = False
    Public esCashback As Boolean
    Public tipocuenta As Integer
    Public nrocuenta As String
    Public RespInvalida As Boolean = False
    Public CausaInvalida As String
    Public tipoMensaje As String
    Public retrefnumber As String

    Public CierreMontoCompras As Double
    Public CierreCantCompras As Integer
    Public CierreCantDevoluciones As Integer
    Public CierreMontoDevoluciones As Double
    Public CierreCantAnulaciones As Integer
    Public CierreMontoAnulaciones As Double
    Public fecha_cierre As Date
    Public OperacionPEI As String
    Public esReqPEI As Boolean
    Public tipoRequerimiento As String = ""
    Private emv As Boolean = False

    Public msjIda As New TransmisorTCP.MensajeIda
    Public msjRespuesta As New TransmisorTCP.MensajeRespuesta

    Public clientStream As System.Net.IPEndPoint
    Public idEncripcion As String
    Public ClaveDatos As String
    Public ClavePines As String
    Friend respuesta As String


#End Region
    ''' <summary>
    ''' Inicializa Req para cierre total.
    ''' </summary>
    ''' <param name="pTerm"></param>
    ''' <param name="host"></param>
    ''' <remarks></remarks>
    Sub New(pTerm As String, host As Integer, fecha As Date)
        fecha_cierre = fecha
        terminal = Trim(pTerm)
        nrohost = CByte(host)
        idComercio = BuscarIDComercioCierre(nrohost)
        inicializaReqCierre()
    End Sub

    ''' <summary>
    ''' Inicializa Req para cierre por caja.
    ''' </summary>
    ''' <param name="host"></param>
    ''' <param name="pPin"></param>
    ''' <param name="pCaja"></param>
    ''' <remarks></remarks>
    Sub New(host As Integer, pPin As Integer, pCaja As Integer)
        pinguino = CStr(pPin)
        nroCaja = pCaja
        terminal = BuscarTerminal(pinguino + nroCaja.ToString("00"), nrohost)
        idComercio = BuscarIDComercioCierre(nrohost)
        inicializaReqCierre()
    End Sub



    ''' <summary>
    ''' Inicializa Req para reverso test.
    ''' </summary>
    ''' <param name="_ida"></param>
    ''' <param name="trace"></param>
    ''' <param name="ticket"></param>
    ''' <remarks></remarks>
    Sub New(_ida As IdaTypeInternal, trace As String, ticket As String, pTerm As String, pHost As String)
        ida = _ida
        nroTrace = CInt(trace)
        nroTicket = CInt(ticket)
        terminal = Trim(pTerm)
        nombreHost = pHost
        inicializaReversoTest()
    End Sub

    Private Sub inicializaReversoTest()
        nrohost = 1
        verSiEsMaestro()
        BuscarMovimiento(Me)
        FechaHoraEnvioMsg = Now
        nroCaja = 20
        pinguino = "04"
        'BuscarEmisorEnRangHab(Me)
        'BuscarEmisor(Me)
        'nroPlan = ida.PLANINT
        'BuscarPlan(Me)
    End Sub
    ''' <summary>
    ''' Inicializa req para reverso.
    ''' </summary>
    ''' <param name="pIda"></param>
    ''' <param name="pTerminal"></param>
    ''' <param name="pHost"></param>
    ''' <remarks></remarks>
    Sub New(pIda As IdaTypeInternal, pTerminal As String, pHost As Integer)
        ida = pIda
        terminal = Trim(pTerminal)
        nrohost = CByte(pHost)

        nombreHost = UCase(CType(pHost, TipoHost).ToString)
        verSiEsMaestro()
        esCashback = verSiEsCashback()
    End Sub

    ''' <summary>
    ''' Inicializa Req para venta normal.
    ''' </summary>
    ''' <param name="_ida"></param>
    ''' <param name="_nrocaja"></param>
    ''' <remarks></remarks>
    Sub New(_ida As IdaTypeInternal, _nrocaja As Integer, _pei As Boolean)
        nroCaja = _nrocaja
        ida = _ida
        pinguino = Mid(ida.cajadir, ida.cajadir.Length - 1)
        If _pei Then
            inicializaReqPEI()
        Else
            inicializaReq()
        End If

    End Sub

    Private Sub inicializaReqPEI()
        esReqPEI = True
        If ida.oper = 0 Then
            OperacionPEI = lib_PEI.Concepto.COMPRA_DE_BIENES.ToString
        ElseIf ida.oper = 1 Or ida.oper = 4 Then
            OperacionPEI = lib_PEI.Concepto.DEVOLUCION.ToString
            If ida.TKTORI = 0 Then InvalidarReq("Falta id de operacion original")
        End If
        validar()
        BuscarEmisorEnRangHab(Me)
        BuscarEmisor(Me)

        'TODO: ver si es devolucion (oper = 1) si tenenmos que buscar segun ida.PLANINTori
        'nroPlan = ida.PLANINT
        'BuscarPlan(Me)
        If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
            nrohost = 7
            idComercio = "385"
        Else
            nrohost = 8
            idComercio = "1540"
        End If
        terminal = BuscarTerminal(pinguino + nroCaja.ToString("00"), nrohost)

        If terminal = "" Then InvalidarReq("No se encontró terminal.")

        Incrementar_Trace(terminal.Trim, nrohost)
        Try
            nroTrace = CInt(CStr(Ultimo_Trace(nrohost, terminal.Trim)) & Now.Date.ToString("yyMMdd"))    '--- al nroTrace le agrega la fecha Ej. trace=1070 + today=200605 ==> 1070200605 
            nroTracePEI = terminal.Trim & "-" & CStr(Ultimo_Trace(nrohost, terminal.Trim)) & Now.Date.ToString("yyMMdd")
        Catch ex As Exception

        End Try
        nroTicket = Ultimo_Ticket(nrohost, terminal.Trim)
        'nroLote = Ultimo_Lote(nrohost, terminal.Trim)

        'If emisor = 20 Then BuscarVisaBin(Me) ' entro solo si es visa por las dudas

        'If ida.oper = 1 And (ida.TKTORI = 0 Or ida.FECORI = "") Then
        '    InvalidarReq("Falta tktori o fecori.")
        'End If

        'If ida.oper = 2 Or ida.oper = 3 Then
        '    retrefnumber = BuscarRetRefNumber(Me)
        '    If retrefnumber = "" Then
        '        InvalidarReq("No existe el movimiento en el lote.")
        '    End If
        'End If
        If ida.MANUAL <> E_ModoIngreso.Manual And ida.TRACK1.Trim <> "" Then
            obtenerNombreCliente()
            'ElseIf ida.MANUAL = E_ModoIngreso.Chip And ida.TRACK1.Trim = "" Then
            '    cliente = ida.NOMBRE
        End If

        nombreHost = UCase(CType(nrohost, TipoHost).ToString)
        'verSiEsMaestro()
        'If ida.PLANINT = 800 And Not esVisaDebito Then InvalidarReq("Plan invalido")
        'esCashback = verSiEsCashback()

        'If Not esCashback And ida.CASHBACK > 0 Then InvalidarReq("Monto cashback incorrecto")
        importe = ida.IMPO
        'calcularImporte()
        If emisor <> 6 And Not controlarCodSeg() Then InvalidarReq("Falta cod de seguridad")
        'setearProcCode()
        'If EMV Then
        '    If (ida.ServCode.Substring(0, 1) = "2" Or ida.ServCode.Substring(0, 1) = "6") And ida.MANUAL <> 5 Then
        '        FALLBACK = True
        '    End If
        'End If
        'operacion = CType(ida.oper, E_ProcCode)
    End Sub

    ''' <summary>
    ''' Inicializa Req para venta normal tcp.
    ''' </summary>
    ''' <param name="mensaje"></param>
    ''' <param name="_client"></param>
    Sub New(mensaje As TransmisorTCP.MensajeIda, _client As System.Net.IPEndPoint)

        '--- a este NEW entra desde  Sub ProcesarTCP(ByVal IDTerminal As System.Net.IPEndPoint) Handles Transmisor.DatosRecibidos

        clientStream = _client
        msjIda = mensaje
        tipoRequerimiento = "TCP"
        nroCaja = CInt(mensaje.caja)
        pinguino = mensaje.ping

        If mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.Sincronizacion Then

            If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                '--- ACA ACA ACA ACA ULTIMO - ver nrohost                        
                '--- ESTA FIJO 5, HAY QUE PONER 1 Y CARGAR TODAS LAS TERMINALES  
                '--- EN LA TABLA NUMEROS, PARA CADA CAJA DE CADA PINGUINO        
                nrohost = 5
            Else
                nrohost = 1
            End If

            nombreHost = UCase(CType(nrohost, TipoHost).ToString)
            terminal = BuscarTerminal(pinguino + nroCaja.ToString("00"), nrohost)
            idEncripcion = mensaje.idEncripcion
            operacion = E_ProcCode.Sincronizacion

        ElseIf mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
            nroPlan = CInt(mensaje.plan)
            inicializaReqAnulacionTCP()

        ElseIf mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion Then
            nroPlan = CInt(mensaje.plan)
            inicializaReqDevolucionTCP()

        ElseIf mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.Reverso Then
            nroPlan = CInt(mensaje.plan)
            inicializaReqTCP()

        ElseIf mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.Compra Or mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Or mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.Advice Then
            nroPlan = CInt(mensaje.plan)
            inicializaReqTCP()
        End If

    End Sub




    Property FechaHoraEnvioMsg As Date
    Property FechaHoraEnvioVtaCaja As Date

    ''' <summary>
    ''' Marca requermiento como no valido.
    ''' </summary>
    ''' <param name="texto">Descripción del problema.</param>
    ''' <remarks></remarks>
    Public Sub InvalidarReq(texto As String)
        CausaInvalida = texto
        RespInvalida = True
    End Sub


    ''' <summary>
    ''' Controla que este todo bien formado antes de mandar.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function validar() As Boolean
        If Len(ida.TRACK1) > 77 Then InvalidarReq("Track1 muy largo.")
        If Len(ida.TRACK2) > 37 Then InvalidarReq("Track2 muy largo.")

        'If Not controlarCodSeg() Then srv.Msg("Falta codigo de seguridad")
        'If Not controlVencimiento() Then srv.Msg("Tarjeta Vencida")
        'control4digitos()

        'If Not controlDigitoVerificador() Then InvalidarReq("Error en nro de tarjeta")

        ' comprobar si requiere cod de seguridad que esté
        ' verificar fecha de vencimiento
        ' verficar ultimos 4 digitos
        ' controlar digito verficador
        Return True
    End Function

    Private Sub inicializaReqCierre()
        nroLote = Ultimo_Lote(nrohost, terminal.Trim)
        Incrementar_Trace(terminal.Trim, nrohost)
        nroTrace = Ultimo_Trace(nrohost, terminal.Trim)
    End Sub
    Private Sub inicializaReqDevolucionTCP()
        'validar()
        BuscarEmisorEnRangHab3DES(Me)
        BuscarEmisorTCP(Me)
        pinBlock = ""
        'TODO: ver si es devolucion (oper = 1) si tenenmos que buscar segun ida.PLANINTori
        'nroPlan = CInt(msjIda.plan)
        BuscarPlan3DES(Me)
        terminal = BuscarTerminal(pinguino + nroCaja.ToString("00"), nrohost)

        If terminal = "" Then
            InvalidarReq("No se encontró terminal.")
            Exit Sub
        End If
        If msjIda.ticketOriginal = 0 Then
            InvalidarReq("Falta ticket original.")
        Else


            If emisor = 20 Then BuscarVisaBin3DES(Me) ' entro solo si es visa por las dudas
            retrefnumber = msjIda.referenceNumber


            'If ida.MANUAL <> E_ModoIngreso.Manual And ida.TRACK1.Trim <> "" Then
            '    obtenerNombreCliente()
            'ElseIf ida.MANUAL = E_ModoIngreso.Chip And ida.TRACK1.Trim = "" Then
            '    cliente = ida.NOMBRE
            'End If

            nombreHost = UCase(CType(nrohost, TipoHost).ToString)
            verSiEsMaestro3DES()
            If nroPlan = 800 And Not esVisaDebito Then InvalidarReq("Plan invalido")
            If (nroPlan = 992 Or nroPlan = 991) And Not esMaestro Then InvalidarReq("Plan invalido")
            esCashback = nroPlan = 800 And esVisaDebito Or nroPlan = 992 And esMaestro

            If Not esCashback And msjIda.importeCashback > 0 Then InvalidarReq("Monto cashback incorrecto")

            If Not RespInvalida And msjIda.reversar = False Then Incrementar_Trace(terminal.Trim, nrohost)
            nroTrace = Ultimo_Trace(nrohost, terminal.Trim)
            If msjIda.reversar = False Then
                nroTicket = Ultimo_Ticket(nrohost, terminal.Trim)
            Else
                If BuscarMovimiento0210TCP(nrohost, nroTrace, terminal) Then

                    DisminuirNroTicket(terminal.Trim, nrohost)
                End If
                nroTicket = Ultimo_Ticket(nrohost, terminal.Trim)

            End If
            nroLote = Ultimo_Lote(nrohost, terminal.Trim)
            'calcularImporte3DES()
            importe = msjIda.importeCompra

            If msjIda.TipoCuentaY07 <> "" Then
                msjIda.tipocuenta = msjIda.TipoCuentaY07   '---  ver que es 1 - 2 - 8 y 9, no como viene en tipocuentay07, corregirlo 
                'If msjIda.TipoCuentaY07 = "1" Then msjIda.tipocuenta = "1"
                'If msjIda.TipoCuentaY07 = "2" Then msjIda.tipocuenta = "2"
                'If msjIda.TipoCuentaY07 = "3" Then msjIda.tipocuenta = "8"
                'If msjIda.TipoCuentaY07 = "4" Then msjIda.tipocuenta = "9"
            End If

            setearProcCodeTCP()
            If (msjIda.serviceCode.Substring(0, 1) = "2" Or msjIda.serviceCode.Substring(0, 1) = "6") And msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Chip Then
                FALLBACK = True
            End If
        End If

        'operacion = CType(ida.oper, E_ProcCode)
    End Sub

    Private Sub inicializaReqAnulacionTCP()
        'validar()
        BuscarEmisorEnRangHab3DES(Me)
        BuscarEmisorTCP(Me)
        pinBlock = ""
        'TODO: ver si es devolucion (oper = 1) si tenenmos que buscar segun ida.PLANINTori
        'nroPlan = CInt(msjIda.plan)
        BuscarPlan3DES(Me)
        terminal = BuscarTerminal(pinguino + nroCaja.ToString("00"), nrohost)

        If terminal = "" Then
            InvalidarReq("No se encontró terminal.")
            Exit Sub
        End If
        If msjIda.ticketOriginal = 0 Then
            InvalidarReq("Falta ticket original.")
        Else


            If emisor = 20 Then BuscarVisaBin3DES(Me) ' entro solo si es visa por las dudas
            retrefnumber = msjIda.referenceNumber


            'If ida.MANUAL <> E_ModoIngreso.Manual And ida.TRACK1.Trim <> "" Then
            '    obtenerNombreCliente()
            'ElseIf ida.MANUAL = E_ModoIngreso.Chip And ida.TRACK1.Trim = "" Then
            '    cliente = ida.NOMBRE
            'End If

            nombreHost = UCase(CType(nrohost, TipoHost).ToString)
            verSiEsMaestro3DES()
            If nroPlan = 800 And Not esVisaDebito Then InvalidarReq("Plan invalido")
            If (nroPlan = 992 Or nroPlan = 991) And Not esMaestro Then InvalidarReq("Plan invalido")
            esCashback = nroPlan = 800 And esVisaDebito Or nroPlan = 992 And esMaestro

            If Not esCashback And msjIda.importeCashback > 0 Then InvalidarReq("Monto cashback incorrecto")

            If Not RespInvalida And msjIda.reversar = False Then Incrementar_Trace(terminal.Trim, nrohost)
            nroTrace = Ultimo_Trace(nrohost, terminal.Trim)
            If msjIda.reversar = False Then
                nroTicket = Ultimo_Ticket(nrohost, terminal.Trim)
            Else
                If BuscarMovimiento0210TCP(nrohost, nroTrace, terminal) Then

                    DisminuirNroTicket(terminal.Trim, nrohost)
                End If
                nroTicket = Ultimo_Ticket(nrohost, terminal.Trim)

            End If
            nroLote = Ultimo_Lote(nrohost, terminal.Trim)
            'calcularImporte3DES()
            importe = msjIda.importeCompra

            If msjIda.TipoCuentaY07 <> "" Then
                msjIda.tipocuenta = msjIda.TipoCuentaY07   '---  ver que es 1 - 2 - 8 y 9, no como viene en tipocuentay07, corregirlo 
                'If msjIda.TipoCuentaY07 = "1" Then msjIda.tipocuenta = "1"
                'If msjIda.TipoCuentaY07 = "2" Then msjIda.tipocuenta = "2"
                'If msjIda.TipoCuentaY07 = "3" Then msjIda.tipocuenta = "8"
                'If msjIda.TipoCuentaY07 = "4" Then msjIda.tipocuenta = "9"
            End If

            setearProcCodeTCP()
            If (msjIda.serviceCode.Substring(0, 1) = "2" Or msjIda.serviceCode.Substring(0, 1) = "6") And msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Chip Then
                FALLBACK = True
            End If
        End If

        'operacion = CType(ida.oper, E_ProcCode)
    End Sub

    Private Sub inicializaReqTCP()


        'validar()
        BuscarEmisorEnRangHab3DES(Me)


        '--- DE ACUERDO AL EMISOR, ACA PONE POR QUE HOST VA.....
        '--- VER COMO SABER CUANDO ESTÁ DESCONECTADO UNO DE LOS 2 HOSTS Y PONER EL QUE NO ESTÁ DESCONECTADO COMO FIJO  
        '--- PERO HAY QUE VER BIEN, PORQUE SI SE CAMBIA EL HOST, VA A BUSCAR OTRO NUMERO DE COMERCIO...  VERLO BIEN CON EL GABY  
        BuscarEmisorTCP(Me)



        pinBlock = ""
        'TODO: ver si es devolucion (oper = 1) si tenenmos que buscar segun ida.PLANINTori
        'nroPlan = CInt(msjIda.plan)
        BuscarPlan3DES(Me)

        '--- LA TERMINAL LA BUSCO EN BASE AL NUMERO DE HOSTS....    SI ES VISA, TRAE LA DE FIRSTDATA, PORQUE 
        '--- EL HOSTS ES FIJO SIEMPRE 1                                                                      
        terminal = BuscarTerminal(pinguino + nroCaja.ToString("00"), nrohost)


        'Try
        '    MsgBox("nrohost    " & nrohost.ToString)
        '    MsgBox("terminal    " & terminal)
        '    MsgBox("idcomercio    " & idComercio)
        'Catch
        'End Try



        If terminal = "" Then InvalidarReq("No se encontró terminal.")
        If msjIda.reversar = False Then
            Incrementar_Trace(terminal.Trim, nrohost)
        End If
        nroTrace = Ultimo_Trace(nrohost, terminal.Trim)

        If msjIda.reversar = False Then
            nroTicket = Ultimo_Ticket(nrohost, terminal.Trim)
        Else
            If BuscarMovimiento0210TCP(nrohost, nroTrace, terminal) Then
                'DisminuirNroTicket(terminal.Trim, nrohost)

                '--- EN EL REVERSO DISMINUYE EL TICKET, PERO COMO EL ADVICE SE MANDA COMO REVERSO, NO HAY QUE 
                '--- DISMINUIR, PORQUE YA SE DISMINUYO CON EL REVERSO ANTES Y SE DISMINUIRIA DE NUEVO         
                If msjIda.CodAutorizaAdv IsNot Nothing Then
                    If msjIda.CodAutorizaAdv.Trim = "Z3" Or msjIda.CodAutorizaAdv.Trim = "Y3" Or
                       msjIda.CodAutorizaAdv.Trim = "Z1" Or msjIda.CodAutorizaAdv.Trim = "Y1" Then

                    Else
                        DisminuirNroTicket(terminal.Trim, nrohost)
                    End If
                Else
                    DisminuirNroTicket(terminal.Trim, nrohost)
                End If

            End If
            nroTicket = Ultimo_Ticket(nrohost, terminal.Trim)
        End If


        If msjIda.CodAutorizaAdv IsNot Nothing Then
            If msjIda.CodAutorizaAdv.Trim = "Z3" Or msjIda.CodAutorizaAdv.Trim = "Y3" Or
               msjIda.CodAutorizaAdv.Trim = "Z1" Or msjIda.CodAutorizaAdv.Trim = "Y1" Then
                'Incrementar_TraceAdvice(terminal.Trim, nrohost)
                'nroTraceAdv = Ultimo_TraceAdvice(nrohost, terminal.Trim)
                '--- CUANDO ES Y1, ES EL UNICO CASO QUE VIENE EL ADVICE SIN EL REVERSO PREVIO, ENTONCES TENGO QUE RESTAR UNO  
                '--- PORQUE COMO SE HIZO LA COMPRA, ANTES QUE EL ADVICE, INCREMENTA EN 1 EL NROTICKET, ENTONCES TENGO QUE     
                '--- RESTAR 1 AL NROTICKET CUANDO ES ADVICE, EN EL RESTO DE LOS ADVICE COMO EN EL REVERSO YA RESTO 1, NO      
                '--- TENGO QUE HACER NADA, QUEDA IGUAL                                                                        
                If msjIda.CodAutorizaAdv.Trim = "Z3" Or msjIda.CodAutorizaAdv.Trim = "Y3" Then
                    Incrementar_Trace(terminal.Trim, nrohost)
                    nroTrace = Ultimo_Trace(nrohost, terminal.Trim)
                    nroTicket = Ultimo_Ticket(nrohost, terminal.Trim)
                Else
                    nroTicket = Ultimo_Ticket(nrohost, terminal.Trim)
                    'If msjIda.CodAutorizaAdv.Trim = "Y1" Then
                    '    IncrementarNroTicket(terminal, nrohost)
                    '    nroTicket = Ultimo_Ticket(nrohost, terminal.Trim)
                    'End If
                End If
            End If
        End If



        nroLote = Ultimo_Lote(nrohost, terminal.Trim)

        If emisor = 20 Then BuscarVisaBin3DES(Me) ' entro solo si es visa por las dudas  (BUSCO SI DATO DISCRECIONAL = 2 ES VISA DEBITO)

        'If ida.oper = 1 And (ida.TKTORI = 0 Or ida.FECORI = "") Then
        '    InvalidarReq("Falta tktori o fecori.")
        'End If

        'If msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
        '    retrefnumber = BuscarRetRefNumberTCP(Me)
        '    If retrefnumber = "" Then
        '        InvalidarReq("No existe el movimiento en el lote.")
        '    End If
        'End If
        'If ida.MANUAL <> E_ModoIngreso.Manual And ida.TRACK1.Trim <> "" Then
        '    obtenerNombreCliente()
        'ElseIf ida.MANUAL = E_ModoIngreso.Chip And ida.TRACK1.Trim = "" Then
        '    cliente = ida.NOMBRE
        'End If

        nombreHost = UCase(CType(nrohost, TipoHost).ToString)


        verSiEsMaestro3DES()
        If nroPlan = 800 And Not esVisaDebito Then InvalidarReq("Plan invalido")
        If (nroPlan = 992 Or nroPlan = 991) And Not esMaestro Then InvalidarReq("Plan invalido")


        '--- pone esCashback=true si cumple todo lo que esta despues del primer =
        'esCashback = nroPlan = 800 And esVisaDebito Or nroPlan = 992 And esMaestro Or nroPlan = 992 And emisor = 44
        If (nroPlan = 800 And esVisaDebito) Or
           (nroPlan = 992 And esMaestro) Or
           (nroPlan = 992 And emisor = 44) Then
            esCashback = True
        End If
        If (nroPlan = 101 And emisor = 4) And msjIda.importeCashback > 0 Then     '--- CABAL 24 
            esCashback = True
        End If
        If Not esCashback And msjIda.importeCashback > 0 Then InvalidarReq("Monto cashback incorrecto")

        calcularImporte3DES()

        setearProcCodeTCP()   '--- CODIGO DE PROCESAMIENTO (VER MANUAL PAG 14 DE 19)
        If (msjIda.serviceCode.Substring(0, 1) = "2" Or msjIda.serviceCode.Substring(0, 1) = "6") And msjIda.tipoIngreso <> TransmisorTCP.TipoIngreso.Chip Then
            FALLBACK = True
        End If
        'operacion = CType(ida.oper, E_ProcCode)
    End Sub

    Private Sub inicializaReq()
        validar()
        BuscarEmisorEnRangHab(Me)
        BuscarEmisor(Me)
        'TODO: ver si es devolucion (oper = 1) si tenenmos que buscar segun ida.PLANINTori
        nroPlan = ida.PLANINT
        BuscarPlan(Me)
        terminal = BuscarTerminal(pinguino + nroCaja.ToString("00"), nrohost)
        If terminal = "" Then InvalidarReq("No se encontró terminal.")

        Incrementar_Trace(terminal.Trim, nrohost)
        nroTrace = Ultimo_Trace(nrohost, terminal.Trim)

        nroTicket = Ultimo_Ticket(nrohost, terminal.Trim)
        nroLote = Ultimo_Lote(nrohost, terminal.Trim)

        If emisor = 20 Then BuscarVisaBin(Me) ' entro solo si es visa por las dudas

        If ida.oper = 1 And (ida.TKTORI = 0 Or ida.FECORI = "") Then
            InvalidarReq("Falta tktori o fecori.")
        End If

        If ida.oper = 2 Or ida.oper = 3 Then
            retrefnumber = BuscarRetRefNumber(Me)
            If retrefnumber = "" Then
                InvalidarReq("No existe el movimiento en el lote.")
            End If
        End If
        If ida.MANUAL <> E_ModoIngreso.Manual And ida.TRACK1.Trim <> "" Then
            obtenerNombreCliente()
        'ElseIf ida.MANUAL = E_ModoIngreso.Chip And ida.TRACK1.Trim = "" Then
        '    cliente = ida.NOMBRE
        End If

        nombreHost = UCase(CType(nrohost, TipoHost).ToString)
        verSiEsMaestro()
        If ida.PLANINT = 800 And Not esVisaDebito Then InvalidarReq("Plan invalido")
        esCashback = verSiEsCashback()

        If Not esCashback And ida.CASHBACK > 0 Then InvalidarReq("Monto cashback incorrecto")
        calcularImporte()
        If Not controlarCodSeg() Then InvalidarReq("Falta cod de seguridad")
        setearProcCode()
        'If EMV Then
        '    If (ida.ServCode.Substring(0, 1) = "2" Or ida.ServCode.Substring(0, 1) = "6") And ida.MANUAL <> 5 Then
        '        FALLBACK = True
        '    End If
        'End If
        'operacion = CType(ida.oper, E_ProcCode)
    End Sub


    Private Sub tratamientoMaestro()
        Try
            tipocuenta = CInt(Mid(ida.CODSEG, 17, 1))
            pinBlock = Mid(ida.CODSEG, 1, 17)
        Catch ex As Exception
            If Len(ida.CODSEG) < 18 Then InvalidarReq("Error en cod. de seguridad.")

        End Try
    End Sub

    Private Function verSiEsCashback() As Boolean
        Return ida.PLANINT = 800 And esVisaDebito
    End Function

    Private Sub tratamientoMaestro3DES()
        Try
            If msjIda.tipocuenta <> "N" Then
                tipocuenta = CInt(msjIda.tipocuenta)
                pinBlock = msjIda.pinBlock
            End If
        Catch ex As Exception
            If Len(msjIda.pinBlock) < 17 Then InvalidarReq("Error en tratamientomaestro3des.")

        End Try
    End Sub
    Private Sub verSiEsMaestro3DES()
        esMaestro = CDec(msjIda.plan) >= 990 And CDec(msjIda.plan) <= 992





        '--- Para contactless Y07 
        '--- A LO MEJOR LO QUE ESTA MAS ABAJO HAY QUE PONERLO ACA....   SI NO ANDA MOVERLO  
        If msjIda.TipoCuentaY07 <> "" Then
            msjIda.tipocuenta = msjIda.TipoCuentaY07
            esMaestro = True
            emisor = 6
        End If




        If esMaestro Then
            tratamientoMaestro3DES()
        Else
            'If emisor = 44 And msjIda.pinBlock <> "" Then 'MC DEBIT
            '    pinBlock = msjIda.pinBlock
            'End If
            'If emisor = 20 And msjIda.pinBlock <> "" Then     '--- VISA DEBITO CON PIN  
            '    pinBlock = msjIda.pinBlock
            'End If
            If msjIda.pinBlock IsNot Nothing Then
                If msjIda.pinBlock <> "" Then
                    pinBlock = msjIda.pinBlock
                End If
            End If
        End If
    End Sub
    Private Sub verSiEsMaestro()
        esMaestro = ida.PLANINT >= 990 And ida.PLANINT <= 992
        If esMaestro Then tratamientoMaestro()

    End Sub

    Private Sub obtenerNombreCliente()
        Dim campos_track1() As String = Split(ida.TRACK1, "^")
        cliente = campos_track1(1)
    End Sub

    Private Function controlarCodSeg() As Boolean
        'Visa debito no requiere cod seguridad.
        If obligarCodSeg Then
            Return ida.CODSEG <> ""
        Else
            Return True
        End If
    End Function

    Private Function controlVencimiento() As Boolean
        Return CDate("01/" + Mid(ida.EXPDATE, 3, 2) + "/" + Mid(ida.EXPDATE, 1, 2)) > Now.Date
    End Function

    Private Sub control4digitos()
        Throw New NotImplementedException
    End Sub

    Private Function controlDigitoVerificador() As Boolean
        Dim pp As String = Trim(ida.TARJ)
        Dim tarjeta As String = Mid(pp, 1, pp.Length - 1)

        Dim digito As String = Right(pp, 1)
        Dim suma, c, multi, diez As Integer

        For c = tarjeta.Length To 1 Step -1
            If c Mod 2 = 1 Then
                multi = CInt(CDbl(Mid(tarjeta, c, 1)) * 2)
            Else
                multi = CInt(Mid(tarjeta, c, 1))
            End If
            If multi > 9 Then
                suma = suma + CInt(Mid(CStr(multi), 1, 1)) + CInt(Mid(CStr(multi), 2, 1))
            Else
                suma = suma + multi
            End If
        Next
        diez = CInt((Fix(suma / 10) + 1) * 10)
        Return diez - suma = CDbl(digito)
    End Function

    ''' <summary>
    ''' Controla que el coef. este bien, se lo incrementa.
    ''' si tiene cashback.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub calcularImporte3DES()
        If coeficiente <= 1 Then coeficiente = 1
        If coeficiente >= 2 Then coeficiente = 1
        'importe = CDec(CInt(ida.IMPO * coeficiente * 100 + 0.5) / 100)
        importe = Math.Round(msjIda.importeCompra * coeficiente, 2)

    End Sub


    ''' <summary>
    ''' Controla que el coef. este bien, se lo incrementa.
    ''' si tiene cashback.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub calcularImporte()
        If coeficiente <= 1 Then coeficiente = 1
        If coeficiente >= 2 Then coeficiente = 1
        'importe = CDec(CInt(ida.IMPO * coeficiente * 100 + 0.5) / 100)
        importe = Math.Round(ida.IMPO * coeficiente, 2)

    End Sub


    Private Sub setearProcCodeTCP()
        Select Case msjIda.tipoMensaje
            Case TransmisorTCP.TipoMensaje.Compra, TransmisorTCP.TipoMensaje.CompraCashback 'compra
                If esCashback Then
                    operacion = E_ProcCode.compra_cashback     '--- SI ES CASHBACK LE SUMA 90000 A LA OPERACION
                    If esMaestro Then
                        Select Case msjIda.tipocuenta
                            Case "1"
                                operacion = CType(CInt(operacion) + CInt(E_ProcCode.Compra_maestro_CajAhorroP), E_ProcCode)
                            Case "2"
                                operacion = CType(CInt(operacion) + CInt(E_ProcCode.Compra_maestro_CtaCteP), E_ProcCode)
                            Case "8"
                                operacion = CType(CInt(operacion) + CInt(E_ProcCode.Compra_maestro_CajAhorroD), E_ProcCode)
                            Case "9"
                                operacion = CType(CInt(operacion) + CInt(E_ProcCode.Compra_maestro_CtaCteD), E_ProcCode)
                        End Select
                    End If
                Else
                    If esMaestro Then
                        Select Case msjIda.tipocuenta
                            Case "1"
                                operacion = E_ProcCode.Compra_maestro_CajAhorroP
                            Case "2"
                                operacion = E_ProcCode.Compra_maestro_CtaCteP
                            Case "8"
                                operacion = E_ProcCode.Compra_maestro_CajAhorroD
                            Case "9"
                                operacion = E_ProcCode.Compra_maestro_CtaCteD
                        End Select
                    Else
                        operacion = E_ProcCode.Compra
                    End If
                End If


            Case TransmisorTCP.TipoMensaje.Devolucion  'devolucion
                If esMaestro Then
                    Select Case msjIda.tipocuenta
                        Case "1"
                            operacion = E_ProcCode.Devolucion_Compra_maestro_CajAhorroP
                        Case "2"
                            operacion = E_ProcCode.Devolucion_Compra_maestro_CtaCteP
                        Case "8"
                            operacion = E_ProcCode.Devolucion_Compra_maestro_CajAhorroD
                        Case "9"
                            operacion = E_ProcCode.Devolucion_Compra_maestro_CtaCteD
                    End Select

                Else
                    operacion = E_ProcCode.Devolucion
                End If


            Case TransmisorTCP.TipoMensaje.Anulacion  'anulacion
                If esCashback Then
                    operacion = E_ProcCode.anulacion_cash_masterdebit
                    Select Case msjIda.tipocuenta
                        Case "1"
                            operacion = CType(CInt(operacion) + CInt(E_ProcCode.Compra_maestro_CajAhorroP), E_ProcCode)
                        Case "2"
                            operacion = CType(CInt(operacion) + CInt(E_ProcCode.Compra_maestro_CtaCteP), E_ProcCode)
                        Case "8"
                            operacion = CType(CInt(operacion) + CInt(E_ProcCode.Compra_maestro_CajAhorroD), E_ProcCode)
                        Case "9"
                            operacion = CType(CInt(operacion) + CInt(E_ProcCode.Compra_maestro_CtaCteD), E_ProcCode)
                    End Select
                    'If emisor = 44 Then
                    '    operacion = E_ProcCode.anulacion_cash_masterdebit
                    'Else
                    '    operacion = E_ProcCode.anulacion_compra_cashback
                    'End If

                Else
                    If esMaestro Then
                        Select Case msjIda.tipocuenta
                            Case "1"
                                operacion = E_ProcCode.Anulacion_Compra_maestro_CajAhorroP
                            Case "2"
                                operacion = E_ProcCode.Anulacion_Compra_maestro_CtaCteP
                            Case "8"
                                operacion = E_ProcCode.Anulacion_Compra_maestro_CtaCteP
                            Case "9"
                                operacion = E_ProcCode.Anulacion_Compra_maestro_CtaCteD
                        End Select
                    Else
                        operacion = E_ProcCode.AnulacionCompra
                    End If

                End If
            Case TransmisorTCP.TipoMensaje.AnulacionDevolucion  'anulacion devolucion
                If esMaestro Then
                    Select Case msjIda.tipocuenta
                        Case "1"
                            operacion = E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroP
                        Case "2"
                            operacion = E_ProcCode.AnulacionDevolucion_Compra_maestro_CtaCteP
                        Case "8"
                            operacion = E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroD
                        Case "9"
                            operacion = E_ProcCode.AnulacionDevolucion_Compra_maestro_CtaCteD
                    End Select
                Else
                    operacion = E_ProcCode.AnulacionDevolucion
                End If
            Case TransmisorTCP.TipoMensaje.Advice            'Advice
                If esMaestro Then
                    Select Case msjIda.tipocuenta
                        Case "1"
                            operacion = E_ProcCode.Advice_maestro_CajAhorroP
                        Case "2"
                            operacion = E_ProcCode.Advice_maestro_CtaCteP
                        Case "8"
                            operacion = E_ProcCode.Advice_maestro_CajAhorroD
                        Case "9"
                            operacion = E_ProcCode.Advice_maestro_CtaCteD
                    End Select
                Else
                    operacion = E_ProcCode.Advice
                End If



            Case Else
                InvalidarReq("Operacion no permitida")
        End Select

    End Sub


    Private Sub setearProcCode()
        Select Case ida.oper
            Case 0 'compra
                If esCashback Then
                    operacion = E_ProcCode.compra_cashback
                Else
                    If esMaestro Then
                        Select Case tipocuenta
                            Case 1
                                operacion = E_ProcCode.Compra_maestro_CajAhorroP
                            Case 2
                                operacion = E_ProcCode.Compra_maestro_CtaCteP
                            Case 8
                                operacion = E_ProcCode.Compra_maestro_CajAhorroD
                            Case 9
                                operacion = E_ProcCode.Compra_maestro_CtaCteD
                        End Select
                    Else
                        operacion = E_ProcCode.Compra
                    End If
                End If


            Case 1 'devolucion
                If esMaestro Then
                    Select Case tipocuenta
                        Case 1
                            operacion = E_ProcCode.Devolucion_Compra_maestro_CajAhorroP
                        Case 2
                            operacion = E_ProcCode.Devolucion_Compra_maestro_CtaCteP
                        Case 8
                            operacion = E_ProcCode.Devolucion_Compra_maestro_CajAhorroD
                        Case 9
                            operacion = E_ProcCode.Devolucion_Compra_maestro_CtaCteD
                    End Select

                Else
                    operacion = E_ProcCode.Devolucion
                End If


            Case 2 'anulacion
                If esCashback Then
                    operacion = E_ProcCode.anulacion_compra_cashback
                Else
                    If esMaestro Then
                        Select Case tipocuenta
                            Case 1
                                operacion = E_ProcCode.Anulacion_Compra_maestro_CajAhorroP
                            Case 2
                                operacion = E_ProcCode.Anulacion_Compra_maestro_CtaCteP
                            Case 8
                                operacion = E_ProcCode.Anulacion_Compra_maestro_CtaCteP
                            Case 9
                                operacion = E_ProcCode.Anulacion_Compra_maestro_CtaCteD
                        End Select
                    Else
                        operacion = E_ProcCode.AnulacionCompra
                    End If

                End If
            Case 3 'anulacion devolucion
                If esMaestro Then
                    Select Case tipocuenta
                        Case 1
                            operacion = E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroP
                        Case 2
                            operacion = E_ProcCode.AnulacionDevolucion_Compra_maestro_CtaCteP
                        Case 8
                            operacion = E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroD
                        Case 9
                            operacion = E_ProcCode.AnulacionDevolucion_Compra_maestro_CtaCteD
                    End Select
                Else
                    operacion = E_ProcCode.AnulacionDevolucion
                End If
            Case Else
                InvalidarReq("Operacion no permitida")
        End Select

    End Sub

End Class
