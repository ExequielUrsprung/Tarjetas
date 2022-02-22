Imports System.Timers
Imports System.Text

Public Class PinPadFD
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

    Private Const INICIALIZAR = "Y00"
    Private Const LEER_TARJETA = "Y01"
    Private Const DATOS_ADICIONALES = "Y02"
    Private Const RESPUESTA_EMISOR = "Y03"
    Private Const REINGRESO_PIN = "Y0400"
    Private Const CANCELAR_COMANDO = "Y06000"

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
    ''' Este evento se ejecuta cuando fue recibido un error Y0E desde el pinpad.
    ''' </summary>
    ''' <param name="comando">Comando que produjo el error.</param>
    ''' <param name="x">Descripción del error.</param>
    ''' <param name="nro">Nro del error.</param>
    ''' <remarks></remarks>
    Public Event Y0ERecibido(ByVal comando As String, ByVal x As String, ByVal nro As String)
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
    Dim _codBanco As Integer
    Dim _codServicio As Integer
    Dim _nroRegistro As Integer
    Dim _vencimiento As String
    Dim _SHA As String
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
    Dim _apn As String
    Dim _aid As String
    Dim _respuesta As String
    Dim _nth As String
    Dim _tipoencripcion As String
    Dim _paqencriptado As String

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

    Public ReadOnly Property CodBanco() As Integer
        Get
            Return _codBanco
        End Get
    End Property

    Public ReadOnly Property CodServicio() As Integer
        Get
            Return _codServicio
        End Get
    End Property

    Public ReadOnly Property NroRegistro() As Integer
        Get
            Return _nroRegistro
        End Get
    End Property

    Public ReadOnly Property SHA() As String
        Get
            Return _SHA
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
    Private Function Paquete(ByVal comando As String) As String

        Informar("STX" + comando + "ETX" + Chr(Lrc(comando + ETX)) & "     ---> |")
        Return STX + comando + ETX + Chr(Lrc(comando + ETX))

    End Function


    Public Function Abrir_puerto() As Boolean
        Try
            If puerto.IsOpen Then Cerrar_Puerto()
            puerto.Open()
            If puerto.IsOpen Then puerto.DiscardInBuffer()
        Catch ex As Exception
            RaiseEvent Y0ERecibido("Abriendo puerto", "No se pudo abrir puerto", Errores.Error_puerto)
        End Try
        Return puerto.IsOpen
    End Function
    Public Function Cerrar_Puerto() As Boolean
        Try
            If puerto.IsOpen Then puerto.DiscardInBuffer()
            If puerto.IsOpen Then puerto.Close()
        Catch ex As Exception
            RaiseEvent Y0ERecibido("Abriendo puerto", "No se pudo abrir puerto", Errores.Error_puerto)
        End Try
        Return puerto.IsOpen
    End Function

    Private Sub Limpiar_Variables()
        _codBanco = 0
        _codServicio = 0
        _modoIngreso = ""
        _nroRegistro = 0
        _SHA = ""
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
                RaiseEvent Y0ERecibido(paquete, "Puerto cerrado", Errores.Error_puerto)

            End If
            If reintento = 4 Then
                Informar("Nro de reintentos de envío superado")
                RaiseEvent Y0ERecibido(paquete, "Nro de reintentos de envío superado", Errores.Reintentos_superados)
            End If
        Catch ex1 As TimeoutException
            tm2.Stop()
            RaiseEvent Y0ERecibido(paquete, "TIMEOUT DE ESCRITURA - REINICIE EL PINPAD", Errores.Error_desconocido)
            mens()
        Catch ex As Exception
            tm2.Stop()
            RaiseEvent Y0ERecibido(paquete, "ERROR DESCONOCIDO - REINICIE EL PINPAD", Errores.Error_desconocido)
            mens()
        End Try

    End Sub

#Region "Comando Y00"
    Public Sub Inicializar_PinPad()
        Envio_datos(Paquete(INICIALIZAR & "0"))
    End Sub
#End Region

#Region "Comando Y01"
    ''' <summary>
    ''' Envia petición al pinpad para ingreso de tarjeta sin esperar respuesta. 
    ''' Se usa con manejo del evento Y01Recibido.
    ''' </summary>
    Public Sub Pedir_Tarjeta_Compra(ByVal tipo As Tipos_transacciones)

        Dim cr As Integer = tipo
        Envio_datos(Paquete(LEER_TARJETA & "030" & cr.ToString("00")))
    End Sub

    Public Sub Pedir_Tarjeta(ByVal tipo As Tipos_transacciones)

        Dim cr As Integer = tipo
        Envio_datos(Paquete(LEER_TARJETA & "V02030" & cr.ToString("00") & "00604" & FS))
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

    'Public Sub Pedir_Datos_Adicionales(Optional ByVal pWKey As String = "")
    ''' <summary>
    ''' Solicita los datos adicionales segun la tarjeta. Sin timeout.
    ''' </summary>
    ''' <param name="pU4D">Solicita Ultimos 4 digitos</param>
    ''' <param name="pCDS">Solicita Codigo de seguridad</param>
    ''' <param name="pET1">Envia Track 1</param>
    ''' <param name="pSPI">Solicita PIN</param>
    ''' <param name="pWK">Working Key</param>
    ''' <param name="pENC">Encripta</param>
    ''' <param name="pPTC">Solicita Tipo de cuenta</param>
    ''' <param name="pIMP">Importe, los ultimos 2 son los decimales, pasar sin coma y las 12 posiciones</param>
    ''' <param name="pICB">Importe Cashback, los ultimos 2 son los decimales, pasar sin coma y las 12 posiciones</param>
    Public Sub Pedir_Datos_Adicionales(ByVal pU4D As Boolean, ByVal pCDS As Boolean, ByVal pET1 As Boolean, ByVal pSPI As Boolean, ByVal pWK As String, ByVal pENC As Boolean, ByVal pPTC As Boolean, ByVal pIMP As String, ByVal pICB As String)
        Try
            Dim comando As String = ""
            comando = DATOS_ADICIONALES
            comando += Convert.ToByte(pU4D).ToString
            comando += Convert.ToByte(pCDS).ToString
            comando += Convert.ToByte(pET1).ToString
            comando += Convert.ToByte(pSPI).ToString
            comando += Convert.ToByte(pPTC).ToString
            If _modoIngreso = "C" And pWK = "" Then
                Informar("Falta WorkingKey")
                RaiseEvent Y0ERecibido("Y02", "Falta Workingkey", Errores.Falta_WorkingKey)
            Else
                If pSPI Or pENC Or _modoIngreso = "C" Then
                        comando += "0"
                        comando += pWK
                    Else
                        comando += "N"
                        comando += "N"
                    End If
                    comando += FS
                    comando += Convert.ToByte(pENC).ToString
                    comando += String.Format(pIMP, "000000000000")
                    comando += String.Format(pICB, "000000000000")
                    Envio_datos(Paquete(comando))
                'End If
            End If

        Catch ex As Exception
            RaiseEvent Y0ERecibido("Y02", "Error desconocido en Y02", Errores.Error_desconocido)

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

            Envio_datos(Paquete(RESPUESTA_EMISOR + pAut + pRespEmi + recibe + pCripto))
        Catch ex As Exception
            RaiseEvent Y0ERecibido("Y03", "Respuesta Y03", Errores.Error_desconocido)

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

            Envio_datos(Paquete(CANCELAR_COMANDO))
            'tm1.Stop()
        Catch ex As Exception
            RaiseEvent Y0ERecibido("Y06", "Error cancelando comando", Errores.Error_desconocido)
            Informar("Error en cancelar comando")
        End Try
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
                caracter = Chr(puerto.ReadChar())
                cadena += caracter
            End If

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
                    Informar("                      | <--- " & cadena)
                    Enviar_ACK()
                    Leer_Mensaje(cadena)
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
                If _modoIngreso <> "M" Then
                    _codServicio = pal(1).Substring(0, 3)
                    _codBanco = pal(1).Substring(3, 3)
                    _nroRegistro = pal(1).Substring(6, 6)
                    _SHA = pal(1).Substring(12, 40)
                End If

                RaiseEvent Y01Recibido(cad)
            ElseIf CID = "Y02" Then
                Dim pal() As String = pCad.Split(FS)
                Select Case pal(0).Substring(0, 1)
                    Case "M"
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

                    Case "C"
                        '------------   CON ENCRIPCION  ---------------
                        '_PAN = pal(0).Substring(1)
                        '_vencimiento = pal(1).Substring(0, 4)
                        '_nroserie = pal(1).Substring(4)
                        '_nroserielogico = pal(2).Substring(0, 16)
                        '_tipoencripcion = pal(2).Substring(16, 1)
                        '_paqencriptado = pal(2).Substring(17)
                        '_criptograma = pal(3)
                        '_autorizacion = pal(4).Substring(0, 6)
                        '_respEmisor = pal(4).Substring(6, 2)
                        '_secuenciaPan = pal(4).Substring(8, 3)
                        '_tipocuenta = pal(4).Substring(11, 1)
                        '_SHA = pal(4).Substring(12, 40)
                        '_pin = pal(4).Substring(52)
                        '_apn = pal(5)
                        '_aid = pal(6)
                        'If pal(7).Substring(1, pal(7).Length - 2) = ETX Then
                        '    _nth = ""
                        'Else
                        '    _nth = pal(7).Substring(0, pal(7).Length - 2)
                        'End If

                        '------------   SIN ENCRIPCION  ---------------
                        _PAN = pal(0).Substring(1)
                        _vencimiento = pal(1).Substring(0, 4)
                        _track1 = pal(1).Substring(4)
                        _track2 = pal(2)
                        _trackNoleido = pal(3).Substring(0, 1)
                        _nroserie = pal(3).Substring(1)
                        _criptograma = pal(4)
                        _autorizacion = pal(5).Substring(0, 6)
                        _respEmisor = pal(5).Substring(6, 2)
                        _secuenciaPan = pal(5).Substring(8, 3)
                        _tipocuenta = pal(5).Substring(11, 1)
                        _SHA = pal(5).Substring(12, 40)
                        _pin = pal(5).Substring(52)
                        _apn = pal(6)
                        _aid = pal(7)
                        If pal(3).Substring(1, pal(3).Length - 2) = ETX Then
                            _nth = ""
                        Else
                            _nth = pal(8).Substring(0, pal(6).Length - 2)
                        End If



                    Case "B"
                        _PAN = pal(0).Substring(1)
                        _vencimiento = pal(1).Substring(0, 4)
                        _track1 = pal(1).Substring(4)
                        _track2 = pal(2)
                        _trackNoleido = pal(3).Substring(0, 1)
                        _codSeguridad = pal(3).Substring(1)
                        _nroserie = pal(4)
                        _tipocuenta = pal(5).Substring(0, 1)
                        _SHA = pal(5).Substring(1, 40)
                        _pin = pal(5).Substring(41)
                        _nth = pal(6).Substring(0, pal(6).Length - 2)

                End Select
                RaiseEvent Y02Recibido(cad)
            ElseIf CID = "Y03" Then

                _autorizacion = pCad.Substring(0, 6)
                _respuesta = pCad.Substring(6, 2)
                _criptograma = pCad.Substring(8, pCad.Length - 10)

                RaiseEvent Y03Recibido(cad)
            ElseIf CID = "Y0E" Then

                RaiseEvent Y0ERecibido(pCad.Substring(0, 3), CType(CInt(pCad.Substring(3, 2)), Errores).ToString, pCad.Substring(3, 2))
            ElseIf CID = "Y00" Then
                Dim pal() As String = pCad.Split(FS)
                _versionSO = pal(0)
                _versionSoft = pal(1)
                _versionKernel = pal(2).Substring(0, pal(2).Length - 2)
                RaiseEvent Y00Recibido(pCad)
            End If

        Catch ex As Exception

            RaiseEvent Y0ERecibido("EEE", "Error leyendo stream", Errores.Error_desconocido)
            RaiseEvent DatosRecibidos("Error leyendo stream " & vbNewLine & "EX.MESSAGE: " & ex.Message & vbNewLine & "EX.STACKTRACE: " & ex.StackTrace & vbNewLine & "EX.SOURCE" & ex.Source)
        End Try


    End Sub


    Private Sub Enviar_ACK()
        puerto.Write(ACK)
        Informar("ACK --->              |")
    End Sub
    Private Sub Enviar_NAK()
        puerto.Write(NAK)
        Informar("NAK --->              |")
    End Sub
    Private Sub Enviar_EOT()
        puerto.Write(EOT)
        Informar("EOT --->              |")
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