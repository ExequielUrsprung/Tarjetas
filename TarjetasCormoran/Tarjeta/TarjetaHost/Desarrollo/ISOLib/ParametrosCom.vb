Public Class ParametrosCom
    Public tr As e_transaccionesNexo
    Public Tarjeta As Long
    Public Monto As Decimal
    Public FechaNegocio As Date
    Public Fechatransaccion As Date
    Public Lote As Short
    Public NroCupon As Short
    Public idTerminalCOM As String
    Public texto As String
 
    Public MontoAimpactar As Decimal
    Public Respuesta As E_CodigosRespuesta
    Public implementacion As E_Implementaciones
    Public REtRefNumber As String
    Public Trace As Integer
    Public ProcCode As E_ProcCode
    Public Modo As e_ModosTransaccion
    Public MotivoReverso As Integer
    Public Cuenta As Long
    Public pIN As String
    Public Cajero As String
    Public NroAutorizacion As Integer
    '  Public p63 As Mensaje63POS
    Public p63Msg As String
    Public CuentaComercio As Integer
    Public p90 As Mensaje90DatosOriginales
    Public modoIngreso As Integer
    Public Vto As Date = Escuchador.Vtomax
    Public wkey As String


    Public Track1 As String
    Public Track2 As String
End Class
