
DEFINT A-Z
DECLARE FUNCTION cashback% (rr AS ANY)
DECLARE FUNCTION visaOn% (R AS ANY)
DECLARE SUB GETR (Fi%, BYVAL recn%, RZ AS ANY, encflag AS INTEGER)
DECLARE SUB putR (Fi%, BYVAL recn%, RZ AS ANY, encflag AS INTEGER)
DECLARE SUB rjinitialize ()
DECLARE SUB decryptr (DAT AS STRING, IDA AS ANY)
DECLARE SUB encryptr (DAT AS STRING, IDA AS ANY)
DECLARE FUNCTION encryptStr (DAT AS STRING, sKEY AS STRING) AS STRING
DECLARE FUNCTION DEcryptStr (DAT AS STRING, sKEY AS STRING) AS STRING
DECLARE SUB DECRYPTIDA (DAT AS STRING, IDA AS ANY)
DECLARE FUNCTION PLAN% (NP%, NE%, NC%, NPO%, NM AS STRING, NCO AS STRING, PLD$, cx AS DOUBLE)
DECLARE SUB writerev (rx AS ANY)
DECLARE FUNCTION CHECKLAST% ()
DECLARE SUB WRITELAST ()
DECLARE SUB MakeEchoTEST (K$, IDHOST%)
' falta cierre segun emisor
'       limite de venta por comercio
DECLARE FUNCTION writevta% (idre%)
DECLARE FUNCTION fechamal% (K$)
DECLARE SUB opcierre (BYVAL modo%)
DECLARE SUB RESPINVAL (K$)
DECLARE SUB MakeTicket (TMP AS ANY)
DECLARE SUB StoreOffLine (nhost%)
DECLARE SUB GENVTA ()
DECLARE SUB BORRAR (K$)
DECLARE SUB RECIBIR (K$, Cid%, infox%)
DECLARE SUB READHOST (nhost%, NCAJ%, modo%, tick%, lote%, trac AS LONG)
DECLARE SUB LoadEmisor ()
DECLARE SUB WritePend ()
DECLARE SUB espera (demora AS SINGLE)
DECLARE SUB cHECKVTAS ()
DECLARE SUB erro (K$)
DECLARE SUB GETEMISOR ()
DECLARE SUB getOnLine (nhost%)
DECLARE SUB InitVars ()
DECLARE SUB Loadreq ()
DECLARE SUB mensaje (K$)
DECLARE SUB OpenFileSys ()
DECLARE FUNCTION SendOffLine% (nhost%)
DECLARE FUNCTION CheckForExit% ()
DECLARE FUNCTION ReqReverso$ (host%, req AS ANY)
DECLARE FUNCTION SendonLine% (nhost%)
DECLARE FUNCTION GetIda% ()
DECLARE FUNCTION Checkrange% ()
DECLARE FUNCTION Chequeacom% (host%)
DECLARE FUNCTION DIGITO% (TARJE$, NROEMI%)
DECLARE FUNCTION MENRESP$ (codrta%, AUTNUM AS LONG, Realizada%, Sacar%, Interno%)
OPTION EXPLICIT
REM $INCLUDE: 'x25lib.bi'
REM $INCLUDE: 'PINC.BI'
REM $INCLUDE: 'conce.bi'
DIM SHARED outofline, cx AS DOUBLE
DIM SHARED nci AS INTEGER
DIM SHARED ComOn, t, ncajdir, cajcur, curd$, WaitAbort AS LONG
DIM ncit AS INTEGER
' -------------- DIMS ----------
REM $INCLUDE: 'PINCD.BI'
CONST pendat = "CONCE.PEN"
CONST ncif = "CONCE.NCI"
REDIM resp$(65)
DIM SHARED REVERSAR AS INTEGER
DIM i AS INTEGER, Nochk, demora  AS SINGLE, K$, orden$
DIM SHARED CAUSA$, Y, cajd$(10), pin(10), RESETIT AS LONG, CODSAL, viel
' PLANES
DIM SHARED MONEF, PLANARC, CUOTF, PLANF, EMISF, COMEF, DESEF, maxscr, COEFF
DIM notauto, TMP@, tt AS SINGLE, KZ1T, KZ2T, KZ1$
DIM emx AS INTEGER
CONST EMI_CABAL = 4
CONST EMI_CABAL2 = 10
CONST EMI_MASTER = 2
CONST EMI_MASTER2 = 3
maxscr = 50
IF INSTR(COMMAND$, "/25") > 0 THEN maxscr = 25
WIDTH , maxscr
CLS
REM $INCLUDE: 'encr.bi'

ON ERROR GOTO sonamos
LOCATE 1, 1: COLOR 0, 7:
PRINT SPACE$(160); : LOCATE 1, 1
PRINT "    CONCENTRADOR DE OPERACIONES"
LOCATE 2, 1: PRINT "OPCIONES:   (ESC) Termina (C)ierre "
COLOR 7, 0
VIEW PRINT 3 TO maxscr
PRINT "Memoria:"; FRE(-1); " Stack:" + STR$(STACK) + STR$(FRE(-2))
Nochk = INSTR(COMMAND$, "/NOCHK") = 0
Notest = INSTR(COMMAND$, "/NOTEST") > 0
INITIALIZE
debug = -1
ComOn = INSTR(COMMAND$, "/NOCOM") = 0
IF ComOn THEN
	MemX25 = 8000
	Inicializar
END IF

PRINT "ENCRYPTER VERSION (C) 2004 by Vimarx Software"
' ******** APERTURA DE ARCHIVOS *******
rjinitialize
InitVars
OpenFileSys
LoadEmisor
	GOSUB READNCI

	K$ = "CONCE.DIR"
		'mensaje "LEYENDO DIRECTORIOS DE TRABAJO " + K$
		IF DIR$(K$) = "" THEN
		ncajdir = 1
		demora = .4
		IF ENVIRON$("CAJADIR") <> "" THEN
			cajd$(1) = ENVIRON$("CAJADIR")
		END IF
	ELSE
		t = FREEFILE
				' mensaje "abre " + K$
		OPEN K$ FOR INPUT AS t
			INPUT #t, ncajdir, demora
			FOR i = 1 TO ncajdir
				 INPUT #t, cajd$(i), pin(i)
			NEXT
		CLOSE t
				'mensaje "cierra " + K$
	END IF


		'leer visaelectron
		DIM SHARED MAXELE AS INTEGER
		DIM SHARED VEBIN() AS LONG
		DIM SHARED NVE AS INTEGER
		DIM VE AS STRING
				MAXELE = 2000

		VE$ = "VISA.BIN"
		IF DIR$(VE$) = "" THEN
			 erro "NO EXISTE " + VE$
		ELSE
			 REDIM VEBIN(MAXELE) AS LONG
			 t = FREEFILE
			 NVE = -1
			 OPEN VE$ FOR INPUT AS t
				   DO WHILE NOT EOF(t)
						LINE INPUT #t, K$
						NVE = NVE + 1
						IF NVE = MAXELE THEN EXIT DO
						VEBIN(NVE) = VAL(MID$(K$, 1, 10))
				   LOOP
			 CLOSE t
						 mensaje "VISA ELECTRON:" + STR$(NVE)
		END IF

	' INTENTAMOS ON-LINE
		IF INSTR(COMMAND$, "/TEST") THEN
                	FOR i = 1 TO MaxHosts
				SELECT CASE i
					CASE 1
						ONLINE(i) = Chequeacom(i)
					CASE ELSE
						ONLINE(i) = -1
				END SELECT
		        NEXT
		ELSE
         		FOR i = 1 TO MaxHosts
			    ONLINE(i) = Chequeacom(i)
                        NEXT

		END IF
	IF ONLINE(1) = -1 AND ONLINE(2) <> -1 THEN
		 ' visa
		erro " ERROR DE COMUNICACION LLAME A:"
		erro "visa: 01-379-3300 sector tecnologia "
		erro "     fuera de hora llame a autorizaciones"
		erro "DIGA que poseemos un enlace x-25 al 4171026124119"
	ELSEIF ONLINE(1) <> -1 AND ONLINE(2) = -1 THEN
		' posnet
		erro " ERROR DE COMUNICACION LLAME A:"
		erro "POSNET : 01-331-2088 INTERNOS 1047/1048 LAS 24 HS"
	ELSEIF ONLINE(1) <> -1 AND ONLINE(2) <> -1 THEN
		'startel
		erro " ERROR DE COMUNICACION GENERAL, LLAME A:"
		erro "ARNET: 0800- NRO :44920017 LAS 24 HS"
		erro "DIGA que poseemos TENEMOS PROBLEMAS CON LA LINEA X-25 NRO 44920017"
	ELSE
		' todo ok
	END IF

	'DIM rr AS reqinfo
	' ACA SE BORRARIAN LOS DATOS DE CIERRES ANTERIORES

	' OPCIERRE rr, 2
	NVTA = 0
	cajcur = 0
	IF INSTR(COMMAND$, "/NOSAL") = 0 THEN RESETIT = TIMER + 60 * 40' salida 20 mins automatica
DO
rearranque:
		cajcur = cajcur + 1
		IF cajcur > ncajdir THEN cajcur = 1
		cajadir = cajd$(cajcur)

		notauto = -1
		orden$ = INKEY$
		IF cajcur = 1 THEN
			IF DIR$(cajadir + "CONCE.ord") <> "" THEN
				 espera .2
				  notauto = FREEFILE
				  mensaje "abriendo orden:" + cajadir + "CONCE.ORD"
				  
				  OPEN cajadir + "CONCE.ord" FOR INPUT SHARED AS notauto
					IF EOF(notauto) THEN
						orden$ = ""
						R.sufijo = ""
					ELSE
						INPUT #notauto, orden$, R.sufijo
					END IF
					IF NOT EOF(notauto) THEN INPUT #notauto, R.host
				  CLOSE notauto
				  BORRAR cajadir + "CONCE.ord"
				  BORRAR "conce.cit"
				  notauto = 0
				  outofline = -1
				  demora = .05
				  RESETIT = 60 * 40 + TIMER

			END IF
		END IF
		' OPCIONES DE TECLADO
		SELECT CASE UCASE$(orden$)
			CASE "L", ":"' lista lote sin cerrar
				IF CheckForExit = 0 THEN
					GOSUB decileqespere
				ELSE
						GOSUB getcaja
						R.MANUAL = notauto
						READHOST R.host, VAL(R.sufijo), 0, R.TICKET, R.lote, R.trace
						R.idter = FldData$(TERNUM)
						GOSUB getidcie
						IF UCASE$(orden$) = "L" THEN opcierre 8
						IF UCASE$(orden$) = ":" AND INSTR(COMMAND$, "/ARRE") THEN opcierre 14
				END IF
			CASE "B" 'borra pantalla
				CLS
			CASE "P" ' ping
				GOSUB getcaja
				GOSUB CHECK800
			CASE "C"
				IF CheckForExit = 0 THEN
					GOSUB decileqespere
				ELSE
						GOSUB getcaja
						R.MANUAL = notauto
						READHOST R.host, VAL(R.sufijo), 0, R.TICKET, R.lote, R.trace
						R.idter = FldData$(TERNUM)
						GOSUB getidcie
						opcierre 3
					   'CODSAL = 0
					   'EXIT DO
				END IF
			CASE "D", "O", "-"'cierre con diferencia
				IF INSTR(COMMAND$, "/ARRE") THEN
					IF CheckForExit = 0 THEN
						GOSUB decileqespere
					ELSE
							GOSUB getcaja
							R.MANUAL = notauto
							READHOST R.host, VAL(R.sufijo), 0, R.TICKET, R.lote, R.trace
							R.idter = FldData$(TERNUM)
							GOSUB getidcie
							IF UCASE$(orden$) = "D" THEN opcierre 5
							IF UCASE$(orden$) = "O" THEN opcierre 13
							IF UCASE$(orden$) = "-" THEN
								R.host = R.host - 1
								opcierre 13
							END IF
						 '   CODSAL = 0
						 '   EXIT DO
					END IF
				ELSE
					erro "MODO SOLO VALIDO EN /ARRE"
				END IF
			CASE "x", "X"
				Pendiente = Pendiente + 1
				R = Pend(Pendiente).R
				opcierre 12
						CASE "Q"
								REVERSAR = 1
								mensaje "REVERSADO ACTIVADA"
						CASE "R"
				CODSAL = 10
				IF CheckForExit THEN EXIT DO
			CASE CHR$(27), "S"
				CODSAL = 3
				IF CheckForExit THEN EXIT DO
			CASE "T", "#"
				IF CheckForExit = 0 THEN
					GOSUB decileqespere
				ELSE
						mensaje "Ventas suspendidas durante Cierre .."
						GOSUB getcaja
						R.sufijo = "0999"
						R.MANUAL = notauto
						READHOST R.host, VAL(R.sufijo), 0, R.TICKET, R.lote, R.trace
						R.idter = FldData$(TERNUM)
						GOSUB getidcie
						outofline = -1
						demora = .05
						IF UCASE$(orden$) = "T" THEN
							opcierre 10
						ELSE
							opcierre 11
						END IF
						nci = nci + 1
						IF nci > 99 THEN nci = 0
						GOSUB WRITENCI

				END IF
						CASE "F"
								IF ONLINE(2) THEN
								   ONLINE(2) = 0
   								   mensaje "MODO OFFLINE"
								ELSE
								   ONLINE(2) = 1
								   mensaje "MODO ONLINE"
								END IF
						CASE "K"
				CODSAL = 8
				IF CheckForExit THEN EXIT DO
		END SELECT

		IF RESETIT > 0 AND TIMER > RESETIT THEN
			  mensaje "Salida Automatica"
			  CODSAL = 2
			  IF CheckForExit THEN EXIT DO
		END IF

			'ESTA ESPERA ES PARA NO SOBRECARGAR LA RED
			espera demora

			Y = CSRLIN
			LOCATE 3, 1
			COLOR 0, 7
			PRINT SPACE$(80);
			LOCATE 3, 1

			PRINT "Memoria:"; FRE(-1); " Stack:" + STR$(STACK) + STR$(FRE(-2)), cajadir; " P:"; Pendiente; " VP:"; NVTAP; cajcur
			LOCATE Y, 1
			COLOR 7, 0
			' cargo requerimientos en IDA()
			IF outofline THEN
			ELSE
				Loadreq
			END IF
			IF NIDA > 0 THEN
				' leer IDA() en IDAREG
				IF GetIda THEN GOTO ELQUESIGUE

				' GUARDAR EL NOMBRE DEL ARCHIVO
				R.TKID = IDAREG.TKID
				R.t1req = 0
				'R.sufijo = FORMAT$(pin(cajcur), "00") + MID$(IDA(NIDA), LEN(CAJANOM) + 3, 2)
				R.sufijo = FORMAT$(pin(cajcur), "00") + MID$(IDA(NIDA), LEN(R.TKID) + 3, 2)
				' TRANSFERENCIA IDAREG=>R
				R.TICKET = 0
				R.CONFIR = 0 ' SIN CONFIRMAR
				R.oper = IDAREG.oper
				R.cmd = IDAREG.cmd
				R.TARJ = IDAREG.TARJ
				R.IMPO = INT(IDAREG.IMPO * 100 + .5@) / 100@
				R.FECCAP = FORMAT$(IDAREG.HORA, "mmdd")
				R.EXPDATE = IDAREG.EXPDATE
				R.fechahora = NOW
				R.PlanInt = IDAREG.PlanInt

				R.CUOTAS = 1 ' SALE DEL PLANINT
				R.MONEDA = "P"
				R.PLAN = 0

				R.TICCAJ = IDAREG.TICCAJ
				R.PLANINTori = IDAREG.PLANINTori

				R.MANUAL = IDAREG.MANUAL
				R.TRACK2 = IDAREG.TRACK2
				R.TRACK1 = IDAREG.TRACK1
				R.CODSEG = IDAREG.CODSEG
				R.RETREF = ""
				R.CodAut = IDAREG.CodAut
				IF R.CodAut < "000000" THEN R.CodAut = "000000"
				R.TKTORI = IDAREG.TKTORI MOD 10000
				R.FECORI = RTRIM$(IDAREG.FECORI) + "000000"
				R.CAJERA = IDAREG.CAJERA
				R.PLANORI = 0
				R.CTAORI = 0
				R.MONORI = "P"
                                R.CASHBACK=IDAREG.CASHBACK
				R.fecenv = 0 'fecha zero?
				R.host = 1    ' TEMPORARIAMENTE
				R.cajadir = ztrim$(IDAREG.cajadir)

				' maestro procesing
				DIM MAESTRO AS INTEGER
				MAESTRO = R.PlanInt >= 990 AND R.PlanInt <= 992
				IF MAESTRO THEN
					 R.tipocta = VAL(MID$(R.CODSEG, 17, 1))
					 R.PINBLOCK = MID$(R.CODSEG, 1, 16)
					 mensaje "Operatoria Maestro " + R.PINBLOCK + " " + STR$(R.tipocta)
					 IF R.PlanInt = 990 THEN R.oper = 4   'SOLICITUD DE SALDO ES EL 4
				ELSE
					R.tipocta = 0
					R.PINBLOCK = "0000000000000000"
				END IF

				

			   'IF R.oper > OPANUDEV OR RTRIM$(R.TARJ) = "1234567890123456" THEN
			   '    ' POR AHORA HOST=1
			   '    R.MANUAL = 0
			   '    IF R.oper = 6 THEN
			   '        Opcierre 5
			   '       ' status = writevta
			   '    ELSE
			   '        Opcierre 3
			   '         VTAREG.OK = -1
			   '         VTAREG.Mensaje = "cierre"
			   '         status = writevta(0)
			   '    END IF
			   '    GOTO ELQUESIGUE
			   'END IF

				IF R.oper = OPDEVOL THEN
				   IF fechamal(R.FECORI) THEN
				     CAUSA$ = "Fecha Orig Incorrecta"
				     GOTO INVALIDA
		                   END IF
				END IF
				curd$ = cajadir
			  ' IF Nochk AND DIR$(cajadir + "nochk") <> "" THEN
			  '      r.HOST = 1
			  '      Mensaje "sin chequeo interno"
			  '      GOTO ENVIAR
			  '  END IF

				' Leer EMIHAB,Larran,EmiInH

				IF Checkrange THEN
					'CAUSA$ = "No en rango"
					GOTO INVALIDA
				END IF

				IF R.PlanInt = 0 AND R.PLANINTori > 0 THEN R.PlanInt = R.PLANINTori
				IF PLAN(R.PlanInt, EMIHAB, R.CUOTAS, R.PLAN, R.MONEDA, R.Idcom, R.PLANID, cx) THEN
				  CAUSA$ = "PLAN INVALIDO" + STR$(R.PlanInt)
				  GOTO INVALIDA
				ELSE
				  IF R.PlanInt > 0 THEN mensaje "Plan:" + STR$(R.PlanInt) + " cuotas:" + STR$(R.CUOTAS) + " moneda:" + R.MONEDA + " " + R.PLANID
				END IF
				IF PLAN(R.PLANINTori, EMIHAB, R.CTAORI, R.PLANORI, R.MONORI, R.Idcom, "", 0) THEN
				  CAUSA$ = "PLAN ORIG INVALIDO" + STR$(R.PLANINTori)
				  GOTO INVALIDA
				END IF


				' EMITAB(EMIHAB) CONTIENE EL EMISOR DE LA TARJETA
				' VALIDACION DE DIGITO Y LARGO
				IF R.oper <> OPCOMPRA AND MAESTRO = 0 THEN
					IF R.TKTORI = 0 THEN
						  CAUSA$ = "Falta Tkt Orig"
						  GOTO INVALIDA
					END IF
				END IF


				IF R.oper = OPANUCPA OR R.oper = OPANUDEV THEN
                                ELSE
				    IF MAESTRO = 0 AND LTRIM$(R.CODSEG) = "" AND EMITAB(EMIHAB).S1 <> 0 AND viel = 0 THEN
					 CAUSA$ = "Requiere.cod.seg."
				         GOTO INVALIDA
                                    END IF
				END IF

				IF DIGITO(NROTMP$, EMIHAB) = 0 THEN
					CAUSA$ = "TARJETA INV:DIGITO O LARGO"
					GOTO INVALIDA
				END IF

				' VALIDACION DE LA FECHA DE VENCIMIENTO DE LA TARJETA
				vto = MID$(R.EXPDATE, 1, 2) + MID$(R.EXPDATE, 3, 2)
				IF MAESTRO = 0 THEN
				 'F MID$(R.EXPDATE, 1, 2) < "50" THEN
					IF "20" + vto < FORMAT$(NOW, "yyyymm") THEN
						  CAUSA$ = "TARJETA VENCIDA " + R.EXPDATE
						  GOTO INVALIDA
					END IF
				' ELSE
				'    IF "19" + vto < FORMAT$(NOW, "yyyymm") THEN
				'          CAUSA$ = "TARJETA VENCIDA " + R.EXPDATE
				'          GOTO INVALIDA
				'    END IF
								' END IF
				END IF

				R.host = EMITAB(EMIHAB).host
				R.t1req = EMITAB(EMIHAB).T1
				emx = EMITAB(EMIHAB).NUMERO
							 '   if emx = 3 or emx =2 or emx=13 or emx=20 then
							 '        if val(r.sufijo)>700 and val(r.sufijo)<800 then
							 '            MENSAJE "VISA/MASTER ACEPTADA "+R.SUFIJO
							 '        else
							 '            CAUSA$ = "TARJETA NO OPERATIVA " + STR$(EMX)
							 '            GOTO INVALIDA
							 '        endif
							 '
							 '   endif

				' chequeo de limites
				' guarda que esta dos veces
				IF ONLINE(R.host) = -1 THEN

				ELSE
					IF R.IMPO > EMITAB(EMIHAB).LIMVTA AND VAL(R.CodAut) = 0 THEN
						  IF EMITAB(EMIHAB).LIMVTA < 1 THEN
							CAUSA$ = "Pida Cod. Autoriz."
						  ELSE
							CAUSA$ = "Supera Lim.Vta /Req.aut."
						  END IF
						  GOTO INVALIDA
					END IF
				END IF

ENVIAR:
				READHOST R.host, VAL(R.sufijo), 0, R.TICKET, R.lote, R.trace
				R.idter = FldData$(TERNUM)

				' calcular codigo de cierre de comercio
				GOSUB getidcie

				IF R.idter < "0" THEN
					  CAUSA$ = "CAJA " + R.sufijo + " NO HAB. EN HOST" + STR$(R.host)
					  GOTO INVALIDA
				END IF

				IF R.oper = OPANUCPA OR R.oper = OPANUDEV THEN
					  CAUSA$ = ""
					  opcierre 6
					  IF R.TKTORI = 0 THEN
	                                      GOTO INVALIDA
					  END IF
				END IF

				IF CHECKLAST AND R.oper <> OPANUCPA AND 0 THEN
				   erro "OPER.REPETIDA-Cambie Importe"
				   CAUSA$ = "OPERACION REPETIDA"
				   GOTO INVALIDA

				 ' COPIAR TICKETS Y VTA
                                   KZ1$ = "TICK" + R.sufijo + ".DAT"
				   GOSUB MANDALOS
				'     KZ1$ = CAJANOM + MID$(R.sufijo, 1, 4) + VTAEXT
                                   KZ1$ = R.TKID + MID$(R.sufijo, 1, 4) + VTAEXT
				   GOSUB MANDALOS

				   GOTO ELQUESIGUE
				END IF
				' PROCESO COEFICIENTE PLANES
				'R.MCOEF = R.IMPO
				R.COEF = cx
				IF cx > 1 THEN
					R.IMPO = INT(R.IMPO * cx * 100 + .5) / 100
					'mensaje "USA COEF: " + STR$(R.MCOEF) + "x" + STR$(cx) + "=" + STR$(R.IMPO)
				END IF

				IF R.oper = OPANUCPA OR R.oper = OPANUDEV THEN R.CODSEG = ""

				IF ONLINE(R.host) = -1 AND ((R.cmd AND 32) = 0) THEN
				   ' ENVIAR PAQUETE
					IF SendonLine(R.host) THEN
						mensaje "Online->offline"
						GOTO mandaroff
					END IF
				ELSE
mandaroff:
										R.CODSEG = ""
					StoreOffLine (R.host)
				END IF

				GOTO ELQUESIGUE
INVALIDA:
								R.CODSEG = ""
				mensaje "INVALIDA:" + CAUSA$
				RESPINVAL CAUSA$
ELQUESIGUE:
				GOSUB BORRAIDA
ELQ2:
				NIDA = NIDA - 1
			END IF

	' VERIFICAR TODOS LOS HOSTS A VER SI HAY RESPUESTA
	IF Pendiente OR (TIMER < WaitAbort) THEN
		FOR i = 1 TO MaxHosts
			IF ONLINE(i) <> 0 THEN getOnLine i
		NEXT
		IF TIMER < WaitAbort THEN
			mensaje "Intentando Reconexion en 2 minutos"
			RESETIT = TIMER + 60 * 2
			WaitAbort = 0
		END IF
	END IF

	cHECKVTAS

	'
	' ------------ FIN DE GRABACION DE VTAS
	'
LOOP

salir:
ON ERROR GOTO 0
' ******  CIERRE DE ARCHIVOS Y SALIDA AL S.O. *****
mensaje "SALIENDO DEL SISTEMA ..."
IF ComOn THEN
	FOR i = 1 TO MaxHosts
		IF ONLINE(i) <> 0 THEN Cortar cIDS(i)
	NEXT
	Finalizar
END IF
setsdat "EXIT", ""
END CODSAL

BORRAIDA:
				BORRAR cajadir + IDA(NIDA)
RETURN

getidcie:
	IF R.host = 1 THEN  'solo para posnet
		TMP@ = VAL(R.sufijo) / 100 - INT(VAL(R.sufijo) / 100) * 100
		IF TMP@ > KDIf THEN
			TMP@ = 10    ' OFFSET DIFERIMIENTOS
		ELSE
			TMP@ = 0
		END IF
		TMP@ = TMP@ + Numval(CIEHOS) + INT(VAL(R.sufijo) / 100@) - 1
		R.IDCIE = FORMAT$(TMP@, "00000000")
	ELSE
		R.IDCIE = FORMAT$(Numval(CIEHOS), "00000000")
	END IF
		 '       IF debug THEN mensaje "Cierre:" + R.IDCIE
RETURN

getcaja:
	IF notauto THEN
		LINE INPUT "Ingrese Nro de Caja a cerrar (ppnn) o enter para cerrar todas:"; R.sufijo
		 INPUT "Ingrese Nro de Host :"; R.host
	END IF
RETURN

CHECK800:
		MakeEchoTEST K$, (R.host)
		mensaje decodeRTa$(K$, resp$())

		tt = ENVIAR(K$, cIDS(R.host))
		mensaje "Esperando respuesta a echo test"
		tt = TIMER
		DO
			RECIBIR K$, cIDS(R.host), 0
		LOOP UNTIL K$ <> "" OR TIMER - tt > 17
		IF K$ <> "" THEN
			mensaje "Host " + STR$(R.host) + " On line"
			IF debug THEN
				mensaje decodeRTa$(K$, resp$())
				mensaje "-----------"
			END IF
		ELSE
			mensaje "No hay Respuesta -> offline"
			ONLINE(R.host) = -2
		END IF
		GOSUB grabvac
RETURN

grabvac:
		tt = FREEFILE
		OPEN "I:\tarjetas\conce.cit" FOR OUTPUT AS tt
		CLOSE tt
RETURN

decileqespere:
	mensaje "EXISTEN OPERACIONES PENDIENTE, ESPERE"
	GOSUB grabvac
RETURN

MANDALOS:
	DIM KZ2$
	IF DIR$(KZ1$) = "" THEN
		erro "NO SE ENCONTRO TICKET " + KZ1$ + "PARA RETORNAR"
		RETURN
	END IF
	KZ1T = FREEFILE
	OPEN KZ1$ FOR BINARY AS KZ1T
	KZ2T = FREEFILE
	IF LEFT$(KZ1$, LEN(R.TKID)) = R.TKID THEN
		MID$(KZ1$, LEN(R.TKID) + 1, 2) = "00"
	END IF
	KZ2$ = RTRIM$(R.cajadir) + KZ1$
	OPEN KZ2$ FOR OUTPUT AS KZ2T
		KZ1$ = INPUT$(LOF(KZ1T), KZ1T)
		PRINT #KZ2T, KZ1$;
	CLOSE KZ1T, KZ2T
RETURN

sonamos:
	DIM K3$
	 SELECT CASE ERR
		CASE 68, 57, 72'errores de red
			K3$ = RTRIM$(ERDEV$)
			erro "Error en Red " + K3$ + " " + ERROR$
			IF K3$ = "" THEN K3$ = LEFT$(curd$, 2)
			FOR i = 1 TO ncajdir
				IF UCASE$(K3$) = LEFT$(cajd$(i), LEN(K3$)) THEN
					erro "Se elimina " + cajd$(i)
					SWAP cajd$(i), cajd$(ncajdir)
					SWAP pin(i), pin(ncajdir)
					ncajdir = ncajdir - 1
					EXIT FOR
				END IF
			NEXT
			IF i > ncajdir THEN
					i = cajcur
					erro "Se elimina " + cajd$(i)
					SWAP cajd$(i), cajd$(ncajdir)
					SWAP pin(i), pin(ncajdir)
					ncajdir = ncajdir - 1
			END IF
			cajcur = 0
			RESETIT = TIMER + 600 'saldremos en 10 minutos para requeckear
			RESUME rearranque
		CASE ELSE
			erro "Error Sistema:" + STR$(ERR) + " " + ERROR$
			CODSAL = 1
			RESUME salir
	 END SELECT
END

READNCI:
	  ncit = FREEFILE
		 IF DIR$(ncif) <> "" THEN
		  OPEN ncif FOR INPUT AS ncit
		  IF NOT EOF(ncit) THEN INPUT #ncit, nci
		  CLOSE ncit
	  ELSE
		 nci = 0
	  END IF
		' mensaje "leyendo nci " + STR$(nci)
RETURN

WRITENCI:
		mensaje "grabando nci " + STR$(nci)
	ncit = FREEFILE
	OPEN ncif FOR OUTPUT AS ncit
	PRINT #ncit, nci
	CLOSE ncit
RETURN

REM $DYNAMIC
FUNCTION cashback% (rr AS reqinfo)
IF rr.PlanInt = cashbackid AND visaOn(rr) <> 0 THEN
        cashback = -1
ELSE
MENSAJE "chequeando cashback 0 "+str$(rr.planint)
        cashback = 0
END IF
END FUNCTION

FUNCTION CheckForExit ()
	DIM i AS INTEGER
	CheckForExit = -1
	IF Pendiente THEN
		mensaje "Quedan Pendientes:" + STR$(Pendiente)
		CheckForExit = 0
	END IF
	IF NVTAP THEN
		mensaje "Quedan Pendientes de responder:" + STR$(NVTAP)
		FOR i = 1 TO NVTAP
			PRINT VTAP(i).cajadir; VTAP(i).ARC, VTAP(i).nregi
		NEXT
		CheckForExit = 0
	END IF
	IF WaitAbort > TIMER THEN
		erro "Esperando Respuesta Abortadas"
		CheckForExit = 0
	END IF
END FUNCTION

REM $STATIC
FUNCTION CHECKLAST ()
	DIM TX, kx$, KX2$
	TX = FREEFILE
	CHECKLAST = 0
	kx$ = "CAJA" + R.sufijo + ".TMP"
	IF DIR$(kx$) = "" THEN EXIT FUNCTION
	ON LOCAL ERROR RESUME NEXT
	   ' mensaje "abre " + kx$
	OPEN kx$ FOR INPUT AS TX
		LINE INPUT #TX, KX2$
		kx$ = R.TARJ + STR$(R.IMPO)
		CHECKLAST = kx$ = KX2$
	CLOSE TX

		ON LOCAL ERROR GOTO 0
END FUNCTION

REM $DYNAMIC
FUNCTION Checkrange ()
		GETEMISOR

		IF EMIHAB <> 0 THEN
			' VERIFICAR EN RANGO INHABILITADO
			EMIINH = 0
			Fi(RANGINHA).keybuffer = MKI$(EMIHAB) + LEFT$(NROTMP$, 8)
			RESULTA = BTRVOP(BLESSEQ, RANGINHA, 0)
			IF RESULTA = 0 THEN
				DESDE$ = LEFT$(RTRIM$(Fmtstr$(DESRHA, true)) + "00000000", 8)
				HASTA$ = LEFT$(RTRIM$(Fmtstr$(HASRHA, true)) + "zzzzzzzz", 8)
				IF EMIHAB = Numval(EMIRIN) AND DESDE$ <= LEFT$(NROTMP$, 8) AND HASTA$ >= LEFT$(NROTMP$, 8) THEN
					' NO ES VALIDA,
					EMIINH = Numval(EMIRIN)
					CAUSA$ = NROTMP$ + " fuera de rango " + DESDE$ + " " + HASTA$ + " " + NROTMP$
					Checkrange = -1
					EXIT FUNCTION
				END IF
			END IF

			IF EMIINH = 0 THEN  ' HASTA AHORA SIRVE
				' CARGAR DATOS DEL EMISOR CORRESPONDIENTE
				IF EMIHAB >= 0 AND EMIHAB <= MAXEMI THEN
					mensaje "TARJETA " + EMITAB(EMIHAB).DESCRIP + " NRO:" + NROTMP$ + " DESDE:" + DESDE$ + " A " + HASTA$
				ELSE

					CAUSA$ = "EMISOR FUERA DE RANGO"
					Checkrange = -1
					EXIT FUNCTION
				END IF
			ELSE
			   CAUSA$ = NROTMP$ + " en rango invalido"
				' TARJETA INVALIDA POR RANGO INVALIDO ***
				Checkrange = -1
				EXIT FUNCTION
			END IF
		ELSE
						' TARJETA NO ESTA EN RANGO ***

			Checkrange = -1
			EXIT FUNCTION
		END IF

END FUNCTION

REM $STATIC
SUB cHECKVTAS () 'purgamos las vtas de la POS
	DIM i
Reha:
	FOR i = 1 TO NVTAP
		 curd$ = VTAP(i).cajadir

		 IF DIR$(RTRIM$(VTAP(i).cajadir) + VTAP(i).ARC) <> "" THEN
			IF (NOW - VTAP(i).TIME) * 86400 > mvPT THEN
				erro "Caja " + VTAP(i).ARC + "no ha completado todavia la operacion:" + FORMAT$(NOW - VTAP(i).TIME, "hh:mm:ss")
				' AQUI SE PODRIAN REVERTIR LAS OPERACIONES
				IF (NOW - VTAP(i).TIME) * 86400 > mvPT + 10 THEN
					   erro "Caja " + VTAP(i).ARC + "no respondio:borrado:" + FORMAT$(NOW - VTAP(i).TIME, "hh:mm:ss")
					   ' aca podemos agarrar el ticket y guardarlos
					   BORRAR RTRIM$(VTAP(i).cajadir) + VTAP(i).ARC
					   BORRAR RTRIM$(VTAP(i).cajadir) + "TICK" + VTAP(i).sufijo + ".DAT"

					   SWAP VTAP(i), VTAP(NVTAP)
					   NVTAP = NVTAP - 1

					   GOTO Reha
				END IF
			ELSE
				' seguimos esperando
			END IF
		 ELSE
			   ' CONFIRMAR RECEPCION POR PARTE DE POS
			   ' sufijo,lote,host
			   R.sufijo = VTAP(i).sufijo
			   R.host = VTAP(i).host
			   R.lote = VTAP(i).lote
			   lastreg = VTAP(i).nregi
			   opcierre 7
			   SWAP VTAP(i), VTAP(NVTAP)
			   NVTAP = NVTAP - 1
			   mensaje "Caja " + R.sufijo + " Confirmo operacion"
			   GOTO Reha
		 END IF
	NEXT
END SUB

REM $DYNAMIC
FUNCTION Chequeacom (HOST5) 'RETORNA -1 SI EXISTE X.25 O 0 SINO EXISTE
DIM K$, tt AS SINGLE, infox AS INTEGER, coderr, i
DIM UDAT$, KUDAT$, IKD AS INTEGER
  READHOST HOST5, 0, -1, 0, 0, 0
  IF INSTR(FldData$(DESHOS), "DINERS") THEN
		DINHOS = HOST5
  END IF
  cIDS(HOST5) = -1
  K$ = ""
  IF status = 0 THEN K$ = RTRIM$(FldData$(CALHOS))
  IF ComOn AND K$ <> "" THEN
	FOR i = 1 TO 2
		IF i > 2 THEN
			IF status = 0 THEN
				K$ = RTRIM$(FldData$(CAL2HOS))
			ELSE
				EXIT FOR
			END IF
		END IF

		' USER DATA
		UDAT$ = ""
		IF UDTHOS > 0 THEN
		   KUDAT$ = LTRIM$(RTRIM$(FldData$(UDTHOS)))
		   FOR IKD = 1 TO LEN(KUDAT$) / 2
			   UDAT$ = UDAT$ + CHR$(VAL("&h" + MID$(KUDAT$, IKD * 2 - 1, 2)))
		   NEXT
		END IF
		mensaje "CONECTANDO :" + K$ + " " + KUDAT$

		cIDS(HOST5) = Conectar(K$, -1, UDAT$)
		IF cIDS(HOST5) >= 0 THEN EXIT FOR
		erro "REINTENTANDO"
		espera 2
	NEXT
	ONLINE(HOST5) = cIDS(HOST5) >= 0
	Chequeacom = cIDS(HOST5) >= 0'POR AHORA RETORNO ON-LINE
	IF ONLINE(HOST5) THEN
		MakeEchoTEST K$, HOST5
		mensaje decodeRTa$(K$, resp$())
		mensaje "-----------"

		coderr = ENVIAR(K$, cIDS(HOST5))
		mensaje "Esperando respuesta a echo test"
		tt = TIMER
		DO
			RECIBIR K$, cIDS(HOST5), infox
		LOOP UNTIL K$ <> "" OR TIMER - tt > 17
		IF K$ <> "" THEN
			mensaje "Host " + STR$(HOST5) + " On line"
			IF debug THEN
				mensaje decodeRTa$(K$, resp$())
				mensaje "-----------"
			END IF
		ELSE
			mensaje "No hay Respuesta -> offline"
			Chequeacom = -2
			ONLINE(HOST5) = -2
		END IF
	ELSE
		Chequeacom = -2

		RESETIT = TIMER + 60 * 2
	END IF
  ELSE
	Chequeacom = 0
  END IF
END FUNCTION

REM $STATIC
FUNCTION DIGITO (TARJE$, NROEMI)       ' MODO=0 ISO
DIM DIG, iz, SUMTOT, TMP$, DIGV, LENT, PRE$, REST1$, MODULO$
DIM SUM1DIG, DIG1, MAYOR, DIGVER, DIG1$, j, DIG2, modo
	LENT = LEN(TARJE$)
	IF LENT > EMITAB(NROEMI).HALARNUM + EMITAB(NROEMI).LLEVADIG OR LENT < EMITAB(NROEMI).DELARNUM + EMITAB(NROEMI).LLEVADIG THEN
		DIGITO = 0
		CAUSA$ = "LARGO INCORRECTO"
	ELSE
		modo = EMITAB(NROEMI).DIGITO
		SELECT CASE modo
			CASE 0 ' ISO
				TMP$ = LEFT$(TARJE$, 15)
				DIGV = VAL(MID$(TARJE$, 16, 1))
				LENT = 15
				GOSUB ISO
			CASE 1 ' CABAL
				LENT = 20
				GOSUB CABAL
			CASE 2 ' VISA Y DINERS
				LENT = LEN(TARJE$) - 1
				TMP$ = LEFT$(TARJE$, LENT)
				DIGV = VAL(MID$(TARJE$, LENT + 1, 1))
				GOSUB ISO
			CASE 3 ' CARTA CREDENCIAL
				DIGITO = -1
				' NO POSEE DIGITO VERIFICADOR
		END SELECT
	END IF
EXIT FUNCTION

CABAL:
	' SI QUERES COMPLICARTE LA VIDA PASA POR ACA
	PRE$ = LEFT$(TARJE$, 4)
	IF PRE$ <> "3121" THEN
		  DIGITO = 0 ' NO ES VALIDO POR PREFIJO
		  RETURN
	END IF
	REST1$ = MID$(TARJE$, 5, 12)
	MODULO$ = "971397139713"
	SUM1DIG = 0
	FOR iz = 1 TO LEN(MODULO$)
		DIG1 = VAL(MID$(MODULO$, iz, 1)) * VAL(MID$(REST1$, iz, 1))
		DIG1 = DIG1 MOD 10
		SUM1DIG = SUM1DIG + DIG1
	NEXT
	MAYOR = (INT(SUM1DIG / 10) + 1) * 10
	DIGVER = MAYOR - SUM1DIG
	IF DIGVER <> VAL(MID$(TARJE$, 17, 1)) THEN
		  ' MAL DIG VER 1
		  DIGITO = 0
		  RETURN
	END IF

	IF MID$(TARJE$, 19, 1) = "0" THEN
		  DIGITO = -1
		  RETURN
	END IF

	REST1$ = MID$(TARJE$, 5, 15)
	MODULO$ = "713971397139713"
	SUM1DIG = 0
	FOR iz = 1 TO LEN(MODULO$)
		DIG1 = VAL(MID$(MODULO$, iz, 1)) * VAL(MID$(REST1$, iz, 1))
		DIG1 = DIG1 MOD 10
		SUM1DIG = SUM1DIG + DIG1
	NEXT
	MAYOR = (INT(SUM1DIG / 10) + 1) * 10
	DIGVER = MAYOR - SUM1DIG
	IF DIGVER <> VAL(MID$(TARJE$, 20, 1)) THEN
		  ' MAL DIG VER 2
		  DIGITO = 0
	ELSE
		  DIGITO = -1
	END IF
RETURN

ISO:
			SUMTOT = 0
			FOR iz = 1 TO LENT
				DIG = VAL(MID$(TARJE$, iz, 1))
				IF (iz AND 1) = 1 THEN
					' IMPAR
					SUM1DIG = DIG * 2
				ELSE
					'PAR
					SUM1DIG = DIG
				END IF
				DIG1$ = LTRIM$(STR$(SUM1DIG))
				FOR j = 1 TO LEN(DIG1$)
					DIG2 = VAL(MID$(DIG1$, j, 1))
					SUMTOT = SUMTOT + DIG2
				NEXT
			NEXT
			MAYOR = (INT(SUMTOT / 10) + 1) * 10
			DIGVER = MAYOR - SUMTOT
			IF DIGVER = 10 THEN DIGVER = 0
			DIGITO = DIGV = DIGVER
RETURN
END FUNCTION

SUB GENVTA ()
	VTAREG.Check = CHECKID
	VTAREG.FILETKT = ""
	VTAREG.MONTOP = ""
	VTAREG.EMINAME = R.EMINAME
	VTAREG.mensaje = ""
	VTAREG.MENRESP = ""
	VTAREG.OK = 0
	VTAREG.VERSION = "1"
END SUB

SUB GETEMISOR ()  'USA IDAREG
	DIM VE AS LONG
		' PTO B.
		EMIHAB = 0
		LARRAN = 0
			' DETERMINAR EMISOR
			NROTMP$ = RTRIM$(R.TARJ)
			Fi(RANGHAB).keybuffer = LEFT$(NROTMP$, 6)
			VE = VAL(LEFT$(NROTMP$, 6))                 ' RANGO VI
			REM PRINT "["; Fi(RANGHAB).keybuffer; "]"
			RESULTA = BTRVOP(BLESSEQ, RANGHAB, 0)
			IF RESULTA = 0 THEN
				DESDE$ = LEFT$(RTRIM$(Fmtstr$(DESRHA, true)) + "000000", 6)
				HASTA$ = LEFT$(RTRIM$(Fmtstr$(HASRHA, true)) + "zzzzzz", 6)
				IF DESDE$ <= LEFT$(NROTMP$, 6) AND HASTA$ >= LEFT$(NROTMP$, 6) THEN
					' ESTA BIEN
					EMIHAB = Numval(EMIRHA)
					LARRAN = LEN(RTRIM$(Fmtstr$(DESRHA, true)))
				ELSE
					' si esta mal
					GOTO versimaestro
				END IF
			ELSE
versimaestro:
			   Writehst NROTMP$ + "f1"
			   IF LEFT$(NROTMP$, 6) >= "500000" AND LEFT$(NROTMP$, 6) <= "690000" THEN
					RESULTA = 0
					EMIHAB = 6' maestro
			   END IF
			END IF
			IF EMIHAB <> 0 THEN
			' CHEQUEAR NOMBRE DEL EMISOR
				Fi(EMISOR).keybuffer = MKI$(EMIHAB)
				RESULTA = BTRVOP(BEQUAL, EMISOR, 0)
								viel = 0
				IF RESULTA = 0 THEN
					R.EMINAME = FldData$(DESEMI)
					IF INSTR(R.EMINAME, "VISA") THEN
						 mensaje "buscando rango electron  " + STR$(VE)


						 DIM XVE AS INTEGER
						 FOR XVE = 1 TO NVE
							 IF VE = VEBIN(XVE) THEN
								  mensaje "VISA-ELECTRON"
								  R.EMINAME = "VISA ELECTRON"
									viel = 1
									EXIT FOR
							 END IF
						 NEXT
					END IF
					IF R.EMINAME > "!" THEN
					   ' IF RTRIM$(IDAREG.EMINAME) <> RTRIM$(FldData$(DESEMI)) THEN
						 '   EMIHAB = 0
					   '     CAUSA$ = "NO COINCIDE EMISOR " + IDAREG.EMINAME + " " + FldData$(DESEMI)
					   ' ELSE

					   ' END IF
					END IF
				ELSE
					CAUSA$ = "NO EXISTE EMISOR:" + STR$(EMIHAB)
					EMIHAB = 0
				END IF
			ELSE
				CAUSA$ = "tarjeta " + NROTMP$ + " no esta en rango"
			END IF
END SUB

FUNCTION GetIda ()
		' ABRIR ARCHIVO DE CAJA PARA LEER REQUISITO
		NIDAARC = FREEFILE

		NRETRY = 0
Retry:
		mensaje "LEYENDO " + cajadir + IDA(NIDA)
		ON LOCAL ERROR RESUME NEXT
		OPEN cajadir + IDA(NIDA) FOR BINARY ACCESS READ LOCK READ WRITE AS NIDAARC
		IF ERR > 0 THEN
			  erro "PERMISO DENEGADO " + cajadir + IDA(NIDA)
			  NRETRY = NRETRY + 1
			  IF NRETRY > 10 THEN
					GetIda = -1
					EXIT FUNCTION
			  END IF
			  espera .2
			  GOTO Retry

		END IF
			' LEO REGISTRO
		IF INSTR(UCASE$(IDA(NIDA)), "IDZ") THEN
			' SI ESTA ENCRIPTADO  -------------------
			DIM K AS STRING
			K = INPUT$(LOF(NIDAARC), NIDAARC)
			IF LEN(K) <> 288 THEN
				CLOSE NIDAARC
				erro "IDZ LARGO INVALIDO"
				GOTO ERRORENIDA
			ELSE
				DECRYPTIDA K, IDAREG
			END IF
		ELSE
			' SI NO ESTA ENCRIPTADO (ORIGINAL)
			GET NIDAARC, 1, IDAREG
		END IF
		IDAREG.cajadir = cajadir
		IDAREG.TKID = MID$(IDA(NIDA), 1, 4)
		CLOSE NIDAARC
		'IDAREG.timeori = NOW
		IF IDAREG.Check <> CHECKID THEN
			erro "ARCHIVO SIN CHECK:SE IGNORA"
ERRORENIDA:
			BORRAR cajadir + IDA(NIDA)
			GetIda = -1
			EXIT FUNCTION
		END IF
		ON LOCAL ERROR GOTO 0

				REM BORRAR RTRIM$(r.cajadir) + LEFT$(IDA(NIDA), LEN(IDA(NIDA)) - LEN(IDAEXT)) + VTAEXT
		
END FUNCTION

SUB getOnLine (nhost)
	DIM k2$, infox, codrta, OK, Interno, Sacar, j, K$, K3$
	DIM AUTNUM AS LONG, CurCid AS INTEGER, CurTrace AS LONG
	DIM i, coderr
	' verificar el tiempo
	CurCid = cIDS(nhost)
	IF ComOn THEN
		RECIBIR K$, CurCid, infox
	END IF
	IF K$ <> "" THEN
		IF infox = 0 THEN
			IF debug THEN
			  '  mensaje FormatStr$(K$)
				mensaje "Host:" + STR$(nhost)
				mensaje decodeRTa$(K$, resp$())
				mensaje "-----------"
			END IF
			k2$ = decodeRTa$(K$, resp$())
			codrta = VAL(resp$(39))
			CurTrace = VAL(resp$(11))
						K3$ = MENRESP$(codrta, VAL(resp$(38)), OK, Sacar, Interno)
						IF debug THEN
								mensaje "pendientes:" + STR$(Pendiente) + " sacar:" + STR$(Sacar)

								i = Pendiente
								mensaje "trace:" + STR$(Pend(i).R.trace) + " CURTRACE:" + STR$(CurTrace)
								mensaje "host :" + STR$(Pend(i).R.host) + " " + STR$(nhost)
								mensaje "IDTER:" + Pend(i).R.idter + " " + resp$(41)

						END IF
						IF REVERSAR THEN
							 Sacar = 0
							 REVERSAR = 0
							 mensaje "FORZAR REVERSADO"

						END IF
						mensaje "sacar:" + STR$(Sacar)
			IF Sacar THEN
				  FOR i = 1 TO Pendiente
										  mensaje "   " + STR$(Pend(i).R.trace) + "=" + STR$(CurTrace)
										  mensaje "   " + STR$(Pend(i).R.host) + "=" + STR$(nhost)
										  mensaje "   " + Pend(i).R.idter + "=" + resp$(41) + "<"

										  IF Pend(i).R.trace = CurTrace AND Pend(i).R.host = nhost AND Pend(i).R.idter = resp$(41) THEN
							GOSUB Exito
							EXIT FOR

										  END IF
				  NEXT
			END IF
		ELSE
			IF infox <> -1 THEN
				erro "error :" + STR$(infox) + " En recepcion, si pasa varias veces notifique"

			END IF
		END IF
	END IF
	' Verificar si de los pendientes hay alguno muy viejo
	FOR i = 1 TO Pendiente
		IF Pend(i).Cid = CurCid AND (NOW - Pend(i).Enviado) * 86400 > SendTimeOut THEN
			' mandarle
                        erro "REVERSO" + STR$(i) + " de Req " + STR$(Pendiente)
			K$ = ReqReverso$(nhost, Pend(i).R)
		   ' IF debug THEN
			  '  mensaje FormatStr$(K$)
				mensaje "Host:" + STR$(nhost) + STR$(R.host)
				mensaje decodeRTa$((K$), resp$())
				mensaje "-----------"
		  '  END IF

			writerev Pend(i).R

			coderr = ENVIAR(K$, CurCid)
			IF coderr = 0 THEN
				  erro "Reverso Enviado OK"
                        else
                           error "error enviando reverso "+str$(coderr)
			END IF

			SWAP Pend(i), Pend(Pendiente)
		   'SWAP Pack$(I), Pack$(PENDIENTE)


			   mensaje "PROB.EN COMUNIC."
			   RESPINVAL " REINTENTE (R)"
				WaitAbort = TIMER + 40


			Pendiente = Pendiente - 1
			WritePend
		END IF
	NEXT
EXIT SUB

Exito:
		mensaje "sacando pendiente" ' ** sacar
	R = Pend(i).R
	R.RETREF = resp$(37)
	R.CodAut = resp$(38)
	IF visaOn(R) THEN
		Writehst "campo 34:" + resp$(34)
		 R.CODSEG = resp$(34)
	END IF
	GENVTA
	GOSUB wkk
	IF INSTR(resp$(63), "CABAL 24") > 0 THEN
			'mensaje "c24:"+r.track1
			mensaje "CABAL24:" + R.TRACK1
			R.EMINAME = "CABAL24"
	END IF
	IF OK THEN
	  'READHOST R.HOST, VAL(R.sufijo), 4, R.TICKET, 0, 0
	  R.ONLINE = -1
	  opcierre 1

	END IF
	VTAREG.OK = OK

	SWAP Pend(i), Pend(Pendiente)
	Pendiente = Pendiente - 1
	mensaje "Operacion Realizada"

   ' vtareg.CODRESP = codrta
	VTAREG.mensaje = resp$(63)
	VTAREG.MENRESP = K3$
	K$ = "TICK" + R.sufijo + ".DAT"
	VTAREG.FILETKT = K$
	'VTAREG.MONTOP = FORMAT$(R.IMPO * 100, "00000000")
	IF R.COEF > 1 THEN
		VTAREG.MONTOP = FORMAT$(R.IMPO * 100, "00000000")
		Writehst "MONTOP:" + VTAREG.MONTOP
	END IF
	MakeTicket VTAREG

	IF writevta(OK * -lastreg) THEN
		erro "ERROR GRABANDO VTA -> TRANS.REALIZADA PERO NO SE PUEDE COMUNICAR CON LA CAJA"
	ELSE
		mensaje "TRANSACCION COMPLETADA "
	END IF

	WritePend
	IF OK THEN
	  WRITELAST
	  READHOST R.host, VAL(R.sufijo), 4, R.TICKET, 0, 0
		  opcierre 9
	END IF


RETURN

wkk:
		DIM K7 AS STRING, T7 AS INTEGER
IF resp$(52) <> "" THEN
		K7$ = RTRIM$(R.cajadir) + "WKEY" + R.sufijo + ".DAT"
	mensaje "Writing WKey " + K7$
	T7 = FREEFILE
	OPEN K7$ FOR OUTPUT AS T7
		PRINT #T7, resp$(52);
	CLOSE T7
END IF
RETURN

END SUB

SUB InitVars ()
	CONCDIR = ".\"    'C:\CONCEN\

	CAJANOM = "CAJA"        ' NOMBRE DE ARCHIVO CAJAppnn
	REDIM cIDS(MaxHosts)

	SendTimeOut = 38 ' tiempo en segundos que se debe esperar la respuesta
	DINHOS = -1
	Mxsr = 2
	Pendiente = 0
	Maxpend = 30
	MAXVTAP = 20
	mvPT = 270 + debug * 120
	REDIM Pend(Maxpend) AS pendinfo
   'REDIM Pack$(Maxpend)
	REDIM ONLINE(MaxHosts)
	REDIM IDA(Maxpend) AS STRING
	NVTAP = 0
	REDIM VTAP(MAXVTAP) AS VTAPINFO
END SUB

SUB LoadEmisor ()
DIM i AS INTEGER
'
' CARGA DE EMISORES
'
IF BTRVOP(BHIGHEST, EMISOR, 0) = 0 THEN
	MAXEMI = Numval(NUMEMI)
ELSE
	erro "NO HAY EMISORES"
END IF
mensaje "CARGANDO EMISORES :" + STR$(MAXEMI)
REDIM EMITAB(MAXEMI + 1) AS EMITYPE
IF BTRVOP(BLOWEST, EMISOR, 0) = 0 THEN
	DO
		i = Numval(NUMEMI)
		EMITAB(i).NUMERO = i
		EMITAB(i).DESCRIP = Fmtstr$(DESEMI, true)
		EMITAB(i).ULTBOL = Numval(ULTEMI)
		IF EMITAB(i).ULTBOL < NOW - Numval(VIGEMI) THEN
						  REM erro "BOLETION DE " + EMITAB(I).DESCRIP + " NECESITA ACTUALIZACION, FECHA ULT BOL:" + FORMAT$(EMITAB(I).ULTBOL, "dd/mm/yy")
		END IF
		EMITAB(i).DIGITO = Numval(DIGEMI)
		EMITAB(i).LIMVTA = Numval(LIMEMI)
		EMITAB(i).host = Numval(HOSEMI)
		EMITAB(i).DELARNUM = Numval(DELEMI)
		EMITAB(i).HALARNUM = Numval(HALEMI)
		EMITAB(i).LLEVADIG = Numval(LLEEMI)
		EMITAB(i).METNEG = Numval(METEMI)
		EMITAB(i).ARCHIVO = ztrim$(FldData$(ARCEMI))
		EMITAB(i).T1 = Fmtstr$(T1EMI, true) = "S"
		EMITAB(i).S1 = Fmtstr$(S1EMI, true) = "S"

	LOOP WHILE BTRVOP(BNEXT, EMISOR, 0) = 0
END IF
END SUB

SUB Loadreq ()
DIM i, j
STATIC LREQ
	IF NIDA = 0 THEN
		  ' LEER DIRECTORIO P/CARGAR REQUERIMIENTOS
			curd$ = cajadir
		  CAJATMP$ = DIR$(cajadir + "C*.ID?") '+ IDAEXT)
		  DO WHILE CAJATMP$ <> ""
			 IF NIDA = 1 THEN EXIT DO
			 IF NIDA >= UBOUND(IDA) THEN
				erro "DEMASIADOS IDA"
				EXIT DO
			 END IF
			 NIDA = NIDA + 1
			 j = 0
			 FOR i = 1 TO NIDA
				 IF IDA(NIDA) = CAJATMP$ THEN j = -1
			 NEXT
			 IF NOT j THEN IDA(NIDA) = CAJATMP$
			 CAJATMP$ = DIR$
		  LOOP
		  IF LREQ <> NIDA - 1 THEN
						'mensaje "REQUERIMIENTO CARGADOS:" + STR$(NIDA)
			LREQ = NIDA - 1
		  END IF
	END IF

END SUB

SUB MakeTicket (TMP AS VTATYPE)
	DIM maxlen   AS INTEGER
	DIM MAESTRO AS INTEGER
	DIM ISOFF AS INTEGER
	DIM ISMAN AS INTEGER
	DIM ISCABAL AS INTEGER
	DIM ISMASTER AS INTEGER

	ISMASTER = INSTR(R.EMINAME, "MASTER") > 0
	ISCABAL = INSTR(R.EMINAME, "CABAL") > 0
	MAESTRO = R.tipocta > 0
	maxlen = 28
	DIM i, K$, i2, i3, V, nuevo
	i = FREEFILE
	curd$ = R.cajadir
	OPEN RTRIM$(R.cajadir) + TMP.FILETKT FOR OUTPUT AS i
		GOSUB WRITETK
	CLOSE i
	OPEN TMP.FILETKT FOR OUTPUT AS i
		GOSUB WRITETK
	CLOSE i
EXIT SUB

WRITETK:
						nuevo = INSTR(PARAM$, "/NT")
			FOR V = 1 TO 1
						PRINT #i, FORMAT$(R.fechahora, "dd/mm/yy") + SPACE$(maxlen - 20) + "Hora Trans:" + FORMAT$(R.fechahora, "hh:mm")
				K$ = RTRIM$(LEFT$(TMP.EMINAME, 20)): GOSUB centra
			SELECT CASE R.oper
				CASE OPCOMPRA
		  IF cashback(R) THEN
						K$ = "COMPRA + EXTRACCION"
		  ELSE
						K$ = "COMPRA"
		  END IF
				CASE OPANUCPA
		  IF cashback(R) THEN
								K$ = "ANUL. DE COMPRA+EXTRACCION"
				ELSE
								K$ = "ANULACION DE COMPRA"
				END IF
				CASE OPDEVOL
					K$ = "DEVOLUCION"
				CASE OPANUDEV
					K$ = "ANULACION DE DEVOLUCION"
				CASE 4
					K$ = "CONSULTA DE SALDO"
			END SELECT
			GOSUB centra

			PRINT #i,
		   'IF nuevo THEN
				K$ = "PINGUINO" + STR$(INT(VAL(R.sufijo) / 100))
				GOSUB centra
		   'END IF
						'K$ = custom$(1): GOSUB centra
						'K$ = custom$(2): GOSUB centra
						'K$ = custom$(3): GOSUB centra
			PRINT #i,
			PRINT #i, "N.Com:" + RTRIM$(R.Idcom)
			PRINT #i, "Term:" + R.idter + "    Caja:" + MID$(R.sufijo, 3, 4)
			PRINT #i, USING "Lote:###         Cargo:####"; R.lote; R.TICKET

						IF TMP.OK = 0 THEN
								K$ = TMP.mensaje + " " + TMP.MENRESP
								GOSUB centra
								RETURN
						END IF
			IF visaOn(R) THEN

					K$ = MID$(R.TARJ, 1, 6) + "XXXXXX" + MID$(R.TARJ, 13)
					Writehst "emancaramiento visa " + K$
			ELSE
				IF ISMASTER OR ISCABAL THEN   ' MASTER NO ENMASCARA TARJETA
					K$ = MID$(R.TARJ, 1)
				ELSE
					K$ = "XXXXXXXXXXXX" + MID$(R.TARJ, 13)
				END IF
			END IF
			K$ = K$ + " " + MID$("* ", 2 + (R.MANUAL <> 0), 1) + " "
			IF ISCABAL THEN
				K$ = K$ + MID$(R.EXPDATE, 3, 2) + "/" + MID$(R.EXPDATE, 1, 2)
			ELSE
				K$ = K$ + "XX/XX"
			END IF
			PRINT #i, K$

			IF visaOn(R) THEN
				Writehst "ticket:" + R.CODSEG
				K$ = "Nro. Cta.:" + RTRIM$(R.CODSEG)
				PRINT #i, K$
			END IF

				   SELECT CASE R.tipocta
			   CASE 1
					K$ = "DEBITO CA $"
			   CASE 2
					K$ = "DEBITO CC $"
			   CASE 8
					K$ = "DEBITO CA u$S"
			   CASE 9
					K$ = "DEBITO CC U$S"
			CASE ELSE
				K$ = "CREDITO"
		   END SELECT
			PRINT #i, K$ + " ";
			IF R.ONLINE AND ((R.cmd AND 8) = 0) THEN
				PRINT #i, "ON-LINE  ";
				PRINT #i, "Aut:" + R.CodAut
			ELSE
				PRINT #i, "OFF-LINE ";
				IF R.oper = OPANUCPA OR R.oper = OPANUDEV THEN
					PRINT #i, "Aut:000000"
				ELSE
					PRINT #i, "Aut:" + R.CodAut
				END IF
			END IF
			
			IF R.oper <> OPCOMPRA THEN PRINT #i, USING "Cargo Orig:######"; R.TKTORI
			IF R.oper = OPDEVOL THEN
								PRINT #i, "Fecha Orig:" + MID$(R.FECORI, 1, 2) + "/" + MID$(R.FECORI, 3, 2) + "/" + MID$(R.FECORI, 5, 2)
			ELSE
				PRINT #i,
			END IF
			PRINT #i,
			K$ = "$"
			IF R.MONEDA = "D" THEN K$ = "U$S"
						IF R.MONEDA = "B" THEN K$ = "-B-"
						REM IF R.CUOTAS > 1 THEN K$="U$S" 'DECRETO
						IF cashback(R) THEN
								PRINT #i, USING "Compra    :" + K$ + "##,###.## "; R.IMPO - R.cashback
								PRINT #i, USING "Extraccion:" + K$ + "##,###.## "; R.cashback
						END IF
														
						PRINT #i, USING "Imp.Total:" + K$ + "##,###.## "; R.IMPO
			PRINT #i, USING " Cuotas:##"; R.CUOTAS
			IF R.PLANID > "!" THEN
				  PRINT #i, "Tipo de Plan:" + RTRIM$(R.PLANID)
			END IF
                        IF TMP.mensaje > "      !" THEN
				  PRINT #i, TMP.mensaje
                        else
                                writehst "mensaje:"+tmp.mensaje
                        END IF
		'    IF TMP.NROFACT > 0 THEN PRINT #i, USING "Nro fact:"; TMP.NROFACT
			PRINT #i,
			PRINT #i,
						IF R.MANUAL <> 0 OR R.ONLINE = 0 AND R.host = 1 THEN
				 ' ESPACIO RESERVADO PARA EMBOSADO
				 DIM V2 AS INTEGER
				 FOR V2 = 1 TO 9
					 PRINT #i,
				 NEXT
			END IF
			K$ = "Firma :....................": GOSUB centra
			K$ = "Aclaraci¢n :...............": GOSUB centra
			i2 = INSTR(R.TRACK1, "^")
			i3 = INSTR(i2 + 1, R.TRACK1, "^")

			IF R.MANUAL <> 0 THEN

			ELSE
				IF i2 > 0 AND i3 > i2 THEN
					K$ = MID$(R.TRACK1, i2 + 1, i3 - i2 - 1): GOSUB centra
										'PRINT #I, K$
				END IF
			END IF


			REM PRINT #i, r.TARJ + " " + MID$("* ", 2 + (r.MANUAL <> 0), 1) + " " + MID$(r.EXPDATE, 3, 2) + "/" + MID$(r.EXPDATE, 1, 2)


			K$ = "Tipo y Nro Doc:............": GOSUB centra
			PRINT #i, " Cajero:" + FORMAT$(R.CAJERA, "0000") + "/" + FORMAT$(R.TICCAJ, "00000000")
		  '  SELECT CASE v
		  '      CASE 1
		  '          k$ = "Original Comercio ": GOSUB centra
		  '      CASE 2
		  '          k$ = "Copia Cliente": GOSUB centra
		  '  END SELECT
			IF V = 1 THEN PRINT #i, "@"
		NEXT

RETURN

centrae:
	maxlen = maxlen / 2
	GOSUB centra
	maxlen = maxlen * 2
RETURN

centra:
		IF LEN(K$) / 2 < maxlen / 2 THEN
			K$ = SPACE$(maxlen / 2 - LEN(K$) / 2) + K$
		ELSE
			K$ = LEFT$(K$, maxlen)
		END IF
		PRINT #i, K$
RETURN
END SUB

SUB opcierre (BYVAL modo)  ' agregar al cierre
		DIM mandalX AS INTEGER
	DIM f1$, f2$, F3$, t, t2, T3, T4, nreg2 AS LONG, cc AS CIERREINFO, K$
		DIM o1 AS INTEGER
	DIM f5$, tt5, tt52
	DIM NREG AS LONG, i AS LONG, r2 AS reqinfo, j AS LONG, t5
	DIM codrta, k2$, infox, tt!, nbatch, ret, resend, OK, NOTCONF
	DIM coderr, ok2, i2, R3 AS reqinfo, NREG3 AS LONG, idcomt$
	DIM F4$, T40, HOSTC AS INTEGER, REOFF
	DIM nohacefalta AS INTEGER
	CONST lotedir = "Lotes\"
	CONST OFFdir = "OFFLINE\"
	' los cierres son
	'        x caja
	'          host
	' modo 0-add 1-updte 2-inicializar 3-Cierre
	' A LA OPCION DE CIERRE DEBE ENTRARSE SIN HABER OPERACIONES PENDIENTES DE RESPONDER
	' EN ESE HOST
	IF VAL(R.sufijo) = 0 THEN
		' GOSUB cierratoda
		 EXIT SUB
	END IF
	IF (ONLINE(R.host) <> -1 AND R.host > 0) AND (modo = 3 OR modo = 5 OR modo = 13) THEN
		  GOSUB opencit
		  erro "No se puede realizar Cierre, Host OFF-Line"
		  PRINT #t5, "No se puede realizar Cierre, Host OFF-Line"
		  CLOSE #t5
		  EXIT SUB
	END IF
	HOSTC = R.host
	GOSUB getcien
	'abrir archivos
	t = FREEFILE
	OPEN f1$ FOR RANDOM AS t LEN = LRE
	t2 = FREEFILE
	OPEN f2$ FOR RANDOM AS t2 LEN = 128
	IF NOT EOF(t2) THEN GET t2, 1, NREG ELSE NREG = 0
	OK = 0
	SELECT CASE modo
		CASE 0, 1
			NREG = NREG + 1
			putR t, NREG, R, ENCRY
			lastreg = NREG
		CASE 2
			NREG = 0
		CASE 3, 5' cierre     5=CON DIF
			GOSUB makecie
		CASE 13     ' cierre con off-line re-enviadas
			REOFF = -1
			GOSUB makecie
		CASE 4
			R.ONLINE = 0
			NREG = NREG + 1
			putR t, NREG, R, ENCRY
			lastreg = NREG
		CASE 6 ' CHEQUEO ANULACION
			GOSUB CHKANUL
		CASE 7
			IF lastreg > 0 THEN
				GETR t, lastreg, r2, ENCRY
				r2.CONFIR = -1
				putR t, lastreg, r2, ENCRY
			END IF
		CASE 8 ' Lista Operaciones
			GOSUB ListLoteorD
		CASE 14 ' Lista Operaciones
			REOFF = -1
			GOSUB ListLoteorD
		CASE 10 ' RESETA F4
			GOSUB RESET40
		CASE 11 ' LISTTOT
			GOSUB List40
		CASE 12
			NREG = NREG - 1
			lastreg = NREG
				CASE 9 ' descargar 1 off
						GOSUB F2L
						OK = 0' por las dudas
	END SELECT

	CLOSE t
	'cerrar contadores
	PUT t2, 1, NREG
	CLOSE t2
	IF OK THEN
		mensaje "Resguardo Datos Lote" + STR$(R.lote)
		t = FREEFILE
		OPEN f1$ FOR RANDOM AS t LEN = LRE
		t2 = FREEFILE
		OPEN lotedir + f1$ FOR RANDOM AS t2 LEN = LRE
			FOR i = 1 TO NREG
				GETR t, i, r2, ENCRY
				putR t2, i, r2, ENCRY
			NEXT
		CLOSE t, t2

		t2 = FREEFILE
		OPEN lotedir + f2$ FOR RANDOM AS t2 LEN = 128
			PUT t2, 1, NREG
		CLOSE t2

		t = FREEFILE
		OPEN F3$ FOR INPUT AS t
		t2 = FREEFILE
		OPEN lotedir + F3$ FOR OUTPUT AS t2
			WHILE NOT EOF(t)
				LINE INPUT #t, K$
				PRINT #t2, K$
			WEND
		CLOSE t2, t

		IF HOSTC = 0 THEN
			mensaje "Resguardo Datos OFFLINE LOTE " + STR$(R.lote)
			t = FREEFILE
			OPEN f1$ FOR RANDOM AS t LEN = LRE
			t2 = FREEFILE
			OPEN OFFdir + f1$ FOR RANDOM AS t2 LEN = LRE
				FOR i = 1 TO NREG
					GETR t, i, r2, ENCRY
					putR t2, i, r2, ENCRY
				NEXT
			CLOSE t, t2

			t2 = FREEFILE
			OPEN OFFdir + f2$ FOR RANDOM AS t2 LEN = 128
				PUT t2, 1, NREG
			CLOSE t2

			t = FREEFILE
			OPEN F3$ FOR INPUT AS t
			t2 = FREEFILE
			OPEN OFFdir + F3$ FOR OUTPUT AS t2
				WHILE NOT EOF(t)
					LINE INPUT #t, K$
					PRINT #t2, K$
				WEND
			CLOSE t2, t
		END IF

		BORRAR f1$
		BORRAR f2$

		t = FREEFILE
		OPEN F3$ FOR INPUT AS t
		t2 = FREEFILE
		curd$ = R.cajadir
		OPEN "i:\tarjetas\Conce.cit" FOR OUTPUT AS t2
			WHILE NOT EOF(t)
				LINE INPUT #t, K$
				PRINT #t2, K$
			WEND
		CLOSE t2, t
		BORRAR F3$

				mensaje "Pasando datos ..." + lotedir$ + f5$
		IF DIR$(f5$) <> "" THEN
			tt5 = FREEFILE
			OPEN f5$ FOR INPUT AS tt5
			tt52 = FREEFILE
			OPEN lotedir$ + f5$ FOR APPEND AS tt52
				WHILE NOT EOF(tt5)
					 LINE INPUT #tt5, K$
					 PRINT #tt52, K$
				WEND
			CLOSE tt5, tt52
			BORRAR f5$
		END IF
		mensaje "Resguardo OK"

	END IF
EXIT SUB

RESET40:
	T40 = FREEFILE
	OPEN F4$ FOR OUTPUT AS T40
	CLOSE T40
	RESETIT = 60 * 40 + TIMER
RETURN

CHKANUL:
	mensaje "BUSCANDO " + STR$(R.TKTORI) + " EN " + STR$(NREG)
	  FOR i = 1 TO NREG
		GETR t, i, r2, ENCRY
		IF r2.TICKET = R.TKTORI AND r2.IMPO = R.IMPO THEN
		   IF (r2.oper = OPCOMPRA AND R.oper = OPANUCPA) OR (r2.oper = OPDEVOL AND R.oper = OPANUDEV) THEN
				R.RETREF = r2.RETREF
				R.CodAut = r2.CodAut
				mensaje "SE ENCONTRO:" + STR$(r2.TICKET) + " RETREF:" + R.RETREF + " COD AUT:" + R.CodAut
				IF r2.ONLINE = 0 THEN
					mensaje "Se Almacena Off-line"
					R.cmd = R.cmd OR 32
				END IF
				EXIT FOR
			END IF
		END IF
	  NEXT
	  IF i > NREG THEN
		CAUSA$ = "No existe ticket " + STR$(R.TKTORI) + "en lote"
		R.TKTORI = 0
	  ELSE
		  FOR i = i + 1 TO NREG
			GETR t, i, r2, ENCRY
			IF r2.TKTORI = R.TKTORI AND r2.oper = R.oper THEN
				R.TKTORI = 0
				CAUSA$ = "Ya se Anulo Ticket "
				mensaje "YA SE ANULO:" + STR$(r2.TICKET) + " RETREF:" + R.RETREF + " COD AUT:" + R.CodAut
				EXIT FOR
			END IF
		  NEXT

	 END IF
RETURN

ZEROCC:
	NOTCONF = 0
	cc.COMPRAS = 0
	cc.MONTO = 0
	cc.ANUL = 0
	cc.MONTOANUL = 0
	cc.DEVOL = 0
	cc.MONTODEV = 0
RETURN

opencit:
	t5 = FREEFILE
	OPEN "I:\tarjetas\conce.cit" FOR OUTPUT AS t5

RETURN

ListLoteorD:
	T40 = FREEFILE
	OPEN F4$ FOR RANDOM AS T40 LEN = LRE

	GOSUB opencit
	PRINT #t5, "CAJA:" + R.sufijo + " HORA:" + TIME$ + " FECHA:" + FORMAT$(NOW, "dd/mm/yy"); " lote:"; R.lote; " Terminal:"; R.idter
	IF BTRVOP(BLOWEST, PLANARC, 2) = 0 THEN
		DO
		  idcomt$ = RTRIM$(FldData$(COMEF))
		  GOSUB ZEROCC
		  ' LEEMOS EMISOR
		  j = Numval(EMISF)
		  IF j < UBOUND(EMITAB) THEN
			   K$ = EMITAB(j).DESCRIP
		  ELSE
			   K$ = "EMISOR DESCONOCIDO" + STR$(j)
		  END IF

		  mensaje "Procesando  " + idcomt$
		  NREG3 = 0
		  FOR i = 1 TO NREG
			GETR t, i, r2, ENCRY
			IF REOFF AND r2.ONLINE = -2 THEN
				  r2.ONLINE = 0
				  putR t, i, r2, ENCRY
			END IF
			IF RTRIM$(r2.Idcom) = idcomt$ THEN
				NREG3 = NREG3 + 1
				GOSUB SUMARENG
				putR T40, LOF(T40) / LRE + 1, r2, ENCRY
			END IF
		  NEXT

		  IF NREG3 THEN
			  mensaje K$ + " " + idcomt$
			  mensaje "Compras:" + STR$(cc.COMPRAS)
			  mensaje "      $:" + STR$(cc.MONTO)
			  mensaje "Devoluc:" + STR$(cc.DEVOL)
			  mensaje "      $:" + STR$(cc.MONTODEV)
			  mensaje "Anulac.:" + STR$(cc.ANUL)
			  mensaje "      $:" + STR$(cc.MONTOANUL)

			  PRINT #t5, "Compras:" + STR$(cc.COMPRAS); "      $:" + STR$(cc.MONTO); , " Devoluc:" + STR$(cc.DEVOL); "      $:" + STR$(cc.MONTODEV); " Anulac.:" + STR$(cc.ANUL); "      $:" + STR$(cc.MONTOANUL)
			  PRINT #t5, "TOTAL DE " + K$ + " " + idcomt$
			  PRINT #t5,
		  END IF
		DO
		   i = BTRVOP(BNEXT, PLANARC, 2)
		LOOP WHILE i = 0 AND idcomt$ = RTRIM$(FldData$(COMEF))
		IF i THEN EXIT DO
	LOOP
	END IF
	PRINT #t5, "*"
	CLOSE t5, T40
RETURN

List40:
	T40 = FREEFILE
	OPEN F4$ FOR RANDOM AS T40 LEN = LRE

	GOSUB opencit
	PRINT #t5, "CAJA:" + R.sufijo + " HORA:" + TIME$ + " FECHA:" + FORMAT$(NOW, "dd/mm/yy")
	PRINT #t5, "lote:"; R.lote; " Terminal:"; R.idter
	PRINT #t5,
	IF BTRVOP(BLOWEST, PLANARC, 2) = 0 THEN
		DO
		  idcomt$ = RTRIM$(FldData$(COMEF))
		  GOSUB ZEROCC
		  ' LEEMOS EMISOR
		  j = Numval(EMISF)
		  IF j < UBOUND(EMITAB) THEN
			   K$ = EMITAB(j).DESCRIP
		  ELSE
			   K$ = "EMISOR DESCONOCIDO" + STR$(j)
		  END IF

		  mensaje "Procesando  " + idcomt$
		  NREG3 = 0
		  FOR i = 1 TO LOF(T40) / LRE
			GETR T40, i, r2, ENCRY
			IF RTRIM$(r2.Idcom) = idcomt$ THEN
				NREG3 = NREG3 + 1
				GOSUB SUMARENG
			END IF
		  NEXT

		  IF NREG3 THEN
			  mensaje K$ + " " + idcomt$
			  mensaje "Compras:" + STR$(cc.COMPRAS)
			  mensaje "      $:" + STR$(cc.MONTO)
			  mensaje "Devoluc:" + STR$(cc.DEVOL)
			  mensaje "      $:" + STR$(cc.MONTODEV)
			  mensaje "Anulac.:" + STR$(cc.ANUL)
			  mensaje "      $:" + STR$(cc.MONTOANUL)

			  PRINT #t5, "Compras:" + STR$(cc.COMPRAS); "      $:" + STR$(cc.MONTO), " Devoluc:" + STR$(cc.DEVOL); "      $:" + STR$(cc.MONTODEV), " Anulac.:" + STR$(cc.ANUL); "      $:" + STR$(cc.MONTOANUL)
			  PRINT #t5, "TOTAL DE " + K$ + " " + idcomt$
			  PRINT #t5,
		  END IF
		DO
		   i = BTRVOP(BNEXT, PLANARC, 2)
		LOOP WHILE i = 0 AND idcomt$ = RTRIM$(FldData$(COMEF))
		IF i THEN EXIT DO
	LOOP
	END IF
	PRINT #t5, "*"
	CLOSE t5, T40
RETURN

F2L:
		o1 = 1
		mensaje "BUSCANDO PENDIENTES"
		 GOSUB ZEROCC
		  FOR i = 1 TO NREG
				GETR t, i, r2, ENCRY
				GOSUB SUMARENG
				IF nbatch THEN EXIT FOR
		  NEXT
		  IF nbatch THEN
			   mensaje "Enviando Offline pendiente"
			   GOSUB SendBatch
		  END IF
RETURN


ListLote:
	  GOSUB ZEROCC
	  IF t5 = 0 THEN
		  t5 = FREEFILE
		  OPEN F3$ FOR OUTPUT AS t5
	  END IF
	  mensaje "Procesando Cierre " + f1$ + " Registros:" + STR$(NREG) + " lote:" + STR$(R.lote)
	  PRINT #t5, "Procesando Cierre " + f1$ + " Registros:" + STR$(NREG) + " lote:" + STR$(R.lote) + " " + f5$
	  ' hacemos el cierre de acuerdo a r.host
	  tt5 = FREEFILE
	  OPEN f5$ FOR OUTPUT AS tt5
		  FOR i = 1 TO NREG
			GETR t, i, r2, ENCRY
			r2.fecenv = NOW
			putR t, i, r2, ENCRY
			GOSUB SUMARENG

			' registro para progress
			PRINT #tt5, CHR$(34);
			PRINT #tt5, FORMAT$(r2.lote, "0000");
			PRINT #tt5, FORMAT$(r2.TICKET, "0000");
						PRINT #tt5, "XXXXXXXXXXXX" + MID$(r2.TARJ, 13);
						PRINT #tt5, "0000";
			PRINT #tt5, FORMAT$(INT(r2.IMPO * 100), "0000000");
			PRINT #tt5, FORMAT$(r2.fechahora, "yymmdd");
			PRINT #tt5, FORMAT$(r2.fechahora, "hhmm");
			PRINT #tt5, r2.sufijo;
			PRINT #tt5, FORMAT$(r2.TICCAJ, "000000");
			SELECT CASE r2.ONLINE
				CASE -1
					PRINT #tt5, "0";
				CASE -2
					PRINT #tt5, "1";
				CASE ELSE
					PRINT #tt5, "2";
			END SELECT
			PRINT #tt5, FORMAT$(NOW, "yymmdd");
			SELECT CASE r2.CONFIR
				CASE -1
					PRINT #tt5, "1";
				CASE ELSE
					PRINT #tt5, "0";
			END SELECT
			' no especificadas
			PRINT #tt5, FORMAT$(r2.oper, "0");
			PRINT #tt5, r2.EMINAME; r2.idter; r2.Idcom; r2.IDCIE;
						PRINT #tt5, r2.CUOTAS;
						PRINT #tt5, FORMAT$(r2.PlanInt, "000");
						PRINT #tt5, CHR$(34)
		  NEXT
	  CLOSE tt5

	  mensaje "Compras:" + STR$(cc.COMPRAS)
	  mensaje "      $:" + STR$(cc.MONTO)
	  mensaje "Devoluc:" + STR$(cc.DEVOL)
	  mensaje "      $:" + STR$(cc.MONTODEV)
	  mensaje "Anulac.:" + STR$(cc.ANUL)
	  mensaje "      $:" + STR$(cc.MONTOANUL)

	  PRINT #t5, "Compras:" + STR$(cc.COMPRAS); "      $:" + STR$(cc.MONTO)
	  PRINT #t5, "Devoluc:" + STR$(cc.DEVOL); "      $:" + STR$(cc.MONTODEV)
	  PRINT #t5, "Anulac.:" + STR$(cc.ANUL); "      $:" + STR$(cc.MONTOANUL)
	  PRINT #t5,
		  nohacefalta = cc.COMPRAS = 0 AND cc.DEVOL = 0 AND cc.ANUL = 0 AND INSTR(COMMAND$, "/FC") = 0
		  IF INSTR(COMMAND$, "/TEST") THEN nohacefalta = 0
RETURN

SUMARENG:
	GOSUB sumcie2
	IF r2.ONLINE = 0 THEN
		nbatch = nbatch + 1
	END IF
		IF o1 THEN RETURN
		PRINT #t5, USING "### - # #### & & ####.## &"; i; r2.oper; r2.TICKET; "XXXXXXXXXXXX" + MID$(r2.TARJ, 13); "XXXX"; r2.IMPO;
	PRINT #t5, FORMAT$(r2.fechahora, " dd/mm hh:mm ");
	PRINT #t5, USING "CAJA:####"; VAL(r2.sufijo);
	PRINT #t5, USING " TK:######"; r2.TICCAJ;
	PRINT #t5, USING " OP:#### "; r2.CAJERA;

	PRINT USING "### - # #### & & ####.## &"; i; r2.oper; r2.TICKET; r2.TARJ; r2.EXPDATE; r2.IMPO; r2.Idcom;
	PRINT FORMAT$(r2.fechahora, " dd/mm hh:mm ");
	SELECT CASE r2.ONLINE
		CASE -1
			PRINT #t5, "ON LINE";
		CASE -2
			PRINT #t5, "OFF ENV";
		CASE ELSE
			PRINT #t5, "OFF NO ENV!";
	END SELECT
	IF r2.CONFIR = 0 THEN
		  PRINT #t5, " S/CONF!"
		  NOTCONF = NOTCONF + 1
	END IF
	PRINT #t5,
RETURN


makecie:
		  nbatch = 0

	  GOSUB ListLote

	  IF NOTCONF THEN
			erro "EXISTEN " + STR$(NOTCONF) + " OPERACION\ES SIN CONFIRMAR"
	  END IF
	  IF R.host > 0 THEN GOSUB SendBatch

	  IF modo = 5 THEN
		 mensaje "SE GENERA DIFERENCIA A PROPOSITO"
		 cc.COMPRAS = cc.COMPRAS + 1000
		 cc.MONTO = cc.MONTO + 100000
	  END IF
	  cc.DIFE = 0

	  IF nohacefalta OR R.host = 0 THEN
		IF R.host = 0 THEN
			mensaje "Cierre de Host por archivo"
		ELSE
			mensaje "Cierre no es necesario"
		END IF
		resend = 0
		OK = -1
	  ELSE
		GOSUB MAKCIE
	  END IF
	   IF resend THEN
		  mensaje "Enviando Transacciones para corregir"
		  PRINT #t5, "Enviando Transacciones para corregir"
		  GOSUB ZEROCC
		  ret = 1
		  ' activar si se desea enviar nros de tarjetas en operaciones
		  ' con banda
		  ' por ahora activado para todos
				  mandalX = -1
		  FOR i = 1 TO NREG
			GETR t, i, r2, ENCRY
			r2.ULTIMO = i = NREG

			K$ = ReqBatch$(r2)

			GOSUB ESPRES
			mensaje MENRESP$(codrta, 0, ok2, 0, 0)
			IF ok2 = 0 THEN
				erro "Error Nro" + STR$(codrta) + MENRESP$(codrta, 0, 0, 0, 0) + " enviando Transaccion Nro " + STR$(i)
			ELSE
				GOSUB sumcie2
			END IF
		   'r.codrta = codrta
		   'PUTR t, i, r2  ,ENCRY
		  NEXT
		  cc.DIFE = -1
		  GOSUB MAKCIE
		  IF resend THEN
				PRINT #t5, "Cierre con Dif., intente nuevamente"
				mensaje "Cierre con Dif., intente nuevamente"
		  END IF
	   END IF

	   IF OK THEN   '   se incrementa el lote
			PRINT #t5, "Cierre OK " + resp$(63)
			Writehst "Cierre OK " + resp$(63)
			READHOST R.host, VAL(R.sufijo), 16 + 8, 0, R.lote, 0
	   END IF
		CLOSE t5
RETURN

getcien:
	f1$ = "C" + FORMAT$(VAL(R.sufijo), "0000") + FORMAT$(R.host, "00") + FORMAT$(R.lote MOD 10, "0") + EXTR$
	f2$ = "C" + FORMAT$(VAL(R.sufijo), "0000") + FORMAT$(R.host, "00") + FORMAT$(R.lote MOD 10, "0") + ".cin"
	F3$ = "C" + FORMAT$(VAL(R.sufijo), "0000") + FORMAT$(R.host, "00") + FORMAT$(R.lote MOD 10, "0") + ".cir"
	F4$ = "CTOTA.cir"
	f5$ = FORMAT$(NOW, "yymmdd")
	f5$ = f5$ + FORMAT$(nci, "00") + ".POS"
RETURN

ESPRES:
	  IF debug THEN
		 ' mensaje FormatStr$(K$)
		  mensaje "Host:" + STR$(HOSTC)
		  mensaje decodeRTa$(K$, resp$())
		  mensaje "-----------"
	  END IF
	  FOR j = 1 TO ret
		  IF ret > 1 THEN mensaje "Enviando Datos del Cierre intento nro:" + STR$(j)
		  IF ComOn THEN
			  coderr = ENVIAR(K$, cIDS(HOSTC))
		  END IF
		  IF coderr = 0 THEN
				tt! = TIMER
				DO
					IF ComOn THEN
						RECIBIR K$, cIDS(HOSTC), infox
					ELSE
						EXIT DO
					END IF
				LOOP WHILE K$ = "" AND TIMER - tt! < 30
				IF K$ <> "" THEN EXIT FOR
		  ELSE
			  erro "Error:" + STR$(coderr)
		  END IF
	  NEXT
		codrta = 100
		IF K$ <> "" AND j <= ret THEN
			IF infox = 0 THEN
				IF debug THEN
				   ' mensaje FormatStr$(K$)
					mensaje decodeRTa$(K$, resp$())
					mensaje "-----------"
				END IF
				k2$ = decodeRTa$(K$, resp$())
				codrta = VAL(resp$(39))
			END IF
	   END IF

RETURN

MAKCIE:
	  ' de r se usa host,idter,idcom
	   cc.BATCH = R.lote
	   K$ = reqCIerre$(R, cc)
	   ret = 3
	   GOSUB ESPRES
	   OK = 0
	   resend = 0
	   SELECT CASE codrta
			CASE 0, 11, 85' Cierre realizado satifactoriamente
				OK = -1
			CASE 95
				mensaje "Diferencia en Cierre:"
				resend = -1
			CASE ELSE  ' otro error
				GOSUB opencit
				erro "Error en Cierre:" + STR$(codrta) + " No se ha efectuado"
				CLOSE #t5
	   END SELECT
RETURN

getall:
	  REDIM ciea$(nreg2 + 1)
	  mensaje "Procesando Cierres:"
	  nreg2 = 0
	  K$ = DIR$("C???????.CIN")
	  DO WHILE K$ <> ""
		  nreg2 = nreg2 + 1
		  IF nreg2 > UBOUND(ciea$) THEN REDIM PRESERVE ciea$(nreg2 + 10)
		  ciea$(nreg2) = K$
		  K$ = DIR$
	  LOOP
RETURN

cierratoda:
	  nreg2 = 0
	  mensaje "Cierres Pendientes:"
	  K$ = DIR$("C???????.CIN")
	  DO WHILE K$ <> ""
		  nreg2 = nreg2 + 1
		  t = FREEFILE
		  OPEN K$ FOR RANDOM AS t LEN = 128
			GET t, 1, NREG
		  CLOSE t
		  mensaje "Caja:" + MID$(K$, 2, 4) + " Host:" + MID$(K$, 6, 2) + " Ult Dig Lot:" + MID$(K$, 8, 1) + " Operaciones:" + STR$(NREG)
		  K$ = DIR$
	  LOOP
	  IF R.MANUAL THEN
			LINE INPUT "Escriba 'S' para cerrar"; K$
			IF K$ <> "S" THEN RETURN
	  END IF

	  GOSUB getall

	  FOR i = 1 TO nreg2
		  K$ = ciea$(i)
		  OPEN K$ FOR RANDOM AS t LEN = 128
			GET t, 1, NREG
		  CLOSE t
		  mensaje "Caja:" + MID$(K$, 2, 4) + " Host:" + MID$(K$, 6, 2) + " Ult Dig Lot:" + MID$(K$, 8, 1) + " Operaciones:" + STR$(NREG)
		  R.sufijo = MID$(K$, 2, 4)
		  R.host = VAL(MID$(K$, 6, 2))
		  READHOST R.host, VAL(R.sufijo), 0, R.TICKET, R.lote, R.trace
		  R.idter = FldData$(TERNUM)
		  opcierre modo
	  NEXT
RETURN

sumcie2:
	r2.IMPO = INT(r2.IMPO * 100@) / 100@
	SELECT CASE r2.oper
 	    CASE OPCOMPRA
	         cc.COMPRAS = cc.COMPRAS + 1
                 cc.MONTO = cc.MONTO + r2.IMPO
	    CASE OPDEVOL
   		 cc.DEVOL = cc.DEVOL + 1
		 cc.MONTODEV = cc.MONTODEV + r2.IMPO
	    CASE OPANUCPA
	 	 cc.ANUL = cc.ANUL + 1
		 cc.MONTOANUL = cc.MONTOANUL + r2.IMPO
		 cc.COMPRAS = cc.COMPRAS - 1
		 cc.MONTO = cc.MONTO - r2.IMPO
	    CASE OPANUDEV
		 cc.ANUL = cc.ANUL + 1
		 cc.MONTOANUL = cc.MONTOANUL + r2.IMPO
		 cc.DEVOL = cc.DEVOL - 1
		 cc.MONTODEV = cc.MONTODEV - r2.IMPO
	END SELECT

RETURN

SendBatch:
	IF nbatch > 0 THEN
		mensaje "Enviando " + STR$(nbatch) + " Off-line"
				IF o1 = 0 THEN PRINT #t5, "Enviando " + STR$(nbatch) + " Off-line"
		FOR i = 1 TO NREG
		  GETR t, i, r2, ENCRY
		  IF r2.ONLINE = -2 AND REOFF THEN
				r2.ONLINE = 0
				putR t, i, r2, ENCRY
		  END IF
		  IF r2.ONLINE = 0 THEN
			   IF r2.oper = OPANUDEV OR r2.oper = OPANUCPA THEN
						  FOR i2 = 1 TO i - 1
							GETR t, i2, R3, ENCRY
							IF R3.TICKET = r2.TKTORI THEN
								r2.RETREF = R3.RETREF
								r2.CodAut = R3.CodAut
								mensaje "SE ENCONTRO:" + STR$(R3.TICKET) + " RETREF:" + R.RETREF + " COD AUT:" + R.CodAut
								EXIT FOR
							END IF
						  NEXT
						  putR t, i, r2, ENCRY
			   END IF
			   IF r2.MANUAL = 0 AND MID$(r2.TRACK2, 1, 6) = "000000" THEN
					 erro "Pasada a Ing.Manual p/track2 Incorrecto"
					 r2.MANUAL = -1
					 putR t, i, r2, ENCRY
			   END IF
			   nbatch = nbatch - 1
						   ' PONER SOLO 1
						   IF o1 THEN nbatch = 0
			   r2.ULTIMO = nbatch = 0
			   K$ = ReqOffLine$(r2.oper, r2)
			   ret = 1
			   GOSUB ESPRES
			   mensaje MENRESP$(codrta, 0, ok2, 0, 0)
			   IF ok2 THEN
				   IF o1 = 0 THEN PRINT #t5, "Offline" + STR$(i) + " Enviada OK"
				   IF VAL(r2.RETREF) = 0 AND (r2.oper = OPDEVOL OR r2.oper = OPCOMPRA) THEN
						r2.RETREF = resp$(37)
				   END IF
				   r2.ONLINE = -2
				   putR t, i, r2, ENCRY
				   IF o1 THEN EXIT FOR
			   ELSE
				   IF o1 = 0 THEN PRINT #t5, "Offline" + STR$(i) + " error" + STR$(codrta) + " " + MENRESP$(codrta, 0, ok2, 0, 0)
			   END IF
		  END IF
		NEXT
	END IF
RETURN

END SUB

SUB OpenFileSys ()
	mensaje "ABRIENDO ARCHIVOS ..."

	EMISOR = openfile("EMISOR", "")
	' CLAVE 0 - X NUMERO
	RANGHAB = openfile("RANGHAB", "")
	RANGINHA = openfile("RANGINHA", "")
	NUMERO = openfile("NUMERO", "")
	HOSTF = openfile("HOST", "")

	PLANARC = openfile("PLANES", "")
	' ****** APERTURA DE CAMPOS ********

	NUMEMI = OPENFIELD(EMISOR, "NUMERO", "")
	DESEMI = OPENFIELD(EMISOR, "DESCRIP", "")
	ULTEMI = OPENFIELD(EMISOR, "ULTBOL", "")
	DIGEMI = OPENFIELD(EMISOR, "DIGITO", "")
	LIMEMI = OPENFIELD(EMISOR, "LIMVTA", "")
	HOSEMI = OPENFIELD(EMISOR, "HOST", "")
	DELEMI = OPENFIELD(EMISOR, "DELARNUM", "")
	HALEMI = OPENFIELD(EMISOR, "HALARNUM", "")
	LLEEMI = OPENFIELD(EMISOR, "LLEVADIG", "")
	VIGEMI = OPENFIELD(EMISOR, "VIGBOL", "")
	ARCEMI = OPENFIELD(EMISOR, "ARCNEG", "")
	METEMI = OPENFIELD(EMISOR, "METNEG", "")
	T1EMI = OPENFIELD(EMISOR, "T1", "")
	S1EMI = OPENFIELD(EMISOR, "CODSEG", "")

	EMIRHA = OPENFIELD(RANGHAB, "EMISOR", "")
	DESRHA = OPENFIELD(RANGHAB, "DESDE", "")
	HASRHA = OPENFIELD(RANGHAB, "HASTA", "")

	EMIRIN = OPENFIELD(RANGINHA, "EMISOR", "")
	DESRIN = OPENFIELD(RANGINHA, "DESDE", "")
	HASRIN = OPENFIELD(RANGINHA, "HASTA", "")

	CAJNUM = OPENFIELD(NUMERO, "CAJA", "")
	HOSNUM = OPENFIELD(NUMERO, "HOST", "")
	TERNUM = OPENFIELD(NUMERO, "TERMINAL", "")
	LOTNUM = OPENFIELD(NUMERO, "LOTE", "")
	TICNUM = OPENFIELD(NUMERO, "TICKET", "")
	TRANUM = OPENFIELD(NUMERO, "TRACE", "")

	NUMHOS = OPENFIELD(HOSTF, "NUMERO", "")
	DESHOS = OPENFIELD(HOSTF, "DESCRIP", "")
	TPDHOS = OPENFIELD(HOSTF, "TPDUHOST", "")
	CALHOS = OPENFIELD(HOSTF, "CALLX25", "")
	CAL2HOS = OPENFIELD(HOSTF, "X252", "")
	VERHOS = OPENFIELD(HOSTF, "VERSOFT", "")
	CIEHOS = OPENFIELD(HOSTF, "CODCOM", "")
	UDTHOS = OPENFIELD(HOSTF, "UDATA", "")

	MONEF = OPENFIELD(PLANARC, "MONEDA", "")
	CUOTF = OPENFIELD(PLANARC, "CUOTAS", "")
	PLANF = OPENFIELD(PLANARC, "PLAN", "")
	EMISF = OPENFIELD(PLANARC, "EMISOR", "")
	COMEF = OPENFIELD(PLANARC, "IDCOM", "")
	DESEF = OPENFIELD(PLANARC, "DESCRIP", "")
	COEFF = OPENFIELD(PLANARC, "COEF", "")
END SUB

FUNCTION PLAN (NP, NE, NC, NPO, NM AS STRING, NCO AS STRING, PLD$, cx AS DOUBLE)
	PLD$ = ""
  ' ANULADO PORQUE LO LEEMOS SIEMPRE
  'IF NP = 0 THEN
  '     NC = 1
  '     NP = 0
  '     NM = "P"
  '     NCO = EMITAB(NE).Idcom
  '     PLAN = 0
  '     EXIT FUNCTION
  ' END IF
	Fi(PLANARC).keybuffer = MKI$(NP) + MKI$(NE)
	IF BTRVOP(BEQUAL, PLANARC, 0) THEN
		PLAN = status
	ELSE
		PLAN = 0
		NPO = Numval(PLANF)
		NC = Numval(CUOTF)
		NM = FldData(MONEF)
		NCO = FldData(COMEF)
		PLD$ = RTRIM$(FldData(DESEF))
		cx = Numval(COEFF)
		IF cx <= 1 THEN cx = 1
		IF cx > 2 THEN
			erro "COEFICIENTE MUY GRANDE"
			cx = 1
		END IF
	END IF
END FUNCTION

SUB READHOST (nhost, NCAJ, modo, tick, lote, trac AS LONG)
	'MODOS
	'   0 = LEETODO
	'   1 = LEE Y ACTUALIZA
	'   2 --> TRACE
	'   4 --> TICK
	'   8 --> LOTE
	IF modo < 1 THEN
		Fi(HOSTF).keybuffer = MKI$(nhost)
		IF BTRVOP(BEQUAL, HOSTF, 0) = 0 THEN

		ELSE
			' 504612 -
			IF nhost > 0 THEN erro "Host " + STR$(nhost) + " no existe en archivo"
		END IF
		IF modo < 0 THEN EXIT SUB
	END IF
	' LEEMOS LA NUMERACION
	Fi(NUMERO).keybuffer = MKI$(NCAJ) + MKI$(nhost)
	IF BTRVOP(BEQUAL, NUMERO, 0) THEN
		mensaje "err " + STR$(status) + " EN LECTURA NUMERACION CAJA" + STR$(NCAJ) + " EN HOST" + STR$(nhost)
		lote = 1
		tick = 1
		trac = -1
		SetFld TERNUM, ""
	ELSE
		lote = Numval(LOTNUM)
		tick = Numval(TICNUM)
		trac = Numval(TRANUM)
		IF modo > 0 THEN
			  IF modo AND 2 THEN trac = trac + 1: IF trac > 999999 THEN trac = 1
			  IF modo AND 4 THEN tick = tick + 1: IF tick > 9999 THEN tick = 1
			  IF modo AND 8 THEN lote = lote + 1: IF lote > 999 THEN lote = 1
			'  IF modo AND 16 THEN tick = 1
			  LSETNUM LOTNUM, lote
			  LSETNUM TICNUM, tick
			  LSETNUM TRANUM, trac
			  IF BTRVOP(BUPDATE, NUMERO, 0) THEN
					erro "ERROR" + STR$(status) + " ACUTALIZANDO NROS"
			  END IF
		END IF
	END IF
END SUB

SUB RESPINVAL (K$)
	GENVTA
  ' VTAREG.CODRESP = 99
	VTAREG.mensaje = K$
	VTAREG.MENRESP = ""
	VTAREG.OK = 0

	IF writevta(0) THEN
		erro "ERROR GRABANDO VTA -> TRANS.REALIZADA PERO NO SE PUEDE COMUNICAR CON LA CAJA"
	ELSE
		mensaje " CONTESTADO A LA CAJA " + R.sufijo + spac + CAUSA$
	END IF

END SUB

FUNCTION SendonLine (nhost) 'envio R
	DIM K$, coderr, OFFT, ABORTALO

	OFFT = R.cmd AND OFFLINE
	ABORTALO = R.cmd AND ABORTAR

	R.ONLINE = -1
	IF OFFT THEN
		K$ = ReqOffLine$(R.oper, R)
	ELSE
		K$ = reqOnLine$(R.oper, R)
	END IF
	IF debug THEN
	   ' mensaje FormatStr$(K$)
		mensaje "Host:" + STR$(nhost)
		mensaje decodeRTa$(K$, resp$())
		mensaje "-----------"
	END IF
	IF ComOn THEN
		coderr = ENVIAR(K$, cIDS(nhost))
	END IF
	IF coderr = 0 THEN
		' poner pendiente
		IF Pendiente > UBOUND(Pend) THEN
			erro "DEMASIADOS REQUERIMIENTOS PENDIENTES"
			SendonLine = -1
		ELSE
			Pendiente = Pendiente + 1
						IF debug THEN
							 mensaje "entrada a pendiente " + STR$(R.trace)
						END IF
			Pend(Pendiente).R = R
			Pend(Pendiente).Cid = cIDS(nhost)
			Pend(Pendiente).Enviado = NOW
			IF ABORTALO THEN
				' esperamos 2 segundos
				Pend(Pendiente).Enviado = NOW - (SendTimeOut - 1#) / 86400                    ' LO MANDE EL AYER
			END IF
			Pend(Pendiente).Retry = 0
		   'Pack$(PENDIENTE) = K$
			SendonLine = 0
			WritePend
		END IF
	ELSE
		' error en el envio
				mensaje "error en envioo"
		SendonLine = coderr
		RESETIT = TIMER + 30' Resetear en 30 segundos
	END IF
		R.CODSEG = ""
END FUNCTION

SUB StoreOffLine (nhost)
	DIM OK, k2$, K3$, K$, t, STEPX   AS LONG, NREG AS LONG, BASEX AS LONG, hayx
	IF nhost > 0 THEN
		mensaje "HOST :" + STR$(nhost) + " OFF-LINE"
	END IF
	GENVTA

	OK = -1
	' VALIDAR NEGATIVAS
	IF EMIHAB > 0 THEN
		K$ = RTRIM$(EMITAB(EMIHAB).ARCHIVO)
		IF DIR$(K$) = "" THEN
			erro "NO EXISTE BOL:" + K$

			OK = 0
		ELSE
			SELECT CASE EMITAB(EMIHAB).METNEG
				CASE 99
					erro "emisor sin chequeo"
					VTAREG.mensaje = "tarj no habilitada"
					VTAREG.MENRESP = VTAREG.mensaje
					OK = 0
					hayx = -1
				CASE 90 ' ARCHVO POSITIVO
					IF NEGATIVA = 0 THEN
												mensaje "ABR ARC.POS"
						NEGATIVA = openfile("NEGA", "")
						TARNEG = OPENFIELD(NEGATIVA, "TARJETA", "")
					END IF
										mensaje "VERIF POSIT " + R.TARJ
					Fi(NEGATIVA).keybuffer = R.TARJ + ZEROKB
					IF BTRVOP(BEQUAL, NEGATIVA, 0) = 0 THEN
						mensaje "ENCONTRADA  " + R.TARJ
						OK = -1
					ELSE
												mensaje "confina : " + STR$(status)
						VTAREG.mensaje = "TARJETA No habilitada " + STR$(status)
						VTAREG.MENRESP = VTAREG.MENRESP
						OK = 0
						mensaje VTAREG.mensaje
					END IF
				CASE ELSE ' ARG/CF/CRED/CAB
					t = FREEFILE
					K3$ = RTRIM$(R.TARJ)
					IF debug THEN mensaje "abriendo " + K$ + " " + K3$
					IF DIR$(K$) = "" OR K$ = "" THEN
						erro "falta boletin p/control"
						VTAREG.mensaje = "falta boletin p/control"
						VTAREG.MENRESP = "Emita Cup¢n Manual"
						OK = 0
					ELSE
						OPEN K$ FOR RANDOM AS t LEN = 40
						FIELD #t, 40 AS K$
							BASEX = 0
							hayx = 0
							NREG = LOF(t) / 40
							IF LEFT$(K3$, 6) = "628161" THEN
			                                   mensaje "leyendo 500 millas nexo " + STR$(NREG)
							   FOR BASEX = 1 TO NREG
									GET t, BASEX
									k2$ = RTRIM$(MID$(K$, 3, LEN(K3$)))
																		'Mensaje "BUSQ SECUENCIAL " + K3$ + " " + k2$
									IF k2$ = K3$ THEN
										hayx = -1
										EXIT FOR
									END IF
							   NEXT
							ELSE
								STEPX = NREG / 2
								DO
									GET t, BASEX + STEPX
									k2$ = RTRIM$(MID$(K$, 3, LEN(K3$)))
									IF k2$ = K3$ THEN
										hayx = -1
										EXIT DO
									ELSEIF k2$ > K3$ THEN

									ELSE
										BASEX = STEPX
									END IF
									IF STEPX <= 1 THEN EXIT DO
									STEPX = STEPX / 2
								LOOP
							END IF
						CLOSE t
						IF hayx THEN mensaje "tarjeta en boletin"
						OK = hayx = 0
						IF hayx THEN
							VTAREG.mensaje = "tarj en bolet¡n"
							VTAREG.MENRESP = VTAREG.mensaje
						END IF
					END IF
			END SELECT
		END IF
	ELSE
		erro "EMISOR = 0"
		OK = 0
	END IF
	' PTO G.
	' VALIDAR LIMITE DE VENTA
	IF OK THEN
		IF R.IMPO > EMITAB(EMIHAB).LIMVTA AND VAL(R.CodAut) = 0 THEN
			  IF EMITAB(EMIHAB).LIMVTA < 1 THEN
				VTAREG.mensaje = "Sist off-line reintente en 1 min o pida Cod Aut"
			  ELSE
				VTAREG.mensaje = "Supera Lim.Vta /Req.aut."
			  END IF
			  VTAREG.MENRESP = VTAREG.mensaje
			  OK = 0
		END IF
	END IF
			   
	R.ONLINE = 0
	IF OK THEN

		mensaje "Operacion Autorizada Off-Line"
		VTAREG.OK = -1
	  ' VTAREG.CODRESP = 0
		VTAREG.mensaje = "AUTORIZADA OFF-LINE"
		VTAREG.MENRESP = VTAREG.mensaje

		K$ = "TICK" + R.sufijo + ".DAT"
		VTAREG.FILETKT = K$
		IF R.COEF > 1 THEN VTAREG.MONTOP = FORMAT$(R.IMPO * 100, "00000000")

		MakeTicket VTAREG
		opcierre 4
	ELSE
		VTAREG.OK = 0
		mensaje "Operacion NO Autor. Off-Line"
	END IF

	IF writevta(OK * -lastreg) THEN
		erro "ERROR GRABANDO VTA -> TRANS.REALIZADA PERO NO SE PUEDE COMUNICAR CON LA CAJA"
	ELSE
		mensaje "TRANSACCION COMPLETADA " 
	END IF

	IF OK THEN
		WRITELAST
		READHOST R.host, VAL(R.sufijo), 4, R.TICKET, 0, 0
	END IF
END SUB

SUB WRITELAST ()
	DIM TX, kx$
	TX = FREEFILE

	kx$ = "CAJA" + R.sufijo + ".TMP"
	ON LOCAL ERROR RESUME NEXT
	OPEN kx$ FOR OUTPUT AS TX
		kx$ = R.TARJ + STR$(R.IMPO)
		PRINT #TX, kx$
	CLOSE TX
	ON LOCAL ERROR GOTO 0
END SUB

SUB WritePend ()
	DIM t
	t = FREEFILE
	OPEN pendat$ FOR OUTPUT AS t
		 WRITE #t, Pendiente, NVTAP, DATE$, TIME$
	CLOSE t
END SUB

SUB writerev (rx AS reqinfo)
DIM KQ$, ty
KQ$ = rx.idter + " " + FORMAT$(rx.fechahora, "dd/mm/yy hh:mm:ss")
KQ$ = KQ$ + " " + rx.TARJ + " " + FORMAT$(rx.IMPO, "0000.00") + " " + rx.EMINAME + " " + FORMAT$(rx.trace, "0000")
mensaje "#REVERSO:" + KQ$
ty = FREEFILE
OPEN "rev"+FORMAT$(rx.fechahora, "ddmm")+".dat" FOR APPEND AS ty
	WRITE #ty, KQ$
CLOSE ty
END SUB

FUNCTION writevta (idre) '-1 ERROR
	DIM i
	'
	' VER SI HAY VTA PARA GRABARLOS
	'
		' ABRIR ARCHIVO DE CAJA PARA GRABAR RESPUESTA
		NVTAARC = FREEFILE
		CAJATMP$ = RTRIM$(R.cajadir) + R.TKID + "00" + MID$(R.sufijo, 3, 2) + VTAEXT
		'CAJATMP$ = RTRIM$(R.cajadir) + CAJANOM + "00" + MID$(R.sufijo, 3, 2) + VTAEXT
		writevta = 0
		NRETRY = 0
RETRY2:
		mensaje "GRABANDO " + CAJATMP$
		ON LOCAL ERROR RESUME NEXT
		OPEN CAJATMP$ FOR BINARY ACCESS WRITE LOCK READ WRITE AS NVTAARC
			IF ERR > 0 THEN
				  erro "PERMISO DENEGADO " + CAJATMP$
				  NRETRY = NRETRY + 1
				  IF NRETRY > 10 THEN
						writevta = -1
						EXIT FUNCTION
				  END IF
				  espera .4
				  GOTO RETRY2
			END IF

			' GRABO REGISTRO
		PUT NVTAARC, 1, VTAREG
			IF ERR > 0 THEN
				writevta = -1
			END IF
		ON LOCAL ERROR GOTO 0
		CLOSE NVTAARC

	' AGREGAR A LA LISTA DE VTAS PENDIENTES DE DESAPARECER

	NVTAP = NVTAP + 1
	IF NVTAP + 5 > UBOUND(VTAP) THEN
				erro "DEMASIADAS CAJAS SIN RESPONDER"
	END IF

	IF NVTAP < UBOUND(VTAP) THEN
		VTAP(NVTAP).nregi = idre
		VTAP(NVTAP).lote = R.lote
		VTAP(NVTAP).host = R.host
		VTAP(NVTAP).sufijo = R.sufijo
		VTAP(NVTAP).cajadir = RTRIM$(R.cajadir)
		'VTAP(NVTAP).ARC = CAJANOM + "00" + MID$(R.sufijo, 3, 2) + VTAEXT
		VTAP(NVTAP).ARC = R.TKID + "00" + MID$(R.sufijo, 3, 2) + VTAEXT
		VTAP(NVTAP).TIME = NOW
	ELSE
		erro "DEMASIADOS VTA PEND. -> SIST.INSEGURO"
		NVTAP = NVTAP - 1
	END IF

IF VTAREG.OK THEN
		NVTAARC = FREEFILE
		'CAJATMP$ = CAJANOM + MID$(R.sufijo, 1, 4) + VTAEXT
		CAJATMP$ = R.TKID + MID$(R.sufijo, 1, 4) + VTAEXT
		writevta = 0
		NRETRY = 0
RETRY22:
		mensaje "GRABANDO BACKUP" + CAJATMP$
		ON LOCAL ERROR RESUME NEXT
		OPEN CAJATMP$ FOR BINARY ACCESS WRITE LOCK READ WRITE AS NVTAARC
			IF ERR > 0 THEN
				  erro "PERMISO DENEGADO " + CAJATMP$
				  NRETRY = NRETRY + 1
				  IF NRETRY > 10 THEN
						writevta = -1
						EXIT FUNCTION
				  END IF
				  espera .4
				  GOTO RETRY22
			END IF

			' GRABO REGISTRO
		PUT NVTAARC, 1, VTAREG
			IF ERR > 0 THEN
				writevta = -1
			END IF
		ON LOCAL ERROR GOTO 0
		CLOSE NVTAARC
END IF




	'CAJATMP$ = RTRIM$(R.cajadir) + CAJANOM + "00" + MID$(R.sufijo, 3, 2) + IDAEXT
	CAJATMP$ = RTRIM$(R.cajadir) + R.TKID + "00" + MID$(R.sufijo, 3, 2) + IDAEXT
	BORRAR CAJATMP$

END FUNCTION

