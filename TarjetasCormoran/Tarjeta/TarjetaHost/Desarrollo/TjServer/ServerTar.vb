Option Strict On

Imports log4net
Imports System.Reflection
Imports TjComun.IdaLib
Imports IsoLib
Imports Trx.Messaging.Iso8583
Imports TjServer
Imports System.Text
Imports TransmisorTCP
Imports lib_PEI


Public Class ServerTar

    Dim ListaReqPendientesOnline As New Dictionary(Of String, Req)
    Dim ListaReversosPendientes As New Dictionary(Of String, Req)
    Dim ListaVtaPendientes As New Dictionary(Of String, Req)
    Dim ListaAdvPendientesOnline As New Dictionary(Of String, Req)
    'Dim ListaCierres As New Dictionary(Of String, Req)

    Dim WithEvents timerBackup As New System.Timers.Timer


#Region "Declaraciones"
    'Public Event Mensaje(msj As mensajeServer)
    Public Event Desconectado(hst As String)
    Public Event Conectado(hst As String)

#End Region

    Public ModoDesconectadoTCP As Boolean = False
    Public elnropinguino As String
    Property estEscuchador As Boolean
    Property estEscuchadorIPN As Boolean
    Public versionServerTar As String = "76"
    Public Property estadoSrv As Boolean
    Property generandoVTA As Boolean = False
    'TODO: sacar despues de implementado
    Public EMV As Boolean = False

    Public ReadOnly Property Tipoconfiguracion As String
        Get
            Return Configuracion.TipoConfiguracion
        End Get
    End Property

    Public ReadOnly Property Version_ISOLIB As String
        Get
            Return IsoLib.declaraciones.versionISOLIB
        End Get
    End Property


    Public ReadOnly Property Version_TJCOMUN As String
        Get
            Return TjComun.IdaLib.versionTJCOMUN
        End Get
    End Property

    Public ReadOnly Property Version_TRANSMISORTCP As String
        Get
            Return TransmisorTCP.Serializador.versionTRANSMISORTCP
        End Get
    End Property

    Public ReadOnly Property Version_SERVERTAR As String
        Get
            Return versionServerTar
        End Get
    End Property

    Public ReadOnly Property Version_LIBPEI As String
        Get
            Dim pei As New lib_PEI.PEI
            Return pei.versionLIBPEI
        End Get
    End Property

    Public ReadOnly Property HoraReinicio As String
        Get
            Return Configuracion.HoraReinicio
        End Get
    End Property

    Public ReadOnly Property HoraCierre As String
        Get
            Return Configuracion.HoraCierre
        End Get
    End Property
    Public ReadOnly Property HoraCierre1 As String
        Get
            Return Configuracion.HoraCierre1
        End Get
    End Property
    Public ReadOnly Property DirectorioTrabajo As String
        Get
            Return Configuracion.DirTarjetas
        End Get
    End Property
    Public ReadOnly Property TipoConexion As String
        Get
            Return Configuracion.TipoConexion
        End Get
    End Property


    Private Sub timerBackup_Elapsed() Handles timerBackup.Elapsed
        If Configuracion.TipoConfiguracion = Configuracion.Produccion Then RespaldarMovimientos()
        If My.Computer.Name = "MARCOS-XP" Then
            'RespaldarMovimientos()
        End If
    End Sub


    Public Sub arrancar()
        'If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or My.Computer.Name = "BACKTARJETAS" Then ArrancarTCP()
        'If My.Computer.Name <> "MARCOS-XP" And My.Computer.Name <> "ROBERTINO-P" And My.Computer.Name <> "NAHUEL-O" And My.Computer.Name <> "BACKTARJETAS" Then Chequear_Integridad()

        ArrancarTCP()
        Chequear_Integridad()

        timerBackup.Interval = 30000     ' 5000
        timerBackup.Enabled = True
        timerBackup.Start()

        arrancarConexionTCP()
        ReversarIncompletas()
        ArrancarEscuchadorCajas()
        estadoSrv = True

    End Sub

    Private Sub Chequear_Integridad()
        If My.Computer.Name = "MARCOS-XP" Then
        Else
            Verificar(Configuracion.DirLocal & "\sistema\", "concentrador.exe", "concentrador")
            Verificar(Configuracion.DirLocal & "\sistema\", "tjcomun.dll", "tjcomun")
            Verificar(Configuracion.DirLocal & "\sistema\", "tjserver.dll", "tjserver")
            Verificar(Configuracion.DirLocal & "\sistema\", "isolib.dll", "isolib")
            Verificar(Configuracion.DirLocal & "\sistema\", "transmisortcp.dll", "transmisortcp")
            Verificar(Configuracion.DirLocal & "\sistema\", "lib_pei.dll", "lib_pei")
        End If
    End Sub

    Private Sub Verificar(directorio As String, Nombre_archivo As String, archivo_clave As String)
        Dim encriptador As New TjComun.Encriptador
        If encriptador.clave_encriptada(directorio & Nombre_archivo, "SHA1") = encriptador.LeerEncriptado(directorio & archivo_clave) Then
            Logger.Info("Integridad " & Nombre_archivo & " CORRECTO")
        Else
            Logger.Fatal("Integridad " & Nombre_archivo & " INCORRECTO")
        End If
    End Sub


    Public Sub Parar_Escuchador()
        If Escuchador.Estado Then
            Logger.Warn("Deteniendo escuchador cajas...")
            Escuchador.Estado = False
            estEscuchador = Escuchador.Estado 'estescuchador es para que no muestre mas que recorre los pinguinos, muesta detenido.
        End If
    End Sub

    Public Sub Detener()
        timerBackup.Stop()
        Logger.Warn("Deteniendo Server...")
        Parar_Escuchador()
        detenerEscuchadorCajas()
        detenerConexionTCP()
        estadoSrv = False
    End Sub



    Sub ReversarIncompletas()
        ' VER QUE SE REVERSEN LOS ULTIMOS MOVIMIENTOS.  
        Logger.Info("Buscando movimientos incompletos para reversar.")
        For Each Req In BuscarVentasIncompletas()
            Dim host = Hosts(Req.nombreHost)
            Dim c As Integer = 1
            If Now.Subtract(Req.FechaHoraEnvioMsg).TotalSeconds > 30 Then
                Logger.Info(String.Format("Reversando ... {0}", c))
                host.enviarReverso(Req)
                BorrarDatosCriticos(Req)
                c += 1
            Else
                'Encola en ListaReqPendientesOnline, para reversar en el caso de que se cumplan los 30 segundos.
                AgregarAPendientes(ClaveIDA(Req.nombreHost, Trim(Req.terminal), Req.nroTrace), Req)
            End If
        Next
    End Sub



    Dim nrocierre As Byte
    ''' <summary>
    ''' Cierra todas las terminales que hayan tenido movimientos.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CierreDiario()
        '----------------------------------------------------------------------------------------------
        '--- ACA HACE EL CIERRE DIARIO DE TARJETAS, AHORA LO HACE A LAS 22:15 Y A LAS 3 DE LA MAÑANA   
        '----------------------------------------------------------------------------------------------
        Dim fecha_cierre As Date = Now
        detenerEscuchadorCajas()
        IncrementarNroCierre(fecha_cierre)
        timerBackup.Stop()

        For Each host In Hosts.Values
            If host.Tipo = 7 Or host.Tipo = 8 Then Continue For  'HOST DE PEI, NO LE DA BOLA PORQUE NO HACE CIERRE
            Dim renglones = FiltrarParaCerrar(host.Tipo)
            If renglones.Count = 0 Then
                Logger.Info("No hay terminales con movimientos para cerrar.")
            End If
            Try
                Logger.Info("Haciendo Cierre para Host " + host.Tipo.ToString)
            Catch

            End Try
            For Each ren In renglones
                Dim req As New Req(ren.Key, host.Tipo, fecha_cierre)
                req.CierreCantCompras = ren.Value.CantCompras
                req.CierreMontoCompras = ren.Value.SumaCompras
                req.CierreCantDevoluciones = ren.Value.CantDevoluciones
                req.CierreMontoDevoluciones = ren.Value.SumaDevoluciones
                req.CierreCantAnulaciones = ren.Value.CantAnulaciones
                req.CierreMontoAnulaciones = ren.Value.SumaAnulaciones

                host.Cerrar(req)
                'cerrarnoaprobados
            Next
        Next


        ListadosReportesTarjetas(fecha_cierre)
        ListadosReportesTarjetasPorPing(1, fecha_cierre)
        ListadosReportesTarjetasPorPing(2, fecha_cierre)
        ListadosReportesTarjetasPorPing(3, fecha_cierre)
        ListadosReportesTarjetasPorPing(4, fecha_cierre)
        ListadosReportesTarjetasPorPing(5, fecha_cierre)
        ListadosReportesTarjetasPorPing(6, fecha_cierre)
        ListadosReportesTarjetasPorPing(7, fecha_cierre)
        ListadosReportesTarjetasPorPing(8, fecha_cierre)
        Exportar_Todos(fecha_cierre)
        If Not Exportar_Movimientos(fecha_cierre) Then
            Eliminar_exportados()
            Eliminar_Archivos(fecha_cierre)
        End If
        Logger.Info(vbNewLine &
                    "================================================" & vbNewLine &
                    "C  I  E  R  R  E    F  I  N  A  L  I  Z  A  D  O" & vbNewLine &
                    "================================================")

        'ArrancarEscuchadorCajas()
    End Sub

    ''' <summary>
    ''' Genera Resumen de transacciones PEI de las terminales que hayan tenido movimientos.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CierrePEI()
        Logger.Info(vbNewLine &
                    "===================================================" & vbNewLine &
                    "C O M I E N Z O      C I E R R E   PEI" & vbNewLine &
                    "===================================================")
        Dim fecha_cierre As Date = Now
        CulminacionCierreSatisfactoriaPEI(fecha_cierre)
        ListadosReportesTarjetasPEI(fecha_cierre)
        ListadosReportesPEITarjetasPorPing(1, fecha_cierre)
        ListadosReportesPEITarjetasPorPing(2, fecha_cierre)
        ListadosReportesPEITarjetasPorPing(3, fecha_cierre)
        ListadosReportesPEITarjetasPorPing(4, fecha_cierre)
        ListadosReportesPEITarjetasPorPing(5, fecha_cierre)
        ListadosReportesPEITarjetasPorPing(6, fecha_cierre)
        ListadosReportesPEITarjetasPorPing(7, fecha_cierre)
        ListadosReportesPEITarjetasPorPing(8, fecha_cierre)
        If Not Exportar_TodosPEI(fecha_cierre) Then Eliminar_exportadosPEI()
        Logger.Info(vbNewLine &
                    "==============================================================" & vbNewLine &
                    "  C  I  E  R  R  E      PEI     F  I  N  A  L  I  Z  A  D  O" & vbNewLine &
                    "==============================================================")

        Logger.Info("Reiniciando el equipo.")
        If Configuracion.TipoConfiguracion = Configuracion.Produccion Then Shell("Shutdown -r -t 0 -f")

    End Sub

    ''' <summary>
    ''' Cierra una sola caja para test.
    ''' </summary>
    ''' <param name="pinguino"></param>
    ''' <param name="caja"></param>
    ''' <param name="hst"></param>
    ''' <remarks></remarks>
    Sub CierreDiario(pinguino As Integer, caja As Integer, hst As String)
        Dim host = Hosts(hst)
        Dim renglones = FiltrarParaCerrar(pinguino, caja, Hosts(hst).Tipo)

        For Each ren In renglones
            Dim req As New Req(host.Tipo, pinguino, caja)
            req.CierreCantCompras = ren.Value.CantCompras
            req.CierreMontoCompras = ren.Value.SumaCompras
            req.CierreCantDevoluciones = ren.Value.CantDevoluciones
            req.CierreMontoDevoluciones = ren.Value.SumaDevoluciones
            req.CierreCantAnulaciones = ren.Value.CantAnulaciones
            req.CierreMontoAnulaciones = ren.Value.SumaAnulaciones
            host.Cerrar(req)
        Next
    End Sub

    Public Sub Sincronizar(hst As String)
        Dim host = Hosts(hst)
        Logger.Info("Preparando Envío Echotest a " + host.Name)
        host.Sincronizar(hst)

    End Sub

    Public Sub ProbarConexion()
        For Each host In Hosts.Values
            Logger.Info("Preparando Envío Echotest a " + host.Name)
            host.SendEchoTest()
        Next
    End Sub

    Public Function importar(archivo As String) As Int16
        timerBackup.Stop()
        Return ImportarMovimientos(archivo)
        timerBackup.Start()
    End Function

    Public Function Logger() As ILog
        Return LogManager.GetLogger(MethodBase.GetCurrentMethod.DeclaringType)
    End Function

    Public Sub MuestraNroPing(xxping As String)
        elnropinguino = xxping
    End Sub


    'Public Sub Msg(s As String)
    '    Dim msj As New mensajeServer() With {.Fecha = Now, .Texto = s}
    '    msj.Tipo = TipoMensaje.Informacion
    '    RaiseEvent Mensaje(msj)
    'End Sub

    'Public Sub MsgError(s As String)
    '    Dim msj As New mensajeServer() With {.Fecha = Now, .Texto = s}
    '    msj.Tipo = TipoMensaje.Error
    '    RaiseEvent Mensaje(msj)
    'End Sub





#Region "CAJAS"
#Region "TCP Nuevo"

    Friend WithEvents Transmisor As New TransmisorTCP.ClaseServidorSocket(5000)
    Private Sub ArrancarTCP()
        Transmisor.IniciarEscucha()
    End Sub

    Private Sub ClienteConectado(cliente As Net.IPEndPoint) Handles Transmisor.NuevaConexion
        Logger.Info(String.Format("Sesión iniciada en socket ({0})", cliente))

    End Sub
    Private Sub ClienteDesconectado(cliente As Net.IPEndPoint) Handles Transmisor.ConexionTerminada
        Logger.Info(String.Format("Sesión finalizada en socket ({0})", cliente))

    End Sub
    Private Sub SocketCerrado(cliente As Net.IPEndPoint) Handles Transmisor.ConexionCerrada
        Logger.Warn(String.Format("No se pudo enviar el mensaje al Cliente ({0}), socket cerrado.", cliente))

    End Sub
#End Region

    ' Friend WithEvents EscuchadorTCP As EscuchadorTCPCajas
    'Private Sub ArrancarEscuchadorTCP()
    '    EscuchadorTCP = New EscuchadorTCPCajas(3000)
    'End Sub


    Friend WithEvents Escuchador As EscuchadorCajaArchivos
    Private Sub ArrancarEscuchadorCajas()
        Escuchador = New EscuchadorCajaArchivos(Me)
        Escuchador.arrancar()
        estEscuchador = Escuchador.Estado
    End Sub

    Private Sub detenerEscuchadorCajas()
        Escuchador.detener()
    End Sub
#End Region


#Region "TCP"

    Public Sub Desconexion(hst As String)
        RaiseEvent Desconectado(hst)
    End Sub

    Public Sub Conexion(hst As String)
        RaiseEvent Conectado(hst)
    End Sub


    Friend Hosts As New Dictionary(Of String, HostTCP)
    Private Sub arrancarConexionTCP()
        ' leer datos desde la BD a memoria

        '--- ACA CONECTA A LOS SERVIDORES VISA Y POSNET                                                       
        '--- RECORRE LA BASE HOSTS, SI EL PARAM ESTA EN HOMOLOGACION RECORRE LOS TIPO H, SI EL PARAM          
        '--- ESTA EN PRODUCCION RECORRE LOS P                                                                 
        '--- VISA TIENE UNA DIRECCION2, QUE SE CONECTA CUANDO SE PONE VPN, CUANDO ESTA MPLS BUSCA DIRECCION1  
        LeerHostsdesdeDB(Me)
        For Each host As HostTCP In Hosts.Values
            If host.Name.Contains("PEI") Then Continue For
            Logger.Info("Intentando conectar con " + host.Name)
            host.Connect()
            System.Threading.Thread.Sleep(1000)

            If host.IsConnected Then
                Logger.Info("Host " + host.Name + " CONECTADO")
                host.SendEchoTest()
            Else
                RaiseEvent Desconectado(host.Name)
                Logger.Error("No se pudo conectar con " + host.Name)
            End If
        Next
    End Sub

    Private Sub detenerConexionTCP()
        For Each host In Hosts.Values
            host.Desconectar()
        Next
    End Sub
#End Region

    'Private Function ConvertirAIda(parametro As TjComun.Mensaje) As IdaTypeInternal

    '    Dim ida As IdaTypeInternal
    '    With ida
    '        .CARDSEQ = parametro.idEncripcion

    '    End With
    '    Return ida
    'End Function


    'Private Function ConvertirAIda(parametro As TjComun.SParametrosInput) As IdaTypeInternal

    '    Dim ida As IdaTypeInternal
    '    With ida
    '        .VERSION = parametro.VERSION            '* 1       ' NRO DE VERSION               
    '        .TARJ = parametro.TARJ            '* 20      ' NRO DE TARJETA               
    '        .EXPDATE = parametro.EXPDATE          '* 4       ' FECHA DE EXPIRACION          
    '        .IMPO = parametro.IMPO          'CURRENCY         ' IMPORTE DE LA TRANSACCION    
    '        .MANUAL = CType(parametro.MANUAL, Short)              ' MODO DE INGRESO 0-MANUAL 1-AUTO 5-CHIP
    '        '.MANUAL = parametro.MANUAL             ' MODO DE INGRESO 0-MANUAL 1-AUTO 5-CHIP
    '        .PLANINT = parametro.PLANINT            ' COD.PLAN                     
    '        .CODSEG = parametro.CODSEG         '* 30      ' COD. SEGURIDAD            
    '        .TICCAJ = parametro.TICCAJ            ' NRO DE TICKET DE LA CAJA     
    '        .CAJERA = parametro.CAJERA           ' COD DE CAJERO (OPERADOR)     
    '        .HORA = parametro.HORA            ' FECHA/HORA                   
    '        .TRACK2 = parametro.TRACK2        '* 37      '                              
    '        .TRACK1 = parametro.TRACK1            '* 77      '                              
    '        .CodAut = parametro.CodAut           '* 6       ' Codigo de autorizacion pa/anu
    '        .TKTORI = parametro.TKTORI            '                              
    '        .FECORI = parametro.FECORI           '* 6       '                              
    '        .PLANINTori = parametro.PLANINTori       ' COD.PLAN                     
    '        .oper = parametro.oper               ' !! operacion                 
    '        .cmd = parametro.cmd                '+8 offline +16 anular                  
    '        .cajadir = parametro.cajadir         '* 26      '                          
    '        .TKID = parametro.TKID      '* 4
    '        .CASHBACK = parametro.CASHBACK       '                          
    '        'Agregados EMV
    '        .CRIPTO = parametro.CRIPTO          ' * 300 CRIPTOGRAMA TARJETAS CHIP
    '        .CARDSEQ = parametro.CARDSEQ          ' * 3 CARD SEQUENCE
    '        '.APPVERSION = parametro.APPVERSION         ' * 15  VERSION APLICACION
    '        .CHECK = parametro.CHECK            '* 2      ' HACER = CHECKID           

    '    End With
    '    Return ida
    'End Function

    Sub ProcesarTCP(ByVal IDTerminal As System.Net.IPEndPoint) Handles Transmisor.DatosRecibidos
        Dim mensaje = Transmisor.ObtenerDatos(IDTerminal)
        Dim cajaPasar As String
        If mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.PedirTerminal Then            '----- Pide nro de terminal  
            Dim mensRespuesta As New TransmisorTCP.MensajeRespuesta

            cajaPasar = ""
            cajaPasar = Format(CInt(mensaje.caja), "00")

            Logger.Info(String.Format("Pinpad - Solicitando Terminal: CAJA: {0} PINGUINO: {1}", cajaPasar, mensaje.ping))

            mensRespuesta.terminal = BuscarTerminal(mensaje.ping & cajaPasar, CByte(mensaje.host))
            mensRespuesta.serie_PP = BuscarSeriePP(mensaje.ping & cajaPasar, CByte(mensaje.host))

            Logger.Info(String.Format("Pinpad - Respondiendo a CAJA: {0} PINGUINO: {1} | Terminal: {2} | Serie: {3}", mensaje.caja, mensaje.ping, mensRespuesta.terminal, mensRespuesta.serie_PP))
            Transmisor.enviarMensajeCliente(IDTerminal, mensRespuesta)

        ElseIf mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.ActualizarVersion Then   '----- Actualiza VersionAPP  
            '-----------------------------------------------------------------------------------------------------------------------------
            '--- CADA VEZ QUE SINCRONIZA - En la tabla numeros UPDATEA los campos  SeriePP, PosicionMK, WKDatos, WKPines y VersionSoftPP  
            '--- ACA actualiza VersionSoftPP                                                                                              
            '-----------------------------------------------------------------------------------------------------------------------------
            Dim mensRespuesta As New TransmisorTCP.MensajeRespuesta

            cajaPasar = ""
            cajaPasar = Format(CInt(mensaje.caja), "00")

            Logger.Info(String.Format("Pinpad - Actualizando Version APP: Caja Propio: {0} | Ver.APP: {1}", mensaje.ping & cajaPasar, mensaje.versionAPP))
            'Logger.Info(String.Format("Pinpad - Actualizando Version APP: Caja Propio: {0} | Ver.APP: {1}", mensaje.ping & mensaje.caja, mensaje.versionAPP))

            mensRespuesta.codRespuesta = Modificar_VersionAPP(mensaje.ping, cajaPasar, CByte(mensaje.host), mensaje.versionAPP)
            'mensRespuesta.codRespuesta = Modificar_VersionAPP(mensaje.ping, mensaje.caja, CByte(mensaje.host), mensaje.versionAPP)
            Transmisor.enviarMensajeCliente(IDTerminal, mensRespuesta)

        ElseIf mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.ActualizarSerie Then     '----- Actualiza nro de serie en la sincronizacion  
            '-----------------------------------------------------------------------------------------------------------------------------
            '--- CADA VEZ QUE SINCRONIZA - En la tabla numeros UPDATEA los campos  SeriePP, PosicionMK, WKDatos, WKPines y VersionSoftPP  
            '--- ACA actualiza SeriePP, PosicionMK                                                                                        
            '-----------------------------------------------------------------------------------------------------------------------------
            Dim mensRespuesta As New TransmisorTCP.MensajeRespuesta
            Logger.Info(String.Format("Pinpad - Actualizando serie Terminal: {0}", mensaje.terminal))
            mensRespuesta.Sincronizado = Modificar_seriePP(mensaje.terminal, CByte(mensaje.host), mensaje.nroSerie, mensaje.posMK)
            Transmisor.enviarMensajeCliente(IDTerminal, mensRespuesta)

        ElseIf mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.PedirInfoAdicional Then    '----- Busca que datos adicionales debe pedir segun la tarjeta.   
            Dim mensRespuesta As New TransmisorTCP.MensajeRespuesta

            cajaPasar = ""
            cajaPasar = Format(CInt(mensaje.caja), "00")

            Logger.Info(String.Format("Pinpad - Solicitando datos adicionales CAJA: {0} PINGUINO: {1}", cajaPasar, mensaje.ping))
            'Logger.Info(String.Format("Pinpad - Solicitando datos adicionales CAJA: {0} PINGUINO: {1}", mensaje.caja, mensaje.ping))
            BuscarDatosAdicionales(mensaje.nroTarjeta.Substring(0, 6), mensRespuesta, mensaje.serviceCode)

            If mensRespuesta.Emisor.TrimEnd = "MASTERCARD DEBIT" And mensaje.tipoIngreso = TipoIngreso.Manual Or mensRespuesta.Emisor.TrimEnd = "MAESTRO" And mensaje.tipoIngreso = TipoIngreso.Manual Then
                Logger.Warn(mensRespuesta.Emisor & " No permite ingreso manual ")
                mensRespuesta.codRespuesta = "XX"
                mensRespuesta.Respuesta = "El emisor " & mensRespuesta.Emisor.TrimEnd & " no permite ingreso manual"
            Else
                If mensRespuesta.codRespuesta <> "XX" Then
                    cajaPasar = ""
                    cajaPasar = Format(CInt(mensaje.caja), "00")

                    BuscarClaves(mensaje.ping & cajaPasar, CByte(mensRespuesta.host), mensRespuesta)
                    'BuscarClaves(mensaje.ping & mensaje.caja, CByte(mensRespuesta.host), mensRespuesta)
                    mensRespuesta.codRespuesta = "00"
                End If
            End If

            Logger.Info(String.Format("Pinpad - Respondiendo a CAJA {0} PINGUINO {1} | Datos adicionales", mensaje.caja, mensaje.ping))
            Transmisor.enviarMensajeCliente(IDTerminal, mensRespuesta)

        ElseIf mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.PedirClaves Then     '--- Aca hace el CONSULTAR_CLAVES - Busca claves para ContactLess.   
            Dim mensRespuesta As New TransmisorTCP.MensajeRespuesta


            '--- ACA ACA ACA ACA ULTIMO                                      
            '--- ESTA FIJO 5, HAY QUE PONER 1 Y CARGAR TODAS LAS TERMINALES  
            '--- EN LA TABLA NUMEROS, PARA CADA CAJA DE CADA PINGUINO        
            cajaPasar = ""
            cajaPasar = Format(CInt(mensaje.caja), "00")

            If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                BuscarClaves(mensaje.ping & cajaPasar, 5, mensRespuesta)
            Else
                BuscarClaves(mensaje.ping & cajaPasar, 1, mensRespuesta)
            End If
            'If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
            '    BuscarClaves(mensaje.ping & mensaje.caja, 5, mensRespuesta)
            'Else
            '    BuscarClaves(mensaje.ping & mensaje.caja, 1, mensRespuesta)
            'End If

            Logger.Info(String.Format("Pinpad - Solicitando Claves CAJA: {0} PINGUINO: {1}", mensaje.caja, mensaje.ping))
            Transmisor.enviarMensajeCliente(IDTerminal, mensRespuesta)

        ElseIf mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.RespuestaChip Then
            Dim mensrespuesta As New TransmisorTCP.MensajeRespuesta
            If Grabar_RespChip(mensaje.RespChip, mensaje.terminal, mensaje.host, mensaje.ticketOriginal) Then
                mensrespuesta.codRespuesta = "00"
            Else
                mensrespuesta.codRespuesta = "99"       '--- no se grabo respuesta chip   
            End If
            Transmisor.enviarMensajeCliente(IDTerminal, mensrespuesta)

        ElseIf mensaje.tipoMensaje = TransmisorTCP.TipoMensaje.ConsultarMovimientoAnulacion Then
            Dim mensRespuesta As New TransmisorTCP.MensajeRespuesta

            cajaPasar = ""
            cajaPasar = Format(CInt(mensaje.caja), "00")

            BuscarDatosAdicionales(mensaje.nroTarjeta.Substring(0, 6), mensRespuesta, mensaje.serviceCode)

            BuscarClaves(mensaje.ping & cajaPasar, CByte(mensRespuesta.host), mensRespuesta)
            'BuscarClaves(mensaje.ping & mensaje.caja, CByte(mensRespuesta.host), mensRespuesta)

            mensRespuesta.terminal = BuscarTerminal(mensaje.ping & cajaPasar, CByte(mensRespuesta.host))
            'mensRespuesta.terminal = BuscarTerminal(mensaje.ping & mensaje.caja, CByte(mensRespuesta.host))

            If BuscarMovimientoTCP(mensaje, mensRespuesta) Then
                mensRespuesta.codRespuesta = "00"
            Else
                mensRespuesta.codRespuesta = "XX"
                mensRespuesta.Respuesta = "Movimiento no encontrado en el Lote o ya anulado, no se puede anular."
            End If
            Transmisor.enviarMensajeCliente(IDTerminal, mensRespuesta)

        Else

            '---------------------------------------------------------------------
            '--- ACA ENTRA SI ES COMPRA, DEVOLUCION, ANULACION, REVERSO           
            '--- HACER CLICK DERECHO SOBRE NEW E IR A DEFINICION                  
            '---------------------------------------------------------------------
            Dim req As New Req(Transmisor.ObtenerDatos(IDTerminal), IDTerminal)



            If req.RespInvalida Then
                Me.ResponderInvalidaACajaTCP(req)

            Else

                If Hosts.ContainsKey(req.nombreHost) Then
                    Dim host = Hosts(req.nombreHost)
                    'If My.Computer.Name = "MARCOS-XP" Then
                    '    If req.ida.MANUAL = 5 Then
                    '        AgregarAdvPendientes(ClaveVta(req.pinguino, req.nroCaja), req)
                    '    End If
                    'End If

                    If mensaje.reversar = False Then

                        If req.msjIda.CodAutorizaAdv IsNot Nothing Then
                            If req.msjIda.CodAutorizaAdv.Trim = "Z1" Or req.msjIda.CodAutorizaAdv.Trim = "Z3" Or
                               req.msjIda.CodAutorizaAdv.Trim = "Y1" Or req.msjIda.CodAutorizaAdv.Trim = "Y3" Then
                                host.enviarAdvice3DES(req)
                            Else
                                host.enviar3DES(req)    '--- ACA MANDA AL SERVIDOR DE TARJETAS LA COMPRA O LO QUE SEA 
                            End If
                        Else
                            host.enviar3DES(req)    '--- ACA MANDA AL SERVIDOR DE TARJETAS LA COMPRA O LO QUE SEA 
                        End If

                    Else

                        '--- CUANDO VIENE Z3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER NO APROBADO  
                        '--- CUANDO VIENE Z1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER NO APROBADO  
                        '--- CUANDO VIENE Y3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER APROBADO  
                        '--- CUANDO VIENE Y1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER APROBADO  
                        If req.msjIda.CodAutorizaAdv IsNot Nothing Then
                            If req.msjIda.CodAutorizaAdv.Trim = "Z1" Or req.msjIda.CodAutorizaAdv.Trim = "Z3" Or
                               req.msjIda.CodAutorizaAdv.Trim = "Y1" Or req.msjIda.CodAutorizaAdv.Trim = "Y3" Then
                                If req.msjIda.CodAutorizaAdv.Trim = "Z3" Or req.msjIda.CodAutorizaAdv.Trim = "Y3" Then
                                    host.enviarAdvice3DES(req)
                                    'host.enviarReverso3DES(req)    '--- no mando el reverso porque ya lo mando desde PINPAD_EMW 
                                End If
                                If req.msjIda.CodAutorizaAdv.Trim = "Z1" Or req.msjIda.CodAutorizaAdv.Trim = "Y1" Then
                                    host.enviarAdvice3DES(req)
                                End If
                            Else
                                host.enviarReverso3DES(req)
                            End If
                        Else
                            host.enviarReverso3DES(req)
                        End If
                    End If
                Else
                    Logger.Warn("No existe host " + req.nombreHost)
                    req.InvalidarReq("INVALIDA: No existe host")
                    ResponderInvalidaACajaTCP(req)
                End If
            End If
        End If
        'Logger.Info(IDTerminal)

    End Sub

    Private Function Reversar_Por_RespuestaCHIP(mensaje As TransmisorTCP.MensajeIda, req As Req) As Boolean
        Return req.msjRespuesta.codRespuesta = "00" And mensaje.RespChip <> "00"

    End Function

    'Sub ProcIda(requerimiento As TjComun.mensaje, nrocaja As Integer, client As System.Net.Sockets.NetworkStream) Handles EscuchadorTCP.NuevoRequerimiento

    '    ' PROCESAR IDA, GRABAR HISTORIA 

    '    Dim req As New Req(ConvertirAIda(requerimiento), CInt(requerimiento.caja), client)
    '    req.vta = TjComun.IdaLib.InicializarVtaDesdeIDA(req.ida)

    '    If req.RespInvalida Then
    '        Me.ResponderInvalidaACaja(req)
    '    Else
    '        If Hosts.ContainsKey(req.nombreHost) Then
    '            Dim host = Hosts(req.nombreHost)
    '            host.enviar_sincro(req)

    '        Else
    '            Logger.Warn("No existe host " + req.nombreHost)
    '            req.InvalidarReq("INVALIDA: No existe host")
    '            ResponderInvalidaACaja(req)
    '        End If
    '    End If
    'End Sub



    Sub ProcPEI(vtapei As lib_PEI.Respuesta, terminal As String) Handles Escuchador.NuevoVtaPEI
        Logger.Info("Llego la respuesta del pei")

        Dim req As Req
        '' buscar req desde la lista de pendientes
        Dim S As String
        Logger.Info("Buscando ClaveIDA: " & terminal & " Trace: " & vtapei.trace)
        If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
            S = ClaveIDA("PEI_homolog", terminal, CInt(vtapei.trace))
        Else
            S = ClaveIDA("PEI", terminal, CInt(vtapei.trace))
        End If

        If S <> "" Then
            Logger.Info("ClaveIDA encontrada")
        Else
            Logger.Error("ClaveIDA NO encontrada")
        End If

        If ListaReqPendientesOnline.ContainsKey(S) Then
            req = ListaReqPendientesOnline(S)
        Else
            ' No lo tengo en cuenta, porque ya se saco de la lista de pendientes porque no llegó nunca y por ende se reversó; o 
            ' Se reverso al abrir el programa despues de un cierre inesperado.
            Logger.Error("Requermiento no encontrado, respuesta tardía")
            Exit Sub
        End If

        Logger.Info("PEI - Clave: " + S + " CAJA: " + CStr(req.nroCaja) + " PING: " + req.pinguino & vbNewLine &
                    "Demoró " + Now.Subtract(req.FechaHoraEnvioMsg).TotalSeconds.ToString & vbNewLine &
                     "*****   ELIMINANDO DE ListaReqPendientesOnline: " + S)

        generandoVTA = True 'es para que no pueda salir hasta que termine de grabar el vta
        ListaReqPendientesOnline.Remove(S)
        Logger.Info("PEI - Operaciones pendientes: " + ListaReqPendientesOnline.Count.ToString)
        ProcesarMensajeEntrantePEI(req.nombreHost, req, vtapei)
    End Sub
















    Sub ProcIda(ida As IdaTypeInternal, nrocaja As Integer) Handles Escuchador.NuevoIda
        Dim req As Req
        'ida.version 2 es para que vaya por MPLS.
        Dim nrohost As Byte
        If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
            nrohost = 7      '--- HOST HOMOLOGACION PEI  
        Else
            nrohost = 8      '--- HOST PRODUCCION PEI  
        End If

        Dim elPingui As Integer
        elPingui = CInt(Mid(ida.cajadir, ida.cajadir.Length - 1))

        '--- If elPingui = 4 And nrocaja = 20 Then
        'If Mid(ida.TARJ, 1, 18) = "778899000000091004" Then    '---  esta es la tarjeta de prueba       
        'If Mid(ida.TARJ, 1, 16) = "4815500007359983" Or Mid(ida.TARJ, 1, 18) = "501041285013131029" Then       '--- esta es la tarjeta visa debito mia y la maestro de janina 
        'If (elPingui = 4 And nrocaja = 10) Or (elPingui = 4 And nrocaja = 11) Or (elPingui = 4 And nrocaja = 12) Then
        'If elPingui = 4 And nrocaja >= 4 Then
        'If elPingui = 1 Or elPingui = 3 Or elPingui = 4 Or elPingui = 7 Or elPingui = 8 Then
        If (elPingui = 4 And nrocaja = 8) Then

            Dim contador As Int32
            Dim hora As String
            Dim controlTableAdapter As New DatosTjTableAdapters.CONTROLTableAdapter

            Dim contadorDB = controlTableAdapter.GetContador
            hora = contadorDB.Item(0).hora
            Dim pepe As Boolean
            pepe = False

            If contadorDB.Item(0).contador >= 5 Then
                'If Now.ToShortTimeString.Equals(Convert.ToDateTime(hora).AddMinutes(30).ToShortTimeString) Or
                '   DateTime.Compare(DateTime.Now, Convert.ToDateTime(hora).AddMinutes(30)) > 0 Then
                If DateTime.Compare(DateTime.Now, Convert.ToDateTime(hora).AddMinutes(30)) > 0 Then
                    'si es < 0 "hora izquierda menor a hora derecha"  
                    'si es = 0 "son iguales"                          
                    'si es > 0 "hora izquierda mayor a hora derecha"  
                    contador = 0
                    hora = Now.ToShortTimeString
                    controlTableAdapter.ActualizarControl(contador, hora)
                    pepe = True
                Else
                    Logger.Info("******************************************************************************************************")
                    Logger.Info("            Contador: " + contadorDB.Item(0).contador.ToString)
                    Logger.Info("             Hora PC: " + DateTime.Now.ToString)
                    Logger.Info(" Hora Tabla + 30 min: " + Convert.ToDateTime(hora).AddMinutes(30).ToString)
                    Logger.Info("******************************************************************************************************")
                End If
            Else
                pepe = True
            End If


            '--- PEI ES SOLAMENTE PARA TARJETAS DE DEBITO                                                                          
            '--- ida.VERSION                                                                                                       
            '--- ida.VERSION = "1", VIENE DESDE EL REG4000 CUANDO SE HACE UNA COMPRA (SIEMPRE, PORQUE RECIEN ACÁ LO MANDA POR      
            '---                    PEI O LO MANDA NORMAL)                                                                         
            '--- ida.VERSION = "3", VIENE DESDE EL REG4000 CUANDO SE HACE UNA DEVOLUCION                                           
            '--- vta.VtaVersion                                                                                                    
            '--- req.vta = TjComun.IdaLib.InicializarVtaDesdeIDAPEI(ida), pone como vta.VtaVersion = "2" (CUANDO LO MANDA POR PEI) 
            '--- req.vta = TjComun.IdaLib.InicializarVtaDesdeIDA(req.ida), pone como vta.VtaVersion = "1" (CUANDO LO MANDA NORMAL) 
            If (Configuracion.modo = Configuracion.PEI AndAlso
                   BuscarTerminal(Mid(ida.cajadir, ida.cajadir.Length - 1) + nrocaja.ToString("00"), nrohost) <> "" AndAlso
                   ida.VERSION = "1" AndAlso
                   MandarPEI(ida.TARJ.Substring(0, 6), ida.TRACK2) AndAlso
                   ida.CASHBACK = 0 AndAlso
                   pepe = True) Or ida.VERSION = "3" Then

                req = New Req(ida, nrocaja, True)
                req.vta = TjComun.IdaLib.InicializarVtaDesdeIDAPEI(ida)

                Logger.Info("******************************************************************************************************")
                Logger.Info("*** ENVIO POR PEI ***     *** ENVIO POR PEI ***     *** ENVIO POR PEI ***     *** ENVIO POR PEI ***   ")
                Logger.Info("*** Ping: " + elPingui.ToString + " - Caja: " + nrocaja.ToString("00"))
                Logger.Info("*** CLIENTE: " + req.cliente)
                Logger.Info("******************************************************************************************************")

                Dim host As HostTCP
                host = Hosts(req.nombreHost)
                'Logger.Info("*** PASO HOST ***")

                If req.RespInvalida Then
                    'Logger.Info("*** req.RespInvalida = TRUE ***")
                    Me.ResponderInvalidaACajaPEI(req)
                Else
                    'Logger.Info("*** req.RespInvalida = FALSE ***")
                    host.enviar_PEI(req)
                End If
                'req.InvalidarReq("INVALIDA: PEI")
                'Me.ResponderInvalidaACaja(req)

            Else

                ' PROCESAR IDA, GRABAR HISTORIA 
                req = New Req(ida, nrocaja, False)
                req.vta = TjComun.IdaLib.InicializarVtaDesdeIDA(req.ida)

                If req.RespInvalida Then
                    Me.ResponderInvalidaACaja(req)
                Else
                    If Hosts.ContainsKey(req.nombreHost) Then
                        Dim host = Hosts(req.nombreHost)
                        'If My.Computer.Name = "MARCOS-XP" Then
                        '    If req.ida.MANUAL = 5 Then
                        '        AgregarAdvPendientes(ClaveVta(req.pinguino, req.nroCaja), req)
                        '    End If
                        'End If

                        host.enviar(req)
                    Else
                        Logger.Warn("No existe host " + req.nombreHost)
                        req.InvalidarReq("INVALIDA: No existe host")
                        ResponderInvalidaACaja(req)
                    End If
                End If
            End If

        Else

            '--- TARJETAS QUE NO VAN POR PEI  
            ' PROCESAR IDA, GRABAR HISTORIA 
            req = New Req(ida, nrocaja, False)
            req.vta = TjComun.IdaLib.InicializarVtaDesdeIDA(req.ida)

            If req.RespInvalida Then
                Me.ResponderInvalidaACaja(req)
            Else
                If Hosts.ContainsKey(req.nombreHost) Then
                    Dim host = Hosts(req.nombreHost)
                    host.enviar(req)
                Else
                    Logger.Warn("No existe host " + req.nombreHost)
                    req.InvalidarReq("INVALIDA: No existe host")
                    ResponderInvalidaACaja(req)
                End If
            End If
        End If



    End Sub
#Region "Claves"
    Public Shared Function ClaveIDA(host As String, nroterm As String, trace As Integer) As String
        ' host+terminal+trace
        Dim s As String = host + "/" + trace.ToString + "/" + Trim(nroterm)
        Return s
    End Function

    Public Shared Function ClaveIDA(nrohost As HostTCP, nroterm As String, msg As Iso8583Message) As String
        ' host+terminal+trace
        Dim s As String = nrohost.Name + "/" + msg.Fields(11).ToString + "/" + Trim(nroterm)
        Return s
    End Function

    Public Shared Function ClaveVta(pinguino As String, caja As Integer) As String
        ' pinguino+caja
        Dim s As String = pinguino + "/" + caja.ToString("00")
        Return s
    End Function



#End Region
    Private Sub responderReverso3DES(req As Req)
        With req.msjRespuesta
            .mensajeHost = "REINTENTE (REV)"
            .Respuesta = "REINTENTE (REV)"
            .cupon = crearTickDATReversoTCP(req)
        End With
        responderAcajaTCP(req)
    End Sub

    Private Sub responderAdvice3DES(req As Req)
        With req.msjRespuesta
            If req.msjIda.CodAutorizaAdv.Trim IsNot Nothing Then
                '--- CUANDO VIENE Z3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER NO APROBADO  
                '--- CUANDO VIENE Z1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER NO APROBADO  
                '--- CUANDO VIENE Y3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER APROBADO  
                '--- CUANDO VIENE Y1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER APROBADO  
                If req.msjIda.CodAutorizaAdv.Trim = "Z1" Or req.msjIda.CodAutorizaAdv.Trim = "Z3" Then
                    .mensajeHost = "RECHAZADO (ADVICE)"
                    .Respuesta = "RECHAZADO (ADVICE)"
                    .cupon = crearTickDATAdviceTCP(req)
                ElseIf req.msjIda.CodAutorizaAdv.Trim = "Y1" Then
                    .mensajeHost = "APROBADO (ADVICE)"
                    .Respuesta = "APROBADO (ADVICE)"
                    .cupon = crearTickDATAdviceTCP_Y1(req)
                    IncrementarNroTicket(req.terminal, req.nrohost)
                End If
            End If
        End With
        responderAcajaTCP(req)
    End Sub



    Private Sub responderReversoPEI(req As Req)
        With req.vta
            .VtaVersion = "2"
            .VtaMensaje = "No hubo respuesta."
            .VtaMenResp = lib_PEI.mensajesRepuesta.SIN_RESPUESTA.ToString
            req.autorizacion = "0"
            .VtaMontop = "0"
            .VtaEmiName = req.descEmisor
            .VtaFileTkt = "TICK00" + req.nroCaja.ToString("00") + ".DAT"
            .VtaTicket = req.ida.TICCAJ
        End With
        responderAcaja(req)
    End Sub

    Private Sub responderReverso(req As Req)
        With req.vta
            .VtaVersion = "1"
            .VtaMensaje = "REINTENTE (REV)"
            .VtaMenResp = "REINTENTE (REV)"
            req.autorizacion = "0"
            .VtaMontop = "0"
            .VtaEmiName = req.descEmisor
            .VtaFileTkt = "TICK00" + req.nroCaja.ToString("00") + ".DAT"
            .VtaTicket = req.ida.TICCAJ
        End With
        responderAcaja(req)
    End Sub

    ''' <summary>
    ''' Arma la respuesta en el caso que haya un error antes de enviar el requerimiento. No se envia al host y se responde a caja.
    ''' </summary>
    ''' <param name="req"></param>
    ''' <remarks></remarks>
    Sub ResponderInvalidaACajaTCP(req As Req)
        Try
            With req.msjRespuesta
                .mensajeHost = req.CausaInvalida
                .Respuesta = E_CodigosRespuesta.TRANSACCION_INVALIDA.ToString
                Dim cr As Integer = E_CodigosRespuesta.TRANSACCION_INVALIDA
                .codRespuesta = cr.ToString
                .Autorizacion = ""
                .criptograma = ""
            End With
        Catch ex As Exception

        End Try
        responderAcajaTCP(req)
    End Sub


    ''' <summary>
    ''' Arma la respuesta en el caso que haya un error antes de enviar el requerimiento. No se envia al host y se responde a caja.
    ''' </summary>
    ''' <param name="req"></param>
    ''' <remarks></remarks>
    Sub ResponderInvalidaACajaPEI(req As Req)

        With req.vta
            .VtaVersion = "2"
            .VtaMensaje = req.CausaInvalida
            .VtaMenResp = E_CodigosRespuesta.TRANSACCION_INVALIDA.ToString
            req.autorizacion = "0"
            .VtaMontop = "0"
            .VtaEmiName = req.descEmisor '"0"
            .VtaFileTkt = "TICK00" + req.nroCaja.ToString("00") + ".DAT"
            .VtaTicket = 0
        End With
        crearTickDATNoAprobadoPEI(req)
        responderAcaja(req)
    End Sub


    ''' <summary>
    ''' Arma la respuesta en el caso que haya un error antes de enviar el requerimiento. No se envia al host y se responde a caja.
    ''' </summary>
    ''' <param name="req"></param>
    ''' <remarks></remarks>
    Sub ResponderInvalidaACaja(req As Req)

        With req.vta
            .VtaVersion = "1"
            .VtaMensaje = req.CausaInvalida
            .VtaMenResp = E_CodigosRespuesta.TRANSACCION_INVALIDA.ToString
            req.autorizacion = "0"
            .VtaMontop = "0"
            .VtaEmiName = "0"
            .VtaFileTkt = "TICK00" + req.nroCaja.ToString("00") + ".DAT"
            .VtaTicket = 0
        End With
        crearTickDATNoAprobado(req)
        responderAcaja(req)
    End Sub

    Sub ProcesarCierre(hst As HostTCP, msg As Iso8583Message, req As Req)
        ' aca va toda la logica para el cierre.
        'Me.Msg("Cierre recibido: " & msg.ToString)
        'Dim req As Req
        'Dim s = ClaveIDA(hst, msg.Fields(41).ToString, msg)
        'req = ListaCierres(s)

        If msg.Fields(39).ToString = "00" Then
            Logger.Info("CIERRE OK")
            CulminacionCierreSatisfactoria(req)
        Else
            Logger.Error("ERROR EN CIERRE")
        End If

        CulminacionCierreNOSatisfactoria(req)


    End Sub

    Private Function largoClave(longitud As Integer) As Integer
        Select Case longitud
            Case 1
                Return 8
            Case 2
                Return 16
            Case 3
                Return 24
            Case Else
                Return 0
        End Select
    End Function


    Private Structure ClavesSincro
        Dim wkp As String
        Dim wkd As String
        Dim controlwkd As String
        Dim controlwkp As String
    End Structure

    Private Function Obtener_Claves(msg As Iso8583Message) As ClavesSincro
        Dim longWK, tipoWK1, largoTotal, puntero As Integer
        Dim infoctlwkp(1) As Byte
        Dim infoctlwkd(1) As Byte

        Try
            Obtener_Claves.wkp = ""
            Obtener_Claves.wkd = ""
            Obtener_Claves.controlwkd = ""
            Obtener_Claves.controlwkp = ""
            largoTotal = CInt(msg.Fields(60).Value.ToString.Length)
            puntero = 0
            longWK = CInt(msg.Fields(60).Value.ToString.Substring(puntero, 1))
            puntero += 1
            While puntero < largoTotal - 1
                tipoWK1 = CInt(msg.Fields(60).Value.ToString.Substring(puntero, 1))
                puntero += 1
                If tipoWK1 = 1 Then
                    For c = 0 To largoClave(longWK) - 1
                        Obtener_Claves.wkd = Obtener_Claves.wkd & Hex(Convert.ToByte(AscW(msg.Fields(60).Value.ToString.Substring(puntero, 1)))).PadLeft(2, Convert.ToChar("0"))
                        puntero += 1
                    Next
                    Obtener_Claves.controlwkd = Obtener_Claves.controlwkd & Hex(Convert.ToByte(AscW(msg.Fields(60).Value.ToString.Substring(puntero, 1)))).PadLeft(2, Convert.ToChar("0"))
                    puntero += 1
                    Obtener_Claves.controlwkd = Obtener_Claves.controlwkd & Hex(Convert.ToByte(AscW(msg.Fields(60).Value.ToString.Substring(puntero, 1)))).PadLeft(2, Convert.ToChar("0"))
                    puntero += 1
                Else
                    For c = 0 To largoClave(longWK) - 1
                        Obtener_Claves.wkp = Obtener_Claves.wkp & Hex(Convert.ToByte(AscW(msg.Fields(60).Value.ToString.Substring(puntero, 1)))).PadLeft(2, Convert.ToChar("0"))
                        puntero += 1
                    Next
                    Obtener_Claves.controlwkp = Obtener_Claves.controlwkp & Hex(Convert.ToByte(AscW(msg.Fields(60).Value.ToString.Substring(puntero, 1)))).PadLeft(2, Convert.ToChar("0"))
                    puntero += 1
                    Obtener_Claves.controlwkp = Obtener_Claves.controlwkp & Hex(Convert.ToByte(AscW(msg.Fields(60).Value.ToString.Substring(puntero, 1)))).PadLeft(2, Convert.ToChar("0"))
                    puntero += 1
                End If
            End While
            Logger.Info(String.Format("WKP             :{0}", Obtener_Claves.wkp))
            Logger.Info(String.Format("WKD             :{0}", Obtener_Claves.wkd))
            Logger.Info(String.Format("INFO CONTROL WKP:{0}", Obtener_Claves.controlwkp))
            Logger.Info(String.Format("INFO CONTROL WKD:{0}", Obtener_Claves.controlwkd))


        Catch ex As Exception
            Obtener_Claves.wkp = ""
            Obtener_Claves.wkd = ""
            Obtener_Claves.controlwkd = ""
            Obtener_Claves.controlwkp = ""
            Logger.Error("No se pudo decodificar WK en Obtener_claves")
        End Try
    End Function

    Private Function Obtener_Claves_General(msg As Iso8583Message) As ClavesSincro
        Dim tipoWK, puntero, cantidadSubCampos As Integer

        Try
            Obtener_Claves_General.wkp = ""
            Obtener_Claves_General.wkd = ""
            cantidadSubCampos = CInt(msg.Fields(60).Value.ToString.Substring(3, 4))

            puntero = 10
            For x As Integer = 1 To cantidadSubCampos

                tipoWK = CInt(msg.Fields(60).Value.ToString.Substring(puntero, 3))
                puntero += 3

                If tipoWK = 37 Then
                    For c = 0 To 15
                        Obtener_Claves_General.wkd = Obtener_Claves_General.wkd & Hex(Convert.ToByte(AscW(msg.Fields(60).Value.ToString.Substring(puntero, 1)))).PadLeft(2, Convert.ToChar("0"))
                        puntero += 1
                    Next

                ElseIf tipoWK = 74 Then

                    For c = 0 To 15
                        Obtener_Claves_General.wkp = Obtener_Claves_General.wkp & Hex(Convert.ToByte(AscW(msg.Fields(60).Value.ToString.Substring(puntero, 1)))).PadLeft(2, Convert.ToChar("0"))
                        puntero += 1
                    Next

                End If
                puntero += 3
            Next
            Logger.Info(String.Format("WKP             :{0}", Obtener_Claves_General.wkp))
            Logger.Info(String.Format("WKD             :{0}", Obtener_Claves_General.wkd))

        Catch ex As Exception
            Obtener_Claves_General.wkp = ""
            Obtener_Claves_General.wkd = ""
            Logger.Error("No se pudo decodificar WK en Obtener_claves_general")
        End Try
    End Function

    Sub ProcesarMensaje3DES(HST As HostTCP, MSG As Iso8583Message, req As Req)
        '--------------------------------------------------------------------------------------------
        '--- ACA TOMO LA RESPUESTA DEL HOST Y LO VOY PONIENDO EN VARIABLES QUE VAN A SER EL VTA      
        '--- TAMBIEN GENERA EL CUPON QUE SE IMPRIME EN LA CAJA                                       
        '--------------------------------------------------------------------------------------------
        With MSG
            Dim cr As String = ""
            'ACA ESTA LA RESPUESTA VER QUE HACER, HAY QUE DESARMAR EL PAQUETE
            If CInt(.MessageTypeIdentifier.ToString) > 9000 Then
                Logger.Error("Mensaje recibido no implementado")
                cr = "96"
            Else
                If .Fields.Contains(39) Then cr = .Fields(39).ToString     '--- SI ESTA APROBADO O NO
            End If
            'If .Fields(3).ToString = "990000" Then Exit Sub 'es un echotest

            req.tipoMensaje = .MessageTypeIdentifier.ToString("0000")

            If .Fields.Contains(4) Then
                req.msjRespuesta.Importe = CDec(.Fields(4).Value) / 100
            Else
                req.msjRespuesta.Importe = req.importe
            End If
            If .Fields.Contains(7) Then req.msjRespuesta.fechaRespuesta = .Fields(7).Value.ToString
            req.idred = CInt(.Fields(24).ToString)
            If .Fields.Contains(34) Then req.nrocuenta = .Fields(34).ToString 'nro de cuenta visa
            If .Fields.Contains(37) Then
                req.retrefnumber = .Fields(37).ToString
            Else
                req.retrefnumber = ""
            End If
            If .Fields.Contains(38) Then
                req.autorizacion = .Fields(38).ToString
                req.msjRespuesta.Autorizacion = .Fields(38).ToString
            Else
                req.autorizacion = ""
            End If
            req.msjRespuesta.codRespuesta = cr
            req.msjRespuesta.terminal = req.terminal

            req.msjRespuesta.host = HST.Name

            If .Fields.Contains(52) Then '---- WORKING KEY. MAESTRO, LLEGA BINARIO, HAY QUE PASARLO A HEXA
                Dim bcdtmp As String
                Dim bytes As Integer = 8
                bcdtmp = ""
                Try
                    bcdtmp = ByteArrayToString(MSG.Fields(52).GetBytes)
                    Logger.Info("Clave PIN: " & bcdtmp)
                Catch ex As Exception
                    bcdtmp = "1234123412341234"
                    Logger.Error(String.Format("Error decodificando wkey: {1} ping: {0}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
                End Try
                req.msjRespuesta.Clave_wkp = bcdtmp
                '-----------------------------------------------------------------------------------------------------------------------------
                '--- CADA VEZ QUE SINCRONIZA - En la tabla numeros UPDATEA los campos  SeriePP, PosicionMK, WKDatos, WKPines y VersionSoftPP  
                '--- ACA actualiza WKDatos, WKPines                                                                                           
                '-----------------------------------------------------------------------------------------------------------------------------
                Modificar_Claves(req.terminal, req.nrohost, "", bcdtmp)   '--- ACA GUARDA LA WORKING KEY EN LA TABLA NUMEROS
            End If

            If .Fields.Contains(55) Then
                req.msjRespuesta.criptograma = .Fields(55).ToString.PadRight(300)
            Else
                req.msjRespuesta.criptograma = "".PadRight(300)
            End If

            If .Fields.Contains(60) Then ' WORKING KEY. 
                If CInt(.MessageTypeIdentifier.ToString) = 210 Then     '--- RESPUESTA DE COMPRA, DEV., ANUL
                    Dim claves = Obtener_Claves_General(MSG)
                    req.msjRespuesta.Clave_wkp = claves.wkp
                    req.msjRespuesta.Clave_wkd = claves.wkd
                    '-----------------------------------------------------------------------------------------------------------------------------
                    '--- CADA VEZ QUE SINCRONIZA - En la tabla numeros UPDATEA los campos  SeriePP, PosicionMK, WKDatos, WKPines y VersionSoftPP  
                    '--- ACA actualiza WKDatos, WKPines                                                                                           
                    '-----------------------------------------------------------------------------------------------------------------------------
                    Modificar_Claves(req.terminal, req.nrohost, claves.wkd, claves.wkp)

                ElseIf CInt(.MessageTypeIdentifier.ToString) = 810 Then    '--- RESPUESTA DE SINCRONIZACION
                    Dim claves = Obtener_Claves(MSG)
                    req.msjRespuesta.Clave_wkp = claves.wkp
                    req.msjRespuesta.Clave_wkd = claves.wkd
                    req.msjRespuesta.Clave_ctrlwkp = claves.controlwkp
                    req.msjRespuesta.Clave_ctrlwkd = claves.controlwkd
                    '-----------------------------------------------------------------------------------------------------------------------------
                    '--- CADA VEZ QUE SINCRONIZA - En la tabla numeros UPDATEA los campos  SeriePP, PosicionMK, WKDatos, WKPines y VersionSoftPP  
                    '--- ACA actualiza WKDatos, WKPines                                                                                           
                    '-----------------------------------------------------------------------------------------------------------------------------
                    Modificar_Claves(req.terminal, req.nrohost, claves.wkd, claves.wkp)
                End If

            End If

            If .Fields.Contains(63) Then
                req.msjRespuesta.mensajeHost = .Fields(63).ToString
            Else
                req.msjRespuesta.mensajeHost = ""
            End If
            req.msjRespuesta.CashBack = req.msjIda.importeCashback

            'If req.msjRespuesta.Respuesta = "" Then
            '    If CInt(.MessageTypeIdentifier.ToString) = 410 Then
            '        req.msjRespuesta.Respuesta = "REINTENTE (REV)"
            '    End If
            'End If

            If CInt(.MessageTypeIdentifier.ToString) = 410 Then     '--- RESPUESTA DE REVERSO
                req.msjRespuesta.Respuesta = "REINTENTE (REV)"
            Else
                req.msjRespuesta.Respuesta = String.Format("{0}", ObtenerTxtRepuesta(cr))
            End If



            If req.msjRespuesta.Respuesta = E_CodigosRespuesta.APROBADA.ToString Then
                req.msjRespuesta.Respuesta = req.msjRespuesta.Respuesta + " " + req.autorizacion
            End If

            'If req.coeficiente > 1 Then

            If req.descEmisor IsNot Nothing Then req.msjRespuesta.Emisor = req.descEmisor.Trim
            'req.vta.VtaFileTkt = "P" + req.pinguino + req.nroCaja.ToString("00") + Now.Date.ToString("yyyyMMdd") + Now.TimeOfDay.ToString("HHmm") + ".DAT"
            'req.vta.VtaFileTkt = "TICK00" + req.nroCaja.ToString("00") + ".DAT"
            req.msjRespuesta.Ticket = req.nroTicket   'TICKET DE LA CAJA



            If req.msjRespuesta.codRespuesta = "00" And .MessageTypeIdentifier <> 510 And .MessageTypeIdentifier <> 410 And .MessageTypeIdentifier <> 230 Then
                IncrementarNroTicket(Trim(req.terminal), req.nrohost)
            End If




            If CInt(.MessageTypeIdentifier.ToString) <> 810 Then
                AgregarMovimiento3DES(req)
            End If
            'Dim ClaveRev = ClaveIDA(HST.Name, req.terminal, req.nroTrace)
            'req.msjRespuesta.clavereverso = ClaveRev
            'AgregarAReversosPendientes(ClaveRev, req)

            '---------------------------------------------------------------
            '--- CREA EL TICKET, CUPON PARA LA CAJA, VOUCHER PARA LA CAJA   
            '---------------------------------------------------------------
            If Not req.RespInvalida Then
                If Val(cr) = 0 Then
                    If CInt(.MessageTypeIdentifier) <> 410 And CInt(.MessageTypeIdentifier) <> 230 And CInt(.MessageTypeIdentifier) <> 810 Then req.msjRespuesta.cupon = crearTicketTCP(req)
                Else
                    If CInt(.MessageTypeIdentifier) <> 410 And CInt(.MessageTypeIdentifier) <> 230 And CInt(.MessageTypeIdentifier) <> 810 Then req.msjRespuesta.cupon = crearTickDATNoAprobadoTCP(req)
                End If
            End If

            If CInt(.MessageTypeIdentifier) <> 410 And CInt(.MessageTypeIdentifier) <> 230 Then
                responderAcajaTCP(req)
            ElseIf CInt(.MessageTypeIdentifier) = 410 Then     '--- respuesta reverso
                'crearTickDATReversoTCP(req)
                responderReverso3DES(req)
            ElseIf CInt(.MessageTypeIdentifier) = 230 Then     '--- respuesta advice
                '--- CUANDO VIENE Z3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER NO APROBADO  
                '--- CUANDO VIENE Z1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER NO APROBADO  
                '--- CUANDO VIENE Y3 HAY QUE MANDAR EL ADVICE Y EL REVERSO Y EL TICKET TIENE QUE SER APROBADO  
                '--- CUANDO VIENE Y1 HAY QUE MANDAR EL ADVICE Y EL TICKET TIENE QUE SER APROBADO  
                responderAdvice3DES(req)
            End If

            BorrarDatosCriticos(req)
        End With

    End Sub


    Sub RecuperarRequerimiento(hst As HostTCP, msg As Iso8583Message)

        If msg.Fields(3).ToString = CStr(E_ProcCode.Echotest) Then
            Logger.Info(String.Format("Mensaje de echo recibido de {0}.", hst.Name))
            Exit Sub
        End If
        Dim req As Req
        '---- ARMA LA CLAVE DEL IDA PARA BUSCARLA EN LA LISTA DE PENDIENTES  
        Dim s = ClaveIDA(hst, msg.Fields(41).ToString, msg)

        '--- LO SACA DE LA COLA DE PENDIENTES, EL MISMO REQ QUE MANDÓ AHORA LO TOMA PARA RESPONDER A LA CAJA   
        If ListaReqPendientesOnline.ContainsKey(s) Then
            req = ListaReqPendientesOnline(s)
        Else
            ' No lo tengo en cuenta, porque ya se saco de la lista de pendientes porque no llegó nunca y por ende se reversó; o   
            ' Se reverso al abrir el programa despues de un cierre inesperado.                                                    
            Logger.Error("Requermiento no encontrado, respuesta tardía")
            Exit Sub
        End If

        Logger.Info("Clave: " + s + " CAJA: " + CStr(req.nroCaja) + " PING: " + req.pinguino & vbNewLine &
                        "Demoró " + Now.Subtract(req.FechaHoraEnvioMsg).TotalSeconds.ToString & vbNewLine &
                        "*****   ELIMINANDO DE ListaReqPendientesOnline: " + s)

        generandoVTA = True   '--- es para que no pueda salir hasta que termine de grabar el vta

        '-----------------------------------------------
        '--- ACA LO SACA DE LA LISTA DE PENDIENTES      
        '-----------------------------------------------
        ListaReqPendientesOnline.Remove(s)


        Logger.Info("Operaciones pendientes: " + ListaReqPendientesOnline.Count.ToString)

        If req.tipoRequerimiento = "" Then       '--- entro por archivos (sistema viejo)
            ProcesarMensajeEntrante(hst, msg, req)
        Else 'es un requerimiento TCP
            ProcesarMensaje3DES(hst, msg, req)
        End If


    End Sub

    Sub ProcesarMensajeEntrante(hst As HostTCP, msg As Iso8583Message, req As Req)


        'Logger.Info("Mensaje recibido:  " + msg.ToString)
        With msg
            Dim cr As String = ""
            'ACA ESTA LA RESPUESTA VER QUE HACER, HAY QUE DESARMAR EL PAQUETE
            If CInt(.MessageTypeIdentifier.ToString) > 9000 Then
                Logger.Error("Mensaje recibido no implementado")
                cr = "96"
            Else
                If .Fields.Contains(39) Then cr = .Fields(39).ToString
            End If


            '--- PARA QUE APRUEBE TODO ---------------------
            'If My.Computer.Name = "MARCOS-XP" Then
            '    cr = "00"
            'End If

            '------- ADVICE ------------
            'Dim adv As Req
            'If My.Computer.Name = "MARCOS-XP" Then
            '    If CInt(.MessageTypeIdentifier.ToString) > 230 Then
            '        Dim A = ClaveADV(hst, msg.Fields(41).ToString)
            '        If ListaAdvPendientesOnline.ContainsKey(A) Then
            '            adv = ListaAdvPendientesOnline(A)
            '            ListaAdvPendientesOnline.Remove(A)
            '            Logger.Info("Advice pendientes: " + ListaAdvPendientesOnline.Count.ToString)
            '        End If
            '    End If
            'End If

            'Si es una respuesta a echotest no me interesa.
            If .Fields(3).ToString = CStr(E_ProcCode.Echotest) Then
                Logger.Info(String.Format("Mensaje de echo recibido de {0}.", hst.Name))
                Exit Sub
            End If


            req.vta.VtaOk = CInt(Val(cr))
            req.vta.VtaCodResp = cr

            req.tipoMensaje = .MessageTypeIdentifier.ToString("0000")
            If .Fields.Contains(37) Then
                req.retrefnumber = .Fields(37).ToString
            Else
                req.retrefnumber = ""
            End If
            req.idred = CInt(.Fields(24).ToString)

            If .Fields.Contains(34) Then req.nrocuenta = .Fields(34).ToString 'nro de cuenta visa


            If .Fields.Contains(52) Then ' WORKING KEY. 
                Dim bcdtmp As String
                Dim bytes As Integer = 8
                bcdtmp = ""
                Try

                    bcdtmp = ByteArrayToString(msg.Fields(52).GetBytes)
                Catch ex As Exception
                    bcdtmp = "1234123412341234"
                    Logger.Error(String.Format("Error decodificando wkey: {1} ping: {0}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
                End Try
                GrabarWkey(bcdtmp, CInt(req.pinguino).ToString("00") & req.nroCaja.ToString("00"), req.ida.cajadir)
            End If
            If .Fields.Contains(63) Then
                req.vta.VtaMensaje = .Fields(63).ToString
            Else
                req.vta.VtaMensaje = ""
            End If

            If .Fields.Contains(55) Then
                req.vta.VtaCripto = .Fields(55).ToString.PadRight(300)
            Else
                req.vta.VtaCripto = "".PadRight(300)
            End If

            req.vta.VtaCashBack = req.ida.CASHBACK

            If req.vta.VtaMensaje = "" Then
                If CInt(.MessageTypeIdentifier.ToString) = 410 Then
                    req.vta.VtaMensaje = "REINTENTE (REV)"
                End If
            End If


            If CInt(.MessageTypeIdentifier.ToString) = 410 Then
                req.vta.VtaMenResp = "REINTENTE (REV)"
            Else
                req.vta.VtaMenResp = String.Format("{0}", ObtenerTxtRepuesta(cr))
            End If

            If .Fields.Contains(38) Then
                req.autorizacion = .Fields(38).ToString
                req.vta.VtaAutorizacion = .Fields(38).ToString
            Else
                req.autorizacion = ""
            End If

            If req.vta.VtaMenResp = E_CodigosRespuesta.APROBADA.ToString Then
                req.vta.VtaMenResp = req.vta.VtaMenResp + " " + req.autorizacion
            End If

            'If req.coeficiente > 1 Then
            req.vta.VtaMontop = CStr(CInt(req.importe * 100)).PadLeft(8)
            req.vta.VtaEmiName = req.descEmisor.Trim
            'req.vta.VtaFileTkt = "P" + req.pinguino + req.nroCaja.ToString("00") + Now.Date.ToString("yyyyMMdd") + Now.TimeOfDay.ToString("HHmm") + ".DAT"
            req.vta.VtaFileTkt = "TICK00" + req.nroCaja.ToString("00") + ".DAT"
            req.vta.VtaTicket = req.nroTicket   'TICKET DE LA CAJA

            If req.vta.VtaOk = 0 And .MessageTypeIdentifier <> 510 And .MessageTypeIdentifier <> 410 And .MessageTypeIdentifier <> 230 Then
                IncrementarNroTicket(Trim(req.terminal), req.nrohost)
            End If

            AgregarMovimiento(req)
            'If CInt(req.vta.VtaCodResp) = E_CodigosRespuesta.APROBADA Then
            '    AgregarUltimaAprobada(req)
            'End If

            ' grabar el ticket si corresponde y el vta
            If Not req.RespInvalida Then
                If Val(cr) = 0 Then
                    If CInt(.MessageTypeIdentifier) <> 410 And CInt(.MessageTypeIdentifier) <> 230 Then crearTickDAT(req)
                Else
                    crearTickDATNoAprobado(req)
                End If

            End If

            If CInt(.MessageTypeIdentifier) <> 410 And CInt(.MessageTypeIdentifier) <> 230 Then
                'If My.Computer.Name = "MARCOS-XP" Then
                '    If req.ida.MANUAL = 5 Then
                '        AgregarAdvPendientes(ClaveVta(req.pinguino, req.nroCaja), req)
                '    End If
                'End If
                'responderAcajaTCP(req)
                'Else
                responderAcaja(req)


                ' End If


            End If
            BorrarDatosCriticos(req)
        End With

    End Sub

    ''' <summary>
    ''' Este se usaba antes del 3des.
    ''' </summary>
    ''' <param name="hst"></param>
    ''' <param name="msg"></param>
    Sub ProcesarMensajeEntrante(hst As HostTCP, msg As Iso8583Message)


        'Logger.Info("Mensaje recibido:  " + msg.ToString)
        With msg
            Dim cr As String = ""
            'ACA ESTA LA RESPUESTA VER QUE HACER, HAY QUE DESARMAR EL PAQUETE
            If CInt(.MessageTypeIdentifier.ToString) > 9000 Then
                Logger.Error("Mensaje recibido no implementado")
                cr = "96"
            Else
                If .Fields.Contains(39) Then cr = .Fields(39).ToString
            End If


            '--- PARA QUE APRUEBE TODO ---------------------
            'If My.Computer.Name = "MARCOS-XP" Then cr = "00"

            '------- ADVICE ------------
            'Dim adv As Req
            'If My.Computer.Name = "MARCOS-XP" Then
            '    If CInt(.MessageTypeIdentifier.ToString) > 230 Then
            '        Dim A = ClaveADV(hst, msg.Fields(41).ToString)
            '        If ListaAdvPendientesOnline.ContainsKey(A) Then
            '            adv = ListaAdvPendientesOnline(A)
            '            ListaAdvPendientesOnline.Remove(A)
            '            Logger.Info("Advice pendientes: " + ListaAdvPendientesOnline.Count.ToString)
            '        End If
            '    End If
            'End If

            'Si es una respuesta a echotest no me interesa.
            If .Fields(3).ToString = CStr(E_ProcCode.Echotest) Then
                Logger.Info(String.Format("Mensaje de echo recibido de {0}.", hst.Name))
                Exit Sub
            End If
            Dim req As Req
            ' buscar req desde la lista de pendientes
            Dim s = ClaveIDA(hst, msg.Fields(41).ToString, msg)

            If ListaReqPendientesOnline.ContainsKey(s) Then
                req = ListaReqPendientesOnline(s)
            Else
                ' No lo tengo en cuenta, porque ya se saco de la lista de pendientes porque no llegó nunca y por ende se reversó; o 
                ' Se reverso al abrir el programa despues de un cierre inesperado.
                Logger.Error("Requermiento no encontrado, respuesta tardía")
                Exit Sub
            End If


            Logger.Info("Clave: " + s + " CAJA: " + CStr(req.nroCaja) + " PING: " + req.pinguino & vbNewLine &
                        "Demoró " + Now.Subtract(req.FechaHoraEnvioMsg).TotalSeconds.ToString & vbNewLine &
                        "*****   ELIMINANDO DE ListaReqPendientesOnline: " + s)

            generandoVTA = True 'es para que no pueda salir hasta que termine de grabar el vta
            ListaReqPendientesOnline.Remove(s)
            Logger.Info("Operaciones pendientes: " + ListaReqPendientesOnline.Count.ToString)


            req.vta.VtaOk = CInt(Val(cr))
            req.vta.VtaCodResp = cr

            req.tipoMensaje = .MessageTypeIdentifier.ToString("0000")
            If .Fields.Contains(37) Then
                req.retrefnumber = .Fields(37).ToString
            Else
                req.retrefnumber = ""
            End If
            req.idred = CInt(.Fields(24).ToString)

            If .Fields.Contains(34) Then req.nrocuenta = .Fields(34).ToString 'nro de cuenta visa


            If .Fields.Contains(52) Then ' WORKING KEY. 
                Dim bcdtmp As String
                Dim bytes As Integer = 8
                bcdtmp = ""
                Try

                    bcdtmp = ByteArrayToString(msg.Fields(52).GetBytes)
                Catch ex As Exception
                    bcdtmp = "1234123412341234"
                    Logger.Error(String.Format("Error decodificando wkey: {1} ping: {0}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
                End Try
                GrabarWkey(bcdtmp, CInt(req.pinguino).ToString("00") & req.nroCaja.ToString("00"), req.ida.cajadir)
            End If
            If .Fields.Contains(63) Then
                req.vta.VtaMensaje = .Fields(63).ToString
            Else
                req.vta.VtaMensaje = ""
            End If

            If .Fields.Contains(55) Then
                req.vta.VtaCripto = .Fields(55).ToString.PadRight(300)
            Else
                req.vta.VtaCripto = "".PadRight(300)
            End If

            req.vta.VtaCashBack = req.ida.CASHBACK

            If req.vta.VtaMensaje = "" Then
                If CInt(.MessageTypeIdentifier.ToString) = 410 Then
                    req.vta.VtaMensaje = "REINTENTE (REV)"
                End If
            End If


            If CInt(.MessageTypeIdentifier.ToString) = 410 Then
                req.vta.VtaMenResp = "REINTENTE (REV)"
            Else
                req.vta.VtaMenResp = String.Format("{0}", ObtenerTxtRepuesta(cr))
            End If

            If .Fields.Contains(38) Then
                req.autorizacion = .Fields(38).ToString
                req.vta.VtaAutorizacion = .Fields(38).ToString
            Else
                req.autorizacion = ""
            End If

            If req.vta.VtaMenResp = E_CodigosRespuesta.APROBADA.ToString Then
                req.vta.VtaMenResp = req.vta.VtaMenResp + " " + req.autorizacion
            End If

            'If req.coeficiente > 1 Then
            req.vta.VtaMontop = CStr(CInt(req.importe * 100)).PadLeft(8)
            req.vta.VtaEmiName = req.descEmisor.Trim
            'req.vta.VtaFileTkt = "P" + req.pinguino + req.nroCaja.ToString("00") + Now.Date.ToString("yyyyMMdd") + Now.TimeOfDay.ToString("HHmm") + ".DAT"
            req.vta.VtaFileTkt = "TICK00" + req.nroCaja.ToString("00") + ".DAT"
            req.vta.VtaTicket = req.nroTicket   'TICKET DE LA CAJA

            If req.vta.VtaOk = 0 And .MessageTypeIdentifier <> 510 And .MessageTypeIdentifier <> 410 And .MessageTypeIdentifier <> 230 Then
                IncrementarNroTicket(Trim(req.terminal), req.nrohost)
            End If

            AgregarMovimiento(req)
            'If CInt(req.vta.VtaCodResp) = E_CodigosRespuesta.APROBADA Then
            '    AgregarUltimaAprobada(req)
            'End If

            ' grabar el ticket si corresponde y el vta
            If Not req.RespInvalida Then
                If Val(cr) = 0 Then
                    If CInt(.MessageTypeIdentifier) <> 410 And CInt(.MessageTypeIdentifier) <> 230 Then crearTickDAT(req)
                Else
                    crearTickDATNoAprobado(req)
                End If

            End If

            If CInt(.MessageTypeIdentifier) <> 410 And CInt(.MessageTypeIdentifier) <> 230 Then
                'If My.Computer.Name = "MARCOS-XP" Then
                '    If req.ida.MANUAL = 5 Then
                '        AgregarAdvPendientes(ClaveVta(req.pinguino, req.nroCaja), req)
                '    End If
                'End If
                'responderAcajaTCP(req)
                'Else
                responderAcaja(req)


                ' End If


            End If
            BorrarDatosCriticos(req)
        End With

    End Sub


    Sub ProcesarMensajeEntrantePEI(hst As String, req As Req, resPEI As lib_PEI.Respuesta)

        Logger.Info("**************************************************************************")
        Logger.Info("Mensaje recibido:  " + resPEI.ToString)
        Logger.Info("Mensaje recibido id_operacion:  " + resPEI.id_operacion)
        Logger.Info("**************************************************************************")

        Dim cr As String = ""
        If resPEI.descRespuesta = mensajesRepuesta.APROBADA Then
            cr = "00"
        Else
            cr = CType(resPEI.descRespuesta, Integer).ToString("00")
        End If

        req.vta.VtaOk = CInt(Val(cr))
        req.vta.VtaCodResp = cr
        req.vta.VtaMensaje = resPEI.descripcion
        req.autorizacion = resPEI.id_operacion
        req.vta.VtaAutorizacion = resPEI.id_operacion

        'IMPORTE_INCORRECTO
        'ULTIMOS_4_DIGITOS_INCORRECTO
        'TARJETA_INHABILITADA
        'TARJETA_VENCIDA
        'FONDOS_INSUFICIENTES

        If resPEI.descRespuesta.ToString = lib_PEI.mensajesRepuesta.IMPORTE_INCORRECTO.ToString Or
                resPEI.descRespuesta.ToString = lib_PEI.mensajesRepuesta.ULTIMOS_4_DIGITOS_INCORRECTO.ToString Or
                resPEI.descRespuesta.ToString = lib_PEI.mensajesRepuesta.TARJETA_INHABILITADA.ToString Or
                resPEI.descRespuesta.ToString = lib_PEI.mensajesRepuesta.TARJETA_INCORRECTA.ToString Or
                resPEI.descRespuesta.ToString = lib_PEI.mensajesRepuesta.TARJETA_VENCIDA.ToString Or
                resPEI.descRespuesta.ToString = lib_PEI.mensajesRepuesta.FONDOS_INSUFICIENTES.ToString Or
                resPEI.descRespuesta.ToString = lib_PEI.mensajesRepuesta.DNI_INCORRECTO.ToString Then
            req.vta.VtaMenResp = "PEI_NOAPROBADA" 'es el mensaje del error
        Else

            'req.vta.VtaMenResp = [Enum].GetName(GetType(lib_PEI.mensajesRepuesta), resPEI.descRespuesta)
            req.vta.VtaMenResp = resPEI.descRespuesta.ToString

        End If

        If req.vta.VtaOk = 0 Then
            IncrementarNroTicket(Trim(req.terminal), req.nrohost)
        End If

        If req.vta.VtaMenResp = lib_PEI.mensajesRepuesta.APROBADA.ToString Then
            req.vta.VtaMenResp = req.vta.VtaMenResp + " " + req.autorizacion
        End If
        req.nrocuenta = resPEI.nro_ref_bancaria

        'If req.coeficiente > 1 Then
        req.vta.VtaMontop = CStr(CInt(req.importe * 100)).PadLeft(8)
        req.vta.VtaEmiName = req.descEmisor.Trim
        'req.vta.VtaFileTkt = "P" + req.pinguino + req.nroCaja.ToString("00") + Now.Date.ToString("yyyyMMdd") + Now.TimeOfDay.ToString("HHmm") + ".DAT"
        req.vta.VtaFileTkt = "TICK00" + req.nroCaja.ToString("00") + ".DAT"
        req.vta.VtaTicket = req.nroTicket   'TICKET DE LA CAJA


        ActualizaMovimientoPEI(req)


        ' grabar el ticket si corresponde y el vta
        If Not req.RespInvalida Then
            If Val(cr) = 0 Then
                crearTickPEI(req)
            Else
                crearTickDATNoAprobadoPEI(req)
            End If

        End If


        responderAcaja(req)

        '        BorrarDatosCriticos(req)


    End Sub


    ''' <summary>
    ''' Procedimiento para convertir array de byte a string. (Pin encriptado)
    ''' </summary>
    ''' <param name="ba"></param>
    ''' <returns></returns>
    Public Shared Function ByteArrayToString(ba As Byte()) As String
        Dim hex As New StringBuilder(ba.Length * 2)
        For Each b As Byte In ba
            hex.AppendFormat("{0:x2}", b)
        Next
        Return hex.ToString().ToUpper
    End Function

    ''' <summary>
    ''' Procedimiento para convertir array de byte a string, dando inicio y fin. (Pin encriptado)
    ''' </summary>
    ''' <param name="ba"></param>
    ''' <param name="inicio"></param>
    ''' <param name="longitud"></param>
    ''' <returns></returns>
    Public Shared Function ByteArrayToString(ba As Byte(), inicio As Integer, longitud As Integer) As String
        Dim hex As New StringBuilder(longitud * 2)
        Dim c = 0
        For Each b As Byte In ba
            If c >= inicio And c <= longitud + 1 Then
                hex.AppendFormat("{0:x2}", b)
            End If
            c += 1
        Next
        Return hex.ToString().ToUpper
    End Function

    Private Function crearTickDATReversoTCP(req As Req) As String
        Dim ticketstring As New StringBuilder()

        '--- le puse que demore un segundo por si el ticket original, tiene la misma hora, entonces tiene el mismo nombre 
        System.Threading.Thread.Sleep(1000)

        Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + req.msjIda.ticketCaja.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
        Try
            Logger.Info(String.Format("Creando {0}", nombreCupon))

            Dim archivoTick = New System.IO.FileStream(Configuracion.DirLocal + "\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
            'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglonTick = New System.IO.StreamWriter(archivoTick)
            With req
                ticketstring.AppendLine("             " + .msjRespuesta.Emisor)
                'If .msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Then  '"compra_cashback" Then
                '    ticketstring.AppendLine("       COMPRA + EXTRACCION")
                'Else
                '    ticketstring.AppendLine(TipoOperacion(.operacion))
                'End If
                ticketstring.AppendLine("             PINGUINO " + req.pinguino)

                ticketstring.AppendLine("             CAJA: " + .nroCaja.ToString("00"))
                ticketstring.AppendLine("Nro.Com.:" + .idComercio.ToString.PadRight(12) + "  Term:" + .terminal.PadRight(8))
                ticketstring.AppendLine("Nro. de Lote:" + .nroLote.ToString.PadRight(3) + "   Nro. de Cupon:" + .nroTicket.ToString.PadRight(4))

                If .nrocuenta = "*" Then
                    ticketstring.AppendLine(Trim(.msjIda.nroTarjeta) + "      (" + .msjIda.tipoIngreso.ToString + ")  **/**")
                Else
                    ticketstring.AppendLine(enmascarar(Trim(.msjIda.nroTarjeta), 0, Trim(.msjIda.nroTarjeta).Length - 4) + "      (" + .msjIda.tipoIngreso.ToString + ")  **/**")
                End If

                If .msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Then
                    ticketstring.AppendLine("Compra: $" + (Math.Round(.msjRespuesta.Importe - .msjRespuesta.CashBack, 2).ToString("###,##0.00") + ".-"))
                    ticketstring.AppendLine("Extraccion: $" + (.msjRespuesta.CashBack).ToString("###,##0.00") + ".-")
                End If

                ticketstring.AppendLine("Importe TOTAL: $" + (Math.Round(.msjRespuesta.Importe, 2)).ToString("###,##0.00") + ".-")
                ticketstring.AppendLine("Cuotas: " + .cuotas.ToString("00"))

                'ticketstring.AppendLine("Nro de comprobante:" + .msjIda.ticketCaja.ToString)

                ticketstring.AppendLine("****************************************")
                ticketstring.AppendLine(" OPERACION REVERSADA ")
                ticketstring.AppendLine("****************************************")

            End With

            renglonTick.Write(ticketstring.ToString)
            renglonTick.Close()

        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
        End Try

        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon))
        End Try
        Return ticketstring.ToString
    End Function


    Private Function crearTickDATAdviceTCP(req As Req) As String
        Dim ticketstring As New StringBuilder()
        Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + req.msjIda.ticketCaja.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
        Try
            Logger.Info(String.Format("Creando {0}", nombreCupon))

            Dim archivoTick = New System.IO.FileStream(Configuracion.DirLocal + "\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
            'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglonTick = New System.IO.StreamWriter(archivoTick)
            With req
                ticketstring.AppendLine("             " + .msjRespuesta.Emisor)
                If .msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Then  '"compra_cashback" Then
                    ticketstring.AppendLine("       COMPRA + EXTRACCION")
                Else
                    ticketstring.AppendLine(TipoOperacion(.operacion))
                End If
                ticketstring.AppendLine("             PINGUINO " + req.pinguino)

                ticketstring.AppendLine("             CAJA: " + .nroCaja.ToString("00"))
                ticketstring.AppendLine("Nro.Com.:" + .idComercio.ToString.PadRight(12) + "  Term:" + .terminal.PadRight(8))
                ticketstring.AppendLine("Nro. de Lote:" + .nroLote.ToString.PadRight(3) + "   Nro. de Cupon:" + .nroTicket.ToString.PadRight(4))

                If .nrocuenta = "*" Then
                    ticketstring.AppendLine(Trim(.msjIda.nroTarjeta) + "      (" + .msjIda.tipoIngreso.ToString + ")  **/**")
                Else
                    ticketstring.AppendLine(enmascarar(Trim(.msjIda.nroTarjeta), 0, Trim(.msjIda.nroTarjeta).Length - 4) + "      (" + .msjIda.tipoIngreso.ToString + ")  **/**")
                End If

                If .msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Then
                    ticketstring.AppendLine("Compra: $" + (Math.Round(.msjRespuesta.Importe - .msjRespuesta.CashBack, 2).ToString("###,##0.00") + ".-"))
                    ticketstring.AppendLine("Extraccion: $" + (.msjRespuesta.CashBack).ToString("###,##0.00") + ".-")
                End If

                ticketstring.AppendLine("Importe TOTAL: $" + (Math.Round(.msjRespuesta.Importe, 2)).ToString("###,##0.00") + ".-")
                ticketstring.AppendLine("Cuotas: " + .cuotas.ToString("00"))

                ticketstring.AppendLine("Nro de comprobante:" + .msjIda.ticketCaja.ToString)

                ticketstring.AppendLine("****************************************")
                ticketstring.AppendLine("      N O  H A Y  R E S P U E S T A ")
                ticketstring.AppendLine("   R E I N T E N T E   M A S   T A R D E ")
                ticketstring.AppendLine("              (ADVICE)")
                ticketstring.AppendLine("****************************************")

            End With

            renglonTick.Write(ticketstring.ToString)
            renglonTick.Close()

        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
        End Try

        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon))
        End Try
        Return ticketstring.ToString
    End Function


    Private Function crearTickDATAdviceTCP_Y1(req As Req) As String
        Dim ticketstring As New StringBuilder()
        Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + req.msjIda.ticketCaja.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
        Try
            Logger.Info(String.Format("Creando {0}", nombreCupon))

            Dim archivoTick = New System.IO.FileStream(Configuracion.DirLocal + "\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
            'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglonTick = New System.IO.StreamWriter(archivoTick)
            With req
                ticketstring.AppendLine("             " + .msjRespuesta.Emisor)
                'If .msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Then  '"compra_cashback" Then
                '    ticketstring.AppendLine("       COMPRA + EXTRACCION")
                'Else
                '    ticketstring.AppendLine(TipoOperacion(.operacion))
                'End If
                ticketstring.AppendLine("              COMPRA")

                ticketstring.AppendLine("             PINGUINO " + req.pinguino)

                ticketstring.AppendLine("             CAJA: " + .nroCaja.ToString("00"))
                ticketstring.AppendLine("Nro.Com.:" + .idComercio.ToString.PadRight(12) + "  Term:" + .terminal.PadRight(8))
                ticketstring.AppendLine("Nro. de Lote:" + .nroLote.ToString.PadRight(3) + "   Nro. de Cupon:" + .nroTicket.ToString.PadRight(4))

                If .nrocuenta = "*" Then
                    ticketstring.AppendLine(Trim(.msjIda.nroTarjeta) + "      (" + .msjIda.tipoIngreso.ToString + ")  **/**")
                Else
                    ticketstring.AppendLine(enmascarar(Trim(.msjIda.nroTarjeta), 0, Trim(.msjIda.nroTarjeta).Length - 4) + "      (" + .msjIda.tipoIngreso.ToString + ")  **/**")
                End If

                ticketstring.AppendLine("AUTORIZACON OFFLINE: Y1")

                'If .msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Then
                '    ticketstring.AppendLine("Compra: $" + (Math.Round(.msjRespuesta.Importe - .msjRespuesta.CashBack, 2).ToString("###,##0.00") + ".-"))
                '    ticketstring.AppendLine("Extraccion: $" + (.msjRespuesta.CashBack).ToString("###,##0.00") + ".-")
                'End If

                ticketstring.AppendLine("Importe TOTAL: $" + (Math.Round(.msjRespuesta.Importe, 2)).ToString("###,##0.00") + ".-")
                ticketstring.AppendLine("Cuotas: " + .cuotas.ToString("00"))

                ticketstring.AppendLine("Nro de comprobante:" + .msjIda.ticketCaja.ToString)

                If .msjIda.nombreApplicacion <> "" Then ticketstring.AppendLine("Nombre de aplicacion: " + .msjIda.nombreApplicacion)
                If .msjIda.appID <> "" Then ticketstring.AppendLine("ID Aplicacion: " + .msjIda.appID)

                ticketstring.AppendLine("****************************************")
                ticketstring.AppendLine("           A P R O B A D O ")
                ticketstring.AppendLine("****************************************")

            End With

            renglonTick.Write(ticketstring.ToString)
            renglonTick.Close()

        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
        End Try

        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon))
        End Try
        Return ticketstring.ToString
    End Function





    Private Function crearTickDATNoAprobadoTCP(req As Req) As String
        Dim ticketstring As New StringBuilder()
        Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + req.msjIda.ticketCaja.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
        Try
            Logger.Info(String.Format("Creando {0}", nombreCupon))

            Dim archivoTick = New System.IO.FileStream(Configuracion.DirLocal + "\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
            'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglonTick = New System.IO.StreamWriter(archivoTick)
            With req
                ticketstring.AppendLine("             " + .msjRespuesta.Emisor)
                If .msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Then  '"compra_cashback" Then
                    ticketstring.AppendLine("       COMPRA + EXTRACCION")
                Else
                    ticketstring.AppendLine(TipoOperacion(.operacion))
                End If
                ticketstring.AppendLine("             PINGUINO " + req.pinguino)

                ticketstring.AppendLine("             CAJA: " + .nroCaja.ToString("00"))
                ticketstring.AppendLine("Nro.Com.:" + .idComercio.ToString.PadRight(12) + "  Term:" + .terminal.PadRight(8))
                ticketstring.AppendLine("Nro. de Lote:" + .nroLote.ToString.PadRight(3) + "   Nro. de Cupon:" + .nroTicket.ToString.PadRight(4))

                If .nrocuenta = "*" Then
                    ticketstring.AppendLine(Trim(.msjIda.nroTarjeta) + "      (" + .msjIda.tipoIngreso.ToString + ")  **/**")
                Else
                    ticketstring.AppendLine(enmascarar(Trim(.msjIda.nroTarjeta), 0, Trim(.msjIda.nroTarjeta).Length - 4) + "      (" + .msjIda.tipoIngreso.ToString + ")  **/**")
                End If

                If .msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Then
                    ticketstring.AppendLine("Compra: $" + (Math.Round(.msjRespuesta.Importe - .msjRespuesta.CashBack, 2).ToString("###,##0.00") + ".-"))
                    ticketstring.AppendLine("Extraccion: $" + (.msjRespuesta.CashBack).ToString("###,##0.00") + ".-")
                End If

                ticketstring.AppendLine("Importe TOTAL: $" + (Math.Round(.msjRespuesta.Importe, 2)).ToString("###,##0.00") + ".-")
                ticketstring.AppendLine("Cuotas: " + .cuotas.ToString("00"))

                ticketstring.AppendLine("Nro de comprobante:" + .msjIda.ticketCaja.ToString)

                ticketstring.AppendLine("****************************************")
                ticketstring.AppendLine("       R  E  C  H  A  Z  A  D  O")
                ticketstring.AppendLine("   " + Mid(.msjRespuesta.Respuesta, 1, 37))
                ticketstring.AppendLine(" " + Mid(.msjRespuesta.mensajeHost, 1, 39))
                ticketstring.AppendLine("****************************************")

            End With

            renglonTick.Write(ticketstring.ToString)
            renglonTick.Close()

        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
        End Try

        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon))
        End Try
        Return ticketstring.ToString
    End Function

    Private Sub crearTickDATInvalido(req As Req)
        Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + req.ida.TICCAJ.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
        Try
            Logger.Info(String.Format("Creando {0}", nombreCupon))

            Dim archivoTick = New System.IO.FileStream(Configuracion.DirLocal + "\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
            'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglonTick = New System.IO.StreamWriter(archivoTick)
            With req
                renglonTick.WriteLine("             " + .vta.VtaEmiName)
                If .operacion = E_ProcCode.compra_cashback Then  '"compra_cashback" Then
                    renglonTick.WriteLine("       COMPRA + EXTRACCION")
                Else
                    renglonTick.WriteLine("             " + .operacion.ToString)
                End If
                renglonTick.WriteLine("             PINGUINO " + req.pinguino)

                renglonTick.WriteLine("             CAJA: " + .nroCaja.ToString("00"))
                renglonTick.WriteLine("Nro.Com.:" + .idComercio.ToString.PadRight(12) + "  Term:" + .terminal.PadRight(8))
                renglonTick.WriteLine("Nro. de Lote:" + .nroLote.ToString.PadRight(3) + "   Nro. de Cupon:" + .nroTicket.ToString.PadRight(4))

                If .nrocuenta = "*" Then
                    renglonTick.WriteLine(Trim(req.ida.TARJ) + "      (" + ObtenerTxtModoIngreso(req.ida.MANUAL) + ")  **/**")
                Else
                    renglonTick.WriteLine(enmascarar(Trim(req.ida.TARJ), 0, Trim(req.ida.TARJ).Length - 4) + "      (" + ObtenerTxtModoIngreso(req.ida.MANUAL) + ")  **/**")
                End If

                If .operacion = E_ProcCode.compra_cashback Then
                    renglonTick.WriteLine("Compra: $" + (Math.Round((CInt(.vta.VtaMontop) / 100) - req.ida.CASHBACK, 2)).ToString("###,##0.00") + ".-")
                    renglonTick.WriteLine("Extraccion: $" + (req.ida.CASHBACK).ToString("###,##0.00") + ".-")
                End If

                renglonTick.WriteLine("Importe TOTAL: $" + (Math.Round(CInt(.vta.VtaMontop) / 100, 2)).ToString("###,##0.00") + ".-")
                renglonTick.WriteLine("Cuotas: " + req.cuotas.ToString("00"))

                renglonTick.WriteLine("Nro de comprobante:" + .ida.TICCAJ.ToString)

                renglonTick.WriteLine("****************************************")
                renglonTick.WriteLine("       R  E  C  H  A  Z  A  D  O")
                renglonTick.WriteLine("   " + Mid(req.vta.VtaMenResp, 1, 37))
                renglonTick.WriteLine(" " + Mid(req.vta.VtaMensaje, 1, 39))
                renglonTick.WriteLine("****************************************")

            End With
            renglonTick.Close()

        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
        End Try

        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            If System.IO.File.Exists(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT") Then
                System.IO.File.Delete(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            End If
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo renombrar cupón {0}", nombreCupon))
        End Try

        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon))
        End Try

    End Sub

    Private Sub crearTickDATNoAprobadoPEI(req As Req)
        Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + req.ida.TICCAJ.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
        Try
            Logger.Info(String.Format("Creando {0}", nombreCupon))

            Dim archivoTick = New System.IO.FileStream(Configuracion.DirLocal + "\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
            'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglonTick = New System.IO.StreamWriter(archivoTick)
            With req
                'renglonTick.WriteLine("             " + .vta.VtaEmiName)
                If .vta.VtaEmiName = "VISA" Then
                    renglonTick.WriteLine("             VISA DEBITO")
                Else
                    renglonTick.WriteLine("             " + .vta.VtaEmiName)
                End If

                If .OperacionPEI = "COMPRA_DE_BIENES" Then
                    renglonTick.WriteLine("                COMPRA          (PEI)")
                Else
                    renglonTick.WriteLine("           " + .OperacionPEI)
                End If

                renglonTick.WriteLine("             PINGUINO " + req.pinguino)

                renglonTick.WriteLine("             CAJA: " + .nroCaja.ToString("00"))
                renglonTick.WriteLine("Nro.Com.:" + .idComercio.ToString.PadRight(12) + "  Term:" + .terminal.PadRight(8))
                renglonTick.WriteLine("Nro. de Cupon:" + .nroTicket.ToString.PadRight(4))

                renglonTick.WriteLine(enmascarar(Trim(req.ida.TARJ), 0, Trim(req.ida.TARJ).Length - 4) + "      (PEI)")
                renglonTick.WriteLine("Importe TOTAL: $" + (Math.Round(CInt(.vta.VtaMontop) / 100, 2)).ToString("###,##0.00") + ".-")


                renglonTick.WriteLine("Nro de comprobante:" + .ida.TICCAJ.ToString)

                renglonTick.WriteLine("****************************************")
                renglonTick.WriteLine("       R  E  C  H  A  Z  A  D  O")
                renglonTick.WriteLine("   " + Mid(req.vta.VtaMenResp, 1, 37))
                renglonTick.WriteLine(" " + Mid(req.vta.VtaMensaje, 1, 39))
                renglonTick.WriteLine("****************************************")

            End With
            renglonTick.Close()

        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
        End Try

        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            If System.IO.File.Exists(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT") Then
                System.IO.File.Delete(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            End If
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo renombrar cupón {0}", nombreCupon))
        End Try

        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon))
        End Try

    End Sub


    Private Sub crearTickDATNoAprobado(req As Req)
        Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + req.ida.TICCAJ.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
        Try
            Logger.Info(String.Format("Creando {0}", nombreCupon))

            Dim archivoTick = New System.IO.FileStream(Configuracion.DirLocal + "\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
            'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglonTick = New System.IO.StreamWriter(archivoTick)
            With req
                renglonTick.WriteLine("             " + .vta.VtaEmiName)
                If .operacion = E_ProcCode.compra_cashback Then  '"compra_cashback" Then
                    renglonTick.WriteLine("       COMPRA + EXTRACCION")
                Else
                    renglonTick.WriteLine("             " + .operacion.ToString)
                End If
                renglonTick.WriteLine("             PINGUINO " + req.pinguino)

                renglonTick.WriteLine("             CAJA: " + .nroCaja.ToString("00"))
                renglonTick.WriteLine("Nro.Com.:" + .idComercio.ToString.PadRight(12) + "  Term:" + .terminal.PadRight(8))
                renglonTick.WriteLine("Nro. de Lote:" + .nroLote.ToString.PadRight(3) + "   Nro. de Cupon:" + .nroTicket.ToString.PadRight(4))

                If .nrocuenta = "*" Then
                    renglonTick.WriteLine(Trim(req.ida.TARJ) + "      (" + ObtenerTxtModoIngreso(req.ida.MANUAL) + ")  **/**")
                Else
                    renglonTick.WriteLine(enmascarar(Trim(req.ida.TARJ), 0, Trim(req.ida.TARJ).Length - 4) + "      (" + ObtenerTxtModoIngreso(req.ida.MANUAL) + ")  **/**")
                End If

                If .operacion = E_ProcCode.compra_cashback Then
                    renglonTick.WriteLine("Compra: $" + (Math.Round((CInt(.vta.VtaMontop) / 100) - req.ida.CASHBACK, 2)).ToString("###,##0.00") + ".-")
                    renglonTick.WriteLine("Extraccion: $" + (req.ida.CASHBACK).ToString("###,##0.00") + ".-")
                End If

                renglonTick.WriteLine("Importe TOTAL: $" + (Math.Round(CInt(.vta.VtaMontop) / 100, 2)).ToString("###,##0.00") + ".-")
                renglonTick.WriteLine("Cuotas: " + req.cuotas.ToString("00"))

                renglonTick.WriteLine("Nro de comprobante:" + .ida.TICCAJ.ToString)

                renglonTick.WriteLine("****************************************")
                renglonTick.WriteLine("       R  E  C  H  A  Z  A  D  O")
                renglonTick.WriteLine("   " + Mid(req.vta.VtaMenResp, 1, 37))
                renglonTick.WriteLine(" " + Mid(req.vta.VtaMensaje, 1, 39))
                renglonTick.WriteLine("****************************************")

            End With
            renglonTick.Close()

        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
        End Try

        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            If System.IO.File.Exists(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT") Then
                System.IO.File.Delete(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            End If
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo renombrar cupón {0}", nombreCupon))
        End Try

        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon))
        End Try

    End Sub

    Public Function Consultar_pendientes() As Integer
        Return ListaReqPendientesOnline.Count
    End Function
    Public Function Consultar_Confirmados() As Integer
        Return ListaVtaPendientes.Count
    End Function

    Public Function ObtenerTxtRepuesta(ByVal s As String) As String
        Dim cr As E_CodigosRespuesta
        Try
            cr = CType(CInt(s), E_CodigosRespuesta)
            Return cr.ToString
        Catch ex As Exception
            Return "Cod.Respuesta No Contemplado"
        End Try
    End Function

    Public Function ObtenerTxtModoIngreso(ByVal s As Short) As String
        Dim cr As E_ModoIngreso
        Try
            cr = CType(s, E_ModoIngreso)
            Return cr.ToString
        Catch ex As Exception
            Return "Cod.Respuesta No Contemplado"
        End Try
    End Function


    Sub actualizarTablasLocales()
        If MsgBox("Actualiza BINES?", vbYesNo) = MsgBoxResult.Yes Then actualizarBINES(Me)
        If MsgBox("Actualiza Diaspromos?", vbYesNo) = MsgBoxResult.Yes Then actualizarDiasPromos(Me)
        If MsgBox("Actualiza Emisor?", vbYesNo) = MsgBoxResult.Yes Then actualizarEmisor(Me)
        If MsgBox("Actualiza Planes?", vbYesNo) = MsgBoxResult.Yes Then actualizarPlanes(Me)
        If MsgBox("Actualiza Ranghab?", vbYesNo) = MsgBoxResult.Yes Then actualizarRanghab(Me)
        'If MsgBox("Actualiza Numero?", vbYesNo) = MsgBoxResult.Yes Then actualizarNumero(Me)
        'actualizarHost(Me)
        If MsgBox("Actualiza Visabin?", vbYesNo) = MsgBoxResult.Yes Then actualizarVisabin(Me)
    End Sub

    'Sub MsgInfo(p1 As String)
    '    Me.Msg(p1)
    'End Sub



#Region "TEST"

    ''' <summary>
    ''' Envía un reverso para test. Se deben colocar los datos de la transacción original.
    ''' </summary>
    ''' <param name="ida"></param>
    ''' <param name="trace"></param>
    ''' <param name="ticket"></param>
    ''' <remarks></remarks>
    Public Sub ReversarTest(ida As IdaTypeInternal, trace As String, ticket As String, terminal As String, pHost As String)
        Dim req As New Req(ida, trace, ticket, Trim(terminal), pHost)
        Dim host = Hosts(req.nombreHost)
        host.enviarReverso(req)
    End Sub



    ''' <summary>
    ''' Prueba de tarjeta de prueba con todos los datos completos.
    ''' </summary>
    ''' <remarks></remarks>
    Sub venderTest(id As IdaTypeInternal, caja As String)
        Me.ProcIda(id, CInt(caja))
        Logger.Info("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" & vbNewLine &
                    " ------------ TRANSACCION TEST COMPLETADA -----------------------------------------------------------------------------------------------------------------------------------------------------------------" & vbNewLine &
                    "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------")
    End Sub

    ''' <summary>
    ''' Prueba posnet con todos los datos completos.
    ''' </summary>
    ''' <remarks></remarks>
    Sub devolucionTest(id As IdaTypeInternal)
        Dim host = Hosts("POSNET")
        Dim req As New TjServer.Req(id, 1, False)
        host.SendtestVenta(req)
        Logger.Info("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" & vbNewLine &
                    " ------------ TRANSACCION COMPLETADA -----------------------------------------------------------------------------------------------------------------------------------------------------------------" & vbNewLine &
                    "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------")
    End Sub

    ''' <summary>
    ''' Prueba sin el check
    ''' </summary>
    ''' <remarks></remarks>
    Sub anulacionVentaTest(id As IdaTypeInternal, pCaja As Integer)
        Dim host = Hosts("VISA")
        Dim req As New TjServer.Req(id, pCaja, False)
        host.SendtestVenta(req)
    End Sub

    Sub anulacionDevolucionTest(id As IdaTypeInternal)
        Dim host = Hosts("VISA")
        Dim req As New TjServer.Req(id, 1, False)
        host.SendtestVenta(req)
        Logger.Info("------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------" & vbNewLine &
                    " ------------ TRANSACCION COMPLETADA -----------------------------------------------------------------------------------------------------------------------------------------------------------------" & vbNewLine &
                    "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------")
    End Sub
#End Region
    Public Sub responderAcajaTCP(req As Req)
        If req.msjIda.tipoMensaje <> TransmisorTCP.TipoMensaje.Sincronizacion Then
            With Logger()
                .Info(vbNewLine &
                  String.Format("        ------ VTA RESPUESTA A Ping: {1} Caja: {0} ------", req.nroCaja, req.pinguino) & vbNewLine &
                  "        Mensaje:   " + req.msjRespuesta.mensajeHost & vbNewLine &
                  "        CodResp:   " + CStr(req.msjRespuesta.codRespuesta) & vbNewLine &
                  "        MenResp:   " + req.msjRespuesta.Respuesta & vbNewLine &
                  "        Montop:    " + CStr(req.msjRespuesta.Importe) & vbNewLine &
                  "        CashBack:  " + CStr(CInt(req.msjRespuesta.CashBack)) & vbNewLine &
                  "        Eminame:   " + req.msjRespuesta.Emisor & vbNewLine &
                  "        Trace:     " + CStr(req.nroTrace) & vbNewLine &
                  "        VtaTicket: " + CStr(req.nroTicket) & vbNewLine &
                  "        Aut:       " + req.autorizacion)
            End With
        End If


        '--- ACA MANDA LA RESPUESTA A LA CAJA  
        Transmisor.enviarMensajeCliente(req.clientStream, req.msjRespuesta)
        MarcarExportable(req)   '--- aca voy guardando en un backup, entonces esta marca es para que lo guarde 
        generandoVTA = False
    End Sub



    ' vta.VtaVersion = Mid(reg.ToString, 1, 1)
    ' vta.VtaMensaje = Mid(reg.ToString, 2, 32)
    ' vta.VtaMontop = Mid(reg.ToString, 34, 8)
    ' vta.VtaMenResp = Mid(reg.ToString, 42, 20)
    ' vta.VtaFileTkt = Mid(reg.ToString, 62, 60)
    ' vta.VtaEmiName = Mid(reg.ToString, 122, 16)
    ' vta.VtaOk = Conversor.QB2VB.CVI(Mid(reg.ToString, 138, 2))
    ' vta.VtaCashBack = Conversor.QB2VB.CVS(Mid(reg.ToString, 140, 4))
    ' vta.VtaTicket = Conversor.QB2VB.CVI(Mid(reg.ToString, 144, 8))
    ' vta.VtaCheck = Mid(reg.ToString, 152, 2)
    Public Sub responderAcaja(req As Req)
        Dim cadena As String = ""
        With Logger()
            .Info(vbNewLine &
                  String.Format("        ------ VTA RESPUESTA A Ping: {1} Caja: {0} ------", req.nroCaja, req.pinguino) & vbNewLine &
                  "        Mensaje:   " + req.vta.VtaMensaje & vbNewLine &
                  "        VtaOk:     " + CStr(req.vta.VtaOk) & vbNewLine &
                  "        MenResp:   " + req.vta.VtaMenResp & vbNewLine &
                  "        Montop:    " + CStr(CInt(req.vta.VtaMontop) / 100) & vbNewLine &
                  "        Eminame:   " + req.vta.VtaEmiName & vbNewLine &
                  "        FileTkt:   " + req.vta.VtaFileTkt & vbNewLine &
                  "        VtaTrace:  " + CStr(req.nroTrace) & vbNewLine &
                  "        VtaTicket: " + CStr(req.vta.VtaTicket) & vbNewLine &
                  "        Version:   " + req.vta.VtaVersion & vbNewLine &
                  "        CashBack:  " + CStr(CInt(req.ida.CASHBACK)) & vbNewLine &
                  "        Aut:       " + req.autorizacion & vbNewLine &
                  "        Check:     " + req.vta.VtaCheck & vbNewLine &
                  "        CodResp    " + req.vta.VtaCodResp & vbNewLine &
                  "        Cripto:    " + req.vta.VtaCripto)
        End With
        Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + req.ida.TICCAJ.ToString + "_" + Now.ToString("yyMMddHHmmss")
        Logger.Info(String.Format("Creando {0}", nombreCupon))
        Try
            Dim archivoVta = New System.IO.FileStream(Configuracion.DirLocal + "\Respuestas\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Dim archivoVta = New System.IO.FileStream(req.ida.cajadir + "\CAJA00" + req.nroCaja.ToString("00") + ".VTA", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglon = New System.IO.StreamWriter(archivoVta)

            With req.vta
                cadena = .VtaVersion +
                         Mid(.VtaMensaje, 1, 32).PadRight(32)
                If req.coeficiente > 1 Then
                    cadena = cadena + .VtaMontop.PadRight(8)
                Else
                    cadena = cadena + "        "
                End If
                cadena = cadena + .VtaMenResp.PadRight(20)
                cadena = cadena + .VtaFileTkt.PadRight(60)
                cadena = cadena + .VtaEmiName.PadRight(16)
                cadena = cadena + .VtaCodResp
                'cadena = cadena + .VtaOk.ToString("00")
                'cadena = cadena + .VtaCashBack.ToString("0000")
                'cadena = cadena + .VtaTicket.ToString("00000000")
                cadena = cadena + .VtaCheck
                'cadena = cadena + .VtaCodResp
                cadena = cadena + .VtaCripto
                cadena = cadena + .VtaAutorizacion

                renglon.WriteLine(cadena)
            End With
            renglon.Close()
        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo generar el archivo {0}\CAJA00{1}.VTA", req.ida.cajadir, req.nroCaja.ToString("00")) & vbNewLine &
                        ex.Message)
        End Try

        Try
            '--- Configuracion.DirLocal --> en maquina de Mauro y en Backtarjetas es c:\tarjetas 
            '--- req.ida.cajadir --> en maquina de Mauro es c:\temp\04 
            Logger.Info(Configuracion.DirLocal + "\Respuestas\" + nombreCupon + "-->" + req.ida.cajadir + "\CAJA00" + req.nroCaja.ToString("00") + ".VTA")
            If System.IO.File.Exists(req.ida.cajadir + "\CAJA00" + req.nroCaja.ToString("00") + ".VTA") Then
                System.IO.File.Delete(req.ida.cajadir + "\CAJA00" + req.nroCaja.ToString("00") + ".VTA")
            End If
            System.IO.File.Copy(Configuracion.DirLocal + "\Respuestas\" + nombreCupon, req.ida.cajadir + "\CAJA00" + req.nroCaja.ToString("00") + ".VTA")
            AgregarAVTAPendientes(ClaveVta(req.pinguino, req.nroCaja), req)
            generandoVTA = False

        Catch ex As Exception
            Logger.Error(String.Format("No se pudo renombrar respuesta VTA {0}", nombreCupon))
        End Try

    End Sub




    Sub AgregarAVTAPendientes(p1 As String, req As Req)
        Logger.Info("Encolado en VTAPendientes: " + Now.ToString)
        req.FechaHoraEnvioVtaCaja = Now
        ListaVtaPendientes.Add(p1, req)
        Logger.Info("*****   AGREGANDO A ListaVtaPendientes: " + p1)
    End Sub

    Sub SacarPendiente(p1 As String, req As Req)
        ListaReqPendientesOnline.Remove(p1)
        Logger.Error("*****   Sacando de pendientes por error: " + p1)
    End Sub

    Sub AgregarAPendientes(p1 As String, req As Req)
        req.FechaHoraEnvioMsg = Now
        ListaReqPendientesOnline.Add(p1, req)
        Logger.Info("*****   AGREGANDO A ListaReqPendientesOnline: " + p1)
    End Sub

    Sub AgregarAReversosPendientes(p1 As String, req As Req)
        req.FechaHoraEnvioMsg = Now
        ListaReversosPendientes.Add(p1, req)
        Logger.Info("*****   AGREGANDO A ListaReversosPndientes: " + p1)
    End Sub

    Sub AgregarAdvPendientes(p1 As String, req As Req)
        req.FechaHoraEnvioMsg = Now
        ListaAdvPendientesOnline.Add(p1, req)
        Logger.Info("*****   AGREGANDO A ListaAdvPendientesOnline: " + p1)
    End Sub

    Sub ProcesarADVPendientes()
        Try
            Dim ListaAsacar As New List(Of KeyValuePair(Of String, Req))
            For Each pend In ListaAdvPendientesOnline
                Dim Edad = Now.Subtract(pend.Value.FechaHoraEnvioMsg).TotalSeconds
                If Edad > 40 Then
                    ListaAsacar.Add(pend)
                End If
            Next
            For Each pend In ListaAsacar
                ListaAdvPendientesOnline.Remove(pend.Key)
                Logger.Info("*****   ELIMINANDO DE ListaADVPendientesOnline: " + pend.Key & vbNewLine &
                            "Operaciones pendientes: " + ListaAdvPendientesOnline.Count.ToString)
                Dim req = pend.Value
                BorrarDatosCriticos(req)
            Next
        Catch EX As Exception
            Logger.Warn("Ocurrió un error en ProcesarADVPendientes." & vbNewLine & EX.Message)
        End Try
    End Sub

    Sub ProcesarReversosPendientes()
        Try
            Dim ListaAsacar As New List(Of KeyValuePair(Of String, Req))
            For Each pend In ListaReversosPendientes
                Dim Edad = Now.Subtract(pend.Value.FechaHoraEnvioMsg).TotalSeconds
                If Edad > 40 Then
                    ListaAsacar.Add(pend)
                End If
            Next
            For Each pend In ListaAsacar
                MarcarExportable(pend.Value)
                ListaReversosPendientes.Remove(pend.Key)
                Logger.Info("*****   ELIMINANDO DE ListaReversosPendientes: " + pend.Key & vbNewLine &
                            "Operaciones Reversos pendientes CHIP: " + ListaReversosPendientes.Count.ToString)
                Dim req = pend.Value
                BorrarDatosCriticos(req)
            Next
        Catch EX As Exception
            Logger.Warn("Ocurrió un error en ReversosPendientes." & vbNewLine & EX.Message)
        End Try
    End Sub

    Property procesandoVta As Boolean = False
    Sub ProcesarVtaPendientes()

        Dim ListaAsacar As New List(Of KeyValuePair(Of String, Req))
        Dim ListaVtaTrabajo As Dictionary(Of String, Req)
        Try
            procesandoVta = True
            ListaVtaTrabajo = New Dictionary(Of String, Req)(ListaVtaPendientes)
            'ListaVtaTrabajo = ListaVtaPendientes
            'SyncLock ListaVtaTrabajo
            For Each pend In ListaVtaTrabajo
                If System.IO.File.Exists(Configuracion.DirTarjetas + "\" + Mid(pend.Key, 1, 2) + "\CAJA00" + Mid(pend.Key, 4, 2) + ".VTA") Then
                    Dim Edad = Now.Subtract(pend.Value.FechaHoraEnvioVtaCaja).TotalSeconds
                    If Edad > 80 Then
                        Logger.Info("Edad: " + Edad.ToString)
                        'Logger.Warn("BORRANDO: \" + Mid(pend.Key, 1, 2) + "\CAJA00" + Mid(pend.Key, 4, 2) + ".VTA" + " - PORQUE NO CONFIRMO LA OPERACION")
                        Try
                            If IO.File.Exists(Configuracion.DirTarjetas + "\" + Mid(pend.Key, 1, 2) + "\CAJA00" + Mid(pend.Key, 4, 2) + ".VTA") Then
                                Logger.Warn("BORRANDO: \" + Mid(pend.Key, 1, 2) + "\CAJA00" + Mid(pend.Key, 4, 2) + ".VTA" + " - PORQUE NO CONFIRMO LA OPERACION")
                                System.IO.File.Delete(Configuracion.DirTarjetas + "\" + Mid(pend.Key, 1, 2) + "\CAJA00" + Mid(pend.Key, 4, 2) + ".VTA")
                            Else
                                Logger.Warn("YA NO EXISTE: \" + Mid(pend.Key, 1, 2) + "\CAJA00" + Mid(pend.Key, 4, 2) + ".VTA")
                                Logger.Info("Ping: " + Mid(pend.Key, 1, 2) + "  Caja: " + Mid(pend.Key, 4, 2) + " - CONFIRMO OPERACION")
                                ListaAsacar.Add(pend)
                                CulminacionSatisfactoria(pend.Value)
                            End If
                        Catch
                            Logger.Error("No se pudo borrar el archivo \" + Mid(pend.Key, 1, 2) + "\CAJA00" + Mid(pend.Key, 4, 2) + ".VTA")
                        End Try
                        ListaAsacar.Add(pend)

                        'reversar??? por ahora no lo reverso por las dudas
                        'borrar el .vta si todavia existe????
                    End If
                    'sino sigue esperando
                Else
                    Logger.Info("Ping: " + Mid(pend.Key, 1, 2) + "  Caja: " + Mid(pend.Key, 4, 2) + " - CONFIRMO OPERACION")
                    ListaAsacar.Add(pend)
                    CulminacionSatisfactoria(pend.Value)

                End If
            Next

            For Each pend In ListaAsacar
                MarcarExportable(pend.Value)
                ListaVtaPendientes.Remove(pend.Key)
                Logger.Info("*****   ELIMINANDO DE ListaVtaPendientes: " + pend.Key & vbNewLine &
                                "Pendientes por confirmar: " + ListaVtaPendientes.Count.ToString)

                Dim req = pend.Value
            Next
            'End SyncLock
        Catch ex As Exception
            'Logger.Warn("ProcesandoVta = " & procesandoVta.ToString)
            Logger.Warn("Error en ProcesandoVtaPendientes: " & ex.Message & vbNewLine &
                        "Pila: " & ex.StackTrace)
        Finally
            procesandoVta = False
        End Try

    End Sub

    Sub ProcesarPendientes()
        Try
            Dim ListaAsacar As New List(Of KeyValuePair(Of String, Req))
            For Each pend In ListaReqPendientesOnline
                Dim Edad = Now.Subtract(pend.Value.FechaHoraEnvioMsg).TotalSeconds
                If Edad > 25 Then                           '--- 10/02/2021   If Edad > 25 Then
                    ListaAsacar.Add(pend)
                End If
            Next
            For Each pend In ListaAsacar
                MarcarExportable(pend.Value)
                ListaReqPendientesOnline.Remove(pend.Key)
                Logger.Info("*****   ELIMINANDO DE ListaReqPendientesOnline: " + pend.Key & vbNewLine &
                            "Operaciones pendientes: " + ListaReqPendientesOnline.Count.ToString)
                If pend.Value.tipoMensaje <> "0400" And pend.Value.tipoMensaje <> "0220" Then
                    Dim req = pend.Value
                    Dim host = Hosts(req.nombreHost)
                    'generar vta con reverso

                    If req.tipoRequerimiento = "" Then              '---  entro por archivos  
                        If req.esReqPEI Then
                            responderReversoPEI(req)



                            'If My.Computer.Name = "MARCOS-XP" Then
                            '    '----   HACER DEVOLUCION    
                            'End If




                        Else
                            responderReverso(req)
                            host.enviarReverso(req)
                        End If
                        'Else
                        '    responderReverso3DES(req)
                        '    host.enviarReverso3DES(req)
                    End If
                    If Not req.esReqPEI Then
                        BorrarDatosCriticos(req)
                    End If
                End If
            Next
        Catch EX As Exception
            Logger.Warn("Ocurrió un error en ProcesarPendientes." & vbNewLine & EX.Message)
        End Try
    End Sub



    Private Function TipoOperacion(oper As E_ProcCode) As String
        Dim descripcion As String = ""
        If oper = E_ProcCode.Compra Or oper = E_ProcCode.Compra_maestro_CajAhorroP Or
            oper = E_ProcCode.Compra_maestro_CtaCteP Or
            oper = E_ProcCode.Compra_maestro_CajAhorroD Or
            oper = E_ProcCode.Compra_maestro_CtaCteD Or
            oper = E_ProcCode.compra_cashback Then
            descripcion = "                 COMPRA"
        End If
        If oper = E_ProcCode.Devolucion Or
            oper = E_ProcCode.Devolucion_Compra_maestro_CajAhorroP Or
            oper = E_ProcCode.Devolucion_Compra_maestro_CtaCteP Or
            oper = E_ProcCode.Devolucion_Compra_maestro_CajAhorroD Or
            oper = E_ProcCode.Devolucion_Compra_maestro_CtaCteD Then
            descripcion = "                DEVOLUCION"
        End If

        If oper = E_ProcCode.AnulacionCompra Or
            oper = E_ProcCode.Anulacion_Compra_maestro_CajAhorroP Or
            oper = E_ProcCode.Anulacion_Compra_maestro_CtaCteP Or
            oper = E_ProcCode.Anulacion_Compra_maestro_CajAhorroD Or
            oper = E_ProcCode.Anulacion_Compra_maestro_CtaCteD Then
            descripcion = "           ANULACION DE COMPRA"
        End If

        If oper = E_ProcCode.AnulacionDevolucion Or
            oper = E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroP Or
            oper = E_ProcCode.AnulacionDevolucion_Compra_maestro_CtaCteP Or
            oper = E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroD Or
            oper = E_ProcCode.AnulacionDevolucion_Compra_maestro_CtaCteD Then
            descripcion = "         ANULACION DE DEVOLUCION"
        End If

        If oper = E_ProcCode.anulacion_compra_cashback Or
            oper = E_ProcCode.anulacion_cash_masterdebit Or
            (oper > E_ProcCode.anulacion_cash_masterdebit And oper < E_ProcCode.AnulacionDevolucion) Then
            descripcion = "        ANULACION COMPRA + EXTRACCION"
        End If

        Return descripcion
    End Function


    Private Function crearTicketTCP(req As Req) As String

        '----------------------------------------------------
        '--- ACA SE CREA EL TICKET CON EL PINPAD NUEVO       
        '----------------------------------------------------

        Dim ticketString As New StringBuilder()
        Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + req.msjIda.ticketCaja.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
        Try
            Logger.Info(String.Format("Creando Ticket {0}", req.msjIda.ticketCaja))
            Dim archivoTick = New System.IO.FileStream(Configuracion.DirLocal + "\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglonTick = New System.IO.StreamWriter(archivoTick)
            With req
                ticketString.AppendLine("             " + .msjRespuesta.Emisor)
                If .msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Then  '"compra_cashback" Then
                    ticketString.AppendLine("         COMPRA + EXTRACCION")
                Else
                    ticketString.AppendLine(TipoOperacion(.operacion))
                End If
                ticketString.AppendLine("             PINGUINO " + req.pinguino)

                ticketString.AppendLine("             CAJA: " + .nroCaja.ToString("00"))
                ticketString.AppendLine("Nro.Com.:" + .idComercio.ToString.PadRight(12) + "  Term:" + .terminal.PadRight(8))
                ticketString.AppendLine("Nro. de Lote:" + .nroLote.ToString.PadRight(3) + "       Nro. de Cupon:" + .nroTicket.ToString.PadRight(4))

                If req.FALLBACK = True Then
                    If .nrocuenta = "*" Then
                        ticketString.AppendLine(Trim(req.msjIda.nroTarjeta) + "      (Fallback)  **/**")
                    Else
                        If req.msjIda.nroTarjeta.Trim.Length <= 12 Then
                            ticketString.AppendLine(enmascarar(Trim(req.msjIda.nroTarjeta), 0, Trim(req.msjIda.nroTarjeta).Length - 4) + "      (Fallback)  **/**")
                        Else
                            ticketString.AppendLine(enmascarar(Trim(req.msjIda.nroTarjeta), 0, 12) + "      (Fallback)  **/**")
                        End If
                    End If
                Else
                    If .nrocuenta = "*" Then
                        ticketString.AppendLine(Trim(req.msjIda.nroTarjeta) + "      (" + req.msjIda.tipoIngreso.ToString + ")  **/**")
                    Else
                        If req.msjIda.nroTarjeta.Trim.Length <= 12 Then
                            ticketString.AppendLine(enmascarar(Trim(req.msjIda.nroTarjeta), 0, Trim(req.msjIda.nroTarjeta).Length - 4) + "      (" + req.msjIda.tipoIngreso.ToString + ")  **/**")
                        Else
                            ticketString.AppendLine(enmascarar(Trim(req.msjIda.nroTarjeta), 0, 12) + "      (" + req.msjIda.tipoIngreso.ToString + ")  **/**")
                        End If
                    End If
                End If

                If .nrocuenta <> "" Then ticketString.AppendLine("Nro Cta.:  " + req.nrocuenta)

                ticketString.AppendLine("AUTORIZACION ON-LINE: " + req.autorizacion)
                If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
                    ticketString.AppendLine("Cupon Anulado:" + .msjIda.ticketOriginal.ToString.PadRight(4))
                ElseIf req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion Then
                    ticketString.AppendLine("Cupon Orig.:" + .msjIda.ticketOriginal.ToString.PadRight(4) + "      Fecha Orig.:" + .msjIda.fechaOperacionOriginal.ToString("dd/MM/yy"))
                End If
                If req.nrohost = TjComun.TipoHost.VISA Or req.nrohost = TjComun.TipoHost.Visa_homolog Then
                    If .msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Then
                        ticketString.AppendLine("Compra: $" + (Math.Round(.msjRespuesta.Importe - req.msjIda.importeCashback, 2)).ToString("###,##0.00") + ".-")
                        ticketString.AppendLine("Extraccion: $" + (req.msjIda.importeCashback).ToString("###,##0.00") + ".-")
                    End If
                    ticketString.AppendLine("Importe TOTAL: $" + (Math.Round(.msjRespuesta.Importe, 2)).ToString("###,##0.00") + ".-")
                    ' ticketString.AppendLine("Cuotas: " + req.cuotas.ToString("00"))
                Else
                    '--- FIRSTDATA (ACA EL IMPORTE VA SEPARADO DEL CASHBACK)
                    If .msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Or (.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion And .msjIda.importeCashback > 0) Then
                        ticketString.AppendLine("Compra: $" + (Math.Round(.msjRespuesta.Importe, 2)).ToString("###,##0.00") + ".-")
                        ticketString.AppendLine("Extraccion: $" + (.msjIda.importeCashback).ToString("###,##0.00") + ".-")
                        ticketString.AppendLine("Importe TOTAL: $" + (Math.Round(.msjRespuesta.Importe + .msjIda.importeCashback, 2)).ToString("###,##0.00") + ".-")
                    Else
                        ticketString.AppendLine("Importe TOTAL: $" + (Math.Round(.msjRespuesta.Importe, 2)).ToString("###,##0.00") + ".-")
                    End If
                    'ticketString.AppendLine("Plan/Cuotas: " + req.planemisor.Substring(0, 1) & "/" & req.planemisor.Substring(1))
                End If


                If (req.cuotas = 3 And req.planemisor = "013") Or
                   (req.cuotas = 6 And req.planemisor = "016") Or
                   (req.cuotas = 12 And req.planemisor = "007") Or
                   (req.cuotas = 18 And req.planemisor = "008") Or
                   (req.cuotas = 24 And req.planemisor = "025") Then
                    ticketString.AppendLine("CUOTAS: " + req.planemisor)
                    ticketString.AppendLine("PLAN AHORA " + req.cuotas.ToString)
                    ticketString.AppendLine("COMPRA EFECTUADA EN " + req.cuotas.ToString + " CUOTAS FIJAS")
                Else
                    If req.nrohost = TjComun.TipoHost.VISA Or req.nrohost = TjComun.TipoHost.Visa_homolog Then
                        ticketString.AppendLine("Cuotas: " + req.cuotas.ToString("00"))
                    Else
                        'ticketString.AppendLine("Plan/Cuotas: " + req.planemisor.Substring(0, 1) & "/" & req.planemisor.Substring(1))
                        ticketString.AppendLine("Plan/Cuotas: 0/" & req.planemisor.Substring(1))
                    End If
                End If


                'ticketString.AppendLine("req.esMaestro" & req.esMaestro.ToString)
                'ticketString.AppendLine(".operacion" & .operacion.ToString)
                'ticketString.AppendLine(".tipocuenta" & req.tipocuenta.ToString)


                If req.esMaestro Then
                    Select Case .operacion
                        Case E_ProcCode.Compra_maestro_CajAhorroP, E_ProcCode.Devolucion_Compra_maestro_CajAhorroP, E_ProcCode.Anulacion_Compra_maestro_CajAhorroP, E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroP
                            ticketString.AppendLine("Caja de Ahorro en Pesos")
                        Case E_ProcCode.Compra_maestro_CtaCteP, E_ProcCode.Devolucion_Compra_maestro_CtaCteP, E_ProcCode.Anulacion_Compra_maestro_CtaCteP, E_ProcCode.AnulacionDevolucion_Compra_maestro_CtaCteP
                            ticketString.AppendLine("Cta. Corriente en Pesos")
                        Case E_ProcCode.Compra_maestro_CajAhorroD, E_ProcCode.Devolucion_Compra_maestro_CajAhorroD, E_ProcCode.Anulacion_Compra_maestro_CajAhorroD, E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroD
                            ticketString.AppendLine("Caja de Ahorro en dolares")
                        Case E_ProcCode.Compra_maestro_CtaCteD, E_ProcCode.Devolucion_Compra_maestro_CtaCteD, E_ProcCode.Anulacion_Compra_maestro_CtaCteD, E_ProcCode.AnulacionDevolucion_Compra_maestro_CtaCteD
                            ticketString.AppendLine("Cta. Corriente en dolares")
                    End Select



                    '--- 91000 es E_ProcCode.Compra_maestro_CajAhorroP + E_ProcCode.Compra_cashback (1000 + 90000)
                    'Compra_maestro_CajAhorroP = 1000
                    'Compra_maestro_CtaCteP = 2000
                    'Compra_maestro_CajAhorroD = 8000
                    'Compra_maestro_CtaCteD = 9000
                    'compra_cashback = 90000
                    If req.operacion = 91000 Then
                        ticketString.AppendLine("Caja de Ahorro en Pesos")
                    ElseIf req.operacion = 92000 Then
                        ticketString.AppendLine("Cta. Corriente en Pesos")
                    ElseIf req.operacion = 98000 Then
                        ticketString.AppendLine("Caja de Ahorro en dolares")
                    ElseIf req.operacion = 99000 Then
                        ticketString.AppendLine("Cta. Corriente en dolares")
                    End If

                End If




                ticketString.AppendLine("Nro. de Factura: " + .msjIda.ticketCaja.ToString)
                If req.msjIda.tipoIngreso = TipoIngreso.Contactless Or req.msjIda.tipoIngreso = TipoIngreso.Chip Then
                    If .msjIda.nombreApplicacion <> "" Then ticketString.AppendLine("Nombre de aplicacion: " + .msjIda.nombreApplicacion)
                    If .msjIda.appID <> "" Then ticketString.AppendLine("ID Aplicacion: " + .msjIda.appID)
                End If

                ticketString.AppendLine("          ")

                Try
                    If Mid(.msjRespuesta.Emisor, 1, 4) = "VISA" Then
                    Else
                        ticketString.AppendLine("FIRMA: ........................................   ")
                    End If
                Catch
                    ticketString.AppendLine("FIRMA: ........................................   ")
                End Try

                ticketString.AppendLine("NRO. DOCUMENTO:  " + .msjIda.NroDocumentoTicket)

                '--- SI ES MANUAL TE PIDE LA ACLARACION
                If req.msjIda.nombreTHabiente IsNot Nothing AndAlso req.msjIda.nombreTHabiente.Trim <> "" Then
                    ticketString.AppendLine("         " + .msjIda.nombreTHabiente)
                Else
                    ticketString.AppendLine("          ")
                    ticketString.AppendLine("ACLARACION: ...................................   ")
                End If

                'ticketString.AppendLine("          ")
                'ticketString.AppendLine("TIPO Y NRO DOCUMENTO: .........................   ")
                ticketString.AppendLine(" ")
                ticketString.AppendLine(" ")
                ticketString.AppendLine(" ")

                If req.msjIda.tipoIngreso = TipoIngreso.Manual Then
                    ticketString.AppendLine(" ")
                    ticketString.AppendLine(" ")
                    ticketString.AppendLine(" ")
                    ticketString.AppendLine(" ")
                    ticketString.AppendLine(" ")
                    ticketString.AppendLine(" ")
                    ticketString.AppendLine(" ")
                    ticketString.AppendLine(" ")
                    ticketString.AppendLine(" ")
                    ticketString.AppendLine(" ")
                    ticketString.AppendLine("          Estampar relieve de la tarjeta")
                End If

                '------------------------------------------------------------------------------------------
                '--- EN ESTE CAMPO EL EMISOR PONE ALGUN COMENTARIO Y SE IMPRIME AL FINAL                   
                '--- campo 63 - si empieza con (,) las lineas vienen separadas por |, fin de linea es ^    
                '------------------------------------------------------------------------------------------
                Dim reng() As String = .msjRespuesta.mensajeHost.Split(CChar("|"))
                For Each lin As String In reng
                    ticketString.AppendLine(lin)
                Next
                If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion Then ticketString.AppendLine("OPERACION A CONFIRMAR")
            End With

            '--- ACA CREA EL ARCHIVO DE CUPONES QUE DESPUES ABAJO SE COPIA AL DIRECTORIO CUPONES PARA REIMPRIMIR  
            renglonTick.Write(ticketString.ToString)
            renglonTick.Close()

        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
        End Try

        '--------------------------------------------------
        '--- ACA COPIA EL CUPON POR SI SE REIMPRIME        
        '--------------------------------------------------
        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon) & vbNewLine &
                         ex.Message)
        End Try
        Return ticketString.ToString

    End Function

    Private Sub crearTickPEI(req As Req)
        Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + req.ida.TICCAJ.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
        Try
            Logger.Info(String.Format("Creando {0}", nombreCupon))

            '--- Configuracion.DirLocal --> para maquina de Mauro es c:\tarjetas  (esta en parametros.txt)  
            Dim archivoTick = New System.IO.FileStream(Configuracion.DirLocal + "\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
            'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglonTick = New System.IO.StreamWriter(archivoTick)
            With req
                If .vta.VtaEmiName = "VISA" Then
                    renglonTick.WriteLine("             VISA DEBITO")
                Else
                    renglonTick.WriteLine("             " + .vta.VtaEmiName)
                End If


                '--- SI ES COMPRA_DE_BIENES PONGO COMPRA, ES PARA QUE NO APAREZCA COMPRA_DE_BIENES, SOLAMENTE APAREZCA COMPRA  
                If .OperacionPEI = "COMPRA_DE_BIENES" Then
                    renglonTick.WriteLine("                COMPRA          (PEI)")
                Else
                    renglonTick.WriteLine("           " + .OperacionPEI)
                End If

                renglonTick.WriteLine("             PINGUINO " + req.pinguino)
                renglonTick.WriteLine("               CAJA: " + .nroCaja.ToString("00"))
                renglonTick.WriteLine("Nro.Com.:" + .idComercio.ToString.PadRight(12) + "  Term:" + .terminal.PadRight(8))
                renglonTick.WriteLine("Nro. de Cupon:" + .nroTicket.ToString.PadRight(4))

                renglonTick.WriteLine(enmascarar(Trim(req.ida.TARJ), 0, Trim(req.ida.TARJ).Length - 4) + "      (PEI)")

                If .nrocuenta <> "" Then renglonTick.WriteLine("Nro Ref. Bancaria:  " + req.nrocuenta)

                renglonTick.WriteLine("ID DE OPERACION: " + req.autorizacion)
                If req.ida.oper = 1 Then
                    renglonTick.WriteLine("ID DE OPERACION ORIGINAL:" + .ida.TKTORI.ToString.PadRight(4))
                End If

                renglonTick.WriteLine("Importe TOTAL: $" + (Math.Round(CInt(.vta.VtaMontop) / 100, 2)).ToString("###,##0.00") + ".-")

                renglonTick.WriteLine("Nro de comprobante:" + .ida.TICCAJ.ToString)
                renglonTick.WriteLine("          ")
                renglonTick.WriteLine("          ")
                renglonTick.WriteLine("          ")
                renglonTick.WriteLine("FIRMA: ........................................   ")

                renglonTick.WriteLine("         " + .cliente)
                '
                renglonTick.WriteLine("Documento:  " + .ida.documento)
                renglonTick.WriteLine(" ")
                renglonTick.WriteLine(" ")
                renglonTick.WriteLine(" ")
            End With
            renglonTick.Close()

        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
        End Try

        'BORRANDO
        Try
            '--- req.ida.cajadir --> en maquina de Mauro es c:\temp\04 
            If System.IO.File.Exists(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT") Then
                Logger.Info("Borrando " + req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
                System.IO.File.Delete(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            Else
                Logger.Info("No se encontró " + req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            End If
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo borrar cupón TICK00{0}", req.nroCaja.ToString("00")) & vbNewLine &
                         ex.Message)
        End Try

        'RENOMBRANDO
        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", True)
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo renombrar cupón {0}", nombreCupon) & vbNewLine &
                         ex.Message)
        End Try

        'COPIANDO
        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon) & vbNewLine &
                         ex.Message)
        End Try

    End Sub




    Private Sub crearTickDAT(req As Req)
        Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + req.ida.TICCAJ.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
        Try
            Logger.Info(String.Format("Creando {0}", nombreCupon))

            Dim archivoTick = New System.IO.FileStream(Configuracion.DirLocal + "\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
            'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglonTick = New System.IO.StreamWriter(archivoTick)
            With req
                renglonTick.WriteLine("             " + .vta.VtaEmiName)
                If .operacion = E_ProcCode.compra_cashback Then  '"compra_cashback" Then
                    renglonTick.WriteLine("       COMPRA + EXTRACCION")
                Else
                    renglonTick.WriteLine(TipoOperacion(.operacion))
                End If
                renglonTick.WriteLine("             PINGUINO " + req.pinguino)

                renglonTick.WriteLine("             CAJA: " + .nroCaja.ToString("00"))
                renglonTick.WriteLine("Nro.Com.:" + .idComercio.ToString.PadRight(12) + "  Term:" + .terminal.PadRight(8))
                renglonTick.WriteLine("Nro. de Lote:" + .nroLote.ToString.PadRight(3) + "   Nro. de Cupon:" + .nroTicket.ToString.PadRight(4))

                If .nrocuenta = "*" Then
                    renglonTick.WriteLine(Trim(req.ida.TARJ) + "      (" + ObtenerTxtModoIngreso(req.ida.MANUAL) + ")  **/**")
                Else
                    If req.ida.TARJ.Trim.Length <= 12 Then
                        renglonTick.WriteLine(enmascarar(Trim(req.ida.TARJ), 0, Trim(req.ida.TARJ).Length - 4) + "      (" + ObtenerTxtModoIngreso(req.ida.MANUAL) + ")  **/**")
                    Else
                        renglonTick.WriteLine(enmascarar(Trim(req.ida.TARJ), 0, 12) + "      (" + ObtenerTxtModoIngreso(req.ida.MANUAL) + ")  **/**")
                    End If

                End If

                If .nrocuenta <> "" Then renglonTick.WriteLine("Nro Cta.:  " + req.nrocuenta)

                renglonTick.WriteLine("AUTORIZACION ON-LINE: " + req.autorizacion)
                If req.ida.oper = 2 Or req.ida.oper = 3 Then
                    renglonTick.WriteLine("Cupon Orig:" + .ida.TKTORI.ToString.PadRight(4))
                ElseIf req.ida.oper = 1 Then
                    renglonTick.WriteLine("Cupon Orig:" + .ida.TKTORI.ToString.PadRight(4) + "      Fecha Orig.:" + .ida.FECORI)
                End If

                If .operacion = E_ProcCode.compra_cashback Then
                    renglonTick.WriteLine("Compra: $" + (Math.Round((CInt(.vta.VtaMontop) / 100) - req.ida.CASHBACK, 2)).ToString("###,##0.00") + ".-")
                    renglonTick.WriteLine("Extraccion: $" + (req.ida.CASHBACK).ToString("###,##0.00") + ".-")
                End If

                renglonTick.WriteLine("Importe TOTAL: $" + (Math.Round(CInt(.vta.VtaMontop) / 100, 2)).ToString("###,##0.00") + ".-")


                If (req.cuotas = 3 And req.planemisor = "013") Or
                   (req.cuotas = 6 And req.planemisor = "016") Or
                   (req.cuotas = 12 And req.planemisor = "007") Or
                   (req.cuotas = 18 And req.planemisor = "008") Then
                    renglonTick.WriteLine("CUOTAS: " + req.planemisor)
                    renglonTick.WriteLine("PLAN AHORA " + req.cuotas.ToString)
                    renglonTick.WriteLine("COMPRA EFECTUADA EN " + req.cuotas.ToString + " CUOTAS FIJAS")
                Else
                    renglonTick.WriteLine("Cuotas: " + req.cuotas.ToString("00"))
                End If


                If req.esMaestro Then
                    Select Case .operacion
                        Case E_ProcCode.Compra_maestro_CajAhorroP, E_ProcCode.Devolucion_Compra_maestro_CajAhorroP, E_ProcCode.Anulacion_Compra_maestro_CajAhorroP, E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroP
                            renglonTick.WriteLine("Caja de Ahorro en Pesos")
                        Case E_ProcCode.Compra_maestro_CtaCteP, E_ProcCode.Devolucion_Compra_maestro_CtaCteP, E_ProcCode.Anulacion_Compra_maestro_CtaCteP, E_ProcCode.AnulacionDevolucion_Compra_maestro_CtaCteP
                            renglonTick.WriteLine("Cta. Corriente en Pesos")
                        Case E_ProcCode.Compra_maestro_CajAhorroD, E_ProcCode.Devolucion_Compra_maestro_CajAhorroD, E_ProcCode.Anulacion_Compra_maestro_CajAhorroD, E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroD
                            renglonTick.WriteLine("Caja de Ahorro en dolares")
                        Case E_ProcCode.Compra_maestro_CtaCteD, E_ProcCode.Devolucion_Compra_maestro_CtaCteD, E_ProcCode.Anulacion_Compra_maestro_CtaCteD, E_ProcCode.AnulacionDevolucion_Compra_maestro_CtaCteD
                            renglonTick.WriteLine("Cta. Corriente en dolares")
                    End Select
                End If

                renglonTick.WriteLine("Nro de comprobante:" + .ida.TICCAJ.ToString)
                renglonTick.WriteLine("          ")
                renglonTick.WriteLine("FIRMA: ........................................   ")

                If req.ida.TRACK1.Trim = "" Then
                    If req.ida.MANUAL = 5 Then
                        renglonTick.WriteLine("         " + .cliente)
                    Else
                        renglonTick.WriteLine("          ")
                        renglonTick.WriteLine("ACLARACION: ...................................   ")
                    End If
                Else
                    renglonTick.WriteLine("         " + .cliente)
                End If
                '
                If .ida.documento = "".PadRight(15) Then
                    renglonTick.WriteLine("          ")
                    renglonTick.WriteLine("TIPO Y NRO DOCUMENTO: .........................   ")
                Else
                    renglonTick.WriteLine("NRO DOCUMENTO: " + .ida.documento.Trim)
                End If
                renglonTick.WriteLine(" ")
                renglonTick.WriteLine(" ")
                renglonTick.WriteLine(" ")

                If req.ida.MANUAL = 1 Then
                    renglonTick.WriteLine(" ")
                    renglonTick.WriteLine(" ")
                    renglonTick.WriteLine(" ")
                    renglonTick.WriteLine(" ")
                    renglonTick.WriteLine(" ")
                    renglonTick.WriteLine(" ")
                    renglonTick.WriteLine(" ")
                    renglonTick.WriteLine(" ")
                    renglonTick.WriteLine(" ")
                    renglonTick.WriteLine(" ")
                End If

                'campo 63 - si empieza con (,) las lineas vienen separadas por |, fin de linea es ^ 
                Dim reng() As String = .vta.VtaMensaje.Split(CChar("|"))
                For Each lin As String In reng
                    renglonTick.WriteLine(lin)
                Next
                If req.ida.oper = 1 Then renglonTick.WriteLine("OPERACION A CONFIRMAR")

            End With
            renglonTick.Close()

        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
        End Try

        'BORRANDO
        Try
            If System.IO.File.Exists(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT") Then
                Logger.Info("Borrando " + req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
                System.IO.File.Delete(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            Else
                Logger.Info("No se encontró " + req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            End If
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo borrar cupón TICK00{0}", req.nroCaja.ToString("00")) & vbNewLine &
                         ex.Message)
        End Try

        'RENOMBRANDO
        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", True)
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo renombrar cupón {0}", nombreCupon) & vbNewLine &
                         ex.Message)
        End Try

        'COPIANDO
        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon) & vbNewLine &
                         ex.Message)
        End Try

    End Sub

    ''' <summary>
    ''' Graba el archivo con la working key de maestro.
    ''' </summary>
    ''' <param name="pWkey">Working Key a grabar</param>
    ''' <param name="pPinCaja">Pinguino/Caja en formato "0000"</param>
    ''' <param name="pDir">Directorio de tarjetas del pinguino correspondiente, sin la barra final.</param>
    ''' <remarks></remarks>
    Private Sub GrabarWkey(pWkey As String, pPinCaja As String, pDir As String)
        Try
            Dim wkey = New System.IO.FileStream(pDir + "\WKEY" + pPinCaja + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglon = New System.IO.StreamWriter(wkey)
            Logger.Info("Grabando la workingkey: " + pWkey)
            renglon.WriteLine(pWkey)

            renglon.Close()
        Catch ex As Exception
            ' NO SE PUDO GRABAR, INFORMAR.
            Logger.Fatal(String.Format("No se pudo generar la workingkey de {0} en {1}", pPinCaja, pDir))

        End Try
    End Sub

End Class
