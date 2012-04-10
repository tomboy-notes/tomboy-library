//  Author:
//       jjennings <jaredljennings@gmail.com>
//  
//  Copyright (c) 2012 jjennings
//  Robert Nordan
//  Alex Graveley (original author)
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
	public class Tag
	{
		public static string SYSTEM_TAG_PREFIX = "system:";
		
		string name;
		string normalized_name;
		bool issystem = false;
		bool isproperty = false;

		// <summary>
		// Used to track which notes are currently tagged by this tag.  The
		// dictionary key is the Note.Uri.
		// </summary>
		Dictionary<string, Note> notes;

		#region Constructors
		public Tag(string tag_name)
		{
			Name = tag_name;
			notes = new Dictionary<string,Note> ();
		}
		#endregion

		#region Public Methods
		// <summary>
		// Associates the specified note with this tag.
		// </summary>
		public void AddNote (Note note)
		{
			if (!notes.ContainsKey (note.Uri)) {
				notes [note.Uri] = note;
			}
		}

		// <summary>
		// Unassociates the specified note with this tag.
		// </summary>
		public void RemoveNote (Note note)
		{
			if (notes.ContainsKey (note.Uri)) {
				notes.Remove (note.Uri);
			}
		}
		#endregion

		#region Properties
		// <summary>
		// The name of the tag.  This is what the user types in as the tag and
		// what's used to show the tag to the user. This includes any 'system:' prefixes
		// </summary>
		public string Name
		{
			get {
					return name;
			}
			set {
				if (value != null) {
					string trimmed_name = (value as string).Trim ();
					if (trimmed_name != String.Empty) {
						name = trimmed_name;
						normalized_name = trimmed_name.ToLower ();
						if(normalized_name.StartsWith(SYSTEM_TAG_PREFIX))
								issystem = true;
						if(value.Split(':').Length >= 3)
								isproperty = true;
					}
				}
			}
		}

		// <summary>
		// Use the string returned here to reference the tag in Dictionaries.
		// </summary>
		public string NormalizedName
		{
			get {
				return normalized_name;
			}
		}
		/// <value>
		/// Is Tag a System Value
		/// </value>
		public bool IsSystem
		{
			get{
				return issystem;
			}
		}
		/// <value>
		/// Is Tag a Property?
		/// </value>
		public bool IsProperty
		{
			get { return isproperty;  }
		}
		// <summary>
		// Returns a list of all the notes that this tag is associated with.
		// </summary>
		public List<Note> Notes
		{
			get {
				return new List<Note> (notes.Values);
			}
		}
		


		// <summary>
		// Returns the number of notes this is currently tagging.
		// </summary>
		public int Popularity
		{
			get {
				return notes.Count;
			}
		}
		#endregion
	}
	
}
