﻿#region Copyright (C) 2004-2012 Zabaleta Asociados SRL
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
using System.Runtime.Serialization;

namespace Trx.Communication.Channels
{
    /// <summary>
    /// Represents errors that occur in channels.
    /// </summary>
    [Serializable]
    public class ChannelException : ApplicationException
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ChannelException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the class with a specified error message.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error.
        /// </param>
        public ChannelException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class with a reference to the inner exception that
        /// is the cause of this exception.
        /// </summary>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception. If the innerException
        /// parameter is not a null reference (Nothing in Visual Basic), the current
        /// exception is raised in a catch block that handles the inner exception.
        /// </param>
        public ChannelException(Exception innerException) :
            base(innerException.Message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class with a specified error message and a reference
        /// to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception. If the innerException
        /// parameter is not a null reference (Nothing in Visual Basic), the current
        /// exception is raised in a catch block that handles the inner exception.
        /// </param>
        public ChannelException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo" /> that holds the serialized object data about the
        /// exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext" /> that contains contextual information about the
        /// source or destination.
        /// </param>
        protected ChannelException(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
        #endregion
    }
}
