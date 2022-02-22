
Imports System.Security
Imports System.Security.Cryptography

Public Module Serializador

    Public versionTRANSMISORTCP As String = "1"

    Dim BF As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
    Public Function SerializarMensaje(msg As MensajeIda) As Byte()
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
    Public Function SerializarMensaje(msg As MensajeRespuesta) As Byte()
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


    Public Function DeserializarMensajeRespuesta(Datos As Byte()) As MensajeRespuesta
        'Verifies that deserializing works fine and shows
        'the result in the Console window.
        Return DirectCast(BF.Deserialize(New System.IO.MemoryStream(Datos)), MensajeRespuesta)
    End Function

    Public Function DeserializarMensajeIda(Datos As Byte()) As MensajeIda
        'Verifies that deserializing works fine and shows
        'the result in the Console window.
        Return DirectCast(BF.Deserialize(New System.IO.MemoryStream(Datos)), MensajeIda)
    End Function



    Public Function Convertir_ASCII(texto As String) As Byte()
        Dim NumberChars As Integer = texto.Length
        Dim bytes((NumberChars / 2) - 1) As Byte
        Dim i As Integer = 0
        Try
            Do While i < NumberChars
                bytes(i / 2) = Convert.ToByte(Convert.ToInt32(texto.Substring(i, 2), 16))
                i += 2
            Loop
        Catch ex As Exception
        End Try
        Return bytes

    End Function

    ''' <summary>
    ''' Procedimiento para convertir array de byte a string. (Pin encriptado)
    ''' </summary>
    ''' <param name="ba"></param>
    ''' <returns></returns>
    Public Function ByteArrayToString(ba As Byte()) As String
        Dim hex As New System.Text.StringBuilder(ba.Length * 2)
        For Each b As Byte In ba
            hex.AppendFormat("{0:x2}", b)
        Next
        Return hex.ToString().ToUpper
    End Function

    Public Function DES_Encriptor(strEncript As String, pKey As String) As String

        Dim des As New DESCryptoServiceProvider
        Dim paquete As String
        Try
            des.BlockSize = 64
            des.Key = Convertir_ASCII(pKey)
            'des.GenerateIV()
            Dim objCrypto As ICryptoTransform = des.CreateEncryptor()
            des.Mode = CipherMode.ECB
            des.Padding = PaddingMode.Zeros

            Dim dato = objCrypto.TransformFinalBlock(System.Text.Encoding.ASCII.GetBytes(strEncript), 0, System.Text.Encoding.ASCII.GetBytes(strEncript).Length)
            paquete = ByteArrayToString(dato)
            Debug.WriteLine(ByteArrayToString(dato))
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try

        Return paquete
    End Function
End Module
