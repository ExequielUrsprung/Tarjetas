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
using Trx.Messaging.Channels;
using Trx.Utilities;
using log4net;
using System.Reflection;

namespace Trx.Messaging.FlowControl {

	/// <summary>
	/// This class implements a basic pool of channels.
	/// </summary>
	public class BasicChannelPool : IChannelPool {

		private const string DefaultChannelPoolName = "BasicChannelPool";

		private int _capacity;
		private Queue _channels;
		private string _name;
		private ILog _logger = null;

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the class <see cref="BasicChannelPool"/>,
		/// adding as many channels of the type as it's indicated.
		/// </summary>
		/// <param name="channelTypeName">
		/// It's the channel type name used to create the channels to fill up the
		/// pool.
		/// </param>
		/// <param name="capacity">
		/// It's the maximum quantity of channels that the pool of channels can store.
		/// </param>
		/// <param name="formatter">
		/// It's the messages formatter to be associated with the channels stored in
		/// the pool of channels.
		/// </param>
		/// <param name="loggerName">
		/// It's the logger name to associate to the channel.
		/// </param>
		/// <param name="channelName">
		/// It's the channel name.
		/// </param>
		/// <param name="name">
		/// It's the pool name.
		/// </param>
		/// <remarks>
		/// When a channel created by the pool is disconnected from its peer,
		/// it's automatically returned to the pool.
		/// </remarks>
		public BasicChannelPool( string channelTypeName, int capacity,
			IMessageFormatter formatter, string loggerName, string channelName, string name) {

			if ( StringUtilities.IsNullOrEmpty( channelTypeName)) {
				throw new ArgumentNullException( "channelTypeName");
			}

			Type type = Type.GetType( channelTypeName);

			if ( !typeof( IChannel).IsAssignableFrom( type)) {
				throw new ArgumentException(
					"The type for the channel doesn't implement the channel interface.",
					"channelTypeName");
			}

			_capacity = capacity;
			_channels = new Queue( _capacity);
			_name = name;

			if ( StringUtilities.IsNullOrEmpty( channelName)) {
				if ( StringUtilities.IsNullOrEmpty( _name)) {
					channelName = string.Format( "{0}Channel", DefaultChannelPoolName);
				} else {
					channelName = string.Format( "{0}Channel", _name);
				}
			}

			object[] args = { formatter};

			for ( int i = 0; i < capacity; i++) {
				IChannel channel = ( IChannel)Activator.CreateInstance( type, args);
				channel.Disconnected += new ChannelDisconnectedEventHandler( OnChannelDisconnected);
				if ( !StringUtilities.IsNullOrEmpty( loggerName)) {
					channel.LoggerName = loggerName;
				}
				channel.Name = string.Format( "{0}{1}", channelName, i + 1);
				_channels.Enqueue( channel);
			}
		}

		/// <summary>
		/// Initializes a new instance of the class <see cref="BasicChannelPool"/>,
		/// adding as many channels of the type as it's indicated.
		/// </summary>
		/// <param name="channelTypeName">
		/// It's the channel type name used to create the channels to fill up the
		/// pool.
		/// </param>
		/// <param name="capacity">
		/// It's the maximum quantity of channels that the pool of channels can store.
		/// </param>
		/// <param name="formatter">
		/// It's the messages formatter to be associated with the channels stored in
		/// the pool of channels.
		/// </param>
		/// <param name="loggerName">
		/// It's the logger name to associate to the channel.
		/// </param>
		/// <param name="channelName">
		/// It's the channel name.
		/// </param>
		/// <remarks>
		/// When a channel created by the pool is disconnected from its peer,
		/// it's automatically returned to the pool.
		/// </remarks>
		public BasicChannelPool( string channelTypeName, int capacity,
			IMessageFormatter formatter, string loggerName,
			string channelName) : this( channelTypeName, capacity, formatter, null, null, null) {

		}

		/// <summary>
		/// Initializes a new instance of the class <see cref="BasicChannelPool"/>,
		/// adding as many channels of the type as it's indicated.
		/// </summary>
		/// <param name="channelTypeName">
		/// It's the channel type name used to create the channels to fill up the
		/// pool.
		/// </param>
		/// <param name="capacity">
		/// It's the maximum quantity of channels that the pool of channels can store.
		/// </param>
		/// <param name="formatter">
		/// It's the messages formatter to be associated with the channels stored in
		/// the pool of channels.
		/// </param>
		/// <param name="name">
		/// It's the pool name.
		/// </param>
		/// <remarks>
		/// When a channel created by the pool is disconnected from its peer,
		/// it's automatically returned to the pool.
		/// </remarks>
		public BasicChannelPool( string channelTypeName, int capacity,
			IMessageFormatter formatter, string name) :
			this( channelTypeName, capacity, formatter, null, null, name) {

		}

		/// <summary>
		/// Initializes a new instance of the class <see cref="BasicChannelPool"/>,
		/// adding as many channels of the type as it's indicated.
		/// </summary>
		/// <param name="channelTypeName">
		/// It's the channel type name used to create the channels to fill up the
		/// pool.
		/// </param>
		/// <param name="capacity">
		/// It's the maximum quantity of channels that the pool of channels can store.
		/// </param>
		/// <param name="formatter">
		/// It's the messages formatter to be associated with the channels stored in
		/// the pool of channels.
		/// </param>
		/// <remarks>
		/// When a channel created by the pool is disconnected from its peer,
		/// it's automatically returned to the pool.
		/// </remarks>
		public BasicChannelPool( string channelTypeName, int capacity,
			IMessageFormatter formatter) : this( channelTypeName, capacity, formatter,
			null, null) {

		}

		/// <summary>
		/// Initializes a new instance of the class <see cref="BasicChannelPool"/>,
		/// adding as many clone channels as it's indicated.
		/// </summary>
		/// <param name="channel">
		/// It's the channel to clone and fill up the pool.
		/// The one received as parameter it's added to the pool too.
		/// </param>
		/// <param name="capacity">
		/// It's the maximum quantity of channels that the pool of channels can store.
		/// </param>
		/// <param name="name">
		/// It's the pool name.
		/// </param>
		/// <remarks>
		/// When a channel created by the pool is disconnected from its peer,
		/// it's automatically returned to the pool.
		/// </remarks>
		public BasicChannelPool( IChannel channel, int capacity, string name) {

			if ( !typeof( ICloneable).IsAssignableFrom( channel.GetType())) {
				throw new ArgumentException(
					"The type for the channel doesn't implements the ICloneable interface.");
			}

			_capacity = capacity;
			_channels = new Queue( _capacity);
			_name = name;

			if ( StringUtilities.IsNullOrEmpty( channel.Name)) {
				if ( StringUtilities.IsNullOrEmpty( _name)) {
					channel.Name = string.Format( "{0}Channel", DefaultChannelPoolName);
				} else {
					channel.Name = string.Format( "{0}Channel", _name);
				}
			}

			channel.Disconnected += new ChannelDisconnectedEventHandler( OnChannelDisconnected);
			_channels.Enqueue( channel);
			for ( int i = capacity - 1; i > 0; i--) {
				IChannel newChannel = channel.Clone() as IChannel;
				newChannel.Disconnected += new ChannelDisconnectedEventHandler( OnChannelDisconnected);
				newChannel.Name = string.Format( "{0}Clone{1}", newChannel.Name, i);
				_channels.Enqueue( newChannel);
			}
		}

		/// <summary>
		/// Initializes a new instance of the class <see cref="BasicChannelPool"/>,
		/// adding as many clone channels as it's indicated.
		/// </summary>
		/// <param name="channel">
		/// It's the channel to clone and fill up the pool.
		/// The one received as parameter it's added to the pool too.
		/// </param>
		/// <param name="capacity">
		/// It's the maximum quantity of channels that the pool of channels can store.
		/// </param>
		/// <remarks>
		/// When a channel created by the pool is disconnected from its peer,
		/// it's automatically returned to the pool.
		/// </remarks>
		public BasicChannelPool( IChannel channel, int capacity) :
			this( channel, capacity, null) {

		}

		/// <summary>
		/// Initializes a new instance of the class <see cref="BasicChannelPool"/>
		/// without channels.
		/// </summary>
		/// <param name="name">
		/// It's the pool name.
		/// </param>
		/// <remarks>
		/// In order to make this pool of channels useful, it must be necessary
		/// to add channels using the <see cref="Add"/> function.
		/// </remarks>
		public BasicChannelPool( string name) {

			_capacity = int.MaxValue;
			_channels = new Queue();
			_name = name;
		}

		/// <summary>
		/// Initializes a new instance of the class <see cref="BasicChannelPool"/>
		/// without channels.
		/// </summary>
		/// <remarks>
		/// In order to make this pool of channels useful, it must be necessary
		/// to add channels using the <see cref="Add"/> function.
		/// </remarks>
		public BasicChannelPool() : this( null) {

		}
		#endregion

		#region Properties
		/// <summary>
		/// It returns the pool capacity.
		/// </summary>
		public int Capacity {

			get {

				return _capacity;
			}
		}

		/// <summary>
		/// It returns the pool name.
		/// </summary>
		public string Name {

			get {

				return _name;
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
		/// It returns the length of the pool.
		/// </summary>
		public int Length {

			get {

				return _channels.Count;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// It adds a channel to the pool.
		/// </summary>
		/// <param name="channel">
		/// It's the channel to be added to the pool.
		/// </param>
		/// <returns>
		/// A logical value equals to true if the channel was added to the
		/// pool, otherwise false.
		/// </returns>
		/// <remarks>
		/// A channel added with this function, it's not automatically returned 
		/// to the pool when it's disconnected from its peer.
		/// </remarks>
		public bool Add( IChannel channel) {

			if ( channel == null) {
				throw new ArgumentNullException( "channel");
			}

			if ( _channels.Count < _capacity) {

				lock ( _channels) {
					if ( _channels.Count < _capacity) {
						_channels.Enqueue( channel);

						if ( Logger.IsInfoEnabled) {
							Logger.Info( string.Format(
								"Channel added to pool '{0}', {1} channel/s remaining in channel pool.",
								_name, _channels.Count));
						}

					} else {
						return false;
					}
				}

				return true;
			} else {

				return false;
			}
		}

		/// <summary>
		/// Handles the <see cref="IChannel.Disconnected"/> event.
		/// </summary>
		/// <param name="sender">
		/// It's the channel sending the event.
		/// </param>
		/// <param name="e">
		/// The parameters for the event.
		/// </param>
		private void OnChannelDisconnected( object sender, EventArgs e) {

			lock ( _channels) {
				_channels.Enqueue( sender);
			}
		}

		/// <summary>
		/// It removes a channel from the pool.
		/// </summary>
		/// <returns>
		/// The channel which was in the pool, or a null reference whether
		/// there weren't channels in the pool.
		/// </returns>
		public IChannel Remove() {

			lock ( _channels) {

				if ( _channels.Count == 0) {
					return null;
				}

				IChannel channel = _channels.Dequeue() as IChannel;

				if ( Logger.IsInfoEnabled) {
					Logger.Info( string.Format( "{0} channel/s remaining in channel pool '{1}'.",
						_channels.Count, _name));
				}

				return channel;
			}
		}
		#endregion
	}
}
