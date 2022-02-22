Imports System.Timers
'Imports System.Security.Cryptography

Class EscuchadorCajaArchivos
    Dim srv As ServerTar
    Dim WithEvents timer As New Timer
    Public Event NuevoQR(qr As TjComun.idaQR, caja As Integer, ping As Integer)


    Dim dirPing As String()
    Dim DirBusq As String = Configuracion.DirTarjetas
    Dim dirlocal As String = Configuracion.DirLocal
    Dim activo As Boolean = False
    Dim funciona_qr As Boolean
    Sub New(server As ServerTar)
        srv = server
        If Configuracion.Ciudad = Configuracion.Rafaela Then
            dirPing = {"01", "02", "03", "04", "05", "06", "07", "08"}
        Else
            dirPing = {"06"}
        End If
        funciona_qr = srv.Qr_activo
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

        'arrancarqr()
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
    Public banderaQR As Boolean

    Dim ptr As Byte = 0
    Dim ptrqr As Byte = 0


    Private Sub timer_Elapsed(sender As Object, e As ElapsedEventArgs) Handles timer.Elapsed

        If Not activo Or Not funciona_qr Then 'desactiva para que no escuche mas
            Exit Sub
        End If

        timer.Stop()

        Dim arc As String = ""
        Dim ping As String = ""
        Dim arc2 As System.IO.FileInfo

        If My.Computer.Name = "MARCOS-XP" Or srv.Tipoconfiguracion <> "Produccion" Then
            DirBusq = "C:\temp"
        End If
        '------------------------------------------------------------------------------------
        '--- ACA BUSCA EN LAS CARPETAS SEGUN EL PINGUINO EL ARCHIVO QR QUE GENERO LA CAJA 
        '------------------------------------------------------------------------------------
        ping = dirPing(ptr)
        If ptr = dirPing.Count - 1 Then
            ptr = 0
        Else
            ptr += 1
        End If
        'Try
        'srv.MuestraNroPing(ping)

        'puse el control de bandera aca porque se pueden solapar los hilos del timer si se demora adentro de algun proceso.
        'inhibe el timer hasta que salga el hilo anterior.

        'srv.ProcesarQRPendientes()
        Try

            Dim directorio As New System.IO.DirectoryInfo(DirBusq + "\" + ping + "\")
            For Each arc2 In directorio.GetFiles("*.QR")

                Logger.Info(String.Format("-------- Nueva requerimiento QR: {0}  --------", arc2.Name) & vbNewLine &
                                String.Format("         Leyendo archivo {0}", arc2.FullName))

                'Dim archi = New IO.StreamReader(arc)
                Dim reqQR = TjComun.IdaLib.LeerQR(arc2.FullName)

                'archi.Close()
                Logger.Info(String.Format("Eliminando archivo {0}", arc2.FullName))

                '--- ACA BORRA EL QR QUE ESTA EN LA CARPETA Y YA LEYO
                System.IO.File.Delete(arc2.FullName)

                '--- DISPARA UN EVENTO   Sub ProcQR dentro de ServerTar
                RaiseEvent NuevoQR(reqQR, CInt(arc2.Name.Substring(6, 2)), ping)

            Next
        Catch ex As IO.DirectoryNotFoundException
            Logger.Warn(String.Format("No existe el directorio, {0}\{1} ", DirBusq, ping))

        Catch ex1 As IO.IOException
            If arc2 IsNot Nothing Then
                Logger.Warn(String.Format("No se pudo leer {0} {1}", arc2.FullName, ex1.Message))
            End If

        Catch ex2 As Exception


            Logger.Error(String.Format(vbNewLine &
                                           "Excepción desconocida" & vbNewLine &
                                           "--- [   Message] {0}" & vbNewLine &
                                           "--- [    Source] {1}" & vbNewLine &
                                           "--- [TargetSite] {2}" & vbNewLine &
                                           "--- [StackTrace] {3}", ex2.Message, ex2.Source, ex2.TargetSite, ex2.StackTrace))
        Finally
            timer.Start()

        End Try


    End Sub

End Class