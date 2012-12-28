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
using Tomboy.Tags;
using System.Linq;

namespace Tomboy.Sync
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
		public void ReadWriteEmptySyncManifest ()
		{
			var empty_manifest = new SyncManifest ();

			// write the sample manifest to XML
			StringBuilder builder = new StringBuilder ();
			XmlWriter writer = XmlWriter.Create (builder);

			SyncManifest.Write (writer, empty_manifest);

			// read in the results
			var textreader = new StringReader (builder.ToString ());
			var xmlreader = new XmlTextReader (textreader);
			var manifest = SyncManifest.Read (xmlreader);
		}

		[Test]
		public void ConvertUriTests ()
		{
			var tomboy_note = new Note ();

			tomboy_note.CreateDate = DateTime.Now;
			tomboy_note.ChangeDate = DateTime.Now;
			tomboy_note.MetadataChangeDate = DateTime.Now;

			var dto_note = tomboy_note.ToDTONote ();

			Assert.That (!string.IsNullOrEmpty (dto_note.Guid));

			Assert.AreEqual (tomboy_note.Guid, dto_note.Guid);
			Assert.That (tomboy_note.Uri.Contains (dto_note.Guid));
			Assert.That (tomboy_note.Uri.Contains (tomboy_note.Guid));

			var tomboy_note_2 = dto_note.ToTomboyNote ();
			Assert.AreEqual (tomboy_note.Guid, tomboy_note_2.Guid);
			Assert.AreEqual (tomboy_note.Uri, tomboy_note_2.Uri);
		}

		[Test]
		public void ConvertFromTomboyNoteToDTO()
		{
			var tomboy_note = new Note ();
			tomboy_note.Title = "This is a sample note";
			tomboy_note.Text = "This is some sample text";

			tomboy_note.ChangeDate = DateTime.Now;
			tomboy_note.CreateDate = DateTime.Now;
			tomboy_note.MetadataChangeDate = DateTime.Now;

			var dto_note = tomboy_note.ToDTONote ();

			Assert.AreEqual (tomboy_note.Title, dto_note.Title);
			Assert.AreEqual (tomboy_note.Text, dto_note.Text);

			Assert.AreEqual (tomboy_note.ChangeDate, DateTime.Parse (dto_note.ChangeDate));
			Assert.AreEqual (tomboy_note.CreateDate, DateTime.Parse (dto_note.CreateDate));
			Assert.AreEqual (tomboy_note.MetadataChangeDate, DateTime.Parse (dto_note.MetadataChangeDate));

			Assert.AreEqual (tomboy_note.Guid, dto_note.Guid);

			var tag_intersection = dto_note.Tags.Intersect (tomboy_note.Tags.Keys);
			Assert.AreEqual (dto_note.Tags.Count (), tag_intersection.Count ());
		}
		[Test]
		public void ConvertFromDTONoteToTomboyNote()
		{
			var dto_note = new DTONote ();
			dto_note.Title = "This is a sample note";
			dto_note.Text = "This is some sample text";

			dto_note.ChangeDate = DateTime.Now.ToString (Tomboy.Writer.DATE_TIME_FORMAT);
			dto_note.MetadataChangeDate = DateTime.Now.ToString (Tomboy.Writer.DATE_TIME_FORMAT);
			dto_note.CreateDate = DateTime.Now.ToString (Tomboy.Writer.DATE_TIME_FORMAT);

			var tomboy_note = dto_note.ToTomboyNote ();

			Assert.AreEqual (tomboy_note.Title, dto_note.Title);
			Assert.AreEqual (tomboy_note.Text, dto_note.Text);

			Assert.AreEqual (tomboy_note.ChangeDate, DateTime.Parse (dto_note.ChangeDate));
			Assert.AreEqual (tomboy_note.CreateDate, DateTime.Parse (dto_note.CreateDate));
			Assert.AreEqual (tomboy_note.MetadataChangeDate, DateTime.Parse (dto_note.MetadataChangeDate));

			var tag_intersection = dto_note.Tags.Intersect (tomboy_note.Tags.Keys);
			Assert.AreEqual (dto_note.Tags.Count (), tag_intersection.Count ());
		}

		[Test]
		public void ConvertFromDTOWithTags ()
		{
			var dto_note = new DTONote ();
			dto_note.Tags = new string[] { "school", "shopping", "fun" };

			var tomboy_note = dto_note.ToTomboyNote ();

			foreach (string tag in dto_note.Tags) {
				Assert.Contains (tag, tomboy_note.Tags.Keys);
			}
		}
		[Test]
		public void ConvertToDTOWithTags ()
		{
			var tomboy_note = new Note ();
			tomboy_note.Tags.Add ("school", new Tag ("school"));
			tomboy_note.Tags.Add ("shopping", new Tag ("shopping"));
			tomboy_note.Tags.Add ("fun", new Tag ("fun"));

			var dto_note = tomboy_note.ToDTONote ();

			foreach (string tag in tomboy_note.Tags.Keys) {
				Assert.Contains (tag, dto_note.Tags);
			}
		}

		[Test]
		public void ConvertBackAndForth ()
		{
			var tn1 = new Note () {
				Title = "This is my Title with Umlauts: äöü",
				Text = "This is my note body text.",
				CreateDate = DateTime.Now - new TimeSpan (365, 0, 0, 0),
				MetadataChangeDate = DateTime.Now,
				ChangeDate = DateTime.Now - new TimeSpan (14, 0, 0, 0)

				// TODO check why OpenOnStartup is of type string in Tomboy
				//OpenOnStartup = "true"
			};

			var dto_note = tn1.ToDTONote ();
			var tn2 = dto_note.ToTomboyNote ();

			// notes should be identical
			Assert.AreEqual (tn1.Guid, tn2.Guid);
			Assert.AreEqual (tn1.Uri, tn2.Uri);
			Assert.AreEqual (tn1.Title, tn2.Title);
			Assert.AreEqual (tn1.Text, tn2.Text);

			Assert.AreEqual (tn1.ChangeDate, tn2.ChangeDate);
			Assert.AreEqual (tn1.MetadataChangeDate, tn2.MetadataChangeDate);
			Assert.AreEqual (tn1.CreateDate, tn2.CreateDate);

			Assert.AreEqual (tn1.OpenOnStartup, tn2.OpenOnStartup);

			// TODO Tags


		}
	}
}
