Module Agente
    Dim WithEvents ClientePEI As New lib_PEI.PEI
    'Dim WithEvents t1 As New Timers.Timer
    Dim arranco As DateTime

    Sub Main()
        arranco = Now
        't1.Interval = 1000
        't1.Enabled = True
        't1.Start()
        Dim respTransaccion As lib_PEI.Respuesta
        Try
            Dim sp As String() = Command.Split("@")
            'ClientePEI.nomarc = sp(2) & "-" & Now.Date.ToString("yyyyMMdd") & ".txt"  'terminal-fecha.txt


            '--- sp(0) tipoop - "c"  (c=compra)      
            '--- sp(1) trx - "4815500001111111^LEONARDI/MARCOS P        ^260822110000        00859000000?|4815500001111111=26082218590000000000|9983|SAZ250-2200922|PAGOCORMORAN|SAZ250|PEIBANDA|20190618||1540|1929,00|0|0"
            '--- sp(2) terminal            
            '--- sp(3) direccion           
            '--- sp(4) comercio - "1540"   
            '--- sp(5) trace - "2200922"   
            '--- sp(6) pinguino            
            '--- sp(7) caja                

            ClientePEI.nomarc = "LogPEI-" & Now.Date.ToString("yyyyMMdd") & "_" & sp(6) & sp(7) & ".txt"
            ClientePEI.nomvta = sp(2) & ".vta"
            ClientePEI.direccion = sp(3)
            ClientePEI.Log("*********************************************************************************************")
            ClientePEI.Log("*********************************************************************************************")
            ClientePEI.Log("VERSION AGENTE PEI: 15")
            ClientePEI.Log("VERSION LIBPEI: " & ClientePEI.versionLIBPEI)
            ClientePEI.Log("TERMINAL: " & sp(2))
            ClientePEI.Log("Comercio: " & sp(4) & " - Trace: " & sp(5))


            respTransaccion = ClientePEI.Operar(sp(1), sp(0), sp(4), sp(5))     '---  trx,tipoop,comercio, trace   


            If respTransaccion IsNot Nothing Then
                'ClientePEI.Log("Respuesta PEI (descripcion): " & respTransaccion.descripcion)
                'ClientePEI.Log("Respuesta PEI (id_operacion): " & respTransaccion.id_operacion)
                ClientePEI.Log("Finalizado (AgentePEI)")
                ClientePEI.Log("*********************************************************************************************")
                ClientePEI.Log("*********************************************************************************************")
            Else
                ClientePEI.Log("No hubo respuesta")
            End If

        Catch ex As Exception
            ClientePEI.Log("Parametro enviado: " & Command() & vbNewLine)
            ClientePEI.Log("Error en la ejecucion: " & ex.Message & vbNewLine &
                           "--------- STACK: " & ex.StackTrace & vbNewLine &
                           "--------- Source: " & ex.Source)
            ClientePEI.Log("Finalizado CON ERROR")
            ClientePEI.Log("*********************************************************************************************")
            ClientePEI.Log("*********************************************************************************************")
            End
        End Try
    End Sub


    'Private Sub t1_elapsed() Handles t1.Elapsed
    '    If Now.Subtract(arranco).TotalSeconds > 60 Then
    '        ClientePEI.Log("Se cerró forzadamente.")
    '        End
    '    End If
    'End Sub

End Module
