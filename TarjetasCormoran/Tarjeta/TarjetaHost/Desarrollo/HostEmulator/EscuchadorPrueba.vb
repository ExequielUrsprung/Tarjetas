Imports IsoLib

Class EscuchadorPrueba
    Inherits Escuchador
    Private _e_Implementaciones As IsoLib.E_Implementaciones
    Private _p2 As String
    Private _p3 As Integer
    Private _p4 As Boolean

    Sub New(e_Implementaciones As IsoLib.E_Implementaciones, p2 As String, p3 As Integer, p4 As Boolean)
        MyBase.New(e_Implementaciones, p2, p3, p4)
    End Sub

End Class
