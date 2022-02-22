Imports TjComun
Imports System.Threading
Imports lib_PP


Public Class Form1

    Dim lecturaTarjeta As String
    Dim listaReqtarj As New List(Of RequerimientoTarjeta)
    Dim WithEvents pp As PinPadFD


    Dim versionsoft As String
    Delegate Sub ManejarY01(dato)
    Delegate Sub ManejarY02(dato)
    Delegate Sub ManejarY03(dato)
    Delegate Sub ManejarY00(dato)
    Delegate Sub ManejarY0E(dato)
    Delegate Sub ManejarDatoRecibido(dato)
    Delegate Sub CambiaSemaforo()

    Dim semaforo As Boolean

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        txtLinea.Focus()
        txtimporte.Text = "100"
        txtplanpago.Text = "102"
        txt4digitos.Text = "7787"
        txtLinea.Text = "%B4507990000977787^PRUEBA/PRUEBA        ^190510109999        00036000000?;4507990000977787=19051010360000000000?"
        Timer1.Enabled = True
        pp = New PinPadFD
    End Sub


    'Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
    '    ' se debe generar uno nuevo por cada requerimiento !
    '    'Dim ReqTarj As New IsoLibCaja.RequerimientoTarjeta
    '    'HabilitarPinPad()

    '    requerimientos("415")
    'End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim hora As Date
        Dim salir As Boolean
        semaforo = False
        salir = True

        ' #######  Y01 #########
        pp.Abrir_puerto()
        pp.Pedir_Tarjeta_Compra(Tipos_transacciones.compra)

        Do
            hora = Now
            While Not semaforo And Now.Subtract(hora).TotalSeconds < 15
                Application.DoEvents()
            End While

            If Not semaforo Then
                If MsgBox("Seguir esperando?", vbYesNo + MsgBoxStyle.Question) = MsgBoxResult.Yes Then
                    salir = False
                Else
                    salir = True
                    pp.Cancelar()
                    Exit Sub
                End If
            End If
        Loop Until salir


        ' #######  Y02 #########
        salir = True
        semaforo = False

        If pp.ModoIngreso = "C" Then
            'pp.Pedir_Datos_Adicionales(True, True, True, False, "1234123412341234", True, False, "000000001200", "000000000000")
            pp.Pedir_Datos_Adicionales(True, True, True, False, "1234123412341234", False, False, "000000001200", "000000000000")
        Else
            pp.Pedir_Datos_Adicionales(True, True, True, False, "", False, False, "000000001200", "000000000000")
        End If
        Do
            hora = Now
            While Not semaforo And Now.Subtract(hora).TotalSeconds < 15
                Application.DoEvents()
            End While
            If Not semaforo Then
                If MsgBox("Seguir esperando?", vbYesNo + MsgBoxStyle.Question) = MsgBoxResult.Yes Then
                    salir = False
                Else
                    salir = True
                    pp.Cancelar()
                    Exit Sub
                End If
            End If
        Loop Until salir

        semaforo = False
        salir = True

        ' #######  Y03 #########
        If pp.ModoIngreso = "C" Then
            Do
                hora = Now
                While Not IO.File.Exists("C:\temp\04\CAJA0015.VTA") And Now.Subtract(hora).TotalSeconds < 30
                    Application.DoEvents()
                End While
                If Not IO.File.Exists("C:\temp\04\CAJA0015.VTA") Then
                    If MsgBox("Seguir esperando respuesta del servidor?", vbYesNo + MsgBoxStyle.Question) = MsgBoxResult.Yes Then
                        salir = False
                    Else
                        salir = True
                        pp.Cancelar()
                    End If
                End If
            Loop Until salir

            Dim reg As String
            Try
                reg = My.Computer.FileSystem.ReadAllText("C:\temp\04\CAJA0015.VTA")
            Catch ex As Exception
                MsgBox("No se pudo leer el vta")
                Exit Sub
            End Try

            If IO.File.Exists("C:\temp\04\CAJA0015.VTA") Then
                salir = True
                pp.Datos_Respuesta_Emisor(Trim(Mid(reg.ToString, 442, 6)), Mid(reg.ToString, 138, 2), True, Trim(Mid(reg.ToString, 142, 300)))

                Do
                    hora = Now
                    While Not semaforo And Now.Subtract(hora).TotalSeconds < 8
                        Application.DoEvents()
                    End While
                    If Not semaforo Then
                        If MsgBox("Seguir esperando?", vbYesNo + MsgBoxStyle.Question) = MsgBoxResult.Yes Then
                            salir = False
                        Else
                            salir = True
                            pp.Cancelar()
                            Exit Sub
                        End If
                    End If
                Loop Until salir


            Else
                MsgBox("No hay respuesta del sistema de tarjetas.")
            End If
        End If
    End Sub

    Public Sub Habilita_Semaforo()
        semaforo = True
    End Sub

    Public Sub Leyo(dato As String)
        'ListView1.Items.Add(Now.TimeOfDay.ToString & "   " & dato)
        'ListView1.Items(ListView1.Items.Count - 1).EnsureVisible()
    End Sub

    Private Sub ControlError(ByVal comando As String, ByVal x As String, ByVal nro As String) Handles pp.Y0ERecibido
        Dim d As New ManejarY0E(AddressOf Leyo)
        Invoke(d, New Object() {"-------- Respuesta a Y0E ----------"})
        Invoke(d, New Object() {"Descrpcion    : " & x})
        Invoke(d, New Object() {"Comando falló : " & comando})
        Invoke(d, New Object() {"Nro error     : " & nro})

    End Sub

    Private Sub ControlMensaje() Handles pp.Y01Recibido
        Dim d As New ManejarY01(AddressOf Leyo)
        Dim s As New CambiaSemaforo(AddressOf Habilita_Semaforo)
        Invoke(d, New Object() {"-------- Respuesta a Y01 ----------"})
        Invoke(d, New Object() {"Modo Ing   : " & pp.ModoIngreso})
        Invoke(d, New Object() {"Nro Tarjeta: " & pp.PAN})
        If pp.ModoIngreso <> "M" Then
            Invoke(d, New Object() {"Cod. Serv. : " & pp.CodServicio})
            Invoke(d, New Object() {"Cod. Banco : " & pp.CodBanco})
            Invoke(d, New Object() {"Nro de reg : " & pp.NroRegistro})
            Invoke(d, New Object() {"SHA        : " & pp.SHA})
        End If
        Invoke(s, New Object() {})
    End Sub

    Private Sub ControlMensajeY02() Handles pp.Y02Recibido
        Dim d As New ManejarY02(AddressOf Leyo)
        Dim s As New CambiaSemaforo(AddressOf Habilita_Semaforo)
        Invoke(d, New Object() {"-------- Respuesta a Y02 ----------"})
        Select Case pp.ModoIngreso
            Case "B"
                Invoke(d, New Object() {"Modo ing.  : Banda"})
                Invoke(d, New Object() {"PAN        : " & pp.PAN})
                Invoke(d, New Object() {"Vencimiento: " & pp.Vencimiento})
                Invoke(d, New Object() {"Track 1    : " & pp.Track1})
                Invoke(d, New Object() {"Track 2    : " & pp.Track2})
                Invoke(d, New Object() {"Tr 1 Leido : " & pp.TrackNoLeido})
                Invoke(d, New Object() {"Cod de seg.: " & pp.CodSeguridad})
                Invoke(d, New Object() {"Serie PP   : " & pp.SeriePinPad})
                Invoke(d, New Object() {"Tipo Cuenta: " & pp.TipoCuenta})
                Invoke(d, New Object() {"SHA        : " & pp.SHA})
                Invoke(d, New Object() {"PIN        : " & pp.getPinEncript})
                Invoke(d, New Object() {"Nombre THab: " & pp.Nombre_THabiente})

            Case "C"
                Invoke(d, New Object() {"Modo ing.  : Chip"})
                Invoke(d, New Object() {"PAN        : " & pp.PAN})
                Invoke(d, New Object() {"Vencimiento: " & pp.Vencimiento})
                Invoke(d, New Object() {"Track 1    : " & pp.Track1})
                Invoke(d, New Object() {"Track 2    : " & pp.Track2})
                Invoke(d, New Object() {"Tr 1 Leido : " & pp.TrackNoLeido})
                Invoke(d, New Object() {"S Fisico   : " & pp.SeriePinPad})
                '                Invoke(d, New Object() {"S Logico   : " & pp.SerieLogicoPinPad})
                Invoke(d, New Object() {"Criptograma: " & pp.Criptograma})
                Invoke(d, New Object() {"Cod. Aut.  : " & pp.Autorizacion})
                Invoke(d, New Object() {"Resp. Emi. : " & pp.RespEmisor})
                Invoke(d, New Object() {"Sec. PAN   : " & pp.SecuenciaPan})
                Invoke(d, New Object() {"Tipo Cuenta: " & pp.TipoCuenta})
                Invoke(d, New Object() {"SHA        : " & pp.SHA})
                Invoke(d, New Object() {"PIN        : " & pp.getPinEncript})
                Invoke(d, New Object() {"Nombre APP : " & pp.Nombre_aplicacion})
                '                Invoke(d, New Object() {"ID APP     : " & pp.Id_aplicacion})
                '                Invoke(d, New Object() {"Encripcion : " & pp.Encripcion})
                '                Invoke(d, New Object() {"Paquete Enc: " & pp.PaqueteEncriptado})
                Invoke(d, New Object() {"Nombre THab: " & pp.Nombre_THabiente})
            Case "M"
                Invoke(d, New Object() {"Modo ing.  : Manual"})
                Invoke(d, New Object() {"PAN        : " & pp.PAN})
                Invoke(d, New Object() {"Vencimiento: " & pp.Vencimiento})
                Invoke(d, New Object() {"Cod de seg.: " & pp.CodSeguridad})
                Invoke(d, New Object() {"Serie PP   : " & pp.SeriePinPad})
                Invoke(d, New Object() {"Tipo Cuenta: " & pp.TipoCuenta})
                Invoke(d, New Object() {"PIN        : " & pp.getPinEncript})

        End Select
        requerimientos("415")

        'Dim ida As New IdaTypeInternal
        'With ida
        '    .VERSION = 1
        '    .TARJ = pp.PAN.PadRight(20)
        '    .TRACK1 = pp.Track1.PadRight(77)
        '    .TRACK2 = pp.Track2.PadRight(37)
        '    .IMPO = "120"
        '    .PLANINT = 0
        '    .TICCAJ = 454545
        '    .EXPDATE = pp.Vencimiento.PadRight(4)
        '    If pp.CodSeguridad Is Nothing Then
        '        .CODSEG = "".PadRight(30)
        '    Else
        '        .CODSEG = pp.CodSeguridad.padright(30)
        '    End If
        '    .CodAut = "".PadRight(6)
        '    .FECORI = "".PadRight(6)
        '    .TKTORI = 0
        '    .TKID = "".PadRight(4)
        '    If pp.ModoIngreso = "C" Then
        '        .MANUAL = 5
        '    ElseIf pp.ModoIngreso = "M" Then
        '        .MANUAL = 1
        '    Else
        '        .MANUAL = 0
        '    End If
        '    .CASHBACK = 0
        '    .cmd = 0
        '    .cajadir = Trim("C:\temp\04").PadRight(26)
        '    .HORA = CDbl(Now.ToOADate) 'VER QUE LA VARIABLE ES UN SINGLE EN EL REG3000 PASA NOW.TOOADATE
        '    .oper = 0
        '    .CAJERA = "9898"
        '    .CARDSEQ = pp.SecuenciaPan.PadRight(3)
        '    .CRIPTO = pp.Criptograma.PadRight(300)
        '    .NOMBRE = pp.Nombre_THabiente.PadRight(45)
        '    '.SERIEF = pp.SeriePinPad.padright(12)
        '    '.SERIEL = pp.SerieLogicoPinPad.padright(16)
        '    .SERIEL = pp.SeriePinPad.PadRight(16)
        '    .ServCode = pp.CodServicio
        '    '.APPVERSION = pp.VersionSoft.padright(15)
        '    '.AID = pp.Id_aplicacion.padright(16)
        '    '.APN = pp.Nombre_aplicacion.padright(16)
        '    '.TIPOENC = pp.Encripcion.PadRight(1)
        '    '.PAQUETE = pp.PaqueteEncriptado.PadRight(120)
        '    .CHECK = "O2"
        'End With
        'Dim VERSION As String          '* 1       ' NRO DE VERSION               
        'Dim TARJ As String             '* 20      ' NRO DE TARJETA               
        'Dim EXPDATE As String          '* 4       ' FECHA DE EXPIRACION          
        'Dim IMPO As Decimal            'CURRENCY         ' IMPORTE DE LA TRANSACCION    
        ''Dim MANUAL As EModoIngreso             ' MODO DE INGRESO 0-MANUAL 1-AUTO 5-CHIP
        'Dim MANUAL As Short            ' MODO DE INGRESO 0-MANUAL 1-AUTO 5-CHIP
        'Dim PLANINT As Short           ' COD.PLAN                     
        'Dim CODSEG As String           '* 30      ' COD. SEGURIDAD            
        'Dim TICCAJ As Integer          ' NRO DE TICKET DE LA CAJA     
        'Dim CAJERA As Short            ' COD DE CAJERO (OPERADOR)     
        'Dim HORA As Double             ' FECHA/HORA                   
        'Dim TRACK2 As String           '* 37      '                              
        'Dim TRACK1 As String           '* 77      '                              
        'Dim CodAut As String           '* 6       ' Codigo de autorizacion pa/anu
        'Dim TKTORI As Integer          '                              
        'Dim FECORI As String           '* 6       '                              
        'Dim PLANINTori As Short        ' COD.PLAN                     
        'Dim oper As Short              ' !! operacion                 
        'Dim cmd As Short               '+8 offline +16 anular                  
        'Dim cajadir As String          '* 26      '                          
        'Dim TKID As String             '* 4
        'Dim CASHBACK As Decimal        '                          
        'Dim CHECK As String            '* 2      ' HACER = CHECKID              
        'Dim CRIPTO As String           ' * 300 CRIPTOGRAMA TARJETAS CHIP
        'Dim CARDSEQ As String          ' * 3 CARD SEQUENCE
        'Dim APPVERSION As String       ' * 15  VERSION APLICACION
        'Dim PAQUETE As String          ' * 120 PAQUETE ENCRIPTADO
        'Dim TIPOENC As String          ' * 1 TIPO DE ENCRIPCION
        'Dim SERIEF As String           ' * 12 SERIE FISICO
        'Dim SERIEL As String           ' * 16 SERIE LOGICO
        'Dim NOMBRE As String           ' * 45 NOMBRE TARJETA HABIENTE
        'Dim AID As String              ' * 16 ID APLICACION
        'Dim APN As String              ' * 16 NOMBRE APLICACION
        'End Structure

        'GrabarIDZ(ida, "C:\temp\04\caja0015.idz")
        Invoke(s, New Object() {})
    End Sub
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Me.Close()
    End Sub



    Private Sub txtLinea_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtLinea.KeyPress
        If e.KeyChar = Chr(13) Then
            lecturaTarjeta = txtLinea.Text
            txtNroTarjetaT1.Text = "" : txtNroTarjetaT2.Text = "" : TxtNombre.Text = ""
            txtVencimientoT1.Text = "" : txtVencimientoT2.Text = ""
            txtnumerotarjeta.Text = "" : txtfechavencimiento.Text = ""
            txtTrack1.Text = "" : txtTrack2.Text = ""
            ErrorAntesMandar.Items.Clear()

            ProcedureValidar()
        End If
    End Sub


    Public Sub ProcedureValidar()
        Dim c, inicio, campo, principioTrack, track As Integer
        Dim primeraparte, segundaparte, nroTarjeta, nombre, vencimiento, track1, track2 As String



        For c = 1 To Len(lecturaTarjeta)
            Select Case Mid(lecturaTarjeta, c, 1)
                Case "Â", "â"
                    primeraparte = Mid(lecturaTarjeta, 1, c - 1)
                    segundaparte = Mid(lecturaTarjeta, c + 1, Len(lecturaTarjeta))
                    lecturaTarjeta = primeraparte & "^A" & segundaparte
                Case "Ê", "ê"
                    primeraparte = Mid(lecturaTarjeta, 1, c - 1)
                    segundaparte = Mid(lecturaTarjeta, c + 1, Len(lecturaTarjeta))
                    lecturaTarjeta = primeraparte & "^E" & segundaparte
                Case "Î", "î"
                    primeraparte = Mid(lecturaTarjeta, 1, c - 1)
                    segundaparte = Mid(lecturaTarjeta, c + 1, Len(lecturaTarjeta))
                    lecturaTarjeta = primeraparte & "^I" & segundaparte
                Case "Ô", "ô"
                    primeraparte = Mid(lecturaTarjeta, 1, c - 1)
                    segundaparte = Mid(lecturaTarjeta, c + 1, Len(lecturaTarjeta))
                    lecturaTarjeta = primeraparte & "^O" & segundaparte
                Case "Û", "û"
                    primeraparte = Mid(lecturaTarjeta, 1, c - 1)
                    segundaparte = Mid(lecturaTarjeta, c + 1, Len(lecturaTarjeta))
                    lecturaTarjeta = primeraparte & "^U" & segundaparte
            End Select
        Next

        'lecturaTarjeta = "%" + lecturaTarjeta
        track = 1
        principioTrack = 1
        inicio = 1
        campo = 1
        For c = 1 To Len(lecturaTarjeta)
            Select Case Mid(lecturaTarjeta, c, 1)
                Case "^", "?", "="
                    If track = 1 Then
                        Select Case campo
                            Case 1
                                If Mid(lecturaTarjeta, 2, 1) = "B" Then
                                    nroTarjeta = Mid(lecturaTarjeta, 3, c - inicio - 2)
                                Else
                                    nroTarjeta = Mid(lecturaTarjeta, 2, c - inicio - 2)
                                End If
                                txtNroTarjetaT1.Text = nroTarjeta
                            Case 2
                                nombre = Mid(lecturaTarjeta, inicio + 1, c - inicio - 1)
                                TxtNombre.Text = nombre
                            Case 3
                                vencimiento = Mid(lecturaTarjeta, inicio + 1, 4)
                                track1 = Mid(lecturaTarjeta, principioTrack + 2, c - 2)
                                txtTrack1.Text = track1
                                txtVencimientoT1.Text = vencimiento
                        End Select
                        campo = campo + 1
                        inicio = c
                    Else
                        Select Case campo
                            Case 1
                                nroTarjeta = Mid(lecturaTarjeta, inicio + 1, c - inicio - 1)
                                txtNroTarjetaT2.Text = Mid(lecturaTarjeta, inicio + 1, c - inicio - 1)
                                txtLinea.PasswordChar = "*"
                                txtnumerotarjeta.Text = Mid(lecturaTarjeta, inicio + 1, c - inicio - 1)
                            Case 2
                                vencimiento = Mid(lecturaTarjeta, inicio + 1, c - inicio - 1)
                                track2 = Mid(lecturaTarjeta, principioTrack, c - principioTrack + 1)
                                txtTrack2.Text = track2
                                txtVencimientoT2.Text = Mid(lecturaTarjeta, inicio + 1, 4)
                                txtfechavencimiento.Text = Mid(lecturaTarjeta, inicio + 1, 4)
                        End Select
                        campo = campo + 1
                        inicio = c
                    End If
                Case ";"
                    track = track + 1
                    principioTrack = c + 1
                    campo = 1
                    inicio = c
            End Select
        Next
        txtLinea.Text = ""
    End Sub


    Private Sub requerimientos(ByVal caja As String)

        ' se debe generar uno nuevo por cada requerimiento !
        Dim ReqTarj As New RequerimientoTarjeta

        With ReqTarj
            .Parametros.VERSION = 1
            .Parametros.TARJ = txtNroTarjetaT2.Text
            .Parametros.CAJERA = 9898
            .Parametros.TRACK1 = txtTrack1.Text
            .Parametros.TRACK2 = txtTrack2.Text
            .Parametros.IMPO = txtimporte.Text
            .Parametros.PLANINT = txtplanpago.Text
            .Parametros.TICCAJ = 454545
            .Parametros.EXPDATE = txtfechavencimiento.Text
            .Parametros.CODSEG = txtcodseg.Text


            .Parametros.MANUAL = 0
            .Parametros.CASHBACK = 0
            .Parametros.cmd = 0
            .Parametros.cajadir = Trim("C:\temp\0" & caja.Substring(0, 1))
            .Parametros.HORA = CDbl(Now.ToOADate) 'VER QUE LA VARIABLE ES UN SINGLE EN EL REG3000 PASA NOW.TOOADATE
            .Parametros.CHECK = "O2"
            .Parametros.oper = 0
            .Enviar()
        End With
        'With ReqTarj
        '    .Parametros.Operacion = TipoRequerimientos.Compra
        '    .Parametros.Importe = CDec(txtimporte.Text)
        '    .Parametros.Tarjeta = txtnumerotarjeta.Text
        '    .Parametros.PLAN = CInt(txtplanpago.Text)
        '    .Parametros.ExpDate = txtfechavencimiento.Text
        '    .Parametros.Track1 = txtTrack1.Text
        '    .Parametros.Track2 = txtTrack2.Text
        '    .Parametros.NroTarj1 = txtNroTarjetaT1.Text
        '    .Parametros.NroTarj2 = txtNroTarjetaT2.Text

        '    .Parametros.CajaID = caja


        '    .Parametros.Ult4Dig = txt4digitos.Text
        '    .Parametros.CodigoSeguridad = Val(txtcodseg.Text)

        '    If .ValidarCaja = "" Then
        '        .Enviar()
        '        'If .Enviar() = Respuesta.Aprobada Then
        '        listaReqtarj.Add(ReqTarj)

        '        'Dim S As Array
        '        'Dim c As Integer

        '        'If .CargarTicket() Then
        '        '    S = .ObtenerTicket.Clone

        '        '    RespuestaServidor.Items.Add(.ObtenerRespuesta)
        '        '    RespuestaServidor.Items.Add("Nro. Autorizacion: " + .NroAutorizacion)
        '        '    RespuestaServidor.Items.Add("Mensaje: " + .Mensaje)
        '        '    RespuestaServidor.Items.Add("Respuesta: " + CStr(.ObtenerRespuesta))
        '        '    RespuestaServidor.Items.Add("----------------------------------------------------------------------------")
        '        '    RespuestaServidor.SelectedIndex = RespuestaServidor.Items.Count - 1

        '        '    For c = 0 To .ObtenerTicket.Length - 1
        '        '        RespuestaTicket.Items.Add(S(c))
        '        '    Next

        '        'Else
        '        '    RespuestaServidor.Items.Add("No existe cupon")
        '        '    RespuestaServidor.SelectedIndex = RespuestaServidor.Items.Count - 1
        '        'End If
        '        'Else
        '        '    RespuestaServidor.Items.Add("Rechazado .." + .MotivoRechazo)
        '        'End If

        '        'Else
        '        '    ErrorAntesMandar.Items.Add(.ValidarCaja)
        '        ' validar tiene el mensaje de error
        '    End If
        'End With



    End Sub

    Private Sub MuestraRespuesta()
        Dim S As Array
        Dim c As Integer
        Timer1.Enabled = False

        'Do While listaReqtarj.Count > 0
        '    With listaReqtarj.Item(0)
        '        If .ObtenerRespuesta = Respuesta.Aprobada Then
        '            If .CargarTicket(.Parametros.CajaID) Then
        '                S = .ObtenerTicket.Clone

        '                RespuestaServidor.Items.Add(.ObtenerRespuesta)
        '                RespuestaServidor.Items.Add("Nro. Autorizacion: " + .NroAutorizacion)
        '                RespuestaServidor.Items.Add("Mensaje: " + .Mensaje)
        '                RespuestaServidor.Items.Add("Respuesta: " + CStr(.ObtenerRespuesta))
        '                RespuestaServidor.Items.Add("----------------------------------------------------------------------------")
        '                RespuestaServidor.SelectedIndex = RespuestaServidor.Items.Count - 1

        '                For c = 0 To .ObtenerTicket.Length - 1
        '                    RespuestaTicket.Items.Add(S(c))
        '                Next

        '            Else
        '                RespuestaServidor.Items.Add("No existe cupon")
        '                RespuestaServidor.SelectedIndex = RespuestaServidor.Items.Count - 1
        '            End If
        '        Else
        '            RespuestaServidor.Items.Add("Rechazado .." + .MotivoRechazo)
        '        End If

        '        listaReqtarj.Remove(listaReqtarj.Item(0))
        '    End With
        'Loop
        Timer1.Enabled = True
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        MuestraRespuesta()
    End Sub

End Class
