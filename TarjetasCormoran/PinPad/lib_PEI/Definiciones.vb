Public MustInherit Class Operacion
    Property tipoOperacion As Concepto
    Property track1 As String
    Property track2 As String
    Property ultimos As String
    Property traceTrxComercio As String
    Property idReferenciaOperacionComercio As String
    Property idTerminal As String
    Property idCanal As String
    Property Documentotitular As String
    Property codigoSeguridad As String
    Property idComercio As String
End Class



Public Class Operacion_Compra
    Inherits Operacion
    Property importe As Decimal
    Property moneda As Moneda


    Public Overrides Function ToString() As String
        Dim s As New System.Text.StringBuilder()
        s.Append(track1 & "|")
        s.Append(track2 & "|")
        s.Append(ultimos & "|")
        s.Append(traceTrxComercio & "|")
        s.Append(idReferenciaOperacionComercio & "|")
        s.Append(idTerminal & "|")
        s.Append(idCanal & "|")
        s.Append(Documentotitular & "|")
        s.Append(codigoSeguridad & "|")
        s.Append(idComercio & "|")
        s.Append(importe & "|")
        s.Append(CInt(moneda) & "|")
        s.Append(CInt(tipoOperacion))

        Return s.ToString
    End Function

    Public Sub LeerArgumento(s As String)
        Dim sp As String() = s.Split("|")
        track1 = sp(0)
        track2 = sp(1)
        ultimos = sp(2)
        traceTrxComercio = sp(3)
        idReferenciaOperacionComercio = sp(4)
        idTerminal = sp(5)
        idCanal = sp(6)
        Documentotitular = sp(7)
        codigoSeguridad = sp(8)
        idComercio = sp(9)
        importe = sp(10)
        moneda = CType(sp(11), lib_PEI.Moneda)
        tipoOperacion = CType(sp(12), lib_PEI.Concepto)

    End Sub

End Class

Public Class Operacion_Devolucion
    Inherits Operacion

    Property idPagoOriginal As String

    Public Overrides Function ToString() As String
        Dim s As New System.Text.StringBuilder()
        s.Append(track1 & "|")
        s.Append(track2 & "|")
        s.Append(ultimos & "|")
        s.Append(traceTrxComercio & "|")
        s.Append(idReferenciaOperacionComercio & "|")
        s.Append(idTerminal & "|")
        s.Append(idCanal & "|")
        s.Append(Documentotitular & "|")
        s.Append(codigoSeguridad & "|")
        s.Append(idComercio & "|")
        s.Append(CInt(tipoOperacion) & "|")
        s.Append(idPagoOriginal)

        Return s.ToString
    End Function

    Public Sub LeerArgumento(s As String)
        Dim sp As String() = s.Split("|")
        track1 = sp(0)
        track2 = sp(1)
        ultimos = sp(2)
        traceTrxComercio = sp(3)
        idReferenciaOperacionComercio = sp(4)
        idTerminal = sp(5)
        idCanal = sp(6)
        Documentotitular = sp(7)
        codigoSeguridad = sp(8)
        idComercio = sp(9)
        tipoOperacion = CType(sp(10), lib_PEI.Concepto)
        idPagoOriginal = sp(11)

    End Sub
End Class

Public Class Operacion_Devolucion_Parcial
    Inherits Operacion_Devolucion
    Property importe As Decimal
    Property moneda As Moneda


    Public Overrides Function ToString() As String
        Dim s As New System.Text.StringBuilder()
        s.Append(track1 & "|")
        s.Append(track2 & "|")
        s.Append(ultimos & "|")
        s.Append(traceTrxComercio & "|")
        s.Append(idReferenciaOperacionComercio & "|")
        s.Append(idTerminal & "|")
        s.Append(idCanal & "|")
        s.Append(Documentotitular & "|")
        s.Append(codigoSeguridad & "|")
        s.Append(idComercio & "|")
        s.Append(importe & "|")
        s.Append(CInt(moneda) & "|")
        s.Append(CInt(tipoOperacion) & "|")
        s.Append(idPagoOriginal)

        Return s.ToString
    End Function

    Public Overloads Sub LeerArgumento(s As String)
        Dim sp As String() = s.Split("|")
        track1 = sp(0)
        track2 = sp(1)
        ultimos = sp(2)
        traceTrxComercio = sp(3)
        idReferenciaOperacionComercio = sp(4)
        idTerminal = sp(5)
        idCanal = sp(6)
        Documentotitular = sp(7)
        codigoSeguridad = sp(8)
        idComercio = sp(9)
        importe = sp(10)
        moneda = CType(sp(11), lib_PEI.Moneda)
        tipoOperacion = CType(sp(12), lib_PEI.Concepto)
        idPagoOriginal = sp(13)
    End Sub
End Class


Public Class Operacion_Consulta
    Inherits Operacion

    Property idPagoOriginal As String
    Property idFechaOriginal As String

    Public Overrides Function ToString() As String
        Dim s As New System.Text.StringBuilder()
        s.Append(track1 & "|")
        s.Append(track2 & "|")
        s.Append(ultimos & "|")
        s.Append(traceTrxComercio & "|")
        s.Append(idReferenciaOperacionComercio & "|")
        s.Append(idTerminal & "|")
        s.Append(idCanal & "|")
        s.Append(Documentotitular & "|")
        s.Append(codigoSeguridad & "|")
        s.Append(idComercio & "|")
        s.Append(CInt(tipoOperacion) & "|")
        s.Append(idPagoOriginal & "|")
        s.Append(idFechaOriginal)

        Return s.ToString
    End Function

    Public Sub LeerArgumento(s As String)
        Dim sp As String() = s.Split("|")
        track1 = sp(0)
        track2 = sp(1)
        ultimos = sp(2)
        traceTrxComercio = sp(3)
        idReferenciaOperacionComercio = sp(4)
        idTerminal = sp(5)
        idCanal = sp(6)
        Documentotitular = sp(7)
        codigoSeguridad = sp(8)
        idComercio = sp(9)
        tipoOperacion = CType(sp(10), lib_PEI.Concepto)
        idPagoOriginal = sp(11)
        idFechaOriginal = sp(6)
    End Sub
End Class















Public Class Operacion_Devolucion_Banelco
    Inherits Operacion_Devolucion
    'para banelco
    Property cbu As String
    Property aliascbu As String

End Class

Public Class Operacion_Devolucion_Parcial_Banelco
    Inherits Operacion_Devolucion
    Property importe As Decimal
    Property moneda As Moneda
    'para banelco
    Property cbu As String
    Property aliascbu As String
End Class



'Public Class Operacion_Compra
'    Property track1 As String
'    Property track2 As String
'    Property ultimos As String
'    Property traceTrxComercio As String
'    Property idReferenciaOperacionComercio As String
'    Property importe As Decimal
'    Property moneda As Moneda
'    Property idTerminal As String
'    Property idCanal As String
'    'Property posEntryMode As EntryMode
'    Property Documentotitular As String
'    Property codigoSeguridad As String
'    'Property conceptoOperacional As Concepto
'    Property idComercio As String


'End Class

'Public Class Operacion_Devolucion_Total
'    Property track1 As String
'    Property track2 As String
'    Property ultimos As String
'    Property traceTrxComercio As String
'    Property idReferenciaOperacionComercio As String
'    'Property importe As Decimal
'    'Property moneda As Moneda
'    Property idTerminal As String
'    Property idCanal As String
'    Property Documentotitular As String
'    Property codigoSeguridad As String
'    Property idComercio As String 'es para el inicio de sesion


'    'devolucion
'    Property idPagoOriginal As String
'    'para banelco
'    Property cbu As String
'    Property aliascbu As String

'End Class

'Public Class Operacion_Devolucion_Parcial

'    Property track1 As String
'    Property track2 As String
'    Property ultimos As String
'    Property traceTrxComercio As String
'    Property idReferenciaOperacionComercio As String
'    Property importe As Decimal
'    Property moneda As Moneda
'    Property idTerminal As String
'    Property idCanal As String
'    Property Documentotitular As String
'    Property codigoSeguridad As String
'    Property idComercio As String

'    'devolucion

'    Property idPagoOriginal As String
'    'para banelco
'    Property cbu As String
'    Property aliascbu As String

'End Class

Public Class Respuesta
    Property descRespuesta As mensajesRepuesta
    Property descripcion As String
    Property tipoOperacion As String
    Property id_operacion As String
    Property nro_ref_bancaria As String
    Property id_operacion_origen As String
    Property fechahora_respuesta As String
    Property trace As String



    Public Overrides Function ToString() As String
        Dim s As New System.Text.StringBuilder()
        s.Append(descRespuesta & "|")
        s.Append(descripcion & "|")
        s.Append(tipoOperacion & "|")
        s.Append(id_operacion & "|")
        s.Append(nro_ref_bancaria & "|")
        s.Append(id_operacion_origen & "|")
        s.Append(fechahora_respuesta & "|")
        s.Append(trace)

        Return s.ToString
    End Function



    Public Sub LeerRespuesta(s As String)
        Dim sp As String() = s.Split("|")
        descRespuesta = sp(0)
        descripcion = sp(1)
        tipoOperacion = sp(2)
        id_operacion = sp(3)
        nro_ref_bancaria = sp(4)
        id_operacion_origen = sp(5)
        fechahora_respuesta = sp(6)
        trace = sp(7)
    End Sub
End Class


Public Enum Moneda
    ARS
    USD
End Enum

Public Enum EntryMode
    Ban = 902

End Enum

Public Enum mensajesRepuesta
    APROBADA
    NO_APROBADA
    FALTA_TOKEN
    ERROR_DESCONOCIDO
    SIN_RESPUESTA
    SUCURSAL_INVALIDA
    CONCEPTO_INVALIDO
    CONCEPTO_NO_DISPONIBLE
    CONCEPTO_INHABILITADO
    CUENTA_DESTINO_INVALIDA
    CUENTA_DESTINO_INHABILITADA
    CUENTA_ORIGEN_INVALIDA
    CUENTA_ORIGEN_INHABILITADA
    RED_DESTINO_INVALIDA
    CUENTA_COMERCIO_INVALIDA
    CUENTA_SUCURSAL_INVALIDA
    LIMITE_COMERCIO_CONCEPTO_EXCEDIDO
    LIMITE_SUCURSAL_CONCEPTO_EXCEDIDO
    LIMITE_DIARIO_EXCEDIDO
    LIMITE_DIARIO_COMERCIO_EXCEDIDO
    LIMITE_MENSUAL_COMERCIO_EXCEDIDO
    LIMITE_TRX_DIARIO_COMERCIO_EXCEDIDO
    LIMITE_TRX_MENSUAL_COMERCIO_EXCEDIDO
    LIMITE_DIARIO_SUCURSAL_EXCEDIDO
    LIMITE_MENSUAL_SUCURSAL_EXCEDIDO
    LIMITE_TRX_DIARIO_SUCURSAL_EXCEDIDO
    LIMITE_TRX_MENSUAL_SUCURSAL_EXCEDIDO
    ENCRIPTACION_INCORRECTA
    DNI_INCORRECTO
    TARJETA_INCORRECTA
    COMERCIO_INVALIDO
    ID_TERMINAL_INVALIDO
    CUENTA_DE_COMERCIO_INVALIDA
    TARJETA_INVALIDA
    REFERENCIA_TRX_COMERCIO_REPETIDA
    IMPORTE_INCORRECTO
    ULTIMOS_4_DIGITOS_INCORRECTO
    TARJETA_INHABILITADA
    TARJETA_VENCIDA
    FONDOS_INSUFICIENTES
    ERROR_GENERICO
    ID_REQUERIMIENTO_INVALIDO
    IP_CLIENTE_INVALIDA

    CBU_BANELCO_INVALIDO
    ALIAS_BANELCO_INVALIDO
    ID_PAGO_INVALIDO
    ID_CANAL_INVALIDO
    SALDO_EXCEDIDO
    PAGO_YA_DEVUELTO
    PAGO_TIENE_DEVOLUCIONES_PARCIALES
    PAGO_NO_ADMITE_DEVOLUCION_TOTAL

    PAGO_NO_ADMITE_DEVOLUCIONES_PARCIALES
End Enum

Public Enum Concepto
    COMPRA_DE_BIENES
    EXTRACCION
    PAGO_DE_SERVICIOS
    COMPRA_EXTRACCION
    DEVOLUCION
    CONSULTA
End Enum





