Imports Trx.Messaging.Iso8583
Imports Trx.Messaging.FlowControl

Public Class PeerRequestPOS
    Inherits PeerRequest
    Dim _implementacion As E_Implementaciones
    Public Sub New(ByVal pClientpeer As ClientPeer, ByVal m As Iso8583messagePOS)
        MyBase.New(pClientpeer, m)
        _implementacion = m.Implementacion
    End Sub
    Public Overloads ReadOnly Property ResponseMessage() As Iso8583Message
        Get
            If MyBase.ResponseMessage Is Nothing Then
                Return Nothing
            Else
                Return New Iso8583messagePOS(CType(MyBase.ResponseMessage, Iso8583Message), _implementacion)
            End If
        End Get
    End Property


End Class