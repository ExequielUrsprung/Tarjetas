Imports System.Threading
Imports System.Net.Sockets
Imports System.Text


Public Class ClaseServidorSocket
    'Esta estructura permite guardar la información sobre un cliente
    Private Structure datosClienteConectado
        Public socketConexion As Socket 'Socket para mantener la conexión con cliente
        Public Thread As Thread 'Hilo para mantener escucha con el cliente
        Public UltimosDatosRecibidos As MensajeIda 'Últimos datos enviados por el cliente
    End Structure

    Private tcpLsn As TcpListener 'Para realizar la escuchas de conexiones de clientes
    Private Clientes As New Hashtable() 'Datos de los clientes conectados
    Private tcpThd As Thread
    Private IDClienteActual As Net.IPEndPoint 'Último cliente conectado
    Private m_PuertoDeEscucha As String

    Public Event NuevaConexion(ByVal IDTerminal As Net.IPEndPoint)
    Public Event DatosRecibidos(ByVal IDTerminal As Net.IPEndPoint)
    Public Event ConexionTerminada(ByVal IDTerminal As Net.IPEndPoint)
    Public Event ConexionCerrada(ByVal IDTerminal As Net.IPEndPoint)



    Sub New(puerto As Integer)
        '--- SIEMPRE VIENE FIJO EL PUERTO 5000 
        m_PuertoDeEscucha = puerto
    End Sub

    'Procedimiento para establecer el servidor en modo escucha
    Public Sub IniciarEscucha()
        tcpLsn = New TcpListener(Net.IPAddress.Any, m_PuertoDeEscucha)

        'Iniciar escucha
        tcpLsn.Start()

        'Crear hilo para dejar escuchando la conexión de clientes
        tcpThd = New Thread(AddressOf EsperarConexionCliente)
        tcpThd.Start()
    End Sub


    'Procedimiento para detener la escucha del servidor
    Public Sub DetenerEscucha()
        CerrarTodosClientes()
        tcpThd.Abort()
        tcpLsn.Stop()
    End Sub


    Public Function ObtenerDatos(ByVal IDCliente As Net.IPEndPoint) As MensajeIda
        Dim InfoClienteSolicitado As datosClienteConectado

        'Obtengo la informacion del cliente solicitado
        InfoClienteSolicitado = Clientes(IDCliente)
        Return InfoClienteSolicitado.UltimosDatosRecibidos
    End Function

    'Cierra la conexión de un cliente conectado
    Public Sub cerrarConexionCliente(ByVal IDCliente As Net.IPEndPoint)
        Dim InfoClienteActual As datosClienteConectado

        'Obtener información del cliente indicado
        InfoClienteActual = Clientes(IDCliente)

        'Cerrar conexión con cliente
        InfoClienteActual.socketConexion.Close()
    End Sub


    'Cerrar todas la conexión de todos los clientes conectados
    Public Sub CerrarTodosClientes()
        Dim InfoClienteActual As datosClienteConectado

        'Cerrar conexión de todos los clientes
        For Each InfoClienteActual In Clientes.Values
            Call cerrarConexionCliente(InfoClienteActual.socketConexion.RemoteEndPoint)
        Next
    End Sub


    Public Sub enviarMensajeCliente(ByVal IDCliente As Net.IPEndPoint, ByVal Datos As MensajeRespuesta)
        Dim Cliente As datosClienteConectado

        'Obtener información del cliente al que se enviará el mensaje  
        Cliente = Clientes(IDCliente)
        'Cliente.UltimosDatosRecibidos = Datos  
        'Enviar mensaje a cliente  
        Try
            Cliente.socketConexion.Send(SerializarMensaje(Datos))     '---- aca manda al ClaseClienteSocket.vb, se conecta a la solucion PINPAD_EMV 
        Catch ex As Exception
            RaiseEvent ConexionCerrada(IDCliente)
        End Try
    End Sub

    Dim libre As Boolean = True
    'Procedimiento que inicia la espera de la conexión de un cliente
    'para ello inicia un hilo (thread)
    Private Sub EsperarConexionCliente()
        Dim datosClienteActual As datosClienteConectado

        While True
            'Se guarda la información del cliente cuando se recibe la conexión
            'Quedará esperando la conexión de un nuevo cliente
            datosClienteActual.socketConexion = tcpLsn.AcceptSocket()

            'Con el IDClienteActual se identificará al cliente conectado
            IDClienteActual = datosClienteActual.socketConexion.RemoteEndPoint

            'Crear un hilo para que quede escuchando los mensajes del cliente
            datosClienteActual.Thread = New Thread(AddressOf LeerSocket)

            'Agregar la información del cliente conectado al array
            SyncLock Me
                Clientes.Add(IDClienteActual, datosClienteActual)
            End SyncLock

            'Generar evento NuevaConexion
            RaiseEvent NuevaConexion(IDClienteActual)

            'Iniciar el hilo que escuchará los mensajes del cliente
            datosClienteActual.Thread.Start()
        End While
    End Sub



    '---------------------------------------------------------------
    '---  Procedimiento para leer datos enviados por el cliente     
    '---------------------------------------------------------------
    Private Sub LeerSocket()
        Dim IDReal As Net.IPEndPoint                    '--- ID del cliente que se va a escuchar  
        Dim Recibir() As Byte                           '--- Array donde se guardarán los datos que lleguen  
        Dim InfoClienteActual As datosClienteConectado  '--- Datos del cliente conectado  
        Dim Ret As Integer = 0

        IDReal = IDClienteActual
        InfoClienteActual = Clientes(IDReal)

        While True
            If Not InfoClienteActual.socketConexion.Connected Then
                Continue While
            End If

            Recibir = New Byte(4096) {}
            Try

                '-----------------------------------------------------
                '--- Espera a que llegue un mensaje desde el cliente  
                '-----------------------------------------------------

                Ret = InfoClienteActual.socketConexion.Receive(Recibir, Recibir.Length, SocketFlags.None)

                If Ret <= 0 Then
                    'Generar el evento ConexionTerminada    
                    'de finalización de la conexión         
                    RaiseEvent ConexionTerminada(IDReal)
                    Exit While
                End If

                'Guardar mensaje recibido
                InfoClienteActual.UltimosDatosRecibidos = DeserializarMensajeIda(Recibir)
                Clientes(IDReal) = InfoClienteActual

                'Dispara el evento DatosRecibidos que está en el ServerTar.vb (en Sub ProcesarTCP...) 
                'para los datos recibidos           
                RaiseEvent DatosRecibidos(IDReal)

            Catch e As Exception
                If InfoClienteActual.socketConexion.Connected Then
                    Continue While
                End If

                'Generar el evento ConexionTerminada 
                'de finalización de la conexión
                RaiseEvent ConexionTerminada(IDReal)
                Exit While
            End Try
        End While
        Call CerrarThread(IDReal)
    End Sub

    'Procedimiento para cerrar el hilo (thread)
    Private Sub CerrarThread(ByVal IDCliente As Net.IPEndPoint)
        Dim InfoClienteActual As datosClienteConectado

        'Finalizar el hilo (thread) iniciado 
        ' encargado de escuchar al cliente
        InfoClienteActual = Clientes(IDCliente)

        Try
            InfoClienteActual.Thread.Abort()
        Catch e As Exception
            SyncLock Me
                'Eliminar el cliente del array
                Clientes.Remove(IDCliente)
            End SyncLock
        End Try
    End Sub
End Class
