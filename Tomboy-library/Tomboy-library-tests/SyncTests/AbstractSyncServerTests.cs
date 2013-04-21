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
using NUnit.Framework;
using System.Collections.Generic;
using Tomboy;
using System.Linq;
using Tomboy.Sync;
using System;

namespace Tomboy.Sync
{
	[TestFixture()]
	/// <summary>
	/// Abstract tests against the ISyncServer implementation. Other classes should derive from this class
	/// to test implementation specific behaviour (FilesystemSyncServer, WebSyncServer). Each test is for 
	/// basic functionality of ISyncServer (in contrast to full-blown sync tests with SyncManager).
	/// </summary>
	public abstract class AbstractSyncServerTests
	{
		protected ISyncServer syncServer;
		protected List<Note> sampleNotes;

		protected void CreateSomeSampleNotes ()
		{
			sampleNotes = GetSomeSampleNotes ();
		}
		public static List<Note> GetSomeSampleNotes ()
		{
			var sample_notes = new List<Note> ();

			// TODO: add tags to the notes!
			sample_notes.Add(new Note () {
				Title = "Sämplé title 1!",
				Text = "** This is the text of Sämple Note 1**",
				CreateDate = DateTime.Now,
				MetadataChangeDate = DateTime.Now,
				ChangeDate = DateTime.Now
			});
			
			sample_notes.Add(new Note () {
				Title = "2nd Example",
				Text = "This is the text of the second sample note",
				CreateDate = new DateTime (1984, 04, 14, 4, 32, 0, DateTimeKind.Utc),
				ChangeDate = new DateTime (2012, 04, 14, 4, 32, 0, DateTimeKind.Utc),
				MetadataChangeDate = new DateTime (2012, 12, 12, 12, 12, 12, DateTimeKind.Utc),
			});
			
			// note that DateTime.MinValue is not an allowed timestamp for notes!
			sample_notes.Add(new Note () {
				Title = "3rd exampel title",
				Text = "Another example note",
				CreateDate = DateTime.MinValue + new TimeSpan (1, 0, 0, 0, 0),
				ChangeDate = DateTime.MinValue + new TimeSpan (1, 0, 0, 0, 0),
				MetadataChangeDate = DateTime.MinValue + new TimeSpan (1, 0, 0, 0, 0)
			});

			return sample_notes;
		}

		[Test]
		public void SyncServerBasic ()
		{
			syncServer.BeginSyncTransaction ();

			Assert.That (!string.IsNullOrEmpty (syncServer.Id));
		}

		[Test]
		public void SyncServerPutNotes ()
		{
			syncServer.BeginSyncTransaction ();
			
			syncServer.UploadNotes (sampleNotes);
			
			// after upload, we should be able to get that very same notes
			var received_notes = syncServer.GetAllNotes (true);

			Assert.AreEqual (sampleNotes.Count, received_notes.Count);
			Assert.AreEqual (sampleNotes.Count, syncServer.UploadedNotes.Count);

			sampleNotes.ToList().ForEach (local_note => {

				Assert.That (syncServer.UploadedNotes.Contains (local_note));
				// pick the note from out returned notes list as the order may be
				// different
				var server_note = received_notes.Where (n => n.Guid == local_note.Guid).FirstOrDefault ();
				Assert.That (server_note != null);

				// assert notes are equal
				Assert.AreEqual(local_note.Title, server_note.Title);
				Assert.AreEqual(local_note.Text, server_note.Text);
				Assert.AreEqual(local_note.CreateDate, server_note.CreateDate);

				// FAILs: Rainy is not allowed to modify the ChangeDate in its own engine
				Assert.AreEqual(local_note.MetadataChangeDate, server_note.MetadataChangeDate);
				Assert.AreEqual(local_note.ChangeDate, server_note.ChangeDate);

			});
		}

		[Test()]
		public void SyncServerGetAllNotesWithBody ()
		{

			syncServer.BeginSyncTransaction ();
			syncServer.UploadNotes (sampleNotes);
			syncServer.CommitSyncTransaction ();

			var notes = syncServer.GetAllNotes (true);

			Assert.Greater (notes.Count, 0);
			notes.ToList ().ForEach (note => {
				Assert.That (!string.IsNullOrEmpty (note.Text));
			});
		}

		[Test()]
		public void SyncServerGetAllNotesWithoutBody ()
		{

			syncServer.BeginSyncTransaction ();
			syncServer.UploadNotes (sampleNotes);
			syncServer.CommitSyncTransaction ();

			var notes = syncServer.GetAllNotes (false);
			notes.ToList ().ForEach (note => {
				Assert.AreEqual ("", note.Text);
			});
		}

		[Test()]
		public void SyncServerDeleteAllNotes()
		{
			syncServer.BeginSyncTransaction ();

			syncServer.UploadNotes (sampleNotes);
			syncServer.DeleteNotes (sampleNotes.Select (n => n.Guid).ToList ());
			var server_notes = syncServer.GetAllNotes (false);

			Assert.AreEqual (0, server_notes.Count);

			syncServer.DeletedServerNotes.ToList ().ForEach (deleted_note_guid => {
				Assert.That (sampleNotes.Select(n => n.Guid).Contains (deleted_note_guid));
			});
		}
		[Test()]
		public void SyncServerDeleteSingleNote ()
		{
			syncServer.BeginSyncTransaction ();

			syncServer.UploadNotes (sampleNotes);
			var deleted_note = sampleNotes.First ();
			syncServer.DeleteNotes (new List<string> () { deleted_note.Guid });

			var server_notes = syncServer.GetAllNotes (false);

			// 2 notes should remain on the server
			Assert.AreEqual (2, server_notes.Count);
			// the deleted note should not be one of them
			Assert.AreEqual (0, server_notes.Where (n => n.Guid == deleted_note.Guid).Count ());

			Assert.AreEqual (deleted_note.Guid, syncServer.DeletedServerNotes.First ());

		}

		[Test()]
		public void SyncServerRevision ()
		{
			syncServer.BeginSyncTransaction ();

			syncServer.GetAllNotes (false);
			Assert.AreEqual(-1, syncServer.LatestRevision);

			syncServer.UploadNotes (new List<Note> () { sampleNotes[0] });
			syncServer.CommitSyncTransaction ();

			Assert.AreEqual(0, syncServer.LatestRevision);

			// TODO
//			server = new WebSyncServer (baseUri, GetAccessToken ());

			/*syncServer.BeginSyncTransaction ();
			syncServer.UploadNotes (new List<Note> () { sampleNotes[1] });
			syncServer.CommitSyncTransaction ();

			Assert.AreEqual(1, syncServer.LatestRevision); */
		}


		[Test]
		public void SyncServerInstanceCanBeReused ()
		{
			syncServer.BeginSyncTransaction ();

			syncServer.UploadNotes (new List<Note> { this.sampleNotes[0] });

			syncServer.CommitSyncTransaction ();

			Assert.AreEqual (1, syncServer.UploadedNotes.Count);
			Assert.AreEqual (sampleNotes[0], syncServer.UploadedNotes[0]);

			syncServer.BeginSyncTransaction ();
			syncServer.UploadNotes (new List<Note> { this.sampleNotes[1] });
			syncServer.CommitSyncTransaction ();

			Assert.AreEqual (1, syncServer.UploadedNotes.Count);
			Assert.AreEqual (sampleNotes[1], syncServer.UploadedNotes[0]);
		}
	}
}
