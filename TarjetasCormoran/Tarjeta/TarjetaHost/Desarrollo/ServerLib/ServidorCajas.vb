Imports IsoLibCaja


' escuchador de conexiones

Public Class ServidorCajas
    Inherits ServerTCP

    Public Sub New(ByVal PortNumber As Integer)
        MyBase.New(PortNumber)
    End Sub

    Public Parametros As SParametrosInput
    Private Resultados As SParametrosOutput
    Dim BF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()

    Public Event RequerimientoCajaRecibido(ParametrosRecibidos As SParametrosInput, ByRef Respuesta As SParametrosOutput)
    Public Event RequerimientoEnviado(ByVal Respuesta As SParametrosOutput)

    Private Function SerializarOutput(ByVal parametros As SParametrosOutput) As Byte()
        Dim MS As New System.IO.MemoryStream()
        BF.Serialize(MS, parametros)
        Return MS.GetBuffer()
    End Function

    Private Function DeserializarInput(Datos As Byte()) As SParametrosInput
        Return DirectCast(BF.Deserialize(New System.IO.MemoryStream(Datos)), SParametrosInput)
    End Function

    Private Function DeserializarOutput(Datos As Byte()) As SParametrosOutput
        'Verifies that deserializing works fine and shows
        'the result in the Console window.
        Return DirectCast(BF.Deserialize(New System.IO.MemoryStream(Datos)), SParametrosOutput)
    End Function


    Public Overrides Sub DataReceived(ByVal Datos() As Byte, ByVal clientStream As System.Net.Sockets.NetworkStream)
        'aca es donde recibe el requerimiento y se lo pasa al servidor para procesar y mandar al host.
        Dim ParametrosRecibidos As SParametrosInput = DeserializarInput(Datos)
        ' procesar la resupuesta
        Dim Respuesta As New SParametrosOutput

        RaiseEvent RequerimientoCajaRecibido(ParametrosRecibidos, Respuesta)
        ' enviar(clientStream, SerializarOutput(Respuesta))
        'RaiseEvent RequerimientoEnviado(Respuesta)
    End Sub

End Class
