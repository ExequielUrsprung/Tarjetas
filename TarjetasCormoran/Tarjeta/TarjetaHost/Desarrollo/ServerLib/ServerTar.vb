Imports System
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports Microsoft.VisualBasic
Imports IsoLibCaja


''' <summary>
''' Clase Principal que representa al servidor de tarjetas
''' </summary>
''' <remarks></remarks>
Public Class ServerTar

    Public Event Mensaje(s As String)
    Public Puerto As Integer = 3000
    Dim WithEvents ServerCajas As ServidorCajas
    Sub New()
    End Sub

    Sub Arrancar()
        RaiseEvent Mensaje("Arrancando Servidor en Puerto " + Puerto.ToString)
        Try
            ServerCajas = New ServidorCajas(Puerto)
            RaiseEvent Mensaje("Servidor Listo!")
        Catch ex As Exception
            RaiseEvent Mensaje("Error Arrancando servidor " + ex.Message)
        End Try
    End Sub


    Private Sub ServerCajas_RequerimientoCajaRecibido(ByVal ParametrosRecibidos As SParametrosInput, ByRef Respuesta As SParametrosOutput) Handles ServerCajas.RequerimientoCajaRecibido

        RaiseEvent Mensaje("Requerimiento recibido")
        Try
            ' aca va la lOGICA, como esto es multithreading esto puede esperar ....
            'Respuesta.mensaje = "Su requerimiento llego a las " + Now.ToLongTimeString + " para tarj " + ParametrosRecibidos.Tarjeta
            ' VALIDACION 
            'respValidacion = validarServer(ParametrosRecibidos)
            '---- PASO 1: SE GRABA EN HISTORIA COMO QUE SE RECIBIO EL REQUERIMIENTO DESDE LA CAJA Y TAMBIEN SE AUMENTA EL TRACE 

            ' GRABAR REQUERIMIENTO CAJA 
            'Select Case ParametrosRecibidos.Operacion
            Select Case ParametrosRecibidos.oper
                Case TipoRequerimientos.Compra

                    '---- PASO 2: SE MODIF. SOLAMENTE ESTADO EN HISTORIA COMO QUE SE ENVIA A CAJA NEGRA GUSTAVO QUE ENVIA A POSNET 
                    'historia.ModifEstadoRequer(ParametrosRecibidos.CajaID, _
                    '                           iemisor.nrohost, _
                    '                           numerotrace, _
                    '                           estado.ReqEnviadoAEmisor)


                    RaiseEvent Mensaje("Requerimiento enviado a POSNET")
                    'Respuesta = envioPosnet(ParametrosRecibidos, iemisor.emisor, iplanes.ObtenerPlan(iemisor.emisor, ParametrosRecibidos.PLAN).Coeficiente)
                    ' ENVIAR 

                    'ArmaTicket(ParametrosRecibidos, Respuesta)

                    RaiseEvent Mensaje("Respuesta recibida")



            End Select

            '--- PASO 3: SI DEVUELVE APROBADA --> AUMENTAR EL NUMERO TICKET (SOLAMENTE SE INCREMENTA CUANDO ESTA APROBADA)    
            'If Respuesta.Respuesta = IsoLibCaja.Respuesta.Aprobada Then
            '    inumeros.incrementaNroTicket(ParametrosRecibidos.CajaID, _
            '                                 iemisor.nrohost)
            'End If

            '--- PASO 4: SE MODIF. SOLAMENTE ESTADO EN HISTORIA COMO QUE SE RECIBIO DEL EMISOR  (VIENE DE LA CAJA NEGRA DE GUSTAVO) 
            'historia.ModifEstadoRequer(ParametrosRecibidos.CajaID, _
            '                           iemisor.nrohost, _
            '                           numerotrace, _
            '                           estado.ReqRecibidoDesdeEmisor)



            '    ------------------------------------------------------->>>>>
            '  ANTES DE POSNET 
            '    enviar mensaje
            '  VUELTA DE POSNET
            '    -------------------------------------------------------<<<<<


            ' recibo respuesta
            ' grabo datos respuesta paso 2

            '--- PASO 5: SE MODIF. SOLAMENTE ESTADO EN HISTORIA COMO QUE SE ENVIA A LA CAJA
            'historia.ModifEstadoRequer(ParametrosRecibidos.CajaID, _
            '                           iemisor.nrohost, _
            '                           numerotrace, _
            '                           estado.ReqEnviadoACaja)




            'Else
            '    Respuesta.Respuesta = IsoLibCaja.Respuesta.Rechazada
            '    Respuesta.CodRespuesta = 1
            '    Respuesta.mensaje = respValidacion
            '    RaiseEvent Mensaje(respValidacion)

            'End If



        Catch ex As Exception
            RaiseEvent Mensaje("Error procesando requerimiento " + ex.Message)
            Dim resp As New SParametrosOutput
            resp.mensaje = "Error Procesando Requerimiento " + ex.Message
            resp.Respuesta = IsoLibCaja.Respuesta.Rechazada
            resp.mensaje = "ERROR EN LA TRANSMISION"
        End Try
    End Sub


    Private Sub ServerCajas_RequerimientoEnviado(ByVal Respuesta As SParametrosOutput) Handles ServerCajas.RequerimientoEnviado


        ' incorporar logica si se pudo enviar o no
        ' grabo recibido por caja paso 3
    End Sub

    Sub detener()

        RaiseEvent Mensaje("Deteniendo servidor ")
        ServerCajas.Detener()
        RaiseEvent Mensaje("Servidor Detenido")

    End Sub

    Private Function envioPosnet(ByVal ida As SParametrosInput, ByVal emisor As Integer, ByVal coe As Decimal) As SParametrosOutput
        Dim resp As New SParametrosOutput
        Dim random As New Random
        System.Threading.Thread.Sleep(random.Next(1, 30))
        With resp
            '.CodRespuesta = 1
            '.importe = ida.Importe * coe
            '.mensaje = " ESTO ES UNA PRUEBA"
            '.NroAutorizacion = random.Next(100000, 999999)
            '.Emisor = 1
            '.Respuesta = random.Next(Respuesta.Aprobada, Respuesta.SinServicio)
        End With
        Return resp
    End Function

End Class

