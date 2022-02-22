TYPE REQINFO
    tarj    AS STRING * 20
    fechahora AS DOUBLE
    impo    AS CURRENCY
    expdate AS STRING * 4
    manual  AS INTEGER
    track2  AS STRING * 37
    TRACK1  AS STRING * 77      ' TRACK I DATA
    idter   AS STRING * 8
    Idcom   AS STRING * 15
    IDCIE   AS STRING * 15 ' COD COMERCIO CIERRE
    codseg  AS STRING * 30
    tickET  AS INTEGER
    CODAUT  AS STRING * 6
    tktori  AS LONG
    fecori  AS STRING * 6
    host    AS INTEGER
    oper    AS INTEGER
    trace   AS LONG
    sufijo  AS STRING * 4
    MONEDA  AS STRING * 1
    CUOTAS  AS INTEGER
    cajera  AS INTEGER
    ticcaj  AS LONG
    ' BATCH UPLOAD
    CODPRO  AS STRING * 6
    ULTIMO  AS INTEGER    'SI ES EL ULTIMO DEL BATCH
    FECCAP  AS STRING * 4
    RETREF  AS STRING * 12
    MENORI  AS INTEGER    ' MENSAJE ORIGINAL
    TRAORI  AS LONG       ' TRACE ORIGINAL
    online  AS INTEGER
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
    COEF AS DOUBLE        ' COEF PLANES
    CashBack AS CURRENCY      ' MONTO ANTES DE MULTIPLICAR
    PINBLOCK AS STRING * 16
    ' QUEDAN 6
    tipocta AS INTEGER  ' 1-2-8-9 a-c-a-c p-p-d-d
    t1req AS INTEGER
    RELLENO AS STRING * 2
END TYPE
CONST lreqx = 448
DIM SHARED lreqxd AS STRING * lreqx

