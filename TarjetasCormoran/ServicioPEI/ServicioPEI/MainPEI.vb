Imports System.Timers

Public Class MainPEI
    Dim t As New Timers.Timer



    Protected Overrides Sub OnContinue()
        t.Start()
    End Sub

    Protected Overrides Sub OnPause()
        t.Stop()
    End Sub

    Protected Overrides Sub OnStart(ByVal args() As String)


        t = New Timers.Timer

        AddHandler t.Elapsed, AddressOf buscarArchivo

        t.Interval = 1000



        t.Start()

        ' Agregue el código aquí para iniciar el servicio. Este método debería poner
        ' en movimiento los elementos para que el servicio pueda funcionar.
    End Sub

    Private Function buscarArchivo() As Boolean
        'Dim sr As New IO.StreamReader("c:\temp\mensajepei.txt")

        'sr.ReadLine()
        'armarTRX(sr)
        'sr.Close()
        Dim DirBusq As String
        Dim arc As String = ""
        Dim ext As String = "*.PEI"
        Dim ping As String = ""

        DirBusq = "C:\temp"


        'puse el control de bandera aca porque se pueden solapar los hilos del timer si se demora adentro de algun proceso.
        'inhibe el timer hasta que salga el hilo anterior.

        Try
            Dim sr As IO.StreamReader
            For Each arc In System.IO.Directory.GetFiles(DirBusq + "\", ext)

                sr = New IO.StreamReader(arc)

                sr.ReadLine()
                armarTRX(sr)
                sr.Close()
                System.IO.File.Delete(arc)

                RaiseEvent NuevoIda(ida, CInt(Mid(arc, arc.Length - 5, 2)))
            Next

        Catch ex As IO.DirectoryNotFoundException

        Catch ex1 As IO.IOException

        Catch ex2 As Exception
        End Try


    End Function

    Private Sub armarTRX(sr)
        Dim datos = sr.ToString.Split("|")


    End Sub


    Protected Overrides Sub OnStop()


        t.Stop()
        ' Agregue el código aquí para realizar cualquier anulación necesaria para detener el servicio.
    End Sub

End Class
