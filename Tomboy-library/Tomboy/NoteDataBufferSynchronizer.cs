using System;

namespace Tomboy
{
	/// <summary>
	/// This class wraps a NoteData instance. Most method calls are
	/// forwarded to the wrapped instance, but there is special behaviour
	/// for the Text attribute. This class takes care that this attribute
	/// is synchronized with the contents of a NoteBuffer instance.
	/// </summary>
	public class NoteDataBufferSynchronizer
	{

		readonly Note data;
		NoteBuffer buffer;

		public NoteDataBufferSynchronizer (Note data)
		{
			this.data = data;
		}

		public Note GetDataSynchronized ()
		{
			// Assert that Data.Text returns the current
			// text from the text buffer.
			SynchronizeText ();
			return data;
		}

		public Note Data {
			get {
				return data;
			}
		}

		public NoteBuffer Buffer {
			get {
				return buffer;
			}
			set {
				buffer = value;
				buffer.Changed += OnBufferChanged;
				buffer.TagApplied += BufferTagApplied;
				buffer.TagRemoved += BufferTagRemoved;

				SynchronizeBuffer ();

				InvalidateText ();
			}
		}

		//Text is actually an Xml formatted string
		public string Text {
			get {
				SynchronizeText ();
				return data.Text;
			}
			set {
				data.Text = value;
				SynchronizeBuffer ();
			}
		}

		// Custom Methods

		void InvalidateText ()
		{
			data.Text = "";
		}

		bool TextInvalid ()
		{
			return data.Text == "";
		}

		void SynchronizeText ()
		{
			if (TextInvalid () && buffer != null) {
				data.Text = NoteBufferArchiver.Serialize (buffer);
			}
		}

		void SynchronizeBuffer ()
		{
			if (!TextInvalid () && buffer != null) {
				// Don't create Undo actions during load
				buffer.Undoer.FreezeUndo ();

				buffer.Clear ();

				// Load the stored xml text
				NoteBufferArchiver.Deserialize (buffer,
				                                buffer.StartIter,
				                                data.Text);
				buffer.Modified = false;

				Gtk.TextIter cursor;
				if (data.CursorPosition != 0) {
					// Move cursor to last-saved position
					cursor = buffer.GetIterAtOffset (data.CursorPosition);
				} else {
					// Avoid title line
					cursor = buffer.GetIterAtLine (2);
				}
				buffer.PlaceCursor (cursor);
				
				if (data.SelectionBoundPosition >= 0) {
					// Move selection bound to last-saved position
					Gtk.TextIter selection_bound;
					selection_bound = buffer.GetIterAtOffset (data.SelectionBoundPosition);
					buffer.MoveMark (buffer.SelectionBound.Name, selection_bound);
				}

				// New events should create Undo actions
				buffer.Undoer.ThawUndo ();
			}
		}

		// Callbacks

		void OnBufferChanged (object sender, EventArgs args)
		{
			InvalidateText ();
		}

		void BufferTagApplied (object sender, Gtk.TagAppliedArgs args)
		{
			if (NoteTagTable.TagIsSerializable (args.Tag)) {
				InvalidateText ();
			}
		}

		void BufferTagRemoved (object sender, Gtk.TagRemovedArgs args)
		{
			if (NoteTagTable.TagIsSerializable (args.Tag)) {
				InvalidateText ();
			}
		}
	}
}