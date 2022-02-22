Imports System.IO
Imports System.IO.Compression
Imports Compactador

Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            Button1.Enabled = False
            Button2.Enabled = False
            Label3.Text = "Compactando"
            Escribe_LOG("                   Comenzando la compactación.")
            Escribe_LOG(String.Format("Desde {0} Hasta {1}", DateTimePicker1.Value.Date, DateTimePicker2.Value.Date))
            Escribe_LOG("===================================================================================")

            Dim compactador As New Compactador.CompressFiles
            compactador.Compactar(DateTimePicker1.Value.Date, DateTimePicker2.Value.Date)

            Escribe_LOG("                      FIN DE LA COMPACTACION")
            Escribe_LOG("===================================================================================")
            Button1.Enabled = True
            Button2.Enabled = True
            Label4.Text = ""
            Label3.Text = "Compactacion Finalizada"
        Catch ex As Exception
            Escribe_LOG("Mensaje: " & ex.Message)
            Escribe_LOG("Pila de llamadas: " & ex.StackTrace)
            Escribe_LOG("Fuente: " & ex.Source)
            Escribe_LOG("Método: " & ex.TargetSite.ToString)

        End Try

    End Sub

    Private Sub Escribe_LOG(v As String)
        Using Log As FileStream = New FileStream("c:\Tarjetas\Log.txt", FileMode.Append)
            Using texto As StreamWriter = New StreamWriter(Log)
                Label4.Text = v
                texto.WriteLine("(" & Now.Date.ToString("dd-MM-yyyy") & "  " & Now.ToString("HH:mm:ss") & ") - " & v)
            End Using
        End Using
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles Me.Load
        DateTimePicker1.Value = Now.Date
        DateTimePicker2.Value = Now.Date
        Label3.Text = ""
        Label4.Text = ""
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If MsgBox("Esta seguro de vaciar la carpeta de borrados?", vbYesNo, "Confirmación") = vbYes Then
            Try
                Escribe_LOG("Borrando directorio c:\tarjetas\borrar")
                Directory.Delete("c:\Tarjetas\borrar", True)
                Escribe_LOG("Directorio borrado.")
            Catch ex As Exception
                Escribe_LOG("No existe el directorio")
                MsgBox("No existe el directorio")
            End Try
        End If
    End Sub
End Class
