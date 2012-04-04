using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;

namespace Tomboy
{
	public class DiskStorage : IStorage
	{
		
		private static DiskStorage instance = null;
		private static readonly object lock_ = new object();
		/// <summary>
		/// Current XML version
		/// </summary>
		private const string CURRENT_VERSION = "0.3";

		// NOTE: If this changes from a standard format, make sure to update
		//       XML parsing to have a DateTime.TryParseExact
		public const string DATE_TIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";
		
		/// <summary>
		/// The path_to_notes.
		/// </summary>
		/// <description>/home/user/.local/share/tomboy</description>
		private static string path_to_notes = null;

		protected DiskStorage ()
		{
		}

		public static DiskStorage Instance
		{
			get
			{
				lock (lock_)
				{
					if (instance == null)
						instance = new DiskStorage ();
					return instance;
				}
			}
			set {
				lock (lock_)
				{
					instance = value;
				}
			}
		}		
		
		/// <summary>
		/// Sets the path to where Notes are located
		/// </summary>
		/// <param name='path'>
		/// Path.
		/// </param>
		public void SetPath (string path)
		{
			path_to_notes = path;
		}
		
		/// <summary>
		/// Write the specified write_file and note to storage
		/// </summary>
		/// <param name='write_file'>
		/// Write_file.
		/// </param>
		/// <param name='note'>
		/// Note.
		/// </param>
		public static void Write (string write_file, Note note)
		{
			Instance.WriteFile (write_file, note);
		}

		public virtual void WriteFile (string write_file, Note note)
		{
			string tmp_file = write_file + ".tmp";

			using (var xml = XmlWriter.Create (tmp_file, XmlEncoder.DocumentSettings))
				Write (xml, note);

			if (File.Exists (write_file)) {
				string backup_path = write_file + "~";
				if (File.Exists (backup_path))
					File.Delete (backup_path);

				// Backup the to a ~ file, just in case
				File.Move (write_file, backup_path);

				// Move the temp file to write_file
				File.Move (tmp_file, write_file);

				// Delete the ~ file
				File.Delete (backup_path);
			} else {
				// Move the temp file to write_file
				File.Move (tmp_file, write_file);
			}
		}

		public static void Write (TextWriter writer, Note note)
		{
			Instance.WriteFile (writer, note);
		}

		public void WriteFile (TextWriter writer, Note note)
		{
			using (var xml = XmlWriter.Create (writer, XmlEncoder.DocumentSettings))
				Write (xml, note);
		}

		void Write (XmlWriter xml, Note note)
		{
			xml.WriteStartDocument ();
			xml.WriteStartElement (null, "note", "http://beatniksoftware.com/tomboy");
			xml.WriteAttributeString(null,
			                         "version",
			                         null,
			                         CURRENT_VERSION);
			xml.WriteAttributeString("xmlns",
			                         "link",
			                         null,
			                         "http://beatniksoftware.com/tomboy/link");
			xml.WriteAttributeString("xmlns",
			                         "size",
			                         null,
			                         "http://beatniksoftware.com/tomboy/size");

			xml.WriteStartElement (null, "title", null);
			xml.WriteString (note.Title);
			xml.WriteEndElement ();

			xml.WriteStartElement (null, "text", null);
			xml.WriteAttributeString ("xml", "space", null, "preserve");
			// Insert <note-content> blob...
			xml.WriteRaw (note.Text);
			xml.WriteEndElement ();

			xml.WriteStartElement (null, "last-change-date", null);
			xml.WriteString (
			        XmlConvert.ToString (note.ChangeDate, DATE_TIME_FORMAT));
			xml.WriteEndElement ();

			xml.WriteStartElement (null, "last-metadata-change-date", null);
			xml.WriteString (
			        XmlConvert.ToString (note.MetadataChangeDate, DATE_TIME_FORMAT));
			xml.WriteEndElement ();

			if (note.CreateDate != DateTime.MinValue) {
				xml.WriteStartElement (null, "create-date", null);
				xml.WriteString (
				        XmlConvert.ToString (note.CreateDate, DATE_TIME_FORMAT));
				xml.WriteEndElement ();
			}

			xml.WriteStartElement (null, "x", null);
			xml.WriteString (note.X.ToString ());
			xml.WriteEndElement ();

			xml.WriteStartElement (null, "y", null);
			xml.WriteString (note.Y.ToString ());
			xml.WriteEndElement ();

			if (note.Tags.Count > 0) {
				xml.WriteStartElement (null, "tags", null);
				foreach (Tag tag in note.Tags.Values) {
					xml.WriteStartElement (null, "tag", null);
					xml.WriteString (tag.Name);
					xml.WriteEndElement ();
				}
				xml.WriteEndElement ();
			}
			
			xml.WriteEndElement (); // Note
			xml.WriteEndDocument ();
		}
		
		public List<Note> GetNotes ()
		{
			List<Note> notes = new List<Note> ();	
			if (path_to_notes == null)
				throw new TomboyException ("No Notes path has been defined");
			
			string [] files = Directory.GetFiles (path_to_notes, "*.note");
			foreach (string file_path in files) {
				try {
					notes.Add (Read (file_path, Utils.GetURI (file_path)));
				} catch (System.Xml.XmlException e) {
					Console.WriteLine ("Failed to read Note {0}", file_path); /* so we know what note we cannot read */
					Console.WriteLine (e);
				} catch (System.IO.IOException e) {
					Console.WriteLine (e);
				} catch (System.UnauthorizedAccessException e) {
					Console.WriteLine (e);
				}				
			}
			return notes;
		}
				
		public static Note Read (string read_file, string uri)
		{
			return ReadFile (read_file, uri);
		}

		private static Note ReadFile (string read_file, string uri)
		{
			Note note;
			using (var xml = new XmlTextReader (new StreamReader (read_file, System.Text.Encoding.UTF8)) {Namespaces = false})
				note = Reader.Read (xml, uri);

			return note;
		}		
	}
}