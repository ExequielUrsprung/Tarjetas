Public Class Mensaje63POS
    Inherits mensajeGenerico
    Private MensajePuro As String
    Public Sub New(ByVal pvalorInicial As String)
        MyBase.New(E_Implementaciones.PosnetComercio, pvalorInicial)
    End Sub
    Public Overrides Sub Fromstring(ByVal datos As String)
        FromstringTokenizer(datos)
    End Sub
    Public Overrides Function ToString() As String
        Return ToStringTokenizer()
    End Function
    Public Sub CreateEmptyToken(ByVal TokenId As String)
        Select Case TokenId.ToUpper
            'Case "04" ' datos de validacion de POS (sin uso)
            '    AddToken(TokenId.ToUpper, 20)
            'Case "C0"' datos de validacion de las transaccion (sin uso)

            '   AddToken(TokenId.ToUpper, 26)
            'Case "C4" ' no usado x ahora
            '    AddToken(TokenId.ToUpper, 12)
            'Case "R2"
            '    AddToken(TokenId.ToUpper, 46)
            'Case "P3"
            '    AddToken(TokenId.ToUpper, 202)
            'Case "Q2"
            '    AddToken(TokenId.ToUpper, 40)
            Case "B0"
                AddToken(TokenId.ToUpper, 64)
            Case Else
                Logger.Warn("TOKEN DESCONOCIDO " + TokenId + " se ignora")
        End Select
    End Sub
    Public Overloads Overrides Function Logstate(ByVal s As String) As String
        s = "ESTADO DE TOKENS" + vbCrLf
        For i As Integer = 1 To NtokenList
            s += TokenList(i) + " " + GetToken(TokenList(i)) + vbCrLf
        Next
        Return s
    End Function
    ' q2
    Public Sub AgregarQ2(ByVal DispAdel As Decimal, ByVal DispCpr As Decimal, ByVal DispCprCtas As Decimal, ByVal DispPrestamos As Decimal, ByVal DispPrestFina As Decimal)
        Dim Tk As String = "Q2"
        AddToken(Tk, 40)
        TokenData(Tk, 0, 8) = Monto2decaStr8(DispAdel)
        TokenData(Tk, 8, 8) = Monto2decaStr8(DispCpr)
        TokenData(Tk, 16, 8) = Monto2decaStr8(DispCprCtas)
        TokenData(Tk, 24, 8) = Monto2decaStr8(DispPrestamos)
        TokenData(Tk, 32, 8) = Monto2decaStr8(DispPrestFina)
    End Sub

    Public Sub AgregarR2(ByVal Cuotas As Integer, ByVal Plan As String)
        Dim Tk As String = "R2"
        AddToken(Tk, 46)
        TokenData(Tk, 0, 2) = Cuotas.ToString("00")
        TokenData(Tk, 6, 1) = Plan.substring(0, 1)

    End Sub
    Public Sub AgregarC0(ByVal CVc2 As String)
        Dim Tk As String = "C0"
        AddToken(Tk, 26)
        TokenData(Tk, 0, 4) = CVc2
     End Sub
    ' r2 
    Public Function Cuotas() As Short
        Return CShort(TokenData("R2", 0, 2))
    End Function
    Public Function Plan() As String
        Return (TokenData("R2", 6, 1))
    End Function
    Public Function CVC2() As Integer
        Try
            Return CInt(Val(TokenData("C0", 0, 4)))
        Catch ex As Exception
            Logger.Error("error obteniendo el cvc2 desde el tocken c0 " + ex.Message + " " + ex.StackTrace)
            Return -1
        End Try
    End Function
    Public Function Factura() As String
        Return TokenData("R2", 9, 12)
    End Function
    Public Property Mensaje() As String
        Get
            If GetToken("P3") Is Nothing Then
                Return ""
            Else
                Dim LARGO As Integer = CInt(TokenData("P3", 0, 3))
                Return TokenData("P3", 3, LARGO).Substring(0, LARGO)
            End If
        End Get
        Set(ByVal value As String)
            Dim s As String = value
            MensajePuro = value
            s = s.Replace("!", "")
            

            If s.Length < 39 Or s.Contains("|") Then
                s = "." + s
            Else
                Dim s2 As String = ""
                s = s.Replace(vbCrLf, "|")
                s = s.Replace(vbCr, "|")
                s = s.Replace(vbLf, "|")

                s = s.Replace("^", "")
                s = s + "^"
                s = "," + s
                Try

                    ' cortar en pedacitos
                    Dim j As Integer = 0
                    For i As Integer = 1 To s.Length
                        j += 1
                        Dim u As String = s.Substring(i - 1, 1)
                        s2 = s2 + u
                        If u = "|" Then j = 1
                        If (j Mod 20) >= 16 And u = " " Then
                            j = 0
                            s2 = s2 + "|"
                        End If
                        If j Mod 20 = 19 Then
                            s2 = s2 + "|"
                            j = 0
                        End If
                    Next
                    s = s2
                Catch ex As Exception

                End Try

                ' Log.logdebug("mensaje parseado: " + s2)

            End If
            If GetTokenInx("P3") = 0 Then AddToken("P3", s.Length + 3)
            TokenData("P3", 0, s.Length + 3) = s.Length.ToString("000") + s
            Logger.Info("MENSAJE al usuario >" + s + "<")
        End Set
    End Property

    Public Sub AgregarAlMensaje(ByVal s As String)
        Mensaje = MensajePuro + "|" + s
    End Sub
End Class
