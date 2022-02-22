Public Class ConectorPPSinEncripcion

    Implements IDisposable

    Dim WithEvents pp As New PinPadFD

    Dim respuesta As Integer
    Sub New()
        pp.Abrir_puerto()
    End Sub

#Region "Sincronizacion"
    'Structure ComandoS00
    '    Dim codRespuesta As Integer
    '    Dim descripcion As String
    'End Structure

    'Structure ComandoS01

    '    Dim NombreHosts As String
    '    Dim PrimaryMK As Integer
    '    Dim IDEncripcion As String
    'End Structure

    'Structure ComandoS02
    '    Dim codRespuesta As Integer
    '    Dim descripcion As String
    '    Dim seriePP As String
    'End Structure

    'Dim respuestaS00 As ComandoS00
    'Dim respuestaS01 As New List(Of ComandoS01)
    'Dim respuestaS02 As ComandoS02

    'Public Function RespuestaInicioSincronizacion() As ComandoS00

    '    Return respuestaS00
    'End Function

    'Public Function RespuestaSolicitudSincronizacion() As List(Of ComandoS01)

    '    Return respuestaS01
    'End Function

    'Public Function RespuestaConfirmacionSincronizacion() As ComandoS02

    '    Return respuestaS02
    'End Function

    'Public Function Inicio_Sincronizacion(pTerminal As String, seriePP As String) As Integer
    '    llegorespuesta = False

    '    pp.Sincronizar_PinPad(pTerminal, seriePP)
    '    Esperar_Respuesta(20)
    '    If respuesta = 0 Then
    '        respuestaS00.codRespuesta = pp.CodigoRespuesta
    '        respuestaS00.descripcion = pp.EstadoSincronizacion

    '    End If

    '    Return respuesta
    'End Function

    'Public Function Solicitar_Sincronizacion(pTerminal As String) As Integer
    '    llegorespuesta = False

    '    pp.Comenzar_Sincronizacion_PinPad(pTerminal)
    '    Esperar_Respuesta(20)
    '    If respuesta = 0 Then

    '        For x As Integer = 0 To pp.CantidadHostsConMK - 1
    '            Dim host As ComandoS01
    '            host.PrimaryMK = pp.PoscicionDeMK(x)
    '            host.NombreHosts = pp.NombreDeHost(x)
    '            host.IDEncripcion = pp.IDdeEncripcion(x)
    '            respuestaS01.Add(host)
    '        Next
    '    End If

    '    Return respuesta
    'End Function



    'Public Sub Cargar_claves(host As String, tipo As Tipos_claves, clave As String, control As String, posicion As String)
    '    Dim pares As New Pares_claves
    '    pares.tipo = tipo
    '    pares.clave = clave
    '    pares.control = control
    '    pares.posicion = posicion
    '    pp.Lista_Claves.Add(host + tipo.ToString, pares)
    'End Sub

    'Public Function Confirmar_Sincronizacion(pTerminal As String) As Integer
    '    llegorespuesta = False

    '    pp.Confirmar_Sincronizacion_Pinpad(pTerminal)
    '    Esperar_Respuesta(20)
    '    If respuesta = 0 Then

    '        respuestaS02.codRespuesta = pp.CodigoRespuesta
    '        respuestaS02.descripcion = pp.RespuestaSincronizacion
    '        respuestaS02.seriePP = pp.SeriePinPad


    '    End If

    '    Return respuesta
    'End Function


    'Private Sub ControlMensajeS00() Handles pp.S00Recibido
    '    llegorespuesta = True

    'End Sub
    'Private Sub ControlMensajeS01() Handles pp.S01Recibido
    '    llegorespuesta = True

    'End Sub
    'Private Sub ControlMensajeS02() Handles pp.S02Recibido
    '    llegorespuesta = True

    'End Sub
#End Region

#Region "Inicializacion"
    Structure ComandoY00
        Dim versionSO As String
        Dim versionSOFT As String
        Dim versionKernel As String
        Dim versionKernelCTLS As String
        Dim soportaCL As String
        Dim soprtaIMP As String
        Dim soportaRSA As String
        Dim soportaTOUCH As String
        Dim serieFisico As String
    End Structure
    Public Function RespuestaInicializacion() As ComandoY00

        Return respuestaY00
    End Function

    Dim respuestaY00 As ComandoY00

    Public Function Inicializar() As Integer
        llegorespuesta = False
        pp.Inicializar_PinPad()
        Esperar_Respuesta(20)
        If respuesta = 0 Then
            respuestaY00.versionSO = pp.VersionSO
            respuestaY00.versionSOFT = pp.VersionSoft
            respuestaY00.versionKernel = pp.Versionkernel
            'respuestaY00.versionKernelCTLS = pp.VersionKernelContactLess
            'respuestaY00.soportaCL = pp.SoportaContactLess
            'respuestaY00.soprtaIMP = pp.SoportaImpresion
            'respuestaY00.soportaRSA = pp.RSAPresente
            'respuestaY00.soportaTOUCH = pp.PantallaTouch
            respuestaY00.serieFisico = pp.SeriePinPad
            Return respuesta
        Else
            Return -1
        End If
    End Function
    Private Sub ControlMensajeY00() Handles pp.Y00Recibido
        llegorespuesta = True

    End Sub
#End Region

#Region "Cancelacion"
    Structure ComandoY06
        Dim cancelado As Boolean
    End Structure
    Public Function RespuestaCancelacion() As ComandoY06

        Return respuestaY06
    End Function

    Dim respuestaY06 As ComandoY06

    Public Function Cancelar() As Integer
        'llegorespuesta = False
        pp.Cancelar()
        'Esperar_Respuesta(20)
        'If respuesta = 0 Then
        '    respuestaY06.cancelado = True
        '    Return respuesta
        'Else
        '    Return -1
        'End If
    End Function
    'Private Sub ControlMensajeY06() Handles pp.Y06Recibido
    '    llegorespuesta = True

    'End Sub
#End Region

#Region "Compra"
    Structure ComandoY01
        Dim modoIngreso As String
        Dim tarjeta As String
        Dim codigoServicio As String
        Dim codigoBanco As String
        Dim nroRegistro As String
        Dim abrebiaturaTarj As String
        Dim appChip As String
    End Structure

    Public Function RespuestaInicioCompra() As ComandoY01

        Return respuestaY01
    End Function

    Dim respuestaY01 As ComandoY01

    Public Function Inicio_Compra(cashback As Boolean) As Integer
        llegorespuesta = False
        llegoerror = False
        If cashback Then
            pp.Pedir_Tarjeta_Compra(Tipos_transacciones.compra_cash)
        Else
            pp.Pedir_Tarjeta_Compra(Tipos_transacciones.compra)
        End If

        Esperar_Respuesta(Integer.MaxValue)
        If respuesta = 0 Then
            respuestaY01.modoIngreso = pp.ModoIngreso
            respuestaY01.tarjeta = pp.PAN
            respuestaY01.codigoServicio = pp.CodServicio
            respuestaY01.codigoBanco = pp.CodBanco
            respuestaY01.nroRegistro = pp.NroRegistro
            'respuestaY01.abrebiaturaTarj = pp.Abreviatura
            'respuestaY01.appChip = pp.NombreAplicacionChip
        End If

        Return respuesta
    End Function


    Public Function Inicio_Anulacion() As Integer
        llegorespuesta = False
        llegoerror = False
        pp.Pedir_Tarjeta(Tipos_transacciones.anulacion)

        Esperar_Respuesta(Integer.MaxValue)
        If respuesta = 0 Then
            respuestaY01.modoIngreso = pp.ModoIngreso
            respuestaY01.tarjeta = pp.PAN
            respuestaY01.codigoServicio = pp.CodServicio
            respuestaY01.codigoBanco = pp.CodBanco
            respuestaY01.nroRegistro = pp.NroRegistro
            'respuestaY01.abrebiaturaTarj = pp.Abreviatura
            'respuestaY01.appChip = pp.NombreAplicacionChip
        End If

        Return respuesta
    End Function


    Private Sub ControlMensajeY01() Handles pp.Y01Recibido
        llegorespuesta = True

    End Sub

    Structure ComandoY02
        Dim tarjeta As String
        Dim serieFisico As String
        Dim idEncription As String
        Dim tipoCuenta As String
        Dim tipoEncripcion As String
        Dim Paquete As String
        Dim PinBlock As String
        Dim TarjetaHabiente As String

        'AGREGADO CHIP
        Dim criptograma As String
        Dim autorizacion As String
        Dim respEmisor As String
        Dim secuenciaPAN As String
        Dim appNombre As String
        Dim ID_app As String

        'SI NO ENCRIPTA
        Dim track1 As String
        Dim track2 As String
        Dim fecha_vencimiento As String
        Dim codigo_seguridad As String
        Dim track1_no_leido As String

        Dim modo As String

    End Structure

    Public Function RespuestaDatosAdicionales() As ComandoY02

        Return respuestaY02
    End Function

    Dim respuestaY02 As ComandoY02

    Public Function Datos_adicionales(modo As Char, u4d As Boolean, cds As Boolean, pin As Boolean, wkp As String, importe As Decimal, cashback As Decimal) As Integer
        llegorespuesta = False
        llegoerror = False
        Select Case modo
            'Case "M"
            '    pp.Solicitar_Datos_Adicionales_FD(True, cds, pin, wkd, wkp, posMK, importe, cashback)
            Case "B", "M"
                pp.Pedir_Datos_Adicionales(u4d, cds, True, pin, wkp, False, False, importe, cashback)

            Case "C"
                'pp.Solicitar_Datos_Adicionales_FD(False, False, False, wkd, wkp, posMK, importe, cashback)
        End Select


        Esperar_Respuesta(Integer.MaxValue)
        If respuesta = 0 Then
            respuestaY02.modo = pp.ModoIngreso
            respuestaY02.tarjeta = pp.PAN
            respuestaY02.serieFisico = pp.SeriePinPad
            'respuestaY02.idEncription = pp.ID_Encripcion_Unico
            respuestaY02.tipoCuenta = pp.TipoCuenta
            If pp.Encripcion = "" Then

                respuestaY02.tipoEncripcion = "N"
            Else
                respuestaY02.tipoEncripcion = pp.Encripcion
            End If
            respuestaY02.Paquete = pp.PaqueteEncriptado
            respuestaY02.PinBlock = pp.getPinEncript

            'sin encripcion
            respuestaY02.track1 = pp.Track1
            respuestaY02.track2 = pp.Track2
            respuestaY02.track1_no_leido = pp.TrackNoLeido
            respuestaY02.codigo_seguridad = pp.CodSeguridad
            respuestaY02.fecha_vencimiento = pp.Vencimiento


            If modo <> "M" Then respuestaY02.TarjetaHabiente = pp.Nombre_THabiente

            If modo = "C" Then
                respuestaY02.criptograma = pp.Criptograma
                respuestaY02.autorizacion = pp.Autorizacion
                respuestaY02.respEmisor = pp.RespEmisor
                respuestaY02.secuenciaPAN = pp.SecuenciaPan
                respuestaY02.appNombre = pp.Nombre_aplicacion
                respuestaY02.ID_app = pp.Id_aplicacion
            End If

        End If

        Return respuesta
    End Function

    Private Sub ControlMensajeY02() Handles pp.Y02Recibido
        llegorespuesta = True

    End Sub

    Structure ComandoY03

        Dim criptograma As String
        Dim autorizacion As String
        Dim codrespuesta As String
        Dim mensajeRepuesta As String
    End Structure

    Public Function RespuestaChip() As ComandoY03

        Return respuestaY03
    End Function

    Dim respuestaY03 As ComandoY03

    Public Function Responder_Chip(autorizacion As String, codResp As String, criptograma As String, respondio As Boolean) As Integer
        llegorespuesta = False
        llegoerror = False

        pp.Datos_Respuesta_Emisor(autorizacion, codResp, respondio, criptograma)



        Esperar_Respuesta(40)
        If respuesta = 0 Then
            respuestaY03.criptograma = pp.Criptograma
            respuestaY03.autorizacion = pp.Autorizacion
            If pp.CodigoRespuesta <> "  " Then
                respuestaY03.codrespuesta = codResp
            Else
                Try
                    respuestaY03.codrespuesta = pp.CodigoRespuesta
                Catch ex As Exception
                    respuestaY03.codrespuesta = -1
                End Try

            End If

            respuestaY03.mensajeRepuesta = pp.Respuesta

        End If

        Return respuesta
    End Function

    Private Sub ControlMensajeY03() Handles pp.Y03Recibido
        llegorespuesta = True

    End Sub


#End Region



#Region "Errores"
    Structure ComandoY0E
        Dim comandoError As String
        Dim codigoError As String
        Dim descripcion As String
        Dim extendido As String
    End Structure
    Dim respuestaY0E As ComandoY0E


    Public Function RespuestaErrores() As ComandoY0E

        Return respuestaY0E
    End Function
    Private Sub ControlMensajeY0E(pComando As String, pDescripcion As String, pCodigoError As String) Handles pp.Y0ERecibido

        respuestaError = pCodigoError
        respuestaY0E.comandoError = pComando
        respuestaY0E.codigoError = pCodigoError
        respuestaY0E.descripcion = pDescripcion
        'respuestaY0E.extendido = pExtendido

        llegoerror = True
    End Sub
#End Region

    Private Sub InfoDebug(s As String) Handles pp.DatosRecibidos

    End Sub

    Dim llegorespuesta, llegoerror As Boolean
    Dim respuestaError As String
    ''' <summary>
    ''' Espera un tiempo pasado como parametro a que llegue la respuesta desde el pinpad.
    ''' 0=Llego respuesta, 7=dio timeout.
    ''' </summary>
    ''' <param name="timeout">Tiempo de espera en segundos.</param>
    Private Sub Esperar_Respuesta(timeout As Integer)

        Dim tiempo As Date = Now
        Do
            'espera
        Loop Until llegorespuesta Or Now.Subtract(tiempo).TotalSeconds > timeout Or llegoerror
        If llegoerror Then
            respuesta = respuestaError
        Else
            If llegorespuesta Then
                respuesta = 0 'correcto
            Else
                respuesta = 7 'timeout
            End If


        End If



    End Sub



    Public Sub Dispose() Implements IDisposable.Dispose

        pp.Cerrar_Puerto()
    End Sub
End Class
