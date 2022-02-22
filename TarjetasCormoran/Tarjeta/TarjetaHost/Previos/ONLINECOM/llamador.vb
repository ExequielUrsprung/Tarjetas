Imports Trx.Messaging
Imports Trx.Messaging.Iso8583
Imports Trx.Messaging.Channels
Imports Trx.Messaging.FlowControl
Public Class llamador
    Inherits ClientPeer
    Public HostOffLine As Boolean
    Private _nombre As String
    Private _port As Integer
    Private _hostname As String
    Sub New(ByVal Nombre As String, ByVal Implementacion As E_Implementaciones, ByVal Hostname As String, ByVal POrt As Int32)
        MyBase.New(Nombre, _
                        ObtenerTcpChannel(Implementacion, Hostname, POrt), _
                        New NexoMessagesIdentifier())
        _Hostname = Hostname
        _Port = POrt
        _Nombre = Nombre
        '-Dim Nombre As String = "NEXO-" + My.Computer.Name + "-" + My.User.Name
    End Sub

    Public Sub conectar()
        Logger.Info("Conexion " + _nombre + " " + _hostname + ":" + _port.ToString)
        Connect()
        Logger.Info("Esperando Conexion ")
    End Sub

    Public Sub Desconectar()
        Logger.Info("Desconectando")
        Close()

    End Sub

    Private Sub llamador_Connected(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Connected

    End Sub
End Class
