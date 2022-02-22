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

namespace Trx.Messaging.FlowControl {

	/// <summary>
	/// This class implements the basic functionality to be a
	/// server peer manager.
	/// </summary>
	public class BasicServerPeerManager : IServerPeerManager, IMessageProcessor {

		private ServerPeerCollection _peers;
		private IMessageProcessor _messageProcessor;
		private IMessagesIdentifier _messagesIdentifier;
		private ILog _logger = null;

		// Used to get different names for new server peers.
		private int _nextPeerNumber;

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the class
		/// <see cref="BasicServerPeerManager"/>.
		/// </summary>
		public BasicServerPeerManager() {

			_peers = new ServerPeerCollection();
			_messageProcessor = null;
			_nextPeerNumber = 1;
			_messagesIdentifier = null;
		}
		#endregion

		#region Properties
		/// <summary>
		/// It returns the collection of known peers by the server peer
		/// manager.
		/// </summary>
		public ServerPeerCollection Peers {

			get {

				return _peers;
			}
		}

		/// <summary>
		/// It returns or sets the messages identificator which are
		/// assigned to each new connection point.
		/// </summary>
		public IMessagesIdentifier MessagesIdentifier {

			get {

				return _messagesIdentifier;
			}

			set {

				_messagesIdentifier = value;
			}
		}

		/// <summary>
		/// It returns the logger used by the class.
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
		/// It returns the logger name used by the class.
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
		/// It returns or sets the objets which process the received messages
		/// by the connections points.
		/// </summary>
		/// <remarks>
		/// The server peer manager sends every received message from the
		/// peers to the messages processor set here.
		/// </remarks>
		public IMessageProcessor MessageProcessor {

			get {

				return _messageProcessor;
			}

			set {

				_messageProcessor = value;
			}
		}

		/// <summary>
		/// It returns or sets the next messages processor.
		/// </summary>
		public IMessageProcessor NextMessageProcessor {

			get {

				return null;
			}

			set {
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// This function is used to know if the peer manager accepts the connection
		/// request.
		/// </summary>
		/// <param name="connectionInfo">
		/// It's the connection request information. The server peer manager
		/// can use this information in order to take the decision to accept
		/// or not the connection request.
		/// </param>
		/// <returns>
		/// A logical value equal to true if the connection request is accepted,
		/// otherwise false .
		/// </returns>
		/// <remarks>
		/// The connection requests arrives from objects implementing
		/// <see cref="IListener"/> interface.
		/// </remarks>
		public virtual bool AcceptConnectionRequest( object connectionInfo) {

			return true;
		}

		/// <summary>
		/// Disables the <see cref="ServerPeer"/>.
		/// </summary>
		/// <param name="peer">
		/// It's the peer to disable.
		/// </param>
		/// <remarks>
		/// It's called when the peer channel is disconnected or an
		/// error occurs.
		/// </remarks>
		protected virtual void DisablePeer( ServerPeer peer) {

			lock ( this) {
				if ( _peers.Contains( peer.Name)) {

					if ( Logger.IsDebugEnabled) {
						Logger.Debug( string.Format( "BasicServerPeerManager - DisablePeer = {0}.", peer.Name));
					}

					peer.MessageProcessor = null;
					peer.Disconnected -= new PeerDisconnectedEventHandler( OnPeerDisconnected);
					_peers.Remove( peer.Name);
					peer.Dispose();
				}
			}
		}

		/// <summary>
		/// It handles the event <see cref="Peer.Disconnected"/>.
		/// </summary>
		/// <param name="sender">
		/// It's the <see cref="ServerPeer"/> which sends the event.
		/// </param>
		/// <param name="e">
		/// It's the event paremeters.
		/// </param>
		private void OnPeerDisconnected( object sender, EventArgs e) {

			if ( sender is ServerPeer) {
				DisablePeer( ( ServerPeer)sender);
			}
		}

		/// <summary>
		/// It creates a new server peer, and associates the provided
		/// channel to it.
		/// </summary>
		/// <param name="channel">
		/// It's the channel to associate with the peer.
		/// </param>
		/// <returns>
		/// It's the new server peer.
		/// </returns>
		protected virtual ServerPeer GetServerPeer(  IChannel channel) {

			ServerPeer peer = null;
			string peerName = string.Format( "Peer-{0}", _nextPeerNumber++);

			if ( _messagesIdentifier == null) {
				peer = new ServerPeer( peerName);
			} else {
				peer = new ServerPeer( peerName, _messagesIdentifier);
			}

			peer.MessageProcessor = this;

			peer.Disconnected += new PeerDisconnectedEventHandler( OnPeerDisconnected);

			peer.Bind( channel);

			if ( Logger.IsDebugEnabled) {
				Logger.Info( string.Format( "BasicServerPeerManager - GetServerPeer = {0}.", peerName));
			}

			return peer;
		}

		/// <summary>
		/// Through the invocation of this method <see cref="Server"/> informs to
		/// the peer manager of the connection of the indicated channel.
		/// </summary>
		/// <param name="channel">
		/// It's the connected channel.
		/// </param>
		/// <returns>
		/// The peer associated to the channel.
		/// </returns>
		/// <remarks>
		/// Normally at this time the peers manager associates the channel to the peer.
		/// </remarks>
		public ServerPeer Connected( IChannel channel) {

			ServerPeer peer = null;

			lock ( this) {
				peer = GetServerPeer( channel);
				_peers.Add( peer);
			}

			return peer;
		}

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
		public virtual bool Process( IMessageSource source, Message message) {

			bool ret = false;

			if ( _messageProcessor != null) {
				if ( source is ServerPeer) {
					if ( _peers.Contains( ( ( ServerPeer)source).Name)) {
						ret = _messageProcessor.Process( source, message);
					}
				}
			}

			return ret;
		}
		#endregion
	}
}
