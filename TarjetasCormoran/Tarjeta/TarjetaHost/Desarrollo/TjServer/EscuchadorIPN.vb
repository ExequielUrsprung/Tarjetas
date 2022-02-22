Imports System.Timers
Public Class EscuchadorIPN
    Dim WithEvents timer As New Timers.Timer
    Dim activo As Boolean = False
    Dim dirlocal As String = Configuracion.DirLocal
    Public Event NuevoIPN(ipn As String)
    Public Event NuevoVtaQR(vta As lib_QR.Respuesta, Terminal As String)
    Dim srv As ServerTar
    Sub detener()
        timer.Stop()
        Logger.Warn("Escuchador IPN detenido")
        activo = False
    End Sub

    Sub arrancar()
        timer.Interval = 1500
        timer.Enabled = True
        timer.Start()
        Logger.Info("Escuchador IPN iniciado")
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
    Sub New(server As ServerTar)
        srv = server
    End Sub
    Public bandera As Boolean
    Private Sub timer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles timer.Elapsed
        Dim arc As System.IO.FileInfo
        Dim directorio As New IO.DirectoryInfo(dirlocal + "\RespuestasQR\")

        If Not bandera Then
            bandera = True
            'srv.ProcesarQRPendientes()

            Try
                If activo Then 'desactiva para que no escuche mas

                    '----QR
                    For Each arc In directorio.GetFiles
                        If arc.Extension.ToUpper = ".VTA" Then

                            Logger.Info(String.Format("-------- Nueva Respuesta QR de Terminal: {0}  --------", arc.FullName) & vbNewLine &
                                        String.Format("         Leyendo archivo {0}", arc.Name))
                            Dim respQR As New lib_QR.Respuesta
                            Try
                                Dim archi = New IO.StreamReader(arc.FullName)
                                Dim resString = archi.ReadToEnd
                                Logger.Info(resString)
                                respQR.LeerRespuesta(resString)
                                archi.Close()
                                Logger.Info(String.Format("Eliminando archivo {0}", arc))
                                System.IO.File.Delete(arc.FullName)
                                RaiseEvent NuevoVtaQR(respQR, arc.Name.Substring(0, arc.Name.Length - 4))
                            Catch ex As Exception
                                Logger.Warn("No se pudo leer respuesta " & arc.Name)

                            End Try


                        ElseIf arc.Extension.ToUpper = ".IPN" Then

                            Logger.Info(String.Format("-------- Nueva IPN: {0}  --------", arc.Name) & vbNewLine &
                                        String.Format("         Leyendo archivo {0}", arc.Name))

                            Logger.Info(String.Format("Eliminando archivo {0}", arc.FullName))
                            System.IO.File.Delete(arc.FullName)

                            RaiseEvent NuevoIPN(arc.Name.Substring(0, arc.Name.Length - 4))
                        End If
                    Next
                End If

            Catch ex As IO.DirectoryNotFoundException
                Logger.Warn(String.Format("No existe el directorio, {0} ", dirlocal))

            Catch ex1 As IO.IOException
                If arc IsNot Nothing Then
                    Logger.Warn(String.Format("No se pudo leer {0} {1}", arc, ex1.Message))
                End If

            Catch ex2 As Exception
                If IsoLib.declaraciones.StringLog IsNot Nothing Then Logger.Warn(IsoLib.declaraciones.StringLog)

                Logger.Error(String.Format(vbNewLine &
                                               "Excepción desconocida" & vbNewLine &
                                               "--- [   Message] {0}" & vbNewLine &
                                               "--- [    Source] {1}" & vbNewLine &
                                               "--- [TargetSite] {2}" & vbNewLine &
                                               "--- [StackTrace] {3}", ex2.Message, ex2.Source, ex2.TargetSite, ex2.StackTrace))

            End Try
        End If
        bandera = False
    End Sub


    'Private Sub timer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles timer.Elapsed
    '    Dim arc As String = ""

    '    If Not bandera Then
    '        bandera = True
    '        srv.ProcesarQRPendientes()

    '        Try
    '            If activo Then 'desactiva para que no escuche mas

    '                '----QR

    '                For Each arc In System.IO.Directory.GetFiles(dirlocal + "\RespuestasQR\", "*.IPN")
    '                    Logger.Info(String.Format("-------- Nueva IPN: {0}  --------", Mid(arc.Split("\")(3), 1, arc.Split("\")(3).Length - 3)) & vbNewLine &
    '                                        String.Format("         Leyendo archivo {0}", arc))

    '                    Logger.Info(Mid(arc.Split("\")(3), 1, arc.Split("\")(3).Length - 4))
    '                    Logger.Info(String.Format("Eliminando archivo {0}", arc))
    '                    System.IO.File.Delete(arc)

    '                    RaiseEvent NuevoIPN(Mid(arc.Split("\")(3), 1, arc.Split("\")(3).Length - 4))
    '                Next
    '            End If

    '        Catch ex As IO.DirectoryNotFoundException
    '            Logger.Warn(String.Format("No existe el directorio, {0} ", dirlocal))

    '        Catch ex1 As IO.IOException
    '            Logger.Warn(String.Format("No se pudo leer {0} {1}", arc, ex1.Message))

    '        Catch ex2 As Exception
    '            If IsoLib.declaraciones.StringLog IsNot Nothing Then Logger.Warn(IsoLib.declaraciones.StringLog)

    '            Logger.Error(String.Format(vbNewLine &
    '                                           "Excepción desconocida" & vbNewLine &
    '                                           "--- [   Message] {0}" & vbNewLine &
    '                                           "--- [    Source] {1}" & vbNewLine &
    '                                           "--- [TargetSite] {2}" & vbNewLine &
    '                                           "--- [StackTrace] {3}", ex2.Message, ex2.Source, ex2.TargetSite, ex2.StackTrace))

    '        End Try
    '    End If
    '    bandera = False
    'End Sub

End Class
