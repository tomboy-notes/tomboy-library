//  Author:
//       Robert Nordan
//  
//  Copyright (c) 2012
//  Robert Nordan
//  Jared Jennings
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
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tomboy
{
	[TestFixture()]
	public class DiskStorageTests
	{
		private const string NOTE_FOLDER_PROPER_NOTES = "../../test_notes/proper_notes";
		private const string NOTE_FOLDER_CORRUPT_NOTES = "../../test_notes/corrupt_notes";
		
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
			storage.SetPath (NOTE_FOLDER_PROPER_NOTES);
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
			const string NOTE_PATH = "../../test_notes/temp_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51d.note";
			System.IO.File.Delete (NOTE_PATH); //Make sure it doesn't exist from before
			
			DiskStorage.Write (NOTE_PATH, TesterNote.GetTesterNote ());
			Assert.IsTrue (System.IO.File.Exists (NOTE_PATH));
			
			System.IO.File.Delete (NOTE_PATH); //Clear up test for next time
		}
		
		[Test()]
		public void WriteNote_NoteFolderDoesNotExist_FolderAndNoteFileIsCreated ()
		{	
			const string NOTE_PATH = "../../nonexistant_folder/90d8eb70-989d-4b26-97bc-ba4b9442e51d.note";
			const string DIR_PATH = "../../nonexistant_folder";
			

			try {
				System.IO.File.Delete (NOTE_PATH);
				System.IO.Directory.Delete (DIR_PATH);
			}
			catch (System.IO.DirectoryNotFoundException) {} //This is expected provided the test passed last time.
			
			DiskStorage.Write (NOTE_PATH, TesterNote.GetTesterNote ());
			Assert.IsTrue (System.IO.File.Exists (NOTE_PATH));
			
			System.IO.File.Delete (NOTE_PATH); //Clear up test for next time
			System.IO.Directory.Delete (DIR_PATH);
		}
		
		[Test()]
		public void WriteNote_NoteFileExists_NoteFileIsOverwritten ()
		{	
			const string NOTE_PATH = "../../test_notes/temp_notes/existing_note.note";
			
			System.IO.File.WriteAllText (NOTE_PATH, "Test");
			
			DiskStorage.Write (NOTE_PATH, TesterNote.GetTesterNote ());
		
			string noteContents = System.IO.File.ReadAllText (NOTE_PATH);
			Assert.AreNotEqual (noteContents, "Test", "The pre-existing note has not been overwritten!");
			
			System.IO.File.Delete (NOTE_PATH); //Clear up test for next time
		}
		
	}
}

