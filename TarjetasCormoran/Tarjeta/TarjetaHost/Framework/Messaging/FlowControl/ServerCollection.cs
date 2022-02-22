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
	/// Implementa una colección de elementos de tipo <see cref="Server"/>.
	/// </summary>
	/// <remarks>
	/// El nombre del servidor es empleado como clave dentro de la colección.
	/// </remarks>
	public class ServerCollection : IEnumerable, ICollection {

		private Hashtable _servers;

		#region Constructors
		/// <summary>
		/// Crea una nueva instancia de la colección de servidores.
		/// </summary>
		public ServerCollection() {

			_servers = new Hashtable( 8);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Retorna o asigna un servidor de la colección de servidores.
		/// </summary>
		/// <remarks>
		/// Si el servidor no existe en la colección, un valor nulo es
		/// retornado.
		///
		/// Si existe se está agregando y ya existe es reemplazado.
		/// </remarks>
		public Server this[string name] {

			get {

				return ( Server)_servers[name];
			}

			set {

				if ( value == null) {
					return;
				}

				_servers[name] = value;
			}
		}

		/// <summary>
		/// Retorna la cantidad de servidores incluidos en la colección.
		/// </summary>
		public int Count {

			get {

				return _servers.Count;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Agrega un servidor a la colección.
		/// </summary>
		/// <param name="server">
		/// Es el servidor a agregar a la colección.
		/// </param>
		/// <remarks>
		/// Si existe es reemplazado.
		/// </remarks>
		public void Add( Server server) {

			if ( server == null) {
				return;
			}

			this[server.Name] = server;
		}

		/// <summary>
		/// Elimina el servidor cuyo nombre coincida con el especificado.
		/// </summary>
		/// <param name="name">
		/// Es el nombre del servidor que se desea eliminar de la colección.
		/// </param>
		public void Remove( string name) {

			_servers.Remove( name);
		}
		/// <summary>
		/// Elimina todos los servidores de la colección.
		/// </summary>
		public void Clear() {

			if ( _servers.Count == 0) {
				return;
			}

			_servers.Clear();
		}

		/// <summary>
		/// Indica si la colección contiene un servidor con el nombre
		/// indicado.
		/// </summary>
		/// <param name="name">
		/// Es el nombre del servidor que se desea conocer si existe en
		/// la colección.
		/// </param>
		/// <returns>
		/// <see langref="true"/> si el servidor está contenido en la colección,
		/// <see langref="false"/> en caso contrario.
		/// </returns>
		public bool Contains( string name) {

			return _servers.Contains( name);
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

			return new ServersEnumerator( _servers);
		}

		/// <summary>
		/// Implementa el enumerador de la colección.
		/// </summary>
		private class ServersEnumerator : IEnumerator {

			private IEnumerator _serversEnumerator;

			#region Constructors
			/// <summary>
			/// Crea una nueva instancia de la clase <see cref="ServersEnumerator"/>.
			/// </summary>
			/// <param name="servers">
			/// Es la tabla de hash que contiene los campos.
			/// </param>
			public ServersEnumerator( Hashtable servers) {

				_serversEnumerator = servers.GetEnumerator();
			}
			#endregion

			#region Implementation of IEnumerator
			/// <summary>
			/// Reinicia la enumeración.
			/// </summary>
			public void Reset() {
			
				_serversEnumerator.Reset();
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

				return _serversEnumerator.MoveNext();
			}

			/// <summary>
			/// Retorna el elemento actual de la enumeración.
			/// </summary>
			public object Current {

				get {

					return ( ( DictionaryEntry)_serversEnumerator.Current).Value;
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

			_servers.CopyTo( array, index);
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