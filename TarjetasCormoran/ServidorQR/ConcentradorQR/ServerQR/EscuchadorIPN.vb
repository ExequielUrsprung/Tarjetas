Imports System.Timers
Public Class EscuchadorIPN
    Dim WithEvents timer As New Timers.Timer
    Dim WithEvents timer_lector As New Timers.Timer
    Dim WithEvents timer_atrasadas As New Timers.Timer
    Const MAX_TIMERS_LECTORES As Integer = 4 ' siempre hay que pensar que hay 3 threads minimos sin contar estos
    Dim timers_lectores_actuales As Integer = 0
    Dim procesando_atrasadas As Boolean = False
    Dim last_vacio As Boolean = False
    Dim last_db_vacio As Boolean = False
    Dim activo As Boolean = False
    Dim dirlocal As String = Configuracion.DirLocal
    Public Event NuevoIPN(ipn As String)
    Public Event NuevoVtaQR(vta As lib_QR.Respuesta, Terminal As String)
    Dim srv As ServerTar
    Sub detener()
        timer.Stop()
        timer_lector.Stop()
        timer_atrasadas.Stop()

        Logger.Warn("Escuchador IPN detenido")
        activo = False
    End Sub

    Sub arrancar()
        timer.Interval = 500
        timer.Enabled = True
        timer.Start()

        timer_lector.Interval = 500
        timer_lector.Enabled = True
        timer_lector.Start()

        timer_atrasadas.Interval = 1500
        timer_atrasadas.Enabled = True
        timer_atrasadas.Start()

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
    ' Private Shared banderaIPN As Boolean
    Private Sub timer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles timer.Elapsed

        If Not activo Then
            Exit Sub
        End If

        timer.Stop()

        Dim arc As System.IO.FileInfo
        Dim directorio As New IO.DirectoryInfo(dirlocal + "\RespuestasQR\")

        Try
            For Each arc In directorio.GetFiles
                If arc.Extension.ToUpper = ".VTA" OrElse arc.Extension.ToUpper = ".IPN" Then
                    ColaIPN.Instance.add_to_waiting_queue(arc.FullName)
                End If
            Next

        Catch ex As IO.DirectoryNotFoundException
            Logger.Warn(String.Format("No existe el directorio, {0} ", dirlocal))

        Catch ex1 As IO.IOException
            If arc IsNot Nothing Then
                Logger.Warn(String.Format("No se pudo leer {0} {1}", arc, ex1.Message))
            End If

        Catch ex2 As Exception

            Logger.Error(String.Format(vbNewLine &
                                                "Excepción desconocida" & vbNewLine &
                                                "--- [   Message] {0}" & vbNewLine &
                                                "--- [    Source] {1}" & vbNewLine &
                                                "--- [TargetSite] {2}" & vbNewLine &
                                                "--- [StackTrace] {3}", ex2.Message, ex2.Source, ex2.TargetSite, ex2.StackTrace))

        End Try

        timer.Start()


    End Sub
    Private Sub timer_atrasadas_Elapsed(sender As Object, e As ElapsedEventArgs) Handles timer_atrasadas.Elapsed

        timer_atrasadas.Stop()

        'Controla el timer lectores por si se freno
        If timers_lectores_actuales < MAX_TIMERS_LECTORES AndAlso Not timer_lector.Enabled Then
            timer_lector.Start()
        End If

        If Not srv.ProcesarQRPendientes() Then
            If Not last_db_vacio Then
                Logger.Warn("No existen ordenes atrasadas en la DB")
                last_db_vacio = True
            End If
            timer_atrasadas.Start()
            Exit Sub
        End If
        last_db_vacio = False

        timer_atrasadas.Start()

    End Sub
    Private Sub timer_lector_Elapsed(sender As Object, e As ElapsedEventArgs) Handles timer_lector.Elapsed
        Dim arc As String

        If timers_lectores_actuales >= MAX_TIMERS_LECTORES Then
            Logger.Warn("Maximo de lectores alcanzado")
            timer_lector.Stop()
            Exit Sub
        End If

        timers_lectores_actuales += 1

        Try
            arc = ColaIPN.Instance.take_from_waiting_queue()

            Dim arc_name As String

            arc_name = Split(arc, "\").Last


            If arc_name.ToUpper.IndexOf(".VTA") <> -1 Then

                Logger.Info(String.Format("-------- Nueva Respuesta QR de Terminal: {0}  --------", arc) & vbNewLine &
                                String.Format("         Leyendo archivo {0}", arc_name))
                Dim respQR As New lib_QR.Respuesta
                Try
                    Dim archi = New IO.StreamReader(arc)
                    Dim resString = archi.ReadToEnd
                    Logger.Info(resString)
                    archi.Close()
                    System.IO.File.Delete(arc)     '--- borra el vta leido
                    respQR.LeerRespuesta(resString)

                    Logger.Info(String.Format("Eliminando archivo {0}", arc))

                    ColaIPN.Instance.final_procc(arc)

                    RaiseEvent NuevoVtaQR(respQR, arc_name.Substring(0, arc_name.Length - 4))

                Catch ex As Exception
                    Logger.Warn("No se pudo leer respuesta " & arc_name)

                End Try


            ElseIf arc_name.ToUpper.IndexOf(".IPN") <> -1 Then

                Logger.Info(String.Format("-------- Nueva IPN: {0}  --------", arc_name) & vbNewLine &
                            String.Format("         Leyendo archivo {0}", arc_name))

                Logger.Info(String.Format("Eliminando archivo {0}", arc))
                System.IO.File.Delete(arc)

                ColaIPN.Instance.final_procc(arc)

                RaiseEvent NuevoIPN(arc_name.Substring(0, arc_name.Length - 4))

            End If
        Catch ex As IO.DirectoryNotFoundException
            Logger.Warn(String.Format("No existe el directorio, {0} ", dirlocal))

        Catch ex1 As IO.IOException
            If arc IsNot Nothing Then
                Logger.Warn(String.Format("No se pudo leer {0} {1}", arc, ex1.Message))
            End If

        Catch ioe As InvalidOperationException
            If Not last_vacio Then
                Logger.Warn(String.Format("No hay archivos en espera: {0}", ioe.Message))
                last_vacio = True
            End If

            timers_lectores_actuales -= 1

            Exit Sub
        Catch ex2 As Exception

            Logger.Error(String.Format(vbNewLine &
                                               "Excepción desconocida" & vbNewLine &
                                               "--- [   Message] {0}" & vbNewLine &
                                               "--- [    Source] {1}" & vbNewLine &
                                               "--- [TargetSite] {2}" & vbNewLine &
                                               "--- [StackTrace] {3}", ex2.Message, ex2.Source, ex2.TargetSite, ex2.StackTrace))

        End Try
        last_vacio = False

        timers_lectores_actuales -= 1

    End Sub


End Class
