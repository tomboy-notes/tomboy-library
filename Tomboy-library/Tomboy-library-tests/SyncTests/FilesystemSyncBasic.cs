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
using Tomboy.Sync.Filesystem;

namespace Tomboy
{

	[TestFixture]
	public partial class FilesystemSyncTests
	{
		protected ISyncServer syncServer;
		protected ISyncClient syncClientOne;
		protected ISyncClient syncClientTwo;

		private Engine serverEngine;
		private IStorage serverStorage;
		private SyncManifest serverManifest;

		protected Engine clientEngineOne;
		protected IStorage clientStorageOne;
		protected SyncManifest clientManifestOne;

		protected Engine clientEngineTwo;
		protected IStorage clientStorageTwo;
		protected SyncManifest clientManifestTwo;

		private string serverStorageDir;
		protected string clientStorageDirOne;
		protected string clientStorageDirTwo;

		[SetUp]
		public virtual void SetUp ()
		{
			Console.WriteLine ("SETUP BASE! " + this.GetType ());
			var current_dir = Directory.GetCurrentDirectory ();
			serverStorageDir = Path.Combine (current_dir, "../../syncserver/");
			clientStorageDirOne = Path.Combine (current_dir, "../../syncclient_one/");
			clientStorageDirTwo = Path.Combine (current_dir, "../../syncclient_two/");

			// make sure we start from empty data store directories
			CleanupClientDirectoryOne ();
			CleanupServerDirectory ();
			CleanupClientDirectoryTwo ();

			InitClientOne ();
			InitClientTwo ();
			InitServer ();

			CreateSomeSampleNotes (clientEngineOne);

		}
		[TearDown]
		public virtual void TearDown ()
		{
			CleanupClientDirectoryOne ();
			CleanupClientDirectoryTwo ();
			CleanupServerDirectory ();
		}

		protected virtual void InitClientOne ()
		{
			Console.WriteLine ("init BASE");

			clientStorageOne = new DiskStorage ();
			clientStorageOne.SetPath (clientStorageDirOne);
			clientEngineOne = new Engine (clientStorageOne);
			clientManifestOne = new SyncManifest ();
			syncClientOne = new FilesystemSyncClient (clientEngineOne, clientManifestOne);
		}
		protected virtual void InitClientTwo ()
		{
			clientManifestTwo = new SyncManifest ();
			clientStorageTwo = new DiskStorage ();
			clientStorageTwo.SetPath (clientStorageDirTwo);
			clientEngineTwo = new Engine (clientStorageTwo);
			syncClientTwo = new FilesystemSyncClient (clientEngineTwo, clientManifestTwo);
		}
		protected virtual void InitServer ()
		{
			serverStorage = new DiskStorage ();
			serverStorage.SetPath (serverStorageDir);
			serverEngine = new Engine (serverStorage);
			serverManifest = new SyncManifest ();
			syncServer = new FilesystemSyncServer (serverEngine, serverManifest);
		}
		protected virtual void CreateSomeSampleNotes (Engine engine)
		{
			// add some notes to the store
			engine.SaveNote (new Note () {
				Text = "This is some sample note text.",
				Title = "Sample Note 1",
			});
			engine.SaveNote (new Note () {
				Text = "This is some sample note text.",
				Title = "Sample Note 2",
			});
			engine.SaveNote (new Note () {
				Text = "This is some sample note text.",
				Title = "Sample Note 3",
			});
		}

		private void CleanupServerDirectory ()
		{
			// delete the test storage
			if (Directory.Exists (serverStorageDir))
				Directory.Delete (serverStorageDir, true);
		}
		private void CleanupClientDirectoryOne ()
		{
			if (Directory.Exists (clientStorageDirOne))
				Directory.Delete (clientStorageDirOne, true);
		}
		private void CleanupClientDirectoryTwo ()
		{
			if (Directory.Exists (clientStorageDirTwo))
				Directory.Delete (clientStorageDirTwo, true);
		}
		// forces re-readin from disk, and will make sure a client does not hold
		// Notes which are equal by reference as the server
		private void ClearClientOne (bool reset = false)
		{
			if (reset) {
				clientManifestOne = new SyncManifest ();
				CleanupClientDirectoryOne ();
			}

			clientStorageOne = new DiskStorage ();
			clientStorageOne.SetPath (clientStorageDirOne);
			clientEngineOne = new Engine (clientStorageOne);
			syncClientOne = new FilesystemSyncClient (clientEngineOne, clientManifestOne);
		}
		private void ClearClientTwo (bool reset = false)
		{
			if (reset) {
				clientManifestTwo = new SyncManifest ();
				CleanupClientDirectoryTwo ();
			}
			clientStorageTwo = new DiskStorage ();
			clientStorageTwo.SetPath (clientStorageDirTwo);
			clientEngineTwo = new Engine (clientStorageTwo);
			syncClientTwo = new FilesystemSyncClient (clientEngineTwo, clientManifestTwo);
		}
		private void ClearServer (bool reset = false)
		{
			if (reset) {
				serverManifest = new SyncManifest ();
				CleanupServerDirectory ();
			}
			serverStorage = new DiskStorage ();
			serverStorage.SetPath (serverStorageDir);
			serverEngine = new Engine (serverStorage);
			syncServer = new FilesystemSyncServer (serverEngine, serverManifest);
		}

		[Test]
		public virtual void FirstSyncForBothSides ()
		{
			SyncManager sync_manager = new SyncManager (this.syncClientOne, this.syncServer);

			// before the sync, the client should have an empty AssociatedServerId
			Assert.That (string.IsNullOrEmpty (syncClientOne.AssociatedServerId));
			Assert.That (string.IsNullOrEmpty (clientManifestOne.ServerId));

			sync_manager.DoSync ();

			var local_notes = clientEngineOne.GetNotes ().Values;
			var server_notes = serverEngine.GetNotes ().Values;

			// make sure each local note exists on the server
			foreach (var note in local_notes) {
				Assert.That (server_notes.Contains (note));
			}

			// after the sync the client should carry the associated ServerId
			Assert.That (!string.IsNullOrEmpty (syncClientOne.AssociatedServerId));
			Assert.AreEqual (syncClientOne.AssociatedServerId, syncServer.Id);

			Assert.AreEqual (clientManifestOne.ServerId, serverManifest.ServerId);
			Assert.That (!string.IsNullOrEmpty (clientManifestOne.ServerId));

			// both revisions should be 0
			Assert.AreEqual (0, syncClientOne.LastSynchronizedRevision);
			Assert.AreEqual (0, clientManifestOne.LastSyncRevision);

			Assert.AreEqual (0, syncServer.LatestRevision);
			Assert.AreEqual (0, serverManifest.LastSyncRevision);
		}

		[Test]
		public void ServerNoteRevisionsAfterSync ()
		{
			FirstSyncForBothSides ();

			Assert.AreEqual (0, syncClientOne.LastSynchronizedRevision);
			Assert.AreEqual (3, syncServer.UploadedNotes.Count);

			// client revisions are equal to 0
			var server_notes = serverEngine.GetNotes ().Values;
			foreach (var note in server_notes) {
				Assert.AreEqual (0, serverManifest.NoteRevisions[note.Guid]);
			}
		}
		[Test]
		public void NoteDatesAfterSync ()
		{
			FirstSyncForBothSides ();

			// now make sure, the metadata change date is smaller or equal to the last
			// sync date
			foreach (var note in clientEngineOne.GetNotes ().Values) {
				Assert.LessOrEqual (note.MetadataChangeDate, clientManifestOne.LastSyncDate);
			}
		}

		[Test]
		public void MakeSureTextIsSynced ()
		{
			FirstSyncForBothSides ();

			// re-read server notes from disk
			ClearServer (reset: false);

			var server_notes = serverEngine.GetNotes ().Values;
			foreach (var note in clientEngineOne.GetNotes ().Values) {
				var note_on_server = server_notes.First (n => n == note);
				Assert.AreEqual (note_on_server.Text, note.Text);
			}
		}

		[Test]
		public void ClientSyncsToNewServer()
		{
			// perform initial sync for both ends
			FirstSyncForBothSides ();

			// now switch the client to a new, empty server
			ClearServer (reset: true);

			var sync_manager = new SyncManager (syncClientOne, syncServer);

			sync_manager.DoSync ();

			// three notes should have been uploaded
			Assert.AreEqual (3, syncServer.UploadedNotes.Count);

			// zero notes should have been deleted from Server
			Assert.AreEqual (0, syncServer.DeletedServerNotes.Count);

			// zero notes should have been deleted from client
			Assert.AreEqual (0, syncClientOne.DeletedNotes.Count);

			// make sure the client and the server notes are equal
			var local_notes = clientEngineOne.GetNotes ();
			var server_notes = serverEngine.GetNotes ();
			foreach (var kvp in local_notes) {
				Assert.Contains (kvp.Key, server_notes.Keys);
			}

			// after the sync the client should carry the associated ServerId
			// from the new server
			Assert.That (!string.IsNullOrEmpty (syncClientOne.AssociatedServerId));
			Assert.AreEqual (syncClientOne.AssociatedServerId, syncServer.Id);
			
			Assert.AreEqual (clientManifestOne.ServerId, serverManifest.ServerId);
		}

		[Test]
		public void ClientDeletesNotesAfterFirstSync ()
		{
			// perform initial sync
			FirstSyncForBothSides ();

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
			Assert.AreEqual (2, serverEngine.GetNotes ().Count);

			// all notes on the client and the server should be equal
			var client_notes = clientEngineOne.GetNotes ().Values;
			var server_notes = serverEngine.GetNotes ().Values;
			var intersection = client_notes.Intersect (server_notes);
			Assert.AreEqual (2, intersection.Count ());
		}

		[Test]
		public void NoSyncingNeededIfNoChangesAreMade ()
		{
			// perform initial sync
			FirstSyncForBothSides ();

			var server_id = syncClientOne.AssociatedServerId;

			// new instance of the server needed (to simulate a new connection)
			ClearServer (reset:  false);

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

		//[Test]
		public void MassiveAmountOfNotes ()
		{
			// we have some notes added by default, so substract from total amount
			int num_notes = 1024 - clientEngineOne.GetNotes ().Count;

			while (num_notes-- > 0) {
				var note = NoteCreator.NewNote ("Sample note number " + num_notes, "This is a sample note body.");
				clientEngineOne.SaveNote (note);
			}

			// perform first sync
			FirstSyncForBothSides ();

			Assert.AreEqual (1024, clientEngineOne.GetNotes ().Count);
		}
	}
}
