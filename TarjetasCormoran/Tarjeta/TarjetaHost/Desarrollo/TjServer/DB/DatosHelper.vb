Imports TjServer.ServerTar
' aca poner todas las operaciones de la DB
Imports System.Linq
Imports System.IO


Public Module DatosHelper

#Region "Actualizaciones de tablas"

    Private Param As New Parametros
    Public Function Configuracion() As Parametros
        Return Param
    End Function


    Public Sub BorrarDatosCriticos(req As Req)
        Dim datostjTA As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim tablalocal As New DatosTj.MovimientosDataTable
        Try
            datostjTA.BorrarDatosCriticos(Trim(req.terminal), req.nrohost, req.nroTrace, "0200")
            datostjTA.BorrarDatosCriticos(Trim(req.terminal), req.nrohost, req.nroTrace, "0210")
            datostjTA.BorrarDatosCriticos(Trim(req.terminal), req.nrohost, req.nroTrace, "0220")
            datostjTA.BorrarDatosCriticos(Trim(req.terminal), req.nrohost, req.nroTrace, "0230")
            datostjTA.BorrarDatosCriticos(Trim(req.terminal), req.nrohost, req.nroTrace, "0400")
            datostjTA.BorrarDatosCriticos(Trim(req.terminal), req.nrohost, req.nroTrace, "0410")
        Catch
            Logger.Error(String.Format("Error al borrar datos críticos {0}/{1}/{2}", req.nombreHost, req.nroTrace, Trim(req.terminal)))
        End Try
    End Sub

    Public Sub BorrarDatosCriticos(pterm As String, pHost As Integer, pTrace As Integer)
        Dim datostjTA As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim tablalocal As New DatosTj.MovimientosDataTable
        Try
            datostjTA.BorrarDatosCriticos(Trim(pterm), pHost, pTrace, "0200")
            datostjTA.BorrarDatosCriticos(Trim(pterm), pHost, pTrace, "0210")
            datostjTA.BorrarDatosCriticos(Trim(pterm), pHost, pTrace, "0400")
            datostjTA.BorrarDatosCriticos(Trim(pterm), pHost, pTrace, "0410")
        Catch
            Logger.Error(String.Format("Error al borrar datos críticos {0}/{1}/{2}", pHost, pTrace, Trim(pterm)))
        End Try
    End Sub

    ''' <summary>
    ''' Contador de cierres por dia que se hicieron.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub IncrementarNroCierre(fecha_cierre As Date)
        Dim datostjTA As New DatosTjTableAdapters.CIERRESTableAdapter
        datostjTA.InsertarCierre(fecha_cierre, BuscarNroCierre(fecha_cierre) + 1)
    End Sub

    Private Sub BorrarUltimaAprobada(req As Req)
        Dim TarultTA As New CajasTableAdapters.tarultTableAdapter
        Dim TarultDT As New Cajas.tarultDataTable
        Try
            TarultTA.Fill(TarultDT)
            TarultTA.DeleteTarult(req.pinguino, req.nroCaja)
            Logger.Info(String.Format("Borrando Tarult Ping. {0} Caja {1}", req.pinguino, req.nroCaja))
        Catch ex As Exception
            Logger.Error(String.Format("Error al borrar el Tarult {0}/{1}", req.pinguino, req.nroCaja) & vbNewLine &
                        ex.Message)
        End Try
    End Sub



    Public Sub AgregarUltimaAprobada(req As Req)

        Dim cont As Int16 = 0
        Dim CajasDS As New Cajas
        Dim TarultTA As New CajasTableAdapters.tarultTableAdapter
        Dim TarultDT As New Cajas.tarultDataTable
        BorrarUltimaAprobada(req)
        Try
            TarultTA.Fill(TarultDT)
            TarultTA.InsertTarult(req.pinguino,
                                  req.nroCaja,
                                  req.FechaHoraEnvioMsg.ToString("dd/MM/yy"),
                                  req.FechaHoraEnvioMsg.ToString("HH:mm:ss"),
                                  req.ida.TARJ.Substring(0, 6),
                                  req.ida.TARJ.Substring(Trim(req.ida.TARJ).Length - 4),
                                  req.importe)

        Catch ex As Exception
            Logger.Error(String.Format("Error al registrar el Tarult {0}/{1}/{2}", req.pinguino, req.nroCaja, req.importe) & vbNewLine &
                         ex.Message)
        End Try
    End Sub

    Public Function Grabar_RespChip(respuesta As String, terminal As String, host As Integer, ticket As Integer) As Boolean

        Dim movimientosTA As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim tabla As New DatosTj.MovimientosDataTable
        Try
            Dim action = Sub(row)
                             row("RespuestaChip") = IIf(String.IsNullOrEmpty(respuesta), "", respuesta)
                         End Sub
            movimientosTA.Fill(tabla)
            tabla.Select("NroHost = " + Str(host) + " and Trim(Terminal) = '" + Trim(terminal) + "' and TicketPosnet = " + Str(ticket)).ToList().ForEach(action)

            movimientosTA.Update(tabla)

            Return True
        Catch ex As Exception
            Logger.Error(String.Format("Error actualizando confirmación de operación de terminal {0} host: {1} ticket: {2}", terminal, host, ticket) & vbNewLine &
                          ex.Message)
            Return False
        End Try

    End Function

    Public Sub AgregarMovimiento3DES(req As Req)

        Dim cont As Int16 = 0
        Dim datosDS As New DatosTj
        Dim datostjTA As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim tablalocal As New DatosTj.MovimientosDataTable
        Try
            '--- GRABA EN LA TABLA MOVIMIENTOS
            datostjTA.Fill(tablalocal)
            Dim fila = datosDS.Movimientos.NewMovimientosRow
            fila.Terminal = Trim(req.terminal)
            fila.NroHost = req.nrohost
            fila.NroTrace = req.nroTrace
            fila.TipoMensaje = Trim(req.tipoMensaje)
            'fila.Tarjeta = TjComun.IdaLib.EncriptarStr(Trim(req.msjIda.nroTarjeta))
            fila.Primerso6 = Mid(Trim(req.msjIda.nroTarjeta), 1, 6)
            fila.Ultimos4 = Mid(Trim(req.msjIda.nroTarjeta), Trim(req.msjIda.nroTarjeta).Length - 3)
            fila.Cajera = req.msjIda.cajera
            fila.Cashback = req.msjIda.importeCashback
            fila.TicketCaja = req.msjIda.ticketCaja
            fila.CodProc = Trim(req.operacion)    '  E_ProcCode
            fila.Importe = req.importe
            fila.FechaHora = req.FechaHoraEnvioMsg
            fila.FechaHoraLocal = req.FechaHoraEnvioMsg
            'fila.Vencimiento = req.ida.EXPDATE ****** no lo tengo cuando es emv *********
            fila.ModoIngreso = req.msjIda.tipoIngreso
            fila.IdRed = req.idred
            fila.NroCuenta = Trim(req.nrocuenta)
            'fila.Track2 = TjComun.IdaLib.EncriptarStr(Trim(req.ida.TRACK2)) ****** NO TENGO TRACK 2 CUANDO ES EMV *******
            fila.RetRefNumber = req.retrefnumber
            If Trim(req.tipoMensaje) = "0220" Then
                fila.CodAutorizacion = Trim(req.msjIda.CodAutorizaAdv)
            Else
                fila.CodAutorizacion = Trim(req.autorizacion)
            End If
            fila.CodRespuesta = Trim(CInt(req.msjRespuesta.codRespuesta))
            fila.IdComercio = req.idComercio
            ' fila.Track1 = TjComun.IdaLib.EncriptarStr(Trim(req.ida.TRACK1))  *********** no tengo el track 1 cuando es emv ***************
            fila.TrackNoLeido = ""
            fila.Pinguino = req.pinguino
            fila.NroCajaPropio = req.nroCaja
            fila.Cuotas = Trim(req.cuotas)
            fila.CodMoneda = Trim(req.moneda)
            'fila.CodSeg = TjComun.IdaLib.EncriptarStr(req.ida.CODSEG) *************** no tengo cod seg en 3 des
            'fila.InfoAdicional = req.vta.VtaMensaje
            fila.TicketPosnet = req.nroTicket
            fila.NroEmisor = req.emisor
            fila.NombreEmisor = req.descEmisor  'BuscarNombreEmisor(req.emisor)
            fila.NroPlan = req.nroPlan
            If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Compra Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.CompraCashback Then
                fila.TipoOperacion = "0" ' compra/anulacion/devolucion
            ElseIf req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Then
                fila.TipoOperacion = "2" ' compra/anulacion/devolucion
            ElseIf req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion Then
                fila.TipoOperacion = "1" ' compra/anulacion/devolucion
            ElseIf req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Then
                fila.TipoOperacion = "3" ' compra/anulacion/devolucion
            End If

            'fila.TipoOperacion = req.operacion
            fila.estado = False
            fila.hizocierre = False
            fila.Criptograma = req.msjIda.criptograma
            fila.PaqueteEncriptado = req.msjIda.datosEncriptados
            fila.ServiceCode = req.msjIda.serviceCode
            fila.Fallback = req.FALLBACK
            fila.CuponOriginal = 0
            If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Anulacion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.AnulacionDevolucion Or req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion Then
                fila.CuponOriginal = req.msjIda.ticketOriginal
            End If
            If req.msjIda.tipoMensaje = TransmisorTCP.TipoMensaje.Devolucion Then
                fila.FechaCuponOriginal = req.msjIda.fechaOperacionOriginal
            End If
            fila.ControlBackup = 0
            datosDS.Movimientos.AddMovimientosRow(fila)
            datostjTA.Update(fila)
        Catch ex As Exception
            Logger.Error(String.Format("Error al registrar el movimiento {0}/{1}/{2}", req.nombreHost, req.nroTrace, Trim(req.terminal)) & vbNewLine &
                         ex.Message)
        End Try
    End Sub


    Public Sub ActualizaMovimientoPEI(req As Req)
        Dim datostjTA As New DatosTjTableAdapters.MovimientosPEITableAdapter
        Dim tablalocal As New DatosTj.MovimientosPEIDataTable
        Dim fila As DatosTj.MovimientosPEIRow
        Try
            tablalocal = datostjTA.GetDataByClave(req.terminal, req.nroTrace)
            fila = tablalocal.Item(0)
            If req.vta.VtaOk = 0 Then fila.Cupon = req.nroTicket

            '--- aca aca aca aca se actualiza la respuesta  
            fila.CodRespuesta = req.vta.VtaCodResp.Trim
            fila.IdOperacion = req.vta.VtaAutorizacion.Trim
            If fila.Operacion = lib_PEI.Concepto.DEVOLUCION.ToString Then fila.IdOperacionOrigen = req.ida.TKTORI.ToString.Trim
            fila.ReferenciaBancaria = req.nrocuenta.Trim
            fila.Importe = req.importe
            datostjTA.Update(fila)
            Logger.Info(String.Format("Movimiento actualizado {0}/{1}/{2}", req.nrohost.ToString, req.terminal.Trim, req.nroTrace.ToString))
        Catch
            Logger.Error(String.Format("Movimiento NO actualizado {0}/{1}/{2} con respuesta {3}", req.nrohost.ToString, req.terminal.Trim, req.nroTrace.ToString, req.respuesta))
        End Try
    End Sub


    Public Sub AgregarMovimientoPEI(req As Req)
        Dim cont As Int16 = 0
        Dim datosDS As New DatosTj
        Dim datostjTA As New DatosTjTableAdapters.MovimientosPEITableAdapter
        Dim tablalocal As New DatosTj.MovimientosPEIDataTable
        Try
            'datostjTA.Fill(tablalocal)
            Dim fila = datosDS.MovimientosPEI.NewMovimientosPEIRow
            With fila
                .Terminal = Trim(req.terminal)
                .Trace = req.nroTrace
                .FechaHora = req.FechaHoraEnvioMsg
                .Primeros6 = Mid(Trim(req.ida.TARJ), 1, 6)
                .Ultimos4 = Mid(Trim(req.ida.TARJ), Trim(req.ida.TARJ).Length - 3)
                .CodMoneda = lib_PEI.Moneda.ARS.ToString
                .Emisor = req.descEmisor
                .NroEmisor = req.emisor
                Select Case req.ida.oper
                    Case 0
                        If req.ida.CASHBACK > 0 Then
                            .Operacion = lib_PEI.Concepto.EXTRACCION.ToString
                        Else
                            .Operacion = lib_PEI.Concepto.COMPRA_DE_BIENES.ToString
                        End If
                    Case 1
                        .Operacion = lib_PEI.Concepto.DEVOLUCION.ToString
                End Select
                .CodRespuesta = ""
                .TicketCaja = req.ida.TICCAJ
                .ReferenciaBancaria = ""
                .IdOperacion = ""
                .IdOperacionOrigen = req.ida.TKTORI
                .Importe = req.importe
                .Cajera = req.ida.CAJERA
                .Caja = req.nroCaja
                .Pinguino = req.pinguino
                .Exportado = False
            End With

            datosDS.MovimientosPEI.AddMovimientosPEIRow(fila)
            datostjTA.Update(fila)
        Catch ex As Exception
            Logger.Error(String.Format("Error al registrar el movimiento PEI {0}/{1}/{2}", req.nombreHost, req.nroTrace, Trim(req.terminal)) & vbNewLine &
                         ex.Message)
        End Try
    End Sub


    Public Sub AgregarMovimiento(req As Req)
        Dim cont As Int16 = 0
        Dim datosDS As New DatosTj
        Dim datostjTA As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim tablalocal As New DatosTj.MovimientosDataTable
        Try
            'datostjTA.Fill(tablalocal)
            Dim fila = datosDS.Movimientos.NewMovimientosRow
            fila.Terminal = Trim(req.terminal)
            fila.NroHost = req.nrohost
            fila.NroTrace = req.nroTrace
            fila.TipoMensaje = Trim(req.tipoMensaje)
            fila.Tarjeta = TjComun.IdaLib.EncriptarStr(Trim(req.ida.TARJ))
            fila.Primerso6 = Mid(Trim(req.ida.TARJ), 1, 6)
            fila.Ultimos4 = Mid(Trim(req.ida.TARJ), Trim(req.ida.TARJ).Length - 3)
            fila.Cajera = req.ida.CAJERA
            fila.Cashback = req.ida.CASHBACK
            fila.TicketCaja = req.ida.TICCAJ
            fila.CodProc = Trim(req.operacion)    '  E_ProcCode
            fila.Importe = req.importe
            fila.FechaHora = req.FechaHoraEnvioMsg
            fila.FechaHoraLocal = req.FechaHoraEnvioMsg
            fila.Vencimiento = req.ida.EXPDATE
            fila.ModoIngreso = req.ida.MANUAL
            fila.IdRed = req.idred
            fila.NroCuenta = Trim(req.nrocuenta)
            fila.Track2 = TjComun.IdaLib.EncriptarStr(Trim(req.ida.TRACK2))
            fila.RetRefNumber = req.retrefnumber
            fila.CodAutorizacion = Trim(req.autorizacion)
            fila.CodRespuesta = Trim(req.vta.VtaOk)
            fila.IdComercio = req.idComercio
            fila.Track1 = TjComun.IdaLib.EncriptarStr(Trim(req.ida.TRACK1))
            fila.TrackNoLeido = ""
            fila.Pinguino = req.pinguino
            fila.NroCajaPropio = req.nroCaja
            fila.Cuotas = Trim(req.cuotas)
            fila.CodMoneda = Trim(req.moneda)
            fila.CodSeg = TjComun.IdaLib.EncriptarStr(req.ida.CODSEG)
            'fila.InfoAdicional = req.vta.VtaMensaje
            fila.TicketPosnet = req.nroTicket
            fila.NroEmisor = req.emisor
            fila.NombreEmisor = req.descEmisor  'BuscarNombreEmisor(req.emisor)
            fila.NroPlan = req.nroPlan
            fila.TipoOperacion = Trim(req.ida.oper) ' compra/anulacion/devolucion
            'fila.TipoOperacion = req.operacion
            fila.estado = False
            fila.hizocierre = False
            'fila.Criptograma = req.ida.CRIPTO
            'fila.ServiceCode = req.ida.ServCode
            fila.Fallback = req.FALLBACK
            fila.ControlBackup = 0
            datosDS.Movimientos.AddMovimientosRow(fila)
            datostjTA.Update(fila)
        Catch ex As Exception
            Logger.Error(String.Format("Error al registrar el movimiento {0}/{1}/{2}", req.nombreHost, req.nroTrace, Trim(req.terminal)) & vbNewLine &
                         ex.Message)
        End Try
    End Sub

    Public Sub MarcarExportable(req As Req)
        Dim datostjTA As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim tablalocal As New DatosTj.MovimientosDataTable
        Try
            datostjTA.FillByTrace(tablalocal, Trim(req.terminal), req.nrohost, req.nroTrace)
            For Each movimientosrow In tablalocal
                movimientosrow.ControlBackup = 1
                datostjTA.Update(movimientosrow)
            Next
        Catch ex As Exception
            Logger.Error(String.Format("Error marcando como exportable de operación de caja {0}/{1}/{2}", req.nombreHost, req.nroTrace, Trim(req.terminal)) & vbNewLine &
                         ex.Message)
        End Try
    End Sub


    Public Sub RespaldarMovimientos()

        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim result = From c In dt.GetData
                     Select c Where c.ControlBackup = 1
        Dim fileMovimientos As FileStream
        Dim renglon As StreamWriter
        Try
            If result.Count > 0 Then
                '------------------------------------------------------------------------------------------
                '--- 19/05/21 ASTERISQUEE DONDE GUARDA EN PINGUINO1-1 PARA QUE GUARDE EN EL C:\TEMP        
                '------------------------------------------------------------------------------------------
                'If Directory.Exists(Configuracion.DirTarjetas & "\respaldos\") Then
                '    fileMovimientos = New FileStream(Configuracion.DirTarjetas & "\respaldos\mov" + Now.Date.ToString("yyMMdd") + ".bkp", FileMode.Append, FileAccess.Write)
                'Else
                '    Logger.Error("No existe el directorio para respaldar los movimientos. Se usa el directorio alternativo.")
                fileMovimientos = New FileStream("C:\temp\respaldos\mov" + Now.Date.ToString("yyMMdd") + ".bkp", FileMode.Append, FileAccess.Write)
                'End If
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
                    dt.MarcarRespaldado(reg.Terminal, reg.NroHost, reg.NroTrace, reg.TipoMensaje)
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

    Public Sub CulminacionSatisfactoria(req As Req)
        Dim datostjTA As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim tablalocal As New DatosTj.MovimientosDataTable
        Try
            datostjTA.FillByTrace(tablalocal, Trim(req.terminal), req.nrohost, req.nroTrace)
            For Each movimientosrow In tablalocal
                movimientosrow.estado = True
                datostjTA.Update(movimientosrow)
            Next
        Catch ex As Exception
            Logger.Error(String.Format("Error actualizando confirmación de operación de caja {0}/{1}/{2}", req.nombreHost, req.nroTrace, Trim(req.terminal)) & vbNewLine &
                         ex.Message)
        End Try
    End Sub


    'Public Sub BusquedaNoCulminados(req As Req)
    '    Dim datostjTA As New DatosTjTableAdapters.MovimientosTableAdapter
    '    Dim tablalocal As New DatosTj.MovimientosDataTable
    '    datostjTA.FillByEstado(tablalocal, False)
    '    For Each movimientosrow In tablalocal
    '        req.nroCaja = movimientosrow.NroCajaPropio
    '        req.pinguino = movimientosrow.Pinguino
    '        ' inicializaReq
    '    Next
    'End Sub

    Public Sub actualizarDiasPromos(srv As ServerTar)

        Logger.Info("Sincronizando Tabla Local DIAS PROMOS")
        Dim cont As Int16 = 0
        Dim tablaODBC As New Cajas.peiproDataTable
        Dim cajasTA As New CajasTableAdapters.peiproTableAdapter
        Dim datosDS As New DatosTj
        Dim datostjTA As New DatosTjTableAdapters.DiasPromosTableAdapter
        Dim tablalocal As New DatosTj.DiasPromosDataTable

        '---------------------------------------------------------------------------
        '--- PRIMERO ACTUALIZO LA TABLA LOCAL CON LO QUE TIENE LA BASE PROGRESS     
        '---------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each peiproRow In tablaODBC
            Dim fila = tablalocal.FindByDiaBanco(peiproRow._dia_ppr, peiproRow._ban_ppr)
            If fila Is Nothing Then
                fila = datosDS.DiasPromos.NewDiasPromosRow
                fila.Dia = peiproRow._dia_ppr
                fila.Banco = peiproRow._ban_ppr
                datosDS.DiasPromos.AddDiasPromosRow(fila)
            End If
            datostjTA.Update(fila)
            cont = cont + 1
        Next

        '-------------------------------------------------------------------------------------
        '--- SEGUNDO, BORRO LO DE LA TABLA LOCAL CON LO QUE NO EXISTE EN LA BASE PROGRESS     
        '-------------------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each diaspromosRow In tablalocal
            Dim fila = tablaODBC.FindBy_dia_ppr_ban_ppr(diaspromosRow.Dia, diaspromosRow.Banco)
            If fila Is Nothing Then
                datostjTA.Delete(diaspromosRow.Dia, diaspromosRow.Banco)
            End If
        Next
        Logger.Info("Actualizó Tabla Local DIASPROMOS con " + cont.ToString + " Registros")
    End Sub

    Public Sub actualizarBINES(srv As ServerTar)

        Logger.Info("Sincronizando Tabla Local BINES")
        Dim cont As Int16 = 0
        Dim tablaODBC As New Cajas.peibanDataTable
        Dim cajasTA As New CajasTableAdapters.peibanTableAdapter
        Dim datosDS As New DatosTj
        Dim datostjTA As New DatosTjTableAdapters.BINESTableAdapter
        Dim tablalocal As New DatosTj.BINESDataTable

        '---------------------------------------------------------------------------
        '--- PRIMERO ACTUALIZO LA TABLA LOCAL CON LO QUE TIENE LA BASE PROGRESS     
        '---------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each peiBANRow In tablaODBC
            Dim fila = tablalocal.FindByBIN(peiBANRow._bin_pba)
            If fila Is Nothing Then
                fila = datosDS.BINES.NewBINESRow
                fila.BIN = peiBANRow._bin_pba
                fila.Banco = peiBANRow._ban_pba
                fila.TipoTarjeta = peiBANRow._tip_pba
                fila.Categoria = peiBANRow._cat_pba
                datosDS.BINES.AddBINESRow(fila)
            Else
                fila.Banco = peiBANRow._ban_pba
                fila.TipoTarjeta = peiBANRow._tip_pba
                fila.Categoria = peiBANRow._cat_pba

            End If
            datostjTA.Update(fila)
            cont = cont + 1
        Next

        '-------------------------------------------------------------------------------------
        '--- SEGUNDO, BORRO LO DE LA TABLA LOCAL CON LO QUE NO EXISTE EN LA BASE PROGRESS     
        '-------------------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each BINESRow In tablalocal
            Dim fila = tablaODBC.FindBy_bin_pba(BINESRow.BIN)
            If fila Is Nothing Then
                datostjTA.Delete(BINESRow.BIN, BINESRow.Banco, BINESRow.TipoTarjeta, BINESRow.Categoria)
            End If
        Next
        Logger.Info("Actualizó Tabla Local BINES con " + cont.ToString + " Registros")
    End Sub

    Public Sub actualizarEmisor(srv As ServerTar)

        Logger.Info("Sincronizando Tabla Local EMISOR")
        Dim cont As Int16 = 0
        Dim tablaODBC As New Cajas.emisorDataTable
        Dim cajasTA As New CajasTableAdapters.emisorTableAdapter
        Dim datosDS As New DatosTj
        Dim datostjTA As New DatosTjTableAdapters.EMISORTableAdapter
        Dim tablalocal As New DatosTj.EMISORDataTable

        '---------------------------------------------------------------------------
        '--- PRIMERO ACTUALIZO LA TABLA LOCAL CON LO QUE TIENE LA BASE PROGRESS     
        '---------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each EmisorRow In tablaODBC
            Dim fila = tablalocal.FindByEmisor(EmisorRow._nro_emi)
            If fila Is Nothing Then
                fila = datosDS.EMISOR.NewEMISORRow
                fila.Emisor = EmisorRow._nro_emi

                fila.Descripcion = EmisorRow._des_emi
                fila.Host = EmisorRow._hos_emi
                fila.IDComercioRaf = EmisorRow._icr_emi
                fila.IDComercioSF = EmisorRow._ics_emi
                fila.ReqCodSeguridad = EmisorRow._seg_emi
                fila.Req4Digitos = EmisorRow._cua_emi
                fila.ReqFechaVenc = EmisorRow._ven_emi

                datosDS.EMISOR.AddEMISORRow(fila)
            Else
                fila.Descripcion = EmisorRow._des_emi
                fila.Host = EmisorRow._hos_emi
                fila.IDComercioRaf = EmisorRow._icr_emi
                fila.IDComercioSF = EmisorRow._ics_emi
                fila.ReqCodSeguridad = EmisorRow._seg_emi
                fila.Req4Digitos = EmisorRow._cua_emi
                fila.ReqFechaVenc = EmisorRow._ven_emi
            End If
            datostjTA.Update(fila)
            cont = cont + 1
        Next

        '-------------------------------------------------------------------------------------
        '--- SEGUNDO, BORRO LO DE LA TABLA LOCAL CON LO QUE NO EXISTE EN LA BASE PROGRESS     
        '-------------------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each EmisorRow In tablalocal
            Dim fila = tablaODBC.FindBy_nro_emi(EmisorRow.Emisor)
            If fila Is Nothing Then
                datostjTA.DeleteEmisor(EmisorRow.Emisor)
            End If
        Next
        Logger.Info("Actualizó Tabla Local EMISOR con " + cont.ToString + " Registros")
    End Sub

    Public Sub actualizarPlanes(srv As ServerTar)
        Logger.Info("Sincronizando Tabla Local PLANES")
        Dim cont As Int16 = 0
        Dim tablaODBC As New Cajas.planesDataTable
        Dim cajasTA As New CajasTableAdapters.planesTableAdapter
        Dim datosDS As New DatosTj
        Dim datostjTA As New DatosTjTableAdapters.PLANESTableAdapter
        Dim tablalocal As New DatosTj.PLANESDataTable

        '---------------------------------------------------------------------------
        '--- PRIMERO ACTUALIZO LA TABLA LOCAL CON LO QUE TIENE LA BASE PROGRESS     
        '---------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each planesRow In tablaODBC
            Dim fila = tablalocal.FindByEmisorNroPlan(planesRow._nro_emi, planesRow._nro_pla)
            If fila Is Nothing Then
                fila = datosDS.PLANES.NewPLANESRow
                fila.Emisor = planesRow._nro_emi

                fila.NroPlan = planesRow._nro_pla
                fila.Cuotas = planesRow._cuo_pla
                fila.Moneda = planesRow._mon_pla
                fila.DescPlan = planesRow._des_pla
                fila.Coeficiente = planesRow._coe_pla
                fila.PlanEmisor = planesRow._pla_pla
                datosDS.PLANES.AddPLANESRow(fila)
            Else
                fila.NroPlan = planesRow._nro_pla
                fila.Cuotas = planesRow._cuo_pla
                fila.Moneda = planesRow._mon_pla
                fila.DescPlan = planesRow._des_pla
                fila.Coeficiente = planesRow._coe_pla
                fila.PlanEmisor = planesRow._pla_pla
            End If
            datostjTA.Update(fila)
            cont = cont + 1
        Next

        '-------------------------------------------------------------------------------------
        '--- SEGUNDO, BORRO LO DE LA TABLA LOCAL CON LO QUE NO EXISTE EN LA BASE PROGRESS     
        '-------------------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each PLANESRow In tablalocal
            Dim fila = tablaODBC.FindBy_nro_emi_nro_pla(PLANESRow.Emisor, PLANESRow.NroPlan)
            If fila Is Nothing Then
                datostjTA.DeletePlanes(PLANESRow.Emisor, PLANESRow.NroPlan)
            End If
        Next
        Logger.Info("Actualizó Tabla Local PLANES con " + cont.ToString + " Registros")
    End Sub

    Public Sub actualizarRanghab(srv As ServerTar)
        Logger.Info("Sincronizando Tabla Local RANGHAB")
        Dim cont As Int16 = 0
        Dim tablaODBC As New Cajas.ranghabDataTable
        Dim cajasTA As New CajasTableAdapters.ranghabTableAdapter
        Dim datosDS As New DatosTj
        Dim datostjTA As New DatosTjTableAdapters.RANGHABTableAdapter
        Dim tablalocal As New DatosTj.RANGHABDataTable

        '---------------------------------------------------------------------------
        '--- PRIMERO ACTUALIZO LA TABLA LOCAL CON LO QUE TIENE LA BASE PROGRESS     
        '---------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each ranghabRow In tablaODBC
            Dim fila = tablalocal.FindByRangoDesdeRangoHasta(ranghabRow._des_rha, ranghabRow._has_rha)
            If fila Is Nothing Then
                fila = datosDS.RANGHAB.NewRANGHABRow
                fila.RangoDesde = ranghabRow._des_rha
                fila.RangoHasta = ranghabRow._has_rha
                fila.Emisor = ranghabRow._nro_emi
                datosDS.RANGHAB.AddRANGHABRow(fila)
            Else
                fila.Emisor = ranghabRow._nro_emi
            End If
            datostjTA.Update(fila)
            cont = cont + 1
        Next

        '-------------------------------------------------------------------------------------
        '--- SEGUNDO, BORRO LO DE LA TABLA LOCAL CON LO QUE NO EXISTE EN LA BASE PROGRESS     
        '-------------------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each ranghabRow In tablalocal
            Dim fila = tablaODBC.FindBy_des_rha_has_rha(ranghabRow.RangoDesde, ranghabRow.RangoHasta)
            If fila Is Nothing Then
                datostjTA.DeleteRanghab(ranghabRow.RangoDesde, ranghabRow.RangoHasta)
            End If
        Next
        Logger.Info("Actualizó Tabla Local RANGHAB con " + cont.ToString + " Registros")
    End Sub

    Public Sub actualizarNumero(srv As ServerTar)
        Logger.Info("Sincronizando Tabla Local NUMEROS")
        Dim cont As Int16 = 0
        Dim tablaODBC As New Cajas.numerosDataTable
        Dim cajasTA As New CajasTableAdapters.numerosTableAdapter
        Dim datosDS As New DatosTj
        Dim datostjTA As New DatosTjTableAdapters.NUMEROSTableAdapter
        Dim tablalocal As New DatosTj.NUMEROSDataTable

        '---------------------------------------------------------------------------
        '--- PRIMERO ACTUALIZO LA TABLA LOCAL CON LO QUE TIENE LA BASE PROGRESS     
        '---------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each numerosRow In tablaODBC
            Dim fila = tablalocal.FindByNroCajaPropioNroHost(numerosRow._caj_nro, numerosRow._hos_nro)
            If fila Is Nothing Then
                fila = datosDS.NUMEROS.NewNUMEROSRow
                fila.NroCajaPropio = numerosRow._caj_nro
                fila.NroHost = numerosRow._hos_nro
                fila.Terminal = Trim(numerosRow._ter_nro)
                fila.NroLote = numerosRow._lot_nro
                fila.NroTicket = numerosRow._tic_nro
                fila.NroTrace = numerosRow._tra_nro
                fila.PinBlock = numerosRow._pin_nro
                datosDS.NUMEROS.AddNUMEROSRow(fila)
            Else
                fila.Terminal = Trim(numerosRow._ter_nro)
                'fila.NroLote = numerosRow._lot_nro
                'fila.NroTicket = numerosRow._tic_nro
                'fila.NroTrace = numerosRow._tra_nro
                'fila.PinBlock = numerosRow._pin_nro
            End If
            datostjTA.Update(fila)
            cont = cont + 1
        Next

        '-------------------------------------------------------------------------------------
        '--- SEGUNDO, BORRO LO DE LA TABLA LOCAL CON LO QUE NO EXISTE EN LA BASE PROGRESS     
        '-------------------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each numerosRow In tablalocal
            Dim fila = tablaODBC.FindBy_caj_nro_hos_nro(numerosRow.NroCajaPropio, numerosRow.NroHost)
            If fila Is Nothing Then
                datostjTA.DeleteNumeros(numerosRow.NroCajaPropio, numerosRow.NroHost)
            End If
        Next
        Logger.Info("Actualizó Tabla Local NUMEROS con " + cont.ToString + " Registros")
    End Sub

    Public Sub actualizarHost(srv As ServerTar)
        Logger.Info("Sincronizando Tabla Local HOSTS")
        Dim cont As Int16 = 0
        Dim tablaODBC As New Cajas.hostsDataTable
        Dim cajasTA As New CajasTableAdapters.hostsTableAdapter
        Dim datosDS As New DatosTj
        Dim datostjTA As New DatosTjTableAdapters.HOSTSTableAdapter
        Dim tablalocal As New DatosTj.HOSTSDataTable

        '---------------------------------------------------------------------------
        '--- PRIMERO ACTUALIZO LA TABLA LOCAL CON LO QUE TIENE LA BASE PROGRESS     
        '---------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each hostsRow In tablaODBC
            Dim fila = tablalocal.FindByNroHost(hostsRow._nro_hos)
            If fila Is Nothing Then
                fila = datosDS.HOSTS.NewHOSTSRow
                fila.NroHost = hostsRow._nro_hos
                fila.Descripcion = hostsRow._des_hos
                fila.Direccion1 = hostsRow._di1_hos
                fila.Puerto1 = hostsRow._pu1_hos
                fila.Direccion2 = hostsRow._di2_hos
                fila.Puerto2 = hostsRow._pu2_hos
                fila.IDComercioCierre = hostsRow._com_hos
                fila.VersionSoft = hostsRow._ver_hos
                datosDS.HOSTS.AddHOSTSRow(fila)
            Else
                fila.Descripcion = hostsRow._des_hos
                fila.Direccion1 = hostsRow._di1_hos
                fila.Puerto1 = hostsRow._pu1_hos
                fila.Direccion2 = hostsRow._di2_hos
                fila.Puerto2 = hostsRow._pu2_hos
                fila.IDComercioCierre = hostsRow._com_hos
                fila.VersionSoft = hostsRow._ver_hos
            End If
            datostjTA.Update(fila)
            cont = cont + 1
        Next

        '-------------------------------------------------------------------------------------
        '--- SEGUNDO, BORRO LO DE LA TABLA LOCAL CON LO QUE NO EXISTE EN LA BASE PROGRESS     
        '-------------------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each hostsRow In tablalocal
            Dim fila = tablaODBC.FindBy_nro_hos(hostsRow.NroHost)
            If fila Is Nothing Then
                datostjTA.DeleteHosts(hostsRow.NroHost)
            End If
        Next
        Logger.Info("Actualizó Tabla Local HOSTS con " + cont.ToString + " Registros")
    End Sub

    Public Sub actualizarVisabin(srv As ServerTar)
        Logger.Info("Sincronizando Tabla Local VISABIN")
        Dim cont As Int16 = 0
        Dim tablaODBC As New Cajas.visabinDataTable
        Dim cajasTA As New CajasTableAdapters.visabinTableAdapter
        Dim datosDS As New DatosTj
        Dim datostjTA As New DatosTjTableAdapters.VISABINTableAdapter
        Dim tablalocal As New DatosTj.VISABINDataTable

        '---------------------------------------------------------------------------
        '--- PRIMERO ACTUALIZO LA TABLA LOCAL CON LO QUE TIENE LA BASE PROGRESS     
        '---------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each visabinRow In tablaODBC
            Dim fila = tablalocal.FindBynrobin(visabinRow._bin_vbi)
            If fila Is Nothing Then
                fila = datosDS.VISABIN.NewVISABINRow
                fila.nrobin = visabinRow._bin_vbi
                datosDS.VISABIN.AddVISABINRow(fila)
                datostjTA.Update(fila)
            End If
            cont = cont + 1
        Next

        '-------------------------------------------------------------------------------------
        '--- SEGUNDO, BORRO LO DE LA TABLA LOCAL CON LO QUE NO EXISTE EN LA BASE PROGRESS     
        '-------------------------------------------------------------------------------------
        datostjTA.Fill(tablalocal)
        cajasTA.Fill(tablaODBC)
        For Each visabinRow In tablalocal
            Dim fila = tablaODBC.FindBy_bin_vbi(visabinRow.nrobin)
            If fila Is Nothing Then
                datostjTA.DeleteVisaBin(visabinRow.nrobin)
            End If
        Next
        Logger.Info("Actualizó Tabla Local VISABIN con " + cont.ToString + " Registros")
    End Sub
#End Region



#Region "Estructuras"
    Public Structure MovCierres
        Dim SumaCompras As Double
        Dim CantCompras As Integer
        Dim SumaAnulaciones As Double
        Dim CantAnulaciones As Integer
        Dim SumaDevoluciones As Double
        Dim CantDevoluciones As Integer
    End Structure
#End Region



#Region "Busqueda en tablas"

    Public Function MandarPEI(bin As String, track2 As String) As Boolean
        Dim permite As Boolean = True
        Dim debito As Boolean = False
        Dim banco As String = ""
        Dim nro As Byte = 0
        Dim dt As New DatosTjTableAdapters.DiasPromosTableAdapter

        'VER SI ES DEBITO O CREDITO (MC DEBIT, VISA DEBITO, CABAL DEBITO)
        '--- PEI ES SOLAMENTE PARA TARJETAS DE DEBITO !!!!!

        nro = esmaestro(bin)
        If nro = 6 Then
            'VER SI ES MAESTRO --> PERMITE TRUE
            debito = True
        ElseIf nro = 20 Then
            If track2 <> "" AndAlso track2.Split("=")(1).Substring(5, 1) = "2" Then
                debito = True
            End If
        ElseIf nro = 2 Then
            If track2 <> "" AndAlso track2.Split("=")(1).Substring(5, 1) = "2" Then
                debito = True
            End If
        ElseIf nro = 44 Then
            debito = True
        ElseIf nro = 4 Then
            debito = True
        End If


        If debito Then
            '--- ACA BUSCO EN DIASPROMO     
            Dim result = From c In dt.GetData()
                         Where c.Dia = Now.DayOfWeek + 1            '--- es +1 porque en progress el weekday empieza de 1 y aca de 0 
                         Select c
            If result.Count > 0 Then
                If nro = 6 Then
                    'VER SI ES MAESTRO --> PERMITE TRUE 
                    permite = True

                    banco = BuscarBanco(bin)                    '--- BUSCA EN LA TABLA BINES   
                    If banco <> "" Then
                        For Each reg In result
                            If reg.Banco.Trim = banco.Trim Then
                                permite = False
                            End If
                        Next
                    End If
                Else
                    banco = BuscarBanco(bin)
                    If banco <> "" Then
                        For Each reg In result
                            If reg.Banco.Trim = banco.Trim Then
                                permite = False
                            End If
                        Next
                    Else
                        permite = False
                    End If
                End If

                '--- SI NO EXISTE EL BANCO EN LA TABLA BINES --> PERMITE = FALSE                
                '--- SI EXISTE BANCO Y ES EL DIA DE PROMO DE ESE BANCO --> PERMITE = FALSE      
                '--- SI EXISTE BANCO Y NO ES EL DIA DE PROMO DE ESE BANCO --> PERMITE = TRUE    

            Else
                permite = True
            End If
        Else
            permite = False
        End If

        Return permite
    End Function


    Private Function BuscarBanco(bin As String) As String
        Dim dt As New DatosTjTableAdapters.BINESTableAdapter
        Dim result = From c In dt.GetData()
                     Where c.BIN = bin
                     Select c
        If result.Count = 1 Then
            Return result.First.Banco
        Else
            Return ""
        End If
    End Function


    Public Function BuscarNroCierre(fecha_cierre As Date) As Byte
        Dim nro As Byte = 0
        Dim dt As New DatosTjTableAdapters.CIERRESTableAdapter
        Dim result = From c In dt.GetData()
                     Where c.FechaCierre = fecha_cierre.Date
                     Order By c.Numero Descending
                     Select c.Numero
        For Each reg In result
            nro = reg
            Exit For
        Next

        Return nro
    End Function

    Public Function BuscarMovimientoTCP(req As Req) As Boolean
        Dim referenceNumber As String = ""
        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim result = From c In dt.GetData()
                     Where Trim(c.Terminal) = Trim(req.terminal) And c.TicketPosnet = req.msjIda.ticketOriginal And c.TipoMensaje = "0210" And c.NroHost = req.nrohost
                     Select c
        For Each reg In result
            Dim result2 = From x In dt.GetData()
                          Where Trim(x.Terminal) = Trim(reg.Terminal) And x.TipoMensaje = "0400" And x.NroHost = req.nrohost And x.NroTrace = reg.NroTrace
                          Select x
            If result2.Count > 0 Then

                Exit For
            End If
            If reg.Primerso6 = req.msjIda.nroTarjeta.Substring(0, 6) And reg.Ultimos4 = req.msjIda.nroTarjeta.Substring(req.msjIda.nroTarjeta.Length - 4) Then
                referenceNumber = reg.RetRefNumber
                req.nroPlan = reg.NroPlan
                req.importe = reg.Importe
                req.msjIda.importeCashback = reg.Cashback
                Return True
            End If
        Next
        Return False
    End Function

    Public Function BuscarMovimiento0210TCP(host As Integer, trace As Integer, terminal As String) As Boolean
        Dim esta As Boolean = False
        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim result = From c In dt.GetData()
                     Where Trim(c.Terminal) = Trim(terminal) And c.NroTrace = trace And c.TipoMensaje = "0210" And c.NroHost = host
                     Select c
        For Each reg In result
            If reg.CodRespuesta.Trim = "0" Then
                esta = True
            End If
        Next
        Return esta
    End Function


    Public Function BuscarMovimiento0410TCP(host As Integer, trace As Integer, terminal As String) As Boolean
        Dim esta As Boolean = False
        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim result = From c In dt.GetData()
                     Where Trim(c.Terminal) = Trim(terminal) And c.NroTrace = trace And c.TipoMensaje = "0410" And c.NroHost = host
                     Select c
        For Each reg In result
            If reg.CodRespuesta.Trim = "0" Then
                esta = True
            End If
        Next
        Return esta
    End Function

    Public Function BuscarMovimientoTCP(ida As TransmisorTCP.MensajeIda, msj As TransmisorTCP.MensajeRespuesta) As Boolean

        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim result = From c In dt.GetData()
                     Where Trim(c.Terminal) = Trim(msj.terminal) And c.TicketPosnet = ida.ticketOriginal And c.TipoMensaje = "0210" And c.NroHost = msj.host
                     Select c
        For Each reg In result
            Dim result2 = From x In dt.GetData()
                          Where Trim(x.Terminal) = Trim(reg.Terminal) And x.TipoMensaje = "0400" And x.NroHost = msj.host And x.NroTrace = reg.NroTrace
                          Select x
            If result2.Count > 0 Then
                Continue For
            End If

            Try
                '--- REVISA SI YA ESTA ANULADO EL MOVIMIENTO 
                Dim result3 = From x In dt.GetData()
                              Where Trim(x.Terminal) = Trim(msj.terminal) And x.CuponOriginal = ida.ticketOriginal And x.TipoMensaje = "0210" And x.NroHost = msj.host
                              Select x
                If result3.Count > 0 Then
                    Exit For
                End If
            Catch
            End Try

            If reg.Primerso6 = ida.nroTarjeta.Substring(0, 6) And reg.Ultimos4 = ida.nroTarjeta.Substring(ida.nroTarjeta.Length - 4) Then
                msj.referenceNumber = reg.RetRefNumber
                msj.nroPlan = reg.NroPlan
                msj.Importe = reg.Importe
                msj.CashBack = reg.Cashback
                msj.nroPlan = reg.NroPlan
                msj.Ticket = reg.TicketCaja
                msj.tipooperacion = reg.CodProc.Trim.PadLeft(6, "0")
                Return True
            End If
        Next
        Return False
    End Function

    Public Function BuscarRetRefNumber(req As Req) As String
        Dim referenceNumber As String = ""
        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim result = From c In dt.GetData()
                     Where Trim(c.Terminal) = Trim(req.terminal) And c.TicketPosnet = req.ida.TKTORI And c.TipoMensaje = "0210" And c.NroHost = req.nrohost
                     Select c
        For Each reg In result
            Dim result2 = From x In dt.GetData()
                          Where Trim(x.Terminal) = Trim(reg.Terminal) And x.TipoMensaje = "0400" And x.NroHost = req.nrohost And x.NroTrace = reg.NroTrace
                          Select x
            If result2.Count > 0 Then

                Exit For
            End If
            referenceNumber = reg.RetRefNumber
        Next
        Return referenceNumber
    End Function


    Public Sub BuscarMovimiento(req As Req)
        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim result = From c In dt.GetData()
                     Where c.TicketPosnet = req.nroTicket And Trim(c.Terminal) = Trim(req.terminal) And c.NroTrace = req.nroTrace And c.TipoMensaje = "0200"
                     Select c

        If result.Count = 1 Then
            req.operacion = result(0).CodProc
            req.importe = result(0).Importe
            req.descEmisor = result(0).NombreEmisor
            req.cuotas = result(0).Cuotas
            req.idComercio = result(0).IdComercio
            req.moneda = result(0).CodMoneda
            req.emisor = result(0).NroEmisor
            req.respuesta = result(0).CodRespuesta

            Try
                If req.ida.oper = 2 Or req.ida.oper = 3 Then
                    req.retrefnumber = result(0).RetRefNumber
                End If
            Catch ex As Exception
            End Try
            If result(0).NroPlan = 0 Then
                req.planemisor = result(0).NroPlan.ToString + CInt(result(0).Cuotas).ToString("00")
            Else
                If result(0).NroPlan = 991 Then
                    req.planemisor = "001"
                Else
                    req.planemisor = result(0).NroPlan
                End If


            End If
        End If

    End Sub



    ''' <summary>
    ''' Busca la terminal en la tabla Numeros.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Modificar_Claves(terminal As String, pHost As Byte, wkd As String, wkp As String)

        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where Trim(c.Terminal) = Trim(terminal) And c.NroHost = pHost
                         Select c

            For Each reg In result
                If wkd <> "" Then reg.WKDatos = wkd
                If wkp <> "" Then reg.WKPines = wkp
                dt.Update(reg)

            Next

        Catch ex As Exception
            Logger.Error("No se pudo modificar claves.")

        End Try
    End Sub
    ''' <summary>
    ''' Busca la terminal en la tabla Numeros.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub BuscarClaves(pCajaPropio As String, pHost As Byte, req As Req)

        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where c.NroCajaPropio = pCajaPropio And c.NroHost = pHost
                         Select c

            For Each reg In result

                req.ClaveDatos = Trim(reg.WKDatos)
                req.ClavePines = Trim(reg.WKPines)

            Next

        Catch ex As Exception
            Logger.Error("No se pudo buscar terminal.")

        End Try
    End Sub


    ''' <summary>
    ''' Busca la terminal en la tabla Numeros.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub BuscarClaves(pCajaPropio As String, pHost As Byte, respuesta As TransmisorTCP.MensajeRespuesta)
        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where c.NroCajaPropio = pCajaPropio And c.NroHost = pHost
                         Select c

            For Each reg In result
                respuesta.PosicionMK = Trim(reg.PosicionMK)
                respuesta.Clave_wkd = Trim(reg.WKDatos)
                respuesta.Clave_wkp = Trim(reg.WKPines)
            Next
        Catch ex As Exception
            Logger.Error(String.Format("No se pudo buscar clave Host: {0} Caja: {1}.", pHost, pCajaPropio))
        End Try
    End Sub

    ''' <summary>
    ''' Busca la terminal en la tabla Numeros.
    ''' </summary>
    ''' <remarks></remarks>
    Public Function BuscarTerminal(pCajaPropio As String, pHost As Byte) As String
        Dim term As String = ""
        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where c.NroCajaPropio = pCajaPropio And c.NroHost = pHost
                         Select c

            For Each reg In result
                term = Trim(reg.Terminal)
            Next
            Return Trim(term)
        Catch ex As Exception
            Logger.Error("No se pudo buscar terminal.")
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' Busca la Nro de version del pp en la tabla Numeros, para cierre 3DES
    ''' </summary>
    ''' <remarks></remarks>
    Public Function BuscarVersionSoftPP(pterminal As String, pHost As Byte) As String
        Dim VersionPP As String = ""
        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where c.Terminal.Trim = pterminal And c.NroHost = pHost
                         Select c

            For Each reg In result
                If reg.VersionSoftPP.Trim IsNot Nothing Then
                    VersionPP = Trim(reg.VersionSoftPP)
                End If
            Next
            Return Trim(VersionPP)
        Catch ex As Exception
            Logger.Error("No se pudo buscar VersionPP.")
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' Busca la Nro de serie del pp en la tabla Numeros.
    ''' </summary>
    ''' <remarks></remarks>
    Public Function BuscarSeriePP(pCajaPropio As String, pHost As Byte) As String
        Dim seriePP As String = ""
        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where c.NroCajaPropio = pCajaPropio And c.NroHost = pHost
                         Select c

            For Each reg In result
                seriePP = Trim(reg.SeriePP)
            Next
            Return Trim(seriePP)
        Catch ex As Exception
            Logger.Error("No se pudo buscar SeriePP.")
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' Busca en tabla movimientos los tipo mensaje 200 que no tienen respuesta 210 ni reverso 400 y que correspondan al ultimo trace de la terminal.
    ''' Arma el req y crea una lista de movimientos para reversar.
    ''' </summary>
    ''' <remarks></remarks>
    Public Function BuscarVentasIncompletas() As List(Of Req)
        Dim reversar As Boolean
        Dim lista As New List(Of Req)
        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter

        '--- mensaje 200 es VENTA, ANULACION Y DEVOLUCION  
        '---      CUANDO ES VENTA      CODPROC=0           
        '---      CUANDO ES ANULACION  CODPROC=20000       
        '---      CUANDO ES DEVOLUCION CODPROC=200000      
        '--- Igual con el mensaje 210 de respuesta         
        '--- Mensaje 400 es reverso y 410 es la respuesta  
        Dim Datos0200 = From c In dt.GetData() Where c.TipoMensaje = "0200" And c.FechaHora.Date = Now.Date Select c.NroHost, c.Terminal, c.NroTrace
        Dim Datos0210 = From c In dt.GetData() Where c.TipoMensaje = "0210" And c.FechaHora.Date = Now.Date Select c.NroHost, c.Terminal, c.NroTrace
        Dim Datos0400 = From c In dt.GetData() Where c.TipoMensaje = "0400" And c.FechaHora.Date = Now.Date Select c.NroHost, c.Terminal, c.NroTrace

        Dim DatosNo200x210 = Datos0200.Except(Datos0210) ' estos son los que no tuvieron respuesta

        Dim SinRespuesta = DatosNo200x210.Except(Datos0400) ' estos son los que no tuvieron respuesta y no tienen reverso

        For Each reg In SinRespuesta
            If Ultimo_Trace(reg.NroHost, Trim(reg.Terminal)) <> reg.NroTrace Then
                BorrarDatosCriticos(Trim(reg.Terminal), reg.NroHost, reg.NroTrace)
                Logger.Info(String.Format("Movimiento muy antiguo, sin reversar. Req: {0}/{1}/{2}", CType(reg.NroHost, TjComun.IdaLib.TipoHost), Trim(reg.Terminal), reg.NroTrace))
                Continue For
            End If

            Dim DatosReverso = From c In dt.GetData()
                               Where c.TipoMensaje = "0200" And c.NroHost = reg.NroHost And c.Terminal.Trim = reg.Terminal.Trim And c.NroTrace = reg.NroTrace
                               Select c
            Dim id As New TjComun.IdaLib.IdaTypeInternal
            id.CAJERA = DatosReverso(0).Cajera
            id.MANUAL = DatosReverso(0).ModoIngreso
            Try
                id.TRACK2 = TjComun.IdaLib.DesencriptarStr(DatosReverso(0).Track2)
            Catch ex As Exception
                reversar = False
            End Try
            Try
                id.CODSEG = TjComun.IdaLib.DesencriptarStr(DatosReverso(0).CodSeg)
            Catch ex As Exception
                reversar = False
            End Try
            id.CASHBACK = DatosReverso(0).Cashback
            Try
                id.EXPDATE = DatosReverso(0).Vencimiento
            Catch ex As Exception
                reversar = False
            End Try
            id.FECORI = DatosReverso(0).FechaHoraLocal
            Try
                id.TARJ = TjComun.IdaLib.DesencriptarStr(DatosReverso(0).Tarjeta)
            Catch ex As Exception
                reversar = False
            End Try
            id.PLANINT = DatosReverso(0).NroPlan
            id.cajadir = Configuracion.DirTarjetas + "\" + DatosReverso(0).Pinguino.ToString("00")
            'id.HORA = reg.FechaHora

            Try
                If DatosReverso(0).Track1 IsNot Nothing AndAlso Not String.IsNullOrEmpty(DatosReverso(0).Track1.ToString) Then id.TRACK1 = TjComun.IdaLib.DesencriptarStr(DatosReverso(0).Track1)
            Catch ex As Exception
                reversar = False
            End Try
            id.CHECK = "O2"

            Dim rq As New Req(id, Trim(reg.Terminal), reg.NroHost)
            rq.vta = TjComun.IdaLib.InicializarVtaDesdeIDA(rq.ida)
            rq.operacion = DatosReverso(0).CodProc
            rq.nroTrace = reg.NroTrace
            rq.FechaHoraEnvioMsg = DatosReverso(0).FechaHora
            rq.emisor = DatosReverso(0).NroEmisor
            rq.descEmisor = DatosReverso(0).NombreEmisor
            '--------------------------------------------------
            'es para el caso de reversar una anulacion o devolucion. Por eso lo saco
            'rq.retrefnumber = reg.RetRefNumber 
            'Try
            '    If reg.CodAutorizacion <> "" Then rq.autorizacion = reg.CodAutorizacion
            'Catch ex As Exception
            'End Try
            '--------------------------------------------------
            rq.pinguino = DatosReverso(0).Pinguino.ToString("00")
            rq.nroCaja = DatosReverso(0).NroCajaPropio
            rq.emisor = DatosReverso(0).NroEmisor
            rq.cuotas = DatosReverso(0).Cuotas
            rq.moneda = DatosReverso(0).CodMoneda
            rq.nroTicket = DatosReverso(0).TicketPosnet
            rq.importe = DatosReverso(0).Importe
            rq.FechaHoraEnvioMsg = DatosReverso(0).FechaHora
            rq.idComercio = Trim(DatosReverso(0).IdComercio)
            Try
                'va solo en caso de ser un reverso de anulacion o anulacion/devolucion
                If rq.operacion = IsoLib.E_ProcCode.AnulacionCompra Or rq.operacion = IsoLib.E_ProcCode.AnulacionDevolucion Or
                   rq.operacion = IsoLib.E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroD Or rq.operacion = IsoLib.E_ProcCode.AnulacionDevolucion_Compra_maestro_CajAhorroP Or
                   rq.operacion = IsoLib.E_ProcCode.AnulacionDevolucion_Compra_maestro_CtaCteD Or rq.operacion = IsoLib.E_ProcCode.AnulacionDevolucion_Compra_maestro_CtaCteP Or
                   rq.operacion = IsoLib.E_ProcCode.anulacion_compra_cashback Or rq.operacion = IsoLib.E_ProcCode.Anulacion_Compra_maestro_CajAhorroD Or
                   rq.operacion = IsoLib.E_ProcCode.Anulacion_Compra_maestro_CajAhorroP Or rq.operacion = IsoLib.E_ProcCode.Anulacion_Compra_maestro_CtaCteD Or
                   rq.operacion = IsoLib.E_ProcCode.Anulacion_Compra_maestro_CtaCteP Then
                    rq.retrefnumber = DatosReverso(0).RetRefNumber
                End If
            Catch ex As Exception
                reversar = False
            End Try
            'TODO: Agregar para reversar los campos para EMV.
            If reversar Then
                lista.Add(rq)
            Else
                Logger.Info(String.Format("Imposible reversar, faltan datos obligatorios. Req: {0}/{1}/{2}", rq.nombreHost, Trim(rq.terminal), rq.nroTrace))
            End If
        Next
        Logger.Info(String.Format("Cant de movimientos a reversar {0}", lista.Count))
        Return lista
    End Function


    '''' <summary>
    '''' Busca en tabla movimientos los tipo mensaje 200 que no tienen respuesta 210 ni reverso 400 y que correspondan al ultimo trace de la terminal.
    '''' Arma el req y crea una lista de movimientos para reversar.
    '''' </summary>
    '''' <remarks></remarks>
    'Public Function BuscarVentasIncompletas() As List(Of Req)
    '    Dim reversar As Boolean
    '    Dim lista As New List(Of Req)
    '    Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
    '    Dim result = From c In dt.GetData() Where c.TipoMensaje = "0200" And c.FechaHora.Date = Now.Date Select c
    '    For Each reg In result
    '        reversar = True
    '        Dim result2 = From x In dt.GetData()
    '                      Where x.TipoMensaje = "0210" And Trim(x.Terminal) = Trim(reg.Terminal) And x.NroHost = reg.NroHost And x.NroTrace = reg.NroTrace And x.FechaHora.Date = reg.FechaHora.Date
    '                      Select x

    '        Dim result3 = From z In dt.GetData()
    '                      Where z.TipoMensaje = "0400" And Trim(z.Terminal) = Trim(reg.Terminal) And z.NroHost = reg.NroHost And z.NroTrace = reg.NroTrace And z.FechaHora.Date = reg.FechaHora.Date
    '                      Select z
    '        If result2.Count = 0 And result3.Count = 0 Then
    '            If Ultimo_Trace(reg.NroHost, Trim(reg.Terminal)) <> reg.NroTrace Then
    '                BorrarDatosCriticos(Trim(reg.Terminal), reg.NroHost, reg.NroTrace)
    '                Logger.Info(String.Format("Movimiento muy antiguo, sin reversar. Req: {0}/{1}/{2}", CType(reg.NroHost, TjComun.IdaLib.TipoHost), Trim(reg.Terminal), reg.NroTrace))
    '                Continue For
    '            End If

    '            Dim id As New TjComun.IdaLib.IdaTypeInternal
    '            id.CAJERA = reg.Cajera
    '            id.MANUAL = reg.ModoIngreso
    '            Try
    '                id.TRACK2 = TjComun.IdaLib.DesencriptarStr(reg.Track2)
    '            Catch ex As Exception
    '                reversar = False
    '            End Try
    '            Try
    '                id.CODSEG = TjComun.IdaLib.DesencriptarStr(reg.CodSeg)
    '            Catch ex As Exception
    '                reversar = False
    '            End Try
    '            id.CASHBACK = reg.Cashback
    '            Try
    '                id.EXPDATE = reg.Vencimiento
    '            Catch ex As Exception
    '                reversar = False
    '            End Try
    '            id.FECORI = reg.FechaHoraLocal
    '            Try
    '                id.TARJ = TjComun.IdaLib.DesencriptarStr(reg.Tarjeta)
    '            Catch ex As Exception
    '                reversar = False
    '            End Try
    '            id.PLANINT = reg.NroPlan
    '            id.cajadir = Configuracion.DirTarjetas + "\" + reg.Pinguino.ToString("00")
    '            'id.HORA = reg.FechaHora

    '            Try
    '                If reg.Track1 IsNot Nothing AndAlso Not String.IsNullOrEmpty(reg.Track1.ToString) Then id.TRACK1 = TjComun.IdaLib.DesencriptarStr(reg.Track1)
    '            Catch ex As Exception
    '                reversar = False
    '            End Try
    '            id.CHECK = "O2"

    '            Dim rq As New Req(id, Trim(reg.Terminal), reg.NroHost)
    '            rq.vta = TjComun.IdaLib.InicializarVtaDesdeIDA(rq.ida)
    '            rq.operacion = reg.CodProc
    '            rq.nroTrace = reg.NroTrace
    '            rq.FechaHoraEnvioMsg = reg.FechaHora
    '            rq.emisor = reg.NroEmisor
    '            rq.descEmisor = reg.NombreEmisor
    '            '--------------------------------------------------
    '            'es para el caso de reversar una anulacion o devolucion. Por eso lo saco
    '            'rq.retrefnumber = reg.RetRefNumber 
    '            'Try
    '            '    If reg.CodAutorizacion <> "" Then rq.autorizacion = reg.CodAutorizacion
    '            'Catch ex As Exception
    '            'End Try
    '            '--------------------------------------------------
    '            rq.pinguino = reg.Pinguino.ToString("00")
    '            rq.nroCaja = reg.NroCajaPropio
    '            rq.emisor = reg.NroEmisor
    '            rq.cuotas = reg.Cuotas
    '            rq.moneda = reg.CodMoneda
    '            rq.nroTicket = reg.TicketPosnet
    '            rq.importe = reg.Importe
    '            rq.FechaHoraEnvioMsg = reg.FechaHora
    '            rq.idComercio = Trim(reg.IdComercio)
    '            rq.retrefnumber = reg.RetRefNumber
    '            If reversar Then
    '                lista.Add(rq)
    '            Else
    '                Logger.Info(String.Format("Imposible reversar, faltan datos obligatorios. Req: {0}/{1}/{2}", rq.nombreHost, Trim(rq.terminal), rq.nroTrace))
    '            End If
    '        End If
    '    Next

    '    Logger.Info(String.Format("Cant de movimientos a reversar {0}", lista.Count))
    '    Return lista
    'End Function

    Public Function Logger() As log4net.ILog
        Return log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod.DeclaringType)
    End Function

    ' ''' <summary>
    ' ''' Filtra la tabla movimientos por host para realizar el cierre.
    ' ''' </summary>
    ' ''' <param name="host">Nro de host a cerrar</param>
    ' ''' <returns></returns>
    ' ''' <remarks></remarks>
    'Public Function FiltrarParaCerrar(host As Integer) As Dictionary(Of String, MovCierres)

    '    '----- MARCOS 
    '    Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
    '    Dim mov As MovCierres

    '    Dim result = From c In dt.GetData() _
    '                     Where c.TipoMensaje = "0210" And c.NroHost = host And c.hizocierre = False And c.CodRespuesta = 0 _
    '                     Group c By c.NroHost, c.Terminal, c.CodProc Into suma = Sum(c.Importe), cant = Count() _
    '                     Select NroHost, Terminal, CodProc, cant, suma

    '    Dim acumulados As New Dictionary(Of String, MovCierres)
    '    For Each reg In result

    '        If acumulados.ContainsKey(reg.Terminal) Then
    '            mov = acumulados(reg.Terminal)
    '        Else
    '            mov = New MovCierres
    '        End If
    '        Select Case CInt(reg.CodProc)
    '            Case 0, 1000, 2000, 8000, 9000, 90000
    '                mov.SumaCompras = mov.SumaCompras + reg.suma
    '                mov.CantCompras = mov.CantCompras + reg.cant
    '            Case 200000, 201000, 202000, 208000, 209000 ' DEVOLUCION
    '                mov.SumaDevoluciones = mov.SumaDevoluciones + reg.suma
    '                mov.CantDevoluciones = mov.CantDevoluciones + reg.cant
    '                mov.SumaCompras = mov.SumaCompras - reg.suma
    '                mov.CantCompras = mov.CantCompras - reg.cant
    '            Case 20000, 21000, 22000, 28000, 29000, 140000 ' ANULACION COMPRA
    '                mov.SumaAnulaciones = mov.SumaAnulaciones + reg.suma
    '                mov.CantAnulaciones = mov.CantAnulaciones + reg.cant
    '                mov.SumaDevoluciones = mov.SumaDevoluciones - reg.suma
    '                mov.CantDevoluciones = mov.CantDevoluciones - reg.cant
    '            Case 220000, 221000, 222000, 228000, 229000 ' ANULACION DEVOLUCION
    '                mov.SumaAnulaciones = mov.SumaAnulaciones + reg.suma
    '                mov.CantAnulaciones = mov.CantAnulaciones + reg.cant
    '                mov.SumaDevoluciones = mov.SumaDevoluciones - reg.suma
    '                mov.CantDevoluciones = mov.CantDevoluciones - reg.cant
    '        End Select

    '        If acumulados.ContainsKey(reg.Terminal) Then ' ver la clave si esta bien o si se pueden repetir 
    '            acumulados(reg.Terminal) = mov
    '        Else
    '            'mov.emisor = reg.NroEmisor
    '            acumulados.Add(reg.Terminal, mov)
    '        End If
    '    Next
    '    '-----------------------------
    '    ' RESTA LOS REVERSOS          
    '    '-----------------------------
    '    Dim dt2 As New DatosTjTableAdapters.MovimientosTableAdapter
    '    Dim result2 = From c In dt.GetData() _
    '                     Where c.TipoMensaje = "0400" And c.NroHost = host And c.hizocierre = False _
    '                     Group c By c.NroHost, c.Terminal, c.CodProc Into suma = Sum(c.Importe), cant = Count() _
    '                     Select NroHost, Terminal, CodProc, cant, suma
    '    For Each reg2 In result2
    '        If acumulados.ContainsKey(reg2.Terminal) Then
    '            mov = acumulados(reg2.Terminal)
    '        Else
    '            'mov = New MovCierres
    '            MsgBox("No existe clave mov 400 1")
    '        End If
    '        Select Case CInt(reg2.CodProc)
    '            Case 0, 1000, 2000, 8000, 9000, 90000
    '                mov.SumaCompras = mov.SumaCompras - reg2.suma
    '                mov.CantCompras = mov.CantCompras - reg2.cant
    '            Case 200000, 201000, 202000, 208000, 209000 ' DEVOLUCION
    '                mov.SumaDevoluciones = mov.SumaDevoluciones - reg2.suma
    '                mov.CantDevoluciones = mov.CantDevoluciones - reg2.cant
    '            Case 20000, 21000, 22000, 28000, 29000, 140000 ' ANULACION COMPRA
    '                mov.SumaAnulaciones = mov.SumaAnulaciones - reg2.suma
    '                mov.CantAnulaciones = mov.CantAnulaciones - reg2.cant
    '                mov.SumaCompras = mov.SumaCompras + reg2.suma
    '                mov.CantCompras = mov.CantCompras + reg2.cant
    '            Case 220000, 221000, 222000, 228000, 229000 ' ANULACION DEVOLUCION
    '                mov.SumaAnulaciones = mov.SumaAnulaciones - reg2.suma
    '                mov.CantAnulaciones = mov.CantAnulaciones - reg2.cant
    '                mov.SumaDevoluciones = mov.SumaDevoluciones + reg2.suma
    '                mov.CantDevoluciones = mov.CantDevoluciones + reg2.cant
    '        End Select
    '        If acumulados.ContainsKey(reg2.Terminal) Then ' ver la clave si esta bien o si se pueden repetir 
    '            acumulados(reg2.Terminal) = mov
    '        Else
    '            'acumulados.Add(reg2.Terminal, mov)
    '            MsgBox("No existe clave mov 400 2")
    '        End If
    '    Next

    '    Return acumulados
    'End Function


#Region "Generación de reportes"


    ''' <summary>
    ''' Arma el .PEI con los movimientos aprobados
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CulminacionCierreSatisfactoriaPEI(fecha_cierre As Date)
        Dim linea As String
        Dim dt As New DatosTjTableAdapters.MovimientosPEITableAdapter
        Dim res0200 = From c In dt.GetData()
                      Where c.CodRespuesta = "00"
                      Select c
        Dim nombrearc As String

        If Configuracion.Ciudad = Configuracion.SanFrancisco Then
            nombrearc = fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre.ToString("yyyy-MM-dd")).ToString("00") + ".PE6"
        Else
            nombrearc = fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".PEI"
        End If
        Logger.Info("Generando arhivo para importacion: " & nombrearc)

        For Each reg0200 In res0200
            Dim oper As String = ""
            With reg0200
                If .Operacion.Trim = lib_PEI.Concepto.COMPRA_DE_BIENES.ToString Then
                    oper = "0"
                ElseIf .Operacion.Trim = lib_PEI.Concepto.DEVOLUCION.ToString Then
                    oper = "1"
                End If

                'linea = $";{ .Cupon};{ .Primeros6}XXX{ .Ultimos4};{ .Importe};{ Format(.FechaHora, "dd/MM/yy")};{ Format(.FechaHora, "HHmm") };{ .Pinguino};{ .Caja};{ .TicketCaja};;{Format(fecha_cierre, "dd/MM/yy")};;{oper};{ .Emisor};{ .Terminal};;;;;;{ .IdOperacion};{ .IdOperacionOrigen};{ .ReferenciaBancaria}"
                linea = $"{ .Cupon};{ .Primeros6}XXX{ .Ultimos4};{ .Importe};{ Format(.FechaHora, "dd/MM/yy")};{ Format(.FechaHora, "HHmm") };{ .Pinguino};{ .Caja};{ .TicketCaja};{Format(fecha_cierre, "dd/MM/yy")};{oper};{ .Emisor};{ .Terminal};{ .IdOperacion};{ .IdOperacionOrigen};{ .ReferenciaBancaria}"
            End With
            EscribeArchivoCierrePEI(nombrearc, linea)
        Next

        Logger.Info("Archivo de importacion: " & nombrearc & " generado.")
    End Sub

    ''' <summary>
    ''' Arma el .POS con los movimientos 210 aprobados y que no tenga 410.
    ''' </summary>
    ''' <param name="req"></param>
    ''' <remarks></remarks>
    Public Sub CulminacionCierreSatisfactoria(req As Req)
        Dim linea As String
        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim res0200 = From c In dt.GetData()
                      Where Trim(c.Terminal) = Trim(req.terminal) And c.NroHost = req.nrohost And c.hizocierre = False And c.TipoMensaje = "0200"
                      Select c
        Dim nombrearc As String
        If Configuracion.Ciudad = Configuracion.SanFrancisco Then
            nombrearc = req.fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(req.fecha_cierre).ToString("00") + ".VI6"
        Else
            nombrearc = req.fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(req.fecha_cierre).ToString("00") + ".VIS"
        End If


        For Each reg0200 In res0200
            Dim res0210 = From x In dt.GetData()
                          Where Trim(x.Terminal) = Trim(reg0200.Terminal) And x.NroHost = reg0200.NroHost And x.NroTrace = reg0200.NroTrace And x.TipoMensaje = "0210"
                          Select x

            Dim res0400 = From x In dt.GetData()
                          Where Trim(x.Terminal) = Trim(reg0200.Terminal) And x.NroHost = reg0200.NroHost And x.NroTrace = reg0200.NroTrace And x.TipoMensaje = "0400"
                          Select x

            If res0210.Count > 0 AndAlso res0210(0).CodRespuesta.Trim = "0" AndAlso res0400.Count = 0 Then
                Dim reg = res0210(0)
                linea = req.nroLote.ToString + ";"         '--- Nro de lote
                linea = linea + reg.TicketPosnet.ToString + ";"                       '--- Nro de cupon
                linea = linea + reg.Primerso6 + "XXX" + reg.Ultimos4 + ";"            '--- tarjeta
                If reg.NroHost = TjComun.TipoHost.POSNET Or reg.NroHost = TjComun.TipoHost.Posnet_homolog Then
                    linea = linea + (reg.Importe + reg.Cashback).ToString + ";"                            '--- Importe
                Else
                    linea = linea + reg.Importe.ToString + ";"                            '--- Importe
                End If
                'linea = linea + reg.Importe.ToString + ";"                            '--- Importe
                linea = linea + Format(reg.FechaHora, "dd/MM/yy") + ";"               '--- Fecha Transaccion
                linea = linea + Format(reg.FechaHora, "HHmm") + ";"                   '--- Hora Transaccion
                linea = linea + reg.Pinguino.ToString + ";"                           '--- Pinguino
                linea = linea + reg.NroCajaPropio.ToString + ";"                      '--- caja
                linea = linea + reg.TicketCaja.ToString + ";"                         '--- ticket
                linea = linea + "0;"                                                  '--- off on
                linea = linea + Format(req.fecha_cierre, "dd/MM/yy") + ";"            '--- Fecha Cierre
                linea = linea + "1;"                                                  '--- estado
                linea = linea + reg.TipoOperacion.Trim + ";"                          '--- operacion 0=venta
                linea = linea + reg.NombreEmisor + ";"                                '--- nombre tarjeta
                linea = linea + reg.Terminal + ";"                                    '--- id terminal
                linea = linea + reg.IdComercio + ";"                                  '--- id comercio
                linea = linea + BuscarIDComercioCierre(reg.NroHost) + ";"             '--- id comercio cierre
                linea = linea + reg.Cuotas.ToString + ";"                             '--- cuotas
                linea = linea + reg.NroPlan.ToString + ";"                            '--- planes
                linea = linea + reg.Cashback.ToString + ";"                           '--- cashback
                linea = linea + reg.CodAutorizacion + ";"                             '--- Cod. Autorización
                EscribeArchivoCierre(nombrearc, linea)
            End If
        Next
        '--- PONE HIZOCIERRE=TRUE 
        CerrarTransacciones(Trim(req.terminal).ToString, req.nrohost, req.nroLote)
        borrar_claves()
        Incrementar_Lote(Trim(req.terminal), req.nrohost)
    End Sub


    Public Sub CulminacionCierreNOSatisfactoria(req As Req)

        Dim linea As String
        Dim db As New DatosTj
        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim result = From c In dt.GetData()
                     Where Trim(c.Terminal) = Trim(req.terminal) And c.NroHost = req.nrohost And c.hizocierre = False
                     Select c
        Dim nombrearc As String = "TarjPendientes_" & req.fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(req.fecha_cierre).ToString("00") + ".txt"


        For Each reg In result
            'linea = reg.TipoMensaje + ";"                                         '--- Tipo Mensaje 200=Compra, 400=Reverso, etc 
            'linea = linea + Ultimo_Lote(reg.NroHost, Trim(reg.Terminal)).ToString + ";" '--- Nro de lote
            'linea = linea + reg.TicketPosnet.ToString + ";"                       '--- Nro de cupon
            'linea = reg.Primerso6 + "XXX" + reg.Ultimos4 + ";"            '--- tarjeta
            linea = reg.Pinguino.ToString + ";"                           '--- Pinguino
            linea = linea + reg.NroCajaPropio.ToString + ";"                      '--- caja
            linea = linea + reg.TicketCaja.ToString + ";"                         '--- ticket
            If reg.NroHost = TjComun.TipoHost.POSNET Or reg.NroHost = TjComun.TipoHost.Posnet_homolog Then
                linea = linea + (reg.Importe + reg.Cashback).ToString + ";"                            '--- Importe
            Else
                linea = linea + reg.Importe.ToString + ";"                            '--- Importe
            End If
            'linea = linea + reg.Importe.ToString + ";"                            '--- Importe
            linea = linea + Format(reg.FechaHora, "dd/MM/yy") + ";"               '--- Fecha Transaccion
            linea = linea + Format(reg.FechaHora, "HHmm") + ";"                   '--- Hora Transaccion
            'linea = linea + reg.Pinguino.ToString + ";"                           '--- Pinguino
            'linea = linea + reg.NroCajaPropio.ToString + ";"                      '--- caja
            'linea = linea + reg.TicketCaja.ToString + ";"                         '--- ticket
            'linea = linea + "0;"                                                  '--- off on
            'linea = linea + Format(req.fecha_cierre, "dd/MM/yy") + ";"            '--- Fecha Cierre
            'linea = linea + "1;"                                                  '--- estado
            linea = linea + reg.TipoOperacion.Trim + ";"                          '--- operacion 0=venta
            linea = linea + reg.NombreEmisor + ";"                                '--- nombre tarjeta
            'linea = linea + reg.Terminal + ";"                                    '--- id terminal
            'linea = linea + reg.IdComercio + ";"                                  '--- id comercio
            'linea = linea + BuscarIDComercioCierre(reg.NroHost) + ";"             '--- id comercio cierre
            linea = linea + reg.Cuotas.ToString + ";"                             '--- cuotas
            linea = linea + reg.NroPlan.ToString + ";"                            '--- planes
            linea = linea + reg.Cashback.ToString + ";"                           '--- cashback
            EscribeArchivoCierre(nombrearc, linea)
        Next
        'Enviar_Achivos_Mail(0, adjuntos, "CIERRES TARJETAS Fecha: " + Now.Date.ToString("dd/MM/yyyy"))
    End Sub

    Public Sub ListadosReportesTarjetasPEI(ByVal fecha_cierre As Date)
        Dim hayalgo As Boolean = False
        Dim adjuntos As String
        Dim linea, nombreEmisor As String
        Dim lacanti, lacantitot As Integer
        Dim eltot, eltottot As Decimal
        Dim nombrearc2, nombrearc3 As String

        Dim dt As New DatosTjTableAdapters.MovimientosPEITableAdapter
        Logger.Info("Generando reporte PEI tarjetas global...")
        '--- TODOS LOS PINGUINOS ----------------
        Dim res0200 = From c In dt.GetData()
                      Where c.CodRespuesta = "00"
                      Order By c.Emisor, c.Pinguino, c.Caja, c.FechaHora
                      Select c

        If res0200.Count = 0 Then
            Logger.Info("No hay movimientos para informar. " & fecha_cierre.ToString("dd/MM/yy"))
            Exit Sub
        End If

        nombrearc2 = "ReportePEIMov_" & fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt"
        adjuntos = nombrearc2 + ";"

        linea = "REPORTE GENERADO: " + Format(fecha_cierre, "dd/MM/yy") + "   " + fecha_cierre.ToString("HH:mm")
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "---------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "TARJETA        OPERACION                IMPORTE  PING CAJA CAJERA  CUPON    FEC/HORA TRANS.  ID OPERACION   ID OPERACION ORIG"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "---------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)

        nombrearc3 = "ReportesPEITotales_" & fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt"
        adjuntos = adjuntos + nombrearc3 + ";"
        linea = "REPORTE GENERADO: " + Format(fecha_cierre, "dd/MM/yy") + "   " + fecha_cierre.ToString("HH:mm")
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "TOTAL DE OPERACIONES"
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "---------------------------------------------------"
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "EMISOR            CANT.TRANSACCIONES         TOTAL"
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "---------------------------------------------------"
        EscribeArchivoCierre(nombrearc3, linea)


        lacanti = 0
        eltot = 0
        lacantitot = 0
        eltottot = 0
        nombreEmisor = ""
        For Each reg0200 In res0200

            hayalgo = True
            If nombreEmisor = "" Then nombreEmisor = reg0200.Emisor
            If nombreEmisor <> reg0200.Emisor Then 'cambio emisor

                linea = $"{nombreEmisor.Trim.PadRight(25)}{Format(lacanti, "###,##0").PadLeft(7)}{Format(eltot, "##,###,##0.00").PadLeft(15)}"
                'linea = $"{nombreEmisor} { Format(lacanti, "###,##0").PadLeft(7)} {Format(eltot, "##,###,##0.00").PadLeft(13)}"
                EscribeArchivoCierre(nombrearc2, linea)

                linea = $"{nombreEmisor.Trim.PadRight(25)}{Format(lacanti, "###,##0").PadLeft(7)}{Format(eltot, "##,###,##0.00").PadLeft(18)}"
                EscribeArchivoCierre(nombrearc3, linea)

                lacanti = 0
                eltot = 0
                linea = "  "
                EscribeArchivoCierre(nombrearc2, linea)
                linea = "  "
                EscribeArchivoCierre(nombrearc2, linea)
                nombreEmisor = reg0200.Emisor
            End If


            linea = $"{reg0200.Primeros6}XXX{reg0200.Ultimos4}  {reg0200.Operacion.Trim.PadRight(20)}{reg0200.Importe.ToString.PadLeft(12)}   {reg0200.Pinguino.ToString("00")}   {reg0200.Caja.ToString("00")}   {reg0200.Cajera.ToString.PadLeft(4)}  {reg0200.Cupon.ToString.PadLeft(6)}    {Format(reg0200.FechaHora, "dd/MM/yy")} {Format(reg0200.FechaHora, "HH:mm")}   {reg0200.IdOperacion.Trim.PadLeft(12)}   {reg0200.IdOperacionOrigen.Trim.PadLeft(12)}"
            EscribeArchivoCierre(nombrearc2, linea)

            lacanti = lacanti + 1
            lacantitot = lacantitot + 1

            If reg0200.Operacion.Trim = lib_PEI.Concepto.COMPRA_DE_BIENES.ToString Then
                eltot += reg0200.Importe
                eltottot += reg0200.Importe
            ElseIf reg0200.Operacion.Trim = lib_PEI.Concepto.DEVOLUCION.ToString Then
                eltot -= reg0200.Importe
                eltottot -= reg0200.Importe
            ElseIf reg0200.Operacion.Trim = lib_PEI.Concepto.EXTRACCION.ToString Then
                eltot += reg0200.Importe
                eltottot += reg0200.Importe
            End If
        Next
        linea = $"{nombreEmisor.Trim.PadRight(25)}{Format(lacanti, "###,##0").PadLeft(7)}{Format(eltot, "##,###,##0.00").PadLeft(15)}"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "---------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = $"TOTAL GENERAL            {lacantitot.ToString.PadLeft(7)}{eltottot.ToString("##,###,##0.00").PadLeft(15)} "
        EscribeArchivoCierre(nombrearc2, linea)

        linea = $"{nombreEmisor.Trim.PadRight(25)}{Format(lacanti, "###,##0").PadLeft(7)}{Format(eltot, "##,###,##0.00").PadLeft(18)}"
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "---------------------------------------------------"
        EscribeArchivoCierre(nombrearc3, linea)
        linea = $"TOTAL GENERAL            {Format(lacantitot, "###,##0").PadLeft(7)}{Format(eltottot, "##,###,##0.00").PadLeft(18)}"
        EscribeArchivoCierre(nombrearc3, linea)


        Logger.Info("Fin generación reporte PEI tarjetas global ...")
        If hayalgo Then
            Enviar_Achivos_Mail(0, adjuntos, "CIERRES TARJETAS PEI Fecha: " + fecha_cierre.ToString("dd/MM/yyyy"))
        Else
            Logger.Info(String.Format("Archivo Vacío, no se envió mail global"))
        End If
    End Sub


    Public Sub ListadosReportesTarjetas(ByVal fecha_cierre As Date)
        Dim hayalgo As Boolean = False
        Dim adjuntos As String
        Dim linea, nombreEmisor, lafecha As String
        Dim lacanti, lacantitot As Integer
        Dim eltot, eltottot As Decimal
        Dim nombrearc2, nombrearc3, nombrearc4 As String
        Dim cantiarray As Integer
        Dim imporarray As Decimal


        Dim ii As Integer
        Dim _Fecha(99) As String
        Dim _Emisor(99) As String
        Dim _Cantidad(99) As Integer
        Dim _Importe(99) As Decimal

        For ii = 1 To 99
            _Fecha(ii) = ""
            _Emisor(ii) = ""
            _Cantidad(ii) = 0
            _Importe(ii) = 0
        Next

        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Logger.Info("Generando reporte tarjetas global...")
        '--- TODOS LOS PINGUINOS ----------------
        Dim res0200 = From c In dt.GetData()
                      Where c.TipoMensaje = "0200" And c.hizocierre = True
                      Order By c.NombreEmisor, c.Pinguino, c.NroCajaPropio, c.FechaHora
                      Select c

        nombrearc2 = "TarjRep2_" & fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt"
        adjuntos = nombrearc2 + ";"

        linea = "REPORTE GENERADO: " + Format(fecha_cierre, "dd/MM/yy") + "   " + fecha_cierre.ToString("HH:mm")
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "---------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "TARJETA        VENC A/M OPERACION       IMPORTE  PING CAJA CAJERA TICKET LOTE  FEC/HORA CIERRE CUOTAS  FEC/HORA TRANS.  COD. AUT."
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "---------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)

        nombrearc3 = "TarjRep_" & fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt"
        adjuntos = adjuntos + nombrearc3 + ";"
        linea = "REPORTE GENERADO: " + Format(fecha_cierre, "dd/MM/yy") + "   " + fecha_cierre.ToString("HH:mm")
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "TOTAL DE OPERACIONES"
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "---------------------------------------------------"
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "EMISOR            CANT.TRANSACCIONES         TOTAL"
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "---------------------------------------------------"
        EscribeArchivoCierre(nombrearc3, linea)

        nombrearc4 = "TarjReversos_" & fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt"
        adjuntos = adjuntos + nombrearc4 + ";"

        linea = "REPORTE DE    R E V E R S O S    GENERADO: " + Format(fecha_cierre, "dd/MM/yy") + "   " + fecha_cierre.ToString("HH:mm")
        EscribeArchivoCierre(nombrearc4, linea)
        linea = "---------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc4, linea)
        linea = "TARJETA        VENC A/M OPERACION       IMPORTE  PING CAJA CAJERA TICKET LOTE  FEC/HORA CIERRE CUOTAS  FEC/HORA TRANS.  COD. AUT."
        EscribeArchivoCierre(nombrearc4, linea)
        linea = "---------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc4, linea)

        lacanti = 0
        eltot = 0
        lacantitot = 0
        eltottot = 0
        nombreEmisor = ""
        For Each reg0200 In res0200

            Dim res0210 = From y In dt.GetData()
                          Where y.TipoMensaje = "0210" And Trim(y.Terminal) = Trim(reg0200.Terminal) And y.NroHost = reg0200.NroHost And y.NroTrace = reg0200.NroTrace
                          Select y

            Dim res0400 = From x In dt.GetData()
                          Where x.TipoMensaje = "0400" And Trim(x.Terminal) = Trim(reg0200.Terminal) And x.NroHost = reg0200.NroHost And x.NroTrace = reg0200.NroTrace
                          Select x

            If res0210.Count > 0 AndAlso res0210(0).CodRespuesta = 0 AndAlso res0400.Count = 0 Then
                hayalgo = True
                Dim reg = res0210(0)
                If nombreEmisor = "" Then nombreEmisor = reg.NombreEmisor


                If nombreEmisor <> reg.NombreEmisor Then

                    linea = nombreEmisor + " "
                    linea = linea + Format(lacanti, "###,##0").PadLeft(7) + " "
                    linea = linea + Format(eltot, "##,###,##0.00").PadLeft(13)
                    EscribeArchivoCierre(nombrearc2, linea)

                    linea = nombreEmisor + " "
                    linea = linea + Format(lacanti, "###,##0").PadLeft(7) + "    "
                    linea = linea + Format(eltot, "##,###,##0.00").PadLeft(13)
                    EscribeArchivoCierre(nombrearc3, linea)

                    lacanti = 0
                    eltot = 0
                    linea = "  "
                    EscribeArchivoCierre(nombrearc2, linea)
                    linea = "  "
                    EscribeArchivoCierre(nombrearc2, linea)
                    nombreEmisor = reg.NombreEmisor
                End If
                linea = reg.Primerso6 + "XXX" + reg.Ultimos4 + "  "                    '--- tarjeta
                linea = linea + "XXXX  "                                               '--- vencimiento
                If reg.ModoIngreso = 1 Then
                    linea = linea + "M  "                                              '--- autom/manual
                ElseIf reg.ModoIngreso = 0 Then
                    linea = linea + "A  "                                              '--- autom/manual
                ElseIf reg.ModoIngreso = 5 Then
                    linea = linea + "C  "                                              '--- autom/manual
                Else
                    linea = linea + reg.ModoIngreso.ToString + "  "                    '--- autom/manual
                End If

                If reg.TipoOperacion.Trim = "0" Then                                   '--- operacion 0=venta
                    linea = linea + "COMPRA".PadRight(10)
                ElseIf reg.TipoOperacion.Trim = "1" Then
                    linea = linea + "DEVOLUCION"
                ElseIf reg.TipoOperacion.Trim = "2" Then
                    linea = linea + "AN. COMPRA"
                ElseIf reg.TipoOperacion.Trim = "3" Then
                    linea = linea + "AN. DEV.".PadRight(10)
                End If
                If reg.NroHost = TjComun.TipoHost.POSNET Or reg.NroHost = TjComun.TipoHost.Posnet_homolog Then
                    linea = linea + (reg.Importe + reg.Cashback).ToString.PadLeft(13) + "   "               '--- Importe
                Else
                    linea = linea + reg.Importe.ToString.PadLeft(13) + "   "               '--- Importe
                End If
                'linea = linea + reg.Importe.ToString.PadLeft(13) + "   "               '--- Importe

                linea = linea + reg.Pinguino.ToString("00") + "   "                    '--- Pinguino
                linea = linea + reg.NroCajaPropio.ToString("00") + "   "               '--- caja
                linea = linea + reg.Cajera.ToString.PadLeft(4) + "   "                 '--- codigo cajera
                linea = linea + reg.TicketPosnet.ToString.PadLeft(4) + "   "           '--- Nro de cupon
                linea = linea + reg.nrolote.ToString.PadLeft(3) + "  "                 '--- Nro de lote
                linea = linea + Format(fecha_cierre, "dd/MM/yy") + " "                 '--- Fecha Cierre
                linea = linea + fecha_cierre.ToString("HH:mm") + "    "                '--- Hora cierre
                linea = linea + CInt(reg.Cuotas).ToString("00") + "    "               '--- cuotas
                linea = linea + Format(reg.FechaHora, "dd/MM/yy") + "  "               '--- Fecha Transaccion
                linea = linea + Format(reg.FechaHora, "HH:mm") + "    "                '--- Hora Transaccion
                linea = linea + reg.CodAutorizacion.PadLeft(6)                         '--- Codigo de autorizacion (porque una vez hubo 2 mov con mismo codigo y se cobro 1)
                EscribeArchivoCierre(nombrearc2, linea)

                lacanti = lacanti + 1
                lacantitot = lacantitot + 1


                Try
                    For ii = 1 To 99
                        If _Fecha(ii) = Format(reg.FechaHora, "dd/MM/yy") And _Emisor(ii) = reg.NombreEmisor Then
                            _Cantidad(ii) = _Cantidad(ii) + 1
                            If reg.NroHost = TjComun.TipoHost.POSNET Or reg.NroHost = TjComun.TipoHost.Posnet_homolog Then
                                If reg.TipoOperacion.Trim = "0" Then                                    '--- operacion 0=venta
                                    _Importe(ii) = _Importe(ii) + (reg.Importe + reg.Cashback)        '--- Importe
                                ElseIf reg.TipoOperacion.Trim = "1" Then                                ' 1 = devolucion
                                    _Importe(ii) = _Importe(ii) - (reg.Importe + reg.Cashback)        '--- Importe
                                ElseIf reg.TipoOperacion.Trim = "2" Then                                ' 2 = anulacion compra
                                    _Importe(ii) = _Importe(ii) - (reg.Importe + reg.Cashback)        '--- Importe
                                ElseIf reg.TipoOperacion.Trim = "3" Then                                ' 3 = anulacion devolucion
                                    _Importe(ii) = _Importe(ii) + (reg.Importe + reg.Cashback)        '--- Importe
                                End If
                            Else
                                If reg.TipoOperacion.Trim = "0" Then                                    '--- operacion 0=venta
                                    _Importe(ii) = _Importe(ii) + reg.Importe                         '--- Importe
                                ElseIf reg.TipoOperacion.Trim = "1" Then                                ' 1 = devolucion
                                    _Importe(ii) = _Importe(ii) - reg.Importe                         '--- Importe
                                ElseIf reg.TipoOperacion.Trim = "2" Then                                ' 2 = anulacion compra
                                    _Importe(ii) = _Importe(ii) - reg.Importe                         '--- Importe
                                ElseIf reg.TipoOperacion.Trim = "3" Then                                ' 3 = anulacion devolucion
                                    _Importe(ii) = _Importe(ii) + reg.Importe                         '--- Importe
                                End If
                            End If
                            Exit For
                        Else
                            If _Fecha(ii) = "" Then
                                _Fecha(ii) = Format(reg.FechaHora, "dd/MM/yy")
                                _Emisor(ii) = reg.NombreEmisor
                                _Cantidad(ii) = 1
                                If reg.NroHost = TjComun.TipoHost.POSNET Or reg.NroHost = TjComun.TipoHost.Posnet_homolog Then
                                    If reg.TipoOperacion.Trim = "0" Then                                    '--- operacion 0=venta
                                        _Importe(ii) = _Importe(ii) + (reg.Importe + reg.Cashback)        '--- Importe
                                    ElseIf reg.TipoOperacion.Trim = "1" Then                                ' 1 = devolucion
                                        _Importe(ii) = _Importe(ii) - (reg.Importe + reg.Cashback)        '--- Importe
                                    ElseIf reg.TipoOperacion.Trim = "2" Then                                ' 2 = anulacion compra
                                        _Importe(ii) = _Importe(ii) - (reg.Importe + reg.Cashback)        '--- Importe
                                    ElseIf reg.TipoOperacion.Trim = "3" Then                                ' 3 = anulacion devolucion
                                        _Importe(ii) = _Importe(ii) + (reg.Importe + reg.Cashback)        '--- Importe
                                    End If
                                Else
                                    If reg.TipoOperacion.Trim = "0" Then                                    '--- operacion 0=venta
                                        _Importe(ii) = _Importe(ii) + reg.Importe                         '--- Importe
                                    ElseIf reg.TipoOperacion.Trim = "1" Then                                ' 1 = devolucion
                                        _Importe(ii) = _Importe(ii) - reg.Importe                         '--- Importe
                                    ElseIf reg.TipoOperacion.Trim = "2" Then                                ' 2 = anulacion compra
                                        _Importe(ii) = _Importe(ii) - reg.Importe                         '--- Importe
                                    ElseIf reg.TipoOperacion.Trim = "3" Then                                ' 3 = anulacion devolucion
                                        _Importe(ii) = _Importe(ii) + reg.Importe                         '--- Importe
                                    End If
                                End If
                                Exit For
                            End If
                        End If
                    Next
                Catch

                End Try



                If reg.NroHost = 1 Or reg.NroHost = 5 Then
                    If reg.TipoOperacion.Trim = "0" Then                                    '--- operacion 0=venta
                        eltot += (reg.Importe + reg.Cashback)
                        eltottot += (reg.Importe + reg.Cashback)
                    ElseIf reg.TipoOperacion.Trim = "1" Then                                ' 1 = devolucion
                        eltot -= (reg.Importe + reg.Cashback)
                        eltottot -= (reg.Importe + reg.Cashback)
                    ElseIf reg.TipoOperacion.Trim = "2" Then                                ' 2 = anulacion compra
                        eltot -= (reg.Importe + reg.Cashback)
                        eltottot -= (reg.Importe + reg.Cashback)
                    ElseIf reg.TipoOperacion.Trim = "3" Then                                ' 3 = anulacion devolucion
                        eltot += (reg.Importe + reg.Cashback)
                        eltottot += (reg.Importe + reg.Cashback)
                    End If

                Else

                    If reg.TipoOperacion.Trim = "0" Then                                    '--- operacion 0=venta
                        eltot += reg.Importe
                        eltottot += reg.Importe
                    ElseIf reg.TipoOperacion.Trim = "1" Then                                ' 1 = devolucion
                        eltot -= reg.Importe
                        eltottot -= reg.Importe
                    ElseIf reg.TipoOperacion.Trim = "2" Then                                ' 2 = anulacion compra
                        eltot -= reg.Importe
                        eltottot -= reg.Importe
                    ElseIf reg.TipoOperacion.Trim = "3" Then                                ' 3 = anulacion devolucion
                        eltot += reg.Importe
                        eltottot += reg.Importe
                    End If
                End If

            ElseIf res0400.Count > 0 Then
                Dim reg = res0400(0)
                '---- IMPRIME REVERSOS  ------------
                linea = reg.Primerso6 + "XXX" + reg.Ultimos4 + "  "                    '--- tarjeta
                linea = linea + "XXXX  "                                               '--- vencimiento
                If reg.ModoIngreso = 1 Then
                    linea = linea + "M  "                                              '--- autom/manual
                ElseIf reg.ModoIngreso = 0 Then
                    linea = linea + "A  "                                              '--- autom/manual
                ElseIf reg.ModoIngreso = 5 Then
                    linea = linea + "C  "                                              '--- autom/manual
                Else
                    linea = linea + reg.ModoIngreso.ToString + "  "                    '--- autom/manual
                End If

                If reg.TipoOperacion.Trim = "0" Then                                   '--- operacion 0=venta
                    linea = linea + "COMPRA".PadRight(10)
                ElseIf reg.TipoOperacion.Trim = "1" Then
                    linea = linea + "DEVOLUCION"
                ElseIf reg.TipoOperacion.Trim = "2" Then
                    linea = linea + "AN. COMPRA"
                ElseIf reg.TipoOperacion.Trim = "3" Then
                    linea = linea + "AN. DEV.".PadRight(10)
                End If

                If reg.NroHost = TjComun.TipoHost.POSNET Or reg.NroHost = TjComun.TipoHost.Posnet_homolog Then
                    linea = linea + (reg.Importe + reg.Cashback).ToString.PadLeft(13) + "   "               '--- Importe
                Else
                    linea = linea + reg.Importe.ToString.PadLeft(13) + "   "               '--- Importe
                End If

                'linea = linea + reg.Importe.ToString.PadLeft(13) + "   "               '--- Importe
                linea = linea + reg.Pinguino.ToString("00") + "   "                    '--- Pinguino
                linea = linea + reg.NroCajaPropio.ToString("00") + "   "               '--- caja
                linea = linea + reg.Cajera.ToString.PadLeft(4) + "   "                 '--- codigo cajera
                linea = linea + reg.TicketPosnet.ToString.PadLeft(4) + "   "           '--- Nro de cupon
                linea = linea + reg.nrolote.ToString.PadLeft(3) + "  "                 '--- Nro de lote
                linea = linea + Format(fecha_cierre, "dd/MM/yy") + " "                 '--- Fecha Cierre
                linea = linea + fecha_cierre.ToString("HH:mm") + "    "                '--- Hora cierre
                linea = linea + CInt(reg.Cuotas).ToString("00") + "    "               '--- cuotas
                linea = linea + Format(reg.FechaHora, "dd/MM/yy") + "  "               '--- Fecha Transaccion
                linea = linea + Format(reg.FechaHora, "HH:mm") + "    "                '--- Hora Transaccion
                linea = linea + reg.CodAutorizacion.PadLeft(6)                         '--- Codigo de autorizacion (porque una vez hubo 2 mov con mismo codigo y se cobro 1)
                EscribeArchivoCierre(nombrearc4, linea)
            End If
        Next
        linea = nombreEmisor + " "
        linea = linea + Format(lacanti, "###,##0").PadLeft(7) + " "
        linea = linea + Format(eltot, "##,###,##0.00").PadLeft(13)
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "----------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "TOTAL GENERAL".PadRight(26)
        linea = linea + lacantitot.ToString.PadLeft(7) + " "
        linea = linea + eltottot.ToString("##,###,##0.00").PadLeft(13)
        EscribeArchivoCierre(nombrearc2, linea)

        linea = nombreEmisor + " "
        linea = linea + Format(lacanti, "###,##0").PadLeft(7) + "    "
        linea = linea + Format(eltot, "##,###,##0.00").PadLeft(13)
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "---------------------------------------------------"
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "TOTAL GENERAL".PadRight(26)
        linea = linea + Format(lacantitot, "###,##0").PadLeft(7) + "    "
        linea = linea + Format(eltottot, "##,###,##0.00").PadLeft(13)
        EscribeArchivoCierre(nombrearc3, linea)





        Try
            EscribeArchivoCierre(nombrearc3, " ")
            EscribeArchivoCierre(nombrearc3, " ")
            EscribeArchivoCierre(nombrearc3, " ")
            EscribeArchivoCierre(nombrearc3, " ")
            EscribeArchivoCierre(nombrearc3, " ")
            EscribeArchivoCierre(nombrearc3, " ")
            EscribeArchivoCierre(nombrearc3, " ")
            linea = "----------------------------------------------------------------------"
            EscribeArchivoCierre(nombrearc3, linea)
            linea = "FECHA       EMISOR                CANT.TRANSACCIONES         TOTAL"
            EscribeArchivoCierre(nombrearc3, linea)
            linea = "----------------------------------------------------------------------"
            EscribeArchivoCierre(nombrearc3, linea)

            cantiarray = 0
            imporarray = 0
            lafecha = ""
            For ii = 1 To 99
                If _Fecha(ii) = "" Then
                    Exit For
                Else
                    If lafecha = "" Then lafecha = _Fecha(ii)
                    If lafecha <> _Fecha(ii) Then
                        linea = "----------------------------------------------------------------------"
                        EscribeArchivoCierre(nombrearc3, linea)
                        linea = "                                         "
                        linea = linea + Format(cantiarray, "###,##0").PadLeft(7) + "    "
                        linea = linea + Format(imporarray, "###,###,##0.00").PadLeft(14)
                        EscribeArchivoCierre(nombrearc3, linea)

                        EscribeArchivoCierre(nombrearc3, " ")
                        cantiarray = 0
                        imporarray = 0
                    End If
                    linea = _Fecha(ii) + "    "
                    linea = linea + _Emisor(ii) + "    "
                    linea = linea + Format(_Cantidad(ii), "###,##0").PadLeft(7) + "    "
                    linea = linea + Format(_Importe(ii), "###,###,##0.00").PadLeft(14)
                    EscribeArchivoCierre(nombrearc3, linea)
                    cantiarray = cantiarray + _Cantidad(ii)
                    imporarray = imporarray + _Importe(ii)
                End If
            Next
            linea = "----------------------------------------------------------------------"
            EscribeArchivoCierre(nombrearc3, linea)
            linea = "                                         "
            linea = linea + Format(cantiarray, "###,##0").PadLeft(7) + "    "
            linea = linea + Format(imporarray, "###,###,##0.00").PadLeft(14)
            EscribeArchivoCierre(nombrearc3, linea)
        Catch

        End Try





        Try
            If IO.File.Exists(Configuracion.DirTarjetas + "\Cierres\TarjPendientes_" & fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt") Then
                'adjuntos = adjuntos + Configuracion.DirTarjetas + "\Cierres\TarjPendientes_" & fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt;"
                adjuntos = adjuntos + "TarjPendientes_" & fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt;"
            End If
        Catch ex As Exception
            Logger.Error(String.Format("No encontró archivo de pendientes"))
        End Try
        Logger.Info("Fin generación reporte tarjetas global ...")
        If hayalgo Then
            Enviar_Achivos_Mail(0, adjuntos, "CIERRES TARJETAS Fecha: " + fecha_cierre.ToString("dd/MM/yyyy"))
        Else
            Logger.Info(String.Format("Archivo Vacío, no se envió mail global"))
        End If
    End Sub


    Public Sub ListadosReportesPEITarjetasPorPing(ping As Integer, fecha_cierre As Date)
        Dim hayalgo As Boolean = False
        Dim adjuntos As String
        Dim linea, nombreEmisor As String
        Dim lacanti, lacantitot, caja As Integer
        Dim eltot, eltottot, totalxcaja As Decimal
        Dim nombrearc2, nombrearc3 As String
        Dim dt As New DatosTjTableAdapters.MovimientosPEITableAdapter
        Logger.Info(String.Format("Generando reporte PEI tarjetas P.{0} ...", ping))
        Dim res0200 = From c In dt.GetData()
                      Where c.CodRespuesta = "00" And c.Pinguino = ping
                      Order By c.Emisor, c.Caja, c.FechaHora
                      Select c


        If res0200.Count = 0 Then
            Logger.Info("No hay movimientos para informar. " & fecha_cierre.ToString("dd/MM/yy") & " Pinguino:" & ping.ToString)
            Exit Sub
        End If

        nombrearc2 = "CiePEI" & ping.ToString("00") & "_DET_" & fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt"
        adjuntos = nombrearc2 & ";"

        linea = "Reporte Detalle Transacciones PINGUINO " & ping.ToString("00") & "  -   Fecha Generacion: " + Format(fecha_cierre, "dd/MM/yy") + "   " + fecha_cierre.ToString("HH:mm")
        EscribeArchivoCierrePEI(nombrearc2, linea)
        linea = "----------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierrePEI(nombrearc2, linea)
        linea = "TARJETA        OPERACION                IMPORTE  PING CAJA CAJERA  CUPON    FEC/HORA TRANS.  ID OPERACION   ID OPERACION ORIG"
        EscribeArchivoCierrePEI(nombrearc2, linea)
        linea = "----------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierrePEI(nombrearc2, linea)

        nombrearc3 = "CiePEI" & ping.ToString("00") & "_TOT_" & fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt"
        adjuntos = adjuntos & nombrearc3 & ";"

        linea = "Reporte Totales Transacciones PINGUINO " & ping.ToString("00") & "  -   Fecha Generacion: " + Format(fecha_cierre, "dd/MM/yy") + "   " + fecha_cierre.ToString("HH:mm")
        EscribeArchivoCierrePEI(nombrearc3, linea)
        linea = "---------------------------------------------------"
        EscribeArchivoCierrePEI(nombrearc3, linea)
        linea = "EMISOR            CANT.TRANSACCIONES         TOTAL"
        EscribeArchivoCierrePEI(nombrearc3, linea)
        linea = "---------------------------------------------------"
        EscribeArchivoCierrePEI(nombrearc3, linea)

        lacanti = 0
        eltot = 0
        lacantitot = 0
        eltottot = 0
        nombreEmisor = ""
        For Each reg0200 In res0200

            hayalgo = True

            If nombreEmisor = "" Then nombreEmisor = reg0200.Emisor
            If caja = 0 Then caja = reg0200.Caja

            If caja <> reg0200.Caja Then
                linea = "==========".PadLeft(47)
                EscribeArchivoCierrePEI(nombrearc2, linea)
                linea = $"Total caja {caja.ToString("00").PadRight(26)}{totalxcaja.ToString("###,##0.00").PadLeft(10)}"
                'linea = String.Format("Total caja {0}", caja.ToString("00")).PadRight(37) + totalxcaja.ToString("###,##0.00").PadLeft(10)
                EscribeArchivoCierrePEI(nombrearc2, linea)
                linea = " "
                EscribeArchivoCierrePEI(nombrearc2, linea)
                totalxcaja = 0
                caja = reg0200.Caja
            End If

            If nombreEmisor <> reg0200.Emisor Then

                linea = $"{nombreEmisor.Trim.PadRight(25)}{Format(lacanti, "###,##0").PadLeft(7)}{Format(eltot, "##,###,##0.00").PadLeft(15)}"
                EscribeArchivoCierrePEI(nombrearc2, linea)

                linea = $"{nombreEmisor.Trim.PadRight(25)}{Format(lacanti, "###,##0").PadLeft(7)}{Format(eltot, "##,###,##0.00").PadLeft(18)}"
                EscribeArchivoCierrePEI(nombrearc3, linea)
                lacanti = 0
                eltot = 0

                linea = "  "
                EscribeArchivoCierrePEI(nombrearc2, linea)
                linea = "  "
                EscribeArchivoCierrePEI(nombrearc2, linea)
                nombreEmisor = reg0200.Emisor
            End If

            linea = $"{reg0200.Primeros6}XXX{reg0200.Ultimos4}  {reg0200.Operacion.Trim.PadRight(20)}{reg0200.Importe.ToString.PadLeft(12)}   {reg0200.Pinguino.ToString("00")}   {reg0200.Caja.ToString("00")}   {reg0200.Cajera.ToString.PadLeft(4)}  {reg0200.Cupon.ToString.PadLeft(6)}    {Format(reg0200.FechaHora, "dd/MM/yy")}  {Format(reg0200.FechaHora, "HHmm")}   {reg0200.IdOperacion.Trim.PadLeft(12)}   {reg0200.IdOperacionOrigen.Trim.PadLeft(12)}"
            EscribeArchivoCierrePEI(nombrearc2, linea)

            lacanti = lacanti + 1
            lacantitot = lacantitot + 1

            If reg0200.Operacion.Trim = lib_PEI.Concepto.COMPRA_DE_BIENES.ToString Then                                   '--- operacion 0=venta
                eltot += reg0200.Importe
                eltottot += reg0200.Importe
                totalxcaja += reg0200.Importe
            ElseIf reg0200.Operacion.Trim = lib_PEI.Concepto.DEVOLUCION.ToString Then                               ' 1 = devolucion
                eltot -= reg0200.Importe
                eltottot -= reg0200.Importe
                totalxcaja -= reg0200.Importe
            ElseIf reg0200.Operacion.Trim = lib_PEI.Concepto.EXTRACCION.ToString Then                               ' 2 = anulacion compra
                eltot += reg0200.Importe
                eltottot += reg0200.Importe
                totalxcaja += reg0200.Importe
            End If

        Next

        linea = "==========".PadLeft(47)
        EscribeArchivoCierrePEI(nombrearc2, linea)
        linea = $"Total caja {caja.ToString("00").PadRight(26)}{totalxcaja.ToString("###,##0.00").PadLeft(10)}"
        'linea = String.Format("Total caja {0}", caja.ToString("00")).PadRight(37) + totalxcaja.ToString("###,##0.00").PadLeft(10)
        EscribeArchivoCierrePEI(nombrearc2, linea)
        linea = " "
        EscribeArchivoCierrePEI(nombrearc2, linea)

        linea = $"{nombreEmisor.Trim.PadRight(25)}{Format(lacanti, "###,##0").PadLeft(7)}{Format(eltot, "##,###,##0.00").PadLeft(15)}"
        EscribeArchivoCierrePEI(nombrearc2, linea)
        linea = "---------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierrePEI(nombrearc2, linea)
        linea = $"TOTAL GENERAL            {lacantitot.ToString("###,##0").PadLeft(7)}{eltottot.ToString("##,###,##0.00").PadLeft(15)}"
        EscribeArchivoCierrePEI(nombrearc2, linea)


        linea = $"{nombreEmisor.Trim.PadRight(25)}{Format(lacanti, "###,##0").PadLeft(7)}{Format(eltot, "##,###,##0.00").PadLeft(18)}"
        EscribeArchivoCierrePEI(nombrearc3, linea)

        linea = "---------------------------------------------------"
        EscribeArchivoCierrePEI(nombrearc3, linea)

        linea = $"TOTAL GENERAL            {Format(lacantitot, "###,##0").PadLeft(7)}{Format(eltottot, "##,###,##0.00").PadLeft(18)}"
        EscribeArchivoCierrePEI(nombrearc3, linea)
        Logger.Info(String.Format("Fin generación reporte PEI tarjetas P.{0} ...", ping))
        If hayalgo Then
            Enviar_Achivos_MailPEI(ping, adjuntos, "REPORTES PEI DIARIOS Ping. " & ping & " Fecha: " + fecha_cierre.ToString("dd/MM/yyyy"))
        Else
            Logger.Info(String.Format("Archivo Vacío PEI, no se envió mail a Ping. {0}", ping))
        End If
    End Sub

    Public Sub ListadosReportesTarjetasPorPing(ping As Integer, fecha_cierre As Date)
        Dim hayalgo As Boolean = False
        Dim adjuntos As String
        Dim linea, nombreEmisor, lafecha As String
        Dim lacanti, lacantitot, caja As Integer
        Dim eltot, eltottot, totalxcaja As Decimal
        Dim nombrearc2, nombrearc3 As String
        Dim cantiarray As Integer
        Dim imporarray As Decimal

        Dim ii As Integer
        Dim _Fecha(99) As String
        Dim _Emisor(99) As String
        Dim _Cantidad(99) As Integer
        Dim _Importe(99) As Decimal

        For ii = 1 To 99
            _Fecha(ii) = ""
            _Emisor(ii) = ""
            _Cantidad(ii) = 0
            _Importe(ii) = 0
        Next


        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Logger.Info(String.Format("Generando reporte tarjetas P.{0} ...", ping))



        Dim res0200 = From c In dt.GetData()
                      Where c.TipoMensaje = "0200" And c.hizocierre = True And c.Pinguino = ping
                      Order By c.NombreEmisor, c.NroCajaPropio, c.FechaHora
                      Select c
        nombrearc2 = "Cie" & ping.ToString("00") & "_DET_" & fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt"
        adjuntos = nombrearc2 & ";"

        linea = "Reporte Detalle Transacciones PINGUINO " & ping.ToString("00") & "  -   Fecha Generacion: " + Format(fecha_cierre, "dd/MM/yy") + "   " + fecha_cierre.ToString("HH:mm")
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "----------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "TARJETA        VENC A/M OPERACION       IMPORTE  PING CAJA CAJERA TICKET LOTE  FEC/HORA CIERRE CUOTAS  FEC/HORA TRANS.  COD. AUT."
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "----------------------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)

        nombrearc3 = "Cie" & ping.ToString("00") & "_TOT_" & fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt"
        adjuntos = adjuntos & nombrearc3 & ";"

        linea = "Reporte Totales Transacciones PINGUINO " & ping.ToString("00") & "  -   Fecha Generacion: " + Format(fecha_cierre, "dd/MM/yy") + "   " + fecha_cierre.ToString("HH:mm")
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "---------------------------------------------------"
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "EMISOR            CANT.TRANSACCIONES         TOTAL"
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "---------------------------------------------------"
        EscribeArchivoCierre(nombrearc3, linea)

        lacanti = 0
        eltot = 0
        lacantitot = 0
        eltottot = 0
        nombreEmisor = ""
        For Each reg0200 In res0200
            Dim res0210 = From y In dt.GetData()
                          Where y.TipoMensaje = "0210" And Trim(y.Terminal) = Trim(reg0200.Terminal) And y.NroHost = reg0200.NroHost And y.NroTrace = reg0200.NroTrace
                          Select y

            Dim res0400 = From x In dt.GetData()
                          Where x.TipoMensaje = "0400" And Trim(x.Terminal) = Trim(reg0200.Terminal) And x.NroHost = reg0200.NroHost And x.NroTrace = reg0200.NroTrace
                          Select x

            If res0210.Count > 0 AndAlso res0210(0).CodRespuesta = 0 AndAlso res0400.Count = 0 Then
                hayalgo = True
                Dim reg = res0210(0)
                If nombreEmisor = "" Then nombreEmisor = reg.NombreEmisor
                If caja = 0 Then caja = reg.NroCajaPropio


                If caja <> reg.NroCajaPropio Then
                    linea = "==========".PadLeft(46)
                    EscribeArchivoCierre(nombrearc2, linea)
                    linea = String.Format("Total caja {0}", caja.ToString("00")).PadRight(37) + totalxcaja.ToString("###,##0.00").PadLeft(10)
                    EscribeArchivoCierre(nombrearc2, linea)
                    linea = " "
                    EscribeArchivoCierre(nombrearc2, linea)
                    totalxcaja = 0
                    caja = reg.NroCajaPropio
                End If

                If nombreEmisor <> reg.NombreEmisor Then

                    linea = nombreEmisor + " "
                    linea = linea + Format(lacanti, "###,##0").PadLeft(7) + " "
                    linea = linea + Format(eltot, "##,###,##0.00").PadLeft(13)
                    EscribeArchivoCierre(nombrearc2, linea)

                    linea = nombreEmisor + " "
                    linea = linea + Format(lacanti, "###,##0").PadLeft(7) + "    "
                    linea = linea + Format(eltot, "##,###,##0.00").PadLeft(13)
                    EscribeArchivoCierre(nombrearc3, linea)
                    lacanti = 0
                    eltot = 0

                    linea = "  "
                    EscribeArchivoCierre(nombrearc2, linea)
                    linea = "  "
                    EscribeArchivoCierre(nombrearc2, linea)
                    nombreEmisor = reg.NombreEmisor
                End If

                linea = reg.Primerso6 + "XXX" + reg.Ultimos4 + "  "                    '--- tarjeta
                linea = linea + "XXXX  "                                               '--- vencimiento
                If reg.ModoIngreso = 1 Then
                    linea = linea + "M  "                                              '--- autom/manual
                ElseIf reg.ModoIngreso = 0 Then
                    linea = linea + "A  "                                              '--- autom/manual
                ElseIf reg.ModoIngreso = 5 Then
                    linea = linea + "C  "                                              '--- autom/manual
                Else
                    linea = linea + reg.ModoIngreso.ToString + "  "                    '--- autom/manual
                End If
                If reg.TipoOperacion.Trim = "0" Then                                   '--- operacion 0=venta
                    linea = linea + "COMPRA".PadRight(10)
                ElseIf reg.TipoOperacion.Trim = "1" Then
                    linea = linea + "DEVOLUCION"
                ElseIf reg.TipoOperacion.Trim = "2" Then
                    linea = linea + "AN. COMPRA"
                ElseIf reg.TipoOperacion.Trim = "3" Then
                    linea = linea + "AN. DEV.".PadRight(10)
                End If

                If reg.NroHost = TjComun.TipoHost.POSNET Or reg.NroHost = TjComun.TipoHost.Posnet_homolog Then
                    linea = linea + (reg.Importe + reg.Cashback).ToString.PadLeft(13) + "   "               '--- Importe
                Else
                    linea = linea + reg.Importe.ToString.PadLeft(13) + "   "               '--- Importe
                End If

                'linea = linea + reg.Importe.ToString.PadLeft(13) + "   "               '--- Importe
                linea = linea + reg.Pinguino.ToString("00") + "   "                    '--- Pinguino
                linea = linea + reg.NroCajaPropio.ToString("00") + "   "               '--- caja
                linea = linea + reg.Cajera.ToString.PadLeft(4) + "   "                 '--- codigo cajera
                linea = linea + reg.TicketPosnet.ToString.PadLeft(4) + "   "           '--- Nro de cupon
                linea = linea + reg.nrolote.ToString.PadLeft(3) + "  "                 '--- Nro de lote
                linea = linea + Format(fecha_cierre, "dd/MM/yy") + " "                 '--- Fecha Cierre
                linea = linea + fecha_cierre.ToString("HH:mm") + "    "                '--- Hora cierre
                linea = linea + CInt(reg.Cuotas).ToString("00") + "    "               '--- cuotas
                linea = linea + Format(reg.FechaHora, "dd/MM/yy") + "  "               '--- Fecha Transaccion
                linea = linea + Format(reg.FechaHora, "HH:mm") + "    "                 '--- Hora Transaccion
                linea = linea + reg.CodAutorizacion.PadLeft(6)                         '--- Codigo de autorizacion (porque una vez hubo 2 mov con mismo codigo y se cobro 1)

                EscribeArchivoCierre(nombrearc2, linea)

                lacanti = lacanti + 1
                lacantitot = lacantitot + 1



                Try
                    For ii = 1 To 99
                        If _Fecha(ii) = Format(reg.FechaHora, "dd/MM/yy") And _Emisor(ii) = reg.NombreEmisor Then
                            _Cantidad(ii) = _Cantidad(ii) + 1
                            If reg.NroHost = TjComun.TipoHost.POSNET Or reg.NroHost = TjComun.TipoHost.Posnet_homolog Then
                                If reg.TipoOperacion.Trim = "0" Then                                    '--- operacion 0=venta
                                    _Importe(ii) = _Importe(ii) + (reg.Importe + reg.Cashback)        '--- Importe
                                ElseIf reg.TipoOperacion.Trim = "1" Then                                ' 1 = devolucion
                                    _Importe(ii) = _Importe(ii) - (reg.Importe + reg.Cashback)        '--- Importe  
                                ElseIf reg.TipoOperacion.Trim = "2" Then                                ' 2 = anulacion compra
                                    _Importe(ii) = _Importe(ii) - (reg.Importe + reg.Cashback)        '--- Importe  
                                ElseIf reg.TipoOperacion.Trim = "3" Then                                ' 3 = anulacion devolucion
                                    _Importe(ii) = _Importe(ii) + (reg.Importe + reg.Cashback)        '--- Importe
                                End If
                            Else
                                If reg.TipoOperacion.Trim = "0" Then                                   '--- operacion 0=venta
                                    _Importe(ii) = _Importe(ii) + reg.Importe                         '--- Importe
                                ElseIf reg.TipoOperacion.Trim = "1" Then                               ' 1 = devolucion
                                    _Importe(ii) = _Importe(ii) - reg.Importe                         '--- Importe
                                ElseIf reg.TipoOperacion.Trim = "2" Then                               ' 2 = anulacion compra
                                    _Importe(ii) = _Importe(ii) - reg.Importe                         '--- Importe
                                ElseIf reg.TipoOperacion.Trim = "3" Then                               ' 3 = anulacion devolucion
                                    _Importe(ii) = _Importe(ii) + reg.Importe                         '--- Importe
                                End If
                            End If
                            Exit For
                        Else
                            If _Fecha(ii) = "" Then
                                _Fecha(ii) = Format(reg.FechaHora, "dd/MM/yy")
                                _Emisor(ii) = reg.NombreEmisor
                                _Cantidad(ii) = 1
                                If reg.NroHost = TjComun.TipoHost.POSNET Or reg.NroHost = TjComun.TipoHost.Posnet_homolog Then
                                    If reg.TipoOperacion.Trim = "0" Then                                    '--- operacion 0=venta
                                        _Importe(ii) = _Importe(ii) + (reg.Importe + reg.Cashback)        '--- Importe
                                    ElseIf reg.TipoOperacion.Trim = "1" Then                                ' 1 = devolucion
                                        _Importe(ii) = _Importe(ii) - (reg.Importe + reg.Cashback)        '--- Importe  
                                    ElseIf reg.TipoOperacion.Trim = "2" Then                                ' 2 = anulacion compra
                                        _Importe(ii) = _Importe(ii) - (reg.Importe + reg.Cashback)        '--- Importe  
                                    ElseIf reg.TipoOperacion.Trim = "3" Then                                ' 3 = anulacion devolucion
                                        _Importe(ii) = _Importe(ii) + (reg.Importe + reg.Cashback)        '--- Importe
                                    End If
                                Else
                                    If reg.TipoOperacion.Trim = "0" Then                                   '--- operacion 0=venta
                                        _Importe(ii) = _Importe(ii) + reg.Importe                         '--- Importe
                                    ElseIf reg.TipoOperacion.Trim = "1" Then                               ' 1 = devolucion
                                        _Importe(ii) = _Importe(ii) - reg.Importe                         '--- Importe
                                    ElseIf reg.TipoOperacion.Trim = "2" Then                               ' 2 = anulacion compra
                                        _Importe(ii) = _Importe(ii) - reg.Importe                         '--- Importe
                                    ElseIf reg.TipoOperacion.Trim = "3" Then                               ' 3 = anulacion devolucion
                                        _Importe(ii) = _Importe(ii) + reg.Importe                         '--- Importe
                                    End If
                                End If
                                Exit For
                            End If
                        End If
                    Next
                Catch

                End Try






                If reg.NroHost = 1 Or reg.NroHost = 5 Then
                    If reg.TipoOperacion.Trim = "0" Then                                    '--- operacion 0=venta
                        eltot += (reg.Importe + reg.Cashback)
                        eltottot += (reg.Importe + reg.Cashback)
                        totalxcaja += (reg.Importe + reg.Cashback)
                    ElseIf reg.TipoOperacion.Trim = "1" Then                                ' 1 = devolucion
                        eltot -= (reg.Importe + reg.Cashback)
                        eltottot -= (reg.Importe + reg.Cashback)
                        totalxcaja -= (reg.Importe + reg.Cashback)
                    ElseIf reg.TipoOperacion.Trim = "2" Then                                ' 2 = anulacion compra
                        eltot -= (reg.Importe + reg.Cashback)
                        eltottot -= (reg.Importe + reg.Cashback)
                        totalxcaja -= (reg.Importe + reg.Cashback)
                    ElseIf reg.TipoOperacion.Trim = "3" Then                                ' 3 = anulacion devolucion
                        eltot += (reg.Importe + reg.Cashback)
                        eltottot += (reg.Importe + reg.Cashback)
                        totalxcaja += (reg.Importe + reg.Cashback)
                    End If

                Else


                    If reg.TipoOperacion.Trim = "0" Then                                   '--- operacion 0=venta
                        eltot += reg.Importe
                        eltottot += reg.Importe
                        totalxcaja += reg.Importe
                    ElseIf reg.TipoOperacion.Trim = "1" Then                               ' 1 = devolucion
                        eltot -= reg.Importe
                        eltottot -= reg.Importe
                        totalxcaja -= reg.Importe
                    ElseIf reg.TipoOperacion.Trim = "2" Then                               ' 2 = anulacion compra
                        eltot -= reg.Importe
                        eltottot -= reg.Importe
                        totalxcaja -= reg.Importe
                    ElseIf reg.TipoOperacion.Trim = "3" Then                               ' 3 = anulacion devolucion
                        eltot += reg.Importe
                        eltottot += reg.Importe
                        totalxcaja += reg.Importe
                    End If

                End If


                'If reg.TipoOperacion.Trim = "0" Then                                   '--- operacion 0=venta
                '    eltot += reg.Importe
                '    eltottot += reg.Importe
                '    totalxcaja += reg.Importe
                'ElseIf reg.TipoOperacion.Trim = "1" Then                               ' 1 = devolucion
                '    eltot -= reg.Importe
                '    eltottot -= reg.Importe
                '    totalxcaja -= reg.Importe
                'ElseIf reg.TipoOperacion.Trim = "2" Then                               ' 2 = anulacion compra
                '    eltot -= reg.Importe
                '    eltottot -= reg.Importe
                '    totalxcaja -= reg.Importe
                'ElseIf reg.TipoOperacion.Trim = "3" Then                               ' 3 = anulacion devolucion
                '    eltot += reg.Importe
                '    eltottot += reg.Importe
                '    totalxcaja += reg.Importe
                'End If
            End If
        Next

        linea = "==========".PadLeft(47)
        EscribeArchivoCierre(nombrearc2, linea)
        linea = String.Format("Total caja {0}", caja.ToString("00")).PadRight(37) + totalxcaja.ToString("###,##0.00").PadLeft(10)
        EscribeArchivoCierre(nombrearc2, linea)
        linea = " "
        EscribeArchivoCierre(nombrearc2, linea)

        linea = nombreEmisor + " "
        linea = linea + Format(lacanti, "###,##0").PadLeft(7) + " "
        linea = linea + Format(eltot, "##,###,##0.00").PadLeft(13)
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "----------------------------------------------------------------------------------------------------------------------"
        EscribeArchivoCierre(nombrearc2, linea)
        linea = "TOTAL GENERAL".PadRight(26)
        linea = linea + lacantitot.ToString("###,##0").PadLeft(7) + " "
        linea = linea + eltottot.ToString("##,###,##0.00").PadLeft(13)
        EscribeArchivoCierre(nombrearc2, linea)


        linea = nombreEmisor + " "
        linea = linea + Format(lacanti, "###,##0").PadLeft(7) + "    "
        linea = linea + Format(eltot, "##,###,##0.00").PadLeft(13)
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "---------------------------------------------------"
        EscribeArchivoCierre(nombrearc3, linea)
        linea = "TOTAL GENERAL".PadRight(26)
        linea = linea + Format(lacantitot, "###,##0").PadLeft(7) + "    "
        linea = linea + Format(eltottot, "##,###,##0.00").PadLeft(13)
        EscribeArchivoCierre(nombrearc3, linea)





        Try
            EscribeArchivoCierre(nombrearc3, " ")
            EscribeArchivoCierre(nombrearc3, " ")
            EscribeArchivoCierre(nombrearc3, " ")
            EscribeArchivoCierre(nombrearc3, " ")
            EscribeArchivoCierre(nombrearc3, " ")
            EscribeArchivoCierre(nombrearc3, " ")
            EscribeArchivoCierre(nombrearc3, " ")
            linea = "----------------------------------------------------------------------"
            EscribeArchivoCierre(nombrearc3, linea)
            linea = "FECHA       EMISOR                CANT.TRANSACCIONES         TOTAL"
            EscribeArchivoCierre(nombrearc3, linea)
            linea = "----------------------------------------------------------------------"
            EscribeArchivoCierre(nombrearc3, linea)

            cantiarray = 0
            imporarray = 0
            lafecha = ""
            For ii = 1 To 99
                If _Fecha(ii) = "" Then
                    Exit For
                Else
                    If lafecha = "" Then lafecha = _Fecha(ii)
                    If lafecha <> _Fecha(ii) Then
                        linea = "----------------------------------------------------------------------"
                        EscribeArchivoCierre(nombrearc3, linea)
                        linea = "                                         "
                        linea = linea + Format(cantiarray, "###,##0").PadLeft(7) + "    "
                        linea = linea + Format(imporarray, "###,###,##0.00").PadLeft(14)
                        EscribeArchivoCierre(nombrearc3, linea)

                        EscribeArchivoCierre(nombrearc3, " ")
                        cantiarray = 0
                        imporarray = 0
                    End If
                    linea = _Fecha(ii) + "    "
                    linea = linea + _Emisor(ii) + "    "
                    linea = linea + Format(_Cantidad(ii), "###,##0").PadLeft(7) + "    "
                    linea = linea + Format(_Importe(ii), "###,###,##0.00").PadLeft(14)
                    EscribeArchivoCierre(nombrearc3, linea)
                    cantiarray = cantiarray + _Cantidad(ii)
                    imporarray = imporarray + _Importe(ii)
                End If
            Next
            linea = "----------------------------------------------------------------------"
            EscribeArchivoCierre(nombrearc3, linea)
            linea = "                                         "
            linea = linea + Format(cantiarray, "###,##0").PadLeft(7) + "    "
            linea = linea + Format(imporarray, "###,###,##0.00").PadLeft(14)
            EscribeArchivoCierre(nombrearc3, linea)
        Catch

        End Try






        Logger.Info(String.Format("Fin generación reporte tarjetas P.{0} ...", ping))
        If hayalgo Then
            Enviar_Achivos_Mail(ping, adjuntos, "REPORTES DIARIOS Ping. " & ping & " Fecha: " + fecha_cierre.ToString("dd/MM/yyyy"))
        Else
            Logger.Info(String.Format("Archivo Vacío, no se envió mail a Ping. {0}", ping))
        End If
    End Sub

    Public Sub Enviar_Achivos_Mail(ping As Integer, adjuntos As String, asunto As String)
        Dim ii As Integer
        Dim estaOk As Boolean

        estaOk = False
        For ii = 1 To 5
            Try
                '---- 08/06
                If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                    Exit Sub
                End If

                Logger.Info(String.Format("Enviando mail (" + ii.ToString + ") con reportes asunto {0} ...", asunto))
                Dim mensaje As New Net.Mail.MailMessage
                Dim smtp As New Net.Mail.SmtpClient()
                Dim dt As New DatosTjTableAdapters.MAILSTableAdapter

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
                    estaOk = True
                Else
                    estaOk = True
                End If
            Catch ex As Exception
                estaOk = False
                Logger.Error("No se pudo enviar mail con reportes de ping: " + ping.ToString & vbNewLine & 
                            ex.Message)
            End Try


            If estaOk = False Then
                System.Threading.Thread.Sleep(10000)           '--- 10 segundos 
            Else
                Exit For
            End If
        Next

    End Sub


    Public Sub Enviar_Achivos_MailPEI(ping As Integer, adjuntos As String, asunto As String)
        Try
            If My.Computer.Name = "MARCOS-XP" Or My.Computer.Name = "ROBERTINO-P" Or My.Computer.Name = "NAHUEL-O" Or Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                Exit Sub
            End If
            Logger.Info(String.Format("Enviando mail con reportes PEI asunto {0} ...", asunto))
            Dim mensaje As New Net.Mail.MailMessage
            Dim smtp As New Net.Mail.SmtpClient()
            Dim dt As New DatosTjTableAdapters.MAILSTableAdapter

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
                        att = New Net.Mail.Attachment(Configuracion.DirTarjetas + "\PEI\Cierres\" + arc)
                        mensaje.Attachments.Add(att)
                    End If
                Next

                mensaje.From = New Net.Mail.MailAddress("tarjetas@cormoran.com.ar", "CIERRES TARJETAS")

                mensaje.Subject = asunto

                mensaje.Body = ""

                smtp.Send(mensaje)
                Logger.Info("Mail PEI enviado ...")
            End If
        Catch ex As Exception
            Logger.Error("No se pudo enviar mail con reportes PEI de ping: " + ping.ToString & vbNewLine &
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


    Public Sub EscribeArchivoCierrePEI(nombre As String, ByVal linea As String)
        'GENERAR .POS
        'GENERAR TARJREP
        'GENERAR TARJREP2
        Console.WriteLine(linea)
        Dim archivoSalida, archivoSalidaLocal As FileStream
        Dim leerfile, leerfileLocal As StreamWriter
        Try
            archivoSalida = New FileStream(Configuracion.DirTarjetas + "\PEI\Cierres\" + nombre, FileMode.Append, FileAccess.Write)
            leerfile = New StreamWriter(archivoSalida)
            If linea <> "" Then leerfile.WriteLine(linea)
            leerfile.Close()
        Catch ex As IOException
            Logger.Error(String.Format("No se pudo grabar el archivo {0}\PEI\Cierres\{1}", Configuracion.DirTarjetas, nombre))
        Catch ex1 As Exception
            Logger.Fatal(ex1.Message)
        End Try

        Try
            archivoSalidaLocal = New FileStream(Configuracion.DirLocal + "\CierresPEI\" + nombre, FileMode.Append, FileAccess.Write)
            leerfileLocal = New StreamWriter(archivoSalidaLocal)
            If linea <> "" Then leerfileLocal.WriteLine(linea)
            leerfileLocal.Close()
        Catch ex As IOException
            Logger.Error(String.Format("No se pudo grabar el archivo {0}\CierresPEI\{1}", Configuracion.DirLocal, nombre))
        Catch ex1 As Exception
            Logger.Fatal(ex1.Message)
        End Try

    End Sub
#End Region


    Public Function ImportarMovimientos(archivo As String) As Int16

        Dim arc As FileIO.TextFieldParser
        Dim renglon As String()
        Dim cont As Int16 = 0
        Dim datosDS As New DatosTj
        Dim datostjTA As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim tablalocal As New DatosTj.MovimientosDataTable

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
                    Dim fila = datosDS.Movimientos.NewMovimientosRow
                    fila.Terminal = renglon(0)
                    fila.NroHost = renglon(1)
                    fila.NroTrace = renglon(2)
                    fila.TipoMensaje = renglon(3)
                    'fila.Tarjeta = renglon(4)
                    fila.Primerso6 = renglon(5)
                    fila.Ultimos4 = renglon(6)
                    fila.CodProc = renglon(7)   '  E_ProcCode
                    fila.Importe = CDec(renglon(8)) / 100

                    fila.Cashback = CDec(renglon(9)) / 100
                    fila.FechaHora = renglon(10)
                    fila.FechaHoraLocal = renglon(11)
                    'fila.Vencimiento = renglon(12) 
                    fila.ModoIngreso = renglon(13)
                    fila.IdRed = renglon(14)
                    fila.NroCuenta = renglon(15)
                    'fila.Track2 = renglon(16)
                    fila.RetRefNumber = renglon(17)
                    fila.CodAutorizacion = renglon(18)
                    fila.CodRespuesta = renglon(19)
                    fila.IdComercio = renglon(20)
                    'fila.Track1 = renglon(21)
                    fila.TrackNoLeido = renglon(22)
                    fila.Pinguino = renglon(23)
                    fila.NroCajaPropio = renglon(24)
                    fila.Cuotas = renglon(25)
                    fila.CodMoneda = renglon(26)
                    ' fila.CodSeg =renglon(27)
                    'fila.InfoAdicional =renglon(28)
                    fila.TicketPosnet = renglon(29)
                    fila.TicketCaja = renglon(30)
                    fila.NroEmisor = renglon(31)
                    fila.NombreEmisor = renglon(32)  'BuscarNombreEmisor(req.emisor)
                    fila.NroPlan = renglon(33)
                    fila.TipoOperacion = renglon(34) ' compra/anulacion/devolucion
                    fila.Cajera = renglon(35)
                    fila.estado = renglon(36)
                    fila.hizocierre = 0
                    If renglon(38) <> "" Then fila.nrolote = renglon(38)

                    fila.exportado = 0
                    'fila.Criptograma = renglon(40)
                    fila.Fallback = renglon(41)
                    fila.ServiceCode = renglon(42)
                    'fila.PaqueteEncriptado = renglon(43)

                    If String.IsNullOrEmpty(renglon(44)) Then
                        fila.RespuestaChip = ""
                    Else
                        fila.RespuestaChip = renglon(44)
                    End If

                    If renglon(45) <> "" Then fila.CuponOriginal = renglon(45)
                    If renglon(46) <> "" Then fila.FechaCuponOriginal = renglon(46)
                    fila.ControlBackup = renglon(47)


                    datosDS.Movimientos.AddMovimientosRow(fila)
                    datostjTA.Update(fila)
                    cont += 1
                Catch ex As Exception
                    Logger.Error(String.Format("No se pudo importar el movimiento {0}/{1}/{2}/{3}", renglon(0), renglon(1), renglon(2), renglon(3)))
                End Try

            End While

        Catch ex As Exception
            Logger.Error(ex.Message)
        End Try
        Return cont


    End Function


    Public Function FiltrarParaCerrarPEI(host As Integer) As Dictionary(Of String, MovCierres)
        Dim dt As New DatosTjTableAdapters.MovimientosPEITableAdapter
        Dim mov As MovCierres

        Dim res0200 = From c In dt.GetData()
                      Where c.CodRespuesta = 0
                      Select c

        Dim acumulados As New Dictionary(Of String, MovCierres)
        Logger.Info(" ACUMULANDO CONTADORES HOST: " + host.ToString)
        Logger.Info("==============================")
        For Each reg200 In res0200
            If acumulados.ContainsKey(Trim(reg200.Terminal)) Then
                mov = acumulados(Trim(reg200.Terminal))
            Else
                mov = New MovCierres
            End If

            '************************
            'este es el que se usa
            '************************
            Select Case Val(reg200.Operacion)
                Case lib_PEI.Concepto.COMPRA_DE_BIENES
                    Logger.Info(String.Format("[Compra] Terminal: {0} Importe Total: {1}", Trim(reg200.Terminal), reg200.Importe))
                    mov.SumaCompras += reg200.Importe
                    mov.CantCompras += 1
                Case lib_PEI.Concepto.DEVOLUCION  ' DEVOLUCION
                    Logger.Info(String.Format("[Devol] Terminal: {0} Importe Total: {1}", Trim(reg200.Terminal), reg200.Importe))
                    mov.SumaDevoluciones += reg200.Importe
                    mov.CantDevoluciones += 1
                Case lib_PEI.Concepto.EXTRACCION
                    Logger.Info(String.Format("[Extrac] Terminal: {0} Importe Total: {1}", Trim(reg200.Terminal), reg200.Importe))
                    mov.SumaCompras += reg200.Importe
                    mov.CantCompras += 1
            End Select
            If acumulados.ContainsKey(Trim(reg200.Terminal)) Then ' ver la clave si esta bien o si se pueden repetir (no creo, la terminal es unica por cada host)
                acumulados(Trim(reg200.Terminal)) = mov
            Else
                acumulados.Add(Trim(reg200.Terminal), mov)
            End If


        Next
        Return acumulados
    End Function

    Public Function FiltrarParaCerrar(host As Integer) As Dictionary(Of String, MovCierres)
        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim mov As MovCierres

        Dim res0200 = From c In dt.GetData()
                      Where c.TipoMensaje = "0200" And c.NroHost = host And c.hizocierre = False
                      Select c

        Dim acumulados As New Dictionary(Of String, MovCierres)
        Logger.Info(" ACUMULANDO CONTADORES HOST: " + host.ToString)
        Logger.Info("==============================")
        For Each reg200 In res0200

            Dim res0210 = From x In dt.GetData()
                          Where x.TipoMensaje = "0210" And x.NroHost = reg200.NroHost And x.NroTrace = reg200.NroTrace And Trim(x.Terminal) = Trim(reg200.Terminal)
                          Select x

            Dim res0400 = From y In dt.GetData()
                          Where y.TipoMensaje = "0400" And y.NroHost = reg200.NroHost And y.NroTrace = reg200.NroTrace And Trim(y.Terminal) = Trim(reg200.Terminal)
                          Select y

            If res0210.Count > 0 AndAlso res0210(0).CodRespuesta = 0 Then 'al menos llego la respuesta
                Try
                    If res0210(0).ModoIngreso = 5 AndAlso res0210(0).RespuestaChip <> 0 Then Continue For
                Catch

                End Try

                If res0400.Count = 0 Then 'no tuvo reverso
                    Dim reg = res0210(0)
                    'lo tengo en cuenta y lo acumulo
                    If acumulados.ContainsKey(Trim(reg.Terminal)) Then
                        mov = acumulados(Trim(reg.Terminal))
                    Else
                        mov = New MovCierres
                    End If

                    '************************
                    'este es el que se usa
                    '************************
                    Dim importetotal As Decimal
                    importetotal = reg.Importe
                    'If reg.NroHost = 1 Or reg.NroHost = 5 Then
                    '    importetotal = importetotal + reg.Cashback

                    'End If



                    Select Case Val(reg.CodProc)
                        Case 0, 1000, 2000, 8000, 9000, 90000, 91000, 92000, 98000, 99000
                            Logger.Info(String.Format("[Compra] Terminal: {0} Cod. Proc.: {1}  Importe Total: {2}", Trim(reg.Terminal), reg.CodProc, importetotal))
                            mov.SumaCompras += importetotal
                            mov.CantCompras += 1
                        Case 200000, 201000, 202000, 208000, 209000 ' DEVOLUCION
                            Logger.Info(String.Format("[Devol] Terminal: {0} Cod. Proc.: {1}  Importe Total: {2}", Trim(reg.Terminal), reg.CodProc, importetotal))
                            mov.SumaDevoluciones += importetotal
                            mov.CantDevoluciones += 1
                            'mov.SumaCompras -= reg.Importe
                            'mov.CantCompras -= 1
                        Case 20000, 21000, 22000, 28000, 29000, 140000, 210000, 211000, 212000, 218000, 219000 ' ANULACION COMPRA
                            Logger.Info(String.Format("[An.Com] Terminal: {0} Cod. Proc.: {1}  Importe Total: {2}", Trim(reg.Terminal), reg.CodProc, importetotal))
                            mov.SumaAnulaciones += importetotal
                            mov.CantAnulaciones += 1
                            mov.SumaCompras -= importetotal
                            mov.CantCompras -= 1
                        Case 220000, 221000, 222000, 228000, 229000 ' ANULACION DEVOLUCION
                            Logger.Info(String.Format("[An/dev] Terminal: {0} Cod. Proc.: {1}  Importe Total: {2}", Trim(reg.Terminal), reg.CodProc, importetotal))
                            mov.SumaAnulaciones += importetotal
                            mov.CantAnulaciones += 1
                            mov.SumaDevoluciones -= importetotal
                            mov.CantDevoluciones -= 1
                    End Select
                    If acumulados.ContainsKey(Trim(reg.Terminal)) Then ' ver la clave si esta bien o si se pueden repetir (no creo, la terminal es unica por cada host)
                        acumulados(Trim(reg.Terminal)) = mov
                    Else
                        acumulados.Add(Trim(reg.Terminal), mov)
                    End If
                End If

            End If
        Next
        Return acumulados
    End Function



    ''' <summary>
    ''' Filtra la tabla movimientos de una sola caja para un unico host para realizar el cierre.
    ''' </summary>
    ''' <param name="pinguino">Nro de pinguino</param>
    ''' <param name="caja">Nro de caja</param>
    ''' <param name="host">Nro de host</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function FiltrarParaCerrar(pinguino As Integer, caja As Integer, host As Integer) As Dictionary(Of String, MovCierres)

        '-----------------------------
        ' SUMA LAS COMPRAS
        '-----------------------------
        Dim mov As MovCierres
        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim result = From c In dt.GetData()
                     Where c.NroCajaPropio = caja And c.Pinguino = pinguino And c.TipoMensaje = "0200" And c.NroHost = host And c.hizocierre = False
                     Group c By c.NroHost, c.Terminal, c.CodProc Into suma = Sum(c.Importe), cant = Count()
                     Select NroHost, Terminal, CodProc, cant, suma

        Dim acumulados As New Dictionary(Of String, MovCierres)
        Logger.Info(String.Format("Filtrando movimientos para cerrar de caja {0}/{1}/{2}", host, pinguino, caja))
        For Each reg In result
            If acumulados.ContainsKey(Trim(reg.Terminal)) Then
                mov = acumulados(Trim(reg.Terminal))
            Else
                mov = New MovCierres
            End If
            Select Case CInt(reg.CodProc)
                Case 0, 1000, 2000, 8000, 9000, 90000
                    mov.SumaCompras = mov.SumaCompras + reg.suma
                    mov.CantCompras = mov.CantCompras + reg.cant
                Case 200000, 201000, 202000, 208000, 209000, 90000 ' DEVOLUCION
                    mov.SumaDevoluciones = mov.SumaDevoluciones + reg.suma
                    mov.CantDevoluciones = mov.CantDevoluciones + reg.cant
                Case 20000, 21000, 22000, 28000, 29000, 140000 ' ANULACION COMPRA
                    mov.SumaAnulaciones = mov.SumaAnulaciones + reg.suma
                    mov.CantAnulaciones = mov.CantAnulaciones + reg.cant
                    mov.SumaCompras = mov.SumaCompras - reg.suma
                    mov.CantCompras = mov.CantCompras - reg.cant
                Case 220000, 221000, 222000, 228000, 229000 ' ANULACION DEVOLUCION
                    mov.SumaAnulaciones = mov.SumaAnulaciones + reg.suma
                    mov.CantAnulaciones = mov.CantAnulaciones + reg.cant
                    mov.SumaDevoluciones = mov.SumaDevoluciones - reg.suma
                    mov.CantDevoluciones = mov.CantDevoluciones - reg.cant
            End Select
            If acumulados.ContainsKey(Trim(reg.Terminal)) Then ' ver la clave si esta bien o si se pueden repetir 
                acumulados(Trim(reg.Terminal)) = mov
            Else
                'mov.emisor = reg.NroEmisor
                acumulados.Add(Trim(reg.Terminal), mov)
            End If
        Next

        '-----------------------------
        ' RESTA LOS REVERSOS          
        '-----------------------------
        Dim dt2 As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim result2 = From c In dt.GetData()
                      Where c.NroCajaPropio = caja And c.Pinguino = pinguino And c.TipoMensaje = "0400" And c.NroHost = host And c.hizocierre = False
                      Group c By c.NroHost, c.Terminal, c.CodProc Into suma = Sum(c.Importe), cant = Count()
                      Select NroHost, Terminal, CodProc, cant, suma 'NroEmisor, Terminal, CodProc, cant, suma
        For Each reg2 In result2
            If acumulados.ContainsKey(Trim(reg2.Terminal)) Then
                mov = acumulados(Trim(reg2.Terminal))
            Else
                'mov = New MovCierres
                MsgBox("No existe clave mov 400")
            End If
            Select Case CInt(reg2.CodProc)
                Case 0, 1000, 2000, 8000, 9000, 90000
                    mov.SumaCompras = mov.SumaCompras - reg2.suma
                    mov.CantCompras = mov.CantCompras - reg2.cant
                Case 200000, 201000, 202000, 208000, 209000, 90000 ' DEVOLUCION
                    mov.SumaDevoluciones = mov.SumaDevoluciones - reg2.suma
                    mov.CantDevoluciones = mov.CantDevoluciones - reg2.cant
                Case 20000, 21000, 22000, 28000, 29000, 140000 ' ANULACION COMPRA
                    mov.SumaAnulaciones = mov.SumaAnulaciones - reg2.suma
                    mov.CantAnulaciones = mov.CantAnulaciones - reg2.cant
                    mov.SumaCompras = mov.SumaCompras + reg2.suma
                    mov.CantCompras = mov.CantCompras + reg2.cant
                Case 220000, 221000, 222000, 228000, 229000 ' ANULACION DEVOLUCION
                    mov.SumaAnulaciones = mov.SumaAnulaciones - reg2.suma
                    mov.CantAnulaciones = mov.CantAnulaciones - reg2.cant
                    mov.SumaDevoluciones = mov.SumaDevoluciones + reg2.suma
                    mov.CantDevoluciones = mov.CantDevoluciones + reg2.cant
            End Select
            If acumulados.ContainsKey(Trim(reg2.Terminal)) Then ' ver la clave si esta bien o si se pueden repetir 
                acumulados(Trim(reg2.Terminal)) = mov
            Else
                'acumulados.Add(reg2.Terminal, mov)
                MsgBox("No existe clave mov 400")
            End If
        Next

        Return acumulados
    End Function


    ''' <summary>
    ''' Exporta todos los movimientos, se hayan o no hecho el cierre.
    ''' </summary>
    Public Function Exportar_TodosPEI(fecha_cierre As Date) As Boolean
        Dim errores As Boolean = False
        Dim dt As New DatosTjTableAdapters.MovimientosPEITableAdapter
        Dim result = From c In dt.GetData
                     Select c
        Dim fileMovimientos As FileStream
        Dim renglon As StreamWriter
        Try
            Logger.Info("Exportando TODOS los movimientos PEI...")
            '---  Configuracion.DirTarjetas  = \\pinguino1-1\sys\tarjetas\visa
            fileMovimientos = New FileStream(Configuracion.DirTarjetas + "\PEI\Exportados\movpei" + fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + "TOT.txt", FileMode.Append, FileAccess.Write)
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
            Logger.Info("Exportación TOTAL PEI finalizada.")
            errores = False
        Catch ex1 As IO.IOException
            Logger.Error("Falló la exportación de uno o mas registros de la tabla movimientos PEI.")
            errores = True
        Catch ex As Exception
            Logger.Error("Falló la exportación de uno o mas registros de la tabla movimientos PEI.")
            errores = True
        End Try
        Return errores
    End Function

    ''' <summary>
    ''' Exporta todos los movimientos, se hayan o no hecho el cierre.
    ''' </summary>
    Public Sub Exportar_Todos(fecha_cierre As Date)
        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim result = From c In dt.GetData
                     Select c
        Dim fileMovimientos As FileStream
        Dim renglon As StreamWriter
        Try
            Logger.Info("Exportando TODOS los movimientos...")
            fileMovimientos = New FileStream(Configuracion.DirTarjetas + "\Exportados\mov" + fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + "TOT.txt", FileMode.Append, FileAccess.Write)
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
                    Debug.Write(reg(i).ToString + ";")
                    renglon.Write(reg(i).ToString + ";")
                Next
                renglon.Write(vbNewLine)
            Next
            fileMovimientos.Close()
            Logger.Info("Exportación TOTAL finalizada.")
        Catch ex1 As IO.IOException
            Logger.Error("Falló la exportación de uno o mas registros de la tabla movimientos.")

        Catch ex As Exception
            Logger.Error("Falló la exportación de uno o mas registros de la tabla movimientos.")

        End Try


    End Sub







    Public Function Exportar_Movimientos(fecha_cierre As Date) As Boolean
        Dim VerRespChip As Boolean
        Dim errores As Boolean = False
        ' And (c.TipoMensaje = "0210" Or c.TipoMensaje = "0410")
        Dim dt As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim result = From c In dt.GetData
                     Where c.hizocierre = True Or (c.hizocierre = False And c.CodRespuesta <> 0) Or (c.hizocierre = False And c.CodRespuesta = 0)
                     Select c


        Dim fileMovimientos As FileStream
        Dim renglon As StreamWriter
        Dim cant As Integer
        Try
            Logger.Info("Exportando movimientos...")
            fileMovimientos = New FileStream(Configuracion.DirTarjetas + "\Exportados\mov" + fecha_cierre.ToString("yyMMdd") + BuscarNroCierre(fecha_cierre).ToString("00") + ".txt", FileMode.Append, FileAccess.Write)
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
            cant += result.Count
            For Each reg In result
                If reg.hizocierre = False And reg.CodRespuesta.Trim = 0 Then
                    '--- MODOINGRESO 5 = CHIP        
                    '--- MODOINGRESO 7 = CONTACTLESS 
                    '--- MODIFICADO 12/08/2021 - VENIA NULL O "   " Y DABA ERROR PORQUE RESPUESTACHIP ES DOUBLE  
                    Try
                        If reg.ModoIngreso = 5 AndAlso reg.RespuestaChip = 0 Then Continue For
                    Catch

                    End Try


                    '--- lo solucione con el try catch...  al pedo este codigo 
                    'VerRespChip = False
                    'If reg.ModoIngreso = 5 Then
                    '    If String.IsNullOrEmpty(reg.RespuestaChip) = True Then
                    '    Else
                    '        If Trim(reg.RespuestaChip) = "" Then
                    '        Else
                    '            VerRespChip = True
                    '        End If
                    '    End If
                    '    If VerRespChip = True Then
                    '        If reg.RespuestaChip = 0 Then
                    '            Continue For
                    '        End If
                    '    End If
                    'End If

                    Dim result3 = From c In dt.GetData
                                  Where c.TipoMensaje = "0400" And Trim(c.Terminal) = Trim(reg.Terminal) And c.NroHost = reg.NroHost And c.NroTrace = reg.NroTrace
                                  Select c
                    If result3.Count = 0 Then
                        Continue For
                    End If
                End If

                If reg.hizocierre = False And reg.CodRespuesta.Trim <> 0 Then
                    Dim result2 = From c In dt.GetData
                                  Where (c.TipoMensaje = "0200" Or c.TipoMensaje = "0400") And Trim(c.Terminal) = Trim(reg.Terminal) And c.NroHost = reg.NroHost And c.NroTrace = reg.NroTrace
                                  Select c
                    If result2.Count > 0 Then
                        cant += result2.Count
                        For i As Integer = 0 To x
                            renglon.Write(result2(0)(i).ToString + ";")
                        Next
                        renglon.Write(vbNewLine)
                        Marcar_exportado(Trim(reg.Terminal), result2(0).TipoMensaje, reg.NroHost, reg.NroTrace)
                    End If
                End If

                For i As Integer = 0 To x
                    renglon.Write(reg(i).ToString + ";")
                Next
                renglon.Write(vbNewLine)
                Marcar_exportado(Trim(reg.Terminal), reg.TipoMensaje, reg.NroHost, reg.NroTrace)
            Next


            fileMovimientos.Close()
            Logger.Info(String.Format("Exportación finalizada. Se exportaron {0} registros", cant.ToString))
        Catch ex1 As IO.IOException
            Logger.Error("Falló la exportación de uno o mas registros de la tabla movimientos (ex1).")
            errores = True
        Catch ex As Exception
            Logger.Error("Falló la exportación de uno o mas registros de la tabla movimientos (ex).")
            errores = True
        End Try
        Return errores

    End Function

    Public Sub Eliminar_exportadosPEI()
        Dim datostjTA As New DatosTjTableAdapters.MovimientosPEITableAdapter
        Dim tablalocal As New DatosTj.MovimientosPEIDataTable
        Try
            Logger.Info("Borrando movimientos PEI exportados.")
            datostjTA.DeleteTodos()
            Logger.Info("Se borraron los movimientos PEI exportados.")
        Catch ex As Exception
            Logger.Error("No se pudieron borrar los movimientos PEI exportados.")
        End Try
    End Sub

    Public Sub Eliminar_exportados()
        Dim datostjTA As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim tablalocal As New DatosTj.MovimientosDataTable
        Try
            Logger.Info("Borrando movimientos exportados.")
            datostjTA.DeleteExportados()
            Logger.Info("Se borraron los movimientos exportados.")
        Catch ex As Exception
            Logger.Error("No se pudieron borrar los movimientos exportados.")
        End Try
    End Sub

    Public Sub Eliminar_Archivos(fecha As Date)
        Try
            If Configuracion.TipoConfiguracion = Configuracion.Produccion Then
                Logger.Info("Borrando archivo de respaldo mov" & fecha.Date.ToString("yyMMdd") & ".bkp.")

                If File.Exists(Configuracion.DirTarjetas & "\respaldos\mov" + fecha.Date.ToString("yyMMdd") + ".bkp") Then File.Delete(Configuracion.DirTarjetas & "\respaldos\mov" + fecha.Date.ToString("yyMMdd") + ".bkp")
                If File.Exists("C:\temp\respaldos\mov" + fecha.Date.ToString("yyMMdd") + ".bkp") Then File.Delete("C:\temp\respaldos\mov" + fecha.Date.ToString("yyMMdd") + ".bkp")
            End If

        Catch ex As Exception
            Logger.Error("No se pudieron borrar los archivos de respaldo.")
        End Try

    End Sub

    Private Sub Marcar_exportado(pTerm As String, pTipoMov As String, pHost As Integer, pTrace As Integer)
        Dim datostjTA As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim tablalocal As New DatosTj.MovimientosDataTable
        Dim fila As DatosTj.MovimientosRow
        Try
            tablalocal = datostjTA.GetDataByClave(pTerm.Trim, pHost, pTrace, pTipoMov)
            fila = tablalocal.Item(0)
            fila.exportado = True
            datostjTA.Update(fila)
            Logger.Info(String.Format("Marcado como exportado {0}/{1}/{2}/{3}", pHost.ToString, pTerm.Trim, pTrace.ToString, pTipoMov))
        Catch
            Logger.Error(String.Format("No se pudo marcar como exportado el movimiento {0}/{1}/{2}/{3}", pHost.ToString, pTerm.Trim, pTrace.ToString, pTipoMov))
        End Try
    End Sub

    Public Function BuscarIDComercioCierre(pHost As Integer) As String
        Dim dt As New DatosTjTableAdapters.HOSTSTableAdapter
        Dim result = From c In dt.GetData
                     Where c.NroHost = pHost
                     Select c.IDComercioCierre
        Return result(0)
    End Function



    Public Function BuscarIDComercio(nroemisor As Integer) As String
        Dim dt As New DatosTjTableAdapters.EMISORTableAdapter
        Dim result = From c In dt.GetData()
                     Where c.Emisor = nroemisor
                     Select c.IDComercioRaf, c.IDComercioSF

        For Each reg In result
            If Configuracion.Ciudad = Configuracion.Rafaela Then
                Return reg.IDComercioRaf
            Else
                Return reg.IDComercioSF
            End If
        Next
        Return ""
    End Function


    Public Sub BuscarDatosAdicionales(rango As String, mensaje As TransmisorTCP.MensajeRespuesta, Optional servCode As String = "", Optional appnombre As String = "")
        Dim ranghabTA As New DatosTjTableAdapters.RANGHABTableAdapter
        Dim emisoresTA As New DatosTjTableAdapters.EMISORTableAdapter
        Dim tabla As New DatosTj.RANGHABDataTable
        Dim tablaEmisores As New DatosTj.EMISORDataTable
        Dim emisor As Integer
        'TODO: ver que hacer si hay mas de un rango para esa tarjeta.
        Try
            ranghabTA.FillByRango(tabla, rango)
            If tabla.Rows.Count >= 1 Then
                If tabla.Rows.Count > 1 AndAlso tabla.Rows(1).Item("Emisor") = 44 Then
                    emisor = 44       '--- MASTERCARD DEBIT  
                Else
                    emisor = tabla.Rows(0).Item("Emisor")
                End If


                If emisor = 2 Then '--- MASTERCARD  
                    If servCode IsNot Nothing AndAlso servCode.Trim <> "" AndAlso servCode.Substring(1, 1) = 2 Then
                        emisor = 44 '--- MASTER DEBIT  
                    ElseIf appnombre.Trim <> "" AndAlso (appnombre.Contains("Debit") Or appnombre.Contains("debit")) Then
                        emisor = 44 '--- MASTER DEBIT  
                    End If
                End If

            Else

                '--- verifica el rango de maestro porque no esta en el ranghab, esta dentro del emisor 2, como mastercard      
                '--- en el ranghab están cargadas las mastercard no las maestro, entonces si no existe en el ranghab, me fijo  
                '--- el rango si es maestro                                                                                    
                If CDbl(rango) > 500000 And CDbl(rango) < 699999 Then
                    emisor = 6      '--- MAESTRO   
                Else
                    If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                        If CDbl(rango) = 778899 Then        ' -- maestro redlink de prueba  
                            emisor = 6
                        End If
                    Else
                        emisor = 0
                    End If
                    'emisor = 0
                End If
            End If
            If emisor = 0 Then
                mensaje.codRespuesta = "XX"
                mensaje.Emisor = ""
                mensaje.Respuesta = String.Format("No existe el rango para la tarjeta especificada. BIN: {0}", rango)
                'InvalidarReq("INVALIDA: Rango Tarjeta inexistente")
                Logger.Warn(String.Format("No existe el rango para la tarjeta especificada. BIN: {0}", rango))
            Else
                emisoresTA.Fill(tablaEmisores)
                If tablaEmisores.Rows.Count > 0 Then
                    Dim fila = tablaEmisores.FindByEmisor(emisor)
                    mensaje.Emisor = fila.Descripcion
                    mensaje.cds = fila.ReqCodSeguridad
                    mensaje.u4d = fila.Req4Digitos

                    If emisor = 20 Then              '--- VISA   
                        If servCode <> "" AndAlso servCode.Substring(1, 1) = "2" Then
                            '--- es tarj. debito  
                            Logger.Info("PinPad - Es VISA DEBITO")
                            mensaje.Emisor = "VISA DEBITO"
                            mensaje.cds = False
                        End If
                    End If

                    If emisor = 6 Then    '--- MAESTRO   
                        mensaje.spi = True        '--- PONIENDO EN TRUE PIDE 1-CAJA AHORRO ($) - 2-CUENTA CORRIENTE ($), etc 
                    End If


                    '--- FUERZO SIEMPRE HOST DE FIRSTDATA, PORQUE ACA SOLO PASA CUANDO ES EMV.   
                    If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                        mensaje.host = "5"       '--- POSNET_HOMOLOG   
                    Else
                        mensaje.host = "1"       '--- POSNET  (PRODUCCION)  
                    End If




                    'mensaje.host = BuscarHost(emisor)






                End If
            End If
        Catch ex As Exception
            If rango < 6 Then
                mensaje.codRespuesta = "XX"
                mensaje.Emisor = ""
                mensaje.Respuesta = "No se especificó el nro de tarjeta."
                Logger.Info("No se especificó el nro de tarjeta.")
            Else
                mensaje.codRespuesta = "XX"
                mensaje.Emisor = ""
                mensaje.Respuesta = "Fallo en el servidor SQL, verifique, avise a COMPUTOS"
                Logger.Fatal("Fallo en el servidor SQL, verifique, avise a COMPUTOS (3)")
            End If

        End Try





    End Sub

    Private Function BuscarHost(emisor As Integer) As String
        'Try
        '    Dim dt As New DatosTjTableAdapters.EMISORTableAdapter
        '    Dim result = From c In dt.GetData()
        '                 Where c.Emisor = emisor
        '                 Select c.Host

        '    Dim h As String = 0

        '    'CAMBIAR PARA PRODUCCION
        '    If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
        '        If result(0) = 1 Then h = "5"
        '        If result(0) = 2 Then h = "5"
        '    Else
        '        If CStr(result(0)) = 2 Then
        '            h = "1"
        '        Else
        '            h = CStr(result(0))
        '        End If
        '        h = CStr(result(0))
        '    End If
        '    Return h
        'Catch ex As Exception
        '    Return "DESCONOCIDO"
        '    Logger.Warn(String.Format("Emisor DESCONOCIDO Nro: {0}", emisor))
        'End Try
    End Function

    Public Sub BuscarEmisorEnRangHab3DES(req As Req)
        Dim ranghabTA As New DatosTjTableAdapters.RANGHABTableAdapter
        Dim tabla As New DatosTj.RANGHABDataTable
        'TODO: ver que hacer si hay mas de un rango para esa tarjeta.
        Try
            ranghabTA.FillByRango(tabla, req.msjIda.nroTarjeta.Substring(0, 6))
            If tabla.Rows.Count >= 1 Then
                If tabla.Rows.Count > 1 AndAlso tabla.Rows(1).Item("Emisor") = 44 Then
                    req.emisor = 44             '--- Mastercard Debit 
                Else
                    req.emisor = tabla.Rows(0).Item("Emisor")
                End If

                If req.emisor = 2 Then 'MASTERCARD
                    If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Banda Then
                        If req.msjIda.serviceCode <> "" AndAlso req.msjIda.serviceCode.Substring(1, 1) = 2 Then
                            req.emisor = 44 'MC DEBIT
                        End If
                    ElseIf req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Then
                        'If req.msjIda.nombreApplicacion <> "" AndAlso (req.msjIda.nombreApplicacion.Contains("Debit") Or req.msjIda.nombreApplicacion.Contains("debit")) Then
                        If req.msjIda.nombreApplicacion <> "" AndAlso (req.msjIda.nombreApplicacion.Contains("Debit") Or req.msjIda.nombreApplicacion.Contains("debit") Or req.msjIda.nombreApplicacion.Contains("DEBIT")) Then
                            req.emisor = 44        'MC DEBIT
                        End If
                    End If
                End If
            Else
                'verifica el rango de maestro porque no esta en el ranghab

                If CDbl(req.msjIda.nroTarjeta.Substring(0, 6)) > 500000 And CDbl(req.msjIda.nroTarjeta.Substring(0, 6)) < 699999 Then
                    req.emisor = 6
                Else
                    If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                        If CDbl(req.msjIda.nroTarjeta.Substring(0, 6)) = 778899 Then ' -- maestro redlink de prueba
                            req.emisor = 6
                        End If
                    Else
                        req.emisor = 0
                    End If
                    'req.emisor = 0
                End If
            End If

            '--- caja ahorro $ hecho por mi  
            'If req.emisor = 0 And req.tipocuenta > 1 Then
            '    req.emisor = 6
            'End If

            Try
                Logger.Info("PinPad - Pinguino: " + req.pinguino + " Caja: " + req.nroCaja.ToString)
            Catch
            End Try
            Try
                Logger.Info("PinPad - Buscar Emisor Ranghab: " + req.emisor.ToString)
            Catch
            End Try

            If req.emisor = 0 Then
                req.InvalidarReq("INVALIDA: Rango Tarjeta inexistente")
                Logger.Warn(String.Format("No existe el rango para la tarjeta especificada. BIN: {0}", req.msjIda.nroTarjeta.Substring(0, 6)))
            End If
        Catch ex As Exception
            If Len(req.ida.TARJ) < 6 Then
                req.InvalidarReq("Falta numero de tarjeta.")
                Logger.Info("No se especificó el nro de tarjeta.")
            Else
                req.InvalidarReq("Fallo de servidor: Reintente mas tarde")
                Logger.Fatal("Fallo en el servidor SQL, verifique, avise a COMPUTOS (4)")
            End If
        End Try

    End Sub

    Public Function esmaestro(rango As String) As Integer

        Dim ranghabTA As New DatosTjTableAdapters.RANGHABTableAdapter
        Dim tabla As New DatosTj.RANGHABDataTable
        'TODO: ver que hacer si hay mas de un rango para esa tarjeta.
        Try
            ranghabTA.FillByRango(tabla, rango)
            If tabla.Rows.Count >= 1 Then
                Return tabla.Rows(0).Item("Emisor")
            Else
                'verifica el rango de maestro porque no esta en el ranghab
                If CDbl(rango) > 500000 And CDbl(rango) < 690000 Then
                    Return 6
                Else
                    If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                        If CDbl(rango) = 778899 Then ' -- maestro redlink de prueba
                            Return 6
                        End If
                    Else
                        Return 0
                    End If
                End If
            End If

        Catch ex As Exception
            If Len(rango) < 6 Then
                Logger.Info("No se especificó el nro de tarjeta.")
            Else
                Logger.Fatal("Fallo en el servidor SQL, verifique, avise a COMPUTOS (1)")
            End If
        End Try

    End Function

    Public Sub BuscarEmisorEnRangHab(req As Req)
        Dim ranghabTA As New DatosTjTableAdapters.RANGHABTableAdapter
        Dim tabla As New DatosTj.RANGHABDataTable
        'TODO: ver que hacer si hay mas de un rango para esa tarjeta.
        Try
            ranghabTA.FillByRango(tabla, req.ida.TARJ.Substring(0, 6))
            If tabla.Rows.Count >= 1 Then
                req.emisor = tabla.Rows(0).Item("Emisor")
            Else
                '--- verifica el rango de maestro porque no esta en el ranghab 
                If CDbl(req.ida.TARJ.Substring(0, 6)) > 500000 And CDbl(req.ida.TARJ.Substring(0, 6)) < 690000 Then
                    req.emisor = 6
                Else
                    If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                        If CDbl(req.ida.TARJ.Substring(0, 6)) = 778899 Then ' -- maestro redlink de prueba
                            req.emisor = 6
                        End If
                    Else
                        req.emisor = 0
                    End If
                End If
            End If
            If req.emisor = 0 Then
                req.InvalidarReq("INVALIDA: Rango Tarjeta inexistente")
                Logger.Warn(String.Format("No existe el rango para la tarjeta especificada. BIN: {0}", req.ida.TARJ.Substring(0, 6)))
            End If
        Catch ex As Exception
            If Len(req.ida.TARJ) < 6 Then
                req.InvalidarReq("Falta numero de tarjeta.")
                Logger.Info("No se especificó el nro de tarjeta.")
            Else
                req.InvalidarReq("Fallo de servidor: Reintente mas tarde")
                Logger.Fatal("Fallo en el servidor SQL, verifique, avise a COMPUTOS (2)")
            End If

        End Try

    End Sub


    Public Sub BuscarVisaBin3DES(req As Req)
        '        Dim VisaBinTA As New DatosTjTableAdapters.VISABINTableAdapter
        '        Dim tabla As New DatosTj.VISABINDataTable
        Try
            '           VisaBinTA.Fill(tabla)
            '          Dim fila As DatosTj.VISABINRow = tabla.FindBynrobin(req.ida.TARJ.Substring(0, 6))
            '            If fila IsNot Nothing Then

            '----------------------------------------------------
            '--- DATO DISCRECIONAL = 2 ES VISA DEBITO
            '----------------------------------------------------
            '--- CORREGIDO EL 25/08/2021 (PREGUNTABA POR CHIP O BANDA Y NO POR CONTACTLESS, PUSE QUE PREUNTE POR TODO 
            'If (req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Banda Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip) AndAlso req.msjIda.serviceCode.Substring(1, 1) = "2" Then
            If req.msjIda.serviceCode.Substring(1, 1) = "2" Then
                '--- es tarj. debito
                Logger.Info("*** Pinpad - Es VISA DEBITO")
                req.descEmisor = "VISA DEBITO"
                req.esVisaDebito = True
                req.obligarCodSeg = False
            Else
                '--- es tarj. credito
                '--- CORREGIDO EL 25/08/2021 (PREGUNTABA POR CHIP O BANDA Y NO POR CONTACTLESS, PUSE QUE PREUNTE POR TODO 
                'If req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Banda Or req.msjIda.tipoIngreso = TransmisorTCP.TipoIngreso.Chip Then Logger.Info(String.Format("Pinpad - Es crédito, dato discrecional {0}", req.msjIda.serviceCode.Substring(1, 1)))
                Logger.Info(String.Format("*** Pinpad - Es credito, dato discrecional {0}", req.msjIda.serviceCode.Substring(1, 1)))
                req.esVisaDebito = False
            End If
        Catch ex As Exception
            '            If Len(req.ida.TARJ) < 6 Then
            '           req.InvalidarReq("Falta numero de tarjeta.")
            '          Logger.Info("No se especificó el nro de tarjeta.")
            '         Else
            req.InvalidarReq("Fallo en BuscarVISABin")
            Logger.Fatal("Fallo en BuscarVISABin")
            '        End If
        End Try
    End Sub



    Public Sub BuscarVisaBin(req As Req)
        '        Dim VisaBinTA As New DatosTjTableAdapters.VISABINTableAdapter
        '        Dim tabla As New DatosTj.VISABINDataTable
        Try

            '           VisaBinTA.Fill(tabla)
            '          Dim fila As DatosTj.VISABINRow = tabla.FindBynrobin(req.ida.TARJ.Substring(0, 6))
            '            If fila IsNot Nothing Then
            If req.ida.MANUAL = 0 AndAlso req.ida.TRACK2.Split("=")(1).Substring(5, 1) = "2" Then
                '--- es tarj. debito
                Logger.Info(" Es VISA DEBITO")
                req.descEmisor = "VISA DEBITO"
                req.esVisaDebito = True
                req.obligarCodSeg = False
            Else
                '--- es tarj. credito
                If req.ida.MANUAL = 0 Then Logger.Info(String.Format(" Es crédito, dato discrecional {0}", req.ida.TRACK2.Split("=")(1).Substring(5, 1)))
                req.esVisaDebito = False
            End If
        Catch ex As Exception
            '            If Len(req.ida.TARJ) < 6 Then
            '           req.InvalidarReq("Falta numero de tarjeta.")
            '          Logger.Info("No se especificó el nro de tarjeta.")
            '         Else
            req.InvalidarReq("Fallo en BuscarVISABin")
            Logger.Fatal("Fallo en BuscarVISABin")
            '        End If
        End Try
    End Sub

    Public Sub CerrarTransacciones(pterm As String, phost As Byte, lote As Integer)
        Dim movimientosTA As New DatosTjTableAdapters.MovimientosTableAdapter
        Dim tabla As New DatosTj.MovimientosDataTable
        Try
            movimientosTA.FillByHostTerminal(tabla, Trim(pterm), phost)

            For Each movRow In tabla
                movRow.hizocierre = True
                movRow.nrolote = lote
                Logger.Info(String.Format("Cerrando movimientos: {0}/{1}/{2} {3} con lote {4}", phost, pterm.Trim, movRow.NroTrace, movRow.TipoMensaje, lote))
            Next
            movimientosTA.Update(tabla)
        Catch ex As Exception
            Logger.Fatal("No se pudo actualizar registros con informacion de cierre. Avise a COMPUTOS (5).")
        End Try
    End Sub

    Public Sub borrar_claves()
        Dim clavesTA As New DatosTjTableAdapters.NUMEROSTableAdapter
        Dim tabla As New DatosTj.NUMEROSDataTable
        Try
            Dim action = Sub(row)
                             row("WkDatos") = ""
                             row("WKPines") = ""
                             row("PosicionMK") = ""
                             row("SeriePP") = ""
                         End Sub
            clavesTA.Fill(tabla)
            tabla.Select("TRIM(WKDatos) <> ''").ToList().ForEach(action)

            clavesTA.Update(tabla)
        Catch ex As Exception
            Logger.Fatal("No se pudieron borrar las claves.")
        End Try
    End Sub

    Public Function BuscarNombreEmisor(emi As Integer) As String
        Try
            Dim dt As New DatosTjTableAdapters.EMISORTableAdapter
            Dim result = From c In dt.GetData()
                         Where c.Emisor = emi
                         Select c.Descripcion
            Return result(0)
        Catch ex As Exception
            Return "DESCONOCIDO"
            Logger.Warn(String.Format("Emisor DESCONOCIDO Nro: {0}", emi))
        End Try

    End Function


    Public Sub BuscarEmisorTCP(req As Req)
        Dim emisorTA As New DatosTjTableAdapters.EMISORTableAdapter
        Dim tabla As New DatosTj.EMISORDataTable
        Try
            emisorTA.Fill(tabla)
            If req.emisor <> 0 Then
                Dim fila As DatosTj.EMISORRow = tabla.FindByEmisor(req.emisor)
                If fila IsNot Nothing Then
                    With req
                        .descEmisor = fila.Descripcion
                        If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                            'If fila.Host = TjComun.TipoHost.VISA Then
                            '    .nrohost = TjComun.TipoHost.Visa_homolog
                            'ElseIf fila.Host = TjComun.TipoHost.POSNET Then
                            .nrohost = TjComun.TipoHost.Posnet_homolog
                            'End If

                        Else

                            '--- ACA ACA ACA ACA ULTIMO - ver nrohost                        
                            '--- ESTA FIJO 5, HAY QUE PONER 1 Y CARGAR TODAS LAS TERMINALES  
                            '--- EN LA TABLA NUMEROS, PARA CADA CAJA DE CADA PINGUINO        

                            '--- ESTA FIJO TjComun.TipoHost.POSNET, porque fue con el unico que se homologo, cuando se homologue  
                            '--- con visa se verá por donde va                                                                    
                            .nrohost = TjComun.TipoHost.POSNET

                            '.nrohost = TjComun.TipoHost.VISA
                            '.nrohost = fila.Host    '--- ACA MANDA DE ACUERDO AL HOST QUE TIENE EL EMISOR  
                        End If

                        If Configuracion.Ciudad = Configuracion.Rafaela And req.pinguino <> "06" Then
                            If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                                If fila.Host = TjComun.TipoHost.VISA Then
                                    .idComercio = "00000013"
                                    '.idComercio = "03659307"
                                ElseIf fila.Host = TjComun.TipoHost.POSNET Then
                                    .idComercio = "00000013"
                                End If
                            Else
                                If req.nrohost = TjComun.TipoHost.VISA Then
                                    .idComercio = fila.IDComercioRafFD          ' Fuerza IDComercio FirstData  "06473369" 
                                Else
                                    '--- Pregunto el host que busco del Emisor, porque arriba fuerzo .nrohost (del req) como FirstData 
                                    '--- y si es visa tengo que forzar IdComercio como FirstData                                       
                                    If fila.Host = 1 Then
                                        .idComercio = fila.IDComercioRaf
                                    Else
                                        .idComercio = fila.IDComercioRafFD          ' Fuerza IDComercio FirstData  "06473369" 
                                    End If
                                End If
                            End If
                        Else
                            If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                                If fila.Host = TjComun.TipoHost.VISA Then
                                    '.idComercio = "03659307"
                                    .idComercio = "00000013"
                                ElseIf fila.Host = TjComun.TipoHost.POSNET Then
                                    .idComercio = "00000013"
                                End If
                            Else
                                If req.nrohost = TjComun.TipoHost.VISA Then
                                    .idComercio = fila.IDComercioSFFD          ' Fuerza IDComercio FirstData 
                                Else
                                    '--- Pregunto el host que busco del Emisor, porque arriba fuerzo .nrohost como FirstData 
                                    '--- y si es visa tengo que forzar IdComercio como FirstData                             
                                    If fila.Host = 1 Then
                                        .idComercio = fila.IDComercioSF
                                    Else
                                        .idComercio = fila.IDComercioSFFD          ' Fuerza IDComercio FirstData  "06473369" 
                                    End If
                                End If
                            End If
                        End If
                        .obligar4Dig = fila.Req4Digitos
                        .obligarCodSeg = fila.ReqCodSeguridad
                        .obligarFechaVenc = fila.ReqFechaVenc

                        Try
                            Logger.Info("PinPad - IdComercio: " + .idComercio)
                        Catch
                        End Try

                    End With
                End If
            Else
                req.InvalidarReq("INVALIDA: Emisor inexistente")
                Logger.Info(String.Format("Emisor inexistente Nro: {0}", req.emisor))
            End If
        Catch ex As Exception
            req.InvalidarReq("Falla de servidor: Reintente mas tarde.")
            Logger.Fatal("Falla con el servidor SQL. Avise a COMPUTOS (6).")
        End Try
    End Sub

    Public Sub BuscarEmisor(req As Req)
        Dim emisorTA As New DatosTjTableAdapters.EMISORTableAdapter
        Dim tabla As New DatosTj.EMISORDataTable
        Try
            emisorTA.Fill(tabla)
            If req.emisor <> 0 Then
                Dim fila As DatosTj.EMISORRow = tabla.FindByEmisor(req.emisor)
                If fila IsNot Nothing Then
                    With req
                        .descEmisor = fila.Descripcion
                        If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                            If fila.Host = TjComun.TipoHost.VISA Then
                                .nrohost = TjComun.TipoHost.Visa_homolog
                            ElseIf fila.Host = TjComun.TipoHost.POSNET Then
                                .nrohost = TjComun.TipoHost.Posnet_homolog
                            End If
                        Else
                            .nrohost = fila.Host
                        End If

                        If Configuracion.Ciudad = Configuracion.Rafaela And req.pinguino <> "06" Then
                            If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                                If fila.Host = TjComun.TipoHost.VISA Then
                                    .idComercio = "03659307"
                                ElseIf fila.Host = TjComun.TipoHost.POSNET Then
                                    .idComercio = "00000013"
                                End If
                            Else
                                .idComercio = fila.IDComercioRaf
                            End If
                        Else
                            If Configuracion.TipoConfiguracion = Configuracion.Homologacion Then
                                If fila.Host = TjComun.TipoHost.VISA Then
                                    .idComercio = "03659307"
                                ElseIf fila.Host = TjComun.TipoHost.POSNET Then
                                    .idComercio = "00000013"
                                End If
                            Else
                                .idComercio = fila.IDComercioSF
                            End If
                        End If
                        .obligar4Dig = fila.Req4Digitos
                        .obligarCodSeg = fila.ReqCodSeguridad
                        .obligarFechaVenc = fila.ReqFechaVenc
                    End With
                End If
            Else
                req.InvalidarReq("INVALIDA: Emisor inexistente")
                Logger.Info(String.Format("Emisor inexistente Nro: {0}", req.emisor))
            End If
        Catch ex As Exception
            req.InvalidarReq("Falla de servidor: Reintente mas tarde.")
            Logger.Fatal("Falla con el servidor SQL. Avise a COMPUTOS (7).")
        End Try
    End Sub


    Public Sub BuscarPlan3DES(req As Req)
        Dim planesTA As New DatosTjTableAdapters.PLANESTableAdapter
        Dim tabla As New DatosTj.PLANESDataTable
        Try


            'Try
            '    MsgBox("req.msjIda.plan    " & req.msjIda.plan.ToString)
            '    MsgBox("req.emisor    " & req.emisor.ToString)
            'Catch
            'End Try
            Try
                Logger.Info("PinPad - Buscando Emisor: " + req.emisor.ToString + " Plan: " + req.msjIda.plan)
            Catch
            End Try


            planesTA.Fill(tabla)
            If req.msjIda.plan >= 0 And req.emisor > 0 Then
                Dim fila As DatosTj.PLANESRow = tabla.FindByEmisorNroPlan(req.emisor, req.msjIda.plan)
                If fila IsNot Nothing Then
                    With req
                        .cuotas = fila.Cuotas
                        .moneda = fila.Moneda
                        .descPlan = fila.DescPlan
                        .coeficiente = fila.Coeficiente
                        .planemisor = fila.PlanEmisor
                    End With
                Else
                    req.InvalidarReq("INVALIDA: Plan o Emisor inexistente (8)")
                    req.RespInvalida = "INVALIDA: Plan o Emisor inexistente (Plan: " + req.msjIda.plan + ")  (8)"
                    Logger.Info(String.Format("Plan inexistente Nro: {0}  (8)", req.msjIda.plan))
                End If
            End If

            Try
                Logger.Info("PinPad - Buscando PlanEmisor: " + req.planemisor)
            Catch
            End Try


        Catch ex As Exception
            req.InvalidarReq("Fallo en servidor: Reintente mas tarde. (8)")
            Logger.Fatal("Falla con el servidor SQL. Avise a COMPUTOS  (8)")
        End Try
    End Sub

    Public Sub BuscarPlan(req As Req)
        Dim planesTA As New DatosTjTableAdapters.PLANESTableAdapter
        Dim tabla As New DatosTj.PLANESDataTable
        Try
            planesTA.Fill(tabla)
            If req.ida.PLANINT >= 0 And req.emisor > 0 Then
                Dim fila As DatosTj.PLANESRow = tabla.FindByEmisorNroPlan(req.emisor, req.nroPlan)
                If fila IsNot Nothing Then
                    With req
                        .cuotas = fila.Cuotas
                        .moneda = fila.Moneda
                        .descPlan = fila.DescPlan
                        .coeficiente = fila.Coeficiente
                        .planemisor = fila.PlanEmisor
                    End With
                Else
                    req.InvalidarReq("INVALIDA: Plan o Emisor inexistente (9)")
                    Logger.Info(String.Format("Plan inexistente Nro: {0}  (9)", req.ida.PLANINT))
                End If
            End If
        Catch ex As Exception
            req.InvalidarReq("Fallo en servidor: Reintente mas tarde. (9)")
            Logger.Fatal("Falla con el servidor SQL. Avise a COMPUTOS (9).")
        End Try
    End Sub



    Public Function Ultimo_Ticket(hst As Byte, term As String) As Integer

        Try
            Dim ticket As Integer = 0
            Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
            Dim result = From c In dt.GetData()
                         Where c.NroHost = hst And Trim(c.Terminal) = Trim(term)
                         Select c
            For Each reg In result
                ticket = reg.NroTicket
            Next
            If ticket = 0 Then
                Logger.Warn(String.Format("Ticket 0 para la terminal {0}/{1}", hst.ToString, Trim(term)))
            End If

            Return ticket
        Catch ex As Exception
            Return 0
            Logger.Warn(String.Format("Ticket 0 para la terminal {0}/{1}", hst.ToString, Trim(term)))
        End Try

    End Function

    Public Function Ultimo_Trace(hst As Byte, term As String) As Integer
        Try
            Dim trace As Integer = 0
            Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
            Dim result = From c In dt.GetData()
                         Where c.NroHost = hst And Trim(c.Terminal) = Trim(term)
                         Select c
            For Each reg In result
                trace = reg.NroTrace
            Next
            If trace = 0 Then
                Logger.Warn(String.Format("Trace 0 para la terminal {0}/{1}", hst.ToString, Trim(term)))
            End If
            Return trace
        Catch ex As Exception
            Return 0
            Logger.Warn(String.Format("Trace 0 para la terminal {0}/{1}", hst.ToString, Trim(term)))
        End Try

    End Function

    'Public Function Ultimo_TraceAdvice(hst As Byte, term As String) As Integer
    '    Try
    '        Dim trace As Integer = 0
    '        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
    '        Dim result = From c In dt.GetData()
    '                     Where c.NroHost = hst And Trim(c.Terminal) = Trim(term)
    '                     Select c
    '        For Each reg In result
    '            trace = reg.NroTraceAdv
    '        Next
    '        If trace = 0 Then
    '            Logger.Warn(String.Format("Trace Advice 0 para la terminal {0}/{1}", hst.ToString, Trim(term)))
    '        End If
    '        Return trace
    '    Catch ex As Exception
    '        Return 0
    '        Logger.Warn(String.Format("Trace Advice 0 para la terminal {0}/{1}", hst.ToString, Trim(term)))
    '    End Try
    'End Function



    Public Function Ultimo_Lote(hst As Integer, term As String) As Integer
        Try
            Dim lote As Integer = 0
            Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
            Dim result = From c In dt.GetData()
                         Where c.NroHost = hst And Trim(c.Terminal) = Trim(term)
                         Select c
            For Each reg In result
                lote = reg.NroLote
            Next
            If lote = 0 Then
                Logger.Warn(String.Format("Lote 0 para la terminal {0}/{1}", hst.ToString, Trim(term)))
            End If
            Return lote
        Catch ex As Exception
            Return 0
            Logger.Warn(String.Format("Lote 0 para la terminal {0}/{1}", hst.ToString, Trim(term)))
        End Try

    End Function

    Public Function Modificar_VersionAPP(pinguino As String, caja As String, pHost As Byte, _versionaPP As String) As String
        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where c.NroCajaPropio = pinguino & caja And c.NroHost = pHost
                         Select c

            For Each reg In result
                reg.VersionSoftPP = _versionaPP
                dt.Update(reg)
            Next
            Return "00"
        Catch
            Logger.Warn(String.Format("No se pudo actualizar Version Soft PP de Terminal {0}/{1}. Serie: {2}", pHost.ToString, Trim(pinguino), Trim(caja)))
            Return "XX"
        End Try

    End Function


    Public Function Modificar_seriePP(pTerm As String, pHost As Byte, _seriePP As String, _posMK As Integer) As Boolean
        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where Trim(c.Terminal) = Trim(pTerm) And c.NroHost = pHost
                         Select c

            For Each reg In result
                reg.SeriePP = _seriePP
                reg.PosicionMK = _posMK
                dt.Update(reg)
            Next
            Return True
        Catch
            Logger.Warn(String.Format("No se pudo actualizar Serie PP de Terminal {0}/{1}. Serie: {2}", pHost.ToString, Trim(pTerm), Trim(_seriePP)))
            Return False
        End Try

    End Function


    Public Sub Incrementar_Lote(pTerm As String, pHost As Byte)
        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where Trim(c.Terminal) = Trim(pTerm) And c.NroHost = pHost
                         Select c

            For Each reg In result
                If reg.NroLote >= 999 Then
                    reg.NroLote = 1
                Else
                    reg.NroLote = reg.NroLote + 1
                End If
                dt.Update(reg)
            Next
        Catch
            Logger.Error(String.Format("No se pudo incrementar el nro de lote. Term: {0}/{1}", pHost.ToString, pTerm.ToString))
        End Try

    End Sub

    Public Sub Incrementar_Trace(pTerm As String, pHost As Byte)
        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where Trim(c.Terminal) = Trim(pTerm) And c.NroHost = pHost
                         Select c

            For Each reg In result
                If reg.NroTrace >= 999999 Then
                    reg.NroTrace = 1
                Else
                    reg.NroTrace = reg.NroTrace + 1
                End If
                dt.Update(reg)
            Next
        Catch
            Logger.Warn(String.Format("No se pudo incrementar el trace. Terminal {0}/{1}", pHost.ToString, Trim(pTerm)))
        End Try
    End Sub

    'Public Sub Incrementar_TraceAdvice(pTerm As String, pHost As Byte)
    '    Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
    '    Try
    '        Dim result = From c In dt.GetData()
    '                     Where Trim(c.Terminal) = Trim(pTerm) And c.NroHost = pHost
    '                     Select c
    '        For Each reg In result
    '            If reg.NroTraceAdv >= 999999 Then
    '                reg.NroTraceAdv = 1
    '            Else
    '                reg.NroTraceAdv = reg.NroTraceAdv + 1
    '            End If
    '            dt.Update(reg)
    '        Next
    '    Catch
    '        Logger.Warn(String.Format("No se pudo incrementar el trace Advice. Terminal {0}/{1}", pHost.ToString, Trim(pTerm)))
    '    End Try
    'End Sub



    Public Sub IncrementarNroTicket(pTerm As String, pHost As Byte)
        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where Trim(c.Terminal) = Trim(pTerm) And c.NroHost = pHost
                         Select c

            For Each reg In result
                If reg.NroTicket >= 9999 Then
                    reg.NroTicket = 1
                Else
                    reg.NroTicket = reg.NroTicket + 1
                End If
                dt.Update(reg)
            Next
        Catch
            Logger.Warn(String.Format("No se pudo incrementar el trace. Terminal {0}/{1}", pHost.ToString, Trim(pTerm)))
        End Try

    End Sub

    Public Sub DisminuirNroTicket(pTerm As String, pHost As Byte)

        Dim dt As New DatosTjTableAdapters.NUMEROSTableAdapter
        Try
            Dim result = From c In dt.GetData()
                         Where Trim(c.Terminal) = Trim(pTerm) And c.NroHost = pHost
                         Select c

            For Each reg In result
                If reg.NroTicket = 0 Then
                    reg.NroTicket = 9999
                Else
                    reg.NroTicket = reg.NroTicket - 1
                End If
                dt.Update(reg)
            Next
        Catch
            Logger.Warn(String.Format("No se pudo incrementar el trace. Terminal {0}/{1}", pHost.ToString, Trim(pTerm)))
        End Try

    End Sub
#End Region



    Public Sub LeerHostsdesdeDB(srv As ServerTar)
        Logger.Info("Leyendo Hosts")
        srv.Hosts.Clear()
        Dim dtj As New DatosTjTableAdapters.HOSTSTableAdapter
        For Each HostRow As DatosTj.HOSTSRow In dtj.GetData.Rows
            If HostRow.TipoHost <> "H" And HostRow.TipoHost <> "P" Then
                Throw New Exception("Tipo de host dentro de la tabla HOSTS incorrecto. Verifique")
            End If
            If Configuracion.TipoConfiguracion = Configuracion.Produccion Then
                If HostRow.TipoHost = "H" Then Continue For
            Else
                If HostRow.TipoHost = "P" Then Continue For
            End If

            Dim otro As Boolean = False
            Dim host As HostTCP
            Dim clave = HostRow.Descripcion.Trim.ToUpper

            If Configuracion.TipoConexion = Configuracion.Mpls Or Not HostRow.PermiteVPN Then 'mpls
                If HostRow.Puerto1 > 65000 Or HostRow.Puerto1 <= 0 Then
                    Logger.Warn("Puerto erróneo verifique " & HostRow.Descripcion)
                    otro = True
                End If
                If HostRow.Direccion1.Trim = "" Then
                    Logger.Warn("Dirección errónea verifique " & HostRow.Descripcion)
                    otro = True
                End If
                If Not otro Then
                    host = New HostTCP(HostRow.NroHost, HostRow.Direccion1.Trim, HostRow.Puerto1, srv)
                    srv.Hosts.Add(clave, host)
                End If
            Else 'vpn
                If HostRow.Puerto2 > 65000 Or HostRow.Puerto2 <= 0 Then
                    Logger.Warn("Puerto erróneo verifique " & HostRow.Descripcion)
                    otro = True
                End If
                If HostRow.Direccion2.Trim = "" Then
                    Logger.Warn("Dirección errónea verifique " & HostRow.Descripcion)
                    otro = True
                End If
                If Not otro Then
                    host = New HostTCP(HostRow.NroHost, HostRow.Direccion2.Trim, HostRow.Puerto2, srv)
                    srv.Hosts.Add(clave, host)
                End If
            End If
        Next
    End Sub


End Module
