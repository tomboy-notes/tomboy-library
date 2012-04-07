//  Author:
//       jjennings <jaredljennings@gmail.com>
//  
//  Copyright (c) 2012 jjennings
//  Robert Nordan
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace Tomboy
{
	/// <summary>
	/// Tomboy Engine.
	/// </summary>
	public class Engine
	{
		/* holds whatever storage interface will be used */
		private IStorage storage;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Tomboy.Engine"/> class.
		/// </summary>
		/// <description>Must provide whatever storage class that should be used by the Engine</description>
		/// <param name='storage'>
		/// Storage.
		/// </param>
		public Engine (IStorage storage)
		{
			this.storage = storage;
		}
		
		/* holds the current notes
		 * This will change as notes are added or removed */
		private Dictionary<string, Note> notes;
		
		/// <summary>
		/// Gets the notes.
		/// </summary>
		/// <returns>
		/// Dictionary<string, Note>
		/// </returns>
		public Dictionary<string, Note> GetNotes ()
		{
			if (this.notes == null)
				this.notes = this.storage.GetNotes ();
			return this.notes;
		}
	}
}

