Public Class FormQR
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click '-crear suc
        Dim qr As New lib_QR.QR("T")
        Dim id As String
        id = qr.Crear_Sucursales("Pinguino 1", "Bv. Lehmann", "425", "Rafaela", "Santa Fe", "-31.248960", "-61.491130", "1")
        id = qr.Crear_Sucursales("Pingüino 2", "Av. Ernesto Salva", "960", "Rafaela", "Santa Fe", "", "", "2")
        id = qr.Crear_Sucursales("Pingüino 3", "Aristobulo del valle", "884", "Rafaela", "Santa Fe", "", "", "3")
        id = qr.Crear_Sucursales("Pingüino 4", "Roque Saenz Peña", "321", "Rafaela", "Santa Fe", "", "", "4")
        id = qr.Crear_Sucursales("Pingüino 5", "Velez Sarsfield", "1441", "Rafaela", "Santa Fe", "", "", "5")
        id = qr.Crear_Sucursales("Pingüino 6", "25 de Mayo", "1129", "San Francisco", "Cordoba", "", "", "6")
        id = qr.Crear_Sucursales("Pingüino 7", "Gobernador Crespo", "285", "Rafaela", "Santa Fe", "", "", "7")
        id = qr.Crear_Sucursales("Pingüino 8", "Av. Luis Fanti", "295", "Rafaela", "Santa Fe", "", "", "8")
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click '-borrar suc
        Dim qr As New lib_QR.QR("T")
        qr.Borrar_sucursales()

    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click '-modificar suc

    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click '-crear caja
        Dim qr As New lib_QR.QR("T")
        'qr.Crear_Caja()
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click 'borrar caja
        Dim qr As New lib_QR.QR("T")
        qr.Borrar_Cajas()
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click '-modificar caja
        Dim qr As New lib_QR.QR("T")
        qr.Modificar_cajas("3513170")
        'qr.Traer_cajas()
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        'Dim qr As New lib_QR.QR
        'qr.nomarc = "caja15p4"
        'qr.terminal = "caja15p4"

        'Dim resp = qr.Generar_orden("12840", "C15P4-00002")
        'Label1.Text = "GENERADO"

        Dim ClienteQR As New lib_QR.QR("T")

        Try
            Dim respTransaccion As Boolean
            Dim sp As String() = "COMPRA@caja15p4-000017|caja15p4|83.76|0|@caja15p4@T".Split("@")
            ClienteQR.nomarc = sp(2) & "-" & Now.Date.ToString("yyyyMMdd") & ".txt" 'terminal-fecha.txt
            ClienteQR.nomvta = sp(2) & ".vta"

            ClienteQR.Log("VERSION AGENTE QR: 1")
            'ClienteQR.Log("VERSION LIBQR: " & ClienteQR.versionQR)
            'ClienteQR.Log(Command)
            ClienteQR.Log(sp(0) & "   " & sp(1))
            If sp(0) = "COMPRA" Or sp(0) = "DEVOLUCION" Then

                respTransaccion = ClienteQR.Operar(sp(1), sp(0))
            ElseIf sp(0) = "CANCELA" Then

                '    ClienteQR.Log("VERSION AGENTE QR: 1")
                '    ClienteQR.Log("VERSION LIBQR: " & ClienteQR.versionQR)
                '    ClienteQR.Log(Command)
                '    ClienteQR.Log(sp(0) & "   " & sp(1))

                respTransaccion = ClienteQR.Operar(sp(2), sp(0))

            End If
            If respTransaccion = True Then
                ClienteQR.Log("Finalizado")
            Else
                ClienteQR.Log("No hubo respuesta")
            End If
        Catch ex As Exception

            ClienteQR.Log("Parametro enviado: " & Command() & vbNewLine)
            ClienteQR.Log("Error en la ejecucion: " & ex.Message & vbNewLine &
                "--------- STACK: " & ex.StackTrace & vbNewLine &
                "--------- Source: " & ex.Source)
            End
        End Try
    End Sub


    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        Dim qr As New lib_QR.QR("T")
        qr.nomarc = "caja15p4"

        qr.Cancelar_orden("")

    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        Dim qr As New lib_QR.QR("T")
        qr.nomarc = "caja15p4"

        qr.Obtener_MerchantOrder("5364670380")
    End Sub

    'Dim WithEvents t1 As New Timers.Timer
    'Dim status As String
    'Private Sub BuscarIPN() Handles t1.Elapsed

    '    For Each arc In System.IO.Directory.GetFiles("C:\TARJETAS\RESPUESTASQR\IPN\", "*")
    '        System.IO.File.Delete(arc)

    '        Dim id_operacion = Mid(arc.Split("\")(4), 1, arc.Split("\")(4).Length)


    '        Dim qr As New lib_QR.QR
    '        qr.nomarc = "caja15p4"
    '        status = qr.Obtener_MerchantOrder(id_operacion.Split("-")(0))




    '    Next
    'End Sub

    Private Sub FormQR_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        't1.Interval = 500
        't1.Enabled = True
        't1.Start()
        ' Timer1.Start()
    End Sub

    'Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
    '    Label1.Text = status
    'End Sub
End Class