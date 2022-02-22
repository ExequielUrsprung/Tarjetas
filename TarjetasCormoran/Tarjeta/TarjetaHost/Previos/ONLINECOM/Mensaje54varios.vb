Public Class Mensaje54varios
    Inherits mensajeGenerico
    'Excepto para PAGO AUTOMATICO DE SERVICIOS y TRANSACCIONES AFJP
    'LONGITUD	Longitud del Primary Bit 54. El valor a informar es `100`.	9(3)
    'FILLER	Se deberá informar BLANCOS.	 X(12)
    'WITH-ADV-AVAIL	Cantidad de extracciones disponibles para  cuenta de débito o cantidad de adelantos disponibles para cuenta de crédito.	9(2)
    'INT-OWE- AUSTRAL	Para cuenta de crédito representa el  saldo  del último  resumen en pesos o los intereses ganados para cuenta débito.
    'CASH-AVAIL	Dinero  disponible  para  cuenta  de débito o crédito.	9(10)v99
    'MIN-PAYMENT	Pago mínimo para cuenta de crédito.	9(10)v99
    'PAYMENT-DATE	Fecha de vencimiento de resumen para cuenta de crédito o fecha de saldo de apertura para cuenta de débito. El formato es AAMMDD.
    'Ahora se utiliza para informar la fecha de alta de la cuenta. Sólo 
    'para las transacciones de consulta y extracción	9(6)
    'INTEREST-RATE	Tasa  nominal anual por cash-advance para cuenta de crédito.Ahora se utiliza sólo par depósitos en efectivo	9(4)v99
    'OWE-DOLAR	Saldo último resumen en dólares para cuenta de crédito.
    'MIN-PAYMENT-DOLAR	Pago  mínimo en  dólares para cuenta de crédito. 	9(8)v99
    'PURCHASE-DOLAR	Compra  en  dólares  para  cuenta de crédito.	9(8)v99
    'CASH-FEE	Arancel por cash-advance para cuenta de crédito.	9(6)v99
  
    Public CantExtrac As Integer
    Public SaldoUltres As Decimal
    Public DisponibleAdelanto As Decimal
    Public PagoMinimo As Decimal
    Public FechaVto As Date
    Public TNA As Decimal
    Public tem As Decimal
    Public SaldoUltResDOlares As Decimal
    Public PagoMinimoDolares As Decimal
    Public COmpraDolares As Decimal
    Public CargoAdelanto As Decimal


    Public Sub New(ByVal p126 As Mensaje126Adelanto)
        MyBase.New(p126.Implementacion)
        from126(p126)
    End Sub

    Public Overrides Sub Fromstring(ByVal datos As String)
        Dim resultado As String = "OK"
        Dim s As String = ""
        Try
            Select Case Implementacion
                Case E_Implementaciones.Link
                    If s.Length >= 101 Then
                        s = datos.Replace(" ", "0")
                        CantExtrac = CInt(s.Substring(12, 2))
                        SaldoUltres = CDec(s.Substring(14, 12)) / 100
                        DisponibleAdelanto = CDec(s.Substring(26, 12)) / 100
                        PagoMinimo = CDec(s.Substring(38, 12)) / 100
                        FechaVto = DateSerial(2000 + CInt(s.Substring(54, 2)), CInt(s.Substring(52, 2)), CInt(s.Substring(50, 2)))
                        TNA = CDec(s.Substring(56, 6)) / 100
                        SaldoUltResDOlares = CDec(s.Substring(62, 10)) / 100
                        PagoMinimoDolares = CDec(s.Substring(72, 10)) / 100
                        COmpraDolares = CDec(s.Substring(82, 10)) / 100
                        CargoAdelanto = CDec(s.Substring(92, 8)) / 100
                    Else

                    End If
            End Select
        Catch ex As Exception
            resultado = ex.Message
        End Try

        LogMessageFromString("From String " + resultado + vbCrLf + Logstate(s))

    End Sub

    Public Overrides Function Tostring() As String
        Dim s As String = ""
        Select Case Implementacion

            Case E_Implementaciones.Link
                s = s + "            " + _
                  CantExtrac.ToString("00") + _
                  Monto2decaStr(SaldoUltres) + _
                  Monto2decaStr(DisponibleAdelanto) + _
                  Monto2decaStr(PagoMinimo) + _
                  FechaVto.ToString("yyMMdd") + _
                  (TNA * 100).ToString("000000") + _
                   (SaldoUltResDOlares * 100).ToString("0000000000") + _
                   (PagoMinimoDolares * 100).ToString("0000000000") + _
                   (COmpraDolares * 100).ToString("0000000000") + _
                  (CargoAdelanto * 100).ToString("00000000")
        End Select
                LogMessageToString(Logstate(s))

                Return s
    End Function


    Public Overrides Function Logstate(ByVal s As String) As String

        s = "54 VARIOS " + vbCrLf + _
             "Deuda Dolar:" + SaldoUltResDOlares.ToString + vbCrLf + _
              "Deuda Pesos  :" + SaldoUltres.ToString + vbCrLf + _
              "Pago Minimo  :" + PagoMinimoDolares.ToString + vbCrLf + _
              "Pago Pesos   :" + PagoMinimo.ToString + vbCrLf + _
              "Fecha Vto    :" + FechaVto.ToString + vbCrLf + _
              "Disp.Adel    :" + DisponibleAdelanto.ToString + vbCrLf + _
              "TNA  Adel    :" + TNA.ToString + vbCrLf + _
              "Cant Adel    :" + CantExtrac.ToString + vbCrLf
        Return s
     End Function

    Public Sub from126(ByVal p126 As Mensaje126Adelanto)
        With p126
            CantExtrac = .Consultasaldo.CantAdel
            SaldoUltres = .Consultasaldo.DEudaPesos
            DisponibleAdelanto = .Consultasaldo.DispAdelantos
            PagoMinimo = .Consultasaldo.PagoMinimo
            FechaVto = .Consultasaldo.FechaVto
            TNA = .tnaPesos
            tem = .TemPesos
            SaldoUltResDOlares = 0
            PagoMinimoDolares = 0
            COmpraDolares = 0
            CargoAdelanto = 0

        End With
    End Sub
    Public Function ToStringDescriptivo() As String
        Return "COMPRAS"
    End Function

End Class
