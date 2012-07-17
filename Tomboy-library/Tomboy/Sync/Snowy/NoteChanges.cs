//
//  NoteChanges.cs
//
//  Author:
//       Robert Nordan <rpvn@robpvn.net>
//
//  Copyright (c) 2012 
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
using System.Collections.Generic;

namespace Tomboy.Sync.Snowy
{
	/// <summary>
	///  Holds a dictionary of Note changes and a dictionary of Note deletions. 
	/// Can be passed to the encoder when uploading and retrieved when downloading.
	/// </summary>
	public struct NoteChanges
	{
		public Dictionary<string, Note> ChangedNotes;

		public List<string> DeletedNoteGuids;

		public int SyncRevision;
	}
}

