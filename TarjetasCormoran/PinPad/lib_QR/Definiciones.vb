Public Enum Estado
    CLOSED
    CREATED
    OPENED
    CANCELLED
    UNKNOWN
    NORESPONSE
End Enum

Public Enum Moneda
    ARS
    USD
End Enum

Public MustInherit Class Operacion
    Property trace As String
    Property idTerminal As String
End Class

Public Class Operacion_Compra
    Inherits Operacion
    Property importe As String
    Property moneda As Moneda
    Property idpago As String


    Public Overrides Function ToString() As String
        Dim s As New System.Text.StringBuilder()
        s.Append(trace & "|")
        s.Append(idTerminal & "|")
        s.Append(importe & "|")
        s.Append(CInt(moneda) & "|")
        s.Append(idpago)

        Return s.ToString
    End Function

    Public Sub LeerArgumento(s As String)
        Dim sp As String() = s.Split("|")
        trace = sp(0)
        idTerminal = sp(1)
        importe = sp(2)
        moneda = CType(sp(3), Moneda)
        idpago = sp(4)
    End Sub

End Class

Public Class Respuesta
    Property estado As Estado
    Property descripcion As String
    Property tipoOperacion As String
    Property id_operacion As String
    Property id_pago_origen As String
    Property fechahora_respuesta As String
    Property trace As String
    Property id_pago As String
    Property id_user As String
    Property mail As String



    Public Overrides Function ToString() As String
        Dim s As New System.Text.StringBuilder()

        s.Append(descripcion & "|")
        s.Append(tipoOperacion & "|")
        s.Append(id_operacion & "|")
        s.Append(id_pago_origen & "|")
        s.Append(fechahora_respuesta & "|")
        s.Append(estado & "|")
        s.Append(trace & "|")
        s.Append(id_pago & "|")
        s.Append(id_user & "|")
        s.Append(mail)

        Return s.ToString
    End Function

    Public Sub LeerRespuesta(s As String)
        Dim sp As String() = s.Split("|")
        descripcion = sp(0)
        tipoOperacion = sp(1)
        id_operacion = sp(2)
        id_pago_origen = sp(3)
        fechahora_respuesta = sp(4)
        estado = sp(5)
        trace = sp(6)
        id_pago = sp(7)
        id_user = sp(8)
        mail = sp(9)


    End Sub
End Class