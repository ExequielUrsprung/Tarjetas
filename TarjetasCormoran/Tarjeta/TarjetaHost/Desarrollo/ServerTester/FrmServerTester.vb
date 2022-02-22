Imports TjServer
Imports TjComun
Imports IsoLib

Public Class FrmServerTester

    Dim WithEvents Srv As ServerTar
    Dim log As Logeador
    Dim ColaMensajes As New Queue(Of mensajeServer)


    Private Sub FrmServerTester_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        'arrancarlo
        Application.DoEvents()
        Srv.arrancar()
    End Sub



    Private Sub FrmServerTester_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim forceDotCulture As System.Globalization.CultureInfo
        forceDotCulture = Application.CurrentCulture.Clone()
        forceDotCulture.NumberFormat.NumberDecimalSeparator = "."
        forceDotCulture.NumberFormat.NumberGroupSeparator = ","
        forceDotCulture.NumberFormat.CurrencyDecimalSeparator = "."
        forceDotCulture.NumberFormat.CurrencyGroupSeparator = ","
        forceDotCulture.DateTimeFormat.DateSeparator = "/"
        forceDotCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy"
        Application.CurrentCulture = forceDotCulture


        ' crear objeto servidor
        Srv = New ServerTar
        'logeador para mostrar en pantalla.
        log = New Logeador(True)
        Mostrar_versiones()
    End Sub
    Private Sub Mostrar_versiones()
        Me.Text = "Concentrador version 1.3"
        log.logInfo("Version de la aplicacion: 1.3")
        log.logInfo("Version ISOLIB: " & Srv.Version_ISOLIB)
        log.logInfo("Version TJSERVER: " & Srv.Version_SERVERTAR)
        log.logInfo("Version TJCOMUN: " & Srv.Version_TJCOMUN)

    End Sub
    Private Sub llenar_cmbModoIng() Handles Me.Shown
        cmbModoIngreso.Items.Add(IsoLib.E_ModoIngreso.Manual)
        cmbModoIngreso.Items.Add(IsoLib.E_ModoIngreso.Banda)
        cmbModoIngreso.Items.Add(IsoLib.E_ModoIngreso.Chip)
        'cmbModoIngreso.Items.Add(IsoLib.E_ModoIngreso.Ecommerce)
        'cmbModoIngreso.Items.Add(IsoLib.E_ModoIngreso.POS_track2_no)
        'cmbModoIngreso.Items.Add(IsoLib.E_ModoIngreso.POS_track2_si)

        txthost.Items.Add(TjComun.TipoHost.Visa_homolog.ToString)
        txthost.Items.Add(TjComun.TipoHost.Posnet_homolog.ToString)
        txthost.SelectedIndex = 0

        ComboBox1.Items.Add("VISA CREDITO (Ingreso manual)")
        ComboBox1.Items.Add("VISA CREDITO (Lectura tracks)")
        ComboBox1.Items.Add("VISA CREDITO (Vencida)")
        ComboBox1.Items.Add("VISA DEBITO")
        ComboBox1.Items.Add("VISA DEBITO + CASHBACK")
        ComboBox1.Items.Add("MASTERCARD BANDA")
        ComboBox1.Items.Add("MAESTRO BANDA")
        ComboBox1.Items.Add("MASTERCARD MANUAL")
        ComboBox1.Items.Add("MAESTRO MANUAL")
        ComboBox1.SelectedIndex = 0
    End Sub



    ' Formatea los mensajes del server en un
    'Private Sub Srv_Mensaje(msj As mensajeServer) Handles Srv.Mensaje
    '    ColaMensajes.Enqueue(msj)
    'End Sub



    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If log IsNot Nothing Then log.GetArrayAndClear(ListBox1)
        txtnropinguino.Text = Srv.elnropinguino
        Application.DoEvents()

        'Dim n As Integer = 0
        'Do While ColaMensajes.Count > 0
        '    Dim msj = ColaMensajes.Dequeue
        '    Dim s As String = String.Format("{0:dd/MM hh:mm:ss} {1}", msj.Fecha, msj.Texto)
        '    ListBox1.Items.Add(s)
        '    ListBox1.SelectedIndex = ListBox1.Items.Count - 1
        '    n += 1
        '    'esto es para que no se mate sacando mensajes de la colaMensajes
        '    If n > 20 Then Exit Do
        'Loop

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Srv.Detener()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        'Cierre Total
        Srv.CierreDiario()
    End Sub

    Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
        'Cerrar 1 caja
        Srv.CierreDiario(CInt(txtpinguino.Text), CInt(txtcaja.Text), txthost.Text)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        'Echo test
        Srv.ProbarConexion()
    End Sub

    Private Sub btnActualizarDB_Click(sender As Object, e As EventArgs) Handles btnActualizarDB.Click
        Srv.actualizarTablasLocales()
    End Sub
    Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
        Dim ida As New TjComun.IdaTypeInternal
        cargar_ida(ida)
        Srv.ReversarTest(ida, txtTrace.Text, txtTicket.Text, txtTerminal.Text, txthost.Text)
    End Sub
    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        'compra
        Dim ida As New TjComun.IdaTypeInternal
        cargar_ida(ida)
        Srv.venderTest(ida, txtcaja.Text)

    End Sub

    Private Sub cargar_ida(ByRef id As TjComun.IdaTypeInternal)
        id.TARJ = txtnrotarj.Text
        id.CAJERA = CShort(txtcajera.Text)
        id.TRACK1 = txttrack1.Text
        id.TRACK2 = txttrack2.Text
        id.IMPO = CDec(txtimporte.Text)
        id.PLANINT = CShort(Val(txtplan.Text))
        id.TICCAJ = CInt(txtticketcf.Text)
        id.EXPDATE = txtañovenc.Text + txtmesvenc.Text
        id.CODSEG = txtcodseg.Text
        'anulacion
        id.FECORI = txtFecOri.Text
        id.TKTORI = CInt(txtTicket.Text)
        id.PLANINTori = 0
        '--
        id.MANUAL = CShort(txtmodoingreso.Text)
        id.CASHBACK = CDec(txtcashback.Text)
        id.cmd = CShort(txtcmd.Text)
        id.cajadir = Trim("C:\temp\" & CInt(txtpinguino.Text).ToString("00"))
        id.HORA = CSng(Now.ToOADate) 'VER QUE LA VARIABLE ES UN SINGLE EN EL REG3000 PASA NOW.TOOADATE
        id.CHECK = "O2"
        id.oper = CShort(txtoper.Text)
    End Sub


    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        'Anulacion
        Dim ida As New TjComun.IdaTypeInternal
        cargar_ida(ida)
        Srv.venderTest(ida, txtcaja.Text)
        'Srv.anulacionVentaTest(ida, txtcaja.Text)
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        'Devolucion
        Dim ida As New TjComun.IdaTypeInternal
        cargar_ida(ida)
        Srv.devolucionTest(ida)

    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        'Anulacion/Devolucion
        Dim ida As New TjComun.IdaTypeInternal
        cargar_ida(ida)
        Srv.anulacionDevolucionTest(ida)

    End Sub


    Private Sub cmbModoIngreso_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbModoIngreso.SelectedIndexChanged
        If cmbModoIngreso.SelectedIndex = 0 Then txtmodoingreso.Text = "1" '--- manual 
        If cmbModoIngreso.SelectedIndex = 1 Then txtmodoingreso.Text = "0" '--- banda 
        If cmbModoIngreso.SelectedIndex = 2 Then txtmodoingreso.Text = "5" '--- Chip 
        '        If cmbModoIngreso.SelectedIndex = 3 Then txtmodoingreso.Text = "81" '--- Ecommerce 
        '        If cmbModoIngreso.SelectedIndex = 4 Then txtmodoingreso.Text = "2" '--- POS_track2_no 
        '        If cmbModoIngreso.SelectedIndex = 5 Then txtmodoingreso.Text = "90" '--- POS_track2_si 
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        CompletaDatos()

        If ComboBox1.SelectedItem = "VISA CREDITO (Ingreso manual)" Then completar_manual_visa()
        If ComboBox1.SelectedItem = "VISA CREDITO (Lectura tracks)" Then completar_tracks_visa()
        If ComboBox1.SelectedItem = "VISA CREDITO (Vencida)" Then completar_visa_vencida()
        If ComboBox1.SelectedItem = "VISA DEBITO" Then completar_visa_debito()
        If ComboBox1.SelectedItem = "VISA DEBITO + CASHBACK" Then completar_visa_debito_cashback()
        If ComboBox1.SelectedItem = "MASTERCARD BANDA" Then completar_mastercard_banda()
        If ComboBox1.SelectedItem = "MAESTRO BANDA" Then completar_maestro_banda()
        If ComboBox1.SelectedItem = "MASTERCARD MANUAL" Then completar_mastercard_manual()
        If ComboBox1.SelectedItem = "MAESTRO MANUAL" Then completar_maestro_manual()
        'If ComboBox1.SelectedItem = "Track1 VACIO" Then txttrack1.Text = ""
        'If ComboBox1.SelectedItem = "TEST COMPRA POSNET MAESTRO" Then DatosMaestro()
        'If ComboBox1.SelectedItem = "TEST COMPRA POSNET CABAL" Then txttrack1.Text = ""

        'If ComboBox1.SelectedItem = "Track2 VACIO" Then txttrack2.Text = ""
        'If ComboBox1.SelectedItem = "Fecha Vencimiento VACIA" Then
        '    txtmesvenc.Text = ""
        '    txtañovenc.Text = ""
        'End If
        'If ComboBox1.SelectedItem = "Fecha Vencimiento VENCIDA" Then
        '    txtmesvenc.Text = "03"
        '    txtañovenc.Text = "15"
        'End If
        'If ComboBox1.SelectedItem = "Codigo Seguridad VACIO" Then txtcodseg.Text = ""
        'If ComboBox1.SelectedItem = "Numero Tarjeta VACIO" Then txtnrotarj.Text = ""

        'If ComboBox1.SelectedItem = "Importe VACIO" Then txtimporte.Text = ""

        'If ComboBox1.SelectedItem = "Plan VACIO" Then txtplan.Text = ""
        'If ComboBox1.SelectedItem = "Plan NO EXISTENTE" Then txtplan.Text = "874"

    End Sub

    Private Sub Button16_Click(sender As Object, e As EventArgs)

        txtnrotarj.Text = "4507990000004905"
        txtcajera.Text = 9898
        txttrack1.Text = "4507990000004905^PRUEBA/IMPRE         ^160310199999        00546000000?      "
        txttrack2.Text = "4507990000004905=16031015460000000000"
        txtimporte.Text = 450
        txtplan.Text = 0
        txtticketcf.Text = 454545
        txtmesvenc.Text = "03"
        txtañovenc.Text = "15"
        txtcodseg.Text = "678"
        txtcashback.Text = 0
        txtcmd.Text = 0
        cmbModoIngreso.SelectedIndex = 1
        txtoper.Text = 0
        'txthost.SelectedIndex = 0
        txtcaja.Text = "15"
        txtpinguino.Text = "4"
        'txtTerminal.Text = "72009901"
        ' id.MANUAL.text = 0
        ' id.CHECK.text = "O2"
        ' id.HORA.text = Now.Hour ' TODO: VER QUE LA VARIABLE ES UN SINGLE EN EL REG3000 PASA NOW.TOOADATE

    End Sub

    Private Sub completar_mastercard_banda()
        txthost.SelectedIndex = 1
        txtnrotarj.Text = "2222439997001019"
        txtcajera.Text = "9898"
        txttrack1.Text = "2222439997001019^PRUEBA 1                  ^1710101170040000000800000?"
        txttrack2.Text = "2222439997001019=17101011700400000008"
        txtimporte.Text = "10"
        txtplan.Text = "0"
        txtticketcf.Text = "200"
        txtmesvenc.Text = "10"
        txtañovenc.Text = "17"
        txtcodseg.Text = "000"
        txtcashback.Text = "0"
        txtcmd.Text = "0"
        cmbModoIngreso.SelectedIndex = 1
        txtoper.Text = "0"
        txtcaja.Text = "15"
        txtpinguino.Text = "4"
        txtTerminal.Text = "06000099"
    End Sub
    Private Sub completar_mastercard_manual()
        txthost.SelectedIndex = 1
        txtnrotarj.Text = "2222439997001019"
        txtcajera.Text = "9898"
        txttrack1.Text = ""
        txttrack2.Text = ""
        txtimporte.Text = "10"
        txtplan.Text = "0"
        txtticketcf.Text = "200"
        txtmesvenc.Text = "10"
        txtañovenc.Text = "17"
        txtcodseg.Text = "000"
        txtcashback.Text = "0"
        txtcmd.Text = "0"
        cmbModoIngreso.SelectedIndex = 0
        txtoper.Text = "0"
        txtcaja.Text = "15"
        txtpinguino.Text = "4"
        txtTerminal.Text = "06000099"
    End Sub
    Private Sub completar_maestro_banda()
        txthost.SelectedIndex = 1
        txtnrotarj.Text = "501063999000007007"
        txtcajera.Text = "9898"
        txttrack1.Text = "501063999000007007^MAESTRO VERIFICACION POS^9912101             ?"
        txttrack2.Text = "501063999000007007=991210100000000008"
        txtimporte.Text = "10"
        txtplan.Text = "0"
        txtticketcf.Text = "200"
        txtmesvenc.Text = "12"
        txtañovenc.Text = "24"
        txtcodseg.Text = ""
        txtcashback.Text = "0"
        txtcmd.Text = "0"
        cmbModoIngreso.SelectedIndex = 1
        txtoper.Text = "0"
        txtcaja.Text = "15"
        txtpinguino.Text = "4"
        txtTerminal.Text = "06000099"
    End Sub
    Private Sub completar_maestro_manual()
        txthost.SelectedIndex = 1
        txtnrotarj.Text = "501063999000007007"
        txtcajera.Text = "9898"
        txttrack1.Text = ""
        txttrack2.Text = ""
        txtimporte.Text = "10"
        txtplan.Text = "0"
        txtticketcf.Text = "200"
        txtmesvenc.Text = "12"
        txtañovenc.Text = "24"
        txtcodseg.Text = ""
        txtcashback.Text = "0"
        txtcmd.Text = "0"
        cmbModoIngreso.SelectedIndex = 0
        txtoper.Text = "0"
        txtcaja.Text = "15"
        txtpinguino.Text = "4"
        txtTerminal.Text = "06000099"
    End Sub


    Private Sub completar_manual_visa()
        txthost.SelectedIndex = 0
        txtnrotarj.Text = "4507990000977787"
        txtcajera.Text = "9898"
        txttrack1.Text = ""
        txttrack2.Text = ""
        txtimporte.Text = "10"
        txtplan.Text = "0"
        txtticketcf.Text = "196"
        txtmesvenc.Text = "05"
        txtañovenc.Text = "19"
        txtcodseg.Text = "648"
        txtcashback.Text = "0"
        txtcmd.Text = "0"
        cmbModoIngreso.SelectedIndex = 0
        txtoper.Text = "0"
        txtcaja.Text = "15"
        txtpinguino.Text = "4"
        txtTerminal.Text = "72009901"
    End Sub

    Private Sub completar_tracks_visa()
        txthost.SelectedIndex = 0
        'datos de tarjeta de prueba.
        txtnrotarj.Text = "4507990000977787"
        txtcajera.Text = "9898"
        txttrack1.Text = "4507990000977787^PRUEBA/PRUEBA        ^190510109999        00036000000?"
        txttrack2.Text = "4507990000977787=19051010360000000000"
        txtimporte.Text = "10"
        txtplan.Text = 0
        txtticketcf.Text = "196"
        txtmesvenc.Text = "05"
        txtañovenc.Text = "19"
        txtcodseg.Text = "648"
        txtcashback.Text = 0
        txtcmd.Text = 0
        cmbModoIngreso.SelectedIndex = 1
        txtoper.Text = "0"
        txtcaja.Text = "15"
        txtpinguino.Text = "4"
        txtTerminal.Text = "72009901"
    End Sub

    Private Sub completar_visa_vencida()
        'datos de tarjeta de prueba.
        txthost.SelectedIndex = 0
        txtnrotarj.Text = "4507990000977787"
        txtcajera.Text = "9898"
        txttrack1.Text = "4507990000004905^PRUEBA/IMPRE         ^150310199999        00546000000?"
        txttrack2.Text = "4507990000004905=15031015460000000000"
        txtimporte.Text = "10"
        txtplan.Text = "0"
        txtticketcf.Text = "196"
        txtmesvenc.Text = "03"
        txtañovenc.Text = "15"
        txtcodseg.Text = "678"
        txtcashback.Text = "0"
        txtcmd.Text = "0"
        cmbModoIngreso.SelectedIndex = 1
        txtoper.Text = "0"
        txtcaja.Text = "15"
        txtpinguino.Text = "4"
        txtTerminal.Text = "72009901"
    End Sub

    Private Sub completar_visa_debito()
        txthost.SelectedIndex = 0
        txtnrotarj.Text = "4487790000000018"
        txtcajera.Text = "9898"
        txttrack1.Text = "4487790000000018^PRUEBA/DEBITO            ^171112110000        00984000000?"
        txttrack2.Text = "4487790000000018=17111219840000000000"
        txtimporte.Text = "10"
        txtplan.Text = "0"
        txtticketcf.Text = "196"
        txtmesvenc.Text = "11"
        txtañovenc.Text = "17"
        txtcodseg.Text = "0"
        txtcashback.Text = "0"
        txtcmd.Text = "0"
        cmbModoIngreso.SelectedIndex = 1
        txtoper.Text = "0"
        txtcaja.Text = "15"
        txtpinguino.Text = "4"
        txtTerminal.Text = "72009901"
    End Sub

    Private Sub completar_visa_debito_cashback()
        txthost.SelectedIndex = 0
        txtnrotarj.Text = "4487790000000018"
        txtcajera.Text = "9898"
        txttrack1.Text = "4487790000000018^PRUEBA/DEBITO            ^171112110000        00984000000?"
        txttrack2.Text = "4487790000000018=17111219840000000000"
        txtimporte.Text = "20"
        txtplan.Text = "800"
        txtticketcf.Text = "196"
        txtmesvenc.Text = "11"
        txtañovenc.Text = "17"
        txtcodseg.Text = "0"
        txtcashback.Text = "10"
        txtcmd.Text = "0"
        cmbModoIngreso.SelectedIndex = 1
        txtoper.Text = "0"
        txtcaja.Text = "15"
        txtpinguino.Text = "4"
        txtTerminal.Text = "72009901"
    End Sub


    Private Sub DatosMaestro()
        txthost.SelectedIndex = 1
        txtnrotarj.Text = "5002650123400000001"
        txtcajera.Text = 9898
        txttrack1.Text = "5002650123400000001^PRUEBA MAESTRO^9912101?                                  "
        txttrack2.Text = "4507990000004905=16031015460000000000"
        txtimporte.Text = 450
        txtplan.Text = 991
        txtticketcf.Text = 454545
        txtmesvenc.Text = "03"
        txtañovenc.Text = "16"
        txtcodseg.Text = "11341234123412341"
        txtcashback.Text = 0
        txtcmd.Text = 0
        cmbModoIngreso.SelectedIndex = 1
        txtoper.Text = 0
        txthost.SelectedIndex = 0
        txtcaja.Text = "15"
        txtpinguino.Text = "4"
        txtTerminal.Text = "75804015"
        ' id.MANUAL.text = 0
        ' id.CHECK.text = "O2"
        ' id.HORA.text = Now.Hour ' TODO: VER QUE LA VARIABLE ES UN SINGLE EN EL REG3000 PASA NOW.TOOADATE

    End Sub


    Private Sub CompletaDatos()
        txtnrotarj.Text = "4507990000004905"
        txtcajera.Text = 9898
        txttrack1.Text = "4507990000004905^PRUEBA/IMPRE         ^160310199999        00546000000?      "
        txttrack2.Text = "4507990000004905=16031015460000000000"
        txtimporte.Text = 450
        txtplan.Text = 0
        txtticketcf.Text = 454545
        txtmesvenc.Text = "03"
        txtañovenc.Text = "15"
        txtcodseg.Text = "678"
        txtcashback.Text = 0
        txtcmd.Text = 0
        cmbModoIngreso.SelectedIndex = 1
        txtoper.Text = 0
        'txthost.SelectedIndex = 0
        txtcaja.Text = "15"
        txtpinguino.Text = "4"
        'txtTerminal.Text = "72009901"
        ' id.MANUAL.text = 0
        ' id.CHECK.text = "O2"
        ' id.HORA.text = Now.Hour ' TODO: VER QUE LA VARIABLE ES UN SINGLE EN EL REG3000 PASA NOW.TOOADATE

    End Sub


    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        System.IO.File.Copy("c:\temp\CAJA0008.IDZ", "c:\temp\04\CAJA0015.IDZ")
    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        System.IO.File.Copy("c:\temp\CAJA0003.IDZ", "c:\temp\03\CAJA0003.IDZ")
        System.IO.File.Copy("c:\temp\CAJA0008.IDZ", "c:\temp\06\CAJA0008.IDZ")
        System.IO.File.Copy("c:\temp\CAJA0010.IDZ", "c:\temp\01\CAJA0010.IDZ")
        System.IO.File.Copy("c:\temp\CAJA0012.IDZ", "c:\temp\04\CAJA0012.IDZ")
    End Sub


    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        System.IO.File.Copy("c:\temp\CAJA0003.IDZ", "c:\temp\01\CAJA0003.IDZ")
        System.IO.File.Copy("c:\temp\CAJA0008.IDZ", "c:\temp\01\CAJA0008.IDZ")
        System.IO.File.Copy("c:\temp\CAJA0010.IDZ", "c:\temp\01\CAJA0010.IDZ")
        System.IO.File.Copy("c:\temp\CAJA0012.IDZ", "c:\temp\01\CAJA0012.IDZ")
    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        System.IO.File.Copy("c:\temp\CAJA0008.IDZ", "c:\temp\01\CAJA0008.IDZ")
    End Sub

    Private Sub Button15_Click(sender As Object, e As EventArgs) Handles Button15.Click
        Srv.Parar_Escuchador()
        If Srv.Consultar_pendientes = 0 And Srv.Consultar_Confirmados = 0 Then
            Srv.Detener()
            Me.Close()
        Else
            log.logInfo("QUEDAN MOVIMIENTOS POR CONFIRMAR, POR FAVOR ESPERE...")
        End If
    End Sub



    Private Sub txtconectar(hst As String, conec As Boolean)
        If hst = TipoHost.VISA.ToString Or hst = TipoHost.Visa_homolog.ToString Then
            If Not conec Then
                txtVisa.BackColor = Color.Red
                txtVisa.ForeColor = Color.Maroon
                txtVisa.Text = "DESCONECTADO"
            Else
                txtVisa.BackColor = Color.LawnGreen
                txtVisa.ForeColor = Color.LimeGreen
                txtVisa.Text = "CONECTADO"
            End If
        ElseIf hst = TipoHost.POSNET.ToString Or hst = TipoHost.Posnet_homolog.ToString Then
            If Not conec Then
                txtPosnet.BackColor = Color.Red
                txtPosnet.ForeColor = Color.Maroon
                txtPosnet.Text = "DESCONECTADO"
            Else
                txtPosnet.BackColor = Color.LawnGreen
                txtPosnet.ForeColor = Color.LimeGreen
                txtPosnet.Text = "CONECTADO"
            End If
        End If
    End Sub

    Delegate Sub Control_Conexion(hst As String, con As Boolean)


    Public Sub Control_Desconectado(hst As String) Handles Srv.Desconectado
        Dim d As New Control_Conexion(AddressOf txtconectar)
        Me.Invoke(d, New Object() {hst, False})
    End Sub

    Public Sub Control_Conectado(hst As String) Handles Srv.Conectado
        Dim d As New Control_Conexion(AddressOf txtconectar)
        Me.Invoke(d, New Object() {hst, True})
    End Sub

    Private Sub txtoper_TextChanged(sender As Object, e As EventArgs) Handles txtoper.TextChanged
        If txtoper.Text = "0" Then Label27.Text = "Venta"
        If txtoper.Text = "1" Then Label27.Text = "Devol"
        If txtoper.Text = "2" Then Label27.Text = "Anula"
        If txtoper.Text = "3" Then Label27.Text = "An/De"
    End Sub

    Private Sub Button16_Click_1(sender As Object, e As EventArgs) Handles Button16.Click
        Srv.Sincronizar("POSNET_HOMOLOG")
    End Sub
End Class
