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
using System.Linq;

namespace Tomboy
{
	/// <summary>
	/// Tomboy Engine.
	/// </summary>
	public class Engine
	{

		#region fields
		/* holds whatever storage interface will be used */
		private IStorage storage;
		private static TagManager tagMgr = TagManager.Instance;

		/* holds the current notes
		 * This will change as notes are added or removed */
		public Dictionary<string, Note> Notes { get; private set; }

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
			if (storage == null)
				throw new ArgumentNullException ("storage");
			this.storage = storage;
			this.Notes = new Dictionary<string, Note> ();
			var temp_notes = this.storage.GetNotes ();
			foreach (string item in temp_notes.Keys) {
				this.Notes.Add (item, temp_notes [item]);
				tagMgr.AddTagMap (temp_notes [item]);
			}
		}
		#endregion

		#region delegate

		public delegate void NoteAddedEventHandler (Note note);
		public delegate void NoteRemovedEventHandler (Note note);
		public delegate void NoteUpdatedEventHandler (Note note);

		#endregion

		#region event handlers

		public event NoteAddedEventHandler NoteAdded;
		public event NoteAddedEventHandler NoteRemoved;
		public event NoteAddedEventHandler NoteUpdated;

		#endregion

		#region public methods

		/// <summary>
		/// Gets the tags, which is built from the collection of Notes
		/// </summary>
		/// <returns>
		/// The tags.
		/// </returns>
		public List<Tag> GetTags ()
		{
			return tagMgr.AllTags;
		}

		/// <summary>
		/// Gets the notes.
		/// </summary>
		/// <description>
		/// Dictionary looks like <note://tomboy/44a1a2d6-7ffb-46e0-9b5c-a00260a5bb50, Note>
		/// </description>
		/// <returns>
		/// Dictionary<string, Note>
		/// </returns>
		[Obsolete("Use the property 'Notes' on the Engine instead")]
		public Dictionary<string, Note> GetNotes ()
		{
			return this.Notes;
		}

		/// <summary>
		/// Gets the specified Note based on Title search
		/// </summary>
		/// <returns>
		/// The note.
		/// </returns>
		/// <param name='title'>
		/// Title.
		/// </param>
		public Note GetNote (string title)
		{
			foreach (KeyValuePair<string, Note> n in this.Notes){
				if (String.Compare (n.Value.Title, title, StringComparison.InvariantCultureIgnoreCase) == 0)
				    return n.Value;
			}
			return null;
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
			if (!searchContent) {
				return SearchEngine.SearchTitles (searchTerm.ToLowerInvariant (), this.Notes);
			} else {
				return SearchEngine.SearchContent (searchTerm.ToLowerInvariant (), this.Notes);
			}
		}
			
		public Dictionary<string, Note> GetNotesForNotebook(string notebook)
		{
			if (notebook.Equals ("All Notebooks", StringComparison.Ordinal)) {
				return this.Notes;
			} else {
				var results = this.Notes.Values.Where(
					n => n.Notebook != null && n.Notebook.Equals (notebook, StringComparison.Ordinal)
				).ToDictionary(n => n.Guid, n => n);
				return results;
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
			// We maybe need a way to detect if we've tried loading the notes yet.
			// basically it's possible to call NewNote () and not have loaded the notes database yet.
			// this would then generate a 0 note list.
			Note note = NoteCreator.NewNote (Notes.Count);
			Notes.Add (note.Uri, note);
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
		/// <param name='update_dates'>
		/// By default true and can be omited. If set to false, do not update the ChangeDate
		/// and MetadataChangeDate fields. Usefull for pure data storage, and when syncing.
		/// </param>
		public void SaveNote (Note note, bool update_dates = true)
		{
			if (update_dates) {
				DateTime saveUtcNow = DateTime.UtcNow;
				note.ChangeDate = saveUtcNow;
			}

			/* Update the dictionary of notes */
			if (Notes.ContainsKey (note.Uri))
				Notes.Remove (note.Uri);
			Notes.Add (note.Uri, note);
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
			if (Notes.ContainsKey (note.Uri))
				Notes.Remove (note.Uri);
			tagMgr.RemoveNote (note);
			this.storage.DeleteNote (note);
			if (NoteRemoved != null)
				NoteRemoved (note);
		}

		/// <summary>
		/// Adds existing notes, to be used by for example sync agents
		/// </summary>
		/// <param name='note'>
		/// Note.
		/// </param>
		public void AddAndSaveNotes (Dictionary<string, Note> newNotes)
		{
			foreach (string guid in newNotes.Keys) {
				SaveNote (newNotes[guid]);
			}
		}

		public void SetConfigVariable (string key, string value) 
		{
			storage.SetConfigVariable (key, value);
		}

		public string GetConfigVariable (string key)
		{
			return storage.GetConfigVariable (key);
		}

		#endregion

		#region Events

		#endregion
	}
}