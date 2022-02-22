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
