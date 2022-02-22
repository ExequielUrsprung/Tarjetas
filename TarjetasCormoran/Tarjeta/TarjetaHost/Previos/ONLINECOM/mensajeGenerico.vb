Public MustInherit Class mensajeGenerico
    Public Implementacion As E_Implementaciones
    Public positivos As String = "{ABCDEFGHI"
    Public negativos As String = "}JKLMNOPQR"
    Public NtokenList As Integer
    Public TokenList() As String
    Private TkData() As String
    Private TkLen() As Integer

    Public Sub New(ByVal pImplementacion As E_Implementaciones, Optional ByVal FromStringInicial As String = "")
        Try

            Implementacion = pImplementacion ' POR AHORA SOLO LINK
            If FromStringInicial <> "" Then Fromstring(FromStringInicial)
        Catch ex As Exception
            Logger.Error("fallo en New de mensaje generico " + FromStringInicial + ex.Message + " " + ex.StackTrace)
        End Try

    End Sub
    Public Sub LogMessageToString(ByVal s As String)
        With Logger()
            s += " Implementacion " + Implementacion.ToString
            .Debug(s)
        End With
    End Sub
    Public Sub LogMessageFromString(ByVal s As String)
        Try
            With Logger()
                s += "  Implementacion " + Implementacion.ToString
                .Debug(s)
            End With

        Catch ex As Exception

        End Try
    End Sub
    Public Function Monto2decaStr(ByVal monto As Decimal, Optional ByVal formatearpositivos As Boolean = False) As String
        Dim s As String = Math.Abs(monto * 100).ToString("000000000000")
        If monto < 0 Then
            s = "-" + s.Substring(1)
        Else
            'If formatearpositivos Then
            '    Dim u As String = s.Substring((s.Length - 1), 1)
            '    u = positivos.Substring(CInt(u), 1)
            '    s = s.Remove(s.Length - 1, 1) & u
            'End If
        End If
        Return s
    End Function
    Public Function Monto2decaStr8(ByVal monto As Decimal, Optional ByVal formatearpositivos As Boolean = False) As String
        Dim s As String = Math.Abs(monto * 100).ToString("00000000")
        If monto < 0 Then
            s = "-" + s.Substring(1)
        End If
        Return s
    End Function
    Public MustOverride Function Logstate(ByVal s As String) As String
    Public Function LogState() As String
        Return LogState(ToString)
    End Function
    Public MustOverride Sub Fromstring(ByVal s As String)
    Public Sub ClearTokenList()
        NTokenList = 0
    End Sub

    Public Sub FromstringTokenizer(ByVal datos As String)
        If datos = "" Then Exit Sub
        ' procesar los tokens
        Try

            Dim CantSeg As Integer
            Dim Ltoken As Integer
            NtokenList = 0
            CantSeg = CInt(datos.Substring(2, 5)) - 1
            Dim PorProcesar As String = datos.Substring(12)
            Do While PorProcesar <> ""
                Dim HeaderSegmento As String = PorProcesar.Substring(2, 2)
                Ltoken = CInt(Val(PorProcesar.Substring(4, 5)))

                If Ltoken > PorProcesar.Length Then
                    Logger.Error("Error REsto de token con largo insuficiente:" + Ltoken.ToString + " > " + PorProcesar)
                    PorProcesar = ""
                Else
                    Ltoken = CInt(Val(PorProcesar.Substring(4, 5)))
                    PorProcesar = PorProcesar.Substring(10)
                    Dim TkdataTemp As String = PorProcesar.Substring(0, Ltoken)
                    ProcessToken(HeaderSegmento, TkdataTemp)
                    AddToken(HeaderSegmento, TkdataTemp)
                    PorProcesar = PorProcesar.Substring(Ltoken)
                End If
            Loop
        Catch ex As Exception
            Logger.Error("error : " + ex.Message + "FromStringTokenizer:" + datos + vbCrLf + ex.StackTrace)
        End Try

    End Sub

    Public Overridable Function ProcessToken(ByVal TokenId As String, ByVal Datos As String) As Boolean

    End Function

    Public Function GetToken(ByVal IdToken As String) As String

        Return TkData(GetTokenInx(IdToken))

    End Function
    Public Property TokenData(ByVal idToken As String, ByVal StartPosition As Integer, ByVal largo As Integer) As String

        Get
            Try

                Return GetToken(idToken).Substring(StartPosition, largo)
            Catch ex As Exception
                Return ""
            End Try
        End Get
        Set(ByVal value As String)
            Try

                Dim i As Integer = GetTokenInx(idToken)
                If i > 0 Then
                    If StartPosition + value.Length > TkData(i).Length Then
                        TkData(i) = TkData(i).Substring(0, StartPosition) + value
                    Else
                        TkData(i) = TkData(i).Substring(0, StartPosition) + value + TkData(i).Substring(StartPosition + value.Length)
                    End If

                End If

            Catch ex As Exception

            End Try
        End Set
    End Property
    Public Function SetTokenData(ByVal idToken As String, ByVal StartPosition As Integer, ByVal Valor As String) As Boolean
        Try
          
            Dim I As Integer = GetTokenInx(idToken)
            If I > 0 Then
                TkData(I) = TkData(I).Substring(0, StartPosition) + Valor + TkData(I).Substring(StartPosition + Valor.Length)
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Function TokenDatastr(ByVal IdToken As String) As String
        Dim S As String = GetToken(IdToken)
        Return "! " + IdToken + S.Length.ToString("00000") + " " + S
    End Function
    Public Function ToStringTokenizer() As String
        Dim s As String = ""

        For i As Integer = 1 To NtokenList
            s = s + TokenDatastr(TokenList(i))
        Next
        Dim Ltot As Integer = s.Length + 12
        s = "& " + (NtokenList + 1).ToString("00000") + Ltot.ToString("00000") + s
        Return s
    End Function
    Public Sub AddToken(ByVal idToken As String, ByVal largo As Integer)
        NtokenList += 1
        ReDim Preserve TokenList(NtokenList)
        ReDim Preserve TkData(NtokenList)
        ReDim Preserve TkLen(NtokenList)
        TokenList(NtokenList) = idToken
        TkData(NtokenList) = StrDup(largo, "0"c)
        TkLen(NtokenList) = largo
    End Sub
    Public Function GetTokenInx(ByVal IdToken As String) As Integer
        For i As Integer = 1 To NtokenList
            If TokenList(i).ToUpper = IdToken Then
                Return i
            End If
        Next
        Return 0
    End Function
    Public Sub AddToken(ByVal idToken As String, ByVal valor As String)
        Dim Np As Integer = GetTokenInx(idToken)
        If Np = 0 Then
            NtokenList += 1
            ReDim Preserve TokenList(NtokenList)
            ReDim Preserve TkData(NtokenList)
            ReDim Preserve TkLen(NtokenList)
            Np = NtokenList
        End If
        TokenList(Np) = idToken
        TkData(Np) = valor
        TkLen(Np) = valor.Length

    End Sub
    Public Sub removeToken(ByVal idToken As String)
        Try
            Dim i As Integer = GetTokenInx(idToken)
            If i <= NtokenList Then
                For j As Integer = i + 1 To NtokenList
                    TokenList(j - 1) = TokenList(j)
                    TkData(j - 1) = TkData(j)
                Next
                NtokenList = NtokenList - 1
            End If
        Catch ex As Exception
            Logger.Error("error " + ex.Message + " removiendo token " + idToken + " " + ex.StackTrace)
        End Try

    End Sub
    Public MustOverride Overrides Function ToString() As String
    Public Function Largo() As Integer
        Return ToString.Length
    End Function
End Class
