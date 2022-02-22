Imports log4net
Imports log4net.Config
Imports Trx.Messaging
Imports Trx.Utilities
Imports Trx.Messaging.Iso8583
Imports ONLINECOM
' <Assembly: log4net.Config.XmlConfigurator()> 

Public Class Form1
    '' banelco test messages
    Dim Log As New Logeador.Logeador(True)
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim formatter As Trx.Messaging.Iso8583.Iso8583MessageFormatter
        
        formatter = New Iso8583Nexo(E_Implementaciones.Banelco)
        Mostrar("usando " + formatter.Name)
        Dim context As New Trx.Messaging.ParserContext(ParserContext.DefaultBufferSize)
        Dim s As String
        Dim m As Trx.Messaging.Message
        If CheckBox1.Checked Then
            Dim s2 As String = TextBox1.Text
            s = ""
            Dim s3() As String = s2.Split(Chr(13))
            For Each s2 In s3
                s2 = s2.Replace(Chr(10), "")
                s = s + (s2 + Space$(7 + 73)).Substring(6, 73)
            Next

        Else
            s = TextBox1.Text
        End If


        context.Write(s)
        With formatter


            m = .Parse(context)

            Mostrar(m.ToString())
            Debug.Print(m.ToString)



        End With
    End Sub

    Public Sub Mostrar(ByVal s As String)
        ListBox1.Items.Add(s)

    End Sub

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        'Dim i As log4net.ILog
        'i = LogManager.GetLogger("root")
        'BasicConfigurator.Configure()

        ComboBox1.SelectedItem = 0
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox1.SelectedIndexChanged

    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ComboBox2.SelectedIndexChanged
        Dim s As String
        Select Case ComboBox2.SelectedIndex
            Case 0
                s = Banelco0800
            Case 1
                s = Banelco0810
            Case 2
                s = banelco0200
            Case 3
                s = banelco0210
            Case 4
                s = banelco0420
            Case 5
                s = Banelco0430
        End Select
        TextBox1.Text = s
    End Sub

    Private Sub CheckBox1_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CheckBox1.CheckedChanged

    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Log.GetArrayAndClear(ListBox2)
    End Sub
End Class
