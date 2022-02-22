#region Copyright (C) 2004-2006 Diego Zabaleta, Leonardo Zabaleta
//
// Copyright © 2004-2006 Diego Zabaleta, Leonardo Zabaleta
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
#endregion

using System;

namespace Trx.Messaging.FlowControl {

	/// <summary>
	/// This interface defines which a class must implement to receive
	/// messages from other components.
	/// </summary>
	public interface IMessageProcessor {

		/// <summary>
		/// It's called to process the indicated message.
		/// </summary>
		/// <param name="source">
		/// It's the source of the message.
		/// </param>
		/// <param name="message">
		/// It's the message to be processed.
		/// </param>
		/// <returns>
		/// A logical value the same to true, if the messages processor
		/// processeced it, otherwise it returns false.
		/// </returns>
		/// <remarks>
		/// If the messages processor doesn't process it, the system
		/// delivers it to the next processor in the list, and so on until
		/// one process it, or there aren't other processors.
		/// </remarks>
		bool Process( IMessageSource source, Message message);

		/// <summary>
		/// It returns or sets the next messages processor.
		/// </summary>
		IMessageProcessor NextMessageProcessor {
			get;
			set;
		}
	}
}
