Public Class Hosts
    Dim SQLHostsDataTable As TarjetasDataSet.HOSTSDataTable
    Dim SQLHostsTableAdapter As TarjetasDataSetTableAdapters.HOSTSTableAdapter
    Dim SQLHostsFila As TarjetasDataSet.HOSTSRow


    Dim ODBCHostsDataTable As ODBC.hostsDataTable
    Dim ODBCHostsTableAdapter As ODBCTableAdapters.hostsTableAdapter
    Dim ODBCHostsFila As ODBC.hostsRow

    ''' <summary>
    ''' Sincroniza con la base de datos Progress para actualizar la Tabla Hosts en la DB Local
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SincronizaHosts()
        Try
            SQLHostsDataTable = New TarjetasDataSet.HOSTSDataTable
            SQLHostsTableAdapter = New TarjetasDataSetTableAdapters.HOSTSTableAdapter
            ODBCHostsDataTable = New ODBC.hostsDataTable
            ODBCHostsTableAdapter = New ODBCTableAdapters.hostsTableAdapter


            SQLHostsTableAdapter.Fill(SQLHostsDataTable)
            ODBCHostsTableAdapter.Fill(ODBCHostsDataTable)
            For Each ODBCHostsFila In ODBCHostsDataTable
                SQLHostsFila = SQLHostsDataTable.FindByNroHost(ODBCHostsFila._nro_hos)
                If SQLHostsFila Is Nothing Then
                    SQLHostsFila = SQLHostsDataTable.NewHOSTSRow
                    SQLHostsFila.NroHost = ODBCHostsFila._nro_hos
                    SQLHostsFila.VersionSoft = ODBCHostsFila._ver_hos
                    SQLHostsFila.IDComercioCierre = ODBCHostsFila._com_hos
                    SQLHostsFila.Descripcion = ODBCHostsFila._des_hos
                    SQLHostsFila.Direccion1 = ODBCHostsFila._di1_hos
                    SQLHostsFila.Direccion2 = ODBCHostsFila._di2_hos
                    SQLHostsFila.Puerto1 = ODBCHostsFila._pu1_hos
                    SQLHostsFila.Puerto2 = ODBCHostsFila._pu2_hos

                    SQLHostsDataTable.AddHOSTSRow(SQLHostsFila)
                Else
                    SQLHostsFila.VersionSoft = ODBCHostsFila._ver_hos
                    SQLHostsFila.IDComercioCierre = ODBCHostsFila._com_hos
                    SQLHostsFila.Descripcion = ODBCHostsFila._des_hos
                    SQLHostsFila.Direccion1 = ODBCHostsFila._di1_hos
                    SQLHostsFila.Direccion2 = ODBCHostsFila._di2_hos
                    SQLHostsFila.Puerto1 = ODBCHostsFila._pu1_hos
                    SQLHostsFila.Puerto2 = ODBCHostsFila._pu2_hos
                End If
            Next

            For Each SQLHostsFila In SQLHostsDataTable
                ODBCHostsFila = ODBCHostsDataTable.FindBy_nro_hos(SQLHostsFila.NroHost)
                If ODBCHostsFila Is Nothing Then
                    SQLHostsTableAdapter.Delete(SQLHostsFila.NroHost, SQLHostsFila.Descripcion, SQLHostsFila.Direccion1, SQLHostsFila.Puerto1, SQLHostsFila.Direccion2, SQLHostsFila.Puerto2, SQLHostsFila.IDComercioCierre, SQLHostsFila.VersionSoft)
                End If
            Next
            SQLHostsTableAdapter.Update(SQLHostsDataTable)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
        End Try
    End Sub
    ''' <summary>
    ''' Obtiene un host, devuelve un datarow del tipo hostRow, sino lo encuentra devuelve nothing 
    ''' </summary>
    ''' <param name="host ">Nro de host</param>
    ''' <remarks></remarks>
    Public Function ObtenerNumeros(ByVal host As Integer) As TarjetasDataSet.HOSTSRow
        Try
            SQLHostsDataTable = New TarjetasDataSet.HOSTSDataTable
            SQLHostsTableAdapter = New TarjetasDataSetTableAdapters.HOSTSTableAdapter
            SQLHostsTableAdapter.Fill(SQLHostsDataTable)
            Return SQLHostsDataTable.FindByNroHost(host)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
            Return Nothing
        End Try
    End Function
End Class
