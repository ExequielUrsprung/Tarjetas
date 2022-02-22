Imports System
Imports System.Net
Imports System.Text
Imports System.IO
Imports System.Threading
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports log4net
Imports System.Reflection

Public Class MensajeCliente
    Property Texto As String
    Property Fecha As Date
    Property Tipo As TipoMensaje
End Class




Public Enum TipoMensaje
    Informacion = 0
    [Error] = 10
    Debug = 20
End Enum
Public Class RequestState
    ' This class stores the request state of the request.
    Public request As HttpWebRequest
    Public json As String = ""


    Public Sub New()
        request = Nothing
    End Sub ' New
End Class ' RequestState

Public Class QR
    Public Shared allDone As New ManualResetEvent(False)
    Property nomarc As String
    Property nomvta As String
    Property terminal As String

    Property StringLog As String = ""
    Property loguear As Boolean = True
    Property puerto As String

    Property version As String = "7"
    Dim estado_produccion As String

    Public Sub Logger(mensaje As String)
        StringLog = StringLog & mensaje & vbNewLine

    End Sub

    Public Function Logger() As ILog
        Return LogManager.GetLogger(MethodBase.GetCurrentMethod.DeclaringType)
    End Function

    Public Event Mensaje(msj As MensajeCliente)

    ' token productivo  APP_USR-8926656760477656-112613-6aa8ba59457fdf4bddda560c9ea66b68-180824062
    ' token test        APP_USR-6315357662534704-101512-3ee546fe5844214d9db3ffa14af0d58a-478976545

    ' user-id productivo  180824062
    ' user-id test        478976545


    Dim token As String
    Dim user_id As String

    Private Sub Setear_parametros(modo As String)
        If modo = "P" Then 'productivo
            token = "APP_USR-8926656760477656-112613-6aa8ba59457fdf4bddda560c9ea66b68-180824062"
            user_id = "180824062"
            estado_produccion = "PRODUCCION"
        Else 'test
            token = "APP_USR-6315357662534704-101512-3ee546fe5844214d9db3ffa14af0d58a-478976545"
            user_id = "478976545"
            estado_produccion = "TEST"
        End If
    End Sub
    Public Sub New(tipo As String)
        Setear_parametros(tipo)
    End Sub

    Public Sub Log(texto As String)
        Dim objStreamWriter As StreamWriter

        'Pass the file path and the file name to the StreamWriter constructor.
        objStreamWriter = New StreamWriter("C:\SistemaQR\QR\Log\" & nomarc, True)
        objStreamWriter.WriteLine(String.Format("{0:dd/MM hh:mm:ss} {1}", Now, texto))
        objStreamWriter.Close()

    End Sub
    Private Sub GrabarRespuesta(resp As Respuesta)
        Dim objStreamWriter As New StreamWriter("C:\SistemaQR\RespuestasQR\" & nomvta, False)
        Try
            'Pass the file path and the file name to the StreamWriter constructor.
            'objStreamWriter = New StreamWriter("C:\Proyecto Tarjetas\Pei\" & nomvta, True)
            objStreamWriter.WriteLine(resp.ToString)

        Catch ex As Exception
        Finally
            objStreamWriter.Close()
        End Try
    End Sub

    Private Sub mensajeQR(s As String)
        Dim msj As New MensajeCliente() With {.Fecha = Now, .Texto = s}
        msj.Tipo = TipoMensaje.Informacion
        If loguear Then Log(s)
        Console.WriteLine(s)
        Logger.Info(s)
        RaiseEvent Mensaje(msj)

    End Sub

#Region "suc/cajas"
    Public Function Crear_Sucursales(Nombre As String, calle As String, numero As String, ciudad As String, provincia As String, latitud As String, longitud As String, external_id As String) As String
        mensajeQR("Estado productivo " & estado_produccion.ToUpper)
        mensajeQR("----------------- Creando sucursal " & Nombre & " ----------------------")
        ' Create a new request.
        ServicePointManager.SecurityProtocol = 3072
        Dim myWebRequest As HttpWebRequest = WebRequest.Create($"https://api.mercadopago.com/users/{user_id}/stores?access_token={token}")


        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        myWebRequest.Timeout = 30000
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "POST"
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"


        myRequestState.json = "{""name"": """ & Nombre &
                               """, ""location"": {""street_number"": """ & numero &
                               """,""street_name"": """ & calle &
                               """,""city_name"": """ & ciudad &
                               """,""state_name"": """ & provincia &
                               """,""latitude"": " & latitud &
                               ",""longitude"": " & longitud &
                               "},""external_id"": """ & external_id & """}"


        Console.WriteLine(myRequestState.json)
        mensajeQR(myRequestState.json)
        Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject = Nothing
        Dim jsontext As String = ""
        Try
            mensajeQR("----------------- Respuesta sucursal " & Nombre & " ----------------------")
            mywebresponse = myWebRequest.GetResponse()
            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)
            Dim readBuff(256) As [Char]
            Dim count As Integer = streamRead.Read(readBuff, 0, 256)



            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                jsonObject = JObject.Parse(outputData)
                Console.WriteLine(outputData)
                count = streamRead.Read(readBuff, 0, 256)
            End While



            If jsonObject IsNot Nothing Then
                mensajeQR(jsonObject.ToString)
                jsontext = jsonObject.SelectToken("id").ToString
            End If
            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()

        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            Console.WriteLine(jsonObject.ToString)
            mensajeQR(jsonObject.ToString)
            'mensajePei("Error en compra: " & exWeb.Message)
            'mensajePei("Status Code: " & exWeb.Status)
            'mensajePei("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString)
            'mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))
        Catch ex As Exception
            mensajeQR("Error creando sucursal: " & ex.Message)
            jsontext = ""
        Finally
            If Not mywebresponse Is Nothing Then

                mywebresponse.Close()
            End If


        End Try
        Return jsontext
    End Function

    Public Sub Traer_sucursales()
        ' Create a new request.
        ServicePointManager.SecurityProtocol = 3072
        Dim myWebRequest As HttpWebRequest = WebRequest.Create($"https://api.mercadopago.com/users/{user_id}/stores/search?access_token={token}")


        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        'myWebRequest.Timeout = 30000
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "GET"
        'myRequestState.request.Accept = "application/json"
        'myRequestState.request.ContentType = "application/json"

        'myRequestState.json = "{""name"": ""Pinguino 1"", ""location"": {""street_number"": ""425"",""street_name"": ""Av. Lehmann"",""city_name"": ""Castellanos"",""state_name"": ""Santa Fe"",""latitude"": -32.8897322,""longitude"": -68.8443275,""reference"": """"},""external_id"": ""1235""}"
        '"{""credenciales"":{""contrasena"":""Prueba123"",""usuario"":""sv_30521387862_pei""}}"
        ''''Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback2, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        ''''allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject
        Dim jsontext As String = ""
        Try



            mywebresponse = myWebRequest.GetResponse()
            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)
            Dim readBuff(1024) As [Char]
            Dim count As Integer = streamRead.Read(readBuff, 0, 1024)



            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                jsonObject = JObject.Parse(outputData)
                Console.WriteLine(outputData)
                count = streamRead.Read(readBuff, 0, 1024)
            End While

            Console.WriteLine(jsonObject.ToString)

            If jsonObject IsNot Nothing Then
                'jsontext = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
                Dim jsonchild = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
            End If
            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()

        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            Console.WriteLine(jsonObject.ToString)
            'mensajePei("Error en compra: " & exWeb.Message)
            'mensajePei("Status Code: " & exWeb.Status)
            'mensajePei("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString)
            'mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            If Not mywebresponse Is Nothing Then

                mywebresponse.Close()
            End If


        End Try

    End Sub

    Public Sub Borrar_sucursales()
        ' Create a new request.
        ServicePointManager.SecurityProtocol = 3072
        Dim myWebRequest As HttpWebRequest = WebRequest.Create($"https://api.mercadopago.com/users/{user_id}/stores/30287298?access_token={token}")


        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        'myWebRequest.Timeout = 30000
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "DELETE"
        'myRequestState.request.Accept = "application/json"
        'myRequestState.request.ContentType = "application/json"

        'myRequestState.json = "{""name"": ""Pinguino 1"", ""location"": {""street_number"": ""425"",""street_name"": ""Av. Lehmann"",""city_name"": ""Castellanos"",""state_name"": ""Santa Fe"",""latitude"": -32.8897322,""longitude"": -68.8443275,""reference"": """"},""external_id"": ""1235""}"
        '"{""credenciales"":{""contrasena"":""Prueba123"",""usuario"":""sv_30521387862_pei""}}"
        ''''Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback2, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        ''''allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject
        Dim jsontext As String = ""
        Try



            mywebresponse = myWebRequest.GetResponse()
            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)
            Dim readBuff(1024) As [Char]
            Dim count As Integer = streamRead.Read(readBuff, 0, 1024)



            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                jsonObject = JObject.Parse(outputData)
                Console.WriteLine(outputData)
                count = streamRead.Read(readBuff, 0, 1024)
            End While

            Console.WriteLine(jsonObject.ToString)

            If jsonObject IsNot Nothing Then
                'jsontext = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
                Dim jsonchild = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
            End If
            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()

        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            Console.WriteLine(jsonObject.ToString)
            'mensajePei("Error en compra: " & exWeb.Message)
            'mensajePei("Status Code: " & exWeb.Status)
            'mensajePei("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString)
            'mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            If Not mywebresponse Is Nothing Then

                mywebresponse.Close()
            End If


        End Try

    End Sub


    Public Function Crear_Caja(nombre_caja As String, store_id As String, external_store_id As String, external_id As String) As Respuesta
        mensajeQR("Estado productivo " & estado_produccion.ToUpper)
        mensajeQR("----------------- Creando caja " & nombre_caja & " ----------------------")
        Dim resp As New Respuesta
        ' Create a new request.
        ServicePointManager.SecurityProtocol = 3072
        Dim myWebRequest As HttpWebRequest = WebRequest.Create($"https://api.mercadopago.com/pos?access_token={token}")


        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        'myWebRequest.Timeout = 30000
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "POST"
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"

        myRequestState.json = "{""name"":""" & nombre_caja &
                              """,""fixed_amount"": true, ""store_id"": """ & store_id &
                              """,""external_store_id"": """ & external_store_id &
                              """,""external_id"": """ & external_id & """}"
        mensajeQR(myRequestState.json)
        Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject = Nothing
        Dim jsontext As String = ""
        Dim streamresponse As Stream = Nothing
        Dim streamread As StreamReader = Nothing
        Try
            mensajeQR("----------------- Respuesta Creando caja " & nombre_caja & " ----------------------")
            mywebresponse = myWebRequest.GetResponse()
            streamresponse = mywebresponse.GetResponseStream()
            streamread = New StreamReader(streamresponse)
            Dim readBuff(mywebresponse.ContentLength) As [Char]
            Dim count As Integer = streamread.Read(readBuff, 0, mywebresponse.ContentLength)


            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                jsonObject = JObject.Parse(outputData)
                Console.WriteLine(outputData)
                count = streamread.Read(readBuff, 0, mywebresponse.ContentLength)
            End While

            If jsonObject IsNot Nothing Then
                mensajeQR(jsonObject.ToString)
                resp.id_user = jsonObject.SelectToken("id").ToString
                resp.descripcion = jsonObject.SelectToken("qr").SelectToken("template_document").ToString
            End If


        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            resp.id_user = ""
            mensajeQR(jsonObject.ToString)
            Console.WriteLine(jsonObject.ToString)
            'mensajePei("Error en compra: " & exWeb.Message)
            'mensajePei("Status Code: " & exWeb.Status)
            'mensajePei("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString)
            'mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))
        Catch ex As Exception
            mensajeQR("Error creando caja " & nombre_caja & " " & ex.Message)
            resp.id_user = ""

        Finally
            If Not mywebresponse Is Nothing Then mywebresponse.Close()
            If Not streamread Is Nothing Then streamread.Close()
            If Not mywebresponse Is Nothing Then mywebresponse.Close()


        End Try
        Return resp
    End Function

    Public Sub Borrar_Cajas()
        ' Create a new request.
        ServicePointManager.SecurityProtocol = 3072
        Dim myWebRequest As HttpWebRequest = WebRequest.Create($"https://api.mercadopago.com/pos/3513170?access_token={token}")


        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        'myWebRequest.Timeout = 30000
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "DELETE"
        'myRequestState.request.Accept = "application/json"
        'myRequestState.request.ContentType = "application/json"


        ''''Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback2, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        ''''allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject
        Dim jsontext As String = ""
        Try



            mywebresponse = myWebRequest.GetResponse()
            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)
            Dim readBuff(1024) As [Char]
            Dim count As Integer = streamRead.Read(readBuff, 0, 1024)



            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                jsonObject = JObject.Parse(outputData)
                Console.WriteLine(outputData)
                count = streamRead.Read(readBuff, 0, 1024)
            End While

            Console.WriteLine(jsonObject.ToString)

            If jsonObject IsNot Nothing Then
                'jsontext = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
                Dim jsonchild = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
            End If
            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()

        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            Console.WriteLine(jsonObject.ToString)
            'mensajePei("Error en compra: " & exWeb.Message)
            'mensajePei("Status Code: " & exWeb.Status)
            'mensajePei("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString)
            'mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            If Not mywebresponse Is Nothing Then

                mywebresponse.Close()
            End If


        End Try

    End Sub

    Public Sub Traer_cajas(id As String, ext_store_id As String, ext_id As String)
        ' Create a new request.
        ServicePointManager.SecurityProtocol = 3072
        Dim myWebRequest As HttpWebRequest = WebRequest.Create($"https://api.mercadopago.com/pos?access_token={token}")


        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        'myWebRequest.Timeout = 30000
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "GET"
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"

        'myRequestState.json = "{""store_id"": """ & id & """,""external_store_id"": """ & ext_store_id & """,""external_id"": """ & ext_id & """}"
        '"{""credenciales"":{""contrasena"":""Prueba123"",""usuario"":""sv_30521387862_pei""}}"
        ''Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback2, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        ''allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject
        Dim jsontext As String = ""
        Try



            mywebresponse = myWebRequest.GetResponse()
            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)
            Dim readBuff(2048) As [Char]
            Dim count As Long = streamRead.Read(readBuff, 0, 2048)


            Dim texto As String = ""
            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                texto = texto + outputData
                Console.WriteLine(outputData)
                count = streamRead.Read(readBuff, 0, 2048)
            End While
            jsonObject = JObject.Parse(texto)
            Console.WriteLine(jsonObject.ToString)
            mensajeQR(jsonObject.ToString)
            If jsonObject IsNot Nothing Then
                'jsontext = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
                Dim jsonchild = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
            End If
            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()

        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            Console.WriteLine(jsonObject.ToString)
            'mensajePei("Error en compra: " & exWeb.Message)
            'mensajePei("Status Code: " & exWeb.Status)
            'mensajePei("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString)
            'mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            If Not mywebresponse Is Nothing Then

                mywebresponse.Close()
            End If


        End Try

    End Sub

    Public Sub Modificar_cajas(id As String)
        ' Create a new request.
        ServicePointManager.SecurityProtocol = 3072
        Dim myWebRequest As HttpWebRequest = WebRequest.Create($"https://api.mercadopago.com/pos/{id}?access_token={token}")


        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        'myWebRequest.Timeout = 30000
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "PUT"
        'myRequestState.request.Accept = "application/json"
        'myRequestState.request.ContentType = "application/json"
        myRequestState.json = "{""category"":0 ,""fixed_amount"" : True ,""store_id"": ""30287603"",""external_store_id"": ""1235"",""external_id"": ""caja15p4"", ""url"": ""http://pinguino.com.ar/wsIpn/api/ipn""}"
        '"{""credenciales"":{""contrasena"":""Prueba123"",""usuario"":""sv_30521387862_pei""}}"
        Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject
        Dim jsontext As String = ""
        Try



            mywebresponse = myWebRequest.GetResponse()
            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)
            Dim readBuff(2048) As [Char]
            Dim count As Integer = streamRead.Read(readBuff, 0, 2048)



            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                jsonObject = JObject.Parse(outputData)
                Console.WriteLine(outputData)
                count = streamRead.Read(readBuff, 0, 2048)
            End While

            Console.WriteLine(jsonObject.ToString)

            If jsonObject IsNot Nothing Then
                'jsontext = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
                Dim jsonchild = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
            End If
            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()

        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            Console.WriteLine(jsonObject.ToString)
            'mensajePei("Error en compra: " & exWeb.Message)
            'mensajePei("Status Code: " & exWeb.Status)
            'mensajePei("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString)
            'mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))
        Catch ex As Exception
            Console.WriteLine(ex.Message)
        Finally
            If Not mywebresponse Is Nothing Then

                mywebresponse.Close()
            End If


        End Try

    End Sub
#End Region

    Public Function Operar(s As String, oper As String) As Boolean
        Dim resp As New Respuesta
        Dim fin As Boolean

        '--------------------------------------------------------------------
        '--- ESTO ES LLAMADO POR EL ClienteQR.EXE
        '--------------------------------------------------------------------
        Try
            Log("Estado productivo " & estado_produccion.ToUpper)
            If oper = "COMPRA" Then ' trx.tipoOperacion = lib_PEI.Concepto.COMPRA_DE_BIENES Then
                Log("Compra")
                Dim trx As New lib_QR.Operacion_Compra
                Log("Leyendo argumentos")
                trx.LeerArgumento(s)
                Log("Argumentos leídos")
                terminal = trx.idTerminal
                resp.trace = trx.trace
                Log("Comenzando a generar orden para terminal " & terminal & " trace " & trx.trace & " importe " & trx.importe)

                '--- ACA GENERO LA ORDEN PARA MERCADO PAGO
                resp = Generar_orden(trx.importe, trx.trace)

                fin = True

            ElseIf oper = "CANCELA" Then
                Log("Cancelar operación.")
                terminal = s.Split("-")(0)
                resp = Cancelar_orden(s)
                fin = True

            ElseIf oper = "DEVOLUCION" Then
                Log("Devolucion")
                Dim trx As New lib_QR.Operacion_Compra
                Log("Leyendo argumentos")
                trx.LeerArgumento(s)
                Log("Argumentos leídos")
                terminal = trx.idTerminal
                Log("Comenzando a generar devolucion para terminal " & terminal & " id de Pago " & trx.idpago & " Trace: " & trx.trace & " importe " & trx.importe)
                resp = Generar_devolucion(trx.importe, trx.idpago, trx.trace)

                fin = True

            End If
            '    Else
            '        resp.descRespuesta = mensajesRepuesta.FALTA_TOKEN
            '        resp.trace = trace.Split("-")(1)
            '        resp.descripcion = "EL HOST REMOTO NO RESPONDE."
            '        resp.fechahora_respuesta = Now


            '        'no pudo iniciar sesion

        Catch ex As Exception
            'mensajeQR(mensajesRepuesta.ERROR_DESCONOCIDO.ToString & " " & ex.Message & " " & ex.StackTrace)
            'resp.descRespuesta = mensajesRepuesta.ERROR_DESCONOCIDO
            'resp.trace = 
            Log(ex.Message)
            resp.estado = Estado.UNKNOWN
            resp.descripcion = "ERROR DESCONOCIDO"
            resp.fechahora_respuesta = Now
            resp.tipoOperacion = oper

            fin = False

        End Try
        GrabarRespuesta(resp)
        Return fin

    End Function



    Public Function Generar_orden(importe As String, trace As String) As Respuesta
        mensajeQR("---------------  Generando orden de pago --------------------------")
        Dim streamResponse As Stream = Nothing
        Dim streamRead As StreamReader = Nothing
        ' Create a new request.
        Dim respuesta_orden As New Respuesta
        ServicePointManager.SecurityProtocol = 3072

        '---------------------------------------------------------------------------------------------------------------------------------------
        '--- ACA DE ACUERDO AL USER_ID Y TOKEN QUE SON 2 VARIABLES, FORMO EL LINK DE LA PAGINA DE MARCADO PAGO DONDE VOY A REALIZAR LA COMPRA
        '--- USER_ID Y TRACE ESTAN SETEADOS ARRIBA, SEGUN SEA TEST O PRODUCCION
        '---------------------------------------------------------------------------------------------------------------------------------------
        Dim myWebRequest As HttpWebRequest = WebRequest.Create($"https://api.mercadopago.com/mpmobile/instore/qr/{user_id}/{terminal}?access_token={token}")
        mensajeQR($"https://api.mercadopago.com/mpmobile/instore/qr/{user_id}/{terminal}?access_token={token}")

        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        'myWebRequest.Timeout = 30000
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "POST"          '--- POST ES MANDAR, GET ES PARA LEER
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"
        If puerto = "9292" Then
            myRequestState.json = "{""external_reference"":""" & trace & """,""notification_url"":""http://www.pinguino.com.ar:" & puerto & "/wsIpn/api/ipn"",""items"":[{""id"":""1"",""title"" : ""Compra Supermercados Pinguino"",""currency_id"" : ""ARS"",""unit_price"" : " & importe & ",""quantity"" : 1,""description"": ""Articulos Varios""}]}"
        Else
            If puerto = "" Then
                myRequestState.json = "{""external_reference"":""" & trace & """,""items"":[{""id"":""1"",""title"" : ""Compra Supermercados Pinguino"",""currency_id"" : ""ARS"",""unit_price"" : " & importe & ",""quantity"" : 1,""description"": ""Articulos Varios""}]}"
            Else
                myRequestState.json = "{""external_reference"":""" & trace & """,""notification_url"":""https://pinguino.com.ar:" & puerto & "/wsIpn/api/ipn"",""items"":[{""id"":""1"",""title"" : ""Compra Supermercados Pinguino"",""currency_id"" : ""ARS"",""unit_price"" : " & importe & ",""quantity"" : 1,""description"": ""Articulos Varios""}]}"
            End If
        End If


        mensajeQR(myRequestState.request.Method & " " & myRequestState.request.RequestUri.ToString & vbNewLine &
                   "                <HEADERS>" & vbNewLine &
                   "                    " & "content-type: " & myRequestState.request.Headers.Item("content-type") & vbNewLine &
                   "                    " & "accept: " & myRequestState.request.Headers.Item("accept") & vbNewLine &
                   "                <DATA>" & vbNewLine &
                   JObject.Parse(myRequestState.json).ToString())



        '------------------------------------------------------------------------------------------------------------------------------------------
        '--- ACA MANDA EL MENSAJE (ACA SE GENERA LA ORDEN) - HACER CLICK SOBRE READCALLBACK
        '--- A PARTIR DE ACA MERCADO PAGO YA TIENE LA ORDEN COMENZADA, Y ESPERA QUE EL CLIENTE ESCANEE EL QR Y SELECCIONE EL PAGO
        '--- DESPUES DE MANDAR LA ORDEN, ME RELACIONO CON MERCADO PAGO CON LOS IPN O CON EL SEARCH QUE ES CUANDO LEE EL QR, SELECCIONA PAGO, ETC
        '--- ACA YA TERMINE, NO HACE MAS NADA, NO IMPORTA LA RESPUESTA, NO ME INTERESA.
        '------------------------------------------------------------------------------------------------------------------------------------------
        Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        allDone.WaitOne()



        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject = Nothing
        Dim jsontext As String = ""
        Try
            mensajeQR("-------------------------- RESPUESTA ORDEN -----------------------------")
            '-------------------------------------
            '--- ACA OBTIENE LA RESPUESTA
            '-------------------------------------
            mywebresponse = myWebRequest.GetResponse()
            streamResponse = mywebresponse.GetResponseStream()
            streamRead = New StreamReader(streamResponse)

            If mywebresponse.ContentLength > 0 Then
                mensajeQR("mywebresponse.contentlength: " & mywebresponse.ContentLength.ToString)
                Dim readBuff(mywebresponse.ContentLength) As [Char]
                Dim count As Integer = streamRead.Read(readBuff, 0, mywebresponse.ContentLength)
                While count > 0
                    Dim outputData As New [String](readBuff, 0, count)
                    jsonObject = JObject.Parse(outputData)
                    count = streamRead.Read(readBuff, 0, mywebresponse.ContentLength)
                End While
            Else
                mensajeQR("mywebresponse.contentlength: " & mywebresponse.ContentLength.ToString)
                Dim readBuff(1024) As [Char]
                Dim count As Integer = streamRead.Read(readBuff, 0, 1024)
                Dim texto As String = ""
                While count > 0
                    Dim outputData As New [String](readBuff, 0, count)
                    texto = texto + outputData
                    count = streamRead.Read(readBuff, 0, 1024)
                End While
                jsonObject = JObject.Parse(texto)
            End If


            If jsonObject IsNot Nothing Then
                mensajeQR(jsonObject.ToString)
                respuesta_orden.tipoOperacion = "COMPRA"
                respuesta_orden.fechahora_respuesta = Now
                respuesta_orden.estado = Estado.CREATED
                respuesta_orden.mail = ""
                respuesta_orden.trace = trace
            Else
                mensajeQR("JsonObject is nothing")
                respuesta_orden.tipoOperacion = "COMPRA"
                respuesta_orden.fechahora_respuesta = Now
                respuesta_orden.estado = Estado.CREATED
                respuesta_orden.mail = ""
                respuesta_orden.descripcion = "Se produjo un error leyendo la respuesta."
                respuesta_orden.trace = trace
            End If

        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            mensajeQR(jsonObject.ToString)

            respuesta_orden.estado = Estado.UNKNOWN
            respuesta_orden.descripcion = jsonObject.SelectToken("message").ToString
            respuesta_orden.fechahora_respuesta = Now
            respuesta_orden.trace = trace

            mensajeQR("Error en compra: " & exWeb.Message)
            mensajeQR("Status Code: " & exWeb.Status)
            mensajeQR("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("message").ToString)

        Catch ex As Exception
            mensajeQR(ex.Message)
            respuesta_orden.estado = Estado.UNKNOWN
            respuesta_orden.descripcion = "ERROR DESCONOCIDO"
            respuesta_orden.fechahora_respuesta = Now
            respuesta_orden.trace = trace


        Finally
            If mywebresponse IsNot Nothing Then mywebresponse.Close()
            If streamResponse IsNot Nothing Then streamResponse.Close()
            If streamRead IsNot Nothing Then streamRead.Close()

        End Try
        Return respuesta_orden
    End Function


    Public Function Generar_devolucion(importe As String, idPago As String, trace As String) As Respuesta
        mensajeQR("---------------  Generando orden de pago --------------------------")
        Dim streamResponse As Stream = Nothing
        Dim streamRead As StreamReader = Nothing
        ' Create a new request.
        Dim respuesta_orden As New Respuesta
        ServicePointManager.SecurityProtocol = 3072
        Dim myWebRequest As HttpWebRequest = WebRequest.Create($"https://api.mercadopago.com/v1/payments/{idPago}/refunds?access_token={token }")


        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        'myWebRequest.Timeout = 30000
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "POST"
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"
        If importe > 0 Then
            myRequestState.json = "{""amount"":" & importe & "}"
        End If
        If importe > 0 Then
            mensajeQR(myRequestState.request.Method & " " & myRequestState.request.RequestUri.ToString & vbNewLine &
                   "                <HEADERS>" & vbNewLine &
                   "                    " & "content-type: " & myRequestState.request.Headers.Item("content-type") & vbNewLine &
                   "                    " & "accept: " & myRequestState.request.Headers.Item("accept") & vbNewLine &
                   "                <DATA>" & vbNewLine &
                   JObject.Parse(myRequestState.json).ToString())
        Else
            mensajeQR(myRequestState.request.Method & " " & myRequestState.request.RequestUri.ToString & vbNewLine &
               "                <HEADERS>" & vbNewLine &
               "                    " & "content-type: " & myRequestState.request.Headers.Item("content-type") & vbNewLine &
               "                    " & "accept: " & myRequestState.request.Headers.Item("accept"))


        End If


        Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject = Nothing
        Dim jsontext As String = ""
        Try
            mensajeQR("-------------------------- RESPUESTA DEVOLUCION -----------------------------")
            mywebresponse = myWebRequest.GetResponse()
            streamResponse = mywebresponse.GetResponseStream()
            streamRead = New StreamReader(streamResponse)

            Dim readBuff(mywebresponse.ContentLength) As [Char]
            Dim count As Integer = streamRead.Read(readBuff, 0, mywebresponse.ContentLength)



            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                jsonObject = JObject.Parse(outputData)

                count = streamRead.Read(readBuff, 0, mywebresponse.ContentLength)
            End While

            If jsonObject IsNot Nothing Then
                mensajeQR(jsonObject.ToString)
                respuesta_orden.tipoOperacion = "DEVOLUCION"
                respuesta_orden.fechahora_respuesta = Now
                respuesta_orden.estado = Estado.CLOSED
                respuesta_orden.id_pago = idPago
            Else
                respuesta_orden.tipoOperacion = "DEVOLUCION"
                respuesta_orden.fechahora_respuesta = Now
                respuesta_orden.estado = Estado.UNKNOWN
                respuesta_orden.descripcion = "Se produjo un error leyendo la respuesta."
                respuesta_orden.id_pago = idPago
            End If

            mywebresponse.Close()

        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            mensajeQR(jsonObject.ToString)


            respuesta_orden.estado = Estado.UNKNOWN
            respuesta_orden.descripcion = jsonObject.SelectToken("message").ToString
            respuesta_orden.fechahora_respuesta = Now
            respuesta_orden.id_pago = idPago
            respuesta_orden.tipoOperacion = "DEVOLUCION"

            mensajeQR("Error en compra: " & exWeb.Message)
            mensajeQR("Status Code: " & exWeb.Status)
            mensajeQR("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("message").ToString)

        Catch ex As Exception
            mensajeQR(ex.Message)
            respuesta_orden.estado = Estado.UNKNOWN
            respuesta_orden.descripcion = "ERROR DESCONOCIDO"
            respuesta_orden.fechahora_respuesta = Now
            respuesta_orden.id_pago = idPago
            respuesta_orden.tipoOperacion = "DEVOLUCION"


        Finally
            If Not mywebresponse Is Nothing Then mywebresponse.Close()
            If streamResponse IsNot Nothing Then streamResponse.Close()
            If streamRead IsNot Nothing Then streamRead.Close()

        End Try
        Return respuesta_orden
    End Function

    Public Function Obtener_MerchantOrder(ByVal id As String) As Respuesta

        '---------------------------------------------------------------------------------
        '--- ACA OBTENGO LA RESPUESTA QUE ME DA MERCADO PAGO, EN BASE AL IPN QUE ME DIO
        '---------------------------------------------------------------------------------

        ' Create a new request.
        mensajeQR("-------------------------- CONSULTA IPN -----------------------------")
        Dim respuesta_orden As New Respuesta
        ServicePointManager.SecurityProtocol = 3072
        Dim myWebRequest As HttpWebRequest = WebRequest.Create($"https://api.mercadopago.com/merchant_orders/{id}?access_token={token}")
        mensajeQR($"https://api.mercadopago.com/merchant_orders/{id}?access_token={token}")

        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        'myWebRequest.Timeout = 30000
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "GET"
        'myRequestState.request.Accept = "application/json"
        'myRequestState.request.ContentType = "application/json"


        Dim mywebresponse As WebResponse = Nothing
        Dim streamResponse As Stream = Nothing
        Dim streamRead As StreamReader = Nothing
        Dim jsonObject As JObject = Nothing
        Dim jsontext As String = ""
        Try
            mensajeQR("-------------------------- RESPUESTA CONSULTA IPN -----------------------------")

            mywebresponse = myWebRequest.GetResponse()
            streamResponse = mywebresponse.GetResponseStream()
            streamRead = New StreamReader(streamResponse)


            '--------------------------------------------------------------------------------------
            '--- A VECES  mywebresponse.ContentLengt LLEGA CON -1 POR ESO HACE 2 LECTURAS
            '--- CUANDO ES -1 NO LEE TODO, LEE CADA 1024
            '--------------------------------------------------------------------------------------
            If mywebresponse.ContentLength > 0 Then
                mensajeQR("mywebresponse.contentlength: " & mywebresponse.ContentLength.ToString)
                Dim readBuff(mywebresponse.ContentLength) As [Char]
                Dim count As Integer = streamRead.Read(readBuff, 0, mywebresponse.ContentLength)
                While count > 0
                    Dim outputData As New [String](readBuff, 0, count)
                    jsonObject = JObject.Parse(outputData)
                    Console.WriteLine(outputData)
                    count = streamRead.Read(readBuff, 0, mywebresponse.ContentLength)
                End While
            Else
                mensajeQR("mywebresponse.contentlength: " & mywebresponse.ContentLength.ToString)
                Dim readBuff(1024) As [Char]
                Dim count As Integer = streamRead.Read(readBuff, 0, 1024)
                Dim texto As String = ""
                While count > 0
                    Dim outputData As New [String](readBuff, 0, count)
                    texto = texto + outputData
                    Console.WriteLine(outputData)
                    count = streamRead.Read(readBuff, 0, 1024)
                End While
                jsonObject = JObject.Parse(texto)
            End If

            '--------------
            'Dim readBuff(mywebresponse.ContentLength) As [Char]
            'Dim count As Integer = streamRead.Read(readBuff, 0, mywebresponse.ContentLength)

            'While count > 0
            '    Dim outputData As New [String](readBuff, 0, count)
            '    jsonObject = JObject.Parse(outputData)
            '    Console.WriteLine(outputData)
            '    count = streamRead.Read(readBuff, 0, mywebresponse.ContentLength)
            'End While
            '--------------
            'Console.WriteLine(jsonObject.ToString)

            If jsonObject IsNot Nothing Then
                mensajeQR(jsonObject.ToString)
                Try
                    respuesta_orden.estado = [Enum].Parse(GetType(lib_QR.Estado), jsonObject.SelectToken("status").ToString.ToUpper)  '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
                Catch ex As Exception
                End Try
                nomvta = jsonObject.SelectToken("external_reference").ToString.Split("-")(0)
                respuesta_orden.fechahora_respuesta = Now
                respuesta_orden.trace = jsonObject.SelectToken("external_reference").ToString
                respuesta_orden.tipoOperacion = "IPN"
                respuesta_orden.id_operacion = jsonObject.SelectToken("id").ToString
                If respuesta_orden.estado = Estado.CLOSED Then
                    '--------------------------------------------------------------------------------------------------------------
                    '--- DENTRO DE PAGOS (PAYMENTS), ME FIJO EL ESTADO DE LA OPERACION, POR ESO SE HACE UN FOR EACH, RECORRO HASTA
                    '--- ENCONTRAR EL CAMPO "STATUS" Y OBTENGO LA RESPUESTA
                    '
                    '--- EJEMPLO DE LO QUE VIENE EN JSON
                    '
                    '"preference_id" "Preference identification",
                    '"payments": [
                    '  {
                    '    "id": 9999999999,
                    '    "transaction_amount": 1,
                    '    "total_paid_amount": 1,
                    '    "shipping_cost": 0,
                    '    "currency_id": "[FAKER][CURRENCY][ACRONYM]",
                    '    "status": "approved",
                    '    "status_detail": "accredited",
                    '    "operation_type": "regular_payment",
                    '    "date_approved": "2019-04-02T14:35:35.000-04:00",
                    '    "date_created": "2019-04-02T14:35:34.000-04:00",
                    '    "last_modified": "2019-04-02T14:35:35.000-04:00",
                    '    "amount_refunded": 0
                    '  }
                    '--------------------------------------------------------------------------------------------------------------
                    For Each fp In jsonObject.SelectToken("payments").Children()
                        If fp.SelectToken("status") = "approved" Then
                            respuesta_orden.id_pago = fp.SelectToken("id")
                        End If
                    Next
                    respuesta_orden.id_user = jsonObject.SelectToken("payer").SelectToken("id").ToString
                    respuesta_orden.mail = jsonObject.SelectToken("payer").SelectToken("email").ToString
                End If
            Else
                respuesta_orden.tipoOperacion = "IPN"
                respuesta_orden.fechahora_respuesta = Now
                respuesta_orden.estado = Estado.UNKNOWN
                respuesta_orden.descripcion = "Se produjo un error leyendo la respuesta ipn."

                'Dim jsonchild = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
            End If


        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            mensajeQR("Parseando JSONObject")
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)

            respuesta_orden.tipoOperacion = "IPN"
            respuesta_orden.fechahora_respuesta = Now
            respuesta_orden.estado = Estado.UNKNOWN
            respuesta_orden.descripcion = jsonObject.SelectToken("message").ToString

            If jsonObject IsNot Nothing Then mensajeQR(jsonObject.ToString)

            mensajeQR("Error en compra: " & exWeb.Message)
            mensajeQR("Status Code: " & exWeb.Status)
            ' mensajeQR("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("message").ToString)

        Catch ex As Exception
            mensajeQR(ex.Message)
            respuesta_orden.estado = Estado.UNKNOWN
            respuesta_orden.descripcion = "ERROR DESCONOCIDO"
            respuesta_orden.fechahora_respuesta = Now
            respuesta_orden.trace = id
        Finally
            If mywebresponse IsNot Nothing Then mywebresponse.Close()
            If streamResponse IsNot Nothing Then streamResponse.Close()
            If streamRead IsNot Nothing Then streamRead.Close()

        End Try
        Return respuesta_orden
    End Function
    Public Function Cancelar_orden(trace As String) As Respuesta
        mensajeQR("-------------------------- Cancelando orden ----------------------------")
        Dim streamResponse As Stream = Nothing
        Dim streamRead As StreamReader = Nothing
        Dim respuesta_orden As New Respuesta
        ' Create a new request.

        ServicePointManager.SecurityProtocol = 3072
        Dim myWebRequest As HttpWebRequest = WebRequest.Create($"https://api.mercadopago.com/mpmobile/instore/qr/{user_id}/{terminal}?access_token={token}")


        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        myWebRequest.Timeout = 30000
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "DELETE"
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"


        mensajeQR(myRequestState.request.Method & " " & myRequestState.request.RequestUri.ToString & vbNewLine &
                   "                <HEADERS>" & vbNewLine &
                   "                    " & "content-type: " & myRequestState.request.Headers.Item("content-type") & vbNewLine &
                   "                    " & "accept: " & myRequestState.request.Headers.Item("accept"))

        Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback2, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        allDone.WaitOne()


        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject = Nothing
        Dim jsontext As String = ""
        Try
            mensajeQR("-------------------------- RESPUESTA CANCELA-----------------------------")
            mywebresponse = myWebRequest.GetResponse()
            streamResponse = mywebresponse.GetResponseStream()
            streamRead = New StreamReader(streamResponse)



            If mywebresponse.ContentLength > 0 Then
                mensajeQR("mywebresponse.contentlength: " & mywebresponse.ContentLength.ToString)
                Dim readBuff(mywebresponse.ContentLength) As [Char]
                Dim count As Integer = streamRead.Read(readBuff, 0, mywebresponse.ContentLength)
                While count > 0
                    Dim outputData As New [String](readBuff, 0, count)
                    jsonObject = JObject.Parse(outputData)
                    Console.WriteLine(outputData)

                    count = streamRead.Read(readBuff, 0, mywebresponse.ContentLength)
                End While

            Else
                mensajeQR("mywebresponse.contentlength: " & mywebresponse.ContentLength.ToString)
                Dim readBuff(1024) As [Char]
                Dim count As Integer = streamRead.Read(readBuff, 0, 1024)
                Dim texto As String = ""
                While count > 0
                    Dim outputData As New [String](readBuff, 0, count)
                    texto = texto + outputData
                    Console.WriteLine(outputData)

                    count = streamRead.Read(readBuff, 0, 1024)
                End While
                If texto <> "" Then jsonObject = JObject.Parse(texto)
            End If


            'If jsonObject IsNot Nothing Then
            '    Console.WriteLine(jsonObject.ToString)
            '    mensajeQR(jsonObject.ToString)
            'jsontext = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
            ' Dim jsonchild = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
            respuesta_orden.estado = Estado.CANCELLED
            respuesta_orden.descripcion = "CANCELADO"
            respuesta_orden.tipoOperacion = "CANCELA"
            respuesta_orden.trace = trace
            'Else
            '    respuesta_orden.estado = Estado.UNKNOWN
            '    respuesta_orden.descripcion = "CANCELADO"

            'End If


        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            'Console.WriteLine(jsonObject.ToString)

            mensajeQR(jsonObject.ToString)
            mensajeQR("Error cancelando: " & exWeb.Message)
            mensajeQR("Status Code: " & exWeb.Status)
            mensajeQR("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("message").ToString & " - " & jsonObject.SelectToken("error").ToString)

            respuesta_orden.descripcion = jsonObject.SelectToken("message").ToString
            respuesta_orden.estado = Estado.CANCELLED
            respuesta_orden.tipoOperacion = "CANCELA"
            respuesta_orden.trace = trace
        Catch ex As Exception
            Console.WriteLine(ex.Message)
            mensajeQR("Error cancelando: " & ex.Message)
            mensajeQR("Stack trace: " & ex.StackTrace)
            mensajeQR("Source: " & ex.Source)
            respuesta_orden.descripcion = "Exception"
            respuesta_orden.estado = Estado.CANCELLED
            respuesta_orden.tipoOperacion = "CANCELA"
            respuesta_orden.trace = trace
        Finally
            If mywebresponse IsNot Nothing Then mywebresponse.Close()
            If streamResponse IsNot Nothing Then streamResponse.Close()
            If streamRead IsNot Nothing Then streamRead.Close()

        End Try
        Return respuesta_orden
    End Function

    Public Function Search_MerchantOrder(ByVal reference As String) As Respuesta
        mensajeQR("-------------------------- SEARCH " & reference & " -----------------------------")

        ' Create a new request.
        Dim respuesta_orden As New Respuesta
        ServicePointManager.SecurityProtocol = 3072
        Dim myWebRequest As HttpWebRequest = WebRequest.Create($"https://api.mercadopago.com/merchant_orders/search?external_reference={reference}&access_token={token}")
        mensajeQR($"https://api.mercadopago.com/merchant_orders/search?external_reference={reference}&access_token={token}")

        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        'myWebRequest.Timeout = 30000
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "GET"
        'myRequestState.request.Accept = "application/json"
        'myRequestState.request.ContentType = "application/json"


        Dim mywebresponse As WebResponse = Nothing
        Dim streamResponse As Stream = Nothing
        Dim streamRead As StreamReader = Nothing
        Dim jsonObject As JObject = Nothing
        Dim jsontext As String = ""
        Try
            mensajeQR("-------------------------- RESPUESTA SEARCH -----------------------------")

            mywebresponse = myWebRequest.GetResponse()
            streamResponse = mywebresponse.GetResponseStream()
            streamRead = New StreamReader(streamResponse)


            If mywebresponse.ContentLength > 0 Then
                mensajeQR("mywebresponse.contentlength: " & mywebresponse.ContentLength.ToString)
                Dim readBuff(mywebresponse.ContentLength) As [Char]
                Dim count As Integer = streamRead.Read(readBuff, 0, mywebresponse.ContentLength)

                While count > 0
                    Dim outputData As New [String](readBuff, 0, count)
                    jsonObject = JObject.Parse(outputData)
                    Console.WriteLine(outputData)
                    count = streamRead.Read(readBuff, 0, mywebresponse.ContentLength)
                End While

            Else
                mensajeQR("mywebresponse.contentlength: " & mywebresponse.ContentLength.ToString)
                Dim readBuff(1024) As [Char]
                Dim count As Integer = streamRead.Read(readBuff, 0, 1024)

                Dim texto As String = ""
                While count > 0
                    Dim outputData As New [String](readBuff, 0, count)
                    texto = texto + outputData
                    Console.WriteLine(outputData)
                    count = streamRead.Read(readBuff, 0, 1024)
                End While
                jsonObject = JObject.Parse(texto)
            End If

            Console.WriteLine(jsonObject.ToString)
            mensajeQR(jsonObject.ToString)
            If jsonObject IsNot Nothing Then
                If jsonObject.SelectToken("elements").ToString <> "" Then


                    For Each j In jsonObject.SelectToken("elements").Children()
                        'mensajeQR(j.ToString)
                        Try
                            respuesta_orden.estado = [Enum].Parse(GetType(lib_QR.Estado), j.SelectToken("status").ToString.ToUpper)  '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
                        Catch ex As Exception


                        End Try
                        nomvta = j.SelectToken("external_reference").ToString.Split("-")(0)
                        respuesta_orden.fechahora_respuesta = Now
                        respuesta_orden.trace = j.SelectToken("external_reference").ToString
                        respuesta_orden.tipoOperacion = "IPN"
                        respuesta_orden.id_operacion = j.SelectToken("id").ToString
                        If respuesta_orden.estado = Estado.CLOSED Then

                            For Each fp In j.SelectToken("payments").Children()
                                If fp.SelectToken("status") = "approved" Then
                                    respuesta_orden.id_pago = fp.SelectToken("id")
                                End If
                            Next
                            respuesta_orden.id_user = j.SelectToken("payer").SelectToken("id").ToString
                            respuesta_orden.mail = j.SelectToken("payer").SelectToken("email").ToString
                            Exit For

                        End If
                    Next
                Else
                    respuesta_orden.tipoOperacion = "IPN"
                    respuesta_orden.fechahora_respuesta = Now
                    respuesta_orden.estado = Estado.NORESPONSE
                    respuesta_orden.descripcion = "El cliente no escaneó el QR aún."
                    respuesta_orden.trace = reference
                End If
            Else
                respuesta_orden.tipoOperacion = "IPN"
                respuesta_orden.fechahora_respuesta = Now
                respuesta_orden.estado = Estado.NORESPONSE
                respuesta_orden.descripcion = "Se produjo un error leyendo la respuesta ipn."
                respuesta_orden.trace = reference
                'Dim jsonchild = jsonObject.SelectToken("results").Children() '.SelectToken("external_id").ToString '.SelectToken.("location").address_line").ToString
            End If


        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            If exWeb.Response IsNot Nothing Then
                jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
                mensajeQR(jsonObject.ToString)
            End If


            respuesta_orden.tipoOperacion = "IPN"
            respuesta_orden.fechahora_respuesta = Now
            respuesta_orden.estado = Estado.UNKNOWN
            If exWeb.Response IsNot Nothing Then respuesta_orden.descripcion = jsonObject.SelectToken("message").ToString
            respuesta_orden.trace = reference

            mensajeQR("Error en compra: " & exWeb.Message)
            mensajeQR("Status Code: " & exWeb.Status)
            If exWeb.Response IsNot Nothing Then mensajeQR("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("message").ToString)

        Catch ex As Exception
            mensajeQR(ex.Message)
            respuesta_orden.estado = Estado.UNKNOWN
            respuesta_orden.descripcion = "ERROR DESCONOCIDO"
            respuesta_orden.fechahora_respuesta = Now
            respuesta_orden.trace = reference
        Finally
            If mywebresponse IsNot Nothing Then mywebresponse.Close()
            If streamResponse IsNot Nothing Then streamResponse.Close()
            If streamRead IsNot Nothing Then streamRead.Close()

        End Try
        Return respuesta_orden
    End Function


    Private Shared Sub ReadCallback(asynchronousResult As IAsyncResult)
        Dim myRequestState As RequestState = CType(asynchronousResult.AsyncState, RequestState)
        Dim myWebRequest As WebRequest = myRequestState.request
        ' End the request.
        Dim streamResponse As Stream = myWebRequest.EndGetRequestStream(asynchronousResult)
        ' Create a string that is to be posted to the uri.

        Dim postData As String = myRequestState.json
        Dim encoder As New ASCIIEncoding()
        ' Convert  the string into a byte array.
        If postData <> "" Then
            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
            ' Write the data to the stream.
            streamResponse.Write(byteArray, 0, postData.Length)
        End If
        streamResponse.Close()
        ' Allow the main thread to resume.
        allDone.Set()
    End Sub ' ReadCallback 

    Private Shared Sub ReadCallback2(asynchronousResult As IAsyncResult)
        Dim myRequestState As RequestState = CType(asynchronousResult.AsyncState, RequestState)
        Dim myWebRequest As WebRequest = myRequestState.request

        Dim streamResponse As Stream = myWebRequest.EndGetRequestStream(asynchronousResult)
        streamResponse.Close()
        ' Allow the main thread to resume.
        allDone.Set()
    End Sub ' ReadCallback 

End Class
