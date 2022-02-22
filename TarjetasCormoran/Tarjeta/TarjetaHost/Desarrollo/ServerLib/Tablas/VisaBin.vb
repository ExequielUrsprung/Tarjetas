Public Class VisaBin

    Dim SQLVisabinDataTable As TarjetasDataSet.VISABINDataTable
    Dim SQLVisabinTableAdapter As TarjetasDataSetTableAdapters.VISABINTableAdapter
    Dim SQLVisabinFila As TarjetasDataSet.VISABINRow


    Dim ODBCVisabinDataTable As ODBC.visabinDataTable
    Dim ODBCVisabinTableAdapter As ODBCTableAdapters.visabinTableAdapter
    Dim ODBCVisabinFila As ODBC.visabinRow

    ''' <summary>
    ''' Sincroniza con la base de datos Progress para actualizar la Tabla Visabin en la DB Local
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SincronizaVisabin()
        Try
            SQLVisabinDataTable = New TarjetasDataSet.VISABINDataTable
            SQLVisabinTableAdapter = New TarjetasDataSetTableAdapters.VISABINTableAdapter
            ODBCVisabinDataTable = New ODBC.visabinDataTable
            ODBCVisabinTableAdapter = New ODBCTableAdapters.visabinTableAdapter

            SQLVisabinTableAdapter.Fill(SQLVisabinDataTable)
            ODBCVisabinTableAdapter.Fill(ODBCVisabinDataTable)
            For Each ODBCVisabinFila In ODBCVisabinDataTable
                SQLVisabinFila = SQLVisabinDataTable.FindBybin(ODBCVisabinFila._bin_vbi)
                If SQLVisabinFila Is Nothing Then
                    SQLVisabinFila = SQLVisabinDataTable.NewVISABINRow
                    SQLVisabinFila.bin = ODBCVisabinFila._bin_vbi
                    SQLVisabinDataTable.AddVISABINRow(SQLVisabinFila)
                End If
            Next

            For Each SQLVisabinFila In SQLVisabinDataTable
                ODBCVisabinFila = ODBCVisabinDataTable.FindBy_bin_vbi(SQLVisabinFila.bin)
                If ODBCVisabinFila Is Nothing Then
                    SQLVisabinTableAdapter.Delete(SQLVisabinFila.bin)
                End If
            Next
            SQLVisabinTableAdapter.Update(SQLVisabinDataTable)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
        End Try
    End Sub

    ''' <summary>
    ''' Obtiene un bin, devuelve un datarow del tipo VisabinRow, sino lo encuentra devuelve nothing 
    ''' </summary>
    ''' <param name="Bin">Nro. Bin</param>
    ''' <remarks></remarks>
    Public Function ObtenerBin(ByVal bin As Short) As TarjetasDataSet.VISABINRow
        Try
            SQLVisabinDataTable = New TarjetasDataSet.VISABINDataTable
            SQLVisabinTableAdapter = New TarjetasDataSetTableAdapters.VISABINTableAdapter
            SQLVisabinTableAdapter.Fill(SQLVisabinDataTable)
            Return SQLVisabinDataTable.FindBybin(bin)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
            Return Nothing
        End Try
    End Function




End Class
