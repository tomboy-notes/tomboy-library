// 
//  EngineTests.cs
//  
//  Author:
//       Jared Jennings <jaredljennings@gmail.com>
//  
//  Copyright (c) 2012 Jared L Jennings
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
using System.Xml;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tomboy
{
	[TestFixture()]
	public class EngineTests
	{
		Engine engine;
		Note note;
		[TestFixtureSetUp] public void Init()
		{
			//TODO: The storage instance needs swapping with a stub/mock!
			DiskStorage.Instance.SetPath ("../../test_notes/proper_notes");
			engine = new Engine (DiskStorage.Instance);
			// get a new note instance
			note = engine.NewNote ();
			note.Title = "Unit Test Note";
			note.Text = "Unit test note by NewNote() method";
			engine.SaveNote (note);
		}
		
		[Test()]
		public void Engine_Get_Notes ()
		{
			Assert.IsFalse (engine.GetNotes ().ContainsKey ("note://tomboy/90d8eb70-989d-4b26-97bc-EXAMPLE"));
			Assert.IsTrue (engine.GetNotes ().ContainsKey ("note://tomboy/90d8eb70-989d-4b26-97bc-ba4b9442e51f"));
		}
		
		[Test()]
		public void Engine_New_Note ()
		{
			Note note2;
			Dictionary<string, Note> notes = engine.GetNotes ();
			notes.TryGetValue (note.Uri, out note2);
			Console.WriteLine ("Note2 URI '" + note2.Uri + "'");
			Assert.IsTrue (engine.GetNotes ().ContainsKey (note.Uri));
			string NOTE_PATH = Path.Combine ("../../test_notes/proper_notes", Utils.GetNoteFileNameFromURI (note));
			// make sure the note exists
			Assert.IsTrue (System.IO.File.Exists (NOTE_PATH));
			System.IO.File.Delete (NOTE_PATH); //Clear up test for next time
		}

		[Test()]
		public void Engine_Save_Note_Success ()
		{
			note.Text = "Unit test note by NewNote() method \\n Added text";
			engine.SaveNote (note);
			string NOTE_PATH = Path.Combine ("../../test_notes/proper_notes", Utils.GetNoteFileNameFromURI (note));
			string noteContents = System.IO.File.ReadAllText (NOTE_PATH);
			Assert.IsTrue (noteContents.Contains ("Added text"));
			System.IO.File.Delete (NOTE_PATH); //Clear up test for next time

		}

		[Test()]
		public void Engine_Save_Note_CorrectModifiedTime_Success ()
		{
			note.Text = "Unit test note by NewNote() method \\n Added text";
			engine.SaveNote (note);
			string NOTE_PATH = Path.Combine ("../../test_notes/proper_notes", Utils.GetNoteFileNameFromURI (note));
			string noteContents = System.IO.File.ReadAllText (NOTE_PATH);
			Console.WriteLine (noteContents);
			Assert.IsFalse (noteContents.Contains ("<last-change-date>0001-01-01T00:00:00.0000000-06:00</last-change-date>"));
			System.IO.File.Delete (NOTE_PATH); //Clear up test for next time

		}

		[Test()]
		public void Engine_Delete_Note_Success ()
		{
			string NOTE_PATH = Path.Combine ("../../test_notes/proper_notes", Utils.GetNoteFileNameFromURI (note));
			Assert.IsTrue (System.IO.File.Exists (NOTE_PATH));
			engine.DeleteNote (note);
			Assert.IsFalse (System.IO.File.Exists (NOTE_PATH));
			string BACKUP_NOTE = Path.Combine ("../../test_notes/proper_notes/Backup", Utils.GetNoteFileNameFromURI (note));
			Assert.IsTrue (System.IO.File.Exists (BACKUP_NOTE));
			File.Delete (BACKUP_NOTE);
		}

		[Test()]
		public void Engine_Save_MidifiedTime_Success ()
		{
			DateTime time = note.ChangeDate;
			note.Text = "Modified Text Body";
			// make sure the ChangeDate is different since we modified the note.
			engine.SaveNote (note);
			Assert.AreNotSame (time, note.ChangeDate);
			/* clean-up */
			string NOTE_PATH = Path.Combine ("../../test_notes/proper_notes", Utils.GetNoteFileNameFromURI (note));
			engine.DeleteNote (note);
		}
	}
}

