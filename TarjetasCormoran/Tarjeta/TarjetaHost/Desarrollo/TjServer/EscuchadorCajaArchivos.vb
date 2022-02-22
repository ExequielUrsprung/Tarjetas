Imports System.Timers
'Imports System.Security.Cryptography

Class EscuchadorCajaArchivos
    Dim srv As ServerTar
    Dim WithEvents timer As New Timer

    Public Event NuevoIda(ida As TjComun.IdaTypeInternal, caja As Integer)
    Public Event NuevoVtaPEI(vta As lib_PEI.Respuesta, Terminal As String)

    Dim dirPing As String()

    Dim DirBusq As String = Configuracion.DirTarjetas
    Dim dirlocal As String = Configuracion.DirLocal
    Dim activo As Boolean = False


    Sub New(server As ServerTar)
        srv = server
        If Configuracion.Ciudad = Configuracion.Rafaela Then
            dirPing = {"01", "02", "03", "04", "05", "06", "07", "08"}
            'dirPing = {"04"}
        Else
            dirPing = {"06"}
        End If
    End Sub

    Sub detener()
        timer.Stop()

        For Each pin As String In dirPing
            Try
                Logger.Info(String.Format("Borrando {0}\CONCE.OP...", pin))
                System.IO.File.Delete(DirBusq + "\" + pin + "\CONCE.OP")
            Catch
                Logger.Error(String.Format("No se pudo crear {0}\CONCE.OP", pin))
            End Try
        Next

    End Sub

    Sub arrancar()
        timer.Interval = 200
        timer.Enabled = True
        timer.Start()
        For Each pin As String In dirPing
            Try
                Logger.Info(String.Format("Creando {0}\CONCE.OP...", pin))
                System.IO.File.Create(DirBusq + "\" + pin + "\CONCE.OP").Close()
            Catch
                Logger.Error(String.Format("No se pudo crear {0}\CONCE.OP", pin))
            End Try
        Next

        activo = True
    End Sub



    Public Property Estado As Boolean
        Set(value As Boolean)
            activo = value
        End Set
        Get
            Return activo
        End Get
    End Property

    Public bandera As Boolean


    Dim ptr As Byte = 0


    Private Sub timer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles timer.Elapsed

        'Dim arc As String = ""
        'Dim ext As String = "*.IDZ"
        Dim ping As String = ""
        Dim arc2 As System.IO.FileInfo
        'Dim arc3 As System.IO.FileInfo

        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or srv.Tipoconfiguracion <> "Produccion" Then
            DirBusq = "C:\temp"
        End If

        '--- MUESTRA EN PANTALLA COMO VA RECORRIENDO LOS PINGUINOS  
        ping = dirPing(ptr)
        If ptr = dirPing.Count - 1 Then
            ptr = 0
        Else
            ptr += 1
        End If
        'Try
        srv.MuestraNroPing(ping)

        '--- puse el control de bandera aca porque se pueden solapar los hilos del timer si se demora adentro de algun proceso.  
        '--- inhibe el timer hasta que salga el hilo anterior.                                                                   
        If Not bandera Then
            bandera = True

            If srv.procesandoVta = False Then srv.ProcesarVtaPendientes()       '--- para que no se solapen los hilos
            srv.ProcesarPendientes()

            Try
                If activo Then 'desactiva para que no escuche mas

                    Dim dirVtaPEI As New System.IO.DirectoryInfo(dirlocal + "\RespuestasPEI\")    '--- dirlocal = c:\tarjetas 
                    For Each arc2 In dirVtaPEI.GetFiles
                        If arc2.Extension.ToUpper = ".VTA" Then
                            Logger.Info("****************** LEYENDO .VTA EscuchadorCajaArchivos ******************")
                            Logger.Info(String.Format("-------- Nueva Respuesta PEI de Terminal: {0}  --------", arc2.Name))
                            Logger.Info(String.Format("         Leyendo archivo {0}", arc2.Name))
                            Dim respPEI As New lib_PEI.Respuesta
                            Dim archi = New IO.StreamReader(arc2.FullName)
                            Dim resString = archi.ReadToEnd
                            Logger.Info(resString)
                            respPEI.LeerRespuesta(resString)
                            archi.Close()
                            Logger.Info(String.Format("Eliminando archivo {0}", arc2.FullName))
                            System.IO.File.Delete(arc2.FullName)

                            '--- RESPUESTAS DE LA LECTURA DEL .VTA     
                            'Logger.Info(respPEI.descRespuesta)
                            'Logger.Info(respPEI.descripcion)
                            'Logger.Info(respPEI.tipoOperacion)
                            'Logger.Info(respPEI.id_operacion)
                            'Logger.Info(respPEI.nro_ref_bancaria)
                            'Logger.Info(respPEI.id_operacion_origen)
                            'Logger.Info(respPEI.fechahora_respuesta)
                            'Logger.Info(respPEI.trace)

                            'MsgBox(Mid(arc2.Name, 1, arc2.Name.Length - 4))
                            'MsgBox(Mid(arc2.Name, arc2.Name.Length - 3, 4))
                            'RaiseEvent NuevoVtaPEI(respPEI, arc2.Name)     '--- ASI LO TENIA MAURO, PERO AGREGABA AL PING01 EL .vta, Y DABA ERROR EN listaReqPendientesOnline 
                            '                                                --- CUANDO LO QUIERE SACAR, PORQUE NO LO ENCUENTRA                                                
                            If Mid(arc2.Name, arc2.Name.Length - 3, 4) = ".vta" Then
                                RaiseEvent NuevoVtaPEI(respPEI, Mid(arc2.Name, 1, arc2.Name.Length - 4))
                            Else
                                RaiseEvent NuevoVtaPEI(respPEI, arc2.Name)
                            End If
                        End If
                    Next

                    '--- DirBusq=\\pinguino1-1\sys\tarjetas\visa 
                    Dim directorio As New System.IO.DirectoryInfo(DirBusq + "\" + ping + "\")
                    For Each arc2 In directorio.GetFiles
                        'If Not bandera Then 
                        'bandera = True
                        If arc2.Extension.ToUpper = ".IDZ" Then
                            Logger.Info(arc2.FullName)

                            Logger.Info(String.Format("-------- Nueva Transacción de Caja: {0} Ping: {1} --------", arc2.Name.Substring(6, 2), ping) & vbNewLine &
                                        String.Format("         Leyendo archivo {0}", arc2.Name))
                            Dim ida = TjComun.IdaLib.LeerIdz(arc2.FullName)
                            Logger.Info(String.Format("Eliminando archivo {0}", arc2.FullName))
                            System.IO.File.Delete(arc2.FullName)
                            ida.cajadir = Trim(DirBusq + "\" + ping)
                            RaiseEvent NuevoIda(ida, CInt(Mid(arc2.Name, arc2.Name.Length - 5, 2)))
                        End If
                    Next
                End If
            Catch ex As IO.DirectoryNotFoundException
                Logger.Warn(String.Format("No existe el directorio, {0}\{1} ", DirBusq, ping))

            Catch ex1 As IO.IOException
                If arc2 IsNot Nothing Then
                    Logger.Warn(String.Format("No se pudo leer {0} {1}", arc2.FullName, ex1.Message))
                End If

            Catch ex2 As Exception
                If IsoLib.declaraciones.StringLog IsNot Nothing Then Logger.Warn(IsoLib.declaraciones.StringLog)

                Logger.Error(String.Format(vbNewLine &
                                           "Excepción desconocida" & vbNewLine &
                                           "--- [   Message] {0}" & vbNewLine &
                                           "--- [    Source] {1}" & vbNewLine &
                                           "--- [TargetSite] {2}" & vbNewLine &
                                           "--- [StackTrace] {3}", ex2.Message, ex2.Source, ex2.TargetSite, ex2.StackTrace))

                Logger.Info(String.Format(vbNewLine &
                                           "Excepción desconocida" & vbNewLine &
                                           "--- [   Message] {0}" & vbNewLine &
                                           "--- [    Source] {1}" & vbNewLine &
                                           "--- [TargetSite] {2}" & vbNewLine &
                                           "--- [StackTrace] {3}", ex2.Message, ex2.Source, ex2.TargetSite, ex2.StackTrace))

            Finally
                bandera = False
            End Try
        End If

    End Sub

End Class