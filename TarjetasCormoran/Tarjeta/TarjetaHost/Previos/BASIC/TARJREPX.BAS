DECLARE SUB maskar (R AS ANY)
DECLARE FUNCTION FILEEMI$ (emi AS STRING)
DECLARE FUNCTION fmt$ (N@, IMG$)
TYPE reqinfo
	TARJ    AS STRING * 20
	fechahora AS DOUBLE
	IMPO    AS CURRENCY
	EXPDATE AS STRING * 4
	MANUAL  AS INTEGER
	track2  AS STRING * 37
	TRACK1  AS STRING * 77      ' TRACK I DATA
	idter   AS STRING * 8
	Idcom   AS STRING * 15
	IDCIE   AS STRING * 15 ' COD COMERCIO CIERRE
	codseg  AS STRING * 30
	TICKET  AS INTEGER
	codaut  AS STRING * 6
	tktori  AS LONG
	fecori  AS STRING * 6
	host    AS INTEGER
	OPER    AS INTEGER
	trace   AS LONG
	sufijo  AS STRING * 4
	MONEDA  AS STRING * 1
	CUOTAS  AS INTEGER
	CAJERA  AS INTEGER
	TICCAJ  AS LONG
	' BATCH UPLOAD
	CODPRO  AS STRING * 6
	ULTIMO  AS INTEGER    'SI ES EL ULTIMO DEL BATCH
	FECCAP  AS STRING * 4
	RETREF  AS STRING * 12
	MENORI  AS INTEGER    ' MENSAJE ORIGINAL
	TRAORI  AS LONG       ' TRACE ORIGINAL
	ONLINE  AS INTEGER
	lote    AS INTEGER
	cajadir AS STRING * 30
	cmd     AS INTEGER
	EMINAME AS STRING * 16
	PLAN    AS INTEGER
	planint AS INTEGER
	planintori AS INTEGER
	PLANORI AS INTEGER
	CTAORI  AS INTEGER
	MONORI  AS STRING * 1
	PLANID  AS STRING * 20
	CONFIR  AS INTEGER
	'RELLENO VERSION 00
	fecenv  AS DOUBLE '   fecha hora envio
	TKID AS STRING * 4    ' 4 LETRAS ARCHIVO
	'FILLER  AS STRING * 38
	COEF AS DOUBLE        ' COEF PLANES
	Cashback AS CURRENCY      ' MONTO ANTES DE MULTIPLICAR
	FILLER  AS STRING * 22

END TYPE

' procesa todos los archivos de la linea de comando
' y los lista
TOTAF$ = "TARJREP.TMP"
TOUTF$ = "TARJREP2.TXT"
TOUTF2$ = "TARJREP.TXT"
REDIM ARC$(10)
PRINT "Leyendo directorio"
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
k$ = DIR$(cmd$ + ".cie")
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
DIM R AS reqinfo, LASTFEC AS LONG
t = FREEFILE
lreq = LEN(R)
IF DIR$(TOTAF$) <> "" THEN KILL TOTAF$
OPEN TOTAF$ FOR RANDOM AS t LEN = lreq
FOR I = 1 TO NFILE
	PRINT ".";
	T2 = FREEFILE
	OPEN ARC$(I) + ".CIN" FOR RANDOM AS T2 LEN = 128
		GET T2, 1, cant&
	CLOSE T2

	OPEN ARC$(I) + ".CIE" FOR RANDOM AS T2 LEN = lreq
		FOR J = 1 TO cant&
			GET T2, J, R
			IF selcom$ <> "" THEN
				ok = selcom$ = LEFT$(RTRIM$(R.Idcom), LEN(selcom$))
			ELSE
				ok = -1
			END IF
			IF ok THEN
				j2 = j2 + 1
				IF R.CUOTAS > 1 THEN
					  MID$(R.Idcom, LEN(R.Idcom) - 1, 2) = FORMAT$(R.CUOTAS, "00")
					  MID$(R.EMINAME, LEN(R.EMINAME) - 3, 4) = " " + FORMAT$(R.CUOTAS, "00") + "C"
				END IF
				IF R.PLAN = 999 OR (R.PLAN = 101 AND LEFT$(R.EMINAME, 3) = "500") THEN
					  MID$(R.Idcom, LEN(R.Idcom) - 2, 3) = "DIF"
					  MID$(R.EMINAME, LEN(R.EMINAME) - 3, 4) = " DIF"
				END IF
				PUT t, j2, R
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
	GET t, J, R
	IF xfec THEN
		SRT$ = R.Idcom + R.EMINAME + FORMAT$(INT(R.fechahora), "000000") + FORMAT$(R.ONLINE, "+0000")
	ELSE
		SRT$ = R.Idcom + R.EMINAME + FORMAT$(INT(R.fecenv), "000000") + FORMAT$(R.ONLINE, "+0000")
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
TOUT = FREEFILE
OPEN TOUTF$ FOR OUTPUT AS TOUT
TOUT2 = FREEFILE
OPEN TOUTF2$ FOR OUTPUT AS TOUT2

PRINT #TOUT2, " TOTALES DE OPERACIONES "
IF selcom$ = "" THEN PRINT #TOUT, " REPORTE GENERADO:" + MID$(DATE$, 4, 2) + "/" + LEFT$(DATE$, 2) + "/" + RIGHT$(DATE$, 2) + " " + TIME$
IF selcom$ = "" THEN PRINT #TOUT, " TARJETA            VTO  A/M OP.IMPORTE     P CAJ CAJERA/TIC. LOTE  FEC.CIE CTAS"
PRINT #TOUT, STRING$(79, 95)

DIM r2 AS reqinfo
REDIM TOTA(5, 3) AS CURRENCY
f5$ = "tarjrep.pos"
tt5 = FREEFILE
OPEN f5$ FOR OUTPUT AS tt5

S = FREEFILE
OPEN "TARJREP.S2" FOR INPUT AS S
FOR I = 1 TO j2
	r2 = R
	INPUT #S, SRT$
	GET t, VAL(RIGHT$(SRT$, 5)), R
	IF I = 1 THEN
		LASTON = R.ONLINE
		LASTCOM$ = R.Idcom + R.EMINAME
		IF xfec THEN
			LASTFEC = INT(R.fechahora)
		ELSE
			LASTFEC = INT(R.fecenv)
		END IF
	END IF
	IF R.ONLINE <> LASTON THEN
		inx = 0
		GOSUB makecut
	END IF
	IF xfec THEN
		IF INT(R.fechahora) <> LASTFEC THEN
			inx = 1
			GOSUB makecut
		END IF
	ELSE
		IF INT(R.fecenv) <> LASTFEC THEN
			inx = 1
			GOSUB makecut
		END IF
	END IF
	IF R.Idcom + R.EMINAME <> LASTCOM$ THEN
		inx = 2
		GOSUB makecut
	END IF
	FOR J = 0 TO UBOUND(TOTA, 2)
		TOTA(0, J) = TOTA(0, J) + 1
		SELECT CASE R.OPER
			CASE 0
				TOTA(1, J) = TOTA(1, J) + R.IMPO
			CASE 1
				TOTA(1, J) = TOTA(1, J) - R.IMPO
			CASE 2
				TOTA(1, J) = TOTA(1, J) - R.IMPO
			CASE 3
				TOTA(1, J) = TOTA(1, J) + R.IMPO
		END SELECT
	NEXT
	'b
	maskar R
	PRINT #TOUT, R.TARJ; R.EXPDATE; " ";
	SELECT CASE R.MANUAL
			  CASE 0
				PRINT #TOUT, " A ";
			  CASE ELSE
				PRINT #TOUT, " M ";
	END SELECT
	SELECT CASE R.OPER
		CASE 0
			PRINT #TOUT, "COMPRA ";
		CASE 1
			PRINT #TOUT, "DEVOL. ";
		CASE 2
			PRINT #TOUT, "AN.CPR ";
		CASE 3
			PRINT #TOUT, "AN.DEV ";
	END SELECT
	PRINT #TOUT, fmt$(R.IMPO, "####0.00") + " ";
	PRINT #TOUT, LEFT$(R.sufijo, 2) + " " + RIGHT$(R.sufijo, 2);
			ON ERROR RESUME NEXT
	IF selcom$ = "" THEN
		PRINT #TOUT, " "; fmt$((R.CAJERA), "0000"); "-"; fmt$((R.TICCAJ), "000000");
		PRINT #TOUT, " "; FORMAT$(R.lote, "0000");

		PRINT #TOUT, FORMAT$(R.fecenv, " dd/mm hh:mm");

		PRINT #TOUT, " "; FORMAT$(R.CUOTAS, "00");
		PRINT #TOUT, FORMAT$(R.fechahora, " dd/mm hh:mm");
	ELSE
		PRINT #TOUT, " " + R.Idcom;
	END IF
	PRINT #TOUT,
	' fecha de cierre
		LASTON = R.ONLINE
		LASTCOM$ = R.Idcom + R.EMINAME
		IF xfec THEN
			LASTFEC = INT(R.fechahora)
		ELSE
			LASTFEC = INT(R.fecenv)
		END IF
	' POR EMISOR
	emifi$ = FILEEMI(R.EMINAME)
	IF emifi$ <> lemi$ THEN
		  IF toutx = 0 THEN
			toutx = FREEFILE
		  ELSE
			 CLOSE toutx
		  END IF
		  PRINT "Generando " + emifi$
		  OPEN emifi$ FOR OUTPUT AS toutx
		  lemi$ = emifi$
	END IF
	'b
	maskar R
	PRINT #toutx, R.TARJ; R.EXPDATE; " ";
	SELECT CASE R.MANUAL
			  CASE 0
				PRINT #toutx, " A ";
			  CASE ELSE
				PRINT #toutx, " M ";
	END SELECT
	SELECT CASE R.OPER
		CASE 0
			PRINT #toutx, "COMPRA ";
		CASE 1
			PRINT #toutx, "DEVOL. ";
		CASE 2
			PRINT #toutx, "AN.CPR ";
		CASE 3
			PRINT #toutx, "AN.DEV ";
	END SELECT
	PRINT #toutx, fmt$(R.IMPO, "####0.00") + " ";
	PRINT #toutx, LEFT$(R.sufijo, 2) + RIGHT$(R.sufijo, 2);

	IF selcom$ = "" THEN
		PRINT #toutx, "-"; fmt$((R.TICCAJ), "000000");
		PRINT #toutx, FORMAT$(R.fechahora, " dd/mm/yy hh:mm");
		PRINT #toutx, " C:"; FORMAT$(R.CUOTAS, "00");
		PRINT #toutx, " " + R.Idcom;
	ELSE
		PRINT #toutx, " " + R.Idcom;
	END IF
	PRINT #toutx, "AUT:" + R.codaut;
	PRINT #toutx,
	' fecha de cierre
		LASTON = R.ONLINE
		LASTCOM$ = R.Idcom + R.EMINAME
		IF xfec THEN
			LASTFEC = INT(R.fechahora)

		ELSE
			LASTFEC = INT(R.fecenv)
		END IF

	GOSUB writepos
NEXT

GOSUB makecut
inx = UBOUND(TOTA, 2)
GOSUB makecut
CLOSE t, TOUT, TOUT2, toutx
CLOSE tt5
END


writepos:
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
	PRINT #tt5, FORMAT$(r2.OPER, "0");
	PRINT #tt5, r2.EMINAME; r2.idter; r2.Idcom; r2.IDCIE;
	PRINT #tt5, r2.CUOTAS;
	PRINT #tt5, FORMAT$(r2.planint, "000");
	PRINT #tt5, CHR$(34)

RETURN

makecut:
	  FOR ji = 0 TO inx
			GOSUB makecut2
	  NEXT
	  PRINT #TOUT,
RETURN

makecut2:
	IF TOTA(0, ji) = 0 THEN RETURN
	PRINT #TOUT, " TOTAL ";
	PRINT #TOUT2, " TOTAL ";
	IF ji = UBOUND(TOTA, 2) THEN
		k$ = " GENERAL                               "
	ELSE
		k$ = r2.EMINAME
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
			  SELECT CASE r2.ONLINE
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
	PRINT #TOUT, k$; fmt$(TOTA(0, ji), "###0"); " " + fmt$(TOTA(1, ji), "######0.00")
	PRINT #TOUT2, k$; fmt$(TOTA(0, ji), "###0"); " " + fmt$(TOTA(1, ji), "######0.00")
	FOR ji2 = 0 TO UBOUND(TOTA, 1)
		TOTA(ji2, ji) = 0
	NEXT
RETURN

FUNCTION FILEEMI$ (emi AS STRING)
	k$ = ""
	FOR I = 1 TO LEN(emi)
		IF INSTR("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", MID$(emi, I, 1)) THEN
			k$ = k$ + MID$(emi, I, 1)
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

SUB maskar (R AS reqinfo)
	IF R.host = 0 THEN EXIT SUB
	MID$(R.TARJ, 1, 16) = "XXXXXXXXXXXXXXXX"
	R.EXPDATE = "XXXX"

END SUB

