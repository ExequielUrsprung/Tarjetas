Option Strict On
Imports TjComun.IdaLib
'Imports IsoLib

Public Class ReqQR
#Region "Atributos"
    Property ida As idaQR

    Public vta As VtaQR
    'Public nroTrace As Integer

    Public pinguino As String
    Public nroCaja As Integer
    Public pos_id As String
    Public external_ref As Integer 'nro de trace
    Public external_id As String   '
    Public estado As lib_QR.Estado
    Public idPago As String

    Public ticket As String
    Public cajero As Integer
    Public importe As String
    Public fecha As DateTime



    Public RespInvalida As Boolean = False
    Public CausaInvalida As String

#End Region
    Sub New(_ida As idaQR, pCaja As Integer, pPinguino As Integer)
        ida = _ida
        nroCaja = pCaja
        pinguino = pPinguino.ToString("00")
        BuscarTerminalMP(pCaja, pPinguino, Me)
        If ida.TIPOOPERACION = "COMPRA" Then
            Incrementar_TraceMP(pCaja, pPinguino)
        ElseIf ida.TIPOOPERACION = "DEVOLUCION" Then
            idPago = ida.ticket
            Incrementar_TraceMP(pCaja, pPinguino)
        ElseIf ida.TIPOOPERACION = "CANCELA" Then
            BuscarTransaccionQR(Me)
            If estado = lib_QR.Estado.CANCELLED Then
                InvalidarReq("Orden ya cancelada. Se ignora el pedido.")
            ElseIf estado = lib_QR.Estado.CLOSED Then
                InvalidarReq("Orden ya Aprobada. Se ignora el pedido.")
            End If
        End If

    End Sub

    Sub New(respuesta As lib_QR.Respuesta)
        external_id = respuesta.trace.Split(CChar("-"))(0)
        external_ref = CInt(respuesta.trace.Split(CChar("-"))(1))
        nroCaja = CInt(external_id.Substring(4, 2))
        pinguino = CInt(external_id.Substring(7, 1)).ToString("00")
        idPago = respuesta.id_pago

        BuscarTransaccionQR(Me)
    End Sub




    ''' <summary>
    ''' Marca requermiento como no valido.
    ''' </summary>
    ''' <param name="texto">Descripción del problema.</param>
    ''' <remarks></remarks>
    Public Sub InvalidarReq(texto As String)
        CausaInvalida = texto
        RespInvalida = True
    End Sub

End Class