Imports Trx.Messaging.Iso8583
Imports Trx.Messaging.FlowControl

Public Class Iso8583messagePOS
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
                Case E_Implementaciones.PosnetHomol
                    HeaderID = "6000090000"

                Case E_Implementaciones.PosnetComercio
                    HeaderID = "6000030000"
                    '    HeaderID = "026000011"

                Case E_Implementaciones.Visa, E_Implementaciones.VisaHomol
                    HeaderID = "6000050000"
                Case Else
                    Logger.Info("falta Implementar Header para Implementacion " + _Implementacion.ToString)
            End Select
        End If
        Header = New Trx.Messaging.StringMessageHeader(HeaderID) '.PadLeft(9))
    End Sub

    'MODIFIQUE ACA PORQUE ESTABA MAL EL HEADER
    Public Sub SetHeaderResponse()
        'Logger.Debug("seteandio header " + Header.ToString)
        SetHeader(Header.ToString.Substring(0, 5) + "00005")
        'SetHeader(Header.ToString.Substring(3, 8) + "5")
        Logger.Debug("header " + Header.ToString)
    End Sub

    Public Overloads Sub SetResponseMEssageTypeIdentifier()
        SetHeaderResponse()
        MyBase.SetResponseMessageTypeIdentifier()
    End Sub

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
