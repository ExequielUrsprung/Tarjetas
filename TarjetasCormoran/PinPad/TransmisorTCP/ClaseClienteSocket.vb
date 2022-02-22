Imports System.Net.Sockets
Imports System.Threading
Imports System.Text
Imports System.IO

Public Class ClaseClienteSocket
    Private mensajesEnviarRecibir As Stream 'Para enviar y recibir datos del servidor
    Private ipServidor As String 'Dirección IP
    Private puertoServidor As String 'Puerto de escucha

    Private clienteTCP As TcpClient
    Private hiloMensajeServidor As Thread 'Escuchar mensajes enviados desde el servidor
    Private mensaje As MensajeRespuesta

    Public Event ConexionTerminada()
    Public Event DatosRecibidos(ByVal datos As MensajeRespuesta)


    Public ReadOnly Property Respuesta() As MensajeRespuesta
        Get
            Return mensaje
        End Get
    End Property

    Sub New(puerto As Integer, ipServer As String)
        puertoServidor = puerto
        ipServidor = ipServer
    End Sub
    'Procedimiento para realizar la conexión con el servidor
    Public Sub Conectar()
        clienteTCP = New TcpClient()

        Try
            'Conectar con el servidor
            clienteTCP.Connect(ipServidor, puertoServidor)
            mensajesEnviarRecibir = clienteTCP.GetStream()

            'Crear hilo para establecer escucha de posibles mensajes
            'enviados por el servidor al cliente
            hiloMensajeServidor = New Thread(AddressOf LeerSocket)
            hiloMensajeServidor.Start()
        Catch ex1 As SocketException
            Console.WriteLine(ex1.Message)
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        End Try

    End Sub

    'Procedimiento para cerrar la conexión con el servidor
    Public Sub Desconectar()
        Try
            'desconectamos del servidor
            If clienteTCP.Connected Then clienteTCP.Close()
            'abortamos el hilo (thread)
        Catch ex As Exception
            Console.WriteLine()
        End Try
        If hiloMensajeServidor IsNot Nothing Then hiloMensajeServidor.Abort()
    End Sub


    Public Function EsperarRespuestaServer(timeout As Integer) As Boolean

        Dim hora = Now

        While Not semaforo And Now.Subtract(hora).TotalSeconds < timeout

        End While

        Return semaforo

    End Function

    Dim semaforo As Boolean
    Public Sub EnviarDatos(ByVal Datos As MensajeIda)
        Dim BufferDeEscritura() As Byte = Serializador.SerializarMensaje(Datos)
        If Not (mensajesEnviarRecibir Is Nothing) Then
            semaforo = False
            mensajesEnviarRecibir.Write(BufferDeEscritura, 0, BufferDeEscritura.Length)    '--- ACA LO MANDA AL ClaseServidorSocket ---> LeerSocket  
        End If
    End Sub

    Private Sub LeerSocket()
        Dim BufferDeLectura() As Byte

        While True
            Try
                BufferDeLectura = New Byte(4096) {}

                '--- Esperar a que llegue algún mensaje   
                mensajesEnviarRecibir.Read(BufferDeLectura, 0, BufferDeLectura.Length)
                semaforo = True
                mensaje = DeserializarMensajeRespuesta(BufferDeLectura)
                '--- Dispara un evento DatosRecibidos cuando se recibien datos desde el servidor  
                '--- El evento que dispara es *** Private Sub Respuesta_Recibida() Handles transmisor.DatosRecibidos *** que esta en Cliente.vb  
                RaiseEvent DatosRecibidos(mensaje)
            Catch e As Exception
                Exit While
            End Try
        End While

        'Finalizar conexión y generar evento ConexionTerminada
        RaiseEvent ConexionTerminada()
    End Sub
End Class
