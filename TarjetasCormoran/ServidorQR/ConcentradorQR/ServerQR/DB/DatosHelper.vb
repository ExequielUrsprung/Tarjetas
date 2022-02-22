' aca poner todas las operaciones de la DB
Imports System.Linq
Imports System.IO


Public Module DatosHelper

#Region "Actualizaciones de tablas"

    Private Param As New Parametros
    Public Function Configuracion() As Parametros
        Return Param
    End Function



    Public Function Buscar_Suc_id(ext_id As String) As String
        Dim dt As New QRDataSetTableAdapters.SucursalesQRTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where Trim(c.External_store_id) = ext_id
                         Select c

            If result.Count = 1 Then
                Return Trim(result(0).Store_id)
            Else
                Return ""
            End If
        Catch
            Return ""
            Logger.Error(String.Format("No se pudo buscar la sucursal con el id externo: {0}", ext_id))
        End Try
    End Function



    Public Sub ModificaEstadoQR(externalReference As String, estado As String, _ipn As Boolean)
        Dim dt As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where Trim(c.external_pos_id) = externalReference.Split("-")(0) And c.external_reference = CInt(externalReference.Split("-")(1))
                         Select c

            For Each reg In result
                reg.FechaHoraUltima = Now
                If reg.status.Trim = lib_QR.Estado.OPENED.ToString And estado = lib_QR.Estado.OPENED.ToString Then

                Else
                    If reg.status.Trim <> lib_QR.Estado.CLOSED.ToString And reg.status.Trim <> lib_QR.Estado.CANCELLED.ToString Then
                        reg.status = estado
                        'reg.FechaHoraUltima = Now
                        If estado = lib_QR.Estado.OPENED.ToString Then
                            reg.ipn = _ipn
                            reg.FechaHoraOpen = Now
                        End If
                    Else
                        Logger.Error(String.Format("Esta intentando cerrar o cancelar un movimiento finalizado, no se modificó: {0}", externalReference))
                    End If
                End If

                dt.Update(reg)
            Next
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo modificar el estado de la tansaccion con external_reference: {0}", externalReference))
        End Try
    End Sub
    Public Sub ModificaTransaccionQR(req As ReqQR)
        Dim dt As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where Trim(c.external_pos_id) = req.external_id And c.external_reference = req.external_ref
                         Select c

            For Each reg In result
                reg.idOperacion = req.vta.VtaIdOperacion
                reg.idUsuario = req.vta.VtaIdUsuario
                reg.mail = req.vta.VtaMail
                reg.idPago = req.idPago
                reg.FechaHoraCierre = Now
                reg.FechaHoraUltima = Now
                dt.Update(reg)
            Next
        Catch ex As Exception
            Logger.Error(String.Format("No se pudieron agregar info de la tansaccion con external_reference: {0}|{1}|{2}", req.external_ref, req.pinguino, req.nroCaja))
        End Try
    End Sub


    Private Function separar_por_grupos(dt As QRDataSetTableAdapters.TransaccionesQRTableAdapter) As IEnumerable
        Dim queryr = From c In dt.GetData()
                     Group c By c.external_pos_id
                     Into g = Group
                     Select g.OrderByDescending(Function(p) p.FechaHora).First()
        queryr = queryr.Where(
                    Function(d)
                        Return (Trim(d.status) = lib_QR.Estado.OPENED.ToString Or
                                Trim(d.status) = lib_QR.Estado.CREATED.ToString Or
                                Trim(d.status) = lib_QR.Estado.NORESPONSE.ToString) And
                                Trim(d.tipoOperacion) <> "DEVOLUCION"
                    End Function)
        Return queryr
    End Function



    Public Function BuscarTransaccionQR_Atrasadas(to_search As Integer) As List(Of String)
        Dim lista As New List(Of String)
        Dim dt As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Try
            'Dim result = From c In dt.GetData()
            'Where Trim(c.status) = lib_QR.Estado.OPENED.ToString Or Trim(c.status) = lib_QR.Estado.CREATED.ToString Or Trim(c.status) = lib_QR.Estado.NORESPONSE.ToString And Trim(c.tipoOperacion) <> "DEVOLUCION"
            'Select Case c

            Dim result = separar_por_grupos(dt)

            For Each reg In result
                Dim modificar As Boolean = False
                If Now.Subtract(reg.FechaHora).TotalMinutes > 5 Then ' mayor a 10 minutos de creado lo cancela.
                    reg.status = lib_QR.Estado.CANCELLED.ToString
                    dt.Update(reg)
                    Continue For
                End If
                If reg.ipn = False Then 'nunca llego ipn

                    If Now.Subtract(reg.FechaHoraUltima).TotalSeconds > to_search Then 'va 50
                        lista.Add(Trim(reg.external_pos_id) & "-" & CInt(reg.external_reference).ToString("000000"))
                        reg.FechaHoraUltima = Now
                        'reg.ipn = True
                        dt.Update(reg)
                    End If
                    'ElseIf Trim(reg.status) = lib_QR.Estado.OPENED.ToString Then ' a los 50 segundos de abierto lanza el search.
                    '    If Now.Subtract(reg.FechaHoraUltima).TotalSeconds > 10 Then 'va 50
                    '        lista.Add(Trim(reg.external_pos_id) & "-" & reg.external_reference.ToString("000000"))
                    '        reg.FechaHoraUltima = Now
                    '        'reg.ipn = True
                    '        dt.Update(reg)

                    '    End If
                Else
                    If Now.Subtract(reg.FechaHoraUltima).TotalSeconds > 15 Then '10 seg desde el ultimo search.
                        lista.Add(Trim(reg.external_pos_id) & "-" & CInt(reg.external_reference).ToString("000000"))
                        reg.FechaHoraUltima = Now
                        dt.Update(reg)
                        'modificar = True
                    End If
                    'If modificar Then
                    '    'reg.FechaHoraUltima = Now
                    '    dt.Update(reg)
                    'End If

                End If

            Next

        Catch ex As Exception
            Logger.Error("No se pudieron buscar tansacciones " & ex.Message)
        End Try
        Return lista
    End Function


    Public Sub BuscarTransaccionQR(rq As ReqQR)

        Dim dt As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where Trim(c.external_pos_id) = rq.external_id And c.external_reference = rq.external_ref
                         Select c

            For Each reg In result
                rq.estado = [Enum].Parse(GetType(lib_QR.Estado), reg.status.ToUpper)
                rq.ticket = Trim(reg.ticket)
                rq.pos_id = reg.external_pos_id
                rq.pinguino = reg.pinguino.ToString("00")
                rq.nroCaja = reg.caja
                rq.importe = reg.importe
                rq.fecha = reg.FechaHora
                rq.cajero = reg.cajero
            Next

        Catch
            Logger.Error(String.Format("Error al buscar la transaccion con external_reference: {0}", rq.external_ref))
        End Try
    End Sub

    Public Sub AgregarMovimientoQR(req As ReqQR)
        Dim cont As Int16 = 0
        Dim datosDS As New QRDataSet
        Dim datostjTA As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Dim tablalocal As New QRDataSet.TransaccionesQRDataTable
        Try
            'datostjTA.Fill(tablalocal)
            '--- DA DE ALTA UN REGISTRO EN TRANSACCIONES, PONE EN ESTATUS "CREATED"
            Dim fila = datosDS.TransaccionesQR.NewTransaccionesQRRow
            With fila
                .tipoOperacion = req.ida.TIPOOPERACION
                .external_pos_id = req.external_id
                .external_reference = req.external_ref
                .FechaHora = Now
                .importe = CDec(req.ida.IMPORTE) / 100
                .status = lib_QR.Estado.CREATED.ToString
                .ticket = req.ida.ticket
                .cajero = req.ida.CAJERO
                .caja = req.nroCaja
                .pinguino = CInt(req.pinguino)
                .FechaHoraUltima = Now
                .idUsuario = ""
                .idOperacion = ""
                .idPago = ""
                .mail = ""
                .ipn = False
            End With
            datosDS.TransaccionesQR.AddTransaccionesQRRow(fila)
            datostjTA.Update(fila)
        Catch ex As Exception
            Logger.Error(String.Format("Error al registrar el movimiento {0}/{1}/{2}", req.external_id, req.external_ref, Trim(req.external_id)) & vbNewLine &
                         ex.Message)
        End Try
    End Sub

#End Region



#Region "Busqueda en tablas"


    ''' <summary>
    ''' Busca la terminal en la tabla Numeros.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub BuscarTerminalMP(pCaja As Integer, pPinguino As Integer, rq As ReqQR)
        Dim term As String = ""
        Dim dt As New QRDataSetTableAdapters.TerminalesQRTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where c.Pinguino = pPinguino And c.Caja = pCaja
                         Select c

            For Each reg In result
                rq.pos_id = reg.Pos_id.Trim
                rq.external_id = reg.external_id.Trim
                If rq.ida.TIPOOPERACION = "COMPRA" Or rq.ida.TIPOOPERACION = "DEVOLUCION" Then
                    rq.external_ref = reg.external_reference
                Else
                    rq.external_ref = reg.external_reference - 1
                End If
            Next

        Catch ex As Exception
            Logger.Error("No se pudo buscar terminal.")

        End Try
    End Sub
    ''' <summary>
    ''' Busca la terminal en la tabla Numeros.
    ''' </summary>
    ''' <remarks></remarks>
    Public Function BuscarIDTerminalMP(pCaja As String) As String
        Dim term As String = ""
        Dim dt As New QRDataSetTableAdapters.TerminalesQRTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where c.external_id.Trim = pCaja.Trim
                         Select c


            If result.Count > 0 Then
                term = result(0).Pos_id
            End If

        Catch ex As Exception
            Logger.Error("No se pudo buscar terminal.")
            term = ""

        End Try
        Return term.Trim
    End Function


    Public Function Logger() As log4net.ILog
        Return log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    End Function




#Region "Generación de reportes"
    Public Sub ModificaTransaccionQR(renglon As String())
        Dim dt As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where c.caja = CByte(renglon(1)) And c.pinguino = CByte(renglon(0)) And c.ticket.Trim = renglon(5) And CDec(c.importe) = CDec(renglon(6))
                         Select c

            For Each reg In result
                Logger.Info(reg.importe)
                If reg.status.Trim <> lib_QR.Estado.CLOSED.ToString Then
                    reg.idPago = renglon(4)
                    reg.status = lib_QR.Estado.CLOSED.ToString
                    Try
                        reg.FechaHoraCierre = CDate(renglon(2) & " " & renglon(3))

                    Catch
                    End Try
                    Logger.Info(String.Format("Se corrigió el mov. de la Caja: {0} Ping: {1} Ticket: {2} Importe: {3}", renglon(1), renglon(0), renglon(5), renglon(6)))
                    dt.Update(reg)
                End If

                'reg.idOperacion = req.vta.VtaIdOperacion
                'reg.idUsuario = req.vta.VtaIdUsuario
                'reg.mail = req.vta.VtaMail
                'reg.idPago = req.idPago
                'reg.FechaHoraCierre = Now
                'reg.FechaHoraUltima = Now

            Next
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo modificar el movimiento de la Caja: {0} Ping: {1} Ticket: {2}", renglon(1), renglon(0), renglon(5)))
        End Try
    End Sub

    Public Sub Corregir_movimientos()
        Dim arc As FileIO.TextFieldParser
        Try
            If IO.File.Exists(Configuracion.DirTarjetas & "\manual.err") Then
                Dim renglon As String()
                arc = New FileIO.TextFieldParser(Configuracion.DirTarjetas & "\manual.err")
                arc.TextFieldType = FileIO.FieldType.Delimited
                arc.SetDelimiters(";")
                Dim hay As Boolean = False
                While Not arc.EndOfData
                    hay = True
                    renglon = arc.ReadFields
                    ModificaTransaccionQR(renglon)

                End While
                If hay Then
                    Logger.Warn("Existían movimientos a corregir.")
                Else
                    Logger.Info("No habia moviemientos para corregir.")
                End If


            End If
        Catch ex As Exception
            Logger.Error("Hubo un problema corrigiendo los renglones. " & vbNewLine &
                         "Ex.Message: " & ex.Message)
        Finally
            If arc IsNot Nothing Then arc.Close()
            If IO.File.Exists(Configuracion.DirTarjetas & "\manual.err") Then IO.File.Move(Configuracion.DirTarjetas & "\manual.err", Configuracion.DirTarjetas & "\CIERRES\manual.err." & Now.ToString("yyyyMMdd"))
        End Try
    End Sub


    ''' <summary>
    ''' Arma el .QR con los movimientos aprobados
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CulminacionCierreSatisfactoriaQR(fecha_cierre As Date)
        Dim linea As String
        Dim dt As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Dim res0200 = From c In dt.GetData()
                      Where c.status.Trim = "CLOSED"
                      Select c
        Dim nombrearc As String

        If Configuracion.Ciudad = Configuracion.SanFrancisco Then
            nombrearc = fecha_cierre.ToString("yyMMdd") + fecha_cierre.ToString("HHmmss") + ".QR6"
        Else
            nombrearc = fecha_cierre.ToString("yyMMdd") + fecha_cierre.ToString("HHmmss") + ".QR1"
        End If
        Logger.Info("Generando arhivo para importacion: " & nombrearc)

        For Each reg0200 In res0200

            With reg0200
                Try
                    linea = $"{ .caja};{ .pinguino};{ .ticket};{ .importe};{ .idPago};{ .idUsuario};{BuscarIDTerminalMP(.external_pos_id)}"
                Catch ex As Exception
                    Logger.Warn("Error en el cierre del movimeinto de la caja " & .caja & " ticket: " & .ticket & " Importe: " & .importe)
                    linea = ""
                End Try
            End With
            If linea <> "" Then
                EscribeArchivoCierre(nombrearc, linea)
            End If
        Next

        Logger.Info("Archivo de importacion: " & nombrearc & " generado.")
    End Sub
    Public Sub RespaldarMovimientos()

        Dim dt As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Dim result = From c In dt.GetData
                     Select c
        Dim fileMovimientos As FileStream
        Dim renglon As StreamWriter
        Try
            If result.Count > 0 Then
                If Directory.Exists(Configuracion.DirTarjetas & "\respaldos\") Then
                    fileMovimientos = New FileStream(Configuracion.DirTarjetas & "\respaldos\movQR" + Now.Date.ToString("yyMMdd") + ".bkp", FileMode.Create, FileAccess.Write)
                Else
                    Logger.Error("No existe el directorio para respaldar los movimientos. Se usa el directorio alternativo.")
                    fileMovimientos = New FileStream("C:\temp\respaldos\movQR" + Now.Date.ToString("yyMMdd") + ".bkp", FileMode.Create, FileAccess.Write)
                End If


                Logger.Info("Respaldando movimientos...")

                renglon = New StreamWriter(fileMovimientos)
                renglon.AutoFlush = True
                Dim x As Integer = 0
                Try
                    Do While result(0)(x) IsNot Nothing
                        x += 1
                    Loop
                Catch ex As Exception
                    x -= 1
                End Try

                For Each reg In result
                    For i As Integer = 0 To x
                        renglon.Write(reg(i).ToString + ";")
                    Next
                    renglon.Write(vbNewLine)
                Next

                Logger.Info("Respaldo movimientos finalizado.")
            End If
        Catch ex1 As IO.IOException
            Logger.Error("Falló el respaldo de uno o mas registros de la tabla movimientos." & vbNewLine &
                         ex1.Message)

        Catch ex As Exception
            Logger.Error("Falló el respaldo de uno o mas registros de la tabla movimientos." & vbNewLine &
                         ex.Message)
        Finally
            If fileMovimientos IsNot Nothing Then fileMovimientos.Close()
        End Try

    End Sub

    Public Function ImportarMovimientos(archivo As String) As Int16

        Dim cont As Int16 = 0
        Dim datosDS As New QRDataSet
        Dim datostjTA As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Dim tablalocal As New QRDataSet.TransaccionesQRDataTable
        Dim arc As FileIO.TextFieldParser
        Dim renglon As String()

        Try

            arc = New FileIO.TextFieldParser(archivo)
            arc.TextFieldType = FileIO.FieldType.Delimited
            arc.SetDelimiters(";")

            Logger.Info("Importando Movimientos")
            While Not arc.EndOfData
                Try
                    renglon = arc.ReadFields
                    Logger.Info(renglon)
                    datostjTA.Fill(tablalocal)
                    Dim fila = datosDS.TransaccionesQR.NewTransaccionesQRRow
                    With fila
                        .external_pos_id = renglon(0)
                        .external_reference = renglon(1)
                        .importe = renglon(2)
                        .ticket = renglon(3)
                        If renglon(4) <> "" Then .FechaHora = renglon(4)
                        .status = renglon(5)
                        .idOperacion = renglon(6)
                        .idUsuario = renglon(7)
                        .mail = renglon(8)
                        .caja = renglon(9)
                        .pinguino = renglon(10)
                        .cajero = renglon(11)
                        If renglon(12) <> "" Then .FechaHoraOpen = renglon(12)
                        .ipn = renglon(13)
                        If renglon(14) <> "" Then .FechaHoraUltima = renglon(14)
                        .idPago = renglon(15)
                        If renglon(16) <> "" Then .FechaHoraCierre = renglon(16)
                        .tipoOperacion = renglon(17)
                    End With
                    datosDS.TransaccionesQR.AddTransaccionesQRRow(fila)
                    datostjTA.Update(fila)
                    cont += 1
                Catch ex As Exception
                    Logger.Error(String.Format("No se pudo importar el movimiento {0}/{1}/{2}", renglon(0), renglon(1), renglon(2)))
                End Try

            End While

        Catch ex As Exception
            Logger.Error(ex.Message)
        End Try
        Return cont


    End Function
    Public Sub Eliminar_Archivos(fecha As Date)
        Try
            If Configuracion.TipoConfiguracion = Configuracion.Produccion Then
                Logger.Info("Borrando archivo de respaldo movQR" & fecha.Date.ToString("yyMMdd") & ".bkp.")

                If File.Exists(Configuracion.DirTarjetas & "\respaldos\movQR" + fecha.Date.ToString("yyMMdd") + ".bkp") Then File.Delete(Configuracion.DirTarjetas & "\respaldos\movQR" + fecha.Date.ToString("yyMMdd") + ".bkp")
                If File.Exists("C:\temp\respaldos\movQR" + fecha.Date.ToString("yyMMdd") + ".bkp") Then File.Delete("C:\temp\respaldos\movQR" + fecha.Date.ToString("yyMMdd") + ".bkp")
            End If
        Catch ex As Exception
            Logger.Error("No se pudieron borrar los archivos de respaldo.")
        End Try

    End Sub


    Public Sub depurar_duplicados()

        Dim dt As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Logger.Info("Depurando tabla Transacciones ...")
        '--- TODOS LOS PINGUINOS ----------------
        Try
            Dim res0200 = From c In dt.GetData(), d In dt.GetData()
                          Where c.external_pos_id = d.external_pos_id And c.importe = d.importe And c.ticket = d.ticket And c.status.Trim = "CLOSED" And d.status.Trim = "CLOSED" And c.external_reference <> d.external_reference
                          Select c


            If res0200.Count > 0 Then
                Dim cant = res0200.Count
                For Each reg0200 In res0200

                    If cant = 1 Then Exit For
                    If reg0200.idOperacion.Trim = "" Or reg0200.idUsuario.Trim = "" Then
                        Logger.Info($"Eliminando movimiento duplicado {reg0200.external_pos_id.Trim} | {reg0200.external_reference} | {reg0200.ticket.Trim} | {reg0200.importe} | {reg0200.status.Trim}")
                        dt.DeleteRenglon(reg0200.external_pos_id, reg0200.external_reference)
                        cant = cant - 1

                    End If
                Next
            End If
        Catch ex As Exception
            Logger.Error($"Error depurando tabla transaccionesqr 
                         Message: {ex.Message}
                         Source:  {ex.Source}
                         StackTrace: {ex.StackTrace}")
        End Try

    End Sub

    Public Sub ListadosReportesTarjetasQR(ByVal fecha_cierre As Date)
        Dim hayalgo As Boolean = False
        Dim adjuntos As String
        Dim linea As String
        Dim lacanti As Integer
        Dim eltot As Decimal
        Dim nombrearc2 As String

        Dim dt As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Logger.Info("Generando reporte QR tarjetas global...")
        '--- TODOS LOS PINGUINOS ----------------
        Dim res0200 = From c In dt.GetData()
                      Where c.status.Trim = "CLOSED"
                      Order By c.pinguino, c.caja
                      Select c

        If res0200.Count = 0 Then
            Logger.Info("No hay movimientos para informar. " & fecha_cierre.ToString("dd/MM/yy"))
            Exit Sub
        End If

        nombrearc2 = "ReporteQRMov_" & fecha_cierre.ToString("yyMMdd") + fecha_cierre.ToString("HHmmss") + ".txt"
        adjuntos = nombrearc2 + ";"

        linea = "REPORTE GENERADO: " + Format(fecha_cierre, "dd/MM/yy") + "   " + fecha_cierre.ToString("HH:mm")
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "-------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "PING      CAJA           IMPORTE       CAJERA        FEC/HORA TRANS.  ID OPERACION   ID USUARIO"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "-------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)

        lacanti = 0
        eltot = 0
        For Each reg0200 In res0200

            hayalgo = True
            Try
                linea = $" {reg0200.pinguino.ToString("00")}        {reg0200.caja.ToString("00")}       {reg0200.importe.ToString.PadLeft(12)}         {reg0200.cajero.ToString.PadLeft(4)}        {Format(reg0200.FechaHora, "dd/MM/yy")} {Format(reg0200.FechaHora, "HH:mm")}   {reg0200.idOperacion.Trim.PadLeft(12)} {reg0200.idUsuario.Trim.PadLeft(12)}"
                EscribeArchivoCierre(nombrearc2, linea)

                lacanti = lacanti + 1
                eltot += reg0200.importe
            Catch ex As Exception
                Logger.Warn("Se produjo un error generando el reporte para la caja: " & reg0200.caja.ToString("00") & " Ping: " & reg0200.pinguino.ToString("00") & " Imp: " & reg0200.importe)
            End Try
        Next
        linea = "---------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = $"TOTAL GENERAL   {eltot.ToString("##,###,##0.00").PadLeft(15)}        CANT. MOV. {lacanti.ToString.PadLeft(7)} "
        EscribeArchivoCierre(nombrearc2, linea)

        Logger.Info("Fin generación reporte QR tarjetas global ...")
        If hayalgo Then
            Enviar_Achivos_Mail(0, adjuntos, "CIERRES TARJETAS QR Fecha: " + fecha_cierre.ToString("dd/MM/yyyy"))
        Else
            Logger.Info(String.Format("Archivo Vacío, no se envió mail global"))
        End If
    End Sub


    Public Sub ListadosReportesQRTarjetasPorPing(ping As Integer, fecha_cierre As Date)
        Dim hayalgo As Boolean = False
        Dim adjuntos As String
        Dim linea As String
        Dim lacanti, caja As Integer
        Dim eltot, totalxcaja As Decimal
        Dim nombrearc2 As String
        Dim dt As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Logger.Info(String.Format("Generando reporte QR tarjetas P.{0} ...", ping))
        Dim res0200 = From c In dt.GetData()
                      Where c.status.trim = "CLOSED" And c.pinguino = ping
                      Order By c.caja, c.FechaHora
                      Select c


        If res0200.Count = 0 Then
            Logger.Info("No hay movimientos para informar. " & fecha_cierre.ToString("dd/MM/yy") & " Pinguino:" & ping.ToString)
            Exit Sub
        End If

        nombrearc2 = "CieQR" & ping.ToString("00") & "_DET_" & fecha_cierre.ToString("yyMMdd") + fecha_cierre.ToString("HHmmss") + ".txt"
        adjuntos = nombrearc2 & ";"

        linea = "Reporte Detalle Transacciones PINGUINO " & ping.ToString("00") & "  -   Fecha Generacion: " + Format(fecha_cierre, "dd/MM/yy") + "   " + fecha_cierre.ToString("HH:mm")
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "----------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = " CAJA                IMPORTE           CAJERA            FEC/HORA TRANS.       ID OPERACION          ID USUARIO"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "----------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)

        lacanti = 0
        eltot = 0

        For Each reg0200 In res0200

            hayalgo = True

            If caja = 0 Then caja = reg0200.Caja


            If caja <> reg0200.Caja Then
                linea = "==========".PadLeft(28)
                EscribeArchivoCierre(nombrearc2, linea)
                linea = $"Total caja {caja.ToString("00")}     {totalxcaja.ToString("###,##0.00").PadLeft(10)}"
                'linea = String.Format("Total caja {0}", caja.ToString("00")).PadRight(37) + totalxcaja.ToString("###,##0.00").PadLeft(10)
                EscribeArchivoCierre(nombrearc2, linea)
                linea = " "
                EscribeArchivoCierre(nombrearc2, linea)
                totalxcaja = 0
                caja = reg0200.Caja
            End If

            linea = $"  {reg0200.caja.ToString("00")}            {reg0200.importe.ToString.PadLeft(12)}            {reg0200.cajero.ToString.PadLeft(4)}             {Format(reg0200.FechaHora, "dd/MM/yy")}  {Format(reg0200.FechaHora, "HH:mm")}      {reg0200.idOperacion.Trim.PadLeft(12)}         {reg0200.idUsuario.Trim.PadLeft(12)}"
            EscribeArchivoCierre(nombrearc2, linea)

            lacanti = lacanti + 1
            eltot += reg0200.importe

            totalxcaja += reg0200.importe


        Next

        linea = "==========".PadLeft(28)
        EscribeArchivoCierre(nombrearc2, linea)
        linea = $"Total caja {caja.ToString("00")}     {totalxcaja.ToString("###,##0.00").PadLeft(10)}"
        'linea = String.Format("Total caja {0}", caja.ToString("00")).PadRight(37) + totalxcaja.ToString("###,##0.00").PadLeft(10)
        EscribeArchivoCierre(nombrearc2, linea)
        linea = " "
        EscribeArchivoCierre(nombrearc2, linea)

        linea = "---------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = $"TOTAL GENERAL     {eltot.ToString("##,###,##0.00").PadLeft(10)}     CANT. MOV.  {lacanti.ToString("###,##0").PadLeft(7)}"
        EscribeArchivoCierre(nombrearc2, linea)

        Logger.Info(String.Format("Fin generación reporte QR tarjetas P.{0} ...", ping))
        If hayalgo Then
            Enviar_Achivos_Mail(ping, adjuntos, "REPORTES QR DIARIOS Ping. " & ping & " Fecha: " + fecha_cierre.ToString("dd/MM/yyyy"))
        Else
            Logger.Info(String.Format("Archivo Vacío QR, no se envió mail a Ping. {0}", ping))
        End If
    End Sub


    Public Sub Enviar_Achivos_Mail(ping As Integer, adjuntos As String, asunto As String)
        Try
            If My.Computer.Name = "MARCOS-XP" Or Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                Exit Sub
            End If
            Logger.Info(String.Format("Enviando mail con reportes asunto {0} ...", asunto))
            Dim mensaje As New Net.Mail.MailMessage
            Dim smtp As New Net.Mail.SmtpClient()
            Dim dt As New QRDataSetTableAdapters.MAILSTableAdapter

            Dim result = From c In dt.GetData()
                         Where c.Pinguino = ping
                         Select c
            If result.Count > 0 Then
                smtp.Host = Trim(Configuracion.ServidorCorreo)
                smtp.Port = Trim(Configuracion.PuertoCorreo)
                For Each mail In result
                    mensaje.To.Add(mail.Mail)
                Next

                Dim att As Net.Mail.Attachment

                Dim adj = adjuntos.Split(";")
                For Each arc In adj
                    If arc <> "" Then
                        att = New Net.Mail.Attachment(Configuracion.DirTarjetas + "\Cierres\" + arc)
                        mensaje.Attachments.Add(att)
                    End If
                Next

                mensaje.From = New Net.Mail.MailAddress("tarjetas@cormoran.com.ar", "CIERRES TARJETAS")

                mensaje.Subject = asunto

                mensaje.Body = ""

                smtp.Send(mensaje)
                Logger.Info("Mail enviado ...")
            End If
        Catch ex As Exception
            Logger.Error("No se pudo enviar mail con reportes de ping: " + ping.ToString & vbNewLine &
                        ex.Message)
        End Try

    End Sub
    Public Sub EscribeArchivoCierre(nombre As String, ByVal linea As String)
        'GENERAR .POS
        'GENERAR TARJREP
        'GENERAR TARJREP2
        Console.WriteLine(linea)
        Dim archivoSalida, archivoSalidaLocal As FileStream
        Dim leerfile, leerfileLocal As StreamWriter
        Try
            archivoSalida = New FileStream(Configuracion.DirTarjetas + "\Cierres\" + nombre, FileMode.Append, FileAccess.Write)
            leerfile = New StreamWriter(archivoSalida)
            If linea <> "" Then leerfile.WriteLine(linea)
            leerfile.Close()
        Catch ex As IOException
            Logger.Error(String.Format("No se pudo grabar el archivo {0}\Cierres\{1}", Configuracion.DirTarjetas, nombre))
        Catch ex1 As Exception
            Logger.Fatal(ex1.Message)
        End Try

        Try
            archivoSalidaLocal = New FileStream(Configuracion.DirLocal + "\Cierres\" + nombre, FileMode.Append, FileAccess.Write)
            leerfileLocal = New StreamWriter(archivoSalidaLocal)
            If linea <> "" Then leerfileLocal.WriteLine(linea)
            leerfileLocal.Close()
        Catch ex As IOException
            Logger.Error(String.Format("No se pudo grabar el archivo {0}\Cierres\{1}", Configuracion.DirLocal, nombre))
        Catch ex1 As Exception
            Logger.Fatal(ex1.Message)
        End Try

    End Sub
#End Region


    'Public Function ImportarMovimientos(archivo As String) As Int16

    '    Dim arc As FileIO.TextFieldParser
    '    Dim renglon As String()
    '    Dim cont As Int16 = 0
    '    Dim datosDS As New DatosTj
    '    Dim datostjTA As New DatosTjTableAdapters.MovimientosTableAdapter
    '    Dim tablalocal As New DatosTj.MovimientosDataTable

    '    Try

    '        arc = New FileIO.TextFieldParser(archivo)
    '        arc.TextFieldType = FileIO.FieldType.Delimited
    '        arc.SetDelimiters(";")

    '        Logger.Info("Importando Movimientos")
    '        While Not arc.EndOfData
    '            Try
    '                renglon = arc.ReadFields
    '                Logger.Info(renglon)
    '                datostjTA.Fill(tablalocal)
    '                Dim fila = datosDS.Movimientos.NewMovimientosRow
    '                fila.Terminal = renglon(0)
    '                fila.NroHost = renglon(1)
    '                fila.NroTrace = renglon(2)
    '                fila.TipoMensaje = renglon(3)
    '                'fila.Tarjeta = renglon(4)
    '                fila.Primerso6 = renglon(5)
    '                fila.Ultimos4 = renglon(6)
    '                fila.CodProc = renglon(7)   '  E_ProcCode
    '                fila.Importe = renglon(8)
    '                fila.Cashback = renglon(9)
    '                fila.FechaHora = renglon(10)
    '                fila.FechaHoraLocal = renglon(11)
    '                'fila.Vencimiento = renglon(12) 
    '                fila.ModoIngreso = renglon(13)
    '                fila.IdRed = renglon(14)
    '                fila.NroCuenta = renglon(15)
    '                'fila.Track2 = renglon(16)
    '                fila.RetRefNumber = renglon(17)
    '                fila.CodAutorizacion = renglon(18)
    '                fila.CodRespuesta = renglon(19)
    '                fila.IdComercio = renglon(20)
    '                'fila.Track1 = renglon(21)
    '                fila.TrackNoLeido = renglon(22)
    '                fila.Pinguino = renglon(23)
    '                fila.NroCajaPropio = renglon(24)
    '                fila.Cuotas = renglon(25)
    '                fila.CodMoneda = renglon(26)
    '                ' fila.CodSeg =renglon(27)
    '                'fila.InfoAdicional =renglon(28)
    '                fila.TicketPosnet = renglon(29)
    '                fila.TicketCaja = renglon(30)
    '                fila.NroEmisor = renglon(31)
    '                fila.NombreEmisor = renglon(32)  'BuscarNombreEmisor(req.emisor)
    '                fila.NroPlan = renglon(33)
    '                fila.TipoOperacion = renglon(34) ' compra/anulacion/devolucion
    '                fila.Cajera = renglon(35)
    '                fila.estado = renglon(36)
    '                fila.hizocierre = 0
    '                fila.nrolote = renglon(38)
    '                fila.exportado = 0
    '                fila.Criptograma = renglon(40)
    '                fila.Fallback = renglon(41)
    '                fila.ServiceCode = renglon(42)

    '                'fila.TipoOperacion = req.operacion
    '                datosDS.Movimientos.AddMovimientosRow(fila)
    '                datostjTA.Update(fila)
    '                cont += 1
    '            Catch
    '                Logger.Error(String.Format("No se pudo importar el movimiento {0}/{1}/{2}", renglon(0), renglon(1), renglon(2)))
    '            End Try

    '        End While

    '    Catch ex As Exception
    '        Logger.Error(ex.Message)
    '    End Try
    '    Return cont


    'End Function



    ''' <summary>
    ''' Exporta todos los movimientos, se hayan o no hecho el cierre.
    ''' </summary>
    Public Function Exportar_TodosQR(fecha_cierre As Date) As Boolean
        Dim errores As Boolean = False
        Dim dt As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Dim result = From c In dt.GetData
                     Select c
        Dim fileMovimientos As FileStream
        Dim renglon As StreamWriter
        Try
            Logger.Info("Exportando TODOS los movimientos QR...")
            fileMovimientos = New FileStream(Configuracion.DirTarjetas + "\Exportados\movQR" + fecha_cierre.ToString("yyMMdd") + fecha_cierre.ToString("HHmmss") + ".txt", FileMode.Append, FileAccess.Write)
            renglon = New StreamWriter(fileMovimientos)
            renglon.AutoFlush = True
            Dim x As Integer = 0
            Try
                Do While result(0)(x) IsNot Nothing
                    x += 1
                Loop
            Catch ex As Exception
                x -= 1
            End Try

            For Each reg In result
                For i As Integer = 0 To x
                    renglon.Write(reg(i).ToString + ";")
                Next
                renglon.Write(vbNewLine)
            Next
            fileMovimientos.Close()
            Logger.Info("Exportación TOTAL QR finalizada.")
            errores = False
        Catch ex1 As IO.IOException
            Logger.Error("Falló la exportación de uno o mas registros de la tabla movimientos QR.")
            errores = True
        Catch ex As Exception
            Logger.Error("Falló la exportación de uno o mas registros de la tabla movimientos QR.")
            errores = True
        End Try
        Return errores
    End Function



    Public Sub Eliminar_exportadosQR()
        Dim datostjTA As New QRDataSetTableAdapters.TransaccionesQRTableAdapter
        Dim tablalocal As New QRDataSet.TransaccionesQRDataTable
        Try
            Logger.Info("Borrando movimientos QR exportados.")
            datostjTA.DeleteTodos()
            Logger.Info("Se borraron los movimientos QR exportados.")
        Catch ex As Exception
            Logger.Error("No se pudieron borrar los movimientos QR exportados.")
        End Try
    End Sub


    Public Sub Incrementar_TraceMP(caja As Integer, ping As Integer)
        Dim dt As New QRDataSetTableAdapters.TerminalesQRTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where c.Pinguino = ping And c.Caja = caja
                         Select c

            For Each reg In result
                If reg.external_reference >= 999999 Then
                    reg.external_reference = 1
                Else
                    reg.external_reference = reg.external_reference + 1
                End If
                dt.Update(reg)
            Next
        Catch
            Logger.Warn(String.Format("No se pudo incrementar el trace. Terminal {0}/{1}", ping, caja))

        End Try

    End Sub



#End Region


End Module
