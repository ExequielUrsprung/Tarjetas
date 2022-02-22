Public Class Mensaje90DatosOriginales
    Inherits mensajeGenerico
    Public RetRefNumber37 As Integer
    Public Fecha13 As Date
    Public fechaHora12 As Date
    Public FEchaNegocio17 As Date
    Public Sub New(ByVal fromString As String)
        MyBase.New(E_Implementaciones.PosnetComercio, fromString)
    End Sub

    Public Overrides Sub Fromstring(ByVal Valor As String)
        Try
            RetRefNumber37 = CInt(Val(Valor.Substring(4, 12)))

            Dim s As String = Valor.Substring(16, 4)
            Dim Anio As Integer = Year(FechaActual)
            Dim _fechatran As Date = DateSerial(Anio, CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)))
            If _fechatran.Subtract(FechaActual).TotalDays > 100 Then
                _fechatran = DateSerial(Anio - 1, CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)))
            End If
            Fecha13 = _fechatran

            ' agregar la hora desde el campo 12
            s = Valor.Substring(20, 8)
            _fechatran.AddHours(CInt(Val(s.Substring(0, 2))))
            _fechatran.AddMinutes(CInt(Val(s.Substring(2, 2))))
            _fechatran.AddSeconds(CInt(Val(s.Substring(4, 2))))
            fechaHora12 = _fechatran

            s = Valor.Substring(28, 4)
            _fechatran = DateSerial(Anio, CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)))
            If _fechatran.Subtract(FechaActual).TotalDays > 100 Then
                _fechatran = DateSerial(Anio - 1, CInt(s.Substring(0, 2)), CInt(s.Substring(2, 2)))
            End If
            FEchaNegocio17 = _fechatran
        Catch ex As Exception
            Logger.Error("Error " + ex.Message + " Parseando campo 90:" + Valor)
        End Try
    End Sub

    Public Overloads Overrides Function Logstate(ByVal s As String) As String
        Return "90:" + Fecha13.ToString + " " + fechaHora12.ToString + " " + FEchaNegocio17.ToString + " " + RetRefNumber37.ToString
    End Function

    Public Overrides Function ToString() As String
        Return "0200" + RetRefNumber37.ToString("000000000000") + Fecha13.ToString("ddMM") + fechaHora12.ToString("hhmmss") + "00" + FEchaNegocio17.ToString("ddMM") + Space(10)
    End Function
End Class
