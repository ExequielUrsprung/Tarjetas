

Public Class RequerimientoTarjeta

    Sub New(tipoMsg As TipoRequerimientos)
        msg = New Mensaje(tipoMsg)
    End Sub

#Region "Shared"
    Shared _Canal As TransmisorPOS

    Public Shared Function ObtenerCanal() As TransmisorPOS
        If _Canal Is Nothing Then
            _Canal = New TransmisorPOS
        End If
        Return _Canal
    End Function

#End Region

#Region "Entrada"
    Public Parametros As SParametrosInput
    Private Resultados As SParametrosOutput

    Public msg As Mensaje

    Dim BF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
#End Region

    Private Function SerializarMensaje() As Byte()
        'Retrieves a BinaryFormatter object anda
        'instantiates a new Memorystream to which
        'associates the above BinaryFormatter, then writes
        'the content to a binary file via My namespace
        Dim MS As New System.IO.MemoryStream()
        'Serialization is first in memory,
        'then written to the binary file

        BF.Serialize(MS, msg)

        Return MS.GetBuffer()

    End Function


    Private Function DeserializarMensaje(Datos As Byte()) As Mensaje
        'Verifies that deserializing works fine and shows
        'the result in the Console window.
        Return DirectCast(BF.Deserialize(New System.IO.MemoryStream(Datos)), Mensaje)
    End Function
    Private Function SerializarInput() As Byte()
        'Retrieves a BinaryFormatter object and
        'instantiates a new Memorystream to which
        'associates the above BinaryFormatter, then writes
        'the content to a binary file via My namespace
        Dim MS As New System.IO.MemoryStream()

        'Serialization is first in memory,
        'then written to the binary file
        BF.Serialize(MS, Parametros)
        Return MS.GetBuffer()

    End Function

    Private Function DeserializarInput(Datos As Byte()) As SParametrosInput
        'Verifies that deserializing works fine and shows
        'the result in the Console window.
        Return DirectCast(BF.Deserialize(New System.IO.MemoryStream(Datos)), SParametrosInput)
    End Function

    Private Function DeserializarOutput(Datos As Byte()) As SParametrosOutput
        'Verifies that deserializing works fine and shows
        'the result in the Console window.
        Return DirectCast(BF.Deserialize(New System.IO.MemoryStream(Datos)), SParametrosOutput)
    End Function

#Region "Metodos"
    Public Const TIEMPOESPERA As Integer = 30
    Function Enviar() As Respuesta
        Dim resp As Respuesta
        Dim c As Integer = 0
        Try
            Using canal As New TransmisorPOS()
                canal.Enviar(SerializarInput)
                Dim hora As Date = Now
                Do
                    ' esperar resupuesta
                Loop Until Now.Subtract(hora).TotalSeconds > TIEMPOESPERA Or canal.Comprobar_Datos

                'TODO: ver si comprobar datos es true y datos es 0, es porque no recibio nada
                Resultados = (DeserializarOutput(canal.Recibir))
                resp = ObtenerRespuesta()
            End Using
            Return resp
        Catch ex3 As NullReferenceException
            'Resultados.mensaje = "Servidor Fuera de linea"
            'Resultados.Respuesta = Respuesta.SinServicio
            Return Respuesta.SinServicio
        Catch ex2 As Net.Sockets.SocketException
            'Resultados.mensaje = "Servidor Fuera de linea"
            Return Respuesta.SinServicio
            ' error de comunicacion

        Catch ex As Exception
            Return Respuesta.Rechazada
        End Try

    End Function



    Function EnviarSincronizacion() As Respuesta
        Dim resp As Respuesta
        Dim c As Integer = 0
        Try
            Using canal As New TransmisorPOS()
                canal.Enviar(SerializarMensaje)
                Dim hora As Date = Now
                Do
                    ' esperar resupuesta
                Loop Until Now.Subtract(hora).TotalSeconds > TIEMPOESPERA Or canal.Comprobar_Datos

                'TODO: ver si comprobar datos es true y datos es 0, es porque no recibio nada
                Resultados = (DeserializarOutput(canal.Recibir))
                resp = ObtenerRespuesta()
            End Using
            Return resp
        Catch ex3 As NullReferenceException
            'Resultados.mensaje = "Servidor Fuera de linea"
            'Resultados.Respuesta = Respuesta.SinServicio
            Return Respuesta.SinServicio
        Catch ex2 As Net.Sockets.SocketException
            'Resultados.mensaje = "Servidor Fuera de linea"
            Return Respuesta.SinServicio
            ' error de comunicacion

        Catch ex As Exception
            Return Respuesta.Rechazada
        End Try

    End Function
    Function ObtenerRespuesta() As Respuesta
        'Return Resultados.Respuesta
    End Function

    Function ObtenerTicket() As String()

        'Return Resultados.LineasTicket
    End Function


#End Region

#Region "SALIDA"
    Function MotivoRechazo() As String
        ' Return Resultados.mensaje
    End Function

    Function NroAutorizacion() As String
        ' Return Resultados.NroAutorizacion
    End Function

    Private Salida As SParametrosOutput
#End Region

    Function Mensaje() As String
        ' Return Resultados.mensaje

    End Function

End Class

'<Serializable()>
'Public Structure SParametrosInput
'    Dim Operacion As TipoRequerimientos
'    Dim ExpDate As String
'    Dim Importe As Decimal
'    Dim ModoIngreso As EModoIngreso
'    Dim Tarjeta As String
'    Dim PLAN As Integer
'    Dim CodigoSeguridad As Integer
'    Dim CajaID As String
'    Dim NroTicket As Integer
'    Dim Cajero As String
'    Dim FechaHoraTransaccion As Date
'    Dim Track1 As String
'    Dim Track2 As String
'    Dim Ult4Dig As String
'    Dim titular As String

'    Dim NroTarj1 As String
'    Dim NroTarj2 As String

'    ' Anulacion
'    Dim AnulacionNroTicket As Integer
'    Dim AnulacionCodAut As Integer
'    Dim AnulacionPlan As Integer

'End Structure

<Serializable()>
Public Structure SParametrosInput
    'Dim client As System.Net.Sockets.NetworkStream
    Dim VERSION As String          '* 1       ' NRO DE VERSION               
    Dim TARJ As String             '* 20      ' NRO DE TARJETA               
    Dim EXPDATE As String          '* 4       ' FECHA DE EXPIRACION          
    Dim IMPO As Decimal            'CURRENCY         ' IMPORTE DE LA TRANSACCION    
    'Dim MANUAL As Short            ' MODO DE INGRESO 0-MANUAL 1-AUTO 5-CHIP
    Dim MANUAL As EModoIngreso             ' MODO DE INGRESO 0-MANUAL 1-AUTO 5-CHIP
    Dim PLANINT As Short           ' COD.PLAN                     
    Dim CODSEG As String           '* 30      ' COD. SEGURIDAD            
    Dim TICCAJ As Integer          ' NRO DE TICKET DE LA CAJA     
    Dim CAJERA As Short            ' COD DE CAJERO (OPERADOR)     
    Dim HORA As Double             ' FECHA/HORA                   
    Dim TRACK2 As String           '* 37      '                              
    Dim TRACK1 As String           '* 77      '                              
    Dim CodAut As String           '* 6       ' Codigo de autorizacion pa/anu
    Dim TKTORI As Integer          '                              
    Dim FECORI As String           '* 6       '                              
    Dim PLANINTori As Short        ' COD.PLAN                     
    Dim oper As Short              ' !! operacion                 
    Dim cmd As Short               '+8 offline +16 anular                  
    Dim cajadir As String          '* 26      '                          
    Dim TKID As String             '* 4
    Dim CASHBACK As Decimal        '                          
    'Agregados EMV
    Dim CRIPTO As String           ' * 300 CRIPTOGRAMA TARJETAS CHIP
    Dim CARDSEQ As String          ' * 3 CARD SEQUENCE
    Dim APPVERSION As String       ' * 15  VERSION APLICACION
    Dim CHECK As String            '* 2      ' HACER = CHECKID         
End Structure




'<Serializable()>
'Public Structure SParametrosOutput
'    Dim NroAutorizacion As Integer
'    Dim CodRespuesta As Integer
'    Dim Emisor As String
'    Dim LineasTicket() As String
'    Dim Respuesta As Respuesta
'    Dim importe As Decimal
'    Dim mensaje As String
'End Structure

<Serializable()>
Public Structure SParametrosOutput
    'Declare data members
    Public VtaVersion As String
    Public VtaMensaje As String
    Public VtaMontop As String
    Public VtaMenResp As String
    Public VtaFileTkt As String
    Public VtaEmiName As String
    Public VtaOk As Integer
    Public VtaCashBack As Decimal
    Public VtaTicket As Long
    Public VtaCheck As String
End Structure

Public Enum EModoIngreso
    Manual
    Banda
    Chip
    Contactless
    Ecomerce
End Enum