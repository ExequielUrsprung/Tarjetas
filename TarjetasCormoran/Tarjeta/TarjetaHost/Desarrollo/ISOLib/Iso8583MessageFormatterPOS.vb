Imports Trx.Messaging.Iso8583
Imports Trx.Messaging.Channels
Imports Trx.Messaging
Imports Trx.Utilities

Public Class Iso8583MessageFormatterPOS
    Inherits Iso8583MessageFormatter
    Private _implementacion As E_Implementaciones

    Public Sub New(ByVal Implementacion As E_Implementaciones)
        SetupFields(Implementacion)
    End Sub

    Private Sub SetupFields(ByVal Implementacion As E_Implementaciones)
        MessageHeaderFormatter = New StringMessageHeaderFormatter(New FixedLengthManager(10), BcdStringEncoder.GetInstance(True, 10))
        MessageTypeIdentifierFormatter = New StringFieldFormatter(-1, New FixedLengthManager(4), BcdStringEncoder.GetInstance(True, 4), ZeroPaddingLeft.GetInstance(False, True), "Message type identifier")
        FieldFormatters.Add(New BitMapFieldFormatter(0, 1, 64, BinaryEncoder.GetInstance(), "Primary bitmap"))
        FieldFormatters.Add(New StringFieldFormatter(2, New VariableLengthManager(0, 20, BcdLengthEncoder.GetInstance(20)), BcdStringEncoder.GetInstance(False, 0), NumericValidator.GetInstance(), "Nro de Cuenta"))
        FieldFormatters.Add(New StringFieldFormatter(3, New FixedLengthManager(6), BcdStringEncoder.GetInstance(False, 6), ZeroPaddingLeft.GetInstance(False, False), "Processing code"))
        FieldFormatters.Add(New StringFieldFormatter(4, New FixedLengthManager(12), BcdStringEncoder.GetInstance(False, 12), ZeroPaddingLeft.GetInstance(False, True), "Transaction amount"))
        FieldFormatters.Add(New StringFieldFormatter(5, New FixedLengthManager(12), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Reconciliation amount"))
        FieldFormatters.Add(New StringFieldFormatter(6, New FixedLengthManager(12), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Cardholder billing amount"))
        FieldFormatters.Add(New StringFieldFormatter(7, New FixedLengthManager(10), BcdStringEncoder.GetInstance(False, 10), ZeroPaddingLeft.GetInstance(False, False), "Transmission date and time"))
        FieldFormatters.Add(New StringFieldFormatter(8, New FixedLengthManager(8), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Cardholder billing fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(9, New FixedLengthManager(8), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Reconciliation conversion rate"))
        FieldFormatters.Add(New StringFieldFormatter(10, New FixedLengthManager(8), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Cardholder billing conversion rate"))
        FieldFormatters.Add(New StringFieldFormatter(11, New FixedLengthManager(6), BcdStringEncoder.GetInstance(False, 6), ZeroPaddingLeft.GetInstance(False, False), "Systems trace audit number"))
        FieldFormatters.Add(New StringFieldFormatter(12, New FixedLengthManager(6), BcdStringEncoder.GetInstance(False, 6), ZeroPaddingLeft.GetInstance(False, False), "Local transaction time"))
        FieldFormatters.Add(New StringFieldFormatter(13, New FixedLengthManager(4), BcdStringEncoder.GetInstance(False, 4), ZeroPaddingLeft.GetInstance(False, False), "Local transaction date"))
        FieldFormatters.Add(New StringFieldFormatter(14, New FixedLengthManager(4), BcdStringEncoder.GetInstance(False, 4), ZeroPaddingLeft.GetInstance(False, False), "Expiration date"))
        FieldFormatters.Add(New StringFieldFormatter(15, New FixedLengthManager(4), BcdStringEncoder.GetInstance(False, 4), ZeroPaddingLeft.GetInstance(False, False), "Settlement date"))
        FieldFormatters.Add(New StringFieldFormatter(16, New FixedLengthManager(4), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Conversion date"))
        FieldFormatters.Add(New StringFieldFormatter(17, New FixedLengthManager(4), BcdStringEncoder.GetInstance(False, 4), ZeroPaddingLeft.GetInstance(False, False), "Capture date"))
        FieldFormatters.Add(New StringFieldFormatter(18, New FixedLengthManager(4), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Merchant type"))

        'EMV 19 MODIFICADO
        'FieldFormatters.Add(New StringFieldFormatter(19, New FixedLengthManager(3), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Acquiring institution country code"))
        FieldFormatters.Add(New StringFieldFormatter(19, New FixedLengthManager(4), BcdStringEncoder.GetInstance(False, 4), ZeroPaddingLeft.GetInstance(False, False), "Acquiring institution country code"))

        FieldFormatters.Add(New StringFieldFormatter(20, New FixedLengthManager(3), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Primary account number country code"))
        FieldFormatters.Add(New StringFieldFormatter(21, New FixedLengthManager(3), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Forwarding institution country code"))
        'FieldFormatters.Add(New StringFieldFormatter(22, New FixedLengthManager(3), BcdStringEncoder.GetInstance(False, 3), ZeroPaddingLeft.GetInstance(False, False), "Point of service record mode"))
        FieldFormatters.Add(New StringFieldFormatter(22, New FixedLengthManager(4), BcdStringEncoder.GetInstance(False, 4), ZeroPaddingLeft.GetInstance(False, False), "Point of service record mode"))
        'EMV 19 MODIFICADO
        'FieldFormatters.Add(New StringFieldFormatter(23, New FixedLengthManager(3), StringEncoder.GetInstance(False, 3), ZeroPaddingLeft.GetInstance(False, True), "Card sequence number"))
        FieldFormatters.Add(New StringFieldFormatter(23, New FixedLengthManager(4), BcdStringEncoder.GetInstance(False, 4), ZeroPaddingLeft.GetInstance(False, False), "Card sequence number"))
        'FieldFormatters.Add(New StringFieldFormatter(24, New FixedLengthManager(3), BcdStringEncoder.GetInstance(False, 3), ZeroPaddingLeft.GetInstance(False, True), "Network international identifier"))
        FieldFormatters.Add(New StringFieldFormatter(24, New FixedLengthManager(4), BcdStringEncoder.GetInstance(False, 4), ZeroPaddingLeft.GetInstance(False, True), "Network international identifier"))
        FieldFormatters.Add(New StringFieldFormatter(25, New FixedLengthManager(2), BcdStringEncoder.GetInstance(False, 2), ZeroPaddingLeft.GetInstance(False, False), "Point of service condition code"))
        FieldFormatters.Add(New StringFieldFormatter(26, New FixedLengthManager(2), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Point of service PIN capture code"))
        FieldFormatters.Add(New StringFieldFormatter(27, New FixedLengthManager(1), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Authorization Identification response length"))
        FieldFormatters.Add(New StringFieldFormatter(28, New FixedLengthManager(9), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Transaction fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(29, New FixedLengthManager(9), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Settlement fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(30, New FixedLengthManager(9), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Transaction processing fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(31, New FixedLengthManager(9), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Settlement processing fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(32, New VariableLengthManager(0, 11, StringLengthEncoder.GetInstance(11)), StringEncoder.GetInstance(), "Acquirer institution identification code"))
        FieldFormatters.Add(New StringFieldFormatter(33, New VariableLengthManager(0, 11, StringLengthEncoder.GetInstance(11)), StringEncoder.GetInstance(), NumericValidator.GetInstance(), "Forwarding institution identification code"))
        'FieldFormatters.Add(New StringFieldFormatter(34, New VariableLengthManager(0, 28, StringLengthEncoder.GetInstance(28)), StringEncoder.GetInstance(), "Extended primary account number"))
        FieldFormatters.Add(New StringFieldFormatter(34, New VariableLengthManager(0, 28, BcdLengthEncoder.GetInstance(28)), StringEncoder.GetInstance(), "Extended primary account number"))
        FieldFormatters.Add(New StringFieldFormatter(35, New VariableLengthManager(0, 37, BcdLengthEncoder.GetInstance(37)), BcdStringEncoder.GetInstance(False, 0), "Track 2 data"))
        FieldFormatters.Add(New StringFieldFormatter(36, New VariableLengthManager(0, 104, StringLengthEncoder.GetInstance(104)), StringEncoder.GetInstance(), "Track 3 data"))
        FieldFormatters.Add(New StringFieldFormatter(37, New FixedLengthManager(12), StringEncoder.GetInstance(), "Retrieval reference number"))
        FieldFormatters.Add(New StringFieldFormatter(38, New FixedLengthManager(6), StringEncoder.GetInstance(), "Authorization identification response"))
        FieldFormatters.Add(New StringFieldFormatter(39, New FixedLengthManager(2), StringEncoder.GetInstance(), "Response code"))
        FieldFormatters.Add(New StringFieldFormatter(40, New FixedLengthManager(3), StringEncoder.GetInstance(), "Service restriction code"))
        FieldFormatters.Add(New StringFieldFormatter(41, New FixedLengthManager(8), StringEncoder.GetInstance(), "Card acceptor terminal identification"))
        FieldFormatters.Add(New StringFieldFormatter(42, New FixedLengthManager(15), StringEncoder.GetInstance(), "Card acceptor identification code"))
        FieldFormatters.Add(New StringFieldFormatter(43, New FixedLengthManager(40), StringEncoder.GetInstance(), "Card acceptor name/location"))
        FieldFormatters.Add(New StringFieldFormatter(44, New VariableLengthManager(0, 27, StringLengthEncoder.GetInstance(25)), StringEncoder.GetInstance(), "Additional response data"))
        FieldFormatters.Add(New StringFieldFormatter(45, New VariableLengthManager(0, 76, BcdLengthEncoder.GetInstance(76)), StringEncoder.GetInstance(), "Info Track 1"))
        FieldFormatters.Add(New StringFieldFormatter(46, New VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)), BcdStringEncoder.GetInstance(False, 0), "TRACK 1 NO LEIDO"))


        FieldFormatters.Add(New StringFieldFormatter(47, New VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Paquete Encriptado"))
        'FieldFormatters.Add(New StringFieldFormatter(47, New VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)), BcdStringEncoder.GetInstance(False, 0), "BLOQUE DE DATOS ENCRIPTADOS"))

        FieldFormatters.Add(New StringFieldFormatter(48, New VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Cuotas / Datos Originales"))
        FieldFormatters.Add(New StringFieldFormatter(49, New FixedLengthManager(3), StringEncoder.GetInstance(), "Transaction currency code"))
        FieldFormatters.Add(New StringFieldFormatter(50, New FixedLengthManager(3), StringEncoder.GetInstance(), "Reconciliation currency code"))
        FieldFormatters.Add(New StringFieldFormatter(51, New FixedLengthManager(3), StringEncoder.GetInstance(), "Cardholder billing currency code"))

        'FieldFormatters.Add(New StringFieldFormatter(52, New FixedLengthManager(16), BcdStringEncoder.GetInstance(False, 0), "Personal identification number (PIN) data"))
        FieldFormatters.Add(New BinaryFieldFormatter(52, New FixedLengthManager(8), BinaryEncoder.GetInstance(), "Personal identification number (PIN) data"))
        FieldFormatters.Add(New StringFieldFormatter(53, New FixedLengthManager(16), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Security related control information"))
        FieldFormatters.Add(New StringFieldFormatter(54, New VariableLengthManager(0, 120, BcdLengthEncoder.GetInstance(120)), StringEncoder.GetInstance(), "Amounts, additional"))
        FieldFormatters.Add(New StringFieldFormatter(55, New VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for ISO use"))
        FieldFormatters.Add(New StringFieldFormatter(56, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for ISO use"))
        FieldFormatters.Add(New StringFieldFormatter(57, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(58, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        'FieldFormatters.Add(New StringFieldFormatter(59, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(59, New VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(60, New VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Software version"))
        FieldFormatters.Add(New BinaryFieldFormatter(61, New FixedLengthManager(22), BinaryEncoder.GetInstance(), "reserved"))
        FieldFormatters.Add(New StringFieldFormatter(62, New VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Nro de ticket"))
        FieldFormatters.Add(New StringFieldFormatter(63, New VariableLengthManager(0, 999, BcdLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))
        FieldFormatters.Add(New BinaryFieldFormatter(64, New FixedLengthManager(8), HexadecimalBinaryEncoder.GetInstance(), "Message authentication code field"))

    End Sub

    Public Overrides Function Clone() As Object
        Dim formatter As New Iso8583MessageFormatterPOS(_implementacion)
        CopyTo(formatter)
        Return formatter
    End Function
End Class

