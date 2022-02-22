Imports System.IO
Public Class Puerto
    Inherits System.IO.Ports.SerialPort



#Region "Eventos"

#End Region
#Region "Constantes"

#End Region

#Region "Métodos Públicos"
    Public Sub New()
        With Me
            '.PortName = "COM" + My.Settings.puerto.ToString
            '.BaudRate = 9600
            '.DataBits = 8
            '.Parity = Ports.Parity.None
            '.ReadBufferSize = 2048
            '.WriteTimeout = 3000
            '.ReadTimeout = 3000
            '.PortName = "COM" + My.Settings.puerto.ToString
            '.Parity = Ports.Parity.Even
            '.DataBits = 7
            '.BaudRate = My.Settings.velocidad
            '.ReadBufferSize = 2048
            '.WriteTimeout = 3000
            '.ReadTimeout = 3000

            .PortName = "COM" + My.Settings.puerto.ToString
            .Parity = Ports.Parity.None
            .DataBits = 8
            .BaudRate = 9600
            .ReadBufferSize = 2048
            .WriteTimeout = 3000
            .ReadTimeout = 3000

            .DiscardNull = True

        End With


    End Sub


#End Region







End Class
