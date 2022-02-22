
<Serializable()>
Public Class MensajeIda
    Implements ICloneable
    Public tipoMensaje As TipoMensaje
    Public reversar As Boolean
    Public caja As String
    Public ping As String
    Public host As TipoHost
    Public idEncripcion As String
    Public importeCompra As Decimal
    Public importeCashback As Decimal
    Public track1 As String
    Public track2 As String
    Public plan As String
    Public codigoSeguridad As String
    Public pinBlock As String
    Public tipoIngreso As TipoIngreso
    Public nroTarjeta As String
    Public fechaTransaccion As DateTime
    Public horaTransaccion As String
    Public fechaExpiracion As String
    Public nroSerie As String
    Public terminal As String
    Public cajera As Integer
    Public ticketCaja As Integer
    Public ticketOriginal As Integer
    Public fechaOperacionOriginal As DateTime


    Public secuencia As String
    Public datosEncriptados As String
    Public cuotas As String
    Public moneda As Moneda
    Public versionAPP As String
    Public criptograma As String
    Public serviceCode As String
    Public nombreApplicacion As String
    Public appID As String
    Public datosemisor As String



    Public posMK As Integer
    Public RespChip As String

    Public clavereverso As String
    Public tipocuenta As String
    Public nombreTHabiente As String
    Public referenceNumber As String

    Public NroDocumentoTicket As String
    Public TipoCuentaY07 As String
    Public CodAutorizaAdv As String
    Public versionMSG As Integer

    Public Function Clone() As Object Implements ICloneable.Clone
        Dim copia As New MensajeIda
        With copia
            .tipoMensaje = Me.tipoMensaje
            .caja = Me.caja
            .ping = Me.ping
            .idEncripcion = Me.idEncripcion
            .importeCompra = Me.importeCompra
            .importeCashback = Me.importeCashback
            .track1 = Me.track1
            .track2 = Me.track2
            .plan = Me.plan
            .codigoSeguridad = Me.codigoSeguridad
            .pinBlock = Me.pinBlock
            .tipoIngreso = Me.tipoIngreso
            .nroTarjeta = Me.nroTarjeta
            .fechaTransaccion = Me.fechaTransaccion
            .horaTransaccion = Me.horaTransaccion
            .fechaExpiracion = Me.fechaExpiracion
            .nroSerie = Me.nroSerie
            .terminal = Me.terminal
            .cajera = Me.cajera
            .ticketCaja = Me.ticketCaja
            .ticketOriginal = Me.ticketOriginal
            .fechaOperacionOriginal = Me.fechaOperacionOriginal
            .datosemisor = Me.datosemisor

            .secuencia = Me.secuencia
            .datosEncriptados = Me.datosEncriptados
            .cuotas = Me.cuotas
            .moneda = Me.moneda
            .versionAPP = Me.versionAPP
            .criptograma = Me.criptograma
            .serviceCode = Me.serviceCode
            .posMK = Me.posMK
            .RespChip = Me.RespChip
            .clavereverso = Me.clavereverso
            .tipocuenta = Me.tipocuenta
            .nombreTHabiente = Me.nombreTHabiente
            .appID = Me.appID
            .nombreApplicacion = Me.nombreApplicacion
            .referenceNumber = Me.referenceNumber

            .NroDocumentoTicket = Me.NroDocumentoTicket

            .versionMSG = Me.versionMSG
            Return copia

        End With



    End Function
End Class

<Serializable()>
Public Class MensajeRespuesta
    Public host As String


    Public codRespuesta As String
    Public clave As String 'wrk
    Public fechaRespuesta As String
    Public horaRespuesta As String

    Public Clave_wkp As String
    Public Clave_wkd As String
    Public Clave_ctrlwkp As String
    Public Clave_ctrlwkd As String
    Public PosicionMK As String
    Public terminal As String
    Public serie_PP As String

    Public u4d, cds, spi As Boolean

    Public Sincronizado As Boolean
    Public criptograma As String
    Public mensajeHost As String
    Public Respuesta As String
    Public Autorizacion As String
    Public Importe As Decimal
    Public Emisor As String
    Public Ticket As Integer
    Public CashBack As Decimal
    Public clavereverso As String
    Public cupon As String
    Public nroPlan As String
    Public referenceNumber As String

    Public tipooperacion As String
End Class

Public Enum TipoMensaje
    Compra
    CompraCashback
    Devolucion
    Anulacion
    AnulacionDevolucion
    Sincronizacion
    Echotest
    CierreLote
    PedirTerminal
    ActualizarSerie
    PedirInfoAdicional
    PedirClaves
    RespuestaChip
    ConsultarMovimientoAnulacion
    Reverso
    ActualizarVersion
    Advice
End Enum

Public Enum TipoIngreso
    Banda = 0
    Manual = 1
    Chip = 5
    Contactless = 7
    CL_Magstripe = 91
End Enum

Public Enum TipoHost
    POSNET = 1
    VISA = 2
    Comfiar = 3
    Diners = 4
    Posnet_homolog = 5
    Visa_homolog = 6
End Enum

Public Enum Moneda
    Pesos
    Dolares
End Enum