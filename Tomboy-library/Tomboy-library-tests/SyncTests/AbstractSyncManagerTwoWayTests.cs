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

using System.Linq;
using NUnit.Framework;
using Tomboy.Sync;
using System;

namespace Tomboy.Sync
{
	public abstract partial class AbstractSyncManagerTests
	{
		[Test]
		public void TwoWaySyncBasic ()
		{
			// initial sync
			FirstSyncForBothSidesTest ();

			// revision should have increased from -1 to 0
			Assert.AreEqual (0, syncServer.LatestRevision);
			Assert.That (string.IsNullOrEmpty (syncClientTwo.AssociatedServerId));

			// sync with another client
			new SyncManager (syncClientTwo, syncServer).DoSync ();

			var server_notes = syncServer.GetAllNotes (true);
			var client1_notes = clientEngineOne.GetNotes ().Values;
			var client2_notes = clientEngineTwo.GetNotes ().Values;

			// all three participants should now have the same notes
			Assert.AreEqual (sampleNotes.Count, server_notes.Count);
			foreach (var server_note in server_notes) {
				Assert.Contains (server_note, client1_notes);
				Assert.Contains (server_note, client2_notes);
			}

			// while the global revision should still be 0
			Assert.AreEqual (0, syncServer.LatestRevision);

			// the second client should be on the same level as the server
			Assert.AreEqual (syncServer.LatestRevision, syncClientTwo.LastSynchronizedRevision);


			// all three participants should have the same server/sync id
			Assert.AreNotEqual (string.Empty, syncServer.Id);
			Assert.AreEqual (syncServer.Id, syncClientOne.AssociatedServerId);
			Assert.AreEqual (syncServer.Id, syncClientTwo.AssociatedServerId);

			Assert.AreEqual (sampleNotes.Count, client2_notes.Count);
			Assert.AreEqual (sampleNotes.Count, client1_notes.Count);
			
		}

		[Test]
		public void TwoWaySyncEditedNote ()
		{
			// perform a two way sync, so that both client have
			// the same notes
			TwoWaySyncBasic ();

			// edit a note
			var edited_note = clientEngineOne.GetNotes ().Values.First ();
			edited_note.Title = "This note has changed!";
			clientEngineOne.SaveNote (edited_note);

			new SyncManager (syncClientOne, syncServer).DoSync ();

			ClearClientOne ();
			ClearClientTwo ();
			ClearServer ();

			new SyncManager (syncClientTwo, syncServer).DoSync ();

			var synced_edited_note = clientEngineTwo.GetNotes ().Values.Single (n => edited_note == n);
			Assert.AreEqual (edited_note.Title, synced_edited_note.Title);
		}

		[Test]
		public void TwoWaySyncFetchOnlyRevisions ()
		{
			FirstSyncForBothSidesTest ();
			// LastSynchronizedRevision is now 0 for clientOne and server

			new SyncManager (this.syncClientTwo, this.syncServer).DoSync ();

			// the second client did not update or delete a note, so the global repo counter
			// should not have advanced
			Assert.AreEqual (0, this.syncServer.LatestRevision);
			Assert.AreEqual (0, this.syncClientOne.LastSynchronizedRevision);
			Assert.AreEqual (0, this.syncClientTwo.LastSynchronizedRevision);

			ClearClientOne (reset: false);
			ClearClientTwo (reset: false);
			ClearServer (reset: false);

			// edit a note
			var edited_note = clientEngineOne.GetNotes ().Values.First ();
			edited_note.Title = "This note has changed!";
			clientEngineOne.SaveNote (edited_note);

			new SyncManager (this.syncClientOne, this.syncServer).DoSync ();

			Assert.AreEqual (1, this.syncServer.LatestRevision);
			// LastSynchronizedRevision is now 1 for clientOne and server

			ClearClientOne (reset: false);
			ClearClientTwo (reset: false);
			ClearServer (reset: false);

			new SyncManager (this.syncClientTwo, this.syncServer).DoSync ();

			Assert.AreEqual (1, this.syncClientOne.LastSynchronizedRevision);
			Assert.AreEqual (1, this.syncServer.LatestRevision);
			Assert.AreEqual (1, this.syncClientTwo.LastSynchronizedRevision);

		}

		[Test]
		public void TwoWaySync_Deletion ()
		{
			// initial sync
			FirstSyncForBothSidesTest ();
			
			// sync with second client
			new SyncManager (syncClientTwo, syncServer).DoSync ();

			// delete a note on the first client
			var deleted_note = clientEngineOne.GetNotes ().Values.First ();
			clientEngineOne.DeleteNote (deleted_note);
			clientManifestOne.NoteDeletions.Add (deleted_note.Guid, deleted_note.Title);

			// delete a note on the second client
			// which is not the same note as deleted by the first client
			var second_deleted_note = clientEngineTwo.GetNotes ().Values.First (n => deleted_note != n);
			clientEngineTwo.DeleteNote (second_deleted_note);
			clientManifestTwo.NoteDeletions.Add (second_deleted_note.Guid, second_deleted_note.Title);

			// re-read all notes from disk
			ClearClientOne (reset: false);
			ClearClientTwo (reset: false);
			ClearServer (reset: false);

			// sync the first client again
			new SyncManager (syncClientOne, syncServer).DoSync ();

			// server should now hold two notes (because we deleted one)
			var server_notes = syncServer.GetAllNotes (true);
			Assert.AreEqual (2, server_notes.Count);
			// first client should hold two notes
			Assert.AreEqual (2, clientEngineOne.GetNotes ().Count);

			// those two notes should be the same as on the server
			var client_one_notes = clientEngineOne.GetNotes ().Values;
			var intersection_one = server_notes.Intersect (client_one_notes);
			Assert.AreEqual (2, intersection_one.Count ());

			// second client should hold two notes, too
			var client_two_notes = clientEngineTwo.GetNotes ().Values;
			Assert.AreEqual (2, client_two_notes.Count);

			var intersection_two = server_notes.Intersect (client_two_notes);
			// and one should equal to a clientOne note (only one because we deleted a note on client two that wa
			Assert.AreEqual (1, intersection_two.Count ());

			// now sync the second client again
			ClearServer (reset: false);
			ClearClientTwo (reset: false);
			new SyncManager (syncClientTwo, syncServer).DoSync ();

			server_notes = syncServer.GetAllNotes (true);

			// the second client should have deleted one note because the server
			// wanted so, and now has a total of 1 note
			Assert.AreEqual (1, syncClientTwo.DeletedNotes.Count);
			Assert.AreEqual (1, syncClientTwo.Engine.GetNotes ().Count);
	
			// check that the server does not hold any of the deleted notes
			Assert.AreNotEqual (server_notes.First (), deleted_note);
			Assert.AreNotEqual (server_notes.First (), second_deleted_note);
			
			// the server should also hold only one note
			Assert.AreEqual (1, server_notes.Count);
			// which should be the same as the note on client2
			Assert.AreEqual (server_notes.First (), clientEngineTwo.GetNotes ().Values.First ());
		}
		
		[Test]
		public void TwoWayClientDeletedNoteHasUpdatesOnServer ()
		{
			FirstSyncForBothSidesTest ();
			
			// sync second client
			new SyncManager (syncClientTwo, syncServer).DoSync ();
			
			// reset the server storage
			// delete a note on the first client
			//var note_of_interest = clientEngineOne.GetNotes.First ();

		}
	}
}
