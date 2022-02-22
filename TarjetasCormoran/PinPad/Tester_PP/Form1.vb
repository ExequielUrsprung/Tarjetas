Imports System.IO



Public Class Form1
    Dim encripta As Boolean = True

    'Dim WithEvents cliente As New ClienteTCP.Cliente(encripta, "04", "01", 9898, 5000, "172.16.1.7")    '--- tarjetasp1  
    Dim WithEvents cliente As New ClienteTCP.Cliente(encripta, "4", "20", 9898, 5000, "172.16.1.190")    '--- backtarjetas  
    'Dim WithEvents cliente As New ClienteTCP.Cliente(encripta, "4", "20", 9898, 5000, "172.16.20.34")    '--- server  

    'clientePinPad = New ClienteTCP.Cliente(encripta, param.pinguino, param.nrocaja, codCajero, 5000, "172.16.1.7") 

    Delegate Sub ManejarMensajes(s As String)

    Private Sub Registrar_movimiento(texto As String)
        Dim objStreamWriter As StreamWriter
        'Pass the file path and the file name to the StreamWriter constructor.
        objStreamWriter = New StreamWriter("C:\ProyectosNETTarjetas\TarjetasCormoran\Pei\ComprasPei.txt", True)               '"C:\Proyecto Tarjetas\Pei\ComprasPei.txt", True)
        objStreamWriter.WriteLine(texto)
        objStreamWriter.Close()
    End Sub

    Private Sub Log(texto As String)
        Dim objStreamWriter As StreamWriter
        'Pass the file path and the file name to the StreamWriter constructor.
        objStreamWriter = New StreamWriter("C:\ProyectosNETTarjetas\TarjetasCormoran\Pei\LogPEI.txt", True)           'C:\Proyecto Tarjetas\Pei\LogPEI.txt", True)
        objStreamWriter.WriteLine(texto)
        objStreamWriter.Close()
    End Sub


    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click 'COMPRA/ANULACION/DEVOLUCION
        Dim respuesta As ClienteTCP.Cliente.ResponseToPOS

        If rdbCompra.Checked Then
            respuesta = cliente.Compra(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text))
            'respuesta = cliente.Compra(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), "4", "15", CInt(ticket.Text), CInt(cajera.Text))
        ElseIf rdbAnulacionCompra.Checked Then
            respuesta = cliente.Anulacion(CInt(TKTOriginal.Text), CInt(cajera.Text))
        ElseIf rdbDevolucion.Checked Then
            respuesta = cliente.Devolucion(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text), FechaTRXOriginal.Text)
        ElseIf rdbAnulacionDevolucion.Checked Then
            respuesta = cliente.AnulacionDevolucion(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text))
            ' ElseIf rdbReverso.Checked Then
            'respuesta = cliente.Reversar_Test(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text))
        Else
            respuesta = Nothing
        End If




        If respuesta.Response = ClienteTCP.ResponseDescription.APROBADA Then
            ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION APROBADA: " & respuesta.Emisor)
            ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()


        ElseIf respuesta.Response = ClienteTCP.ResponseDescription.NO_APROBADA Then
            ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION NO APROBADA - " & respuesta.DisplayString)
            ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()

        Else
            ListView2.Items.Add(Now.TimeOfDay.ToString & " " & respuesta.DisplayString)
            ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()

        End If


        If respuesta.TicketString IsNot Nothing Then
            Imprime_Cupon(respuesta.TicketString)
        End If

    End Sub


    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click 'REVERSAR
        Dim respuesta As ClienteTCP.Cliente.ResponseToPOS
        respuesta.Response = cliente.ReversoTest().codRespuesta
        respuesta.DisplayString = "Reverso"
        'If rdbCompra.Checked Then
        '    respuesta = cliente.Reverso_Compra(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text))
        '    'respuesta = cliente.Compra(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), "4", "15", CInt(ticket.Text), CInt(cajera.Text))
        'ElseIf rdbAnulacionCompra.Checked Then
        '    respuesta = cliente.Anulacion(CInt(TKTOriginal.Text), CInt(cajera.Text))
        'ElseIf rdbDevolucion.Checked Then
        '    respuesta = cliente.Devolucion(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text), FechaTRXOriginal.Text)
        'ElseIf rdbAnulacionDevolucion.Checked Then
        '    respuesta = cliente.AnulacionDevolucion(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text))
        '    ' ElseIf rdbReverso.Checked Then
        '    'respuesta = cliente.Reversar_Test(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text))
        'Else
        '    respuesta = Nothing
        'End If
        If respuesta.Response = ClienteTCP.ResponseDescription.APROBADA Then
            ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION APROBADA: " & respuesta.DisplayString)
            ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()


        ElseIf respuesta.Response = ClienteTCP.ResponseDescription.NO_APROBADA Then
            ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION NO APROBADA - " & respuesta.DisplayString)
            ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()

        Else
            ListView2.Items.Add(Now.TimeOfDay.ToString & " " & respuesta.DisplayString)
            ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()

        End If


        If respuesta.TicketString IsNot Nothing Then
            Imprime_Cupon(respuesta.TicketString)
        End If
    End Sub


    Private Sub Imprime_Cupon(ticketString As String)

        For Each c As String In ticketString.Split(vbNewLine) 'vbCrLf
            If c = vbLf & "@" Then
                ListView3.Items.Add("-----------------------------------------------------------")
                ListView3.Items(ListView3.Items.Count - 1).EnsureVisible()

            Else
                ListView3.Items.Add(c)
                ListView3.Items(ListView3.Items.Count - 1).EnsureVisible()

            End If

        Next
        ListView3.Items.Add("-----------------------------------------------------------")
        ListView3.Items(ListView3.Items.Count - 1).EnsureVisible()

    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click 'INICIALIZACION
        If cliente.Inicializar_PinPad() = ClienteTCP.ResponseDescription.INICIALIZADO_OK Then
            MsgBox("Inicializado OK, puede comenzar a operar.")
        Else
            MsgBox("No se pudo inicializar, no puede operar.")
        End If

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load


        Me.ListView1.View = View.Details
        Me.ListView1.Font = New System.Drawing.Font("Courier New", 9, FontStyle.Regular)
        Me.ListView1.Columns.Clear()
        Me.ListView1.Columns.Add(" ", Me.Width * 2, HorizontalAlignment.Left)
        Me.ListView1.HeaderStyle = ColumnHeaderStyle.None

        Me.ListView2.View = View.Details
        Me.ListView2.Font = New System.Drawing.Font("Courier New", 9, FontStyle.Regular)
        Me.ListView2.Columns.Clear()
        Me.ListView2.Columns.Add(" ", Me.Width * 2, HorizontalAlignment.Left)
        Me.ListView2.HeaderStyle = ColumnHeaderStyle.None


        Me.ListView3.View = View.Details
        Me.ListView3.Font = New System.Drawing.Font("Courier New", 9, FontStyle.Regular)
        Me.ListView3.Columns.Clear()
        Me.ListView3.Columns.Add(" ", Me.Width * 2, HorizontalAlignment.Left)
        Me.ListView3.HeaderStyle = ColumnHeaderStyle.None
        rdbCompra.Checked = True

        If encripta Then cliente.Arrancar_PP()

        BackgroundWorker1.RunWorkerAsync()

        't.IsBackground = False
        't.Priority = Threading.ThreadPriority.Highest

        't.Start()

    End Sub


    'Private Sub ControlMensajeY21() Handles pp.Y21Recibido
    '    Dim d As New ManejarY21(AddressOf Leyo)
    '    Invoke(d, New Object() {"-------- Respuesta a Y21 ----------"})
    '    Invoke(d, New Object() {"Modo ing.  : Contactless"})
    '    Invoke(d, New Object() {"PAN        : " & pp.PAN})
    '    Invoke(d, New Object() {"Cod. Serv. : " & pp.CodServicio})
    '    Invoke(d, New Object() {"Cod. Banco : " & pp.CodBanco})
    '    Invoke(d, New Object() {"Nro de reg : " & pp.NroRegistro})
    '    Invoke(d, New Object() {"S Fisico   : " & pp.SeriePinPad})
    '    Invoke(d, New Object() {"Vencimiento: " & pp.Vencimiento})
    '    Invoke(d, New Object() {"Track 1    : " & pp.Track1})
    '    Invoke(d, New Object() {"Track 2    : " & pp.Track2})
    '    Invoke(d, New Object() {"Tr 1 Leido : " & pp.TrackNoLeido})
    '    Invoke(d, New Object() {"Criptograma: " & pp.Criptograma})
    '    Invoke(d, New Object() {"Cod. Aut.  : " & pp.Autorizacion})
    '    Invoke(d, New Object() {"Resp. Emi. : " & pp.RespEmisor})
    '    Invoke(d, New Object() {"Sec. PAN   : " & pp.SecuenciaPan})
    '    Invoke(d, New Object() {"Tipo Cuenta: " & pp.TipoCuenta})
    '    Invoke(d, New Object() {"PIN        : " & pp.getPinEncript})
    '    Invoke(d, New Object() {"Nombre APP : " & pp.Nombre_aplicacion})
    '    Invoke(d, New Object() {"ID APP     : " & pp.Id_aplicacion})
    '    Invoke(d, New Object() {"Nombre THab: " & pp.Nombre_THabiente})
    '    Invoke(d, New Object() {"Cons. Copia: " & pp.ConsultaCopia})
    '    Invoke(d, New Object() {"Sol. Firma : " & pp.SolicitaFirmaCliente})
    '    Invoke(d, New Object() {"Datos Emis : " & pp.DatosdelEmisor})



    'End Sub

    'Private Sub ControlMensajeY03() Handles pp.Y03Recibido
    '    Dim d As New ManejarY03(AddressOf Leyo)
    '    Dim s As New CambiaSemaforo(AddressOf Habilita_Semaforo)
    '    Invoke(d, New Object() {"-------- Respuesta a Y03 ----------"})
    '    Invoke(d, New Object() {"Cod. Aut.  : " & pp.Autorizacion})
    '    Invoke(d, New Object() {"Resp. Emi. : " & pp.RespEmisor})
    '    Invoke(d, New Object() {"Criptograma: " & pp.Criptograma})

    '    Dim advice As New Advice
    '    With advice
    '        .autorizacion = pp.Autorizacion.PadRight(6)
    '        .cripto = pp.Criptograma.PadRight(300)
    '        .resp = pp.CodigoRespuesta.padright(2)
    '    End With
    '    GrabarADV(advice, "C:\temp\04\caja0015.adz")
    '    Invoke(s, New Object() {})
    'End Sub




    'Private Sub Informacion(dato As String) Handles pp.DatosRecibidos
    '    Dim d As New ManejarDatoRecibido(AddressOf Informar)
    '    Invoke(d, New Object() {dato})
    'End Sub



    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click 'CANCELAR
        cliente.Cancelar_PinPad()
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click ' SINCRONIZAR
        If cliente.Sincronizar() = ClienteTCP.ResponseDescription.SINCRONIZADO_OK Then
            MsgBox("PinPad Sincronizado OK... Puede comenzar a operar.")
        Else
            MsgBox("Imposible sincronizar, no puede operar con este pinpad.")
        End If

    End Sub


    'Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
    '    pp.Pedir_Tarjeta_Compra_CL("00")
    'End Sub



#Region "Muestra Mensajes"
    Dim ColaMensajes As New Queue(Of ClienteTCP.MensajeCliente)
    Dim ColaMensajesPei As New Queue(Of lib_PEI.MensajeCliente)

    Private Sub InfoMensaje(msj As lib_PEI.MensajeCliente) Handles ClientePEI.Mensaje
        ColaMensajesPei.Enqueue(msj)
    End Sub

    ' Formatea los mensajes del server en un
    Private Sub InfoMensaje(msj As ClienteTCP.MensajeCliente) Handles cliente.Mensaje
        ColaMensajes.Enqueue(msj)
    End Sub

    Private Sub MostrarMensaje(texto As String) 'MUESTRA DETALLE RESPUESTAS
        ListView1.Items.Add(texto)
        ListView1.Items(ListView1.Items.Count - 1).EnsureVisible()
        Debug.WriteLine(texto)
    End Sub

    Private Sub MostrarProgreso(texto As String) 'MUESTRA DETALLE RESPUESTAS
        Label8.Text = texto
        Debug.WriteLine(texto)
        Application.DoEvents()
    End Sub

    'Public Sub Informar(dato As String) 'MUESTRA APROBADO O NO
    '    ListView2.Items.Add(Now.TimeOfDay.ToString & "   " & dato)
    '    ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()
    'End Sub
    'Dim t As New System.Threading.Thread(AddressOf Hilo_Mensajes)
    'Private Sub Hilo_Mensajes()
    '    While True
    '        Dim n As Integer = 0
    '        Application.DoEvents()

    '        Do While ColaMensajes.Count > 0
    '            Dim msj = ColaMensajes.Dequeue
    '            If msj.Tipo = ClienteTCP.TipoMensaje.Informacion Then
    '                Dim d As New ManejarMensajes(AddressOf MostrarMensaje)

    '                Dim s As String = String.Format("{0:dd/MM hh:mm:ss} {1}", msj.Fecha, msj.Texto)
    '                Invoke(d, New Object() {s})
    '                'MostrarMensaje(s)
    '            End If

    '            If msj.Tipo = ClienteTCP.TipoMensaje.Progress Then
    '                Dim d As New ManejarMensajes(AddressOf MostrarProgreso)
    '                Dim s As String = String.Format("Paquete {0} de {1} ", msj.Texto.Split("|")(0), msj.Texto.Split("|")(1))
    '                Invoke(d, New Object() {s})
    '                'MostrarProgreso(s)
    '            End If

    '            n += 1
    '            'esto es para que no se mate sacando mensajes de la colaMensajes
    '            If n > 20 Then Exit Do
    '        Loop
    '        Do While ColaMensajesPei.Count > 0
    '            Dim msj = ColaMensajesPei.Dequeue
    '            If msj.Tipo = lib_PEI.TipoMensaje.Informacion Then
    '                Dim s As String = String.Format("{0:dd/MM hh:mm:ss} {1}", msj.Fecha, msj.Texto)
    '                Log(s)
    '                For Each h In s.Split(vbNewLine)
    '                    MostrarMensaje(h)
    '                Next

    '            End If

    '            n += 1
    '            'esto es para que no se mate sacando mensajes de la colaMensajes
    '            If n > 20 Then Exit Do
    '        Loop

    '    End While
    'End Sub


    'Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
    '    Dim n As Integer = 0
    '    Application.DoEvents()

    '    Do While ColaMensajes.Count > 0
    '        Dim msj = ColaMensajes.Dequeue
    '        If msj.Tipo = ClienteTCP.TipoMensaje.Informacion Then
    '            Dim s As String = String.Format("{0:dd/MM hh:mm:ss} {1}", msj.Fecha, msj.Texto)
    '            MostrarMensaje(s)
    '        End If

    '        If msj.Tipo = ClienteTCP.TipoMensaje.Progress Then
    '            Dim s As String = String.Format("Paquete {0} de {1} ", msj.Texto.Split("|")(0), msj.Texto.Split("|")(1))
    '            MostrarProgreso(s)
    '        End If

    '        n += 1
    '        'esto es para que no se mate sacando mensajes de la colaMensajes
    '        If n > 20 Then Exit Do
    '    Loop
    '    Do While ColaMensajesPei.Count > 0
    '        Dim msj = ColaMensajesPei.Dequeue
    '        If msj.Tipo = lib_PEI.TipoMensaje.Informacion Then
    '            Dim s As String = String.Format("{0:dd/MM hh:mm:ss} {1}", msj.Fecha, msj.Texto)
    '            Log(s)
    '            For Each h In s.Split(vbNewLine)
    '                MostrarMensaje(h)
    '            Next

    '        End If

    '        n += 1
    '        'esto es para que no se mate sacando mensajes de la colaMensajes
    '        If n > 20 Then Exit Do
    '    Loop

    'End Sub

    Private Sub rdbCompra_CheckedChanged(sender As Object, e As EventArgs) Handles rdbCompra.CheckedChanged
        Habilitar_Compra()
    End Sub

    Private Sub rdbDevolucion_CheckedChanged(sender As Object, e As EventArgs) Handles rdbDevolucion.CheckedChanged
        Habilitar_Devolucion()
    End Sub

    Private Sub rdbAnulacionCompra_CheckedChanged(sender As Object, e As EventArgs) Handles rdbAnulacionCompra.CheckedChanged
        Habilitar_Anulacion()
    End Sub

    Private Sub rdbAnulacionDevolucion_CheckedChanged(sender As Object, e As EventArgs) Handles rdbAnulacionDevolucion.CheckedChanged
        Habilitar_Anulacion()
    End Sub

    Private Sub rdbReverso_CheckedChanged(sender As Object, e As EventArgs) Handles rdbReverso.CheckedChanged
        Habilitar_Reverso()
    End Sub

    Private Sub rdbConsulta_CheckedChanged(sender As Object, e As EventArgs) Handles rdbConsulta.CheckedChanged
        Habilitar_Consulta()
    End Sub


    Private Sub Habilitar_Compra()
        TKTOriginal.Enabled = False
        FechaTRXOriginal.Enabled = False

        Importe.Enabled = True
        cajera.Enabled = True
        Cashback.Enabled = True
        ticket.Enabled = True
        PlanCuotas.Enabled = True
    End Sub

    Private Sub Habilitar_Devolucion()
        TKTOriginal.Enabled = True
        FechaTRXOriginal.Enabled = True

        Importe.Enabled = True
        cajera.Enabled = True
        Cashback.Enabled = True
        'ticket.Enabled = False
        PlanCuotas.Enabled = True
    End Sub

    Private Sub Habilitar_Anulacion()
        TKTOriginal.Enabled = True
        FechaTRXOriginal.Enabled = False

        Importe.Enabled = False
        cajera.Enabled = True
        Cashback.Enabled = False
        ticket.Enabled = False
        PlanCuotas.Enabled = False
    End Sub

    Private Sub Habilitar_Reverso()
        TKTOriginal.Enabled = True
        FechaTRXOriginal.Enabled = True

        Importe.Enabled = True
        cajera.Enabled = False
        Cashback.Enabled = True
        ticket.Enabled = False
        PlanCuotas.Enabled = True
    End Sub

    Private Sub Habilitar_Consulta()
        TKTOriginal.Enabled = True
        FechaTRXOriginal.Enabled = True
        Importe.Enabled = True
        cajera.Enabled = True
        Cashback.Enabled = True
        PlanCuotas.Enabled = True

        TKTOriginal.Enabled = "566409"
        FechaTRXOriginal.Text = Format(Now, "yyyy-MM-dd")          ' "2020-06-22"

    End Sub



    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click 'CONTACTLESS
        Dim respuesta As ClienteTCP.Cliente.ResponseToPOS

        If rdbCompra.Checked Then
            respuesta = cliente.Compra_CL(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text))
            'respuesta = cliente.Compra(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), "4", "15", CInt(ticket.Text), CInt(cajera.Text))
        ElseIf rdbAnulacionCompra.Checked Then
            respuesta = cliente.Anulacion(CInt(TKTOriginal.Text), CInt(cajera.Text))
        ElseIf rdbDevolucion.Checked Then
            respuesta = cliente.Devolucion(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text), FechaTRXOriginal.Text)
        ElseIf rdbAnulacionDevolucion.Checked Then
            respuesta = cliente.AnulacionDevolucion(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text))
            ' ElseIf rdbReverso.Checked Then
            'respuesta = cliente.Reversar_Test(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text))
        Else
            respuesta = Nothing
        End If




        If respuesta.Response = ClienteTCP.ResponseDescription.APROBADA Then
            ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION APROBADA: " & respuesta.Emisor)
            ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()

        ElseIf respuesta.Response = ClienteTCP.ResponseDescription.NO_APROBADA Then
            ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION NO APROBADA - " & respuesta.DisplayString)
            ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()
        Else
            ListView2.Items.Add(Now.TimeOfDay.ToString & " " & respuesta.DisplayString)
            ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()
        End If


        If respuesta.TicketString IsNot Nothing Then
            Imprime_Cupon(respuesta.TicketString)
        End If

    End Sub

    Dim WithEvents ClientePEI As New lib_PEI.PEI
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click 'PEI

        If rdbCompra.Checked And CDec(Cashback.Text) = 0 Then
            ticket.Text = "PING01-" + Now.Date.ToString("yyMMdd") + Now.ToString("HHmmss")

            '--- LO USE YO PARA PRODUCCION    
            'Dim trx As New lib_PEI.Operacion_Compra With {
            '        .importe = CDec(Importe.Text) * 100,
            '        .codigoSeguridad = "965",                                                                          '"882",
            '        .moneda = lib_PEI.Moneda.ARS,
            '        .track1 = "%B778899000000091004^APELLIDO66/NOMBRE66      ^371212000000000224?",                    '"%B501063999000007007^MAESTRO VERIFICACION POS^9912101             ?", 
            '        .track2 = ";778899000000091004=371212000000000224?",                                               '";501063999000007007=991210100000000008?", 
            '        .ultimos = "1004",                                                                                 '"3018", 
            '        .Documentotitular = "34000919",                                                                    '"34000919",
            '        .idTerminal = "PING01",
            '        .idReferenciaOperacionComercio = "PAGOCORMORAN",                                                   '"PAGOCORMORAN"
            '        .traceTrxComercio = ticket.Text,
            '        .idCanal = "PEIBANDA",
            '        .idComercio = "1540",
            '        .tipoOperacion = lib_PEI.Concepto.COMPRA_DE_BIENES
            '}

            '--- LO USE YO PARA HOMOLOGACION  
            Dim trx As New lib_PEI.Operacion_Compra With {
                    .importe = CDec(Importe.Text) * 100,
                    .codigoSeguridad = "965",                                                                          '"882",
                    .moneda = lib_PEI.Moneda.ARS,
                    .track1 = "%B778899000000091004^APELLIDO66/NOMBRE66      ^371212000000000224?",                    '"%B501063999000007007^MAESTRO VERIFICACION POS^9912101             ?", 
                    .track2 = ";778899000000091004=371212000000000224?",                                               '";501063999000007007=991210100000000008?", 
                    .ultimos = "1004",                                                                                 '"3018", 
                    .Documentotitular = "34000919",                                                                    '"34000919",
                    .idTerminal = "PING01",
                    .idReferenciaOperacionComercio = "PAGOCORMORAN",                                                   '"PAGOCORMORAN"
                    .traceTrxComercio = ticket.Text,
                    .idCanal = "PEIBANDA",
                    .idComercio = "385",
                    .tipoOperacion = lib_PEI.Concepto.COMPRA_DE_BIENES
            }


            'Dim trx As New lib_PEI.Operacion_Compra With {
            '        .importe = CDec(Importe.Text) * 100,
            '        .codigoSeguridad = "882",                                                                          '"882",
            '        .moneda = lib_PEI.Moneda.ARS,
            '        .track1 = "%B501063999000007007^MAESTRO VERIFICACION POS^9912101             ?",                   '"%B501063999000007007^MAESTRO VERIFICACION POS^9912101             ?", 
            '        .track2 = ";501063999000007007=991210100000000008?",                                               '";501063999000007007=991210100000000008?", 
            '        .ultimos = "3018",                                                                                 '"3018", 
            '        .Documentotitular = "34000919",                                                                    '"34000919",
            '        .idTerminal = "PING01",
            '        .idReferenciaOperacionComercio = "PAGOCORMORAN",
            '        .traceTrxComercio = ticket.Text,
            '        .idCanal = "PEIBANDA",
            '        .idComercio = "385",
            '        .tipoOperacion = lib_PEI.Concepto.COMPRA_DE_BIENES
            '}

            Dim starinfo As New ProcessStartInfo
            'starinfo.FileName = "C:\Proyecto Tarjetas\AgentePEI\AgentePEI\bin\release\AgentePEI.exe"
            starinfo.FileName = "C:\ProyectosNETTarjetas\TarjetasCormoran\AgentePEI\AgentePEI\bin\Release"

            starinfo.WindowStyle = ProcessWindowStyle.Hidden
            starinfo.Arguments = trx.ToString
            Process.Start(starinfo)

            Dim respTransaccion As New lib_PEI.Respuesta
            respTransaccion = ClientePEI.Pagar(trx)
            Registrar_movimiento($"COMPRA;{trx.traceTrxComercio};{Now};{trx.importe / 100};{respTransaccion.id_operacion};{respTransaccion.descRespuesta};{respTransaccion.descripcion};{respTransaccion.nro_ref_bancaria}")
            ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION " & respTransaccion.descRespuesta.ToString)

        ElseIf rdbDevolucion.Checked Then
            ticket.Text = "PING01-" + Now.Date.ToString("yyMMdd") + Now.ToString("HHmmss")
            If CDec(Importe.Text) = 0 Then 'devolucion total
                Dim trx As New lib_PEI.Operacion_Devolucion With {
                      .codigoSeguridad = "965",
                      .track1 = "%B778899000000091004^APELLIDO66/NOMBRE66      ^371212000000000224?",
                      .track2 = ";778899000000091004=371212000000000224?",
                      .ultimos = "1004",
                      .Documentotitular = "34000919",
                      .idTerminal = "PING01",
                      .idReferenciaOperacionComercio = "DEVCORMORAN",
                      .traceTrxComercio = ticket.Text,
                      .idCanal = "PEIBANDA",
                      .idComercio = "385",
                      .idPagoOriginal = TKTOriginal.Text,
                      .tipoOperacion = lib_PEI.Concepto.DEVOLUCION
                      }
                Dim respTransaccion As New lib_PEI.Respuesta
                respTransaccion = ClientePEI.DevolverTotal(trx)
                Registrar_movimiento($"DEVTOTAL {trx.idPagoOriginal};{trx.traceTrxComercio};{Now};TOTAL;{respTransaccion.id_operacion};{respTransaccion.descRespuesta};{respTransaccion.descripcion};{respTransaccion.nro_ref_bancaria}")
                ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION " & respTransaccion.descRespuesta.ToString)
            Else
                Dim trx As New lib_PEI.Operacion_Devolucion_Parcial With {
                      .codigoSeguridad = "965",
                      .track1 = "%B778899000000091004^APELLIDO66/NOMBRE66      ^371212000000000224?",
                      .track2 = ";778899000000091004=371212000000000224?",
                      .ultimos = "1004",
                      .Documentotitular = "34000919",
                      .idTerminal = "PING01",
                      .idReferenciaOperacionComercio = "DEVCORMORAN",
                      .traceTrxComercio = ticket.Text,
                      .idCanal = "PEIBANDA",
                      .idComercio = "385",
                      .idPagoOriginal = TKTOriginal.Text,
                      .importe = CDec(Importe.Text) * 100,
                      .moneda = lib_PEI.Moneda.ARS,
                      .tipoOperacion = lib_PEI.Concepto.DEVOLUCION
                      }
                Dim respTransaccion As New lib_PEI.Respuesta
                respTransaccion = ClientePEI.DevolverPacial(trx)
                Registrar_movimiento($"DEVPARCIAL {trx.idPagoOriginal};{trx.traceTrxComercio};{Now};{trx.importe / 100};{respTransaccion.id_operacion};{respTransaccion.descRespuesta};{respTransaccion.descripcion};{respTransaccion.nro_ref_bancaria}")
                ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION " & respTransaccion.descRespuesta.ToString)
            End If

        ElseIf CDec(Cashback.Text) > 0 Then
            ticket.Text = "PING01-" + Now.Date.ToString("yyMMdd") + Now.ToString("HHmmss")
            Dim trx As New lib_PEI.Operacion_Compra With {
                  .importe = CDec(Cashback.Text) * 100,
                  .codigoSeguridad = "965",
                  .moneda = lib_PEI.Moneda.ARS,
                  .track1 = "%B778899000000091004^APELLIDO66/NOMBRE66      ^371212000000000224?",
                  .track2 = ";778899000000091004=371212000000000224?",
                  .ultimos = "1004",
                  .Documentotitular = "34000919",
                  .idTerminal = "PING01",
                  .idReferenciaOperacionComercio = "EXTCORMORAN",
                  .traceTrxComercio = ticket.Text,
                  .idCanal = "PEIBANDA",
                  .idComercio = "385",
                  .tipoOperacion = lib_PEI.Concepto.EXTRACCION
          }
            Dim respTransaccion As New lib_PEI.Respuesta
            respTransaccion = ClientePEI.Retirar(trx)
            Registrar_movimiento($"CASHBACK;{trx.traceTrxComercio};{Now};{trx.importe / 100};{respTransaccion.id_operacion};{respTransaccion.descRespuesta};{respTransaccion.descripcion};{respTransaccion.nro_ref_bancaria}")
            ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION " & respTransaccion.descRespuesta.ToString)



        ElseIf rdbconsulta.Checked Then
            ticket.Text = "PING01-" + Now.Date.ToString("yyMMdd") + Now.ToString("HHmmss")
            Dim trx As New lib_PEI.Operacion_Consulta With {
                  .codigoSeguridad = "965",
                  .track1 = "%B778899000000091004^APELLIDO66/NOMBRE66      ^371212000000000224?",
                  .track2 = ";778899000000091004=371212000000000224?",
                  .ultimos = "1004",
                  .Documentotitular = "34000919",
                  .idTerminal = "PING01",
                  .idReferenciaOperacionComercio = "CONSCORMORAN",
                  .traceTrxComercio = ticket.Text,
                  .idCanal = "PEIBANDA",
                  .idComercio = "385",
                  .idPagoOriginal = TKTOriginal.Text,
                  .tipoOperacion = lib_PEI.Concepto.CONSULTA,
                  .idFechaOriginal = FechaTRXOriginal.Text
                  }
            Dim respTransaccion As New lib_PEI.Respuesta
            respTransaccion = ClientePEI.ConsultaTotal(trx)
            'Registrar_movimiento($"DEVTOTAL {trx.idPagoOriginal};{trx.traceTrxComercio};{Now};TOTAL;{respTransaccion.id_operacion};{respTransaccion.descRespuesta};{respTransaccion.descripcion};{respTransaccion.nro_ref_bancaria}")
            ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION " & respTransaccion.descRespuesta.ToString)
        End If
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click    '--- COMPRA/ANULACION/DEVOLUCION MULTIMODO
        Dim respuesta As ClienteTCP.Cliente.ResponseToPOS

        If rdbCompra.Checked Then
            respuesta = cliente.CompraMM(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text), txtnrodocumento.Text)
            'respuesta = cliente.Compra(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), "4", "15", CInt(ticket.Text), CInt(cajera.Text))
        ElseIf rdbAnulacionCompra.Checked Then
            respuesta = cliente.AnulacionMM(CInt(TKTOriginal.Text))
        ElseIf rdbDevolucion.Checked Then
            respuesta = cliente.DevolucionMM(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text), CInt(TKTOriginal.Text), FechaTRXOriginal.Text)
        ElseIf rdbAnulacionDevolucion.Checked Then
            'respuesta = cliente.AnulacionDevolucion(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text))
            ' ElseIf rdbReverso.Checked Then
            'respuesta = cliente.Reversar_Test(CInt(PlanCuotas.Text), CDec(Importe.Text), CDec(Cashback.Text), CInt(ticket.Text))
        Else
            respuesta = Nothing
        End If




        If respuesta.Response = ClienteTCP.ResponseDescription.APROBADA Then
            ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION APROBADA: " & respuesta.Emisor)
            ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()


        ElseIf respuesta.Response = ClienteTCP.ResponseDescription.NO_APROBADA Then
            ListView2.Items.Add(Now.TimeOfDay.ToString & "   TRANSACCION NO APROBADA - " & respuesta.DisplayString)
            ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()

        Else
            ListView2.Items.Add(Now.TimeOfDay.ToString & " " & respuesta.Response.ToString & " - " & respuesta.DisplayString)
            ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()

        End If


        If respuesta.TicketString IsNot Nothing Then
            Imprime_Cupon(respuesta.TicketString)
        End If
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        FormQR.ShowDialog()
    End Sub


    Private Function convertir_archivo(archivo As String) As String
        Dim bytes = My.Computer.FileSystem.ReadAllBytes(archivo)
        Return TransmisorTCP.Serializador.ByteArrayToString(bytes)
    End Function




    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click

        Dim arc As String = ""

        OpenFileDialog1.InitialDirectory = "C:\Tarjetas"
        OpenFileDialog1.Filter = "Zip Files (*.zip)|*.zip"
        OpenFileDialog1.FilterIndex = 2
        OpenFileDialog1.RestoreDirectory = True

        If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Try
                arc = OpenFileDialog1.FileName
            Catch Ex As Exception
                MessageBox.Show("No se puede importar este archivo: " & Ex.Message)
            End Try
        End If

        If arc <> "" Then
            If cliente.Actualizar_PinPad(convertir_archivo(arc)) = 0 Then
                ListView2.Items.Add(Now.TimeOfDay.ToString & "   Actualizado OK")
                ListView2.Items(ListView2.Items.Count - 1).EnsureVisible()
            End If
        End If
    End Sub

    Private Sub BackgroundWorker1_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        While True
            Dim n As Integer = 0
            Application.DoEvents()

            Do While ColaMensajes.Count > 0
                Dim msj = ColaMensajes.Dequeue
                If msj.Tipo = ClienteTCP.TipoMensaje.Informacion Then
                    Dim d As New ManejarMensajes(AddressOf MostrarMensaje)

                    Dim s As String = String.Format("{0:dd/MM hh:mm:ss} {1}", msj.Fecha, msj.Texto)
                    Invoke(d, New Object() {s})
                    'MostrarMensaje(s)
                End If

                If msj.Tipo = ClienteTCP.TipoMensaje.Progress Then
                    Dim d As New ManejarMensajes(AddressOf MostrarProgreso)
                    Dim s As String = String.Format("Paquete {0} de {1} ", msj.Texto.Split("|")(0), msj.Texto.Split("|")(1))
                    Invoke(d, New Object() {s})
                    'MostrarProgreso(s)
                End If

                n += 1
                'esto es para que no se mate sacando mensajes de la colaMensajes
                If n > 20 Then Exit Do
            Loop
            Do While ColaMensajesPei.Count > 0
                Dim msj = ColaMensajesPei.Dequeue
                If msj.Tipo = lib_PEI.TipoMensaje.Informacion Then
                    Dim s As String = String.Format("{0:dd/MM hh:mm:ss} {1}", msj.Fecha, msj.Texto)
                    Log(s)
                    For Each h In s.Split(vbNewLine)
                        MostrarMensaje(h)
                    Next

                End If

                n += 1
                'esto es para que no se mate sacando mensajes de la colaMensajes
                If n > 20 Then Exit Do
            Loop

        End While
    End Sub

    Private Sub FechaTRXOriginal_TextChanged(sender As Object, e As EventArgs) Handles FechaTRXOriginal.TextChanged

    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        cliente.IniTransQR(0, 1.0, 12345, "20220208", "091800", "9898")
    End Sub


#End Region
End Class