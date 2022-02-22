Imports System.IO

Public Class Parametros
    Const _HOMOLOG As String = "Homologacion"
    Const _PROD As String = "Produccion"
    Const _RAF As String = "Rafaela"
    Const _SF As String = "San Francisco"
    Const _CIERREAUTO As String = "Auto"
    Const _CIERREMANUAL As String = "Manual"
    Const _CONEXIONVPN As String = "VPN"
    Const _CONEXIONMPLS As String = "MPLS"
    Const _MODOPEI As String = "PEI"
    Const _MODONOPEI As String = "NOPEI"


    Property DirTarjetas As String
    Property DirLocal As String
    Property HoraReinicio As String
    Property HoraCierre As String
    Property HoraCierre1 As String
    Property ServidorCorreo As String
    Property PuertoCorreo As String
    Property TipoConfiguracion As String
    Property Ciudad As String
    Property TipoCierre As String
    Property TipoConexion As String
    Property modo As String

    Property Homologacion As String = _HOMOLOG
    Property Produccion As String = _PROD
    Property Rafaela As String = _RAF
    Property SanFrancisco As String = _SF
    Property CierreAut As String = _CIERREAUTO
    Property CierreMan As String = _CIERREMANUAL
    Property Vpn As String = _CONEXIONVPN
    Property Mpls As String = _CONEXIONMPLS
    Property PEI As String = _MODOPEI
    Property nopei As String = _MODONOPEI

    Sub New()
        leerParametros()
    End Sub

    Private Sub leerParametros()
        Try
            Dim arc As FileIO.TextFieldParser
            Dim renglon As String()
            arc = New FileIO.TextFieldParser("parametros.txt")
            arc.TextFieldType = FileIO.FieldType.Delimited
            arc.SetDelimiters("=")

            While Not arc.EndOfData
                renglon = arc.ReadFields
                If renglon(0) = "Ciudad (Rafaela - San Francisco)" Then
                    Ciudad = renglon(1)
                    If Ciudad <> _Rafaela And Ciudad <> _SanFrancisco Then
                        Throw New Exception("PARAMETRO CIUDAD INCORRECTO.")
                    End If
                ElseIf renglon(0) = "DirTarjetas" Then
                    DirTarjetas = renglon(1)
                ElseIf renglon(0) = "DirLocal" Then
                    DirLocal = renglon(1)
                ElseIf renglon(0) = "TipoConfiguracion (Produccion - Homologacion)" Then
                    TipoConfiguracion = renglon(1)
                    If TipoConfiguracion <> _Produccion And TipoConfiguracion <> _Homologacion Then
                        Throw New Exception("PARAMETRO TIPOCONFIGURACION INCORRECTO.")
                    End If
                ElseIf renglon(0) = "HoraCierre" Then
                    HoraCierre = renglon(1)
                ElseIf renglon(0) = "HoraCierre1" Then
                    HoraCierre1 = renglon(1)
                ElseIf renglon(0) = "HoraReinicio" Then
                    HoraReinicio = renglon(1)
                ElseIf renglon(0) = "ServidorCorreo" Then
                    ServidorCorreo = renglon(1)
                ElseIf renglon(0) = "PuertoCorreo" Then
                    PuertoCorreo = renglon(1)
                ElseIf renglon(0) = "Cierre (Auto - Manual)" Then
                    TipoCierre = renglon(1)
                    If TipoCierre <> _CIERREAUTO And TipoCierre <> _CIERREMANUAL Then
                        Throw New Exception("PARAMETRO TIPO CIERRE INCORRECTO.")
                    End If
                ElseIf renglon(0) = "Conexion (MPLS - VPN)" Then
                    TipoConexion = renglon(1)
                    If TipoConexion <> _CONEXIONVPN And TipoConexion <> _CONEXIONMPLS Then
                        Throw New Exception("PARAMETRO TIPO CONEXION INCORRECTO.")
                    End If
                ElseIf renglon(0) = "MODO (PEI - NOPEI)" Then
                    modo = renglon(1)
                    If modo <> _MODOPEI And modo <> _MODONOPEI Then
                        Throw New Exception("PARAMETRO MODO PEI INCORRECTO.")
                    End If
                End If
            End While
        Catch
            Logger.Error("No se pudo cargar el archivo de parametros.")
        End Try
    End Sub



End Class
