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
using NUnit.Framework;

namespace Tomboy
{
	[TestFixture()]
	public class EngineTests
	{
		Engine engine;
		[TestFixtureSetUp] public void Init()
		{
			//TODO: The storage instance needs swapping with a stub/mock!
			DiskStorage.Instance.SetPath ("../../test_notes/proper_notes");
			engine = new Engine (DiskStorage.Instance);
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
			//TODO: Needs fixing
			Note note = engine.NewNote ();
			note.Title = "Unit Test Note";
			note.Text = "Unit test note by NewNote() method";
			engine.SaveNote (note);
						
			Console.WriteLine ("Note URI '" + note.Uri + "'");
			Note note2 = null;
			Dictionary<string, Note> notes = engine.GetNotes ();
			notes.TryGetValue (note.Uri, out note2);
			Console.WriteLine ("Note2 URI '" + note2.Uri + "'");
			Assert.IsTrue (engine.GetNotes ().ContainsKey (note.Uri));
						
			//string StartHereNotePath = "../../test_notes/" + Utils.GetNoteFileNameFromURI (note) + ".note";
			//using (var xml = new XmlTextReader (new StreamReader (StartHereNotePath, System.Text.Encoding.UTF8)) {Namespaces = false})
			//	Console.WriteLine (xml);
		}
	}
}

