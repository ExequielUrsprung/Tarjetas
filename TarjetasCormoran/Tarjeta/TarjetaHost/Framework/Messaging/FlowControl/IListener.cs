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
	/// Es el delegado del evento <see cref="IListener.ConnectionRequest"/>.
	/// </summary>
	public delegate void ListenerConnectionRequestEventHandler(
		object sender, ListenerConnectionRequestEventArgs e);

	/// <summary>
	/// Es el delegado del evento <see cref="IListener.Connected"/>.
	/// </summary>
	public delegate void ListenerConnectedEventHandler(
		object sender, ListenerConnectedEventArgs e);

	/// <summary>
	/// Es el delegado del evento <see cref="IListener.Error"/>.
	/// </summary>
	public delegate void ListenerErrorEventHandler( object sender, ErrorEventArgs e);

	/// <summary>
	/// Define una interfaz que es capaz de aceptar canales de conexión
	/// desde otros sistemas.
	/// </summary>
	public interface IListener {

		/// <summary>
		/// Se dispara cuando intentan conectarse. Este evento permite
		/// decidir si se acepta la conexión o no.
		/// </summary>
		event ListenerConnectionRequestEventHandler ConnectionRequest;

		/// <summary>
		/// Se dispara cuando se ha creado una conexión.
		/// </summary>
		event ListenerConnectedEventHandler Connected;

		/// <summary>
		/// Se dispara cuando se ha producido un error en procesamiento
		/// interno del listener.
		/// </summary>
		/// <remarks>
		/// Este evento se recibe desde el listener, cuando se produce un error
		/// que provoca que el listener quede deshabilitado, debiendo ser
		/// necesario invocar nuevamente a <see cref="Start"/> para
		/// utilizarlo.
		/// </remarks>
		event ListenerErrorEventHandler Error;

		/// <summary>
		/// Comienza a escuchar requerimientos de conexión.
		/// </summary>
		void Start();

		/// <summary>
		/// Termina de escuchar requerimientos de conexión.
		/// </summary>
		void Stop();

		/// <summary>
		/// Informa si el canal está esperando requerimientos de connexión.
		/// </summary>
		bool Listening {
			get;
		}

		/// <summary>
		/// Rotorna o asigna el pool de canales desde el que el listener
		/// obtiene los canales que asocia a las conexiones.
		/// </summary>
		IChannelPool ChannelPool {
			get;
			set;
		}

		/// <summary>
		/// Informa si existen requerimientos de conexión pendientes
		/// de aceptación.
		/// </summary>
		/// <returns>
		/// Retorna verdadero si existen conexiones pendientes, en
		/// caso contrario retorna falso.
		/// </returns>
		bool Pending();
	}
}