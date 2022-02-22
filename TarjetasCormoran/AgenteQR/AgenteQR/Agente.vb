Module Agente
    Dim WithEvents ClienteQR As lib_QR.QR
    Sub Main()
        Try
            Dim respTransaccion As Boolean
            '"COMPRA@caja15p4-000753|caja15p4|62|0|@caja15p4@T"
            Dim sp As String() = Command.Split("@")

            '--- aca se instancia el objeto LIB_QR
            ClienteQR = New lib_QR.QR(sp(3))
            ClienteQR.nomarc = sp(2) & "-" & Now.Date.ToString("yyyyMMdd") & ".txt" 'terminal-fecha.txt
            ClienteQR.nomvta = sp(2) & ".vta"

            ClienteQR.Log("VERSION AGENTE QR: 3")
            ClienteQR.Log("VERSION LIBQR: " & ClienteQR.version)
            ClienteQR.Log(Command)
            ClienteQR.Log(sp(0) & "   " & sp(1))

            '--- seteo el puerto
            If sp(3) = "T" Then
                ClienteQR.puerto = "9292"    '--- test
            Else
                ClienteQR.puerto = My.Settings.puerto.Trim
            End If

            If sp(0) = "COMPRA" Or sp(0) = "DEVOLUCION" Then
                respTransaccion = ClienteQR.Operar(sp(1), sp(0))   '--- esta en el LIB-QR
            ElseIf sp(0) = "CANCELA" Then
                respTransaccion = ClienteQR.Operar(sp(1), sp(0))

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

End Module
