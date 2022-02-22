Imports System.Net.Sockets
Imports System.Text
Public Class TransmisorPOS
    Implements IDisposable

    Dim Puerto As Integer = 3000
    ' ip al que hay que llamar (el servidor)
    'Dim Server As String = "132.161.20.34"
    Dim Server As String = "127.0.0.1"
    'Dim Server As String = "172.16.1.190"
    Dim BufferSIze As Integer = 1024
    Dim WithEvents networkStream As NetworkStream

    Sub Enviar(Datos As Byte())

        If networkStream Is Nothing Then Conectar()
        networkStream.Write(Datos, 0, Datos.Length)

    End Sub

    Function Recibir() As Byte()
        ' Read the NetworkStream into a byte buffer.
        Dim bytes(BufferSIze) As Byte
        networkStream.Read(bytes, 0, CInt(BufferSIze))
        ' Output the data received from the host to the console.
        Return bytes
    End Function


    Sub Conectar()
        Dim tcpClient As New System.Net.Sockets.TcpClient()

        tcpClient.Connect(Server, Puerto)
        networkStream = tcpClient.GetStream()

    End Sub

    Function Comprobar_Datos() As Boolean
        Return networkStream.DataAvailable
    End Function


#Region "IDisposable Support"
    Private disposedValue As Boolean ' Para detectar llamadas redundantes

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                networkStream.Dispose()
                ' TODO: eliminar estado administrado (objetos administrados).
            End If

            ' TODO: liberar recursos no administrados (objetos no administrados) e invalidar Finalize() below.
            ' TODO: Establecer campos grandes como Null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: invalidar Finalize() sólo si la instrucción Dispose(ByVal disposing As Boolean) anterior tiene código para liberar recursos no administrados.
    'Protected Overrides Sub Finalize()
    '    ' No cambie este código. Ponga el código de limpieza en la instrucción Dispose(ByVal disposing As Boolean) anterior.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' Visual Basic agregó este código para implementar correctamente el modelo descartable.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' No cambie este código. Coloque el código de limpieza en Dispose (ByVal que se dispone como Boolean).
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class

