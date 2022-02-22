Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Public Class Encriptador
    Public Function clave_encriptada(nombreArchivo As String, alg As String) As String
        Dim hash As HashAlgorithm = GetHashProvider(alg)
        Dim fs As FileStream
        Try
            fs = New FileStream(nombreArchivo, FileMode.Open, FileAccess.Read)
            Return ArrayToString(hash.ComputeHash(fs))
        Catch ex As Exception
            MsgBox("No se pudo leer hash.", MsgBoxStyle.Critical)
            Return ""
        Finally
            fs.Close()
        End Try
    End Function

    Private Enum Algoritmo
        MD5
        SHA1
        SHA256
        SHA384
        SHA512
    End Enum
    Private Function GetHashProvider(alg As String) As HashAlgorithm
        Dim tipo As Algoritmo = [Enum].Parse(GetType(Algoritmo), alg)
        Select Case tipo
            Case Algoritmo.MD5
                Return New MD5CryptoServiceProvider()
            Case Algoritmo.SHA1
                Return New SHA1Managed()
            Case Algoritmo.SHA256
                Return New SHA256Managed()
            Case Algoritmo.SHA384
                Return New SHA384Managed()
            Case Algoritmo.SHA512
                Return New SHA512Managed()
            Case Else
                Throw New Exception("Invalid Provider.")
        End Select
    End Function


    Private Function ArrayToString(ByVal byteArray As Byte()) As String
        Dim sb As StringBuilder = New StringBuilder(byteArray.Length)

        For i = 0 To byteArray.Length - 1
            sb.Append(byteArray(i).ToString("X2"))
        Next
        Return sb.ToString()
    End Function

    Public Sub GrabarEncriptado(ByVal firma As String, ByVal nombreArchivoSalida As String)
        Try
            My.Computer.FileSystem.WriteAllBytes(nombreArchivoSalida, EncriptarStr(firma), False)
        Catch ex As Exception
            MsgBox("No se pudo encriptar hash", MsgBoxStyle.Critical)
        End Try
    End Sub

    Public Function LeerEncriptado(ByVal nombreArchivoEntrada As String) As String
        Try
            Return DesencriptarStr(My.Computer.FileSystem.ReadAllBytes(nombreArchivoEntrada))

        Catch ex As Exception
            MsgBox("No se pudo leer hash encriptado.", MsgBoxStyle.Critical)
            Return "ERROR"
        End Try
    End Function

    'Const clave As String = "MATEPAVABOMBILLA"
    Public Function EncriptarStr(texto As String) As Byte()
        Return crypto.strEncryptNEwB(System.Text.UTF8Encoding.UTF8.GetBytes(texto), System.Text.UTF8Encoding.UTF8.GetBytes(clave))
    End Function

    Public Function DesencriptarStr(texto As Byte()) As String
        Return System.Text.UTF8Encoding.UTF8.GetString(Crypto.strDecryptNewB(texto, System.Text.UTF8Encoding.UTF8.GetBytes(clave)))
    End Function

    Private Function clave() As String
        Dim fs As FileStream
        Dim sr As StreamReader

        Try
            fs = New FileStream("C:\windows\ABCDEF123456789", FileMode.Open, FileAccess.Read)
            sr = New StreamReader(fs)

            Return sr.ReadLine
        Catch ex As Exception
            MsgBox("No se puede leer clave.", MsgBoxStyle.Critical)
            Return ""
        End Try

    End Function


End Class
