Imports Trx.Messaging.FlowControl
Imports Trx.Messaging.Channels
Imports Trx.Messaging.Iso8583
Imports Trx.Messaging
Imports TjComun.IdaLib
Imports log4net
Imports System.Reflection
Imports Trx.Utilities

Public Module declaraciones
    Public versionISOLIB As String = "1"
    Public Const DemoraMaxima As Integer = 20
    Const versionsoft = "CR00"
    Property StringLog As String = ""
    'TODO: sacar despues de implementado
    Dim EMV As Boolean = False

    Public Sub Logger(mensaje As String)
        StringLog = StringLog & mensaje & vbNewLine

    End Sub

    Public Function Logger() As ILog
        Return LogManager.GetLogger(MethodBase.GetCurrentMethod.DeclaringType)
    End Function

    'Public Function Settings() As Object
    '    Return My.Settings
    'End Function

    Public Function Field39_PonerRespuesta(ByRef mensaje As Iso8583messagePOS, ByVal pCodResp As E_CodigosRespuesta) As String
        Dim CodResp As String = CInt(pCodResp).ToString("00")
        mensaje.Fields.Add(39, CodResp)
        Logger("        Respuesta " + CodResp)

        Return CodResp
    End Function

    Public Function Agregar7_FechaHora(ByRef mensaje As Iso8583messagePOS, fechahora As DateTime) As DateTime
        Dim transmissionDate As DateTime = fechahora

        mensaje.Fields.Add(E_Fields.Field7TransDateTime, String.Format("{0}{1}",
         String.Format("{0:00}{1:00}", transmissionDate.Month, transmissionDate.Day),
         String.Format("{0:00}{1:00}{2:00}", transmissionDate.Hour, transmissionDate.Minute, transmissionDate.Second)))

        Logger("        (7) Transmision Date:" + transmissionDate.ToString)

        Return transmissionDate
    End Function

    Public Function Agregar7_FechaHora(ByRef mensaje As Iso8583messagePOS) As DateTime
        Dim transmissionDate As DateTime = FechaActual()

        mensaje.Fields.Add(E_Fields.Field7TransDateTime, String.Format("{0}{1}",
         String.Format("{0:00}{1:00}", transmissionDate.Month, transmissionDate.Day),
         String.Format("{0:00}{1:00}{2:00}", transmissionDate.Hour, transmissionDate.Minute, transmissionDate.Second)))

        Logger("        (7) Transmision Date:" + transmissionDate.ToString)

        Return transmissionDate
    End Function

    'Public Function Field11_PonerTrace(ByRef mensaje As Iso8583messagePOS, ByRef Numerador As Trx.Messaging.VolatileStanSequencer) As Integer
    '    Dim s As String = Numerador.Increment().ToString()
    '    mensaje.Fields.Add(E_Fields.Field11Trace, s)

    '    Logger.Info("Numerador:" + s)
    '    Return Numerador.CurrentValue
    'End Function

    Public Function Field4_ObtenerMonto(ByRef mensaje As Iso8583messagePOS) As Decimal
        Dim _Monto As Decimal
        _Monto = CDec(mensaje.Fields(4).ToString) / 100
        Logger("        (4) MONTO:" + _Monto.ToString)
        Return _Monto
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
        Logger("        (17) FECHA NEGOCIO:" + _fechaNegocio.ToString)
        Return _fechaNegocio
    End Function

    Public Function Field12y13_ObteneHoraFecha(ByRef mensaje As Iso8583messagePOS) As Date
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
        Logger("        FECHA/HORA TRANS:" + _fechatran.ToString)
        Return _fechatran
    End Function

    Public Function ObtenerTcpChannel(ByVal pImplementacion As E_Implementaciones, Optional ByVal pHostName As String = "", Optional ByVal pport As Integer = 0, Optional ByVal tipo As String = "", Optional ByVal ciudad As String = "") As TcpChannel
        ' aca manejamos como vienen los mensajes ISO dentro de un paquete TCP/IP

        Dim pTcpChannel As TcpChannel
        Select Case pImplementacion

            Case Else
                ' y este es el resto.....
                pTcpChannel = New TwoBytesNboHeaderChannel(New Iso8583MessageFormatterPOS(pImplementacion))
        End Select
        pTcpChannel.Send3 = False

        pTcpChannel.InactivityInterval = 1800000


        Logger.Info(String.Format("Seteando conector para {0}", pImplementacion.ToString))
        ' Esto lo modifico Gustavo porque el sessionheader  tenia que ir en formato ascii.
        ' Lo cambio y lo volvio a poner como estaba. En visa tuvieron que cambiar algo porque sino no andaba.
        ' Chr(0) + Chr(1 * 16 + 2) --> (0018) es el largo del sessionheader 0012 en hexa.
        ' El sessionheader es por el saludo de 3 vias en tcp/ip. Yo les di el CUD C700000054023100 y ellos me dieron este nro "540231000000000100".
        ' CONECTOR PARA MIGRAR 541529000000000100 DESPUES HAY QUE VOLVERLO AL OTRO.
        Dim conector As String = ""
        If pImplementacion = E_Implementaciones.Visa Or pImplementacion = E_Implementaciones.VisaHomol Then
            If tipo = "Produccion" And My.Computer.Name <> "MARCOS-XP" And My.Computer.Name <> "ROBERTINO-P" And My.Computer.Name <> "NAHUEL-O" Then
                If ciudad = "Rafaela" Then
                    conector = "541529000000000100" 'CONECTOR PRODUCCION VISA RAFAELA
                ElseIf ciudad = "San Francisco" Then
                    conector = "540232000000000100" 'CONECTOR PRODUCCION VISA SAN FRANCISCO
                End If
                Logger.Info(String.Format("Conector de PRODUCCION VISA {0} seteado", conector))
            Else
                conector = "540231000000000100" 'CONECTOR PRUEBA VISA
                Logger.Warn(String.Format("Conector de HOMOLOGACION VISA {0} seteado.", conector))
            End If
            Dim previo = FrameworkEncoding.GetInstance().Encoding
            FrameworkEncoding.GetInstance().Encoding = System.Text.Encoding.ASCII
            pTcpChannel.SessionHeader = Chr(0) + Chr(1 * 16 + 2) + conector
            ' pTcpChannel.SessionHeader = Chr(0) + Chr(1 * 16 + 2) + "540231000000000100"
            FrameworkEncoding.GetInstance().Encoding = previo

        End If

        '//////////////////////////////////////////////////////////////////////////////////////////////////////////


        If pport <> 0 Then pTcpChannel.Port = pport
        If pHostName <> "" Then pTcpChannel.HostName = pHostName
        Logger.Info(String.Format("La dirección de conexión para {0} es {1}:{2}", pImplementacion.ToString, pHostName, pport))
        Return pTcpChannel
    End Function

    Public Function Agregar12y13_HoraFecha(ByRef mensaje As Iso8583messagePOS, ByVal FechaHoraNegocio As DateTime) As DateTime
        mensaje.Fields.Add(12, String.Format("{0}{1}{2}", FechaHoraNegocio.Hour.ToString("00"), FechaHoraNegocio.Minute.ToString("00"), FechaHoraNegocio.Second.ToString("00")))
        mensaje.Fields.Add(13, String.Format("{0}{1}", FechaHoraNegocio.Month.ToString("00"), FechaHoraNegocio.Day.ToString("00")))
        Logger(String.Format("       (12) Hora Trx: {0}{1}{2}", FechaHoraNegocio.Hour.ToString("00"), FechaHoraNegocio.Minute.ToString("00"), FechaHoraNegocio.Second.ToString("00")))
        Logger(String.Format("       (13) Fecha Trx: {0}{1}", FechaHoraNegocio.Month.ToString("00"), FechaHoraNegocio.Day.ToString("00")))
        Return FechaHoraNegocio
    End Function

    Public Sub Agregar14_expdate(ByVal Mensaje As Iso8583messagePOS, ByVal Expdate As Date)
        Mensaje.Fields.Add(14, Expdate.ToString("yyMM"))
        Logger("       (14) Fecha de venc.: " + TjComun.IdaLib.enmascarar(Expdate.ToString("yyMM"), 0, Expdate.ToString("yyMM").Length))
    End Sub
    Public Sub agregar22_modoIngreso3DES(ByVal mensaje As Iso8583messagePOS, ByVal ModoIngreso As E_ModoIngreso, host As Byte)
        Dim s As String = "000"
        'If maestro Then
        '    Select Case ModoIngreso
        '        Case E_ModoIngreso.Banda
        '            s = "021"
        '        Case E_ModoIngreso.Manual
        '            s = "011"
        '    End Select
        'Else
        If host = TipoHost.Posnet_homolog Or host = TipoHost.POSNET Then

            Select Case ModoIngreso
                Case E_ModoIngreso.Banda
                    s = "021" ' terminan en 1 porque tienen pp instalado.
                Case E_ModoIngreso.Manual
                    s = "011"
                Case E_ModoIngreso.Chip
                    s = "051"
                Case E_ModoIngreso.Contactless
                    s = "071"
                Case E_ModoIngreso.CL_Magstripe
                    s = "911"
            End Select
        ElseIf host = TipoHost.VISA Or host = TipoHost.Visa_homolog Then
            Select Case ModoIngreso
                Case E_ModoIngreso.Banda
                    s = "022" ' terminan en 1 porque tienen pp instalado.
                Case E_ModoIngreso.Manual
                    s = "012"
                Case E_ModoIngreso.Chip
                    s = "051"
            End Select

        End If

        'End If
        mensaje.Fields.Add(22, s)
        Logger("       (22) Modo de ingreso: " + s)
    End Sub
    Public Sub agregar22_modoIngreso(ByVal mensaje As Iso8583messagePOS, ByVal ModoIngreso As E_ModoIngreso, maestro As Boolean)
        Dim s As String = "000"
        If maestro Then
            Select Case ModoIngreso
                Case E_ModoIngreso.Banda
                    s = "021"
                Case E_ModoIngreso.Manual
                    s = "011"
            End Select
        Else
            Select Case ModoIngreso
                Case E_ModoIngreso.Banda
                    s = "022"
                Case E_ModoIngreso.Manual
                    s = "012"
                Case E_ModoIngreso.Chip
                    s = "051"
            End Select
        End If
        mensaje.Fields.Add(22, s)
        Logger("       (22) Modo de ingreso: " + s)
    End Sub
    Public Sub agregar23_cardsec(ByVal mensaje As Iso8583messagePOS, ByVal secuencia As String)
        mensaje.Fields.Add(23, secuencia)
        Logger("       (23) Card sequence number: " + secuencia)

    End Sub

    Public Function Field49_ObtenerMoneda(ByRef mensaje As Iso8583messagePOS) As E_Monedas
        Return CType(CInt(mensaje.Fields(49).ToString), E_Monedas)
    End Function

    ' obtiene el nro de tarjeta del campo 2 o de alguno de los tracks (campo 35,track 2), por si no viene en el campo2 (posnet)
    Public Function Field2or35_ObtenerTarjeta(ByRef mensaje As Iso8583messagePOS) As Int64
        Dim _tarjeta As Int64
        If mensaje.Fields.Contains(2) Then
            _tarjeta = CLng(mensaje.Fields(2).ToString)
            Logger("        (2) PAN:" + _tarjeta.ToString)
        ElseIf mensaje.Fields.Contains(35) Then
            ' Validar bien la posicion del PAN (nro de tarjeta) dentro del trakc 2
            _tarjeta = CLng(mensaje.Fields(35).ToString.Substring(3, 16))
            Logger("       (35) PAN:" + _tarjeta.ToString)
        End If
        'Logger.Info("  (2) PAN:" + _tarjeta.ToString)
        Return _tarjeta
    End Function

    Public Function FechaActual() As DateTime
        ' esta es la fecha que usamos para todos los mensajes
        Return DateTime.Now
    End Function


    '---------------------------------------
    Public Sub agregar2_tarjeta(echoMsg As Iso8583messagePOS, TARJ As String)
        Dim s As String = Trim(TARJ)
        echoMsg.Fields.Add(2, s)
        Logger(String.Format("        (2) Tarj: {0} ", TjComun.IdaLib.enmascarar(s, 6, s.Length - 4)))
    End Sub
    Public Sub agregar3_codProc(ByVal echomsg As Iso8583messagePOS, ByVal Codigo As Integer)
        Dim s As String = Int(Codigo).ToString("000000")
        echomsg.Fields.Add(3, s)
        Logger(String.Format("        (3) Cod.Proc: {0} a {1}", Codigo, s))
    End Sub
    Public Sub agregar4_Monto(ByVal echomsg As Iso8583messagePOS, ByVal Monto As Double)
        Dim s As String = (Monto * 100).ToString("000000000000")
        echomsg.Fields.Add(4, s)
        Logger(String.Format("        (4) Monto: {0} a {1}", Monto, s))
    End Sub
    Public Sub agregar11_trace(ByVal echomsg As Iso8583messagePOS, ByVal trace As Integer)
        echomsg.Fields.Add(11, trace.ToString("000000"))
        Logger(String.Format("       (11) Nro. Trace: {0} ", trace.ToString("000000")))
    End Sub
    Public Sub agregar17_CaptureDate(ByVal echomsg As Iso8583messagePOS, ByVal Capdate As Date)
        'Dim s As String = Capdate.ToString("MMdd")
        'echomsg.Fields.Add(17, s)
        'Logger("       (17) Capture Date: " + s)

        echomsg.Fields.Add(17, Now.Date.ToString("MMdd"))
        Logger("       (17) Capture Date: " + Now.Date.ToString("MMdd"))
    End Sub
    Public Sub agregar24_IDRed(echoMsg As Iso8583messagePOS, nrohost As Byte)
        Select Case nrohost
            Case TipoHost.Visa
                echoMsg.Fields.Add(24, "112")
                Logger("       (24) ID RED: 112 VISA")
            Case TipoHost.POSNET
                echoMsg.Fields.Add(24, "003") 'Prod
                'echoMsg.Fields.Add(24, "009")
                Logger("       (24) ID RED: 003 POSNET")
            Case TipoHost.Diners
                Logger(String.Format("        Host no implementado: {0}", nrohost))
            Case TipoHost.Comfiar
                Logger(String.Format("        Host no implementado: {0}", nrohost))
            Case TipoHost.Posnet_homolog
                echoMsg.Fields.Add(24, "009")
                Logger("       (24) ID RED: 009 POSNET HOMOLOG")
            Case TipoHost.Visa_homolog
                echoMsg.Fields.Add(24, "112")
                Logger("       (24) ID RED: 112 VISA HOMOLOG")
            Case Else
                Logger(String.Format("Host desconocido: {0}", nrohost))
        End Select
        'Logger.Info(String.Format(" (24) Host: {0}", nrohost))
    End Sub
    Public Sub agregar25_condicionlapos(echoMsg As Iso8583messagePOS)
        echoMsg.Fields.Add(25, "00")
        Logger(String.Format("       (25) Condicion de la POS: 00"))
    End Sub
    'Public Sub agregar32(ByVal echomsg As Iso8583messagePOS)
    '    '32 LLVAR ..R9(11) 32	acq-inst-id	LLVAR	..9(11)
    '    Dim s As String = "12345678901"
    '    echomsg.Fields.Add(32, s)
    '    srv.MsgInfo(String.Format(" (32) acq-inst-id: {0}", s))
    'End Sub
    Public Sub agregar35_Track2(ByVal echomsg As Iso8583messagePOS, track As String) ' ByVal NroTarjeta As Int64, ByVal Vto As String)
        Try
            echomsg.Fields.Add(35, track)
            Logger(String.Format("       (35) Track II: {0}={1}", TjComun.IdaLib.enmascarar(track.Split("=")(0), 6, track.Split("=")(0).Length - 4),
                                                                 TjComun.IdaLib.enmascarar(track.Split("=")(1), 0, track.Split("=")(1).Length)))
        Catch ex As Exception
            Logger.Error("Track 2 mal formado. Verifique.")

        End Try
    End Sub
    Dim Numerador As VolatileStanSequencer
    Public Sub agregar37_RetrefNumber(ByVal echomsg As Iso8583messagePOS, pRetRef As String)
        '37X(12) 37	retrvl-ref-num		g X(12)
        'Dim s As String
        's = (Numerador.CurrentValue).ToString("000000")
        echomsg.Fields.Add(37, pRetRef.ToString())
        Logger("       (37) RET REF N: " + pRetRef.ToString())
    End Sub
    Public Sub agregar38_codautorizacion(echoMsg As Iso8583messagePOS, codaut As String)
        echoMsg.Fields.Add(38, codaut)
        Logger("       (38) Cod Autorizacion: " + codaut)
    End Sub
    Public Sub agregar39_CodigoRespuesta(ByVal echomsg As Iso8583messagePOS, ByVal CodResp As E_CodigoREspuestaReversos)
        Dim s As String = CInt(CodResp).ToString("00")
        echomsg.Fields.Add(39, s)
        Logger(String.Format("       (39) Monto: {0} a {1}", CodResp, s))
    End Sub
    Public Sub agregar41_IdTerminal(ByVal echomsg As Iso8583messagePOS, idterm As String)
        echomsg.Fields.Add(41, Trim(idterm))
        Logger(String.Format("       (41) Id.Terminal: {0}", Trim(idterm)))
    End Sub
    Public Sub agregar42_IdComercio(ByVal echomsg As Iso8583messagePOS, idcom As String)
        echomsg.Fields.Add(42, idcom)
        Logger(String.Format("       (42) Id.del Comercio: {0}", idcom))
    End Sub
    Public Sub agregar43_atmloc(ByVal echomsg As Iso8583messagePOS)
        '43
        'Campo 43	El formato es:
        'Posiciones 01-22  =  Nombre de la  Institución dueña del ATM.
        Dim s As String = "1234567890123456789012"
        'Posiciones 23-35  =  Localidad  donde  se  encuentra ubicado el ATM.
        s += "RAFAELA      "
        'Posiciones 36-38  =  Código  de  Provincia  donde se encuentra ubicado el ATM.
        s += "222"
        'Posiciones 39-40  =  Código de País donde se encuentra ubicado el ATM.
        s += "33"
        Debug.Assert(s.Length = 40)
        echomsg.Fields.Add(43, s)
        Logger("        TERM (43): " + s)
    End Sub
    Public Sub agregar45_track1(echoMsg As Iso8583messagePOS, TRACK1 As String)
        Dim s As String
        'echoMsg.Fields.Add(45, RTrim(req.ida.TRACK1))
        s = String.Format(Mid(TRACK1, 1, 76))
        echoMsg.Fields.Add(45, s)
        Logger(String.Format("       (45) Track I: {0}^{1}^{2}?", TjComun.IdaLib.enmascarar(s.Split("^")(0), 6, s.Split("^")(0).Length - 4),
                                                                 s.Split("^")(1),
                                                                 TjComun.IdaLib.enmascarar(s.Split("^")(2), 0, s.Split("^")(2).Length)))
    End Sub



    Public Sub agregar46_Track1NoLeido(echoMsg As Iso8583messagePOS, p2 As String)
        echoMsg.Fields.Add(46, p2)
        Logger("       (46) Track I no leido: " + p2)
    End Sub



    Public Sub agregar47_BloqueEncriptado(echoMsg As Iso8583messagePOS, paquete As String, hst As TipoHost)
        Dim s As String
        If hst = TipoHost.VISA Or hst = TipoHost.Visa_homolog Then
            s = "1"
        Else
            s = "2"
        End If


        Dim NumberChars As Integer = paquete.Length
        Dim bytes((NumberChars / 2) - 1) As Byte
        Dim i As Integer = 0
        Try
            Do While i < NumberChars
                bytes(i / 2) = Convert.ToByte(Convert.ToInt32(paquete.Substring(i, 2), 16))
                i += 2
            Loop
        Catch ex As Exception
        End Try


        For Each b As Byte In bytes
            s += ChrW(b)
        Next
        's += paquete

        echoMsg.Fields.Add(47, s)
        Logger("       (47) Paquete de datos encriptados: " + s)
    End Sub

    Public Sub agregar48_cuotas(ByVal echomsg As Iso8583messagePOS, cuotas As String, Optional pTick As Integer = 0, Optional pFecOri As String = "")
        Dim s As String
        s = Mid(cuotas, 1, 3)
        If pTick <> 0 Then s += pTick.ToString("0000")
        If pFecOri <> "" Then s += pFecOri 'ddmmyy

        'echomsg.Fields.Add(48, cuotas.ToString("000"))
        echomsg.Fields.Add(48, s)
        Logger("       (48) Cuotas/Datos Ori: " + s)
    End Sub
    Public Sub agregar49_moneda(ByVal echomsg As Iso8583messagePOS, Optional ByVal pmoneda As E_Monedas = E_Monedas.PESOS)
        Dim s As String
        s = Int(pmoneda).ToString("000")
        echomsg.Fields.Add(49, s)
        Logger("       (49) MONEDA: " + s)
    End Sub
    Public Sub agregar52_Pinblock(ByVal echomsg As Iso8583messagePOS, pinblock As String)
        'TODO: Controlar error, ver que pasa si viene vacio.
        'If pinblock = "" Then
        '    Dim pin = ""
        '    Dim NumberChars As Integer = pin.Length
        '    Dim bytes((NumberChars / 2) - 1) As Byte
        '    Dim i As Integer = 0
        '    Do While i < NumberChars
        '        bytes(i / 2) = Convert.ToByte(Convert.ToInt32(pin.Substring(i, 2), 16))
        '        i += 2
        '    Loop
        '    echomsg.Fields.Add(52, bytes)
        '    Logger("       (52) PINBLOCK: ")
        'Else
        Dim pin = pinblock.Substring(0, 16)
        Dim NumberChars As Integer = pin.Length
        Dim bytes((NumberChars / 2) - 1) As Byte
        Dim i As Integer = 0
        Do While i < NumberChars
            bytes(i / 2) = Convert.ToByte(Convert.ToInt32(pin.Substring(i, 2), 16))
            i += 2
        Loop
        echomsg.Fields.Add(52, bytes)
        Logger("       (52) PINBLOCK: " + TjComun.IdaLib.enmascarar(pinblock.Substring(0, 16), 0, pinblock.Substring(0, 16).Length))
        'End If
    End Sub
    Public Sub agregar54_Cashback(echomsg As Iso8583messagePOS, importe As Double)
        Dim s As String = (importe * 100).ToString("000000000000")
        echomsg.Fields.Add(54, s)
        Logger("       (54) Importe cashback: " + importe.ToString)
    End Sub
    Public Sub agregar55_CodSeg(echoMsg As Iso8583messagePOS, codseg As String)
        echoMsg.Fields.Add(55, codseg)
        Logger("       (55) Cod. Seguridad: " + TjComun.IdaLib.enmascarar(codseg, 0, codseg.Length))
    End Sub

    Public Sub agregar55_CodSeg3DES(echoMsg As Iso8583messagePOS, codseg As String)
        echoMsg.Fields.Add(55, codseg)
        Logger("       (55) Criptograma: " + codseg)
    End Sub
    Public Sub agregar19_codigopais(echoMsg As Iso8583messagePOS)
        Dim s As String = "032"

        echoMsg.Fields.Add(19, s)
        Logger("       (19) Codigo de pais: " + s)


    End Sub
    Public Sub agregar59_InfAdicional3DES(echoMsg As Iso8583messagePOS, idEncripcion As String, nrohost As Integer, fallback As Boolean, dpe As String, Optional versoft As String = "")
        Dim s As String = ""
        If nrohost = TipoHost.POSNET Or nrohost = TipoHost.Posnet_homolog Then
            s = "002"
            s += "0001"
            s += "001"
            s += "009"
            s += "1"

            s += "021"
            s += "0001"
            s += "004"
            s += "070"
            s += "3" '5=chip, 3=contactless

            If dpe <> "" Then
                s += "023"
                s += "0001"
                s += (dpe.Length + 3).ToString("000") '"005 a 070"
                s += "001"
                s += dpe
            End If


            s += "028"
            s += "0001"
            s += "001"
            s += "078"
            s += "2"

            s += "000"
            s += "0001"
            s += "016"
            s += "000"
            s += idEncripcion

            If fallback Then
                s += "022"
                s += "0001"
                s += "004"
                s += "071"
                s += "F"

            End If

        ElseIf nrohost = TipoHost.VISA Or nrohost = TipoHost.Visa_homolog Then
            s = "000"
            s += "0001"
            s += "019"
            s += "000"
            s += "900443467FFFFFFF"

            s += "039"
            s += "0001"
            s += "023"
            s += "113"
            s += "900443467FFFFFFFFFFF"

            s += "021"
            s += "0001"
            s += "004"
            s += "070"
            s += "5"

            s += "060"
            s += "0001"
            s += "018"
            s += "153"
            s += versoft.PadRight(15, " ")
        Else
            Logger.Error("Host desconocido")

            's += "000"
            's += "0001"
            's += "016"
            's += "000"
            's += idEncripcion

        End If
        echoMsg.Fields.Add(59, s)
        Logger("       (59) Inf.Adicional: " + s)
    End Sub




    Public Sub agregar59_InfAdicional(echoMsg As Iso8583messagePOS)
        Dim s As String = ""

        s = "002"
        s += "0001"
        s += "001"
        s += "009"
        s += "1"

        echoMsg.Fields.Add(59, s)
        Logger("       (59) Inf.Adicional: " + s)
    End Sub

    Public Sub agregar60_VersionSoft(echoMsg As Iso8583messagePOS, Optional verapp As String = "")
        Dim s As String = versionsoft

        'TODO: ver para chip
        If verapp <> "" Then
            s = s.PadRight(10)
            s += verapp
        End If


        echoMsg.Fields.Add(60, s)

        Logger("       (60) Version Soft: " + s)
    End Sub
    Public Sub agregar62_NroTicket(echoMsg As Iso8583messagePOS, nroticket As Integer)
        Dim s As String = CInt(nroticket).ToString("0000")

        echoMsg.Fields.Add(62, s)
        Logger("       (62) Nro.Ticket: " + s)
    End Sub
    Public Sub agregar62_IDEncripcion(echoMsg As Iso8583messagePOS, id_encripcion As String)
        Dim s As String = id_encripcion

        echoMsg.Fields.Add(62, s)
        Logger("       (62) ID de encripción: " + s)
    End Sub
    Public Sub agregar63_Mensaje(echoMsg As Iso8583messagePOS, p2 As String)
        echoMsg.Fields.Add(63, p2)
        Logger("       (63) Mensaje para mostrar" + p2)
    End Sub

    'Public Sub agregar70(echoMsg As Iso8583messagePOS)
    '    Dim s As String = 302.ToString("000")
    '    echoMsg.Fields.Add(70, s)
    '    Logger.Info(" (70): " + s)
    'End Sub
    Public Sub agregar95_MontoREversado(ByVal echomsg As Iso8583messagePOS, ByVal Monto As Double)
        Dim s As String = (Monto * 100).ToString("000000000000")
        echomsg.Fields.Add(95, s.PadRight(42, "0"c))
        Logger(String.Format("       (95) Monto: {0} a {1}", Monto, s))
        '* el campo 95 se envía solamente cuando la transacción es reversada parcialmente.
        'Para mensajes de reverso (0420/0421) parcial (P-039: “32”) de extracción (código transacción: “01”):
        '1-12 Monto por el que realmente se completó la transacción en la moneda de la operación (P-049) (2 decimales).
        '13-42: Sin uso.
    End Sub

    Sub agregar15_FechaCierre(echoMsg As Iso8583messagePOS)
        echoMsg.Fields.Add(15, Now.Date.ToString("MMdd"))

        Logger("       (15) Fecha Cierre:" + Now.Date.ToString("MMdd"))
    End Sub

    Sub agregar63_InfoCierre(echoMsg As Iso8583messagePOS, plote As Integer, pcancom As Integer, pmoncom As Double,
                             pcandev As Integer, pmondev As Double, pcananu As Integer, pmonanu As Double)
        Dim s As String = plote.ToString("000")
        s = s + pcancom.ToString("0000")
        s = s + (pmoncom * 100).ToString("000000000000")
        s = s + pcandev.ToString("0000")
        s = s + (pmondev * 100).ToString("000000000000")
        s = s + pcananu.ToString("0000")
        s = s + (pmonanu * 100).ToString("000000000000")
        echoMsg.Fields.Add(63, s)
        Logger("       (63) Informacion de Cierre: " + s)
    End Sub



End Module
