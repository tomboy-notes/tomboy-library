//
//  Author:
//       Timo Dörr <timo@latecrew.de>
//
//  Copyright (c) 2012 Timo Dörr
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
using System.Xml;
using Tomboy.Sync;
using System.Text;
using System.IO;
using Tomboy.Sync.DTO;
using ServiceStack.Common;

namespace Tomboy
{
	[TestFixture()]
	public class SyncObjectsTest
	{
		SyncManifest sampleManifest;

		[SetUp]
		public void Setup ()
		{
			var manifest = new SyncManifest ();
			manifest.LastSyncDate = DateTime.UtcNow - new TimeSpan (1, 0, 0);
			manifest.LastSyncRevision = 2;

			manifest.NoteRevisions.Add ("1234-5678-9012-3456", 4);
			manifest.NoteRevisions.Add ("1111-2222-3333-4444", 9293);
			manifest.NoteRevisions.Add ("6666-2222-3333-4444", 17);
			
			manifest.NoteDeletions.Add ("1111-11111-1111-1111", "Deleted note 1");
			manifest.NoteDeletions.Add ("1111-11111-2222-2222", "Gelöschte Notiz 2");

			sampleManifest = manifest;
		}
		[Test()]
		public void ReadWriteSyncManifest ()
		{
			// write the sample manifest to XML
			StringBuilder builder = new StringBuilder ();
			XmlWriter writer = XmlWriter.Create (builder);

			SyncManifest.Write (writer, sampleManifest);

			// read in the results
			var textreader = new StringReader (builder.ToString ());
			var xmlreader = new XmlTextReader (textreader);
			var manifest = SyncManifest.Read (xmlreader);

			// verify
			Assert.AreEqual (sampleManifest.LastSyncDate, manifest.LastSyncDate);
			Assert.AreEqual (sampleManifest.LastSyncRevision, manifest.LastSyncRevision);

			Assert.AreEqual (sampleManifest.NoteRevisions.Count, manifest.NoteRevisions.Count);

			foreach (var kvp in sampleManifest.NoteRevisions) {
				Assert.That (manifest.NoteRevisions.ContainsKey (kvp.Key));
				Assert.That (manifest.NoteRevisions [kvp.Key] == kvp.Value);
			}
			foreach (var kvp in sampleManifest.NoteDeletions) {
				Assert.That (manifest.NoteDeletions.ContainsKey (kvp.Key));
				Assert.That (manifest.NoteDeletions[kvp.Key] == kvp.Value);
			}

		}

		[Test]
		public void ConvertFromTomboyNoteToDTO()
		{
			var tomboy_note = new Note ();
			tomboy_note.Title = "This is a sample note";
			tomboy_note.Text = "This is some sample text";

			var dto_note = tomboy_note.ToDTONote ();

			Assert.AreEqual (tomboy_note.Title, dto_note.Title);
			Assert.AreEqual (tomboy_note.Text, dto_note.Text);

			Assert.AreEqual (tomboy_note.ChangeDate, dto_note.ChangeDate);
			Assert.AreEqual (tomboy_note.CreateDate, dto_note.CreateDate);
			Assert.AreEqual (tomboy_note.MetadataChangeDate, dto_note.MetadataChangeDate);

			Assert.AreEqual (tomboy_note.Guid, dto_note.Guid);

			Assert.Fail ("TODO: Tags must be copied over");
			Assert.AreEqual (tomboy_note.Tags, dto_note.Tags);
		}
		[Test]
		public void ConvertFromDTONoteToTomboyNote()
		{
			var dto_note = new DTONote ();
			dto_note.Title = "This is a sample note";
			dto_note.Text = "This is some sample text";

			var tomboy_note = dto_note.ToTomboyNote ();

			Assert.AreEqual (tomboy_note.Title, dto_note.Title);
			Assert.AreEqual (tomboy_note.Text, dto_note.Text);

			Assert.AreEqual (tomboy_note.ChangeDate, dto_note.ChangeDate);
			Assert.AreEqual (tomboy_note.CreateDate, dto_note.CreateDate);
			Assert.AreEqual (tomboy_note.MetadataChangeDate, dto_note.MetadataChangeDate);

			Assert.AreEqual (tomboy_note.Guid, dto_note.Guid);

			// FAILS:
			Assert.Fail ("TODO: Tags must be copied over");
			Assert.AreEqual (tomboy_note.Tags, dto_note.Tags);
		}
	}
}

