//
//  TwoWaySyncTests.cs
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

namespace Tomboy.Sync.Filesystem.Tests
{
	public partial class SyncingTests
	{
		[Test]
		public void TwoWaySyncBasic ()
		{
			// initial sync
			FirstSyncForBothSides ();

			// revision should have increased from -1 to 0
			Assert.AreEqual (0, syncServer.LatestRevision);
			// and all notes on the server should be on revision 0
			foreach (var rev in serverManifest.NoteRevisions.Values)
				Assert.AreEqual (0, rev);
			
			Assert.That (string.IsNullOrEmpty (syncClientTwo.AssociatedServerId));
			
			// sync with another client
			new SyncManager (syncClientTwo, syncServer).DoSync ();

			// the note revisions shouldn't have changed and be still 0
			foreach (var rev in serverManifest.NoteRevisions.Values)
				Assert.AreEqual (0, rev);

			// while the global revision is increased by 1 again
			// TODO - does the server revision really increase if no changes are made on the server?
			Assert.AreEqual (1, syncServer.LatestRevision);
			
			// the second client should be on the same level as the server
			// TODO is it really correct that a sync between an empty client and the server
			// advances the LatestSyncRevision on the server? If not, this should be 0 here
			Assert.AreEqual (1, syncClientTwo.LastSynchronizedRevision);

			// all three participants should have the same server/sync id
			Assert.AreNotEqual (string.Empty, syncServer.Id);
			Assert.AreEqual (syncServer.Id, syncClientOne.AssociatedServerId);
			Assert.AreEqual (syncServer.Id, syncClientTwo.AssociatedServerId);

			Assert.AreEqual (3, clientEngineTwo.GetNotes ().Count);
			
			// notes should be equal to the first client
			var client1_notes = clientEngineOne.GetNotes ();
			var client2_notes = clientEngineTwo.GetNotes ();
	
			foreach (var kvp in client1_notes) {
				Assert.Contains (kvp.Key, client2_notes.Keys);
			}
		}
		[Test]
		public void TwoWaySyncDeletion ()
		{
			// initial sync
			FirstSyncForBothSides ();
			
			// sync with second client
			new SyncManager (syncClientTwo, syncServer).DoSync ();
		
			// delete a note on the first client
			var deleted_note = clientEngineOne.GetNotes ().Values.First ();
			clientEngineOne.DeleteNote (deleted_note);
			clientManifestOne.NoteDeletions.Add (deleted_note.Guid, deleted_note.Title);

			// delete another note on the second client
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
			Assert.AreEqual (2, serverEngine.GetNotes ().Count);
			// first client should hold two notes
			Assert.AreEqual (2, clientEngineOne.GetNotes ().Count);

			// server_notes should be identical to notes on clientOne
			var server_notes = serverEngine.GetNotes ().Values;
			var client_notes = clientEngineOne.GetNotes ().Values;
			var intersection = server_notes.Intersect (client_notes);
			Assert.AreEqual (2, intersection.Count ());

			// second client should hold two notes
			Assert.AreEqual (2, clientEngineTwo.GetNotes ().Count);
			// one of it equals a note on the server
			intersection = clientEngineTwo.GetNotes ().Values.Intersect (serverEngine.GetNotes ().Values);
			Assert.AreEqual (1, intersection.Count ());
			// and one should equal to a clientOne note
			intersection = clientEngineTwo.GetNotes ().Values.Intersect (clientEngineOne.GetNotes ().Values);
			Assert.AreEqual (1, intersection.Count ());

			// now sync the second client again
			ClearServer (reset: false);
			ClearClientTwo (reset: false);
			new SyncManager (syncClientTwo, syncServer).DoSync ();
	
			// the second client should have deleted one note because the server
			// wanted so, and now has a total of 1 note
			Assert.AreEqual (1, syncClientTwo.DeletedNotes.Count);
			Assert.AreEqual (1, syncClientTwo.Engine.GetNotes ().Count);
	
			// check that the server does not hold any of the deleted notes
			Assert.AreNotEqual (serverEngine.GetNotes ().Values.First (), deleted_note);
			Assert.AreNotEqual (serverEngine.GetNotes ().Values.First (), second_deleted_note);
			
			// the server should also hold only one note
			Assert.AreEqual (1, serverEngine.GetNotes ().Count);
			// which should be the same as the note on client2
			Assert.AreEqual (serverEngine.GetNotes ().Values.First (), clientEngineTwo.GetNotes ().Values.First ());
		}
		
		[Test]
		public void TwoWayClientDeletedNoteHasUpdatesOnServer ()
		{
			FirstSyncForBothSides ();
			
			// sync second client
			new SyncManager (syncClientTwo, syncServer).DoSync ();
			
			// reset the server storage
			// delete a note on the first client
			//var note_of_interest = clientEngineOne.GetNotes.First ();
			
			
			
		}
	}
}
