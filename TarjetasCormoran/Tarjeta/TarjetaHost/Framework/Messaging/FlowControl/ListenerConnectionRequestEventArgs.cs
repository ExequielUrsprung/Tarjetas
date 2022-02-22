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
	/// This class defines the arguments of the event <see cref="IListener.ConnectionRequest"/>.
	/// </summary>
	public class ListenerConnectionRequestEventArgs : EventArgs {

		private bool _accept = true;
		private object _connectionInfo;

		#region Constructors
		/// <summary>
		/// It creates and initializes a new instance of the
		/// type <see cref="ListenerConnectionRequestEventArgs"/>.
		/// </summary>
		/// <param name="connectionInfo">
		/// It's the associated information to the connection request.
		/// </param>
		public ListenerConnectionRequestEventArgs( object connectionInfo) {

			_connectionInfo = connectionInfo;
		}
		#endregion

		#region Properties
		/// <summary>
		/// It returns or sets the parameter which allows to accept or deny the
		/// incoming connection.
		/// </summary>
		public bool Accept {

			get {

				return _accept;
			}

			set {

				_accept = true;
			}
		}

		/// <summary>
		/// It returns the associated information to the connection request.
		/// </summary>
		/// <remarks>
		/// The type of this parameter if it holds a valid value, will depend
		/// of the class which implements the interface <see cref="IListener"/>.
		/// </remarks>
		public object ConnectionInfo {

			get {

				return _connectionInfo;
			}
		}
		#endregion
	}
}
