//  Author:
//       jjennings <jaredljennings@gmail.com>
//  
//  Copyright (c) 2012 jjennings
//  Robert Nordan
// 
// This library is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation; either version 2.1 of the
// License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using System.Collections.Generic;

namespace Tomboy
{
	/// <summary>
	/// Handles interaction with backend system is used to store notes.
	/// </summary>
	public interface IStorage
	{
		/// <summary>
		/// Gets notes.
		/// </summary>
		/// <returns>
		/// A Generic list of Notes
		/// </returns>
		Dictionary<string, Note> GetNotes ();
		
		/// <summary>
		/// Sets the path to where Notes are located
		/// </summary>
		/// <param name='path'>
		/// Path.
		/// </param>
		void SetPath (string path);
		
		/// <summary>
		/// Saves the note.
		/// </summary>
		/// <param name='note'>
		/// Note.
		/// </param>
		void SaveNote (Note note);

		/// <summary>
		/// Deletes the note.
		/// </summary>
		/// <param name='note'>
		/// Note.
		/// </param>
		void DeleteNote (Note note);

		/// <summary>
		/// Stores arbitrary config variables.
		/// </summary>
		void SetConfigVariable (string key, string value);

		/// <summary>
		/// Retrieves arbitrary config variables, or null if unset.
		/// </summary>
		string GetConfigVariable (string key);
	}
}

