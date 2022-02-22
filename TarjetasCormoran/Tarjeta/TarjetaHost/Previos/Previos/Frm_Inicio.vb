Imports log4net
Imports log4net.Config
Public Class Frm_Inicio
    Dim OcupadoTimer2 As Boolean
    Private Sub Frm_Inicio_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason = Windows.Forms.CloseReason.UserClosing Then
            Dim s As String = "Desea cerrar el sistema?"
            If HaySAFPendiente() > 0 Then
                s = s + ", existen " + HaySAFPendiente.ToString + " SAF pendientes de grabar, espere unos instantes para intentarlo nuevamente"
            End If
            If MsgBox(s, MsgBoxStyle.OkCancel, "ATENCION") = MsgBoxResult.Cancel Then
                e.Cancel = True
                Exit Sub
            End If
        End If
        log.logWarning("Cerrando Aplicacion " + e.CloseReason.ToString + " en " + My.Settings.Entorno)
        TmrEmail.Enabled = False
        TmrReconect.Enabled = False
        Timer2.Enabled = False
        Timer2.Enabled = False
        Finalizar()

    End Sub

    Private Sub Frm_Inicio_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            TextBox1.Text = My.Settings.IpLlamar
            Arrancar(Me)
            Timer2.Enabled = True
            Label1.Text = My.Settings.Entorno
            Label2.Text = EscuchadorImplementado.ToString
            PropertyGrid1.SelectedObject = ONLINECOM.declaraciones.Settings
            grilla.SelectedObject = My.Settings
            Me.Text = Label2.Text + " " + Label1.Text + " " + My.Application.Info.Version.ToString
            TmrEmail.Enabled = True
            GrpDesarrollo.Enabled = My.Settings.Entorno = "TES"
        Catch ex As Exception
            MsgBox("Error " + ex.Message + " " + ex.StackTrace)
        End Try
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        'Actualiza Listbox
        log.GetArrayAndClear(ListBox1, ListBox2)
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        ListBox1.Items.Clear()

    End Sub

  
    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        conectar(TextBox1.Text)

    End Sub
    

    Private Sub TabPage1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TabPage1.Click

    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        desconectar()
    End Sub

    Private Sub TabPage3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TabPage3.Click

    End Sub

    Private Sub TabPage2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TabPage2.Click

    End Sub

    Private Sub Button4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button4.Click
        DataGridView1.DataSource = conexion.EjecutarConsulta(My.Settings.SQLAutorizaciones)
    End Sub

    Private Sub Button5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button5.Click
        DataGridView1.SelectAll()
    End Sub

    Private Sub Button6_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button6.Click
        Reconectar("Se reconecto manualmente")
    End Sub

    Private Sub TmrReconect_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TmrReconect.Tick
        If DebeReconectar Then

            DebeReconectar = False
            Reconectar("Reconexion Solicitada")
            DebeReconectar = False
        End If
    End Sub

    Private Sub Button7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        log.logWarning("prueba de Warning")
    End Sub

    Private Sub Button8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        log.logerror("prueba de ERROR")
    End Sub

    Private Sub Timer2_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer2.Tick
        If OcupadoTimer2 Then Exit Sub
        OcupadoTimer2 = True
        PasoTiempo()
        OcupadoTimer2 = False
    End Sub

    Private Sub TmrEmail_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TmrEmail.Tick
        MandarColaEmails()
    End Sub

   

    Private Sub Button9_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button9.Click
        Probar(TextBox2.Text)
    End Sub

    Private Sub Button8_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button8.Click

    End Sub

    Private Sub FileSystemWatcher1_Changed(ByVal sender As System.Object, ByVal e As System.IO.FileSystemEventArgs)

    End Sub
End Class