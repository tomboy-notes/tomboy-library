// 
//  ISyncAgent.cs
//  
//  Author:
//       Robert Nordan <rpvn@robpvn.net>
// 
//  Copyright (c) 2012 Robert Nordan
// 
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
// 
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
using System;
namespace Tomboy.Sync
{
	/// <summary>
	/// Tomboy Engine.
	/// </summary>
	public interface ISyncAgent
	{
		/// <summary>
		/// Performs the sync operation, with two-way merging.
		/// </summary>
		/// <param name='parent'>
		/// The engine that allows access to notes.
		/// </param>
		void PerformSync (); //TODO: parent might as well also go in the repsective constructors?

		/// <summary>
		/// Performs a one-way sync, overwriting all server notes with local notes.
		/// </summary>
		void CopyFromLocal ();

		/// <summary>
		/// Performs a one-way sync, overwriting all local notes with notes from the server.
		/// </summary>
		void CopyFromRemote ();
	}

}

