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

using Trx.Exceptions;

namespace Trx.Messaging
{
    public class BinaryFieldFormatterFactory : FieldFormatterFactory
    {
        public IDataEncoderFactory Encoder { get; set; }

        public ILengthManagerFactory LengthManager { get; set; }

        public override FieldFormatter GetInstance()
        {
            if (Encoder == null)
                throw new ConfigurationException("A binary encoder factory must be set in property Encoder");

            if (LengthManager == null)
                throw new ConfigurationException("A length manager factory must be set in property LengthManager");

            return new BinaryFieldFormatter(FieldNumber, LengthManager.GetInstance(), Encoder.GetInstance(), Description);
        }
    }
}
