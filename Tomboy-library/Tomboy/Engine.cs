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
		public delegate void TagAddedEventHandler (Tag tag);
		public delegate void TagRemovedEventHandler (string tag_name);

		#region private fields
		/* holds whatever storage interface will be used */
		private IStorage storage;

		private static object tag_locker = new object ();
		private static object bookmark_locker = new object ();
		/* holds the current notes
		 * This will change as notes are added or removed */
		private Dictionary<string, Note> notes;
				
		/* Contains a list of user-defined tags */
		private static Dictionary<string, Tag> tags = new Dictionary<string, Tag> ();

		/* Special tags that are used by the Tomboy system */
		private static Dictionary<string,Tag> internal_tags = new Dictionary<string, Tag> ();

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
		#endregion

		#region public methods

		public void AddNoteToTag (string tag_name, Note note)
		{
			Tag tag = GetOrCreateTag (tag_name);
			note.Tags.Add (tag.NormalizedName, tag);
		}

		// <summary>
		// Return an existing tag for the specified tag name.  If no Tag exists
		// null will be returned.
		// </summary>
		public Tag GetTag (string tag_name)
		{
			if (tag_name == null)
				throw new ArgumentNullException ("TagManager.GetTag () called with a null tag name.");

			string normalized_tag_name = tag_name.Trim ().ToLower ();
			if (normalized_tag_name == String.Empty)
				throw new ArgumentException ("TagManager.GetTag () called with an empty tag name.");

			if (normalized_tag_name.StartsWith(Tag.SYSTEM_TAG_PREFIX) || normalized_tag_name.Split(':').Length > 2){
				lock (tag_locker) {
				if(internal_tags.ContainsKey(normalized_tag_name))
					return internal_tags[normalized_tag_name];
				return null;
				}
			}

			Tag tag = null;
			if (tags.TryGetValue (normalized_tag_name, out tag))
				return tag;

			return null;
		}

		// <summary>
		// Same as GetTag () but will create a new tag if one doesn't already exist.
		// </summary>
		public Tag GetOrCreateTag (string tag_name)
		{
			Tag t = null;
			if (tag_name == null)
				throw new ArgumentNullException ("TagManager.GetOrCreateTag () called with a null tag name.");

			string normalized_tag_name = tag_name.Trim ().ToLower ();
			if (normalized_tag_name == String.Empty)
				throw new ArgumentException ("TagManager.GetOrCreateTag () called with an empty tag name.");

			if (normalized_tag_name.StartsWith (Tag.SYSTEM_TAG_PREFIX) || normalized_tag_name.Split (':').Length > 2) {
				lock (tag_locker) {
					if (internal_tags.ContainsKey (normalized_tag_name))
						return internal_tags [normalized_tag_name];
					else {
						t = new Tag (normalized_tag_name);
						internal_tags.Add (normalized_tag_name, t);
						return t;
					}
				}
			}

			lock (tag_locker) {
				t = GetTag (tag_name);
				if (t == null) {
					t = new Tag (tag_name.Trim ());
					tags.Add (normalized_tag_name, t);
				}
			}
			return t;
		}

		/// <summary>
		/// Same as GetTag(), but for a system tag.
		/// </summary>
		/// <param name="tag_name">
		/// A <see cref="System.String"/>.  This method will handle adding
		/// any needed "system:" or identifier needed.
		/// </param>
		/// <returns>
		/// A <see cref="Tag"/>
		/// </returns>
		public Tag GetSystemTag (string tag_name)
		{
			return GetTag (Tag.SYSTEM_TAG_PREFIX + tag_name);
		}
		
		/// <summary>
		/// Same as <see cref="Tomboy.TagManager.GetSystemTag"/> except that
		/// a new tag will be created if the specified one doesn't exist.
		/// </summary>
		/// <param name="tag_name">
		/// A <see cref="System.String"/>
		/// </param>
		/// <returns>
		/// A <see cref="Tag"/>
		/// </returns>
		public Tag GetOrCreateSystemTag (string tag_name)
		{
			return GetOrCreateTag (Tag.SYSTEM_TAG_PREFIX + tag_name);
		}
		
		// <summary>
		// This will remove the tag from every note that is currently tagged
		// and from the main list of tags.
		// </summary>
		public void RemoveTag (Tag tag)
		{
			if (tag == null)
				throw new ArgumentNullException ("TagManager.RemoveTag () called with a null tag");

			if(tag.IsProperty || tag.IsSystem){
				lock (tag_locker) {
					internal_tags.Remove(tag.NormalizedName);
				}
			}

			lock (tag_locker) {
				if (tags.ContainsKey (tag.NormalizedName)) {
					tags.Remove (tag.NormalizedName);
					Console.WriteLine ("Tag Removed: {0}", tag.NormalizedName);
				}
			}
		}

		#region Properties
		public Dictionary<string, Tag> Tags
		{
			get {
				return tags;
			}
		}
		
		
		/// <value>
		/// All tags (including system and property tags)
		/// </value>
		public List<Tag> AllTags
		{
			get {
				List<Tag> temp = new List<Tag>();
				
				// Add in the system tags first
				temp.AddRange (internal_tags.Values);
				
				// Now all the other tags
				temp.AddRange (tags.Values);
				
				return temp;
			}
		}
		
		#endregion

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
					if (!this.notes.ContainsKey (item))
						this.notes.Add (item, temp_notes[item]);
				}
			}
			return this.notes;
		}
		
		/// <summary>
		/// Generates a New Note instance
		/// </summary>
		/// <returns>
		/// The note.
		/// </returns>
		public Note NewNote ()
		{
			Note note = NoteCreator.NewNote ();
			notes.Add (note.Uri, note);
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
			/* Save Note to Storage */
			this.storage.SaveNote (note);
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
			this.storage.DeleteNote (note);
		}
		#endregion

		#region Events
		public static event TagAddedEventHandler TagAdded;
		public static event TagRemovedEventHandler TagRemoved;
		#endregion
	}
}

