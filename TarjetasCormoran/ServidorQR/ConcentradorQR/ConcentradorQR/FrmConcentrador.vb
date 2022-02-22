Imports ServerQR
Imports TjComun
'Imports IsoLib
Public Class FrmConcentrador
    Dim WithEvents Srv As ServerTar
    Dim log As Logeador
    Delegate Sub Control_Conexion(hst As String, con As Boolean)

    Private Sub FrmConcentrador_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim forceDotCulture As System.Globalization.CultureInfo
        forceDotCulture = Application.CurrentCulture.Clone()
        forceDotCulture.NumberFormat.NumberDecimalSeparator = "."
        forceDotCulture.NumberFormat.NumberGroupSeparator = ","
        forceDotCulture.NumberFormat.CurrencyDecimalSeparator = "."
        forceDotCulture.NumberFormat.CurrencyGroupSeparator = ","
        forceDotCulture.DateTimeFormat.DateSeparator = "/"
        forceDotCulture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy"
        Application.CurrentCulture = forceDotCulture



        Me.ListView1.View = View.Details
        Me.ListView1.Font = New System.Drawing.Font("Courier New", 9, FontStyle.Regular)
        Me.ListView1.Columns.Clear()
        Me.ListView1.Columns.Add(" ", Me.Width * 8, HorizontalAlignment.Left)
        Me.ListView1.HeaderStyle = ColumnHeaderStyle.None

        ' crear objeto servidor
        Srv = New ServerTar
        'lblDirectorio.Text = Srv.DirectorioTrabajo
        'logeador para mostrar en pantalla.
        log = New Logeador(True)
        Mostrar_versiones()
    End Sub


    Private Sub Mostrar_versiones()
        If Srv.Tipo_QR = "P" Then
            lblTipo.Text = "Productivo"
        Else
            lblTipo.Text = "Test"
        End If

        Me.Text = "Concentrador MercadoPago version 3"
        log.logInfo("Concentrador MercadoPago version 3")
        log.logInfo("Version TJSERVER: " & Srv.Version_SERVERTAR)
        log.logInfo("Version TJCOMUN: " & Srv.Version_TJCOMUN)
        log.logInfo("Version LIBQR: " & Srv.Version_LIBQR)
        lblVersionAPLICACION.Text = "Concentrador MercadoPago version 3"
        lblVersionTJSERVER.Text = "Version ServerTar: " & Srv.versionServerTar
        lblVersionTJCOMUN.Text = "Version TJCOMUN: " & Srv.Version_TJCOMUN
        lblVersionLibQR.Text = "Version LIBQR: " & Srv.Version_LIBQR
        If Srv.Tipo_QR = "P" Then Button3.Visible = False
    End Sub

    Private Sub FrmConcentrador_Shown(sender As Object, e As EventArgs) Handles Me.Shown
        'arrancarlo
        Application.DoEvents()
        Srv.arrancar()
        Timer1.Enabled = True

    End Sub


    Dim t As Integer = 0
    Dim cerrando As Boolean = False
    Dim reiniciando As Boolean = False
    Dim anular As Boolean = False

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        If Now.TimeOfDay.ToString("hhmmss") = Srv.HoraCierre And Not cerrando And Configuracion.TipoCierre = Configuracion.CierreAut Then
            log.logInfo("Solicitud de cierre diario automático.")
            cerrar()
            log.logInfo("Limpiando listview")
            ListView1.Items.Clear()
        End If
        'If Now.TimeOfDay.ToString("hhmmss") = Srv.HoraReinicio And Not reiniciando Then
        '    reiniciando = True
        '    log.logInfo("Petición de reinicio del equipo.")
        '    Srv.Detener()
        '    System.Threading.Thread.Sleep(10000)
        '    System.Diagnostics.Process.Start("shutdown", "-r -t 00 -f")
        'End If
        If ListView1.Items.Count > 30000 Then
            log.logInfo("Limpiando listview")
            ListView1.Items.Clear()
        End If

        If Srv.estEscuchador Then
            lblChequeando.Text = "Buscando..."
            Application.DoEvents()
            If t = 5 Then
                lblChequeando.Visible = Not lblChequeando.Visible
                t = 0
            Else
                t += 1
            End If
        Else
            If Not anular Then
                'If Srv.Consultar_pendientes = 0 And Srv.Consultar_Confirmados = 0 And Not Srv.generandoVTA Then
                '    anular = True
                '    Srv.Detener()
                '    lblChequeando.Text = "Detenido..."
                'Else
                '    lblChequeando.Text = "Deteniendo..."
                'End If
                lblChequeando.Visible = True
            End If

            '        lblChequeando.Text = "Detenido..."
            Application.DoEvents()
        End If
        If log IsNot Nothing Then log.GetArrayAndClear(ListView1)
        ' txtPinguino.Text = Srv.elnropinguino
        Application.DoEvents()
    End Sub



    'Private Sub btnSalir_Click(sender As Object, e As EventArgs) Handles btnSalir.Click
    '    log.logWarning("Solicitud de salida del sistema.")
    '    If Srv.estadoSrv = True Then
    '        log.logerror("EL SISTEMA ESTA FUNCIONANDO, PRIMERO PRESIONE DETENER SERVIDOR ...")
    '    Else
    '        Me.Close()
    '    End If
    '    'Srv.Parar_Escuchador()

    '    'If Srv.Consultar_pendientes = 0 And Srv.Consultar_Confirmados = 0 Then
    '    '    Srv.Detener()
    '    '    Me.Close()
    '    'Else
    '    '    log.logWarning("QUEDAN MOVIMIENTOS POR CONFIRMAR, POR FAVOR ESPERE...")
    '    'End If
    'End Sub

    'Private Sub btnArrancarSrv_Click(sender As Object, e As EventArgs) Handles btnArrancarSrv.Click
    '    log.logWarning("Solicitud de arranque del servidor.")
    '    Srv.arrancar()
    '    anular = False
    'End Sub

    'Private Sub btnDetenerSrv_Click(sender As Object, e As EventArgs) Handles btnDetenerSrv.Click

    '    If validado Or validadoComputos Then
    '        log.logWarning("Solicitud para detener el servidor.")
    '        Srv.Parar_Escuchador()
    '        If Srv.Consultar_pendientes = 0 And Srv.Consultar_Confirmados = 0 And Not Srv.generandoVTA Then
    '            'Srv.Detener()
    '        Else
    '            log.logWarning("QUEDAN MOVIMIENTOS POR CONFIRMAR, POR FAVOR ESPERE...")
    '        End If
    '    Else
    '        log.logWarning("Usuario no válido o contraseña incorrecta.")
    '    End If

    '    Borrar_validacion()

    'End Sub

    Private Sub cerrar()
        log.logInfo("Comenzó el proceso de cierre.")
        cerrando = True
        Srv.Parar_Escuchador()

        Srv.CierreQR()


        cerrando = False
    End Sub
    Dim validado As Boolean = False
    Dim validadoComputos As Boolean = False
    Private Sub CIERRE_Click(sender As Object, e As EventArgs) Handles CIERRE.Click

        log.logInfo("Solicitud de cierre diario manual.")
        If MsgBox("Va a realizar el cierre de diario, esta seguro?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo + vbDefaultButton2, "Aviso") = MsgBoxResult.Yes Then
            cerrar()
        End If

    End Sub
    'Private Sub Borrar_validacion()
    '    txtValidacion.Text = ""
    '    validado = False
    '    validadoComputos = False
    '    Label2.Text = False
    'End Sub


    'Private Sub btnActualizar_Click(sender As Object, e As EventArgs) Handles btnActualizar.Click

    '    If validadoComputos Then
    '        log.logInfo("Solicitud de actualización de tablas")
    '        If MsgBox("Va a realizar una actualización de las tablas, esta seguro?", MsgBoxStyle.Exclamation + MsgBoxStyle.YesNo + vbDefaultButton2, "Aviso") = MsgBoxResult.Yes Then
    '            Srv.actualizarTablasLocales()
    '        End If
    '    Else
    '        log.logWarning("Usuario no válido o contraseña incorrecta.")
    '    End If

    '    Borrar_validacion()

    'End Sub



    'Private Sub txtValidacion_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtValidacion.KeyPress
    '    If e.KeyChar = Chr(13) Then
    '        If txtValidacion.Text = "35528hgd3" Then
    '            log.logInfo("Computos logueado")
    '            Label2.Text = "Computos"
    '            validadoComputos = True
    '        ElseIf txtValidacion.Text = "supervisora" Then
    '            log.logInfo("Supevisora logueada")
    '            Label2.Text = "Supervisora"
    '            validado = True
    '        End If
    '    End If
    'End Sub

    'Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles Button1.Click
    '    'importacion de movimientos desde los exportados


    '    Dim arc As String = ""

    '    If validadoComputos Then
    '        OpenFileDialog1.InitialDirectory = Srv.DirectorioTrabajo & "\Exportados"
    '        OpenFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
    '        OpenFileDialog1.FilterIndex = 2
    '        OpenFileDialog1.RestoreDirectory = True

    '        If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
    '            Try
    '                arc = OpenFileDialog1.FileName
    '                If arc <> "" Then
    '                    Dim cant As Int16 = 0
    '                    ' Insert code to read the stream here.
    '                    cant = Srv.importar(arc)
    '                    MsgBox(String.Format("Importación finalizada. Se importaron {0} registros.", cant))
    '                End If
    '            Catch Ex As Exception
    '                MessageBox.Show("No se puede importar este archivo: " & Ex.Message)
    '            End Try
    '        End If
    '    Else
    '        log.logWarning("Usuario no válido o contraseña incorrecta.")
    '    End If
    '    Borrar_validacion()

    'End Sub

    Private Sub Importar_Click(sender As Object, e As EventArgs) Handles importar.Click
        'importacion de movimientos desde los exportados

        Dim arc As String = ""
        OpenFileDialog1.InitialDirectory = Srv.DirectorioTrabajo & "\Exportados"
        OpenFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*"
        OpenFileDialog1.FilterIndex = 2
        OpenFileDialog1.RestoreDirectory = True

        If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            Try
                arc = OpenFileDialog1.FileName
                If arc <> "" Then
                    Dim cant As Int16 = 0
                    ' Insert code to read the stream here.
                    cant = Srv.importar(arc)
                    MsgBox(String.Format("Importación finalizada. Se importaron {0} registros.", cant))
                End If
            Catch Ex As Exception
                MessageBox.Show("No se puede importar este archivo: " & Ex.Message)
            End Try
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim qr As New IdaLib.idaQR


        'Srv.Cargar_sucursales()

        'For x As Integer = 1 To 20
        '    Srv.Cargar_terminales("1", x.ToString("00"))
        'Next
        'For x As Integer = 1 To 3
        '    Srv.Cargar_terminales("2", x.ToString("00"))
        'Next
        'For x As Integer = 1 To 7
        '    Srv.Cargar_terminales("3", x.ToString("00"))
        'Next
        'For x As Integer = 1 To 16
        '    Srv.Cargar_terminales("4", x.ToString("00"))
        'Next
        'For x As Integer = 1 To 4
        '    Srv.Cargar_terminales("5", x.ToString("00"))
        'Next
        'For x As Integer = 1 To 13
        '    Srv.Cargar_terminales("6", x.ToString("00"))
        'Next
        'For x As Integer = 1 To 5
        '    Srv.Cargar_terminales("7", x.ToString("00"))
        'Next
        'For x As Integer = 1 To 7
        '    Srv.Cargar_terminales("8", x.ToString("00"))
        'Next

        'qr.CAJERO = "9898"
        'qr.IMPORTE = "9982"
        'qr.TIPOOPERACION = "COMPRA"
        'qr.ticket = "2"
        'IdaLib.GrabarQR(qr, "c:\temp\04\CAJA0002.QR")


        'qr.CAJERO = "9898"
        'qr.IMPORTE = "6400"
        'qr.TIPOOPERACION = "COMPRA"
        ''qr.TIPOOPERACION = "CANCELA"
        'qr.ticket = "4444"
        'IdaLib.GrabarQR(qr, "c:\temp\04\CAJA0015.QR")


        'qr.CAJERO = "9898"
        'qr.IMPORTE = "6400"
        ''qr.TIPOOPERACION = "COMPRA"
        'qr.TIPOOPERACION = "CANCELA"
        'qr.ticket = "4444"
        'IdaLib.GrabarQR(qr, "c:\temp\04\CAJA0015.QR")

        qr.CAJERO = "9898"
        qr.IMPORTE = "7900"
        qr.TIPOOPERACION = "COMPRA"
        'qr.TIPOOPERACION = "CANCELA"
        qr.ticket = "4444"
        IdaLib.GrabarQR(qr, "c:\temp\04\CAJA0015.QR")




        'qr.CAJERO = "9898"
        'qr.IMPORTE = "40082"
        'qr.TIPOOPERACION = "COMPRA"
        'qr.ticket = "2"
        'IdaLib.GrabarQR(qr, "c:\temp\04\CAJA0003.QR")

        'qr.CAJERO = "9898"
        'qr.IMPORTE = "30082"
        'qr.TIPOOPERACION = "COMPRA"
        'qr.ticket = "2"
        'IdaLib.GrabarQR(qr, "c:\temp\04\CAJA0005.QR")

        'qr.CAJERO = "9898"
        'qr.IMPORTE = "569806"
        'qr.TIPOOPERACION = "COMPRA"
        'qr.ticket = "2"
        'IdaLib.GrabarQR(qr, "c:\temp\04\CAJA0004.QR")

        'qr.CAJERO = "9898"
        'qr.IMPORTE = "1082"
        'qr.TIPOOPERACION = "COMPRA"
        'qr.ticket = "2"
        'IdaLib.GrabarQR(qr, "c:\temp\04\CAJA0001.QR")
    End Sub

    Private Sub btnSalir_Click(sender As Object, e As EventArgs) Handles btnSalir.Click
        Srv.Detener()
        Me.Close()
    End Sub
End Class


Public Class ListViewDoubleBuffered
    Inherits System.Windows.Forms.ListView
    Sub New()
        Me.DoubleBuffered = True
    End Sub
End Class