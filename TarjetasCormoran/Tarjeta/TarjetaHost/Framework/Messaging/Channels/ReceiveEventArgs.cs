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

namespace Trx.Messaging.Channels {

	/// <summary>
	/// This class defines the arguments of the events which notifies
	/// the message reception.
	/// </summary>
	public class ReceiveEventArgs : EventArgs {

		private Message _message;

		#region Constructors
		/// <summary>
		/// Creates and initializes a new instance of type <see cref="ReceiveEventArgs"/>.
		/// </summary>
		/// <param name="message">
		/// It's the received message.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// message holds a null reference.
		/// </exception>
		public ReceiveEventArgs( Message message) {

			if ( message == null) {
				throw new ArgumentNullException( "message");
			}

			_message = message;
		}
		#endregion

		#region Properties
		/// <summary>
		/// It returns the received message.
		/// </summary>
		public Message Message {

			get {

				return _message;
			}
		}
		#endregion
	}
}
