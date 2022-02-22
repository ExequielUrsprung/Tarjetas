Public Class Planes
    Dim SQLPlanesDataTable As TarjetasDataSet.PLANESDataTable
    Dim SQLPlanesTableAdapter As TarjetasDataSetTableAdapters.PLANESTableAdapter
    Dim SQLPlanesFila As TarjetasDataSet.PLANESRow


    Dim ODBCPlanesDataTable As ODBC.planesDataTable
    Dim ODBCPlanesTableAdapter As ODBCTableAdapters.planesTableAdapter
    Dim ODBCPlanesFila As ODBC.planesRow

    ''' <summary>
    ''' Sincroniza con la base de datos Progress para actualizar la Tabla Planes en la DB Local
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SincronizaPlanes()
        Try
            SQLPlanesDataTable = New TarjetasDataSet.PLANESDataTable
            SQLPlanesTableAdapter = New TarjetasDataSetTableAdapters.PLANESTableAdapter
            ODBCPlanesDataTable = New ODBC.planesDataTable
            ODBCPlanesTableAdapter = New ODBCTableAdapters.planesTableAdapter


            SQLPlanesTableAdapter.Fill(SQLPlanesDataTable)
            ODBCPlanesTableAdapter.Fill(ODBCPlanesDataTable)
            For Each ODBCPlanesFila In ODBCPlanesDataTable
                SQLPlanesFila = SQLPlanesDataTable.FindByEmisorNroPlan(ODBCPlanesFila._nro_emi, ODBCPlanesFila._nro_pla)
                If SQLPlanesFila Is Nothing Then
                    SQLPlanesFila = SQLPlanesDataTable.NewPLANESRow
                    SQLPlanesFila.Emisor = ODBCPlanesFila._nro_emi
                    SQLPlanesFila.NroPlan = ODBCPlanesFila._nro_pla
                    SQLPlanesFila.Moneda = ODBCPlanesFila._mon_pla
                    SQLPlanesFila.Cuotas = ODBCPlanesFila._cuo_pla
                    SQLPlanesFila.Coeficiente = ODBCPlanesFila._coe_pla
                    SQLPlanesFila.DescPlan = Mid(ODBCPlanesFila._des_pla, 1, 25)

                    SQLPlanesDataTable.AddPLANESRow(SQLPlanesFila)
                Else
                    SQLPlanesFila.Moneda = ODBCPlanesFila._mon_pla
                    SQLPlanesFila.Cuotas = ODBCPlanesFila._cuo_pla
                    SQLPlanesFila.Coeficiente = ODBCPlanesFila._coe_pla
                    SQLPlanesFila.DescPlan = Mid(ODBCPlanesFila._des_pla, 1, 25)

                End If
            Next

            For Each SQLPlanesFila In SQLPlanesDataTable
                ODBCPlanesFila = ODBCPlanesDataTable.FindBy_nro_emi_nro_pla(SQLPlanesFila.Emisor, SQLPlanesFila.NroPlan)
                If ODBCPlanesFila Is Nothing Then
                    SQLPlanesTableAdapter.Delete(SQLPlanesFila.Emisor, SQLPlanesFila.NroPlan, SQLPlanesFila.Cuotas, _
                                                 SQLPlanesFila.Moneda, SQLPlanesFila.DescPlan, _
                                                 SQLPlanesFila.Coeficiente)
                End If
            Next
            SQLPlanesTableAdapter.Update(SQLPlanesDataTable)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
        End Try
    End Sub

    ''' <summary>
    ''' Obtiene un plan, devuelve un datarow del tipo planesRow, sino lo encuentra devuelve nothing 
    ''' </summary>
    ''' <param name="emisor">Nro de emisor</param>
    ''' <param name="plan">Nro de plan</param>
    ''' <remarks></remarks>
    Public Function ObtenerPlan(ByVal emisor As Integer, ByVal plan As Integer) As TarjetasDataSet.PLANESRow
        Try
            SQLPlanesDataTable = New TarjetasDataSet.PLANESDataTable
            SQLPlanesTableAdapter = New TarjetasDataSetTableAdapters.PLANESTableAdapter
            SQLPlanesTableAdapter.Fill(SQLPlanesDataTable)
            Return SQLPlanesDataTable.FindByEmisorNroPlan(emisor, plan)
        Catch ex As Exception
            MsgBox(ex.Message)
            'ESCRIBIR LOG DE ERROR
            Return Nothing
        End Try
    End Function
End Class
