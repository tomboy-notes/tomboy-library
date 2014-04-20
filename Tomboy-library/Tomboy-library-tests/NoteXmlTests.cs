// 
//  NoteXmlTests.cs
//  
//  Author:
//       Robert Nordan <rpvn@robpvn.net>
//       Timo Dörr <timo@latecrew.de>
//  
//  Copyright (c) 2012 Robert Nordan
//  Copyright (c) 2014 Timo Dörr
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
using NUnit.Framework;
using System.IO;
using Tomboy.Xml;

namespace Tomboy
{
	[TestFixture()]
	public class NoteXmlTests
	{
		//TODO: Make this independent of Diskstorage
		[Test()]
		[ExpectedException (typeof (TomboyException),
		 ExpectedMessage = "Note XML is corrupted!")] //TODO: This message subject to change!
		public void Read_NonsenseNoteFile_ThrowsException ()
		{
			string StartHereNotePath = "../../test_notes/corrupt_notes/nonsense.note";
			DiskStorage.Read (StartHereNotePath, "tomboy://nonsense");
		}

		[Test()]
		public void Read_ProperNoteFile_CorrectTitle ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");

			Assert.AreEqual ("Start Here", toCheck.Title);
		}
		[Test()]
		public void Read_ProperNoteFile_CorrectText ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");

			Assert.That (toCheck.Text.Contains ("Welcome to Tomboy!"));
		}
		[Test()]
		public void Read_ProperNoteFile_HyperTagsArePreserved ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");

			Assert.That (toCheck.Text.Contains ("<bold>"));
		}
		[Test()]
		public void Read_ProperNoteFile_CreateDateMatches ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (toCheck.CreateDate.Ticks, 633770466544871750);
		}
		[Test()]
		public void Read_ProperNoteFile_LastChangeDateMatches ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (toCheck.ChangeDate.Ticks, 634683883142191587);
		}
		[Test()]
		public void Read_ProperNoteFile_LastMetadataChangeDateMatches ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (toCheck.MetadataChangeDate.Ticks, 634683883142221590);
		}
		[Test()]
		public void Read_ProperNoteFile_WidthAndHeightMatches ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (toCheck.Width, 450);
			Assert.AreEqual (toCheck.Height, 360);
		}
		[Test()]
		public void Read_ProperNoteFile_XAndYMatches ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (toCheck.X, 1305);
			Assert.AreEqual (toCheck.Y, 93);
		}
		[Test()]
		public void Read_ProperNoteFile_OpenOnStartupMatches ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (toCheck.OpenOnStartup, false);
			Assert.IsInstanceOfType (typeof(bool), toCheck.OpenOnStartup);
		}

		[Test]
		public void ReadWrite_Note ()
		{ 
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note sampleNote = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			
			string note_xml = XmlNoteWriter.Write (sampleNote);
			Note toCheck = XmlNoteReader.Read (note_xml, sampleNote.Uri);
			Assert.AreEqual (sampleNote.Title, toCheck.Title);
			Assert.AreEqual (sampleNote.Text, toCheck.Text);
			Assert.AreEqual (sampleNote.ChangeDate, toCheck.ChangeDate);
			Assert.AreEqual (sampleNote.CreateDate, toCheck.CreateDate);
			// TODO compare all fields
		}
		
		[Test]
		public void WriteNotesWithCustomNamespaceTags()
		{
			// this tests checks if the XML writing engine can handle embedded tags with custom namespaces in the 
			// note body. The size and link prefixed tags are CUSTOM namespaces. For valid XML, these have to be
			// defined, and this is what we test.
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note sampleNote = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			
			sampleNote.Text = "<size:huge>About</size:huge><link:url>http://www.google.com</link:url>";
			string note_xml = XmlNoteWriter.Write (sampleNote);
		}
	}
}

