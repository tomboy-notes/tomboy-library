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
using Tomboy.Tags;

namespace Tomboy
{
	/// <summary>
	/// Tomboy Engine.
	/// </summary>
	public class Engine
	{

		#region private fields
		/* holds whatever storage interface will be used */
		private IStorage storage;
		private static TagManager tagMgr = TagManager.Instance;

		/* holds the current notes
		 * This will change as notes are added or removed */
		private Dictionary<string, Note> notes;

		#endregion

		#region Constructors
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
			if (notes == null)
				notes = new Dictionary<string, Note> ();
		}
		#endregion Constructors


		#region delegate

		public delegate void NoteAddedEventHandler (Note note);
		public delegate void NoteRemovedEventHandler (Note note);
		public delegate void NoteUpdatedEventHandler (Note note);

		#endregion delegate

		#region event handlers

		public static event NoteAddedEventHandler NoteAdded;
		public static event NoteAddedEventHandler NoteRemoved;
		public static event NoteAddedEventHandler NoteUpdated;

		#endregion event handlers

		#region public methods

		/// <summary>
		/// Gets the notes.
		/// </summary>
		/// <returns>
		/// Dictionary<string, Note>
		/// </returns>
		public Dictionary<string, Note> GetNotes ()
		{
			Dictionary<string, Note> temp_notes = this.storage.GetNotes ();
			if (this.notes == null)
				this.notes = temp_notes;
			else {
				foreach (string item in temp_notes.Keys) {
					if (!this.notes.ContainsKey (item)) {
						this.notes.Add (item, temp_notes[item]);
						tagMgr.AddTagMap (temp_notes[item]);
					}
				}
			}
			return this.notes;
		}

		/// <summary>
		/// Gets the notes that match the search term.
		/// </summary>
		/// <param name='searchTerm'>
		/// Search term.
		/// </param>
		/// <param name='searchContent'>
		/// Decides whether to search only the titles or to search the content as well.
		/// </param>
		public Dictionary<string, Note> GetNotes (string searchTerm, bool searchContent)
		{
			if (this.notes == null)
				GetNotes ();

			if (!searchContent) {
				return SearchEngine.SearchTitles (searchTerm, this.notes);
			} else {
				return SearchEngine.SearchContent (searchTerm, this.notes);
			}
		}
		
		/// <summary>
		/// Generates a New Note instance
		/// </summary>
		/// <returns>
		/// The note.
		/// </returns>
		public Note NewNote ()
		{
			Note note = NoteCreator.NewNote (GetNotes ().Count);
			notes.Add (note.Uri, note);
			if (NoteAdded != null)
				NoteAdded (note);
			return note;
		}
		
		/// <summary>
		/// Saves the note.
		/// </summary>
		/// <param name='note'>
		/// Note.
		/// </param>
		public void SaveNote (Note note)
		{
			/* jjennings
			 * Setting the save time of the note. I'm not for sure is this is the best method at this point.
			 * it is possible that maybe the UI will want to control this. */
			DateTime saveUtcNow = DateTime.UtcNow;
			note.ChangeDate = saveUtcNow;

			/* Update the dictionary of notes */
			if (notes.ContainsKey (note.Uri))
				notes.Remove (note.Uri);
			notes.Add (note.Uri, note);
			tagMgr.AddTagMap (note);
			/* Save Note to Storage */
			this.storage.SaveNote (note);
			if (NoteUpdated != null)
				NoteUpdated (note);
		}

		/// <summary>
		/// Deletes the note.
		/// </summary>
		/// <param name='note'>
		/// Note.
		/// </param>
		public void DeleteNote (Note note)
		{
			if (notes.ContainsKey (note.Uri))
				notes.Remove (note.Uri);
			tagMgr.RemoveNote (note);
			this.storage.DeleteNote (note);
			if (NoteRemoved != null)
				NoteRemoved (note);
		}
		#endregion

		#region Events

		#endregion
	}
}