// 
//  SearchTests.cs
//  
//  Author:
//       Robert Nordan <rpvn@robpvn.net>
// 
//  Copyright (c) 2012 Robert Nordan
// 
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
// 
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
using System;
using NUnit.Framework;
using System.Collections.Generic;

namespace Tomboy
{
	[TestFixture()]
	public class SearchTests
	{
		Engine engine; 

		[SetUp()]
		public void Init ()
		{
			IStorage storage = new DiskStorage ("../../test_notes/proper_notes");
			engine = new Engine (storage);
		}

		[Test()]
		public void SearchTitles_RelevantNotesExist_AreFound ()
		{
			Dictionary<string, Note> result = engine.GetNotes ("note", false);

			Assert.AreEqual (3, result.Count); //We have at least 3 notes in the test collection
		}

		[Test()]
		public void SearchTitles_NoRelevantNotes_NoneReturned ()
		{
			Dictionary<string, Note> result = engine.GetNotes ("slartibartfast", false);

			Assert.AreEqual (0, result.Count);
		}

		[Test()]
		public void SearchContent_RelevantNotesExist_AreFound ()
		{
			Dictionary<string, Note> result = engine.GetNotes ("note", true);

			Assert.AreEqual (4, result.Count); //One note has only got "note" in the title, but being in the title also means it's in the content!
		}

		[Test()]
		public void SearchContent_NoRelevantNotes_NoneReturned ()
		{
			Dictionary<string, Note> result = engine.GetNotes ("slartibartfast", true);

			Assert.AreEqual (0, result.Count);
		}

		[Test()]
		[Ignore ("Ideally this should be like this, but implementing it is work which is best put off atm.")]
		public void SearchContent_SearchXMLTags_NoneReturned ()
		{
			//This test searches for strings found in XML tags, which shouldn't be returned be hits
			//note.text includes internal XML tags, we want a pure text thingy?
			Dictionary<string, Note> result = engine.GetNotes ("bold", true);

			Assert.AreEqual (0, result.Count);
		}

	}
}

