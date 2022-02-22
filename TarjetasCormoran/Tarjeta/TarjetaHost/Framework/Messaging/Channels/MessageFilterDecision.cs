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

namespace Trx.Messaging.Channels {

	/// <summary>
	/// The return result from <see cref="IMessageFilter.Decide"/>
	/// </summary>
	/// <remarks>
	/// The return result from <see cref="IMessageFilter.Decide"/>
	/// </remarks>
	public enum MessageFilterDecision : int {

		/// <summary>
		/// The message must be dropped immediately without 
		/// consulting with the remaining filters, if any, in the chain.
		/// </summary>
		Deny = -1,

		/// <summary>
		/// This filter is neutral with respect to the message. 
		/// The remaining filters, if any, should be consulted for a final decision.
		/// </summary>
		Neutral = 0,

		/// <summary>
		/// The message must processed immediately without 
		/// consulting with the remaining filters, if any, in the chain.
		/// </summary>
		Accept = 1,
	}
}
