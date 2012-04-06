//  Author:
//       jjennings <jaredljennings@gmail.com>
//  
//  Copyright (c) 2012 jjennings
//  Robert Nordan
//  Alex Graveley (original author)
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
	/// Contains all pure note data, like the note title and note text.
	/// </summary>
	public class Note
	{
		readonly string uri;
		string title;
		string text;
		DateTime create_date;
		DateTime change_date;
		DateTime metadata_change_date;
		int x, y;
		Dictionary<string, Tag> tags;

		public Note (string uri)
		{
			this.uri = uri;
			this.text = "";
			tags = new Dictionary<string, Tag> ();

			create_date = DateTime.MinValue;
			change_date = DateTime.MinValue;
			metadata_change_date = DateTime.MinValue;
		}

		public string Uri {
			get {
				return uri;
			}
		}

		public string Title {
			get {
				return title;
			}
			set {
				title = value;
			}
		}

		public string Text {
			get {
				return text;
			}
			set {
				text = value;
			}
		}

		public DateTime CreateDate {
			get {
				return create_date;
			}
			set {
				create_date = value;
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
				change_date = value;
				metadata_change_date = value;
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
				metadata_change_date = value;
			}
		}

		public int X {
			get {
				return x;
			}
			set {
				x = value;
			}
		}

		public int Y {
			get {
				return y;
			}
			set {
				y = value;
			}
		}

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
	}
}

