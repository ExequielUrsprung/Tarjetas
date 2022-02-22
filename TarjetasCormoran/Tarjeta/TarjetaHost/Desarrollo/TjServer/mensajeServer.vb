
Public Class mensajeServer
    Property Texto As String
    Property Fecha As Date
    Property Tipo As TipoMensaje
End Class

Public Enum TipoMensaje
    Informacion = 0
    [Error] = 10
End Enum
