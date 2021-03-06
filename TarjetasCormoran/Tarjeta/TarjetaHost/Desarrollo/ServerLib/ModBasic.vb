Option Strict On
Imports Trx.Messaging.Channels
Imports log4net
Imports System.Reflection

Module ModBasic
    Public Login As String = ""
    Public DebeReconectar As Boolean = False
    Public conexion As TarjetasAcceso.Acceso
    Public ObjPermisos As TarjetaSeguridad.Permisos
    Public objSeguridad As TarjetaSeguridad.ActiveDirectory
    Public objConsultas As TarjetasControl.Consultas
    Public objLiq As TarjetasControl.LiquidacionesCliente
    Public WithEvents objaut As TarjetasControl.Autorizaciones
    Public WithEvents EscuchadorImplementado As ONLINECOM.Escuchador
    Public log As Logeador.Logeador
    Private RespuestaError As ONLINECOM.E_CodigosRespuesta
    Public WithEvents ContestadorImplementado As ONLINECOM.llamador
    Public NcomErr As Integer
    Public Ncomerr2 As Integer
    Public UltimaReconexion As Date = #1/1/2000#
    Dim FrmInicio As Frm_Inicio
    Public ColaEmailsN As Integer

    Public Nsaf As Integer
    Public Safqueue(0) As TsafQueue
    Public Structure tSAFqueue
        Dim e As ONLINECOM.ParametrosCom
        Dim Resultado As Integer
    End Structure
    Private Structure Temails
        Public Destino As String
        Public Asunto As String
        Public Mensaje As String

    End Structure

    Private ColaEmails() As Temails
    Public Enum E_TipoTerminales
        ATM = 2
        POS = 3
    End Enum

    Public Function ObtenerAcceso(Optional ByVal Loguear As Boolean = True) As TarjetasAcceso.Acceso
        Dim ta As TarjetasAcceso.Acceso
        ta = New TarjetasAcceso.Acceso("ONL" + My.Settings.Red, My.Settings.Entorno, Loguear)
        ta.Nolock = My.Settings.activarNolock
        ta.DemoraConsultaLenta = 4
        ta.registrarConsultasLentas = My.Settings.RegistrarConsultasLentas
        Return ta
    End Function
    Public Sub Conectardb()
        ModBasic.conexion = ObtenerAcceso()
        ModBasic.conexion.BorrarCaches()
        log = ModBasic.conexion.Logueador
        log.AddMemoryAppender()
        ModBasic.conexion.trace = ModBasic.conexion.DESARROLLO
    End Sub
    Public Sub Reconectar(Optional ByVal Motivo As String = "Reconexion")
        Try
            Try
                EscuchadorImplementado.HostOfflIne = True
            Catch ex As Exception
                log.logerror("Error Reconectando escuchador offline" + ex.Message + " " + ex.StackTrace)
            End Try

            Try
                If ContestadorImplementado IsNot Nothing Then ContestadorImplementado.HostOffLine = True
            Catch ex As Exception
                log.logerror("Error Reconectando contestador offline" + ex.Message + " " + ex.StackTrace)
            End Try

            Try
                Conectardb()
            Catch ex As Exception
                log.logerror("Error Reconectando conectardb" + ex.Message + " " + ex.StackTrace)
            End Try

            Try
                Cargar(motivo)
            Catch ex As Exception
                log.logerror("Error Reconectando cargar " + ex.Message + " " + ex.StackTrace)
            End Try

            Try
                EscuchadorImplementado.HostOfflIne = False
            Catch ex As Exception
                log.logerror("Error Reconectando escuchador online" + ex.Message + " " + ex.StackTrace)
            End Try

            Try
                If ContestadorImplementado IsNot Nothing Then ContestadorImplementado.HostOffLine = False
            Catch ex As Exception
                log.logerror("Error Reconectando contestador online" + ex.Message + " " + ex.StackTrace)
            End Try
            Try
                ProcesarColaSaf()

            Catch ex As Exception
                log.logerror("Error procesando cola saf " + ex.Message + " " + ex.StackTrace)

            End Try
        Catch ex As Exception
            log.logerror("Error Reconectando " + ex.Message + " " + ex.StackTrace)
        End Try
        DebeReconectar = False
    End Sub
    Public Sub Cargar(Optional ByVal motivo As String = "")
        Try
            log.log("Creando Objeto Liquidacion " + motivo)
            objLiq = New TarjetasControl.LiquidacionesCliente(ModBasic.conexion, 1)
            objLiq.Silent = True
            log.log("Creando Objeto Autorizacion")
            objaut = New TarjetasControl.Autorizaciones(ModBasic.conexion)
            ' precargar el liq
            log.log("Generando liquidacion inicial para precargar tablas...")
            objLiq.SetLiqID(objLiq.Vencimiento.VencimientoActual)

            objLiq.AdelantoEnCuotasAfectaLimiteCompraCuotas = True 'My.Settings.AdelantoenCuotasAfectaLimiteCompraCuotas
            log.log("Adelanto en cuotas Afecta Limite Compra Cuotas :" + objLiq.Disponibles.DisponibleATMenCuotas.ToString)

            objLiq.Liquidar(380199, False, False, True, True)
            log.log("Precarga tablas Autorizacion:")
            objaut.Precargar()


            log.log("Carga de tablas completada")
            EncolarEMail(My.Settings.EmailNoeco, "Arranque Sistema " + My.Settings.Red + " en " + My.Settings.Entorno + " " + My.Computer.Name + "  Motivo:" + motivo, "")
        Catch ex As Exception
            log.logerror("Error Arrancando objetos  " + ex.Message)
        End Try
    End Sub

    Public Sub PasoTiempo()
        Ncomerr2 = 0

        If (Now.TimeOfDay.Hours = 0 And UltimaReconexion.Date < Now.Date) Or cambioLiqId Then
            UltimaReconexion = Now
            log.logInfo("Solicitando reconexion Por Cambio de Parametros")
            DebeReconectar = True
            Try
                FrmInicio.ListBox1.Items.Clear()
            Catch ex As Exception

            End Try
        End If

    End Sub

    Public Function CambioLiqId() As Boolean

        Static LastLiqId As Long
        Try

            Dim ta As TarjetasAcceso.Acceso = ModBasic.ObtenerAcceso(False)
            Dim dr As DataRow = ta.EjecutarFila("SELECT MAX(IDLIQUIDACIONCLIENTE) as UltLiq,Sum(idliquidacionCliente+idgrupoliquidacion) as sumaControl FROM VENCIMIENTOSCLIENTES with (Nolock) WHERE APROBADA = 1 ")
            ta.Dispose()
            Dim LiqIdNueva As Long = CLng(dr.Item("SumaControl"))
            If LiqIdNueva <> LastLiqId Then
                log.logInfo("Ultima Liquidacion Aprobada " + dr.Item("UltLiq").ToString)
                If LastLiqId > 0 Then
                    LastLiqId = LiqIdNueva
                    Return True
                Else
                    LastLiqId = LiqIdNueva
                    Return False
                End If
            Else
                Return False

            End If
        Catch ex As Exception
            log.logDebug("error " + ex.Message + " obteniendo cambioliqid")
        Finally
            GC.Collect()
        End Try

    End Function
    Public Sub Arrancar(ByVal frm As Frm_Inicio)
        ' arrancar el log
        FrmInicio = frm
        System.Threading.Thread.CurrentThread.CurrentCulture = New System.Globalization.CultureInfo("es-AR")

        Conectardb()
        log.log("fecha en servidor:" + ModBasic.conexion.FechaServidor.ToString)
        FrmInicio.Timer1.Enabled = True

        objConsultas = New TarjetasControl.Consultas
        objSeguridad = New TarjetaSeguridad.ActiveDirectory
        ObjPermisos = New TarjetaSeguridad.Permisos

        Login = objSeguridad.LocalUserName.ToString.ToUpper
        log.log("aplicacion " + My.Application.Info.Version.ToString)
        log.log("Usuario :" + Login)

        log.log("RED:" + My.Settings.Red + " entorno:" + My.Settings.Entorno)
        Dim Tmp As String = conexion.strCon
        Dim p1 As Integer = InStr(Tmp.ToLower, "pwd=")
        Dim p2 As Integer = InStr(p1 + 1, Tmp, ";")
        If p1 > 0 Then Tmp = Tmp.Remove(p1 - 1, p2 - p1 + 1)
        My.Application.DoEvents()
        log.logInfo("Conexion:" + Tmp)
        Cargar("Arranque Aplicativo")
        log.log("Entorno " + My.Settings.Entorno)
        Dim Auditar As Boolean = My.Settings.auditar

        Select Case My.Settings.Red
            Case "B"
                EscuchadorImplementado = New ONLINECOM.Escuchador(ONLINECOM.E_Implementaciones.Banelco, My.Settings.IP, My.Settings.Puerto, auditar)

            Case "L"
                EscuchadorImplementado = New ONLINECOM.Escuchador(ONLINECOM.E_Implementaciones.Link, My.Settings.IP, My.Settings.Puerto, auditar)

            Case "P"
                EscuchadorImplementado = New ONLINECOM.Escuchador(ONLINECOM.E_Implementaciones.PosnetComercio, My.Settings.IP, My.Settings.Puerto, auditar)

            Case "S"
                EscuchadorImplementado = New ONLINECOM.Escuchador(ONLINECOM.E_Implementaciones.PosnetSalud, My.Settings.IP, My.Settings.Puerto, auditar)

        End Select

        If My.Settings.llamarAlInicio Then
            conectar(My.Settings.IpLlamar)
        End If

    End Sub

    Public Function Logger() As ILog
        Return LogManager.GetLogger(MethodBase.GetCurrentMethod.DeclaringType)
    End Function

    Public Sub Finalizar()
        Try

            conexion.Dispose()

        Catch ex As Exception
        End Try
        conexion = Nothing
    End Sub

    Public Function ObtenerIdTipoTerminal(ByRef e As ONLINECOM.ParametrosCom) As TarjetasControl.Terminal.TTipoTerminales
        Select Case e.implementacion
            Case ONLINECOM.E_Implementaciones.Banelco
                Return TarjetasControl.Terminal.TTipoTerminales.ATMBanelco
            Case ONLINECOM.E_Implementaciones.Link
                Return TarjetasControl.Terminal.TTipoTerminales.ATMLink
            Case ONLINECOM.E_Implementaciones.PosnetComercio, ONLINECOM.E_Implementaciones.PosnetSalud
                Return TarjetasControl.Terminal.TTipoTerminales.Posnet


        End Select


    End Function
    Public Function ObtenerIdTerminal(ByRef e As ONLINECOM.ParametrosCom, ByRef IdTipoTerminal As TarjetasControl.Terminal.TTipoTerminales) As Int32
        Dim idProveedor As TarjetasControl.Terminal.TipoPRoveedores
        IdTipoTerminal = ObtenerIdTipoTerminal(e)
        Select Case e.implementacion
            Case ONLINECOM.E_Implementaciones.Banelco
                idProveedor = TarjetasControl.Terminal.TipoPRoveedores.banelco
            Case ONLINECOM.E_Implementaciones.Link
                idProveedor = TarjetasControl.Terminal.TipoPRoveedores.Link
            Case ONLINECOM.E_Implementaciones.PosnetComercio, ONLINECOM.E_Implementaciones.PosnetSalud
                idProveedor = TarjetasControl.Terminal.TipoPRoveedores.Posnet
        End Select
        'Buscar la terminal por codigo y por tipo terminal y dar de alta si no existe
        Try
            Dim objcons As New TarjetasControl.Consultas()
            Dim itm As DataRow = objcons.Terminales_ExisteTerminalxCodigo(conexion, e.idTerminalCOM, CShort(IdTipoTerminal))

            If itm Is Nothing Then
                Dim ProxId As Int32 = Convert.ToInt32(objcons.Terminales_ObtenerProxima(conexion, CShort(IdTipoTerminal)))
                If e.CuentaComercio = 0 Then e.CuentaComercio = My.Settings.CuentaComercio
                Dim Term As New TarjetasControl.Terminal(ProxId, IdTipoTerminal, e.CuentaComercio, idProveedor, e.idTerminalCOM)

                If Not Term.Alta(conexion) Then
                    log.logWarning("Error al realizar el alta de la terminal " + ProxId.ToString)
                    Term = Nothing
                    Return 0
                Else
                    Term = Nothing
                    Return ProxId
                End If
            Else
                Return Convert.ToInt32(itm("IdTerminal"))
            End If
        Catch ex As Exception
            DebeReconectar = True
            log.logWarning("Se ha pedido una reconexion")
            Throw ex
        End Try

    End Function

    Private Sub EscuchadorImplementado_BeforeSend(ByRef msg As ONLINECOM.Iso8583messageNexo) Handles EscuchadorImplementado.BeforeSend
        Dim Demora As Integer
        If Integer.TryParse(FrmInicio.txtDemora.Text, Demora) Then
            If Demora > 0 Then
                Dim t As DateTime = Now
                Do
                    My.Application.DoEvents()

                Loop While Now.Subtract(t).TotalSeconds < Demora
                log.logInfo("se demoro intencionalmente " + Demora.ToString)
            End If
        End If
    End Sub

    Public OcupadoDB As Boolean = False
    Public Sub CoordinarEntrada()
        If OcupadoDB Then
            Dim t1 As Date = Now
            Dim t As Integer = 0
            Try
                Do While OcupadoDB

                    t = CInt(Now.Subtract(t1).TotalMilliseconds)
                    If t > My.Settings.EsperaMaxima Then Exit Do
                    Threading.Thread.Sleep(50)
                Loop

                log.logInfo("Se espero " + (t / 1000.0#).ToString + " segundos ")
                If OcupadoDB Then log.logDebug("No alcanzo el tiempo...")

            Catch ex As Exception
                log.logerror("error en el loop de espera " + ex.Message + " " + ex.StackTrace)
            End Try
        End If
        OcupadoDB = True
        objLiq.borrarcaches()
    End Sub

    Public Sub CoordinarSalida()
         If OcupadoDB Then
            OcupadoDB = False
        Else
            log.logDebug("Deberia estar en ON")
        End If
    End Sub

    Private Sub EscuchadorImplementado_HostDown(ByRef UltimaHoraEco As Date) Handles EscuchadorImplementado.HostDown
        EnviarEMailInterno(My.Settings.EmailNoeco, "No eco en " + EscuchadorImplementado.ToString + " desde " + UltimaHoraEco.ToString, "")
    End Sub

    Private Sub EscuchadorImplementado_HostUp(ByRef UltimaHoraEco As Date) Handles EscuchadorImplementado.HostUp
        EnviarEMailInterno(My.Settings.EmailNoeco, "Eco Recibido (conexion restaurada) en " + EscuchadorImplementado.ToString + " desde " + UltimaHoraEco.ToString, "")

    End Sub


    Private Sub EscuchadorImplementado_Logoff() Handles EscuchadorImplementado.Logoff
        With FrmInicio.lblstatus
            .ForeColor = Drawing.Color.Red
            '.Text = "Fuera de linea :" + Date.Now.ToString
        End With

    End Sub

    Private Sub EscuchadorImplementado_LogOn() Handles EscuchadorImplementado.LogOn
        With FrmInicio.lblstatus
            .ForeColor = Drawing.Color.Green
            '.Text = "En linea :" + Date.Now.ToString
        End With

    End Sub

    Private Sub EscuchadorImplementado_ReversoSospechoso(ByVal parametros As ONLINECOM.ParametrosCom) Handles EscuchadorImplementado.ReversoSospechoso
        EncolarEMail("operaciones@tarjetanexo.com.ar", "reverso sospechoso, puede generar diferencias de conciliacion", "Cuenta:" + parametros.Cuenta.ToString + " " + parametros.Tarjeta.ToString)
    End Sub

    Private Sub EscuchadorImplementado_TransaccionNEXOAdelanto(ByRef e As ONLINECOM.ParametrosCom) Handles EscuchadorImplementado.TransaccionNEXOAdelanto
        CoordinarEntrada()
        With objLiq
            Dim CtaCliente As Integer
            'esta consulta deberia estar en otra capa?
            Try
                Dim PCuentaComercio As Integer = My.Settings.CuentaComercio
                Dim PtipoTransaccion As Short
                Dim pnrocuotas As Short
                Dim pidTipoTerminal As TarjetasControl.Terminal.TTipoTerminales
                Dim pidTerminal As Integer = ObtenerIdTerminal(e, pidTipoTerminal)

                Dim FechaVigenciaDesde As Date
                Dim FechaVigenciaHasta As Date
               
                Dim PfechaBajaOk As Boolean
                Dim CCfechaBajaOk As Boolean
                Dim pEstadoPlastico As Integer
                '	Al enviarse la transacción al HOST se le indicará el tipo de transacción en el Campo TIPO-TRANSACCION del TOKEN otras tarjetas, con los siguientes valores:
                '	0 = Transacción de Adelanto de Efectivo Normal.
                '  1 = Transacción de Adelanto de Efectivo en Cuotas
                Dim pidPlan As String = "0"
                Dim LimiteCompraDiario As Integer
                Dim LimiteDevolucionDiario As Integer
                Dim LimiteCompraDiarioCuenta As Integer
                ' 17/12 leer desde cuentasclientes cuando este el campo..
                Dim LimiteAdelantoDiario As Integer

                Dim MotivoBaja As Integer
                Select Case e.p126.Tiptran
                    Case 0
                        PtipoTransaccion = My.Settings.TipoTransaccionAdel1pago
                        pnrocuotas = 1
                        pidPlan = "1"
                    Case 1
                        PtipoTransaccion = My.Settings.TipoTransaccionAdelCtas
                        pnrocuotas = e.p126.adelanto.CantCuotas
                        pidPlan = CStr(My.Settings.IdPlanAdelCtas)
                End Select

                Dim dtr As DataRow = objConsultas.Cuentas_ObtenerDatosAut(ModBasic.conexion, e.Tarjeta.ToString)
                If dtr Is Nothing Then
                    log.logInfo("Tarjeta " + e.Tarjeta.ToString + "  inexistente")
                    RespuestaError = ONLINECOM.E_CodigosRespuesta.TARJETA_INVALIDA
                    e.Respuesta = RespuestaError
                Else

                    CtaCliente = CInt(dtr("idcuentacliente"))
                    Dim EstadoCuentaCliente As Integer = CInt(dtr("idestadocuentacliente"))

                    Try
                        FechaVigenciaDesde = CDate(dtr("FechaVigenciaDesde"))
                        FechaVigenciaHasta = CDate(dtr("FechaVigenciaHasta"))
                        log.logInfo("vigencia:" + FechaVigenciaDesde.Date.ToShortDateString + " a " + FechaVigenciaHasta.ToShortDateString)
                        If IsDBNull(dtr.Item("pfechaBaja")) Then
                            PfechaBajaOk = True
                        Else
                            PfechaBajaOk = False
                        End If
                        If IsDBNull(dtr.Item("ccfechaBaja")) Then
                            CCfechaBajaOk = True
                        Else
                            CCfechaBajaOk = False
                        End If
                        pEstadoPlastico = CInt(dtr("pidestadoPlastico"))
                        'LimiteCompraDiario = CInt(dtr("LimiteCompraDiario"))
                        'LimiteDevolucionDiario = CInt(dtr("LimiteDevolucionDiario"))
                        MotivoBaja = CInt(dtr("idMotivoBaja"))
                        If IsDBNull(dtr.Item("TopeExtraccionDiario")) Then
                            LimiteAdelantoDiario = 4000
                        Else
                            LimiteAdelantoDiario = CInt(dtr.Item("TopeExtraccionDiario"))
                        End If
                    Catch ex As Exception
                        log.logerror("error seteando campos " + ex.Message + " " + ex.StackTrace)
                    End Try

                    ' 	Al enviarse la transacción al HOST se le indicará el tipo de transacción en el Campo TIPO-TRANSACCION del TOKEN otras tarjetas, con los siguientes valores:
                    If e.Cuenta > 0 And e.Cuenta <> CtaCliente And e.implementacion = ONLINECOM.E_Implementaciones.Link Then
                        log.logDebug("Cuenta No Coincide " + CtaCliente.ToString + " " + e.Cuenta.ToString + "  inexistente")
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Cuenta_invalida
                        e.Respuesta = RespuestaError
                        Debug.Assert(e.Respuesta <> 0)
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, "Cuenta No coincide", pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    End If

                    If pEstadoPlastico <> 1 Then
                        log.logInfo("plastico" + e.Tarjeta.ToString + " estado:" + pEstadoPlastico.ToString)
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        e.Respuesta = RespuestaError
                        Debug.Assert(e.Respuesta <> 0)
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, "estado plastico", pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    End If

                    If Not PfechaBajaOk Or MotivoBaja <> -1 Then
                        log.logInfo("plastico" + e.Tarjeta.ToString + " dado de baja:")
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        e.Respuesta = RespuestaError
                        Debug.Assert(e.Respuesta <> 0)
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, "fecha Baja", pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    End If

                    If Not CCfechaBajaOk Then
                        log.logInfo("Cuenta " + e.Cuenta.ToString + " dado de baja:")
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        e.Respuesta = RespuestaError
                        Debug.Assert(e.Respuesta <> 0)
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, "CC fecha Baja", pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    End If

                    .SetLiqID(objLiq.Vencimiento.VencimientoActual(e.FechaNegocio.Date))
                    .Disponibles.DisponibleATMenCuotas = pnrocuotas > 1
                    log.logInfo("Adelanto afecta limite cuotas:" + .AdelantoEnCuotasAfectaLimiteCompraCuotas.tostring)
                    .Liquidar(CtaCliente, False, False, True, True)
                    LogearDisponibles(objLiq, e.Tarjeta)

                    Dim pEstadoAutorizacion As TarjetasControl.Autorizaciones.TEstadosAutorizaciones

                    'si se desea realizar OTRA validacion mas, se debe poner esto en RECHAZADA, se graba igual pero con rechazada

                    pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Autorizada
                    'monto utilizado para validar disponible --
                    If Math.Round(.Disponibles.DisponibleAdelantoATM, 2) < e.Monto Then
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Fondos_insuficientes
                    Else
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.OK
                    End If
                    ' aca agregar OTRAS VALIDACIONES

                    If My.Settings.HabilitarTopePlastico And .getPlastico(e.Tarjeta).DisponiblePlastico < e.Monto Then
                        log.logInfo("Plastico sin Disponible por tope " + e.Tarjeta.ToString + " Disp:" + .getPlastico(e.Tarjeta).DisponiblePlastico.ToString)
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Fondos_insuficientes
                    End If

                    ' 17/12 ---activar aca para validar LimiteAdelantosdiarios
                    If e.Monto + .Disponibles.AutorizacionesAdelantoHoy > LimiteAdelantoDiario Then
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Excede_disponible_diario
                    End If

                    If EstadoCuentaCliente <> 1 Then
                        log.logInfo("Cuenta " + CtaCliente.ToString + " estado:" + EstadoCuentaCliente.ToString)
                        'e.Respuesta = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Fondos_insuficientes
                        e.Respuesta = RespuestaError
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, "Cuenta estado " + EstadoCuentaCliente.ToString, pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        Debug.Assert(e.Respuesta <> 0)

                        CoordinarSalida()

                        Exit Sub
                    End If

                    ' forzar SAF si es un SAF porque debe aceptarse SI o SI --- DESAHBILITADO NO LO USAMOS

                    'If e.Modo = ONLINECOM.e_ModosTransaccion.SAF Then
                    '    pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.SAF
                    'End If

                    If objaut.Grabar(e.Monto, e.Tarjeta, PCuentaComercio, e.Fechatransaccion.Date, PtipoTransaccion, pEstadoAutorizacion, _
                         CInt(.Disponibles.LimiteCompra), CInt(.Disponibles.limiteCompraCuotas), CInt(.Disponibles.LimiteAdelanto), _
                         .Disponibles.DisponibleCompra, .Disponibles.DisponibleCompraCuotas, .Disponibles.DisponibleAdelanto, _
                          pnrocuotas, CInt(e.NroCupon), CInt(e.Lote), pidTerminal, Gettrace(e.REtRefNumber), _
                           False, "", pidPlan, CShort(pidTipoTerminal), e.FechaNegocio) > 0 Then

                        e.Respuesta = CType(objaut.Ultima.ResultadoIso, ONLINECOM.E_CodigosRespuesta)
                        If e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK Then
                            ' disminuir los dispnibiles
                            objLiq.Disponibles.DisponibleAdelantoATM -= e.Monto
                            log.logDebug("Se autorizo transaccion " + objaut.GetTicket)
                            Try
                                If e.Monto > My.Settings.MontoAviso Then
                                    Dim MEnsaje As String = "Se ha registrado un adelanto de " + objaut.GetTicket + vbCrLf + e.Cajero + vbCrLf + e.Fechatransaccion.ToString
                                    EncolarEMail(My.Settings.EmailAvisoMaximo, "Adelanto supera $" + My.Settings.MontoAviso.ToString, MEnsaje)
                                End If
                            Catch ex As Exception
                                log.logDebug("No se pudo mandar info a " + My.Settings.EmailAvisoMaximo)
                            End Try


                            'Try
                            '    Dim EmailUsuario As String = ""
                            '    If EmailUsuario <> "" Then
                            '        Dim MEnsaje As String = "Se ha registrado un Adelanto de Efectivo" + objaut.GetTicket + vbCrLf + e.Cajero + vbCrLf + e.Fechatransaccion.ToString
                            '        EncolarEMail(My.Settings.EmailAvisoMaximo, "Adelanto en efectivo efectuado", MEnsaje)
                            '    End If
                            'Catch ex As Exception
                            '    log.logDebug("No se pudo mandar info a " + My.Settings.EmailAvisoMaximo)
                            'End Try


                        Else
                            log.logInfo("No Se autorizo transaccion " + objaut.GetTicket + vbCrLf + objaut.Errores.ListaErrores)
                        End If
                        'si llegamos aca la consulta esta ok
                        Try

                            If objaut.RevisarReversosNoProcesados(Gettrace(e.REtRefNumber), pidTerminal) Then
                                log.logWarning("Se reverso con reverso anterior !!")
                            End If
                        Catch ex As Exception
                            log.logWarning("error " + ex.ToString + " buscando reverso anterior")
                        End Try

                    Else
                        log.logDebug("No se pudo graba autorizacion " + objaut.GetTicket + vbCrLf + objaut.Errores.ListaErrores)
                        'mapear los errores
                        e.Respuesta = ObtenerCodRespuesta(objaut.Ultima.EstadoTransacc, e.implementacion)
                        If e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK Then
                            log.logDebug("ATENCION! codigo de respuesta seteado a error del sistema por no poder grabar autorizacion")
                            e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
                        End If
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, "Cuenta estado No activo", pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try

                        If e.Modo = ONLINECOM.e_ModosTransaccion.SAF Then
                            ' no se pudo pre

                        End If
                    End If
                    CompletarDatos(e.p126, objLiq, False, pnrocuotas > 1)
                End If
            Catch ex As Exception
                log.logerror("Error Adelanto " + ex.Message + " " + ex.StackTrace)
                e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
            End Try

        End With
        CoordinarSalida()

    End Sub

    Public Function Gettrace(ByVal Orig As String) As Integer
        Return CInt(Orig)
    End Function
    Public Function ObtenerCodRespuesta(ByVal EstadoTransaccion As TarjetasControl.TEstadosTransacciones, ByVal implementacion As ONLINECOM.E_Implementaciones) As ONLINECOM.E_CodigosRespuesta
        Dim Codigo As ONLINECOM.E_CodigosRespuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
        Select Case implementacion
            Case ONLINECOM.E_Implementaciones.PosnetComercio
                Select Case EstadoTransaccion
                    Case TarjetasControl.TEstadosTransacciones.ErrorPlasticoNoActivo
                        Codigo = ONLINECOM.E_CodigosRespuesta.TARJETA_INVALIDA
                    Case TarjetasControl.TEstadosTransacciones.Normal
                        Codigo = ONLINECOM.E_CodigosRespuesta.OK

                    Case Else
                        Codigo = ONLINECOM.E_CodigosRespuesta.ErrorExplicado

                End Select
            Case Else
                Select Case EstadoTransaccion
                    Case TarjetasControl.TEstadosTransacciones.ErrorPlasticoNoActivo
                        Codigo = ONLINECOM.E_CodigosRespuesta.TARJETA_INVALIDA
                    Case TarjetasControl.TEstadosTransacciones.ErrorAut
                        Codigo = ONLINECOM.E_CodigosRespuesta.ErrorVarios
                    Case TarjetasControl.TEstadosTransacciones.ErrorCantidadCuotas, TarjetasControl.TEstadosTransacciones.ErrorPlan
                        Codigo = ONLINECOM.E_CodigosRespuesta.Nro_de_cuotas_invalidas
                    Case TarjetasControl.TEstadosTransacciones.ErrorBoletin, _
                         TarjetasControl.TEstadosTransacciones.ErrorCuentaInactiva

                        Codigo = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                    Case TarjetasControl.TEstadosTransacciones.ErrorPlasticoSinCredito
                        Codigo = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                    Case TarjetasControl.TEstadosTransacciones.Normal
                        Codigo = ONLINECOM.E_CodigosRespuesta.OK
                    Case TarjetasControl.TEstadosTransacciones.ErrorExcedeLimite
                        Codigo = ONLINECOM.E_CodigosRespuesta.Excede_disponible_diario
                    Case Else 'esto es MUY importante que no salga con error=ok si no se encuentra ninguna equivalencia de error
                        Codigo = ONLINECOM.E_CodigosRespuesta.ErrorVarios
                End Select
        End Select

        log.logDebug("Codigo de Respuesta " + EstadoTransaccion.ToString + " mapeado a " + Codigo.ToString)
        Return Codigo
    End Function

    Private Sub EscuchadorImplementado_TransaccionNEXOAdelantoReverso(ByRef e As ONLINECOM.ParametrosCom) Handles EscuchadorImplementado.TransaccionNEXOAdelantoReverso
        CoordinarEntrada()
        With objLiq
            Dim CtaCliente As Integer
            'esta consulta deberia estar en otra capa?
            Try
                Dim pidTipoTerminal As TarjetasControl.Terminal.TTipoTerminales
                Dim pidTerminal As Integer = ObtenerIdTerminal(e, pidTipoTerminal)
                Dim dtr As DataRow = objConsultas.Cuentas_ObtenerDatosAut(ModBasic.conexion, e.Tarjeta.ToString)
                Dim pnroCuotas As Integer
                Try
                    pnroCuotas = e.p126.adelanto.CantCuotas
                Catch ex As Exception
                    pnroCuotas = 1
                    log.logWarning("no se pudo determinar numero de cuotas desde el reverso " + ex.Message)
                End Try

                ' 	Al enviarse la transacción al HOST se le indicará el tipo de transacción en el Campo TIPO-TRANSACCION del TOKEN otras tarjetas, con los siguientes valores:
                If dtr Is Nothing Then
                    log.logDebug(" Tarjeta " + e.Tarjeta.ToString + "  inexistente")
                    e.Respuesta = ONLINECOM.E_CodigosRespuesta.TARJETA_INVALIDA
                Else
                    CtaCliente = CInt(dtr("idcuentacliente"))
                    Dim Srev As String = "Reverso :" + CtaCliente.ToString + " trace" + e.REtRefNumber + " " + e.Fechatransaccion.ToString + _
                    " Term:" + pidTerminal.ToString + " $" + e.Monto.ToString + " Impactar:" + e.MontoAimpactar.ToString
                    log.logDebug(Srev)
                    If objaut.Reversar(Gettrace(e.REtRefNumber), e.Fechatransaccion.Date, pidTerminal, e.Monto, e.MontoAimpactar, e.Tarjeta) Then
                        'reversa OK
                        e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK
                        'recalcular dispnible DESPUES de reversar
                        Try
                            If e.Monto > My.Settings.MontoAviso Then
                                Dim MEnsaje As String = "Se ha registrado un REVERSO de adelanto  " + vbCrLf + e.Cajero + vbCrLf + e.Fechatransaccion.ToString
                                EncolarEMail(My.Settings.EmailAvisoMaximo, "Reverso de Adelanto supera $" + My.Settings.MontoAviso.ToString, MEnsaje)
                            End If
                        Catch ex As Exception
                            log.logDebug("No se pudo mandar info a " + My.Settings.EmailAvisoMaximo)
                        End Try
                    Else
                        'error al reversar
                        e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK
                        log.logerror("Reverso no procesado ..." + Srev)
                    End If

                    .SetLiqID(objLiq.Vencimiento.VencimientoActual(e.FechaNegocio.Date))
                    .Disponibles.DisponibleATMenCuotas = pnroCuotas > 1 ' en los reversos no conozca facilmente si es en cuotas
                    .Liquidar(CtaCliente, False, False, True, True)
                    CompletarDatos(e.p126, objLiq)
                End If

            Catch ex As Exception
                log.logerror("Error reverso Adelanto " + ex.Message)
                e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
            End Try

        End With
        CoordinarSalida()


    End Sub

    Private Sub EscuchadorImplementado_TransaccionNEXOCambioPin(ByRef parametros As ONLINECOM.ParametrosCom) Handles EscuchadorImplementado.TransaccionNEXOCambioPin
        Try
            log.logInfo("CAMBIO PIN:" + parametros.pIN)

        Catch ex As Exception

        End Try
    End Sub

    Private Sub EscuchadorImplementado_TransaccionNEXOConsulta(ByRef e As ONLINECOM.ParametrosCom) Handles EscuchadorImplementado.TransaccionNEXOConsulta
        CoordinarEntrada()
        With objLiq
            Dim CtaCliente As Integer
            Dim PCuentaComercio As Integer = My.Settings.CuentaComercio
            Dim PtipoTransaccion As Short
            Dim pnrocuotas As Short
            Dim pidPlan As String = "0"
            Dim pidTipoTerminal As TarjetasControl.Terminal.TTipoTerminales
            Dim pidTerminal As Integer
            Dim FechaVigenciaDesde As Date
            Dim FechaVigenciaHasta As Date

            Dim PfechaBajaOk As Boolean
            Dim CCfechaBajaOk As Boolean
            Dim pEstadoPlastico As Integer

            Try
                pidTerminal = ObtenerIdTerminal(e, pidTipoTerminal)

            Catch ex As Exception
                log.logerror("Error Consulta Adelanto PIDTERMINAL " + ex.Message)
                e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
            End Try
            Try
                Select Case e.p126.Tiptran
                    Case 0, 2
                        PtipoTransaccion = My.Settings.TipoTransaccionAdel1pago
                        pnrocuotas = 1
                        pidPlan = "1"
                    Case 1
                        PtipoTransaccion = My.Settings.TipoTransaccionAdelCtas
                        pnrocuotas = e.p126.ConsultaAdelanto.CantCuotas
                        If pnrocuotas = 0 Then
                            log.logWarning("cantidad de cuotas viene en 0 en consulta")
                            pnrocuotas = 1
                        End If
                        pidPlan = CStr(My.Settings.IdPlanAdelCtas)
                    Case Else
                        log.logWarning("tipo incorrecto en tiptran en consulta" + e.p126.Tiptran.ToString)
                End Select
            Catch EX As Exception
                log.logerror("Error Consulta Adelanto 1 " + EX.Message)
                e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
            End Try
            Try
                Dim dtr As DataRow = objConsultas.Cuentas_ObtenerDatosAut(ModBasic.conexion, e.Tarjeta.ToString)
                If dtr Is Nothing Then
                    log.logDebug(" Tarjeta " + e.Tarjeta.ToString + "  inexistente")
                    e.Respuesta = ONLINECOM.E_CodigosRespuesta.TARJETA_INVALIDA
                Else
                    CtaCliente = CInt(dtr("idcuentacliente"))

                    Try
                        FechaVigenciaDesde = CDate(dtr("FechaVigenciaDesde"))
                        FechaVigenciaHasta = CDate(dtr("FechaVigenciaHasta"))
                        log.logInfo("vigencia:" + FechaVigenciaDesde.Date.ToShortDateString + " a " + FechaVigenciaHasta.ToShortDateString)
                        If IsDBNull(dtr.Item("pfechaBaja")) Then
                            PfechaBajaOk = True
                        Else
                            PfechaBajaOk = False
                        End If
                        If IsDBNull(dtr.Item("ccfechaBaja")) Then
                            CCfechaBajaOk = True
                        Else
                            CCfechaBajaOk = False
                        End If
                        pEstadoPlastico = CInt(dtr("pidestadoPlastico"))

                    Catch ex As Exception
                        log.logerror("error seteando campos " + ex.Message + " " + ex.StackTrace)
                    End Try

                    If e.Cuenta > 0 And e.Cuenta <> CtaCliente And e.implementacion = ONLINECOM.E_Implementaciones.Link Then
                        log.logDebug("Cuenta No Coincide " + CtaCliente.ToString + " " + e.Cuenta.ToString + "  inexistente")
                        e.Respuesta = ONLINECOM.E_CodigosRespuesta.Cuenta_invalida
                        CoordinarSalida()

                        Exit Sub
                    End If


                    If pEstadoPlastico <> 1 Then
                        log.logInfo("plastico" + e.Tarjeta.ToString + " estado:" + pEstadoPlastico.ToString)
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        e.Respuesta = RespuestaError
                        Debug.Assert(e.Respuesta <> 0)
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, "estado plastico " + pEstadoPlastico.ToString, pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    End If

                    If Not PfechaBajaOk Then
                        log.logInfo("plastico" + e.Tarjeta.ToString + " dado de baja:")
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        e.Respuesta = RespuestaError
                        Debug.Assert(e.Respuesta <> 0)
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, "fecha Baja", pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    End If

                    If Not CCfechaBajaOk Then
                        log.logInfo("Cuenta " + e.Cuenta.ToString + " dado de baja:")
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        e.Respuesta = RespuestaError
                        Debug.Assert(e.Respuesta <> 0)
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, "CC fecha Baja", pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    End If

                    ' 	Al enviarse la transacción al HOST se le indicará el tipo de transacción en el Campo TIPO-TRANSACCION del TOKEN otras tarjetas, con los siguientes valores:
                    log.logDebug("consulta adelanto " + CtaCliente.ToString)
                    .SetLiqID(objLiq.Vencimiento.VencimientoActual(e.FechaNegocio.Date))
                    .Disponibles.DisponibleATMenCuotas = pnrocuotas > 1
                    .Liquidar(CtaCliente, False, False, True, True)
                    LogearDisponibles(objLiq, e.Tarjeta)

                    Dim pEstadoAutorizacion As TarjetasControl.Autorizaciones.TEstadosAutorizaciones

                    ' se debe poner esto en RECHAZADA o similar, se graba igual pero con rechazada

                    pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Consulta
                    log.logDebug("Monto en consulta:" + e.Monto.ToString)
                    If Math.Round(.Disponibles.DisponibleAdelantoATM, 2) < e.Monto Then
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Fondos_insuficientes
                    Else
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.OK
                    End If

                    ' aca agregar otras validaciones

                    If objaut.Grabar(e.Monto, e.Tarjeta, PCuentaComercio, e.Fechatransaccion.Date, PtipoTransaccion, pEstadoAutorizacion, _
                         CInt(.Disponibles.LimiteCompra), CInt(.Disponibles.limiteCompraCuotas), CInt(.Disponibles.LimiteAdelanto), _
                         .Disponibles.DisponibleCompra, .Disponibles.DisponibleCompraCuotas, .Disponibles.DisponibleAdelanto, _
                          pnrocuotas, CInt(e.NroCupon), CInt(e.Lote), pidTerminal, Gettrace(e.REtRefNumber), _
                          False, "", pidPlan, CShort(pidTipoTerminal), e.FechaNegocio) > 0 Then

                        e.Respuesta = CType(objaut.Ultima.ResultadoIso, ONLINECOM.E_CodigosRespuesta)
                        If e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK Then
                            ' disminuir los dispnibiles
                            objLiq.Disponibles.DisponibleAdelantoATM -= e.Monto
                            log.logDebug("Se autorizo Consulta " + objaut.GetTicket)
                        Else
                            log.logDebug("No Se autorizo Consulta " + objaut.GetTicket + vbCrLf + objaut.Errores.ListaErrores)
                        End If
                        'si llegamos aca la consulta esta ok
                    Else
                        log.logDebug("No se pudo graba autorizacion de COnsulta " + objaut.GetTicket + vbCrLf + objaut.Errores.ListaErrores)
                        'mapear los errores
                        e.Respuesta = ObtenerCodRespuesta(objaut.Ultima.EstadoTransacc, e.implementacion)
                        If e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK Then
                            log.logerror("ver intenta salir ok, desde ultima.estadotransacc")
                            e.Respuesta = RespuestaError
                            If e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK Then
                                log.logerror("Intenta salir con OK al no poder grabar autorizacion de consulta!")
                                e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
                            End If
                        End If
                    End If


                    ' ----
                    CompletarDatos(e.p126, objLiq)

                    'If objaut.ObtenerMaxCuotas(PCuentaComercio, pidPlan) < pnrocuotas Then
                    '    e.Respuesta = ONLINECOM.E_CodigosRespuesta.Nro_de_cuotas_invalidas
                    'Else
                    '    e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK
                    'End If
                End If

            Catch ex As Exception
                log.logerror("Error Consulta Adelanto " + ex.Message)
                e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
            End Try

        End With
        CoordinarSalida()

    End Sub

    Private Sub EscuchadorImplementado_TransaccionNEXOCredito(ByRef e As ONLINECOM.ParametrosCom, ByVal CodigoTransaccion As ONLINECOM.E_ProcCode) Handles EscuchadorImplementado.TransaccionNEXOCredito
        CoordinarEntrada()
        With objLiq
            Dim CtaCliente As Integer
            Try
                Dim PtipoTransaccion As Short
                Dim pidTipoTerminal As TarjetasControl.Terminal.TTipoTerminales
                Dim pidPlan As String = "0"
                Dim pnrocuotas As Short = e.p63.Cuotas
                Try
                    pidPlan = CStr(e.p63.Plan)
                Catch ex As Exception
                    Logger.Error("Plan invalido " + e.p63.Plan)
                    pidPlan = "1"
                End Try
                If pidPlan = "0" Then pidPlan = "1"

                If pnrocuotas = 0 Then pnrocuotas = 1
                Dim PCuentaComercio As Integer = e.CuentaComercio
                Dim FechaVigenciaDesde As Date
                Dim FechaVigenciaHasta As Date
                Dim cvc2 As Integer
                Dim Track1 As String
                Dim track2 As String
                Dim PfechaBajaOk As Boolean
                Dim CCfechaBajaOk As Boolean
                Dim pEstadoPlastico As Integer
                Dim pnota As String = e.p63.Factura
                Dim ValidarLimites As Boolean = True
                Dim LimiteCompraDiario As Integer
                Dim LimiteDevolucionDiario As Integer
                Dim LimiteCompraDiarioCuenta As Integer
                Dim MotivoBaja As Integer
                Select Case CodigoTransaccion
                    Case ONLINECOM.E_ProcCode.Devolucion_Posnet
                        PtipoTransaccion = My.Settings.TipoTransaccionDevolucion
                        ValidarLimites = False
                        e.IdenteCobranza = My.Settings.IdEnteCobranza

                        ' validar devolucion
                    Case ONLINECOM.E_ProcCode.Compra_posnet
                        PtipoTransaccion = My.Settings.TipoTransaccionCompra
                        ValidarLimites = True
                End Select

                e.pCuentaComercio = PCuentaComercio
                e.pidplan = pidPlan
                e.pnrocuotas = pnrocuotas
                e.ptipotransaccion = PtipoTransaccion
                e.pidtipoterminal = ObtenerIdTipoTerminal(e)
                e.pidterminal = 1 'esto lo pongo inicialmente porque los errores de la BD pueden ocurrir a partir de aca
                Dim pidTerminal As Integer = ObtenerIdTerminal(e, pidTipoTerminal)
                e.pidterminal = pidTerminal

                Dim dtr As DataRow = objConsultas.Cuentas_ObtenerDatosAut(ModBasic.conexion, e.Tarjeta.ToString)
                If dtr Is Nothing Then
                    log.logDebug(" Tarjeta " + e.Tarjeta.ToString + "  inexistente")
                    RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                    e.Respuesta = RespuestaError
                Else
                    CtaCliente = CInt(dtr("idcuentacliente"))
                    Dim pEstadoAutorizacion As TarjetasControl.Autorizaciones.TEstadosAutorizaciones
                    pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Autorizada
                    RespuestaError = ONLINECOM.E_CodigosRespuesta.OK

                    If e.Cuenta > 0 And e.Cuenta <> CtaCliente Then
                        log.logDebug("Cuenta No Coincide " + CtaCliente.ToString + " " + e.Cuenta.ToString + "  inexistente")
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        e.Respuesta = RespuestaError
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, pnota, pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    Else
                        e.Cuenta = CtaCliente
                    End If
                    Dim EstadoCuentaCliente As Integer
                    Try
                        cvc2 = CInt(dtr("cvv2")) '-1 significa NULL
                        Track1 = CStr(dtr("track1"))
                        track2 = CStr(dtr("track2"))
                        FechaVigenciaDesde = CDate(dtr("FechaVigenciaDesde"))
                        FechaVigenciaHasta = CDate(dtr("FechaVigenciaHasta"))
                        log.logInfo("CVV2:" + cvc2.ToString)
                        log.logInfo("vigencia:" + FechaVigenciaDesde.Date.ToShortDateString + " a " + FechaVigenciaHasta.ToShortDateString)
                        EstadoCuentaCliente = CInt(dtr("idestadocuentacliente"))
                        '

                        If IsDBNull(dtr.Item("pfechaBaja")) Then
                            PfechaBajaOk = True
                        Else
                            PfechaBajaOk = False
                        End If
                        If IsDBNull(dtr.Item("ccfechaBaja")) Then
                            CCfechaBajaOk = True
                        Else
                            CCfechaBajaOk = False
                        End If
                        ' fechaembozado
                        pEstadoPlastico = CInt(dtr("pidestadoPlastico"))
                        'LimiteCompraDiario = CInt(dtr("LimiteCompraDiario"))
                        'LimiteDevolucionDiario = CInt(dtr("LimiteDevolucionDiario"))
                        MotivoBaja = CInt(dtr("idMotivoBaja"))
                    Catch ex As Exception
                        log.logerror("error seteando campos " + ex.Message + " " + ex.StackTrace)
                    End Try

                    .SetLiqID(objLiq.Vencimiento.VencimientoActual(e.FechaNegocio.Date))
                    .Liquidar(CtaCliente, False, False, True, True)
                    LogearDisponibles(objLiq, e.Tarjeta)
                    '  chequear disponibles Compra --- Validacion Doble por ahora
                    ' ver tipo de transaccion

                    Dim MontoMesActual As Double = Math.Round(e.Monto * (1 / pnrocuotas), 2)
                    Dim MontoMesFuturo As Double = e.Monto - MontoMesActual
                    'manejo plan 3 cuetna nueva
                    If pidPlan = "3" Then
                        pnota = ("plan cuenta nueva")
                        MontoMesActual = e.Monto
                        MontoMesFuturo = e.Monto
                    End If

                    If ValidarLimites Then

                        If .Disponibles.DisponibleCompra < MontoMesActual Then
                            pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                            RespuestaError = ONLINECOM.E_CodigosRespuesta.Fondos_insuficientes
                            If My.Settings.MensajeSAldo Then e.p63.Mensaje = "Disp.Cpra:" + .Disponibles.DisponibleCompra.ToString("00000.00")
                        Else

                            If .Disponibles.DisponibleCompraCuotas < MontoMesFuturo Then
                                pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                                RespuestaError = ONLINECOM.E_CodigosRespuesta.Fondos_insuficientes
                                If My.Settings.MensajeSAldo Then e.p63.Mensaje = "Disp.Ctas:" + .Disponibles.DisponibleCompraCuotas.ToString("00000.00")
                            End If
                        End If
                        If e.Monto < 0.5 Then
                            pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                            RespuestaError = ONLINECOM.E_CodigosRespuesta.DECLINACIÓN_EXTERNA
                            e.p63.Mensaje = "Monto muy bajo"
                            pnota = "Monto muy bajo"
                        End If

                        'validar por plastico
                        Try
                            Dim ixPlastico As Integer = .GetPlasticoIx(e.Tarjeta)
                            If e.Monto + .Plasticos(ixPlastico).xDisponible.AutorizacionesCompraHoy + .Plasticos(ixPlastico).xDisponible.AutorizacionesCompraCUotasHoy > LimiteCompraDiario _
                                And LimiteCompraDiario > 0 Then
                                pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                                RespuestaError = ONLINECOM.E_CodigosRespuesta.ErrorExplicado
                                pnota = "Excede limite diario del plastico"
                                e.p63.Mensaje = pnota

                            End If
 
                            If e.Monto + .Disponibles.AutorizacionesCompraHoy + .Disponibles.AutorizacionesCompraCUotasHoy > LimiteCompraDiarioCuenta _
                                And LimiteCompraDiarioCuenta > 0 Then
                                pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                                RespuestaError = ONLINECOM.E_CodigosRespuesta.ErrorExplicado
                                pnota = "Excede limite diario de la cuenta"
                                e.p63.Mensaje = pnota

                            End If

                            If My.Settings.HabilitarTopePlastico And .Plasticos(ixPlastico).DisponiblePlastico < e.Monto Then
                                log.logWarning("Plastico sin Disponible por tope " + e.Tarjeta.ToString + " Disp:" + .getPlastico(e.Tarjeta).DisponiblePlastico.ToString)
                                pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                                RespuestaError = ONLINECOM.E_CodigosRespuesta.ErrorExplicado
                                pnota = "Excede Limite Plastico"
                                e.p63.Mensaje = pnota
                            End If

                        Catch ex As Exception
                            log.logerror("error en calculo limite diario " + ex.Message + " " + ex.StackTrace)
                        End Try

                    End If

                    ' otras validaciones
                    If EstadoCuentaCliente <> 1 Then
                        log.logInfo("Cuenta " + CtaCliente.ToString + " estado:" + EstadoCuentaCliente.ToString)
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        pnota = "Tarjeta:" + CStr(EstadoCuentaCliente)
                        e.p63.Mensaje = "" ' "Cuenta en estado " + CStr(EstadoCuentaCliente)
                        e.Respuesta = RespuestaError
                    End If

                    If pEstadoPlastico <> 1 Then
                        log.logInfo("plastico" + e.Tarjeta.ToString + " estado:" + pEstadoPlastico.ToString)
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        pnota = "estado:" + CStr(EstadoCuentaCliente)
                        e.p63.Mensaje = "" ' "Tarjeta en estado " + CStr(pEstadoPlastico)
                        e.Respuesta = RespuestaError
                    End If

                    If Not PfechaBajaOk Or MotivoBaja <> -1 Then
                        log.logInfo("plastico" + e.Tarjeta.ToString + " dado de baja:")
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        pnota = "Tarjeta dada de Baja " + MotivoBaja.ToString
                        e.p63.Mensaje = "" '"Tarjeta dada de Baja"
                        e.Respuesta = RespuestaError
                    End If

                    If Not CCfechaBajaOk Then
                        log.logInfo("Cuenta " + e.Cuenta.ToString + " dado de baja:")
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        e.Respuesta = RespuestaError
                        Debug.Assert(e.Respuesta <> 0)
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, "CC fecha Baja", pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    End If
                    'validar cvc2 o cvv2

                    If e.modoIngreso = ONLINECOM.E_ModoIngreso.Ecommerce Then

                        'validar vto si coincide con nuestros registros
                        If (e.Vto.Year <> FechaVigenciaHasta.Year Or _
                            e.Vto.Month <> FechaVigenciaHasta.Month) And _
                            e.Vto <> ONLINECOM.Escuchador.Vtomax Then
                            RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_vencida
                            pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                            pnota = ("Plastico no coincide vto :" + e.Tarjeta.ToString + "vt:" + e.Vto.ToString + " vig:" + FechaVigenciaHasta.ToString)
                            e.p63.Mensaje = "" '"Vto Incorrecto"
                        End If

                        'validar vigencia plastico
                        If e.Fechatransaccion > DateSerial(FechaVigenciaHasta.Year, FechaVigenciaHasta.Month + 1, 0) Then
                            RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_vencida
                            pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                            pnota = ("Plastico Vencido Vigencia:" + e.Tarjeta.ToString + " " + FechaVigenciaHasta.ToString)
                            e.p63.Mensaje = "" ' "Plastico Vencido"
                            log.logInfo(pnota)
                        End If
                        If e.Fechatransaccion < FechaVigenciaDesde Then
                            RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_vencida
                            pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                            pnota = ("Plastico No vigente :" + e.Tarjeta.ToString + " " + FechaVigenciaDesde.ToString)
                            e.p63.Mensaje = "" ' "Plastico No vigente"
                            log.logInfo(pnota)
                        End If

                        If e.p63.CVC2 <> cvc2 Then
                            RespuestaError = ONLINECOM.E_CodigosRespuesta.TARJETA_INVALIDA
                            pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada

                            pnota = ("Plastico No Cvv2 :" + e.Tarjeta.ToString + " " + e.p63.CVC2.ToString + " " + cvc2.ToString)
                            e.p63.Mensaje = "" ' "CVC Incorrecto"
                            log.logInfo(pnota)
                        End If

                        If cvc2 = -1 Then
                            RespuestaError = ONLINECOM.E_CodigosRespuesta.ErrorExplicado
                            pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada

                            pnota = ("Plastico Sin Cvv2 en base:" + e.Tarjeta.ToString + " " + e.p63.CVC2.ToString + " " + cvc2.ToString)
                            e.p63.Mensaje = "" ' "Requiere activacion de CVC"
                            log.logInfo(pnota)
                        End If

                    Else
                        'validar vto si coincide con nuestros registros, corregid para evitar
                        If (e.Vto.Year <> FechaVigenciaHasta.Year Or _
                            e.Vto.Month <> FechaVigenciaHasta.Month) And _
                            e.Vto <> ONLINECOM.Escuchador.Vtomax And FechaVigenciaDesde >= DateSerial(2006, 11, 1) Then
                            RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_vencida
                            pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                            pnota = ("Plastico no coincide vto :" + e.Tarjeta.ToString + "vt:" + e.Vto.ToString + " vig:" + FechaVigenciaHasta.ToString)
                            e.p63.Mensaje = "" '"Vto Incorrecto"
                            log.logInfo(pnota)
                        End If

                        'validar vigencia plastico
                        If e.Fechatransaccion > DateSerial(FechaVigenciaHasta.Year, FechaVigenciaHasta.Month + 1, 0) Then
                            RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_vencida
                            pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                            pnota = ("Plastico Vencido Vigencia:" + e.Tarjeta.ToString + " " + FechaVigenciaHasta.ToString)
                            e.p63.Mensaje = "" '"Plastico Vencido"
                            log.logInfo(pnota)
                        End If
                        If e.Fechatransaccion < FechaVigenciaDesde Then
                            RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_vencida
                            pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                            pnota = ("Plastico No vigente :" + e.Tarjeta.ToString + " " + FechaVigenciaDesde.ToString)
                            e.p63.Mensaje = "" ' "Plastico No Vigente"
                            log.logInfo(pnota)
                        End If
                        '26/7  activado 
                        If e.modoIngreso = ONLINECOM.E_ModoIngreso.Manual Then
                            If e.p63.CVC2 <> cvc2 And cvc2 >= 0 Then
                                RespuestaError = ONLINECOM.E_CodigosRespuesta.TARJETA_INVALIDA
                                pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada

                                pnota = ("Plastico No Cvv2 :" + e.Tarjeta.ToString + " " + e.p63.CVC2.ToString + " " + cvc2.ToString)
                                e.p63.Mensaje = "" '"CVC Incorrecto"
                                log.logInfo(pnota)
                            End If
                        End If
                    End If
                        ' forzar SAF si es un       SAF porque debe aceptarse SI o SI
                        If e.Modo = ONLINECOM.e_ModosTransaccion.SAF Then
                            pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.SAF
                            pnota = "SAF:" + pnota
                        End If

                        Dim Resultado As Integer
                    Resultado = objaut.Grabar(e.Monto, e.Tarjeta, PCuentaComercio, e.Fechatransaccion.Date, PtipoTransaccion, pEstadoAutorizacion, _
                         CInt(.Disponibles.LimiteCompra), CInt(.Disponibles.limiteCompraCuotas), CInt(.Disponibles.LimiteAdelanto), _
                         .Disponibles.DisponibleCompra, .Disponibles.DisponibleCompraCuotas, .Disponibles.DisponibleAdelanto, _
                          pnrocuotas, CInt(e.NroCupon), CInt(e.Lote), pidTerminal, Gettrace(e.REtRefNumber), _
                           e.Modo = ONLINECOM.e_ModosTransaccion.SAF, pnota, pidPlan, CShort(pidTipoTerminal), e.FechaNegocio, e.IdenteCobranza)
                        If Resultado > 0 Then

                            e.Respuesta = CType(objaut.Ultima.ResultadoIso, ONLINECOM.E_CodigosRespuesta)
                            If e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK Then
                                ' disminuir los dispnibiles
                            If CodigoTransaccion = ONLINECOM.E_ProcCode.Devolucion_Posnet Then
                            Else
                                If pidPlan = "3" Then

                                Else
                                    .Disponibles.DisponibleCompra -= MontoMesActual
                                End If
                                .Disponibles.DisponibleCompraCuotas -= MontoMesFuturo

                            End If

                            ' calcular nro de autorizacion
                            If e.Modo = ONLINECOM.e_ModosTransaccion.SAF Then
                                log.logDebug("Se autorizo Compra SAF " + objaut.GetTicket)
                            Else
                                Try
                                    e.NroAutorizacion = CInt(Right(objaut.GenerarNroAutorizacion(Resultado.ToString).ToString, 6))
                                Catch ex As Exception
                                    log.logerror("seteando el nro de autorizacion ")
                                End Try
                                log.logDebug("Se autorizo Compra " + objaut.GetTicket)
                            End If
                            Try
                                objaut.ActualizarNroAut(Resultado, e.NroAutorizacion)
                            Catch ex As Exception
                                log.logerror("seteando el nro de autorizacion ")
                            End Try

                            Try
                                If e.Monto > My.Settings.MontoAviso Then
                                    Dim MEnsaje As String = "Se ha registrado una Compra de " + objaut.GetTicket + vbCrLf + e.Cajero + vbCrLf + e.Fechatransaccion.ToString
                                    EncolarEMail(My.Settings.EmailAvisoMaximo, "Compra supera $" + My.Settings.MontoAviso.ToString, MEnsaje)
                                End If
                            Catch ex As Exception
                                log.logDebug("No se pudo mandar info a " + My.Settings.EmailAvisoMaximo)
                            End Try
                            ' prueba de mensajes 
                            Try
                                'If CtaCliente = 337503 Or CtaCliente = 380199 Then
                                '    e.p63.Mensaje = "Gracias por Utilizar NEXO!"
                                '    Dim MEnsaje As String = "Se ha registrado una Compra de " + objaut.GetTicket + vbCrLf + e.Cajero + vbCrLf + e.Fechatransaccion.ToString
                                '    EncolarEMail("gustavo.marzioni@cfr.com.ar", "Nueva Compra Nexo de " + CtaCliente.ToString, MEnsaje)
                                '    Select Case CtaCliente
                                '        Case 337503
                                '            EncolarEMail("marcelo.fiuri@unidos.com.ar", "Nueva Compra Nexo", MEnsaje)
                                '    End Select
                                'Else
                                '    e.p63.Mensaje = " "
                                'End If

                                Try

                                    If objaut.RevisarReversosNoProcesados(Gettrace(e.REtRefNumber), pidTerminal) Then
                                        log.logWarning("Se reverso con reverso anterior !!")
                                    End If
                                Catch ex As Exception
                                    log.logWarning("error " + ex.ToString + " buscando reverso anterior")
                                End Try

                            Catch ex As Exception

                            End Try
                        Else
                            log.logDebug("No Se autorizo Compra " + objaut.GetTicket + vbCrLf + objaut.Errores.ListaErrores)

                        End If
                        'si llegamos aca la consulta esta ok
                    Else
                        log.logDebug("No se pudo graba Compra " + objaut.GetTicket + vbCrLf + objaut.Errores.ListaErrores)
                        'mapear los errores
                        e.Respuesta = ObtenerCodRespuesta(objaut.Ultima.EstadoTransacc, e.implementacion)
                        If e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK Then
                            log.logerror("MAL! intenta salir con OK al no poder grabar autorizacion!" + objaut.GetTicket + vbCrLf + objaut.Errores.ListaErrores + vbCrLf)
                            e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
                        End If
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, "Error grabando", pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try

                    End If

                    CompletarDatosCompra(e, objLiq, objaut)
                End If
            Catch ex As Exception
                log.logerror("Error Compra " + ex.Message + " " + ex.StackTrace)
                Try
                    e.p63.Mensaje = "Reintente.."

                Catch ex2 As Exception

                End Try
                e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
            End Try

        End With
        CoordinarSalida()

    End Sub

      
    Public Sub GrabarRechazo(ByRef e As ONLINECOM.ParametrosCom, ByVal pCuentaComercio As Integer, ByVal ptipoTransaccion As Short, _
    ByVal pnroCuotas As Short, ByVal pidTerminal As Integer, ByVal pnota As String, ByVal pidPlan As String, ByVal pidTipoTerminal As TarjetasControl.Terminal.TTipoTerminales)

        Try
            Dim irechazo As Integer
            Dim xmonto As Double
            If e.EsCredito Then
                xmonto = -e.Monto
            Else
                xmonto = e.Monto
            End If
            irechazo = (objaut.GrabarRechazoAutorizacion(xMonto, e.Tarjeta, pCuentaComercio, e.Fechatransaccion.Date, ptipoTransaccion, _
                         CInt(objLiq.Disponibles.LimiteCompra), CInt(objLiq.Disponibles.limiteCompraCuotas), CInt(objLiq.Disponibles.LimiteAdelanto), _
                         objLiq.Disponibles.DisponibleCompra, objLiq.Disponibles.DisponibleCompraCuotas, objLiq.Disponibles.DisponibleAdelanto, _
                          pnroCuotas, CInt(e.NroCupon), CInt(e.Lote), pidTerminal, Gettrace(e.REtRefNumber), _
                           e.Modo = ONLINECOM.e_ModosTransaccion.SAF, pnota, pidPlan, CShort(pidTipoTerminal), e.FechaNegocio, e.Respuesta, e.IdenteCobranza))
            log.logInfo("Rechazo:" + irechazo.ToString)
        Catch ex As Exception
            log.logerror("Error grabando Rechazo " + ex.Message + " " + ex.StackTrace)
        End Try
    End Sub

    Public Sub LogearDisponibles(ByVal l As TarjetasControl.LiquidacionesCliente, Optional ByVal pIdPlastico As Int64 = 0)
        Try
            With l.Disponibles
                Dim s As String = "Disponibles Cta " + l.CurCuenta.ToString + vbCrLf
                s += "Lim .Adel  :" + .LimiteAdelanto.ToString + vbCrLf
                s += "Lim .cpra  :" + .LimiteCompra.ToString + vbCrLf
                s += "Lim .cpctas:" + .limiteCompraCuotas.ToString + vbCrLf
                s += "Lim .Marg  :" + .LimiteMargen.ToString + vbCrLf

                s += "Cons.Adel  :" + .ConsumoAdelanto.ToString + vbCrLf
                s += "Cons.cpra  :" + .ConsumoCompra.ToString + vbCrLf
                s += "Cons.cpctas:" + .ConsumoCompraCuotas.ToString + vbCrLf
                s += "Cons.Futuro:" + .COnsumoFuturo.ToString + vbCrLf

                s += "Aut .Adel  :" + .AutorizacionesAdelanto.ToString + vbCrLf
                s += "Aut .cpra  :" + .AutorizacionesCompra.ToString + vbCrLf
                s += "Aut .cpctas:" + .AutorizacionesCompraCUotas.ToString + vbCrLf
                s += "Aut pagos  :" + .AutorizacionesPagos.ToString + vbCrLf

                s += "Saldo a    :" + l.Pagos.SaldoaPagar.ToString + vbCrLf

                s += "Disp.Adel  :" + .DisponibleAdelanto.ToString + vbCrLf
                s += "Disp.ATM   :" + .DisponibleAdelantoATM.ToString + vbCrLf
                s += "Disp.cpra  :" + .DisponibleCompra.ToString + vbCrLf
                s += "Disp.cpctas:" + .DisponibleCompraCuotas.ToString + vbCrLf
                s += "Disp.Marg  :" + .DisponibleMargen.ToString + vbCrLf
                If pIdPlastico > 0 Then
                    s += "Disp.plas    :" + l.getPlastico(pIdPlastico).DisponiblePlastico.ToString + " ------ " + vbCrLf
                    s += "Cons.Adel   P:" + l.getPlastico(pIdPlastico).xDisponible.ConsumoAdelanto.ToString + vbCrLf
                    s += "Cons.cpra   P:" + l.getPlastico(pIdPlastico).xDisponible.ConsumoCompra.ToString + vbCrLf
                    s += "Cons.cpctas P:" + l.getPlastico(pIdPlastico).xDisponible.ConsumoCompraCuotas.ToString + vbCrLf

                    s += "Aut .Adel   P:" + l.getPlastico(pIdPlastico).xDisponible.AutorizacionesAdelanto.ToString + vbCrLf
                    s += "Aut .cpra   P:" + l.getPlastico(pIdPlastico).xDisponible.AutorizacionesCompra.ToString + vbCrLf
                    s += "Aut .cpctas P:" + l.getPlastico(pIdPlastico).xDisponible.AutorizacionesCompraCUotas.ToString + vbCrLf
                End If
                s += "Aut .Adel   Hoy:" + .AutorizacionesAdelantoHoy.ToString + vbCrLf
                s += "Aut .cpra   Hoy:" + .AutorizacionesCompraHoy.ToString + vbCrLf
                s += "Aut .cpctas Hoy:" + .AutorizacionesCompraCUotasHoy.ToString + vbCrLf
                log.logInfo(s)
            End With

        Catch ex As Exception
            log.logerror("Error Logeando disponibles " + ex.Message)
        End Try
    End Sub

    Public Sub Probar(ByVal s As String)
        With objLiq

            Dim dtr As DataRow = objConsultas.Cuentas_ObtenerDatosAut(ModBasic.conexion, s)
            If dtr Is Nothing Then
                log.logDebug(" Tarjeta " + s + "  inexistente")
            Else
                Dim CtaCliente As Integer
                CtaCliente = CInt(dtr("idcuentacliente"))

                .SetLiqID(objLiq.Vencimiento.VencimientoActual(Now.Date))
                .Liquidar(CtaCliente, False, False, True, True)
                LogearDisponibles(objLiq, CLng(s))
            End If
        End With
    End Sub

    Private Sub EscuchadorImplementado_TransaccionNEXOCreditoAnulacion(ByRef e As ONLINECOM.ParametrosCom) Handles EscuchadorImplementado.TransaccionNEXOCreditoAnulacion
        CoordinarEntrada()
        With objLiq
            Dim CtaCliente As Integer
            Try
                Dim pidTipoTerminal As TarjetasControl.Terminal.TTipoTerminales
                Dim pidTerminal As Integer = ObtenerIdTerminal(e, pidTipoTerminal)
                Dim dtr As DataRow = objConsultas.Cuentas_ObtenerDatosAut(ModBasic.conexion, e.Tarjeta.ToString)
                Dim Resultado As Integer
                If dtr Is Nothing Then
                    log.logDebug(" Tarjeta " + e.Tarjeta.ToString + "  inexistente")
                    e.Respuesta = ONLINECOM.E_CodigosRespuesta.TARJETA_INVALIDA
                Else
                    CtaCliente = CInt(dtr("idcuentacliente"))
                    e.Cuenta = CtaCliente
                    Dim RefTrace As Integer = Gettrace(e.REtRefNumber)
                    Dim Srev As String = "Anulacion :" + CtaCliente.ToString + " trace" + e.REtRefNumber + " " + e.Fechatransaccion.ToString + _
                    " Term:" + pidTerminal.ToString + " $" + e.Monto.ToString + " Impactar:" + e.MontoAimpactar.ToString
                    log.logDebug(Srev)
                    If e.p90.RetRefNumber37 > 0 Then
                        log.logDebug(" validando usando campo 90")

                        Resultado = objaut.AnularPosnet(e.p90.RetRefNumber37, e.p90.Fecha13, pidTerminal, e.Monto, e.CuentaComercio, RefTrace, e.p63.Cuotas)
                    Else
                        Resultado = objaut.AnularPosnet(e.Fechatransaccion.Date, pidTerminal, e.Monto, e.CuentaComercio, RefTrace, e.p63.Cuotas, e.Tarjeta)
                    End If
                    If Resultado <> 0 Then
                        'reversa OK
                        e.NroAutorizacion = Resultado Mod 1000000
                        e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK
                        'recalcular dispnible DESPUES de reversar
                        e.p63.Mensaje = "Anulacion Realizada"

                        'Try
                        '    If objaut.RevisarReversosNoProcesados(Gettrace(e.REtRefNumber), pidTerminal) Then
                        '        log.logWarning("Se reverso con reverso anterior !!")
                        '    End If
                        'Catch ex As Exception
                        '    log.logWarning("error " + ex.ToString + " buscando reverso anterior")
                        'End Try
                    Else
                        'error al anular
                        e.Respuesta = ONLINECOM.E_CodigosRespuesta.Transaccion_Invalida
                        log.logerror("Anulacion no procesada ..." + Srev)
                        e.p63.Mensaje = "error en Anulacion:"
                    End If

                    .SetLiqID(objLiq.Vencimiento.VencimientoActual(e.FechaNegocio.Date))
                    .Liquidar(CtaCliente, False, False, True, True)
                    CompletarDatosCompra(e, objLiq, objaut)
                End If

            Catch ex As Exception
                log.logerror("Error Anulacion " + ex.Message)
                e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
            End Try

        End With
        ' 
        CoordinarSalida()
    End Sub

    Private Sub EscuchadorImplementado_TransaccionNEXOCreditoReverso(ByRef e As ONLINECOM.ParametrosCom) Handles EscuchadorImplementado.TransaccionNEXOCreditoReverso
        CoordinarEntrada()
        With objLiq
            Dim CtaCliente As Integer
            'esta consulta deberia estar en otra capa?
            Try
                Dim pidTipoTerminal As TarjetasControl.Terminal.TTipoTerminales
                Dim pidTerminal As Integer = ObtenerIdTerminal(e, pidTipoTerminal)
                Dim dtr As DataRow = objConsultas.Cuentas_ObtenerDatosAut(ModBasic.conexion, e.Tarjeta.ToString)
                ' 	Al enviarse la transacción al HOST se le indicará el tipo de transacción en el Campo TIPO-TRANSACCION del TOKEN otras tarjetas, con los siguientes valores:
                If dtr Is Nothing Then
                    log.logDebug(" Tarjeta " + e.Tarjeta.ToString + "  inexistente")
                    e.Respuesta = ONLINECOM.E_CodigosRespuesta.TARJETA_INVALIDA
                Else
                    CtaCliente = CInt(dtr("idcuentacliente"))
                    Dim Srev As String = "Reverso :" + CtaCliente.ToString + " trace" + e.REtRefNumber + " " + e.Fechatransaccion.ToString + _
                    " Term:" + pidTerminal.ToString + " $" + e.Monto.ToString + " Impactar:" + e.MontoAimpactar.ToString
                    log.logDebug(Srev)
                    If objaut.Reversar(Gettrace(e.REtRefNumber), e.Fechatransaccion.Date, pidTerminal, e.Monto, e.MontoAimpactar, e.Tarjeta) Then
                        'reversa OK
                        e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK
                        'recalcular dispnible DESPUES de reversar
                    Else
                        'error al reversar
                        e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK
                        log.logerror("Reverso no procesado ..." + Srev)
                    End If

                    .SetLiqID(objLiq.Vencimiento.VencimientoActual(e.FechaNegocio.Date))
                    .Liquidar(CtaCliente, False, False, True, True)
                    CompletarDatosCompra(e, objLiq, objaut)
                End If

            Catch ex As Exception
                log.logerror("Error reverso Compra" + ex.Message)
                e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
            End Try

        End With
        CoordinarSalida()

    End Sub

    Public Sub CompletarDatosCompra(ByVal e As ONLINECOM.ParametrosCom, _
                                    ByVal Liq As TarjetasControl.LiquidacionesCliente, _
                                    ByVal aut As TarjetasControl.Autorizaciones)
        With Liq.Disponibles
            '   e.AgregarQ2(CDec(.DisponibleAdelanto), CDec(.DisponibleCompra), CDec(.DisponibleCompraCuotas), 0, 0)
        End With

        If e.Respuesta = ONLINECOM.E_CodigosRespuesta.ErrorVarios And e.implementacion = ONLINECOM.E_Implementaciones.PosnetComercio Then
            e.Respuesta = ONLINECOM.E_CodigosRespuesta.ErrorExplicado
            e.p63.Mensaje = objaut.Errores.ListaErrores
        End If
        If e.Respuesta = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida And e.implementacion = ONLINECOM.E_Implementaciones.PosnetComercio Then
            e.Respuesta = ONLINECOM.E_CodigosRespuesta.ErrorExplicado
            If objaut.Errores.ListaErrores = "" Then
                e.p63.Mensaje = RespuestaError.ToString

            Else
                e.p63.Mensaje = objaut.Errores.ListaErrores

            End If
        End If
        If e.Respuesta = ONLINECOM.E_CodigosRespuesta.ErrorExplicado And e.implementacion = ONLINECOM.E_Implementaciones.PosnetComercio Then

            If objaut.Errores.ListaErrores.Length > 0 Then
                e.p63.Mensaje = objaut.Errores.ListaErrores
            Else
                log.logDebug("error Sin Descripcion " + objaut.Ultima.EstadoTransacc.ToString)
                e.p63.Mensaje = "Error:" + RespuestaError.ToString

            End If
        End If
        Try
            Dim s As String = "DISP.CPR:" + Liq.Disponibles.DisponibleCompra.ToString("####0.00") + "/CTAS:" + Liq.Disponibles.DisponibleCompraCuotas.ToString("####0.00")
            If My.Settings.MensajeSAldo Then e.p63.AgregarAlMensaje(s)
            log.logDebug(" saldo Informado :" + s)
        Catch ex As Exception
            log.logerror("error informando saldo " + ex.Message)
        End Try
        If e.Modo = ONLINECOM.e_ModosTransaccion.SAF Then
            e.p63.removeToken("P3")
        End If
    End Sub

    Private Sub EscuchadorImplementado_TransaccionNEXOCreditoReversoAnulacion(ByRef e As ONLINECOM.ParametrosCom) Handles EscuchadorImplementado.TransaccionNEXOCreditoReversoAnulacion
        CoordinarEntrada()
        With objLiq
            Dim CtaCliente As Integer
            'esta consulta deberia estar en otra capa?
            Try
                Dim pidTipoTerminal As TarjetasControl.Terminal.TTipoTerminales
                Dim pidTerminal As Integer = ObtenerIdTerminal(e, pidTipoTerminal)
                Dim dtr As DataRow = objConsultas.Cuentas_ObtenerDatosAut(ModBasic.conexion, e.Tarjeta.ToString)
                ' 	Al enviarse la transacción al HOST se le indicará el tipo de transacción en el Campo TIPO-TRANSACCION del TOKEN otras tarjetas, con los siguientes valores:
                If dtr Is Nothing Then
                    log.logDebug(" Tarjeta " + e.Tarjeta.ToString + "  inexistente")
                    e.Respuesta = ONLINECOM.E_CodigosRespuesta.TARJETA_INVALIDA
                Else
                    CtaCliente = CInt(dtr("idcuentacliente"))
                    Dim Srev As String = "Reverso ANulacion :" + CtaCliente.ToString + " trace" + e.REtRefNumber + " " + e.Fechatransaccion.ToString + _
                    " Term:" + pidTerminal.ToString + " $" + e.Monto.ToString + " Impactar:" + e.MontoAimpactar.ToString
                    log.logDebug(Srev)
                    If objaut.ReversarAnulacion(Gettrace(e.REtRefNumber), e.Fechatransaccion.Date, pidTerminal, e.Monto, e.MontoAimpactar, e.Tarjeta) Then
                        'reversa OK
                        e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK
                        'recalcular dispnible DESPUES de reversar
                    Else
                        'error al reversar
                        e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK
                        log.logerror("Reverso Anulacion no procesado ..." + Srev)
                    End If

                    .SetLiqID(objLiq.Vencimiento.VencimientoActual(e.FechaNegocio.Date))
                    .Liquidar(CtaCliente, False, False, True, True)
                    CompletarDatosCompra(e, objLiq, objaut)
                End If

            Catch ex As Exception
                log.logerror("Error reverso ANulacion" + ex.Message)
                e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
            End Try

        End With
        CoordinarSalida()
    End Sub

    Private Sub EscuchadorImplementado_TransaccionNEXONoImplementada(ByRef parametros As ONLINECOM.ParametrosCom) Handles EscuchadorImplementado.TransaccionNEXONoImplementada
        ' 
    End Sub

    Private Sub EscuchadorImplementado_TransaccionNEXOPagoResumen(ByRef e As ONLINECOM.ParametrosCom, ByVal CodigoTransaccion As ONLINECOM.E_ProcCode) Handles EscuchadorImplementado.TransaccionNEXOPagoResumen
        CoordinarEntrada()
        With objLiq
            Dim CtaCliente As Integer
            Try
                Dim PtipoTransaccion As Short
                Dim pidTipoTerminal As TarjetasControl.Terminal.TTipoTerminales
                ' leer desde el mensaje
                Dim pidPlan As String = "0"
                Dim pnrocuotas As Short = 1
                Dim PCuentaComercio As Integer = e.CuentaComercio
                Dim pnota As String = e.p63.Factura
                e.pCuentaComercio = PCuentaComercio
                e.pidplan = pidPlan
                e.pidterminal = 1
                e.pidtipoterminal = pidTipoTerminal
                e.pnrocuotas = pnrocuotas
                e.ptipotransaccion = PtipoTransaccion

                Dim pidTerminal As Integer = ObtenerIdTerminal(e, pidTipoTerminal)
                e.pidterminal = pidTerminal

                Dim dtr As DataRow = objConsultas.Cuentas_ObtenerDatosAut(ModBasic.conexion, e.Tarjeta.ToString)
                If dtr Is Nothing Then
                    log.logDebug(" Tarjeta " + e.Tarjeta.ToString + "  inexistente")
                    RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                    e.Respuesta = RespuestaError
                Else
                    CtaCliente = CInt(dtr("idcuentacliente"))
                    Dim pEstadoAutorizacion As TarjetasControl.Autorizaciones.TEstadosAutorizaciones
                    pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Autorizada
                    RespuestaError = ONLINECOM.E_CodigosRespuesta.OK

                    If e.Cuenta > 0 And e.Cuenta <> CtaCliente Then
                        log.logDebug("Cuenta No Coincide " + CtaCliente.ToString + " " + e.Cuenta.ToString + "  inexistente")
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        e.Respuesta = RespuestaError
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, pnota, pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    Else
                        e.Cuenta = CtaCliente
                    End If


                    .SetLiqID(objLiq.Vencimiento.VencimientoActual(e.FechaNegocio.Date))
                    .Liquidar(CtaCliente, False, False, True, True)
                    LogearDisponibles(objLiq, e.Tarjeta)


                    Dim MontoMaximo As Double = .Pagos.SaldoaPagar * 1.4
                    If MontoMaximo < 0 Then MontoMaximo = 0
                    MontoMaximo += 500
                    If MontoMaximo < Math.Abs(e.Monto) Then
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.DECLINACIÓN_EXTERNA
                        e.p63.Mensaje = " " ' "Monto demasiado grande, Max:" + MontoMaximo.ToString("00000.00")
                    End If


                    '  chequear disponibles Compra --- Validacion Doble por ahora
                    ' ver tipo de transaccion
                    PtipoTransaccion = My.Settings.TipotransaccionPago
                    ' otras validaciones
                    'validar cvc2 o cvv2
                    If e.modoIngreso = ONLINECOM.E_ModoIngreso.Ecommerce Then
                    Else
                    End If
                    ' forzar SAF si es un       SAF porque debe aceptarse SI o SI
                    If e.Modo = ONLINECOM.e_ModosTransaccion.SAF Then
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.SAF
                        pnota = "SAF:" + pnota
                    End If

                    Dim Resultado As Integer
                    Resultado = objaut.Grabar(e.Monto, e.Tarjeta, PCuentaComercio, e.Fechatransaccion.Date, PtipoTransaccion, pEstadoAutorizacion, _
                         CInt(.Disponibles.LimiteCompra), CInt(.Disponibles.limiteCompraCuotas), CInt(.Disponibles.LimiteAdelanto), _
                         .Disponibles.DisponibleCompra, .Disponibles.DisponibleCompraCuotas, .Disponibles.DisponibleAdelanto, _
                          pnrocuotas, CInt(e.NroCupon), CInt(e.Lote), pidTerminal, Gettrace(e.REtRefNumber), _
                           e.Modo = ONLINECOM.e_ModosTransaccion.SAF, pnota, pidPlan, CShort(pidTipoTerminal), e.FechaNegocio, e.IdenteCobranza)
                    If Resultado > 0 Then

                        e.Respuesta = CType(objaut.Ultima.ResultadoIso, ONLINECOM.E_CodigosRespuesta)
                        If e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK Then

                            ' calcular nro de autorizacion
                            If e.Modo = ONLINECOM.e_ModosTransaccion.SAF Then
                                log.logDebug("Se autorizo Pago SAF " + objaut.GetTicket)
                            Else
                                Try
                                    e.NroAutorizacion = CInt(Right(objaut.GenerarNroAutorizacion(Resultado.ToString).ToString, 6))
                                Catch ex As Exception
                                    log.logerror("seteando el nro de autorizacion ")
                                End Try
                                log.logDebug("Se autorizo Pago " + objaut.GetTicket)
                            End If
                            Try
                                objaut.ActualizarNroAut(Resultado, e.NroAutorizacion)
                            Catch ex As Exception
                                log.logerror("seteando el nro de autorizacion ")
                            End Try

                            Try
                                If e.Monto > My.Settings.MontoAviso Then
                                    Dim MEnsaje As String = "Se ha registrado un PAGO de " + objaut.GetTicket + vbCrLf + e.Cajero + vbCrLf + e.Fechatransaccion.ToString
                                    EncolarEMail(My.Settings.EmailAvisoMaximo, "PAGO supera $" + My.Settings.MontoAviso.ToString, MEnsaje)
                                End If
                            Catch ex As Exception
                                log.logDebug("No se pudo mandar info a " + My.Settings.EmailAvisoMaximo)
                            End Try
                            ' prueba de mensajes 
                            Try
                                If e.Tarjeta = 6281610038744002 Then
                                    e.p63.Mensaje = "ID:" + Resultado.ToString + " "
                                    Dim MEnsaje As String = "Se ha registrado un pago de " + objaut.GetTicket + vbCrLf + e.Cajero + vbCrLf + e.Fechatransaccion.ToString
                                    EncolarEMail("gustavo.marzioni@cfr.com.ar", "Pago $", MEnsaje)
                                Else
                                    e.p63.Mensaje = " "
                                End If

                            Catch ex As Exception

                            End Try
                        Else
                            log.logDebug("No Se autorizo Pago " + objaut.GetTicket + vbCrLf + objaut.Errores.ListaErrores)

                        End If
                        'si llegamos aca la consulta esta ok
                    Else
                        log.logDebug("No se pudo graba Pago " + objaut.GetTicket + vbCrLf + objaut.Errores.ListaErrores)
                        'mapear los errores
                        e.Respuesta = ObtenerCodRespuesta(objaut.Ultima.EstadoTransacc, e.implementacion)
                        If e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK Then
                            log.logerror("MAL! intenta salir con OK al no poder grabar autorizacion!" + objaut.GetTicket + vbCrLf + objaut.Errores.ListaErrores + vbCrLf)
                            e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
                        End If
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, pnrocuotas, pidTerminal, "Error grabando", pidPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try

                    End If

                    CompletarDatosCompra(e, objLiq, objaut)
                End If
            Catch ex As Exception
                log.logerror("Error Pago " + ex.Message + " " + ex.StackTrace)
                Try
                    e.p63.Mensaje = "Reintente.."

                Catch ex2 As Exception

                End Try
                e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
            End Try

        End With
        CoordinarSalida()

    End Sub

    Private Sub EscuchadorImplementado_TransaccionNEXOPAS(ByRef e As ONLINECOM.ParametrosCom) _
                                                  Handles EscuchadorImplementado.TransaccionNEXOPAS
        CoordinarEntrada()
        With objLiq
            Dim CtaCliente As Integer
            'esta consulta deberia estar en otra capa?
            Try
                Dim PCuentaComercio As Integer = My.Settings.CuentaComercioPAS
                Dim PtipoTransaccion As Short = My.Settings.TipoTransaccionPAS

                Dim pidTipoTerminal As TarjetasControl.Terminal.TTipoTerminales
                Dim pidTerminal As Integer = ObtenerIdTerminal(e, pidTipoTerminal)
                Dim pIdPlan As String = CStr(My.Settings.IdPlanPAS)
                Dim Nota As String = ""
                Dim FechaVigenciaDesde As Date
                Dim FechaVigenciaHasta As Date

                Dim PfechaBajaOk As Boolean
                Dim CCfechaBajaOk As Boolean
                Dim pEstadoPlastico As Integer
                Dim LimiteCompraDiario As Integer
                Dim LimiteDevolucionDiario As Integer
                Dim LimiteCompraDiarioCuenta As Integer
                Dim MotivoBaja As Integer
                Try
                    If e.ProcCode = ONLINECOM.E_ProcCode.CompraPulsos_Link Or e.ProcCode = ONLINECOM.E_ProcCode.CompraPulsos_Link1 Then
                        PtipoTransaccion = My.Settings.TipotransaccionPulsos
                    End If
                Catch ex As Exception
                    log.logerror(" error seteando tipo transaccion pulsos" + ex.Message)
                End Try
                Try

                    Nota = e.texto
                    If Not e.p54 Is Nothing Then
                        Nota += e.p54.ToStringDescriptivo
                    End If
                Catch ex As Exception
                    log.logDebug("no se puedo obtener nota " + ex.Message + ex.StackTrace)
                End Try

                Dim dtr As DataRow = objConsultas.Cuentas_ObtenerDatosAut(ModBasic.conexion, e.Tarjeta.ToString)
                If dtr Is Nothing Then
                    log.logDebug(" Tarjeta " + e.Tarjeta.ToString + "  inexistente")
                    e.Respuesta = ONLINECOM.E_CodigosRespuesta.TARJETA_INVALIDA
                Else
                    Try
                        log.logInfo("TITULAR:" + CStr(dtr("titular")))
                    Catch ex As Exception

                    End Try

                    Try
                        FechaVigenciaDesde = CDate(dtr("FechaVigenciaDesde"))
                        FechaVigenciaHasta = CDate(dtr("FechaVigenciaHasta"))
                        log.logInfo("vigencia:" + FechaVigenciaDesde.Date.ToShortDateString + " a " + FechaVigenciaHasta.ToShortDateString)
                        If IsDBNull(dtr.Item("pfechaBaja")) Then
                            PfechaBajaOk = True
                        Else
                            PfechaBajaOk = False
                        End If
                        If IsDBNull(dtr.Item("ccfechaBaja")) Then
                            CCfechaBajaOk = True
                        Else
                            CCfechaBajaOk = False
                        End If
                        pEstadoPlastico = CInt(dtr("pidestadoPlastico"))
                        MotivoBaja = CInt(dtr("idMotivoBaja"))
                    Catch ex As Exception
                        log.logerror("error seteando campos " + ex.Message + " " + ex.StackTrace)
                    End Try

                    CtaCliente = CInt(dtr("idcuentacliente"))
                    If e.Cuenta > 0 And e.Cuenta <> CtaCliente And e.implementacion = ONLINECOM.E_Implementaciones.Link Then
                        log.logDebug("Cuenta No Coincide " + CtaCliente.ToString + " " + e.Cuenta.ToString + "  inexistente")
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Cuenta_invalida
                        e.Respuesta = RespuestaError
                        CoordinarSalida()
                        Exit Sub
                    End If

                    Dim pEstadoAutorizacion As TarjetasControl.Autorizaciones.TEstadosAutorizaciones

                    .SetLiqID(objLiq.Vencimiento.VencimientoActual(e.FechaNegocio.Date))
                    .Disponibles.DisponibleATMenCuotas = False ' para PAS se usa el disponible de efectivo en 1 cuota?
                    .Liquidar(CtaCliente, False, False, True, True)
                    LogearDisponibles(objLiq, e.Tarjeta)

                    'si se desea realizar OTRA validacion mas, se debe poner esto en RECHAZADA, se graba igual pero con rechazada
                    If .Disponibles.DisponibleCompra < e.Monto Then
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Fondos_insuficientes
                    Else
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Autorizada
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.OK
                    End If



                    If My.Settings.HabilitarTopePlastico And .getPlastico(e.Tarjeta).DisponiblePlastico < e.Monto Then
                        log.logWarning("Plastico sin Disponible por tope " + e.Tarjeta.ToString + " Disp:" + .getPlastico(e.Tarjeta).DisponiblePlastico.ToString)
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Fondos_insuficientes
                    End If

                    'otras validaciones
                    Dim EstadoCuentaCliente As Integer = CInt(dtr("idestadocuentacliente"))
                    If EstadoCuentaCliente <> 1 Then
                        log.logInfo("Cuenta " + CtaCliente.ToString + " estado:" + EstadoCuentaCliente.ToString)
                        pEstadoAutorizacion = TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Rechazada
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Cuenta_invalida
                        e.Respuesta = RespuestaError
                        CoordinarSalida()

                        Exit Sub
                    End If


                    If pEstadoPlastico <> 1 Then
                        log.logInfo("plastico" + e.Tarjeta.ToString + " estado:" + pEstadoPlastico.ToString)
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        e.Respuesta = RespuestaError
                        Debug.Assert(e.Respuesta <> 0)
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, 1, pidTerminal, "estado plastico", pIdPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    End If

                    If Not PfechaBajaOk Or MotivoBaja <> -1 Then
                        log.logInfo("plastico" + e.Tarjeta.ToString + " dado de baja:")
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        e.Respuesta = RespuestaError
                        Debug.Assert(e.Respuesta <> 0)
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, 1, pidTerminal, "fecha Baja", pIdPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    End If

                    If Not CCfechaBajaOk Then
                        log.logInfo("Cuenta " + e.Cuenta.ToString + " dado de baja:")
                        RespuestaError = ONLINECOM.E_CodigosRespuesta.Tarjeta_restringida
                        e.Respuesta = RespuestaError
                        Debug.Assert(e.Respuesta <> 0)
                        Try
                            GrabarRechazo(e, PCuentaComercio, PtipoTransaccion, 1, pidTerminal, "CC fecha Baja", pIdPlan, pidTipoTerminal)
                        Catch ex As Exception
                            log.logerror("error llamando a grabarRechazo " + ex.Message)
                        End Try
                        CoordinarSalida()
                        Exit Sub
                    End If


                    If objaut.Grabar(e.Monto, e.Tarjeta, PCuentaComercio, e.Fechatransaccion.Date, PtipoTransaccion, pEstadoAutorizacion, _
                         CInt(.Disponibles.LimiteCompra), CInt(.Disponibles.limiteCompraCuotas), CInt(.Disponibles.LimiteAdelanto), _
                         .Disponibles.DisponibleCompra, .Disponibles.DisponibleCompraCuotas, .Disponibles.DisponibleAdelanto, _
                          1, CInt(e.NroCupon), CInt(e.Lote), pidTerminal, Gettrace(e.REtRefNumber), _
                          False, Nota, pIdPlan, CShort(pidTipoTerminal), e.FechaNegocio) > 0 Then

                        e.Respuesta = CType(objaut.Ultima.ResultadoIso, ONLINECOM.E_CodigosRespuesta)
                        If e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK Then
                            ' disminuir los dispnibiles
                            objLiq.Disponibles.DisponibleCompra -= e.Monto
                            log.logDebug("Se autorizo PAS/Compras " + objaut.GetTicket)
                        Else
                            log.logDebug("No Se autorizo PAS/Compras " + objaut.GetTicket + vbCrLf + objaut.Errores.ListaErrores)
                        End If
                        'si llegamos aca la consulta esta ok
                    Else
                        log.logDebug("No se pudo graba autorizacion de PAS/Compras " + objaut.GetTicket + vbCrLf + "errores:" + objaut.Errores.ListaErrores)
                        'mapear los errores
                        e.Respuesta = ObtenerCodRespuesta(objaut.Ultima.EstadoTransacc, e.implementacion)
                        If e.Respuesta = ONLINECOM.E_CodigosRespuesta.OK Then
                            log.logerror("MAL! intenta salir con OK al no poder grabar autorizacion!")
                            e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
                        End If
                    End If
                    CompletarDatos(e.p126, objLiq, True)
                End If
            Catch ex As Exception
                log.logerror("Error Pas/Compras " + ex.Message + " " + ex.StackTrace)
                e.Respuesta = ONLINECOM.E_CodigosRespuesta.Error_del_sistema
            End Try

        End With
        CoordinarSalida()
        '
    End Sub


    Public Sub CompletarDatos(ByRef p126 As ONLINECOM.Mensaje126Adelanto, _
                                ByRef liq As TarjetasControl.LiquidacionesCliente, _
                                Optional ByVal UsarLimiteCompra As Boolean = False, Optional ByVal enCuotas As Boolean = False)

        log.logDebug("tipo:" + CInt(p126.Tipo).ToString + " " + p126.Tipo.ToString)

        Dim DisponibleInformado As Double
        If UsarLimiteCompra Then
            log.logDebug("Se informa Disponible Compra")
            DisponibleInformado = liq.Disponibles.DisponibleCompra
        Else
            log.logDebug("Se informa Disponible Adelanto")
            DisponibleInformado = liq.Disponibles.DisponibleAdelantoATM
        End If

        With p126
            Select Case p126.Tipo
                Case ONLINECOM.Mensaje126Adelanto.e_m126_tipos.Adelanto, ONLINECOM.Mensaje126Adelanto.e_m126_tipos.aDELANTOENCUOTAS
                    With p126.adelanto
                        .MontoMaxCuotaDolar = 0
                        .SaldoCuotasDolares = 0
                        .SaldoCuotasPesos = CDec(DisponibleInformado)
                        .CantMaxCuotasDolar = 1
                        .CantMaxCuotasPesos = My.Settings.MaxCuotasInformativo
                        .DispCtaDolar = 0
                        .DispCtaPesos = CDec(DisponibleInformado)
                        p126.saldoCampo44 = .DispCtaPesos
                        .MinAdelPesos = 10
                    End With
                    p126.TipoDeb = 0

                Case ONLINECOM.Mensaje126Adelanto.e_m126_tipos.ConsultaAdelanto
                    With p126.ConsultaAdelanto
                        .CantMaxCuotasDolar = 1
                        .CantMaxCuotasPesos = My.Settings.MaxCuotasInformativo
                        .DispCtaDolar = 0
                        .DispCtaPesos = CDec(DisponibleInformado)
                        p126.saldoCampo44 = .DispCtaPesos
                        .MaxCtaDolar = 0
                        .MaxCtaPesos = My.Settings.MontoMaxAdelanto
                        .MinAdelDolar = 0
                        .MinAdelPesos = 10
                        ' ver si es en cuotas retornar la otra tasa
                    End With
                    p126.TipoDeb = 0
                Case ONLINECOM.Mensaje126Adelanto.e_m126_tipos.ConsultaAdelantocUOTAS
                    With p126.ConsultaAdelanto
                        .CantMaxCuotasDolar = 1
                        .CantMaxCuotasPesos = My.Settings.MaxCuotasInformativo
                        .DispCtaDolar = 0
                        .DispCtaPesos = CDec(DisponibleInformado)
                        p126.saldoCampo44 = .DispCtaPesos
                        .MaxCtaDolar = 0
                        .MaxCtaPesos = My.Settings.MontoMaxAdelanto
                        .MinAdelDolar = 0
                        .MinAdelPesos = 10
                    End With
                    p126.TipoDeb = 0
                Case Else '' ONLINECOM.Mensaje126Adelanto.e_m126_tipos.ConsultaSaldo
                    With p126.Consultasaldo
                        .CantAdel = My.Settings.CantidadMaxAdelantos
                        .ConsumoDolares = 0
                        .ConsumoPesos = CDec(liq.Disponibles.ConsumoCompra)
                        .Deudadolar = 0
                        .DEudaDolarConv = 0
                        .DEudaPesos = CDec(liq.LiqAnt.SaldoFinal)
                        .FechaVto = (liq.LiqAnt.Vencimiento.FechaVencimiento)
                        .LimiteAdelantos = CDec(liq.Disponibles.LimiteAdelanto)
                        .PagoDolares = 0
                        .PagoPesos = CDec(liq.Pagos.PagosACuenta)
                        .PagoMinimo = CDec(liq.LiqAnt.PagoMinimo)
                        .DispAdelantos = CDec(DisponibleInformado)
                        p126.saldoCampo44 = CDec(DisponibleInformado)
                    End With
                    p126.TipoDeb = 0

            End Select

            Dim Tasa As Decimal

            If Not liq Is Nothing Then
                If Not Encuotas Then
                    Tasa = CDec(liq.Vencimiento.TasaAdelantos)
                    log.logDebug("TASA 1 pago informada" + Tasa.ToString)
                Else
                    Tasa = CDec(liq.Vencimiento.TasaAdelantosenCuotas)
                    log.logDebug("TASA en cuotas informada" + Tasa.ToString)
                End If
                p126.Consultasaldo.DispAdelantos = CDec(DisponibleInformado)
                log.logDebug("Disponible informado " + p126.Consultasaldo.DispAdelantos.ToString)
            End If
            .TemDolar = 0
            .TemPesos = Tasa / 12
            .tnaDolar = 0
            .tnaPesos = Tasa
        End With
    End Sub

    Private Sub objaut_AsignarResultadoIso(ByVal xEstado As TarjetasControl.TEstadosTransacciones, ByRef ResultadoIso As Integer, ByRef estadoaut As TarjetasControl.Autorizaciones.TEstadosAutorizaciones, ByRef xerrores As TarjetasControl.tErrores) Handles objaut.AsignarResultadoIso
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

    Private Sub ContestadorImplementado_Error(ByVal sender As Object, ByVal e As Trx.Utilities.ErrorEventArgs) Handles ContestadorImplementado.Error
        NcomErr += 1
        Ncomerr2 += 1
        log.logerror("error de comunicaciones:" + Ncomerr2.ToString + "-" + Ncomerr2.ToString + " " + e.Exception.ToString)
        ' cambiar gateway
        If Ncomerr2 > 3 Then
            CambioGateway()
        End If
    End Sub

    Public Sub EncolarEMail(ByVal Destino As String, ByVal asunto As String, ByVal Mensaje As String)
        ReDim Preserve ColaEmails(ColaEmailsN + 1)
        With ColaEmails(ColaEmailsN + 1)
            .Mensaje = Mensaje
            .Asunto = asunto
            .Destino = Destino
        End With
        ' esto es ultimo para evitar datos incompletos
        ColaEmailsN += 1
    End Sub
    Public Sub EnviarEMailInterno(ByVal Destino As String, ByVal asunto As String, ByVal Mensaje As String)

        If Destino.Contains("@") Then
            log.logInfo("Avisando a " + Destino)
            Dim Pc As String = My.Computer.Name
            Try
                Dim s As New System.Net.Mail.SmtpClient(My.Settings.Servidor)
                s.Credentials = New System.Net.NetworkCredential(My.Settings.EmailUsuario, My.Settings.emailPwd)
                s.Send(My.Settings.FromEmail, Destino, Pc + " " + asunto, Pc + " " + Mensaje)
                log.logInfo("Correo enviado " + Destino + " " + asunto + " " + Mensaje)
            Catch ex As Exception
                log.logInfo("error enviando correo a " + Destino + " " + ex.Message + " " + ex.StackTrace)

            End Try

        End If
    End Sub
    Public Sub MandarColaEmails()

        Do While ColaEmailsN > 0
            Try
                With ColaEmails(ColaEmailsN)
                    EnviarEMailInterno(.Destino, .Asunto, .Mensaje)
                End With
            Catch ex As Exception
                log.logerror("Error enviando email " + ex.Message + " " + ex.StackTrace)
            End Try
            ColaEmailsN = ColaEmailsN - 1
        Loop
    End Sub
    Public Sub CambioGateway()
        log.logWarning("deberia cambiarse el gateway ahora")
        Ncomerr2 = 0

        Try

            Dim procID As Integer
            Dim newProc As Diagnostics.Process
            newProc = Diagnostics.Process.Start("C:\windows\system32\Cmd.exe c:\windows\CambioG.bat")
            procID = newProc.Id
            newProc.WaitForExit()
            Dim procEC As Integer = -1
            If newProc.HasExited Then
                procEC = newProc.ExitCode
            End If
            log.logInfo("Process with ID " & CStr(procID) & _
                " terminated with exit code " & CStr(procEC))
        Catch ex As Exception
            log.logWarning("error en cambio de gateway:" + ex.Message)
        End Try
    End Sub

    Private Sub ContestadorImplementado_Receive(ByVal sender As Object, ByVal e As Trx.Messaging.Channels.ReceiveEventArgs) Handles ContestadorImplementado.Receive
        EscuchadorImplementado.ProcesarMensaje(e.Message, ContestadorImplementado)
    End Sub
    Public Sub conectar(ByVal ip As String)
        ContestadorImplementado = New ONLINECOM.llamador("NEXO-" + My.User.Name.ToString + "-" + My.Computer.Name, ONLINECOM.E_Implementaciones.Banelco, ip, My.Settings.Puerto)
        ContestadorImplementado.conectar()
    End Sub

    Public Sub desconectar()
        If ContestadorImplementado IsNot Nothing Then
            ContestadorImplementado.Close()
            ContestadorImplementado.Dispose()
            ContestadorImplementado = Nothing
        End If

    End Sub

    Private Sub EscuchadorImplementado_TransaccionNexoSAFErronea(ByVal e As ONLINECOM.ParametrosCom, ByRef m As ONLINECOM.Iso8583messageNexo) Handles EscuchadorImplementado.TransaccionNexoSAFErronea
        ' ACA debo ENCONTAR UNA MANERA DE GRABAR LAS AUTORIZACIONES QUE CON EL GRABAR NO GRABARON
        ' PERO ME LAS MANDARON COMO SAF Y DEBEMOS IMPACTARLAS
        Dim texto As String
        texto = "Transaccion SAF No Grabada! encolando ..."
        texto += vbCrLf + "Error:" + e.p63.Mensaje
        texto += vbCrLf + "Fecha:" + Now.ToLongTimeString
        texto += vbCrLf + "Monto:" + e.Monto.ToString
        texto += vbCrLf + "Mensaje:" + m.ToString
        Try
            EncolarSAF(e, m)
        Catch ex As Exception
            log.logWarning("error encolando SAF " + ex.Message)
        End Try
        EncolarEMail(My.Settings.EmailAvisoSAF, "Transaccion SAF No Grabada!", texto)
    End Sub

    Public Sub EncolarSAF(ByVal e As ONLINECOM.ParametrosCom, ByVal m As ONLINECOM.Iso8583messageNexo)
        ' 
        Nsaf += 1
        ReDim Preserve Safqueue(Nsaf)
        With Safqueue(Nsaf)
            .e = e
            .Resultado = 0

        End With
    End Sub

    Public Sub ProcesarColaSaf()
        log.logInfo("Procesando Cola SAF " + Nsaf.ToString)
        Dim NsafPendiente As Integer = 0
        For i As Integer = 1 To Nsaf
            With Safqueue(i)
                If .e.pidplan < "1" Then .e.pidplan = "1"
                If .Resultado = 0 Then
                    Try
                        .e.pidtipoterminal = TarjetasControl.Terminal.TTipoTerminales.Posnet
                        Select Case .e.CodigoTransaccion
                            Case ONLINECOM.E_ProcCode.AnulacionCompra_Posnet, _
                              ONLINECOM.E_ProcCode.AnulacionDevolucion_Posnet
                                ' procesar anulacion SAF
                                Dim RefTrace As Integer = Gettrace(.e.REtRefNumber)
                                log.logDebug("Anulacion SAF:" + .e.Cuenta.ToString + " trace" + .e.REtRefNumber + " " + .e.Fechatransaccion.ToString + _
                                " Term:" + .e.pidterminal.ToString + " $" + .e.Monto.ToString + " Impactar:" + .e.MontoAimpactar.ToString)
                                If .e.p90.RetRefNumber37 > 0 Then
                                    log.logDebug(" validando usando campo 90")
                                    objaut.AnularPosnet(.e.p90.RetRefNumber37, .e.p90.Fecha13, .e.pidterminal, .e.Monto, .e.CuentaComercio, RefTrace, .e.p63.Cuotas)
                                Else
                                    objaut.AnularPosnet(.e.Fechatransaccion.Date, .e.pidterminal, .e.Monto, .e.CuentaComercio, RefTrace, .e.p63.Cuotas, .e.Tarjeta)
                                End If
                                .Resultado = -1 ' si llegamos aca , sacarlo de la cola
                            Case ONLINECOM.E_ProcCode.Compra_posnet, _
                                ONLINECOM.E_ProcCode.Devolucion_Posnet

                                .Resultado = objaut.Grabar(.e.Monto, .e.Tarjeta, .e.pCuentaComercio, .e.Fechatransaccion.Date, _
                                                             .e.ptipotransaccion, TarjetasControl.Autorizaciones.TEstadosAutorizaciones.Autorizada, _
                                                         0, 0, 0, _
                                                         0, 0, 0, _
                                                          .e.pnrocuotas, CInt(.e.NroCupon), CInt(.e.Lote), .e.pidterminal, Gettrace(.e.REtRefNumber), _
                                                           True, "SAF reintento ", .e.pidplan, CShort(.e.pidtipoterminal), .e.FechaNegocio, .e.IdenteCobranza)
                                log.logInfo(objaut.Errores.ListaErrores)

                                Try
                                    .e.NroAutorizacion = CInt(Right(objaut.GenerarNroAutorizacion(.Resultado.ToString).ToString, 6))
                                Catch ex As Exception
                                    log.logWarning("seteando el nro de autorizacion en cola saf " + ex.Message)
                                End Try
                                Try
                                    objaut.ActualizarNroAut(.Resultado, .e.NroAutorizacion)
                                Catch ex As Exception
                                    log.logerror("seteando el nro de autorizacion en cola saf " + ex.Message)
                                End Try

                            Case Else
                                log.logerror("Transaccion SAF encolada con codigo de transaccion no esperado " + .e.CodigoTransaccion.ToString)
                                .Resultado = -1  'descartado
                        End Select
                    Catch ex As Exception

                        .Resultado = 0
                        log.logerror("Error Procesando Cola SAF " + ex.Message + "  " + objaut.Errores.ListaErrores)
                    End Try

                    log.logInfo(" SAF " + i.ToString + " = " + .Resultado.ToString)
                    If .Resultado = 0 Then NsafPendiente += 1
                End If
            End With
        Next
        If Nsaf > 0 Then
            If NsafPendiente = 0 Then
                log.logInfo("Todas las safs encoladas procesadas correctamente")
                Nsaf = 0
            Else
                log.logWarning("Existen " + NsafPendiente.ToString + " safs encoladas sin procesar")

            End If
        End If
    End Sub

    Public Function HaySAFPendiente() As Integer
        Dim NsafPendiente As Integer = 0
        For i As Integer = 1 To Nsaf
            With Safqueue(i)
                If .Resultado = 0 Then NsafPendiente += 1
            End With
        next
        Return NsafPendiente
    End Function
End Module
