Imports Trx.Messaging
Imports Trx.Messaging.Iso8583
Imports Trx.Messaging.Channels
Imports Trx.Messaging.FlowControl
Imports ISOLib

Public Class Frm_ecotester
    '  Dim Conexion As TarjetasAcceso.Acceso
    Dim CmbTerm As String = "ECOTEST"
    Dim Log As Logeador
    Dim WithEvents ContestadorPosnetComercio As ClientPeer
    Dim Numerador As VolatileStanSequencer
    Dim PosnetComercioSend As Integer
    Dim PosnetComercioRec As Integer
    Dim PosnetComercioNoCon As Integer
    Dim pconectado As Boolean
    Dim pconectado2 As Boolean
    Public PosnetComercioResp As String
    Dim test As Boolean
    Dim timeOut As Integer = My.Settings.TimeOut
    Dim WithEvents TimerREspuesta As New System.Timers.Timer
    Public Sub Cerrar()
        Log.logInfo("cerrando ")
        Desconectar()
        ' If Conexion IsNot Nothing Then Conexion.Dispose()
        Numerador = Nothing
        '  Conexion = Nothing
        Log = Nothing
    End Sub

    Public Sub Conectar()

        Dim Nombre As String
 
        ContestadorPosnetComercio = New ClientPeer(Nombre, _
                        ObtenerTcpChannel(E_Implementaciones.PosnetComercio, My.Settings.ipPosnetComercio, My.Settings.PortPosnetComercio), _
                        New POSMessagesIdentifier())


        ContestadorPosnetComercio.Connect()

        Nombre = "PC2-" + My.Computer.Name + "-" + My.User.Name
        

    End Sub
    Public Sub Desconectar()
        
        If ContestadorPosnetComercio.IsConnected Then
            ContestadorPosnetComercio.Close()
        End If
     
    End Sub
    


    Public Sub COnectarDB()
        'Conexion = New TarjetasAcceso.Acceso("ECO", "TES")
        'Conexion.trace = True
        ' Log = Conexion.Logueador
        Log = New Logeador
        Log.AddMemoryAppender()

    End Sub

    Private Sub Frm_IsoTester_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        Cerrar()
    End Sub

    Private Sub Frm_IsoTester_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Numerador = New VolatileStanSequencer()
        'Log = New Logeador.Logeador
        Conectar()
        If My.Settings.Intervalo > 3 Then
            Log.logInfo("Intervalo:" + My.Settings.Intervalo.ToString)
            Timer2.Interval = My.Settings.Intervalo * 1000
            Timer2.Enabled = True
            Me.Width = 800
        Else
            Timer2.Enabled = False
        End If
        Timer1.Enabled = True
        PropertyGrid1.SelectedObject = My.Settings
        Show2()
    End Sub
    Public Sub Show2()
       
 
 

        Label16.Text = PosnetComercioResp


    

        Label19.Text = PosnetComercioNoCon.ToString
        Label18.Text = PosnetComercioSend.ToString
        Label17.Text = PosnetComercioRec.ToString

  

    End Sub
    Public Function MensajeDesdeString(ByVal s As String, ByVal Implementacion As E_Implementaciones) As Iso8583messagePOS

        Dim formatter As Trx.Messaging.Iso8583.Iso8583MessageFormatter

        formatter = New Iso8583POS(Implementacion)
        Dim context As New Trx.Messaging.ParserContext(ParserContext.DefaultBufferSize)
        Dim m As Trx.Messaging.Message

        context.Write(s)
        With formatter
            m = .Parse(context)
        End With

        Dim Mn As New Iso8583messagePOS(CType(m, Iso8583Message))

        Mn.Implementacion = Implementacion
        Return Mn
    End Function

    Public Function Generar0800(ByVal Implementacion As E_Implementaciones, Optional ByVal CodigoEcho As Integer = 301) As Iso8583messagePOS

        ' Build echo test message.
        Log.log("Enviando 0800")
        Dim echoMsg As Iso8583messagePOS = New Iso8583messagePOS(800, Implementacion)

        echoMsg.Fields.Add(3, "990000")

        Agregar7_FechaHora(echoMsg)
        agregar11_trace(echoMsg)

        echoMsg.Fields.Add(24, "101")
        agregar41(echoMsg)
        agregar42(echoMsg)
        echoMsg.Fields.Add(70, 302.ToString("000"))

        Return echoMsg
    End Function
    Public Sub agregar32(ByVal echomsg As Iso8583messagePOS)
        '32 LLVAR ..R9(11) 32	acq-inst-id	LLVAR	..9(11)
        Dim s As String

        s = "12345678901"

        echomsg.Fields.Add(32, s)
        Log.logInfo(" acq-inst-id 32 :" + s)
    End Sub
    Public Sub agregar35(ByVal echomsg As Iso8583messagePOS, ByVal NroTarjeta As Int64, ByVal Vto As String)
        '35
        '            El formato es:
        'Posiciones 01-02  =  Indicador de longitud. El valor a informar es `37`.
        'Posiciones 03-39  =  Valor del campo.
        Dim s As String

        s = "37"
        s += "^" + NroTarjeta.ToString + "=" + Vto
        echomsg.Fields.Add(35, s)
        Log.logInfo(" track 35) :" + s)
    End Sub
    Public Sub AGREGAR17_CAPTUREDATE(ByVal echomsg As Iso8583messagePOS, ByVal Capdate As Date)
        '17 mmdd 9(4) 17	cap-dat	mmdd	9(4)
        Dim s As String
        s = Capdate.ToString("MMdd")

        echomsg.Fields.Add(17, s)
        Log.logInfo("Capture Date 17) :" + s)
    End Sub
    Public Sub agregar37_RETREFNUMBER(ByVal echomsg As Iso8583messagePOS)
        '37X(12) 37	retrvl-ref-num		g X(12)
        Dim s As String
        s = (Numerador.CurrentValue).ToString("000000")

        echomsg.Fields.Add(37, s)
        Log.logInfo(" RET REF N 37) :" + s)
    End Sub
    Public Sub AGREGAR43_atmloc(ByVal echomsg As Iso8583messagePOS)
        '43
        'Campo 43	El formato es:
        'Posiciones 01-22  =  Nombre de la  Institución dueña del ATM.
        Dim s As String
        s = "1234567890123456789012"
        'Posiciones 23-35  =  Localidad  donde  se  encuentra ubicado el ATM.
        s += "RAFAELA      "
        'Posiciones 36-38  =  Código  de  Provincia  donde se encuentra ubicado el ATM.
        s += "222"
        'Posiciones 39-40  =  Código de País donde se encuentra ubicado el ATM.
        s += "33"
        Debug.Assert(s.Length = 40)
        echomsg.Fields.Add(43, s)
        Log.logInfo(" TERM 43) :" + s)
    End Sub
    Public Sub agregar48_term(ByVal echomsg As Iso8583messagePOS)
        Dim s As String
        s = "123456789012345678901234"
        s += "1"
        s += "22"
        s += "333"
        s += "444"
        s += "12345678901"
        echomsg.Fields.Add(48, s)
        Log.logInfo(" TERM 48) :" + s)

    End Sub
    Public Sub agregar49_moneda(ByVal echomsg As Iso8583messagePOS, Optional ByVal pmoneda As E_Monedas = E_Monedas.PESOS)
        Dim s As String
        s = Int(pmoneda).ToString("000")
        echomsg.Fields.Add(49, s)
        Log.logInfo(" MONEDA 49) :" + s)
    End Sub
    Public Sub agregar52_Pinblock(ByVal echomsg As Iso8583messagePOS)
        Dim s As String = "0123456789012345"
        echomsg.Fields.Add(52, s)
        Log.logInfo(" PINBLOCK (52) :" + s)
    End Sub
    Public Sub agregar60_ATM(ByVal echomsg As Iso8583messagePOS)

        Dim s As String = "012" + "0926" + "8901" + "2345"
        echomsg.Fields.Add(60, StrToByteArray(s))
        Log.logInfo(" ATM (60) :" + s)
    End Sub
    Public Shared Function StrToByteArray(ByVal str As String) As Byte()
        Dim encoding As New System.Text.ASCIIEncoding()
        Return encoding.GetBytes(str)
    End Function 'StrToByteArray
    Public Sub agregar3_codProc(ByVal echomsg As Iso8583messagePOS, ByVal Codigo As E_ProcCode)
        Dim s As String = Int(Codigo).ToString("000000")
        echomsg.Fields.Add(3, s)
        Log.logInfo(" Cod.Proc :" + Codigo.ToString + " a " + s)
    End Sub
    Public Sub agregar4_Monto(ByVal echomsg As Iso8583messagePOS, ByVal Monto As Double)
        Dim s As String = (Monto * 100).ToString("000000000000")
        echomsg.Fields.Add(4, s)
        Log.logInfo(" Monto (4):" + Monto.ToString + " a " + s)
    End Sub
    Public Sub agregar95_MontoREversado(ByVal echomsg As Iso8583messagePOS, ByVal Monto As Double)
        Dim s As String = (Monto * 100).ToString("000000000000")
        echomsg.Fields.Add(95, s.PadRight(42, "0"c))
        Log.logInfo(" Monto (95):" + Monto.ToString + " a " + s)
        '* el campo 95 se envía solamente cuando la transacción es reversada parcialmente.
        'Para mensajes de reverso (0420/0421) parcial (P-039: “32”) de extracción (código transacción: “01”):
        '1-12 Monto por el que realmente se completó la transacción en la moneda de la operación (P-049) (2 decimales).
        '13-42: Sin uso.

    End Sub
    Public Sub agregar39_CodigoRespuesta(ByVal echomsg As Iso8583messagePOS, ByVal CodResp As E_CodigoREspuestaReversos)
        Dim s As String = CInt(CodResp).ToString("00")
        echomsg.Fields.Add(39, s)
        Log.logInfo(" Monto (39):" + CodResp.ToString + " a " + s)

    End Sub
    Public Sub agregar11_trace(ByVal echomsg As Iso8583messagePOS, Optional ByVal Adelantar As Boolean = True)

        If Adelantar Then
            echomsg.Fields.Add(11, Numerador.Increment().ToString("000000"))
        Else
            echomsg.Fields.Add(11, (Numerador.CurrentValue - 1).ToString("000000"))

        End If
    End Sub
    Public Sub agregar41(ByVal echomsg As Iso8583messagePOS)
        echomsg.Fields.Add(41, CmbTerm)
    End Sub
    Public Sub agregar42(ByVal echomsg As Iso8583messagePOS)
        echomsg.Fields.Add(42, CmbTerm)
    End Sub
    Public Function ObtenerTxtRepuesta(ByVal s As String) As String
        Dim cr As E_CodigosRespuesta
        Try
            cr = CType(CInt(s), E_CodigosRespuesta)
            Return cr.ToString
        Catch ex As Exception
            Return "Cod.Respuesta No Contemplado"
        End Try

    End Function

    Public Sub MandarMensajeyEsperar(ByVal msg As Iso8583messagePOS)
        Log.log("Enviando " + msg.ToString)

        

        Dim m As Iso8583messagePOS
        Dim request As RequestPOS

        request = New RequestPOS(ContestadorImplementado, msg)
        request.Send()
        request.WaitResponse(timeOut)
        m = request.ResponseMessage
        ProcessMessageIN(m, request.ResponseDateTime.Subtract(request.RequestDateTime).TotalSeconds)


    End Sub
    Public Sub ProcessMessageIN(ByVal m As Iso8583messagePOS, ByVal TiempoDeRespuesta As Double)
        If m Is Nothing Then

            EdtRespuesta.Text = " No llego nada"
        Else
            ' ver codigo respuesta
            Dim cr As String = m.Fields(39).ToString
            'tiempo de respuesta
            Log.logInfo("tiempo de repuesta:" + TiempoDeRespuesta.ToString)
            
            'procesar la respuesta
            EdtRespuesta.Text = cr + " " + ObtenerTxtRepuesta(cr)
            ' procesar los campos especificeos
           

        End If
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If Not Log Is Nothing Then Log.GetArrayAndClear(ListBox1)
        Show2()
    End Sub


    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        ListBox1.Items.Clear()
    End Sub



    Public Sub probar()
    
        Static PosnetComercioNoConUlt As Integer
      

        TimerREspuesta.Interval = 10000
        TimerREspuesta.Start()
      

        

        If ContestadorPosnetComercio.IsConnected Then
            Dim request2 As RequestPOS
            request2 = New RequestPOS(ContestadorPosnetComercio, Generar0800(E_Implementaciones.PosnetComercio))
            request2.Send()
            PosnetComercioSend += 1
        Else
            PosnetComercioNoCon += 1
            If PosnetComercioNoCon > PosnetComercioNoConUlt Then
                PosnetComercioResp = Respuesta("02", E_Implementaciones.PosnetComercio)
                PosnetComercioNoConUlt = PosnetComercioNoCon
            End If
        End If

       
         

    End Sub

    Private Sub Timer2_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Timer2.Tick
        probar()

    End Sub
 

    Public Function Respuesta(ByVal s As String, ByVal i As E_Implementaciones) As String
        Dim r As String
        Select Case s
            Case "00"
                r = "EN LINEA"
            Case "02"
                r = "Serv.Local Caido"
            Case "03"
                r = "Serv.Local No Resp."
            Case "90"
                r = "Sin conexion con Host"
            Case Else
                r = "ERROR " + s
        End Select
        If s > "00" Or test Then
            Avisar(i, r)
            test = False
        End If
        Return r
    End Function
    Public Sub Avisar(ByVal i As E_Implementaciones, ByVal Estado As String)
        Dim Destino As String = My.Settings.Email
        If Destino.Contains("@") Then
            Log.logInfo("Avisando a " + Destino)
            Try
                Dim s As New System.Net.Mail.SmtpClient(My.Settings.Servidor)
                Dim b As String = "ERROR " + Estado + " En " + i.ToString
                b += vbCrLf + "Fecha  :" + Now.ToString
                b += vbCrLf + "Origen :" + My.Computer.Name
                b += vbCrLf + "Usuario:" + My.User.Name



                s.Send(My.Settings.from, Destino, "EcoTester: Error " + Estado + " en " + i.ToString, b)

            Catch ex As Exception

            End Try

        End If
    End Sub
    

   




    Private Sub TimerREspuesta_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles TimerREspuesta.Elapsed
        ' con esto vemos si no responde...
        TimerREspuesta.Stop()
    
        If PosnetComercioSend > PosnetComercioRec + 1 Then
            PosnetComercioResp = Respuesta("03", E_Implementaciones.PosnetComercio)
        End If
    

    End Sub

    Private Sub Button1_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        probar()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        test = True
    End Sub

    Private Sub ContestadorPosnetComercio_RequestDone(ByVal sender As Object, ByVal e As Trx.Messaging.FlowControl.PeerRequestDoneEventArgs) Handles ContestadorPosnetComercio.RequestDone
        PosnetComercioRec += 1
        PosnetComercioResp = Respuesta(e.Request.ResponseMessage.Fields(39).Value.ToString, E_Implementaciones.PosnetComercio)
    End Sub

    Private Function ContestadorImplementado() As ClientPeer
        Throw New NotImplementedException
    End Function

 
End Class





