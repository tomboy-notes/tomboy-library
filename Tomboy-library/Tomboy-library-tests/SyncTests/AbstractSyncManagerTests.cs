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
using Tomboy.Sync;
using System.Linq;
using System.Collections.Generic;

namespace Tomboy.Sync
{

	public abstract partial class AbstractSyncManagerTests : AbstractSyncManagerTestsBase
	{

		[Test]
		public void FirstSyncForBothSidesTest ()
		{
			// before the sync, the client should have an empty AssociatedServerId
			Assert.That (string.IsNullOrEmpty (syncClientOne.AssociatedServerId));

			SyncManager sync_manager = new SyncManager (this.syncClientOne, this.syncServer);
			sync_manager.DoSync ();

			// after the sync the client should carry the associated ServerId
			Assert.That (!string.IsNullOrEmpty (syncClientOne.AssociatedServerId));
			Assert.AreEqual (syncClientOne.AssociatedServerId, syncServer.Id);

			// both revisions should be 0
			Assert.AreEqual (0, syncClientOne.LastSynchronizedRevision);
			Assert.AreEqual (0, syncServer.LatestRevision);

			Assert.Greater (syncServer.UploadedNotes.Count, 0);

			ClearClientOne (reset: false);
			ClearClientTwo (reset: false);

			Assert.Greater (syncServer.UploadedNotes.Count, 0);
		}

		[Test]
		public void ClientHasAllNotesAfterFirstSync ()
		{
			FirstSyncForBothSidesTest ();

			var client_one_notes = clientEngineOne.GetNotes ().Values;
			Assert.AreEqual (sampleNotes.Count, client_one_notes.Count);

			foreach (var note in sampleNotes)
				Assert.Contains (note, client_one_notes);
		}
	
		[Test]
		public void NoteDatesAfterSync ()
		{
			// this test makes sure the Dates of the note are not modified by the sync process
			// which involces heavy copy & creation of notes on all sides

			FirstSyncForBothSidesTest ();

			var server_notes = syncServer.GetAllNotes (true);
			var uploaded_notes = syncServer.UploadedNotes;

			foreach (var note in clientEngineOne.GetNotes ().Values) {
				// now make sure, the metadata change date is smaller or equal to the last
				// sync date
				Assert.LessOrEqual (note.MetadataChangeDate, clientManifestOne.LastSyncDate);

				// the corresponding server note should have the exact same dates
				var server_note = server_notes.Single (n => n.Guid == note.Guid);
				Assert.AreEqual (note.ChangeDate, server_note.ChangeDate);
				Assert.AreEqual (note.MetadataChangeDate, server_note.MetadataChangeDate);
				Assert.AreEqual (note.CreateDate, server_note.CreateDate);

				// the same should hold true for the newly uploaded notes
				var uploaded_note = uploaded_notes.Single (n => n == note);
				Assert.AreEqual (note.ChangeDate, uploaded_note.ChangeDate);
				Assert.AreEqual (note.MetadataChangeDate, uploaded_note.MetadataChangeDate);
				Assert.AreEqual (note.CreateDate, uploaded_note.CreateDate);
			}
		}

		[Test]
		public void MakeSureTextIsSynced ()
		{
			FirstSyncForBothSidesTest ();

			var server_notes = syncServer.GetAllNotes (true);
			foreach (var note in clientEngineOne.GetNotes ().Values) {
				var note_on_server = server_notes.Single (n => n == note);
				Assert.AreEqual (note_on_server.Text, note.Text);
			}
		}
		[Test]
		public void MakeSureAllTagsAreSynced ()
		{
			FirstSyncForBothSidesTest ();
			var server_notes = syncServer.GetAllNotes (true);
			foreach (var note_on_client in clientEngineOne.GetNotes ().Values) {
				var note_on_server = server_notes.Single (n => n == note_on_client);

				// tags counts are equal
				Assert.AreEqual (note_on_server.Tags.Count, note_on_client.Tags.Count);
			}
		}

		[Test]
		public void ClientSyncsToNewServer()
		{
			FirstSyncForBothSidesTest ();
		}

		[Test]
		public void ClientDeletesNotesAfterFirstSync ()
		{
			// perform initial sync
			FirstSyncForBothSidesTest ();
			
			Assert.AreEqual (3, clientEngineOne.GetNotes ().Count);
			
			// now, lets delete a note from the client
			var deleted_note = clientEngineOne.GetNotes ().First ().Value;
			clientEngineOne.DeleteNote (deleted_note);
			clientManifestOne.NoteDeletions.Add (deleted_note.Guid, deleted_note.Title);
			
			// perform a sync again
			var sync_manager = new SyncManager (syncClientOne, syncServer);
			sync_manager.DoSync ();
			
			// one note should have been deleted on server
			Assert.AreEqual (1, syncServer.DeletedServerNotes.Count);
			Assert.AreEqual (deleted_note.Guid, syncServer.DeletedServerNotes.First ());
			
			// zero notes were deleted on the client
			Assert.AreEqual (0, syncClientOne.DeletedNotes.Count);
			
			//  server now holds a total of two notes
			Assert.AreEqual (2, syncServer.GetAllNotes (true).Count);
			
			// all notes on the client and the server should be equal
			var client_notes = clientEngineOne.GetNotes ().Values;
			var server_notes = syncServer.GetAllNotes (true);
			var intersection = client_notes.Intersect (server_notes);
			Assert.AreEqual (2, intersection.Count ());
		}

		[Test]
		public void NoSyncingNeededIfNoChangesAreMade ()
		{
			FirstSyncForBothSidesTest ();
				
			var server_id = syncClientOne.AssociatedServerId;
			
			// new instance of the server needed (to simulate a new connection)
			ClearServer (reset: false);
			
			// now that we are synced, there should not happen anything when syncing again
			SyncManager sync_manager = new SyncManager (syncClientOne, syncServer);
			sync_manager.DoSync ();
			
			// the association id should not have changed
			Assert.AreEqual (server_id, syncClientOne.AssociatedServerId);
			Assert.AreEqual (server_id, syncServer.Id);
			
			// no notes should have been transfered or deleted
			Assert.AreEqual (0, syncServer.UploadedNotes.Count);
			Assert.AreEqual (0, syncServer.DeletedServerNotes.Count);
		}

		[Test]
		[Ignore("")]
		public void MassiveAmountOfNotes ()
		{
			// we have some notes added by default, so substract from total amount
			int num_notes = 1024 - clientEngineOne.GetNotes ().Count;
			
			while (num_notes-- > 0) {
				var note = NoteCreator.NewNote ("Sample note number " + num_notes, "This is a sample note body.");
				clientEngineOne.SaveNote (note);
			}
			
			// perform first sync
			FirstSyncForBothSidesTest ();

			Assert.AreEqual (1024, clientEngineOne.GetNotes ().Count);
			Assert.AreEqual (1024, syncServer.UploadedNotes.Count);
		}

		[Test]
		public void GetNoteUpdateSinceTest ()
		{
			FirstSyncForBothSidesTest ();

			var notes = syncServer.GetNoteUpdatesSince (-1);

			var client_notes = clientEngineOne.GetNotes ().Values;
			client_notes.ToList ().ForEach(n=> { Assert.That (notes.Contains (n)); });

			notes = syncServer.GetNoteUpdatesSince (0);
			Assert.AreEqual (0, notes.Count);
		}

		[Test]
		public void ClientSyncsMultipleTimes()
		{
			// perform initial sync
			FirstSyncForBothSidesTest ();

			foreach (var note in clientStorageOne.GetNotes ().Values) {
				note.Title = "New title";
				clientEngineOne.SaveNote (note);
			}

			// perform a sync again
			var sync_manager = new SyncManager (syncClientOne, syncServer);
			sync_manager.DoSync ();

			foreach (var note in clientStorageOne.GetNotes ().Values) {
				note.Title = "New title two";
				clientEngineOne.SaveNote (note);
			}

			return;

			// perform a sync again
			sync_manager = new SyncManager (syncClientOne, syncServer);
			sync_manager.DoSync ();

			foreach (var note in clientStorageOne.GetNotes ().Values) {
				note.Title = "New title three";
				clientEngineOne.SaveNote (note);
			}
		}
	}
}
