DEFINT A-Z
DECLARE SUB GETR (fi%, BYVAL recn%, RZ AS ANY, encflag AS INTEGER)
DECLARE SUB putR (fi%, BYVAL recn%, RZ AS ANY, encflag AS INTEGER)
DECLARE SUB rjinitialize ()
DECLARE SUB maskar (Rx AS ANY)
DECLARE FUNCTION FILEEMI$ (emi AS STRING)
DECLARE FUNCTION fmt$ (N@, IMG$)
REM $INCLUDE: 'req.bi'
DIM r AS REQINFO, LASTFEC AS LONG
DIM tnm AS CURRENCY
DIM tnn AS LONG
DIM tn AS INTEGER
REM $INCLUDE: 'encr.bi'
rjinitialize
' procesa todos los archivos de la linea de comando
' y los lista
TOTAF$ = "TARJREP.TMP"
TOUTF$ = "TARJREP2.TXT"
TOUTF2$ = "TARJREP.TXT"
TOUTFx$ = "T" + FORMAT$(INT(NOW), "yymmdd") + ".TXT"
TOUTF2x$ = "T" + FORMAT$(INT(NOW), "yymmdd") + "2.TXT"
REDIM ARC$(10)
PRINT "TarjRep Version 4.0.1 (c) Vimarx Inc 2012"
PRINT "Leyendo directorio ..."
p = INSTR(COMMAND$, " ")
IF p > 0 THEN
	cmd$ = LEFT$(COMMAND$, p - 1)
	selcom$ = MID$(COMMAND$, p + 1)
	p = INSTR(selcom$, " ")
	IF p > 0 THEN
		 selcom$ = LEFT$(selcom$, p - 1)
		 IF selcom$ = "*" THEN selcom$ = ""
	END IF
ELSE
	cmd$ = COMMAND$
	selcom$ = ""
END IF
xfec = INSTR(COMMAND$, "/F")
NFILE = 0
IF cmd$ = "" THEN cmd$ = "c*"
k$ = DIR$(cmd$ + extr$)
DO WHILE k$ <> ""
	NFILE = NFILE + 1
	IF NFILE > UBOUND(ARC$) THEN REDIM PRESERVE ARC$(NFILE + 10)
	ARC$(NFILE) = LEFT$(k$, LEN(k$) - 4)
	t = FREEFILE
	OPEN k$ FOR INPUT AS t
	IF LOF(t) = 0 THEN NFILE = NFILE - 1
	CLOSE t
	k$ = DIR$
LOOP
PRINT "Se procesar n" + STR$(NFILE) + " archivos"
PRINT

PRINT "Leyendo ";
t = FREEFILE
lreq = Lre
IF DIR$(TOTAF$) <> "" THEN KILL TOTAF$
OPEN TOTAF$ FOR RANDOM AS t LEN = LEN(r)
FOR i = 1 TO NFILE
	PRINT ".";
	T2 = FREEFILE
	OPEN ARC$(i) + ".CIN" FOR RANDOM AS T2 LEN = 128
		GET T2, 1, cant&
	CLOSE T2

	OPEN ARC$(i) + extr$ FOR RANDOM AS T2 LEN = lreq
		FOR J = 1 TO cant&
			GETR T2, J, r, encry
			IF selcom$ <> "" THEN
				ok = selcom$ = LEFT$(RTRIM$(r.idcom), LEN(selcom$))
			ELSE
				ok = -1
			END IF
			IF ok THEN
				j2 = j2 + 1
				'IF r.cuotas > 1 THEN
				'      MID$(r.idcom, LEN(r.idcom) - 1, 2) = FORMAT$(r.cuotas, "00")
				'      MID$(r.eminame, LEN(r.eminame) - 3, 4) = " " + FORMAT$(r.cuotas, "00") + "C"
				'END IF
				'IF r.PLAN = 999 OR r.planint = 999 OR (r.PLAN = 101 AND LEFT$(r.eminame, 3) = "500") THEN
				'      MID$(r.idcom, LEN(r.idcom) - 2, 3) = "DIF"
				'      MID$(r.eminame, LEN(r.eminame) - 3, 4) = " DIF"
				'END IF
				PUT t, j2, r
			END IF
		NEXT
	CLOSE T2
NEXT
PRINT
IF selcom$ <> "" THEN
	PRINT STR$(j2) + " registros seleccionados Cod.Comercio:" + selcom$
ELSE
	PRINT STR$(j2) + " registros totales"
END IF

PRINT
PRINT "Ordenando 1/2"; FRE("")
S = FREEFILE
OPEN "TARJREP.S1" FOR OUTPUT AS S
FOR J = 1 TO j2
	GET t, J, r ', encry
	IF xfec THEN
		SRT$ = r.idcom + r.eminame + FORMAT$(INT(r.fechahora), "000000") + FORMAT$(r.online, "+0000")
	ELSE
		SRT$ = r.idcom + r.eminame + FORMAT$(INT(r.fecenv), "000000") + FORMAT$(r.online, "+0000")
	END IF
	SRT$ = SRT$ + FORMAT$(J, "00000")
	IF J = 1 THEN NX = LEN(SRT$)
	PRINT #S, SRT$
NEXT
CLOSE S
PRINT "Ordenando 2/2";
OPEN "TARJREP.ORD" FOR OUTPUT AS S
	PRINT #S, "S(1," + STR$(NX) + ",C,A)"
CLOSE S
SHELL "SORT4 TARJREP.S1 TARJREP.S2 TARJREP.ORD"

PRINT "Generando Listado"
tout = FREEFILE
OPEN TOUTF$ FOR OUTPUT AS tout
tout2 = FREEFILE
OPEN TOUTF2$ FOR OUTPUT AS tout2

PRINT #tout2, " TOTALES DE OPERACIONES "
IF selcom$ = "" THEN PRINT #tout, " REPORTE GENERADO:" + MID$(DATE$, 4, 2) + "/" + LEFT$(DATE$, 2) + "/" + RIGHT$(DATE$, 2) + " " + TIME$
IF selcom$ = "" THEN PRINT #tout, " TARJETA            VTO  A/M OP.IMPORTE     P CAJ CAJERA/TIC. LOTE  FEC.CIE CTAS"
PRINT #tout, STRING$(79, 95)

DIM r2 AS REQINFO
REDIM TOTA(5, 3) AS CURRENCY
f5$ = "tarjrep.pos"
tt5 = FREEFILE
OPEN f5$ FOR OUTPUT AS tt5
tn = FREEFILE
emitn$ = "NEXO.txt"
PRINT "Generando " + emitn$
OPEN emitn$ FOR OUTPUT AS tn
PRINT #tn, "FH" + fmt$(2371, "00000000");
PRINT #tn, FORMAT$(r.fechahora, "yyyymmdd");
PRINT #tn, SPACE$(155);
PRINT #tn,


S = FREEFILE
OPEN "TARJREP.S2" FOR INPUT AS S
FOR i = 1 TO j2
	'IF i = j2 THEN STOP
	INPUT #S, SRT$
	GET t, VAL(RIGHT$(SRT$, 5)), r
	IF i = 1 THEN
		LASTON = r.online
		LASTCOM$ = r.idcom + r.eminame
		IF xfec THEN
			LASTFEC = INT(r.fechahora)
		ELSE
			LASTFEC = INT(r.fecenv)
		END IF
	END IF
	IF r.online <> LASTON THEN
		inx = 0
		GOSUB makecut
	END IF

	IF xfec THEN
		IF INT(r.fechahora) <> LASTFEC THEN
			inx = 1
			GOSUB makecut
		END IF
	ELSE
		IF INT(r.fecenv) <> LASTFEC THEN
			inx = 1
			GOSUB makecut
		END IF
	END IF
	IF r.idcom + r.eminame <> LASTCOM$ THEN
		inx = 2
		GOSUB makecut
	END IF
	r2 = r

	FOR J = 0 TO UBOUND(TOTA, 2)
		TOTA(0, J) = TOTA(0, J) + 1
		SELECT CASE r.OPER
			CASE 0
				TOTA(1, J) = TOTA(1, J) + r.impo
			CASE 1
				TOTA(1, J) = TOTA(1, J) - r.impo
			CASE 2
				TOTA(1, J) = TOTA(1, J) - r.impo
			CASE 3
				TOTA(1, J) = TOTA(1, J) + r.impo
		END SELECT
	NEXT
	'b
	on500 = INSTR(r.eminame, "500") > 0
	IF on500 THEN
		PRINT
	ELSE
	maskar r

	END IF
	PRINT #tout, r.tarj; r.expdate; " ";
	SELECT CASE r.manual
			  CASE 0
				PRINT #tout, " A ";
			  CASE ELSE
				PRINT #tout, " M ";
	END SELECT
	SELECT CASE r.OPER
		CASE 0
			IF r.planint = 800 THEN
				PRINT #tout, "CPA+CSH";
			ELSE
				PRINT #tout, "COMPRA ";
			END IF
		CASE 1
			PRINT #tout, "DEVOL. ";
		CASE 2
			PRINT #tout, "AN.CPR ";
		CASE 3
			PRINT #tout, "AN.DEV ";
	END SELECT
	PRINT #tout, fmt$(r.impo, "####0.00") + " ";
	PRINT #tout, LEFT$(r.sufijo, 2) + " " + RIGHT$(r.sufijo, 2);
			ON ERROR RESUME NEXT
	IF selcom$ = "" THEN
		PRINT #tout, " "; fmt$((r.cajera), "0000"); "-"; fmt$((r.TICCAJ), "000000");
		PRINT #tout, " "; FORMAT$(r.lote, "0000");

		PRINT #tout, FORMAT$(r.fecenv, " dd/mm hh:mm");

		PRINT #tout, " "; FORMAT$(r.cuotas, "00");
		PRINT #tout, FORMAT$(r.fechahora, " dd/mm hh:mm");
	ELSE
		PRINT #tout, " " + r.idcom;
	END IF
	PRINT #tout,
	' fecha de cierre
		LASTON = r.online
		LASTCOM$ = r.idcom + r.eminame
		IF xfec THEN
			LASTFEC = INT(r.fechahora)
		ELSE
			LASTFEC = INT(r.fecenv)
		END IF
	' POR EMISOR
	emifi$ = FILEEMI(r.eminame)
	IF emifi$ <> lemi$ THEN
		  IF toutX = 0 THEN
			toutX = FREEFILE
		  ELSE
			 CLOSE toutX
		  END IF
		  PRINT "Generando " + emifi$
		  OPEN emifi$ FOR OUTPUT AS toutX
		  lemi$ = emifi$
	END IF
	'b
	IF on500 THEN

	ELSE
	maskar r
	END IF
	PRINT #toutX, r.tarj; r.expdate; " ";
	SELECT CASE r.manual
			  CASE 0
				PRINT #toutX, " A ";
			  CASE ELSE
				PRINT #toutX, " M ";
	END SELECT
	SELECT CASE r.OPER
		CASE 0
			IF r.planint = 800 THEN
				PRINT #toutX, "CPA+CSH";
			ELSE
				PRINT #toutX, "COMPRA ";
			END IF

		CASE 1
			PRINT #toutX, "DEVOL. ";
		CASE 2
			PRINT #toutX, "AN.CPR ";
		CASE 3
			PRINT #toutX, "AN.DEV ";
	END SELECT
	PRINT #toutX, fmt$(r.impo, "####0.00") + " ";
	PRINT #toutX, LEFT$(r.sufijo, 2) + RIGHT$(r.sufijo, 2);

	IF selcom$ = "" THEN
		PRINT #toutX, "-"; fmt$((r.TICCAJ), "000000");
		PRINT #toutX, FORMAT$(r.fechahora, " dd/mm/yy hh:mm");
		PRINT #toutX, " C:"; FORMAT$(r.cuotas, "00");
		PRINT #toutX, " " + r.idcom;
	ELSE
		PRINT #toutX, " " + r.idcom;
	END IF
	PRINT #toutX, "AUT:" + r.codaut;
	PRINT #toutX,
	' fecha de cierre
		LASTON = r.online
		LASTCOM$ = r.idcom + r.eminame
		IF xfec THEN
			LASTFEC = INT(r.fechahora)

		ELSE
			LASTFEC = INT(r.fecenv)
		END IF
	 GOSUB writepos
NEXT
r2 = r
GOSUB makecut
inx = UBOUND(TOTA, 2)
GOSUB makecut
CLOSE t, tout, tout2, toutX
CLOSE tt5
				'
	PRINT #tn, "FT" + fmt$(2371, "00000000");
  PRINT #tn, "00000";
  PRINT #tn, fmt$(tnm * 100, "000000000000000");
  PRINT #tn, fmt$((tnn), "000000");
  PRINT #tn, SPACE$(137);
	PRINT #tn,
	'
	CLOSE tn


tout = FREEFILE
OPEN TOUTF$ FOR INPUT AS tout
tout2 = FREEFILE
OPEN TOUTFx$ FOR OUTPUT AS tout2
	  DO WHILE NOT EOF(tout)
		  LINE INPUT #tout, l$
		  PRINT #tout2, l$
	  LOOP
CLOSE tout, tout2

tout = FREEFILE
OPEN TOUTF2$ FOR INPUT AS tout
tout2 = FREEFILE
OPEN TOUTF2x$ FOR OUTPUT AS tout2
	  DO WHILE NOT EOF(tout)
		  LINE INPUT #tout, l$
		  PRINT #tout2, l$
	  LOOP
CLOSE tout, tout2

END

writenexo:
	  IF on500 = 0 THEN RETURN
	  PRINT #tn, "01";
	  PRINT #tn, LEFT$(r.tarj, 16);
	  PRINT #tn, FORMAT$(r.fechahora, "yyyymmddhhmm");
	  IF r.PLAN = 999 OR r.planint = 999 OR (r.PLAN = 101 AND LEFT$(r.eminame, 3) = "500") THEN
		PRINT #tn, "3";
	  ELSE
		PRINT #tn, "1";
	  END IF
	  PRINT #tn, FORMAT$(r.cuotas, "00");

	  PRINT #tn, fmt$(r.impo * 100, "000000000000000");
	  PRINT #tn, fmt$(VAL(r.codaut), "00000000");
	  PRINT #tn, fmt$((r.ticket), "00000");
	  PRINT #tn, LEFT$(r.sufijo, 2) + RIGHT$(r.sufijo, 2);
	  PRINT #tn, fmt$((r.TICCAJ), "00000000");
	  PRINT #tn, SPACE$(100);
	  PRINT #tn,
	 tnn = tnn + 1
	 tnm = tnm + r.impo
RETURN

writepos:
	' registro para progress
	PRINT #tt5, CHR$(34);
	PRINT #tt5, FORMAT$(r2.lote, "0000");
	PRINT #tt5, FORMAT$(r2.ticket, "0000");
	IF INSTR(COMMAND$, "/NM") = 0 THEN
			PRINT #tt5, "XXXXXXXXXXXX" + MID$(r2.tarj, 13);
			ELSE
			PRINT #tt5, MID$(r2.tarj, 1);
	END IF
		PRINT #tt5, "0000";
	PRINT #tt5, FORMAT$(INT(r2.impo * 100), "0000000");
	PRINT #tt5, FORMAT$(r2.fechahora, "yymmdd");
	PRINT #tt5, FORMAT$(r2.fechahora, "hhmm");
	PRINT #tt5, r2.sufijo;
	PRINT #tt5, FORMAT$(r2.TICCAJ, "000000");
	SELECT CASE r2.online
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
	PRINT #tt5, FORMAT$(r2.OPER, "0");
	eminame$ = r2.eminame
	idcom$ = r2.idcom

				IF r2.cuotas > 1 THEN
					  MID$(idcom$, LEN(idcom$) - 1, 2) = "  "
					  MID$(eminame$, LEN(eminame$) - 3, 4) = "    "
				END IF
				IF r2.PLAN = 999 OR r2.planint = 999 OR (r2.PLAN = 101 AND LEFT$(r2.eminame, 3) = "500") THEN
					  MID$(idcom$, LEN(idcom$) - 2, 3) = "   "
					  MID$(eminame$, LEN(eminame$) - 3, 4) = "    "
				END IF

	PRINT #tt5, eminame$;
	PRINT #tt5, r2.idter;
	PRINT #tt5, idcom$;
	PRINT #tt5, r2.IDCIE;
	PRINT #tt5, " ";
	PRINT #tt5, FORMAT$(r2.cuotas, "00");
	PRINT #tt5, " ";
	PRINT #tt5, FORMAT$(r2.planint, "000");
	'cashback
	IF r2.planint <> 800 THEN r2.cashback = 0
	PRINT #tt5, FORMAT$(INT(r2.cashback * 100), "0000000");

	PRINT #tt5, CHR$(34)

RETURN


makecut:
	  FOR ji = 0 TO inx
			GOSUB makecut2
	  NEXT
	  PRINT #tout,
RETURN

makecut2:
	IF TOTA(0, ji) = 0 THEN RETURN
	PRINT #tout, " TOTAL ";
	PRINT #tout2, " TOTAL ";
	IF ji = UBOUND(TOTA, 2) THEN
		k$ = " GENERAL                               "
	ELSE
		k$ = r2.eminame
		IF ji < 2 THEN
			IF xfec THEN
				k$ = k$ + FORMAT$(r2.fechahora, " dd/mm hh:mm ")
			ELSE
				k$ = k$ + FORMAT$(r2.fecenv, " dd/mm hh:mm ")
			END IF
		ELSE
			k$ = k$ + SPACE$(13)
		END IF
		IF ji = 0 THEN
			  SELECT CASE r2.online
				CASE -1
					k$ = k$ + " ON-LINE  "
				CASE -2
					k$ = k$ + " OFF-ENV. "
				CASE 0
					k$ = k$ + " OFF-NO E "
			  END SELECT
		ELSE
			  k$ = k$ + SPACE$(10)
		END IF
	END IF
	PRINT #tout, k$; fmt$(TOTA(0, ji), "###0"); " " + fmt$(TOTA(1, ji), "######0.00")
	PRINT #tout2, k$; fmt$(TOTA(0, ji), "###0"); " " + fmt$(TOTA(1, ji), "######0.00")
	FOR ji2 = 0 TO UBOUND(TOTA, 1)
		TOTA(ji2, ji) = 0
	NEXT
RETURN

DEFSNG A-Z
FUNCTION FILEEMI$ (emi AS STRING)
	k$ = ""
	FOR i = 1 TO LEN(emi)
		IF INSTR("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", MID$(emi, i, 1)) THEN
			k$ = k$ + MID$(emi, i, 1)
		END IF
	NEXT
	IF LEN(k$) > 8 THEN
		k$ = LEFT$(k$, 6) + RIGHT$(k$, 2)
	END IF
	FILEEMI$ = k$ + ".EMI"
END FUNCTION

FUNCTION fmt$ (N@, IMG$)
	k$ = FORMAT$(N@, IMG$)
	IF LEN(k$) > LEN(IMG$) THEN
		fmt$ = RIGHT$(k$, LEN(IMG$))
	ELSE
	fmt$ = SPACE$(LEN(IMG$) - LEN(k$)) + k$
	END IF
END FUNCTION

SUB maskar (r AS REQINFO)
	IF r.host = 0 THEN EXIT SUB
	IF INSTR(COMMAND$, "/NM") = 0 THEN
		MID$(r.tarj, 7, 6) = "XXXXXXXXXXXX"
		r.expdate = "XXXX"
	END IF
END SUB

