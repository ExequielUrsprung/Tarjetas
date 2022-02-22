DEFINT A-Z OPTION EXPLICIT DECLARE FUNCTION cashback% (rr AS ANY) DECLARE
FUNCTION visaOn% (r AS ANY) DECLARE FUNCTION pin$ (KK$) DECLARE FUNCTION
numtobcd2$ (N$, BYVAL DIG%) DECLARE FUNCTION FMTPLAN$ (N AS INTEGER) DECLARE
FUNCTION getnii (host AS INTEGER) AS STRING DECLARE SUB erro (K$) DECLARE
FUNCTION fechamal% (K$) DECLARE FUNCTION numtobcdl$ (KK$, BYVAL DIG%) DECLARE
SUB mensaje (K$) DECLARE SUB espera (DEMORA AS SINGLE) DECLARE FUNCTION
GETTRACE& (host%, SUMAR%, NCAJ%) DECLARE SUB READHOST (NHOST%, NCAJ%, MODO%,
TICK%, lote%, TRAC AS LONG) DECLARE FUNCTION GetId$ (MODO%) DECLARE FUNCTION
ReqReverso$ (host%, req AS ANY) DECLARE FUNCTION MENRESP$ (CODRTA%, AUTNUM AS
LONG, REALIZADA%, Sacar%, Interno%) DECLARE FUNCTION ENVIAR% (K$, Cid%) DECLARE
SUB RECIBIR (K$, Cid%, info%) DECLARE SUB SHOWSTAT (Cid AS INTEGER) DECLARE SUB
SETBIT (MAPA$, p%) DECLARE SUB CopyStrToMem (K$, pointer AS LONG, lens AS
INTEGER)

REM $INCLUDE: 'x25lib.bi'
REM $INCLUDE: 'PINC.BI'
REM $INCLUDE: 'conce.bi'
REM $INCLUDE: 'x25dec.bi'

REM $DYNAMIC
FUNCTION BinToStr$ (Inbin$)
DIM bin(8), K$, i, j
bin(7) = 128
bin(6) = 64
bin(5) = 32
bin(4) = 16
bin(3) = 8
bin(2) = 4
bin(1) = 2
bin(0) = 1

K$ = ""
	FOR i = 1 TO LEN(Inbin$)
		FOR j = 7 TO 0 STEP -1
			IF ASC(MID$(Inbin$, i, 1)) AND bin(j) THEN
				K$ = K$ + "1"
			ELSE
				K$ = K$ + "0"
			END IF
		NEXT
	NEXT
	BinToStr$ = K$

END FUNCTION

REM $STATIC
SUB BORRAR (K$)
	DIM i, retr
	ON LOCAL ERROR RESUME NEXT
	i = 0
	IF DIR$(K$) = "" THEN EXIT SUB
sig:
	mensaje "BORRANDO " + K$
		DIM i1 AS RegTypeX, o1 AS RegTypeX
		DIM s1x AS STRING
		s1x = K$ + CHR$(0)
		i1.ax = &H4301
		i1.cx = 0
		i1.dx = SADD(s1x)
		i1.ds = SSEG(s1x)
		InterruptX &H21, i1, o1
		KILL K$
	IF DIR$(K$) <> "" THEN
		IF retr < 4 THEN
		  retr = retr + 1
		  espera .2 * retr
		  GOTO sig
		ELSE
			erro "no se pudo borrar " + K$
		END IF
	END IF
SIG2:
	ON LOCAL ERROR GOTO 0
EXIT SUB

nada:
i = i + 1
IF i < 5 THEN
	espera .1
	RESUME sig:
ELSE
	RESUME SIG2
END IF
END SUB

REM $DYNAMIC
FUNCTION Conectar% (remoteDte$, waitit AS INTEGER, BYVAL UDATA$)  'retorna cid o cero si hay error
	DIM r AS LONG, Cid AS INTEGER, K$
	DIM localdte$, remote$, wait2
	localdte$ = SPACE$(dteLen)
	IF INSTR(COMMAND$, "/P6") THEN
		LSET localdte$ = "45100203" + CHR$(0)
	ELSE
		LSET localdte$ = cormorandte$ + CHR$(0)
	END IF
	remote$ = SPACE$(dteLen)
	LSET remote$ = remoteDte$ + CHR$(0)
	StatMSg "Llamando " + LEFT$(RTRIM$(localdte$), LEN(RTRIM$(localdte$)) - 1) + " > " + remote$, 0
	K$ = "  "
	wait2 = ABS(waitit <> 0)
	IF UDATA$ = "" THEN
		r = XCALL%(SSEGADD(K$), 1, 1, 0, 0, 0, SSEGADD(remote$), SSEGADD(localdte$), 0)
	ELSE
		UDATA$ = MKI$(LEN(UDATA$)) + UDATA$ + STRING$(32, 0)
		r = XCALL%(SSEGADD(K$), 1, 1, 0, 0, SSEGADD(UDATA$), SSEGADD(remote$), SSEGADD(localdte$), 0)
	END IF
	IF r <> 0 THEN
		ShowERr

		Conectar% = -1
	ELSE
		StatMSg "Conexion Efectuada!! " + remoteDte$, 0
		Conectar% = CVI(K$)
	END IF
END FUNCTION

FUNCTION decodeRTa$ (K$, resp$())
	DIM i, MAPA$, curpos, ii, o$, OutStr$
	DIM RES$, LARGO, lbcd, bytes, bcdtmp$
	FOR i = 1 TO UBOUND(resp$)
		resp$(i) = ""
	NEXT
	IF K$ = "" THEN
		decodeRTa$ = ""
		EXIT FUNCTION
	END IF
	ON LOCAL ERROR GOTO malrta
	MAPA$ = BinToStr$(MID$(K$, 8, 8))
	curpos = 6 + 8 + 2
	
	o$ = "mensaje " + hexa$(ASC(MID$(K$, 6, 1))) + hexa$(ASC(MID$(K$, 7, 1))) + CHR$(13)
	FOR ii = 1 TO 64
		OutStr$ = ""
		IF MID$(MAPA$, ii, 1) = "1" THEN
			  SELECT CASE ii
				CASE 2'
					RES$ = "TARJETA"
					GOSUB llvarbcd
				CASE 3 ' cod proc
					RES$ = "PROCESO"
					lbcd = 6
					GOSUB Bcd
				CASE 4 ' cod proc
					RES$ = "IMPORTE"
					lbcd = 12
					GOSUB Bcd
				CASE 7
					RES$ = "FECHA"
					lbcd = 10
					GOSUB Bcd
				CASE 11' trace oc
					RES$ = "N.TRACE"
					lbcd = 6
					GOSUB Bcd
				CASE 12' cod proc
					RES$ = "HORA LOC"
					lbcd = 6
					GOSUB Bcd
				CASE 13'
					RES$ = "FEC. LOC"
					lbcd = 4
					GOSUB Bcd
				CASE 14'
					RES$ = "EXP-DATE"
					lbcd = 4
					GOSUB Bcd
				CASE 15'                 mmdd
					RES$ = "FEC.CIER"
					lbcd = 4
					GOSUB Bcd

				CASE 17'                 mmdd
					RES$ = "FEC.CAP."
					lbcd = 4
					GOSUB Bcd
				CASE 22'
					RES$ = "MODO ING"
					lbcd = 3
					GOSUB Bcd
				CASE 24'
					RES$ = "ID.RED"
					lbcd = 3
					GOSUB Bcd
				CASE 25'
					RES$ = "COND.POS"
					lbcd = 2
					GOSUB Bcd
				CASE 34'
					RES$ = "NROCTA"
					GOSUB LLVAR
                                        Writehst "34:" + RES$ + OutStr$

				CASE 35'
					RES$ = "TRACK II"
					GOSUB llvarbcd
				CASE 37'
					RES$ = "RETREF NUM"
					LARGO = 12
					GOSUB ALFA
				CASE 38'
					RES$ = "COD.AUT."
					LARGO = 6
					GOSUB ALFA
				CASE 39'
					RES$ = "COD.RTA."
					LARGO = 2
					GOSUB ALFA

				CASE 41'
					RES$ = "ID.TERM"
					LARGO = 8
					GOSUB ALFA
				CASE 42'
					RES$ = "ID COM"
					LARGO = 15
					GOSUB ALFA
				CASE 48'
					RES$ = "CUOT.ORIG "
					GOSUB LLLVAR
				CASE 46'
						RES$ = "TRAKCI NO LEIDO "
						GOSUB LLLVAR
				CASE 49'
					RES$ = "MONEDA"
					'lbcd = 3
					'GOSUB bcd
					LARGO = 3
					GOSUB ALFA
				CASE 52
					RES$ = "WKEY"
					lbcd = 16
					GOSUB Bcd
				CASE 54
					RES$ = "IMP.ADIC"
					GOSUB LLLVAR
				CASE 55'
					RES$ = "COD.SEG "
					GOSUB LLLVAR
				CASE 59'

					RES$ = "59.VAR"
                                        GOSUB llLvar
					Writehst "59:" + RES$ + OutStr$
				CASE 60'
					RES$ = "VERSION "
					GOSUB LLLVAR
				CASE 62'
					RES$ = "TICKET"
					GOSUB LLLVAR
				CASE 63'
					RES$ = "MENSAJE"
					GOSUB LLLVAR
				CASE 45'
						RES$ = "TRACK 1"
						GOSUB LLVAR
				CASE ELSE
					RES$ = "CAMPO" + STR$(i) + "NO IMPLE"

			  END SELECT
			  resp$(ii) = OutStr$
			  o$ = o$ + UCASE$(LEFT$(RES$ + SPACE$(20), 20)) + ":" + OutStr$
			  'IF ii = 39 THEN o$ = " " + MENRESP$(VAL(OutStr$), 0, 0, 0, 0)
			  IF ii = 39 THEN o$ = o$ + " " + MENRESP$(VAL(OutStr$), 0, 0, 0, 0)
			  o$ = o$ + CHR$(13) + CHR$(10)
		END IF

	NEXT
	IF curpos <> LEN(K$) + 1 THEN o$ = o$ + "largo:" + STR$(curpos) + "/" + STR$(LEN(K$)) + CHR$(13)
resal:
	ON LOCAL ERROR GOTO 0
	decodeRTa$ = o$

EXIT FUNCTION

malrta:
	o$ = o$ + "error"
RESUME resal:

ALFA:
  OutStr$ = MID$(K$, curpos, LARGO)
  curpos = curpos + LARGO
RETURN

llvarbcd:
  LARGO = VAL(hexa$(ASC(MID$(K$, curpos, 1))))
  curpos = curpos + 1
  lbcd = LARGO
  GOSUB Bcd
RETURN


LLVAR:
  LARGO = VAL(hexa$(ASC(MID$(K$, curpos, 1))))
  OutStr$ = MID$(K$, curpos + 1, LARGO)
  curpos = curpos + LARGO + 1
RETURN

LLLVAR:
  LARGO = VAL(hexa$(ASC(MID$(K$, curpos, 1))) + hexa$(ASC(MID$(K$, curpos + 1, 1))))
  OutStr$ = MID$(K$, curpos + 2, LARGO)
  curpos = curpos + LARGO + 2
RETURN


Bcd:
  bytes = INT((lbcd + 1) / 2)
  bcdtmp$ = ""
  FOR i = 1 TO bytes
	 bcdtmp$ = bcdtmp$ + hexa$(ASC(MID$(K$, curpos + i - 1, 1)))
  NEXT
  OutStr$ = bcdtmp$
  curpos = curpos + bytes
RETURN
END FUNCTION

FUNCTION ENVIAR (K$, Cid)
	DIM lens AS INTEGER, r2
	' EN LAS NO-WAIT NO HAY QUE MODIFICAR BUF$
	IF K$ = "" THEN EXIT FUNCTION

	K$ = LEFT$(K$, lpbuft)
	CopyStrToMem K$, pbuft, lens

	r2 = X25SEND(Cid, pbuft, lens, 0, 0)
	IF r2 THEN ShowERr
	ENVIAR = r2
END FUNCTION

SUB erro (K$)
	DIM nada$
	SOUND 1000, 1
COLOR 15, 0
mensaje "ERROR:" + K$
COLOR 7, 0
'LINE INPUT nada$
END SUB

REM $STATIC
SUB espera (DEMORA AS SINGLE)
	DIM t AS SINGLE
	t = TIMER
	DO
	LOOP WHILE TIMER - t! < DEMORA
END SUB

FUNCTION fechamal (K$)
	DIM d1 AS LONG, o1, o2, o3
	IF MID$(K$, 1, 2) = "00" THEN
		K$ = MID$(r.FECORI, 5, 2) + MID$(r.FECORI, 3, 2) + MID$(r.FECORI, 1, 2)
	END IF
	o1 = VAL(MID$(K$, 5, 2))
	IF o1 < 30 THEN o1 = o1 + 2000 ELSE o1 = o1 + 1900
	o2 = VAL(MID$(K$, 3, 2))
	o3 = VAL(MID$(K$, 1, 2))
	IF (o1 > 2010 OR o1 < 1998) OR o2 = 0 OR o3 = 0 THEN
		  mensaje "fechamal:" + K$
		  fechamal = -1
	ELSE
		d1 = DATESERIAL(o1, o2, o3)
		IF MONTH(d1) <> o2 OR DAY(d1) <> o3 THEN
			mensaje "fechamal2:" + K$
			fechamal = -1
		END IF
	END IF
END FUNCTION

FUNCTION FMTPLAN$ (N AS INTEGER)
IF N < 10 THEN
	FMTPLAN$ = FORMAT$(N, "0")
ELSE
	FMTPLAN$ = CHR$(N)
END IF
END FUNCTION

REM $DYNAMIC
FUNCTION FormatStr$ (K$)
	DIM k2$, k3$, i, k4$, j, k5
	k2$ = StrToHEx$(K$)
	k3$ = ""
	FOR i = 1 TO LEN(k2$) STEP 12 * 3
		k4$ = MID$(k2$, i, 12 * 3)
		k3$ = k3$ + LEFT$(k4$ + SPACE$(39), 39) + "   >"
		FOR j = 1 TO 16
			IF j * 3 > LEN(k4$) THEN
				k5 = 32
			ELSE
				k5 = (VAL("&H" + MID$(k4$, j * 3 - 2, 3)))
				IF k5 < 32 THEN k5 = 46
			END IF
			k3$ = k3$ + CHR$(k5)
		NEXT
		k3$ = k3$ + "<" + CHR$(13)' + CHR$(10)
	NEXT
	FormatStr$ = k3$
END FUNCTION

REM $STATIC
FUNCTION getnii (host AS INTEGER) AS STRING
	 IF host = 2 THEN           ' visa cierre unificado
		 getnii = NUMTOBCD$(112, 3)
	 ELSE
		 IF host = DINHOS THEN
			 getnii = NUMTOBCD$(8, 3)
		 ELSE
						 IF INSTR(COMMAND$, "/TE") THEN
								getnii = NUMTOBCD$(9, 3)

						 ELSE
								getnii = NUMTOBCD$(3, 3)
						 END IF
				 END IF
		END IF
END FUNCTION

REM $DYNAMIC
FUNCTION GetTpdu$ (HOST2)
  IF Notest THEN
	GetTpdu$ = NUMTOBCD$(1, 10)
  ELSE
	GetTpdu$ = NUMTOBCD$(6000030000@, 10)
		'  GetTpdu$ = NUMTOBCD$(6000068000@, 10)
  END IF
END FUNCTION

FUNCTION GETTRACE& (HOST3, SUMAR, NCAJ)
'    DIM K$, T, ntr&
'    K$ = "X25Trace." + FORMAT$(HOST, "000")
'    t = FREEFILE
'    IF DIR$(K$) <> "" THEN
'        OPEN K$ FOR INPUT AS t
'            INPUT #t, ntr&
'        CLOSE t
'    ELSE
'        ntr& = 0
'    END IF
'    IF sumar THEN ntr& = ntr& + 1
'    OPEN K$ FOR OUTPUT AS t
'                PRINT #t, ntr&
'    CLOSE t
'    GETTRACE& = ntr&
	 DIM t, NTR&
	 IF SUMAR THEN t = 2 ELSE t = 0
	 READHOST HOST3, NCAJ, t, 0, 0, NTR&
	 GETTRACE& = NTR&
END FUNCTION

FUNCTION hexa$ (BYVAL N%)
hexa$ = RIGHT$("00" + HEX$(N%), 2)
END FUNCTION

SUB HexToStr (Inhex$, OutStr$)
	DIM i
	OutStr$ = ""
	FOR i = 1 TO LEN(Inhex$) STEP 3
		OutStr$ = OutStr$ + CHR$(VAL("&H" + MID$(Inhex$, i, 2)))
	NEXT
END SUB

SUB MakeEchoTEST (K$, idhost)
	DIM TPDU$, MAPA$, t, k2$
	TPDU$ = GetTpdu$(idhost)
	K$ = TPDU$ + NUMTOBCD$(800, 4)

	 MAPA$ = ""
	 SETBIT MAPA$, 3
	 SETBIT MAPA$, 7
	 SETBIT MAPA$, 11
	 SETBIT MAPA$, 24
	 SETBIT MAPA$, 41
	 SETBIT MAPA$, 42

	 ' mapa de bits
	 K$ = K$ + STRtOBIN$(MAPA$)

	 ' codigo de procesamiento echotest
	 K$ = K$ + NUMTOBCD$(990000, 6)

	 ' FECHAHORA
	 K$ = K$ + NUMTOBCD$(VAL(FORMAT$(NOW, "mmddhhmmss")), 10)

	 IF DIR$("conce.ech") = "" THEN
		 ' TRACE
		 K$ = K$ + NUMTOBCD$(GETTRACE&(idhost, -1, 0), 6)
		 ' IDENT INT.RED
		 K$ = K$ + getnii(idhost)
		 ' ID TERMINAL
		 K$ = K$ + "75801001"
		 ' ID COMERCIO
		 K$ = K$ + "51272001       "

	 ELSE
		 t = FREEFILE
		 k2$ = "conce.ech"
		 IF idhost > 1 THEN
			   k2$ = "CONCE.EC" + LTRIM$(STR$(idhost))
		 END IF
		 OPEN k2$ FOR INPUT AS t
			 ' TRACE
			 LINE INPUT #t, k2$
			 K$ = K$ + NUMTOBCD$(GETTRACE&(idhost, -1, VAL(k2$)), 6)

			 ' IDENT INT.RED
			 K$ = K$ + getnii(idhost)

			 ' ID TERMINAL
			 LINE INPUT #t, k2$
			 K$ = K$ + LEFT$(k2$ + SPACE$(8), 8)

			 ' ID COMERCIO
			 LINE INPUT #t, k2$
			 K$ = K$ + LEFT$(k2$ + SPACE$(15), 15)

		 CLOSE t
	 END IF


END SUB

FUNCTION MENRESP$ (CODRTA, AUTNUM AS LONG, REALIZADA, Sacar, Interno)
	REALIZADA = 0
	Sacar = -1
	Interno = 0
	SELECT CASE CODRTA
		CASE 0
			MENRESP$ = "APROBADA" + STR$(AUTNUM)
			REALIZADA = -1
		CASE 1, 2
			MENRESP$ = "PEDIR AUTORIZACION"
		CASE 3
			MENRESP$ = "COMERCIO INVALIDO"
		CASE 4
			MENRESP$ = "CAPTURAR TARJETA"
		CASE 5
			MENRESP$ = "DENEGADA"
		CASE 7
			MENRESP$ = "RETENGA Y LLAME"
		CASE 11
			MENRESP$ = "APROBADA"
			REALIZADA = -1
		CASE 12
			MENRESP$ = "TRANSAC.INVALIDA"
		CASE 13
			MENRESP$ = "MONTO INVALIDA"
		CASE 14
			MENRESP$ = "TARJETA INVALIDA"
		CASE 25
			MENRESP$ = "NO EXISTE ORIGINAL"
		CASE 30
			MENRESP$ = "ERROR EN FORMATO"
			Interno = -1
		CASE 38
			MENRESP$ = "EXCEDE ING. DE PIN"
		CASE 43
			MENRESP$ = "RETENER TARJETA"
		CASE 45
			MENRESP$ = "NO OPERA EN CUOTAS"
		CASE 46
			MENRESP$ = "TARJETA NO VIGENTE"
		CASE 47
			MENRESP$ = "PIN REQUERIDO"
		CASE 48
			MENRESP$ = "EXCEDE MAX. CUOTAS"
		CASE 49
			MENRESP$ = "ERROR FECHA VENC."
		CASE 51
			MENRESP$ = "FONDOS INSUFICIENTES"
		CASE 53
			MENRESP$ = "CUENTA INEXISTENTE"
		CASE 54
			MENRESP$ = "TARJETA VENCIDA"
		CASE 55
						MENRESP$ = "PIN.INCORRECTO"
		CASE 56
			MENRESP$ = "TARJ.NO HABILITADA"
		CASE 57
			MENRESP$ = "TRANS.NO PERMITIDA"
		CASE 58
			MENRESP$ = "SERVICIO INVALIDO"
		CASE 61
			MENRESP$ = "EXCEDE LIMITE"
		CASE 65
			MENRESP$ = "EXCEDE LIM.TARJETA"
		CASE 76
			MENRESP$ = "LLAMAR AL EMISOR P/AUT"
		CASE 77
			MENRESP$ = "ERROR PLAN/CUOTAS"
		CASE 85
			MENRESP$ = "APROBADA"
			REALIZADA = -1
		CASE 89
			MENRESP$ = "TERMINAL INVALIDA"
		CASE 91
			MENRESP$ = "EMISOR FUERA LINEA"
		CASE 94
			MENRESP$ = "NRO.TRACE DUPLIC."
		CASE 95
			MENRESP$ = "RETRANSMITIENDO   "
		CASE 96
			MENRESP$ = "ERROR EN SISTEMA"
		case 98 
		  menresp$ ="RECHAZADA POR IMP.DISP. P/ CASHBACK"
		CASE ELSE
			MENRESP$ = "RECHAZADA" + STR$(CODRTA)
	END SELECT
END FUNCTION

SUB mensaje (K$)
		IF INSTR(K$, "COD.SEG") AND INSTR(COMMAND$, "/ARRE") = 0 THEN
				PRINT "CODSEG ********"
		ELSE
				PRINT TIME$ + " " + K$
				Writehst K$
		END IF
END SUB

FUNCTION NUMTOBCD$ (N#, BYVAL DIG)
	DIM K$, Z$, i, byte, z2$, z1$
	DIG = INT((DIG + 1) / 2)
	K$ = RIGHT$(FORMAT$(N#, "00000000000000000000"), DIG * 2)
	Z$ = ""
	FOR i = 1 TO DIG
		z1$ = MID$(K$, i * 2 - 1, 1)
		z2$ = MID$(K$, i * 2, 1)
		byte = INSTR("123456789", z1$) * 16
		byte = byte + INSTR("123456789", z2$)
		Z$ = Z$ + CHR$(byte)
	NEXT
	NUMTOBCD$ = Z$
END FUNCTION

REM $STATIC
FUNCTION numtobcd2$ (KK$, BYVAL DIG)
	DIM K$, Z$, i, byte, z2$, z1$
	DIG = INT((DIG + 1) / 2)'
	K$ = LEFT$(KK$ + STRING$(DIG, "0"), DIG * 2)
	Z$ = ""
	FOR i = 1 TO DIG
		z1$ = MID$(K$, i * 2 - 1, 1)
		z2$ = MID$(K$, i * 2, 1)
		byte = INSTR("123456789@@@=", z1$) * 16
		byte = byte + INSTR("123456789@@@=", z2$)
		Z$ = Z$ + CHR$(byte)
	NEXT
	numtobcd2$ = NUMTOBCD$(LEN(RTRIM$(KK$)), 2) + Z$
END FUNCTION

FUNCTION numtobcdl$ (KK$, BYVAL DIG)
	DIM K$, Z$, i, byte, z2$, z1$
	DIG = INT((DIG + 1) / 2)' alterado para track2
	K$ = LEFT$(KK$ + STRING$(DIG, "0"), DIG * 2)
	Z$ = ""
	FOR i = 1 TO DIG
		z1$ = MID$(K$, i * 2 - 1, 1)
		z2$ = MID$(K$, i * 2, 1)
		byte = INSTR("123456789@@@=", z1$) * 16
		byte = byte + INSTR("123456789@@@=", z2$)
		Z$ = Z$ + CHR$(byte)
	NEXT
	numtobcdl$ = NUMTOBCD$(37, 2) + Z$
END FUNCTION

REM $DYNAMIC
FUNCTION packBcd$ (K$)
	packBcd$ = NUMTOBCD$(LEN(K$) * 2, 2) + K$

END FUNCTION

FUNCTION PACKSTR$ (K$)
	PACKSTR$ = NUMTOBCD$(LEN(K$), 2) + K$
END FUNCTION

FUNCTION Packstrl$ (K$)
	Packstrl$ = NUMTOBCD$(LEN(K$), 3) + K$
END FUNCTION

SUB PAUSa (K$)
	StatMSg "Presione una tecla para " + K$, 0
	LINE INPUT K$
END SUB

REM $STATIC
FUNCTION pin$ (KK$)
	DIM K$, Z$, i, byte, z2$, z1$, DIG AS INTEGER
		DIG = 8' alterado para track2
		K$ = LEFT$(KK$ + "0000000000000000", DIG * 2)
				'WRITEHST "PIN:" + KK$
		Z$ = ""
	FOR i = 1 TO DIG
		z1$ = MID$(K$, i * 2 - 1, 1)
		z2$ = MID$(K$, i * 2, 1)
				byte = INSTR("123456789ABCDEF", z1$) * 16
																'WRITEHST " B:" + STR$(byte) + " " + HEX$(byte)
								byte = byte + INSTR("123456789ABCDEF", z2$)
																'WRITEHST " B:" + STR$(byte) + " " + HEX$(byte)
				Z$ = Z$ + CHR$(byte)
	NEXT
		pin$ = Z$
END FUNCTION

REM $DYNAMIC
SUB RECIBIR (K$, Cid, infox)
	DIM p1 AS LONG, sta AS x25stat
	DIM r AS INTEGER, ki$, errox
		infox = 0
		K$ = ""
		r = x25status(Cid, sta, 0)
		IF r THEN
						  mensaje "x25 err" + STR$(r)
						  ShowERr
			  EXIT SUB
		END IF
		 IF sta.pktread > 0 THEN

			ki$ = MKL$(0)
			r = X25RECV(Cid, pbuf1, lpbuf1, SSEGADD(ki$), 0)
			errox = X25ERROR()

			IF errox THEN
								 mensaje "err2"
								 ShowERr
			ELSE
				K$ = MemtoStr$(pbuf1, r)
			END IF
			infox = CVI(ki$)
		 ELSE
			 infox = -1
		 END IF
END SUB

FUNCTION ReqBatch$ (req AS reqinfo)
	 DIM TPDU$, MAPA$, K$, tmp$, modoin, codcta$
	 TPDU$ = GetTpdu$(req.host)
	 K$ = TPDU$ + NUMTOBCD$(320, 4)
	 MAPA$ = ""
		IF req.MANUAL <> 0 OR mandalo THEN SETBIT MAPA$, 2
	 SETBIT MAPA$, 3
	 SETBIT MAPA$, 4
	 SETBIT MAPA$, 7
	 SETBIT MAPA$, 11
	 SETBIT MAPA$, 12
	 SETBIT MAPA$, 13
	IF req.MANUAL THEN SETBIT MAPA$, 14
	 SETBIT MAPA$, 17
	 SETBIT MAPA$, 22
	 SETBIT MAPA$, 24
	 SETBIT MAPA$, 25
	 IF req.MANUAL = 0 THEN SETBIT MAPA$, 35
	 SETBIT MAPA$, 37
	 SETBIT MAPA$, 38
	 SETBIT MAPA$, 41
	 SETBIT MAPA$, 42

	 SETBIT MAPA$, 48
	 SETBIT MAPA$, 49
	 'IF RTRIM$(req.codseg) > "" THEN setbit mapa$, 55
	 SETBIT MAPA$, 60
	 SETBIT MAPA$, 62
	 SETBIT MAPA$, 63

	 ' mapa de bits
	 K$ = K$ + STRtOBIN$(MAPA$)

	 ' numero de tarjeta
	IF req.MANUAL OR mandalo THEN
		'K$ = K$ + packBcd$(NUMTOBCD(VAL(req.tarj), LEN(RTRIM$(req.tarj))))
		IF LEN(RTRIM$(req.TARJ)) <> 16 THEN
			K$ = K$ + (numtobcd2((req.TARJ), LEN(RTRIM$(req.TARJ))))
		ELSE
			K$ = K$ + packBcd$(NUMTOBCD(VAL(req.TARJ), LEN(RTRIM$(req.TARJ))))
		END IF
	 END IF
	 tmp$ = req.codpro
	 IF req.ULTIMO = 0 THEN MID$(tmp$, 6, 1) = "1"
	 K$ = K$ + NUMTOBCD$(VAL(tmp$), 6)

	 ' importe
	 K$ = K$ + NUMTOBCD$(INT(req.IMPO * 100), 12)

	 ' FECHAHORA
	 K$ = K$ + NUMTOBCD$(VAL(FORMAT$(NOW, "mmddhhmmss")), 10)

	 ' TRACE
	 req.trace = GETTRACE&(req.host, -1, VAL(req.sufijo))
	 K$ = K$ + NUMTOBCD$((req.trace), 6)

	 ' FECHAHORA 2
	 K$ = K$ + NUMTOBCD$(VAL(FORMAT$(req.fechahora, "hhmmssmmdd")), 10)

	 ' expdate aamm
	IF req.MANUAL THEN K$ = K$ + NUMTOBCD$(VAL(req.EXPDATE), 4)

	 ' FECHA DE CAPTURA
	 K$ = K$ + NUMTOBCD$(VAL(req.FECCAP), 4)

	 IF req.MANUAL THEN
		modoin = 12
	 ELSE

		modoin = 22
	 END IF

	 ' modo ingreso
	 K$ = K$ + NUMTOBCD$((modoin), 3)

	 ' IDENT INT.RED
	 K$ = K$ + getnii(req.host)

	 ' condicion de la pos
	 K$ = K$ + NUMTOBCD$(0, 2)

	 ' track2
	 IF req.MANUAL = 0 THEN
		 K$ = K$ + numtobcdl$(req.TRACK2, LEN(req.TRACK2))
	 END IF

	 ' retr.ref .number
		 K$ = K$ + LEFT$(LTRIM$(req.RETREF) + SPACE$(12), 12)

	 ' COD.AUTORIZA  '6
	 K$ = K$ + FORMAT$(VAL(req.CodAut), "000000")

	 ' ID TERMINAL
	 K$ = K$ + req.idter         'REQ.IDTER

	 ' ID COMERCIO
	 K$ = K$ + req.Idcom      'REQ.IDCOM


	 ' cuotas
	 codcta$ = FMTPLAN$(req.PLAN) + FORMAT$(req.CUOTAS, "00")
	 IF LEFT$(req.codpro, 2) = "20" THEN
		codcta$ = codcta$ + FORMAT$(req.TKTORI, "0000") + req.FECORI$
	 END IF
	 K$ = K$ + Packstrl$(codcta$)

	' codigo moneda
	' k$ = k$ + NUMTOBCD$(32, 3)
		 IF req.MONEDA = "B" THEN
				K$ = K$ + "993"
		 ELSE
		 IF req.MONEDA = "D" THEN
		K$ = K$ + "840"
	 ELSE
		K$ = K$ + "032"
	 END IF
		 END IF
	 'codigo seg
	 IF RTRIM$(req.CODSEG$) <> "" THEN
  '      k$ = k$ + Packstrl$(RTRIM$(req.codseg$))
	 END IF

	 ' Version Software
	 K$ = K$ + Packstrl$(VERSOFT$)

	 ' nro ticket
	 K$ = K$ + Packstrl$(FORMAT$(req.TICKET, "0000"))

	 ' trans orig + trace orig
	 K$ = K$ + Packstrl$(FORMAT$(req.MENORI, "0000") + FORMAT$(req.TRAORI, "000000"))

	 ReqBatch$ = K$
END FUNCTION

FUNCTION reqCIerre$ (req AS reqinfo, cierre AS CIERREINFO)
	 DIM TPDU$, MAPA$, K$, cietmp$
	 TPDU$ = GetTpdu$(req.host)
	 K$ = TPDU$ + NUMTOBCD$(500, 4)

	 MAPA$ = ""
	 SETBIT MAPA$, 3
	 SETBIT MAPA$, 7
	 SETBIT MAPA$, 11
	 SETBIT MAPA$, 15
	 SETBIT MAPA$, 24
	 SETBIT MAPA$, 41
	 SETBIT MAPA$, 42
	 SETBIT MAPA$, 60
	 SETBIT MAPA$, 63

	 ' mapa de bits
	 K$ = K$ + STRtOBIN$(MAPA$)

	 IF cierre.DIFE THEN
		K$ = K$ + NUMTOBCD$(960000, 6)
	 ELSE
		K$ = K$ + NUMTOBCD$(920000, 6)
	 END IF
	 ' FECHAHORA
	 K$ = K$ + NUMTOBCD$(VAL(FORMAT$(NOW, "mmddhhmmss")), 10)

	 ' TRACE
	 K$ = K$ + NUMTOBCD$(GETTRACE&(req.host, -1, VAL(req.sufijo)), 6)

	 IF cierre.feccierre < "0000" THEN cierre.feccierre = FORMAT$(NOW, "mmdd")
	 ' cierre
	 K$ = K$ + NUMTOBCD$(VAL(cierre.feccierre), 3)

	 ' IDENT INT.RED
	 K$ = K$ + getnii(req.host)

	 ' ID TERMINAL
	 K$ = K$ + req.idter  'REQ.IDTER

	 ' ID COMERCIO
	 K$ = K$ + req.IDCIE      'REQ.IDCIE

	 ' Version Software
	 K$ = K$ + Packstrl$(VERSOFT$)

	 cietmp$ = FORMAT$(cierre.BATCH, "000")

	 cietmp$ = cietmp$ + FORMAT$(cierre.COMPRAS, "0000")
	 cietmp$ = cietmp$ + FORMAT$(INT(cierre.MONTO * 100), "000000000000")

	 cietmp$ = cietmp$ + FORMAT$(cierre.DEVOL, "0000")
	 cietmp$ = cietmp$ + FORMAT$(INT(cierre.MONTODEV * 100), "000000000000")

	 cietmp$ = cietmp$ + FORMAT$(cierre.ANUL, "0000")
	 cietmp$ = cietmp$ + FORMAT$(INT(cierre.MONTOANUL * 100), "000000000000")

	 K$ = K$ + Packstrl$(cietmp$)

	 reqCIerre$ = K$
END FUNCTION

FUNCTION ReqOffLine$ (oper, req AS reqinfo)
	DIM modoin, TPDU$, MAPA$, codcta$, K$
	DIM t1no AS INTEGER

	t1no = LTRIM$(RTRIM$(req.TRACK1)) = ""
		 ' emparchadiores
		 req.CODSEG = ""
		 IF req.oper = OPANUCPA THEN
			  req.CodAut = "000000"
		 END IF

	 TPDU$ = GetTpdu$(req.host)
	 K$ = TPDU$ + NUMTOBCD$(220, 4)
	 req.MENORI = 220
	 MAPA$ = ""
	 DIM T1 AS INTEGER

	 T1 = (req.MANUAL = 0 AND req.t1req <> 0)
	 IF T1 = 0 THEN
		SETBIT MAPA$, 2
	 END IF
	 SETBIT MAPA$, 3
	 SETBIT MAPA$, 4
	 SETBIT MAPA$, 7
	 SETBIT MAPA$, 11
	 SETBIT MAPA$, 12
	 SETBIT MAPA$, 13
	 SETBIT MAPA$, 14

	 SETBIT MAPA$, 17

	 SETBIT MAPA$, 22
	 SETBIT MAPA$, 24
	 SETBIT MAPA$, 25
	 
	 ' track 2
	 IF req.MANUAL = 0 THEN SETBIT MAPA$, 35
	 SETBIT MAPA$, 38

	 SETBIT MAPA$, 41
	 SETBIT MAPA$, 42

		 't1=maestro
		 IF T1 THEN
				IF t1no THEN
					 SETBIT MAPA$, 46
				ELSE
					 SETBIT MAPA$, 45
				END IF
		 END IF


	 SETBIT MAPA$, 48
	 SETBIT MAPA$, 49

	 IF oper = 2 OR oper = 3 THEN SETBIT MAPA$, 37

	 IF RTRIM$(req.CODSEG) > "" THEN
		SETBIT MAPA$, 55
	 END IF

	 SETBIT MAPA$, 60
	 ' numero de ticket
	 SETBIT MAPA$, 62

	 ' mapa de bits
	 K$ = K$ + STRtOBIN$(MAPA$)

	 ' numero de tarjeta
	 ' K$ = K$ + packBcd$(NUMTOBCD(VAL(req.tarj), LEN(RTRIM$(req.tarj))))
	 IF T1 THEN

	 ELSE
		 IF LEN(RTRIM$(req.TARJ)) <> 16 THEN
			 K$ = K$ + (numtobcd2((req.TARJ), LEN(RTRIM$(req.TARJ))))
		 ELSE
			 K$ = K$ + packBcd$(NUMTOBCD(VAL(req.TARJ), LEN(RTRIM$(req.TARJ))))
		 END IF
	 END IF


	 SELECT CASE oper
		 CASE 0  ' codigo de procesamiento compra req
			K$ = K$ + NUMTOBCD$(0, 6)
			req.codpro = "000000"
		 CASE 1  ' codigo de procesamiento compra req
			K$ = K$ + NUMTOBCD$(200000, 6)
			req.codpro = "200000"
		 CASE 2  ' codigo de procesamiento compra req
			K$ = K$ + NUMTOBCD$(20000, 6)
			req.codpro = "020000"
		 CASE 3  ' codigo de procesamiento compra req
			K$ = K$ + NUMTOBCD$(220000, 6)
			req.codpro = "220000"
	 END SELECT
	 ' importe
	 K$ = K$ + NUMTOBCD$(INT(req.IMPO * 100), 12)

	 ' FECHAHORA
	 K$ = K$ + NUMTOBCD$(VAL(FORMAT$(NOW, "mmddhhmmss")), 10)

	 ' TRACE
	 req.trace = GETTRACE&(req.host, -1, VAL(req.sufijo))
	 req.TRAORI = req.trace
	 K$ = K$ + NUMTOBCD$((req.trace), 6)

	 ' FECHAHORA 2
	 K$ = K$ + NUMTOBCD$(VAL(FORMAT$(req.fechahora, "hhmmssmmdd")), 10)

	 ' expdate aamm
	 K$ = K$ + NUMTOBCD$(VAL(req.EXPDATE), 4)

	 ' FECHA DE CAPTURA
	 K$ = K$ + NUMTOBCD$(VAL(req.FECCAP), 4)


	 IF req.MANUAL THEN
		modoin = 12
	 ELSE
		modoin = 22
	 END IF

	 ' modo ingreso
	 K$ = K$ + NUMTOBCD$((modoin), 3)

	 ' IDENT INT.RED
	 K$ = K$ + getnii(req.host)

	 ' condicion de la pos
	 K$ = K$ + NUMTOBCD$(0, 2)

	 ' track2
	  IF req.MANUAL = 0 THEN
		 K$ = K$ + numtobcdl$(req.TRACK2, LEN(req.TRACK2))
	  END IF

	' retr.ref .number (para anulacion)
	 IF oper = 2 OR oper = 3 THEN
		   K$ = K$ + LEFT$(req.RETREF + SPACE$(12), 12)
	 END IF

	 ' COD.AUTORIZA   para offline
	 K$ = K$ + FORMAT$(VAL(req.CodAut), "000000")
	
	 ' ID TERMINAL
	 K$ = K$ + req.idter         'REQ.IDTER

	 ' ID COMERCIO
	 K$ = K$ + req.Idcom      'REQ.IDCOM

		 ' track1  track 1 track i
		 IF T1 THEN
				IF t1no THEN
					 K$ = K$ + Packstrl$("1")
				ELSE
					DIM JU AS INTEGER
					FOR JU = LEN(req.TRACK1) TO 1 STEP -1
						IF MID$(req.TRACK1, JU, 1) = "?" THEN
							MID$(req.TRACK1, JU, 1) = " "
						END IF
					NEXT

					'mensaje "track i len:" + STR$(LEN(RTRIM$(req.track1)))
					IF r.host <> 2 THEN
						mensaje "B" + req.TRACK1
						K$ = K$ + PACKSTR$(RTRIM$("B" + LEFT$(req.TRACK1, 74)))
					ELSE
						mensaje "%b" + req.TRACK1
						K$ = K$ + PACKSTR$(RTRIM$("%b" + LEFT$(req.TRACK1, 74)))
					END IF
				END IF
		 END IF


	 ' cuotas
	 codcta$ = FMTPLAN$(req.PLAN) + FORMAT$(req.CUOTAS, "00")
	 IF oper = 1 THEN
		codcta$ = codcta$ + FORMAT$(req.TKTORI, "0000") + req.FECORI$
	 END IF
	 K$ = K$ + Packstrl$(codcta$)

	' codigo moneda
	' k$ = k$ + NUMTOBCD$(32, 3)
		 IF req.MONEDA = "B" THEN
				K$ = K$ + "993"
		 ELSE

		 IF req.MONEDA = "D" THEN
		K$ = K$ + "840"
	 ELSE
		K$ = K$ + "032"
	 END IF
		 END IF
	 'codigo seg
	 IF RTRIM$(req.CODSEG$) <> "" THEN
		K$ = K$ + Packstrl$(RTRIM$(req.CODSEG$))
	 END IF

	 ' Version Software
	 K$ = K$ + Packstrl$(VERSOFT$)

	 ' nro ticket
	 K$ = K$ + Packstrl$(FORMAT$(req.TICKET, "0000"))

	 ReqOffLine$ = K$
END FUNCTION

FUNCTION reqOnLine$ (oper, req AS reqinfo) ' 0-cpra 1-dev 2/3-anu 4 -SALDO
	DIM modoin, TPDU$, MAPA$, codcta$, K$
	DIM maestro AS INTEGER
	DIM t1no AS INTEGER

		t1no = LTRIM$(RTRIM$(req.TRACK1)) = ""
		 maestro = req.tipocta > 0
		TPDU$ = GetTpdu$(req.host)
		IF maestro THEN req.MANUAL = 0

		IF oper = 4 THEN
			 K$ = TPDU$ + NUMTOBCD$(100, 4)
			 req.MENORI = 100
		ELSE
			 K$ = TPDU$ + NUMTOBCD$(200, 4)
			 req.MENORI = 200
		END IF

	 MAPA$ = ""
	 IF req.MANUAL THEN SETBIT MAPA$, 2
	 SETBIT MAPA$, 3
	 IF oper <> 4 THEN SETBIT MAPA$, 4
	 SETBIT MAPA$, 7
	 SETBIT MAPA$, 11

	 IF oper <> 4 THEN
			SETBIT MAPA$, 12
			SETBIT MAPA$, 13
	 END IF

	 IF req.MANUAL THEN SETBIT MAPA$, 14
	 SETBIT MAPA$, 22
	 SETBIT MAPA$, 24
	 SETBIT MAPA$, 25
	 ' track 2
	 IF req.MANUAL = 0 THEN
			SETBIT MAPA$, 35
	 END IF
	 SETBIT MAPA$, 41
	 SETBIT MAPA$, 42

		 DIM T1 AS INTEGER
		 T1 = maestro <> 0 OR (req.MANUAL = 0 AND req.t1req <> 0)
		 't1=maestro
		 IF T1 THEN
				IF t1no THEN
					 SETBIT MAPA$, 46
				ELSE
					 SETBIT MAPA$, 45
				END IF
		 END IF
		 IF oper <> 4 THEN SETBIT MAPA$, 48
	 SETBIT MAPA$, 49
	 IF maestro THEN SETBIT MAPA$, 52

	 IF cashback(req) THEN SETBIT MAPA$, 54

	 IF oper = 2 OR oper = 3 THEN SETBIT MAPA$, 37
	 IF RTRIM$(req.CODSEG) > "" AND maestro = 0 THEN
		SETBIT MAPA$, 55
	 END IF
	 IF visaOn(req) THEN
		SETBIT MAPA$, 59
	 END IF
	 SETBIT MAPA$, 60
	 ' numero de ticket
	 IF oper <> 4 THEN SETBIT MAPA$, 62

	 ' mapa de bits
	 K$ = K$ + STRtOBIN$(MAPA$)

	 ' numero de tarjeta
	 IF req.MANUAL THEN
		IF LEN(RTRIM$(req.TARJ)) <> 16 THEN
			K$ = K$ + (numtobcd2((req.TARJ), LEN(RTRIM$(req.TARJ))))
		ELSE
			K$ = K$ + packBcd$(NUMTOBCD(VAL(req.TARJ), LEN(RTRIM$(req.TARJ))))
		END IF
	 END IF
' codigo procesamiento ------------------------
	 SELECT CASE oper
		 CASE 0  ' codigo de procesamiento compra req
			IF cashback(req) THEN
			   req.codpro = "090000"
			ELSE
				req.codpro = "000000"
			END IF
		 CASE 1  ' codigo de procesamiento compra req
				req.codpro = "200000"
		 CASE 2  ' codigo de procesamiento compra req
			IF cashback(req) THEN
				req.codpro = "140000"
			ELSE
				req.codpro = "020000"
			END IF
		 CASE 3  ' codigo de procesamiento compra req
			req.codpro = "220000"
		 CASE 4  ' codigo de procesamiento consulta
						req.codpro = "310000"
		 END SELECT
		 ' patch for maestro
		 SELECT CASE req.tipocta
				CASE 1
				   MID$(req.codpro, 3, 1) = "1"' Ca$
				CASE 2
				   MID$(req.codpro, 3, 1) = "2"' Cc$
				CASE 8
				   MID$(req.codpro, 3, 1) = "8"' Ca$
				CASE 9
				   MID$(req.codpro, 3, 1) = "9"' CC$
		 END SELECT
		 K$ = K$ + NUMTOBCD$(VAL(req.codpro), 6)

	 req.oper = oper
	 ' importe
	 IF oper <> 4 THEN K$ = K$ + NUMTOBCD$(INT(req.IMPO * 100), 12)

	 ' FECHAHORA
	 K$ = K$ + NUMTOBCD$(VAL(FORMAT$(NOW, "mmddhhmmss")), 10)

	 ' TRACE
	 req.trace = GETTRACE&(req.host, -1, VAL(req.sufijo))
	 req.TRAORI = req.trace
	 K$ = K$ + NUMTOBCD$((req.trace), 6)

	 ' FECHAHORA 2
	 IF oper <> 4 THEN K$ = K$ + NUMTOBCD$(VAL(FORMAT$(req.fechahora, "hhmmssmmdd")), 10)

	 ' expdate aamm
	 IF req.MANUAL THEN K$ = K$ + NUMTOBCD$(VAL(req.EXPDATE), 4)


	 IF req.MANUAL THEN
		modoin = 12
	 ELSE
		modoin = 22
				IF maestro THEN modoin = 21
	 END IF
				 'IF oper = 4 THEN modoin = modoin - 1
	 ' modo ingreso
	 K$ = K$ + NUMTOBCD$((modoin), 3)

	 ' IDENT INT.RED
	 ' para diners = 8
	 K$ = K$ + getnii(req.host)

	 ' condicion de la pos
	 K$ = K$ + NUMTOBCD$(0, 2)

	 ' track2
		 IF req.MANUAL = 0 THEN
	   ' K$ = K$ + PACKSTR$(REQ.TRACK2)
		   Writehst "SENDED TRACK2"
		   K$ = K$ + numtobcdl$(req.TRACK2, LEN(req.TRACK2))
		 END IF

	 ' retr.ref .number (para anulacion)
	 IF oper = 2 OR oper = 3 THEN
		   K$ = K$ + LEFT$(req.RETREF + SPACE$(12), 12)
	 END IF

	 ' ID TERMINAL
	 K$ = K$ + req.idter         'REQ.IDTER

	 ' ID COMERCIO
	 K$ = K$ + req.Idcom      'REQ.IDCOM



		 ' track1  track 1 track i
		 IF T1 THEN
				IF t1no THEN
					 K$ = K$ + Packstrl$("1")
				ELSE
					DIM JU AS INTEGER
					FOR JU = LEN(req.TRACK1) TO 1 STEP -1
						IF MID$(req.TRACK1, JU, 1) = "?" THEN
							MID$(req.TRACK1, JU, 1) = " "
						END IF
					NEXT

					'mensaje "track i len:" + STR$(LEN(RTRIM$(req.track1)))
					IF r.host <> 2 THEN
						mensaje "B" + req.TRACK1
						K$ = K$ + PACKSTR$(RTRIM$("B" + LEFT$(req.TRACK1, 74)))
					ELSE
						mensaje "%b" + req.TRACK1
						K$ = K$ + PACKSTR$(RTRIM$("%b" + LEFT$(req.TRACK1, 74)))
					END IF
				END IF
		 END IF

		 ' cuotas
		 IF oper <> 4 THEN
		 codcta$ = FMTPLAN$(req.PLAN) + FORMAT$(req.CUOTAS, "00")
		 IF oper = 1 THEN
				codcta$ = codcta$ + FORMAT$(req.TKTORI, "0000") + req.FECORI$
		 END IF
		 K$ = K$ + Packstrl$(codcta$)
		 END IF


	' codigo moneda
	' k$ = k$ + NUMTOBCD$(32, 3)
		  IF req.MONEDA = "B" THEN
				K$ = K$ + "993"
		  ELSE

				IF req.MONEDA = "D" THEN
						K$ = K$ + "840"
				ELSE
						K$ = K$ + "032"
				END IF
		 END IF
		 
	  
	 'codigo seg
	  IF oper <> 4 AND maestro = 0 THEN
		 IF RTRIM$(req.CODSEG$) <> "" THEN
					K$ = K$ + Packstrl$(RTRIM$(req.CODSEG$))
		 END IF
	  END IF

	  IF maestro THEN
		K$ = K$ + pin$(req.PINBLOCK)
	  END IF
	
                             ' cashback
          IF cashback(req) THEN
                        MENSAJE "CASHBACK "+STR$(REQ.CASHBACK)
                        K$ = K$ + Packstrl$(format$(Int(req.cashback*100),"000000000000"))
          END IF


	  ' indicador cta visa
	  IF visaOn(req) THEN
		   K$ = K$ + NUMTOBCD$(14, 3) + "002" + "0001" + "004" + "009" + "1"
	  END IF
	 ' Version Software
	 K$ = K$ + Packstrl$(VERSOFT$)

	 ' nro ticket
		 IF maestro THEN
				 IF oper <> 4 THEN K$ = K$ + Packstrl$(FORMAT$(req.TICKET, "0000"))
		 ELSE
				' TODOS
				IF oper <> 4 THEN K$ = K$ + Packstrl$(FORMAT$(req.TICKET, "0000"))
		 END IF
	 reqOnLine$ = K$
END FUNCTION

FUNCTION ReqReverso$ (HOST4, req AS reqinfo)
	DIM modoin, TPDU$, MAPA$, codcta$, K$, loper
		DIM maestro AS INTEGER, t1no AS INTEGER
		maestro = req.tipocta > 0
		t1no = LTRIM$(RTRIM$(req.TRACK1)) = ""

		 TPDU$ = GetTpdu$(req.host)
	 K$ = TPDU$ + NUMTOBCD$(400, 4)

	 MAPA$ = ""
	IF req.MANUAL THEN SETBIT MAPA$, 2
	 SETBIT MAPA$, 3
		 IF req.oper <> 4 THEN SETBIT MAPA$, 4
	 SETBIT MAPA$, 7
	 SETBIT MAPA$, 11
		 IF req.oper <> 4 THEN
	 SETBIT MAPA$, 12
	 SETBIT MAPA$, 13
		 END IF
	IF req.MANUAL THEN SETBIT MAPA$, 14
	 SETBIT MAPA$, 22
	 SETBIT MAPA$, 24
	 SETBIT MAPA$, 25
	 ' track 2
	 IF req.MANUAL = 0 THEN SETBIT MAPA$, 35
	 SETBIT MAPA$, 41
	 SETBIT MAPA$, 42
		 DIM T1 AS INTEGER
		 T1 = maestro <> 0 OR (req.MANUAL = 0 AND req.t1req <> 0)
		 'T1 = 0' REVERSOS NO MANDAN T1
		 IF T1 THEN
				IF t1no THEN
					 SETBIT MAPA$, 46
				ELSE
					 SETBIT MAPA$, 45
				END IF
		 END IF
		 IF req.oper <> 4 THEN SETBIT MAPA$, 48
		 SETBIT MAPA$, 49
		 IF maestro THEN SETBIT MAPA$, 52

	 loper = req.oper
		 ' ver reverso online
		 'if req.online then
		 '   req.codseg=""
		 '   if loper=opanucpa then
		 '     req.codaut = "000000"
		 '   endif
		 'endif

' no va
	 IF loper = 2 OR loper = 3 THEN SETBIT MAPA$, 37
		 IF RTRIM$(req.CODSEG) > "" AND maestro = 0 THEN
		SETBIT MAPA$, 55
	 END IF
	 SETBIT MAPA$, 60
	 ' numero de ticket
		 IF loper <> 4 THEN SETBIT MAPA$, 62


	 ' mapa de bits
	 K$ = K$ + STRtOBIN$(MAPA$)

	 ' numero de tarjeta
	IF req.MANUAL THEN
		'K$ = K$ + packBcd$(NUMTOBCD(VAL(req.tarj), LEN(RTRIM$(req.tarj))))
		IF LEN(RTRIM$(req.TARJ)) <> 16 THEN
			K$ = K$ + (numtobcd2((req.TARJ), LEN(RTRIM$(req.TARJ))))
		ELSE
			K$ = K$ + packBcd$(NUMTOBCD(VAL(req.TARJ), LEN(RTRIM$(req.TARJ))))
		END IF
	 END IF
		 'SELECT CASE loper
		 '        CASE 0  ' codigo de procesamiento compra req
		 '               K$ = K$ + NUMTOBCD$(0, 6)
		 '        CASE 1  ' codigo de procesamiento compra req
		 '               K$ = K$ + NUMTOBCD$(200000, 6)
		 '        CASE 2  ' codigo de procesamiento compra req
		 '               K$ = K$ + NUMTOBCD$(20000, 6)
		 '        CASE 3  ' codigo de procesamiento compra req
		 '               K$ = K$ + NUMTOBCD$(220000, 6)
		 'END SELECT

		 
		 SELECT CASE loper
		 CASE 0  ' codigo de procesamiento compra req
			req.codpro = "000000"
		 CASE 1  ' codigo de procesamiento compra req
			req.codpro = "200000"
		 CASE 2  ' codigo de procesamiento compra req
			req.codpro = "020000"
		 CASE 3  ' codigo de procesamiento compra req
			req.codpro = "220000"
				 CASE 4  ' codigo de procesamiento consulta
						req.codpro = "310000"
		 END SELECT
		 ' patch for maestro
		 SELECT CASE req.tipocta
				CASE 1
				   MID$(req.codpro, 3, 1) = "1"' Ca$
				CASE 2
				   MID$(req.codpro, 3, 1) = "2"' Cc$
				CASE 8
				   MID$(req.codpro, 3, 1) = "8"' Ca$
				CASE 9
				   MID$(req.codpro, 3, 1) = "9"' CC$
		 END SELECT
		 K$ = K$ + NUMTOBCD$(VAL(req.codpro), 6)



	 ' importe
		 IF loper <> 4 THEN K$ = K$ + NUMTOBCD$(INT(req.IMPO * 100), 12)

	 ' FECHAHORA esta fecha -hora estamal para posnet
	 K$ = K$ + NUMTOBCD$(VAL(FORMAT$(NOW + 1# / 86400, "mmddhhmmss")), 10)

	 ' TRACE
	 req.trace = GETTRACE&(req.host, 0, VAL(req.sufijo))
	 K$ = K$ + NUMTOBCD$((req.trace), 6)

	 ' FECHAHORA 2
		 IF loper <> 4 THEN K$ = K$ + NUMTOBCD$(VAL(FORMAT$(req.fechahora, "hhmmssmmdd")), 10)

	 ' expdate aamm
	IF req.MANUAL THEN K$ = K$ + NUMTOBCD$(VAL(req.EXPDATE), 4)

	 IF req.MANUAL THEN
		modoin = 12
	 ELSE
		modoin = 22
	 END IF
		 IF maestro THEN modoin = modoin - 1


	 ' modo ingreso
	 K$ = K$ + NUMTOBCD$((modoin), 3)

	 ' IDENT INT.RED
	 K$ = K$ + getnii(req.host)

	 ' condicion de la pos
	 K$ = K$ + NUMTOBCD$(0, 2)

	 ' track2
	IF req.MANUAL = 0 THEN
		 K$ = K$ + numtobcdl$(req.TRACK2, LEN(req.TRACK2))
	END IF

  '  no va
  ' retr.ref .number (para anulacion)
	 IF loper = 2 OR loper = 3 THEN
		   K$ = K$ + LEFT$(req.RETREF + SPACE$(12), 12)
	 END IF

	 ' ID TERMINAL
	 K$ = K$ + req.idter         'REQ.IDTER

	 ' ID COMERCIO
	 K$ = K$ + req.Idcom      'REQ.IDCOM

		 ' track1  track 1 track i
		 IF T1 THEN
				IF t1no THEN
					 K$ = K$ + Packstrl$("1")
				ELSE
					DIM JU AS INTEGER
					FOR JU = LEN(req.TRACK1) TO 1 STEP -1
						IF MID$(req.TRACK1, JU, 1) = "?" THEN
							MID$(req.TRACK1, JU, 1) = " "
						END IF
					NEXT

					'mensaje "track i len:" + STR$(LEN(RTRIM$(req.track1)))
					IF r.host <> 2 THEN
						mensaje "B" + req.TRACK1
						K$ = K$ + PACKSTR$(RTRIM$("B" + LEFT$(req.TRACK1, 74)))
					ELSE
						mensaje "%b" + req.TRACK1
						K$ = K$ + PACKSTR$(RTRIM$("%b" + LEFT$(req.TRACK1, 74)))
					END IF
				END IF
		 END IF


	 ' cuotas
		 IF loper <> 4 THEN
		 codcta$ = FMTPLAN$(req.PLAN) + FORMAT$(req.CUOTAS, "00")
	 IF loper = 1 THEN
		codcta$ = codcta$ + FORMAT$(req.TKTORI, "0000") + req.FECORI$
	 END IF
	 K$ = K$ + Packstrl$(codcta$)
		 END IF

				  ' track1  track 1 track i
	 '    IF maestro THEN
	 '           if t1no then
	 '                k$=k$+packstrl$("1")
	 '           else
	 '                   K$ = K$ + PACKSTR$(REQ.TRACK1)
	 '           endif
	 '    END IF



	' codigo moneda
	' k$ = k$ + NUMTOBCD$(32, 3)
		 IF req.MONEDA = "B" THEN
				K$ = K$ + "993"
		 ELSE

		 IF req.MONEDA = "D" THEN
		K$ = K$ + "840"
	 ELSE
		K$ = K$ + "032"
	 END IF
		 END IF
	 'codigo seg
		 IF loper <> 4 AND maestro = 0 THEN
		 IF RTRIM$(req.CODSEG$) <> "" THEN
		K$ = K$ + Packstrl$(RTRIM$(req.CODSEG$))
	 END IF
		 END IF
		 
	  IF maestro THEN
					K$ = K$ + pin$(req.PINBLOCK)
		  END IF

	 ' Version Software
	 K$ = K$ + Packstrl$(VERSOFT$)

	 ' nro ticket
		 'K$ = K$ + Packstrl$(FORMAT$(req.TICKET, "0000"))
		 ' nro ticket
		 IF maestro THEN
				 IF loper <> 4 THEN K$ = K$ + Packstrl$(FORMAT$(req.TICKET, "0000"))
		 ELSE
			' LO CAMBIE PARA TODOS!
				IF loper <> 4 THEN K$ = K$ + Packstrl$(FORMAT$(req.TICKET, "0000"))
		 END IF

	 ReqReverso$ = K$


END FUNCTION

SUB SETBIT (MAPA$, p)
	IF MAPA$ = "" THEN MAPA$ = STRING$(64, "0")
	MID$(MAPA$, p, 1) = "1"
END SUB

SUB SHOWSTAT (Cid)
STATIC ni AS LONG
	DIM sta AS x25stat, x, Y, i, r
  x = POS(0)
  Y = CSRLIN
	VIEW PRINT
  COLOR 0, 7
  FOR i = 36 TO 50
	  LOCATE i, 1: PRINT SPACE$(80);
  NEXT
		ni = ni + 1
		LOCATE 45, 51: PRINT "loops    "; ni;
		r = x25status(Cid, sta, 0)
		IF r > 0 THEN
			LOCATE 44, 51
			PRINT "ESTADO:"; r
		ELSE
			  LOCATE 36, 1: PRINT "lcn0     "; ASC(sta.lcn0);
			  LOCATE 37, 1: PRINT "lcn1     "; ASC(sta.lcn1);
			  LOCATE 38, 1: PRINT "sesion   "; ASC(sta.session);
			  LOCATE 39, 1: PRINT "status   "; ASC(sta.status);
			  LOCATE 40, 1: PRINT "aname    "; sta.aname;
			  LOCATE 41, 1: PRINT "mname    "; sta.mname;
			  LOCATE 42, 1: PRINT "recvp    "; ASC(sta.recvp);
			  LOCATE 43, 1: PRINT "sendp    "; ASC(sta.sendp);
			  LOCATE 44, 1: PRINT "dteaddr  "; ' sta.dteaddr;
			  LOCATE 45, 1: PRINT "pkttxmt  "; sta.pkttxmt;
			  LOCATE 46, 1: PRINT "pktack   "; sta.pktack;
			  LOCATE 36, 51: PRINT "chrtxmt  "; sta.chrtxmt;
			  LOCATE 37, 51: PRINT "chrack   "; sta.chrack;
			  LOCATE 38, 51: PRINT "pktrecv  "; sta.pktrecv;
			  LOCATE 39, 51: PRINT "pktread  "; sta.pktread;
			  LOCATE 40, 51: PRINT "chrrecv  "; sta.chrrecv;
			  LOCATE 41, 51: PRINT "chrread  "; sta.chrread;
			  LOCATE 42, 51: PRINT "lreset   "; sta.lreset;
			  LOCATE 43, 51: PRINT "rreset   "; sta.rreset;
		END IF



	COLOR 7, 0
  IF Y < 35 THEN Y = 35
  VIEW PRINT 1 TO Y
  LOCATE Y, x
END SUB

FUNCTION STRtOBIN$ (Instr$)
DIM bin(8), K$, i, Z, j
bin(7) = 1
bin(6) = 2
bin(5) = 4
bin(4) = 8
bin(3) = 16
bin(2) = 32
bin(1) = 64
bin(0) = 128

K$ = ""
	FOR i = 1 TO LEN(Instr$) STEP 8
		Z = 0
		FOR j = 7 TO 0 STEP -1
			IF MID$(Instr$, i + j, 1) = "1" THEN Z = Z OR bin(j)
		NEXT
		K$ = K$ + CHR$(Z)
	NEXT
	STRtOBIN$ = K$
END FUNCTION

FUNCTION StrToHEx$ (K$)
  DIM o$, i
  o$ = ""
  FOR i = 1 TO LEN(K$)
	o$ = o$ + hexa$(ASC(MID$(K$, i, 1))) + " "
  NEXT
  StrToHEx$ = o$
END FUNCTION

REM $STATIC
FUNCTION visaOn% (req AS reqinfo)
	 visaOn = r.host = 2 AND INSTR(r.EMINAME, "VISA") > 0 AND INSTR(COMMAND$, "/VI1")
END FUNCTION

REM $DYNAMIC
FUNCTION waitForAnswer$ (Cid)
		DIM infox, K$
		infox = 0
	   DO
		  SHOWSTAT Cid
		  RECIBIR K$, Cid, infox
		  IF infox <> 0 THEN PRINT "err:"; infox: EXIT DO
		  IF K$ <> "" THEN
		   END IF
	   LOOP UNTIL K$ <> "" OR INKEY$ <> ""
	waitForAnswer$ = K$
END FUNCTION

