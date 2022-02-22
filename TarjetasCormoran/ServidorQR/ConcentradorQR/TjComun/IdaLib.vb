Public Module IdaLib

#Region "CONSTANTES"
    Const clave As String = "MATEPAVABOMBILLA"
#End Region

#Region "Declaraciones"
    'TODO: sacar despues de implementado


    Public versionTJCOMUN As String = "2"

    Public Structure idaQR
        Dim IMPORTE As String            '* CON .DECIMAL
        Dim CAJERO As String              '* 2 cod de respuesta
        Dim TIPOOPERACION As String      '* 6 cod autorizacion
        Dim ticket As String

    End Structure

    Public Structure VtaQR
        'Declare data members
        Public VtaOk As Integer
        Public VtaExtReference As String
        Public VtaIdOperacion As String
        Public VtaIDPago As String
        Public VtaIdUsuario As String
        Public VtaMail As String
        Public VtaMensaje As String
    End Structure


#End Region



#Region "Lectura Ida"

    Public Function LeerQR(ByVal idaqr As String) As idaQR
        Return decodificarIdaQR(New System.IO.FileStream(idaqr, IO.FileMode.Open))
    End Function




    Public Function decodificarIdaQR(ByVal StreamEntrada As System.IO.Stream) As idaQR
        Dim streamFile1 As New System.IO.StreamReader(StreamEntrada)
        Dim i As New idaQR
        Dim renglon = streamFile1.ReadLine.Split("|")

        '--- ACA SEPARA LOS CAMPOS QUE ESTAN DELIMITADOS POR @ Y EL MENSAJE QUE ESTA SEPARADO POR PIPELINE, QUE ESTÁ EN RENGLON
        i.CAJERO = renglon(0)
        i.IMPORTE = renglon(1)
        i.TIPOOPERACION = renglon(2)
        i.ticket = renglon(3)

        streamFile1.Close()
        Return i
    End Function



#End Region


#Region "Escritura Ida"

    Public Sub GrabarQR(ByVal Ida As idaQR, ByVal idaarc As String)
        codificarqr(Ida, New System.IO.FileStream(idaarc, IO.FileMode.Create))
    End Sub

    Public Sub codificarqr(ByVal i As idaQR, ByVal StreamSalida As System.IO.Stream, Optional ByVal Cerrar As Boolean = True)
        Dim streamFile1 As New System.IO.StreamWriter(StreamSalida)

        streamFile1.WriteLine(i.CAJERO & "|" & i.IMPORTE & "|" & i.TIPOOPERACION & "|" & i.ticket)


        If Cerrar Then streamFile1.Close()

    End Sub


#End Region


End Module
