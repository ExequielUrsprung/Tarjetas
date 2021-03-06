Public Class Mensaje126Adelanto
    Inherits mensajeGenerico
    Public Enum e_m126_tipos
        ConsultaSaldo = 0
        ConsultaAdelantocUOTAS = 1
        ConsultaAdelanto = 2
        Adelanto = 3
        aDELANTOENCUOTAS = 4
    End Enum

    Public Structure tConsultaAdelanto
        Public DispCtaDolar As Decimal
        Public MinAdelPesos As Decimal

        Public MinAdelDolar As Decimal
        Public MaxCtaPesos As Decimal
        Public MaxCtaDolar As Decimal
        Public CantMaxCuotasPesos As Integer
        Public CantMaxCuotasDolar As Integer
        Public CantAdel As Integer
        Public DispCtaPesos As Decimal
        Public CantCuotas As Short

    End Structure

    Public Structure tConsultaSaldo
        Dim Deudadolar As Decimal
        Dim DEudaDolarConv As Decimal
        Dim DEudaPesos As Decimal
        Dim PagoMinimo As Decimal
        Dim PagoPesos As Decimal
        ' arreglar fecha
        Dim FechaVto As Date
        Dim PagoDolares As Decimal
        Dim ConsumoDolares As Decimal
        Dim ConsumoPesos As Decimal
        Dim DispAdelantos As Decimal
        Dim LimiteAdelantos As Decimal
        Dim CantAdel As Short
     End Structure
    Public Structure TAdelantos
        Public DispCtaDolar As Decimal
        Public MinAdelPesos As Decimal
        Public SaldoCuotasPesos As Decimal
        Public SaldoCuotasDolares As Decimal
        Public MontoMaxCuotaDolar As Decimal

        Public CantMaxCuotasPesos As Integer
        Public CantMaxCuotasDolar As Integer
        Public Cantadel As Integer


        Public DispCtaPesos As Decimal

        Public CantCuotas As Short
    End Structure
    Public tnaPesos As Decimal
    Public tnaDolar As Decimal
    Public TemPesos As Decimal
    Public TemDolar As Decimal


    Public Tiptran As Integer
    Public TipoDeb As Integer

    Public NewPinData As Integer
    Public CodigoBcra As Integer
    Public ServiceCode As Integer

    Dim CantMaxAdel As Integer
    Dim TipoPago As Int16

    ' rk7
    Dim SegvidaPesos As Decimal
    Dim SegvidaPesostipo As String = ""
    Dim SegvidaPesosiva As String = ""
    Dim SegvidaDolares As Decimal
    Dim SegvidaDolaresTipo As String = ""
    Dim SegvidaDolaresIva As String = ""
    Dim CargoAdminpesos As Decimal
    Dim CargoAdminpesosTipo As String = ""
    Dim CargoAdminpesosIva As String = ""
    Dim CargoAdminDolares As Decimal
    Dim CargoAdminDolaresTipo As String = ""
    Dim CargoAdminDolaresIva As String = ""
    Dim DispCajeropesos As Decimal
    Dim DispCajeroDolares As Decimal
    Public _Tipo As Integer
    Private _EsConsulta As Boolean
    Public adelanto As TAdelantos
    Public ConsultaAdelanto As tConsultaAdelanto
    Public Consultasaldo As tConsultaSaldo
    Private _saldoCampo44 As Decimal


#Region "Txt 126"
    ' -------------------- banelco
    '0	12	DEUDA-DOLARES	ND 12	Deuda en Dólares facturada en el último resumen.
    '0	12	DISP-CUOTAS-DOLARES	REDEFINICION	Para Transacciones de Consulta de Adelanto en Cuotas y Adelanto de Efectivo en Cuotas en Dólares contiene:
    '				El Disponible en Cuotas en Dólares a informarse al Usuario en el Comprobante.
    '12	12	DEUDA-DOLARES-CONV	ND 12	Deuda en Dólares facturada en el último resumen expresada en Pesos.
    '12	12	MONTO-MIN-ADEL-PESOS	REDEFINICION	Para Transacción de Consulta de Adelanto de Efectivo en Cuotas en Pesos contiene:
    '				El Monto Mínimo de Adelanto de Efectivo en Cuotas en Pesos permitido por la entidad para informase al Usuario en el Comprobante.
    '24	12	DEUDA-PESOS	ND 12	Deuda en Pesos facturada en el último resumen.
    '24	12	MONTO-MIN-ADEL-DOLAR	REDEFINICION	Para Transacciones de Consulta de Adelanto de Efectivo en Cuotas en Dólares contiene:
    '				El Monto Mínimo del Adelanto de Efectivo en Cuotas en Dólares permitido por entidad para informase al Usuario en el Comprobante.
    '24	12	SALDO-CUOTAS-PESOS	REDEFINICION	Para Transacciones de Adelanto de Efectivo en Cuotas en Pesos contiene:
    '				El Saldo en Cuotas en Pesos a informarse al Usuario en el Comprobante.
    '36	12	PAGO-MINIMO	ND 12	Pago mínimo informado en el último resumen.
    '36	12	MONTO-MAX-CUOTA-PESOS	REDEFINICION	Para Transacciones de Consulta de Adelanto de Efectivo en Cuotas en Pesos contiene:
    '				El Monto Máximo de las Cuotas en Pesos permitido por entidad para informase al Usuario en el Comprobante.
    '36	12	SALDO-CUOTAS-DOLARES	REDEFINICION	Para Transacciones Adelanto de Efectivo en Cuotas en Dólares contiene:
    '				El Saldo en Cuotas en Dólares a informarse al Usuario en el Comprobante.
    '48	12	PAGO-PESOS	ND 12	Deuda Total expresada en Pesos.
    '48	12	MONTO-MAX-CUOTA-DÓLAR	REDEFINICION	Para Transacciones Consulta de Adelanto de Efectivo en Cuotas en Dólares contiene:
    '60	6	FECHA-VTO	AN 06	Fecha de Vencimiento del Resumen.
    '60	6	CUOTAS	REDEFINICION	Para Transacciones de Consulta de Adelanto de Efectivo en Cuotas contiene:
    '60	2	CANT-MAXIMA-PESOS	AN 02	La Cantidad Máxima de Cuotas en Pesos permitidas por la institución, para informarse al Usuario en el Comprobante.
    '62	2	CANT-MAXIMA-DOLARES	AN 02	La Cantidad Máxima de Cuotas en Dólares permitidas por la institución, para informarse al Usuario en el Comprobante.
    '64	2	CANT-ADEL-DISPONIBLE	AN 02	La cantidad de ADELANTOS DISPONIBLES para la consulta de adelanto en efectivo.
    '66	12	PAGO-DOLARES	ND 12	Deuda Total expresada en Dólares.
    '66	12	TNA-PESOS	REDEFINICION	Para Transacciones de Consulta de Adelanto en efectivo, Consulta y Adelanto de Efectivo en Cuotas contiene:
    '				La Tasa Nominal Anual en Pesos a aplicarse, para informarse al Usuario en el Comprobante.
    '78	12	CONSUMO-DOLARES	ND 12	Consumos del Período en Dólares.
    '78	12	TNA-DOLARES	REDEFINICION	Para Transacciones de Consulta de Adelanto en efectivo, Consulta y Adelanto de Efectivo en Cuotas contiene:
    '				La Tasa Nominal Anual en Dólares a aplicarse, para informarse al Usuario en el Comprobante.
    '90	12	CONSUMO-PESOS	ND 12	Consumos del Período en Pesos.
    '90	12	TEM-PESOS	REDEFINICION	Para Transacciones de Consulta de Adelanto en efectivo, Consulta y Adelanto de Efectivo en Cuotas contiene:
    '				La Tasa Estimada Mensual en Pesos a aplicarse, para informarse al Usuario en el Comprobante.
    '102	12	DISP-ADELANTOS	ND 12	Disponible de Adelantos.
    '102	12	DISP-CUOTAS-PESOS	REDEFINICION	Para Transacciones de Consulta y Adelanto de Efectivo en Cuotas en Pesos contiene:
    '				El Disponible en Cuotas en Pesos a informarse al Usuario en el Comprobante.
    '114	12	LIM-ADELANTOS	ND 12	Límite de Adelantos del Período.
    '114	12	TEM-DOLARES	REDEFINICION	Para Transacciones de Adelanto de Efectivo en Cuotas en Dólares contiene:
    '				La Tasa Estimada Mensual en Dólares a aplicarse, para informarse al Usuario en el Comprobante.
    '126	2	CANT-ADELANTOS	ND 2	Cantidad de Adelantos Disponibles.
    '126	2	CANTIDAD-CUOTAS	REDEFINICION	Para Transacciones de Consulta y Adelanto de Efectivo en Cuotas (en Pesos y en Dólares) contiene:
    '				La Cantidad de Cuotas seleccionada por el usuario al realizar la operación.
    '128	1	TIPO-DEBITO	AN 1	Código que indica cuando debe imputarse un Pago de Tarjeta
    '129	1	TIPO-PAGO	AN 1	Código que indica cuanto debe imputarse para un Pago (Mínimo, Total, Saldo, otro importe).
    '129	1	TIPO-TRANSACCION	REDEFINICION	Para Todas las Transacciones, en Pesos o en Dólares:
    '				Indica los Tipos de Operación a procesarse:
    '				"0" - Transacciones Normales.
    '				"1" - Transacciones en Cuotas.
    '				“2” – Consulta de adelanto en efectivo.
    '130	4	NEW-PIN-DATA	AN 4	Nuevo código de identificación personal seleccionado por el usuario.
    '134	3	CODIGO-BCRA	AN 3	Código de Banco Central del Administrador del Cajero.
    '137	3	SERVICE-CDE	AN 3	Service Code de la Tarjeta.
#End Region

    Public Property saldoCampo44() As Decimal
        Get
            Return _saldoCampo44
        End Get
        Set(ByVal value As Decimal)
            _saldoCampo44 = value
        End Set
    End Property
    Public Function Tipo() As e_m126_tipos
        Logger.Info("Esconsulta " + _EsConsulta.ToString + " " + " " + _Tipo.ToString)
        If _EsConsulta Then
            '	Al enviarse la transacción al HOST se le indicará el tipo de transacción en el Campo TIPO-TRANSACCION del TOKEN otras tarjetas, con los siguientes valores:
            '	0 = Transacción de Consulta de Saldo de Cuenta de Crédito Normal.
            '	1 = Transacción de Consulta de Adelanto de Efectivo en Cuotas.
            '	2 = Transacción de Consulta de Adelanto de Efectivo Normal
            Select Case _Tipo
                Case 0
                    Return e_m126_tipos.ConsultaSaldo
                Case 1
                    Return e_m126_tipos.ConsultaAdelantocUOTAS
                Case 2
                    Return e_m126_tipos.ConsultaAdelanto
                Case Else
                    Logger.Warn("error en _tipo esconsulta" + _Tipo.ToString)
            End Select
        Else
            '  
            Select Case _Tipo

                '            adelanto()
                '	Al enviarse la transacción al HOST se le indicará el tipo de transacción en el Campo TIPO-TRANSACCION del TOKEN otras tarjetas, con los siguientes valores:
                '	0 = Transacción de Adelanto de Efectivo Normal.
                '	1 = Transacción de Adelanto de Efectivo en Cuotas
                Case 0
                    Return e_m126_tipos.Adelanto
                Case 1
                    Return e_m126_tipos.aDELANTOENCUOTAS
            End Select
        End If
    End Function
    Public Sub New(ByVal pImplementacion As E_Implementaciones, ByVal pValorInicial As String, ByVal pTipo As e_m126_tipos)
        MyBase.New(pImplementacion, pValorInicial)
        _Tipo = pTipo
    End Sub

    Public Sub New(ByVal pImplementacion As E_Implementaciones, ByVal pValorInicial As String, ByVal pcodProc As Integer)
        MyBase.New(pImplementacion, "")

        Select Case pCodproc
            Case E_ProcCode.ConsultaAdelanto_Banelco, E_ProcCode.ConsultaAdelanto_Link, _
                E_ProcCode.ConsultaSaldo_Banelco, E_ProcCode.ConsultaSaldo_link
                _EsConsulta = True
            Case E_ProcCode.AdelantoEnEfectivo_Banelco, E_ProcCode.AdelantoEnEfectivo_Link
                _EsConsulta = False
            Case E_ProcCode.CompraPulsos_Link, E_ProcCode.CompraPulsos_Link1, _
                E_ProcCode.AdhesionPASConsulta_link, E_ProcCode.AdhesionPASConsulta1_link, _
                E_ProcCode.AdhesionPAS_Link, E_ProcCode.AdhesionPASConsulta_ultpagos_link, _
                E_ProcCode.HomeBanking_link
                ' Logger.Info("no se ha definido manera de parsear 126 (3) para codproc:" + pcodProc.ToString + ", pero no se necesita")
                _EsConsulta = False
            Case Else
                'Logger.Info("no se ha definido manera de parsear 126 (2) para codproc:" + pcodProc.ToString)
                _EsConsulta = False
        End Select
        Try
            If pValorInicial <> "" Then Fromstring(pValorInicial)

        Catch ex As Exception
            Logger.Error("FromString de campo 126 error:" + ex.Message + " " + ex.StackTrace)
        End Try

    End Sub

    Public Function EnCuotas() As Boolean
        Return Tiptran = 1
    End Function
    Public Overrides Sub Fromstring(ByVal datos As String)
        Dim resultado As String = "OK"
        Dim s As String = ""
        Try
            Select Case Implementacion
                Case E_Implementaciones.Banelco
                    'banelco tiene varios paquetes adentro
                    Dim CantSeg As Integer
                    CantSeg = CInt(datos.Substring(2, 5)) - 1
                    Dim PorProcesar As String = datos.Substring(12)
                    Do While PorProcesar <> ""
                        Dim HeaderSegmento As String = PorProcesar.Substring(2, 2)
                        PorProcesar = PorProcesar.Substring(10)
                        Select Case HeaderSegmento
                            Case "RK"
                                s = PorProcesar
                                SegvidaPesos = CDec(s.Substring(0, 12)) / 100
                                SegvidaPesostipo = s.Substring(12, 1)
                                SegvidaPesosiva = s.Substring(13, 1)
                                SegvidaDolares = CDec(s.Substring(14, 12)) / 100
                                SegvidaDolaresTipo = s.Substring(26, 1)
                                SegvidaDolaresIva = s.Substring(27, 1)
                                CargoAdminpesos = CDec(s.Substring(28, 12)) / 100
                                CargoAdminpesosTipo = s.Substring(40, 1)
                                CargoAdminpesosIva = s.Substring(41, 1)
                                CargoAdminDolares = CDec(s.Substring(42, 12)) / 100
                                CargoAdminDolaresTipo = s.Substring(54, 1)
                                CargoAdminDolaresIva = s.Substring(55, 1)
                                DispCajeropesos = CDec(s.Substring(56, 12)) / 100
                                DispCajeroDolares = CDec(s.Substring(68, 12)) / 100
                                PorProcesar = PorProcesar.Substring(80)
                            Case "P7"
                                s = PorProcesar
                                TipoDeb = CInt(Val(s.Substring(128, 1)))
                                Tiptran = CInt(Val(s.Substring(129, 1)))
                                Logger.Info("Tipo tran:" + Tiptran.ToString)
                                _Tipo = CInt(Tiptran)
                                Select Case Tipo()
                                    Case e_m126_tipos.Adelanto, e_m126_tipos.aDELANTOENCUOTAS
                                        With adelanto
                                            .DispCtaDolar = CDec(s.Substring(0, 12)) / 100
                                            .MinAdelPesos = CDec(s.Substring(12, 12)) / 100
                                            .SaldoCuotasPesos = CDec(s.Substring(24, 12)) / 100
                                            .SaldoCuotasDolares = CDec(s.Substring(36, 12)) / 100
                                            .MontoMaxCuotaDolar = CDec(s.Substring(48, 12)) / 100
                                            .CantMaxCuotasPesos = CInt(Val(s.Substring(60, 2)))
                                            .CantMaxCuotasDolar = CInt(Val(s.Substring(62, 2)))
                                            .Cantadel = CInt(Val(s.Substring(64, 2)))
                                            tnaPesos = CDec(s.Substring(66, 12)) / 100
                                            tnaDolar = CDec(s.Substring(78, 12)) / 100
                                            TemPesos = CDec(s.Substring(90, 12)) / 100
                                            .DispCtaPesos = CDec(s.Substring(102, 12)) / 100
                                            TemDolar = CDec(s.Substring(114, 12)) / 100
                                            .CantCuotas = CShort(s.Substring(126, 2))
                                        End With
                                    Case e_m126_tipos.ConsultaAdelanto, e_m126_tipos.ConsultaAdelantocUOTAS
                                        'ok
                                        With ConsultaAdelanto
                                            .DispCtaDolar = CDec(s.Substring(0, 12)) / 100
                                            .MinAdelPesos = CDec(s.Substring(12, 12)) / 100
                                            .MinAdelDolar = CDec(s.Substring(24, 12)) / 100
                                            .MaxCtaPesos = CDec(s.Substring(36, 12)) / 100
                                            .MaxCtaDolar = CDec(s.Substring(48, 12)) / 100
                                            .CantMaxCuotasPesos = CInt(Val(s.Substring(60, 2)))
                                            .CantMaxCuotasDolar = CInt(Val(s.Substring(62, 2)))
                                            .CantAdel = CInt(Val(s.Substring(64, 2)))
                                            tnaPesos = CDec(s.Substring(66, 12)) / 100
                                            tnaDolar = CDec(s.Substring(78, 12)) / 100
                                            TemPesos = CDec(s.Substring(90, 12)) / 100
                                            .DispCtaPesos = CDec(s.Substring(102, 12)) / 100
                                            TemDolar = CDec(s.Substring(114, 12)) / 100
                                            .CantCuotas = CShort(s.Substring(126, 2))
                                        End With
                                    Case e_m126_tipos.ConsultaSaldo
                                        Try

                                            With Consultasaldo
                                                .Deudadolar = CDec(s.Substring(0, 12)) / 100
                                                .DEudaDolarConv = CDec(s.Substring(12, 12)) / 100
                                                .DEudaPesos = CDec(s.Substring(24, 12)) / 100
                                                .PagoMinimo = CDec(s.Substring(36, 12)) / 100
                                                .PagoPesos = CDec(s.Substring(48, 12)) / 100
                                                ' arreglar fecha
                                                .FechaVto = DateSerial(2000 + CInt(s.Substring(60, 2)), CInt(s.Substring(62, 2)), CInt(s.Substring(64, 2)))
                                                .PagoDolares = CDec(s.Substring(66, 12)) / 100
                                                .ConsumoDolares = CDec(s.Substring(78, 12)) / 100
                                                .ConsumoPesos = CDec(s.Substring(90, 12)) / 100
                                                .DispAdelantos = CDec(s.Substring(102, 12)) / 100
                                                .LimiteAdelantos = CDec(s.Substring(114, 12)) / 100
                                                .CantAdel = CShort(s.Substring(126, 2))
                                            End With
                                        Catch ex As Exception
                                            Logger.Debug("Error parseando 126 from string2 :" + datos + "/" + ex.Message + " " + ex.StackTrace)


                                        End Try

                                End Select
                                NewPinData = CInt(Val(s.Substring(130, 4)))
                                CodigoBcra = CInt(Val(s.Substring(134, 3)))
                                ServiceCode = CInt(Val(s.Substring(137, 3)))
                                PorProcesar = PorProcesar.Substring(140)
                            Case Else
                                Logger.Error("segmento desconocido en 126:" + HeaderSegmento)
                        End Select
                    Loop

                Case E_Implementaciones.Link
                    s = datos
                    '            TipoDeb = CInt(Val(s.Substring(128, 1)))
                    Tiptran = CInt(Val(s.Substring(128, 1)))
                    Logger.Info("Tipo tran:" + Tiptran.ToString)
                    _Tipo = CInt(Tiptran)
                    Select Case Tipo()
                        Case e_m126_tipos.Adelanto, e_m126_tipos.aDELANTOENCUOTAS
                            With adelanto
                                .DispCtaDolar = CDec(s.Substring(0, 12)) / 100
                                .MinAdelPesos = CDec(s.Substring(12, 12)) / 100
                                .SaldoCuotasPesos = CDec(s.Substring(24, 12)) / 100
                                .SaldoCuotasDolares = CDec(s.Substring(36, 12)) / 100
                                .MontoMaxCuotaDolar = CDec(s.Substring(48, 12)) / 100
                                .CantMaxCuotasPesos = CInt(Val(s.Substring(60, 2)))
                                .CantMaxCuotasDolar = CInt(Val(s.Substring(62, 2)))
                                .Cantadel = CInt(Val(s.Substring(64, 2)))
                                tnaPesos = CDec(s.Substring(66, 12)) / 100
                                tnaDolar = CDec(s.Substring(78, 12)) / 100
                                TemPesos = CDec(s.Substring(90, 12)) / 100
                                .DispCtaPesos = CDec(s.Substring(102, 12)) / 100
                                TemDolar = CDec(s.Substring(114, 12)) / 100
                                .CantCuotas = CShort(s.Substring(126, 2))
                            End With

                        Case e_m126_tipos.ConsultaAdelanto, e_m126_tipos.ConsultaAdelantocUOTAS
                            'ok
                            With ConsultaAdelanto
                                .DispCtaDolar = CDec(s.Substring(0, 12)) / 100
                                .MinAdelPesos = CDec(s.Substring(12, 12)) / 100
                                .MinAdelDolar = CDec(s.Substring(24, 12)) / 100
                                .MaxCtaPesos = CDec(s.Substring(36, 12)) / 100
                                .MaxCtaDolar = CDec(s.Substring(48, 12)) / 100
                                .CantMaxCuotasPesos = CInt(Val(s.Substring(60, 2)))
                                .CantMaxCuotasDolar = CInt(Val(s.Substring(62, 2)))
                                .CantAdel = CInt(Val(s.Substring(64, 2)))
                                tnaPesos = CDec(s.Substring(66, 12)) / 100
                                tnaDolar = CDec(s.Substring(78, 12)) / 100
                                TemPesos = CDec(s.Substring(90, 12)) / 100
                                .DispCtaPesos = CDec(s.Substring(102, 12)) / 100
                                TemDolar = CDec(s.Substring(114, 12)) / 100
                                .CantCuotas = CShort(s.Substring(126, 2))
                            End With
                        Case e_m126_tipos.ConsultaSaldo
                            With Consultasaldo
                                .Deudadolar = CDec(s.Substring(0, 12)) / 100
                                .DEudaDolarConv = CDec(s.Substring(12, 12)) / 100
                                .DEudaPesos = CDec(s.Substring(24, 12)) / 100
                                .PagoMinimo = CDec(s.Substring(36, 12)) / 100
                                .PagoPesos = CDec(s.Substring(48, 12)) / 100
                                ' arreglar fecha
                                .FechaVto = DateSerial(2000 + CInt(s.Substring(60, 2)), CInt(s.Substring(62, 2)), CInt(s.Substring(64, 2)))
                                .PagoDolares = CDec(s.Substring(66, 12)) / 100
                                .ConsumoDolares = CDec(s.Substring(78, 12)) / 100
                                .ConsumoPesos = CDec(s.Substring(90, 12)) / 100
                                .DispAdelantos = CDec(s.Substring(102, 12)) / 100
                                .LimiteAdelantos = CDec(s.Substring(114, 12)) / 100
                                .CantAdel = CShort(s.Substring(126, 2))
                            End With

                    End Select
                    NewPinData = CInt(Val(s.Substring(129, 4)))
                    CodigoBcra = CInt(Val(s.Substring(133, 3)))
                    ServiceCode = CInt(Val(s.Substring(136, 3)))
                    'PorProcesar = PorProcesar.Substring(140)
                Case Else
                    Logger.Error("segmento desconocido en 126:")


            End Select
        Catch ex As Exception
            Logger.Debug("Error parseando 126 from string :" + datos + "/" + ex.Message + " " + ex.StackTrace)
            resultado = ex.Message
        End Try

        LogMessageFromString("From String " + resultado + vbCrLf + Logstate(s))

    End Sub

    Public Overrides Function Tostring() As String
        Dim s As String = ""
        Select Case Implementacion

            Case E_Implementaciones.Banelco
                'largo de los headers
                s = "& 00003" + (10 + 140 + 10 + 80 + 12).ToString("00000")
                'segmento p7
                s = s + "! P700140 "
                Select Case Tipo()
                    Case e_m126_tipos.Adelanto, e_m126_tipos.aDELANTOENCUOTAS
                        With adelanto
                            s = s + Monto2decaStr(.DispCtaDolar) + _
                              Monto2decaStr(.MinAdelPesos) + _
                              Monto2decaStr(.SaldoCuotasPesos) + _
                              Monto2decaStr(.SaldoCuotasDolares) + _
                              Monto2decaStr(.MontoMaxCuotaDolar) + _
                              .CantMaxCuotasPesos.ToString("00") + _
                              .CantMaxCuotasDolar.ToString("00") + _
                              .Cantadel.ToString("00") + _
                              Monto2decaStr(tnaPesos) + _
                              Monto2decaStr(tnaDolar) + _
                              Monto2decaStr(TemPesos) + _
                              Monto2decaStr(.DispCtaPesos) + _
                              Monto2decaStr(TemDolar) + _
                              .CantCuotas.ToString("00") + _
                              TipoDeb.ToString("0") + _
                              Tiptran.ToString("0")
                        End With
                    Case e_m126_tipos.ConsultaAdelanto, e_m126_tipos.ConsultaAdelantocUOTAS
                        With ConsultaAdelanto
                            s = s + Monto2decaStr(.DispCtaDolar) + _
                              Monto2decaStr(.MinAdelPesos) + _
                              Monto2decaStr(.MinAdelDolar) + _
                              Monto2decaStr(.MaxCtaPesos) + _
                              Monto2decaStr(.MaxCtaDolar) + _
                              .CantMaxCuotasPesos.ToString("00") + _
                              .CantMaxCuotasDolar.ToString("00") + _
                              .CantAdel.ToString("00") + _
                              Monto2decaStr(tnaPesos) + _
                              Monto2decaStr(tnaDolar) + _
                              Monto2decaStr(TemPesos) + _
                              Monto2decaStr(.DispCtaPesos) + _
                              Monto2decaStr(TemDolar) + _
                              .CantCuotas.ToString("00") + _
                              TipoDeb.ToString("0") + _
                              Tiptran.ToString("0")
                        End With
                    Case e_m126_tipos.ConsultaSaldo
                        With Consultasaldo
                            s = s + Monto2decaStr(.Deudadolar) + _
                              Monto2decaStr(.DEudaDolarConv) + _
                              Monto2decaStr(.DEudaPesos) + _
                              Monto2decaStr(.PagoMinimo) + _
                              Monto2decaStr(.PagoPesos) + _
                              .FechaVto.ToString("yyMMdd") + _
                              Monto2decaStr(.PagoDolares) + _
                              Monto2decaStr(.ConsumoDolares) + _
                              Monto2decaStr(.ConsumoPesos) + _
                              Monto2decaStr(.DispAdelantos) + _
                              Monto2decaStr(.LimiteAdelantos) + _
                              .CantAdel.ToString("00") + _
                              TipoDeb.ToString("0") + _
                              Tiptran.ToString("0")
                        End With
                End Select

                s = s + NewPinData.ToString("0000") + _
                  CodigoBcra.ToString("000") + _
                  ServiceCode.ToString("000")
                'segmento RK
                s = s + "! RK00080 " + _
                  Monto2decaStr(SegvidaPesos) + _
                  SegvidaPesostipo.PadLeft(1) + _
                  SegvidaPesosiva.PadLeft(1) + _
                  Monto2decaStr(SegvidaDolares) + _
                  SegvidaDolaresTipo.PadLeft(1) + _
                  SegvidaDolaresIva.PadLeft(1) + _
                  Monto2decaStr(CargoAdminpesos) + _
                  CargoAdminpesosTipo.PadLeft(1) + _
                  CargoAdminpesosIva.PadLeft(1) + _
                  Monto2decaStr(CargoAdminDolares) + _
                  CargoAdminDolaresTipo.PadLeft(1) + _
                  CargoAdminDolaresIva.PadLeft(1) + _
                  Monto2decaStr(DispCajeropesos) + _
                  Monto2decaStr(DispCajeroDolares)
            Case E_Implementaciones.Link
                'largo de los headers
                ' s = "& 00003" + (10 + 140 + 10 + 80 + 12).ToString("00000")
                'segmento p7
                's = s + "! P700140 "
                s = ""
                Select Case Tipo()
                    Case e_m126_tipos.Adelanto, e_m126_tipos.aDELANTOENCUOTAS
                        With adelanto
                            s = s + Monto2decaStr(.DispCtaDolar) + _
                              Monto2decaStr(.MinAdelPesos) + _
                              Monto2decaStr(.SaldoCuotasPesos) + _
                              Monto2decaStr(.SaldoCuotasDolares) + _
                              Monto2decaStr(.MontoMaxCuotaDolar) + _
                              .CantMaxCuotasPesos.ToString("00") + _
                              .CantMaxCuotasDolar.ToString("00") + _
                              .Cantadel.ToString("00") + _
                              Monto2decaStr(tnaPesos) + _
                              Monto2decaStr(tnaDolar) + _
                              Monto2decaStr(TemPesos) + _
                              Monto2decaStr(.DispCtaPesos) + _
                              Monto2decaStr(TemDolar) + _
                              .CantCuotas.ToString("00") + _
                        Tiptran.ToString("0")
                            'TipoDeb.ToString("0") + _

                        End With
                    Case e_m126_tipos.ConsultaAdelanto, e_m126_tipos.ConsultaAdelantocUOTAS
                        With ConsultaAdelanto
                            s = s + Monto2decaStr(.DispCtaDolar) + _
                              Monto2decaStr(.MinAdelPesos) + _
                              Monto2decaStr(.MinAdelDolar) + _
                              Monto2decaStr(.MaxCtaPesos) + _
                              Monto2decaStr(.MaxCtaDolar) + _
                              .CantMaxCuotasPesos.ToString("00") + _
                              .CantMaxCuotasDolar.ToString("00") + _
                              .CantAdel.ToString("00") + _
                              Monto2decaStr(tnaPesos) + _
                              Monto2decaStr(tnaDolar) + _
                              Monto2decaStr(TemPesos) + _
                              Monto2decaStr(.DispCtaPesos) + _
                              Monto2decaStr(TemDolar) + _
                              .CantCuotas.ToString("00") + _
                              TipoDeb.ToString("0") + _
                              Tiptran.ToString("0")
                        End With
                    Case e_m126_tipos.ConsultaSaldo
                        With Consultasaldo
                            s = s + Monto2decaStr(.Deudadolar) + _
                              Monto2decaStr(.DEudaDolarConv) + _
                              Monto2decaStr(.DEudaPesos) + _
                              Monto2decaStr(.PagoMinimo) + _
                              Monto2decaStr(.PagoPesos) + _
                              .FechaVto.ToString("ddMMyy") + _
                              Monto2decaStr(.PagoDolares) + _
                              Monto2decaStr(.ConsumoDolares) + _
                              Monto2decaStr(.ConsumoPesos) + _
                              Monto2decaStr(.DispAdelantos) + _
                              Monto2decaStr(.LimiteAdelantos) + _
                              .CantAdel.ToString("00") + _
                              TipoDeb.ToString("0") + _
                              Tiptran.ToString("0")
                        End With
                End Select

                s = s + NewPinData.ToString("0000") + _
                  CodigoBcra.ToString("000") + _
                  ServiceCode.ToString("000")
                ''segmento RK
                's = s + "! RK00080 " + _
                '  Monto2decaStr(SegvidaPesos) + _
                '  SegvidaPesostipo.PadLeft(1) + _
                '  SegvidaPesosiva.PadLeft(1) + _
                '  Monto2decaStr(SegvidaDolares) + _
                '  SegvidaDolaresTipo.PadLeft(1) + _
                '  SegvidaDolaresIva.PadLeft(1) + _
                '  Monto2decaStr(CargoAdminpesos) + _
                '  CargoAdminpesosTipo.PadLeft(1) + _
                '  CargoAdminpesosIva.PadLeft(1) + _
                '  Monto2decaStr(CargoAdminDolares) + _
                '  CargoAdminDolaresTipo.PadLeft(1) + _
                '  CargoAdminDolaresIva.PadLeft(1) + _
                '  Monto2decaStr(DispCajeropesos) + _
                '  Monto2decaStr(DispCajeroDolares)
                'With ConsultaAdelanto
                '    s = _
                '      Monto2decaStr(.DispCtaDolar) + _
                '      Monto2decaStr(.MinAdelPesos) + _
                '      Monto2decaStr(.MinAdelDolar) + _
                '      Monto2decaStr(.MaxCtaPesos) + _
                '      Monto2decaStr(.MaxCtaDolar) + _
                '      .CantMaxCuotasPesos.ToString("00") + _
                '      .CantMaxCuotasDolar.ToString("00 ") + "  " + _
                '      Monto2decaStr(.tnaPesos) + _
                '      Monto2decaStr(.tnaDolar) + _
                '      Monto2decaStr(.TemPesos) + _
                '      Monto2decaStr(.DispCtaPesos) + _
                '      Monto2decaStr(.TemDolar) + _
                '      .CantCuotas.ToString("00") + _
                '      Tiptran.ToString("0")
                'End With
                's = s + NewPinData.ToString("0000") + _
                '  CodigoBcra.ToString("000") + _
                '  ServiceCode.ToString("000")
        End Select
        LogMessageToString(Logstate(s))

        Return s
    End Function


    Public Overrides Function Logstate(ByVal s As String) As String

        Select Case _Tipo
            Case e_m126_tipos.Adelanto
                With adelanto
                    s = "ADELANTO " + vbCrLf + _
                      "Disp. Dolar  :" + .DispCtaDolar.ToString + vbCrLf + _
                      "Min Adel $   :" + .MinAdelPesos.ToString + vbCrLf + _
                      "Saldo cta $  :" + .SaldoCuotasPesos.ToString + vbCrLf + _
                      "Saldo Cta U$s:" + .SaldoCuotasDolares.ToString + vbCrLf + _
                      "Monto Max u$s:" + .MontoMaxCuotaDolar.ToString + vbCrLf + _
                      "Cant Max Cta$:" + .CantMaxCuotasPesos.ToString + vbCrLf + _
                      "Cant Max CtaU:" + .CantMaxCuotasDolar.ToString + vbCrLf + _
                      "TNA $        :" + tnaPesos.ToString + vbCrLf + _
                      "TNA u$S      :" + tnaDolar.ToString + vbCrLf + _
                      "TEM $        :" + TemPesos.ToString + vbCrLf + _
                      "Disp Pesos   :" + .DispCtaPesos.ToString + vbCrLf + _
                      "TEM u$S      :" + TemDolar.ToString + vbCrLf + _
                      "Cant Cuotas  :" + .CantCuotas.ToString + vbCrLf
                End With
            Case e_m126_tipos.ConsultaAdelanto
                With ConsultaAdelanto
                    s = "CONSULTA ADELANTO " + vbCrLf + _
                     "Disp. Dolar  :" + .DispCtaDolar.ToString + vbCrLf + _
                      "Min Adel $   :" + .MinAdelPesos.ToString + vbCrLf + _
                      "Min Adel u$s :" + .MinAdelDolar.ToString + vbCrLf + _
                      "Max Cta $    :" + .MaxCtaPesos.ToString + vbCrLf + _
                      "Max Cta u$s  :" + .MaxCtaDolar.ToString + vbCrLf + _
                      "Cant Max Cta$:" + .CantMaxCuotasPesos.ToString + vbCrLf + _
                      "Cant Max CtaU:" + .CantMaxCuotasDolar.ToString + vbCrLf + _
                      "TNA $        :" + tnaPesos.ToString + vbCrLf + _
                      "TNA u$S      :" + tnaDolar.ToString + vbCrLf + _
                      "TEM $        :" + TemPesos.ToString + vbCrLf + _
                      "Disp Pesos   :" + .DispCtaPesos.ToString + vbCrLf + _
                      "TEM u$S      :" + TemDolar.ToString + vbCrLf + _
                      "Cant Cuotas  :" + .CantCuotas.ToString + vbCrLf
                End With
            Case e_m126_tipos.ConsultaSaldo
                With Consultasaldo
                    s = "CONSULTA SALDO " + vbCrLf + _
                     "Deuda Dolar:" + .Deudadolar.ToString + vbCrLf + _
                      "Deuda Dol$   :" + .DEudaDolarConv.ToString + vbCrLf + _
                      "Deuda Pesos  :" + .DEudaPesos.ToString + vbCrLf + _
                      "Pago Minimo  :" + .PagoMinimo.ToString + vbCrLf + _
                      "Pago Pesos   :" + .PagoPesos.ToString + vbCrLf + _
                      "Fecha Vto    :" + .FechaVto.ToString + vbCrLf + _
                      "Pago Dolares :" + .PagoDolares.ToString + vbCrLf + _
                      "Consumo Dolar:" + .ConsumoDolares.ToString + vbCrLf + _
                      "Consumo Pesos:" + .ConsumoPesos.ToString + vbCrLf + _
                      "Disp.Adel    :" + .DispAdelantos.ToString + vbCrLf + _
                      "Limite Adel  :" + .LimiteAdelantos.ToString + vbCrLf + _
                      "Cant Adel    :" + .CantAdel.ToString + vbCrLf
                End With
        End Select
        Return s + "Tipo Trans   :" + Tiptran.ToString + vbCrLf + _
          "Tipo Debito  :" + TipoDeb.ToString + vbCrLf + _
          "New Pin Data :" + NewPinData.ToString + vbCrLf + _
          "Codigo BCRA  :" + CodigoBcra.ToString + vbCrLf + _
          "Service Code :" + ServiceCode.ToString + vbCrLf
        '  "RK " + _
        '"Seg Vida    $:" + SegvidaPesos.ToString + vbCrLf + _
        '"Seg Vida Tip$:" + SegvidaPesostipo.ToString + vbCrLf + _
        '"Seg Vida iva$:" + SegvidaPesosiva.ToString + vbCrLf + _
        '"Seg Vida    D:" + SegvidaDolares.ToString + vbCrLf + _
        '"Seg Vida TipD:" + SegvidaDolaresTipo.ToString + vbCrLf + _
        '"Seg Vida ivaD:" + SegvidaDolaresIva.ToString + vbCrLf + _
        '"Carg.adm     :" + CargoAdminpesos.ToString + vbCrLf + _
        '"Carg.adm Tip$:" + CargoAdminpesosTipo.ToString + vbCrLf + _
        '"Carg.adm iva$:" + CargoAdminpesosIva.ToString + vbCrLf + _
        '"Carg.adm    D:" + CargoAdminDolares.ToString + vbCrLf + _
        '"Carg.adm TipD:" + CargoAdminDolaresTipo.ToString + vbCrLf + _
        '"Carg.adm ivaD:" + CargoAdminDolaresIva.ToString + vbCrLf + _
        '"Saldo en 44  :" + saldoCampo44.ToString + vbCrLf + _
        '  "126          :" + s

    End Function

End Class

