
Imports System
Imports System.Net
Imports System.Text
Imports System.IO
Imports System.Threading
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports TjComun.Encriptador


Public Class RequestState
    ' This class stores the request state of the request.
    Public request As HttpWebRequest
    Public json As String = ""


    Public Sub New()
        request = Nothing
    End Sub ' New
End Class ' RequestState

Public Class MensajeCliente
    Property Texto As String
    Property Fecha As Date
    Property Tipo As TipoMensaje
End Class


Public Structure RESPUESTA_PEI
    Dim CodigoRespuesta As Integer
    Dim MensajeRespuesta As String

End Structure

Public Enum TipoMensaje
    Informacion = 0
    [Error] = 10
    Debug = 20
End Enum

Public Class PEI
    Public versionLIBPEI As String = "21"
    Public Event Mensaje(msj As MensajeCliente)

    Public Shared allDone As New ManualResetEvent(False)     '   Public Shared allDone As New ManualResetEvent(False)

    Private timeOutGeneral As Integer = 45000     '--- corregido 03/08/21, estaba 30000   '--- 30 segundos  lo puse en 45 segundos 
    Dim token As String
    Property direccion As String
    Property comercio As String
    Property nomarc As String
    Property nomvta As String

    Public Sub Log(texto As String)
        Dim objStreamWriter As StreamWriter
        'Pass the file path and the file name to the StreamWriter constructor.
        objStreamWriter = New StreamWriter("C:\Tarjetas\AgentePei\Log\" & nomarc, True)
        objStreamWriter.WriteLine(String.Format("{0:dd/MM HH:mm:ss} {1}", Now, texto))
        objStreamWriter.Close()
    End Sub


    Private Sub mensajePei(s As String)
        Dim msj As New MensajeCliente() With {.Fecha = Now, .Texto = s}
        msj.Tipo = TipoMensaje.Informacion
        Log(s)

        RaiseEvent Mensaje(msj)

    End Sub
    ''' <summary>
    ''' Genera cadenas de 38 caracteres aleatorias.
    ''' </summary>
    ''' <returns></returns>
    Private Function Requerimiento() As String
        Dim obj As New Random()
        Dim posibles As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"
        Dim longitud As Integer = posibles.Length

        Dim letra As Char
        Dim longitudnuevacadena As Integer = 35
        Dim nuevacadena As String = ""
        For i As Integer = 0 To longitudnuevacadena - 1
            letra = posibles(obj.[Next](longitud))
            nuevacadena += letra.ToString()
        Next

        Return nuevacadena

    End Function


    Private Sub GrabarRespuesta(resp As Respuesta)
        Dim objStreamWriter As New StreamWriter("C:\Tarjetas\RespuestasPei\" & nomvta, False)
        Try
            objStreamWriter.WriteLine(resp.ToString)
        Catch ex As Exception
        Finally
            objStreamWriter.Close()
        End Try
    End Sub


    Public Function Operar(s As String, oper As String, idcom As String, trace As String) As Respuesta   '--- trx, tipoop, comercio, trace
        Dim resp As New Respuesta
        Dim laResp As String = ""
        Try
            token = Iniciar_Sesion(idcom)

            If token <> "" Then              'inicio sesion correctamente

                Try
                    If oper = "c" Then           ' trx.tipoOperacion = lib_PEI.Concepto.COMPRA_DE_BIENES Then
                        Log("Compra")
                        Dim trx As New lib_PEI.Operacion_Compra
                        trx.LeerArgumento(s)
                        resp = Compra(trx)
                        '--- 04/02/2021 ------ UNICA CORRECCION - DEVUELVE LA RESPUESTA, QUE SE GRABA COMO .VTA DESPUES 
                        '                      DE LA COMPRA, NO IMPORTA COMO CIERRA LA SESION                           
                        '                      ANTES ESTABA ABAJO, Y SI EL CIERRE DE SESION DABA ERROR, NO GRABABA      
                        '                      NINGUNA RESPUESTA Y EL CLIENTE LO PASABA DE NUEVO                        
                        GrabarRespuesta(resp)



                    ElseIf oper = "dt" Then      ' trx.tipoOperacion = lib_PEI.Concepto.DEVOLUCION Then
                        Log("Dev total")
                        Dim trx As New lib_PEI.Operacion_Devolucion
                        trx.LeerArgumento(s)
                        resp = Devolucion_Total(trx)

                    ElseIf oper = "dp" Then
                        Log("Dev Parcial")
                        Dim trx As New lib_PEI.Operacion_Devolucion_Parcial
                        trx.LeerArgumento(s)
                        resp = Devolucion_Parcial(trx)

                    ElseIf oper = "cb" Then
                        Log("Extraccion")
                        Dim trx As New lib_PEI.Operacion_Compra
                        trx.LeerArgumento(s)
                        resp = Extraccion(trx)

                    ElseIf oper = "co" Then
                        Log("Consulta")
                        Dim trx As New lib_PEI.Operacion_Consulta
                        trx.LeerArgumento(s)
                        resp = Consulta(trx)
                    End If

                    Cerrar_Sesion()

                Catch
                    '--- ESTE TRY CATCH LO AGREGUE PORQUE DIO ERROR LA COMPRA Y NUNCA SE CERRO LA SESION Y QUEDO ABIERTA  
                    '--- Y A PARTIR DE AHI NO PUDE MAS COMPRAR, ENTONCES DE ESTA FORMA PASA SIEMPRE POR  Cerrar_Sesion()  
                    mensajePei("Try-Catch ex (operar - Dentro de Token <> )")
                    mensajePei(mensajesRepuesta.ERROR_DESCONOCIDO.ToString)
                    resp.descRespuesta = mensajesRepuesta.ERROR_DESCONOCIDO
                    resp.trace = trace.Split("-")(1)
                    resp.descripcion = "ERROR DESCONOCIDO"
                    resp.fechahora_respuesta = Now
                End Try

            Else

                mensajePei("No se puedo INICIAR LA SESION - EL HOST REMOTO NO RESPONDE")
                resp.descRespuesta = mensajesRepuesta.FALTA_TOKEN
                resp.trace = trace.Split("-")(1)
                resp.descripcion = "EL HOST REMOTO NO RESPONDE."
                resp.fechahora_respuesta = Now
                'no pudo iniciar sesion
            End If

        Catch ex As Exception
            mensajePei("Try-Catch ex (operar)")
            mensajePei(mensajesRepuesta.ERROR_DESCONOCIDO.ToString & " " & ex.Message & " " & ex.StackTrace)
            resp.descRespuesta = mensajesRepuesta.ERROR_DESCONOCIDO
            resp.trace = trace.Split("-")(1)
            resp.descripcion = "ERROR DESCONOCIDO"
            resp.fechahora_respuesta = Now
        End Try
        'GrabarRespuesta(resp)
        Return resp

    End Function


    Public Function DevolucionTotalPorError(s As String, oper As String, idcom As String, trace As String) As Respuesta   '--- trx, tipoop, comercio, trace
        Dim resp As New Respuesta
        Try
            mensajePei("**********   VA A REALIZAR DEVOLUCION TOTAL   **********")
            token = Iniciar_Sesion(idcom)

            If token <> "" Then
                Try
                    Log("Dev total")
                    Dim trx As New lib_PEI.Operacion_Devolucion
                    trx.LeerArgumento(s)
                    resp = Devolucion_Total(trx)

                    Cerrar_Sesion()
                Catch
                    '--- ESTE TRY CATCH LO AGREGUE PORQUE DIO ERROR LA COMPRA Y NUNCA SE CERRO LA SESION Y QUEDO ABIERTA  
                    '--- Y A PARTIR DE AHI NO PUDE MAS COMPRAR, ENTONCES DE ESTA FORMA PASA SIEMPRE POR  Cerrar_Sesion()  
                    mensajePei("DevolucionTotalPorError  -  Try-Catch ex (Dentro de Token <> )")
                    mensajePei(mensajesRepuesta.ERROR_DESCONOCIDO.ToString)
                    resp.descRespuesta = mensajesRepuesta.ERROR_DESCONOCIDO
                    resp.trace = trace.Split("-")(1)
                    resp.descripcion = "ERROR DESCONOCIDO"
                    resp.fechahora_respuesta = Now
                End Try

            Else

                mensajePei("DevolucionTotalPorError  -  No se puedo INICIAR LA SESION - EL HOST REMOTO NO RESPONDE")
                resp.descRespuesta = mensajesRepuesta.FALTA_TOKEN
                resp.trace = trace.Split("-")(1)
                resp.descripcion = "EL HOST REMOTO NO RESPONDE."
                resp.fechahora_respuesta = Now
                'no pudo iniciar sesion
            End If

        Catch ex As Exception
            mensajePei("DevolucionTotalPorError  -  Try-Catch ex (operar)")
            mensajePei(mensajesRepuesta.ERROR_DESCONOCIDO.ToString & " " & ex.Message & " " & ex.StackTrace)
            resp.descRespuesta = mensajesRepuesta.ERROR_DESCONOCIDO
            resp.trace = trace.Split("-")(1)
            resp.descripcion = "ERROR DESCONOCIDO"
            resp.fechahora_respuesta = Now
        End Try
        GrabarRespuesta(resp)
        Return resp

    End Function





    Public Function Pagar(trx As Operacion_Compra) As Respuesta
        Dim resp As New Respuesta
        Dim laResp As String = ""
        Try
            'nomarc = trx.idTerminal & "-" & Now.Date.ToString("yyyyMMdd") & ".TXT"
            'nomarc = "LogPEI-" & Now.Date.ToString("yyyyMMdd") & ".TXT"
            nomarc = "LogPEI-" & Now.Date.ToString("yyyyMMdd") & "-" & trx.idTerminal & ".TXT"

            nomvta = trx.idTerminal & ".vta"

            token = Iniciar_Sesion(trx.idComercio)

            If token <> "" Then 'inicio sesion correctamente
                resp = Compra(trx)
                GrabarRespuesta(resp)

                Cerrar_Sesion()
            Else
                resp.descRespuesta = mensajesRepuesta.FALTA_TOKEN
                resp.trace = trx.traceTrxComercio
                resp.descripcion = "EL HOST REMOTO NO RESPONDE."
                resp.fechahora_respuesta = Now
                'no pudo iniciar sesion
            End If
        Catch ex As Exception
            mensajePei(mensajesRepuesta.ERROR_DESCONOCIDO.ToString & " " & ex.Message & " " & ex.StackTrace)
            resp.descRespuesta = mensajesRepuesta.ERROR_DESCONOCIDO
            resp.trace = trx.traceTrxComercio
            resp.descripcion = "ERROR DESCONOCIDO"
            resp.fechahora_respuesta = Now
        End Try
        'GrabarRespuesta(resp)
        Return resp

    End Function


    Public Function Retirar(trx As Operacion_Compra) As Respuesta
        Dim resp As New Respuesta
        Try
            'nomarc = trx.idTerminal & "-" & Now.Date.ToString("yyyyMMdd") & ".TXT"
            'nomarc = "LogPEI-" & Now.Date.ToString("yyyyMMdd") & ".TXT"
            nomarc = "LogPEI-" & Now.Date.ToString("yyyyMMdd") & "-" & trx.idTerminal & ".TXT"

            token = Iniciar_Sesion(trx.idComercio)

            If token <> "" Then 'inicio sesion correctamente
                resp = Extraccion(trx)
                Cerrar_Sesion()

            Else

                resp.descRespuesta = mensajesRepuesta.FALTA_TOKEN
                resp.trace = trx.traceTrxComercio
                resp.descripcion = "EL HOST REMOTO NO RESPONDE."
                resp.fechahora_respuesta = Now

                'no pudo iniciar sesion
            End If

        Catch ex As Exception
            resp.descRespuesta = mensajesRepuesta.ERROR_DESCONOCIDO


        End Try
        Return resp

    End Function

    Public Function DevolverTotal(trx As Operacion_Devolucion) As Respuesta
        Dim resp As New Respuesta
        Try
            'nomarc = trx.idTerminal & "-" & Now.Date.ToString("yyyyMMdd") & ".TXT"
            'nomarc = "LogPEI-" & Now.Date.ToString("yyyyMMdd") & ".TXT"
            nomarc = "LogPEI-" & Now.Date.ToString("yyyyMMdd") & "-" & trx.idTerminal & ".TXT"

            nomvta = trx.idTerminal & ".vta"

            token = Iniciar_Sesion(trx.idComercio)
            If token <> "" Then 'inicio sesion correctamente

                resp = Devolucion_Total(trx)
                Cerrar_Sesion()

            Else
                resp.descRespuesta = mensajesRepuesta.FALTA_TOKEN
                'no pudo iniciar sesion
            End If
        Catch ex As Exception
            resp.descRespuesta = mensajesRepuesta.ERROR_DESCONOCIDO


        End Try
        GrabarRespuesta(resp)
        Return resp

    End Function

    Public Function DevolverPacial(trx As Operacion_Devolucion_Parcial) As Respuesta
        Dim resp As New Respuesta
        Try
            'nomarc = trx.idTerminal & "-" & Now.Date.ToString("yyyyMMdd") & ".TXT"
            'nomarc = "LogPEI-" & Now.Date.ToString("yyyyMMdd") & ".TXT"
            nomarc = "LogPEI-" & Now.Date.ToString("yyyyMMdd") & "-" & trx.idTerminal & ".TXT"

            nomvta = trx.idTerminal & ".vta"

            token = Iniciar_Sesion(trx.idComercio)
            If token <> "" Then 'inicio sesion correctamente

                resp = Devolucion_Parcial(trx)
                Cerrar_Sesion()

            Else
                resp.descRespuesta = mensajesRepuesta.FALTA_TOKEN
                'no pudo iniciar sesion
            End If
        Catch ex As Exception
            resp.descRespuesta = mensajesRepuesta.ERROR_DESCONOCIDO


        End Try
        GrabarRespuesta(resp)
        Return resp

    End Function




    Public Function ConsultaTotal(trx As Operacion_Consulta) As Respuesta
        Dim resp As New Respuesta
        Try
            'nomarc = trx.idTerminal & "-" & Now.Date.ToString("yyyyMMdd") & ".TXT"
            'nomarc = "LogPEI-" & Now.Date.ToString("yyyyMMdd") & ".TXT"
            nomarc = "LogPEI-" & Now.Date.ToString("yyyyMMdd") & "-" & trx.idTerminal & ".TXT"

            nomvta = trx.idTerminal & ".vta"

            token = Iniciar_Sesion(trx.idComercio)
            If token <> "" Then 'inicio sesion correctamente
                resp = Consulta(trx)
                Cerrar_Sesion()
            Else
                resp.descRespuesta = mensajesRepuesta.FALTA_TOKEN
                'no pudo iniciar sesion
            End If
        Catch ex As Exception
            resp.descRespuesta = mensajesRepuesta.ERROR_DESCONOCIDO
        End Try
        GrabarRespuesta(resp)
        Return resp

    End Function











    Private Function Iniciar_Sesion(comercio As String) As String


        Dim controlTableAdapter As New DatosTjTableAdapters.CONTROLTableAdapter
        Dim contador As Int32
        Dim hora As String
        Dim contadorDB = controlTableAdapter.GetContador
        Try
            contador = contadorDB.Item(0).contador + 1
            hora = Now.ToShortTimeString
            controlTableAdapter.ActualizarControl(contador, hora)
        Catch

        End Try


        mensajePei("-------------------------- INICIO SESION PEI -----------------------------")
        ServicePointManager.SecurityProtocol = 3072
        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or My.Computer.Name = "BACKTARJETAS" Then
            '--- HOMOLOGACION  
            direccion = "https://h.api.redlink.com.ar/redlink/homologacion/enlacepagosseg/0/0/40/sesion/"
            comercio = 385
        Else
            '--- PRODUCCION  
            direccion = "https://api.redlink.com.ar/redlink/produccion/enlacepagosseg/0/0/40/sesion/"
        End If
        Dim myWebRequest As HttpWebRequest = WebRequest.Create(direccion & comercio)

        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        myWebRequest.Timeout = timeOutGeneral
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "POST"
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"
        myRequestState.request.Headers.Add("cliente: 200.2.127.227")
        myRequestState.request.Headers.Add("requerimiento: SESION_CORMORAN_" & Now.Date & "_" & Now.ToShortTimeString)

        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or My.Computer.Name = "BACKTARJETAS" Then
            '--- HOMOLOGACION   
            myRequestState.request.Headers.Add("x-ibm-client-id: e003d9ed-8474-4639-bcce-4ca8a26567b0")
            myRequestState.json = "{""credenciales"":{""contrasena"":""Prueba123"",""usuario"":""sv_30521387862_pei""}}"   '--- HOMOLOGACION  
            '--- ESTO ESTABA EN LA HOMOLOGACION, LO HIZO MAURO   
            'myRequestState.request.Headers.Add("x-ibm-client-id: eb1741fd-7117-4554-93e7-361804b34030")
        Else
            '--- PRODUCCION   
            myRequestState.request.Headers.Add("x-ibm-client-id: a029f249-5b98-4851-b084-c2b5c44651f1")
            myRequestState.json = "{""credenciales"":{""contrasena"":""vTC*ZQ5gPcs3gH+TDCm7"",""usuario"":""sv_30521387862_pei""}}"   '--- PRODUCCION  
        End If

        mensajePei(myRequestState.request.Method & " " & myRequestState.request.RequestUri.ToString & vbNewLine &
                       "                <HEADERS>" & vbNewLine &
                       "                    " & "content-type: " & myRequestState.request.Headers.Item("content-type") & vbNewLine &
                       "                    " & "accept: " & myRequestState.request.Headers.Item("accept") & vbNewLine &
                       "                    " & "x-ibm-client-id: " & myRequestState.request.Headers.Item("x-ibm-client-id") & vbNewLine &
                       "                    " & "requerimiento: " & myRequestState.request.Headers.Item("requerimiento") & vbNewLine &
                       "                    " & "cliente: " & myRequestState.request.Headers.Item("cliente") & vbNewLine &
                       "                <DATA>" & vbNewLine &
                       "                    " & myRequestState.json)
        Try
            Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback, myRequestState), IAsyncResult)
            'mensajePei("Antes allDone.WaitOne()")
            ' Pause the current thread until the async operation completes.
            allDone.WaitOne()
        Catch
            mensajePei("SE FUE POR ERROR EN TRY CATCH")
        End Try


        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject
        Dim jsontext As String = ""


        Try
            contador = 0
            hora = Now.ToShortTimeString
            controlTableAdapter.ActualizarControl(contador, hora)
        Catch

        End Try

        Try
            mensajePei("-------------------------- RESPUESTA INICIO SESION -----------------------------")
            mensajePei("*** 1 ***")
            mywebresponse = myWebRequest.GetResponse()

            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)
            Dim readBuff(256) As [Char]
            mensajePei("*** 2 ***")
            Dim count As Integer = streamRead.Read(readBuff, 0, 256)
            'mensajePei("Count 1: " & count.ToString)

            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                mensajePei(outputData)
                jsonObject = JObject.Parse(outputData)
                count = streamRead.Read(readBuff, 0, 256)
            End While

            'mensajePei("Count 2: " & count.ToString)

            If jsonObject IsNot Nothing Then
                jsontext = jsonObject.SelectToken("token").ToString
                mensajePei("jsontext: " & jsontext)
            End If

            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()
            Return jsontext
        Catch exWeb As WebException
            mensajePei("Try-Catch exWeb (Iniciar_Sesion)")
            mensajePei("Error de inicio de sesión: " & exWeb.Response.Headers("exception-message"))
            Return ""

        Catch ex As Exception
            mensajePei("Try-Catch ex (Iniciar_Sesion)")
            mensajePei(ex.Message)
            Return ""

        Finally
            If Not mywebresponse Is Nothing Then mywebresponse.Close()
        End Try

    End Function


    Private Function Compra(trx As Operacion_Compra) As Respuesta
        Dim respuesta_compra As New Respuesta
        '--- IMPRIME EN EL LOG   
        mensajePei("-------------------------- COMPRA PEI -----------------------------")
        ' Create a new request.
        ServicePointManager.SecurityProtocol = 3072
        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or My.Computer.Name = "BACKTARJETAS" Then
            '--- HOMOLOGACION  
            direccion = "https://h.api.redlink.com.ar/redlink/homologacion/enlacepagosseg/0/0/40/pagos/sinbilletera/td/mediopresente/bandamagnetica"
        Else
            '--- PRODUCCION  
            direccion = "https://api.redlink.com.ar/redlink/produccion/enlacepagosseg/0/0/40/pagos/sinbilletera/td/mediopresente/bandamagnetica"
        End If
        Dim myWebRequest As HttpWebRequest = WebRequest.Create(direccion)


        myWebRequest.Timeout = timeOutGeneral
        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "POST"
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"
        myRequestState.request.Headers.Add("cliente: 200.2.127.227")
        myRequestState.request.Headers.Add("requerimiento: PAGO_CORMORAN_" & Now.Date & "_" & Now.ToShortTimeString)

        Dim clave As String = ""
        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or My.Computer.Name = "BACKTARJETAS" Then
            '--- HOMOLOGACION  
            myRequestState.request.Headers.Add("x-ibm-client-id: e003d9ed-8474-4639-bcce-4ca8a26567b0")
            clave = "DDB7A35EB166273505190FBF13390FE00945C602A58E25F06BB42DCD06AE8509" 'GENERADA CUANDO CREE LA API.
            '--- CLAVE DE MAURO 
            'myRequestState.request.Headers.Add("x-ibm-client-id: eb1741fd-7117-4554-93e7-361804b34030")
            'Dim clave As String = "17D42285325EB34060E29E82AABD23650C90BCF31C38471E3920DD687ACF7AA2" 'GENERADA CUANDO CREE LA API.
        Else
            '--- PRODUCCION   
            myRequestState.request.Headers.Add("x-ibm-client-id: a029f249-5b98-4851-b084-c2b5c44651f1")
            clave = "E3448399448A33EF859EB95B05E705A2291BCFBA6322D46E4908604E2BF6528A" 'GENERADA CUANDO CREE LA API.
        End If

        myRequestState.request.Headers.Add("token: " & token)

        Dim encriptador As New TjComun.crypto

        myRequestState.json = "{""pago"":{""posEntryMode"":""" & EntryMode.Ban &
                               """,""tracks"":{""track1"":""" & encriptador.AES_Encrypt(trx.track1, clave) &
                               """,""track2"":""" & encriptador.AES_Encrypt(trx.track2, clave) &
                               """},""numero"":""" & trx.ultimos &
                               """,""titularDocumento"":""" & trx.Documentotitular &
                               """,""idCanal"":""" & trx.idCanal &
                               """,""idTerminal"":""" & trx.idTerminal &
                               """,""idReferenciaTrxComercio"":""" & trx.traceTrxComercio &
                               """,""idReferenciaOperacionComercio"":""" & trx.idReferenciaOperacionComercio &
                               """,""importe"":" & CInt(trx.importe) &
                               ",""moneda"":""" & trx.moneda.ToString &
                               """,""codigoSeguridad"":""" & encriptador.AES_Encrypt(trx.codigoSeguridad, clave) &
                               """,""concepto"":""" & Concepto.COMPRA_DE_BIENES.ToString &
                               """}}"

        '--- IMPRIME EN EL LOG   
        mensajePei(myRequestState.request.Method & " " & myRequestState.request.RequestUri.ToString & vbNewLine &
                   "                <HEADERS>" & vbNewLine &
                   "                    " & "content-type: " & myRequestState.request.Headers.Item("content-type") & vbNewLine &
                   "                    " & "accept: " & myRequestState.request.Headers.Item("accept") & vbNewLine &
                   "                    " & "x-ibm-client-id: " & myRequestState.request.Headers.Item("x-ibm-client-id") & vbNewLine &
                   "                    " & "requerimiento: " & myRequestState.request.Headers.Item("requerimiento") & vbNewLine &
                   "                    " & "cliente: " & myRequestState.request.Headers.Item("cliente") & vbNewLine &
                   "                    " & "token: " & myRequestState.request.Headers.Item("token") & vbNewLine &
                   "                <DATA>" & vbNewLine &
                   JObject.Parse(myRequestState.json).ToString())

        '--- IMPRIME EN EL LOG   
        mensajePei(myRequestState.json)
        Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject
        Dim jsontext As String = ""
        Try
            mensajePei("-------------------------- RESPUESTA COMPRA -----------------------------")
            mensajePei("*** 1 ***")
            mywebresponse = myWebRequest.GetResponse()

            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)
            Dim readBuff(256) As [Char]
            mensajePei("*** 2 ***")
            Dim count As Integer = streamRead.Read(readBuff, 0, 256)








            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                mensajePei(outputData)
                jsonObject = JObject.Parse(outputData)
                count = streamRead.Read(readBuff, 0, 256)
            End While

            'mensajePei("Count 2: " & count.ToString)

            If jsonObject IsNot Nothing Then
                mensajePei("Respuesta: " & mensajesRepuesta.APROBADA.ToString)
                mensajePei("Operacion: " & jsonObject.SelectToken("tipoOperacion").ToString)
                mensajePei("ID Operacion: " & jsonObject.SelectToken("idOperacion").ToString)
                mensajePei("ID Operacion Origen: " & jsonObject.SelectToken("idOperacionOrigen").ToString)
                mensajePei("Nro Referencia Bancaria: " & jsonObject.SelectToken("numeroReferenciaBancaria").ToString)
                mensajePei("Fecha Operación: " & jsonObject.SelectToken("fecha").ToString)
                mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))

                respuesta_compra.descRespuesta = mensajesRepuesta.APROBADA
                respuesta_compra.id_operacion = jsonObject.SelectToken("idOperacion").ToString
                respuesta_compra.tipoOperacion = jsonObject.SelectToken("tipoOperacion").ToString
                respuesta_compra.nro_ref_bancaria = jsonObject.SelectToken("numeroReferenciaBancaria").ToString
                respuesta_compra.fechahora_respuesta = jsonObject.SelectToken("fecha").ToString
                respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)

            Else
                mensajePei("Respuesta: " & mensajesRepuesta.NO_APROBADA.ToString)
                mensajePei("Descripcion: NO SE PUDO PARSEAR LA RESPUESTA.")
                mensajePei("Fecha Operación: " & Now)
                mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))

                respuesta_compra.descRespuesta = mensajesRepuesta.NO_APROBADA
                respuesta_compra.descripcion = "NO SE PUDO PARSEAR LA RESPUESTA."
                respuesta_compra.fechahora_respuesta = Now
                respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)
            End If
            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()


        Catch exWeb As WebException

            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)

            mensajePei("Try-Catch exWeb (Compra)")
            mensajePei(jsonObject.ToString)
            mensajePei("Error en compra: " & exWeb.Message)
            mensajePei("Status Code: " & exWeb.Status)
            mensajePei("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString)
            mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))

            respuesta_compra.descRespuesta = DirectCast([Enum].Parse(GetType(mensajesRepuesta), jsonObject.SelectToken("codigo").ToString), mensajesRepuesta)

            'respuesta_compra.descRespuesta = jsonObject.SelectToken("codigo").ToString
            respuesta_compra.descripcion = jsonObject.SelectToken("descripcion").ToString
            respuesta_compra.fechahora_respuesta = Now
            respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)

        Catch ex As Exception
            mensajePei("Try-Catch ex (Compra)")
            mensajePei(ex.Message)
            respuesta_compra.descRespuesta = mensajesRepuesta.ERROR_DESCONOCIDO
            respuesta_compra.descripcion = "ERROR_DESCONOCIDO"
            respuesta_compra.fechahora_respuesta = Now
            respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)
        Finally
            If Not mywebresponse Is Nothing Then mywebresponse.Close()
        End Try
        System.Threading.Thread.Sleep(500)
        Return respuesta_compra
    End Function


    Private Function Devolucion_Total(trx As Operacion_Devolucion) As Respuesta
        Dim respuesta_compra As New Respuesta
        mensajePei("-------------------------- DEVOLUCION TOTAL PEI -----------------------------")
        ' Create a new request.
        ServicePointManager.SecurityProtocol = 3072

        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or My.Computer.Name = "BACKTARJETAS" Then
            '--- HOMOLOGACION  
            'Dim myWebRequest As HttpWebRequest = WebRequest.Create("https://h.api.redlink.com.ar/redlink/homologacion/enlacepagosseg/0/0/40/pagos/sinbilletera/td/mediopresente/bandamagnetica/devolucion/total")
            direccion = "https://h.api.redlink.com.ar/redlink/homologacion/enlacepagosseg/0/0/40/pagos/sinbilletera/td/mediopresente/bandamagnetica/devolucion/total"
        Else
            '--- PRODUCCION  
            'Dim myWebRequest As HttpWebRequest = WebRequest.Create(direccion & "pagos/sinbilletera/td/mediopresente/bandamagnetica/devolucion/total")
            direccion = "https://api.redlink.com.ar/redlink/produccion/enlacepagosseg/0/0/40/pagos/sinbilletera/td/mediopresente/bandamagnetica/devolucion/total"
        End If
        Dim myWebRequest As HttpWebRequest = WebRequest.Create(direccion)

        myWebRequest.Timeout = timeOutGeneral


        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "POST"
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"
        myRequestState.request.Headers.Add("cliente: 200.2.127.227")
        myRequestState.request.Headers.Add("requerimiento: DEV_CORMORAN_" & Now.Date & "_" & Now.ToShortTimeString)

        Dim clave As String = ""
        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or My.Computer.Name = "BACKTARJETAS" Then
            '--- HOMOLOGACION  
            myRequestState.request.Headers.Add("x-ibm-client-id: e003d9ed-8474-4639-bcce-4ca8a26567b0")
            clave = "DDB7A35EB166273505190FBF13390FE00945C602A58E25F06BB42DCD06AE8509" 'GENERADA CUANDO CREE LA API.
            '--- CLAVE DE MAURO 
            'myRequestState.request.Headers.Add("x-ibm-client-id: eb1741fd-7117-4554-93e7-361804b34030")
            'Dim clave As String = "17D42285325EB34060E29E82AABD23650C90BCF31C38471E3920DD687ACF7AA2" 'GENERADA CUANDO CREE LA API.
        Else
            '--- PRODUCCION   
            myRequestState.request.Headers.Add("x-ibm-client-id: a029f249-5b98-4851-b084-c2b5c44651f1")
            clave = "E3448399448A33EF859EB95B05E705A2291BCFBA6322D46E4908604E2BF6528A" 'GENERADA CUANDO CREE LA API.
        End If

        myRequestState.request.Headers.Add("token: " & token)

        Dim encriptador As New TjComun.crypto


        myRequestState.json = "{""devolucion"":{""posEntryMode"":""" & EntryMode.Ban &
                               """,""tracks"":{""track1"":""" & encriptador.AES_Encrypt(trx.track1, clave) &
                               """,""track2"":""" & encriptador.AES_Encrypt(trx.track2, clave) &
                               """},""numero"":""" & trx.ultimos &
                               """,""titularDocumento"":""" & trx.Documentotitular &
                               """,""idCanal"":""" & trx.idCanal &
                               """,""idTerminal"":""" & trx.idTerminal &
                               """,""idReferenciaTrxComercio"":""" & trx.traceTrxComercio & '^[a-zA-Z0-9$&()-]{1,40}$
                               """,""idReferenciaOperacionComercio"":""" & trx.idReferenciaOperacionComercio & '^[a-zA-Z0-9$&()-]{1,12}$
                               """,""codigoSeguridad"":""" & encriptador.AES_Encrypt(trx.codigoSeguridad, clave) &
                               """,""operacionOrigen"":{""idPago"":""" & trx.idPagoOriginal &
                               """}}}"


        mensajePei(myRequestState.request.Method & " " & myRequestState.request.RequestUri.ToString & vbNewLine &
                   "                <HEADERS>" & vbNewLine &
                   "                    " & "content-type: " & myRequestState.request.Headers.Item("content-type") & vbNewLine &
                   "                    " & "accept: " & myRequestState.request.Headers.Item("accept") & vbNewLine &
                   "                    " & "x-ibm-client-id: " & myRequestState.request.Headers.Item("x-ibm-client-id") & vbNewLine &
                   "                    " & "requerimiento: " & myRequestState.request.Headers.Item("requerimiento") & vbNewLine &
                   "                    " & "cliente: " & myRequestState.request.Headers.Item("cliente") & vbNewLine &
                   "                    " & "token: " & myRequestState.request.Headers.Item("token") & vbNewLine &
                   "                <DATA>" & vbNewLine &
                   JObject.Parse(myRequestState.json).ToString())
        '"                              
        mensajePei(myRequestState.json)
        Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject
        Dim jsontext As String = ""
        Try


            mensajePei("-------------------------- RESPUESTA DEVOLUCION TOTAL -----------------------------")

            mywebresponse = myWebRequest.GetResponse()

            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)
            Dim readBuff(256) As [Char]
            Dim count As Integer = streamRead.Read(readBuff, 0, 256)



            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                mensajePei(outputData)
                jsonObject = JObject.Parse(outputData)
                count = streamRead.Read(readBuff, 0, 256)
            End While

            If jsonObject IsNot Nothing Then
                mensajePei("Respuesta Devolucion Total: " & mensajesRepuesta.APROBADA.ToString)
                mensajePei("Operacion: " & jsonObject.SelectToken("tipoOperacion").ToString)
                mensajePei("ID Operacion: " & jsonObject.SelectToken("idOperacion").ToString)
                mensajePei("ID Operacion Origen: " & jsonObject.SelectToken("idOperacionOrigen").ToString)
                mensajePei("Nro Referencia Bancaria: " & jsonObject.SelectToken("numeroReferenciaBancaria").ToString)
                mensajePei("Fecha Operación: " & jsonObject.SelectToken("fecha").ToString)


                respuesta_compra.id_operacion = jsonObject.SelectToken("idOperacion").ToString
                respuesta_compra.tipoOperacion = jsonObject.SelectToken("tipoOperacion").ToString
                respuesta_compra.nro_ref_bancaria = jsonObject.SelectToken("numeroReferenciaBancaria").ToString
                respuesta_compra.fechahora_respuesta = jsonObject.SelectToken("fecha").ToString
                respuesta_compra.descRespuesta = mensajesRepuesta.APROBADA
                respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)

            Else
                mensajePei("Respuesta Devolucion Total: " & mensajesRepuesta.NO_APROBADA.ToString)
                mensajePei("Descripcion: Status Description: NO SE PUDO PARSEAR LA RESPUESTA.")
                mensajePei("Fecha Operación: " & Now)

                respuesta_compra.descRespuesta = mensajesRepuesta.NO_APROBADA
                respuesta_compra.descripcion = "Status Description: NO SE PUDO PARSEAR LA RESPUESTA."
                respuesta_compra.fechahora_respuesta = Now
                respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)

            End If
            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()





        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            mensajePei(jsonObject.ToString)
            mensajePei("Error en Devolucion Total: " & exWeb.Message)
            mensajePei("Status Code: " & exWeb.Status)
            mensajePei("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString)
            mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))
            'respuesta_compra.descRespuesta = mensajesRepuesta.NO_APROBADA
            'respuesta_compra.descripcion = "Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString
            'respuesta_compra.fechahora_respuesta = Now


            respuesta_compra.descRespuesta = DirectCast([Enum].Parse(GetType(mensajesRepuesta), jsonObject.SelectToken("codigo").ToString), mensajesRepuesta)
            'respuesta_compra.descRespuesta = jsonObject.SelectToken("codigo").ToString
            respuesta_compra.descripcion = jsonObject.SelectToken("descripcion").ToString
            respuesta_compra.fechahora_respuesta = Now
            respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)


        Catch ex As Exception
            mensajePei(ex.Message)

        Finally
            If Not mywebresponse Is Nothing Then mywebresponse.Close()

        End Try
        Return respuesta_compra
    End Function



    Private Function Devolucion_Parcial(trx As Operacion_Devolucion_Parcial) As Respuesta
        Dim respuesta_compra As New Respuesta
        mensajePei("-------------------------- DEVOLUCION PARCIAL PEI -----------------------------")
        ' Create a new request.
        ServicePointManager.SecurityProtocol = 3072
        'Dim myWebRequest As HttpWebRequest = WebRequest.Create("https://h.api.redlink.com.ar/redlink/homologacion/enlacepagosseg/0/0/40/pagos/sinbilletera/td/mediopresente/bandamagnetica/devolucion/parcial")
        Dim myWebRequest As HttpWebRequest = WebRequest.Create(direccion & "pagos/sinbilletera/td/mediopresente/bandamagnetica/devolucion/parcial")
        myWebRequest.Timeout = timeOutGeneral
        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "POST"
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"
        myRequestState.request.Headers.Add("cliente: 200.2.127.227")
        myRequestState.request.Headers.Add("requerimiento: DEV_CORMORAN_" & Now.Date & "_" & Now.ToShortTimeString)
        myRequestState.request.Headers.Add("x-ibm-client-id: e003d9ed-8474-4639-bcce-4ca8a26567b0")
        '--- CLAVE DE MAURO 
        'myRequestState.request.Headers.Add("x-ibm-client-id: eb1741fd-7117-4554-93e7-361804b34030")
        myRequestState.request.Headers.Add("token: " & token)

        Dim clave As String = "DDB7A35EB166273505190FBF13390FE00945C602A58E25F06BB42DCD06AE8509" 'GENERADA CUANDO CREE LA API.
        '--- LA USO MAURO   
        'Dim clave As String = "17D42285325EB34060E29E82AABD23650C90BCF31C38471E3920DD687ACF7AA2" 'GENERADA CUANDO CREE LA API.
        Dim encriptador As New TjComun.crypto



        myRequestState.json = "{""devolucion"":{""posEntryMode"":""" & EntryMode.Ban &
                               """,""tracks"":{""track1"":""" & encriptador.AES_Encrypt(trx.track1, clave) &
                               """,""track2"":""" & encriptador.AES_Encrypt(trx.track2, clave) &
                               """},""numero"":""" & trx.ultimos &
                               """,""titularDocumento"":""" & trx.Documentotitular &
                               """,""idCanal"":""" & trx.idCanal &
                               """,""idTerminal"":""" & trx.idTerminal &
                               """,""idReferenciaTrxComercio"":""" & trx.traceTrxComercio &
                               """,""idReferenciaOperacionComercio"":""" & trx.idReferenciaOperacionComercio &
                               """,""importe"":" & CInt(trx.importe) &
                               ",""moneda"":""" & trx.moneda.ToString &
                               """,""codigoSeguridad"":""" & encriptador.AES_Encrypt(trx.codigoSeguridad, clave) &
                               """,""operacionOrigen"":{""idPago"":""" & trx.idPagoOriginal &
                               """}}}"


        mensajePei(myRequestState.request.Method & " " & myRequestState.request.RequestUri.ToString & vbNewLine &
                   "                <HEADERS>" & vbNewLine &
                   "                    " & "content-type: " & myRequestState.request.Headers.Item("content-type") & vbNewLine &
                   "                    " & "accept: " & myRequestState.request.Headers.Item("accept") & vbNewLine &
                   "                    " & "x-ibm-client-id: " & myRequestState.request.Headers.Item("x-ibm-client-id") & vbNewLine &
                   "                    " & "requerimiento: " & myRequestState.request.Headers.Item("requerimiento") & vbNewLine &
                   "                    " & "cliente: " & myRequestState.request.Headers.Item("cliente") & vbNewLine &
                   "                    " & "token: " & myRequestState.request.Headers.Item("token") & vbNewLine &
                   "                <DATA>" & vbNewLine &
                   JObject.Parse(myRequestState.json).ToString())
        '"                              
        mensajePei(myRequestState.json)
        Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject
        Dim jsontext As String = ""
        Try


            mensajePei("-------------------------- RESPUESTA DEVOLUCION PARCIAL -----------------------------")

            mywebresponse = myWebRequest.GetResponse()

            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)
            Dim readBuff(256) As [Char]
            Dim count As Integer = streamRead.Read(readBuff, 0, 256)



            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                mensajePei(outputData)
                jsonObject = JObject.Parse(outputData)
                count = streamRead.Read(readBuff, 0, 256)
            End While

            If jsonObject IsNot Nothing Then
                mensajePei("Respuesta: " & mensajesRepuesta.APROBADA.ToString)
                mensajePei("Operacion: " & jsonObject.SelectToken("tipoOperacion").ToString)
                mensajePei("ID Operacion: " & jsonObject.SelectToken("idOperacion").ToString)
                mensajePei("ID Operacion Origen: " & jsonObject.SelectToken("idOperacionOrigen").ToString)
                mensajePei("Nro Referencia Bancaria: " & jsonObject.SelectToken("numeroReferenciaBancaria").ToString)
                mensajePei("Fecha Operación: " & jsonObject.SelectToken("fecha").ToString)


                respuesta_compra.id_operacion = jsonObject.SelectToken("idOperacion").ToString
                respuesta_compra.tipoOperacion = jsonObject.SelectToken("tipoOperacion").ToString
                respuesta_compra.nro_ref_bancaria = jsonObject.SelectToken("numeroReferenciaBancaria").ToString
                respuesta_compra.fechahora_respuesta = jsonObject.SelectToken("fecha").ToString
                respuesta_compra.descRespuesta = mensajesRepuesta.APROBADA
                respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)

            Else
                mensajePei("Respuesta: " & mensajesRepuesta.NO_APROBADA.ToString)
                mensajePei("Descripcion: Status Description: NO SE PUDO PARSEAR LA RESPUESTA.")
                mensajePei("Fecha Operación: " & Now)

                respuesta_compra.descRespuesta = mensajesRepuesta.NO_APROBADA
                respuesta_compra.descripcion = "Status Description: NO SE PUDO PARSEAR LA RESPUESTA."
                respuesta_compra.fechahora_respuesta = Now
                respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)

            End If
            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()




        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            mensajePei(jsonObject.ToString)
            mensajePei("Error en compra: " & exWeb.Message)
            mensajePei("Status Code: " & exWeb.Status)
            mensajePei("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString)
            mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))
            'respuesta_compra.descRespuesta = mensajesRepuesta.NO_APROBADA
            'respuesta_compra.descripcion = "Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString
            'respuesta_compra.fechahora_respuesta = Now


            respuesta_compra.descRespuesta = DirectCast([Enum].Parse(GetType(mensajesRepuesta), jsonObject.SelectToken("codigo").ToString), mensajesRepuesta)
            'respuesta_compra.descRespuesta = jsonObject.SelectToken("codigo").ToString
            respuesta_compra.descripcion = jsonObject.SelectToken("descripcion").ToString
            respuesta_compra.fechahora_respuesta = Now
            respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)


        Catch ex As Exception
            mensajePei(ex.Message)

        Finally
            If Not mywebresponse Is Nothing Then mywebresponse.Close()

        End Try
        Return respuesta_compra
    End Function

    Private Function Extraccion(trx As Operacion_Compra) As Respuesta
        Dim respuesta_compra As New Respuesta
        mensajePei("-------------------------- EXTRACCION PEI -----------------------------")
        ' Create a new request.
        ServicePointManager.SecurityProtocol = 3072
        '--- HOMOLOGACION  
        'Dim myWebRequest As HttpWebRequest = WebRequest.Create("https://h.api.redlink.com.ar/redlink/homologacion/enlacepagosseg/0/0/40/pagos/sinbilletera/td/mediopresente/bandamagnetica")
        '--- PRODUCCION  
        direccion = "https://api.redlink.com.ar/redlink/produccion/enlacepagosseg/0/0/40/pagos/sinbilletera/td/mediopresente/bandamagnetica"
        Dim myWebRequest As HttpWebRequest = WebRequest.Create(direccion)

        myWebRequest.Timeout = timeOutGeneral
        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "POST"
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"
        myRequestState.request.Headers.Add("cliente: 200.2.127.227")
        myRequestState.request.Headers.Add("requerimiento: EXTRAC_CORMORAN_" & Now.Date & "_" & Now.ToShortTimeString)
        myRequestState.request.Headers.Add("x-ibm-client-id: e003d9ed-8474-4639-bcce-4ca8a26567b0")
        '--- CLAVE DE MAURO 
        'myRequestState.request.Headers.Add("x-ibm-client-id: eb1741fd-7117-4554-93e7-361804b34030")
        myRequestState.request.Headers.Add("token: " & token)

        '--- LA USE YO - PRODUCCION  
        Dim clave As String = "E3448399448A33EF859EB95B05E705A2291BCFBA6322D46E4908604E2BF6528A" 'GENERADA CUANDO CREE LA API.
        '--- LA USE YO - HOMOLOGACION  
        'Dim clave As String = "DDB7A35EB166273505190FBF13390FE00945C602A58E25F06BB42DCD06AE8509" 'GENERADA CUANDO CREE LA API.
        '--- LA USO MAURO  
        'Dim clave As String = "17D42285325EB34060E29E82AABD23650C90BCF31C38471E3920DD687ACF7AA2" 'GENERADA CUANDO CREE LA API.
        Dim encriptador As New TjComun.crypto

        myRequestState.json = "{""pago"":{""posEntryMode"":""" & EntryMode.Ban &
                               """,""tracks"":{""track1"":""" & encriptador.AES_Encrypt(trx.track1, clave) &
                               """,""track2"":""" & encriptador.AES_Encrypt(trx.track2, clave) &
                               """},""numero"":""" & trx.ultimos &
                               """,""titularDocumento"":""" & trx.Documentotitular &
                               """,""idCanal"":""" & trx.idCanal &
                               """,""idTerminal"":""" & trx.idTerminal &
                               """,""idReferenciaTrxComercio"":""" & trx.traceTrxComercio & '^[a-zA-Z0-9$&()-]{1,40}$
                               """,""idReferenciaOperacionComercio"":""" & trx.idReferenciaOperacionComercio & '^[a-zA-Z0-9$&()-]{1,12}$
                               """,""importe"":" & CInt(trx.importe) &
                               ",""moneda"":""" & trx.moneda.ToString &
                               """,""codigoSeguridad"":""" & encriptador.AES_Encrypt(trx.codigoSeguridad, clave) &
                               """,""concepto"":""" & Concepto.EXTRACCION.ToString &
                               """}}"


        mensajePei(myRequestState.request.Method & " " & myRequestState.request.RequestUri.ToString & vbNewLine &
                   "                <HEADERS>" & vbNewLine &
                   "                    " & "content-type: " & myRequestState.request.Headers.Item("content-type") & vbNewLine &
                   "                    " & "accept: " & myRequestState.request.Headers.Item("accept") & vbNewLine &
                   "                    " & "x-ibm-client-id: " & myRequestState.request.Headers.Item("x-ibm-client-id") & vbNewLine &
                   "                    " & "requerimiento: " & myRequestState.request.Headers.Item("requerimiento") & vbNewLine &
                   "                    " & "cliente: " & myRequestState.request.Headers.Item("cliente") & vbNewLine &
                   "                    " & "token: " & myRequestState.request.Headers.Item("token") & vbNewLine &
                   "                <DATA>" & vbNewLine &
                    JObject.Parse(myRequestState.json).ToString())
        '"                              
        mensajePei(myRequestState.json)
        Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject
        Dim jsontext As String = ""
        Try

            mensajePei("-------------------------- RESPUESTA EXTRACCION -----------------------------")

            mywebresponse = myWebRequest.GetResponse()

            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)
            Dim readBuff(256) As [Char]
            Dim count As Integer = streamRead.Read(readBuff, 0, 256)



            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                mensajePei(outputData)
                jsonObject = JObject.Parse(outputData)
                count = streamRead.Read(readBuff, 0, 256)
            End While

            If jsonObject IsNot Nothing Then
                mensajePei("Respuesta: " & mensajesRepuesta.APROBADA.ToString)
                mensajePei("Operacion: " & jsonObject.SelectToken("tipoOperacion").ToString)
                mensajePei("ID Operacion: " & jsonObject.SelectToken("idOperacion").ToString)
                mensajePei("ID Operacion Origen: " & jsonObject.SelectToken("idOperacionOrigen").ToString)
                mensajePei("Nro Referencia Bancaria: " & jsonObject.SelectToken("numeroReferenciaBancaria").ToString)
                mensajePei("Fecha Operación: " & jsonObject.SelectToken("fecha").ToString)

                respuesta_compra.id_operacion = jsonObject.SelectToken("idOperacion").ToString
                respuesta_compra.tipoOperacion = jsonObject.SelectToken("tipoOperacion").ToString
                respuesta_compra.nro_ref_bancaria = jsonObject.SelectToken("numeroReferenciaBancaria").ToString
                respuesta_compra.fechahora_respuesta = jsonObject.SelectToken("fecha").ToString
                respuesta_compra.descRespuesta = mensajesRepuesta.APROBADA
                respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)
            Else
                mensajePei("Respuesta: " & mensajesRepuesta.NO_APROBADA.ToString)
                mensajePei("Descripcion: Status Description: NO SE PUDO PARSEAR LA RESPUESTA.")
                mensajePei("Fecha Operación: " & Now)

                respuesta_compra.descRespuesta = mensajesRepuesta.NO_APROBADA
                respuesta_compra.descripcion = "Status Description: NO SE PUDO PARSEAR LA RESPUESTA."
                respuesta_compra.fechahora_respuesta = Now
                respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)

            End If
            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()




        Catch exWeb As WebException

            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            mensajePei("Try-Catch exWeb (Compra + Extraccion)")
            mensajePei(jsonObject.ToString)
            mensajePei("Error en compra: " & exWeb.Message)
            mensajePei("Status Code: " & exWeb.Status)
            mensajePei("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString)
            mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))

            respuesta_compra.descRespuesta = DirectCast([Enum].Parse(GetType(mensajesRepuesta), jsonObject.SelectToken("codigo").ToString), mensajesRepuesta)
            'respuesta_compra.descRespuesta = jsonObject.SelectToken("codigo").ToString
            respuesta_compra.descripcion = jsonObject.SelectToken("descripcion").ToString
            respuesta_compra.fechahora_respuesta = Now
            respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)

        Catch ex As Exception
            mensajePei("Try-Catch ex (Compra + Extraccion)")
            mensajePei(ex.Message)

        Finally
            If Not mywebresponse Is Nothing Then mywebresponse.Close()

        End Try

        Return respuesta_compra
    End Function




    Private Function Consulta(trx As Operacion_Consulta) As Respuesta
        Dim respuesta_compra As New Respuesta
        mensajePei("-------------------------- CONSULTA PEI -----------------------------")
        ' Create a new request.
        ServicePointManager.SecurityProtocol = 3072

        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or My.Computer.Name = "BACKTARJETAS" Then
            '--- HOMOLOGACION  
            direccion = "https://h.api.redlink.com.ar/redlink/homologacion/enlacepagosseg/0/0/40/pagos/sinbilletera/td/mediopresente/bandamagnetica/consulta"
        Else
            '--- PRODUCCION  
            direccion = "https://h.api.redlink.com.ar/redlink/produccion/enlacepagosseg/0/0/40/pagos/sinbilletera/td/mediopresente/bandamagnetica/consulta"
        End If
        Dim myWebRequest As HttpWebRequest = WebRequest.Create(direccion)




        ' Dim myWebRequest As HttpWebRequest = WebRequest.Create(direccion & "pagos/sinbilletera/td/mediopresente/bandamagnetica/consulta")
        myWebRequest.Timeout = timeOutGeneral
        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "POST"
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"
        myRequestState.request.Headers.Add("cliente: 200.2.127.227")
        myRequestState.request.Headers.Add("requerimiento: CONS_CORMORAN_" & Now.Date & "_" & Now.ToShortTimeString)
        myRequestState.request.Headers.Add("x-ibm-client-id: e003d9ed-8474-4639-bcce-4ca8a26567b0")
        '--- CLAVE DE MAURO 
        'myRequestState.request.Headers.Add("x-ibm-client-id: eb1741fd-7117-4554-93e7-361804b34030")

        myRequestState.request.Headers.Add("token: " & token)

        Dim clave As String = ""
        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or My.Computer.Name = "BACKTARJETAS" Then
            '--- HOMOLOGACION  
            myRequestState.request.Headers.Add("x-ibm-client-id: e003d9ed-8474-4639-bcce-4ca8a26567b0")
            clave = "DDB7A35EB166273505190FBF13390FE00945C602A58E25F06BB42DCD06AE8509" 'GENERADA CUANDO CREE LA API.
            '--- CLAVE DE MAURO 
            'myRequestState.request.Headers.Add("x-ibm-client-id: eb1741fd-7117-4554-93e7-361804b34030")
            'Dim clave As String = "17D42285325EB34060E29E82AABD23650C90BCF31C38471E3920DD687ACF7AA2" 'GENERADA CUANDO CREE LA API.
        Else
            '--- PRODUCCION   
            myRequestState.request.Headers.Add("x-ibm-client-id: a029f249-5b98-4851-b084-c2b5c44651f1")
            clave = "E3448399448A33EF859EB95B05E705A2291BCFBA6322D46E4908604E2BF6528A" 'GENERADA CUANDO CREE LA API.
        End If

        Dim encriptador As New TjComun.crypto

        myRequestState.json = "{""consulta"":{""idTerminal"":""" & trx.idTerminal &
                               """,""tracks"":{""track1"":""" & encriptador.AES_Encrypt(trx.track1, clave) &
                               """,""track2"":""" & encriptador.AES_Encrypt(trx.track2, clave) &
                               """},""fechaDesde"":""" & trx.idFechaOriginal & "T00:00:00Z" &
                               """,""fechaHasta"":""" & trx.idFechaOriginal & "T23:59:00Z" &
                               """}}"

        mensajePei(myRequestState.request.Method & " " & myRequestState.request.RequestUri.ToString & vbNewLine &
                   "                <HEADERS>" & vbNewLine &
                   "                    " & "content-type: " & myRequestState.request.Headers.Item("content-type") & vbNewLine &
                   "                    " & "accept: " & myRequestState.request.Headers.Item("accept") & vbNewLine &
                   "                    " & "x-ibm-client-id: " & myRequestState.request.Headers.Item("x-ibm-client-id") & vbNewLine &
                   "                    " & "requerimiento: " & myRequestState.request.Headers.Item("requerimiento") & vbNewLine &
                   "                    " & "cliente: " & myRequestState.request.Headers.Item("cliente") & vbNewLine &
                   "                    " & "token: " & myRequestState.request.Headers.Item("token") & vbNewLine &
                   "                <DATA>" & vbNewLine &
                   JObject.Parse(myRequestState.json).ToString())
        '"                              
        mensajePei(myRequestState.json)
        Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        Dim jsonObject As JObject
        Dim jsontext As String = ""
        Try

            mensajePei("-------------------------- RESPUESTA  CONSULTA -----------------------------")

            mywebresponse = myWebRequest.GetResponse()

            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)

            'Dim readBuff(256) As [Char]
            'Dim count As Integer = streamRead.Read(readBuff, 0, 256)
            'While count > 0
            '    Dim outputData As New [String](readBuff, 0, count)
            '    mensajePei(outputData)
            '    jsonObject = JObject.Parse(outputData)
            '    count = streamRead.Read(readBuff, 0, 256)
            'End While


            Dim readBuff(1024) As [Char]
            Dim count As Integer = streamRead.Read(readBuff, 0, 1024)
            Dim texto As String = ""
            While count > 0
                Dim outputData As New [String](readBuff, 0, count)
                mensajePei(outputData)
                texto = texto + outputData
                Console.WriteLine(outputData)
                count = streamRead.Read(readBuff, 0, 1024)
            End While
            jsonObject = JObject.Parse(texto)


            If jsonObject IsNot Nothing Then
                mensajePei("Respuesta: " & mensajesRepuesta.APROBADA.ToString)
                mensajePei("Total: " & jsonObject.SelectToken("total").ToString)

                For Each fp In jsonObject.SelectToken("resultado").Children()
                    'If fp.SelectToken("idOperacion") = "568046" Then
                    mensajePei("idReferenciaTrxComercio: " & fp.SelectToken("idReferenciaTrxComercio").ToString)
                    mensajePei("idReferenciaOperacionComercio: " & fp.SelectToken("idReferenciaOperacionComercio").ToString)
                    mensajePei("concepto: " & fp.SelectToken("concepto").ToString)
                    mensajePei("idOperacion: " & fp.SelectToken("idOperacion").ToString)
                    mensajePei("estado: " & fp.SelectToken("estado").ToString)
                    mensajePei("---------------------------------------------------------------")

                    'End If
                Next

                'mensajePei("Resultado: " & jsonObject.SelectToken("resultado").ToString)
                'mensajePei("Operacion: " & jsonObject.SelectToken("tipoOperacion").ToString)
                'mensajePei("ID Operacion: " & jsonObject.SelectToken("idOperacion").ToString)
                'mensajePei("ID Operacion Origen: " & jsonObject.SelectToken("idOperacionOrigen").ToString)
                'mensajePei("Nro Referencia Bancaria: " & jsonObject.SelectToken("numeroReferenciaBancaria").ToString)
                'mensajePei("Fecha Operación: " & jsonObject.SelectToken("fecha").ToString)


                'respuesta_compra.id_operacion = jsonObject.SelectToken("idOperacion").ToString
                'respuesta_compra.tipoOperacion = jsonObject.SelectToken("tipoOperacion").ToString
                'respuesta_compra.nro_ref_bancaria = jsonObject.SelectToken("numeroReferenciaBancaria").ToString
                'respuesta_compra.fechahora_respuesta = jsonObject.SelectToken("fecha").ToString
                respuesta_compra.descRespuesta = mensajesRepuesta.APROBADA
                respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)

            Else
                mensajePei("Respuesta: " & mensajesRepuesta.NO_APROBADA.ToString)
                mensajePei("Descripcion: Status Description: NO SE PUDO PARSEAR LA RESPUESTA.")
                mensajePei("Fecha Operación: " & Now)

                respuesta_compra.descRespuesta = mensajesRepuesta.NO_APROBADA
                respuesta_compra.descripcion = "Status Description: NO SE PUDO PARSEAR LA RESPUESTA."
                respuesta_compra.fechahora_respuesta = Now
                respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)
            End If
            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()



        Catch exWeb As WebException
            Dim respuesta = CType(exWeb.Response, HttpWebResponse)
            jsontext = ""
            jsonObject = JObject.Parse(New StreamReader(exWeb.Response.GetResponseStream).ReadToEnd)
            mensajePei(jsonObject.ToString)
            mensajePei("Error en consulta: " & exWeb.Message)
            mensajePei("Status Code: " & exWeb.Status)
            mensajePei("Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString)
            mensajePei("TRX Comercio: " & trx.traceTrxComercio.Split("-")(1))
            'respuesta_compra.descRespuesta = mensajesRepuesta.NO_APROBADA
            'respuesta_compra.descripcion = "Status Description: " & respuesta.StatusDescription & " - " & jsonObject.SelectToken("codigo").ToString & " - " & jsonObject.SelectToken("descripcion").ToString
            'respuesta_compra.fechahora_respuesta = Now


            respuesta_compra.descRespuesta = DirectCast([Enum].Parse(GetType(mensajesRepuesta), jsonObject.SelectToken("codigo").ToString), mensajesRepuesta)
            'respuesta_compra.descRespuesta = jsonObject.SelectToken("codigo").ToString
            respuesta_compra.descripcion = jsonObject.SelectToken("descripcion").ToString
            respuesta_compra.fechahora_respuesta = Now
            respuesta_compra.trace = trx.traceTrxComercio.Split("-")(1)

        Catch ex As Exception
            mensajePei(ex.Message)
        Finally
            If Not mywebresponse Is Nothing Then mywebresponse.Close()
        End Try
        Return respuesta_compra
    End Function



    Private Function Cerrar_Sesion() As String
        mensajePei("-------------------------- CIERRE SESION PEI -----------------------------")
        ServicePointManager.SecurityProtocol = 3072
        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or My.Computer.Name = "BACKTARJETAS" Then
            '--- HOMOLOGACION  
            direccion = "https://h.api.redlink.com.ar/redlink/homologacion/enlacepagosseg/0/0/40/sesion"
        Else
            '--- PRODUCCION  
            direccion = "https://api.redlink.com.ar/redlink/produccion/enlacepagosseg/0/0/40/sesion"
        End If
        Dim myWebRequest As HttpWebRequest = WebRequest.Create(direccion)

        myWebRequest.Timeout = timeOutGeneral
        ' Create an instance of the RequestState and assign 
        ' myWebRequest' to it's request field.
        Dim myRequestState As New RequestState()
        myRequestState.request = myWebRequest
        myRequestState.request.Method = "DELETE"
        myRequestState.request.Accept = "application/json"
        myRequestState.request.ContentType = "application/json"
        myRequestState.request.Headers.Add("cliente: 200.2.127.227")
        myRequestState.request.Headers.Add("requerimiento: FIN_CORMORAN_" & Now.Date & "_" & Now.ToShortTimeString)

        Dim clave As String = ""
        If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or My.Computer.Name = "BACKTARJETAS" Then
            '--- HOMOLOGACION  
            myRequestState.request.Headers.Add("x-ibm-client-id: e003d9ed-8474-4639-bcce-4ca8a26567b0")
            clave = "DDB7A35EB166273505190FBF13390FE00945C602A58E25F06BB42DCD06AE8509" 'GENERADA CUANDO CREE LA API.
        Else
            '--- PRODUCCION   
            myRequestState.request.Headers.Add("x-ibm-client-id: a029f249-5b98-4851-b084-c2b5c44651f1")
            clave = "E3448399448A33EF859EB95B05E705A2291BCFBA6322D46E4908604E2BF6528A" 'GENERADA CUANDO CREE LA API.
        End If

        myRequestState.request.Headers.Add("token: " & token)

        mensajePei(myRequestState.request.Method & " " & myRequestState.request.RequestUri.ToString & vbNewLine &
                   "                <HEADERS>" & vbNewLine &
                   "                    " & "content-type: " & myRequestState.request.Headers.Item("content-type") & vbNewLine &
                   "                    " & "accept: " & myRequestState.request.Headers.Item("accept") & vbNewLine &
                   "                    " & "x-ibm-client-id: " & myRequestState.request.Headers.Item("x-ibm-client-id") & vbNewLine &
                   "                    " & "requerimiento: " & myRequestState.request.Headers.Item("requerimiento") & vbNewLine &
                   "                    " & "cliente: " & myRequestState.request.Headers.Item("cliente") & vbNewLine &
                   "                    " & "token: " & myRequestState.request.Headers.Item("token") & vbNewLine &
                   "                <DATA>" & vbNewLine &
                   "                    " & myRequestState.json)

        Dim r As IAsyncResult = CType(myWebRequest.BeginGetRequestStream(AddressOf ReadCallback, myRequestState), IAsyncResult)
        ' Pause the current thread until the async operation completes.
        allDone.WaitOne()
        ' Send the Post and get the response.
        Dim mywebresponse As WebResponse = Nothing
        'Dim jsonObject As JObject
        Dim jsontext As String = ""
        Try
            mensajePei("-------------------------- RESPUESTA CIERRE SESION -----------------------------")
            System.Threading.Thread.Sleep(1000)
            mensajePei("*** 1 ***")
            mywebresponse = myWebRequest.GetResponse()

            Dim streamResponse As Stream = mywebresponse.GetResponseStream()
            Dim streamRead As New StreamReader(streamResponse)
            Dim readBuff(256) As [Char]
            mensajePei("*** 2 ***")
            Dim count As Integer = streamRead.Read(readBuff, 0, 256)

            'mensajePei("Count 1: " & count.ToString)

            streamResponse.Close()
            streamRead.Close()
            mywebresponse.Close()

            If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or My.Computer.Name = "BACKTARJETAS" Then
                '--- PARA QUE DE ERROR 
                'Throw New Exception("")
            End If

            mensajePei("Sesion CERRADA en PEI")
        Catch exWeb As WebException
            mensajePei("Try-Catch exWeb (Cerrar_Sesion)")
            mensajePei("La Sesion no se cerró")
            mensajePei("Error de cierre de sesión: " & exWeb.Response.Headers("exception-message"))
        Catch ex As Exception
            mensajePei("Try-Catch ex (Cerrar_Sesion)")
            mensajePei("La Sesion no se cerró")
            mensajePei(ex.Message)
        Finally
            If Not mywebresponse Is Nothing Then mywebresponse.Close()
        End Try
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
    End Sub


End Class
