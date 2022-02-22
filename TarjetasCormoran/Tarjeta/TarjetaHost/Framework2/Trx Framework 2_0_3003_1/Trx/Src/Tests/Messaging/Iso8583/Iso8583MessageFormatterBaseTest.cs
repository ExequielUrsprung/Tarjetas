#region Copyright (C) 2004-2012 Zabaleta Asociados SRL
//
// Trx Framework - <http://www.trxframework.org/>
// Copyright (C) 2004-2012  Zabaleta Asociados SRL
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion

using Trx.Messaging;
using Trx.Messaging.Iso8583;
using NUnit.Framework;

namespace Tests.Trx.Messaging.Iso8583 {

	/// <summary>
	/// Base class for ISO 8583 messages formatter tests.
	/// </summary>
	public abstract class Iso8583MessageFormatterBaseTest {

		#region Methods
		public void DoTest( Iso8583MessageFormatter formatter, string[] fieldValues,
			FieldFormatter[] fieldFormatters,
			Iso8583MessageFormatterTestConfig[] tests, bool partialWriting) {

			FormatterContext formatterContext = new FormatterContext(
				FormatterContext.DefaultBufferSize);
			ParserContext parserContext = new ParserContext(
				ParserContext.DefaultBufferSize);
			Iso8583Message messageToFormat;
			Iso8583Message parsedMessage;
			string formattedData;

			// Add field Formatters.
			for ( int i = 0; i < fieldFormatters.Length; i++) {
				if ( fieldFormatters[i] != null) {
					formatter.FieldFormatters.Add( fieldFormatters[i]);
				}
			}

			for ( int i = 0; i < tests.Length; i++) {
				messageToFormat = new Iso8583Message( tests[i].MessageTypeIdentifier);
				formatterContext.Clear();
				parserContext.Clear();
				parsedMessage = null;
				formattedData = null;

				// Set header formatter.
				formatter.MessageHeaderFormatter = tests[i].HeaderFormatter;

				// Set message type identifier formatter.
				formatter.MessageTypeIdentifierFormatter = tests[i].MessageTypeIdentifierFormatter;

				// Set header.
				messageToFormat.Header = tests[i].Header;

				// Set message fields.
				for ( int j = 0; j < tests[i].Fields.Length; j++) {
					messageToFormat.Fields.Add( tests[i].Fields[j],
						fieldValues[tests[i].Fields[j] - 1]);
				}

				// Format message.
				formatter.Format( messageToFormat, ref formatterContext);
				formattedData = formatterContext.GetDataAsString();

				//Console.WriteLine( "Data:[" + formattedData + "]");

				// Check formatted data.
				Assert.IsTrue( tests[i].ExpectedFormattedData.Equals( formattedData));

				// Now parse to see if we get a copy of our message.
				if ( partialWriting) {
					for ( int j = 0; j < formattedData.Length; j++) {
						parserContext.Write( formattedData.Substring( j, 1));
						parsedMessage = ( Iso8583Message)formatter.Parse( ref parserContext);

						// Message must be acquired only with the last char of formattedData.
						if ( ( j + 1) == formattedData.Length) {
							Assert.IsNotNull( parsedMessage);
						} else {
							Assert.IsNull( parsedMessage);
						}
					}
				} else {
					parserContext.Write( formattedData);
					parsedMessage =  ( Iso8583Message)formatter.Parse( ref parserContext);
				}

				// Parser data must be exhausted.
				Assert.IsTrue( parserContext.DataLength == 0);

				// Check parsed message.
				Assert.IsNotNull( parsedMessage);

				// Check header.
				if ( tests[i].HeaderFormatter == null) {
					Assert.IsNull( parsedMessage.Header);
				} else {
					Assert.IsNotNull( parsedMessage.Header);

					if ( tests[i].Header == null) {
						Assert.IsTrue( string.Empty.Equals( ( ( StringMessageHeader)
							( parsedMessage.Header)).Value));
					} else {
						if ( ( ( StringMessageHeader)( tests[i].Header)).Value == null) {
							Assert.IsTrue( string.Empty.Equals( ( ( StringMessageHeader)
								( parsedMessage.Header)).Value));
						} else {
							Assert.IsTrue( ( ( StringMessageHeader)
								( messageToFormat.Header)).Value.Equals( ( ( StringMessageHeader)
								( parsedMessage.Header)).Value));
						}
					}
				}

				// Check message type identifier.
				Assert.IsTrue( messageToFormat.MessageTypeIdentifier ==
					parsedMessage.MessageTypeIdentifier);

				// Check fields.
				Assert.IsTrue( messageToFormat.Fields.Count == parsedMessage.Fields.Count);
				Assert.IsTrue( messageToFormat.Fields.MaximumFieldNumber ==
					parsedMessage.Fields.MaximumFieldNumber);
				for ( int j = 0; j < tests[i].Fields.Length; j++) {
					Assert.IsTrue( fieldValues[tests[i].Fields[j] - 1].Equals(
						( ( StringField)( parsedMessage[tests[i].Fields[j]])).FieldValue));
				}
			}
		}
		#endregion
	}
}
