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

using Tomboy.Tags;
using System.Linq;
using PortableIoC;

namespace Tomboy
{
	[TestFixture()]
	public class EngineTests
	{
		Engine engine;
		Note note;
		DiskStorage diskStorage;
		string NOTE_PATH = "";

		[SetUp] public void Init()
		{
			IPortableIoC container = new PortableIoc ();
			container.Register<DiskStorage> (c => {
				return new DiskStorage ("../../test_notes/proper_notes") {
					Logger = new ConsoleLogger ()
				};
			});
			diskStorage = container.Resolve<DiskStorage> ();

			engine = new Engine (diskStorage);
			// get a new note instance
			note = engine.NewNote ();
			note.Title = "Unit Test Note";
			note.Text = "Unit test note by NewNote() method";
			engine.SaveNote (note);
			NOTE_PATH = Path.Combine ("../../test_notes/proper_notes", Utils.GetNoteFileNameFromURI (note));
		}

		[TearDown]
		public void Cleanup ()
		{
			try {
				System.IO.File.Delete (NOTE_PATH); //Clear up test for next time
			} catch (Exception) {};
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
			/* holds a list of events (notes) received */
			List<string> addNotes = new List<string> ();
			engine.NoteAdded += delegate(Note addedNote) {
				addNotes.Add (addedNote.Title);
			};
			Note newNote = engine.NewNote ();
			Assert.IsTrue (engine.GetNotes ().ContainsKey (newNote.Uri));
			Assert.IsTrue (addNotes.Contains (newNote.Title));
		}

		[Test()]
		public void Engine_Save_Note_Success ()
		{
			/* holds a list of events (notes) received */
			List<string> addNotes = new List<string> ();
			engine.NoteUpdated += delegate(Note addedNote) {
				addNotes.Add (addedNote.Title);
			};
			note.Text = "Unit test note by NewNote() method \\n Added text";
			note.Tags.Add ("school", new Tag ("school"));
			engine.SaveNote (note);
			string noteContents = System.IO.File.ReadAllText (NOTE_PATH);
			Assert.IsTrue (noteContents.Contains ("Added text"));
			Assert.IsTrue (noteContents.Contains ("school"));
			Assert.IsTrue (addNotes.Contains (note.Title));
		}

		[Test()]
		public void Engine_Save_Note_Tags_Updated()
		{
			note.Tags.Add ("school", new Tag ("school"));
			engine.SaveNote (note);

			var storedNotes = engine.GetNotes ().Values;
			var storedNote = storedNotes.First (n => n == note);
			Assert.AreEqual (storedNote.Tags.Count, 1);
			Assert.AreEqual (storedNote.Tags.Keys.First (), "school");
		}
		[Test()]
		public void Engine_Save_Note_CorrectModifiedTime_Success ()
		{
			note.Text = "Unit test note by NewNote() method \\n Added text";
			engine.SaveNote (note);
			string noteContents = System.IO.File.ReadAllText (NOTE_PATH);
			Console.WriteLine (noteContents);
			Assert.IsFalse (noteContents.Contains ("<last-change-date>0001-01-01T00:00:00.0000000-06:00</last-change-date>"));

		}

		[Test()]
		public void Engine_Delete_Note_Success ()
		{
			/* holds a list of events (notes) received */
			List<string> addNotes = new List<string> ();
			engine.NoteRemoved += delegate(Note addedNote) {
				addNotes.Add (addedNote.Title);
			};
			Assert.IsTrue (System.IO.File.Exists (NOTE_PATH));
			engine.DeleteNote (note);
			Assert.IsFalse (System.IO.File.Exists (NOTE_PATH));
			string BACKUP_NOTE = Path.Combine ("../../test_notes/proper_notes/Backup", Utils.GetNoteFileNameFromURI (note));
			Assert.IsTrue (System.IO.File.Exists (BACKUP_NOTE));
			Assert.IsTrue (addNotes.Contains (note.Title));
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
		}
	}
}

