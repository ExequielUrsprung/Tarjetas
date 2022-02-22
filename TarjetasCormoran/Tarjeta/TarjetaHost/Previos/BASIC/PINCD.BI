DIM SHARED dummy
' Constantes de modulo vxsoft.bas
CONST Lnk_dup = 1
CONST Lnk_NoDup = 0

' vxsoft.dim

REM $DYNAMIC
debug = INSTR(COMMAND$, "/DEBUG") > 0
STACK 5800 + (debug * 1200)
MEMORY& = FRE("")

' INICIALIZACION DE VARIABLES
CR = CHR$(13) + CHR$(10)
ZEROKB = STRING$(KBLEN, 0)
REM $DYNAMIC

