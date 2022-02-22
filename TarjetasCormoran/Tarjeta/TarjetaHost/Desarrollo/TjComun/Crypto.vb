Imports vx.Crypt
Imports System.Runtime.InteropServices
Imports System.Text

Public Class crypto
    Public Shared Function hexView(ByVal s As Byte()) As String
        Dim s2 As String = ""
        For i As Integer = 0 To s.Length - 1
            If i Mod 16 = 0 Then
                If s2.Length > 0 Then s2 += vbCrLf
                s2 += i.ToString("x4") + ": "
            End If
            If i Mod 16 = 8 Then
                s2 += " "
            End If
            s2 += " " + s(i).ToString("x2")
        Next
        Return s2
    End Function


    Private Function HexStringToByte(texto As String) As Byte()
        Dim NumberChars As Integer = texto.Length
        Dim temp((NumberChars / 2) - 1) As Byte

        Dim i As Integer = 0

        Do While i < NumberChars
            temp(i / 2) = Convert.ToByte(Convert.ToInt32(texto.Substring(i, 2), 16))
            i += 2
        Loop
        Return temp

    End Function

    Public Function AES_Encrypt(ByVal input As String, ByVal pass As String) As String
        Dim AES As New System.Security.Cryptography.RijndaelManaged
        Dim encrypted As Byte()
        Try

            AES.Key = HexStringToByte(pass)
            AES.Padding = Security.Cryptography.PaddingMode.PKCS7
            AES.Mode = Security.Cryptography.CipherMode.CBC
            Dim DESEncrypter As System.Security.Cryptography.ICryptoTransform = AES.CreateEncryptor
            Dim Buffer As Byte() = System.Text.UTF8Encoding.UTF8.GetBytes(input)

            encrypted = DESEncrypter.TransformFinalBlock(Buffer, 0, Buffer.Length)
            Dim finalBuffer(AES.IV.Length + encrypted.Length - 1) As Byte


            Array.Copy(AES.IV, finalBuffer, AES.IV.Length)
            Array.Copy(encrypted, 0, finalBuffer, AES.IV.Length, encrypted.Length)

            Debug.WriteLine(Convert.ToBase64String(finalBuffer))
            Return Convert.ToBase64String(finalBuffer)
        Catch ex As Exception
            Return ""

        End Try
    End Function


    Public Function AES_Decrypt(ByVal input As String, ByVal pass As String) As String
        Dim AES As New System.Security.Cryptography.RijndaelManaged
        Dim Hash_AES As New System.Security.Cryptography.MD5CryptoServiceProvider
        Dim decrypted As String = ""
        Try

            'Dim hash(31) As Byte
            'Dim temp As Byte() = Hash_AES.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(pass))
            'Array.Copy(temp, 0, hash, 0, 16)
            'Array.Copy(temp, 0, hash, 15, 16)
            AES.Key = HexStringToByte(pass)
            AES.IV = HexStringToByte("26744a68b53dd87bb395584c00f7290a")
            AES.Mode = Security.Cryptography.CipherMode.CBC 
            Dim DESDecrypter As System.Security.Cryptography.ICryptoTransform = AES.CreateDecryptor
            Dim Buffer As Byte() = Convert.FromBase64String(input)
            decrypted = System.Text.ASCIIEncoding.ASCII.GetString(DESDecrypter.TransformFinalBlock(Buffer, 0, Buffer.Length))
            Debug.WriteLine(decrypted)


            Return decrypted
        Catch ex As Exception
        End Try
    End Function





    Public Shared Function strEncryptNEwB(ByVal strMsg As Byte(), ByVal pKey As Byte()) As Byte()
        Dim blockSize As Rijndael.BlockSize = Rijndael.BlockSize.Block256
        Dim KeySize As Rijndael.KeySize = Rijndael.KeySize.Key256
        Dim CryptMode As Rijndael.EncryptionMode = Rijndael.EncryptionMode.ModeECB
        Dim Ivtext(1532) As Byte
        Dim s As Byte()
        s = Rijndael.EncryptData(strMsg, pKey, Ivtext, blockSize, KeySize, CryptMode)
        Return s
    End Function

    Public Shared Function strEncrypt(ByVal strMsg As String, ByVal pKey As String) As String
        Dim ByteArray() As Byte
        Dim byteKey() As Byte
        Dim CryptText() As Byte
        ByteArray = System.Text.UTF8Encoding.UTF8.GetBytes(StrConv(strMsg, VbStrConv.None))
        byteKey = System.Text.UTF8Encoding.UTF8.GetBytes(StrConv(pKey, VbStrConv.None))
        CryptText = strEncryptNEwB(ByteArray, byteKey)
        Return StrConv(System.Text.UTF8Encoding.UTF8.GetString(CryptText), VbStrConv.None)
    End Function

    Public Shared Function strDecryptNewB(ByVal strMsg As Byte(), ByVal pKey As Byte()) As Byte()
        Dim blockSize As Rijndael.BlockSize = Rijndael.BlockSize.Block256
        Dim KeySize As Rijndael.KeySize = Rijndael.KeySize.Key256
        Dim CryptMode As Rijndael.EncryptionMode = Rijndael.EncryptionMode.ModeECB
        Dim Ivtext(1532) As Byte
        Return Rijndael.DecryptData(strMsg, pKey, Ivtext, blockSize, KeySize, CryptMode)
    End Function

    Public Shared Function strDecrypt(ByVal strMsg As String, ByVal pKey As String) As String
        Dim ByteArray() As Byte
        Dim byteKey() As Byte
        Dim CryptText() As Byte
        ByteArray = System.Text.UTF8Encoding.UTF8.GetBytes(StrConv(strMsg, VbStrConv.None))
        byteKey = System.Text.UTF8Encoding.UTF8.GetBytes(StrConv(pKey, VbStrConv.None))
        CryptText = strDecryptNewB(ByteArray, byteKey)
        Return StrConv(System.Text.UTF8Encoding.UTF8.GetString(CryptText), VbStrConv.None)
    End Function

End Class
