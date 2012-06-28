//  Author:
//       Robert Nordan
//  
//  Copyright (c) 2012
//  Robert Nordan
//  Jared Jennings
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 2.1 of the License, or
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
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tomboy
{
	[TestFixture()]
	public class DiskStorageTests
	{
		private const string NOTE_FOLDER_PROPER_NOTES = "../../test_notes/proper_notes";
		private const string NOTE_FOLDER_CORRUPT_NOTES = "../../test_notes/corrupt_notes";
		private const string NOTE_FOLDER_INVALID = "../../test_notes/invalid_notes";
		private const string NOTE_FOLDER_TEMP = "../../test_notes/temp_notes";

		[Test()]
		public void SetPath_NoteFolderDoesNotExist_CreatesFolder ()
		{

			try {
				System.IO.Directory.Delete (NOTE_FOLDER_INVALID);
			}
			catch (System.IO.DirectoryNotFoundException) {} //This is expected provided the test passed last time.

			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_INVALID);

			Assert.IsTrue (System.IO.Directory.Exists (NOTE_FOLDER_INVALID));
			System.IO.Directory.Delete (NOTE_FOLDER_INVALID);

		}

		[Test()]
		public void Read_ProperNoteFile_ReadCorrectly ()
		{
			//TODO: Has a dependence on Reader.Read, how do we break it?
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note StartHere =  DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (StartHere.Title, "Start Here");
		}
		
		[Test()]
		public void GetNotes_NotesExist_ReturnsNotes ()
		{
			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_PROPER_NOTES);
			Dictionary<string, Note> notes = storage.GetNotes ();
			Assert.IsNotNull (notes);
			Assert.IsTrue (notes.Count > 0);			
		}
		
		[Test()]
		public void GetNotes_NoteFolderDoesNotExist_ReturnsNone ()
		{
			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_INVALID);
			Dictionary<string, Note> notes = storage.GetNotes ();
			Assert.IsNotNull (notes);
			Assert.IsTrue (notes.Count == 0);			
		}
		
		[Test()]
		public void GetNotes_NoteExistsWithSpecificKey_ReturnsNoteWithSpecificKey ()
		{			
			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_PROPER_NOTES);
			Dictionary<string, Note> notes = storage.GetNotes ();
			Assert.IsTrue (notes.ContainsKey ("note://tomboy/90d8eb70-989d-4b26-97bc-ba4b9442e51f"));
		}
		
		[Test()]
		public void GetNotes_NoteDoesNotExistWithSpecificKey_DoesNotReturnNoteWithSpecificKey ()
		{			
			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_PROPER_NOTES);
			Dictionary<string, Note> notes = storage.GetNotes ();
			Assert.IsFalse (notes.ContainsKey ("not-a-key"));
		}
		
		[Test()]
		public void WriteNote_NoteFileDoesNotExist_NoteFileIsCreated ()
		{	
			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_TEMP);

			string note_name = "90d8eb70-989d-4b26-97bc-ba4b9442e51d.note";
			string note_path = Path.Combine (NOTE_FOLDER_TEMP, note_name);
			System.IO.File.Delete (note_path); //Make sure it doesn't exist from before
			
			DiskStorage.Write (note_name, TesterNote.GetTesterNote ());
			Assert.IsTrue (System.IO.File.Exists (note_path));
			
			System.IO.File.Delete (note_path); //Clear up test for next time
		}

		[Test()]
		public void WriteNote_NoteFileExists_NoteFileIsOverwritten ()
		{	

			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_TEMP);
			
			string note_name = "existing_note.note";
			string note_path = Path.Combine (NOTE_FOLDER_TEMP, note_name);

			System.IO.File.WriteAllText (note_path, "Test");
			
			DiskStorage.Write (note_name, TesterNote.GetTesterNote ());
		
			string noteContents = System.IO.File.ReadAllText (note_path);
			Assert.AreNotEqual (noteContents, "Test", "The pre-existing note has not been overwritten!");
			
			System.IO.File.Delete (note_path); //Clear up test for next time
		}

		[Test()]
		public void SetConfigVariable_NoConfigFile_CreatesFileAndVariable ()
		{
			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_TEMP);

			string config_name = "config.xml";
			string config_path = Path.Combine (NOTE_FOLDER_TEMP, config_name);
			System.IO.File.Delete (config_path); //Make sure it doesn't exist from before

			storage.SetConfigVariable ("testvar", "testval");

			Assert.IsTrue (System.IO.File.Exists (config_path));
			XDocument config = XDocument.Load (config_path);
			Assert.AreEqual ("testval", config.Root.Element ("testvar").Value);
			
			System.IO.File.Delete (config_path); //Clear up test for next time
		}

		[Test()]
		public void SetConfigVariable_ConfigFileExists_CreatesVariable ()
		{
			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_PROPER_NOTES);
			string config_name = "config.xml";
			string config_path = Path.Combine (NOTE_FOLDER_PROPER_NOTES, config_name);

			storage.SetConfigVariable ("testvar2", "testval2");

			XDocument config = XDocument.Load (config_path);
			Assert.AreEqual ("testval2", config.Root.Element ("testvar2").Value);

			config.Root.Element ("testvar2").Remove (); //Reset
			config.Save (config_path);
		}
		
		[Test()]
		public void SetConfigVariable_ConfigFileExists_UpdatesVariable ()
		{
			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_PROPER_NOTES);
			string config_name = "config.xml";
			string config_path = Path.Combine (NOTE_FOLDER_PROPER_NOTES, config_name);

			storage.SetConfigVariable ("testvar", "testval2");

			XDocument config = XDocument.Load (config_path);
			Assert.AreEqual ("testval2", config.Root.Element ("testvar").Value);

			config.Root.Element ("testvar").Value = "testval"; //Reset
			config.Save (config_path);
		}
		
		[Test()]
		public void ReadConfigVariable_ConfigFileExists_ReturnsVariable ()
		{
			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_PROPER_NOTES);

			Assert.AreEqual ("testval", storage.GetConfigVariable ("testvar"));
		}

		[Test()]
		public void ReadConfigVariable_NoConfigFile_ReturnsNull ()
		{
			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_TEMP);

			Assert.IsNull (storage.GetConfigVariable ("whatever"));
		}

		[Test()]
		public void ReadConfigVariable_NoVariableExists_ReturnsNull ()
		{
			IStorage storage = DiskStorage.Instance;
			storage.SetPath (NOTE_FOLDER_PROPER_NOTES);

			Assert.IsNull (storage.GetConfigVariable ("whatever"));
		}
	}
}

