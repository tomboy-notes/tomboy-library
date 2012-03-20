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
		int cursor_pos, selection_bound_pos;
		int width, height;
		int x, y;
		bool open_on_startup;
		Dictionary<string, Tag> tags;
		const int noPosition = -1;

		public Note (string uri)
		{
			this.uri = uri;
			this.text = "";
			x = noPosition;
			y = noPosition;
			selection_bound_pos = noPosition;

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
		

		// FIXME: the next six attributes don't belong here (the data
		// model), but belong into the view; for now they are kept here
		// for backwards compatibility

		public int CursorPosition {
			get {
				return cursor_pos;
			}
			set {
				cursor_pos = value;
			}
		}
		
		public int SelectionBoundPosition {
			get {
				return selection_bound_pos;
			}
			set {
				selection_bound_pos = value;
			}
		}

		public int Width {
			get {
				return width;
			}
			set {
				width = value;
			}
		}

		public int Height {
			get {
				return height;
			}
			set {
				height = value;
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

		public bool IsOpenOnStartup {
			get {
				return open_on_startup;
			}
			set {
				open_on_startup = value;
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
			this.width = width;
			this.height = height;
		}

		public bool HasPosition ()
		{
			return x != noPosition && y != noPosition;
		}

		public bool HasExtent ()
		{
			return width != 0 && height != 0;
		}
	}
}

