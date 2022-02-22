Imports Trx.Messaging.Iso8583
Imports Trx.Messaging.Channels

Imports Trx.Messaging
Imports Trx.Utilities

Public Class NexoMessagesIdentifier
    Inherits BasicMessagesIdentifier

    Public Sub New()
        'por default los mensajes nexo se definen con el campo 11
        MyBase.New(11)
    End Sub

End Class

Public Class Iso8583Nexo
    Inherits Iso8583MessageFormatter
    Private _implementacion As E_Implementaciones

    Public Sub New(ByVal Implementacion As E_Implementaciones)

        SetupFields(Implementacion)
    End Sub 'New

    Private Sub SetupFields(ByVal Implementacion As E_Implementaciones)

        MessageHeaderFormatter = New StringMessageHeaderFormatter(New FixedLengthManager(12), StringEncoder.GetInstance())
        '------------------------------
        MessageTypeIdentifierFormatter = New StringFieldFormatter(-1, New FixedLengthManager(4), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Message type identifier")

        FieldFormatters.Add(New BitMapFieldFormatter(0, 1, 64, HexadecimalBinaryEncoder.GetInstance(), "Primary bitmap"))
        FieldFormatters.Add(New BitMapFieldFormatter(1, 65, 128, HexadecimalBinaryEncoder.GetInstance(), "Secondary bitmap"))
        FieldFormatters.Add(New StringFieldFormatter(2, New VariableLengthManager(0, 19, StringLengthEncoder.GetInstance(19)), StringEncoder.GetInstance(), NumericValidator.GetInstance(), "Primary account number"))
        FieldFormatters.Add(New StringFieldFormatter(3, New FixedLengthManager(6), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Processing code"))
        FieldFormatters.Add(New StringFieldFormatter(4, New FixedLengthManager(12), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Transaction amount"))
        FieldFormatters.Add(New StringFieldFormatter(5, New FixedLengthManager(12), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Reconciliation amount"))
        FieldFormatters.Add(New StringFieldFormatter(6, New FixedLengthManager(12), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Cardholder billing amount"))
        FieldFormatters.Add(New StringFieldFormatter(7, New FixedLengthManager(10), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Transmission date and time"))
        FieldFormatters.Add(New StringFieldFormatter(8, New FixedLengthManager(8), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Cardholder billing fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(9, New FixedLengthManager(8), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Reconciliation conversion rate"))
        FieldFormatters.Add(New StringFieldFormatter(10, New FixedLengthManager(8), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Cardholder billing conversion rate"))
        FieldFormatters.Add(New StringFieldFormatter(11, New FixedLengthManager(6), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Systems trace audit number"))
        FieldFormatters.Add(New StringFieldFormatter(12, New FixedLengthManager(6), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Local transaction time"))
        FieldFormatters.Add(New StringFieldFormatter(13, New FixedLengthManager(4), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Local transaction date"))
        FieldFormatters.Add(New StringFieldFormatter(14, New FixedLengthManager(4), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Expiration date"))
        FieldFormatters.Add(New StringFieldFormatter(15, New FixedLengthManager(4), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Settlement date"))

        FieldFormatters.Add(New StringFieldFormatter(16, New FixedLengthManager(4), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Conversion date"))
        FieldFormatters.Add(New StringFieldFormatter(17, New FixedLengthManager(4), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Capture date"))
        FieldFormatters.Add(New StringFieldFormatter(18, New FixedLengthManager(4), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Merchant type"))
        FieldFormatters.Add(New StringFieldFormatter(19, New FixedLengthManager(3), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Acquiring institution country code"))
        FieldFormatters.Add(New StringFieldFormatter(20, New FixedLengthManager(3), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Primary account number country code"))
        FieldFormatters.Add(New StringFieldFormatter(21, New FixedLengthManager(3), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Forwarding institution country code"))
        FieldFormatters.Add(New StringFieldFormatter(22, New FixedLengthManager(3), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Point of service record mode"))
        FieldFormatters.Add(New StringFieldFormatter(23, New FixedLengthManager(3), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Card sequence number"))
        FieldFormatters.Add(New StringFieldFormatter(24, New FixedLengthManager(3), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Network international identifier"))
        FieldFormatters.Add(New StringFieldFormatter(25, New FixedLengthManager(2), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Point of service condition code"))
        FieldFormatters.Add(New StringFieldFormatter(26, New FixedLengthManager(2), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Point of service PIN capture code"))
        FieldFormatters.Add(New StringFieldFormatter(27, New FixedLengthManager(1), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Authorization Identification response length"))
        FieldFormatters.Add(New StringFieldFormatter(28, New FixedLengthManager(9), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Transaction fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(29, New FixedLengthManager(9), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Settlement fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(30, New FixedLengthManager(9), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Transaction processing fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(31, New FixedLengthManager(9), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Settlement processing fee amount"))

        ' Dice Numeric, pero vienen espacios...
        'FieldFormatters.Add(  new StringFieldFormatter( 
        '    32, new VariableLengthManager(  0, 11, StringLengthEncoder.GetInstance( 11 ) ),
        '    StringEncoder.GetInstance(),  NumericValidator.GetInstance() ,
        '    "Acquirer institution identification code" ) );
        FieldFormatters.Add(New StringFieldFormatter(32, New VariableLengthManager(0, 11, StringLengthEncoder.GetInstance(11)), StringEncoder.GetInstance(), "Acquirer institution identification code"))



        FieldFormatters.Add(New StringFieldFormatter(33, New VariableLengthManager(0, 11, StringLengthEncoder.GetInstance(11)), StringEncoder.GetInstance(), NumericValidator.GetInstance(), "Forwarding institution identification code"))
        FieldFormatters.Add(New StringFieldFormatter(34, New VariableLengthManager(0, 28, StringLengthEncoder.GetInstance(28)), StringEncoder.GetInstance(), "Extended primary account number"))
        FieldFormatters.Add(New StringFieldFormatter(35, New VariableLengthManager(0, 37, StringLengthEncoder.GetInstance(37)), StringEncoder.GetInstance(), "Track 2 data"))
        FieldFormatters.Add(New StringFieldFormatter(36, New VariableLengthManager(0, 104, StringLengthEncoder.GetInstance(104)), StringEncoder.GetInstance(), "Track 3 data"))
        FieldFormatters.Add(New StringFieldFormatter(37, New FixedLengthManager(12), StringEncoder.GetInstance(), "Retrieval reference number"))
        FieldFormatters.Add(New StringFieldFormatter(38, New FixedLengthManager(6), StringEncoder.GetInstance(), "Authorization identification response"))
        FieldFormatters.Add(New StringFieldFormatter(39, New FixedLengthManager(2), StringEncoder.GetInstance(), "Response code"))
        FieldFormatters.Add(New StringFieldFormatter(40, New FixedLengthManager(3), StringEncoder.GetInstance(), "Service restriction code"))

        'FieldFormatters.Add( new StringFieldFormatter(
        '    41, new FixedLengthManager( 8 ), StringEncoder.GetInstance(),
        '    "Card acceptor terminal identification" ) );
        FieldFormatters.Add(New StringFieldFormatter(41, New FixedLengthManager(16), StringEncoder.GetInstance(), "Card acceptor terminal identification"))


        FieldFormatters.Add(New StringFieldFormatter(42, New FixedLengthManager(15), StringEncoder.GetInstance(), "Card acceptor identification code"))

        FieldFormatters.Add(New StringFieldFormatter(43, New FixedLengthManager(40), StringEncoder.GetInstance(), "Card acceptor name/location"))

        Select Case Implementacion
            Case E_Implementaciones.Banelco
                FieldFormatters.Add(New StringFieldFormatter(44, New VariableLengthManager(0, 27, StringLengthEncoder.GetInstance(25)), StringEncoder.GetInstance(), "Additional response data"))
            Case E_Implementaciones.Link
                FieldFormatters.Add(New StringFieldFormatter(44, New VariableLengthManager(0, 27, StringLengthEncoder.GetInstance(25)), StringEncoder.GetInstance(), "Additional response data"))
            Case E_Implementaciones.PosnetSalud
                FieldFormatters.Add(New StringFieldFormatter(44, New VariableLengthManager(0, 27, StringLengthEncoder.GetInstance(25)), StringEncoder.GetInstance(), "Additional response data"))
            Case E_Implementaciones.PosnetComercio
                FieldFormatters.Add(New StringFieldFormatter(44, New VariableLengthManager(0, 27, StringLengthEncoder.GetInstance(25)), StringEncoder.GetInstance(), "Additional response data"))

        End Select
        FieldFormatters.Add(New StringFieldFormatter(45, New VariableLengthManager(0, 76, StringLengthEncoder.GetInstance(76)), StringEncoder.GetInstance(), "Track 1 data"))
        FieldFormatters.Add(New StringFieldFormatter(46, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Additional data - ISO"))
        FieldFormatters.Add(New StringFieldFormatter(47, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Additional data - national"))
        FieldFormatters.Add(New StringFieldFormatter(48, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Additional data - private"))
        FieldFormatters.Add(New StringFieldFormatter(49, New FixedLengthManager(3), StringEncoder.GetInstance(), "Transaction currency code"))
        FieldFormatters.Add(New StringFieldFormatter(50, New FixedLengthManager(3), StringEncoder.GetInstance(), "Reconciliation currency code"))
        FieldFormatters.Add(New StringFieldFormatter(51, New FixedLengthManager(3), StringEncoder.GetInstance(), "Cardholder billing currency code"))
        Select Case Implementacion
            Case E_Implementaciones.Banelco, E_Implementaciones.Link, E_Implementaciones.PosnetSalud

                FieldFormatters.Add(New StringFieldFormatter(52, New FixedLengthManager(16), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Personal identification number (PIN) data"))
            Case Else
                FieldFormatters.Add(New BinaryFieldFormatter(52, New FixedLengthManager(8), HexadecimalBinaryEncoder.GetInstance(), "Personal identification number (PIN) data"))
        End Select

        FieldFormatters.Add(New StringFieldFormatter(53, New FixedLengthManager(16), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Security related control information"))
        FieldFormatters.Add(New StringFieldFormatter(54, New VariableLengthManager(0, 120, StringLengthEncoder.GetInstance(120)), StringEncoder.GetInstance(), "Amounts, additional"))
        FieldFormatters.Add(New StringFieldFormatter(55, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for ISO use"))

        FieldFormatters.Add(New StringFieldFormatter(56, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for ISO use"))
        FieldFormatters.Add(New StringFieldFormatter(57, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(58, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(59, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        Select Case Implementacion
            Case E_Implementaciones.Banelco
                'FieldFormatters.Add(New StringFieldFormatter(60, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Terminal data"))
                'FieldFormatters.Add(New StringFieldFormatter(61, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))

                FieldFormatters.Add(New BinaryFieldFormatter(60, New FixedLengthManager(15), BinaryEncoder.GetInstance(), "Terminal data"))
                FieldFormatters.Add(New BinaryFieldFormatter(61, New FixedLengthManager(16), BinaryEncoder.GetInstance(), "reserved"))
            Case E_Implementaciones.Link
                'FieldFormatters.Add(New StringFieldFormatter(60, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Terminal data"))
                'FieldFormatters.Add(New StringFieldFormatter(61, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))
                Logger.Debug("FIJANDO CAMPO 60 link")
                FieldFormatters.Add(New BinaryFieldFormatter(60, New FixedLengthManager(15), BinaryEncoder.GetInstance(), "Terminal data"))
                FieldFormatters.Add(New BinaryFieldFormatter(61, New FixedLengthManager(16), BinaryEncoder.GetInstance(), "reserved"))

            Case Else
                FieldFormatters.Add(New BinaryFieldFormatter(60, New FixedLengthManager(19), BinaryEncoder.GetInstance(), "Terminal data"))
                FieldFormatters.Add(New BinaryFieldFormatter(61, New FixedLengthManager(22), BinaryEncoder.GetInstance(), "reserved"))

                'FieldFormatters.Add(New StringFieldFormatter(60, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Terminal data"))
                'FieldFormatters.Add(New StringFieldFormatter(61, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))
        End Select
        Select Case Implementacion
            Case E_Implementaciones.PosnetComercio
                FieldFormatters.Add(New BinaryFieldFormatter(62, New FixedLengthManager(13), BinaryEncoder.GetInstance(), "reserved"))

            Case Else
                FieldFormatters.Add(New StringFieldFormatter(62, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))

        End Select
        FieldFormatters.Add(New StringFieldFormatter(63, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))
        FieldFormatters.Add(New BinaryFieldFormatter(64, New FixedLengthManager(8), HexadecimalBinaryEncoder.GetInstance(), "Message authentication code field"))
        FieldFormatters.Add(New BinaryFieldFormatter(65, New FixedLengthManager(8), BinaryEncoder.GetInstance(), "Reserved for ISO use"))
        FieldFormatters.Add(New StringFieldFormatter(66, New FixedLengthManager(1), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Settlement code"))
        FieldFormatters.Add(New StringFieldFormatter(67, New FixedLengthManager(2), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Extended payment data"))
        FieldFormatters.Add(New StringFieldFormatter(68, New FixedLengthManager(3), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Receiving institution country code"))
        FieldFormatters.Add(New StringFieldFormatter(69, New FixedLengthManager(3), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Settlement institution country code"))
        FieldFormatters.Add(New StringFieldFormatter(70, New FixedLengthManager(3), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Network management information code"))
        FieldFormatters.Add(New StringFieldFormatter(71, New FixedLengthManager(4), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Message number"))
        FieldFormatters.Add(New StringFieldFormatter(72, New FixedLengthManager(4), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Last message number"))
        FieldFormatters.Add(New StringFieldFormatter(73, New FixedLengthManager(6), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Action date"))
        FieldFormatters.Add(New StringFieldFormatter(74, New FixedLengthManager(10), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Credits, number"))
        FieldFormatters.Add(New StringFieldFormatter(75, New FixedLengthManager(10), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Credits, reversal number"))
        FieldFormatters.Add(New StringFieldFormatter(76, New FixedLengthManager(10), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Debits, number"))
        FieldFormatters.Add(New StringFieldFormatter(77, New FixedLengthManager(10), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Debits, reversal number"))
        FieldFormatters.Add(New StringFieldFormatter(78, New FixedLengthManager(10), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Transfer, number"))
        FieldFormatters.Add(New StringFieldFormatter(79, New FixedLengthManager(10), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Transfer, reversal number"))
        FieldFormatters.Add(New StringFieldFormatter(80, New FixedLengthManager(10), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Inquiries, number"))
        FieldFormatters.Add(New StringFieldFormatter(81, New FixedLengthManager(10), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Authorizations, number"))
        FieldFormatters.Add(New StringFieldFormatter(82, New FixedLengthManager(12), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Credits, processing fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(83, New FixedLengthManager(12), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Credits, transaction fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(84, New FixedLengthManager(12), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Debits, processing fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(85, New FixedLengthManager(12), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Debits, transaction fee amount"))
        FieldFormatters.Add(New StringFieldFormatter(86, New FixedLengthManager(16), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Credits, amount"))
        FieldFormatters.Add(New StringFieldFormatter(87, New FixedLengthManager(16), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Credits, reversal amount"))
        FieldFormatters.Add(New StringFieldFormatter(88, New FixedLengthManager(16), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Debits, amount"))
        FieldFormatters.Add(New StringFieldFormatter(89, New FixedLengthManager(16), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Debits, reversal amount"))
        FieldFormatters.Add(New StringFieldFormatter(90, New FixedLengthManager(42), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, False), "Original Data Elements"))
        FieldFormatters.Add(New StringFieldFormatter(91, New FixedLengthManager(1), StringEncoder.GetInstance(), "File update code"))
        FieldFormatters.Add(New StringFieldFormatter(92, New FixedLengthManager(2), StringEncoder.GetInstance(), "File security code"))
        FieldFormatters.Add(New StringFieldFormatter(93, New FixedLengthManager(6), StringEncoder.GetInstance(), "Response indicator"))
        FieldFormatters.Add(New StringFieldFormatter(94, New FixedLengthManager(7), StringEncoder.GetInstance(), "Service indicator"))
        FieldFormatters.Add(New StringFieldFormatter(95, New FixedLengthManager(42), StringEncoder.GetInstance(), "Replacement amounts"))
        FieldFormatters.Add(New BinaryFieldFormatter(96, New FixedLengthManager(16), HexadecimalBinaryEncoder.GetInstance(), "Message security code"))
        FieldFormatters.Add(New StringFieldFormatter(97, New FixedLengthManager(17), StringEncoder.GetInstance(), "Amount, net settlement"))
        FieldFormatters.Add(New StringFieldFormatter(98, New FixedLengthManager(25), StringEncoder.GetInstance(), "Payee"))
        FieldFormatters.Add(New StringFieldFormatter(99, New VariableLengthManager(0, 11, StringLengthEncoder.GetInstance(11)), StringEncoder.GetInstance(), NumericValidator.GetInstance(), "Settlement institution Id code"))
        FieldFormatters.Add(New StringFieldFormatter(100, New VariableLengthManager(0, 11, StringLengthEncoder.GetInstance(11)), StringEncoder.GetInstance(), "Receiving institution Id code"))
        FieldFormatters.Add(New StringFieldFormatter(101, New VariableLengthManager(0, 17, StringLengthEncoder.GetInstance(17)), StringEncoder.GetInstance(), "File name"))
        FieldFormatters.Add(New StringFieldFormatter(102, New VariableLengthManager(0, 28, StringLengthEncoder.GetInstance(28)), StringEncoder.GetInstance(), "Account identification 1"))
        FieldFormatters.Add(New StringFieldFormatter(103, New VariableLengthManager(0, 28, StringLengthEncoder.GetInstance(28)), StringEncoder.GetInstance(), "Account identification 2"))
        FieldFormatters.Add(New StringFieldFormatter(104, New VariableLengthManager(0, 100, StringLengthEncoder.GetInstance(100)), StringEncoder.GetInstance(), "Transaction description"))
        FieldFormatters.Add(New StringFieldFormatter(105, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for ISO use"))
        FieldFormatters.Add(New StringFieldFormatter(106, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for ISO use"))
        FieldFormatters.Add(New StringFieldFormatter(107, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for ISO use"))
        FieldFormatters.Add(New StringFieldFormatter(108, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for ISO use"))
        FieldFormatters.Add(New StringFieldFormatter(109, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for ISO use"))
        FieldFormatters.Add(New StringFieldFormatter(110, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for ISO use"))
        FieldFormatters.Add(New StringFieldFormatter(111, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for ISO use"))
        FieldFormatters.Add(New StringFieldFormatter(112, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(113, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(114, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(115, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(116, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(117, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(118, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(119, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for national use"))
        FieldFormatters.Add(New StringFieldFormatter(120, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))
        FieldFormatters.Add(New StringFieldFormatter(121, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))
        FieldFormatters.Add(New StringFieldFormatter(122, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))
        FieldFormatters.Add(New StringFieldFormatter(123, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))

        Select Case Implementacion
            Case E_Implementaciones.Banelco, E_Implementaciones.Link
                FieldFormatters.Add(New StringFieldFormatter(124, New FixedLengthManager(4), StringEncoder.GetInstance(), ZeroPaddingLeft.GetInstance(False, True), "Terminal data"))

            Case Else
                FieldFormatters.Add(New StringFieldFormatter(124, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))

        End Select

        FieldFormatters.Add(New StringFieldFormatter(125, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))
        FieldFormatters.Add(New StringFieldFormatter(126, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))
        FieldFormatters.Add(New StringFieldFormatter(127, New VariableLengthManager(0, 999, StringLengthEncoder.GetInstance(999)), StringEncoder.GetInstance(), "Reserved for private use"))
        FieldFormatters.Add(New BinaryFieldFormatter(128, New FixedLengthManager(8), HexadecimalBinaryEncoder.GetInstance(), "Message authentication code"))
    End Sub 'SetupFields


    Public Overrides Function Clone() As Object

        Dim formatter As New Iso8583Nexo(_implementacion)
        CopyTo(formatter)

        Return formatter
    End Function 'Clone
End Class 'Iso8583AsciNexoMessageFormatter '

