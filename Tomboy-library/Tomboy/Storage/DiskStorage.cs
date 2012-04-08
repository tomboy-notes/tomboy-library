//  Author:
//       jjennings <jaredljennings@gmail.com>
//  
//  Copyright (c) 2012 jjennings
//  Robert Nordan
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
		
		public void SaveNote (Note note)
		{
			Console.WriteLine ("Saving Note " + note.Title);
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
			WriteFile (write_file, note);
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
			
			/* For anyone wanting to implement another / different backend,
			 * this could be changed in the implementing class to retreive whatever notes
			 */
			string [] files = Directory.GetFiles (path_to_notes, "*.note");
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
	}
}