
Public Class Numeros
    Dim SQLNumerosDataTable As TarjetasDataSet.NUMEROSDataTable
    Dim SQLNumerosTableAdapter As TarjetasDataSetTableAdapters.NUMEROSTableAdapter
    Dim SQLNumerosFila As TarjetasDataSet.NUMEROSRow


    Dim ODBCNumerosDataTable As ODBC.numerosDataTable
    Dim ODBCNumerosTableAdapter As ODBCTableAdapters.numerosTableAdapter
    Dim ODBCNumerosFila As ODBC.numerosRow
#Region "Propiedades"
    Dim nro_terminal As String
    Public ReadOnly Property nroterminal()
        Get
            Return nro_terminal
        End Get
    End Property

    Dim nro_lote As Integer
    Public ReadOnly Property nrolote()
        Get
            Return nro_lote
        End Get
    End Property
    Dim nro_ticket As Integer
    Public ReadOnly Property nroticket()
        Get
            Return nro_ticket
        End Get
    End Property
    Dim nro_trace As Integer
    Public ReadOnly Property nrotrace()
        Get
            Return nro_trace
        End Get
    End Property
    Dim nro_PINblock As String
    Public ReadOnly Property nropinblock()
        Get
            Return nro_PINblock
        End Get
    End Property
#End Region

    ''' <summary>
    ''' Sincroniza con la base de datos Progress para actualizar la Tabla Numeros en la DB Local
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SincronizaNumeros()
        Try
            SQLNumerosDataTable = New TarjetasDataSet.NUMEROSDataTable
            SQLNumerosTableAdapter = New TarjetasDataSetTableAdapters.NUMEROSTableAdapter
            ODBCNumerosDataTable = New ODBC.numerosDataTable
            ODBCNumerosTableAdapter = New ODBCTableAdapters.numerosTableAdapter


            SQLNumerosTableAdapter.Fill(SQLNumerosDataTable)
            ODBCNumerosTableAdapter.Fill(ODBCNumerosDataTable)
            For Each ODBCNumerosFila In ODBCNumerosDataTable
                SQLNumerosFila = SQLNumerosDataTable.FindByNroCajaPropioNroHost(ODBCNumerosFila._caj_nro, ODBCNumerosFila._hos_nro)
                If SQLNumerosFila Is Nothing Then
                    SQLNumerosFila = SQLNumerosDataTable.NewNUMEROSRow
                    SQLNumerosFila.NroHost = ODBCNumerosFila._hos_nro
                    SQLNumerosFila.NroCajaPropio = ODBCNumerosFila._caj_nro
                    SQLNumerosFila.NroLote = ODBCNumerosFila._lot_nro
                    SQLNumerosFila.NroTicket = ODBCNumerosFila._tic_nro
                    SQLNumerosFila.NroTrace = ODBCNumerosFila._tra_nro
                    SQLNumerosFila.Terminal = ODBCNumerosFila._ter_nro
                    SQLNumerosFila.PinBlock = ODBCNumerosFila._pin_nro
                    SQLNumerosDataTable.AddNUMEROSRow(SQLNumerosFila)
                Else
                    SQLNumerosFila.NroLote = ODBCNumerosFila._lot_nro
                    SQLNumerosFila.NroTicket = ODBCNumerosFila._tic_nro
                    SQLNumerosFila.NroTrace = ODBCNumerosFila._tra_nro
                    SQLNumerosFila.PinBlock = ODBCNumerosFila._pin_nro
                    SQLNumerosFila.Terminal = ODBCNumerosFila._ter_nro
                End If
            Next

            For Each SQLNumerosFila In SQLNumerosDataTable
                ODBCNumerosFila = ODBCNumerosDataTable.FindBy_caj_nro_hos_nro(SQLNumerosFila.NroCajaPropio, SQLNumerosFila.NroHost)
                If ODBCNumerosFila Is Nothing Then
                    SQLNumerosTableAdapter.Delete(SQLNumerosFila.NroCajaPropio, SQLNumerosFila.NroHost, SQLNumerosFila.Terminal, SQLNumerosFila.NroLote, SQLNumerosFila.NroTicket, SQLNumerosFila.NroTrace, SQLNumerosFila.PinBlock)
                End If
            Next
            SQLNumerosTableAdapter.Update(SQLNumerosDataTable)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
        End Try
    End Sub


    ''' <summary>
    ''' Obtiene un Numeros, devuelve un datarow del tipo numerosRow, sino lo encuentra devuelve nothing 
    ''' </summary>
    ''' <param name="nrocaja ">Nro de nro caja</param>
    ''' <param name="host ">Nro de host</param>
    ''' <remarks></remarks>
    Public Function ObtenerNumeros(ByVal nrocaja As Integer, ByVal host As Integer) As TarjetasDataSet.NUMEROSRow
        Try
            SQLNumerosDataTable = New TarjetasDataSet.NUMEROSDataTable
            SQLNumerosTableAdapter = New TarjetasDataSetTableAdapters.NUMEROSTableAdapter
            SQLNumerosTableAdapter.Fill(SQLNumerosDataTable)
            SQLNumerosFila = SQLNumerosDataTable.FindByNroCajaPropioNroHost(nrocaja, host)
            If SQLNumerosFila IsNot Nothing Then
                With SQLNumerosFila
                    nro_terminal = .Terminal
                    nro_trace = .NroTrace
                    nro_ticket = .NroTicket
                    nro_lote = .NroLote
                    nro_PINblock = .PinBlock
                End With
            End If
            Return SQLNumerosFila

        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
            Return Nothing
        End Try
    End Function




    Public Function incrementaTrace(ByVal nrocaja As Integer, ByVal host As Integer) As Integer
        Try
            SQLNumerosDataTable = New TarjetasDataSet.NUMEROSDataTable

            SQLNumerosTableAdapter = New TarjetasDataSetTableAdapters.NUMEROSTableAdapter
            SQLNumerosTableAdapter.Fill(SQLNumerosDataTable)

            SQLNumerosFila = SQLNumerosDataTable.FindByNroCajaPropioNroHost(nrocaja, host)

            If SQLNumerosFila IsNot Nothing Then
                SQLNumerosFila.NroTrace = SQLNumerosFila.NroTrace + 1
                SQLNumerosTableAdapter.Update(SQLNumerosFila)
            Else
                Return -1
            End If

            Return SQLNumerosFila.NroTrace
        Catch ex As Exception
            Return -1
        End Try

    End Function



    Public Function incrementaNroTicket(ByVal nrocaja As Integer, ByVal host As Integer) As Integer
        Try
            SQLNumerosDataTable = New TarjetasDataSet.NUMEROSDataTable
            SQLNumerosTableAdapter = New TarjetasDataSetTableAdapters.NUMEROSTableAdapter
            SQLNumerosTableAdapter.Fill(SQLNumerosDataTable)
            SQLNumerosFila = SQLNumerosDataTable.FindByNroCajaPropioNroHost(nrocaja, host)
            If SQLNumerosFila IsNot Nothing Then
                SQLNumerosFila.NroTicket = SQLNumerosFila.NroTicket + 1
                SQLNumerosTableAdapter.Update(SQLNumerosFila)
            Else
                Return -1
            End If

            Return SQLNumerosFila.NroTicket
        Catch ex As Exception
            Return -1
        End Try

    End Function




End Class
