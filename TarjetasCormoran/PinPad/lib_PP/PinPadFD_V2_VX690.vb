Imports System.Timers
Imports System.Text
Imports System.Security.Cryptography

Public Class PinPadFD_V2_VX690

    Private WithEvents puerto As New Puerto
    Dim WithEvents tm2 As New Timer(8000)
    Private nro_nak_recibidos As Integer
    Private nro_nak_enviados As Integer

#Region "Constantes"
    Private Const STX = Chr(&H2)
    Private Const ETX = Chr(&H3)
    Private Const EOT = Chr(&H4)
    Private Const ACK = Chr(&H6)
    Private Const NAK = Chr(&H15)
    Private Const FS = Chr(&H1C)
    Private Const ESP = Chr(&H20)
    Private Const SI = Chr(&HF)
    Private Const SO = Chr(&HE)
    Private Const DLE = Chr(&H10)

    Private Const INICIALIZAR = "Y00"
    Private Const LEER_TARJETA = "Y01"
    Private Const DATOS_ADICIONALES = "Y02"
    Private Const RESPUESTA_EMISOR = "Y03"
    Private Const REINGRESO_PIN = "Y04"
    Private Const LEER_TARJETA_CL = "Y21"
    Private Const ACTUALIZACION_REMOTA = "Y31"
    Private Const LEER_TARJETA_MULTIMODO = "Y41"
    Private Const CANCELAR_COMANDO = "Y06"
    Private Const MENSAJE_PANTALLA = "Y07"                 '--- TIPO CUENTA CTLS 
    Private Const SINCRONIZAR = "S00"
    Private Const COMENZARSINCRONIZACION = "S01"
    Private Const CONFIRMARSINCRONIZACION = "S02"
    Private Const CONSULTAPROTOCOLO = "S03"
    Private Const _ERROR As Integer = 2
    Private Const _PINCORRECTO As Integer = 1
    Private Const _REINTENTAR As Integer = 0

    'Private Const TIEMPOESPERA As Integer = 45

#End Region

#Region "Eventos"
    ''' <summary>
    ''' Este evento devuelve la cadena de respuesta completa.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <remarks></remarks>
    Public Event DatosRecibidos(ByVal x As String)
    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando Y00.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <remarks></remarks>
    Public Event Y00Recibido(ByVal x As String)
    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando Y01.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <remarks></remarks>
    Public Event Y01Recibido(ByVal x As String)
    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando Y01.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <remarks></remarks>
    Public Event Y41Recibido(ByVal x As String)
    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando Y02.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <remarks></remarks>
    Public Event Y02Recibido(ByVal x As String)
    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando Y03.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <remarks></remarks>
    Public Event Y03Recibido(ByVal x As String)
    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando Y04.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <remarks></remarks>
    Public Event Y04Recibido(ByVal x As String)
    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando Y21.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <remarks></remarks>
    Public Event Y21Recibido(ByVal x As String)


    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando Y31.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <remarks></remarks>
    Public Event Y31Recibido(ByVal x As String)

    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando Y03.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <remarks></remarks>
    Public Event Y07Recibido(ByVal x As String)


    ''' <summary>
    ''' Este evento se ejecuta cuando fue recibido un error Y0E desde el pinpad.
    ''' </summary>
    ''' <param name="comando">Comando que produjo el error.</param>
    ''' <param name="x">Descripción del error.</param>
    ''' <param name="nro">Nro del error.</param>
    ''' <param name="extendido">Descripción extendida.</param>
    ''' <remarks></remarks>
    Public Event Y0ERecibido(ByVal comando As String, ByVal x As String, ByVal nro As String, ByVal extendido As String)

    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando S03.
    ''' </summary>
    ''' <param name="x">Descripción del error.</param>
    ''' <remarks></remarks>
    Public Event S00Recibido(ByVal x As String)

    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando S03.
    ''' </summary>
    ''' <param name="x">Descripción del error.</param>
    ''' <remarks></remarks>
    Public Event S01Recibido(ByVal x As String)

    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando S03.
    ''' </summary>
    ''' <param name="x">Descripción del error.</param>
    ''' <remarks></remarks>
    Public Event S02Recibido(ByVal x As String)

    ''' <summary>
    ''' Este evento se produce cuando se recibe la respuesta del comando S03.
    ''' </summary>
    ''' <param name="x">Descripción del error.</param>
    ''' <remarks></remarks>
    Public Event S03Recibido(ByVal x As String)

    ''' <summary>
    ''' Este evento se produce cuando falla el control LRC del paquete recibido.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event ErrorTransmision()
    ''' <summary>
    ''' Este evento se produce cuando se ingresa el pin por el pinpad.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event PinRecibido()


#End Region

#Region "Propiedades"
    Dim _modoIngreso As String
    Dim _PAN As String
    Dim _codBanco As String
    Dim _codServicio As String
    Dim _nroRegistro As String
    Dim _vencimiento As String
    Dim _abreviatura As String
    Dim _aplicacionChip As String
    Dim _track1 As String
    Dim _track2 As String
    Dim _trackNoleido As Integer
    Dim _codSeguridad As String
    Dim _nroserie As String
    Dim _nroserielogico As String
    Dim _nombre As String
    Dim _tipocuenta As String
    Dim _pin As String
    Dim _criptograma As String
    Dim _autorizacion As String
    Dim _respEmisor As String
    Dim _secuenciaPan As String
    Dim _versionSO As String
    Dim _versionSoft As String
    Dim _versionKernel As String
    Dim _versionProtocolo As String

    Dim _versionKernelCTLS As String
    Dim _soportaCL As Integer
    Dim _soportaImp As Integer
    Dim _claveRSA As Integer
    Dim _pantallaTouch As Integer

    Dim _apn As String
    Dim _aid As String
    Dim _respuesta As String
    Dim _nth As String
    Dim _tipoencripcion As String
    Dim _paqencriptado As String


    Dim _CantHostMK As String
    Dim _NombreHost() As String
    Dim _posicionMK() As String
    Dim _IDEncripcion() As String
    Dim _IDEncripcionUnico As String
    'Dim _restoIDEncripcion As String

    Dim _comando_recibido As String
    Dim _codigo_error As Integer
    Dim _mensaje_extendido As String


    Dim _contador_paquete As String
    Dim _nombre_archivo As String

    Dim _respuesta_y07 As String              '--- TIPO CUENTA CTLS 

    Dim _cst As String
    Dim _solicitaFirma As String
    Dim _datosEmisor As String
    Private cadena_completa As String

    Public ReadOnly Property ConsultaCopia()
        Get
            Return IIf(_cst = "1", "Consulta por copia", "No Consulta por copia")
        End Get
    End Property
    Public ReadOnly Property get_cadena_completa()
        Get
            Return cadena_completa
        End Get
    End Property

    Public ReadOnly Property Archivo()
        Get
            Return _nombre_archivo
        End Get
    End Property




    Public ReadOnly Property SolicitaFirmaCliente()
        Get
            Return IIf(_solicitaFirma = "1", "Solicita firma Cliente en ticket", "NO Solicita firma Cliente en ticket")
        End Get
    End Property


    Public ReadOnly Property DatosdelEmisor()
        Get
            Return _datosEmisor
        End Get
    End Property



    Public ReadOnly Property CodigoRespuesta()
        Get
            Return _respuesta
        End Get
    End Property


    Public ReadOnly Property Respuesta()
        Get
            Return IIf(_respuesta = "00", "Aprobada", "Denegada")
        End Get
    End Property
    Public ReadOnly Property RespuestaSincronizacion()
        Get
            If _respuesta = "00" Then
                Return "Sincronizado OK"
            End If

            If _respuesta = "01" Then
                Return "Reintente Error"
            End If

            If _respuesta = "02" Then
                Return "Error WK. (Solicitar WK)"
            End If

            Return "Cod Respuesta desconocido"
        End Get
    End Property

    Public ReadOnly Property EstadoSincronizacion()
        Get
            If _respuesta = "00" Then
                Return "Sincronizado"
            End If

            If _respuesta = "02" Then
                Return "Debe Sincronizar"
            End If

            Return "Error: Devolvió " & _respuesta
        End Get
    End Property
    Public ReadOnly Property Nombre_aplicacion() As String
        Get
            Return _apn
        End Get
    End Property
    Public ReadOnly Property Id_aplicacion() As String
        Get
            Return _aid
        End Get
    End Property
    Public ReadOnly Property Nombre_THabiente() As String
        Get
            Return _nth
        End Get
    End Property
    Public ReadOnly Property ID_Encripcion_Unico() As String
        Get
            Return _IDEncripcionUnico
        End Get
    End Property

    Public ReadOnly Property VersionSO() As String
        Get
            Return _versionSO
        End Get
    End Property

    Public ReadOnly Property VersionSoft() As String
        Get
            Return _versionSoft
        End Get
    End Property

    Public ReadOnly Property Versionkernel() As String
        Get
            Return _versionKernel
        End Get
    End Property


    Public ReadOnly Property VersionKernelContactLess() As String
        Get
            Return _versionKernelCTLS
        End Get
    End Property


    Public ReadOnly Property SoportaContactLess() As String
        Get
            Return IIf(_soportaCL = 0, "No soporta", "Soporta")
        End Get
    End Property


    Public ReadOnly Property SoportaImpresion() As String
        Get
            Return IIf(_soportaImp = 0, "No soporta", "Soporta")
        End Get
    End Property

    Public ReadOnly Property RSAPresente() As String
        Get
            Return IIf(_claveRSA = 0, "No tiene", "Presente")
        End Get
    End Property

    Public ReadOnly Property PantallaTouch() As String
        Get
            Return IIf(_pantallaTouch = 0, "No tiene", "Tiene")
        End Get
    End Property

    Public ReadOnly Property VersionProtocolo() As String
        Get
            Return _versionProtocolo
        End Get
    End Property

    Public ReadOnly Property CantidadHostsConMK() As String
        Get
            Return _CantHostMK
        End Get
    End Property
    Public ReadOnly Property NombreDeHost(pos As Integer) As String
        Get
            Return _NombreHost(pos)
        End Get
    End Property
    Public ReadOnly Property PoscicionDeMK(pos As Integer) As String
        Get
            Return _posicionMK(pos)
        End Get
    End Property
    Public ReadOnly Property IDdeEncripcion(pos As Integer) As String
        Get
            Return _IDEncripcion(pos)
        End Get
    End Property


    Public ReadOnly Property ModoIngreso() As String
        Get
            Return _modoIngreso
        End Get
    End Property

    Public ReadOnly Property PAN() As String
        Get
            Return _PAN
        End Get
    End Property

    Public ReadOnly Property CodBanco() As String
        Get
            Return _codBanco
        End Get
    End Property

    Public ReadOnly Property CodServicio() As String
        Get
            Return _codServicio
        End Get
    End Property

    Public ReadOnly Property NroRegistro() As String
        Get
            Return _nroRegistro
        End Get
    End Property

    Public ReadOnly Property Abreviatura() As String
        Get
            Return _abreviatura
        End Get
    End Property

    Public ReadOnly Property NombreAplicacionChip() As String
        Get
            Return _aplicacionChip
        End Get
    End Property

    Public ReadOnly Property CodSeguridad()
        Get
            Return _codSeguridad
        End Get
    End Property

    Public ReadOnly Property TipoCuenta() As String
        Get
            Return _tipocuenta
        End Get
    End Property


    Public ReadOnly Property SeriePinPad() As String
        Get
            Return _nroserie
        End Get
    End Property

    Public ReadOnly Property SerieLogicoPinPad() As String
        Get
            Return _nroserielogico
        End Get
    End Property

    Public ReadOnly Property Vencimiento() As String
        Get
            Return _vencimiento
        End Get
    End Property

    Public ReadOnly Property Track1() As String
        Get
            Return _track1
        End Get
    End Property
    Public ReadOnly Property Track2() As String
        Get
            Return _track2
        End Get
    End Property

    Public ReadOnly Property TrackNoLeido() As String
        Get
            Return IIf(_trackNoleido = 1, "No Leido", "Leido")
        End Get
    End Property

    Public ReadOnly Property getPinEncript() As String
        Get
            Return _pin
        End Get
    End Property
    Public ReadOnly Property Criptograma() As String
        Get
            Return _criptograma
        End Get
    End Property

    Public ReadOnly Property Autorizacion() As String
        Get
            Return _autorizacion
        End Get
    End Property
    Public ReadOnly Property RespEmisor() As String
        Get
            Return _respEmisor
        End Get
    End Property
    Public ReadOnly Property SecuenciaPan() As String
        Get
            Return _secuenciaPan
        End Get
    End Property
    Public ReadOnly Property Encripcion As String
        Get
            Return _tipoencripcion
        End Get
    End Property
    Public ReadOnly Property PaqueteEncriptado As String
        Get
            Return _paqencriptado
        End Get
    End Property



#End Region
    Public Shared tr2 As New TextWriterTraceListener(System.IO.File.CreateText("Output.txt"))

    Public Sub New()

        If Not Debug.Listeners.Count > 1 Then
            Debug.Listeners.Add(tr2)
        End If

    End Sub

#Region "Inicializacion"

    Private Sub Informar(dato As String)
        Debug.WriteLine(dato)
        Debug.Flush()
        RaiseEvent DatosRecibidos(dato)
    End Sub

    ''' <summary>
    ''' Devuelve la representación ascii de la cadena enviada al PP.
    ''' </summary>
    ''' <param name="prefix"></param>
    ''' <param name="data"></param>
    ''' <param name="offset"></param>
    ''' <param name="len"></param>
    ''' <returns></returns>
    Public Function GetPrintableBuffer(ByVal prefix As String, ByVal data() As Byte, ByVal offset As Integer, ByVal len As Integer) As String
        Try
            If data Is Nothing OrElse data.Length = 0 OrElse len = 0 OrElse offset + len > data.Length Then
                Return String.Empty
            End If

            Dim s As New StringBuilder(len * 4)
            Dim c As Byte
            Dim cc As Char
            Dim i, j As Integer
            Dim charsPerLine As Integer = 20

            If Not String.IsNullOrEmpty(Trim(prefix)) Then
                s.Append(prefix)
                s.Append(Environment.NewLine)
            End If

            s.Append("     1 |")

            For i = 0 To len - 1
                If i > 0 And (i Mod charsPerLine) = 0 Then
                    s.Append("| ")

                    j = i - charsPerLine
                    add_hex_code(j, i, s, data, offset)

                    s.Append(String.Format("{0,6} |", i + 1))

                End If

                c = data(i + offset)
                cc = Convert.ToChar(c)

                If cc <> "\t" AndAlso (Char.IsLetterOrDigit(cc) OrElse Char.IsPunctuation(cc) OrElse Char.IsSeparator(cc) OrElse Char.IsSymbol(cc) OrElse Char.IsWhiteSpace(cc)) Then

                    s.Append(cc)

                Else
                    Select Case cc
                        Case STX
                            s.Append("<STX>")
                        Case ETX
                            s.Append("<ETX>")
                        Case FS
                            s.Append("<FS>")
                        Case EOT
                            s.Append("<EOT>")
                        Case ACK
                            s.Append("<ACK>")
                        Case NAK
                            s.Append("<NAK>")
                        Case ESP
                            s.Append("<b>")
                        Case DLE
                            s.Append("<DLE>")
                        Case Else
                            s.Append(".")
                    End Select
                End If
            Next

            s.Append("|")

            j = 0
            If i Mod charsPerLine <> 0 Then
                j = charsPerLine - (i Mod charsPerLine)
            End If

            While j > 0
                s.Append(" ")
                j -= 1
            End While

            s.Append(" ")

            j = i - charsPerLine

            If i Mod charsPerLine <> 0 Then
                j = i - (i Mod charsPerLine)
            End If

            add_hex_code(j, i, s, data, offset)

            Return s.ToString()
        Catch ex As Exception
            Return "No se puede mostrar"
        End Try
    End Function

    Private Sub add_hex_code(j As Integer, i As Integer, ByRef s As StringBuilder, data As Byte(), offset As Integer)
        Dim c As Byte

        For j = j To i - 1
            c = data(j + offset)
            s.Append(toHex(c >> 4 And &HF))

            s.Append(toHex(c And &HF))

            If (j + 1) < i Then
                s.Append(" ")
            End If
        Next

        s.Append(Environment.NewLine)
    End Sub

    Private Function toHex(c As Byte) As String
        Return IIf(c < 10, c.ToString(), Hex(c))
    End Function

    Public Sub GetPrintableBuffer(texto As String, cadena As String)
        Dim encoding As New System.Text.ASCIIEncoding
        Debug.Write(Now.TimeOfDay.ToString & " " & GetPrintableBuffer(texto, encoding.GetBytes(cadena), 0, cadena.Length))
        Debug.Flush()
    End Sub

    Private Function Paquete(ByVal comando As String) As String
        Dim PackCompleto As String
        '-----------------------------------------------------------------------------------------------------------
        '--- TERMINO DE FORMAR EL PAQUETE PARA SINCRONIZAR PINPAD (PAG 51) o  REALIZAR UNA COMPRA (PAG 41), etc     
        '--- LE AGREGO AL PRINCIPIO STX Y AL FINAL ETX Y DESPUES HALLO EL LRC Y LO AGREGO AL FINAL DE TODO          
        '-----------------------------------------------------------------------------------------------------------
        comando = PreprocesarDLEIda(comando)

        Dim lrcChar As Char = Chr(Lrc(comando + ETX))

        PackCompleto = STX + comando + ETX + lrcChar

        Informar("STX" + comando + "ETX" + lrcChar + "     ---> |")

        GetPrintableBuffer(String.Format("Enviado a PP: ({0} {1})", PackCompleto.Length, If(PackCompleto.Length = 1, "byte", "bytes")), PackCompleto)  '--- MUESTRA ABAJO LOS DATOS
        Return PackCompleto
    End Function

    Public Function Abrir_puerto() As Boolean
        Try
            If puerto.IsOpen Then Cerrar_Puerto()
            puerto.Open()
            If puerto.IsOpen Then puerto.DiscardInBuffer()
        Catch ex As Exception
            RaiseEvent Y0ERecibido("Abriendo puerto", "No se pudo abrir puerto", Errores.Error_puerto, "")
        End Try
        Return puerto.IsOpen
    End Function

    Public Function Cerrar_Puerto() As Boolean
        Dim estado As Boolean = False
        Try
            If puerto.IsOpen Then
                puerto.DiscardInBuffer()
                puerto.DiscardOutBuffer()
            End If
            If puerto.IsOpen Then puerto.Close()
            estado = puerto.IsOpen
        Catch ex As Exception
            RaiseEvent Y0ERecibido("Abriendo puerto", "No se pudo abrir puerto", Errores.Error_puerto, "")
        Finally
            puerto.Dispose()
        End Try
        Return estado
    End Function

    Private Sub Limpiar_Variables()
        _codBanco = 0
        _codServicio = 0
        _modoIngreso = ""
        _nroRegistro = 0

        _PAN = ""
        _vencimiento = ""
        _track1 = ""
        _track2 = ""
        _nombre = ""
        _pin = ""
        _criptograma = ""
        _autorizacion = ""
        _respEmisor = ""
        _secuenciaPan = ""
        _versionSoft = ""

        _contador_paquete = ""
        _nombre_archivo = ""

        _respuesta_y07 = ""            '--- TIPO CUENTA CTLS 
    End Sub

#End Region

    Private Sub mens() Handles tm2.Elapsed
        Informar("Timer Elapsed")
        Cerrar_Puerto()
        Abrir_puerto()
    End Sub


    Private Sub Envio_datos(paquete As String)
        Dim hora As DateTime
        Try
            Dim last_nak = -1 ' Para que el primer bucle ejecute el envio
            nro_nak_recibidos = 0
            nro_nak_enviados = 0
            leyendo_respuesta = False

            If Not puerto.IsOpen Then
                Informar("Puerto cerrado")
                RaiseEvent Y0ERecibido(paquete, "Puerto cerrado", Errores.Error_puerto, "")
                Return
            End If

            puerto.DiscardInBuffer()
            puerto.DiscardOutBuffer()

            hora = Now
            esperar = True

            Do While esperar AndAlso nro_nak_recibidos < 4 AndAlso Now.Subtract(hora).TotalSeconds < 45
                If leyendo_respuesta OrElse nro_nak_recibidos = last_nak OrElse Not esperar Then
                    Continue Do
                End If

                tm2.Start()

                Informar("Envío intento: " & nro_nak_recibidos)

                puerto.Write(paquete)    '--- Aca hace reintento por los naks

                tm2.Stop()
                last_nak = nro_nak_recibidos

            Loop

            If nro_nak_recibidos = 4 Then
                Informar("Nro de reintentos de envío superado")
                RaiseEvent Y0ERecibido(paquete, "Nro de reintentos de envío superado", Errores.Reintentos_superados, "")
                Return
            ElseIf esperar Then
                Informar("El pinpad demoro mas de 30 segundos en mandar una respuesta satisfactoria")
                RaiseEvent Y0ERecibido(paquete, "El pinpad demoro mas de 30 segundos en mandar una respuesta satisfactoria", Errores.TimeOut, "")
                Return
            End If

        Catch ex1 As TimeoutException
            tm2.Stop()
            RaiseEvent Y0ERecibido(paquete, "TIMEOUT DE ESCRITURA - REINICIE EL PINPAD", Errores.Error_desconocido, "")
            mens()
        Catch ex As Exception
            tm2.Stop()
            RaiseEvent Y0ERecibido(paquete, "ERROR DESCONOCIDO - REINICIE EL PINPAD", Errores.Error_desconocido, "")
            mens()
        End Try

    End Sub

#Region "Comando Y00"

    Public Sub Inicializar_PinPad()
        Envio_datos(Paquete(INICIALIZAR & "V0211101100000N"))
    End Sub
#End Region

#Region "Comando Y01"

    ''' <summary>
    ''' Solicita tarjeta de compra para dar inicio a la transacción.
    ''' </summary>
    ''' <param name="tipo"></param>
    ''' <param name="sincronizarFechaHora">True si sincroniza Fecha y Hora actual del equipo en donde se encuentra conectado el PP</param>
    Public Sub Pedir_Tarjeta(ByVal tipo As Tipos_transacciones, ByVal sincronizarFechaHora As Boolean)
        Dim cr As Integer = tipo
        If sincronizarFechaHora Then
            Envio_datos(Paquete(LEER_TARJETA & "V02030" & cr.ToString("00") & "00604" & Now.Date.ToString("YYYYMMDD") & FS & Now.Date.ToString("HHmmSS")))
            Return
        End If

        Envio_datos(Paquete(LEER_TARJETA & "V02030" & cr.ToString("00") & "00604" & FS))

    End Sub


#End Region

#Region "Comando Y02"

    Dim encripta As Boolean = False

    ''' <summary>
    ''' Solicita los datos adicionales segun la tarjeta. Sin timeout.
    ''' </summary>
    ''' <param name="pU4D">Solicita Ultimos 4 digitos</param>
    ''' <param name="pCDS">Solicita Codigo de seguridad</param>
    ''' <param name="pSPI">Solicita PIN</param>
    ''' <param name="pWKD">Working Key de datos o N si no encripta.</param>
    ''' <param name="pWKP">Working Key de pines o N si no envía no pide PIN.</param>
    ''' <param name="posicionMK">Posición de la MK de 0 a 9 o N si no encripta.</param>
    ''' <param name="pIMP">Importe, los ultimos 2 son los decimales, pasar sin coma y las 12 posiciones</param>
    ''' <param name="pICB">Importe Cashback, los ultimos 2 son los decimales, pasar sin coma y las 12 posiciones</param>
    ''' <param name="pServCod">Service code: si termina en 0, 3, 5, 6 ó 7 tiene que solicitar pin </param>
    Public Sub Solicitar_Datos_Adicionales_FD(ByVal pU4D As Boolean, ByVal pCDS As Boolean, ByVal pSPI As Boolean, ByVal pWKD As String, ByVal pWKP As String, posicionMK As String, ByVal pIMP As Decimal, ByVal pICB As Decimal, ByVal pServCod As Integer)
        Try
            Dim valores_piden_pin As String() = {"0", "3", "5", "6", "7"}
            Dim comando As String = ""

            '--- PAGINA 18 "Y02"   
            encripta = True

            comando = DATOS_ADICIONALES
            comando += "V02"
            comando += Convert.ToByte(pU4D).ToString
            comando += Convert.ToByte(pCDS).ToString
            comando += "1"        '---  transmite track 1 siempre

            '--- SERVICE CODE:  LO TRAE DEL TRACK 2 (SON LOS 3 DIGITOS QUE ESTAN DESPUES DE LA FECHA DE VENCIMIENTO) 
            '--- CUANDO EL TERCER DIGITO DE ESOS 3 DIGITOS ES UN 0, 3, 5, 6 ó 7 TIENE QUE SOLICITAR PIN              

            If Array.IndexOf(valores_piden_pin, Right(pServCod.ToString, 1)) <> -1 Then
                comando += Convert.ToByte(True).ToString
            Else
                comando += Convert.ToByte(pSPI).ToString
            End If

            If pSPI Then
                comando += "1"        '--- pide tipo de cuenta si solicita pin. (caja ahorro, cuenta corriente)  
            Else
                comando += "0"
            End If

            comando += posicionMK
            comando += "2"
            comando += pWKP
            comando += FS
            comando += pWKD
            comando += FS
            comando += "2"
            comando += (pIMP * 100).ToString("000000000000")
            comando += (pICB * 100).ToString("000000000000")

            Envio_datos(Paquete(comando))

        Catch ex As Exception
            RaiseEvent Y0ERecibido("Y02", "Error desconocido en Y02", Errores.Error_desconocido, "")
        End Try

    End Sub

#End Region

#Region "Comando Y03"
    ''' <summary>
    ''' Sin Timout
    ''' </summary>
    ''' <param name="pAut"></param>
    ''' <param name="pRespEmi"></param>
    ''' <param name="pRecibe"></param>
    ''' <param name="pCripto"></param>
    Public Sub Datos_Respuesta_Emisor(ByVal pAut As String, ByVal pRespEmi As String, ByVal pRecibe As Boolean, ByVal pCripto As String)
        Dim recibe As String
        Try
            recibe = "0"

            If pRecibe Then
                recibe = "1"
            End If

            If pAut = "" Then
                pAut = "      "
            End If

            If pRespEmi = "" Then
                pRespEmi = "  "
            End If

            Envio_datos(Paquete(RESPUESTA_EMISOR + "V02" + pAut + pRespEmi + recibe + pCripto.Trim))
        Catch ex As Exception
            RaiseEvent Y0ERecibido("Y03", "Respuesta Y03", Errores.Error_desconocido, "")
        End Try
    End Sub

#End Region

#Region "Comando Y04"
    Public Sub Reingresar_pin(posicion As String, ByVal pWkey As String, tarjeta As String)
        Envio_datos(Paquete(REINGRESO_PIN & "V02" & "1" & posicion & pWkey & FS & tarjeta))
    End Sub
#End Region

#Region "Comando Y06"

    Public Sub Cancelar()
        Try
            Envio_datos(Paquete(CANCELAR_COMANDO & "V02000"))
        Catch ex As Exception
            RaiseEvent Y0ERecibido("Y06", "Error cancelando comando", Errores.Error_desconocido, "")
            Informar("Error en cancelar comando")
        End Try
    End Sub
#End Region

#Region "Comando Y21"
    ''' <summary>
    ''' Envia petición al pinpad para ingreso de tarjeta Contactless sin esperar respuesta. 
    ''' Se usa con manejo del evento Y21Recibido.
    ''' </summary>
    Public Sub Pedir_Tarjeta_Compra_CL(ByVal tipo As Tipos_transacciones, ByVal sincronizarFechaHora As Boolean, importe As Decimal, importeCashback As Decimal, wp As String, wd As String)
        Dim imp As String = (importe * 100).ToString("000000000000")
        Dim impCB As String = (importeCashback * 100).ToString("000000000000")
        Dim fecha As String = ""
        Dim hora As String = ""

        If sincronizarFechaHora Then
            fecha = Now.Date.ToString("YYYYMMDD")
            hora = Now.Date.ToString("HHmmSS")
        End If

        Envio_datos(Paquete(LEER_TARJETA_CL & "V02030" & Int(tipo).ToString("00") & "00604" & fecha & FS & hora & FS & "0" & "2" & wp & FS & wd & FS & "2" & imp & impCB))
    End Sub

#End Region


#Region "Y31"
    Public Sub Actualizar(ByVal archivo As String, paq As Integer, tipoArchivo As String)
        Dim reinicia As String

        reinicia = 0

        If tipoArchivo = "APP" Or tipoArchivo = "SYS" Then
            reinicia = 1
        End If

        Envio_datos(Paquete(ACTUALIZACION_REMOTA & "V02" & paq.ToString("0000000") & reinicia & tipoArchivo & "00000" & archivo))
    End Sub
#End Region



#Region "Comando Y41"

    ''' <summary>
    ''' Solicita tarjeta de compra para dar inicio a la transacción.
    ''' </summary>
    ''' <param name="tipo"></param>
    ''' <param name="sincronizarFechaHora">True si sincroniza Fecha y Hora actual del equipo en donde se encuentra conectado el PP</param>
    Public Sub Pedir_Tarjeta_MultiModo(ByVal tipo As Tipos_transacciones, ByVal sincronizarFechaHora As Boolean, ByVal pWKD As String, ByVal pWKP As String, posicionMK As String, ByVal pIMP As Decimal, ByVal pICB As Decimal)
        Dim comando As String
        Dim cr As String = Int(tipo).ToString("00")

        Try
            '--- ACA HACE EL Y41 (EN MULTIMODO ES Y41) - PAG 41  

            comando = LEER_TARJETA_MULTIMODO & "V02030" & cr & "10000" & FS & FS

            If sincronizarFechaHora Then
                comando = LEER_TARJETA_MULTIMODO & "V02030" & cr & "10000" & Now.Date.ToString("yyyyMMdd") & FS & Now.ToString("HHmmss") & FS
            End If

            comando += posicionMK
            comando += "2"
            comando += pWKP & FS        '--- FS es separador  
            comando += pWKD & FS
            comando += "2"              '--- 2 = 3DES  
            comando += (pIMP * 100).ToString("000000000000")
            comando += (pICB * 100).ToString("000000000000")

            Envio_datos(Paquete(comando))

        Catch ex As Exception
            RaiseEvent Y0ERecibido("Y41", "Error desconocido en Y41", Errores.Error_desconocido, "")
        End Try

    End Sub



#End Region




#Region "Comando Y07"
    ''' <summary>
    ''' Envia petición al pinpad para mostrar en pantalla Tipo de Cuenta (1=Caja Ahorro $, 2=Cuenta Corriente $, etc)
    ''' ya que con tarjeta Contactless no se puede hacer el Y02 como el chip y banda. 
    ''' </summary>
    Public Sub Mostrar_Pantalla_TipoCuenta_CL()
        '--- TIPO CUENTA CTLS 
        Dim comando As String
        Try
            comando = MENSAJE_PANTALLA & "V02030"
            comando += FS
            comando += "4"
            comando += FS
            comando += "1 Caja Ahorro $"
            comando += FS
            comando += "2 Cta Cte en $"
            comando += FS
            comando += "3 C. Ahorro U$S"
            comando += FS
            comando += "4 Cta Cte U$S"

            Envio_datos(Paquete(comando))

        Catch ex As Exception
            RaiseEvent Y0ERecibido("Y07", "Error desconocido en Y07", Errores.Error_desconocido, "")
        End Try
    End Sub

#End Region



#Region "Sincronizacion"
    ''' <summary>
    ''' Verifica si se debe sincronizar el PinPad.
    ''' </summary>
    ''' <param name="pTerminal"></param>
    ''' <param name="pSerieFisico"></param>
    Public Sub Sincronizar_PinPad(pTerminal As String, pSerieFisico As String)
        '--- FORMO PAQUETE PARA SINCRONIZAR PINPAD - PAG 51 de 73  
        Envio_datos(Paquete(SINCRONIZAR & "V02" & pTerminal & pSerieFisico))
    End Sub

    Public Sub Comenzar_Sincronizacion_PinPad(pTerminal As String)
        Envio_datos(Paquete(COMENZARSINCRONIZACION & "V02" & pTerminal))
    End Sub


    Public Lista_Claves As New Dictionary(Of String, Pares_claves)
    Public Sub Confirmar_Sincronizacion_Pinpad(pTerminal As String)
        Dim s As String
        Dim lis As String = ""
        Dim pos As String = ""
        Dim repe As String = ""
        Dim canthost As Integer = 0
        Dim cantWK As Integer = 0
        Dim nombre As String = ""

        '--- ENVIO DEL S02 
        s = CONFIRMARSINCRONIZACION & "V02" & pTerminal
        For Each clave In Lista_Claves
            If nombre = "" Then
                nombre = clave.Key.Substring(0, clave.Key.Length - 5)
                canthost = 1
                pos = clave.Value.posicion

            ElseIf nombre <> clave.Key.Substring(0, clave.Key.Length - 5) Then
                canthost += 1
                lis = pos & cantWK & "2" & repe
                pos = clave.Value.posicion
                cantWK = 0
                nombre = clave.Key.Substring(0, clave.Key.Length - 5)
                repe = ""
            End If
            cantWK += 1
            repe += clave.Value.tipo & clave.Value.clave & FS & clave.Value.control & FS
        Next
        s += canthost & lis & pos & cantWK & 2 & repe

        Envio_datos(Paquete(s))

    End Sub

    Public Sub Consultar_Protocolo()
        Envio_datos(Paquete(CONSULTAPROTOCOLO))
    End Sub

#End Region

#Region "Respuestas"

    Dim esperar As Boolean
    Dim leyendo_respuesta As Boolean
    Dim cadenaComando As String = ""

    Private Sub Recuperar_Respuesta(ByVal sender As Object, ByVal e As IO.Ports.SerialDataReceivedEventArgs) Handles puerto.DataReceived
        Dim cadena As String = ""
        Dim inicio As DateTime = Now
        leyendo_respuesta = True
        Try
            While cadena = "" AndAlso Now.Subtract(inicio).TotalSeconds < 2
                cadena = puerto.ReadExisting()
            End While

            While cadena.Contains(STX) AndAlso Not cadena.Contains(ETX)
                cadena += puerto.ReadExisting()
            End While

            While cadena.EndsWith(ETX) AndAlso Not cadena.EndsWith(ETX + ETX)
                Try
                    cadena += Chr(puerto.ReadChar()).ToString()
                Catch ex As Exception
                    Informar(ex.Message)
                End Try
            End While

            GetPrintableBuffer(String.Format("Recibido desde PP: ({0} {1})", cadena.Length, If(cadena.Length = 1, "byte", "bytes")), cadena)  '--- MUESTRA EN PANTALLA SE PUEDE BORRAR
        Catch ex As Exception
            Informar(ex.Message)
        End Try

        analizar_cadena(cadena)
        leyendo_respuesta = False

    End Sub

    Private Sub analizar_cadena(cadena As String)
        Dim tiene_eot As Boolean
        Try

            If cadena = "" Then
                esperar = True
                Return
            End If

            If cadena = ACK Then
                esperar = False
                Informar("                      | <--- ACK")
                Return
            End If

            If cadena = NAK Then
                cadenaComando = ""
                nro_nak_recibidos += 1
                Informar("                      | <--- NAK")
                Return
            End If

            If cadena = EOT Then
                esperar = False
                Informar("                      | <--- EOT")
                If cadenaComando <> "" Then

                    '--------------------------------------------------------------------------------------------------------
                    '--- ACA DESEMPAQUETA LO QUE RESPONDE, YA SEA S00, S01, ES LA RESPUESTA DEL PINPAD (LA CADENA COMPLETA)  
                    '--------------------------------------------------------------------------------------------------------

                    Leer_Mensaje(cadenaComando)
                End If

                Return
            End If

            '--- ENTRA POR ACA POR LA CADENA COMPLETA Y VUELVE A ENVIAR EL ACK, NAK o EOT  

            If cadena.StartsWith(ACK.ToString()) Then
                cadena = cadena.Substring(1)
            End If

            If cadena.EndsWith(EOT.ToString()) AndAlso Not cadena.EndsWith(ETX + EOT) Then
                cadena = cadena.Substring(0, cadena.Length - 1)
                tiene_eot = True
            End If

            Dim control As String = ""
            Try
                control = Chr(Lrc(cadena.Substring(1, cadena.Length - 2)))      '--- calculo lrc para comparar con el lrc que vino.
            Catch
                Informar("NO SE PUDO OBTENER LRC")
            End Try

            If control = cadena.Substring(cadena.Length - 1) Then

                Informar("                      | <--- " & cadena)

                cadenaComando = cadena
                If tiene_eot AndAlso cadenaComando <> "" Then

                    '--------------------------------------------------------------------------------------------------------
                    '--- ACA DESEMPAQUETA LO QUE RESPONDE, YA SEA S00, S01, ES LA RESPUESTA DEL PINPAD (LA CADENA COMPLETA)  
                    '--------------------------------------------------------------------------------------------------------

                    Leer_Mensaje(cadenaComando)
                Else
                    Enviar_ACK()                  '--- si el LRC que viene es correcto, mando ACK, quiere decir que termino todo bien  
                End If
                esperar = False
                Return
            End If

            Informar("ERROR DE TRANSMISION. Cadena recibida: " + cadena)
            nro_nak_enviados += 1
            If nro_nak_enviados = 4 Then
                Enviar_EOT()      '--- si el LRC no es correcto, lo vuelvo a leer hasta 3 veces y luego mando un EOT  
            Else
                Enviar_NAK()      '--- si el LRC no es correcto, mando NAK, quiere decir que no termino todo bien  
            End If

            esperar = False

        Catch ex As Exception
            Informar(ex.Message)
            If cadena <> "" And Not puerto.IsOpen Then
                Informar("Reintenta analizar cadena")
                Abrir_puerto()
                analizar_cadena(cadena)
            End If
        End Try
    End Sub

    Private Function PreprocesarDLEIda(s As String) As String
        Dim cadena As String = ""
        For i = 0 To s.Length - 1

            If s.Substring(i, 1) = ETX Or s.Substring(i, 1) = DLE Then
                cadena += DLE
            End If

            cadena += s.Substring(i, 1)
        Next

        Return cadena
    End Function

    Private Function check_codServicio(cod_servicio As String, app_code As String) As String

        If Not String.IsNullOrEmpty(Trim(cod_servicio)) Then
            Return cod_servicio
        End If

        cod_servicio = "111"

        If UCase(app_code) = "VISA DEBITO" Then
            cod_servicio = "121"
        ElseIf UCase(app_code) = "DEBIT MASTERCARD" OrElse UCase(app_code) = "MASTERCARD DEBIT" Then
            cod_servicio = "221"
        End If

        Return cod_servicio
    End Function

    Private Sub Leer_Mensaje(pCad As String)

        '---------------------------------------------------------------
        '--- ACA ESTAN TODAS LAS RESPUESTAS DEL PINPAD                  
        '--- ACA DESARMA TODOS LOS PAQUETES QUE VIENE DEL PINPAD        
        '---------------------------------------------------------------

        Dim CID As String
        Dim cad As String = pCad.Substring(1, pCad.Length - 3)
        cadena_completa = cad

        Try
            If pCad.Substring(0, 1) = STX Then
                pCad = pCad.Remove(0, 1)     '--- SACO EL STX 
            End If

            CID = pCad.Substring(0, 3)      '--- LEO EL ID DE COMANDO
            pCad = pCad.Remove(0, 3)        '--- ACA SACO LOS PRIMEROS 3 CARACTERES, Y41, S00, Y01, ETc

            Dim pal() As String = pCad.Split(FS)              '---  separa en el SEPARADOR (hoja 52) 

            If CID = "Y01" Then

                Limpiar_Variables()
                _modoIngreso = pal(0).Substring(0, 1)      '--- MODO INGRESO  
                _PAN = pal(0).Substring(1)                 '--- PAN  

                '--- SERVICE CODE:  LO TRAE DEL TRACK 2 (SON LOS 3 DIGITOS QUE ESTAN DESPUES DE LA FECHA DE VENCIMIENTO) 
                '--- CUANDO EL TERCER DIGITO DE ESOS 3 DIGITOS ES UN 0, 3, 5, 6 ó 7 TIENE QUE SOLICITAR PIN              
                _codServicio = pal(1).Substring(0, 3)


                _codBanco = pal(1).Substring(3, 3)
                _nroRegistro = pal(1).Substring(6, 6)
                _abreviatura = pal(1).Substring(12, 2)

                _aplicacionChip = ""
                If pal(1).Substring(14, 1) <> ETX Then
                    _aplicacionChip = pal(1).Substring(14, (pal(1).Length - 2) - 14)
                End If

                '--- SERVICE CODE:  LO TRAE DEL TRACK 2 (SON LOS 3 DIGITOS QUE ESTAN DESPUES DE LA FECHA DE VENCIMIENTO) 
                '--- CUANDO EL TERCER DIGITO DE ESOS 3 DIGITOS ES UN 0, 3, 5, 6 ó 7 TIENE QUE SOLICITAR PIN              
                '--- CUANDO EL PRIMER DIGITO ES UN 2 ES VISA DEBITO                                                      

                ' TODO: Por que esto es diferente a los otros?? Se puede reemplazar por la nueva funcion check_codServicio???

                If String.IsNullOrEmpty(Trim(_codServicio)) Then
                    _codServicio = "111"

                    If _aplicacionChip = "VISA DEBITO" Then
                        _codServicio = "211"
                    End If
                End If


                RaiseEvent Y01Recibido(cad)

            ElseIf CID = "Y02" Then

                _modoIngreso = pal(0).Substring(0, 1)      '--- MODO INGRESO   
                _PAN = pal(0).Substring(1)
                _nroserie = pal(1)

                Select Case _modoIngreso
                    Case "M"
                        If encripta Then

                            _IDEncripcionUnico = pal(2).Substring(0, 16)
                            _tipocuenta = pal(2).Substring(16, 1)
                            _tipoencripcion = pal(2).Substring(17, 1)
                            _paqencriptado = pal(2).Substring(18)

                        Else
                            _nroserie = pal(2)
                            _vencimiento = pal(1).Substring(0, 4)
                            _codSeguridad = pal(1).Substring(4)
                            _tipocuenta = pal(3).Substring(0, 1)

                        End If

                        _pin = pal(3).Substring(1, pal(3).Length - 2)

                        If _pin = ETX Then
                            _pin = ""
                        End If

                    Case "C"
                        '------------   CON ENCRIPCION  ---------------

                        _IDEncripcionUnico = pal(2).Substring(0, 16)
                        _tipoencripcion = pal(2).Substring(16, 1)
                        _paqencriptado = pal(2).Substring(17)
                        _criptograma = pal(3)

                        _autorizacion = pal(4).Substring(0, 6)
                        _respEmisor = pal(4).Substring(6, 2)
                        _secuenciaPan = pal(4).Substring(8, 3)
                        _tipocuenta = pal(4).Substring(11, 1)
                        _pin = pal(4).Substring(12)
                        _apn = pal(5)
                        _aid = pal(6)

                        _nth = pal(7).Substring(1, pal(7).Length - 2)

                        If _nth = ETX Then
                            _nth = ""
                        End If

                    Case "B"
                        If pal.Length = 7 Then       '--- no encripta  

                            _nroserie = pal(4)
                            _vencimiento = pal(1).Substring(0, 4)
                            _track1 = pal(1).Substring(3)
                            _track2 = pal(2).Substring(0)
                            _trackNoleido = pal(3).Substring(0, 1)
                            _codSeguridad = pal(3).Substring(1)
                            _tipocuenta = pal(5).Substring(0, 1)
                            _pin = pal(5).Substring(1)

                            _nth = pal(6).Substring(0, pal(6).Length - 2)

                        Else                    '--- encripta   
                            _IDEncripcionUnico = pal(2).Substring(0, 16)
                            _tipocuenta = pal(2).Substring(16, 1)
                            _tipoencripcion = pal(2).Substring(17, 1)
                            _paqencriptado = pal(2).Substring(18)
                            _pin = pal(3)

                            _nth = pal(4).Substring(0, pal(4).Length - 2)

                        End If

                        If _nth = ETX Then
                            _nth = ""
                        End If

                End Select

                RaiseEvent Y02Recibido(cad)

            ElseIf CID = "Y41" Then

                _modoIngreso = pal(0).Substring(0, 1)
                _PAN = pal(0).Substring(1)
                _codBanco = pal(1).Substring(3, 3)
                _codServicio = pal(1).Substring(0, 3)
                _nroRegistro = pal(1).Substring(6, 6)

                Select Case _modoIngreso
                    Case "M", "C", "B"

                        '--- FINANYA (primeros 6 digitos 639518) pone _codServicio = "000" y pede pin, y no tene que pedir, fuerzo ""   
                        Try
                            If _PAN.Substring(0, 6) = "639518" Then
                                _codServicio = ""
                            End If
                        Catch
                        End Try

                        _abreviatura = pal(1).Substring(12, 2)

                        _aplicacionChip = pal(1).Substring(14, (pal(1).Length - 2) - 14)

                        If _aplicacionChip = ETX Then
                            _aplicacionChip = "" '--- SIN CHIP  
                        End If

                        '--- SERVICE CODE:  LO TRAE DEL TRACK 2 (SON LOS 3 DIGITOS QUE ESTAN DESPUES DE LA FECHA DE VENCIMIENTO) 
                        '--- CUANDO EL TERCER DIGITO DE ESOS 3 DIGITOS ES UN 0, 3, 5, 6 ó 7 TIENE QUE SOLICITAR PIN              
                        '--- CUANDO EL PRIMER DIGITO ES UN 2 ES VISA DEBITO                                                      

                        _codServicio = check_codServicio(_codServicio, _aplicacionChip)

                        RaiseEvent Y01Recibido(cad)      '--- DISPARA EL EVENTO    *** Private Sub ControlMensajeY01() Handles pp.Y01Recibido  

                    Case "L"
                        '------------   CON ENCRIPCION  ---------------

                        _nroserie = pal(1).Substring(12)   '--- A PARTIR DE LA POSICION 12 HASTA EL FINAL
                        _IDEncripcionUnico = pal(2).Substring(0, 16)
                        _tipoencripcion = pal(2).Substring(16, 1)
                        _paqencriptado = pal(2).Substring(17)      '--- A PARTIR DE LA POSICION 17 HASTA EL FINAL
                        _criptograma = pal(3)
                        _autorizacion = pal(4).Substring(0, 6)
                        _respEmisor = pal(4).Substring(6, 2)
                        _secuenciaPan = pal(4).Substring(8, 3)
                        _tipocuenta = pal(4).Substring(11, 1)
                        _pin = pal(4).Substring(12)
                        _apn = pal(5)
                        _aid = pal(6)
                        _nth = pal(7)
                        _cst = pal(8).Substring(0, 1)
                        _solicitaFirma = pal(8).Substring(1, 1)

                        _datosEmisor = pal(8).Substring(2, pal(8).Length - 4)

                        If _datosEmisor = ETX Then
                            _datosEmisor = ""
                        End If

                        '--- SERVICE CODE:  LO TRAE DEL TRACK 2 (SON LOS 3 DIGITOS QUE ESTAN DESPUES DE LA FECHA DE VENCIMIENTO) 
                        '--- CUANDO EL TERCER DIGITO DE ESOS 3 DIGITOS ES UN 0, 3, 5, 6 ó 7 TIENE QUE SOLICITAR PIN              
                        '--- CUANDO EL PRIMER DIGITO ES UN 2 ES VISA DEBITO                                                      

                        _codServicio = check_codServicio(_codServicio, _apn)

                        RaiseEvent Y21Recibido(cad)

                End Select

            ElseIf CID = "Y03" Then
                _autorizacion = pCad.Substring(0, 6)
                _respuesta = pCad.Substring(6, 2)

                _criptograma = pCad.Substring(8, pCad.Length - 10)

                If _criptograma = ETX Then
                    _criptograma = ""
                End If

                RaiseEvent Y03Recibido(cad)

            ElseIf CID = "Y04" Then

                Limpiar_Variables()

                If pal(0).Length > 15 Then
                    _pin = pal(0).Substring(0, 16)
                End If

                RaiseEvent Y04Recibido(cad)

            ElseIf CID = "Y21" Then

                Limpiar_Variables()

                _modoIngreso = pal(0).Substring(0, 1) 'MODO INGRESO
                _PAN = pal(0).Substring(1) 'PAN
                _codServicio = pal(1).Substring(0, 3)
                _codBanco = pal(1).Substring(3, 3)
                _nroRegistro = pal(1).Substring(6, 6)
                _nroserie = pal(1).Substring(12)
                _IDEncripcionUnico = pal(2).Substring(0, 16)
                _tipoencripcion = pal(2).Substring(16, 1)
                _paqencriptado = pal(2).Substring(17)
                _criptograma = pal(3)
                _autorizacion = pal(4).Substring(0, 6)
                _respEmisor = pal(4).Substring(6, 2)
                _secuenciaPan = pal(4).Substring(8, 3)
                _tipocuenta = pal(4).Substring(11, 1)
                _pin = pal(4).Substring(12)
                _apn = pal(5)
                _aid = pal(6)
                _nth = pal(7)
                _cst = pal(8).Substring(0, 1)
                _solicitaFirma = pal(8).Substring(1, 1)

                _datosEmisor = pal(8).Substring(2, pal(8).Length - 4)

                If _datosEmisor = ETX Then
                    _datosEmisor = ""
                End If

                '--- SERVICE CODE:  LO TRAE DEL TRACK 2 (SON LOS 3 DIGITOS QUE ESTAN DESPUES DE LA FECHA DE VENCIMIENTO) 
                '--- CUANDO EL TERCER DIGITO DE ESOS 3 DIGITOS ES UN 0, 3, 5, 6 ó 7 TIENE QUE SOLICITAR PIN              
                '--- CUANDO EL PRIMER DIGITO ES UN 2 ES VISA DEBITO                                                      

                _codServicio = check_codServicio(_codServicio, _apn)

                RaiseEvent Y21Recibido(cad)

            ElseIf CID = "Y31" Then

                Limpiar_Variables()
                _contador_paquete = pal(0).Substring(0, 7)
                _nombre_archivo = pal(0).Substring(7, 15)

                RaiseEvent Y31Recibido(_nombre_archivo)


            ElseIf CID = "Y07" Then
                '--- TIPO CUENTA CTLS 

                _respuesta_y07 = pal(0).Substring(0, 1)

                RaiseEvent Y07Recibido(_respuesta_y07)


            ElseIf CID = "Y0E" Then             '--- ES ERROR EL Y0E (LO MANDA EL PINPAD CUANDO HAY UN ERROR)

                _comando_recibido = pal(0).Substring(0, 3)
                _respuesta = pal(0).Substring(3, 2)

                _mensaje_extendido = pal(0).Substring(5, (pal(0).Length - 2) - 5)

                If _mensaje_extendido = ETX Then
                    _mensaje_extendido = ""
                End If

                RaiseEvent Y0ERecibido(pCad.Substring(0, 3), CType(CInt(pCad.Substring(3, 2)), Errores).ToString, pCad.Substring(3, 2), _mensaje_extendido)

            ElseIf CID = "Y00" Then

                _versionSO = pal(0)
                _versionSoft = pal(1)
                _versionKernel = pal(2)
                _versionKernelCTLS = pal(3)
                _soportaCL = pal(4).Substring(0, 1)
                _soportaImp = pal(4).Substring(1, 1)
                _claveRSA = pal(4).Substring(2, 1)
                _pantallaTouch = pal(4).Substring(3, 1)

                _nroserie = pal(5).Substring(0, pal(5).Length - 2)

                If _nroserie = ETX Then
                    _nroserie = ""
                End If

                RaiseEvent Y00Recibido(pCad)

            ElseIf CID = "S00" Then
                _respuesta = pCad.Substring(0, 2)

                RaiseEvent S00Recibido(pCad)

            ElseIf CID = "S01" Then


                _CantHostMK = pal(0).Substring(0, 1)

                ReDim _NombreHost(_CantHostMK)                    '--- re define el array de acuerdo a los hosts que hay
                ReDim _posicionMK(_CantHostMK)
                ReDim _IDEncripcion(_CantHostMK)
                For x As Integer = 0 To _CantHostMK - 1

                    _NombreHost(x) = pal(x).Substring(IIf(x = 0, 1, 17))

                    _posicionMK(x) = pal(x + 1).Substring(0, 1)      '--- toma valores despues de SEPARADOR
                    _IDEncripcion(x) = pal(x + 1).Substring(1, 16)
                Next

                RaiseEvent S01Recibido(pCad)

            ElseIf CID = "S02" Then
                _respuesta = pCad.Substring(0, 2)
                _nroserie = pCad.Substring(2, pCad.Length - 4)
                RaiseEvent S02Recibido(pCad)

            ElseIf CID = "S03" Then
                _versionProtocolo = pCad.Substring(0, 3)
                RaiseEvent S03Recibido(pCad)

            End If

        Catch ex As Exception
            RaiseEvent Y0ERecibido("EEE", "Error leyendo stream", Errores.Error_desconocido, "")
            RaiseEvent DatosRecibidos("Error leyendo stream")
        End Try


    End Sub



    Private Sub Enviar_ACK()
        puerto.Write(ACK)
        'Envio_datos(ACK)
        Informar("ACK --->              |")
        GetPrintableBuffer("Enviado al PP: (1 byte)", ACK)
    End Sub
    Private Sub Enviar_NAK()
        puerto.Write(NAK)
        Informar("NAK --->              |")
        GetPrintableBuffer("Enviado al PP: (1 byte)", NAK)
    End Sub
    Private Sub Enviar_EOT()
        puerto.Write(EOT)
        Informar("EOT --->              |")
        GetPrintableBuffer("Enviado al PP: (1 byte)", EOT)
    End Sub
#End Region

#Region "Control de errores"

    ''' <summary>
    ''' Digito de comprobación de errores LRC.
    ''' </summary>
    ''' <param name="cad"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function Lrc(ByVal cad As String) As Integer
        Dim acu, letr As String
        Dim dig, f, sol As Single

        acu = Bin(Asc(Mid(cad, 1, 1)))
        dig = DXor(acu)

        For f = 2 To Len(cad)
            letr = Mid(cad, f, 1)
            dig += DXor(Bin(Asc(letr)))
            acu = BinXor(acu, Bin(Asc(letr)))
        Next

        sol = 0
        For f = 1 To 8
            If Mid(acu, f, 1) = "1" Then sol += 2 ^ (f - 1)
        Next

        Return sol
    End Function


    Private Function Bin(ByVal num As Integer) As String
        Dim resu As String
        Dim j As Single

        resu = ""
        For j = 1 To 8
            resu += LTrim(Str(num Mod 2))
            num = Fix(num / 2)
        Next

        Return resu
    End Function

    Private Function DXor(ByVal dat As String) As Integer
        Dim j, resd As Integer

        For j = 1 To 7
            resd += Val(Mid(dat, j, 1))
        Next

        Return IIf(resd Mod 2 = 1, 1, 0)
    End Function


    Private Function BinXor(ByVal bin1 As String, ByVal bin2 As String) As String
        Dim j As Integer
        Dim resu As String

        resu = ""
        For j = 1 To 8
            If Val(Mid(bin1, j, 1)) Xor Val(Mid(bin2, j, 1)) Then
                resu += "1"
            Else
                resu += "0"
            End If
        Next

        Return resu
    End Function

#End Region
End Class
