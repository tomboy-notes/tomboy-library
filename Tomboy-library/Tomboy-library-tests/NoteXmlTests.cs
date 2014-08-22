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
using NUnit.Framework;
using System.IO;
using Tomboy.Xml;

namespace Tomboy
{
	[TestFixture()]
	public class NoteXmlTests
	{
		private Note ReadNoteFromFile (string path, string uri)
		{
			return XmlNoteReader.Read (File.ReadAllText (path), uri);
		}

		[Test()]
		[ExpectedException (typeof (System.Xml.XmlException))]
		public void Read_NonsenseNoteFile_ThrowsException ()
		{
			string StartHereNotePath = "../../test_notes/corrupt_notes/nonsense.note";
			this.ReadNoteFromFile (StartHereNotePath, "tomboy://nonsense");
		}

		[Test()]
		public void Read_ProperNoteFile_CorrectTitle ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = ReadNoteFromFile (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");

			Assert.AreEqual ("Start Here", toCheck.Title);
		}
		[Test()]
		public void Read_ProperNoteFile_CorrectText ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = ReadNoteFromFile (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");

			Assert.That (toCheck.Text.Contains ("Welcome to Tomboy!"));
		}
		[Test()]
		public void Read_ProperNoteFile_HyperTagsArePreserved ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = ReadNoteFromFile (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");

			Assert.That (toCheck.Text.Contains ("<bold>"));
		}
		[Test()]
		public void Read_ProperNoteFile_CreateDateMatches ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = ReadNoteFromFile (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (toCheck.CreateDate.Ticks, 633770466544871750);
		}
		[Test()]
		public void Read_ProperNoteFile_LastChangeDateMatches ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = ReadNoteFromFile (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (toCheck.ChangeDate.Ticks, 634683883142191587);
		}
		[Test()]
		public void Read_ProperNoteFile_LastMetadataChangeDateMatches ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = ReadNoteFromFile (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (toCheck.MetadataChangeDate.Ticks, 634683883142221590);
		}
		[Test()]
		public void Read_ProperNoteFile_WidthAndHeightMatches ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = ReadNoteFromFile (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (toCheck.Width, 450);
			Assert.AreEqual (toCheck.Height, 360);
		}
		[Test()]
		public void Read_ProperNoteFile_XAndYMatches ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = ReadNoteFromFile (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (toCheck.X, 1305);
			Assert.AreEqual (toCheck.Y, 93);
		}
		[Test()]
		public void Read_ProperNoteFile_OpenOnStartupMatches ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = ReadNoteFromFile (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual ("False", toCheck.OpenOnStartup);
			Assert.IsInstanceOf<string> (toCheck.OpenOnStartup);
		}

		[Test]
		public void ReadWrite_Note ()
		{ 
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note sampleNote = ReadNoteFromFile (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			
			string note_xml = XmlNoteWriter.Write (sampleNote);
			Note toCheck = XmlNoteReader.Read (note_xml, sampleNote.Uri);
			Assert.AreEqual (sampleNote.Title, toCheck.Title);
			Assert.AreEqual (sampleNote.Text, toCheck.Text);
			Assert.AreEqual (sampleNote.ChangeDate, toCheck.ChangeDate);
			Assert.AreEqual (sampleNote.CreateDate, toCheck.CreateDate);
			Assert.AreEqual (sampleNote.MetadataChangeDate, toCheck.MetadataChangeDate);
			// TODO compare all fields
		}
		
		[Test]
		public void WriteNotesWithCustomNamespaceTags()
		{
			// this tests checks if the XML writing engine can handle embedded tags with custom namespaces in the 
			// note body. The size and link prefixed tags are CUSTOM namespaces. For valid XML, these have to be
			// defined, and this is what we test.
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note sampleNote = ReadNoteFromFile (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			
			sampleNote.Text = "<size:huge>About</size:huge><link:url>http://www.google.com</link:url>";
			XmlNoteWriter.Write (sampleNote);
		}

		[Test]
		public void ReadWriteMimeTypeMarkdown ()
		{
			var note = new Note ();
			note.MimeType = "application/x-tomboy-markdown";

			using (var stream = new MemoryStream ()) {
				XmlNoteWriter.Write (note, stream);
				stream.Position = 0;
				var readin_note = XmlNoteReader.Read (stream, note.Uri);
				Assert.AreEqual ("application/x-tomboy-markdown", readin_note.MimeType);
				Assert.AreEqual (note.MimeType, readin_note.MimeType);
			}
		}

		[Test]
		public void ReadWriteMimeTypeDefaultIsConvnetionalMimeType ()
		{
			var note = new Note ();

			using (var stream = new MemoryStream ()) {
				XmlNoteWriter.Write (note, stream);
				stream.Position = 0;
				var readin_note = XmlNoteReader.Read (stream, note.Uri);
				Assert.AreEqual ("application/x-tomboy-note", readin_note.MimeType);
				Assert.AreEqual (note.MimeType, readin_note.MimeType);
			}
		}
	}
}

