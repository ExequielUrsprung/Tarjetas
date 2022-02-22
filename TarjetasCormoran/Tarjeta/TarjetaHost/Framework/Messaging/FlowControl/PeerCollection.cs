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

// TODO: Translate to english.

namespace Trx.Messaging.FlowControl {

	/// <summary>
	/// Implementa una colección de elementos de tipo <see cref="Peer"/>.
	/// </summary>
	/// <remarks>
	/// El nombre del punto de conexión es empleado como clave dentro de
	/// la colección.
	/// </remarks>
	public class PeerCollection : IEnumerable, ICollection {

		private Hashtable _peers;

		#region Constructors
		/// <summary>
		/// Crea una nueva instancia de la colección de puntos de conexión.
		/// </summary>
		public PeerCollection() {

			_peers = new Hashtable( 8);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Retorna o asigna un punto de conexión de la colección de puntos
		/// de conexión.
		/// </summary>
		/// <remarks>
		/// Si el punto de conexión no existe en la colección, un valor nulo
		/// es retornado.
		///
		/// Si existe se está agregando y ya existe es reemplazado.
		/// </remarks>
		public Peer this[string name] {

			get {

				return ( Peer)_peers[name];
			}

			set {

				if ( value == null) {
					return;
				}

				_peers[name] = value;
			}
		}

		/// <summary>
		/// Retorna la cantidad de puntos de conexión incluidos en la colección.
		/// </summary>
		public int Count {

			get {

				return _peers.Count;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Agrega un punto de conexión a la colección.
		/// </summary>
		/// <param name="peer">
		/// Es el punto de conexión a agregar a la colección.
		/// </param>
		/// <remarks>
		/// Si existe es reemplazado.
		/// </remarks>
		public void Add( Peer peer) {

			if ( peer == null) {
				return;
			}

			this[peer.Name] = peer;
		}

		/// <summary>
		/// Elimina el punto de conexión cuyo nombre coincida con el
		/// especificado.
		/// </summary>
		/// <param name="name">
		/// Es el nombre del punto de conexión que se desea eliminar
		/// de la colección.
		/// </param>
		public void Remove( string name) {

			_peers.Remove( name);
		}
		/// <summary>
		/// Elimina todos los puntos de conexión de la colección.
		/// </summary>
		public void Clear() {

			if ( _peers.Count == 0) {
				return;
			}

			_peers.Clear();
		}

		/// <summary>
		/// Indica si la colección contiene un punto de conexión con el nombre
		/// indicado.
		/// </summary>
		/// <param name="name">
		/// Es el nombre del punto de conexión que se desea conocer si existe en
		/// la colección.
		/// </param>
		/// <returns>
		/// <see langref="true"/> si el punto de conexión está contenido en la
		/// colección, <see langref="false"/> en caso contrario.
		/// </returns>
		public bool Contains( string name) {

			return _peers.Contains( name);
		}
		#endregion

		#region Implementation of IEnumerable
		/// <summary>
		/// Devuelve un enumerador de la colección.
		/// </summary>
		/// <returns>
		/// El enumerador sobre la colección.
		/// </returns>
		public IEnumerator GetEnumerator() {

			return new PeersEnumerator( _peers);
		}

		/// <summary>
		/// Implementa el enumerador de la colección.
		/// </summary>
		private class PeersEnumerator : IEnumerator {

			private IEnumerator _peersEnumerator;

			#region Constructors
			/// <summary>
			/// Crea una nueva instancia de la clase <see cref="PeersEnumerator"/>.
			/// </summary>
			/// <param name="peers">
			/// Es la tabla de hash que contiene los campos.
			/// </param>
			public PeersEnumerator( Hashtable peers) {

				_peersEnumerator = peers.GetEnumerator();
			}
			#endregion

			#region Implementation of IEnumerator
			/// <summary>
			/// Reinicia la enumeración.
			/// </summary>
			public void Reset() {
			
				_peersEnumerator.Reset();
			}

			/// <summary>
			/// Se mueve al siguiente elemento en la enumeración.
			/// </summary>
			/// <returns>
			/// Un valor verdadero si logró posicionarse en el siguiente elemento de
			/// la enumeración, un valor igual a falso cuando no existen mas elementos
			/// a enumerar.
			/// </returns>
			public bool MoveNext() {

				return _peersEnumerator.MoveNext();
			}

			/// <summary>
			/// Retorna el elemento actual de la enumeración.
			/// </summary>
			public object Current {

				get {

					return ( ( DictionaryEntry)_peersEnumerator.Current).Value;
				}
			}
			#endregion
		}
		#endregion

		#region ICollection Members
		/// <summary>
		/// Informa si la colección está sincronizada.
		/// </summary>
		public bool IsSynchronized {

			get {

				return false;
			}
		}

		/// <summary>
		/// Copia los elementos de la colección en el array indicado.
		/// </summary>
		/// <param name="array">
		/// Es el array destino donde se copian los elementos.
		/// </param>
		/// <param name="index">
		/// Es el índice en el array desde donde comienza la copia de los
		/// elementos.
		/// </param>
		public void CopyTo( Array array, int index) {

			_peers.CopyTo( array, index);
		}

		/// <summary>
		/// Retorna un objeto que encapsula a la colección sincronizada.
		/// </summary>
		public object SyncRoot {

			get {

				return this;
			}
		}
		#endregion
	}
}