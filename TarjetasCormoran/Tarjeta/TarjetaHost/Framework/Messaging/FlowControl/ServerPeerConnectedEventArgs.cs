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

// TODO: Translate to english.

namespace Trx.Messaging.FlowControl {

	/// <summary>
	/// Esta clase define los argumentos del evento <see cref="Server.Connected"/>.
	/// </summary>
	public class ServerPeerConnectedEventArgs : EventArgs {

		private ServerPeer _peer;

		#region Constructors
		/// <summary>
		/// Crea e inicializa una nueva clase del tipo <see cref="ServerPeerConnectedEventArgs"/>.
		/// </summary>
		/// <param name="peer">
		/// Es el punto de conexión que se ha conectado.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// peer contiene una referencia nula.
		/// </exception>
		public ServerPeerConnectedEventArgs( ServerPeer peer) {

			if ( peer == null) {
				throw new ArgumentNullException( "peer");
			}

			_peer = peer;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Retorna el punto de conexión que se ha conectado.
		/// </summary>
		public ServerPeer Peer {

			get {

				return _peer;
			}
		}
		#endregion
	}
}
