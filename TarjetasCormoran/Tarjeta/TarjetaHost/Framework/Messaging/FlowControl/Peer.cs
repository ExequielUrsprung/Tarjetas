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
using System.Collections;
using Trx.Messaging;
using Trx.Messaging.Channels;
using Trx.Utilities;

// TODO: Translate to english.

namespace Trx.Messaging.FlowControl {

	/// <summary>
	/// Es el delegado del evento <see cref="Peer.Connected"/>.
	/// </summary>
	public delegate void PeerConnectedEventHandler( object sender, EventArgs e);

	/// <summary>
	/// Es el delegado del evento <see cref="Peer.Disconnected"/>.
	/// </summary>
	public delegate void PeerDisconnectedEventHandler( object sender, EventArgs e);

	/// <summary>
	/// Es el delegado del evento <see cref="Peer.Receive"/>.
	/// </summary>
	public delegate void PeerReceiveEventHandler( object sender, ReceiveEventArgs e);

	/// <summary>
	/// Es el delegado del evento <see cref="Peer.RequestDone"/>.
	/// </summary>
	public delegate void PeerRequestDoneEventHandler(
		object sender, PeerRequestDoneEventArgs e);

	/// <summary>
	/// Es el delegado del evento <see cref="Peer.RequestCancelled"/>.
	/// </summary>
	public delegate void PeerRequestCancelledEventHandler(
		object sender, PeerRequestCancelledEventArgs e);

	/// <summary>
	/// Es el delegado del evento <see cref="Peer.Error"/>.
	/// </summary>
	public delegate void PeerErrorEventHandler( object sender, ErrorEventArgs e);

	/// <summary>
	/// Esta clase encapsula los servicios de un canal.
	/// </summary>
	public abstract class Peer : IMessageSource, IMessageProcessor, IDisposable {

		private IMessageProcessor _messageProcessor;
		private IMessageProcessor _nextMessageProcessor;
		private IMessagesIdentifier _messagesIdentifier;
		private Hashtable _pendingRequests;
		private string _name;
		private bool _hostConnect;
		private IChannel _channel = null;

		#region Constructors
		/// <summary>
		/// Inicializa una nueva instancia de la clase <see cref="Peer"/>.
		/// </summary>
		/// <param name="name">
		/// Es el nombre del punto de conexión.
		/// </param>
		public Peer( string name) {

			if ( StringUtilities.IsNullOrEmpty( name)) {
				throw new ArgumentNullException( "name");
			}

			_name = name;
			_hostConnect = true;
			_messageProcessor = null;
			_messagesIdentifier = null;
			_pendingRequests = null;
			_nextMessageProcessor = null;
		}

		/// <summary>
		/// Inicializa una nueva instancia de la clase <see cref="Peer"/>,
		/// configurándola para procesar requerimientos.
		/// </summary>
		/// <param name="name">
		/// Es el nombre del punto de conexión.
		/// </param>
		/// <param name="messagesIdentifier">
		/// Es el identificador de mensajes.
		/// </param>
		public Peer( string name, IMessagesIdentifier messagesIdentifier) : this( name) {

			if ( messagesIdentifier == null) {
				throw new ArgumentNullException( "messagesIdentifier");
			}

			_messagesIdentifier = messagesIdentifier;
			_pendingRequests = new Hashtable( 64);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Informa si el punto de conexión está conectado.
		///
		/// It informs whether the peer is connected.
		/// </summary>
		public bool IsConnected {

			get {

				if ( _channel == null) {
					return false;
				}

				return _channel.IsConnected;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		protected IChannel ProtectedChannel {

			set {

				if ( _channel != null) {
					_channel.MessageProcessor = null;
					_channel.Disconnected -= new ChannelDisconnectedEventHandler( OnChannelDisconnected);
					_channel.Connected -= new ChannelConnectedEventHandler( OnChannelConnected);
					_channel.Error -= new ChannelErrorEventHandler( OnChannelError);
				}

				_channel = value;

				if ( _channel != null) {
					_channel.Error += new ChannelErrorEventHandler( OnChannelError);
					_channel.Connected += new ChannelConnectedEventHandler( OnChannelConnected);
					_channel.Disconnected += new ChannelDisconnectedEventHandler( OnChannelDisconnected);
					_channel.MessageProcessor = this;
				}
			}
		}

		/// <summary>
		/// Es el canal que emplea el punto de conexión para comunicarse
		/// con el sistema remoto.
		///
		/// It's the channel used by the peer to communicate with the
		/// remote system.
		/// </summary>
		public IChannel Channel {

			get {

				return _channel;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool HostConnect {

			get {

				return _hostConnect;
			}

			set {

				_hostConnect = value;
			}
		}

		/// <summary>
		/// Retorna el nombre del punto de conexión.
		/// </summary>
		public string Name {

			get {

				return _name;
			}
		}

		/// <summary>
		/// Retorna o asigna el procesador de mensajes recibidos.
		/// </summary>
		public IMessageProcessor MessageProcessor {

			get {

				return _messageProcessor;
			}

			set {

				_messageProcessor = value;
			}
		}

		/// <summary>
		/// Retorna el identificador de mensajes.
		/// </summary>
		/// <remarks>
		/// El identificador de mensajes se utiliza para corresponder
		/// el mensaje que se envía con el que se recibe en un
		/// requerimiento.
		/// </remarks>
		public IMessagesIdentifier MessagesIdentifier {

			get {

				return _messagesIdentifier;
			}
		}

		/// <summary>
		/// Retorna o asigna el siguiente procesador de mensajes.
		/// </summary>
		public IMessageProcessor NextMessageProcessor {

			get {

				return _nextMessageProcessor;
			}

			set {

				_nextMessageProcessor = value;
			}
		}
		#endregion

		#region Events
		/// <summary>
		/// Se dispara cuando el canal se ha conectado.
		/// </summary>
		public event PeerConnectedEventHandler Connected;

		/// <summary>
		/// Se dispara cuando el canal se ha desconectado.
		/// </summary>
		public event PeerDisconnectedEventHandler Disconnected;

		/// <summary>
		/// Se dispara cuando se ha recibido un mensaje.
		/// </summary>
		public event PeerReceiveEventHandler Receive;

		/// <summary>
		/// Se dispara cuando se ha completado un requerimiento.
		/// </summary>
		public event PeerRequestDoneEventHandler RequestDone;

		/// <summary>
		/// Se dispara cuando se ha cancelado un requerimiento.
		/// </summary>
		public event PeerRequestCancelledEventHandler RequestCancelled;

		/// <summary>
		/// Se dispara cuando se ha producido un error en procesamiento
		/// interno del Peer.
		/// </summary>
		/// <remarks>
		/// Este evento se recibe desde el Peer, cuando se produce un error
		/// que provoca que el Peer quede deshabilitado, debiendo ser
		/// necesario invocar nuevamente a <see cref="Connect"/> para
		/// utilizarlo.
		/// </remarks>
		public event PeerErrorEventHandler Error;
		#endregion

		#region Methods
		/// <summary>
		/// Dispara el evento Error.
		/// </summary>
		/// <param name="e">
		/// Son los parámetros del error que se recibieron
		/// desde el canal.
		/// </param>
		protected virtual void OnError( ErrorEventArgs e) {

			if ( Error != null) {
				Error( this, e);
			}
		}

		/// <summary>
		/// Dispara el evento RequestDone.
		/// </summary>
		/// <param name="request">
		/// Es el requerimiento que se ha completado.
		/// </param>
		protected virtual void OnRequestDone( PeerRequest request) {

			if ( RequestDone != null) {
				RequestDone( this, new PeerRequestDoneEventArgs( request));
			}
		}

		/// <summary>
		/// Dispara el evento RequestCancelled.
		/// </summary>
		/// <param name="request">
		/// Es el requerimiento que se ha cancelado.
		/// </param>
		protected virtual void OnRequestCancelled( PeerRequest request) {

			if ( RequestCancelled != null) {
				RequestCancelled( this, new PeerRequestCancelledEventArgs( request));
			}
		}

		/// <summary>
		/// Dispara el evento <see cref="Receive"/>, luego, si el mensaje
		/// recibido es la respuesta a un requerimiento dispara el evento
		/// <see cref="RequestDone"/> y finaliza la ejecución del método,
		/// en caso contrario invoca a cada procesador de mensajes hasta
		/// que alguno lo procese o no hayan mas procesadores de mensajes en
		/// la lista.
		/// </summary>
		/// <param name="message">
		/// Es el mensaje que se ha recibido.
		/// </param>
		protected virtual void OnReceive( Message message) {

            PeerRequest request = null;

            // Check if received message is response of a pending request.
            if ( ( _pendingRequests != null ) ) {
                lock ( _pendingRequests ) {
                    if ( _pendingRequests.Count > 0 ) {
                        object messageKey = _messagesIdentifier.ComputeIdentifier( message );
                        if ( messageKey != null ) {
                            request = ( PeerRequest )_pendingRequests[messageKey];
                            if ( request != null ) {
                                _pendingRequests.Remove( messageKey );
                            }
                        }
                    }
                }
            }

            if ( request == null ) {

                // The received message not matched a pending request,
                // notify clients via Receive event and peer associated
                // processors.

                if ( Receive != null ) {
                    Receive( this, new ReceiveEventArgs( message ) );
                }

                IMessageProcessor processor = _messageProcessor;

                while ( ( processor != null ) &&
                    !processor.Process( this, message ) ) {
                    processor = processor.NextMessageProcessor;
                }
            }
            else {
                // The message is the response of a pending request.
                if ( request.SetResponseMessage( message ) ) {
                    OnRequestDone( request );
                }
                else {
                    // The peer request was signaled as expired by the request timer.
                    OnRequestCancelled( request );
                }
            }
		}

		/// <summary>
		/// Dispara el evento Connected.
		/// </summary>
		protected virtual void OnConnected() {

			if ( Connected != null) {
				Connected( this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Dispara el evento Disconnected.
		/// </summary>
		protected virtual void OnDisconnected() {

			if ( Disconnected != null) {
				Disconnected( this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Esta función es invocada por el canal para que se procese el
		/// mensaje indicado.
		/// </summary>
		/// <param name="source">
		/// Es el origen del mensaje.
		/// </param>
		/// <param name="message">
		/// Es el mensaje que debe ser 
		/// </param>
		/// <returns>
		/// Un valor lógico igual a verdadero, indicando que
		/// procesó el mensaje.
		/// </returns>
		public virtual bool Process( IMessageSource source, Message message) {

			OnReceive( message);

			return true;
		}

		/// <summary>
		/// Inicia la conexión con el sistema remoto.
		///
		/// It initiates the connection with the remote system.
		/// </summary>
		public virtual void Connect() {

			if ( Channel != null) {
				Channel.Connect();
			}
		}

		/// <summary>
		/// Cierra la conexión, si existe, que se tiene con el sistema remoto.
		///
		/// Closes the connection, if exists, stablished with the remote system.
		/// </summary>
		public virtual void Close() {

			if ( Channel != null) {
				Channel.Close();
			}
		}

		/// <summary>
		/// Le envía el mensaje indicado al sistema remoto.
		///
		/// It sends the indicated message to the remote system.
		/// </summary>
		/// <param name="message">
		/// Es el mensaje que se desea enviar.
		///
		/// It's the message to be sent.
		/// </param>
		public virtual void Send( Message message) {

			if ( Channel != null) {
				Channel.Send( message);
			}
		}

		/// <summary>
		/// Envía un mensaje del que se espera respuesta.
		/// </summary>
		/// <param name="request">
		/// Es el requerimiento que encapsula el mensaje
		/// enviado y el mensaje recibido.
		/// </param>
		internal virtual void Send( PeerRequest request) {

			object messageKey = _messagesIdentifier.ComputeIdentifier(
				request.RequestMessage);

            lock ( _pendingRequests ) {
				if ( _pendingRequests.Contains( messageKey)) {
					_pendingRequests.Remove( messageKey);
				}

				Send( request.RequestMessage);
				request.MarkAsTransmitted();
				_pendingRequests.Add( messageKey, request);
			}
		}

		/// <summary>
		/// Cancela un requerimiento en progreso.
		/// </summary>
		/// <param name="request">
		/// Es el requerimiento a cancelar.
		/// </param>
		internal virtual void Cancel( PeerRequest request) {

			object messageKey = _messagesIdentifier.ComputeIdentifier(
				request.RequestMessage);

			if ( _pendingRequests.Contains( messageKey)) {
                lock ( _pendingRequests ) {
					if ( _pendingRequests.Contains( messageKey)) {
						_pendingRequests.Remove( messageKey);
						OnRequestCancelled( request);
					}
				}
			}
		}

		/// <summary>
		/// Maneja el evento <see cref="IChannel.Error"/>.
		///
		/// It handles the <see cref="IChannel.Error"/> event.
		/// </summary>
		/// <param name="sender">
		/// Es el canal que envía el evento.
		///
		/// It's the channel which sends the event.
		/// </param>
		/// <param name="e">
		/// Son los parámetros del evento.
		///
		/// They are the parameters of the event.
		/// </param>
		protected virtual void OnChannelError( object sender, ErrorEventArgs e) {

			OnError( e);
		}

		/// <summary>
		/// Maneja el evento <see cref="IChannel.Connected"/>.
		///
		/// It handles the <see cref="IChannel.Connected"/> event.
		/// </summary>
		/// <param name="sender">
		/// Es el canal que envía el evento.
		///
		/// It's the channel which sends the event.
		/// </param>
		/// <param name="e">
		/// Son los parámetros del evento.
		///
		/// They are the parameters of the event.
		/// </param>
		protected virtual void OnChannelConnected( object sender, EventArgs e) {

			OnConnected();
		}

		/// <summary>
		/// Maneja el evento <see cref="IChannel.Disconnected"/>.
		///
		/// It handles the <see cref="IChannel.Connected"/> event.
		/// </summary>
		/// <param name="sender">
		/// Es el canal que envía el evento.
		///
		/// It's the channel which sends the event.
		/// </param>
		/// <param name="e">
		/// Son los parámetros del evento.
		///
		/// They are the parameters of the event.
		/// </param>
		protected virtual void OnChannelDisconnected( object sender, EventArgs e) {

			OnDisconnected();
		}

		/// <summary>
		/// 
		/// </summary>
		public virtual void Dispose() {

			this.ProtectedChannel = null;
		}
		#endregion
	}
}