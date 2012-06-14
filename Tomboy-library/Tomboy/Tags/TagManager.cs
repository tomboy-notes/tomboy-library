// 
//  TagManager.cs
//  
//  Author:
//       Jared L Jennings <jaredljennings@gmail.com>
// 
//  Copyright (c) 2012 Jared L Jennings
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
using Tomboy.Tags;

namespace Tomboy.Tags
{
	/// <summary>
	/// Tag manager manages all tags
	/// </summary>
	public class TagManager
	{
		#region Constructors
		public TagManager ()
		{
		}
		#endregion

		#region private fields
		/// <summary>
		/// The tag_to_notes_mapping holds a list of ALL tags and any notes that may be mapped to that tag.
		/// </summary>
		private Dictionary<string, List<Note>> tag_to_notes_mapping = new Dictionary<string, List<Note>> ();

		/// <summary>
		/// The master_tags_list.
		/// </summary>
		private Dictionary<string, Tag> tag_list = new Dictionary<string, Tag> ();

		/// <summary>
		/// Special tags that are used by the Tomboy system
		/// </summary>
		/// <description>
		/// The mapping is a Tag to List<Notes> which is a one to many mapping of Tag to Notes.
		/// </description>
		private static Dictionary<string, List<Note>> internal_tag_to_notes_mapping = new Dictionary<string, List<Note>> ();
		/// <summary>
		/// The master_internal_tags_list.
		/// </summary>

		/// <summary>
		/// The internal_tag_list of Tags. This list does not contain what Notes are part fo the Tag, but just a listing of all Internal Tags
		/// </summary>
		private Dictionary<string, Tag> internal_tag_list = new Dictionary<string, Tag> ();

		private static object tag_locker = new object ();
		#endregion

		#region delegate
		public delegate void TagAddedEventHandler (Tag tag);
		public delegate void TagRemovedEventHandler (string tag_name);
		#endregion

		#region public methods

		/// <summary>
		/// Gets all tags except for internal tags
		/// </summary>
		/// <returns>
		/// The tags.
		/// </returns>
		public Dictionary<string, Tag> GetTags ()
		{
			return tag_list;
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
				if(internal_tag_list.ContainsKey(normalized_tag_name))
					return internal_tag_list[normalized_tag_name];
				return null;
				}
			}

			Tag tag = null;
			if (tag_list.TryGetValue (normalized_tag_name, out tag))
				return tag;

			return null;
		}
		// <summary>
		// Same as GetTag () but will create a new tag if one doesn't already exist.
		// </summary>
		public Tag GetOrCreateTag (string tag_name)
		{
			return GetOrCreateTag (tag_name, null);
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
					internal_tag_list.Remove (tag.NormalizedName);
					internal_tag_to_notes_mapping.Remove (tag.NormalizedName);
				}
			}

			lock (tag_locker) {
				if (tag_list.ContainsKey (tag.NormalizedName)) {
					tag_list.Remove (tag.NormalizedName);
					tag_to_notes_mapping.Remove (tag.NormalizedName);
					Console.WriteLine ("Tag Removed: {0}", tag.NormalizedName);
				}
			}
		}

		/// <summary>
		/// Adds tags from the passed Note.Tags to the Tag_to_Note mapping
		/// </summary>
		/// <param name='note'>
		/// Note.
		/// </param>
		public void AddTagMap (Note note)
		{
			foreach (string tag in note.Tags.Keys) {
				AddTagMap (tag, note);
			}
		}

		/// <summary>
		/// Gets the notes by tag.
		/// </summary>
		/// <returns>
		/// Returns any notes that are contained by the specified tag_name
		/// </returns>
		/// <param name='tag_name'>
		/// Tag_name.
		/// </param>
		public List<Note> GetNotesByTag (string tag_name)
		{
			if (tag_to_notes_mapping.ContainsKey (GetNormalizedName (tag_name)))
				return tag_to_notes_mapping[GetNormalizedName (tag_name)];
			else
				throw new ArgumentException ("No tag by that name exists");
		}

		#endregion public methods

		#region private methods

		/// <summary>
		/// Gets the name of the normalized. <br>
		/// Returns NULL if param is null or empth.
		/// </summary>
		/// <returns>
		/// The normalized name.
		/// </returns>
		/// <param name='tag_name'>
		/// Tag_name.
		/// </param>
		private string GetNormalizedName (string tag_name)
		{
			if (!String.IsNullOrEmpty (tag_name))
				return tag_name.Trim ().ToLower ();
			else
				return null;
		}

		private void AddTagMap (string tag_name, Note note)
		{
			Tag t = null;
			if (tag_name == null)
				throw new ArgumentNullException ("TagManager.GetOrCreateTag () called with a null tag name.");

			if (note == null)
				throw new ArgumentNullException ("TagManager.AddTagMap () called with null Note.");

			string normalized_tag_name = tag_name.Trim ().ToLower ();
			if (normalized_tag_name == String.Empty)
				throw new ArgumentException ("TagManager.GetOrCreateTag () called with an empty tag name.");

			if (normalized_tag_name.StartsWith (Tag.SYSTEM_TAG_PREFIX) || normalized_tag_name.Split (':').Length > 2) {
				lock (tag_locker) {
					if (!internal_tag_list.ContainsKey (normalized_tag_name)) {
						t = new Tag (normalized_tag_name);
						internal_tag_list.Add (normalized_tag_name, t);
						AddNoteToInternalTagMapping (normalized_tag_name, note);
					}
				}
			}

			// otherwise the tag is normal and we need to add it to the normal tag list.
			lock (tag_locker) {
				if (!tag_list.ContainsKey (normalized_tag_name)) {
					t = new Tag (tag_name.Trim ());
					tag_list.Add (normalized_tag_name, t);
					AddNoteToTagMapping (normalized_tag_name, note);
				}
			}
		}

		/// <summary>
		/// Adds the note to tag mapping.
		/// </summary>
		/// <param name='notes_list'>
		/// Notes_list. Either the internal list or the public tag list
		/// </param>
		/// <param name='note'>
		/// Note.
		/// </param>
		private void AddNoteToTagMapping (string tag_name, Note note)
		{
			if (!tag_to_notes_mapping.ContainsKey (tag_name)) {
				tag_to_notes_mapping.Add (tag_name, new List<Note> {note});
			} else {
				foreach (Note item in tag_to_notes_mapping[tag_name]) {
					if (!item.Equals (note))
						tag_to_notes_mapping[tag_name].Add (note);
				}
			}
		}

		/// <summary>
		/// Adds the note to internal tag mapping.
		/// </summary>
		/// <param name='notes_list'>
		/// Notes_list.
		/// </param>
		/// <param name='note'>
		/// Note.
		/// </param>
		private void AddNoteToInternalTagMapping (string tag_name, Note note)
		{
			if (!internal_tag_to_notes_mapping.ContainsKey (tag_name)) {
				internal_tag_to_notes_mapping.Add (tag_name, new List<Note> {note});
			} else {
				foreach (Note item in tag_to_notes_mapping[tag_name]) {
					if (!item.Equals (note))
						internal_tag_to_notes_mapping[tag_name].Add (note);
				}
			}
		}

		private Tag GetOrCreateTag (string tag_name, Note note)
		{
			Tag t = null;
			if (tag_name == null)
				throw new ArgumentNullException ("TagManager.GetOrCreateTag () called with a null tag name.");

			string normalized_tag_name = tag_name.Trim ().ToLower ();
			if (normalized_tag_name == String.Empty)
				throw new ArgumentException ("TagManager.GetOrCreateTag () called with an empty tag name.");

			if (normalized_tag_name.StartsWith (Tag.SYSTEM_TAG_PREFIX) || normalized_tag_name.Split (':').Length > 2) {
				lock (tag_locker) {
					if (internal_tag_list.ContainsKey (normalized_tag_name)) {
						if (note != null)
							AddNoteToInternalTagMapping (normalized_tag_name, note);
						return internal_tag_list [normalized_tag_name];
					}
					else {
						t = new Tag (normalized_tag_name);
						internal_tag_list.Add (normalized_tag_name, t);
						if (note != null)
							AddNoteToInternalTagMapping (normalized_tag_name, note);
						return t;
					}
				}
			}

			lock (tag_locker) {
				t = GetTag (tag_name);
				if (t == null) {
					t = new Tag (tag_name.Trim ());
					tag_list.Add (normalized_tag_name, t);
				}
									
				if (note != null)
					AddNoteToTagMapping (normalized_tag_name, note);
			}
			return t;
		}

		#endregion private methods
		#region Properties
		public Dictionary<string, Tag> Tags
		{
			get {
				return tag_list;
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
				temp.AddRange (internal_tag_list.Values);
				
				// Now all the other tags
				temp.AddRange (tag_list.Values);
				
				return temp;
			}
		}

		#endregion Properties

		#region event handlers
		public static event TagAddedEventHandler TagAdded;
		public static event TagRemovedEventHandler TagRemoved;
		#endregion event handlers
	}
}