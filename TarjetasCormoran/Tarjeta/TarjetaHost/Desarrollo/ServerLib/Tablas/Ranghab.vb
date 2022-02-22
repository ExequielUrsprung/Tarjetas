Public Class Ranghab
    Dim SQLRanghabDataTable As TarjetasDataSet.RANGHABDataTable
    Dim SQLRanghabTableAdapter As TarjetasDataSetTableAdapters.RANGHABTableAdapter
    Dim SQLRanghabFila As TarjetasDataSet.RANGHABRow


    Dim ODBCRanghabDataTable As ODBC.ranghabDataTable
    Dim ODBCRanghabTableAdapter As ODBCTableAdapters.ranghabTableAdapter
    Dim ODBCRanghabFila As ODBC.ranghabRow

    ''' <summary>
    ''' Sincroniza con la base de datos Progress para actualizar la Tabla Ranghab en la DB Local
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SincronizaRANGHAB()
        Try
            SQLRanghabDataTable = New TarjetasDataSet.RANGHABDataTable
            SQLRanghabTableAdapter = New TarjetasDataSetTableAdapters.RANGHABTableAdapter
            ODBCRanghabDataTable = New ODBC.ranghabDataTable
            ODBCRanghabTableAdapter = New ODBCTableAdapters.ranghabTableAdapter


            SQLRanghabTableAdapter.Fill(SQLRanghabDataTable)
            ODBCRanghabTableAdapter.Fill(ODBCRanghabDataTable)
            For Each ODBCRanghabFila In ODBCRanghabDataTable
                SQLRanghabFila = SQLRanghabDataTable.FindByEmisorRangoDesde(ODBCRanghabFila._nro_emi, ODBCRanghabFila._des_rha)
                If SQLRanghabFila Is Nothing Then
                    SQLRanghabFila = SQLRanghabDataTable.NewRANGHABRow
                    SQLRanghabFila.Emisor = ODBCRanghabFila._nro_emi
                    SQLRanghabFila.RangoDesde = ODBCRanghabFila._des_rha
                    SQLRanghabFila.RangoHasta = ODBCRanghabFila._has_rha
                    SQLRanghabDataTable.AddRANGHABRow(SQLRanghabFila)
                Else
                    SQLRanghabFila.RangoHasta = ODBCRanghabFila._has_rha
                End If
            Next

            For Each SQLRanghabFila In SQLRanghabDataTable
                ODBCRanghabFila = ODBCRanghabDataTable.FindBy_nro_emi_des_rha(SQLRanghabFila.Emisor, SQLRanghabFila.RangoDesde)
                If ODBCRanghabFila Is Nothing Then
                    SQLRanghabTableAdapter.Delete(SQLRanghabFila.Emisor, SQLRanghabFila.RangoDesde, SQLRanghabFila.RangoHasta)
                End If
            Next
            SQLRanghabTableAdapter.Update(SQLRanghabDataTable)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
        End Try
    End Sub

    ''' <summary>
    ''' Obtiene un rango, devuelve un datarow del tipo rangHabRow, sino lo encuentra devuelve nothing 
    ''' </summary>
    ''' <param name="emisor">Nro de emisor</param>
    ''' <param name="rangoDesde">Inicio del rango</param>
    ''' <remarks></remarks>
    Public Function ObtenerRango(ByVal emisor As Integer, ByVal rangoDesde As Integer) As TarjetasDataSet.RANGHABRow
        Try
            SQLRanghabDataTable = New TarjetasDataSet.RANGHABDataTable
            SQLRanghabTableAdapter = New TarjetasDataSetTableAdapters.RANGHABTableAdapter
            SQLRanghabTableAdapter.Fill(SQLRanghabDataTable)
            Return SQLRanghabDataTable.FindByEmisorRangoDesde(emisor, rangoDesde)
        Catch ex As Exception
            MsgBox(ex.Message)
            ' ESCRIBIR LOG DE ERROR
            Return Nothing
        End Try
    End Function


    Public Function ObtenerEmisorDeRanghab(ByVal NumeroTarjeta As Integer) As Integer
        Try
            SQLRanghabDataTable = New TarjetasDataSet.RANGHABDataTable
            SQLRanghabTableAdapter = New TarjetasDataSetTableAdapters.RANGHABTableAdapter
            'SQLRanghabTableAdapter.Fill(SQLRanghabDataTable)
            SQLRanghabTableAdapter.FillByRango(SQLRanghabDataTable, NumeroTarjeta)
            If SQLRanghabTableAdapter Is Nothing Then
                Return 0
            Else
                Return SQLRanghabDataTable.Item(0).Emisor
            End If

        Catch ex As Exception
            MsgBox(ex.Message)
            ' ESCRIBIR LOG DE ERROR
            Return Nothing
        End Try
    End Function



End Class
