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
using System.Collections.Generic;
using Tomboy;
using Tomboy.Sync;
using System.Linq;
using NUnit.Framework;
using System.IO;

namespace Tomboy.Sync.Filesystem
{
	[TestFixture]
	public class SyncingTests
	{
		private ISyncServer syncServer;
		private ISyncClient syncClient;
		private ISyncClient secondSyncClient;

		private IStorage serverStorage;
		private SyncManifest serverManifest;

		private IStorage clientStorage;
		private SyncManifest clientManifest;

		private IStorage secondClientStorage;
		private SyncManifest secondClientManifest;

		private string serverStorageDir;
		private string clientStorageDir;
		private string secondClientStorageDir;

		[SetUp]
		public void SetUp ()
		{
			var current_dir = Directory.GetCurrentDirectory ();
			serverStorageDir = Path.Combine (current_dir, "../../syncserver/");
			clientStorageDir = Path.Combine (current_dir, "../../syncclient/");
			secondClientStorageDir = Path.Combine (current_dir, "../../syncclient_two/");

			// make sure we start from empty data store directories
			CleanupClientDirectory ();
			CleanupServerDirectory ();
			CleanupSecondClientDirectory ();

			// setup a sample server
			serverStorage = new DiskStorage ();
			serverStorage.SetPath (serverStorageDir);
			serverManifest = new SyncManifest ();

			// setup a sample client
			clientStorage = new DiskStorage ();
			clientStorage.SetPath (clientStorageDir);
			clientManifest = new SyncManifest ();

			// create a third client that synchronizes
			secondClientManifest = new SyncManifest ();
			secondClientStorage = new DiskStorage ();
			secondClientStorage.SetPath (secondClientStorageDir);
			secondSyncClient = new FilesystemSyncClient (secondClientStorage, secondClientManifest);

			// add some notes to the store
			clientStorage.SaveNote (new Note () {
				Text = "This is some sample note text.",
				Title = "Sample Note 1",
			});
			clientStorage.SaveNote (new Note () {
				Text = "This is some sample note text.",
				Title = "Sample Note 2",
			});
			clientStorage.SaveNote (new Note () {
				Text = "This is some sample note text.",
				Title = "Sample Note 3",
			});

			syncServer = new FilesystemSyncServer (serverStorage, serverManifest);
			syncClient = new FilesystemSyncClient (clientStorage, clientManifest);

		}
		[TearDown]
		public void TearDown ()
		{
			CleanupClientDirectory ();
			CleanupSecondClientDirectory ();
			CleanupServerDirectory ();
		}
		private void CleanupServerDirectory ()
		{
			// delete the test storage
			if (Directory.Exists (serverStorageDir))
				Directory.Delete (serverStorageDir, true);
		}
		private void CleanupClientDirectory ()
		{
			if (Directory.Exists (clientStorageDir))
				Directory.Delete (clientStorageDir, true);
		}
		private void CleanupSecondClientDirectory ()
		{
			if (Directory.Exists (secondClientStorageDir))
				Directory.Delete (secondClientStorageDir, true);
		}

		[Test]
		public void FirstSyncForBothSides ()
		{
			SyncManager sync_manager = new SyncManager (this.syncClient, this.syncServer);

			// before the sync, the client should have an empty AssociatedServerId
			Assert.That (string.IsNullOrEmpty (syncClient.AssociatedServerId));
			Assert.That (string.IsNullOrEmpty (clientManifest.ServerId));

			sync_manager.DoSync ();

			// afterwards, the server storage should consist of three notes
			Assert.AreEqual (3, serverStorage.GetNotes ().Count);

			var local_notes = clientStorage.GetNotes ();
			var server_notes = serverStorage.GetNotes ();
			foreach (var kvp in local_notes) {
				Assert.Contains (kvp.Key, server_notes.Keys);
			}

			// after the sync the client should carry the associated ServerId
			Assert.That (!string.IsNullOrEmpty (syncClient.AssociatedServerId));
			Assert.AreEqual (syncClient.AssociatedServerId, syncServer.Id);

			Assert.AreEqual (clientManifest.ServerId, serverManifest.ServerId);
			Assert.That (!string.IsNullOrEmpty (clientManifest.ServerId));

			// both revisions should be 0
			Assert.AreEqual (0, syncClient.LastSynchronizedRevision);
			Assert.AreEqual (0, clientManifest.LastSyncRevision);

			Assert.AreEqual (0, syncServer.LatestRevision);
			Assert.AreEqual (0, serverManifest.LastSyncRevision);
		}
		[Test]
		public void NoteDatesAfterSync ()
		{
			FirstSyncForBothSides ();

			// now make sure, the metadata change date is smaller or equal to the last
			// sync date
			foreach (var note in clientStorage.GetNotes ().Values) {
				Assert.LessOrEqual (note.MetadataChangeDate, clientManifest.LastSyncDate);
			}
		}

		[Test]
		public void ClientSyncsToNewServer()
		{
			// perform initial sync for both ends
			FirstSyncForBothSides ();

			// now switch the client to a new, empty server
			CleanupServerDirectory ();
			serverStorage = new DiskStorage ();
			serverStorage.SetPath (serverStorageDir);

			serverManifest = new SyncManifest ();
			syncServer = new FilesystemSyncServer (serverStorage, serverManifest);

			var sync_manager = new SyncManager (syncClient, syncServer);

			sync_manager.DoSync ();

			// three notes should have been uploaded
			Assert.AreEqual (3, syncServer.UploadedNotes.Count);

			// zero notes should have been deleted from Server
			Assert.AreEqual (0, syncServer.DeletedServerNotes.Count);

			// zero notes should have been deleted from client
			Assert.AreEqual (0, syncClient.DeletedNotes.Count);

			// make sure the client and the server notes are equal
			var local_notes = clientStorage.GetNotes ();
			var server_notes = serverStorage.GetNotes ();
			foreach (var kvp in local_notes) {
				Assert.Contains (kvp.Key, server_notes.Keys);
			}

			// after the sync the client should carry the associated ServerId
			// from the new server
			Assert.That (!string.IsNullOrEmpty (syncClient.AssociatedServerId));
			Assert.AreEqual (syncClient.AssociatedServerId, syncServer.Id);
			
			Assert.AreEqual (clientManifest.ServerId, serverManifest.ServerId);
		}

		[Test]
		public void ClientDeletesNotesAfterFirstSync ()
		{
			// perform initial sync
			FirstSyncForBothSides ();

			// now, lets delete a note from the client
			var deleted_note = clientStorage.GetNotes ().First ().Value;
			clientStorage.DeleteNote (deleted_note);
			clientManifest.NoteDeletions.Add (deleted_note.Guid, deleted_note.Title);

			// perform a sync again
			var sync_manager = new SyncManager (syncClient, syncServer);
			sync_manager.DoSync ();

			// one note should have been deleted on server
			Assert.AreEqual (1, syncServer.DeletedServerNotes.Count);
			Assert.AreEqual (deleted_note, syncServer.DeletedServerNotes.First ());

			// zero notes were deleted on the client
			Assert.AreEqual (0, syncClient.DeletedNotes.Count);

			// all notes on the client and the server should be equal
			var local_notes = clientStorage.GetNotes ();
			var server_notes = serverStorage.GetNotes ();
			foreach (var kvp in local_notes) {
				Assert.Contains (kvp.Key, server_notes.Keys);
			}
		}

		[Test]
		public void NoSyncingNeededIfNoChangesAreMade ()
		{
			// perform initial sync
			FirstSyncForBothSides ();

			var server_id = syncClient.AssociatedServerId;

			// new instance of the server needed (to simulate a new connection)
			syncServer = new FilesystemSyncServer (serverStorage, serverManifest);


			// now that we are synced, there should not happen anything when syncing again
			SyncManager sync_manager = new SyncManager (syncClient, syncServer);
			sync_manager.DoSync ();

			// the association id should not have changed
			Assert.AreEqual (server_id, syncClient.AssociatedServerId);
			Assert.AreEqual (server_id, syncServer.Id);

			// no notes should have been transfered or deleted

			Assert.AreEqual (0, syncServer.UploadedNotes.Count);
			Assert.AreEqual (0, syncServer.DeletedServerNotes.Count);
		}

		[Test]
		public void TwoWaySyncBasic ()
		{
			// initial sync
			FirstSyncForBothSides ();

			Assert.That (string.IsNullOrEmpty (secondSyncClient.AssociatedServerId));

			// sync with another client
			var sync_manager = new SyncManager (secondSyncClient, syncServer);
			sync_manager.DoSync ();

			// the client should be on the same level as the server
			// TODO is it really correct that a sync between an empty client and the server
			// advances the LatestSyncRevision on the server? If not, this should be 0 here
			Assert.AreEqual (1, secondSyncClient.LastSynchronizedRevision);

			Assert.AreEqual (3, secondClientStorage.GetNotes ().Count);

			// notes should be equal to the first client
			var client1_notes = clientStorage.GetNotes ();
			var client2_notes = secondClientStorage.GetNotes ();

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
			new SyncManager (secondSyncClient, syncServer).DoSync ();

			// delete a note on the first client
			var deleted_note = clientStorage.GetNotes ().First ().Value;
			clientStorage.DeleteNote (deleted_note);
			clientManifest.NoteDeletions.Add (deleted_note.Guid, deleted_note.Title);

			// delete another note on the second cient
			var second_deleted_note = clientStorage.GetNotes ().Last ().Value;
			secondClientStorage.DeleteNote (second_deleted_note);
			secondClientManifest.NoteDeletions.Add (second_deleted_note.Guid, second_deleted_note.Title);

			// sync the first client again
			syncServer = new FilesystemSyncServer (serverStorage, serverManifest);
			new SyncManager (syncClient, syncServer).DoSync ();

			// server should now hold two notes (because we deleted one)
			// first client should hold two notes
			// second client should hold two notes

			// now sync the second client again
			syncServer = new FilesystemSyncServer (serverStorage, serverManifest);
			new SyncManager (secondSyncClient, syncServer).DoSync ();

			// the second client should have deleted one note because the server
			// wanted so
			Assert.AreEqual (1, secondSyncClient.DeletedNotes.Count);

			// the server should now only have one note
			Assert.AreEqual (1, serverStorage.GetNotes ().Count);

			// check that the server does not hold any of the deleted notes
			Assert.AreNotEqual (serverStorage.GetNotes ().Values.First (), deleted_note);
			Assert.AreNotEqual (serverStorage.GetNotes ().Values.First (), second_deleted_note);

			// the second client should also hold only one note
			Assert.AreEqual (1, serverStorage.GetNotes ().Count);
			// which should be the same as the note on client2
			Assert.AreEqual (serverStorage.GetNotes ().Values.First (), secondClientStorage.GetNotes ().Values.First ());
		}

		[Test]
		public void TwoWaySyncConflict ()
		{
			// initial sync
			FirstSyncForBothSides ();

			// sync with second client
			new SyncManager (secondSyncClient, syncServer).DoSync ();

			// mofiy a note on the first client
			var modified_note = clientStorage.GetNotes ().First ().Value;
			modified_note.Text = "This note has changed.";
			// we need to modified the metadata change date so the note will be
			// selected for uploading
			modified_note.MetadataChangeDate = DateTime.Now;
			clientStorage.SaveNote (modified_note);

			// modify the same note on the second client
			var second_modified_note = clientStorage.GetNotes ().Values.Where (n => n.Guid == modified_note.Guid).First ();
			second_modified_note.Text = "This note changed on the second client, too!";
			second_modified_note.MetadataChangeDate = DateTime.Now;
			modified_note.ChangeDate = DateTime.Now;
			secondClientStorage.SaveNote(second_modified_note);

			// sync the first client again
			// this should go well
			syncServer = new FilesystemSyncServer (serverStorage, serverManifest);
			new SyncManager (syncClient, syncServer).DoSync ();

			var server_modified_note = serverStorage.GetNotes ().Values
				.Where (n => n.Guid == modified_note.Guid)
				.First ();

			// check that the note got updated
			Assert.AreEqual ("This note has changed.", server_modified_note.Text);

			// now sync the second client again
			// there should now be a note conflict!
			syncServer = new FilesystemSyncServer (serverStorage, serverManifest);
			new SyncManager (secondSyncClient, syncServer).DoSync ();

			// TODO there is no conflict resoltuion implemented right now
			// we should check if the event of conflict resolution got fired here
			// until it is implemented, make this test fail
			Assert.Fail ();
		}
	}
}
