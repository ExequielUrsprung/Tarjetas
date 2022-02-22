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

using System;
using Trx.Utilities;

namespace Trx.Messaging
{
    /// <summary>
    /// Implements a self announced string field formatter.
    /// </summary>
    public class SelfAnnouncedStringFieldFormatter : FieldFormatter, ISelfAnnouncedFieldFormatter
    {
        private readonly IDataEncoder _encoder;
        private readonly bool _includeSelfAnnouncementInLength;
        private readonly LengthManager _lengthManager;
        private readonly IStringPadding _padding;
        private readonly SelfAnnouncedMarkerManager _selfAnnounceManager;
        private readonly IStringValidator _validator;
        private readonly IStringFieldValueFormatter _valueFormatter;

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="includeSelfAnnouncementInLength">
        /// Flag para indicar que el largo del indicador de campo debe o no sumarse
        /// al largo de los datos.
        /// </param>
        /// <param name="selfAnnounceManager">
        /// Administrador que maneja los indicadores de campo.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        public SelfAnnouncedStringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            bool includeSelfAnnouncementInLength, SelfAnnouncedMarkerManager selfAnnounceManager,
            IDataEncoder encoder)
            : this(fieldNumber, lengthManager, includeSelfAnnouncementInLength,
                selfAnnounceManager, encoder, null, null, null, string.Empty)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="includeSelfAnnouncementInLength">
        /// Flag para indicar que el largo del indicador de campo debe o no sumarse
        /// al largo de los datos.
        /// </param>
        /// <param name="selfAnnounceManager">
        /// Administrador que maneja los indicadores de campo.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="description">
        /// It's the description of the field formatter.
        /// </param>
        public SelfAnnouncedStringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            bool includeSelfAnnouncementInLength, SelfAnnouncedMarkerManager selfAnnounceManager,
            IDataEncoder encoder, string description)
            : this(fieldNumber, lengthManager, includeSelfAnnouncementInLength,
                selfAnnounceManager, encoder, null, null, null, description)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="includeSelfAnnouncementInLength">
        /// Flag para indicar que el largo del indicador de campo debe o no sumarse
        /// al largo de los datos.
        /// </param>
        /// <param name="selfAnnounceManager">
        /// Administrador que maneja los indicadores de campo.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="padding">
        /// It's the field value padder.
        /// </param>
        public SelfAnnouncedStringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            bool includeSelfAnnouncementInLength, SelfAnnouncedMarkerManager selfAnnounceManager,
            IDataEncoder encoder, IStringPadding padding)
            :
                this(fieldNumber, lengthManager, includeSelfAnnouncementInLength,
                    selfAnnounceManager, encoder, padding, null, null, string.Empty)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="includeSelfAnnouncementInLength">
        /// Flag para indicar que el largo del indicador de campo debe o no sumarse
        /// al largo de los datos.
        /// </param>
        /// <param name="selfAnnounceManager">
        /// Administrador que maneja los indicadores de campo.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="padding">
        /// It's the field value padder.
        /// </param>
        /// <param name="description">
        /// It's the description of the field formatter.
        /// </param>
        public SelfAnnouncedStringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            bool includeSelfAnnouncementInLength, SelfAnnouncedMarkerManager selfAnnounceManager,
            IDataEncoder encoder, IStringPadding padding, string description)
            :
                this(fieldNumber, lengthManager, includeSelfAnnouncementInLength,
                    selfAnnounceManager, encoder, padding, null, null, description)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="includeSelfAnnouncementInLength">
        /// Flag para indicar que el largo del indicador de campo debe o no sumarse
        /// al largo de los datos.
        /// </param>
        /// <param name="selfAnnounceManager">
        /// Administrador que maneja los indicadores de campo.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="validator">
        /// It's the field value validator.
        /// </param>
        public SelfAnnouncedStringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            bool includeSelfAnnouncementInLength, SelfAnnouncedMarkerManager selfAnnounceManager,
            IDataEncoder encoder, IStringValidator validator)
            :
                this(fieldNumber, lengthManager, includeSelfAnnouncementInLength,
                    selfAnnounceManager, encoder, null, validator, null, string.Empty)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="includeSelfAnnouncementInLength">
        /// Flag para indicar que el largo del indicador de campo debe o no sumarse
        /// al largo de los datos.
        /// </param>
        /// <param name="selfAnnounceManager">
        /// Administrador que maneja los indicadores de campo.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="validator">
        /// It's the field value validator.
        /// </param>
        /// <param name="description">
        /// It's the description of the field formatter.
        /// </param>
        public SelfAnnouncedStringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            bool includeSelfAnnouncementInLength, SelfAnnouncedMarkerManager selfAnnounceManager,
            IDataEncoder encoder, IStringValidator validator, string description)
            :
                this(fieldNumber, lengthManager, includeSelfAnnouncementInLength,
                    selfAnnounceManager, encoder, null, validator, null, description)
        {
        }

        /// <summary>
        /// It initializes a new instance of the class.
        /// </summary>
        /// <param name="fieldNumber">
        /// It's the number of the field this formatter formats/parse.
        /// </param>
        /// <param name="lengthManager">
        /// It's the field data length manager.
        /// </param>
        /// <param name="includeSelfAnnouncementInLength">
        /// Flag para indicar que el largo del indicador de campo debe o no sumarse
        /// al largo de los datos.
        /// </param>
        /// <param name="selfAnnounceManager">
        /// Administrador que maneja los indicadores de campo.
        /// </param>
        /// <param name="encoder">
        /// It's the data encoder.
        /// </param>
        /// <param name="padding">
        /// It's the field value padder.
        /// </param>
        /// <param name="validator">
        /// It's the field value validator.
        /// </param>
        public SelfAnnouncedStringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            bool includeSelfAnnouncementInLength, SelfAnnouncedMarkerManager selfAnnounceManager,
            IDataEncoder encoder, IStringPadding padding, IStringValidator validator)
            :
                this(fieldNumber, lengthManager, includeSelfAnnouncementInLength,
                    selfAnnounceManager, encoder, padding, validator, null, string.Empty)
        {
        }

        /// <summary>
        /// Construye un nuevo formateador de campos de mensajes cuyos
        /// datos son de tipo cadena de caracteres.
        /// </summary>
        /// <param name="fieldNumber">
        /// Es el n�mero de campo del mensaje que el formateador es capaz
        /// de formatear.
        /// </param>
        /// <param name="lengthManager">
        /// Es el objeto que administra el largo de los datos del campo.
        /// </param>
        /// <param name="includeSelfAnnouncementInLength">
        /// Flag para indicar que el largo del indicador de campo debe o no sumarse
        /// al largo de los datos.
        /// </param>
        /// <param name="selfAnnounceManager">
        /// Administrador que maneja los indicadores de campo.
        /// </param>
        /// <param name="encoder">
        /// Es el objeto capaz de codificar/decodificar los datos del campo.
        /// </param>
        /// <param name="padding">
        /// It's the field value padder.
        /// </param>
        /// <param name="validator">
        /// It's the field value validator.
        /// </param>
        /// <param name="valueFormatter">
        /// It's the field value formatter.
        /// </param>
        public SelfAnnouncedStringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            bool includeSelfAnnouncementInLength, SelfAnnouncedMarkerManager selfAnnounceManager,
            IDataEncoder encoder, IStringPadding padding, IStringValidator validator,
            IStringFieldValueFormatter valueFormatter)
            : this(fieldNumber, lengthManager, includeSelfAnnouncementInLength,
                selfAnnounceManager, encoder, padding, validator, valueFormatter, string.Empty)
        {
        }

        /// <summary>
        /// Construye un nuevo formateador de campos de mensajes cuyos
        /// datos son de tipo cadena de caracteres.
        /// </summary>
        /// <param name="fieldNumber">
        /// Es el n�mero de campo del mensaje que el formateador es capaz
        /// de formatear.
        /// </param>
        /// <param name="lengthManager">
        /// Es el objeto que administra el largo de los datos del campo.
        /// </param>
        /// <param name="includeSelfAnnouncementInLength">
        /// Flag para indicar que el largo del indicador de campo debe o no sumarse
        /// al largo de los datos.
        /// </param>
        /// <param name="selfAnnounceManager">
        /// Administrador que maneja los indicadores de campo.
        /// </param>
        /// <param name="encoder">
        /// Es el objeto capaz de codificar/decodificar los datos del campo.
        /// </param>
        /// <param name="padding">
        /// It's the field value padder.
        /// </param>
        /// <param name="validator">
        /// It's the field value validator.
        /// </param>
        /// <param name="valueFormatter">
        /// It's the field value formatter.
        /// </param>
        /// <param name="description">
        /// It's the description of the field formatter.
        /// </param>
        public SelfAnnouncedStringFieldFormatter(int fieldNumber, LengthManager lengthManager,
            bool includeSelfAnnouncementInLength, SelfAnnouncedMarkerManager selfAnnounceManager,
            IDataEncoder encoder, IStringPadding padding, IStringValidator validator,
            IStringFieldValueFormatter valueFormatter, string description)
            :
                base(fieldNumber, description)
        {
            if (lengthManager == null)
                throw new ArgumentNullException("lengthManager");

            if (selfAnnounceManager == null)
                throw new ArgumentNullException("selfAnnounceManager");

            if (encoder == null)
                throw new ArgumentNullException("encoder");

            _lengthManager = lengthManager;
            _includeSelfAnnouncementInLength = includeSelfAnnouncementInLength;
            _selfAnnounceManager = selfAnnounceManager;
            _encoder = encoder;
            _validator = validator;
            _valueFormatter = valueFormatter;

            if ((padding == null) && (lengthManager is FixedLengthManager))
                _padding = SpacePaddingRight.GetInstance(false);
            else
                _padding = padding;
        }

        /// <summary>
        /// It returns the field data length manager.
        /// </summary>
        public LengthManager LengthManager
        {
            get { return _lengthManager; }
        }

        /// <summary>
        /// It returns the flag which tells the field formatter to add the field
        /// indicator length to the encoded field data length.
        /// </summary>
        public bool IncludeSelfAnnouncementInLength
        {
            get { return _includeSelfAnnouncementInLength; }
        }

        /// <summary>
        /// It returns the field announcement manager.
        /// </summary>
        public SelfAnnouncedMarkerManager SelfAnnounceManager
        {
            get { return _selfAnnounceManager; }
        }

        /// <summary>
        /// It returns the field data encoder.
        /// </summary>
        public IDataEncoder Encoder
        {
            get { return _encoder; }
        }

        /// <summary>
        /// It returns the field data padder.
        /// </summary>
        public IStringPadding Padding
        {
            get { return _padding; }
        }

        /// <summary>
        /// It returns the field data validator.
        /// </summary>
        public IStringValidator Validator
        {
            get { return _validator; }
        }

        /// <summary>
        /// It returns the field data value formatter.
        /// </summary>
        public IStringFieldValueFormatter ValueFormatter
        {
            get { return _valueFormatter; }
        }

        #region ISelfAnnouncedFieldFormatter Members
        /// <summary>
        /// It informs the field number of the next field to be parsed.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <param name="fieldNumber">
        /// The field number of the field to be parsed.
        /// </param>
        /// <returns>
        /// A boolean value indicating if the field number was extracted from
        /// the parser context. If it returns true, the field number it's
        /// stored in <paramref name="fieldNumber"/>.
        /// </returns>
        /// <remarks>
        /// This function doesn't consume data from the parser context.
        /// </remarks>
        public bool GetFieldNumber(ref ParserContext parserContext, out int fieldNumber)
        {
            fieldNumber = -1;

            // If MinValue, at this moment the length hasn't been decoded.
            if (parserContext.DecodedLength == int.MinValue)
            {
                if (!_lengthManager.EnoughData(ref parserContext)) // Insufficient data to parse length, return null.
                    return false;

                // Save length in parser context just in case field value
                // can't be parsed at this time (more data needed).
                parserContext.DecodedLength = _lengthManager.ReadLength(ref parserContext);
            }

            if (parserContext.DataLength < _selfAnnounceManager.GetEncodedLength(ref parserContext))
                // Insufficient data to parse field announcement.
                return false;

            fieldNumber = _selfAnnounceManager.ReadAnnouncement(ref parserContext);
            return true;
        }
        #endregion

        /// <summary>
        /// Formats the specified field.
        /// </summary>
        /// <param name="field">
        /// It's the field to format.
        /// </param>
        /// <param name="formatterContext">
        /// It's the context of formatting to be used by the method.
        /// </param>
        public override void Format(Field field,
            ref FormatterContext formatterContext)
        {
            if (!(field is StringField))
                throw new ArgumentException("Field must be a string message field.", "field");

            string fieldValue = ((StringField) field).FieldValue;

            int announcementLength = 0;
            if (_includeSelfAnnouncementInLength)
                announcementLength = _selfAnnounceManager.GetEncodedLength(field,
                    ref formatterContext);

            // Pad if padding available.
            if (_padding != null)
                fieldValue = _padding.Pad(fieldValue,
                    _lengthManager.MaximumLength - announcementLength);

            if (_validator != null)
                _validator.Validate(fieldValue);

            if (fieldValue == null)
            {
                _lengthManager.WriteLength(field, announcementLength, announcementLength,
                    ref formatterContext);
                _selfAnnounceManager.WriteAnnouncement(field, ref formatterContext);
                _lengthManager.WriteLengthTrailer(field, 0, 0, ref formatterContext);
            }
            else
            {
                _lengthManager.WriteLength(field, fieldValue.Length + announcementLength,
                    _encoder.GetEncodedLength(fieldValue.Length) + announcementLength,
                    ref formatterContext);
                _selfAnnounceManager.WriteAnnouncement(field, ref formatterContext);
                _encoder.Encode(fieldValue, ref formatterContext);
                _lengthManager.WriteLengthTrailer(field, fieldValue.Length,
                    _encoder.GetEncodedLength(fieldValue.Length),
                    ref formatterContext);
            }
        }

        /// <summary>
        /// It parses the information in the parser context and builds the field.
        /// </summary>
        /// <param name="parserContext">
        /// It's the parser context.
        /// </param>
        /// <returns>
        /// The new field built with the information found in the parser context.
        /// </returns>
        public override Field Parse(ref ParserContext parserContext)
        {
            int decodedLength = parserContext.DecodedLength;
            if (_includeSelfAnnouncementInLength)
                decodedLength -= _selfAnnounceManager.GetEncodedLength(ref parserContext);

            if (parserContext.DataLength < _encoder.GetEncodedLength(decodedLength))
                // Insufficient data to parse field value, return null.
                return null;

            // Create the new messaging component with parsing context data.
            string fieldValue;
            if (_padding == null)
                fieldValue = _encoder.DecodeString(ref parserContext, decodedLength);
            else
                fieldValue = _padding.RemovePad(
                    _encoder.DecodeString(ref parserContext, decodedLength));

            if (_validator != null)
                _validator.Validate(fieldValue);

            var field = new StringField(FieldNumber, fieldValue);

            _lengthManager.ReadLengthTrailer(ref parserContext);

            return field;
        }
    }
}