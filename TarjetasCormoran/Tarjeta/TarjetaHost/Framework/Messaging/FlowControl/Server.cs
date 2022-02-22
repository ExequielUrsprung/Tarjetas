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
using log4net;
using System.Reflection;

// TODO: Translate to english.

namespace Trx.Messaging.FlowControl {

	/// <summary>
	/// It's the <see cref="Server.Connected"/> event delegate.
	/// </summary>
	public delegate void ServerPeerConnectedEventHandler(
		object sender, ServerPeerConnectedEventArgs e);

	/// <summary>
	/// It's the <see cref="Server.Disconnected"/> event delegate.
	/// </summary>
	public delegate void ServerPeerDisconnectedEventHandler(
		object sender, ServerPeerDisconnectedEventArgs e);

	/// <summary>
	/// Implementa un servidor capaz de atender los requerimientos
	/// de los clientes del sistema.
	/// </summary>
	public class Server {

		private string _name;
		private IListener _listener;
		private IServerPeerManager _serverPeerManager;
		private ILog _logger = null;
		private bool _hostStart;

		#region Constructors
		/// <summary>
		/// Inicializa una nueva instancia de la clase <see cref="Server"/>.
		/// </summary>
		/// <param name="name">
		/// Es el nombre del servidor.
		/// </param>
		/// <param name="listener">
		/// Es el listener empleado para escuchar los requerimientos de conexión.
		/// </param>
		/// <param name="serverPeerManager">
		/// Es el administrador de puntos de conexión.
		/// </param>
		public Server( string name, IListener listener, IServerPeerManager serverPeerManager) {

			if ( StringUtilities.IsNullOrEmpty( name)) {
				throw new ArgumentNullException( "name");
			}

			if ( listener == null) {
				throw new ArgumentNullException( "listener");
			}

			if ( serverPeerManager == null) {
				throw new ArgumentNullException( "serverPeerManager");
			}

			_name = name;
			_hostStart = true;
			_serverPeerManager = serverPeerManager;
			_listener = listener;
			_listener.ConnectionRequest += new ListenerConnectionRequestEventHandler(
				OnListenerConnectionRequest);
			_listener.Connected += new ListenerConnectedEventHandler( OnListenerConnected);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Retorna el nombre del servidor.
		/// </summary>
		public string Name {

			get {

				return _name;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool HostStart {

			get {

				return _hostStart;
			}

			set {

				_hostStart = value;
			}
		}

		/// <summary>
		/// Retorna o asigna el logger empleado por la clase.
		/// </summary>
		public ILog Logger {

			get {

				if ( _logger == null) {
					_logger = LogManager.GetLogger(
						MethodBase.GetCurrentMethod().DeclaringType);
				}

				return _logger;
			}

			set {

				if ( value == null) {
					_logger = LogManager.GetLogger(
						MethodBase.GetCurrentMethod().DeclaringType);
				} else {
					_logger = value;
				}
			}
		}

		/// <summary>
		/// Retorna o asigna el nombre del logger que se utiliza.
		/// </summary>
		public string LoggerName {

			set {

				if ( StringUtilities.IsNullOrEmpty( value)) {
					Logger = null;
				} else {
					Logger = LogManager.GetLogger( value);
				}
			}

			get {

				return this.Logger.Logger.Name;
			}
		}

		/// <summary>
		/// Retorna el objeto oyente que atiende los requerimientos
		/// de conexión.
		/// </summary>
		public IListener Listener {

			get {

				return _listener;
			}
		}

		/// <summary>
		/// Retorna el conjunto de clientes conocidos por el servidor.
		/// </summary>
		/// <remarks>
		/// Los clientes pueden estar conectados o no.
		/// </remarks>
		public ServerPeerCollection Peers {

			get {

				return _serverPeerManager.Peers;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public IServerPeerManager PeerManager {

			get {

				return _serverPeerManager;
			}
		}
		#endregion

		#region Events
		/// <summary>
		/// Se dispara cuando el punto de conexión se ha conectado.
		///
		/// It's raised when the peer has been connected.
		/// </summary>
		public event ServerPeerConnectedEventHandler Connected;

		/// <summary>
		/// Se dispara cuando el punto de conexión se ha desconectado.
		///
		/// It's raised when the peer has been disconnected.
		/// </summary>
		public event ServerPeerDisconnectedEventHandler Disconnected;
		#endregion

		#region Methods
		/// <summary>
		/// Dispara el evento <see cref="Connected"/>.
		///
		/// It fires the <see cref="Connected"/> event.
		/// </summary>
		/// <param name="peer">
		/// Es el punto de conexión que se conectó.
		/// </param>
		private void OnConnected( ServerPeer peer) {

			if ( Connected != null) {
				Connected( this, new ServerPeerConnectedEventArgs( peer));
			}
		}

		/// <summary>
		/// Dispara el evento Disconnected.
		///
		/// It fires the <see cref="Disconnected"/> event.
		/// </summary>
		/// <param name="peer">
		/// Es el punto de conexión que se desconectó.
		/// </param>
		private void OnDisconnected( ServerPeer peer) {

			if ( Disconnected != null) {
				Disconnected( this, new ServerPeerDisconnectedEventArgs( peer));
			}
		}

		/// <summary>
		/// Maneja el evento <see cref="IListener.ConnectionRequest"/>.
		/// </summary>
		/// <param name="sender">
		/// Es el objeto que envía el evento.
		/// </param>
		/// <param name="e">
		/// Son los parámetros del evento.
		/// </param>
		private void OnListenerConnectionRequest( object sender,
			ListenerConnectionRequestEventArgs e) {

			e.Accept = _serverPeerManager.AcceptConnectionRequest( e.ConnectionInfo);
		}

		/// <summary>
		/// Maneja el evento <see cref="Peer.Disconnected"/>.
		/// </summary>
		/// <param name="sender">
		/// Es el objeto que envía el evento.
		/// </param>
		/// <param name="e">
		/// Son los parámetros del evento.
		/// </param>
		/// <remarks>
		/// El servidor devuelve el canal del punto de conexión al pool de
		/// canales del oyente.
		/// </remarks>
		private void OnPeerDisconnected( object sender, EventArgs e) {

			ServerPeer peer = ( ServerPeer)sender;

			peer.Disconnected -= new PeerDisconnectedEventHandler( OnPeerDisconnected);

			if ( Logger.IsDebugEnabled) {
				Logger.Debug( string.Format( "Server '{0}' - OnPeerDisconnected '{1}'.",
					_name, peer.Name));
			}

			OnDisconnected( peer);
		}

		/// <summary>
		/// Maneja el evento <see cref="IListener.Connected"/>.
		/// </summary>
		/// <param name="sender">
		/// Es el objeto que envía el evento.
		/// </param>
		/// <param name="e">
		/// Son los parámetros del evento.
		/// </param>
		private void OnListenerConnected( object sender, ListenerConnectedEventArgs e) {

			ServerPeer peer = _serverPeerManager.Connected( e.Channel);

			peer.Disconnected += new PeerDisconnectedEventHandler( OnPeerDisconnected);

			if ( Logger.IsDebugEnabled) {
				Logger.Debug( string.Format( "Server '{0}' - OnListenerConnected '{1}'.",
					_name, peer.Name));
			}

			OnConnected( peer);
		}
		#endregion
	}
}