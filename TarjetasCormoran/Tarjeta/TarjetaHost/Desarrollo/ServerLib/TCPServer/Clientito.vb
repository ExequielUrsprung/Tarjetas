Imports System.Text
Imports System.Net.Sockets
Imports System.Threading
Imports System.Net

Public Class Clientito
    Sub algo()
        Dim client As New TcpClient()

        Dim serverEndPoint As New IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000)

        client.Connect(serverEndPoint)

        Dim clientStream As NetworkStream = client.GetStream()

        Dim encoder As New ASCIIEncoding()
        Dim buffer As Byte() = encoder.GetBytes("Hello Server!")

        clientStream.Write(buffer, 0, buffer.Length)
        clientStream.Flush()
    End Sub

End Class
