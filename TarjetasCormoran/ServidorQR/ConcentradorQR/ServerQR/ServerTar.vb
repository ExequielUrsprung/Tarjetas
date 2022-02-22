Option Strict On

Imports log4net
Imports System.Reflection
Imports TjComun.IdaLib
Imports System.Text


Public Class ServerTar

    Public ModoDesconectadoTCP As Boolean = False
    Public elnropinguino As String
    Property estEscuchador As Boolean
    Property estEscuchadorIPN As Boolean
    Public versionServerTar As String = "10"
    Public Property estadoSrv As Boolean
    Property generandoVTA As Boolean = False
    'TODO: sacar despues de implementado
    Public EMV As Boolean = False
    Public Qr_activo As Boolean = Estado_mod_QR
    Public qr_modo As String = Tipo_QR

    Dim WithEvents timerBackup As New System.Timers.Timer

    Public ReadOnly Property timeout_search As Integer
        Get
            Return Configuracion.timeout_qr
        End Get
    End Property

    Public ReadOnly Property Tipo_QR As String
        Get
            If Configuracion.estado_qr = Configuracion.pqr Then
                Return "P"
            Else
                Return "T"
            End If
        End Get
    End Property

    Public ReadOnly Property Estado_mod_QR As Boolean
        Get
            If Configuracion.qr_activo = Configuracion.qr Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

    Public ReadOnly Property Version_LIBQR As String
        Get
            Dim QR As New lib_QR.QR("P")
            Return QR.version

        End Get
    End Property

    Public ReadOnly Property Tipoconfiguracion As String
        Get
            Return Configuracion.TipoConfiguracion
        End Get
    End Property




    Public ReadOnly Property Version_TJCOMUN As String
        Get
            Return TjComun.IdaLib.versionTJCOMUN
        End Get
    End Property


    Public ReadOnly Property Version_SERVERTAR As String
        Get
            Return versionServerTar
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

    Public Sub arrancar()
        '--- ACA ESCUCHA LO QUE DEJA LA CAJA, ARCHIVOS .QR 
        ArrancarEscuchadorCajas()
        '--- ACA ESCUCHA LO QUE DEJA MERCADO PAGO, ARCHIVOS .IPN 
        ArrancarEscuchadorIPN()

        estadoSrv = True
        timerBackup.Interval = 1800000
        timerBackup.Enabled = True
        timerBackup.Start()
    End Sub


    Public Sub Parar_Escuchador()
        If Escuchador.Estado Then
            Logger.Warn("Deteniendo escuchador cajas...")
            Escuchador.Estado = False
            estEscuchador = Escuchador.Estado 'estescuchador es para que no muestre mas que recorre los pinguinos, muesta detenido.
        End If
    End Sub

    Public Sub Detener()
        Logger.Warn("Deteniendo Server...")
        Parar_Escuchador()
        detenerEscuchadorCajas()

        estadoSrv = False
    End Sub

    Private Sub timerBackup_Elapsed() Handles timerBackup.Elapsed
        If Configuracion.TipoConfiguracion = Configuracion.Produccion Then RespaldarMovimientos()
    End Sub



    Public Sub CierreQR()
        Logger.Info(vbNewLine &
                    "===================================================" & vbNewLine &
                    "C O M I E N Z O      C I E R R E   QR" & vbNewLine &
                    "===================================================")


        Corregir_movimientos()
        depurar_duplicados()
        Dim fecha_cierre As Date = Now
        CulminacionCierreSatisfactoriaQR(fecha_cierre)
        ListadosReportesTarjetasQR(fecha_cierre)
        ListadosReportesQRTarjetasPorPing(1, fecha_cierre)
        ListadosReportesQRTarjetasPorPing(2, fecha_cierre)
        ListadosReportesQRTarjetasPorPing(3, fecha_cierre)
        ListadosReportesQRTarjetasPorPing(4, fecha_cierre)
        ListadosReportesQRTarjetasPorPing(5, fecha_cierre)
        ListadosReportesQRTarjetasPorPing(6, fecha_cierre)
        ListadosReportesQRTarjetasPorPing(7, fecha_cierre)
        ListadosReportesQRTarjetasPorPing(8, fecha_cierre)
        If Not Exportar_TodosQR(fecha_cierre) Then
            Eliminar_exportadosQR()
            Eliminar_Archivos(fecha_cierre)
        End If
        Logger.Info(vbNewLine &
                    "===================================================" & vbNewLine &
                    "C  I  E  R  R  E  QR  F  I  N  A  L  I  Z  A  D  O" & vbNewLine &
                    "===================================================")

        'Logger.Info("Reiniciando el equipo.")
        'If My.Computer.Name <> "MARCOS-XP" And My.Computer.Name <> "BACKTARJETAS" Then Shell("Shutdown -r -t 0 -f")
    End Sub


    Public Function importar(archivo As String) As Int16
        Return ImportarMovimientos(archivo)
    End Function

    Public Function Logger() As ILog
        Return LogManager.GetLogger(MethodBase.GetCurrentMethod.DeclaringType)
    End Function


    'Public Sub Cargar_terminales(pinguino As String, caja As String)
    '    Dim qr As New lib_QR.QR(qr_modo)
    '    qr.nomarc = "Log_suc"
    '    qr.loguear = True

    '    Dim resp As lib_QR.Respuesta

    '    resp = qr.Crear_Caja("Caja " & caja & " P" & pinguino, Buscar_Suc_id(pinguino), pinguino, "caja" & caja & "p" & pinguino)
    '    If resp.id_user <> "" Then Agregar_terminal(resp.id_user, "caja" & caja & "p" & pinguino, pinguino, CByte(pinguino), CByte(caja), resp.descripcion)
    'End Sub
    'Public Sub Cargar_sucursales()
    '    Dim qr As New lib_QR.QR(qr_modo)
    '    qr.nomarc = "Log_suc"
    '    qr.loguear = True
    '    Dim id As String
    '    'Sucursales
    '    qr.Traer_cajas("30420902", "4", "caja15p4")


    '    id = qr.Crear_Sucursales("Pinguino 1", "Bv. Lehmann", "425", "Castellanos", "Santa Fe", "-31.248960", "-61.491130", "1")
    '    If id <> "" Then Agregar_sucursal(id, "1", "Pinguino 1")
    '    id = qr.Crear_Sucursales("Pinguino 2", "Av. Ernesto Salva", "960", "Castellanos", "Santa Fe", "-31.241431", "-61.499895", "2")
    '    If id <> "" Then Agregar_sucursal(id, "2", "Pinguino 2")
    '    id = qr.Crear_Sucursales("Pinguino 3", "Aristobulo del valle", "884", "Castellanos", "Santa Fe", "-31.259093", "-61.483447", "3")
    '    If id <> "" Then Agregar_sucursal(id, "3", "Pinguino 3")
    '    id = qr.Crear_Sucursales("Pinguino 4", "Roque Saenz Pena", "321", "Castellanos", "Santa Fe", "-31.251245", "-61.477773", "4")
    '    If id <> "" Then Agregar_sucursal(id, "4", "Pinguino 4")
    '    id = qr.Crear_Sucursales("Pinguino 5", "Velez Sarsfield", "1441", "Castellanos", "Santa Fe", "-31.253151", "-61.508186", "5")
    '    If id <> "" Then Agregar_sucursal(id, "5", "Pinguino 5")
    '    id = qr.Crear_Sucursales("Pinguino 6", "25 de Mayo", "1129", "Castellanos", "Santa Fe", "-31.430706", "-62.079062", "6")
    '    If id <> "" Then Agregar_sucursal(id, "6", "Pinguino 6")
    '    id = qr.Crear_Sucursales("Pinguino 7", "Gobernador Crespo", "285", "Castellanos", "Santa Fe", "-31.252160", "-61.467898", "7")
    '    If id <> "" Then Agregar_sucursal(id, "7", "Pinguino 7")
    '    id = qr.Crear_Sucursales("Pinguino 8", "Av. Luis Fanti", "295", "Castellanos", "Santa Fe", "-31.261968", "-61.497227", "8")
    '    If id <> "" Then Agregar_sucursal(id, "8", "Pinguino 8")

    'End Sub

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
    Friend WithEvents EscuchadorIPN As EscuchadorIPN
    Private Sub ArrancarEscuchadorIPN()
        EscuchadorIPN = New EscuchadorIPN(Me)
        EscuchadorIPN.arrancar()
        estEscuchadorIPN = EscuchadorIPN.Estado
    End Sub

    Private Sub detenerEscuchadorIPN()
        EscuchadorIPN.detener()
    End Sub

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
    Dim LockConsulta As New Object

    Private Function BuscarOperacion(referencia As String) As lib_QR.Respuesta
        Dim clienteQR As New lib_QR.QR(qr_modo)
        clienteQR.loguear = False
        clienteQR.nomarc = "IPN-" & Now.Date.ToString("yyyyMMdd") & ".txt"
        SyncLock LockConsulta
            Return clienteQR.Search_MerchantOrder(referencia)
        End SyncLock

    End Function

    Private Function ConsultarOperacion(nro As String) As lib_QR.Respuesta
        '------------------------------------------------------
        '--- instancio el lib_QR con la variable clienteQR 
        '------------------------------------------------------
        Dim clienteQR As New lib_QR.QR(qr_modo)
        clienteQR.loguear = False
        clienteQR.nomarc = "IPN-" & Now.Date.ToString("yyyyMMdd") & ".txt"
        SyncLock LockConsulta
            Return clienteQR.Obtener_MerchantOrder(nro)
        End SyncLock
    End Function

    Private Sub Buscar_ordenes_QR(referencia As String)
        Try
            'search
            Dim resultado = BuscarOperacion(referencia)

            If resultado.estado = lib_QR.Estado.CREATED Then
                Logger.Info("El cliente aun no escaneo el QR.")
                Exit Sub ' por si llega de nuevo y ya estaba cerrado
            ElseIf resultado.estado = lib_QR.Estado.UNKNOWN Then
                Logger.Error("Se produjo un error al consultar la operación.")
                ModificaEstadoQR(resultado.trace, resultado.estado.ToString, False)
                Exit Sub ' por si llega de nuevo y ya estaba cerrado
            ElseIf resultado.estado = lib_QR.Estado.NORESPONSE Then
                Logger.Info(resultado.descripcion)
                ModificaEstadoQR(resultado.trace, resultado.estado.ToString, False)
                Exit Sub
            ElseIf resultado.estado = lib_QR.Estado.OPENED Then
                Logger.Info("El cliente ya escaneo el QR pero aun no concreto la operacion.")
                ModificaEstadoQR(resultado.trace, resultado.estado.ToString, False)
                Exit Sub
            End If

            Dim rq As New ReqQR(resultado)

            If rq.estado = lib_QR.Estado.CLOSED And resultado.estado = lib_QR.Estado.CLOSED Then
                Logger.Info("Mensaje Search descartado.")
                Exit Sub ' por si llega de nuevo y ya estaba cerrado
            End If


            ModificaEstadoQR(resultado.trace, resultado.estado.ToString, False)

            rq.estado = resultado.estado
            If rq.estado = lib_QR.Estado.CLOSED Then
                Dim vta As TjComun.IdaLib.VtaQR
                vta.VtaExtReference = resultado.trace
                vta.VtaIdOperacion = resultado.id_operacion
                vta.VtaIDPago = resultado.id_pago
                vta.VtaIdUsuario = resultado.id_user
                vta.VtaMail = resultado.mail
                vta.VtaMensaje = resultado.descripcion
                vta.VtaOk = resultado.estado
                rq.vta = vta

                ModificaTransaccionQR(rq)

            End If

            If rq.estado <> lib_QR.Estado.OPENED Then
                responderAcaja(rq)
                crearTickDATQR(rq)
            End If

        Catch ex As Exception
            Logger.Warn("No se pudo consultar la operacion nro. " & referencia)

        End Try
    End Sub

    Private Sub Procesar_orden_QR(ipn As String) Handles EscuchadorIPN.NuevoIPN
        Try

            Dim resultado = ConsultarOperacion(ipn)

            '-------------------------------------------------------------------
            '--- HACER CLIC SOBRE ReqQR (modifica estado tabla y 
            '-------------------------------------------------------------------
            Dim rq As ReqQR
            rq = New ReqQR(resultado)
            'And resultado.estado = lib_QR.Estado.CLOSED 
            If rq.estado = lib_QR.Estado.CLOSED Or rq.estado = lib_QR.Estado.CANCELLED Then
                Logger.Info("Mensaje IPN descartado.")
                Exit Sub ' por si llega de nuevo y ya estaba cerrado
            End If

            ModificaEstadoQR(resultado.trace, resultado.estado.ToString, True)

            rq.estado = resultado.estado
            If rq.estado = lib_QR.Estado.CLOSED Then
                Dim vta As TjComun.IdaLib.VtaQR
                vta.VtaExtReference = resultado.trace
                vta.VtaIdOperacion = resultado.id_operacion
                vta.VtaIDPago = resultado.id_pago
                vta.VtaIdUsuario = resultado.id_user
                vta.VtaMail = resultado.mail
                vta.VtaMensaje = resultado.descripcion
                vta.VtaOk = resultado.estado
                rq.vta = vta

                ModificaTransaccionQR(rq)

            End If
            'SI EL ESTADO ES CLOSED RESPONDE A LA CAJA QUE TERMINO LA OPERACION. YA PAGÓ.
            If rq.estado = lib_QR.Estado.CLOSED Then
                responderAcaja(rq)
                crearTickDATQR(rq)
            End If

        Catch ex As Exception
            Logger.Warn("No se pudo consultar la operacion nro. " & ipn)
        End Try
    End Sub

    Sub ProcVTAQR(rta As lib_QR.Respuesta, terminal As String) Handles EscuchadorIPN.NuevoVtaQR
        Logger.Info("Respuesta terminal: " & terminal)
        'rta.trace = Trim(terminal) & "-" & CInt(rta.trace).ToString("000000")

        Dim rq As New ReqQR(rta)
        rq.estado = rta.estado

        If rta.tipoOperacion = "DEVOLUCION" Then

            Dim vta As TjComun.IdaLib.VtaQR
            vta.VtaExtReference = rta.trace
            vta.VtaIdOperacion = rta.id_operacion
            vta.VtaIDPago = rta.id_pago
            vta.VtaIdUsuario = rta.id_user
            vta.VtaMail = rta.mail
            vta.VtaMensaje = rta.descripcion
            vta.VtaOk = rta.estado
            rq.vta = vta
            ModificaTransaccionQR(rq)

            responderAcaja(rq)

        ElseIf rta.tipoOperacion = "COMPRA" Then
            If rta.estado = lib_QR.Estado.UNKNOWN Then

                Dim vta As TjComun.IdaLib.VtaQR
                vta.VtaExtReference = rta.trace
                vta.VtaIdOperacion = rta.id_operacion
                vta.VtaIDPago = rta.id_pago
                vta.VtaIdUsuario = rta.id_user
                vta.VtaMail = rta.mail
                vta.VtaMensaje = rta.descripcion
                vta.VtaOk = rta.estado
                rq.vta = vta
                ModificaEstadoQR(rta.trace, rta.estado.ToString, True)

                responderAcaja(rq)
                Logger.Error("No se generó la orden, reintentar.")
            Else

                Dim vta As TjComun.IdaLib.VtaQR
                vta.VtaExtReference = rta.trace
                vta.VtaIdOperacion = rta.id_operacion
                vta.VtaIDPago = rta.id_pago
                vta.VtaIdUsuario = rta.id_user
                vta.VtaMail = rta.mail
                vta.VtaMensaje = rta.descripcion
                vta.VtaOk = rta.estado
                rq.vta = vta
                'ModificaEstadoQR(rta.trace, rta.estado.ToString, True)

                informarAcaja(rq)
                Logger.Info("Informando Orden creada a Caja.")

            End If
        ElseIf rta.tipoOperacion = "CANCELA" Then

            Dim vta As TjComun.IdaLib.VtaQR
            vta.VtaExtReference = rta.trace
            vta.VtaIdUsuario = rta.id_user
            vta.VtaMensaje = rta.descripcion
            vta.VtaOk = rta.estado
            rq.vta = vta
            'ModificaEstadoQR(rta.trace, rta.estado.ToString, True)

            informarAcaja(rq)
            Logger.Info("La orden " & vta.VtaExtReference & " fue cancelada")
        End If
    End Sub



    Sub ProcQR(ida As idaQR, nrocaja As Integer, PING As Integer) Handles Escuchador.NuevoQR
        '--- haciendo click en ReqQR  (setea ping, caja, terminal, incrementa trace,....)
        '--- lo hace buscando en la tabla terminales
        Dim rq As New ReqQR(ida, nrocaja, PING)
        If rq.RespInvalida Then
            Me.ResponderInvalidaACaja(rq)
        Else
            '--- ACA MANDA EL AgenteQR.EXE 
            If Not MandarMensajeMP(rq) Then
                rq.InvalidarReq("INVALIDA: Error en envío, reintente mas tarde.")
                ResponderInvalidaACaja(rq)
            End If
        End If
    End Sub



    Private Function MandarMensajeMP(req As ReqQR) As Boolean
        Try
            Dim starinfo As New ProcessStartInfo
            starinfo.FileName = Configuracion.DirLocal & "\QR\AgenteQR.exe"
            starinfo.UseShellExecute = False
            starinfo.WindowStyle = ProcessWindowStyle.Hidden
            starinfo.CreateNoWindow = True

            If req.ida.TIPOOPERACION = "COMPRA" Then
                AgregarMovimientoQR(req)      '--- AGREGA REGISTRO EN TABLA TRANSACCION COMO CREATED
                starinfo.Arguments = "COMPRA@" & GenerarMensajeQR(req).ToString & "@" & req.external_id & "@" & qr_modo
            ElseIf req.ida.TIPOOPERACION = "CANCELA" Then
                ModificaEstadoQR(req.external_id & "-" & req.external_ref, lib_QR.Estado.CANCELLED.ToString, False)
                starinfo.Arguments = "CANCELA@" & req.external_id & "-" & req.external_ref & "@" & req.external_id & "@" & qr_modo
            ElseIf req.ida.TIPOOPERACION = "DEVOLUCION" Then
                AgregarMovimientoQR(req)
                starinfo.Arguments = "DEVOLUCION@" & GenerarMensajeQR(req).ToString & "@" & req.external_id & "@" & qr_modo
            End If

            If starinfo.Arguments <> "" Then
                Logger.Info("Enviando " & req.ida.TIPOOPERACION & " Caja: " & req.nroCaja & " Ping: " & req.pinguino)
                '--- ACA MANDA EL AgenteQR.EXE 
                System.Diagnostics.Process.Start(starinfo)
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            Return False
        End Try
    End Function

    Private Function GenerarMensajeQR(req As ReqQR) As lib_QR.Operacion_Compra
        Dim trx As New lib_QR.Operacion_Compra With {
            .idTerminal = req.external_id,
            .moneda = lib_QR.Moneda.ARS
        }

        If req.ida.TIPOOPERACION = "COMPRA" Then
            Logger.Info("------ GENERANDO MENSAJE QR Terminal: " & req.external_id & vbNewLine &
                    "Tipo Operacion: " & req.ida.TIPOOPERACION & vbNewLine &
                    "Trace: " & req.external_id & "-" & req.external_ref.ToString("000000") & vbNewLine &
                    "Importe: " & CStr(CDec(req.ida.IMPORTE) / 100))
            trx.trace = req.external_id & "-" & req.external_ref.ToString("000000")
            trx.importe = CStr(CDec(req.ida.IMPORTE) / 100)
            trx.importe = trx.importe.Replace(",", ".")
        ElseIf req.ida.TIPOOPERACION = "DEVOLUCION" Then
            Logger.Info("GENERANDO MENSAJE QR" & vbNewLine &
                    "Tipo Operacion: " & req.ida.TIPOOPERACION & vbNewLine &
                    "Trace: " & req.external_id & "-" & req.external_ref.ToString("000000") & vbNewLine &
                    "Importe: " & CStr(CDec(req.ida.IMPORTE) / 100))
            trx.trace = req.external_id & "-" & req.external_ref.ToString("000000")
            trx.importe = "0"
            trx.idpago = req.idPago
        End If
        Return trx
    End Function


    ''' <summary>
    ''' Arma la respuesta en el caso que haya un error antes de enviar el requerimiento. No se envia al host y se responde a caja.
    ''' </summary>
    ''' <param name="req"></param>
    ''' <remarks></remarks>
    Sub ResponderInvalidaACaja(req As ReqQR)
        With req.vta
            .VtaMensaje = req.CausaInvalida
            .VtaOk = req.estado
        End With
        'crearTickDATNoAprobado(req)
        responderAcaja(req)
    End Sub


    Private Sub crearTickDATNoAprobado(req As ReqQR)
        'Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + req.ida.ticket.ToString + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
        'Try
        '    Logger.Info(String.Format("Creando {0}", nombreCupon))

        '    Dim archivoTick = New System.IO.FileStream(Configuracion.DirLocal + "\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
        '    'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
        '    'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
        '    Dim renglonTick = New System.IO.StreamWriter(archivoTick)
        '    With req
        '        renglonTick.WriteLine("             " + .vta.VtaEmiName)

        '        renglonTick.WriteLine("       COMPRA")
        '        renglonTick.WriteLine("             PINGUINO " + req.pinguino)

        '        renglonTick.WriteLine("             CAJA: " + .nroCaja.ToString("00"))


        '        renglonTick.WriteLine("Importe TOTAL: $" + (Math.Round(CInt(.vta.VtaMontop) / 100, 2)).ToString("###,##0.00") + ".-")

        '        renglonTick.WriteLine("Nro de comprobante:" + .ida.ticket.ToString)

        '        renglonTick.WriteLine("****************************************")
        '        renglonTick.WriteLine("       R  E  C  H  A  Z  A  D  O")
        '        renglonTick.WriteLine("   " + Mid(req.vta.VtaMenResp, 1, 37))
        '        renglonTick.WriteLine(" " + Mid(req.vta.VtaMensaje, 1, 39))
        '        renglonTick.WriteLine("****************************************")

        '    End With
        '    renglonTick.Close()

        'Catch ex As Exception
        '    Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
        '                 ex.Message)
        'End Try

        'Try
        '    Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\" + req.pinguino + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
        '    If System.IO.File.Exists(Configuracion.DirTarjetas + "\" + req.pinguino + "\TICK00" + req.nroCaja.ToString("00") + ".DAT") Then
        '        System.IO.File.Delete(Configuracion.DirTarjetas + "\" + req.pinguino + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
        '    End If
        '    System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\" + req.pinguino + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
        'Catch ex As Exception
        '    Logger.Error(String.Format("No se pudo renombrar cupón {0}", nombreCupon))
        'End Try

        'Try
        '    Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
        '    System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\Cupones\" + nombreCupon)
        'Catch ex As Exception
        '    Logger.Error(String.Format("No se pudo copiar cupón {0}", nombreCupon))
        'End Try

    End Sub


    Public Sub responderAcaja(req As ReqQR)
        Dim cadena As String = ""
        With Logger()
            .Info(vbNewLine &
                  String.Format("        ------ VTA RESPUESTA A Ping: {0} Caja: {1} ------", req.pinguino, req.nroCaja) & vbNewLine &
            "        Mensaje:   " + req.vta.VtaMensaje & vbNewLine &
            "        Estado:     " + req.estado.ToString & vbNewLine &
            "        Trace:  " + CStr(req.external_ref) & vbNewLine &
            "        ID Pago:       " + req.vta.VtaIDPago & vbNewLine &
            "        ID Operación:     " + req.vta.VtaIdOperacion & vbNewLine &
            "        ID Usuario:    " + req.vta.VtaIdUsuario & vbNewLine &
            "        MAIL:    " + req.vta.VtaMail)
        End With
        Dim nombreCupon As String = "QR" + req.pinguino + req.nroCaja.ToString("00") + "_" + Trim(req.ticket) + "_" + Now.ToString("yyMMddHHmmss")
        Logger.Info(String.Format("Creando {0}", nombreCupon))
        Try
            Dim archivoVta = New System.IO.FileStream(Configuracion.DirLocal + "\Respuestas\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Dim archivoVta = New System.IO.FileStream(req.ida.cajadir + "\CAJA00" + req.nroCaja.ToString("00") + ".VTA", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglon = New System.IO.StreamWriter(archivoVta)

            With req
                cadena = .vta.VtaOk & "|"
                cadena = cadena & .vta.VtaIDPago & "|"
                cadena = cadena & .vta.VtaExtReference & "|"
                cadena = cadena & .vta.VtaIdUsuario & "|"
                cadena = cadena & .vta.VtaMail & "|"
                cadena = cadena & .vta.VtaIdOperacion & "|"
                cadena = cadena & .vta.VtaMensaje
                renglon.WriteLine(cadena)
            End With
            renglon.Close()
        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo generar el archivo CAJA00{0}.VTA", req.nroCaja) & vbNewLine &
                        ex.Message)
        End Try

        Try
            Logger.Info(Configuracion.DirLocal + "\Respuestas\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\" + req.pinguino + "\CAJA00" + req.nroCaja.ToString("00") + ".VTA")
            If System.IO.File.Exists(Configuracion.DirTarjetas + "\" + req.pinguino + "\CAJA00" + req.nroCaja.ToString("00") + ".VTA") Then
                System.IO.File.Delete(Configuracion.DirTarjetas + "\" + req.pinguino + "\CAJA00" + req.nroCaja.ToString("00") + ".VTA")
            End If
            System.IO.File.Copy(Configuracion.DirLocal + "\Respuestas\" + nombreCupon, Configuracion.DirTarjetas + "\" + req.pinguino + "\CAJA00" + req.nroCaja.ToString("00") + ".VTA")
            ' AgregarAVTAPendientes(ClaveVta(req.pinguino, req.nroCaja), req)
            generandoVTA = False

        Catch ex As Exception
            Logger.Error(String.Format("No se pudo renombrar respuesta VTA {0}", nombreCupon))
        End Try

    End Sub

    Public Sub informarAcaja(req As ReqQR)
        Dim cadena As String = ""
        With Logger()
            .Info(vbNewLine &
                  String.Format("        ------ VTA RESPUESTA A Ping: {0} Caja: {1} ------", req.pinguino, req.nroCaja) & vbNewLine &
            "        Mensaje:   " + req.vta.VtaMensaje & vbNewLine &
            "        Estado:     " + req.estado.ToString & vbNewLine &
            "        Trace:  " + CStr(req.external_ref) & vbNewLine &
            "        ID Pago:       " + req.vta.VtaIDPago & vbNewLine &
            "        ID Operación:     " + req.vta.VtaIdOperacion & vbNewLine &
            "        ID Usuario:    " + req.vta.VtaIdUsuario & vbNewLine &
            "        MAIL:    " + req.vta.VtaMail)
        End With
        Dim nombreCupon As String = "QR" + req.pinguino + req.nroCaja.ToString("00") + "_" + Trim(req.ticket) + "_" + Now.ToString("yyMMddHHmmss")
        Logger.Info(String.Format("Creando {0}", nombreCupon))
        Try
            Dim archivoVta = New System.IO.FileStream(Configuracion.DirLocal + "\Respuestas\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Dim archivoVta = New System.IO.FileStream(req.ida.cajadir + "\CAJA00" + req.nroCaja.ToString("00") + ".VTA", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglon = New System.IO.StreamWriter(archivoVta)

            With req

                cadena = .vta.VtaOk & "|"
                cadena = cadena & .vta.VtaIDPago & "|"
                cadena = cadena & .vta.VtaExtReference & "|"
                cadena = cadena & .vta.VtaIdUsuario & "|"
                cadena = cadena & .vta.VtaMail & "|"
                cadena = cadena & .vta.VtaIdOperacion & "|"
                cadena = cadena & .vta.VtaMensaje
                renglon.WriteLine(cadena)
            End With
            renglon.Close()
        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo generar el archivo CAJA00{0}.VTA", req.nroCaja) & vbNewLine &
                        ex.Message)
        End Try

        Try
            Logger.Info(Configuracion.DirLocal + "\Respuestas\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\" + req.pinguino + "\CAJA00" + req.nroCaja.ToString("00") + ".INF")
            If System.IO.File.Exists(Configuracion.DirTarjetas + "\" + req.pinguino + "\CAJA00" + req.nroCaja.ToString("00") + ".INF") Then
                System.IO.File.Delete(Configuracion.DirTarjetas + "\" + req.pinguino + "\CAJA00" + req.nroCaja.ToString("00") + ".INF")
            End If
            System.IO.File.Copy(Configuracion.DirLocal + "\Respuestas\" + nombreCupon, Configuracion.DirTarjetas + "\" + req.pinguino + "\CAJA00" + req.nroCaja.ToString("00") + ".INF")
            ' AgregarAVTAPendientes(ClaveVta(req.pinguino, req.nroCaja), req)
            generandoVTA = False

        Catch ex As Exception
            Logger.Error(String.Format("No se pudo renombrar respuesta INF {0}", nombreCupon))
        End Try

    End Sub

    Public Function ProcesarQRPendientes() As Boolean
        Dim operacionespendientes = BuscarTransaccionQR_Atrasadas(timeout_search)
        Dim base_con_data As Boolean = False
        For Each op In operacionespendientes
            Buscar_ordenes_QR(op)
            base_con_data = True
        Next
        Return base_con_data
    End Function


    Private Sub crearTickDATQR(req As ReqQR)
        Dim nombreCupon As String = req.pinguino + req.nroCaja.ToString("00") + "_" + Trim(req.ticket) + "_" + Now.ToString("yyMMddHHmmss") + ".DAT"
        Try
            Logger.Info(String.Format("Creando {0}", nombreCupon))

            Dim archivoTick = New System.IO.FileStream(Configuracion.DirLocal + "\Cupones\" + nombreCupon, System.IO.FileMode.Create, System.IO.FileAccess.Write)
            'Logger.Info(String.Format("Creando {0}\TICK00{1}.DAT", req.ida.cajadir, req.nroCaja.ToString("00")))
            'Dim archivoTick = New System.IO.FileStream(req.ida.cajadir + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", System.IO.FileMode.Create, System.IO.FileAccess.Write)
            Dim renglonTick = New System.IO.StreamWriter(archivoTick)
            With req
                renglonTick.WriteLine("      M E R C A D O   P A G O")
                renglonTick.WriteLine("          ")
                renglonTick.WriteLine("             PINGUINO " + req.pinguino)
                renglonTick.WriteLine("             CAJA: " + .nroCaja.ToString("00"))
                renglonTick.WriteLine("         " + Trim(req.ida.TIPOOPERACION))
                renglonTick.WriteLine("Term:" + .pos_id.PadRight(8))
                renglonTick.WriteLine("ID Pago:  " + .vta.VtaIDPago)
                renglonTick.WriteLine("ID Operacion:  " + .vta.VtaIdOperacion)
                renglonTick.WriteLine("Compra: $" + .importe + ".-")
                renglonTick.WriteLine("Nro de comprobante:" + .ticket.ToString)
                renglonTick.WriteLine("ID Usuario: " + .vta.VtaIdUsuario)
            End With
            renglonTick.Close()

        Catch ex As Exception
            Logger.Fatal(String.Format("No se pudo crear correctamente el ticket para la caja: {0} ping: {1}", req.nroCaja.ToString, req.pinguino) & vbNewLine &
                         ex.Message)
        End Try

        'BORRANDO
        Try
            If System.IO.File.Exists(Configuracion.DirTarjetas + "\" + req.pinguino + "\TICK00" + req.nroCaja.ToString("00") + ".DAT") Then
                Logger.Info("Borrando " + Configuracion.DirTarjetas + "\" + req.pinguino + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
                System.IO.File.Delete(Configuracion.DirTarjetas + "\" + req.pinguino + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            Else
                Logger.Info("No se encontró " + Configuracion.DirTarjetas + "\" + req.pinguino + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            End If
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo borrar cupón TICK00{0}", req.nroCaja.ToString("00")) & vbNewLine &
                         ex.Message)
        End Try

        'RENOMBRANDO
        Try
            Logger.Info(Configuracion.DirLocal + "\Cupones\" + nombreCupon + "-->" + Configuracion.DirTarjetas + "\" + req.pinguino + "\TICK00" + req.nroCaja.ToString("00") + ".DAT")
            System.IO.File.Copy(Configuracion.DirLocal + "\Cupones\" + nombreCupon, Configuracion.DirTarjetas + "\" + req.pinguino + "\TICK00" + req.nroCaja.ToString("00") + ".DAT", True)
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


End Class

