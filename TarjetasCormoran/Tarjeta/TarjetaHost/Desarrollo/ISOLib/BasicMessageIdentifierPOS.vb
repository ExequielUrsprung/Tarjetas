Imports Trx.Messaging


Public Class BasicMessageIdentifierPOS
    Inherits BasicMessagesIdentifier

    Public Sub New()
        'por default los mensajes nexo se definen con el campo 11
        'MyBase.New(11)
        MyBase.New(41)
    End Sub

End Class
