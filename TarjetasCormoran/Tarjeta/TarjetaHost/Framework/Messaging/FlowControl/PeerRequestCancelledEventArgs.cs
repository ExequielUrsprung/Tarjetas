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
	/// Esta clase define los argumentos de los eventos que notifican
	/// que un requerimiento ha sido completado.
	/// </summary>
	public class PeerRequestCancelledEventArgs : EventArgs {

		private PeerRequest _request;

		#region Constructors
		/// <summary>
		/// Crea e inicializa una nueva clase del tipo <see cref="PeerRequestCancelledEventArgs"/>.
		/// </summary>
		/// <param name="request">
		/// Es el requerimiento que se ha completado.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// request contiene una referencia nula.
		/// </exception>
		public PeerRequestCancelledEventArgs( PeerRequest request) {

			if ( request == null) {
				throw new ArgumentNullException( "request");
			}

			_request = request;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Retorna el requerimiento que se ha completado.
		/// </summary>
		public PeerRequest Request {

			get {

				return _request;
			}
		}
		#endregion
	}
}
