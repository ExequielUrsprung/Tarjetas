Imports System.Net.Sockets
Imports System.Threading
Imports System.Net


Public MustInherit Class ServerTCP
    Private tcpListener As TcpListener
    Private listenThread As Thread

    Private Sub Escuchar()
        Me.listenThread = New Thread(New ThreadStart(AddressOf ListenForClients))
        Me.listenThread.Start()
    End Sub
    Public Sub New(PortNumber As Integer)
        Me.tcpListener = New TcpListener(IPAddress.Any, PortNumber)
        Escuchar()
    End Sub

    MustOverride Sub DataReceived(Datos As Byte(), clientStream As NetworkStream)

    Private Sub ListenForClients()
        Me.tcpListener.Start()

        While True
            'blocks until a client has connected to the server
            Dim client As TcpClient = Me.tcpListener.AcceptTcpClient()

            'create a thread to handle communication 
            'with connected client
            Dim clientThread As New Thread(New ParameterizedThreadStart(AddressOf HandleClientComm))
            clientThread.Start(client)
        End While
    End Sub

    Private Sub HandleClientComm(client As Object)
        Dim tcpClient As TcpClient = DirectCast(client, TcpClient)
        Using clientStream As NetworkStream = tcpClient.GetStream()
            Dim message As Byte() = New Byte(4095) {}
            Dim bytesRead As Integer
            bytesRead = 0
            Try

                bytesRead = clientStream.Read(message, 0, 4096)
            Catch
                'a socket error has occured
                Exit Try
            End Try
            If bytesRead = 0 Then
                'the client has disconnected from the server
            Else
                Dim Datos(bytesRead) As Byte
                Array.Copy(message, Datos, bytesRead)
                DataReceived(Datos, clientStream)
            End If
        End Using
        tcpClient.Close()
    End Sub



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

    Sub enviar(ClientStream As NetworkStream, Buffer As Byte())

        ClientStream.Write(Buffer, 0, Buffer.Length)
        ClientStream.Flush()
    End Sub
    Sub Detener()

        Me.listenThread.Abort()
        Me.tcpListener.Stop()

    End Sub
End Class
