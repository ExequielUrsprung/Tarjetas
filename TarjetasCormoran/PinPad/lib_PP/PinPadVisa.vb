Imports System.Timers
Imports System.Text
Imports System.Security.Cryptography

Public Class PinPadVISA

    Private WithEvents puerto As New Puerto
    Dim WithEvents tm1 As New Timer
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

    Private Const INICIALIZAR = "Y000"
    Private Const LEER_TARJETA = "Y01"
    'Private Const DATOS_ADICIONALES = "Y02001"
    Private Const DATOS_ADICIONALES = "Y02"
    Private Const RESPUESTA_EMISOR = "Y03"
    Private Const REINGRESO_PIN = "Y0400"
    Private Const CANCELAR_COMANDO = "Y06000"
    Private Const SINCRONIZAR = "S00"

    Private Const _ERROR As Integer = 2
    Private Const _PINCORRECTO As Integer = 1
    Private Const _REINTENTAR As Integer = 0

    Private Const TIEMPOESPERA As Integer = 30

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
    Dim _track1 As String
    Dim _track2 As String
    Dim _trackNoleido As Integer
    Dim _codSeguridad As String
    Dim _nroserie As String
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
    Dim _respuesta As String


    Public ReadOnly Property Respuesta()
        Get
            If _respuesta = "00" Then
                Return "Aprobada"
            Else
                Return "Denegada"
            End If
        End Get
    End Property


    Public ReadOnly Property VersionSO()
        Get
            Return _versionSO
        End Get
    End Property

    Public ReadOnly Property VersionSoft()
        Get
            Return _versionSoft
        End Get
    End Property

    Public ReadOnly Property Versionkernel()
        Get
            Return _versionKernel
        End Get
    End Property

    Public ReadOnly Property ModoIngreso()
        Get
            Return _modoIngreso
        End Get
    End Property

    Public ReadOnly Property PAN()
        Get
            Return _PAN
        End Get
    End Property

    Public ReadOnly Property CodBanco()
        Get
            Return _codBanco
        End Get
    End Property

    Public ReadOnly Property CodServicio()
        Get
            Return _codServicio
        End Get
    End Property

    Public ReadOnly Property NroRegistro()
        Get
            Return _nroRegistro
        End Get
    End Property


    Public ReadOnly Property CodSeguridad()
        Get
            Return _codSeguridad
        End Get
    End Property

    Public ReadOnly Property TipoCuenta()
        Get
            Return _tipocuenta
        End Get
    End Property


    Public ReadOnly Property SeriePinPad()
        Get
            Return _nroserie
        End Get
    End Property

    Public ReadOnly Property Vencimiento()
        Get
            Return _vencimiento
        End Get
    End Property

    Public ReadOnly Property Track1()
        Get
            Return _track1
        End Get
    End Property
    Public ReadOnly Property Track2()
        Get
            Return _track2
        End Get
    End Property

    Public ReadOnly Property TrackNoLeido()
        Get
            If _trackNoleido = 1 Then
                Return "No Leido"
            Else
                Return "Leido"
            End If
        End Get
    End Property

    Public ReadOnly Property Nombre()
        Get
            Return _nombre
        End Get
    End Property
    Public ReadOnly Property getPinEncript()
        Get
            Return _pin
        End Get
    End Property
    Public ReadOnly Property Criptograma()
        Get
            Return _criptograma
        End Get
    End Property

    Public ReadOnly Property Autorizacion()
        Get
            Return _autorizacion
        End Get
    End Property
    Public ReadOnly Property RespEmisor()
        Get
            Return _respEmisor
        End Get
    End Property
    Public ReadOnly Property SecuenciaPan()
        Get
            Return _secuenciaPan
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


    Public Sub Abrir_puerto()
        Try
            If puerto.IsOpen Then Cerrar_Puerto()
            puerto.Open()
            'puerto.Abrir_Conexion()
            puerto.DiscardInBuffer()
        Catch ex As Exception
            'TODO: loguear error
        End Try
    End Sub
    Public Sub Cerrar_Puerto()
        Try
            puerto.DiscardInBuffer()
            If puerto.IsOpen Then puerto.Close() 'puerto.Cerrar_Conexion()
        Catch ex As Exception
            'TODO: loguear error
        End Try
    End Sub

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
    Dim tiempo As Date = Now
#End Region


#Region "Comando Y00"
    Public Sub Inicializar_PinPad()
        Try
            puerto.Write(Paquete(INICIALIZAR))
        Catch ex As Exception

        End Try
    End Sub
#End Region

    Dim rsa As New Security.Cryptography.RSACryptoServiceProvider
    Dim RsaParam As New RSAParameters

#Region "Comando Y01"
    ''' <summary>
    ''' Envia petición al pinpad para ingreso de tarjeta sin esperar respuesta. 
    ''' Se usa con manejo del evento Y01Recibido.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Pedir_Tarjeta_Compra()
        Dim reintento As Byte = 1
        Try
            nro_nak = 0
            tiempo = Now 'hora de envio del comando para controlar timeout.
            tm1.Start()
            puerto.DiscardInBuffer()
            reenviar = True
            Dim rsa As New RSACryptoServiceProvider()


            RsaParam = rsa.ExportParameters(True)



            'Do While reenviar And reintento < 4
            puerto.Write(Paquete(LEER_TARJETA & hexastring(RsaParam.Modulus) & FS & hexastring(RsaParam.Exponent) & FS & "200"))
            'puerto.Write(Paquete(LEER_TARJETA & "EF2894EBAB25E045E7A1A4386B2FBE53333CC588A90979CBBAB5D0B33771A814EB47FD0FBB913EB3F77763B94EC4B7EFFD209BC18EEBE906B516604435097E02BB1B17A113EB33E9685C8F1896F078DC1A36496BD49E1D3C8832DF924D2FB9642271163BD2D53307EEE91C9E24AC98C2B9E2A6254C05D87184BD9AD88C77F1A3" & FS & "10001" & FS & "200"))
            Threading.Thread.Sleep(1000)
            reintento += 1
            'Loop
            If reintento = 4 Then
                tm1.Stop()
                Informar("Nro de reintentos de envío superado")
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub




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

    ''' <summary>
    ''' Convierte un string en un array de bytes
    ''' </summary>
    ''' <param name="hex"></param>
    ''' <returns></returns>
    Private Function StringToByte(hex As String) As Byte()


        Dim NumberChars As Integer = hex.Length
        Dim bytes((NumberChars / 2) - 1) As Byte
        Dim i As Integer = 0
        Do While i < NumberChars

            bytes(i / 2) = Convert.ToByte(Convert.ToInt32(hex.Substring(i, 2), 16))
            'MsgBox(Convert.ToInt32(hex.Substring(i, 2), 16))
            'MsgBox(bytes(0))
            i += 2
        Loop

        Return bytes


    End Function

    ''' <summary>
    ''' Envia Petición al pinpad para ingreso de tarjeta y espera respuesta.
    ''' Respuestas 0=Recibido OK, 1=TIMEOUT, 2=Error
    ''' </summary>
    ''' <param name="timeout">Tiempo de espera máximo antes de cancelar la petición.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Pedir_Tarjeta_Compra(ByVal timeout As Integer) As Integer
        Try
            nro_nak = 0
            tiempo = Now
            llego = False
            puerto.DiscardInBuffer()
            puerto.Write(Paquete(LEER_TARJETA & "TESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTESTTEST" & FS & "2" & FS & "200"))
            Do
                'espera respueta                
            Loop Until llego Or Now.Subtract(tiempo).TotalSeconds > timeout

            If Not llego Then
                Cancelar()
                'puerto.Write(Paquete(CANCELAR_COMANDO))
                Return Errores.TimeOut
            End If
            Return 0
        Catch ex As Exception
            Return 2
        End Try
    End Function

    ''' <summary>
    ''' Envia petición al pinpad para ingreso de tarjeta sin esperar respuesta. 
    ''' Se usa con manejo del evento Y01Recibido. Para transacciones con cashback.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Pedir_Tarjeta_Cashback()
        Try
            tiempo = Now 'hora de envio del comando para controlar timeout.
            tm1.Start()
            puerto.DiscardInBuffer()
            puerto.Write(Paquete(LEER_TARJETA + "09"))
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub
    ''' <summary>
    ''' Envia Petición al pinpad para ingreso de tarjeta y espera respuesta. Compra + Cashback.
    ''' Respuestas 0=Recibido OK, 1=TIMEOUT, 2=Error
    ''' </summary>
    ''' <param name="timeout">Tiempo de espera máximo antes de cancelar la petición.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Pedir_Tarjeta_Cashback(ByVal timeout As Integer) As Integer
        Try
            tiempo = Now
            llego = False
            puerto.DiscardInBuffer()
            puerto.Write(Paquete(LEER_TARJETA + "09"))
            Do
                'espera respueta
            Loop Until llego Or Now.Subtract(tiempo).TotalSeconds > timeout

            If Not llego Then
                puerto.Write(Paquete(CANCELAR_COMANDO))
                Return 1
            End If
            Return 0
        Catch ex As Exception
            Return 2
        End Try
    End Function

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
    ''' <param name="pPMK">Posicion de la MasterKey</param>
    ''' <param name="pWK">Working Key</param>
    ''' <param name="pENC">Encripta</param>
    ''' <param name="pPTC">Solicita Tipo de cuenta</param>
    ''' <param name="pIMP">Importe, los ultimos 2 son los decimales, pasar sin coma y las 12 posiciones</param>
    ''' <param name="pICB">Importe Cashback, los ultimos 2 son los decimales, pasar sin coma y las 12 posiciones</param>
    Public Sub Pedir_Datos_Adicionales(ByVal pU4D As Boolean, ByVal pCDS As Boolean, ByVal pET1 As Boolean, ByVal pSPI As Boolean, ByVal pPMK As String, ByVal pWK As String, ByVal pENC As Boolean, ByVal pPTC As Boolean, ByVal pIMP As String, ByVal pICB As String)
        Try
            Dim comando As String = ""
            comando = DATOS_ADICIONALES
            comando += Convert.ToByte(pU4D).ToString
            comando += Convert.ToByte(pCDS).ToString
            comando += Convert.ToByte(pET1).ToString
            comando += Convert.ToByte(pSPI).ToString
            comando += Convert.ToByte(pPTC).ToString
            If pSPI Or pENC Then
                comando += pPMK
                comando += pWK
            Else
                comando += "N"
                comando += "N"
            End If
            comando += FS
            comando += Convert.ToByte(pENC).ToString
            comando += String.Format(pIMP, "000000000000")
            comando += String.Format(pICB, "000000000000")

            puerto.Write(Paquete(comando))
            'If pWKey = "" Then
            '    'puerto.Write(Paquete(DATOS_ADICIONALES + "00N1N" + FS + "N" + FS + "0" + "000000000000" + "000000000000"))
            '    puerto.Write(Paquete(DATOS_ADICIONALES + "11100NN" + FS + "0" + "000000012000" + "000000000000"))
            'Else
            '    puerto.Write(Paquete(DATOS_ADICIONALES + "10" + "0" + "1" + pWKey + FS + pWKey + FS + "0" + "000000000000" + "000000000000"))
            'End If

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Public Function Pedir_Datos_Adicionales(ByVal pTimeout As Integer, Optional ByVal pWKey As String = "") As Integer
        Try
            tiempo = Now
            llego = False
            If pWKey = "" Then
                puerto.Write(Paquete(DATOS_ADICIONALES + "11100NN" + FS + "0" + "000000012000" + "000000000000"))
            Else
                puerto.Write(Paquete(DATOS_ADICIONALES + "10" + "0" + "1" + pWKey + FS + pWKey + FS + "0" + "000000000000" + "000000000000"))
            End If
            Do
                'espera respueta
            Loop Until llego Or Now.Subtract(tiempo).TotalSeconds > pTimeout

            If Not llego Then
                puerto.Write(Paquete(CANCELAR_COMANDO))
                Return 1
            End If
            Return 0

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Function
#End Region

#Region "Comando Y03"
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

            puerto.Write(Paquete(RESPUESTA_EMISOR + pAut + pRespEmi + recibe + pCripto))
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub

    Public Function Datos_Respuesta_Emisor(ByVal pTimeOut As Integer, ByVal pAut As String, ByVal pRespEmi As String, ByVal pRecibe As Boolean, ByVal pCripto As String) As Integer
        Dim recibe As String
        Try
            'sacar******
            'pAut = "123456"
            'pCripto = _criptograma
            'pRespEmi = "00"
            'pRecibe = True
            '***********
            If pRecibe Then
                recibe = "1"
            Else
                recibe = "0"
            End If
            tiempo = Now
            llego = False
            puerto.Write(Paquete(RESPUESTA_EMISOR + pAut + pRespEmi + recibe + pCripto))

            Do
                'espera respueta
            Loop Until llego Or Now.Subtract(tiempo).TotalSeconds > pTimeOut

            If Not llego Then
                puerto.Write(Paquete(CANCELAR_COMANDO))
                Return 1
            End If
            Return 0

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Function


#End Region

#Region "Comando Y04"
    Public Sub Reingresar_pin(ByVal pWkey As String)
        Try
            puerto.Write(Paquete(REINGRESO_PIN + pWkey))

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try

    End Sub
#End Region

#Region "Comando Y06"
    Private Sub Controlar_TimeOut(ByVal sender As Object, ByVal e As ElapsedEventArgs) Handles tm1.Elapsed
        Try
            If Now.Subtract(tiempo).TotalSeconds > TIEMPOESPERA Then
                tm1.Stop()
                puerto.Write(Paquete(CANCELAR_COMANDO))
            End If
        Catch ex As Exception

        End Try
    End Sub

    Public Sub Cancelar()
        Try
            puerto.Write(Paquete(CANCELAR_COMANDO))
        Catch ex As Exception

        End Try
    End Sub
#End Region

#Region "Respuestas"
    Dim contesta As String
    Private Sub Recuperar_Respuesta(ByVal sender As Object, ByVal e As IO.Ports.SerialDataReceivedEventArgs) Handles puerto.DataReceived
        Dim cadena As String = ""
        'Dim bytes As Integer
        'Dim bufsize As Integer = 1024
        'Dim buffer(bufsize) As Byte
        'MsgBox("xxxxx")
        ''Threading.Thread.Sleep(650)
        'While True
        '    bytes = puerto.Read(buffer, 0, bufsize)
        '    cadena = cadena + hexastring(buffer)
        '    If bytes <= 0 Then Exit While
        'End While
        'MsgBox(cadena)
        Dim caracter As Char
        Dim terminar As Boolean = False
        Dim inicio As DateTime = Now
        cadena = ""
        Do
            caracter = Chr(puerto.ReadChar())
            cadena += caracter
            If Now.Subtract(inicio).Seconds > 5 Then terminar = True
        Loop Until caracter = ETX Or caracter = ACK Or caracter = NAK Or caracter = EOT Or terminar
        If caracter = ETX Then
            caracter = Chr(puerto.ReadChar())
            cadena += caracter
        End If

        'cadena = puerto.ReadExisting 

        If cadena = ACK Then
            resp = True
            contesta = ACK
            reenviar = False
            Informar("                      | <--- ACK")
        ElseIf cadena = NAK Then
            contesta = NAK
            Informar("                      | <--- NAK")
        ElseIf cadena = EOT Then
            contesta = EOT
            Informar("                      | <--- EOT")
        Else
            Dim control As String = ""
            Try
                control = Chr(Lrc(cadena.Substring(1, cadena.Length - 2)))
            Catch
                Informar("NO SE PUDO OBTENER LRC")
            End Try

            If control = cadena.Substring(cadena.Length - 1) Then 'Controla que el LRC este bien.
                tm1.Stop()
                Leer_Mensaje(cadena)
                llego = True
                Informar("                      | <--- " & cadena)

                resp = True
                contesta = ACK
                Enviar_ACK()
            Else
                Informar("ERROR DE TRANSMISION")
                nro_nak += 1
                If nro_nak = 4 Then
                    Enviar_EOT()
                    'Cancelar()
                Else
                    Enviar_NAK()
                End If

            End If
        End If
    End Sub


    Private Sub Leer_Mensaje(ByVal pCad As String)
        Dim CID As String = ""
        Dim cad As String = pCad.Substring(1, Len(pCad) - 3)
        Dim largo As Integer = 0

        Try
            If pCad.Substring(0, 1) = STX Then
                pCad = pCad.Remove(0, 1) 'SACO EL STX
            End If

            CID = pCad.Substring(0, 3) 'LEO EL ID DE COMANDO
            pCad = pCad.Remove(0, 3)
            If CID = "Y01" Then
                Limpiar_Variables()
                For Each s As Char In pCad
                    largo += 1
                    If s = FS Then Exit For
                    _PAN += s
                Next
                pCad = pCad.Remove(0, largo)
                _codServicio = CInt(Val(pCad.Substring(0, 3).Trim))
                pCad = pCad.Remove(0, 3)
                _codBanco = CInt(Val(pCad.Substring(0, 3).Trim))
                pCad = pCad.Remove(0, 3)
                largo = 0
                For Each s As Char In pCad
                    largo += 1
                    If s = FS Then Exit For
                    _nombre += s
                Next
                pCad = pCad.Remove(0, largo)
                _nroRegistro = CInt(Val(pCad.Substring(0, 6).Trim)) 'NRO DE REGISTRO
                pCad = pCad.Remove(0, 6)
                _modoIngreso = pCad.Substring(0, 1) 'MODO INGRESO
                pCad = pCad.Remove(0, 1)
                largo = 0
                For Each s As Char In pCad
                    largo += 1
                    If s = ETX Then Exit For
                    _versionSoft += s
                Next
                RaiseEvent Y01Recibido(cad)
            ElseIf CID = "Y02" Then
                Dim pal() As String = pCad.Split(FS)
                Select Case _modoIngreso
                    Case "M"

                    Case "C"

                    Case "B"

                        _vencimiento = pal(1).Substring(0, 4)

                        Try

                            'Dim venc = pal(3).Substring(1)

                            'Dim vencarray = Convert.ToBase64String(StringToByte(pal(3).Substring(1)))
                            Dim encoder As New UTF8Encoding
                            Dim rsa As New RSACryptoServiceProvider()
                            rsa.ImportParameters(RsaParam)

                            Dim cifr = Convert.ToBase64String(rsa.Encrypt(encoder.GetBytes("0648"), False))
                            Console.WriteLine(cifr)
                            Dim descifrado = rsa.Decrypt(Convert.FromBase64String(cifr), False)

                            Console.Write(Encoding.Default.GetString(descifrado))
                            'Dim descifrado = rsa.Decrypt(StringToByte(pal(3).Substring(1)), False)

                        Catch ex As CryptographicException
                            WriteLine(ex.Message)
                        Catch ex As ArgumentNullException
                            MsgBox("")

                        Catch ex As Exception
                            MsgBox("")
                        End Try
                        '_track1 = Encoding.Default.GetString(rsa.Decrypt(Encoding.Default.GetBytes(cifrado), False))
                        _track1 = pal(1).Substring(4)
                        _track2 = pal(2)
                        _trackNoleido = pal(3).Substring(0, 1)
                        _codSeguridad = pal(3).Substring(1)
                        _nroserie = pal(4)
                        _tipocuenta = pal(5).Substring(0, 1)
                        _pin = pal(5).Substring(1)



                        '                        _criptograma = pal(4)
                        '                        _autorizacion = pal(5).Substring(0, 6)
                        '                        _respEmisor = pal(5).Substring(6, 2)
                        '                        _secuenciaPan = pal(5).Substring(7, 3)
                        '                        _pin = pal(5).Substring(11)
                        '                        _nombre = pal(8).Substring(0, pal(8).Length - 2)

                End Select

                'Dim pal() As String = pCad.Split(FS)
                'If pal(0).Substring(0, 1) = "C" Then
                '    _vencimiento = pal(1).Substring(0, 4)
                '    _track1 = pal(1).Substring(4)
                '    _track2 = pal(2)
                '    _criptograma = pal(4)
                '    _autorizacion = pal(5).Substring(0, 6)
                '    _respEmisor = pal(5).Substring(6, 2)
                '    _secuenciaPan = pal(5).Substring(7, 3)
                '    _pin = pal(5).Substring(11)
                '    _nombre = pal(8).Substring(0, pal(8).Length - 2)
                'ElseIf pal(0).Substring(0, 1) = "B" Then
                '    _vencimiento = pal(1).Substring(0, 4)
                '    _track1 = pal(1).Substring(4)
                '    _track2 = pal(2)
                '    _pin = pal(5).Substring(41)
                '    _nombre = pal(6).Substring(0, pal(6).Length - 2)
                'ElseIf pal(0).Substring(0, 1) = "M" Then
                '    _vencimiento = pal(1).Substring(0, 4)
                '    _pin = pal(3).Substring(1, pal(3).Length - 2)
                'End If
                RaiseEvent Y02Recibido(cad)
            ElseIf CID = "Y03" Then

                _respuesta = pCad.Substring(6, 2)

                RaiseEvent Y03Recibido(cad)
            ElseIf CID = "Y0E" Then
                RaiseEvent Y0ERecibido(pCad.Substring(0, 3), CType(CInt(pCad.Substring(3, 2)), Errores).ToString, pCad.Substring(3, 2))
            ElseIf CID = "Y00" Then
                Dim pal() As String = pCad.Split(FS)
                _versionSO = pal(0)
                _versionSoft = pal(1)
                _versionKernel = pal(2).Substring(0, pal(2).Length - 2)
                RaiseEvent Y00Recibido(pCad)
            ElseIf CID = "71." Then
                _pin = pCad.Substring(5, 16)
                RaiseEvent PinRecibido()

            End If
        Catch ex As Exception
            RaiseEvent DatosRecibidos("Error leyendo stream")
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



#Region "PIN"
    Dim resp As Boolean

    Private Function Enviar_Comando(ByVal pTimeOut As Byte, Optional ByVal pComando As String = "") As Boolean
        puerto.DiscardInBuffer()
        contesta = ""
        Dim reintento As Byte = 1
        Dim time As Date = Now
        resp = False
        Do While reintento <= 3
            If pComando <> "" Then puerto.Write(pComando)
            Do
                'espera
            Loop Until Now.Subtract(time).TotalSeconds > pTimeOut Or contesta <> ""
            If contesta = ACK Then
                Return resp
            Else
                reintento += 1
            End If
        Loop
        Return resp

    End Function


    'Private Function Z1() As Boolean

    '    puerto.DiscardInBuffer()
    '    contesta = ""
    '    Dim reintento As Byte = 1
    '    Dim time As Date = Now
    '    resp = False
    '    Do While reintento <= 3
    '        puerto.Write(Paquete("Z1"))
    '        Do
    '            'espera
    '        Loop Until Now.Subtract(time).TotalSeconds > 2 Or contesta <> ""
    '        If contesta = ACK Then
    '            Return resp
    '        Else
    '            reintento += 1
    '        End If
    '    Loop
    '    Return resp
    'End Function
    'Private Function SI080() As Boolean

    '    puerto.DiscardInBuffer()
    '    contesta = ""
    '    Dim reintento As Byte = 1
    '    Dim time As Date = Now
    '    resp = False
    '    Do While reintento <= 3
    '        Console.WriteLine(SI + "080" + SO + Chr(Lrc("080" + SO)))
    '        puerto.Write(SI + "080" + SO + Chr(Lrc("080" + SO)))
    '        Do
    '            ' espera
    '        Loop Until Now.Subtract(time).TotalSeconds > 2 Or contesta <> ""
    '        If contesta = ACK Then
    '            Return resp
    '        Else
    '            reintento += 1
    '        End If
    '    Loop
    '    Return resp

    'End Function

    'Private Function C72() As Boolean

    '    puerto.DiscardInBuffer()
    '    contesta = ""
    '    Dim reintento As Byte = 1
    '    Dim time As Date = Now
    '    resp = False

    '    Do While reintento <= 3
    '        puerto.Write(Paquete("72"))
    '        Do
    '            ' espera
    '        Loop Until Now.Subtract(time).TotalSeconds > 2 Or contesta <> ""
    '        If contesta = EOT Then
    '            Return resp
    '        Else
    '            reintento += 1
    '        End If
    '    Loop
    '    Return resp

    'End Function

    ''' <summary>
    ''' Cancelación de pedido de pin.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Cancelar_Pin()
        Enviar_Comando(2, Paquete("72"))
    End Sub

    'Private Function Z62(ByVal pTarj As String, ByVal pWk As String) As Boolean

    '    puerto.DiscardInBuffer()
    '    contesta = ""
    '    Dim reintento As Byte = 1
    '    Dim time As Date = Now
    '    resp = False

    '    Do While reintento <= 3
    '        puerto.Write(Paquete("Z62." + pTarj + FS + pWk + "0409NINGRESE" + FS + "PIN" + FS + "PROCESANDO"))
    '        Do
    '            ' espera
    '        Loop Until Now.Subtract(time).TotalSeconds > 2 Or contesta <> ""
    '        If contesta = ACK Then
    '            Return resp
    '        Else
    '            reintento += 1
    '        End If

    '    Loop
    '    Return resp
    'End Function
    'Private Function C71() As Boolean

    '    contesta = ""
    '    Dim reintento As Byte = 1
    '    Dim time As Date = Now
    '    resp = False

    '    Do While reintento <= 3
    '        Do
    '            ' espera
    '        Loop Until Now.Subtract(time).TotalSeconds > 10 Or contesta <> ""
    '        If contesta = ACK Then
    '            Return resp
    '        Else
    '            reintento += 1
    '        End If
    '    Loop
    '    Return resp
    'End Function

    ''' <summary>
    ''' Pide Pin por pinpad y devuelve 0-Reintentar, 1-Pin OK, 2-Error.
    ''' El pin encriptado hay que pedirlo mediante la propiedad GetPinEncript.
    ''' </summary>
    ''' <param name="pTarj">PAN</param>
    ''' <param name="pWk">WorkingKey</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ObtenerPin(ByVal pTarj As String, ByVal pWk As String) As String

        Try
            Dim sigue As Boolean = True
            If pWk <> "" And pWk.Length <> 16 Then
                Return _ERROR
            End If
            If pWk = "" Then pWk = "1234123412341234"

            If sigue Then sigue = Enviar_Comando(2, Paquete("Z1"))
            If sigue Then sigue = Enviar_Comando(2, SI + "080" + SO + Chr(Lrc("080" + SO)))
            If sigue Then sigue = Enviar_Comando(2, Paquete("Z62." + pTarj + FS + pWk + "0409NINGRESE" + FS + "PIN" + FS + "PROCESANDO"))
            If sigue Then sigue = Enviar_Comando(10)

            'If sigue Then sigue = Z1()
            'If sigue Then sigue = SI080()
            'If sigue Then sigue = Z62(pTarj, pWk)
            'If sigue Then sigue = C71()

            If Not sigue Then
                Return _REINTENTAR
            Else
                Return _PINCORRECTO
            End If
            'puerto.Write(Paquete("71.004010000000000000000"))
        Catch ex As Exception
            Return _ERROR
        End Try

    End Function



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