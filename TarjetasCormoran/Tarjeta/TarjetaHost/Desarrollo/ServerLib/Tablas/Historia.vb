

Public Class Historia

    Dim SQLHistoriaDataTable As TarjetasDataSet.HISTORIADataTable
    Dim SQLHistoriaTableAdapter As TarjetasDataSetTableAdapters.HISTORIATableAdapter
    Dim SQLHistoriaFila As TarjetasDataSet.HISTORIARow


#Region "Propiedades"
    Dim NroCajaPropio As Integer
    Public ReadOnly Property getNroCajaPropio()
        Get
            Return NroCajaPropio
        End Get
    End Property

    Dim NroHost As Integer
    Public ReadOnly Property getNroHost()
        Get
            Return NroHost
        End Get
    End Property

    Dim NroTrace As Integer
    Public ReadOnly Property getNroTrace()
        Get
            Return NroTrace
        End Get
    End Property

    Dim TicketPosnet As Integer
    Public ReadOnly Property getTicketPosnet()
        Get
            Return TicketPosnet
        End Get
    End Property

    Dim NroEmisor As Integer
    Public ReadOnly Property getNroEmisor()
        Get
            Return NroEmisor
        End Get
    End Property

    Dim NroPlan As Integer
    Public ReadOnly Property getNroPlan()
        Get
            Return NroPlan
        End Get
    End Property

    Dim TipoOperacion As String
    Public ReadOnly Property getTipoOperacion()
        Get
            Return TipoOperacion
        End Get
    End Property

    Dim TipoIngreso As String
    Public ReadOnly Property getTipoIngreso()
        Get
            Return TipoIngreso
        End Get
    End Property

    Dim Terminal As String
    Public ReadOnly Property getTerminal()
        Get
            Return Terminal
        End Get
    End Property

    Dim Tarjeta As String
    Public ReadOnly Property getTarjeta()
        Get
            Return Tarjeta
        End Get
    End Property

    Dim Vencimiento As Integer
    Public ReadOnly Property getVencimiento()
        Get
            Return Vencimiento
        End Get
    End Property

    Dim Importe As Decimal
    Public ReadOnly Property getImporte()
        Get
            Return Importe
        End Get
    End Property

    Dim Cajera As Integer
    Public ReadOnly Property getCajera()
        Get
            Return Cajera
        End Get
    End Property

    Dim TicketCaja As Integer
    Public ReadOnly Property getTicketCaja()
        Get
            Return TicketCaja
        End Get
    End Property

    Dim Lote As Integer
    Public ReadOnly Property getLote()
        Get
            Return Lote
        End Get
    End Property

    Dim FechaOperacion As Date
    Public ReadOnly Property getFechaOperacion()
        Get
            Return FechaOperacion
        End Get
    End Property

    Dim FechaCierre As Date
    Public ReadOnly Property getFechaCierre()
        Get
            Return FechaCierre
        End Get
    End Property

    Dim CodAutorizacion As Integer
    Public ReadOnly Property getCodAutorizacion()
        Get
            Return CodAutorizacion
        End Get
    End Property

    Dim TicketPosOriginal As Integer
    Public ReadOnly Property getTicketPosOriginal()
        Get
            Return TicketPosOriginal
        End Get
    End Property

    Dim FechaOpOriginal As Date
    Public ReadOnly Property getFechaOpOriginal()
        Get
            Return FechaOpOriginal
        End Get
    End Property

    Dim Est As Integer
    Public ReadOnly Property getEstado()
        Get
            Return Est
        End Get
    End Property
#End Region


    ''' <summary>
    ''' Obtiene el registro del HISTORIA, devuelve un datarow del tipo HistoriaRow, sino lo encuentra devuelve nothing 
    ''' </summary>
    ''' <param name="PingCaja">Nro PingCaja</param>
    ''' <param name="NroTrace">Nro de Trace</param>
    ''' <remarks></remarks>
    Public Function ObtenerHistoria(ByVal hosts As Integer, ByVal PingCaja As Integer, ByVal NroTrace As Integer) As TarjetasDataSet.HISTORIARow
        Try
            SQLHistoriaDataTable = New TarjetasDataSet.HISTORIADataTable
            SQLHistoriaTableAdapter = New TarjetasDataSetTableAdapters.HISTORIATableAdapter
            SQLHistoriaTableAdapter.Fill(SQLHistoriaDataTable)

            SQLHistoriaFila = SQLHistoriaDataTable.FindByNroCajaPropioNroHostNroTrace(PingCaja, hosts, NroTrace)
            If SQLHistoriaFila IsNot Nothing Then
                NroCajaPropio = SQLHistoriaFila.NroCajaPropio
                NroHost = SQLHistoriaFila.NroHost
                NroTrace = SQLHistoriaFila.NroTrace
                TicketPosnet = SQLHistoriaFila.TicketPosnet
                NroEmisor = SQLHistoriaFila.NroEmisor
                NroPlan = SQLHistoriaFila.NroPlan
                TipoOperacion = SQLHistoriaFila.TipoOperacion
                TipoIngreso = SQLHistoriaFila.TipoIngreso
                Terminal = SQLHistoriaFila.Terminal
                Tarjeta = SQLHistoriaFila.tarjeta
                Vencimiento = SQLHistoriaFila.vencimiento
                Importe = SQLHistoriaFila.importe
                Cajera = SQLHistoriaFila.cajera
                TicketCaja = SQLHistoriaFila.ticketcaja
                Lote = SQLHistoriaFila.lote
                FechaOperacion = SQLHistoriaFila.fechaoperacion
                'FechaCierre = SQLHistoriaFila.fechacierre
                CodAutorizacion = SQLHistoriaFila.CodAutorizacion
                TicketPosOriginal = SQLHistoriaFila.TicketPosOriginal
                'FechaOpOriginal = SQLHistoriaFila.FechaOpOriginal
                Est = SQLHistoriaFila.estado
            End If

            Return SQLHistoriaFila

        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
            Return Nothing
        End Try

    End Function

    Public Function chequeaEstado(ByVal estado As Integer) As Integer
        Try
            SQLHistoriaTableAdapter = New TarjetasDataSetTableAdapters.HISTORIATableAdapter
            Return SQLHistoriaTableAdapter.cantPorEstado(estado)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
            Return Nothing
        End Try
    End Function



    Public Sub nuevoReqRecibido(ByVal caja As Integer, _
                                ByVal host As Integer, _
                                ByVal nrotrace As Integer, _
                                ByVal estado As Integer, _
                                ByVal codaut As Integer, _
                                ByVal nroEmi As Integer, _
                                ByVal nroPlan As Integer, _
                                ByVal terminal As Integer, _
                                ByVal tktPos As Integer, _
                                ByVal modoing As Integer, _
                                ByVal tipoOp As Integer, _
                                ByVal cajera As Integer, _
                                ByVal fecOp As String, _
                                ByVal importe As Decimal, _
                                ByVal nroTarj As String, _
                                ByVal tktCaja As Integer, _
                                ByVal vencim As String, _
                                ByVal lote As Integer, _
                                ByVal fecopori As String, _
                                ByVal tktPosOri As Integer, _
                                ByVal fecCierre As String)
        Try
            SQLHistoriaTableAdapter = New TarjetasDataSetTableAdapters.HISTORIATableAdapter
            SQLHistoriaTableAdapter.Insert(caja, _
                                           host, _
                                           nrotrace, _
                                           estado, _
                                           codaut, _
                                           fecopori, _
                                           nroEmi, _
                                           nroPlan, _
                                           terminal, _
                                           tktPosOri, _
                                           tktPos, _
                                           modoing, _
                                           tipoOp, _
                                           cajera, _
                                           fecCierre, _
                                           fecOp, _
                                           importe, _
                                           lote, _
                                           tktCaja, _
                                           vencim, _
                                           nroTarj)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
        End Try
    End Sub

    Public Sub nuevoReqRecibido(ByVal caja As Integer, _
                                ByVal host As Integer, _
                                ByVal nrotrace As Integer, _
                                ByVal estado As Integer, _
                                ByVal codaut As Integer, _
                                ByVal nroEmi As Integer, _
                                ByVal nroPlan As Integer, _
                                ByVal terminal As Integer, _
                                ByVal tktPos As Integer, _
                                ByVal modoing As Integer, _
                                ByVal tipoOp As Integer, _
                                ByVal cajera As Integer, _
                                ByVal fecOp As String, _
                                ByVal importe As Decimal, _
                                ByVal nroTarj As String, _
                                ByVal tktCaja As Integer, _
                                ByVal vencim As String)
        Try
            SQLHistoriaTableAdapter = New TarjetasDataSetTableAdapters.HISTORIATableAdapter
            SQLHistoriaTableAdapter.Insert(caja, _
                                           host, _
                                           nrotrace, _
                                           estado, _
                                           codaut, _
                                           Nothing, _
                                           nroEmi, _
                                           nroPlan, _
                                           terminal, _
                                           0, _
                                           tktPos, _
                                           modoing, _
                                           tipoOp, _
                                           cajera, _
                                           Nothing, _
                                           fecOp, _
                                           importe, _
                                           0, _
                                           tktCaja, _
                                           vencim, _
                                           nroTarj)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
        End Try
    End Sub
    Public Sub ModifEstadoRequer(ByVal caja As Integer, _
                                 ByVal host As Integer, _
                                 ByVal nrotrace As Integer, _
                                 ByVal estado As Integer)
        Try
            SQLHistoriaTableAdapter = New TarjetasDataSetTableAdapters.HISTORIATableAdapter
            SQLHistoriaTableAdapter.UpdateEstado(estado, caja, host, nrotrace)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
        End Try

    End Sub






End Class
