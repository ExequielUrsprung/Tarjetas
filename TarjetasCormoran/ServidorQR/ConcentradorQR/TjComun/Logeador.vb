Imports log4net
Imports log4net.Config
Imports log4net.Appender
Imports log4net.Repository.Hierarchy
Imports System.Windows.Forms
Imports System.Drawing
Public Class Logeador
    Dim _memoryAppender As MemoryAppender

    Dim _logger As log4net.ILog

    Public Sub New(Optional ByVal MemoryAppenderOn As Boolean = False)
        _logger = LogManager.GetLogger("root")

        Try
            log4net.Config.XmlConfigurator.Configure(New System.IO.FileInfo("log4net.config"))
        Catch ex As Exception

        End Try
        'Crear un appender en memoria para mostrarlo en la pantalla
        If MemoryAppenderOn Then
            AddMemoryAppender()
        End If
        'logInfo(String.Format("Arrancando {0} {1}", My.Application.Info.Title, My.Application.Info.Version.ToString))

    End Sub


    Public Sub AddMemoryAppender()
        _memoryAppender = New MemoryAppender
        CType(log4net.LogManager.GetRepository, log4net.Repository.Hierarchy.Hierarchy).Root.AddAppender(_memoryAppender)

    End Sub
    Public Sub GetArrayAndClear(ByVal pListbox As ListBox, Optional ByVal plistboxerrors As ListBox = Nothing)

        If _memoryAppender Is Nothing Then

        Else
            With _memoryAppender
                Dim i As Integer
                Try

                    For Each o As log4net.Core.LoggingEvent In .GetEvents
                        Dim s As String = o.TimeStamp.TimeOfDay.ToString + " " + o.RenderedMessage.ToString
                        For Each s2 As String In s.Split(CChar(vbCrLf))
                            pListbox.Items.Add(s2)
                        Next

                        i += 1
                        If plistboxerrors IsNot Nothing Then
                            If o.Level >= log4net.Core.Level.Warn Then
                                For Each s2 As String In s.Split(CChar(vbCrLf))
                                    pListbox.Items.Add(s2)
                                Next
                            End If
                        End If

                    Next
                    If i > 0 Then
                        .Clear()
                        pListbox.SelectedIndex = pListbox.Items.Count - 1
                    End If

                Catch ex As Exception

                End Try

            End With
        End If

    End Sub
    Public Sub GetArrayAndClear(ByVal pListView As ListView, Optional ByVal plistboxerrors As ListBox = Nothing)

        If _memoryAppender Is Nothing Then

        Else
            With _memoryAppender
                Dim i As Integer
                Try

                    For Each o As log4net.Core.LoggingEvent In .GetEvents
                        Dim s As String = o.TimeStamp.TimeOfDay.ToString + " " + o.Level.ToString + " " + o.RenderedMessage.ToString
                        For Each s2 As String In s.Split(CChar(vbCrLf))
                            pListView.Items.Add(s2)
                            If o.Level = log4net.Core.Level.Debug Then
                                pListView.Items(pListView.Items.Count - 1).ForeColor = Color.CornflowerBlue
                            ElseIf o.Level = log4net.Core.Level.Info Then
                                pListView.Items(pListView.Items.Count - 1).ForeColor = Color.DarkSlateBlue
                            ElseIf o.Level = log4net.Core.Level.Warn Then
                                pListView.Items(pListView.Items.Count - 1).ForeColor = Color.Red

                            ElseIf o.Level = log4net.Core.Level.Error Then
                                pListView.Items(pListView.Items.Count - 1).BackColor = Color.Red
                                pListView.Items(pListView.Items.Count - 1).ForeColor = Color.White

                            ElseIf o.Level = log4net.Core.Level.Fatal Then
                                pListView.Items(pListView.Items.Count - 1).BackColor = Color.Black
                                pListView.Items(pListView.Items.Count - 1).ForeColor = Color.Red

                            End If
                        Next

                        i += 1
                        If plistboxerrors IsNot Nothing Then ' no implementado.
                            If o.Level >= log4net.Core.Level.Warn Then
                                For Each s2 As String In s.Split(CChar(vbCrLf))
                                    pListView.Items.Add(s2)
                                Next
                            End If
                        End If

                    Next

                    If i > 0 Then
                        .Clear()
                        pListView.Items(pListView.Items.Count - 1).EnsureVisible()
                        'pListbox.SelectedIndex = pListbox.Items.Count - 1
                    End If

                Catch ex As Exception

                End Try

            End With
        End If

    End Sub

    Public Sub log(ByVal s As String)
        _logger.Debug(s)
    End Sub

    Public Sub logInfo(ByVal s As String)
        _logger.Info(s)
    End Sub
    Public Sub logDebug(ByVal s As String)
        _logger.Debug(s)
    End Sub
    Public Sub logWarning(ByVal s As String)
        _logger.Warn(s)
    End Sub
    Public Sub logerror(ByVal s As String)
        _logger.Error(s)
    End Sub

End Class