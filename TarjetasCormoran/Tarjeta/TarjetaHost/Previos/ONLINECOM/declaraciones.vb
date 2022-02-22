Imports Trx.Messaging.FlowControl
Imports Trx.Messaging.Channels
Imports Trx.Messaging.Iso8583
Imports Trx.Messaging

Imports log4net
Imports System.Reflection
Public Module declaraciones
  

    Public Enum NexoProcCode
        AdelantoEnEfectivo = 101300
        PagoAutomaticoServicio1 = 883100
        PagoAutomaticoServicio2 = 813100
    End Enum
    Public Function Logger() As ILog
        Return LogManager.GetLogger(MethodBase.GetCurrentMethod.DeclaringType)
    End Function
    Public Function Settings() As Object
        Return My.Settings
    End Function
    Public Function Field39_PonerRespuesta(ByRef mensaje As Iso8583messageNexo, ByVal pCodResp As E_CodigosRespuesta) As String
        Dim CodResp As String = CInt(pCodResp).ToString("00")
        mensaje.Fields.Add(39, CodResp)
        Logger.Info("Respuesta " + CodResp)
        Return CodResp
    End Function
    Public Function Agregar7_FechaHora(ByRef mensaje As Iso8583messageNexo) As DateTime
        Dim transmissionDate As DateTime = FechaActual()

        mensaje.Fields.Add(E_Fields.Field7TransDateTime, String.Format("{0}{1}", _
         String.Format("{0:00}{1:00}", transmissionDate.Month, transmissionDate.Day), _
         String.Format("{0:00}{1:00}{2:00}", transmissionDate.Hour, _
         transmissionDate.Minute, transmissionDate.Second)))

        Logger.Info("Transmision Date:" + transmissionDate.ToString)

        Return transmissionDate

    End Function

    Public Function Field11_PonerTrace(ByRef mensaje As Iso8583messageNexo, ByRef Numerador As Trx.Messaging.VolatileStanSequencer) As Integer
        Dim s As String = Numerador.Increment().ToString()
        mensaje.Fields.Add(E_Fields.Field11Trace, s)

        Logger.Info("Numerador:" + s)
        Return Numerador.CurrentValue
    End Function
    Public Function Field4_ObtenerMonto(ByRef mensaje As Iso8583messageNexo) As Decimal
        Dim _Monto As Decimal
        _Monto = CDec(mensaje.Fields(4).ToString) / 100
        Logger.Info("MONTO:" + _Monto.ToString)
        Return _Monto
    End Function
    
    Public Function Field17_ObtenerFechaNegocio(ByRef mensaje As Iso8583messageNexo) As Date
        '17	cap-dat	Fecha de negocios de la transacción
        'Campo 17	mmdd 9(4)
        'Como el anio no viene, ojo suponerlo en base a la fecha actual
        Dim s As String = mensaje.Fields(17).ToString
        Dim Anio As Integer
        Anio = Year(FechaActual)
        Dim _fechaNegocio As Date = DateSerial(Anio, CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)))
        ' ahora verificamos que la fecha de negocio obtenida no este en el futuro, lo que significaria que supusimos
        ' mal el anio
        If _fechaNegocio.Subtract(FechaActual).TotalDays > 100 Then
            _fechaNegocio = DateSerial(Anio - 1, CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)))
        End If
        Logger.Info("FECHA NEGOCIO:" + _fechaNegocio.ToString)
        Return _fechaNegocio
    End Function
    Public Function Field12y13_ObteneHoraFecha(ByRef mensaje As Iso8583messageNexo) As Date
        Dim s As String = mensaje.Fields(13).ToString
        Dim Anio As Integer
        Anio = Year(FechaActual)
        Dim _fechatran As Date = DateSerial(Anio, CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)))
        ' ahora verificamos que la fecha de negocio obtenida no este en el futuro, lo que significaria que supusimos
        ' mal el anio
        If _fechatran.Subtract(FechaActual).TotalDays > 100 Then
            _fechatran = DateSerial(Anio - 1, CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)))
        End If
        ' agregar la hora desde el campo 12
        s = mensaje.Fields(12).ToString
        _fechatran.AddHours(CInt(s.Substring(0, 2)))
        _fechatran.AddMinutes(CInt(s.Substring(2, 2)))
        _fechatran.AddSeconds(CInt(s.Substring(4, 2)))
        Logger.Info("FECHA/HORA TRANS:" + _fechatran.ToString)
        Return _fechatran
    End Function
    Public Function ObtenerTcpChannel(ByVal pImplementacion As E_Implementaciones, Optional ByVal pHostName As String = "", Optional ByVal pport As Integer = 0) As TcpChannel
        ' aca manejamos como vienen los mensajes ISO dentro de un paquete TCP/IP
        Dim pTcpChannel As TcpChannel
        Select Case pImplementacion
            Case E_Implementaciones.Link
                ' este esta bien, en la especificacion lo aclara
                pTcpChannel = New TwoBytesNboHeaderChannel(New Iso8583Nexo(pImplementacion))
                pTcpChannel.Send3 = False

            Case E_Implementaciones.Banelco
                ' este no esta aclarado en la especificacion y lo mande a preguntar
                pTcpChannel = New TwoBytesNboHeaderChannel(New Iso8583Nexo(pImplementacion))
                pTcpChannel.Send3 = True
            Case Else
                ' y este es el resto.....
                pTcpChannel = New TwoBytesNboHeaderChannel(New Iso8583Nexo(pImplementacion))

        End Select

        If pport <> 0 Then pTcpChannel.Port = pport
        If pHostName <> "" Then pTcpChannel.HostName = pHostName
        Return pTcpChannel
    End Function

    Public Function Agregar12y13_HoraFecha(ByRef mensaje As Iso8583messageNexo, ByVal FEchaHoraNegocio As DateTime) As DateTime
        mensaje.Fields.Add(12, String.Format("{0}{1}{2}", FEchaHoraNegocio.Hour.ToString("00"), FEchaHoraNegocio.Minute.ToString("00"), FEchaHoraNegocio.Second.ToString("00")))
        mensaje.Fields.Add(13, String.Format("{0}{1}", FEchaHoraNegocio.Month.ToString("00"), FEchaHoraNegocio.Day.ToString("00")))
        Logger.Info("Field 12/13:" + FEchaHoraNegocio.ToString)
        Return FEchaHoraNegocio
    End Function

    Public Sub Agregar14_expdate(ByVal Mensaje As Iso8583messageNexo, ByVal Expdate As Date)
        Mensaje.Fields.Add(14, Expdate.ToString("yyMM"))
    End Sub

    Public Sub agregar22_modoIngreso(ByVal mensaje As Iso8583messageNexo, ByVal ModoIngreso As E_ModoIngreso)
        mensaje.Fields.Add(22, CInt(ModoIngreso).ToString("00") + Space(1))
    End Sub
    Public Function Field49_ObtenerMoneda(ByRef mensaje As Iso8583messageNexo) As E_Monedas
        Return CType(CInt(mensaje.Fields(49).ToString), E_Monedas)
    End Function
    ' obtiene el nro de tarjeta del campo 2 o de alguno de los tracks (campo 35,track 2), por si no viene en el campo2 (posnet)
    Public Function Field2or35_ObtenerTarjeta(ByRef mensaje As Iso8583messageNexo) As Int64
        Dim _tarjeta As Int64
        If mensaje.Fields.Contains(2) Then
            _tarjeta = CLng(mensaje.Fields(2).ToString)
        ElseIf mensaje.Fields.Contains(35) Then
            ' TODO: Validar bien la posicion del PAN (nro de tarjeta) dentro del trakc 2
            _tarjeta = CLng(mensaje.Fields(35).ToString.Substring(3, 16))
        End If
        Logger.Info("PAN:" + _tarjeta.ToString)
        Return _tarjeta
    End Function
    Public Function FechaActual() As DateTime
        ' esta es la fecha que usamos para todos los mensajes
        Return DateTime.Now
    End Function

End Module
