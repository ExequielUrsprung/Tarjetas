<Serializable()>
Public Class Mensaje
    Dim tipo As TipoRequerimientos
    Public caja As String
    Public ping As String
    Public idEncripcion As String

    Public Sub New(tipoMensaje As TipoRequerimientos)
        tipo = tipoMensaje
    End Sub





End Class
