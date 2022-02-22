Public Class LecturaDB
    Dim ds As New DatosTj
    Public Sub LeerRangHab(rango As Integer, plan As Integer, caja As Integer)






        Dim numerosTA As New DatosTjTableAdapters.NUMEROSTableAdapter
        numerosTA.Fill(ds.NUMEROS)
        ds.NUMEROS.FindByNroCajaPropioNroHost(caja, ds.EMISOR.Rows(0).Item("Host"))

        'Dim HistoriaTA As New DatosTjTableAdapters.HISTORIATableAdapter




    End Sub

    


End Class

Public Class Emisor
    ' BUSCAR EL EMISOR 
    Dim ds As DatosTj

    Public emisor As Integer
    Public descripcion As String
    Public host As Integer
    Public idComercio As String
    Public reqCodSeg As Boolean
    Public req4digitos As Boolean
    Public reqFechaVenc As Boolean



    Sub New(_emisor As Integer)
        emisor = _emisor
        Buscar_emisor()
    End Sub

    Sub Buscar_emisor()
        Dim emisorTA As New DatosTjTableAdapters.EMISORTableAdapter
        emisorTA.Fill(ds.EMISOR)
        Dim emisorRow = ds.EMISOR.FindByEmisor(emisor)
        descripcion = emisorRow.Descripcion
        host = emisorRow.Host
        ' VER TEMA DE ID DE COMERCIO, REVISAR TABLA
        idComercio = emisorRow.IDComercioRaf
        req4digitos = emisorRow.Req4Digitos
        reqCodSeg = emisorRow.ReqCodSeguridad
        reqFechaVenc = emisorRow.ReqFechaVenc
    End Sub

End Class


Public Class Rango
    ' BUSCAR EL RANGO 
    Dim ds As DatosTj
    Public Function Get_Rango(tarjeta As String) As Integer
        Dim ranghabTA As New DatosTjTableAdapters.RANGHABTableAdapter
        ranghabTA.FillByRango(ds.RANGHAB, tarjeta)
        Return ds.RANGHAB.Rows(0).Item("Emisor")
    End Function
End Class


Public Class Numero



End Class


Public Class Plan
    Dim ds As DatosTj

    Public emisor As Integer
    Public plan As Integer
    Public cuotas As Integer
    Public moneda As Decimal
    Public descripcion As String
    Public coef As Decimal

    Sub Buscar_Plan()
        Dim planesTA As New DatosTjTableAdapters.PLANESTableAdapter
        planesTA.Fill(ds.PLANES)
        ds.PLANES.FindByEmisorNroPlan(Emisor, Plan)
    End Sub
End Class




