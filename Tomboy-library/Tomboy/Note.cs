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

