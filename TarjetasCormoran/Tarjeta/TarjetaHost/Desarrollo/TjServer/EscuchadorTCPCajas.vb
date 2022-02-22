Imports System.Net.Sockets
Imports System.Threading
Imports System.Net
Imports TjComun

Public Class EscuchadorTCPCajas
    'Public Event NuevoRequerimiento(ida As TjComun.Mensaje, caja As Integer, client As NetworkStream)
    'Private tcpListener As TcpListener
    'Private listenThread As Thread

    'Private Sub Escuchar()
    '    Me.listenThread = New Thread(New ThreadStart(AddressOf ListenForClients))
    '    Me.listenThread.Start()
    'End Sub
    'Public Sub New(PortNumber As Integer)
    '    Me.tcpListener = New TcpListener(IPAddress.Any, PortNumber)

    '    Escuchar()
    'End Sub




    'Private Sub ListenForClients()
    '    Me.tcpListener.Start()

    '    While True
    '        'blocks until a client has connected to the server
    '        Dim client As TcpClient = Me.tcpListener.AcceptTcpClient()

    '        'create a thread to handle communication 
    '        'with connected client
    '        Dim clientThread As New Thread(New ParameterizedThreadStart(AddressOf HandleClientComm))
    '        clientThread.Start(client)
    '    End While
    'End Sub

    'Private Sub HandleClientComm(client As Object)
    '    Dim tcpClient As TcpClient = DirectCast(client, TcpClient)
    '    Using clientStream As NetworkStream = tcpClient.GetStream()
    '        Dim message As Byte() = New Byte(4095) {}
    '        Dim bytesRead As Integer
    '        bytesRead = 0
    '        Try
    '            bytesRead = clientStream.Read(message, 0, 4096)
    '            'TODO: ver 4096
    '        Catch
    '            'a socket error has occured
    '            Exit Try
    '        End Try
    '        If bytesRead = 0 Then
    '            'the client has disconnected from the server
    '        Else
    '            Dim Datos(bytesRead) As Byte
    '            Array.Copy(message, Datos, bytesRead)
    '            DataReceived(Datos, clientStream)

    '            enviar(clientStream, SerializarOutput(Resultados))
    '        End If
    '    End Using
    '    tcpClient.Close()
    'End Sub



    ' •———————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————•
    ' | 'The first thing we need to do is cast client as a TcpClient object since the ParameterizedThreadStart delegate can only accept object types. 
    '   Next, we get the NetworkStream from the TcpClient, which we'll be using to do our reading. 
    '   After that we simply sit in a while true loop reading information from the client. 
    '   The Read call will block indefinitely until a message from the client has been received. 
    '   If you read zero bytes from the client, you know the client has disconnected. 
    '   Otherwise, a message has been successfully received from the server. In my example code, I simply convert the byte array to a string and push it to the debug console. 
    '   You will, of course, do something more interesting with the data - I hope. 
    '   If the socket has an error or the client disconnects, you should call Close on the TcpClient object to free up any resources it was using. |
    ' |                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
    ' •—————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————————•

    'Sub enviar(ClientStream As NetworkStream, Buffer As Byte())

    '    ClientStream.Write(Buffer, 0, Buffer.Length)
    '    ClientStream.Flush()
    'End Sub
    'Sub Detener()

    '    Me.listenThread.Abort()
    '    Me.tcpListener.Stop()

    'End Sub



    'Public Parametros As SParametrosInput
    'Private Resultados As SParametrosOutput
    'Public Mensaje As Mensaje
    'Dim BF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()

    'Public Event RequerimientoCajaRecibido(ParametrosRecibidos As SParametrosInput, ByRef Respuesta As SParametrosOutput)
    'Public Event RequerimientoEnviado(ByVal Respuesta As SParametrosOutput)

    'Private Function SerializarOutput(ByVal parametros As SParametrosOutput) As Byte()
    '    Dim MS As New System.IO.MemoryStream()
    '    BF.Serialize(MS, parametros)
    '    Return MS.GetBuffer()
    'End Function

    'Private Function DeserializarMensaje(Datos As Byte()) As Mensaje
    '    Return DirectCast(BF.Deserialize(New System.IO.MemoryStream(Datos)), Mensaje)
    'End Function

    'Private Function DeserializarInput(Datos As Byte()) As SParametrosInput
    '    Return DirectCast(BF.Deserialize(New System.IO.MemoryStream(Datos)), SParametrosInput)
    'End Function

    'Public Sub DataReceived(ByVal Datos() As Byte, ByVal clientStream As System.Net.Sockets.NetworkStream)
    'Public Sub DataReceived(ByVal Datos As Byte(), ByVal clientStream As System.Net.Sockets.NetworkStream)
    '    'aca es donde recibe el requerimiento y se lo pasa al servidor para procesar y mandar al host.
    '    'Dim ParametrosRecibidos As SParametrosInput = DeserializarInput(Datos)
    '    ' procesar la resupuesta
    '    'Dim Respuesta As New SParametrosOutput
    '    Mensaje = DeserializarMensaje(Datos)
    '    'Parametros = DeserializarInput(Datos)

    '    'Parametros.client = clientStream
    '    RaiseEvent NuevoRequerimiento(Mensaje, Mensaje.caja, clientStream)
    '    'RaiseEvent NuevoRequerimiento(Parametros, 15, clientStream)
    '    'RaiseEvent RequerimientoCajaRecibido(ParametrosRecibidos, Respuesta)
    '    'enviar(clientStream, SerializarOutput(Resultados))
    '    'RaiseEvent RequerimientoEnviado(Respuesta)
    'End Sub

    'Private Function ConvertirVTA(datos As VtaType) As SParametrosOutput
    '    With Resultados
    '        .VtaCashBack = datos.VtaCashBack
    '        .VtaCheck = datos.VtaCheck
    '        .VtaEmiName = datos.VtaEmiName
    '        .VtaFileTkt = datos.VtaFileTkt
    '        .VtaMenResp = datos.VtaMenResp
    '        .VtaMensaje = datos.VtaMensaje
    '        .VtaOk = datos.VtaOk
    '        .VtaTicket = datos.VtaTicket
    '        .VtaVersion = datos.VtaVersion
    '    End With
    '    Return Resultados



    'End Function

    'Public Sub SendData(datos As Req)
    '    enviar(datos.clientStream, SerializarOutput(ConvertirVTA(datos.vta)))
    'End Sub



End Class
