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
using Trx.Messaging.Channels;

namespace Trx.Messaging.FlowControl {

	/// <summary>
	/// This class defines the arguments of the event <see cref="IListener.Connected"/>.
	/// </summary>
	public class ListenerConnectedEventArgs : EventArgs {

		private IChannel _channel;

		#region Constructors
		/// <summary>
		/// It creates and initializes a new instance of the
		/// type <see cref="ListenerConnectedEventArgs"/>.
		/// </summary>
		/// <param name="channel">
		/// It's the accepted channel.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// channel holds a null invalid reference.
		/// </exception>
		public ListenerConnectedEventArgs( IChannel channel) {

			if ( channel == null) {
				throw new ArgumentNullException( "channel");
			}

			_channel = channel;
		}
		#endregion

		#region Properties
		/// <summary>
		/// It returns the accepted channel.
		/// </summary>
		public IChannel Channel {

			get {

				return _channel;
			}
		}
		#endregion
	}
}
