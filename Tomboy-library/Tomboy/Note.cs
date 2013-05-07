//  Author:
//       jjennings <jaredljennings@gmail.com>
//  
//  Copyright (c) 2012 jjennings
//  Timo Dörr
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
using Tomboy.Tags;

namespace Tomboy
{
	/// <summary>
	/// Contains all pure note data, like the note title and note text.
	/// </summary>
	public class Note
	{
		private readonly string uri;
		private string title;
		private string text;
		private DateTime create_date = DateTime.MinValue;
		private DateTime change_date = DateTime.MinValue;
		private DateTime metadata_change_date = DateTime.MinValue;
		private int x, y;
		private Dictionary<string, Tag> tags = new Dictionary<string, Tag> ();
		/// <summary>
		/// The open the Note on startup.
		/// </summary>
		private bool openOnStartup = false;

		public Note ()
		{
			this.Guid = System.Guid.NewGuid ().ToString ();
		}
		public Note (string uri) : this ()
		{
			this.Guid = uri.Replace ("note://tomboy/", "").Replace (".note","");
		}

		/// <summary>
		/// Gets or sets should the Note open on startup.
		/// </summary>
		/// <value>
		/// The open on startup.
		/// </value>
		public string OpenOnStartup {
			get { 
				return openOnStartup.ToString ();
				}
			set {
				openOnStartup = Boolean.Parse (value);
			}
		}

		/// <summary>
		/// Gets the URI or Note ID. Example: "note://tomboy/44a1a2d6-7ffb-46e0-9b5c-a00260a5bb50"
		/// </summary>
		/// <value>
		/// The URI.
		/// </value>
		public string Uri {
			get {
				return "note://tomboy/" + this.Guid;
			}
		}

		/// <summary>
		/// Gets or sets the Note title.
		/// </summary>
		/// <value>
		/// The title.
		/// </value>
		public string Title {
			get {
				return title;
			}
			set {
				title = value;
			}
		}

		/// <summary>
		/// Gets or sets the Note text. - this is the same as the Note Content
		/// </summary>
		/// <value>
		/// The text.
		/// </value>
		public string Text {
			get {
				return text;
			}
			set {
				text = value;
			}
		}

		/// <summary>
		/// Gets or sets the create date.
		/// </summary>
		/// <value>
		/// The create date.
		/// </value>
		public DateTime CreateDate {
			get {
				return create_date;
			}
			set {
				create_date = value.ToUniversalTime ();
			}
		}

		/// <summary>
		/// Indicates the last time note content data changed.
		/// Does not include tag/notebook changes (see MetadataChangeDate).
		/// </summary>
		public DateTime ChangeDate {
			get {
				return change_date;
			}
			set {
				// we always convert to utc time
				change_date = value.ToUniversalTime ();
				metadata_change_date = change_date;
			}
		}

		/// <summary>
		/// Indicates the last time non-content note data changed.
		/// This currently only applies to tags/notebooks.
		/// </summary>
		public DateTime MetadataChangeDate {
			get {
				return metadata_change_date;
			}
			set {
				metadata_change_date = value.ToUniversalTime ();
			}
		}

		/// <summary>
		/// Gets or sets the x.position of the note on the screen
		/// </summary>
		/// <description>This is used in the old GTK systems</description>
		/// <value>
		/// The x.
		/// </value>
		public int X {
			get {
				return x;
			}
			set {
				x = value;
			}
		}

		/// <summary>
		/// Gets or sets the y.
		/// </summary>
		/// <description>This is used in the old GTK systems</description>
		/// <value>
		/// The y.
		/// </value>
		public int Y {
			get {
				return y;
			}
			set {
				y = value;
			}
		}

		/// <summary>
		/// Gets the tags.assigned to this note
		/// </summary>
		/// <description>Format: <(string)tag name><(Tag)Tag Object></description>
		/// <value>
		/// The tags.
		/// </value>
		public Dictionary<string, Tag> Tags {
			get {
				return tags;
			}
		}

		public void SetPositionExtent (int x, int y, int width, int height)
		{
			if (x < 0 || y < 0)
				return;
			if (width <= 0 || height <= 0)
				return;

			this.x = x;
			this.y = y;
		}

		public int CursorPosition {
			get;
			set;
		}

		public int SelectionBoundPosition {
			get;
			set;
		}

		public int Height {
			get;
			set;
		}		

		public int Width {
			get;
			set;
		}
		public string Guid {
			get;
			set;
		}
		// note that .Equals is required when using i.e. List<T>.Contains ()
		public override bool Equals (object obj)
		{
			// two notes are considered equal if the use the same Guid
			return (obj is Note && Guid == ((Note)obj).Guid);
		}
		public static bool operator == (Note note1, Note note2)
		{
			// while the == operator usually checks for reference equalness by default,
			// we want to compare the Notes by Guid (i.e. for usage in LINQ queries).
			// Reference equalness can always be checked by object.ReferenceEquals (obj1, obj2)
	
			if ((object)note1 == null && (object)note2 == null)
				return true;

			if ((object)note1 == null || (object)note2 == null)
				return false;

			return note1.Equals (note2);
		}
		public static bool operator != (Note note1, Note note2)
		{
			return !(note1 == note2);
		}

		public override int GetHashCode ()
		{
			// the HashCode has not to be unique like the Guid. It is just
			// good to have it evenly distributed, so lookups in a Hashset
			// won't hit a collision, or if hit a collsision, have only few
			// elements in the same bucket so that linear lookup time is
			// minimized

			// get the first 4 chars
			var chars = Guid.Replace ("-","").Substring (0,4);

			return Convert.ToInt32 (chars, 16);
		}
	}
}

