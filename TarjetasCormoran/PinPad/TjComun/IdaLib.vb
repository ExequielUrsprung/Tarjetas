Public Module IdaLib

#Region "CONSTANTES"
    Const clave As String = "MATEPAVABOMBILLA"
#End Region

#Region "Declaraciones"
    'TODO: sacar despues de implementado
    Dim EMV As Boolean = True

    Public versionTJCOMUN As String = "5"
    Public Enum TipoHost
        POSNET = 1
        VISA = 2
        Comfiar = 3
        Diners = 4
        Posnet_homolog = 5
        Visa_homolog = 6
    End Enum

    Public Structure Advice
        Dim cripto As String            '* 300 criptograma
        Dim resp As String              '* 2 cod de respuesta
        Dim autorizacion As String      '* 6 cod autorizacion

    End Structure




    Public Structure IdaTypeInternal
        Dim VERSION As String          '* 1       ' NRO DE VERSION               
        Dim TARJ As String             '* 20      ' NRO DE TARJETA               
        Dim EXPDATE As String          '* 4       ' FECHA DE EXPIRACION          
        Dim IMPO As Decimal            'CURRENCY         ' IMPORTE DE LA TRANSACCION    
        'Dim MANUAL As EModoIngreso             ' MODO DE INGRESO 0-MANUAL 1-AUTO 5-CHIP
        Dim MANUAL As Short            ' MODO DE INGRESO 0-MANUAL 1-AUTO 5-CHIP
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
        Dim CHECK As String            '* 2      ' HACER = CHECKID     

        'Agregados EMV
        Dim CRIPTO As String           ' * 300 CRIPTOGRAMA TARJETAS CHIP
        Dim CARDSEQ As String          ' * 3 CARD SEQUENCE
        'Dim PAQUETE As String          ' * 120 PAQUETE ENCRIPTADO
        'Dim TIPOENC As String          ' * 1 TIPO DE ENCRIPCION
        'Dim SERIEF As String           ' * 12 SERIE FISICO
        Dim SERIEL As String           ' * 16 SERIE LOGICO
        Dim NOMBRE As String           ' * 45 NOMBRE TARJETA HABIENTE
        Dim ServCode As String         ' * 3 Service code
        'Dim APPVERSION As String       ' * 5  VERSION APLICACION
        'Dim AID As String              ' * 16 ID APLICACION
        'Dim APN As String              ' * 16 NOMBRE APLICACION

    End Structure
    Public Const CHECKID = "O2"



    Public Structure VtaType
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
        Public VtaAutorizacion As String
        Public VtaCheck As String
    End Structure

#End Region

#Region "Encriptacion texto"
    Public Function EncriptarStr(texto As String) As Byte()
        Return crypto.strEncryptNEwB(System.Text.UTF8Encoding.UTF8.GetBytes(texto), System.Text.UTF8Encoding.UTF8.GetBytes(clave))
    End Function

    Public Function DesencriptarStr(texto As Byte()) As String
        Return System.Text.UTF8Encoding.UTF8.GetString(crypto.strDecryptNewB(texto, System.Text.UTF8Encoding.UTF8.GetBytes(clave)))
    End Function

    Public Function enmascarar(cadena As String, startpos As Integer, endpos As Integer) As String
        Try
            If startpos > endpos Then endpos = startpos
            Dim asteriscos As String
            asteriscos = New String("*", endpos - startpos)
            Return cadena.Substring(0, startpos) + asteriscos + cadena.Substring(endpos)
        Catch ex As Exception
            Return "<no se puede mostrar>"
        End Try
    End Function
#End Region

#Region "Lectura Ida"

    Public Function LeerIda(ByVal idaarc As String) As IdaTypeInternal
        Return decodificarIda(New System.IO.FileStream(idaarc, IO.FileMode.Open))
    End Function

    Public Function LeerIdz(ByVal IdzArc As String) As IdaTypeInternal
        Dim iz1() As Byte = My.Computer.FileSystem.ReadAllBytes(IdzArc)
        Dim claveB As Byte() = System.Text.UTF8Encoding.UTF8.GetBytes(clave)
        Dim iz() As Byte = crypto.strDecryptNewB(iz1, claveB)
        Return IdaLib.LeerIdaDesdeBytes(iz)
    End Function

    Public Function LeerADV(ByVal AdvArc As String) As Advice
        Dim ad1() As Byte = My.Computer.FileSystem.ReadAllBytes(AdvArc)
        Dim claveB As Byte() = System.Text.UTF8Encoding.UTF8.GetBytes(clave)
        Dim ad() As Byte = crypto.strDecryptNewB(ad1, claveB)
        Return IdaLib.LeerAdvDesdeBytes(ad)
    End Function

    Public Function LeerIdaDesdeStr(ByVal idaData As String) As IdaTypeInternal
        Dim m As New System.IO.MemoryStream(System.Text.UTF8Encoding.Default.GetBytes(idaData))
        Return decodificarIda(m)
    End Function

    Public Function LeerIdaDesdeBytes(ByVal idaData As Byte()) As IdaTypeInternal
        Dim m As New System.IO.MemoryStream(idaData)
        Return decodificarIda(m)
    End Function

    Public Function LeerAdvDesdeBytes(ByVal advData As Byte()) As Advice
        Dim m As New System.IO.MemoryStream(advData)
        Return decodificarADV(m)
    End Function
    Public Function IdaaStr(ByVal idaData As IdaTypeInternal) As String
        Dim m As New System.IO.MemoryStream()
        codificarIda(idaData, m)
        Dim reader As New System.IO.StreamReader(m)
        Return reader.ReadToEnd()
    End Function

    Public Function IdaaByte(ByVal idaData As IdaTypeInternal) As Byte()
        Dim m As New System.IO.MemoryStream()
        codificarIda(idaData, m, False)
        m.Position = 0
        Dim b2 As New System.IO.BinaryReader(m, System.Text.UTF8Encoding.UTF8)
        Return b2.ReadBytes(m.Length)
    End Function

    Public Function ADVaByte(ByVal advData As Advice) As Byte()
        Dim m As New System.IO.MemoryStream()
        codificarADV(advData, m, False)
        m.Position = 0
        Dim b2 As New System.IO.BinaryReader(m, System.Text.UTF8Encoding.UTF8)
        Return b2.ReadBytes(m.Length)
    End Function

    Public Function decodificarADV(ByVal StreamEntrada As System.IO.Stream) As Advice
        Dim streamFile1 As New System.IO.BinaryReader(StreamEntrada)
        Dim i As New Advice
        With streamFile1
            i.resp = .ReadChars(2)
            i.autorizacion = .ReadChars(6)
            i.cripto = .ReadChars(300)
        End With
        streamFile1.Close()
        Return i
    End Function

    Public Function decodificarIda(ByVal StreamEntrada As System.IO.Stream) As IdaTypeInternal
        Dim streamFile1 As New System.IO.BinaryReader(StreamEntrada)
        Dim i As New IdaTypeInternal
        With streamFile1
            i.VERSION = .ReadChars(1)
            i.TARJ = .ReadChars(20)
            i.EXPDATE = .ReadChars(4)
            i.IMPO = Decimal.FromOACurrency(.ReadInt64)
            i.MANUAL = .ReadInt16
            i.PLANINT = .ReadInt16
            i.CODSEG = .ReadChars(30)
            i.TICCAJ = .ReadInt32
            i.CAJERA = .ReadInt16
            i.HORA = .ReadDouble
            i.TRACK2 = .ReadChars(37)
            i.TRACK1 = .ReadChars(77)
            'i.TRACK1 = i.TRACK1.Trim
            i.CodAut = .ReadChars(6)
            i.TKTORI = .ReadInt32
            i.FECORI = .ReadChars(6)
            i.PLANINTori = .ReadInt16
            i.oper = .ReadInt16
            i.cmd = .ReadInt16
            i.cajadir = .ReadChars(26)
            i.TKID = .ReadChars(4)
            i.CASHBACK = Decimal.FromOACurrency(.ReadInt64)
            i.CHECK = .ReadChars(2)
            If EMV Then
                'TODO: esto es para chip, ver
                i.CRIPTO = .ReadChars(300)
                i.CARDSEQ = .ReadChars(3)
                'i.PAQUETE = .ReadChars(120)          ' * 120 PAQUETE ENCRIPTADO
                'i.TIPOENC = .ReadChars(1)          ' * 1 TIPO DE ENCRIPCION
                'i.SERIEF = .ReadChars(12)           ' * 12 SERIE FISICO
                i.SERIEL = .ReadChars(16)            ' * 16 SERIE LOGICO
                i.NOMBRE = .ReadChars(45)           ' * 45 NOMBRE TARJETA HABIENTE
                i.ServCode = .ReadChars(3)
                'i.APPVERSION = .ReadChars(15)
                'i.AID = .ReadChars(16)              ' * 16 ID APLICACION
                'i.APN = .ReadChars(16)             ' * 16 NOMBRE APLICACION
            End If

        End With
        streamFile1.Close()
        Return i
    End Function

#End Region

#Region "Tests"
    Public Sub TestIdaToIdz()
        Dim idb As Byte() = My.Computer.FileSystem.ReadAllBytes("caja0006.ida")
        Dim claveB As Byte() = System.Text.UTF8Encoding.UTF8.GetBytes(clave)

        Dim iz As Byte() = crypto.strEncryptNEwB(idb, claveB)
        My.Computer.FileSystem.WriteAllBytes("caja00x6.idz", iz, False)

    End Sub

    Public Sub testOBtenerIDZ()
        Dim iz1() As Byte = My.Computer.FileSystem.ReadAllBytes("caja0006.idz")
        Dim claveB As Byte() = System.Text.UTF8Encoding.UTF8.GetBytes(clave)
        Dim iz() As Byte = crypto.strDecryptNewB(iz1, claveB)
        Dim i1 As IdaLib.IdaTypeInternal = IdaLib.LeerIdaDesdeBytes(iz)
        IdaLib.verida(i1)

    End Sub

#End Region

#Region "Escritura Ida"
    Public Sub GrabarIDa(ByVal Ida As IdaTypeInternal, ByVal idaarc As String)
        codificarIda(Ida, New System.IO.FileStream(idaarc, IO.FileMode.Create))
    End Sub

    Public Sub GrabarIDZ(ByVal IdaData As IdaLib.IdaTypeInternal, ByVal IdzArc As String)
        Dim idb As Byte() = IdaaByte(IdaData)
        Dim claveB As Byte() = System.Text.UTF8Encoding.UTF8.GetBytes(clave)
        Dim iz As Byte() = crypto.strEncryptNEwB(idb, claveB)
        My.Computer.FileSystem.WriteAllBytes(IdzArc, iz, False)
    End Sub

    Public Sub GrabarADV(ByVal advice As IdaLib.Advice, ByVal advarc As String)
        Dim adv As Byte() = ADVaByte(advice)
        Dim claveB As Byte() = System.Text.UTF8Encoding.UTF8.GetBytes(clave)
        Dim ad As Byte() = crypto.strEncryptNEwB(adv, claveB)
        My.Computer.FileSystem.WriteAllBytes(advarc, ad, False)
    End Sub

    Public Sub codificarIda(ByVal i As IdaTypeInternal, ByVal StreamSalida As System.IO.Stream, Optional ByVal Cerrar As Boolean = True)
        Dim streamFile1 As New System.IO.BinaryWriter(StreamSalida)

        With streamFile1
            .Write(i.VERSION.ToCharArray, 0, 1)
            .Write(i.TARJ.ToCharArray, 0, 20)
            .Write(i.EXPDATE.ToCharArray, 0, 4)
            .Write(Decimal.ToOACurrency(i.IMPO))
            .Write(i.MANUAL)
            .Write(i.PLANINT)
            .Write(i.CODSEG.ToCharArray, 0, 30)
            .Write(i.TICCAJ)
            .Write(i.CAJERA)
            .Write(i.HORA)
            .Write(i.TRACK2.ToCharArray, 0, 37)
            .Write(i.TRACK1.ToCharArray, 0, 77)
            .Write(i.CodAut.ToCharArray, 0, 6)
            .Write(i.TKTORI)
            .Write(i.FECORI.ToCharArray, 0, 6)
            .Write(i.PLANINTori)
            .Write(i.oper)
            .Write(i.cmd)
            .Write(i.cajadir.ToCharArray, 0, 26)
            .Write(i.TKID.ToCharArray, 0, 4)
            .Write(Decimal.ToOACurrency(i.CASHBACK))
            .Write(i.CHECK.ToCharArray, 0, 2)
            If EMV Then
                'TODO: esto es para chip, ver
                .Write(i.CRIPTO.ToCharArray, 0, 300)
                .Write(i.CARDSEQ.ToCharArray, 0, 3)
                '.Write(i.PAQUETE.ToCharArray, 0, 120)
                '.Write(i.TIPOENC.ToCharArray, 0, 1)
                '.Write(i.SERIEF.ToCharArray, 0, 12)
                .Write(i.SERIEL.ToCharArray, 0, 16)
                .Write(i.NOMBRE.ToCharArray, 0, 45)
                .Write(i.ServCode.ToCharArray, 0, 3)
                '.Write(i.APPVERSION.ToCharArray, 0, 15)
                '.Write(i.AID.ToCharArray, 0, 16)
                '.Write(i.APN.ToCharArray, 0, 16)

            End If

        End With
        If Cerrar Then streamFile1.Close()

    End Sub

    Public Sub codificarADV(ByVal i As Advice, ByVal StreamSalida As System.IO.Stream, Optional ByVal Cerrar As Boolean = True)
        Dim streamFile1 As New System.IO.BinaryWriter(StreamSalida)

        With streamFile1
            .Write(i.resp.ToCharArray, 0, 2)
            .Write(i.autorizacion.ToCharArray, 0, 6)
            .Write(i.cripto.ToCharArray, 0, 300)
        End With
        If Cerrar Then streamFile1.Close()

    End Sub

#End Region

    Public Sub verida(ByVal i As IdaTypeInternal)
        Debug.Print("version " + i.VERSION.ToString)
        Debug.Print("tarj    " + i.TARJ.ToString)
        Debug.Print("expdate " + i.EXPDATE.ToString)
        Debug.Print("impo    " + i.IMPO.ToString)
        Debug.Print("manual  " + i.MANUAL.ToString)
        Debug.Print("planint " + i.PLANINT.ToString)
        Debug.Print("codseg  " + i.CODSEG.ToString)
        Debug.Print("ticcaj  " + i.TICCAJ.ToString)
        Debug.Print("cajera  " + i.CAJERA.ToString)
        Debug.Print("hora    " + i.HORA.ToString)
        Debug.Print("track1  " + i.TRACK1.ToString)
        Debug.Print("tracj2  " + i.TRACK2.ToString)
        Debug.Print("codaut  " + i.CodAut.ToString)
        Debug.Print("tktori  " + i.TKTORI.ToString)
        Debug.Print("fecori  " + i.FECORI.ToString)
        Debug.Print("planino " + i.PLANINTori.ToString)
        Debug.Print("oper    " + i.oper.ToString)
        Debug.Print("cmd     " + i.cmd.ToString)
        Debug.Print("cajadir " + i.cajadir.ToString)
        Debug.Print("tkid    " + i.TKID.ToString)
        Debug.Print("Cback   " + i.CASHBACK.ToString)
        Debug.Print("check   " + i.CHECK.ToString)
        If EMV Then
            'TODO: esto es para chip, ver
            Debug.Print("Cripto  " + i.CRIPTO.ToString)
            Debug.Print("Cardseq " + i.CARDSEQ.ToString)
            'Debug.Print("Appver  " + i.APPVERSION.ToString)
            'Debug.Print("Paquete " + i.PAQUETE.ToString)
            'Debug.Print("TipoEnc " + i.TIPOENC.ToString)
            'Debug.Print("Serief  " + i.SERIEF.ToString)
            Debug.Print("SerieL  " + i.SERIEL.ToString)
            Debug.Print("Nombre  " + i.NOMBRE.ToString)
            Debug.Print("ServCode" + i.ServCode.ToString)
            'Debug.Print("App ID  " + i.AID.ToString)
            'Debug.Print("App Name" + i.APN.ToString)
        End If

    End Sub


#Region "Escritura Vta"

    ' inicializarVTAdesdeIDA
    Public Function InicializarVtaDesdeIDA(ida As IdaTypeInternal) As VtaType
        Dim vta As New VtaType
        vta.VtaCheck = ida.CHECK
        vta.VtaOk = 0
        vta.VtaVersion = "1"
        Return vta
    End Function

#End Region

#Region "Conexion TCP"








#End Region

End Module
