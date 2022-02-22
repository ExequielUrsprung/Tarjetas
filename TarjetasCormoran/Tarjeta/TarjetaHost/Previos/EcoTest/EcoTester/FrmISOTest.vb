Imports ISOLib

Public Class FrmISOTest

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        Dim ConPosnet As New llamador("POSNET", E_Implementaciones.PosnetComercio, llamador.IpPosnet, llamador.PuertoPosnet)
        With ConPosnet
            .conectar()


            .Desconectar()
        End With
    End Sub
End Class