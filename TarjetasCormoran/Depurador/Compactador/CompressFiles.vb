Imports System.IO.Compression
Imports System.IO
Public Class CompressFiles
    Public Sub Compactar(Fecha_Inicial As Date, Fecha_Final As Date)
        Dim cantdias = DateDiff(DateInterval.Day, Fecha_Inicial.Date, Fecha_Final.Date)
        Dim fecha_recorrida As Date = Fecha_Inicial
        Escribe_LOG("Creando C:\Tarjetas\Borrar")
        Directory.CreateDirectory("C:\Tarjetas\Borrar")

        If Directory.Exists("C:\Tarjetas\Borrar") Then

            For x = 0 To cantdias
                Dim hayalgo As Boolean = False
                Escribe_LOG(String.Format("        <<<<  {0} >>>>", fecha_recorrida.Date.ToString("dd-MM-yyyy")))
                Escribe_LOG(String.Format("Creando {0}", "c:\Tarjetas\Compactados\" & fecha_recorrida.Date.ToString("yyMMdd") & ".zip"))
                Using zipToOpen As FileStream = New FileStream("c:\Tarjetas\Compactados\" & fecha_recorrida.Date.ToString("yyMMdd") & ".zip", FileMode.OpenOrCreate)

                    Using archive As ZipArchive = New ZipArchive(zipToOpen, ZipArchiveMode.Update)

                        Escribe_LOG(String.Format(" "))
                        Escribe_LOG(String.Format("    c:\tarjetas\cierres"))

                        Dim dir As New DirectoryInfo("c:\tarjetas\cierres")
                        For Each archivo In dir.GetFiles(fecha_recorrida.Date.ToString("yyMMdd") & "*.VI*")

                            Escribe_LOG(String.Format(archivo.DirectoryName & "\" & archivo.Name, archivo.DirectoryName & "\" & archivo.Name))
                            archive.CreateEntryFromFile(archivo.DirectoryName & "\" & archivo.Name, archivo.DirectoryName & "\" & archivo.Name, CompressionLevel.Optimal)
                            File.Move(archivo.DirectoryName & "\" & archivo.Name, "C:\tarjetas\borrar\" & archivo.Name)
                            hayalgo = True
                        Next
                        For Each archivo In dir.GetFiles("*_" & fecha_recorrida.Date.ToString("yyMMdd") & "*.txt")

                            Escribe_LOG(String.Format(archivo.DirectoryName & "\" & archivo.Name, archivo.DirectoryName & "\" & archivo.Name))
                            archive.CreateEntryFromFile(archivo.DirectoryName & "\" & archivo.Name, archivo.DirectoryName & "\" & archivo.Name, CompressionLevel.Optimal)
                            File.Move(archivo.DirectoryName & "\" & archivo.Name, "C:\tarjetas\borrar\" & archivo.Name)
                            hayalgo = True
                        Next

                        Escribe_LOG(String.Format(" "))
                        Escribe_LOG(String.Format("    c:\tarjetas\cupones"))

                        dir = New DirectoryInfo("c:\tarjetas\cupones")
                        For Each archivo In dir.GetFiles("*_*_" & fecha_recorrida.Date.ToString("yyMMdd") & "*.dat")

                            Escribe_LOG(String.Format(archivo.DirectoryName & "\" & archivo.Name, archivo.DirectoryName & "\" & archivo.Name))
                            archive.CreateEntryFromFile(archivo.DirectoryName & "\" & archivo.Name, archivo.DirectoryName & "\" & archivo.Name, CompressionLevel.Optimal)
                            File.Move(archivo.DirectoryName & "\" & archivo.Name, "C:\tarjetas\borrar\" & archivo.Name)
                            hayalgo = True
                        Next

                        Escribe_LOG(String.Format(" "))
                        Escribe_LOG(String.Format("    c:\tarjetas\respuestas"))

                        dir = New DirectoryInfo("c:\tarjetas\respuestas")
                        For Each archivo In dir.GetFiles("*_*_" & fecha_recorrida.Date.ToString("yyMMdd") & "*")

                            Escribe_LOG(String.Format(archivo.DirectoryName & "\" & archivo.Name, archivo.DirectoryName & "\" & archivo.Name))
                            archive.CreateEntryFromFile(archivo.DirectoryName & "\" & archivo.Name, archivo.DirectoryName & "\" & archivo.Name, CompressionLevel.Optimal)
                            File.Move(archivo.DirectoryName & "\" & archivo.Name, "C:\tarjetas\borrar\" & archivo.Name)
                            hayalgo = True
                        Next

                        Escribe_LOG(String.Format(" "))
                        Escribe_LOG(String.Format("    c:\temp"))

                        dir = New DirectoryInfo("c:\temp")
                        For Each archivo In dir.GetFiles("LOG_" & fecha_recorrida.Date.ToString("yyyy-MM-dd") & ".txt")

                            Escribe_LOG(String.Format(archivo.DirectoryName & "\" & archivo.Name, archivo.DirectoryName & "\" & archivo.Name))
                            archive.CreateEntryFromFile(archivo.DirectoryName & "\" & archivo.Name, archivo.DirectoryName & "\" & archivo.Name, CompressionLevel.Optimal)
                            File.Move(archivo.DirectoryName & "\" & archivo.Name, "C:\tarjetas\borrar\" & archivo.Name)
                            hayalgo = True
                        Next
                    End Using
                End Using
                If Not hayalgo Then
                    Escribe_LOG(String.Format(String.Format("Borrando {0} porque esta vacío.", "c:\Tarjetas\Compactados\" & fecha_recorrida.Date.ToString("yyMMdd") & ".zip")))
                    File.Delete("c:\Tarjetas\Compactados\" & fecha_recorrida.Date.ToString("yyMMdd") & ".zip")
                End If
                fecha_recorrida = fecha_recorrida.AddDays(1)
            Next
        End If
    End Sub

    Private Sub Escribe_LOG(v As String)
        Using Log As FileStream = New FileStream("c:\Tarjetas\Log.txt", FileMode.Append)
            Using texto As StreamWriter = New StreamWriter(Log)
                'Label4.Text = v
                texto.WriteLine("(" & Now.Date.ToString("dd-MM-yyyy") & "  " & Now.ToString("HH:mm:ss") & ") - " & v)
            End Using
        End Using
    End Sub
End Class
