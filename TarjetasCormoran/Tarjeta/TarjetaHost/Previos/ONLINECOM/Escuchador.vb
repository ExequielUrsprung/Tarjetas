Imports Trx.Messaging.FlowControl
Imports Trx.Messaging.Channels
Imports Trx.Messaging.Iso8583
Imports Trx.Messaging

Imports log4net
Imports System.Reflection


#Region "Enums"
Public Enum E_ProcCode
    ' Utilizar el mismo nombre en ambos proveedores para que en la consulta se vean juntos
    ' Link
    AdelantoEnEfectivo_Link = 101300
    AdelantoEnEfectivo_Link1 = 13000
    PagoAutomaticoServicio1_Link = 883100
    PagoAutomaticoServicio2_Link = 813100
    PagoAutomaticoServicio3_Link = 813000
    AdhesionPAS_Link = 890000
    AdhesionPASBaja_Link = 850000
    AdhesionPASConsulta_link = 820000
    AdhesionPASConsulta1_link = 840000
    AdhesionPASConsulta_ultpagos_link = 830000
    CompraPulsos_Link = 173000
    CompraPulsos_Link1 = 171300
    ConsultaAdelanto_Link = 313000
    ConsultaSaldo_link = 313100
    CambioPin_Link = 320000
    HomeBanking_link = 340000
    PagoResumen_link = 500030
    PagoResumen_link1 = 500031
    ' banelco
    AdelantoEnEfectivo_Banelco = 13000 '103100
    ConsultaAdelanto_Banelco = 313000
    ConsultaSaldo_Banelco = 310000
    ' posnet
    Compra_posnet = 30
    AnulacionCompra_Posnet = 20030
    Devolucion_Posnet = 200030
    AnulacionDevolucion_Posnet = 220030

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
    ReversoTransaccionTR = 420
    ReenvioReversoTR = 421
    TransaccionSAF = 220
    ReenvioTransaccionSAF = 221
    MensajeRechazado = 900

    Respuesta_Control = 810
    Respuesta_TransaccionTR = 210
    Respuesta_ReversoTransaccionTR = 430
    Respuesta_ReenvioReversoTR = 431
    Respuesta_TransaccionSAF = 230
    Respuesta_ReenvioTransaccionSAF = 231

End Enum
Public Enum E_Fields
    Field7TransDateTime = 7
    Field11Trace = 11
End Enum
Public Enum E_Implementaciones
    Link = 0
    Banelco = 1
    PosnetComercio = 2
    PosnetSalud = 3
End Enum
Public Enum E_CodigosRespuesta ' luego se pasan a string
    OK = 0
    Retener_tarjeta = 7
    Nro_de_cuotas_invalidas = 17
    Fondos_insuficientes = 51
    Tarjeta_vencida = 54
    Pin_invalido = 55
    Excede_disponible_diario = 61
    Tarjeta_restringida = 62
    Intentos_de_pin_excedidos = 75
    Cuenta_invalida = 76
    Error_del_sistema = 88
    ErrorVarios = 56
    Fondos_insuficiente_menor0 = 58
    Fondos_insuficientes_mayor0 = 59
    ' especificos de Link
    DECLINACI?N_EXTERNA = 5
    Transaccion_Invalida = 12
    MONTO_INVALIDO = 13
    TARJETA_INVALIDA = 14 ' , NO SE HALLA EN EL CARDHOLDER-AUTHORIZATION-FILE
    TRACK1_distinto_de_TRACK2 = 24
    DIA_DE_NEGOCIOS_ERRONEO = 30
    TARJETA_VENCIDA_LINK = 33 ' CAPTURA DE LA TARJETA
    USO_NO_AUTORIZADO_link = 36 ' .CAPTURA DE LA TARJETA.
    Intentos_de_pin_excedidos_LINK = 38 '  DE INGRESO DE PIN EXCEDIDO. CAPTURA DE LA TARJETA.
    EXCEDE_CANTIDAD_MAXIMA_DE_EXTRACCIONES_MENSUALES = 45
    ERROR_EN_EL_CAMBIO_DE_PIN = 46
    EXCEDE_CANTIDAD_MaXIMA_EXTRACCIONES_DIARIAS = 65

    ErrorExplicado = 98
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
    Cancelaci?n_del_usuario = 8
    Error_de_maquina = 10
    Transaccion_sospechosa = 20
    ReversoParcial = 32
    TimeOut_del_HOST = 68
    Extendido = 100
End Enum
Public Enum E_eventosControl
    Logon = 1
    Logoff = 2
    EchoTest = 301
    EchotestInterno = 302
End Enum
Public Enum e_eventosRespuesta
    Aprobado = 0
    Denegado = 1
    HostDown = 90
    ControlInvalido = 1
End Enum
Public Enum E_ModoIngreso
    POS_track2_no = 2
    POS_track2_si = 90
    Ecommerce = 81
    Desconocido = 0
    Manual = 1
    ' 03 = Lector de Barras
    '04 = Lector de Caracteres ?pticos (OCR)
    '05 = Integrated circuit card ( Chip )
    '06-80 = Reservado por ISO para uso futuro

    '82-89 = Reservado para uso privado
    '90 = Lectura de Banda Magn?tica (Track2 Completo)
    '91-99 = Reservado para uso privado
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

#Region "Eventos Transacciones Nexo"

    ' esto no depende de la implementacion
    Public Event TransaccionNEXOAdelanto(ByRef parametros As ONLINECOM.ParametrosCom)

    Public Event TransaccionNEXOAdelantoReverso(ByRef parametros As ONLINECOM.ParametrosCom)

    Public Event TransaccionNEXOConsulta(ByRef parametros As ONLINECOM.ParametrosCom)

    Public Event TransaccionNEXOPAS(ByRef parametros As ONLINECOM.ParametrosCom)
    Public Event TransaccionNEXOCambioPin(ByRef parametros As ONLINECOM.ParametrosCom)
    Public Event TransaccionNEXOHomeBanking(ByRef parametros As ONLINECOM.ParametrosCom)
    Public Event TransaccionNEXONoImplementada(ByRef parametros As ONLINECOM.ParametrosCom)
    Public Event TransaccionRespuesta(ByRef msg As ONLINECOM.Iso8583messageNexo)

    Public Event TransaccionNEXOCredito(ByRef parametros As ONLINECOM.ParametrosCom, ByVal CodigoTransaccion As E_ProcCode)
    Public Event TransaccionNEXOCreditoAnulacion(ByRef parametros As ONLINECOM.ParametrosCom)
    Public Event TransaccionNEXOCreditoReverso(ByRef parametros As ONLINECOM.ParametrosCom)
    Public Event TransaccionNEXOCreditoReversoAnulacion(ByRef parametros As ONLINECOM.ParametrosCom)
    Public Event TransaccionNexoSAFErronea(ByVal e As ONLINECOM.ParametrosCom, ByRef msg As ONLINECOM.Iso8583messageNexo)
    Public Event Control(ByVal pEvento As E_eventosControl, ByRef pRespuesta As ONLINECOM.e_eventosRespuesta)
    Public Event LogOn()
    Public Event Logoff()
    Public Event BeforeSend(ByRef msg As ONLINECOM.Iso8583messageNexo)
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
        Logger.Info("Arrancando escuchador " + NombreInterface + " en " + InterfaceIp.ToString + " Puerto:" + Puerto.ToString)
        Logger.Info("Implementacion " + ToString())

        '' levanto el numerador de TRACE
        Logger.Info("Iniciando Contadores")
        Numerador = New VolatileStanSequencer

        ' levantar comunicaciones
        _Escuchador = New TcpListener(Puerto)
        _Escuchador.LocalInterface = InterfaceIp

        _Server = New Server("Server " + pImplementacion.ToString, _
                                _Escuchador, _
                                New BasicServerPeerManager())

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
        Dim mensajeRecibido As New Iso8583messageNexo(Mr, Implementacion, messageSource)
        If Not mensajeRecibido Is Nothing Then
            ' ----------------- Respuesta de acuerdo al tipo de mensaje
            Logger.Info("Mensaje recibido:" + mensajeRecibido.ToString)
            Select Case mensajeRecibido.MessageTypeIdentifier
                Case E_TiposMensajes.TransaccionTR
                    ' transacciones Online
                    auditarMensaje(mensajeRecibido)
                    ProcesarTransaccion(mensajeRecibido, e_ModosTransaccion.Normal)
                Case E_TiposMensajes.Control
                    ' login/logout/echo

                    AuditarMensaje(mensajeRecibido)
                    ProcesarControl(mensajeRecibido)
                Case E_TiposMensajes.TransaccionSAF, E_TiposMensajes.ReenvioTransaccionSAF
                    ' Store and Forward
                    auditarMensaje(mensajeRecibido)
                    ProcesarTransaccion(mensajeRecibido, e_ModosTransaccion.SAF)
                Case E_TiposMensajes.ReenvioReversoTR, E_TiposMensajes.ReversoTransaccionTR, E_TiposMensajes.Respuesta_ReversoTransaccionTR, E_TiposMensajes.Respuesta_ReenvioReversoTR
                    'reversos
                    auditarMensaje(mensajeRecibido)
                    ProcesarTransaccion(mensajeRecibido, e_ModosTransaccion.Reverso)
                Case E_TiposMensajes.Respuesta_Control, _
                  E_TiposMensajes.Respuesta_ReenvioReversoTR, _
                  E_TiposMensajes.Respuesta_ReenvioTransaccionSAF, _
                  E_TiposMensajes.Respuesta_ReenvioTransaccionSAF, _
                  E_TiposMensajes.Respuesta_ReversoTransaccionTR, _
                  E_TiposMensajes.Respuesta_TransaccionSAF, _
                  E_TiposMensajes.Respuesta_TransaccionTR

                    auditarMensaje(mensajeRecibido)
                    ProcesarRespuesta(mensajeRecibido)
                Case Else
                    auditarMensaje(mensajeRecibido)
                    Responder_MensajeNoImplementado(mensajeRecibido, messageSource)
                    ' tipo de mensaje no previsto
            End Select
        Else
            Logger.Warn("Mensaje no valido recibido_???")
        End If
        ' avisarnos si demora mucho, para ver que pasa
        Dim Demora As TimeSpan = Now.Subtract(Hora)
        If Demora.TotalSeconds > My.Settings.DemoraMaxima Then
            Logger.Warn("Respuesta demor? " + Demora.ToString + " (demasiado)")

        End If
    End Sub
#Region "auditoria"
    Public Sub AuditarMensaje(ByVal m As ONLINECOM.Iso8583messageNexo)
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
    'Public Sub ProcesarReversos(ByVal mensaje As Iso8583messageNexo)
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
    '        '                        _CodigoRespuesta = TransaccionNExo(e_transaccionesNexo.AdelantoEfectivo, Tarjeta, MontoTransaccion, _
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
    ' En el caso de adelanto o consulta de cuenta de cr?dito en cuotas o selecci?n de m?ltiples cuentas solo 
    ' para los ON Line puro, agregar el campo 126.
    'Campo 3	Ver tabla de c?digos.
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
    'Posiciones 01-22  =  Nombre de la  Instituci?n due?a del ATM.
    'Posiciones 23-35  =  Localidad  donde  se  encuentra ubicado el ATM.
    'Posiciones 36-38  =  C?digo  de  Provincia  donde se encuentra ubicado el ATM.
    'Posiciones 39-40  =  C?digo de Pa?s donde se encuentra ubicado el ATM.
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
    'Posiciones 04-07  =  N?mero de  Instituci?n due?a del ATM.
    'Posiciones 08-11  =  C?digo que identifica a la RED due?a del ATM.
    'Posiciones 12-15  =  Diferencia horaria con la RED. El valor a informar es `+000`.
    'Campo 61	El formato es:
    'Posiciones 01-03  =  Indicador de longitud. El valor a informar es `013`.
    'Posiciones 04-07  =  N?mero de la Instituci?n emisora de la tarjeta.
    'Posiciones 08-11  =  C?digo que identifica a la RED. El valor a informar es`PRO1`.
    'Posiciones 12-15  =  Tipos de cuentas involucradas en la transacci?n.
    'Posiciones 16-16  =  El valor a informar es `P`.
    'Campo 62	Es condicional.
    'El formato es:
    'Posiciones 01-03  =  Indicador de longitud. El valor a informar es `025`.
    'Posiciones 04-05  =  Tipo de terminal en la que se realiza la transacci?n.
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
    'Posiciones 33 - 36 Regi?n del cajero.
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
    'Posiciones 04-07  =  Indica la regi?n a la cu?l pertenece el ATM, para transacciones en d?lares. Los valores posibles son:
    '`0000`  =  CAPITAL Y GRAN BUENOS AIRES.
    '`0001`  =  USHUAIA.
    '`0002`  =  RESTO INTERIOR DEL PAIS.
    '`0005`  =  URUGUAY / BRASIL / CIRRUS.
    'Los cuatro campos siguientes deber?n contener los valores informados en la consulta de tipo de cambio,  si la misma fue la transacci?n anterior y dentro del ?ltimo minuto. En caso contrario, informar CEROS.
    'Posiciones 08-15  =  Tipo de cambio comprador U$S / $.El formato es 9(05)v999.      
    'Posiciones 16-23  =  Tipo de cambio vendedor U$S / $. El formato es 9(05)v999.
    'Posiciones 24-31  =  Tipo de cambio,  entre la moneda informada en el campo 49 y Pesos. El formato es 9(05)v999.
    'Posiciones 32-39  =  Tipo de cambio,  entre la moneda informada en el campo 49 y D?lares. El formato es 9(05)v999.
    'Posiciones 40-46  =  Se deber? informar CEROS.

    'De acuerdo al c?digo de movimiento, el formato del campo 48 es el siguiente:
    'Transacciones originadas en ATM.
    'LONGITUD 	Longitud del campo 48. El valor a informar es `044`.	9(3)
    'ATM		
    '	SHRG-GRP	Sharing group	X(24)
    '	TERM-TRAN-ALLOWED	Indica a que nivel se permite una transacci?n para un cliente NOT-ON-US en dicha terminal	9(1)
    '	TERM-ST	Estado en el que reside la terminal. No aplicable.	9(2)
    '	TERM-CNTY	C?digo ANSI del pa?s en el que reside la terminal. No utilizado.	9(3)
    '	TERM-CNTRY	C?digo ISO del pa?s en el que reside la terminal. No utilizado.	9(3)
    '	TERM-RTE-GRP	Grupo de ruteo. No utilizado.	9(11)
    'Transacciones originadas en POS.
    'LONGITUD	Longitud del campo 48. El valor a informar es `079`.	9(3)
    'POS		
    'RETL-ID	C?digo de identificaci?n del comercio donde se origina la transacci?n.	X(19)
    'DC		
    '	CUOTAS	Cantidad de cuotas en compras realizadas con Maestro / Electron.	X(2)
    '	FILLER		X(2)
    'RETL-REGN	Regi?n a la que pertenece el comercio.	X(4)
    'SHRG-GRP	Sharing group	X(24)
    'TERM-TRAN-ALLOWED	Indica a que nivel se permite una transacci?n para un cliente NOT-ON-US en dicha terminal	9(1)
    'TERM-ST	Estado en el que reside la terminal. No aplicable.	9(2)
    'TERM-CNTY	C?digo ANSI del pa?s en el que reside la terminal. No utilizado.	9(3)
    'TERM-CNTRY	C?digo ISO del pa?s en el que reside la terminal. No utilizado.	9(3)
    'TERM-RTE-GRP	Grupo de ruteo. No utilizado.	9(11)
    'ORIG-NUM	En caso de ser una devoluci?n Maestro / Electron, contiene el n?mero de secuencia de la transacci?n original, de lo contrario `0000`	9(4)
    'ORIG-DATE	En caso de ser una devoluci?n Maestro / Electron, contiene la fecha de la transacci?n original, de lo contrario `0000`.	9(4)
    'De acuerdo al c?digo de movimiento, el formato del campo 54 es el siguiente:
    'DEPOSITOS (C?digos 2100xx / 2200xx)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'FILLER	Se deber? informar BLANCOS.	X(12)
    'TIPO-DEP	Tipo de dep?sito. Los valores posibles son:
    ' `E`  =  EFECTIVO en $ y `U` en U$S
    ' `C`  =  CHEQUE en $ y `W` en U$S.	X(1)
    'FILLER	Se deber? informar BLANCOS.	X(10)
    'PAGOS CON D?BITO EN CUENTA A TRAV?S DE TERMINALES DE CAJA (C?digos 10XXYY)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'FILLER	Se deber? informar BLANCOS.	X(12)
    'TIPO-DEP	Tipo de dep?sito. El valor posible es:
    ' `X` Pagos con d?bito en cuenta a trav?s de terminales de caja
    ' 	X(1)
    'FILLER	Se deber? informar BLANCOS.	X(10)
    'PAGO DE SERVICIOS CON ORDEN DE DEBITO (C?digos 90xx00)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'FILLER	Se deber? informar BLANCOS.	X(13)
    'CANT-COMPR	Cantidad de comprobantes.	9(2)
    'FILLER	Se deber? informar BLANCOS.	X(8)
    'CAMBIO DE PIN (C?digo 32) 
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'FILLER	Se deber? informar BLANCOS.	X(15)
    'NUEVO-PIN	PIN-NUEVO-1, PIN-NUEVO-2, alineado a izquierda y relleno con BLANCOS.	X(8)
    'CAMBIO DE PIN ENCRIPTADO (C?digo 32)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'PIN-ENCRIPTADO	Nuevo n?mero de PIN-Offset.	X(16)
    'FILLER	Se deber? informar BLANCOS.	X(7)
    'PAGO AUTOMATICO DE SERVICIOS	(C?digos 80/ 81/ 82/ 83/ 84/ 85/ 87/ 88/ 89)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'CDE-ID-CUSTOM	CODIGO DE IDENTIFICACION DEL CLIENTE	
    '	CDE-ENTE	C?digo de Ente	9(3)
    '	CDE-CUSTOM	C?digo de cliente	X(19)
    'NRO-OPC	Opci?n que corresponde a la selecci?n de la deuda.	9(1)
    'COMPRA CON CASH-BACK (C?digos 76/ 77)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'TRANSACTION-AMOUNT	Es el importe del retiro en efectivo.	9(10)v99
    'FILLER	Se deber? informar BLANCOS.	X(11)
    'TRANSFERENCIA MINORISTA (1B)
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'CBU	N?mero de clave bancaria uniforme.	X(22)
    'FILLER	Se deber? informar BLANCOS.	X(98)
    'RESTANTES CODIGOS
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `023`.	9(3)
    'FILLER	Se deber? informar BLANCOS.	X(23)

    'De acuerdo al c?digo de movimiento, el formato del campo 55 es el siguiente:
    'INTERBANCARIAS (C?digos 09/ 29/ 39)
    'LONGITUD	Indicador de longitud. El valor a informar es `120`.	9(3)
    'TRACK2
    '	Contiene datos del Track 2 de la tarjeta alineado a izquierda y rellenos con blancos.	X(40)
    'CA	C?digo asociado	X(2)
    'FR-ACCT	N?mero de cuenta desde	
    '	FIID	Instituci?n Emisora de la cuenta	X(4)
    '	TYP	Tipo de cuenta	X(2)
    '	ACCT-NUM	N?mero de cuenta desde	X(19)
    'TO-ACCT		
    '	FIID	Instituci?n Emisora de la cuenta	X(4)
    '	TYP	Tipo de cuenta	X(2)
    '	ACCT-NUM	N?mero de cuenta hacia	X(19)
    '	FIID-CPF	Instituci?n a la cual pertenece la cuenta hacia
    'Si se trata de donaciones, este dampo se redefine de la siguiente manera:
    'Ente           X(3)
    'Filler         X(10)	X(13)
    '	RUBRO	Datos del titular	X(15)
    'PAY KEY (C?digo 17)

    'LONGITUD	Indicador de longitud. El valor a informar es `038`.	9(3)
    'COMPRAS		
    'COD-EMPRESA	C?digo de empresa.	X(3)
    'COD-PRODUCTO	C?digo de producto.	X(3)
    'COD-ARTICULO	C?digo de art?culo.	X(32)
    'FCI (C?digos 08/ 28/ 36/ 37/ 38/ 48/ 63/ 68/ A8/ B8/ C8)
    'LONGITUD	Indicador de longitud. El valor a informar es `120`.	9(3)
    'PROD-ID	Identificador de producto.	X(2)
    'SUBPROD-ID	En el caso de las transacciones que necesiten subproducto, como ser subscripci?n, rscate, informaci?n de propaganda, se informar? en este campo.	X(2)
    'SUBPROD-ID-DEST	En el caso de las subscripciones y de los rescates, se deber? informar la descripci?n del nombre del subproducto.	X(2)
    'CURRENCY-CODE	Se informa la moneda del fondo, los valores posibles son : 
    '`032` para pesos o `840` para d?lares. 	X(3)
    'CURRENCY-CODE-DEST	Se informa la moneda del fondo, los valores posibles son : 
    '`032` para pesos o `840` para d?lares. 	X(3)
    'CANT-CP-A-RESCATAR	Solo en el caso del rescate, se informar? la cantidad de cuotas partes a rescatar.	9(9)
    'TXN-TYP-FLG	Para la consulta de un producto (tx 36), como se puede originar desde dos transacciones distintas, este flag indica desde cual de las dos se rigin?:
    '`2` para subscripci?n o `7` para consulta de informaci?n publicitaria.	X(1)
    'LINEA-SELECCION	Item seleccionado por el usuario seg?n lo informa el banco.	X(37)
    'ID-TRANSACCION	C?digo de identificaci?n de transacci?n.	X(20)
    'FLAG-CONFIRMACION	Identifica una transacci?n a confirmar.	X(1)
    'TIPO-IMPRESI?N	Envia S cuando es suscripci?n o R cuando es rescate.	X(1)
    'FILLER	Blancos	X(39)
    'De acuerdo al c?digo de movimiento, el formato del campo 126 es el siguiente:

    'SELECCI?N DE M?LTIPLES CUENTAS
    'LONGITUD	Indicador de longitud. El valor a informar es `010`.	9(3)
    'INDICADOR	Indica si el ATM soporta m?ltiples cuentas o no.
    '    '1? Si
    '    '0? No.	X(1)
    'FILLER		X(9)
#End Region
    Public Sub ProcesarRespuesta(ByRef mensaje As Iso8583messageNexo)
        Logger.Info(" llego una respuesta ..")
        RaiseEvent TransaccionRespuesta(mensaje)
    End Sub

    Public Sub ProcesarNoImplementadas(ByRef mensaje As Iso8583messageNexo, ByRef e As ParametrosCom)
        e.Respuesta = E_CodigosRespuesta.Transaccion_Invalida
        RaiseEvent TransaccionNEXONoImplementada(e)
        Try

            If e.Respuesta = E_CodigosRespuesta.Transaccion_Invalida Then
                If mensaje.Fields(3).ToString = "830000" Then
                    'para que no moleste
                    Logger.Info("Transaccion No Soportada " + mensaje.Fields(3).ToString)

                Else
                    Logger.Warn("Transaccion No Soportada " + mensaje.Fields(3).ToString)
                End If
                Select Case Implementacion
                    Case E_Implementaciones.Link
                        CompletarLink(mensaje, e)
                        'reescribimos el 54
                        If mensaje.Fields.Contains(127) And Not (mensaje.p127 Is Nothing) Then mensaje.Fields.Add(127, mensaje.p127.Tostring)
                    Case E_Implementaciones.PosnetComercio
                        CompletarPosnet(mensaje, e)

                End Select
            End If
        Catch ex As Exception
            Logger.Error("error respondiendo No Implementado")
        End Try

        Responder(mensaje, e.Respuesta)
    End Sub

    Public Overridable Sub ProcesarTransaccion(ByRef mensaje As Iso8583messageNexo, ByVal modo As e_ModosTransaccion)
        Try
            Dim e As New ParametrosCom
            'Los c?digos de transacciones utilizados por el sistema tienen la siguiente estructura: 'nnxxyy', donde:
            Dim CodigoTransaccion As Integer = CInt(mensaje.Fields(3).ToString)
            ' monto de la transaccion
            e.implementacion = mensaje.Implementacion
            e.Monto = Field4_ObtenerMonto(mensaje)
            ' fecha de negocio
            e.FechaNegocio = Field17_ObtenerFechaNegocio(mensaje)
            Logger.Debug("Fecha Negocio:" + e.FechaNegocio.ToShortDateString)
            'fecha de la transaccion, incluyendo HORA!
            e.Fechatransaccion = Field12y13_ObtenerFechaHora(mensaje)
            ' tarjeta
            e.Tarjeta = Field2or35_ObtenerTarjeta(mensaje)
            e.Cuenta = Field102_ObtenerCuenta(mensaje)
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
                e.MontoAimpactar = Field95_ObtenerMontoRealaImpactar(mensaje)
                e.MotivoReverso = Field39_ObtenerRespuestaReverso(mensaje)
            End If

            'el 126 tiene casi toda la info, lo uso a pesar de que no lo envie
            Select Case Implementacion
                Case E_Implementaciones.Banelco, E_Implementaciones.Link
                    If mensaje.Fields.Contains(126) Then
                        ' antes de tirarme de cabeza, verificar el largo
                        e.p126 = mensaje.p126
                    Else
                        e.p126 = New Mensaje126Adelanto(e.implementacion, "", CodigoTransaccion)
                    End If

                Case E_Implementaciones.PosnetComercio
                Case E_Implementaciones.PosnetSalud
            End Select
            ' obtener datos de cajero
            Try
                Select Case Implementacion
                    Case E_Implementaciones.Link, E_Implementaciones.PosnetComercio
                        ' link manda el cajero, posnet los datos del comercio
                        e.Cajero = mensaje.Fields(43).ToString.TrimEnd(" "c) + " " + mensaje.Fields(120).ToString
                    Case E_Implementaciones.Banelco
                        'sacaremos de otra parte
                End Select
            Catch ex As Exception
                Logger.Info("error seteando cajero/pos:" + ex.Message)
            End Try

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
                    Case E_Implementaciones.Link
                        CompletarLink(mensaje, e)
                        Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                    Case E_Implementaciones.Banelco
                        Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                    Case E_Implementaciones.PosnetComercio
                        CompletarPosnet(mensaje, e)
                        Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)

                End Select
            Else
                Select Case Implementacion
                    ' ----------- POSNET COMERCIO
                    Case E_Implementaciones.PosnetComercio
                        e.Vto = Field14_ObtenerVto(mensaje)

                        Select Case CodigoTransaccion
                            Case E_ProcCode.Compra_posnet, _
                                E_ProcCode.Devolucion_Posnet
                                'prepara aca
                                e.NroAutorizacion = Field38_ObtenerNroAutorizacion(mensaje)
                                e.CuentaComercio = Field42_ObtenerCuentaComercio(mensaje)
                                e.texto = "Compra/Dev " + CodigoTransaccion.ToString
                                e.modoIngreso = Field22_ObtenerModoIngreso(mensaje)
                                If mensaje.Fields.Contains(63) Then
                                    e.p63 = New Mensaje63POS(CStr(mensaje.Fields(63).Value))
                                Else
                                    e.p63 = New Mensaje63POS("")
                                End If
                                Try

                                    Dim _CodigoRespuesta As E_CodigosRespuesta = E_CodigosRespuesta.Transaccion_Invalida
                                    Select Case modo
                                        Case e_ModosTransaccion.Normal, e_ModosTransaccion.SAF
                                            RaiseEvent TransaccionNEXOCredito(e, CType(CodigoTransaccion, E_ProcCode))
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
                            Case E_ProcCode.AnulacionCompra_Posnet, _
                              E_ProcCode.AnulacionDevolucion_Posnet
                                'prepara aca
                                e.CuentaComercio = Field42_ObtenerCuentaComercio(mensaje)
                                e.texto = "Compra " + CodigoTransaccion.ToString
                                If mensaje.Fields.Contains(63) Then
                                    e.p63 = New Mensaje63POS(CStr(mensaje.Fields(63).Value))
                                Else
                                    e.p63 = New Mensaje63POS("")
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
                                            RaiseEvent TransaccionNEXOCreditoAnulacion(e)
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
                            Case Else
                                ProcesarNoImplementadas(mensaje, e)
                        End Select
                        ' anulaciones

                        '------------------------ LINK ------------------------
                    Case E_Implementaciones.Link

                        Select Case CodigoTransaccion
                            Case E_ProcCode.HomeBanking_link
                                e.texto = "Home Banking"
                                Try
                                    Dim _CodigoRespuesta As E_CodigosRespuesta = E_CodigosRespuesta.OK
                                    Select Case modo
                                        Case e_ModosTransaccion.Normal, e_ModosTransaccion.SAF
                                            RaiseEvent TransaccionNEXOHomeBanking(e)
                                            CompletarLink(mensaje, e)

                                            Dim p54v As New Mensaje54varios(e.p126)
                                            mensaje.Fields.Add(54, p54v.Tostring)
                                            mensaje.Fields.Add(127, mensaje.p127.Tostring)
                                            ChequearModoSAF(e, mensaje)
                                            Responder(mensaje, e.Respuesta)
                                        Case e_ModosTransaccion.Reverso
                                            Logger.Warn("REverso de Home Banking:")
                                            'RaiseEvent TransaccionNEXOAdelantoReverso(e)
                                            ReversoLink(mensaje, e)
                                    End Select
                                    '  Responder(mensaje, e.Respuesta)
                                Catch ex As Exception
                                    Logger.Error("Error en el parseo  en Home Banking " + ex.Message + " ")
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try


                            Case E_ProcCode.CambioPin_Link
                                e.texto = "Cambio PIN"
                                Try

                                    Dim _CodigoRespuesta As E_CodigosRespuesta = E_CodigosRespuesta.OK
                                    Select Case modo
                                        Case e_ModosTransaccion.Normal, e_ModosTransaccion.SAF
                                            Try
                                                e.pIN = mensaje.Fields(54).ToString
                                            Catch ex As Exception
                                                Logger.Warn("ERROR SETEANDO PIN")
                                            End Try
                                            RaiseEvent TransaccionNEXOCambioPin(e)
                                            CompletarLink(mensaje, e)

                                            Dim p54v As New Mensaje54varios(e.p126)
                                            mensaje.Fields.Add(54, p54v.Tostring)
                                            mensaje.Fields.Add(127, mensaje.p127.Tostring)
                                            ChequearModoSAF(e, mensaje)
                                            Responder(mensaje, e.Respuesta)
                                        Case e_ModosTransaccion.Reverso
                                            Logger.Warn("REverso de Cambio de PIN:")
                                            'RaiseEvent TransaccionNEXOAdelantoReverso(e)
                                            ReversoLink(mensaje, e)
                                    End Select

                                    '  Responder(mensaje, e.Respuesta)
                                Catch ex As Exception
                                    Logger.Error("Error en el parseo  en Cambio Pin " + ex.Message + " ")
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try

                            Case E_ProcCode.AdelantoEnEfectivo_Link, E_ProcCode.AdelantoEnEfectivo_Link1
                                e.texto = "Adelanto en Efectivo"
                                Try
                                    Logger.Info("Llamando a Capa NEXO")
                                    Dim _CodigoRespuesta As E_CodigosRespuesta = E_CodigosRespuesta.Transaccion_Invalida
                                    Select Case modo
                                        Case e_ModosTransaccion.Normal, e_ModosTransaccion.SAF
                                            RaiseEvent TransaccionNEXOAdelanto(e)
                                            Field44_ponerSaldo(mensaje, e.p126.saldoCampo44)
                                            mensaje.Fields.Remove(43)
                                            mensaje.Fields.Remove(48)
                                            mensaje.Fields.Remove(62)
                                            mensaje.Fields.Remove(63)
                                            mensaje.Fields.Remove(120)
                                            mensaje.Fields.Add(126, e.p126.Tostring)

                                            mensaje.Fields.Add(122, "           ")
                                            mensaje.Fields.Add(123, "000000000000")

                                            Dim p54v As New Mensaje54varios(e.p126)
                                            mensaje.Fields.Add(54, p54v.Tostring)
                                            mensaje.Fields.Add(127, mensaje.p127.Tostring)
                                            Responder(mensaje, e.Respuesta)
                                        Case e_ModosTransaccion.Reverso
                                            ReversoLink(mensaje, e)

                                    End Select
                                    Logger.Info("Retorno de Capa NEXO")

                                Catch ex As Exception
                                    Logger.Error("Error en el parseo del campo 126 en adelantos " + ex.Message + " ")
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try
                                '  consulta saldo resumen tarjeta
                            Case E_ProcCode.ConsultaSaldo_link, E_ProcCode.ConsultaAdelanto_Link
                                e.texto = "Consulta de Adelanto"
                                Try
                                    e.Respuesta = E_CodigosRespuesta.Transaccion_Invalida
                                    '------ procesar adelanto en efectivo
                                    Logger.Info("Llamando a Capa NEXO")
                                    RaiseEvent TransaccionNEXOConsulta(e)
                                    Logger.Info("Retorno de Capa NEXO")

                                    CompletarLink(mensaje, e)

                                    Dim p54v As New Mensaje54varios(e.p126)
                                    mensaje.Fields.Add(54, p54v.Tostring)
                                    mensaje.Fields.Add(127, mensaje.p127.Tostring)
                                    Responder(mensaje, e.Respuesta)
                                Catch ex As Exception
                                    Logger.Error("Error en el parseo del campo 126 en adelantos " + ex.Message + " ")
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try
                            Case E_ProcCode.CompraPulsos_Link, E_ProcCode.CompraPulsos_Link1
                                e.texto = "Compras pulsos "
                                Try
                                    e.Respuesta = E_CodigosRespuesta.Transaccion_Invalida
                                    Select Case modo
                                        Case e_ModosTransaccion.Reverso
                                            ReversoLink(mensaje, e)
                                        Case Else
                                            Logger.Info("Llamando a Capa NEXO")
                                            RaiseEvent TransaccionNEXOPAS(e)
                                            Logger.Info("Retorno de Capa NEXO")

                                            CompletarLink(mensaje, e)

                                            Dim p54v As New Mensaje54varios(e.p126)
                                            mensaje.Fields.Add(54, p54v.Tostring)
                                            mensaje.Fields.Add(127, mensaje.p127.Tostring)
                                            Responder(mensaje, e.Respuesta)
                                    End Select
                                Catch ex As Exception
                                    Logger.Error("Error Proceso Compras pulsos " + ex.Message + " ")
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try
                            Case E_ProcCode.PagoResumen_link, E_ProcCode.PagoResumen_link1
                                e.texto = "Pago Resumen"
                                Try
                                    e.Respuesta = E_CodigosRespuesta.Transaccion_Invalida
                                    '------ procesar adelanto en efectivo
                                    Select Case modo
                                        Case e_ModosTransaccion.Reverso
                                            ReversoLink(mensaje, e)
                                        Case Else
                                            Logger.Info("Llamando a Capa NEXO")
                                            RaiseEvent TransaccionNEXOPAS(e)
                                            Logger.Info("Retorno de Capa NEXO")

                                            CompletarLink(mensaje, e)

                                            Dim p54v As New Mensaje54varios(e.p126)
                                            mensaje.Fields.Add(54, p54v.Tostring)
                                            mensaje.Fields.Add(127, mensaje.p127.Tostring)
                                            Responder(mensaje, e.Respuesta)
                                    End Select
                                Catch ex As Exception
                                    Logger.Error("Error en el parseo del campo 126 en adelantos " + ex.Message + " ")
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try
                            Case E_ProcCode.PagoAutomaticoServicio1_Link, _
                                E_ProcCode.PagoAutomaticoServicio2_Link, _
                                E_ProcCode.PagoAutomaticoServicio3_Link


                                e.texto = "Pago Automatico de Servicio:" + CodigoTransaccion.ToString
                                Try
                                    Select Case modo
                                        Case e_ModosTransaccion.Reverso
                                            ReversoLink(mensaje, e)
                                        Case Else
                                            If mensaje.Fields.Contains(54) Then
                                                ' antes de tirarme de cabeza, verificar el largo
                                                e.p54 = mensaje.p54
                                                e.Respuesta = E_CodigosRespuesta.Error_del_sistema
                                                If CodigoTransaccion = E_ProcCode.AdhesionPAS_Link And e.Monto > 0 Then
                                                    Logger.Error("Adhesion a PAS con Monto!" + e.Monto.ToString)
                                                Else
                                                    If CodigoTransaccion = E_ProcCode.AdhesionPAS_Link And e.Monto = 0 Then
                                                        Logger.Info("Adhesion Pas procesada! no error")
                                                    End If
                                                    '------ procesar adelanto en efectivo
                                                    Logger.Info("Llamando a Capa NEXO")
                                                    RaiseEvent TransaccionNEXOPAS(e)
                                                    Logger.Info("Retorno de Capa NEXO")
                                                    ' Comenzar a responder
                                                End If
                                                CompletarLink(mensaje, e)
                                                'reescribimos el 54
                                                mensaje.Fields.Add(54, e.p54.Tostring)
                                                mensaje.Fields.Add(127, mensaje.p127.Tostring)
                                                Responder(mensaje, e.Respuesta)

                                            Else
                                                Logger.Error("Mensaje con PAS SIN campo 54")
                                                Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                            End If
                                    End Select
                                Catch ex As Exception
                                    Logger.Error("Error en el parseo del campo 54 en PAS " + ex.Message + " ")
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try
                            Case E_ProcCode.AdhesionPAS_Link, _
                                 E_ProcCode.AdhesionPASBaja_Link, _
                                 E_ProcCode.AdhesionPASConsulta_link, _
                                 E_ProcCode.AdhesionPASConsulta1_link


                                e.texto = "PAS:" + CodigoTransaccion.ToString

                                Try
                                    Select Case modo
                                        Case e_ModosTransaccion.Reverso
                                            ReversoLink(mensaje, e)
                                        Case Else
                                            If mensaje.Fields.Contains(54) Then
                                                ' antes de tirarme de cabeza, verificar el largo
                                                e.p54 = mensaje.p54
                                                If e.Monto > 0 Then
                                                    e.Respuesta = E_CodigosRespuesta.Error_del_sistema
                                                    Logger.Error("Adhesion a PAS con Monto!" + e.Monto.ToString)
                                                Else
                                                    e.Respuesta = E_CodigosRespuesta.OK
                                                    Logger.Warn(" Pas procesada" + CodigoTransaccion.ToString)

                                                    '------ procesar adelanto en efectivo
                                                    RaiseEvent TransaccionNEXOPAS(e)
                                                    ' Comenzar a responder
                                                End If
                                                CompletarLink(mensaje, e)
                                                'reescribimos el 54
                                                If Not (e.p54 Is Nothing) Then mensaje.Fields.Add(54, e.p54.Tostring)
                                                If Not (mensaje.p127 Is Nothing) Then mensaje.Fields.Add(127, mensaje.p127.Tostring)
                                                Responder(mensaje, e.Respuesta)

                                            Else
                                                Logger.Error("Mensaje con PAS SIN campo 54")
                                                Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                            End If
                                    End Select
                                Catch ex As Exception
                                    Logger.Error("Error en el parseo del campo 54 en PAS " + ex.Message)
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try


                            Case Else
                                ProcesarNoImplementadas(mensaje, e)
                        End Select

                        '--------------------banelco  homologado ----------------------------------------
                    Case E_Implementaciones.Banelco
                        Select Case CodigoTransaccion
                            Case E_ProcCode.AdelantoEnEfectivo_Banelco
                                e.texto = "Adelanto en Efectivo"

                                Try
                                    '------ procesar adelanto en efectivo
                                    Logger.Info("Llamando a Capa NEXO")
                                    Dim _CodigoRespuesta As E_CodigosRespuesta = E_CodigosRespuesta.Transaccion_Invalida
                                    Select Case modo
                                        Case e_ModosTransaccion.Normal
                                            RaiseEvent TransaccionNEXOAdelanto(e)
                                            Field44_ponerSaldo(mensaje, e.p126.saldoCampo44)
                                            mensaje.Fields.Add(126, e.p126.Tostring)
                                        Case e_ModosTransaccion.Reverso
                                            RaiseEvent TransaccionNEXOAdelantoReverso(e)
                                            If mensaje.Fields.Contains(12) Then mensaje.Fields.Remove(12)
                                            If mensaje.Fields.Contains(13) Then mensaje.Fields.Remove(13)
                                            If mensaje.Fields.Contains(17) Then mensaje.Fields.Remove(17)
                                            If mensaje.Fields.Contains(23) Then mensaje.Fields.Remove(23)
                                            If mensaje.Fields.Contains(60) Then mensaje.Fields.Remove(60)
                                        Case e_ModosTransaccion.SAF
                                            Logger.Error("SAF no implementado en Banelco")
                                    End Select
                                    Logger.Info("Retorno de Capa NEXO")
                                    Responder(mensaje, e.Respuesta)

                                Catch ex As Exception
                                    Logger.Error("Error en el parseo del campo 126 en adelantos " + ex.Message + " ")
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try
                                '  consulta saldo resumen tarjeta
                            Case E_ProcCode.ConsultaSaldo_Banelco, E_ProcCode.ConsultaAdelanto_Banelco
                                e.texto = "Consulta de Adelanto"
                                Try
                                    e.Respuesta = E_CodigosRespuesta.Transaccion_Invalida
                                    '------ procesar adelanto en efectivo
                                    Logger.Info("Llamando a Capa NEXO")
                                    RaiseEvent TransaccionNEXOConsulta(e)
                                    Logger.Info("Retorno de Capa NEXO")
                                    Field44_ponerSaldo(mensaje, e.p126.saldoCampo44)
                                    mensaje.Fields.Add(126, e.p126.Tostring)
                                    Responder(mensaje, e.Respuesta)
                                Catch ex As Exception
                                    Logger.Error("Error en el parseo del campo 126 en adelantos " + ex.Message + " ")
                                    Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                                End Try

                            Case Else
                                Logger.Warn("Transaccion Banelco No Soportada " + CodigoTransaccion.ToString)
                                Responder(mensaje, E_CodigosRespuesta.Transaccion_Invalida)
                        End Select
                End Select
            End If
        Catch ex As Exception
            Logger.Error("Error Interpretando Mensaje  " + ex.Message + " " + " " + ex.StackTrace)
        End Try
    End Sub
    Public Sub CompletarPosnet(ByVal mensaje As Iso8583messageNexo, ByVal e As ParametrosCom)
        Try
            ' es la respuesta a esos mensajes
            ChequearModoSAF(e, mensaje)
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


            End Select

            If e.p63 IsNot Nothing Then mensaje.Fields.Add(63, e.p63.ToString)
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
            mensaje.Fields.Remove(52)

            mensaje.Fields.Remove(62)

            mensaje.Fields.Add(102, e.Cuenta.ToString("000000000"))
            mensaje.Fields.Remove(120)
            mensaje.Fields.Remove(123)
            mensaje.Fields.Remove(124)
            mensaje.Fields.Remove(126)
        Catch ex As Exception
            Logger.Error("completando mensaje posnet :" + ex.Message + "  " + ex.StackTrace)
        End Try
    End Sub
    Public Sub CompletarLink(ByRef mensaje As Iso8583messageNexo, ByRef e As ParametrosCom)
        Try
            Field44_ponerSaldo(mensaje, e.p126.saldoCampo44)
        Catch ex As Exception
            Logger.Warn("No se pudo poner saldo 44 en campo 126)")
        End Try
        Try

            mensaje.Fields.Remove(43)
            mensaje.Fields.Remove(48)
            mensaje.Fields.Remove(62)
            mensaje.Fields.Remove(63)
            mensaje.Fields.Remove(120)
            mensaje.Fields.Remove(126)

            mensaje.Fields.Add(122, "           ")
            mensaje.Fields.Add(123, "000000000000")
        Catch ex As Exception
            Logger.Warn("error removiendo algun campo...?")
        End Try

    End Sub
    Public Sub ReversoPosnet(ByRef mensaje As Iso8583messageNexo, ByRef e As ParametrosCom, ByVal ProcCode As Integer)
        If e.MotivoReverso = E_CodigoREspuestaReversos.Transaccion_sospechosa Or e.MotivoReverso = 22 Then
            Logger.Warn("Reverso Sospechoso " + mensaje.Fields(11).ToString)
            e.MontoAimpactar = e.Monto
        End If
        Select Case ProcCode
            Case E_ProcCode.AnulacionCompra_Posnet, E_ProcCode.AnulacionDevolucion_Posnet
                RaiseEvent TransaccionNEXOCreditoReversoAnulacion(e)
            Case Else
                RaiseEvent TransaccionNEXOCreditoReverso(e)

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
        If mensaje.Fields.Contains(38) Then mensaje.Fields.Remove(38)

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
    Public Sub ReversoLink(ByRef mensaje As Iso8583messageNexo, ByRef e As ParametrosCom)
        If e.MotivoReverso = E_CodigoREspuestaReversos.Transaccion_sospechosa Or e.MotivoReverso = 22 Then
            Logger.Warn("Reverso Sospechoso " + mensaje.Fields(11).ToString)
            e.MontoAimpactar = e.Monto
        End If
        RaiseEvent TransaccionNEXOAdelantoReverso(e)
        If mensaje.Fields.Contains(12) Then mensaje.Fields.Remove(12)
        If mensaje.Fields.Contains(13) Then mensaje.Fields.Remove(13)
        If mensaje.Fields.Contains(17) Then mensaje.Fields.Remove(17)
        If mensaje.Fields.Contains(23) Then mensaje.Fields.Remove(23)
        If mensaje.Fields.Contains(60) Then mensaje.Fields.Remove(60)
        Responder(mensaje, e.Respuesta)
    End Sub
    Public Sub ChequearModoSAF(ByRef e As ParametrosCom, ByVal m As Iso8583messageNexo)
        If e.Modo <> e_ModosTransaccion.SAF Then Exit Sub
        If e.Respuesta <> E_CodigosRespuesta.OK Then
            Logger.Warn("Se contesta OK, en lugar de " + e.Respuesta.ToString + " por ser SAF")
            RaiseEvent TransaccionNexoSAFErronea(e, m)
            e.Respuesta = E_CodigosRespuesta.OK

        End If
    End Sub

    Public Sub Responder(ByRef mensaje As Iso8583messageNexo, ByVal pCodigoRespuesta As E_CodigosRespuesta)
        Logger.Info("Respondiendo  Cod Rta:" + pCodigoRespuesta.ToString)
        mensaje.SetResponseMEssageTypeIdentifier()

        '39	resp-cde	C?digo de respuesta que indica el estado del mensaje

        'scar el 52 PIN si viene
        If mensaje.Fields.Contains(52) Then mensaje.Fields.Remove(52)
        Field39_PonerRespuesta(mensaje, pCodigoRespuesta)
        RaiseEvent BeforeSend(mensaje)
        mensaje.Send()

    End Sub

#Region "txt definicion campos"
    'BIT	NOMBRE DEL CAMPO
    '	DESCRIPCION
    '1	secndry-bit-map	Controla la presencia o ausencia de elementos de datos, desde la posici?n 65 a 128. Es un dato en s? mismo y su presencia o ausencia	es controlada por el BIT 1 del campo anterior.
    '3	proc-cde	C?digo de transacci?n
    '4 	tran-amt	Monto de la transacci?n
    '7 	xmit-dat-tim	(TRANSMISSION-DATE-AND-TIME ) Representa la fecha y hora del mensaje
    '11 	trace-num	N?mero que es fijado por el que origina el mensaje y repetido por el receptor del mensaje. Es usado para chequear la respuesta al mensaje original. Puede no ser el mismo durante el transcurso de la transacci?n. Por ejemplo: Un reverso puede no tener el n?mero de la transacci?n original.
    '12 	tran-tim	Es la hora local en que comenz? la transacci?n
    '13	tran-dat	Es la fecha local en que comenz? la transacci?n
    '15	setl-dat	Fecha de negocios de una transacci?n realizada en otra Red. En caso contrario, se deber? informar CEROS
    '32	acq-inst-id	C?digo interno de la Instituci?n pagadora
    '35	track 2 	Contiene los datos del TRACK 2 de la tarjeta, alineado a izquierda y relleno con BLANCOS.
    '37	retrvl-ref-num	N?mero de recibo, alineado a izquierda y relleno con BLANCOS.
    '39	resp-cde	C?digo de respuesta que indica el estado del mensaje
    '41	term-id	(CARD-ACCEPTOR-TERMINAL-ID)N?mero de terminal, alineado a izquierda y relleno con BLANCOS.
    '42	crd-accpt-id-cde	Siempre relleno con BLANCOS. Para otros dispositivos que no sean cajeros autom?ticos, en este campo se informar? 'CIT, lo cual indica que se deber? validar clave telef?nica en lugar de pin de cajero
    '43	crd-accpt-name-loc	Nombre y localidad del due?o del ATM
    '44	resp-data	(ADDITIONAL-RESPONSE-DATA) Datos adicionales del mensaje de respuesta dependientes del tipo de mensaje.
    '48	add-data-prvt 	Datos adicionales de la transacci?n. Depende de donde fue originada la transacci?n si en ATM o POS.
    '49	crncy-cde	C?digo de moneda de la transacci?n
    '52	pin	N?mero de PIN, en forma encriptada
    '54 	add-amts	Datos adicionales dependientes del tipo de mensaje
    '55 	pri-rsrvd1-iso	(CAMPO P55) Datos para transacci?n Interbancarias  o
    '(P55-PAY-DATA) Datos del pago (PAS) o
    '(PRI-RSRVD1-ISO) Cuentas relacionadas a la tarjeta. (Consultas generales) 
    'Depende del tipo de mensaje.
    '60	pri-rsrvd1-prvt	(TERMINAL-DATA) Datos de la terminal que origin? la transacci?n
    '61	pri-rsrvd2-prvt	(CARD-ISSUER-AND-AUTH-DATA) Datos de la Instituci?n emisora de la tarjeta
    '63	pri-rsrvd4-prvt	(PIN-OFFSET) Campo de Pin Offset
    '70	netw-mgmt-cde	(NETWORK-MANAGEMENT-INFORMATION-CODE ) C?digo que es  usado para manejar el  STATUS  de procesamiento ON-LINE entre BASE-24 y un HOST
    '90	orig-info	(ORIGINAL-ELEMENTS) Datos de la transacci?n original dependiente del tipo de mensaje.
    '95	replacement	(REPLACEMENT-AMOUNTS) Monto realmente dispensado en una reversa parcial, dependiente del tipo de mensaje.
    '100	rcv-inst	(RECEIVING-INST-ID-CODE) C?digo interno que identifica la Instituci?n emisora
    '102	acct1	N?mero de cuenta desde
    '103	acct2	N?mero de cuenta hacia
    '120	secndry-rsrvd1-prvt	(TERMINAL-ADDRESS) Direcci?n del ATM dependiente del tipo de mensaje.
    '122	secndry-rsrvd3-prvt	(CARD-ISSUER-IDENT-CODE) C?digo interno de la Instituci?n emisora dependiente del tipo de mensaje.
    '123	secndry-rsrvd4-prvt	(DEPOSIT-CREDIT-AMOUNT) Monto del dep?sito que se suma al saldo disponible dependiente del tipo de mensaje.
    '124	secndry-rsrvd5-prvt	(DEPOSITORY-TYPE) Tipo de depositorio
    '125	secndry-rsrvd6-prvt	(ACCOUNT-INDICATOR) Indicador de cuenta a procesar por el Host,  en una transacci?n que involucre dos cuentas; o bien,
    '(STATEMENT-PRINT-DATA) Statement printer, seg?n el tipo de mensaje.
    '127	secndry-rsrvd8-prvt	(ADDITIONAL-DATA) Datos adicionales de la transacci?n dependiente del tipo de mensaje.
#End Region

#End Region
#Region "Otros Mensajes"
    Public Sub Responder_MensajeNoImplementado(ByRef mensaje As Iso8583messageNexo, ByVal MessageSource As IMessageSource)
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

    Public Sub ProcesarControl(ByRef mensajeRecibido As Iso8583messageNexo)
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

            Dim CodigoControl As Integer = CInt(mensajeRecibido.Fields(70).ToString)
            Select Case CodigoControl
                Case E_eventosControl.Logon, E_eventosControl.Logoff, E_eventosControl.EchoTest ' LOGON
                    Dim Respuesta As e_eventosRespuesta = e_eventosRespuesta.Aprobado
                    If CodigoControl = E_eventosControl.Logon Then Logstatus = True
                    If CodigoControl = E_eventosControl.Logoff Then Logstatus = False
                    RaiseEvent Control(CType(CodigoControl, E_eventosControl), Respuesta)
                    Responder_0800(mensajeRecibido, Respuesta)
                Case E_eventosControl.EchotestInterno
                    Dim Respuesta As e_eventosRespuesta
                    If HostAlive() Then
                        Respuesta = e_eventosRespuesta.Aprobado
                    Else
                        Respuesta = e_eventosRespuesta.HostDown
                    End If
                    RaiseEvent Control(CType(CodigoControl, E_eventosControl), Respuesta)
                    Responder_0800(mensajeRecibido, Respuesta)
                Case Else
                    Logger.Warn("0800 con campo 70 incorrecto:" + mensajeRecibido.Fields(70).ToString)
                    Responder_0800(mensajeRecibido, e_eventosRespuesta.ControlInvalido)
            End Select
        Catch ex As Exception
            Logger.Error("0800 sin campo 70:" + ex.Message + " " + ex.StackTrace)
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

    Public Sub Responder_0800(ByRef mensaje As Iso8583messageNexo, ByVal pCodigoRespuesta As e_eventosRespuesta)
        Logger.Info("****Respondiendo 0800:" + mensaje.Fields(70).ToString)

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
    Public Function Field39_PonerRespuesta(ByRef mensaje As Iso8583messageNexo, ByVal pCodResp As E_CodigosRespuesta) As String
        Dim CodResp As String = CInt(pCodResp).ToString("00")
        mensaje.Fields.Add(39, CodResp)
        Logger.Info("Respuesta " + CodResp)
        Return CodResp
    End Function
    Public Function Field38_PonerAutorizacion(ByRef mensaje As Iso8583messageNexo, ByVal NroAutorizacion As Integer) As String
        Dim NroAut As String = CInt(NroAutorizacion).ToString("000000")
        mensaje.Fields.Add(38, NroAut)
        Logger.Info("Nro Autorizacion " + NroAut)
        Return NroAut
    End Function
    Public Function Field44_ponerSaldo(ByRef mensaje As Iso8583messageNexo, ByVal Saldo As Decimal) As String
        Dim s As String
        Select Case Implementacion
            Case E_Implementaciones.Banelco
                s = "1" + CInt(Saldo * 100).ToString("000000000000") + "000000000000"
            Case E_Implementaciones.Link
                s = "2" + CInt(Saldo * 100).ToString("000000000000") + CInt(Saldo * 100).ToString("000000000000")
        End Select

        mensaje.Fields.Add(44, s)
        Logger.Info("Saldo 44" + s)
        Return s
    End Function

    Public Function Field15_PonerFechaHora(ByRef mensaje As Iso8583messageNexo) As DateTime
        Dim transmissionDate As DateTime = FechaActual()

        mensaje.Fields.Add(15, transmissionDate.ToString("MMdd"))

        Logger.Info(" Date:" + transmissionDate.ToString)

        Return transmissionDate

    End Function
    Public Function Field7_PonerFechaHora(ByRef mensaje As Iso8583messageNexo) As DateTime
        Dim transmissionDate As DateTime = FechaActual()

        mensaje.Fields.Add(E_Fields.Field7TransDateTime, String.Format("{0}{1}", _
         String.Format("{0:00}{1:00}", transmissionDate.Month, transmissionDate.Day), _
         String.Format("{0:00}{1:00}{2:00}", transmissionDate.Hour, _
         transmissionDate.Minute, transmissionDate.Second)))

        Logger.Info("Transmision Date:" + transmissionDate.ToString)

        Return transmissionDate

    End Function

    Public Function Field11_PonerTrace(ByRef mensaje As Iso8583messageNexo, ByRef Numerador As Trx.Messaging.VolatileStanSequencer) As Integer
        Dim s As String = Numerador.Increment().ToString()
        mensaje.Fields.Add(E_Fields.Field11Trace, s)
        Logger.Info("Numerador:" + s)
        Return Numerador.CurrentValue
    End Function

    Public Function Field41_ObtenerIdTerminal(ByRef mensaje As Iso8583messageNexo) As String
        Return mensaje.Fields(41).ToString
    End Function
    Public Function Field38_ObtenerNroAutorizacion(ByRef Mensaje As Iso8583messageNexo) As Integer
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
    Public Function Field42_ObtenerCuentaComercio(ByRef Mensaje As Iso8583messageNexo) As Integer
        Try
            Return CInt(CStr(Mensaje.Fields(42).Value).Replace(" ", ""))
        Catch ex As Exception
            Logger.Info("Error Obteniendo cuenta comercio!" + ex.Message)
            Return 0
        End Try

    End Function
    Public Function Field11_ObtenerTrace(ByRef mensaje As Iso8583messageNexo) As Integer
        Dim trace As Integer = CInt(mensaje.Fields(11).Value)
        Logger.Info("Numerador:" + trace.ToString)
        Return trace
    End Function
    Public Function Field22_ObtenerModoIngreso(ByRef mensaje As Iso8583messageNexo) As Integer
        Dim ModoIngreso As Integer = CInt(CStr(mensaje.Fields(22).Value).Substring(0, 2))

        Logger.Info("modo ingreso:" + ModoIngreso.ToString)
        Return ModoIngreso
    End Function
    Public Function Field4_ObtenerMonto(ByRef mensaje As Iso8583messageNexo) As Decimal
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

    Public Function Field39_ObtenerRespuestaReverso(ByRef mensaje As Iso8583messageNexo) As Integer
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
            End If
        Catch ex As Exception
            Logger.Error("ERROR obteniendo Respuesta Reverso (campo 39) " + ex.Message)
        End Try
        Return _Respuesta
    End Function


    Public Function Field95_ObtenerMontoRealaImpactar(ByRef mensaje As Iso8583messageNexo) As Decimal
        Dim _Monto As Decimal
        '        * el campo 95 se env?a solamente cuando la transacci?n es reversada parcialmente.
        'Para mensajes de reverso (0420/0421) parcial (P-039: ?32?) de extracci?n (c?digo transacci?n: ?01?):
        '1-12 Monto por el que realmente se complet? la transacci?n en la moneda de la operaci?n (P-049) (2 decimales).
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

    Public Function Field14_ObtenerVto(ByVal mensaje As Iso8583messageNexo) As Date
        Dim _fecha As Date
        If mensaje.Fields.Contains(14) Then
            Try

                Dim s As String = mensaje.Fields(14).ToString
                _fecha = DateSerial(CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)), 28)

            Catch ex As Exception
                _fecha = VtoMax

            End Try

        Else
            _fecha = VtoMax
        End If
        Logger.Info("FECHA vto:" + _fecha.ToString)
        Return _fecha
    End Function
    Public Function Field17_ObtenerFechaNegocio(ByRef mensaje As Iso8583messageNexo) As Date
        '17	cap-dat	Fecha de negocios de la transacci?n
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
    Public Function Field12y13_ObtenerFechaHora(ByRef mensaje As Iso8583messageNexo) As Date
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

    Public Function Field37_obtenerRetRefNumber(ByRef mensaje As Iso8583messageNexo) As String
        Return mensaje.Fields(37).ToString
    End Function

    Public Function Field49_ObtenerMoneda(ByRef mensaje As Iso8583messageNexo) As E_Monedas
        Return CType(CInt(mensaje.Fields(49).ToString), E_Monedas)
    End Function

    Public Function Field102_ObtenerCuenta(ByRef mensaje As Iso8583messageNexo) As Integer
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
    Public Function Field2or35_ObtenerTarjeta(ByRef mensaje As Iso8583messageNexo) As Int64
        Dim _tarjeta As Int64
        Try

            If mensaje.Fields.Contains(2) Then
                _tarjeta = CLng(mensaje.Fields(2).ToString)
            ElseIf mensaje.Fields.Contains(35) Then
                ' TODO: Validar bien la posicion del PAN (nro de tarjeta) dentro del trakc 2
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

    Public Function Field35_Obtenertrack2(ByRef mensaje As Iso8583messageNexo) As String
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


    Public Function Field45_Obtenertrack1(ByRef mensaje As Iso8583messageNexo) As String
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

    Public Function HostAlive() As Boolean
        Return Now.Subtract(Last0800).Seconds <= My.Settings.IntervaloEcoWarning
    End Function
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
End Class
