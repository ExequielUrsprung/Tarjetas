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
using Trx.Utilities;

// TODO: Translate to english.

namespace Trx.Messaging.FlowControl {

	/// <summary>
	/// Esta clase encapsula los servicios de un punto de conexión
	/// de tipo servidor.
	/// </summary>
	/// <remarks>
	/// Los puntos de conexión de tipo servidor son aquellos utilizados
	/// por los objetos que implementan <see cref="IServerPeerManager"/>.
	/// </remarks>
	public class ServerPeer : Peer {

		#region Constructors
		/// <summary>
		/// Inicializa una nueva instancia de la clase <see cref="ServerPeer"/>.
		/// </summary>
		/// <param name="name">
		/// Es el nombre del punto de conexión.
		/// </param>
		public ServerPeer( string name) : base( name) {

		}

		/// <summary>
		/// Inicializa una nueva instancia de la clase <see cref="ServerPeer"/>,
		/// configurándola para procesar requerimientos.
		/// </summary>
		/// <param name="name">
		/// Es el nombre del punto de conexión.
		/// </param>
		/// <param name="messagesIdentifier">
		/// Es el identificador de mensajes.
		/// </param>
		public ServerPeer( string name, IMessagesIdentifier messagesIdentifier) :
			base( name, messagesIdentifier) {

		}
		#endregion

		#region Methods
		/// <summary>
		/// Asocia el punto de conexión a un canal.
		/// </summary>
		/// <param name="channel">
		/// Es el canal a asociar al punto de conexión.
		/// </param>
		public virtual void Bind( IChannel channel) {

			if ( Channel != null) {
				throw new InvalidOperationException( "The peer is already binded.");
			}

			if ( channel == null) {
				throw new ArgumentNullException( "channel");
			}

			ProtectedChannel = channel;
		}

		/// <summary>
		/// Inicia la conexión con el sistema remoto.
		/// </summary>
		public override void Connect() {

			throw new InvalidOperationException( "Invalid operation for this kind of peer.");
		}
		#endregion
	}
}
