Imports IsoLib

Public Class FrmHostEmulator
    Dim WithEvents Escuchador As EscuchadorPrueba
    Dim WithEvents EscuchadorPosnet As EscuchadorPrueba
    Dim WithEvents timer1 As Timer

    Private Sub FrmHostEmulator_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        If Escuchador IsNot Nothing Then
            Escuchador.Dispose()
            Escuchador = Nothing
        End If
        If EscuchadorPosnet IsNot Nothing Then
            EscuchadorPosnet.Dispose()
            EscuchadorPosnet = Nothing
        End If

    End Sub

    Dim log As Logeador
    Private Sub FrmHostEmulator_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Escuchador = New EscuchadorPrueba(IsoLib.E_Implementaciones.Visa, "132.161.1.103", 15001, True)

        log = New Logeador(True)
        timer1 = New Timer()
        timer1.Interval = 200
        timer1.Start()
    End Sub
    'pEvento As IsoLib.E_eventosControl, 
    Private Sub Escuchador_Control(ByRef pRespuesta As IsoLib.e_eventosRespuesta) Handles Escuchador.Control
        'Select Case pEvento
        '    Case IsoLib.E_eventosControl.EchoTest
        pRespuesta = IsoLib.e_eventosRespuesta.Aprobado
        '    Case IsoLib.E_eventosControl.VentaPOS
        'pRespuesta = e_eventosRespuesta.Aprobado
        'End Select
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles timer1.Tick
        If log IsNot Nothing Then log.GetArrayAndClear(ListBox1)

    End Sub

    Private Sub Escuchador_TransaccionPOSCredito(ByRef parametros As ParametrosCom, CodigoTransaccion As E_ProcCode) Handles Escuchador.TransaccionPOSCredito, EscuchadorPosnet.TransaccionPOSCredito
        '--- RESPUESTA DE VENTA, aca escribir el codigo de "negocio" para validar y provocar distintos codigos de respuesta 
        If parametros.wkey = "1234123412341234" Then
            System.Threading.Thread.Sleep(20000)
            parametros.Respuesta = E_CodigosRespuesta.Pin_incorrecto
            Exit Sub
        End If
        parametros.p63Msg = "MENSAJE DE TEXTO"
        If parametros.Monto = 100 Then
            parametros.Respuesta = E_CodigosRespuesta.EXCEDE_LIMITE_DIARIO
            parametros.texto = "SALDO 99.99"
            Exit Sub
        End If
        If parametros.Vto < Now.Date Then
            parametros.Respuesta = E_CodigosRespuesta.Tarjeta_vencida
            Exit Sub
        End If
        If parametros.Track2 = "" Then
            parametros.Respuesta = E_CodigosRespuesta.Transaccion_Invalida
            Exit Sub
        End If
        'If parametros.NroAutorizacion = 0 Then
        '    System.Threading.Thread.Sleep(33000)
        'End If



        parametros.Respuesta = E_CodigosRespuesta.APROBADA


    End Sub

    Private Sub Escuchador_TransaccionPOSCreditoAnulacion(ByRef parametros As ParametrosCom) Handles Escuchador.TransaccionPOSCreditoAnulacion

    End Sub

    Private Sub Escuchador_TransaccionPOSCreditoReverso(ByRef parametros As ParametrosCom) Handles Escuchador.TransaccionPOSCreditoReverso
        parametros.texto = "Error envio"
    End Sub

    Private Sub Escuchador_TransaccionPOSCreditoReversoAnulacion(ByRef parametros As ParametrosCom) Handles Escuchador.TransaccionPOSCreditoReversoAnulacion

    End Sub

    Private Sub Escuchador_TransaccionPOSNoImplementada(ByRef parametros As ParametrosCom) Handles Escuchador.TransaccionPOSNoImplementada

    End Sub

    Private Sub Escuchador_TransaccionRespuesta(ByRef msg As Iso8583messagePOS) Handles Escuchador.TransaccionRespuesta

    End Sub

    Private Sub Escuchador_Cierre(ByRef parametros As ParametrosCom) Handles Escuchador.Cierre
        If parametros.idTerminalCOM = "75801003" Then
            parametros.Respuesta = E_CodigosRespuesta.Transaccion_Invalida
            Exit Sub
        End If
        parametros.Respuesta = E_CodigosRespuesta.APROBADA
    End Sub


End Class
