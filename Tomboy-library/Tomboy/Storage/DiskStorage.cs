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
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic;

namespace Tomboy
{
	public class DiskStorage : IStorage
	{
		
		private static DiskStorage instance = null;
		private static readonly object lock_ = new object ();
		
		/// <summary>
		/// The path_to_notes.
		/// </summary>
		/// <description>/home/user/.local/share/tomboy</description>
		private static string path_to_notes = null;
		private static string backup_path_notes = null;
		private static string configPath = null;

		protected DiskStorage ()
		{
		}

		public static DiskStorage Instance {
			get {
				lock (lock_) {
					if (instance == null)
						instance = new DiskStorage ();
					return instance;
				}
			}
			set {
				lock (lock_) {
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
			if (!System.IO.Directory.Exists (path_to_notes)) {
				System.IO.Directory.CreateDirectory (path_to_notes);
			}
			// where notes are backed up too.
			backup_path_notes = Path.Combine (path_to_notes, "Backup");
			configPath = Path.Combine (path_to_notes, "config.xml");
		}
		
		public void SaveNote (Note note)
		{
			string file = Path.Combine (path_to_notes, Utils.GetNoteFileNameFromURI (note));
			Console.WriteLine ("Saving Note {0}, FileName: {1}", note.Title, file);
			Write (file, note);
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
		public static void Write (string filename, Note note)
		{
			WriteFile (Path.Combine (path_to_notes, filename), note);
		}
		
		/// <summary>
		/// Writes the file to the actual file system.
		/// </summary>
		/// <param name='write_file'>
		/// Write_file.
		/// </param>
		/// <param name='note'>
		/// Note.
		/// </param>
		private static void WriteFile (string write_file, Note note)
		{
			string tmp_file = write_file + ".tmp";

			using (var xml = XmlWriter.Create (tmp_file, XmlEncoder.DocumentSettings))
				Writer.Write (xml, note);

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
		
		public Dictionary<string,Note> GetNotes ()
		{
			Dictionary<string,Note> notes = new Dictionary<string,Note> ();
			if (path_to_notes == null)
				throw new TomboyException ("No Notes path has been defined");
			
			try {
				/* For anyone wanting to implement another / different backend,
				 * this could be changed in the implementing class to retreive whatever notes
				 */
				string [] files = Directory.GetFiles (path_to_notes, "*.note");
				if (files.Length == 0)
					Console.WriteLine ("No notes found in note folder.");
				
				foreach (string file_path in files) {
					try {
						Note note = Read (file_path, Utils.GetURI (file_path));
						notes.Add (note.Uri, note);
					} catch (System.Xml.XmlException e) {
						Console.WriteLine ("Failed to read Note {0}", file_path); /* so we know what note we cannot read */
						Console.WriteLine (e);
					} catch (System.IO.IOException e) {
						Console.WriteLine (e);
					} catch (System.UnauthorizedAccessException e) {
						Console.WriteLine (e);
					}				
				}
			} catch (System.IO.DirectoryNotFoundException) {
				Console.WriteLine ("Note folder does not yet exist.");
			}
			return notes;
		}

		/// <summary>
		/// Read the specified read_file and uri.
		/// </summary>
		/// <param name='read_file'>
		/// Read_file : Full path of the note file
		/// </param>
		/// <param name='uri'>
		/// URI. : Example: tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51d
		/// </param>
		public static Note Read (string read_file, string uri)
		{
			Console.WriteLine ("Reading Note {0}", read_file);
			return ReadFile (read_file, uri);
		}

		private static Note ReadFile (string read_file, string uri)
		{
			Note note;
			/* Reader.Read should be called by all storage classes.
			 * The Reader is responsible for taking the XML data and turning it into a Note object
			 */
			using (var xml = new XmlTextReader (new StreamReader (read_file, System.Text.Encoding.UTF8)) {Namespaces = false})
				note = Reader.Read (xml, uri);

			return note;
		}

		public void DeleteNote (Note note)
		{
			string file_path = Path.Combine (path_to_notes, Utils.GetNoteFileNameFromURI (note));
			string file_backup_path = Path.Combine (backup_path_notes, Utils.GetNoteFileNameFromURI (note));
		
			if (!Directory.Exists (backup_path_notes))
				Directory.CreateDirectory (backup_path_notes);
			// not for sure why the note would NOT exist. This is from old code. jlj	
			if (File.Exists (file_path)) {
				if (File.Exists (file_backup_path))
					File.Delete (file_backup_path);
				File.Move (file_path, file_backup_path);
			} else {
				File.Move (file_path, file_backup_path);
			}
		}

		public void SetConfigVariable (string key, string value)
		{
			XDocument config;
			if (!File.Exists (configPath)) {
				config = new XDocument ();
				config.Add (new XElement ("root"));
			} else {
				config = XDocument.Load (configPath);
			}

			if (config.Root.Element (key) != null) {
				config.Root.Element (key).Value = value;
			} else {
				config.Root.Add (new XElement (key, value));
			}

			config.Save (configPath);
		}

		public string GetConfigVariable (string key) 
		{
			if (!File.Exists (configPath)) {
				return null;
			}
			XDocument config = XDocument.Load (configPath);

			try {
				return config.Root.Element (key).Value;
			} catch (NullReferenceException) {
				return null;
			}
		}
	}
}