' DEFINICION DE TIPOS

' PONER VALIDACION DE REGISTRO

TYPE VTATYPE                                                  'OFFSET=0
    VERSION AS STRING * 1       ' NRO DE VERSION                       01
    MENSAJE     AS STRING * 32      ' MENSAJE DEL HOST                 41
    MONTOP      AS STRING * 8
    MENRESP     AS STRING * 20      ' MENSAJE DE ERROR SEGUN CODRESP   61
    FILETKT     AS STRING * 60      ' FILE CON EL TICKET              121
    EMINAME     AS STRING * 16      ' nOMBRE DEL EMISOR               137
    OK          AS INTEGER          '                                 139
    CASHBACK    AS CURRENCY           ' MONTO CASHBACK                                143
    TICKET      AS LONG             ' LO NECESITO PARA LOS TEST
    CHECK       AS STRING * 2       ' HACER = CHECKID                 145
END TYPE

TYPE IDATYPE
    VERSION AS STRING * 1       ' NRO DE VERSION                       1
    TARJ    AS STRING * 20      ' NRO DE TARJETA                      21
    EXPDATE AS STRING * 4       ' FECHA DE EXPIRACION                 25
    IMPO    AS CURRENCY         ' IMPORTE DE LA TRANSACCION           33
    MANUAL  AS INTEGER          ' MODO DE INGRESO 0-MANUAL 1-AUTOMAT  35
    PLANINT AS INTEGER          ' COD.PLAN                            37
    CODSEG  AS STRING * 30      ' COD. SEGURIDAD                      67
    TICCAJ  AS LONG             ' NRO DE TICKET DE LA CAJA            71
    CAJERA  AS INTEGER          ' COD DE CAJERO (OPERADOR)            73
    HORA    AS SINGLE           ' FECHA/HORA                          77
    TRACK2  AS STRING * 37      '                                    114
    TRACK1  AS STRING * 77      '                                    191
    CodAut  AS STRING * 6       ' Codigo de autorizacion pa/anul/ncr 197
    TKTORI  AS LONG             '                                    201
    FECORI  AS STRING * 6       '                                    207
    PLANINTori AS INTEGER       ' COD.PLAN                           209
    oper    AS INTEGER          ' !! operacion                       211
                                ' 0-cpra 1-dev 2/3-anu     5-cierre
    cmd     AS INTEGER '+8 offline +16 anular                        213
    cajadir     AS STRING * 26      '                                243
    TKID AS STRING * 4
    CASHBACK     AS CURRENCY           '                                247
    CHECK    AS STRING * 2      ' HACER = CHECKID                    249
END TYPE
CONST CHECKID = "O2"
CONST encryptOn = -1
CONST CASHBACKID = 800

