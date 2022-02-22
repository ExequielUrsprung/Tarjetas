Imports Trx.Messaging.Iso8583
Imports Trx.Messaging.FlowControl

Public Class RequestNexo
    Inherits PeerRequest
    Dim _implementacion As E_Implementaciones
    Public Sub New(ByVal pClientpeer As ClientPeer, ByVal m As Iso8583messageNexo)
        MyBase.New(pClientpeer, m)
        _implementacion = m.Implementacion
    End Sub
    Public Overloads ReadOnly Property ResponseMessage() As Iso8583messageNexo
        Get
            If MyBase.ResponseMessage Is Nothing Then
                Return Nothing
            Else

                Return New Iso8583messageNexo(CType(MyBase.ResponseMessage, Iso8583Message), _implementacion)
            End If
        End Get
    End Property


End Class
Public Class Iso8583messageNexo
    Inherits Iso8583Message
    Private _Implementacion As E_Implementaciones
    Private _messageSource As IMessageSource
    'Public Sub New()
    '    MyBase.new()
    'End Sub

    Public Sub New(ByVal MessageTypeIdentifier As Int16, ByVal pImplementacion As E_Implementaciones)
        MyBase.new(MessageTypeIdentifier)
        _Implementacion = pImplementacion
        SetHeader()
    End Sub
    Public Sub New(ByVal pImplementacion As E_Implementaciones)
        MyBase.new()
        _Implementacion = pImplementacion
        SetHeader()

    End Sub
    Public Sub New(ByVal m As Iso8583Message)
        MyBase.New()
        SetHeader()
        m.CopyTo(Me)
    End Sub
    Public Sub New(ByVal m As Iso8583Message, ByVal pImplementacion As E_Implementaciones)
        MyBase.New()
        _Implementacion = pImplementacion
        SetHeader()
        m.CopyTo(Me)
    End Sub
    Public Sub New(ByVal m As Iso8583Message, ByVal pImplementacion As E_Implementaciones, ByVal messageSource As IMessageSource)
        MyBase.New()
        _Implementacion = pImplementacion
        _messageSource = messageSource
        SetHeader()
        m.CopyTo(Me)
    End Sub


    Public Sub SetHeader(Optional ByVal HeaderID As String = "")
        If HeaderID = "" Then
            Select Case _Implementacion
                Case E_Implementaciones.Banelco
                    HeaderID = "016000010"
                Case E_Implementaciones.Link
                    HeaderID = "004000010"
                Case E_Implementaciones.PosnetSalud
                    HeaderID = "026000011"
                Case E_Implementaciones.PosnetComercio
                    HeaderID = "026000011"

                Case Else
                    MsgBox("falta Implementar Header para Implementacion " + _Implementacion.ToString)
            End Select
        End If
        Header = New Trx.Messaging.StringMessageHeader("ISO" + HeaderID.PadLeft(9))
    End Sub

    Public Sub SetHeaderResponse()
        'Logger.Debug("seteandio header " + Header.ToString)
        SetHeader(Header.ToString.Substring(3, 8) + "5")
        Logger.Debug("header " + Header.ToString)
    End Sub

    Public Overloads Sub SetResponseMEssageTypeIdentifier()
        SetHeaderResponse()
        MyBase.SetResponseMessageTypeIdentifier()
    End Sub
    Public Property p126() As Mensaje126Adelanto
        Get
            If MyBase.Fields.Contains(126) Then
                Dim codproc As Integer = CInt(Fields(3).Value)
                Return New Mensaje126Adelanto(_Implementacion, Fields(126).ToString, codproc)
            Else
                Logger.Debug("Cuidado se genera p126 sin 126")
                Return New Mensaje126Adelanto(_Implementacion, "", Mensaje126Adelanto.e_m126_tipos.ConsultaSaldo)
            End If
        End Get
        Set(ByVal value As Mensaje126Adelanto)
            Fields.Add(126, value.Tostring)
        End Set
    End Property
    Public Property p54() As Mensaje54Pas
        Get
            If MyBase.Fields.Contains(54) Then
                Return New Mensaje54Pas(_Implementacion, Fields(54).ToString)
            Else
                Return Nothing
            End If
        End Get
        Set(ByVal value As Mensaje54Pas)
            Fields.Add(54, value.Tostring)
        End Set
    End Property

    Public Property p127() As Mensaje127
        Get
            If MyBase.Fields.Contains(127) Then
                Return New Mensaje127(_Implementacion, Fields(127).ToString)
            Else
                Return Nothing
            End If
        End Get
        Set(ByVal value As Mensaje127)
            Fields.Add(127, value.Tostring)
        End Set
    End Property
    Public Sub Send()
        _messageSource.Send(Me)
    End Sub
    Public Property MessageSource() As IMessageSource
        Get
            Return _messageSource
        End Get
        Set(ByVal value As IMessageSource)
            _messageSource = value
        End Set
    End Property
    Public Property Implementacion() As E_Implementaciones
        Get
            Return _Implementacion
        End Get
        Set(ByVal value As E_Implementaciones)
            _Implementacion = value
        End Set
    End Property
End Class
