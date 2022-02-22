
Public Enum Errores
    Cancelado = 1
    Error_Ingreso_4digitos = 2
    Error_lectura_track2 = 3
    Error_ingreso_PIN = 4
    Error_Chip = 5
    Fecha_inválida = 6
    TimeOut = 7
    Error_secuencia_comando_recibido = 8
    Error_formato_comando_recibido = 9
    Error_LRC_comando_recibido = 11
    Error_al_eliminar_registro = 12
    Registro_no_encontrado = 13
    Error_de_desencripción = 14
    Error_de_encripción = 15
    Falta_definición_por_Y00 = 16
    Falta_posicion_MK = 17
    Falta_WorkingKey = 18
    Falta_MasterKey = 19
    Debe_sincronizar = 20
    Error_Protocolo = 21
    Funcionalidad_No_disponible = 22
    Error_long_Certificado = 26
    Formato_DNI_Invalido = 28
    Error_Autenticacion_Archivo = 29
    Error_Clave_RSA_disponible = 30
    Error_Clave_RSA_ausente = 31
    Error_dato_invalido = 32
    Error_puerto = 97
    Reintentos_superados = 98
    Error_desconocido = 99
End Enum

Public Enum Tipos_transacciones
    compra = 0
    anulacion = 2
    compra_cash = 9
    devolucion = 20
    anulacion_dev = 22
    pagoPEI = 98
    anulacionPEI = 99
End Enum

Public Enum Tipos_claves
    datos = 1
    pines = 2
End Enum

Public Structure Pares_claves
    Dim tipo As Tipos_claves
    Dim clave As String
    Dim control As String
    Dim posicion As String
End Structure


