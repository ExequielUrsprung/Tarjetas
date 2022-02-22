Imports System.Timers
Imports System.Text
Imports System.Security.Cryptography

Public Class PinPadFD_V2
    Private WithEvents puerto As New Puerto
    Dim WithEvents tm2 As New Timer(8000)
    Dim nro_nak As Byte
    Dim reenviar As Boolean

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
    Private Const REINGRESO_PIN = "Y0400"
    Private Const LEER_TARJETA_CL = "Y21"
    Private Const CANCELAR_COMANDO = "Y06"
    Private Const MENSAJE_PANTALLA = "Y07"          '--- TIPO CUENTA CTLS 
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
    ''' Este evento se produce cuando se recibe la respuesta del comando Y01.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <remarks></remarks>
    Public Event Y21Recibido(ByVal x As String)

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


    Dim _cst As String
    Dim _solicitaFirma As String
    Dim _datosEmisor As String


    Public ReadOnly Property ConsultaCopia()
        Get
            If _cst = "1" Then
                Return "Consulta por copia"
            Else
                Return "No Consulta por copia"
            End If
        End Get
    End Property

    Public ReadOnly Property SolicitaFirmaCliente()
        Get
            If _solicitaFirma = "1" Then
                Return "Solicita firma Cliente en ticket"
            Else
                Return "NO Solicita firma Cliente en ticket"
            End If
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
            If _respuesta = "00" Then
                Return "Aprobada"
            Else
                Return "Denegada"
            End If
        End Get
    End Property
    Public ReadOnly Property RespuestaSincronizacion()
        Get
            If _respuesta = "00" Then
                Return "Sincronizado OK"
            ElseIf _respuesta = "01" Then
                Return "Reintente Error"
            ElseIf _respuesta = "02" Then
                Return "Error WK. (Solicitar WK)"
            Else
                Return "Cod Respuesta desconocido"
            End If
        End Get
    End Property

    Public ReadOnly Property EstadoSincronizacion()
        Get
            If _respuesta = "00" Then
                Return "Sincronizado"
            ElseIf _respuesta = "02" Then
                Return "Debe Sincronizar"
            Else
                Return "Error: Devolvió " & _respuesta
            End If
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
            If _soportaCL = 0 Then
                Return "No soporta"
            Else
                Return "Soporta"
            End If

        End Get
    End Property


    Public ReadOnly Property SoportaImpresion() As String
        Get
            If _soportaImp = 0 Then
                Return "No soporta"
            Else
                Return "Soporta"
            End If

        End Get
    End Property

    Public ReadOnly Property RSAPresente() As String
        Get
            If _claveRSA = 0 Then
                Return "No tiene"
            Else
                Return "Presente"
            End If

        End Get
    End Property

    Public ReadOnly Property PantallaTouch() As String
        Get
            If _pantallaTouch = 0 Then
                Return "No tiene"
            Else
                Return "Tiene"
            End If

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
            If _trackNoleido = 1 Then
                Return "No Leido"
            Else
                Return "Leido"
            End If
        End Get
    End Property

    Public ReadOnly Property Nombre() As String
        Get
            Return _nombre
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

#Region "Inicializacion"

    Private Sub Informar(dato As String)
        Console.WriteLine(dato)
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
            If data Is Nothing Or data.Length = 0 Or len = 0 Or offset + len > data.Length Then
                Return String.Empty
            End If
            Dim s As New StringBuilder(len * 4)
            If Not IsNullOrEmpty(prefix) Then
                s.Append(prefix)
                s.Append(Environment.NewLine)
            End If
            Dim c As Byte
            Dim i, j As Integer
            Dim charsPerLine As Integer = 20
            s.Append("     1 |")

            i = 0
            While i < len
                If i > 0 And (i Mod charsPerLine) = 0 Then
                    s.Append("| ")
                    j = i - charsPerLine
                    While j < i
                        c = data(j + offset)
                        c = c >> 4 And &HF

                        If c < 10 Then
                            s.Append(c)
                        Else
                            s.Append(Hex(c))
                        End If

                        c = data(j + offset) And &HF
                        If (c < 10) Then
                            s.Append(c)
                        Else
                            s.Append(Hex(c))
                        End If

                        If j + 1 < i Then
                            s.Append(" ")
                        End If
                        j += 1
                    End While
                    s.Append(Environment.NewLine)
                    s.Append(String.Format("{0,6} |", i + 1))
                End If
                c = data(i + offset)
                If ((Not Convert.ToChar(c) = "\t") And (Char.IsLetterOrDigit(Convert.ToChar(c)) Or Char.IsPunctuation(Convert.ToChar(c)) Or
                 Char.IsSeparator(Convert.ToChar(c)) Or Char.IsSymbol(Convert.ToChar(c)) Or Char.IsWhiteSpace(Convert.ToChar(c)))) Then

                    s.Append(Chr(c))
                Else
                    Select Case Chr(c)
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

                i += 1
            End While
            s.Append("|")
            If i Mod charsPerLine = 0 Then
                j = 0
            Else
                j = charsPerLine - (i Mod charsPerLine)
            End If
            While j > 0
                s.Append(" ")
                j -= 1
            End While
            s.Append(" ")
            If i Mod charsPerLine = 0 Then
                j = i - charsPerLine
            Else
                j = i - (i Mod charsPerLine)
            End If
            While j < i
                c = data(j + offset)
                c = c >> 4 And &HF
                If (c < 10) Then
                    s.Append(c)
                Else
                    s.Append(Hex(c))
                End If
                c = data(j + offset) And &HF
                If c < 10 Then
                    s.Append(c)
                Else
                    s.Append(Hex(c))
                End If
                If ((j + 1) < i) Then
                    s.Append(" ")
                End If
                j += 1
            End While
            s.Append(Environment.NewLine)
            Return s.ToString()
        Catch ex As Exception
            Return "No se puede mostrar"

        End Try

    End Function

    Public Sub GetPrintableBuffer(texto As String, cadena As String)
        Dim encoding As New System.Text.ASCIIEncoding
        Debug.Write(GetPrintableBuffer(texto, Encoding.GetBytes(cadena), 0, cadena.Length))
    End Sub
    Public Function IsNullOrEmpty(ByVal value As String) As Boolean
        Return String.IsNullOrEmpty(value) Or value.Length = 0 Or value.Trim().Length = 0
    End Function


    Private Function Paquete(ByVal comando As String) As String

        Dim PackCompleto As String
        comando = PreprocesarDLEIda(comando)
        PackCompleto = STX + comando + ETX + Chr(Lrc(comando + ETX))
        Informar("STX" + comando + "ETX" + Chr(Lrc(comando + ETX)) & "     ---> |")

        'GetPrintableBuffer(String.Format("Enviado a PP: ({0} {1})", PackCompleto.Length, If(PackCompleto.Length = 1, "byte", "bytes")), PackCompleto)
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
        Try
            If puerto.IsOpen Then puerto.DiscardInBuffer()
            If puerto.IsOpen Then puerto.Close()
        Catch ex As Exception
            RaiseEvent Y0ERecibido("Abriendo puerto", "No se pudo abrir puerto", Errores.Error_puerto, "")
        End Try
        Return puerto.IsOpen
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
    End Sub

    Dim llego As Boolean
    Dim tiempo As Date
#End Region

    Private Sub mens() Handles tm2.Elapsed
        Cerrar_Puerto()
        Abrir_puerto()
    End Sub


    Private Sub Envio_datos(paquete As String)
        Dim reintento As Byte = 1
        Dim hora As DateTime
        Try
            reenviar = True
            nro_nak = 0

            If puerto.IsOpen Then
                puerto.DiscardInBuffer()


                Do While reenviar And reintento < 4
                    tm2.Start()
                    esperar = True
                    Informar("Envío intento: " & reintento)
                    puerto.Write(paquete)
                    tm2.Stop()
                    hora = Now

                    Do
                        'espera a que reciba la respuesta
                    Loop Until Not esperar Or Now.Subtract(hora).TotalSeconds > 7
                    reintento += 1
                Loop
            Else
                Informar("Puerto cerrado")
                RaiseEvent Y0ERecibido(paquete, "Puerto cerrado", Errores.Error_puerto, "")

            End If
            If reintento = 4 Then
                Informar("Nro de reintentos de envío superado")
                RaiseEvent Y0ERecibido(paquete, "Nro de reintentos de envío superado", Errores.Reintentos_superados, "")
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

    ''' <summary>
    ''' Convierte una array de byte en un string.
    ''' </summary>
    ''' <param name="tex"></param>
    ''' <returns></returns>
    Private Function hexastring(tex As Byte()) As String
        Dim vari As StringBuilder = New StringBuilder(tex.Length * 2)
        For Each x In tex
            vari.AppendFormat("{0:x2}", x)
        Next
        Return vari.ToString.ToUpper
    End Function

    Public Sub Inicializar_PinPad()
        'Dim rsa As New RSACryptoServiceProvider()
        'Dim RsaParam As New RSAParameters

        'RsaParam = rsa.ExportParameters(True)
        'Envio_datos(Paquete(INICIALIZAR & "V0210000000000" & hexastring(RsaParam.Modulus)))
        Envio_datos(Paquete(INICIALIZAR & "V0210000000000"))
    End Sub
#End Region

#Region "Comando Y01"
    ''' <summary>
    ''' Envia petición al pinpad para ingreso de tarjeta sin esperar respuesta. 
    ''' Se usa con manejo del evento Y01Recibido.
    ''' </summary>
    Public Sub Pedir_Tarjeta_Compra(ByVal tipo As Tipos_transacciones)

        Dim cr As Integer = tipo
        Envio_datos(Paquete(LEER_TARJETA & "060" & cr.ToString("00")))
    End Sub

    ''' <summary>
    ''' Solicita tarjeta de compra para dar inicio a la transacción.
    ''' </summary>
    ''' <param name="tipo"></param>
    ''' <param name="sincronizarFechaHora">True si sincroniza Fecha y Hora actual del equipo en donde se encuentra conectado el PP</param>
    Public Sub Pedir_Tarjeta(ByVal tipo As Tipos_transacciones, ByVal sincronizarFechaHora As Boolean)

        Dim cr As Integer = tipo
        If sincronizarFechaHora Then
            Envio_datos(Paquete(LEER_TARJETA & "V02030" & cr.ToString("00") & "00604" & Now.Date.ToString("YYYYMMDD") & FS & Now.Date.ToString("HHmmSS")))

        Else
            Envio_datos(Paquete(LEER_TARJETA & "V02030" & cr.ToString("00") & "00604" & FS))

        End If
    End Sub

    Public Sub Pedir_Tarjeta_Contactless(ByVal tipo As Tipos_transacciones)

        Dim cr As Integer = tipo
        Envio_datos(Paquete("Y2130" & cr.ToString("00") & "0000020180228" & FS & "110000" & FS & "0211111111111111112222222222222222" & FS & "33333333333333334444444444444444" & "0000000000100000000000000"))
    End Sub

    '''' <summary>
    '''' Envia Petición al pinpad para ingreso de tarjeta y espera respuesta.
    '''' Respuestas 0=Recibido OK, 1=TIMEOUT, 2=Error
    '''' </summary>
    '''' <param name="timeout">Tiempo de espera máximo antes de cancelar la petición.</param>
    '''' <returns></returns>
    '''' <remarks></remarks>
    'Public Function Pedir_Tarjeta_Compra(ByVal timeout As Integer) As Integer
    '    Try
    '        nro_nak = 0
    '        'tiempo = Now
    '        llego = False
    '        puerto.DiscardInBuffer()
    '        Dim cr As Integer = Tipos_transacciones.compra
    '        Envio_datos(Paquete(LEER_TARJETA & "030" & cr.ToString("00")))
    '        Do
    '            'espera respueta                
    '        Loop Until llego Or Now.Subtract(tiempo).TotalSeconds > timeout

    '        If Not llego Then
    '            Cancelar()
    '            'puerto.Write(Paquete(CANCELAR_COMANDO))
    '            Return Errores.TimeOut
    '        End If
    '        Return 0
    '    Catch ex As Exception
    '        Return 2
    '    End Try
    'End Function

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
    Public Sub Solicitar_Datos_Adicionales_prisma(ByVal pU4D As Boolean, ByVal pCDS As Boolean, ByVal pSPI As Boolean, ByVal pWKD As String, ByVal pWKP As String, posicionMK As String, ByVal pIMP As Decimal, ByVal pICB As Decimal)
        Try
            encripta = True
            Dim comando As String = ""
            comando = DATOS_ADICIONALES
            comando += "V02"
            comando += Convert.ToByte(pU4D).ToString
            comando += Convert.ToByte(pCDS).ToString
            comando += "1" 'transmite track 1 siempre
            comando += Convert.ToByte(pSPI).ToString
            If pSPI Then
                comando += "1" 'pide tipo de cuenta si solicita pin.
            Else
                comando += "0"
            End If
            comando += posicionMK
            comando += "1"
            comando += "N" 'pWKP
            comando += FS
            comando += pWKP 'pWKD
            comando += FS
            comando += "1"
            comando += (pIMP * 100).ToString("000000000000")
            comando += (pICB * 100).ToString("000000000000")


            Envio_datos(Paquete(comando))


        Catch ex As Exception
            RaiseEvent Y0ERecibido("Y02", "Error desconocido en Y02", Errores.Error_desconocido, "")

        End Try

    End Sub

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
    Public Sub Solicitar_Datos_Adicionales_FD(ByVal pU4D As Boolean, ByVal pCDS As Boolean, ByVal pSPI As Boolean, ByVal pWKD As String, ByVal pWKP As String, posicionMK As String, ByVal pIMP As Decimal, ByVal pICB As Decimal)
        Try
            encripta = True
            Dim comando As String = ""
            comando = DATOS_ADICIONALES
            comando += "V02"
            comando += Convert.ToByte(pU4D).ToString
            comando += Convert.ToByte(pCDS).ToString
            comando += "1" 'transmite track 1 siempre
            comando += Convert.ToByte(pSPI).ToString
            If pSPI Then
                comando += "1" 'pide tipo de cuenta si solicita pin.
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

    '''' <summary>
    '''' Con Timeout
    '''' </summary>
    '''' <param name="pTimeout"></param>
    '''' <param name="pWKey"></param>
    '''' <returns></returns>
    'Public Function Pedir_Datos_Adicionales(ByVal pTimeout As Integer, Optional ByVal pWKey As String = "") As Integer
    '    Try
    '        tiempo = Now
    '        llego = False
    '        If pWKey = "" Then
    '            puerto.Write(Paquete(DATOS_ADICIONALES + "11100NN" + FS + "0" + "000000012000" + "000000000000"))
    '        Else
    '            puerto.Write(Paquete(DATOS_ADICIONALES + "10" + "0" + "1" + pWKey + FS + pWKey + FS + "0" + "000000000000" + "000000000000"))
    '        End If
    '        Do
    '            'espera respueta
    '        Loop Until llego Or Now.Subtract(tiempo).TotalSeconds > pTimeout

    '        If Not llego Then
    '            puerto.Write(Paquete(CANCELAR_COMANDO))
    '            Return 1
    '        End If
    '        Return 0

    '    Catch ex As Exception
    '        MsgBox(ex.Message)
    '        Return 1
    '    End Try

    'End Function
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
            'sacar******
            'pAut = "123456"
            'pCripto = _criptograma
            'pRespEmi = "00"
            'pRecibe = True
            ''***********
            If pRecibe Then
                recibe = "1"
            Else
                recibe = "0"
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
    '''' <summary>
    '''' Con timeout
    '''' </summary>
    '''' <param name="pTimeOut"></param>
    '''' <param name="pAut"></param>
    '''' <param name="pRespEmi"></param>
    '''' <param name="pRecibe"></param>
    '''' <param name="pCripto"></param>
    '''' <returns></returns>
    'Public Function Datos_Respuesta_Emisor(ByVal pTimeOut As Integer, ByVal pAut As String, ByVal pRespEmi As String, ByVal pRecibe As Boolean, ByVal pCripto As String) As Integer
    '    Dim recibe As String
    '    Try
    '        'sacar******
    '        'pAut = "123456"
    '        'pCripto = _criptograma
    '        'pRespEmi = "00"
    '        'pRecibe = True
    '        '***********
    '        If pRecibe Then
    '            recibe = "1"
    '        Else
    '            recibe = "0"
    '        End If
    '        tiempo = Now
    '        llego = False
    '        Envio_datos(Paquete(RESPUESTA_EMISOR + pAut + pRespEmi + recibe + pCripto))

    '        Do
    '            'espera respueta
    '        Loop Until llego Or Now.Subtract(tiempo).TotalSeconds > pTimeOut

    '        If Not llego Then
    '            Cancelar()
    '            'puerto.Write(Paquete(CANCELAR_COMANDO))
    '            Return 1
    '        End If
    '        Return 0

    '    Catch ex As Exception
    '        MsgBox(ex.Message)
    '    End Try

    'End Function


#End Region

#Region "Comando Y04"
    Public Sub Reingresar_pin(ByVal pWkey As String)
        Envio_datos(Paquete(REINGRESO_PIN + pWkey))
    End Sub
#End Region

#Region "Comando Y06"
    'Private Sub Controlar_TimeOut(ByVal sender As Object, ByVal e As ElapsedEventArgs) Handles tm1.Elapsed
    '    Try
    '        If Now.Subtract(tiempo).TotalSeconds > TIEMPOESPERA Then
    '            tm1.Stop()
    '            puerto.Cerrar_Conexion()
    '            Cancelar()
    '        End If
    '    Catch ex As Exception

    '    End Try
    'End Sub

    Public Sub Cancelar()
        Try

            Envio_datos(Paquete(CANCELAR_COMANDO & "V02000"))
            'tm1.Stop()
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
    Public Sub Pedir_Tarjeta_Compra_CL(ByVal tipo As Tipos_transacciones)

        Dim cr As Integer = tipo
        Envio_datos(Paquete(LEER_TARJETA_CL & "V02030" & cr.ToString("00") & "10000" & FS &
                            FS & "0" & "1" & "1234123412341234" & FS & "N" &
                            FS & "0" & "000000000100" & "000000000000"))
    End Sub

#End Region

#Region "Sincronizacion"
    ''' <summary>
    ''' Verifica si se debe sincronizar el PinPad.
    ''' </summary>
    ''' <param name="pTerminal"></param>
    ''' <param name="pSerieFisico"></param>
    Public Sub Sincronizar_PinPad(pTerminal As String, pSerieFisico As String)
        Envio_datos(Paquete(SINCRONIZAR & "V02" & pTerminal & pSerieFisico))
    End Sub

    Public Sub Comenzar_Sincronizacion_PinPad(pTerminal As String)
        Envio_datos(Paquete(COMENZARSINCRONIZACION & "V02" & pTerminal))
    End Sub


    Public Lista_Claves As New Dictionary(Of String, Pares_claves)
    Public Sub Confirmar_Sincronizacion_Pinpad(pTerminal As String)
        Dim s As String = ""
        Dim lis As String = ""
        Dim pos As String = ""
        Dim repe As String = ""
        Dim canthost As Integer = 0
        Dim cantWK As Integer = 0
        Dim nombre As String = ""

        s = CONFIRMARSINCRONIZACION & "V02" & pTerminal
        For Each clave In Lista_Claves

            If nombre <> clave.Key.Substring(0, clave.Key.Length - 2) Then
                canthost += 1
                repe += pos & cantWK & "2" & lis
                lis = ""
                cantWK = 0
            End If
            nombre = clave.Key.Substring(0, clave.Key.Length - 2)
            If nombre = clave.Key.Substring(0, clave.Key.Length - 2) Then
                cantWK += 1
                pos = clave.Value.posicion
            End If

            lis = clave.Value.tipo & clave.Value.clave & FS & clave.Value.control & FS
        Next
        s += s & canthost & repe


        Envio_datos(Paquete(s))

        'Envio_datos(Paquete(CONFIRMARSINCRONIZACION & "V02" & "06000099" & "1" & "0" & "1" & "2" & "2" & "2B41426341514143592D3F2B41506F41" & FS & "4241" & FS &
        '                                                                                          "1" & "552B4142512D4D2B414E514179774375" & FS & "4142" & FS))
    End Sub

    Public Sub Consultar_Protocolo()
        Envio_datos(Paquete(CONSULTAPROTOCOLO))
    End Sub

#End Region









#Region "Respuestas"

    Dim esperar As Boolean




    Private Sub Recuperar_Respuesta(ByVal sender As Object, ByVal e As IO.Ports.SerialDataReceivedEventArgs) Handles puerto.DataReceived
        Dim cadena As String = ""
        Dim caracter As Char
        Dim terminar As Boolean = False
        Dim inicio As DateTime = Now
        Try
            cadena = ""

            Do
                caracter = Chr(puerto.ReadChar())
                cadena += caracter
                If Now.Subtract(inicio).Seconds > 5 Then terminar = True
            Loop Until caracter = ETX Or caracter = ACK Or caracter = NAK Or caracter = EOT Or terminar
            If caracter = ETX Then 'busca uno mas porque es el LRC
                '            cadena = STX & PreprocesarDLEVuelta(cadena.Substring(1, cadena.Length - 2)) & ETX
                caracter = Chr(puerto.ReadChar())
                cadena += caracter

            End If

            'GetPrintableBuffer(String.Format("Recibido desde PP: ({0} {1})", cadena.Length, If(cadena.Length = 1, "byte", "bytes")), cadena)
            If cadena = ACK Then
                reenviar = False
                esperar = False
                Informar("                      | <--- ACK")
            ElseIf cadena = NAK Then
                esperar = False
                Informar("                      | <--- NAK")
            ElseIf cadena = EOT Then
                esperar = False
                Informar("                      | <--- EOT")
            Else
                Dim control As String = ""
                Try
                    control = Chr(Lrc(cadena.Substring(1, cadena.Length - 2)))
                Catch
                    Informar("NO SE PUDO OBTENER LRC")
                End Try

                If control = cadena.Substring(cadena.Length - 1) Then 'Controla que el LRC este bien.
                    reenviar = False
                    Informar("                      | <--- " & cadena)
                    Enviar_ACK()
                    Leer_Mensaje(cadena)
                    llego = True
                Else
                    Informar("ERROR DE TRANSMISION")
                    nro_nak += 1
                    If nro_nak = 4 Then
                        Enviar_EOT()
                    Else
                        Enviar_NAK()
                    End If

                End If
            End If
        Catch ex As Exception
            Informar(ex.Message)

        Finally
            esperar = False
        End Try
    End Sub


    Private Function PreprocesarDLEIda(s As String) As String
        Dim cadena As String = ""
        For i = 0 To s.Length - 1
            If s.Substring(i, 1) = ETX Or s.Substring(i, 1) = DLE Then
                cadena = cadena & DLE & s.Substring(i, 1)
            Else
                cadena = cadena & s.Substring(i, 1)
            End If
        Next
        Return cadena
    End Function
    Private Function PreprocesarDLEVuelta(s As String) As String
        Dim cadena As String = ""
        For i = 0 To s.Length - 1
            If s.Substring(i, 1) = DLE Then
                cadena = cadena & s.Substring(i + 1, 1)
                i += 1
            Else
                cadena = cadena & s.Substring(i, 1)
            End If
        Next
        Return cadena
    End Function

    Private Sub Leer_Mensaje(ByVal pCad As String)
        Dim CID As String = ""
        Dim cad As String = pCad.Substring(1, pCad.Length - 3)
        Dim largo As Integer = 0

        Try
            If pCad.Substring(0, 1) = STX Then
                pCad = pCad.Remove(0, 1) 'SACO EL STX
            End If
            CID = pCad.Substring(0, 3) 'LEO EL ID DE COMANDO
            pCad = pCad.Remove(0, 3)

            If CID = "Y01" Then
                Dim pal() As String = pCad.Split(FS)
                Limpiar_Variables()
                _modoIngreso = pal(0).Substring(0, 1) 'MODO INGRESO
                _PAN = pal(0).Substring(1) 'PAN

                _codServicio = pal(1).Substring(0, 3)
                _codBanco = pal(1).Substring(3, 3)
                _nroRegistro = pal(1).Substring(6, 6)
                _abreviatura = pal(1).Substring(12, 2)
                If pal(1).Substring(14, 1) = ETX Then
                    _aplicacionChip = ""
                Else
                    _aplicacionChip = pal(1).Substring(14, (pal(1).Length - 2) - 14)
                End If
                RaiseEvent Y01Recibido(cad)

            ElseIf CID = "Y02" Then
                Dim pal() As String = pCad.Split(FS)
                _modoIngreso = pal(0).Substring(0, 1) 'MODO INGRESO
                Select Case pal(0).Substring(0, 1)
                    Case "M"
                        If encripta Then
                            _PAN = pal(0).Substring(1)
                            _nroserie = pal(1)
                            _IDEncripcionUnico = pal(2).Substring(0, 16)
                            _tipocuenta = pal(2).Substring(16, 1)
                            _tipoencripcion = pal(2).Substring(17, 1)
                            _paqencriptado = pal(2).Substring(18)
                            If pal(3).Substring(1, pal(3).Length - 2) = ETX Then
                                _pin = ""
                            Else
                                _pin = pal(3).Substring(1, pal(3).Length - 2)
                            End If
                        Else
                            _PAN = pal(0).Substring(1)
                            _vencimiento = pal(1).Substring(0, 4)
                            _codSeguridad = pal(1).Substring(4)
                            _nroserie = pal(2)
                            _tipocuenta = pal(3).Substring(0, 1)
                            If pal(3).Substring(1, pal(3).Length - 2) = ETX Then
                                _pin = ""
                            Else
                                _pin = pal(3).Substring(1, pal(3).Length - 2)
                            End If
                        End If

                    Case "C"
                        '------------   CON ENCRIPCION  ---------------
                        _PAN = pal(0).Substring(1)
                        _nroserie = pal(1)

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
                        If pal(7).Substring(1, pal(7).Length - 2) = ETX Then
                            _nth = ""
                        Else
                            _nth = pal(7).Substring(0, pal(7).Length - 2)
                        End If

                    Case "B"
                        If pal.Length = 7 Then 'no encripta
                            _modoIngreso = pal(0).Substring(0, 1)
                            _PAN = pal(0).Substring(1)
                            _vencimiento = pal(1).Substring(0, 4)
                            _track1 = pal(1).Substring(3)
                            _track2 = pal(2).Substring(0)
                            _trackNoleido = pal(3).Substring(0, 1)
                            _codSeguridad = pal(3).Substring(1)
                            _nroserie = pal(4)
                            _tipocuenta = pal(5).Substring(0, 1)
                            _pin = pal(5).Substring(1)
                            If pal(6).Substring(0, pal(6).Length - 2) = ETX Then
                                _nth = ""
                            Else
                                _nth = pal(6).Substring(0, pal(6).Length - 2)
                            End If
                        Else 'encripta
                            _modoIngreso = pal(0).Substring(0, 1)
                            _PAN = pal(0).Substring(1)
                            _nroserie = pal(1)
                            _IDEncripcionUnico = pal(2).Substring(0, 16)
                            _tipocuenta = pal(2).Substring(16, 1)
                            _tipoencripcion = pal(2).Substring(17, 1)

                            _paqencriptado = pal(2).Substring(18)
                            _pin = pal(3)
                            If pal(4).Substring(0, pal(4).Length - 2) = ETX Then
                                _nth = ""
                            Else
                                _nth = pal(4).Substring(0, pal(4).Length - 2)
                            End If
                        End If
                End Select
                RaiseEvent Y02Recibido(cad)

            ElseIf CID = "Y03" Then
                _autorizacion = pCad.Substring(0, 6)
                _respuesta = pCad.Substring(6, 2)
                If pCad.Substring(8, pCad.Length - 10) = ETX Then
                    _criptograma = ""
                Else
                    _criptograma = pCad.Substring(8, pCad.Length - 10)
                End If
                RaiseEvent Y03Recibido(cad)

            ElseIf CID = "Y21" Then
                Dim pal() As String = pCad.Split(FS)
                Limpiar_Variables()

                _modoIngreso = pal(0).Substring(0, 1) 'MODO INGRESO
                _PAN = pal(0).Substring(1) 'PAN

                _codServicio = pal(1).Substring(0, 3)
                _codBanco = pal(1).Substring(3, 3)
                _nroRegistro = pal(1).Substring(6, 6)
                _nroserie = pal(1).Substring(12)
                _vencimiento = pal(2).Substring(0, 4)
                _track1 = pal(2).Substring(4)
                _track2 = pal(3)
                _trackNoleido = pal(4).Substring(0, 1)
                _criptograma = pal(4).Substring(1)
                _autorizacion = pal(5).Substring(0, 6)
                _respEmisor = pal(5).Substring(6, 2)
                _secuenciaPan = pal(5).Substring(8, 3)
                _tipocuenta = pal(5).Substring(11, 1)
                _pin = pal(5).Substring(12)
                _apn = pal(6)
                _aid = pal(7)
                _nth = pal(8)
                _cst = pal(9).Substring(0, 1)
                _solicitaFirma = pal(9).Substring(1, 1)
                _datosEmisor = pal(9).Substring(2, pal(9).Length - 4)
                RaiseEvent Y21Recibido(cad)

            ElseIf CID = "Y0E" Then
                Dim pal() As String = pCad.Split(FS)
                _comando_recibido = pal(0).Substring(0, 3)
                _respuesta = pal(0).Substring(3, 2)
                If pal(0).Substring(5, 1) = ETX Then
                    _mensaje_extendido = ""
                Else
                    _mensaje_extendido = pal(0).Substring(5, (pal(0).Length - 2) - 5)
                End If
                RaiseEvent Y0ERecibido(pCad.Substring(0, 3), CType(CInt(pCad.Substring(3, 2)), Errores).ToString, pCad.Substring(3, 2), _mensaje_extendido)

            ElseIf CID = "Y00" Then
                Dim pal() As String = pCad.Split(FS)
                _versionSO = pal(0)
                _versionSoft = pal(1)
                _versionKernel = pal(2)
                _versionKernelCTLS = pal(3)
                _soportaCL = pal(4).Substring(0, 1)
                _soportaImp = pal(4).Substring(1, 1)
                _claveRSA = pal(4).Substring(2, 1)
                _pantallaTouch = pal(4).Substring(3, 1)
                RaiseEvent Y00Recibido(pCad)

            ElseIf CID = "S00" Then
                _respuesta = pCad.Substring(0, 2)
                RaiseEvent S00Recibido(pCad)

            ElseIf CID = "S01" Then
                Dim pal() As String = pCad.Split(FS)

                _CantHostMK = pal(0).Substring(0, 1)

                ReDim _NombreHost(_CantHostMK)
                ReDim _posicionMK(_CantHostMK)
                ReDim _IDEncripcion(_CantHostMK)
                For x As Integer = 0 To _CantHostMK - 1
                    If x = 0 Then
                        _NombreHost(0) = pal(0).Substring(1)
                    Else
                        _NombreHost(x) = pal(x).Substring(17)
                    End If

                    _posicionMK(x) = pal(x + 1).Substring(0, 1)
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
            MsgBox(ex.Message)

            RaiseEvent Y0ERecibido("EEE", "Error leyendo stream", Errores.Error_desconocido, "")
            RaiseEvent DatosRecibidos("Error leyendo stream")
        End Try


    End Sub



    Private Sub Enviar_ACK()
        puerto.Write(ACK)
        Informar("ACK --->              |")
        'GetPrintableBuffer("Recibido desde PP: (1 byte)", ACK)
    End Sub
    Private Sub Enviar_NAK()
        puerto.Write(NAK)
        Informar("NAK --->              |")
        'GetPrintableBuffer("Recibido desde PP: (1 byte)", NAK)
    End Sub
    Private Sub Enviar_EOT()
        puerto.Write(EOT)
        Informar("EOT --->              |")
        'GetPrintableBuffer("Recibido desde PP: (1 byte)", EOT)
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
            dig = dig + DXor(Bin(Asc(letr)))
            acu = BinXor(acu, Bin(Asc(letr)))
        Next
        sol = 0
        For f = 1 To 8
            If Mid(acu, f, 1) = "1" Then sol = sol + 2 ^ (f - 1)
        Next
        Return sol
    End Function


    Private Function Bin(ByVal num As Integer) As String
        Dim resu As String
        Dim j As Single
        resu = ""
        For j = 1 To 8
            resu = resu + LTrim(Str(num Mod 2))
            num = Fix(num / 2)
        Next
        Return resu
    End Function

    Private Function DXor(ByVal dat As String) As Integer
        Dim j, resd As Integer
        For j = 1 To 7
            resd = resd + Val(Mid(dat, j, 1))
        Next
        If resd Mod 2 = 1 Then Return 1 Else Return 0
    End Function


    Private Function BinXor(ByVal bin1 As String, ByVal bin2 As String) As String
        Dim j As Integer
        Dim resu As String
        resu = ""
        For j = 1 To 8
            If Val(Mid(bin1, j, 1)) Xor Val(Mid(bin2, j, 1)) Then
                resu = resu + "1"
            Else
                resu = resu + "0"
            End If
        Next
        Return resu
    End Function

#End Region
End Class
