Imports ServerLib


Public Class Form1
    Dim WithEvents serv As New ServerLib.ServerTar
    Public Sub Conectar()
        serv.Arrancar()

    End Sub

    Delegate Sub Mostrar(dato)

    Sub muestra(s As String)
        ListBox1.Items.Add(s)
    End Sub

    Sub MostrarMensajes(s As String) Handles serv.Mensaje
        Dim d As New Mostrar(AddressOf muestra)
        Invoke(d, New Object() {s})


    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Conectar()
    End Sub
End Class
