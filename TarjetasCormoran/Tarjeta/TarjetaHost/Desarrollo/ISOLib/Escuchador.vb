Imports Trx.Messaging.FlowControl
Imports Trx.Messaging.Channels
Imports Trx.Messaging.Iso8583
Imports Trx.Messaging

Imports log4net
Imports System.Reflection


#Region "Enums"
Public Enum E_ProcCode
    ' posnet
    Compra = 0
    Compra_maestro_CajAhorroP = 1000
    Compra_maestro_CtaCteP = 2000
    Compra_maestro_CajAhorroD = 8000
    Compra_maestro_CtaCteD = 9000

    compra_cashback = 90000
    anulacion_compra_cashback = 140000
    anulacion_cash_masterdebit = 210000


    Devolucion = 200000
    Devolucion_Compra_maestro_CajAhorroP = 201000
    Devolucion_Compra_maestro_CtaCteP = 202000
    Devolucion_Compra_maestro_CajAhorroD = 208000
    Devolucion_Compra_maestro_CtaCteD = 209000

    AnulacionCompra = 20000
    Anulacion_Compra_maestro_CajAhorroP = 21000
    Anulacion_Compra_maestro_CtaCteP = 22000
    Anulacion_Compra_maestro_CajAhorroD = 28000
    Anulacion_Compra_maestro_CtaCteD = 29000

    AnulacionDevolucion = 220000
    AnulacionDevolucion_Compra_maestro_CajAhorroP = 221000
    AnulacionDevolucion_Compra_maestro_CtaCteP = 222000
    AnulacionDevolucion_Compra_maestro_CajAhorroD = 228000
    AnulacionDevolucion_Compra_maestro_CtaCteD = 229000

    Echotest = 990000

    Cierre_Lote = 920000

    Sincronizacion = 920000


    Advice = 0
    Advice_maestro_CajAhorroP = 1000
    Advice_maestro_CtaCteP = 2000
    Advice_maestro_CajAhorroD = 8000
    Advice_maestro_CtaCteD = 9000


    'Batch_Upload_ultimo = 0

    'reverso le va el codigo original



End Enum
Public Enum E_Monedas
    PESOS = 32
    REALES = 76
    DOLARES = 840
    PESOS_URUGUAYOS = 858
End Enum
Public Enum e_ModosTransaccion
    Normal
    Reverso
    SAF
End Enum
Public Enum E_TiposMensajes
    Control = 800
    TransaccionTR = 200
    ReversoTransaccionTR = 400 ' 420
    ReenvioReversoTR = 421
    TransaccionSAF = 220
    ReenvioTransaccionSAF = 221
    MensajeRechazado = 900
    MensajeCierre = 500

    Respuesta_Control = 810
    Respuesta_TransaccionTR = 210
    Respuesta_ReversoTransaccionTR = 410
    Respuesta_ReenvioReversoTR = 431
    Respuesta_TransaccionSAF = 230
    Respuesta_ReenvioTransaccionSAF = 231
    Respuesta_Cierre = 510



End Enum
Public Enum E_Fields
    Field7TransDateTime = 7
    Field11Trace = 11
End Enum
Public Enum E_Implementaciones
    PosnetComercio = 1
    Visa = 2
    PosnetHomol = 5
    VisaHomol = 6

End Enum
Public Enum E_CodigosRespuesta ' luego se pasan a string
    APROBADA = 0
    PEDIR_AUTORIZACION = 1
    PEDIR_AUTORIZACION2 = 2
    COMERCIO_INVALIDO = 3
    CAPTURAR_TARJETA = 4
    DENEGADA = 5
    RETENGA_Y_LLAME = 7
    APROBADA11 = 11
    TRANSACCION_INVALIDA = 12
    MONTO_INVALIDO = 13
    TARJETA_INVALIDA = 14
    NO_EXISTE_ORIGINAL = 25
    SERV_NO_DISPONIBLE = 28
    ERROR_DE_FORMATO = 30
    EXCEDE_ING_PIN = 38
    RETENER_TARJETA = 43
    NO_OPERA_CUOTAS = 45
    TARJ_NO_VIGENTE = 46
    PIN_REQUERIDO = 47
    EXCEDE_MAX_CUOTAS = 48
    ERROR_FECHA_VTO = 49
    FONDOS_INSUFICIENTES = 51
    CUENTA_INEXISTENTE = 53
    TARJETA_VENCIDA = 54
    PIN_INCORRECTO = 55
    TARJ_NO_HABILITADA = 56
    TRANS_NO_PERMITIDA = 57
    SERV_INVALIDO = 58
    EXCEDE_LIMITE_DIARIO = 61
    EXCEDE_LIM_TARJ = 65
    LLAMAR_AL_EMISOR = 76
    ERROR_PLAN_CUOTAS = 77
    APROBADA85 = 85
    TERMINAL_INVALIDA = 89
    EMISOR_FUERA_LINEA = 91
    NRO_TRADCE_DUPLICADO = 94
    RE_TRANSMITIENDO = 95
    ERROR_SISTEMA = 96
    VER_RECHAZO_EN_TICK = 98
    Nro_de_cuotas_invalidas = 17

    Tarjeta_restringida = 62
    Intentos_de_pin_excedidos = 75
    Error_del_sistema = 88
    Fondos_insuficientes_mayor0 = 59
End Enum
Public Enum e_transaccionesNexo
    AdelantoEfectivo
    PagoAutomaticoServicios
End Enum
Public Enum E_CodigoREspuestaReversos
    ' Para los mensajes 0420 indica motivo de reverso.
    TimeOut_del_autorizador = 1
    Comando_rechazado = 2
    Destino_no_disponible = 3
    Cancelación_del_usuario = 8
    Error_de_maquina = 10
    Transaccion_sospechosa = 20
    ReversoParcial = 32
    TimeOut_del_HOST = 68
    Extendido = 100
End Enum
Public Enum E_eventosControl
    Logon = 1
    Logoff = 2
    VentaPOS = 200
    EchoTest = 301
    EchotestInterno = 302
End Enum
#End Region

Public Class Escuchador
    Implements IDisposable, IMessageProcessor
    Public Const Vtomax As Date = #12/31/2029#

    Public HostOfflIne As Boolean
    Public Last0800 As Date
    Public LastNoEco As Date = Date.MaxValue
    Dim _logstatus As Boolean
    Dim WithEvents _Escuchador As TcpListener
    Dim Numerador As Trx.Messaging.VolatileStanSequencer
    'Dim WithEvents respondedor As ListenerPeer
    Dim WithEvents _Server As Server
    Dim NombreInterface As String
    Dim InterfaceIp As String
    Dim Puerto As Integer
    Dim Implementacion As E_Implementaciones
    Public Const CantMaximaConexionesEntrantes As Integer = 4
    Dim _nextMessageProcessor As IMessageProcessor
    Public WithEvents TimerEco As System.Timers.Timer
    Public Noeco As Integer
    Private _auditar As Boolean
    ' necesario para recibir los mensajes desde el obhjeto server
#Region "Implementacion de ImessageProcessor"



    Public Property NextMessageProcessor() As IMessageProcessor Implements IMessageProcessor.NextMessageProcessor
        Get
            Return _nextMessageProcessor
        End Get
        Set(ByVal value As IMessageProcessor)
            _nextMessageProcessor = value
        End Set
    End Property

    Public Function Process(ByVal source As IMessageSource, ByVal message As Message) As Boolean Implements IMessageProcessor.Process
        ProcesarMensaje(message, source)

    End Function
#End Region

#Region "Eventos Transacciones "

    ' esto no depende de la implementacion

    Public Event Cierre(ByRef e As ParametrosCom)
    Public Event TransaccionPOSConsulta(ByRef parametros As ParametrosCom)


    Public Event TransaccionPOSNoImplementada(ByRef parametros As ParametrosCom)
    Public Event TransaccionRespuesta(ByRef msg As Iso8583messagePOS)

    Public Event TransaccionPOSCredito(ByRef parametros As ParametrosCom, ByVal CodigoTransaccion As E_ProcCode)
    Public Event TransaccionPOSCreditoAnulacion(ByRef parametros As ParametrosCom)
    Public Event TransaccionPOSCreditoReverso(ByRef parametros As ParametrosCom)
    Public Event TransaccionPOSCreditoReversoAnulacion(ByRef parametros As ParametrosCom)

    'ByVal pEvento As E_eventosControl, 
    Public Event Control(ByRef pRespuesta As e_eventosRespuesta)
    Public Event LogOn()
    Public Event Logoff()
    Public Event BeforeSend(ByRef msg As Iso8583messagePOS)
    Public Event HostDown(ByRef UltimaHoraEco As Date)
    Public Event HostUp()


#End Region

    Public Overrides Function ToString() As String
        Return Implementacion.ToString
    End Function

    Public Function Conexiones() As Trx.Messaging.FlowControl.ServerPeerCollection
        Return _Server.Peers
    End Function

    Public Sub New(ByVal pImplementacion As E_Implementaciones, ByVal DireccionIP As String, ByVal NroPuerto As Int32, Optional ByVal Auditar As Boolean = False)
        LogManager.GetLogger("root")
        Implementacion = pImplementacion
        ' leer desde Settings
        Puerto = NroPuerto
        InterfaceIp = DireccionIP

        NombreInterface = Implementacion.ToString
        Logger.Info(String.Format("Arrancando escuchador {0} en {1} Puerto:{2}", NombreInterface, InterfaceIp, Puerto))
        Logger.Info("Implementacion " + ToString())

        '' levanto el numerador de TRACE
        Logger.Info("Iniciando Contadores")
        Numerador = New VolatileStanSequencer

        ' levantar comunicaciones
        _Escuchador = New TcpListener(Puerto) With {.LocalInterface = InterfaceIp}

        _Server = New Server("Server " + pImplementacion.ToString, _Escuchador, New BasicServerPeerManager())

        _Server.Listener.ChannelPool = New BasicChannelPool(ObtenerTcpChannel(Implementacion), CantMaximaConexionesEntrantes)
        _Server.PeerManager.MessageProcessor = Me

        _Server.Listener.Start()
        Last0800 = Date.MinValue
        HabilitarTimer()
        _auditar = Auditar
    End Sub

    Public Sub HabilitarTimer()
        If TimerEco Is Nothing Then TimerEco = New System.Timers.Timer(10000)
        TimerEco.Enabled = True
    End Sub

    Public Sub ProcesarMensaje(ByVal m As Message, ByVal messageSource As IMessageSource)
        Dim Mr As Iso8583Message = CType(m, Iso8583Message)
        Dim Hora As DateTime = Now
        Dim mensajeRecibido As New Iso8583messagePOS(Mr, Implementacion, messageSource)
        If Not mensajeRecibido Is Nothing Then
            ' ----------------- Respuesta de acuerdo al tipo de mensaje 
            Logger.Info("Mensaje recibido:" + mensajeRecibido.ToString)
            Select Case mensajeRecibido.MessageTypeIdentifier
                Case E_TiposMensajes.TransaccionTR
                    ' transacciones Online
                    AuditarMensaje(mensajeRecibido)
                    ProcesarTransaccion(mensajeRecibido, e_ModosTransaccion.Normal)
                Case E_TiposMensajes.Control
                    ' login/logout/echo

                    AuditarMensaje(mensajeRecibido)
                    ProcesarControl(mensajeRecibido)
                Case E_TiposMensajes.TransaccionSAF, E_TiposMensajes.ReenvioTransaccionSAF
                    ' Store and Forward
                    AuditarMensaje(mensajeRecibido)
                    ProcesarTransaccion(mensajeRecibido, e_ModosTransaccion.SAF)
                Case E_TiposMensajes.ReenvioReversoTR, E_TiposMensajes.ReversoTransaccionTR, E_TiposMensajes.Respuesta_ReversoTransaccionTR, E_TiposMensajes.Respuesta_ReenvioReversoTR
                    'reversos
                    AuditarMensaje(mensajeRecibido)
                    ProcesarTransaccion(mensajeRecibido, e_ModosTransaccion.Reverso)
                Case E_TiposMensajes.Respuesta_Control, _
                  E_TiposMensajes.Respuesta_ReenvioReversoTR, _
                  E_TiposMensajes.Respuesta_ReenvioTransaccionSAF, _
                  E_TiposMensajes.Respuesta_ReenvioTransaccionSAF, _
                  E_TiposMensajes.Respuesta_ReversoTransaccionTR, _
                  E_TiposMensajes.Respuesta_TransaccionSAF, _
                  E_TiposMensajes.Respuesta_TransaccionTR

                    AuditarMensaje(mensajeRecibido)
                    ProcesarRespuesta(mensajeRecibido)
                Case E_TiposMensajes.MensajeCierre
                    AuditarMensaje(mensajeRecibido)
                    ProcesarCierre(mensajeRecibido)
                Case Else
                    AuditarMensaje(mensajeRecibido)
                    Responder_MensajeNoImplementado(mensajeRecibido, messageSource)
                    ' tipo de mensaje no previsto
            End Select
        Else
            Logger.Warn("Mensaje no valido recibido_???")
        End If
        ' avisarnos si demora mucho, para ver que pasa
        Dim Demora As TimeSpan = Now.Subtract(Hora)
        If Demora.TotalSeconds > DemoraMaxima Then
            Logger.Warn(String.Format("Respuesta demoró {0} (demasiado)", Demora))

        End If
    End Sub
#Region "auditoria"
    Public Sub AuditarMensaje(ByVal m As Iso8583messagePOS)
        ' agregar a la cola
        If _auditar Then
            For i As Integer = 1 To 127
                If m.Fields.Contains(i) Then

                End If
            Next
        End If
    End Sub
    Private Sub ConectarAuditoria()

    End Sub
    Private Sub DesconectarAuditoria()

    End Sub
    Public Property Auditar() As Boolean
        Get
            Return _auditar
        End Get
        Set(ByVal value As Boolean)
            If _auditar <> value Then
                If value Then
                    ConectarAuditoria()
                Else
                    DesconectarAuditoria()
                End If

            End If
            _auditar = value
        End Set
    End Property

#End Region
#Region "Reversos"
    'Public Sub ProcesarReversos(ByVal mensaje As Iso8583messagePOS)
    '    Try
    '        Logger.Info("Procesando REVERSO")
    '        Dim CodigoTransaccion As String = mensaje.Fields(3).ToString
    '        '        ' monto de la transaccion
    '        Dim MontoTransaccion As Decimal = Field4_ObtenerMonto(mensaje)
    '        '        ' fecha de negocio
    '        Dim FechaNegocio As Date = Field17_ObtenerFechaNegocio(mensaje)
    '        Dim FechaTransaccion As Date = Field12y13_ObtenerFechaHora(mensaje)
    '        '        ' tarjeta
    '        Dim Tarjeta As Int64 = Field2or35_ObtenerTarjeta(mensaje)

    '        Dim Moneda As E_Monedas = Field49_ObtenerMoneda(mensaje)
    '        If Moneda <> E_Monedas.PESOS Then
    '            Logger.Warn("Moneda invalida" + Moneda.ToString)
    '            Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
    '            Exit Try
    '        End If

    '        Dim MontoReverso As Decimal = Field95_ObtenerMonto(mensaje)
    '        Logger.Info("Procesando 042x-" + CodigoTransaccion)

    '        '        Select Case CodigoTransaccion
    '        '            Case "103100"
    '        '                Logger.Info("Adelanto en Efectivo")
    '        '                'analizar el campo 126 con la info de adel.en efectivo

    '        '                Try

    '        '                    If mensaje.Fields.Contains(126) Then
    '        '                        ' antes de tirarme de cabeza, verificar el largo
    '        '                        Dim m126 As New Mensaje126Adelanto(Implementacion, mensaje.Fields(126).ToString)
    '        '                        Dim _CodigoRespuesta As E_CodigosRespuesta
    '        '                        '------ procesar adelanto en efectivo
    '        '                        Logger.Info("Llamando a Capa NEXO")
    '        '                        _CodigoRespuesta = TransaccionPOS(e_transaccionesNexo.AdelantoEfectivo, Tarjeta, MontoTransaccion, _
    '        '                                FechaNegocio, FechaTransaccion, m126)
    '        '                        Logger.Info("Retorno de Capa NEXO")
    '        '                        ' Comenzar a responder
    '        '                        mensaje.Fields.Add(126, m126.Tostring)
    '        '                        Responder_02xx(mensaje, _CodigoRespuesta)
    '        '                    Else
    '        '                        Logger.Error("Mensaje 0200 con adelanto SIN campo 126")
    '        '                        Responder_02xx(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
    '        '                    End If
    '        '                Catch ex As Exception
    '        '                    Logger.Error("Error en el parseo del campo 126 en adelantos " + ex.Message + " " + ex.InnerException.Message)
    '        '                    Responder_02xx(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
    '        '                End Try

    '        '                ' grabarAdelanto
    '        '            Case Else
    '        '                Logger.Warn("Transaccion No Soportada " + CodigoTransaccion)
    '        '                Responder_02xx(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
    '        '        End Select
    '    Catch ex As Exception
    '        Logger.Error("Error Interpretando Mensaje 042x" + ex.Message + " " + ex.InnerException.Message)
    '    End Try
    'End Sub

#End Region
#Region "Mensaje 02xx"
#Region "Txt Definicion 0200"
    'MESSAGE 0200		MENSAJE DE REQUERIMIENTO DE TRANSACCION (OUTPUT B24).
    'START-OF-BASE24-HEADER-INDICATOR	El valor a informar es `ISO`.
    'BASE24-HEADER	De acuerdo al tipo de equipamiento que origina el requerimiento, los valores posibles son los siguientes:
    '-  ORIGEN DEL REQUERIMIENTO:  `ATM`. El valor a informar es `014000010`.
    '-  ORIGEN DEL REQUERIMIENTO:  `HOST`. El valor a informar es `014000050`.
    '-  ORIGEN DEL REQUERIMIENTO:  `INTERCHANGE`. El valor a informar es `014000070`.
    'MESSAGE-TYPE-IDENTIFIER	El valor a informar es `0200`.
    'PRIMARY-BIT-MAP	Debe informar la presencia de los campos: 1, 3, 4, 7, 11, 12, 13, 15, 17, 32, 35, 37, 41, 42, 43, 49, 52, 54, 60, 61, 62, 63.
    ' En el caso de extraccion o consulta, agregar el campo 45, solo los acquirer.
    ' SECONDARY-BIT-MAP	Debe informar la presencia de los campos: 100, 102, 103, 120, 124, 125, 127.
    ' En el caso de adelanto o consulta de cuenta de crédito en cuotas o selección de múltiples cuentas solo 
    ' para los ON Line puro, agregar el campo 126.
    'Campo 3	Ver tabla de códigos.
    'Campo 4	9(12)
    'Campo 7	mmddhhmmss 9(10)
    'Campo 11	9(6)
    'Campo 12	hhmmss 9(6)
    'Campo 13	mmdd 9(4)
    'Campo 15	mmdd 9(4)
    'Campo 32	LLVAR ..9(11)
    'Campo 35	El formato es:
    'Posiciones 01-02  =  Indicador de longitud. El valor a informar es `37`.
    'Posiciones 03-39  =  Valor del campo.
    'Campo 37	X(12)
    'Campo 41	X(16)
    'Campo 42	X(15)
    'Campo 43	El formato es:
    'Posiciones 01-22  =  Nombre de la  Institución dueña del ATM.
    'Posiciones 23-35  =  Localidad  donde  se  encuentra ubicado el ATM.
    'Posiciones 36-38  =  Código  de  Provincia  donde se encuentra ubicado el ATM.
    'Posiciones 39-40  =  Código de País donde se encuentra ubicado el ATM.
    'Campo 45	LLVAR ..9(76)
    'Track 1 alineado a izquierda y relleno con blancos. 
    'Campo 48	Ver nota campo 48.
    'Campo 49	Los valores posibles son:
    '`032`  -  PESOS.
    '`076`  -  REALES.
    '`840`  -  DOLARES.
    '`858`  -  PESOS URUGUAYOS.
    'Campo 52	X(16)
    'Campo 54	Ver nota campo 54.
    'Campo 55	Ver nota campo 55. 
    'Campo 60	El formato es:
    'Posiciones 01-03  =  Indicador de longitud. El valor a informar es `012`.
    'Posiciones 04-07  =  Número de  Institución dueña del ATM.
    'Posiciones 08-11  =  Código que identifica a la RED dueña del ATM.
    'Posiciones 12-15  =  Diferencia horaria con la RED. El valor a informar es `+000`.
    'Campo 61	El formato es:
    'Posiciones 01-03  =  Indicador de longitud. El valor a informar es `013`.
    'Posiciones 04-07  =  Número de la Institución emisora de la tarjeta.
    'Posiciones 08-11  =  Código que identifica a la RED. El valor a informar es`PRO1`.
    'Posiciones 12-15  =  Tipos de cuentas involucradas en la transacción.
    'Posiciones 16-16  =  El valor a informar es `P`.
    'Campo 62	Es condicional.
    'El formato es:
    'Posiciones 01-03  =  Indicador de longitud. El valor a informar es `025`.
    'Posiciones 04-05  =  Tipo de terminal en la que se realiza la transacción.
    'Posiciones 06-28  =  Informar blancos.
    'Campo 63	El formato es:
    'Posiciones 01-03  =  Indicador de longitud. El valor a informar es `016`.
    'Posiciones 04-19  =  Pin Offset.
    'Campo 100 	El formato es:
    'Posiciones 01-02  =  Indicador de longitud. El valor a informar es `11`. Posiciones 03-13  =  Valor  del  campo, alineado a izquierda y relleno con BLANCOS.
    'Campo 102	El formato es:
    'Posiciones 01-02  =  Indicador de longitud. El valor a informar es `28`.
    'Posiciones 03-30  =  Valor  del  campo,  alineado  a izquierda y relleno con BLANCOS.
    'Campo 103	El formato es:
    'Posiciones 01-02  =  Indicador de longitud.El valor a informar es `28`.
    'Posiciones 03-30  =  Valor  del  campo,  alineado  a izquierda y relleno con BLANCOS.
    'Campo 120	El formato es : 
    'Posiciones 01 - 03 Indicador de longitud. Fijo 033. 
    'Posiciones 04 - 28 Valor del campo, alineado a izquierda y relleno con blancos. Posiciones 29 - 32 Sucursal del cajero. 
    'Posiciones 33 - 36 Región del cajero.
    'Campo 124	El formato es:
    'Posiciones 01-03  =  Indicador de longitud. El valor a informar es `001`.
    'Posiciones 04-04  =  Los valores posibles son:
    '`0`  =  CERO.
    '` `  =  BLANCO.
    'Campo 125	El formato es:
    'Posiciones 01-03  =  Indicador de longitud. El valor a informar es `001`.
    'Posiciones 04-04  =  Los valores posibles son:
    '`0`  =  CERO.
    '`1`  =  UNO.
    '`2`  =  DOS.
    'Campo 126	Ver nota campo 126.
    'Campo 127	El formato es:
    'Posiciones 01-03  =  Indicador de longitud. El valor a informar es `043`.
    'Posiciones 04-07  =  Indica la región a la cuál pertenece el ATM, para transacciones en dólares. Los valores posibles son:
    '`0000`  =  CAPITAL Y GRAN BUENOS AIRES.
    '`0001`  =  USHUAIA.
    '`0002`  =  RESTO INTERIOR DEL PAIS.
    '`0005`  =  URUGUAY / BRASIL / CIRRUS.
    'Los cuatro campos siguientes deberán contener los valores informados en la consulta de tipo de cambio,  si la misma fue la transacción anterior y dentro del último minuto. En caso contrario, informar CEROS.
    'Posiciones 08-15  =  Tipo de cambio comprador U$S / $.El formato es 9(05)v999.      
    'Posiciones 16-23  =  Tipo de cambio vendedor U$S / $. El formato es 9(05)v999.
    'Posiciones 24-31  =  Tipo de cambio,  entre la moneda informada en el campo 49 y Pesos. El formato es 9(05)v999.
    'Posiciones 32-39  =  Tipo de cambio,  entre la moneda informada en el campo 49 y Dólares. El formato es 9(05)v999.
    'Posiciones 40-46  =  Se deberá informar CEROS.

    'De acuerdo al código de movimiento, el formato del campo 48 es el siguiente:
    'Transacciones originadas en ATM.
    'LONGITUD 	Longitud del campo 48. El valor a informar es `044`.	9(3)
    'ATM		
    '	SHRG-GRP	Sharing group	X(24)
    '	TERM-TRAN-ALLOWED	Indica a que nivel se permite una transacción para un cliente NOT-ON-US en dicha terminal	9(1)
    '	TERM-ST	Estado en el que reside la terminal. No aplicable.	9(2)
    '	TERM-CNTY	Código ANSI del país en el que reside la terminal. No utilizado.	9(3)
    '	TERM-CNTRY	Código ISO del país en el que reside la terminal. No utilizado.	9(3)
    '	TERM-RTE-GRP	Grupo de ruteo. No utilizado.	9(11)
    'Transacciones originadas en POS.
    'LONGITUD	Longitud del campo 48. El valor a informar es `079`.	9(3)
    'POS		
    'RETL-ID	Código de identificación del comercio donde se origina la transacción.	X(19)
    'DC		
    '	CUOTAS	Cantidad de cuotas en compras realizadas con Maestro / Electron.	X(2)
    '	FILLER		X(2)
    'RETL-REGN	Región a la que pertenece el comercio.	X(4)
    'SHRG-GRP	Sharing group	X(24)
    'TERM-TRAN-ALLOWED	Indica a que nivel se permite una transacción para un cliente NOT-ON-US en dicha terminal	9(1)
    'TERM-ST	Estado en el que reside la terminal. No aplicable.	9(2)
    'TERM-CNTY	Código ANSI del país en el que reside la terminal. No utilizado.	9(3)
    'TERM-CNTRY	Código ISO del país en el que reside la terminal. No utilizado.	9(3)
    'TERM-RTE-GRP	Grupo de ruteo. No utilizado.	9(11)
    'ORIG-NUM	En caso de ser una devolución Maestro / Electron, contiene el número de secuencia de la transacción original, de lo contrario `0000`	9(4)
    'ORIG-DATE	En caso de ser una devolución Maestro / Electron, contiene la fecha de la transacción original, de lo contrario `0000`.	9(4)
    'De acuerdo al código de movimiento, el formato del campo 54 es el siguiente:
    'DEPOSITOS (Códigos 2100xx / 2200xx)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'FILLER	Se deberá informar BLANCOS.	X(12)
    'TIPO-DEP	Tipo de depósito. Los valores posibles son:
    ' `E`  =  EFECTIVO en $ y `U` en U$S
    ' `C`  =  CHEQUE en $ y `W` en U$S.	X(1)
    'FILLER	Se deberá informar BLANCOS.	X(10)
    'PAGOS CON DÉBITO EN CUENTA A TRAVÉS DE TERMINALES DE CAJA (Códigos 10XXYY)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'FILLER	Se deberá informar BLANCOS.	X(12)
    'TIPO-DEP	Tipo de depósito. El valor posible es:
    ' `X` Pagos con débito en cuenta a través de terminales de caja
    ' 	X(1)
    'FILLER	Se deberá informar BLANCOS.	X(10)
    'PAGO DE SERVICIOS CON ORDEN DE DEBITO (Códigos 90xx00)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'FILLER	Se deberá informar BLANCOS.	X(13)
    'CANT-COMPR	Cantidad de comprobantes.	9(2)
    'FILLER	Se deberá informar BLANCOS.	X(8)
    'CAMBIO DE PIN (Código 32) 
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'FILLER	Se deberá informar BLANCOS.	X(15)
    'NUEVO-PIN	PIN-NUEVO-1, PIN-NUEVO-2, alineado a izquierda y relleno con BLANCOS.	X(8)
    'CAMBIO DE PIN ENCRIPTADO (Código 32)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'PIN-ENCRIPTADO	Nuevo número de PIN-Offset.	X(16)
    'FILLER	Se deberá informar BLANCOS.	X(7)
    'PAGO AUTOMATICO DE SERVICIOS	(Códigos 80/ 81/ 82/ 83/ 84/ 85/ 87/ 88/ 89)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'CDE-ID-CUSTOM	CODIGO DE IDENTIFICACION DEL CLIENTE	
    '	CDE-ENTE	Código de Ente	9(3)
    '	CDE-CUSTOM	Código de cliente	X(19)
    'NRO-OPC	Opción que corresponde a la selección de la deuda.	9(1)
    'COMPRA CON CASH-BACK (Códigos 76/ 77)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'TRANSACTION-AMOUNT	Es el importe del retiro en efectivo.	9(10)v99
    'FILLER	Se deberá informar BLANCOS.	X(11)
    'TRANSFERENCIA MINORISTA (1B)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'CBU	Número de clave bancaria uniforme.	X(22)
    'FILLER	Se deberá informar BLANCOS.	X(98)
    'RESTANTES CODIGOS
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'FILLER	Se deberá informar BLANCOS.	X(23)

    'De acuerdo al código de movimiento, el formato del campo 55 es el siguiente:
    'INTERBANCARIAS (Códigos 09/ 29/ 39)
    'LONGITUD	Indicador de longitud. El valor a informar es `120`.	9(3)
    'TRACK2
    '	Contiene datos del Track 2 de la tarjeta alineado a izquierda y rellenos con blancos.	X(40)
    'CA	Código asociado	X(2)
    'FR-ACCT	Número de cuenta desde	
    '	FIID	Institución Emisora de la cuenta	X(4)
    '	TYP	Tipo de cuenta	X(2)
    '	ACCT-NUM	Número de cuenta desde	X(19)
    'TO-ACCT		
    '	FIID	Institución Emisora de la cuenta	X(4)
    '	TYP	Tipo de cuenta	X(2)
    '	ACCT-NUM	Número de cuenta hacia	X(19)
    '	FIID-CPF	Institución a la cual pertenece la cuenta hacia
    'Si se trata de donaciones, este dampo se redefine de la siguiente manera:
    'Ente           X(3)
    'Filler         X(10)	X(13)
    '	RUBRO	Datos del titular	X(15)
    'PAY KEY (Código 17)

    'LONGITUD	Indicador de longitud. El valor a informar es `038`.	9(3)
    'COMPRAS		
    'COD-EMPRESA	Código de empresa.	X(3)
    'COD-PRODUCTO	Código de producto.	X(3)
    'COD-ARTICULO	Código de artículo.	X(32)
    'FCI (Códigos 08/ 28/ 36/ 37/ 38/ 48/ 63/ 68/ A8/ B8/ C8)
    'LONGITUD	Indicador de longitud. El valor a informar es `120`.	9(3)
    'PROD-ID	Identificador de producto.	X(2)
    'SUBPROD-ID	En el caso de las transacciones que necesiten subproducto, como ser subscripción, rscate, información de propaganda, se informará en este campo.	X(2)
    'SUBPROD-ID-DEST	En el caso de las subscripciones y de los rescates, se deberá informar la descripción del nombre del subproducto.	X(2)
    'CURRENCY-CODE	Se informa la moneda del fondo, los valores posibles son : 
    '`032` para pesos o `840` para dólares. 	X(3)
    'CURRENCY-CODE-DEST	Se informa la moneda del fondo, los valores posibles son : 
    '`032` para pesos o `840` para dólares. 	X(3)
    'CANT-CP-A-RESCATAR	Solo en el caso del rescate, se informará la cantidad de cuotas partes a rescatar.	9(9)
    'TXN-TYP-FLG	Para la consulta de un producto (tx 36), como se puede originar desde dos transacciones distintas, este flag indica desde cual de las dos se riginó:
    '`2` para subscripción o `7` para consulta de información publicitaria.	X(1)
    'LINEA-SELECCION	Item seleccionado por el usuario según lo informa el banco.	X(37)
    'ID-TRANSACCION	Código de identificación de transacción.	X(20)
    'FLAG-CONFIRMACION	Identifica una transacción a confirmar.	X(1)
    'TIPO-IMPRESIÓN	Envia S cuando es suscripción o R cuando es rescate.	X(1)
    'FILLER	Blancos	X(39)
    'De acuerdo al código de movimiento, el formato del campo 126 es el siguiente:

    'SELECCIÓN DE MÚLTIPLES CUENTAS
    'LONGITUD	Indicador de longitud. El valor a informar es `010`.	9(3)
    'INDICADOR	Indica si el ATM soporta múltiples cuentas o no.
    '    '1’ Si
    '    '0’ No.	X(1)
    'FILLER		X(9)
#End Region
    Public Sub ProcesarRespuesta(ByRef mensaje As Iso8583messagePOS)
        Logger.Info(" llego una respuesta ..")
        RaiseEvent TransaccionRespuesta(mensaje)
    End Sub

    Public Sub ProcesarNoImplementadas(ByRef mensaje As Iso8583messagePOS, ByRef e As ParametrosCom)
        e.Respuesta = E_CodigosRespuesta.Transaccion_Invalida
        RaiseEvent TransaccionPOSNoImplementada(e)
        Try

            If e.Respuesta = E_CodigosRespuesta.Transaccion_Invalida Then
                If mensaje.Fields(3).ToString = "830000" Then
                    'para que no moleste
                    Logger.Info("Transaccion No Soportada " + mensaje.Fields(3).ToString)

                Else
                    Logger.Warn("Transaccion No Soportada " + mensaje.Fields(3).ToString)
                End If
                Select Case Implementacion

                    Case E_Implementaciones.PosnetComercio
                        CompletarPosnet(mensaje, e)

                End Select
            End If
        Catch ex As Exception
            Logger.Error("error respondiendo No Implementado")
        End Try

        Responder(mensaje, e.Respuesta)
    End Sub

    Public Overridable Sub ProcesarTransaccion(ByRef mensaje As Iso8583messagePOS, ByVal modo As e_ModosTransaccion)
        Try
            Dim e As New ParametrosCom
            'Los códigos de transacciones utilizados por el sistema tienen la siguiente estructura: 'nnxxyy', donde:
            Dim CodigoTransaccion As Integer = CInt(mensaje.Fields(3).ToString)
            ' monto de la transaccion
            e.implementacion = mensaje.Implementacion
            e.Monto = Field4_ObtenerMonto(mensaje)
            ' fecha de negocio
            'e.FechaNegocio = Field17_ObtenerFechaNegocio(mensaje)
            Logger.Debug("Fecha Negocio:" + e.FechaNegocio.ToShortDateString)
            'fecha de la transaccion, incluyendo HORA!
            e.Fechatransaccion = Field12y13_ObtenerFechaHora(mensaje)
            ' tarjeta
            e.Tarjeta = Field2or35_ObtenerTarjeta(mensaje)
            'e.Cuenta = Field102_ObtenerCuenta(mensaje)
            ' esto es para que no reviente si llega una transaccion no detallada en E_proccode 
            Try
                e.ProcCode = CType(CodigoTransaccion, E_ProcCode)
            Catch ex As Exception

            End Try
            ' manejo de monedas
            Dim Moneda As E_Monedas = Field49_ObtenerMoneda(mensaje)
            ' esto es paraq proar desde uruugay
            If Moneda <> E_Monedas.PESOS And Moneda <> E_Monedas.PESOS_URUGUAYOS And Moneda <> E_Monedas.REALES Then
                Logger.Warn("Moneda invalida" + Moneda.ToString)
                Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                Exit Try
            End If
            Logger.Info("Procesando  " + mensaje.MessageTypeIdentifier.ToString + "-" + CodigoTransaccion.ToString)

            e.REtRefNumber = Field37_obtenerRetRefNumber(mensaje)
            e.idTerminalCOM = Field41_ObtenerIdTerminal(mensaje)
            e.Trace = Field11_ObtenerTrace(mensaje)
            e.Track1 = Field45_obtenerTrack1(mensaje)
            e.Track2 = Field35_obtenerTrack2(mensaje)

            'campos especificos de 
            e.Modo = modo
            If modo = e_ModosTransaccion.Reverso Then
                e.MotivoReverso = Field39_ObtenerRespuestaReverso(mensaje)
                e.MontoAimpactar = Field95_ObtenerMontoRealaImpactar(mensaje)

            End If



            'manejo de Hostoffline, como es mulithreading, esperar algunos segundos y sino dar erorr
            If HostOfflIne Then
                'esperar 6 segundos
                Logger.Warn("Host Down: esperando ...")

                Do
                    Threading.Thread.Sleep(6 * 1000)
                Loop
            End If
            If HostOfflIne Then
                Logger.Warn("Host Is Down:")
                Select Case Implementacion

                    Case E_Implementaciones.PosnetComercio
                        CompletarPosnet(mensaje, e)
                        Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)

                End Select
            Else
                Select Case Implementacion


                    ' ----------- POSNET COMERCIO 
                    Case E_Implementaciones.PosnetComercio, E_Implementaciones.Visa
                        e.Vto = Field14_ObtenerVto(mensaje)

                        Select Case CodigoTransaccion
                            Case E_ProcCode.Compra, _
                                E_ProcCode.Devolucion
                                'prepara aca
                                e.NroAutorizacion = Field38_ObtenerNroAutorizacion(mensaje)
                                e.CuentaComercio = Field42_ObtenerCuentaComercio(mensaje)
                                e.texto = "Compra/Dev " + CodigoTransaccion.ToString
                                e.modoIngreso = Field22_ObtenerModoIngreso(mensaje)

                                Try

                                    Dim _CodigoRespuesta As E_CodigosRespuesta = E_CodigosRespuesta.Transaccion_Invalida
                                    Select Case modo
                                        Case e_ModosTransaccion.Normal, e_ModosTransaccion.SAF
                                            RaiseEvent TransaccionPOSCredito(e, CType(CodigoTransaccion, E_ProcCode))
                                            CompletarPosnet(mensaje, e)
                                            If modo = e_ModosTransaccion.Normal Then Field38_PonerAutorizacion(mensaje, e.NroAutorizacion)
                                            Responder(mensaje, e.Respuesta)
                                        Case e_ModosTransaccion.Reverso
                                            ReversoPosnet(mensaje, e, CodigoTransaccion)
                                    End Select

                                Catch ex As Exception
                                    Logger.Error("Error en el parseo Compra" + ex.Message + " ")
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try
                            Case E_ProcCode.AnulacionCompra, _
                              E_ProcCode.AnulacionDevolucion
                                'prepara aca
                                e.CuentaComercio = Field42_ObtenerCuentaComercio(mensaje)
                                e.texto = "Compra " + CodigoTransaccion.ToString
                                If mensaje.Fields.Contains(63) Then
                                    e.p63Msg = CStr(mensaje.Fields(63).Value)
                                Else
                                    e.p63Msg = ""
                                End If
                                If mensaje.Fields.Contains(90) Then
                                    e.p90 = New Mensaje90DatosOriginales(CStr(mensaje.Fields(90).Value))
                                Else
                                    e.p90 = New Mensaje90DatosOriginales("")
                                End If
                                Try

                                    Dim _CodigoRespuesta As E_CodigosRespuesta = E_CodigosRespuesta.Transaccion_Invalida
                                    Select Case modo
                                        Case e_ModosTransaccion.Normal, e_ModosTransaccion.SAF
                                            RaiseEvent TransaccionPOSCreditoAnulacion(e)
                                            CompletarPosnet(mensaje, e)
                                            Field38_PonerAutorizacion(mensaje, e.NroAutorizacion)
                                            Responder(mensaje, e.Respuesta)
                                        Case e_ModosTransaccion.Reverso
                                            ReversoPosnet(mensaje, e, CodigoTransaccion)
                                    End Select
                                Catch ex As Exception
                                    Logger.Error("Error en el parseo Compra" + ex.Message + " ")
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try

                            Case E_ProcCode.Compra_maestro_CajAhorroP, E_ProcCode.Compra_maestro_CajAhorroD, E_ProcCode.Compra_maestro_CtaCteD, E_ProcCode.Compra_maestro_CtaCteP
                                'prepara aca
                                e.NroAutorizacion = Field38_ObtenerNroAutorizacion(mensaje)
                                e.CuentaComercio = Field42_ObtenerCuentaComercio(mensaje)
                                e.texto = "Compra/Dev " + CodigoTransaccion.ToString
                                e.modoIngreso = Field22_ObtenerModoIngreso(mensaje)
                                If mensaje.Fields.Contains(52) Then
                                    e.wkey = mensaje.Fields(52).Value
                                Else
                                    Responder(mensaje, E_CodigosRespuesta.Pin_requerido)
                                End If
                                Try

                                    Dim _CodigoRespuesta As E_CodigosRespuesta = E_CodigosRespuesta.Transaccion_Invalida
                                    Select Case modo
                                        Case e_ModosTransaccion.Normal, e_ModosTransaccion.SAF
                                            RaiseEvent TransaccionPOSCredito(e, CType(CodigoTransaccion, E_ProcCode))
                                            CompletarPosnet(mensaje, e)
                                            Field52_PonerWkey(mensaje)
                                            If modo = e_ModosTransaccion.Normal Then Field38_PonerAutorizacion(mensaje, e.NroAutorizacion)
                                            Responder(mensaje, e.Respuesta)
                                        Case e_ModosTransaccion.Reverso
                                            ReversoPosnet(mensaje, e, CodigoTransaccion)
                                    End Select

                                Catch ex As Exception
                                    Logger.Error("Error en el parseo Compra" + ex.Message + " ")
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try

                            Case Else
                                ProcesarNoImplementadas(mensaje, e)
                        End Select
                        ' anulaciones

                End Select
            End If
        Catch ex As Exception
            Logger.Error("Error Interpretando Mensaje  " + ex.Message + " " + " " + ex.StackTrace)
        End Try
    End Sub

    Public Sub ProcesarCierre(ByRef mensaje As Iso8583messagePOS)
        Dim e As New ParametrosCom
        e.idTerminalCOM = Field41_ObtenerIdTerminal(mensaje)

        RaiseEvent Cierre(e)
        Responder(mensaje, e.Respuesta)
    End Sub


    Public Sub CompletarPosnet(ByVal mensaje As Iso8583messagePOS, ByVal e As ParametrosCom)
        Try
            ' es la respuesta a esos mensajes

            Select Case mensaje.MessageTypeIdentifier
                Case 200 '-> 210
                    Field15_PonerFechaHora(mensaje)
                Case 220, 221 ' -> 230
                    mensaje.Fields.Remove(12)
                    mensaje.Fields.Remove(13)
                    mensaje.Fields.Remove(35)
                    mensaje.Fields.Remove(38)
                    mensaje.Fields.Remove(48)
                    mensaje.Fields.Remove(54)
                    mensaje.Fields.Remove(60)

                    mensaje.Fields.Remove(90)
                    mensaje.Fields.Remove(95)

                Case 500
                    mensaje.Fields.Remove(60)

            End Select

            If e.p63Msg IsNot Nothing Then mensaje.Fields.Add(63, e.p63Msg)
            mensaje.Fields.Remove(9)
            mensaje.Fields.Remove(10)
            mensaje.Fields.Remove(14)
            mensaje.Fields.Remove(17)
            mensaje.Fields.Remove(18)

            mensaje.Fields.Remove(22)
            mensaje.Fields.Remove(25)
            mensaje.Fields.Remove(27)

            mensaje.Fields.Remove(32)
            mensaje.Fields.Remove(33)

            mensaje.Fields.Remove(42)
            mensaje.Fields.Remove(43)
            mensaje.Fields.Remove(44)
            mensaje.Fields.Remove(45)

            mensaje.Fields.Remove(50)
            mensaje.Fields.Remove(51)
            'mensaje.Fields.Remove(52)

            mensaje.Fields.Remove(62)

            'mensaje.Fields.Add(102, e.Cuenta.ToString("000000000"))
            mensaje.Fields.Remove(120)
            mensaje.Fields.Remove(123)
            mensaje.Fields.Remove(124)
            mensaje.Fields.Remove(126)
        Catch ex As Exception
            Logger.Error("completando mensaje posnet :" + ex.Message + "  " + ex.StackTrace)
        End Try
    End Sub

    Public Sub ReversoPosnet(ByRef mensaje As Iso8583messagePOS, ByRef e As ParametrosCom, ByVal ProcCode As Integer)
        If e.MotivoReverso = E_CodigoREspuestaReversos.Transaccion_sospechosa Or e.MotivoReverso = 22 Then
            Logger.Warn("Reverso Sospechoso " + mensaje.Fields(11).ToString)
            e.MontoAimpactar = e.Monto
        End If
        Select Case ProcCode
            Case E_ProcCode.AnulacionCompra, E_ProcCode.AnulacionDevolucion
                RaiseEvent TransaccionPOSCreditoReversoAnulacion(e)
            Case Else
                RaiseEvent TransaccionPOSCreditoReverso(e)

        End Select
        If mensaje.Fields.Contains(9) Then mensaje.Fields.Remove(9)
        If mensaje.Fields.Contains(10) Then mensaje.Fields.Remove(10)

        If mensaje.Fields.Contains(14) Then mensaje.Fields.Remove(14)
        If mensaje.Fields.Contains(15) Then mensaje.Fields.Remove(15)
        If mensaje.Fields.Contains(17) Then mensaje.Fields.Remove(17)
        If mensaje.Fields.Contains(18) Then mensaje.Fields.Remove(18)

        If mensaje.Fields.Contains(22) Then mensaje.Fields.Remove(22)
        If mensaje.Fields.Contains(25) Then mensaje.Fields.Remove(25)
        If mensaje.Fields.Contains(27) Then mensaje.Fields.Remove(27)

        If mensaje.Fields.Contains(32) Then mensaje.Fields.Remove(32)
        If mensaje.Fields.Contains(33) Then mensaje.Fields.Remove(33)
        If mensaje.Fields.Contains(35) Then mensaje.Fields.Remove(35)
        'If mensaje.Fields.Contains(38) Then mensaje.Fields.Remove(38)

        If mensaje.Fields.Contains(42) Then mensaje.Fields.Remove(42)
        If mensaje.Fields.Contains(43) Then mensaje.Fields.Remove(43)
        If mensaje.Fields.Contains(44) Then mensaje.Fields.Remove(44)
        If mensaje.Fields.Contains(45) Then mensaje.Fields.Remove(45)
        If mensaje.Fields.Contains(48) Then mensaje.Fields.Remove(48)

        If mensaje.Fields.Contains(50) Then mensaje.Fields.Remove(50)
        If mensaje.Fields.Contains(51) Then mensaje.Fields.Remove(51)
        If mensaje.Fields.Contains(52) Then mensaje.Fields.Remove(52)
        If mensaje.Fields.Contains(54) Then mensaje.Fields.Remove(54)

        If mensaje.Fields.Contains(60) Then mensaje.Fields.Remove(60)
        If mensaje.Fields.Contains(62) Then mensaje.Fields.Remove(62)

        If mensaje.Fields.Contains(95) Then mensaje.Fields.Remove(95)

        If mensaje.Fields.Contains(120) Then mensaje.Fields.Remove(120)
        If mensaje.Fields.Contains(123) Then mensaje.Fields.Remove(123)
        If mensaje.Fields.Contains(124) Then mensaje.Fields.Remove(124)
        If mensaje.Fields.Contains(126) Then mensaje.Fields.Remove(126)

        Responder(mensaje, e.Respuesta)
    End Sub

    Public Sub Responder(ByRef mensaje As Iso8583messagePOS, ByVal pCodigoRespuesta As E_CodigosRespuesta)
        Logger.Info("Respondiendo  Cod Rta:" + pCodigoRespuesta.ToString)
        mensaje.SetResponseMEssageTypeIdentifier()

        '39	resp-cde	Código de respuesta que indica el estado del mensaje

        'scar el 52 PIN si viene

        'If mensaje.Fields.Contains(52) Then mensaje.Fields.Remove(52)
        Field39_PonerRespuesta(mensaje, pCodigoRespuesta)
        RaiseEvent BeforeSend(mensaje)
        mensaje.Send()

    End Sub

#Region "txt definicion campos"
    'BIT	NOMBRE DEL CAMPO
    '	DESCRIPCION
    '1	secndry-bit-map	Controla la presencia o ausencia de elementos de datos, desde la posición 65 a 128. Es un dato en sí mismo y su presencia o ausencia	es controlada por el BIT 1 del campo anterior.
    '3	proc-cde	Código de transacción
    '4 	tran-amt	Monto de la transacción
    '7 	xmit-dat-tim	(TRANSMISSION-DATE-AND-TIME ) Representa la fecha y hora del mensaje
    '11 	trace-num	Número que es fijado por el que origina el mensaje y repetido por el receptor del mensaje. Es usado para chequear la respuesta al mensaje original. Puede no ser el mismo durante el transcurso de la transacción. Por ejemplo: Un reverso puede no tener el número de la transacción original.
    '12 	tran-tim	Es la hora local en que comenzó la transacción
    '13	tran-dat	Es la fecha local en que comenzó la transacción
    '15	setl-dat	Fecha de negocios de una transacción realizada en otra Red. En caso contrario, se deberá informar CEROS
    '32	acq-inst-id	Código interno de la Institución pagadora
    '35	track 2 	Contiene los datos del TRACK 2 de la tarjeta, alineado a izquierda y relleno con BLANCOS.
    '37	retrvl-ref-num	Número de recibo, alineado a izquierda y relleno con BLANCOS.
    '39	resp-cde	Código de respuesta que indica el estado del mensaje
    '41	term-id	(CARD-ACCEPTOR-TERMINAL-ID)Número de terminal, alineado a izquierda y relleno con BLANCOS.
    '42	crd-accpt-id-cde	Siempre relleno con BLANCOS. Para otros dispositivos que no sean cajeros automáticos, en este campo se informará 'CIT, lo cual indica que se deberá validar clave telefónica en lugar de pin de cajero
    '43	crd-accpt-name-loc	Nombre y localidad del dueño del ATM
    '44	resp-data	(ADDITIONAL-RESPONSE-DATA) Datos adicionales del mensaje de respuesta dependientes del tipo de mensaje.
    '48	add-data-prvt 	Datos adicionales de la transacción. Depende de donde fue originada la transacción si en ATM o POS.
    '49	crncy-cde	Código de moneda de la transacción
    '52	pin	Número de PIN, en forma encriptada
    '54 	add-amts	Datos adicionales dependientes del tipo de mensaje
    '55 	pri-rsrvd1-iso	(CAMPO P55) Datos para transacción Interbancarias  o
    '(P55-PAY-DATA) Datos del pago (PAS) o
    '(PRI-RSRVD1-ISO) Cuentas relacionadas a la tarjeta. (Consultas generales) 
    'Depende del tipo de mensaje.
    '60	pri-rsrvd1-prvt	(TERMINAL-DATA) Datos de la terminal que originó la transacción
    '61	pri-rsrvd2-prvt	(CARD-ISSUER-AND-AUTH-DATA) Datos de la Institución emisora de la tarjeta
    '63	pri-rsrvd4-prvt	(PIN-OFFSET) Campo de Pin Offset
    '70	netw-mgmt-cde	(NETWORK-MANAGEMENT-INFORMATION-CODE ) Código que es  usado para manejar el  STATUS  de procesamiento ON-LINE entre BASE-24 y un HOST
    '90	orig-info	(ORIGINAL-ELEMENTS) Datos de la transacción original dependiente del tipo de mensaje.
    '95	replacement	(REPLACEMENT-AMOUNTS) Monto realmente dispensado en una reversa parcial, dependiente del tipo de mensaje.
    '100	rcv-inst	(RECEIVING-INST-ID-CODE) Código interno que identifica la Institución emisora
    '102	acct1	Número de cuenta desde
    '103	acct2	Número de cuenta hacia
    '120	secndry-rsrvd1-prvt	(TERMINAL-ADDRESS) Dirección del ATM dependiente del tipo de mensaje.
    '122	secndry-rsrvd3-prvt	(CARD-ISSUER-IDENT-CODE) Código interno de la Institución emisora dependiente del tipo de mensaje.
    '123	secndry-rsrvd4-prvt	(DEPOSIT-CREDIT-AMOUNT) Monto del depósito que se suma al saldo disponible dependiente del tipo de mensaje.
    '124	secndry-rsrvd5-prvt	(DEPOSITORY-TYPE) Tipo de depositorio
    '125	secndry-rsrvd6-prvt	(ACCOUNT-INDICATOR) Indicador de cuenta a procesar por el Host,  en una transacción que involucre dos cuentas; o bien,
    '(STATEMENT-PRINT-DATA) Statement printer, según el tipo de mensaje.
    '127	secndry-rsrvd8-prvt	(ADDITIONAL-DATA) Datos adicionales de la transacción dependiente del tipo de mensaje.
#End Region

#End Region
#Region "Otros Mensajes"
    Public Sub Responder_MensajeNoImplementado(ByRef mensaje As Iso8583messagePOS, ByVal MessageSource As IMessageSource)
        ' advertir en algun log para control nuestro, que llego algun mensaje que todavia no implementamos
        Logger.Warn("Mensaje No Implementado " + mensaje.MessageTypeIdentifier.ToString)
        ' convertir a 9xxx donde xxx es el numero de mensaje original
        If mensaje.MessageTypeIdentifier > 9000 Then
            Logger.Error("me llego un " + mensaje.MessageTypeIdentifier.ToString)
        Else
            mensaje.MessageTypeIdentifier = mensaje.MessageTypeIdentifier + 9000

            MessageSource.Send(mensaje)
        End If
    End Sub
#End Region

#Region "Mensaje 0800"
    'MESSAGE 0810		MENSAJE DE RESPUESTA DE CONTROL.
    'START-OF-BASE24-HEADER-INDICATOR	El valor a informar es `ISO`
    'BASE24-HEADER	El valor a informar es `004000040`.
    'MESSAGE-TYPE-IDENTIFIER	El valor a informar es `0810`.
    'PRIMARY-BIT-MAP	Debe informar la presencia de los campos: 1, 7, 11, 39.
    'SECONDARY-BIT-MAP	Debe informar la presencia del campo 70.
    'Campo 7	mmddhhmmss 9(10)
    'Campo 11	9(6)
    'Campo 39	Los valores posibles son:
    '`00`  -  APROBADO.
    '`01`  -  DENEGADO.
    '`91`  -  HOST DOWN.
    'Campo 70	Los valores posibles son:
    '`001`  -  LOGON.
    '`002`  -  LOGOFF.
    '`301`  -  ECHO-TEST.

    ' pone a 810
#Region "txt mensaje 0800"
    'MESSAGE 0800		MENSAJE DE REQUERIMIENTO DE CONTROL.
    'START-OF-BASE24-HEADER-INDICATOR	El valor a informar es `ISO`.
    'BASE24-HEADER	El valor a informar es `004000040`.
    'MESSAGE-TYPE-IDENTIFIER	El valor a informar es `0800`.
    'PRIMARY-BIT-MAP	Debe informar la presencia de los campos 1, 7, 11.
    'SECONDARY-BIT-MAP	Debe informar la presencia del campo 70. 
    'Campo 7	mmddhhmmss 9(10)
    'Campo 11	9(6)
    'Campo 70	Los valores posibles son:
    '`001`  -  LOGON.
    '`002`  -  LOGOFF.
    '`301`  -  ECHO-TEST.
#End Region

    Public Sub ProcesarControl(ByRef mensajeRecibido As Iso8583messagePOS)
        Try
            Try
                If mensajeRecibido.Fields.Contains(41) Then
                    If mensajeRecibido.Fields(41).Value.ToString = "ECOTEST" Then

                    Else
                        Last0800 = Now
                    End If
                Else
                    Last0800 = Now
                End If
                Logger.Info(" last 0800:" + Last0800.ToString)
            Catch ex As Exception

            End Try

            'Dim CodigoControl As Integer = CInt(mensajeRecibido.Fields(70).ToString)
            ' Select Case CodigoControl
            '    Case E_eventosControl.Logon, E_eventosControl.Logoff, E_eventosControl.EchoTest ' LOGON
            Dim Respuesta As e_eventosRespuesta = e_eventosRespuesta.Aprobado
            'If CodigoControl = E_eventosControl.Logon Then Logstatus = True
            'If CodigoControl = E_eventosControl.Logoff Then Logstatus = False
            'RaiseEvent Control(CType(CodigoControl, E_eventosControl), Respuesta)
            RaiseEvent Control(Respuesta)
            Responder_0800(mensajeRecibido, Respuesta)
            '    Case E_eventosControl.EchotestInterno
            'Dim Respuesta As e_eventosRespuesta = e_eventosRespuesta.Aprobado

            'RaiseEvent Control(CType(CodigoControl, E_eventosControl), Respuesta)
            'Responder_0800(mensajeRecibido, Respuesta)
            '    Case Else
            'Logger.Warn("0800 con campo 70 incorrecto:" + mensajeRecibido.Fields(70).ToString)
            'Responder_0800(mensajeRecibido, e_eventosRespuesta.ControlInvalido)
            'End Select
        Catch ex As Exception
            Logger.Error(String.Format("0800 sin campo 70:{0} {1}", ex.Message, ex.StackTrace))
        End Try

    End Sub
    Public Property Logstatus() As Boolean
        Get
            Return _logstatus
        End Get
        Set(ByVal value As Boolean)
            If value Then
                Logger.Info("Sistema Logeado")
                RaiseEvent LogOn()
            Else
                Logger.Info("Sistema Des-Logeado")
                RaiseEvent Logoff()
            End If
            _logstatus = False
        End Set
    End Property

    Public Sub Responder_0800(ByRef mensaje As Iso8583messagePOS, ByVal pCodigoRespuesta As e_eventosRespuesta)
        Logger.Info("****Respondiendo 0800")

        mensaje.SetResponseMEssageTypeIdentifier()
        ' Setear Fecha
        'Field7_PonerFechaHora(mensaje)

        ' Field12y13_ObtenerFechaHora(mensaje)
        ' aca si tenemos algun problema y queremos responderle que no andamos bien seria este campo el que tendria que ser
        ' poner respuesta OK
        If pCodigoRespuesta = E_CodigosRespuesta.Transaccion_Invalida Then
            ' usamos este codigo para NO devolver una respuesta
        Else
            Field39_PonerRespuesta(mensaje, CType(pCodigoRespuesta, E_CodigosRespuesta))
        End If
        RaiseEvent BeforeSend(mensaje)
        mensaje.Send()

    End Sub
#End Region

#Region "Funciones adicionales de mensajes"
    Public Function Field39_PonerRespuesta(ByRef mensaje As Iso8583messagePOS, ByVal pCodResp As E_CodigosRespuesta) As String
        Dim CodResp As String = CInt(pCodResp).ToString("00")
        mensaje.Fields.Add(39, CodResp)
        Logger.Info("Respuesta " + CodResp)
        Return CodResp
    End Function
    Public Function Field38_PonerAutorizacion(ByRef mensaje As Iso8583messagePOS, ByVal NroAutorizacion As Integer) As String
        Dim NroAut As String = CInt(NroAutorizacion).ToString("000000")
        mensaje.Fields.Add(38, NroAut)
        Logger.Info("Nro Autorizacion " + NroAut)
        Return NroAut
    End Function
    Public Function Field52_PonerWkey(ByRef mensaje As Iso8583messagePOS) As String
        mensaje.Fields.Add(52, "ABCDEF1234567890")
        Logger.Info("Nro Autorizacion " + "ABCDEF1234567890")
        Return "ABCDEF1234567890"
    End Function

    Public Function Field15_PonerFechaHora(ByRef mensaje As Iso8583messagePOS) As DateTime
        Dim transmissionDate As DateTime = FechaActual()

        mensaje.Fields.Add(15, transmissionDate.ToString("MMdd"))

        Logger.Info(" Date:" + transmissionDate.ToString)

        Return transmissionDate

    End Function
    Public Function Field7_PonerFechaHora(ByRef mensaje As Iso8583messagePOS) As DateTime
        Dim transmissionDate As DateTime = FechaActual()

        mensaje.Fields.Add(E_Fields.Field7TransDateTime, String.Format("{0}{1}", _
         String.Format("{0:00}{1:00}", transmissionDate.Month, transmissionDate.Day), _
         String.Format("{0:00}{1:00}{2:00}", transmissionDate.Hour, _
         transmissionDate.Minute, transmissionDate.Second)))

        Logger.Info("Transmision Date:" + transmissionDate.ToString)

        Return transmissionDate

    End Function

    Public Function Field11_PonerTrace(ByRef mensaje As Iso8583messagePOS, ByRef Numerador As Trx.Messaging.VolatileStanSequencer) As Integer
        Dim s As String = Numerador.Increment().ToString()
        mensaje.Fields.Add(E_Fields.Field11Trace, s)
        Logger.Info("Numerador:" + s)
        Return Numerador.CurrentValue
    End Function

    Public Function Field41_ObtenerIdTerminal(ByRef mensaje As Iso8583messagePOS) As String
        Return mensaje.Fields(41).ToString
    End Function
    Public Function Field38_ObtenerNroAutorizacion(ByRef Mensaje As Iso8583messagePOS) As Integer
        Try
            If Mensaje.Fields.Contains(38) Then
                Return CInt(Val(CStr(Mensaje.Fields(38).Value).Replace(" ", "")))
            Else
                Return 0
            End If
        Catch ex As Exception
            Logger.Info("Error Obteniendo Nro Autorizacion!" + ex.Message)
            Return 0
        End Try

    End Function
    Public Function Field42_ObtenerCuentaComercio(ByRef Mensaje As Iso8583messagePOS) As Integer
        Try
            Return CInt(CStr(Mensaje.Fields(42).Value).Replace(" ", ""))
        Catch ex As Exception
            Logger.Info("Error Obteniendo cuenta comercio!" + ex.Message)
            Return 0
        End Try

    End Function
    Public Function Field11_ObtenerTrace(ByRef mensaje As Iso8583messagePOS) As Integer
        Dim trace As Integer = CInt(mensaje.Fields(11).Value)
        Logger.Info("Numerador:" + trace.ToString)
        Return trace
    End Function
    Public Function Field22_ObtenerModoIngreso(ByRef mensaje As Iso8583messagePOS) As Integer
        Dim ModoIngreso As Integer = CInt(CStr(mensaje.Fields(22).Value).Substring(0, 2))

        Logger.Info("modo ingreso:" + ModoIngreso.ToString)
        Return ModoIngreso
    End Function
    Public Function Field4_ObtenerMonto(ByRef mensaje As Iso8583messagePOS) As Decimal
        Dim _Monto As Decimal
        Try
            If mensaje.Fields.Contains(4) Then
                _Monto = CDec(mensaje.Fields(4).ToString) / 100
                Logger.Info("MONTO  :" + _Monto.ToString)
            Else
                Logger.Info("Monto  No informado (4)")
                _Monto = 0
            End If

        Catch ex As Exception
            Logger.Error("error Obteniendo Monto (campo 4) " + ex.Message)
        End Try
        Return _Monto
    End Function

    Public Function Field39_ObtenerRespuestaReverso(ByRef mensaje As Iso8583messagePOS) As Integer
        Dim _Respuesta As Integer
        _Respuesta = 0
        Try
            If mensaje.Fields.Contains(39) Then
                Try
                    Select Case mensaje.Fields(39).ToString
                        Case Is <= "99"
                            _Respuesta = CInt(mensaje.Fields(39).ToString)
                        Case Else
                            _Respuesta = 100
                    End Select
                    Logger.Info("Motivo Reverso  :" + CType(_Respuesta, E_CodigoREspuestaReversos).ToString)
                Catch ex As Exception

                End Try
            Else
                'lo agregue porque no viene el campo 39.
                mensaje.Fields.Add(39, 0)
            End If
        Catch ex As Exception
            Logger.Error("ERROR obteniendo Respuesta Reverso (campo 39) " + ex.Message)
        End Try
        Return _Respuesta
    End Function


    Public Function Field95_ObtenerMontoRealaImpactar(ByRef mensaje As Iso8583messagePOS) As Decimal
        Dim _Monto As Decimal
        '        * el campo 95 se envía solamente cuando la transacción es reversada parcialmente.
        'Para mensajes de reverso (0420/0421) parcial (P-039: “32”) de extracción (código transacción: “01”):
        '1-12 Monto por el que realmente se completó la transacción en la moneda de la operación (P-049) (2 decimales).
        '13-42: Sin uso.

        Try

            Dim Usar95 As Boolean = mensaje.Fields(39).ToString = "32"
            If Usar95 And mensaje.Fields.Contains(95) Then
                _Monto = CDec(mensaje.Fields(95).ToString.Substring(0, 12)) / 100
                Logger.Info("MONTO reversado:" + _Monto.ToString)
            Else
                If Usar95 Then
                    Logger.Warn("Monto Reversado No informado (95), se toma 0")
                End If
                _Monto = 0

            End If
        Catch ex As Exception
            Logger.Error("ERROR obteniendo monto Reverso (campo 95) " + ex.Message)
        End Try
        Return _Monto
    End Function

    Public Function Field14_ObtenerVto(ByVal mensaje As Iso8583messagePOS) As Date
        Dim _fecha As Date
        If mensaje.Fields.Contains(14) Then
            Try

                Dim s As String = mensaje.Fields(14).ToString
                _fecha = DateSerial(CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)), 28)

            Catch ex As Exception
                _fecha = Vtomax

            End Try

        Else
            _fecha = Vtomax
        End If
        Logger.Info("FECHA vto:" + _fecha.ToString)
        Return _fecha
    End Function
    Public Function Field17_ObtenerFechaNegocio(ByRef mensaje As Iso8583messagePOS) As Date
        '17	cap-dat	Fecha de negocios de la transacción
        'Campo 17	mmdd 9(4)
        'Como el anio no viene, ojo suponerlo en base a la fecha actual
        Dim s As String = mensaje.Fields(17).ToString
        Dim Anio As Integer
        Anio = Year(FechaActual)
        Dim _fechaNegocio As Date = DateSerial(Anio, CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)))
        ' ahora verificamos que la fecha de negocio obtenida no este en el futuro, lo que significaria que supusimos
        ' mal el anio
        If _fechaNegocio.Subtract(FechaActual).TotalDays > 100 Then
            _fechaNegocio = DateSerial(Anio - 1, CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)))
        End If
        Logger.Info("FECHA NEGOCIO:" + _fechaNegocio.ToString)
        Return _fechaNegocio
    End Function
    Public Function Field12y13_ObtenerFechaHora(ByRef mensaje As Iso8583messagePOS) As Date
        Dim s As String = mensaje.Fields(13).ToString
        Dim Anio As Integer
        Anio = Year(FechaActual)
        Dim _fechatran As Date = DateSerial(Anio, CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)))
        ' ahora verificamos que la fecha de negocio obtenida no este en el futuro, lo que significaria que supusimos
        ' mal el anio
        If _fechatran.Subtract(FechaActual).TotalDays > 100 Then
            _fechatran = DateSerial(Anio - 1, CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)))
        End If
        ' agregar la hora desde el campo 12
        s = mensaje.Fields(12).ToString
        _fechatran.AddHours(CInt(s.Substring(0, 2)))
        _fechatran.AddMinutes(CInt(s.Substring(2, 2)))
        _fechatran.AddSeconds(CInt(s.Substring(4, 2)))
        Logger.Info("FECHA/HORA TRANS:" + _fechatran.ToString)
        Return _fechatran
    End Function

    Public Function Field37_obtenerRetRefNumber(ByRef mensaje As Iso8583messagePOS) As String
        Return "999"
        'Return mensaje.Fields(37).ToString 
    End Function

    Public Function Field49_ObtenerMoneda(ByRef mensaje As Iso8583messagePOS) As E_Monedas
        Return CType(CInt(mensaje.Fields(49).ToString), E_Monedas)
    End Function

    Public Function Field102_ObtenerCuenta(ByRef mensaje As Iso8583messagePOS) As Integer
        If mensaje.Fields.Contains(102) Then
            Try
                Return CInt(mensaje.Fields(102).Value)

            Catch ex As Exception
                Return 0
            End Try
        Else
            Return 0
        End If
    End Function
    ' obtiene el nro de tarjeta del campo 2 o de alguno de los tracks (campo 35,track 2), por si no viene en el campo2 (posnet)
    Public Function Field2or35_ObtenerTarjeta(ByRef mensaje As Iso8583messagePOS) As Int64
        Dim _tarjeta As Int64
        Try

            If mensaje.Fields.Contains(2) Then
                _tarjeta = CLng(mensaje.Fields(2).ToString)
            ElseIf mensaje.Fields.Contains(35) Then

                If mensaje.Fields(35).ToString.Substring(16, 1) = "=" Then
                    _tarjeta = CLng(mensaje.Fields(35).ToString.Substring(0, 16))
                Else
                    _tarjeta = CLng(mensaje.Fields(35).ToString.Substring(0, 18))
                    Logger.Debug("Tarjeta 18 digitos " + _tarjeta.ToString)
                End If
            End If
            Logger.Info("PAN:" + _tarjeta.ToString)
        Catch ex As Exception
            Logger.Error("Error determinando PAN:" + _tarjeta.ToString + " " + ex.Message + " " + ex.StackTrace)
        End Try

        Return _tarjeta
    End Function

    Public Function Field35_Obtenertrack2(ByRef mensaje As Iso8583messagePOS) As String
        Try

            If mensaje.Fields.Contains(35) Then
                Return mensaje.Fields(35).ToString
            Else
                Return ""
            End If
        Catch ex As Exception
            Logger.Error("Error determinando track 2:" + ex.Message + " " + ex.StackTrace)
            Return ""
        End Try
    End Function


    Public Function Field45_Obtenertrack1(ByRef mensaje As Iso8583messagePOS) As String
        Try

            If mensaje.Fields.Contains(45) Then
                Return mensaje.Fields(45).ToString
            Else
                Return ""
            End If
        Catch ex As Exception
            Logger.Error("Error determinando track 1:" + ex.Message + " " + ex.StackTrace)
            Return ""
        End Try
    End Function

    Public Function FechaActual() As DateTime
        ' esta es la fecha que usamos para todos los mensajes
        Return DateTime.Now
    End Function
#End Region


    Protected Overrides Sub Finalize()
        Close()
        MyBase.Finalize()
    End Sub
    Private Sub Close()
        TimerEco.Enabled = False
    End Sub
    Public Overridable Sub Dispose() Implements IDisposable.Dispose
        _Server.Listener.Stop()
        _Server = Nothing
    End Sub
    Public Function Logger() As ILog
        Return LogManager.GetLogger(MethodBase.GetCurrentMethod.DeclaringType)
    End Function

    Private Sub _Server_Connected(ByVal sender As Object, ByVal e As Trx.Messaging.FlowControl.ServerPeerConnectedEventArgs) Handles _Server.Connected
        Logger.Info("Conectado:" + e.Peer.ToString + " " + e.Peer.Name)
    End Sub

    Private Sub _Server_Disconnected(ByVal sender As Object, ByVal e As Trx.Messaging.FlowControl.ServerPeerDisconnectedEventArgs) Handles _Server.Disconnected
        Logger.Info("Desconectado:" + e.Peer.ToString + " " + e.Peer.Name)
    End Sub

    Private Sub _Escuchador_Connected(ByVal sender As Object, ByVal e As Trx.Messaging.FlowControl.ListenerConnectedEventArgs) Handles _Escuchador.Connected
        Logger.Info("Conectado channel :" + e.Channel.Name)

    End Sub

    Private Sub _Escuchador_ConnectionRequest(ByVal sender As Object, ByVal e As Trx.Messaging.FlowControl.ListenerConnectionRequestEventArgs) Handles _Escuchador.ConnectionRequest
        Logger.Info("Requerimiento de conexion :" + e.ConnectionInfo.ToString)

    End Sub

    Private Sub _Escuchador_Error(ByVal sender As Object, ByVal e As Trx.Utilities.ErrorEventArgs) Handles _Escuchador.Error
        Logger.Warn("error en capa tcp:" + e.Exception.ToString)
    End Sub


    Private Sub TimerEco_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles TimerEco.Elapsed

        TimerEco.Enabled = False
        If Not HostAlive() Then

            Noeco = Noeco + 1
            Dim avisoM As Integer
            Select Case Noeco
                Case 1
                    avisoM = 0
                Case 2
                    avisoM = 2
                Case 3
                    avisoM = 10
                Case Is > 3
                    avisoM = Noeco * 10
            End Select
            If Now.Subtract(Last0800).Minutes >= avisoM And Last0800 > Date.MinValue Then
                Logger.Error("ECO: No se ha recibido ECO desde las " + Last0800.ToString)
                LastNoEco = Now
                If Noeco = 1 Then RaiseEvent HostDown(Last0800)
            End If

        Else
            If LastNoEco < Last0800 And LastNoEco <> Date.MaxValue Then
                Logger.Warn("ECO: la conexion ha sido restaurada a las " + Last0800.ToString)
                RaiseEvent HostUp()
            End If
            LastNoEco = Date.MaxValue
            Noeco = 0

        End If
        TimerEco.Enabled = True
    End Sub

    Private Function HostAlive() As Boolean
        Return True
    End Function

End Class
