Public Class Emisor

    Dim SQLEmisorDataTable As TarjetasDataSet.EMISORDataTable
    Dim SQLEmisorTableAdapter As TarjetasDataSetTableAdapters.EMISORTableAdapter
    Dim SQLEmisorFila As TarjetasDataSet.EMISORRow


    Dim ODBCEmisorDataTable As ODBC.emisorDataTable
    Dim ODBCEmisorTableAdapter As ODBCTableAdapters.emisorTableAdapter
    Dim ODBCEmisorFila As ODBC.emisorRow

#Region "Propiedades"
    Dim host As Integer
    Public ReadOnly Property nrohost()

        Get
            Return host
        End Get
    End Property

    Dim nrocomercioRAF As String
    Public ReadOnly Property idcomercioRAF()
        Get
            Return nrocomercioRAF
        End Get
    End Property
    Dim nrocomercioSF As String
    Public ReadOnly Property idcomercioSF()
        Get
            Return nrocomercioSF
        End Get
    End Property

    Dim nroemisor As Integer
    Public ReadOnly Property emisor()
        Get
            Return nroemisor
        End Get
    End Property

    Dim descripcion As String
    Public ReadOnly Property descEmisor()
        Get
            Return descripcion
        End Get
    End Property
    Dim ult4dig As Boolean
    Public ReadOnly Property cuatrodigitos()
        Get
            Return ult4dig
        End Get
    End Property
    Dim seguridad As Boolean
    Public ReadOnly Property codSeg()
        Get
            Return seguridad
        End Get
    End Property
    Dim vencimiento As Boolean
    Public ReadOnly Property fecVencimiento()
        Get
            Return vencimiento
        End Get
    End Property

#End Region


    ''' <summary>
    ''' ''' Sincroniza con la base de datos Progress para actualizar la Tabla Emisor en la DB Local
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SincronizaEMISOR()
        Try
            SQLEmisorDataTable = New TarjetasDataSet.EMISORDataTable
            SQLEmisorTableAdapter = New TarjetasDataSetTableAdapters.EMISORTableAdapter
            ODBCEmisorDataTable = New ODBC.emisorDataTable
            ODBCEmisorTableAdapter = New ODBCTableAdapters.emisorTableAdapter


            SQLEmisorTableAdapter.Fill(SQLEmisorDataTable)
            ODBCEmisorTableAdapter.Fill(ODBCEmisorDataTable)
            For Each ODBCEmisorFila In ODBCEmisorDataTable
                SQLEmisorFila = SQLEmisorDataTable.FindByEmisor(ODBCEmisorFila._nro_emi)
                If SQLEmisorFila Is Nothing Then
                    SQLEmisorFila = SQLEmisorDataTable.NewEMISORRow
                    SQLEmisorFila.Emisor = ODBCEmisorFila._nro_emi
                    SQLEmisorFila.Descripcion = ODBCEmisorFila._des_emi
                    SQLEmisorFila.Host = ODBCEmisorFila._hos_emi
                    SQLEmisorFila.IDComercioRaf = ODBCEmisorFila._icr_emi
                    SQLEmisorFila.IDComercioSF = ODBCEmisorFila._ics_emi
                    SQLEmisorFila.ReqCodSeguridad = ODBCEmisorFila._seg_emi
                    SQLEmisorFila.Req4Digitos = ODBCEmisorFila._cua_emi
                    SQLEmisorFila.ReqFechaVenc = ODBCEmisorFila._ven_emi

                    SQLEmisorDataTable.AddEMISORRow(SQLEmisorFila)
                Else
                    SQLEmisorFila.Descripcion = ODBCEmisorFila._des_emi
                    SQLEmisorFila.Host = ODBCEmisorFila._hos_emi
                    SQLEmisorFila.IDComercioRaf = ODBCEmisorFila._icr_emi
                    SQLEmisorFila.IDComercioSF = ODBCEmisorFila._ics_emi
                    SQLEmisorFila.ReqCodSeguridad = ODBCEmisorFila._seg_emi
                    SQLEmisorFila.Req4Digitos = ODBCEmisorFila._cua_emi
                    SQLEmisorFila.ReqFechaVenc = ODBCEmisorFila._ven_emi
                End If
            Next

            For Each SQLEmisorFila In SQLEmisorDataTable
                ODBCEmisorFila = ODBCEmisorDataTable.FindBy_nro_emi(SQLEmisorFila.Emisor)
                If ODBCEmisorFila Is Nothing Then
                    With SQLEmisorFila
                        SQLEmisorTableAdapter.Delete(.Emisor, _
                                                     .Descripcion, _
                                                     .Host, _
                                                     .IDComercioRaf, _
                                                     .IDComercioSF, _
                                                     .Req4Digitos, _
                                                     .ReqCodSeguridad, _
                                                     .ReqFechaVenc)
                    End With

                End If
            Next
            SQLEmisorTableAdapter.Update(SQLEmisorDataTable)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
        End Try
    End Sub

    ''' <summary>
    ''' Obtiene un emisor, devuelve un datarow del tipo emisorRow, sino lo encuentra devuelve nothing 
    ''' </summary>
    ''' <param name="emisor">Nro de emisor</param>
    ''' <remarks></remarks>
    Public Function ObtenerEmisor(ByVal emisor As Integer) As TarjetasDataSet.EMISORRow
        Try
            SQLEmisorDataTable = New TarjetasDataSet.EMISORDataTable
            SQLEmisorTableAdapter = New TarjetasDataSetTableAdapters.EMISORTableAdapter

            SQLEmisorTableAdapter.Fill(SQLEmisorDataTable)
            SQLEmisorFila = SQLEmisorDataTable.FindByEmisor(emisor)
            If SQLEmisorFila IsNot Nothing Then
                host = SQLEmisorFila.Host
                nrocomercioRAF = SQLEmisorFila.IDComercioRaf
                nrocomercioSF = SQLEmisorFila.IDComercioSF
                nroemisor = SQLEmisorFila.Emisor
                descripcion = SQLEmisorFila.Descripcion
                ult4dig = SQLEmisorFila.Req4Digitos
                seguridad = SQLEmisorFila.ReqCodSeguridad
                vencimiento = SQLEmisorFila.ReqFechaVenc
            End If

            Return SQLEmisorFila
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
            Return Nothing
        End Try
    End Function



End Class
