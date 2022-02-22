Imports Trx.Messaging
Imports Trx.Messaging.Iso8583
Imports Trx.Messaging.Channels
Imports Trx.Messaging.FlowControl

Imports ONLINECOM

Public Class Form1
    ' Dim Conexion As TarjetasAcceso.Acceso

    Dim Log As Logeador.Logeador
    Dim WithEvents ContestadorImplementado As ClientPeer
    Public WithEvents EscuchadorImplementado As Escuchador

    Dim Numerador As VolatileStanSequencer
    Dim Port As Integer = My.Settings.PortNumber
    Dim pconectado As Boolean
    Dim pconectado2 As Boolean
    Dim timeOut As Integer = My.Settings.TimeOut
    Public Sub Cerrar()
        If Log IsNot Nothing Then Log.logInfo("cerrando ")
        If ContestadorImplementado IsNot Nothing Then ContestadorImplementado.Dispose()
        '    If Conexion IsNot Nothing Then Conexion.Dispose()
        Numerador = Nothing
        ContestadorImplementado = Nothing
        '   Conexion = Nothing
        Log = Nothing
    End Sub
    

    Public Sub habilitar()
        CmbTransaccion.Items.Clear()
        Select Case Implementacion()

            Case E_Implementaciones.PosnetSalud

        End Select


    End Sub

    Public Sub Conectar()

        Dim Nombre As String = "POS-" + My.Computer.Name + "-" + My.User.Name
        ContestadorImplementado = New ClientPeer(Nombre, _
                        ObtenerTcpChannel(Implementacion, My.Settings.Host, Port), _
                        New NexoMessagesIdentifier())
        COnectarDB()

        Log.log("Conexion " + Nombre + " " + My.Settings.Host + ":" + Port.ToString)
        ContestadorImplementado.Connect()
        Log.log("Esperando Conexion ")
        Do While Not ContestadorImplementado.IsConnected
            My.Application.DoEvents()
        Loop
    End Sub
    Public Sub Desconectar()
        If ContestadorImplementado.IsConnected Then
            ContestadorImplementado.Close()
        End If
    End Sub
    Public Function Implementacion() As E_Implementaciones
        Return E_Implementaciones.PosnetComercio
    End Function


    Public Sub COnectarDB()
        'Conexion = New TarjetasAcceso.Acceso("ONLL", "TES")
        'Conexion.trace = True
        Log.log("Conectar DB")

    End Sub

    Private Sub Frm_IsoTester_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        Cerrar()
    End Sub

    Private Sub Frm_IsoTester_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Numerador = New VolatileStanSequencer()
        Log = New Logeador.Logeador(True)
        Timer1.Enabled = True
        CmbIP.Text = My.Settings.Host
        'CmbIP.SelectedIndex = My.Settings.Host
        SetearEscucha()
        CmbTransaccion.Items.Add(E_ProcCode.Compra_posnet)
        CmbTransaccion.Items.Add(E_ProcCode.AnulacionCompra_Posnet)
        CmbTransaccion.Items.Add(E_ProcCode.Devolucion_Posnet)
        CmbTransaccion.Items.Add(E_ProcCode.AnulacionDevolucion_Posnet)

        CmbModoIngreso.Items.Add("Manual")
        CmbModoIngreso.Items.Add("Banda")
        CmbModoIngreso.Items.Add("E-Commerce")
    End Sub
    Public Function MensajeDesdeString(ByVal s As String, ByVal Implementacion As E_Implementaciones) As Iso8583messageNexo

        Dim formatter As Trx.Messaging.Iso8583.Iso8583MessageFormatter

        formatter = New Iso8583Nexo(Implementacion)
        Dim context As New Trx.Messaging.ParserContext(ParserContext.DefaultBufferSize)
        Dim m As Trx.Messaging.Message

        context.Write(s)
        With formatter
            m = .Parse(context)
        End With

        Dim Mn As New Iso8583messageNexo(CType(m, Iso8583Message))

        Mn.Implementacion = Implementacion
        Return Mn
    End Function

    Public Sub Mandar0800()

        ' Build echo test message.
        Log.log("Enviando 0800")
        Dim echoMsg As Iso8583messageNexo = New Iso8583messageNexo(800, Implementacion)

        echoMsg.Fields.Add(3, "990000")
        Agregar7_FechaHora(echoMsg)
        agregar11_trace(echoMsg)
        echoMsg.Fields.Add(24, "101")
        agregar41(echoMsg)
        agregar42_Comercio(echoMsg, 0)
        echoMsg.Fields.Add(70, "301")
        MandarMensajeyEsperar(echoMsg)

    End Sub

    Public Sub ArmarMensaje(ByVal TipoMEnsaje As Short)
        '  If ContestadorImplementado.IsConnected Then
        ' Build echo test message.
        Log.log("Enviando " + TipoMEnsaje.ToString("0000"))
        Dim NroTarjeta As Int64 = CLng(CmbPlastico.Text)
        Dim Monto As Decimal = CDec(CMBMonto.Text)
        Dim FechaNeg As Date = edtfecnegocio.Value.Date
        Dim FechaTran As Date = edtFecTrans.Value.Date
        Dim mSG As Iso8583messageNexo = New Iso8583messageNexo(TipoMEnsaje, Implementacion)


        Dim ProcCode As String = CStr(CInt((CmbTransaccion.SelectedItem)).ToString("000000"))
        agregar3_codProc(mSG, ProcCode)
        agregar4_Monto(mSG, CDec(CMBMONTO.Text))
        Agregar7_FechaHora(mSG)
        agregar11_trace(mSG, TipoMEnsaje <> 420)
        Agregar12y13_HoraFecha(mSG, FechaTran.Date.Add(Now.TimeOfDay))
        Agregar14_expdate(mSG, EdtVto.Value)
        AGREGAR17_CAPTUREDATE(mSG, FechaNeg)

        Dim ModoIngreso As E_ModoIngreso = E_ModoIngreso.Manual
        Select Case CmbModoIngreso.SelectedIndex
            Case 1
                ModoIngreso = E_ModoIngreso.POS_track2_si
                agregar35(mSG, NroTarjeta, EdtVto.Text)
            Case 2
                ModoIngreso = E_ModoIngreso.Ecommerce
                agregar35(mSG, NroTarjeta, EdtVto.Text)
            Case Else
                agregar35(mSG, NroTarjeta, EdtVto.Text)
        End Select
        agregar22_modoIngreso(mSG, ModoIngreso)

        agregar32(mSG)
        agregar37_RETREFNUMBER(mSG)
        agregar41(mSG)
        agregar42_Comercio(mSG, CInt(CmbComercio.Text))
        AGREGAR43_atmloc(mSG)
        agregar48_term(mSG)
        agregar49_moneda(mSG)
        Agregar120_ubicacion(mSG)

        Select Case TipoMEnsaje
            Case 200
            Case 420
                Dim CodigoRespuestaReverso As E_CodigoREspuestaReversos = 0
                agregar39_CodigoRespuesta(mSG, CodigoRespuestaReverso)
        End Select

        ' propio de salud
        Dim DatosPOS As New Mensaje63POS("")

        DatosPOS.AgregarR2(CInt(cmbcuotas.text), CmbPlan.Text)
        DatosPOS.AgregarC0(TxtCvc.Text.PadLeft(4, "0"c))
        mSG.Fields.Add(63, DatosPOS.ToString)
        MandarMensajeyEsperar(mSG)
        'End If
    End Sub



    Public Sub Mandar0420()
        ArmarMensaje(420)
    End Sub


    Public Sub Agregar126(ByVal echomsg As Iso8583messageNexo, ByVal CantCuotas As Int16)
        Dim p As New Mensaje126Adelanto(echomsg.Implementacion, "", CType(echomsg.Fields(3).Value, E_ProcCode))
        With p
            .adelanto.CantCuotas = CantCuotas
            If CantCuotas <= 1 Then
                .Tiptran = 0
            Else
                .Tiptran = 1
            End If
            Dim s As String = .Tostring
        End With
        echomsg.Fields.Add(126, p.Tostring)

    End Sub
    Public Sub agregar32(ByVal echomsg As Iso8583messageNexo)
        '32 LLVAR ..R9(11) 32	acq-inst-id	LLVAR	..9(11)
        Dim s As String

        s = "12345678901"

        echomsg.Fields.Add(32, s)
        Log.logInfo(" acq-inst-id 32 :" + s)
    End Sub
    Public Sub agregar35(ByVal echomsg As Iso8583messageNexo, ByVal NroTarjeta As Int64, ByVal Vto As String)
        '35
        '            El formato es:
        'Posiciones 01-02  =  Indicador de longitud. El valor a informar es `37`.
        'Posiciones 03-39  =  Valor del campo.
        Dim s As String

        s = ""
        s += "" + NroTarjeta.ToString + "=" + Vto
        echomsg.Fields.Add(35, s)
        Log.logInfo(" track 35) :" + s)
    End Sub
    Public Sub agregar2(ByVal echomsg As Iso8583messageNexo, ByVal NroTarjeta As Int64)
        Dim s As String
        s = ""
        s += "" + NroTarjeta.ToString
        echomsg.Fields.Add(2, s)
        Log.logInfo(" track 2) :" + s)
    End Sub
    Public Sub AGREGAR17_CAPTUREDATE(ByVal echomsg As Iso8583messageNexo, ByVal Capdate As Date)
        '17 mmdd 9(4) 17	cap-dat	mmdd	9(4)
        Dim s As String
        s = Capdate.ToString("MMdd")

        echomsg.Fields.Add(17, s)
        Log.logInfo("Capture Date 17) :" + s)
    End Sub
    Public Sub agregar37_RETREFNUMBER(ByVal echomsg As Iso8583messageNexo)
        '37X(12) 37	retrvl-ref-num		g X(12)
        Dim s As String
        s = (Numerador.CurrentValue).ToString("000000")

        echomsg.Fields.Add(37, s)
        Log.logInfo(" RET REF N 37) :" + s)
    End Sub
    Public Sub AGREGAR43_atmloc(ByVal echomsg As Iso8583messageNexo)
        '43
        'Campo 43	El formato es:
        'Posiciones 01-22  =  Nombre de la  Institución dueña del ATM.
        Dim s As String
        s = "1234567890123456789012"
        'Posiciones 23-35  =  Localidad  donde  se  encuentra ubicado el ATM.
        s += "SUNCHALES    "
        'Posiciones 36-38  =  Código  de  Provincia  donde se encuentra ubicado el ATM.
        s += "222"
        'Posiciones 39-40  =  Código de País donde se encuentra ubicado el ATM.
        s += "33"
        Debug.Assert(s.Length = 40)
        echomsg.Fields.Add(43, s)
        Log.logInfo(" TERM 43) :" + s)
    End Sub
    Public Sub agregar48_term(ByVal echomsg As Iso8583messageNexo)
        Dim s As String
        s = "123456789012345678901234"
        s += "1"
        s += "22"
        s += "333"
        s += "444"
        s += "12345678901"
        echomsg.Fields.Add(48, s)
        Log.logInfo(" TERM 48) :" + s)
        '48
        'SHRG-GRP	Sharing group	X(24)
        'TERM-TRAN-ALLOWED	Indica a que nivel se permite una transacción para un cliente NOT-ON-US en dicha terminal	9(1)
        'TERM-ST	Estado en el que reside la terminal. No aplicable.	9(2)
        'TERM-CNTY	Código ANSI del país en el que reside la terminal. No utilizado.	9(3)
        'TERM-CNTRY	Código ISO del país en el que reside la terminal. No utilizado.	9(3)
        'TERM-RTE-GRP	Grupo de ruteo. No utilizado.	9(11)
    End Sub
    Sub Agregar120_ubicacion(ByVal mensaje As Iso8583messageNexo, Optional ByVal s As String = "NEXO POS SYSTEM")
        mensaje.Fields.Add(120, s)
    End Sub

    Public Sub agregar49_moneda(ByVal echomsg As Iso8583messageNexo, Optional ByVal pmoneda As E_Monedas = E_Monedas.PESOS)
        Dim s As String
        s = Int(pmoneda).ToString("000")
        echomsg.Fields.Add(49, s)
        Log.logInfo(" MONEDA 49) :" + s)
    End Sub
    Public Sub agregar52_Pinblock(ByVal echomsg As Iso8583messageNexo)
        'Dim b() As Byte = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}
        'echomsg.Fields.Add(52, b)
        'Log.logInfo(" PINBLOCK (52) :" + echomsg.Fields(52).Value.ToString)
        Dim b As String = "0123456789012345"
        echomsg.Fields.Add(52, b)
        Log.logInfo(" PINBLOCK (52) :" + echomsg.Fields(52).Value.ToString)
    End Sub
    Public Sub Agregar54_PAS(ByVal echomsg As Iso8583messageNexo)
        Dim s As String = ""
        Dim p54 As New ONLINECOM.Mensaje54Pas(Implementacion, 123, "456", 789)

        s = p54.Tostring
        echomsg.Fields.Add(54, s)
        Log.logInfo(" 54 pas :" + s)
    End Sub
    Public Sub agregar60_ATM(ByVal echomsg As Iso8583messageNexo)
        '60
        'El formato es:
        'Posiciones 01-03  =  Indicador de longitud. El valor a informar es `012`.
        'Posiciones 04-07  =  Número de  Institución dueña del ATM.
        'Posiciones 08-11  =  Código que identifica a la RED dueña del ATM.
        'Posiciones 12-15  =  Diferencia horaria con la RED. El valor a informar es `+000`.
        Dim s As String = "012" + "0926" + "8901" + "2345"
        echomsg.Fields.Add(60, StrToByteArray(s))
        Log.logInfo(" ATM (60) :" + s)
    End Sub

    Public Shared Function StrToByteArray(ByVal str As String) As Byte()
        Dim encoding As New System.Text.ASCIIEncoding()
        Return encoding.GetBytes(str)
    End Function 'StrToByteArray


    Public Sub agregar3_codProc(ByVal echomsg As Iso8583messageNexo, ByVal Codigo As String)
        echomsg.Fields.Add(3, Codigo.PadRight(6, "0"c))
        Log.logInfo(" Cod.Proc :" + Codigo.ToString)
    End Sub

    Public Sub agregar4_Monto(ByVal echomsg As Iso8583messageNexo, ByVal Monto As Double)
        Dim s As String = (Monto * 100).ToString("000000000000")
        echomsg.Fields.Add(4, s)
        Log.logInfo(" Monto (4):" + Monto.ToString + " a " + s)
    End Sub


    Public Sub agregar95_MontoREversado(ByVal echomsg As Iso8583messageNexo, ByVal Monto As Double)
        Dim s As String = (Monto * 100).ToString("000000000000")
        echomsg.Fields.Add(95, s.PadRight(42, "0"c))
        Log.logInfo(" Monto (95):" + Monto.ToString + " a " + s)
        '* el campo 95 se envía solamente cuando la transacción es reversada parcialmente.
        'Para mensajes de reverso (0420/0421) parcial (P-039: “32”) de extracción (código transacción: “01”):
        '1-12 Monto por el que realmente se completó la transacción en la moneda de la operación (P-049) (2 decimales).
        '13-42: Sin uso.

    End Sub
    Public Sub agregar39_CodigoRespuesta(ByVal echomsg As Iso8583messageNexo, ByVal CodResp As E_CodigoREspuestaReversos)
        Dim s As String = CInt(CodResp).ToString("00")
        echomsg.Fields.Add(39, s)
        Log.logInfo(" Monto (39):" + CodResp.ToString + " a " + s)

    End Sub
    Public Sub agregar11_trace(ByVal echomsg As Iso8583messageNexo, Optional ByVal Adelantar As Boolean = True)

        If Adelantar Then
            echomsg.Fields.Add(11, Numerador.Increment().ToString("000000"))
        Else
            echomsg.Fields.Add(11, (Numerador.CurrentValue - 1).ToString("000000"))

        End If
    End Sub
    Public Sub agregar41(ByVal echomsg As Iso8583messageNexo)
        echomsg.Fields.Add(41, CmbTerm.text)
    End Sub
    Public Sub agregar42_Comercio(ByVal echomsg As Iso8583messageNexo, ByVal NroComercio As Integer)
        echomsg.Fields.Add(42, NroComercio.ToString("00000000"))
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

    Public Sub MandarMensajeyEsperar(ByVal msg As Iso8583messageNexo)
        Log.log("Enviando " + msg.ToString)

        ListBox2.Items.Clear()
        EdtRespuesta.ForeColor = Color.CadetBlue
        EdtRespuesta.Text = "Esperando ..." + timeOut.ToString + " ms"
        Dim m As Iso8583messageNexo
        Dim request As RequestNexo
        If EscuchadorImplementado IsNot Nothing Then
            If EscuchadorImplementado.Conexiones.Count > 0 Then
                For Each sp As Trx.Messaging.FlowControl.ServerPeer In EscuchadorImplementado.Conexiones
                    msg.MessageSource = sp
                    msg.Send()
                    Exit For
                Next
            Else
                Log.log("no hay nadie conectado")

            End If

        Else
            request = New RequestNexo(ContestadorImplementado, msg)
            request.Send()
            request.WaitResponse(timeOut)
            m = request.ResponseMessage
            ProcessMessageIN(m, request.ResponseDateTime.Subtract(request.RequestDateTime).TotalSeconds)
        End If


    End Sub
    Public Sub ProcessMessageIN(ByVal m As Iso8583messageNexo, ByVal TiempoDeRespuesta As Double)
        If m Is Nothing Then
            EdtRespuesta.ForeColor = Color.Brown
            EdtRespuesta.Text = " No llego nada"
        Else
            ' ver codigo respuesta
            Dim cr As String = m.Fields(39).ToString
            'tiempo de respuesta
            Log.logInfo("tiempo de repuesta:" + TiempoDeRespuesta.ToString)

            If CInt(cr) = 0 Then
                If TiempoDerespuesta > 2 Then
                    EdtRespuesta.ForeColor = Color.Yellow
                Else
                    EdtRespuesta.ForeColor = Color.Green
                End If

            Else
                EdtRespuesta.ForeColor = Color.Red

            End If
            'procesar la respuesta
            EdtRespuesta.Text = cr + " " + ObtenerTxtRepuesta(cr)
            ' procesar los campos especificeos
            If m.Fields.Contains(63) Then
                Dim mp As New Mensaje63POS("")
                mp.FromstringTokenizer(m.Fields(63).ToString)

                If mp.Mensaje.StartsWith(".") Then
                    txtVisor.Text = mp.Mensaje
                Else
                    txtVisor.Text = ""
                End If

                For Each s2 As String In mp.Mensaje.Split(CChar("|"))
                    ListBox2.Items.Add(s2.Replace(vbCrLf, ""))
                Next
 
        End If


        End If
    End Sub

    Private Sub ContestadorImplementado_Connected(ByVal sender As Object, ByVal e As System.EventArgs) Handles ContestadorImplementado.Connected
        pconectado = True
    End Sub

    Private Sub ContestadorImplementado_Disconnected(ByVal sender As Object, ByVal e As System.EventArgs) Handles ContestadorImplementado.Disconnected
        pconectado = False
    End Sub

    Public Sub Conectado(ByVal Modo As Boolean)


    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged

        SetearEscucha()
    End Sub
    Public Sub SetearEscucha()
        If CheckBox1.Checked Then
            Escuchar()
        Else
            NoEscuchar()
        End If
    End Sub

    Public Sub NoEscuchar()
        If EscuchadorImplementado IsNot Nothing Then EscuchadorImplementado.Dispose()
        EscuchadorImplementado = Nothing
    End Sub

    Public Sub Escuchar()
        EscuchadorImplementado = New ONLINECOM.Escuchador(E_Implementaciones.PosnetComercio, "0.0.0.0", Port)
        habilitar()
    End Sub

    Private Sub ContestadorImplementado_Receive(ByVal sender As Object, ByVal e As Trx.Messaging.Channels.ReceiveEventArgs) Handles ContestadorImplementado.Receive

    End Sub

    Private Sub EscuchadorImplementado_TransaccionRespuesta(ByRef msg As ONLINECOM.Iso8583messageNexo) Handles EscuchadorImplementado.TransaccionRespuesta
        ProcessMessageIN(msg, 0)
    End Sub

    Private Sub CmdEnviar_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CmdEnviar.Click
        ArmarMensaje(200)
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        Mandar0800()
    End Sub

    Private Sub Timer1_Tick1(ByVal sender As Object, ByVal e As System.EventArgs) Handles Timer1.Tick

        If pconectado <> pconectado2 Then
            Conectado(pconectado)
            pconectado2 = pconectado
        End If
        If Not Log Is Nothing Then Log.GetArrayAndClear(ListBox1)

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Conectar()
    End Sub

    Private Sub CodPrac2_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub ListBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBox1.SelectedIndexChanged

    End Sub

    Private Sub EdtVto_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EdtVto.ValueChanged

    End Sub

    'Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
    '    ProcesaXLS()
    'End Sub

    Public Sub ProcesaXLs()

    End Sub
End Class






