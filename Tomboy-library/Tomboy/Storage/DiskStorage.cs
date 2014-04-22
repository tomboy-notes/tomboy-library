//  Author:
//       jjennings <jaredljennings@gmail.com>
//  
//  Copyright (c) 2012 jjennings
//  Robert Nordan
//  Copyright (c) 2014 Timo Dörr
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
using Tomboy.Xml;

namespace Tomboy
{
	/// <summary>
	/// Disk storage. As with every IStorage, only responsible for storing the notes. Higher level function such as
	/// updating a note's change date upon save or seamless en- & decryption is NOT performed - that is
	/// the <see cref="Engine"/> responsibility that wraps the IStorage.
	/// </summary>
	public class DiskStorage : IStorage
	{
		
		/// <summary>
		/// The path_to_notes.
		/// </summary>
		/// <example>/home/user/.local/share/tomboy</example>
		public string PathToNotes { get; private set; }
		public string BackupPath { get; private set; }
		public string ConfigPath { get; private set; }
		public ILogger Logger { get; set; }

		[Obsolete ("use constructor with path argument")]
		public DiskStorage ()
		{
			this.Logger = new DummyLogger ();
		}
		public DiskStorage (string path) : this ()
		{
			this.SetPath (path);
		}

		/// <summary>
		/// Sets the path to where Notes are located
		/// </summary>
		/// <param name='path'>
		/// Path.
		/// </param>
		[Obsolete ("use constructor with path argument")]
		// this will be private in the future. It makes no sense to change the path later on.
		// Currently changing the path a second time will result in undefined behavior.
		public void SetPath (string path)
		{
			PathToNotes = path;
			if (!Directory.Exists (PathToNotes)) {
				Directory.CreateDirectory (PathToNotes);
			}
			// where notes are backed up too.
			BackupPath = Path.Combine (PathToNotes, "Backup");
			ConfigPath = Path.Combine (PathToNotes, "config.xml");
		}

		public void SetBackupPath (string path)
		{
			BackupPath = path;
		}
		
		public void SaveNote (Note note)
		{
			string file = Utils.GetNoteFileNameFromURI (note);
			Logger.Info ("Saving Note {0}, FileName: {1}", note.Title, file);
			Write (file, note);
		}
		
		/// <summary>
		/// Write the specified write_file and note to storage
		/// </summary>
		/// <param name='filename'>
		/// Write_file.
		/// </param>
		/// <param name='note'>
		/// Note.
		/// </param>
		public void Write (string filename, Note note)
		{
			WriteFile (Path.Combine (PathToNotes, filename), note);
		}
		
		/// <summary>
		/// Writes the file to the actual file system.
		/// </summary>
		/// <param name = "file"></param>
		/// <param name='note'>
		/// Note.
		/// </param>
		/// <param name = "note"></param>
		private static void WriteFile (string file, Note note)
		{
			string tmp_file = file + ".tmp";

			using (var fs = File.OpenWrite (tmp_file))
				XmlNoteWriter.Write (note, fs);

			if (File.Exists (file)) {
				string backup_path = file + "~";
				if (File.Exists (backup_path))
					File.Delete (backup_path);

				// Backup the to a ~ file, just in case
				File.Move (file, backup_path);

				// Move the temp file to write_file
				File.Move (tmp_file, file);

				// Delete the ~ file
				File.Delete (backup_path);
			} else {
				// Move the temp file to write_file
				File.Move (tmp_file, file);
			}
		}
		
		public Dictionary<string,Note> GetNotes ()
		{
			Dictionary<string,Note> notes = new Dictionary<string,Note> ();
			if (PathToNotes == null)
				throw new TomboyException ("No Notes path has been defined");
			
			try {
				/* For anyone wanting to implement another / different backend,
				 * this could be changed in the implementing class to retreive whatever notes
				 */
				string [] files = Directory.GetFiles (PathToNotes, "*.note");
				if (files.Length == 0)
					Logger.Warn ("No notes found in note folder.");
				
				foreach (string file_path in files) {
					try {
						Note note = Read (file_path, Utils.GetURI (file_path));
						if (note != null)
							notes.Add (note.Uri, note);
					} catch (XmlException e) {
						Logger.Error ("Failed to read Note {0}", file_path); /* so we know what note we cannot read */
						Logger.Error (e);
					} catch (IOException e) {
						Logger.Error (e);
					} catch (UnauthorizedAccessException e) {
						Logger.Error (e);
					}				
				}
			} catch (DirectoryNotFoundException) {
				Logger.Warn ("Note folder does not yet exist.");
			}
			return notes;
		}

		/// <summary>
		/// Read the specified read_file and uri.
        /// </summary>
        /// <param name = "readFile">
        /// Full path of the note file
        /// </param>
		/// <param name='uri'>
		/// URI. : Example: tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51d
		/// </param>
        /// <param name = "uri"></param>
		public static Note Read (string readFile, string uri)
		{
//			Logger.Info ("Reading Note {0}", readFile);
			return ReadFile (readFile, uri);
		}

		private static Note ReadFile (string readFile, string uri)
		{
			Note note;
			/* Reader.Read should be called by all storage classes.
			 * The Reader is responsible for taking the XML data and turning it into a Note object
			 */
			using (var fs = File.OpenRead (readFile)) {
				note = XmlNoteReader.Read (fs, uri);
			}
			
			return note;
		}

		public void DeleteNote (Note note)
		{
			string file_path = Path.Combine (PathToNotes, Utils.GetNoteFileNameFromURI (note));
			string file_backup_path = Path.Combine (BackupPath, Utils.GetNoteFileNameFromURI (note));
		
			if (!Directory.Exists (BackupPath))
				Directory.CreateDirectory (BackupPath);
			// not for sure why the note would NOT exist. This is from old code. jlj	
			if (File.Exists (file_path)) {
				if (File.Exists (file_backup_path))
					File.Delete (file_backup_path);
				File.Move (file_path, file_backup_path);
			} //TODO: what if there isn't a file to delete. We should at least log this in DEBUG
		}

		public void SetConfigVariable (string key, string value)
		{
			XDocument config;
			if (!File.Exists (ConfigPath)) {
				config = new XDocument ();
				config.Add (new XElement ("root"));
			} else {
				config = XDocument.Load (ConfigPath);
			}

			if (config.Root.Element (key) != null) {
				config.Root.Element (key).Value = value;
			} else {
				config.Root.Add (new XElement (key, value));
			}

			config.Save (ConfigPath);
		}

		public string GetConfigVariable (string key) 
		{
			if (!File.Exists (ConfigPath)) {
				throw new TomboyException ("Config file does not exist");
			}
			XDocument config = XDocument.Load (ConfigPath);

			try {
				return config.Root.Element (key).Value;
			} catch (NullReferenceException) {
				throw new TomboyException ("There is no config variable by that name.");
			}
		}

		public string Backup ()
		{
			string msg = "";
			if (!Directory.Exists (BackupPath))
				Directory.CreateDirectory (BackupPath);
			string[] files = Directory.GetFiles (PathToNotes, "*.note", SearchOption.TopDirectoryOnly);
			if (files.Length == 0) {
				msg += "No files were found to backup";
			} else {
				int count = 0;
				foreach (var item in files) {
					File.Copy (item, Path.Combine (BackupPath, Path.GetFileName (item)));
					count ++;
				}
				msg += "A total of " + count + " files were backed up";
			}
			return msg;
		}
	}
}