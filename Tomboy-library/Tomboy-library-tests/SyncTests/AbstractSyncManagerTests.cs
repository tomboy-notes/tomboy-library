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
using System.IO;
using Tomboy.Sync.Filesystem;
using System.Linq;
using System.Collections.Generic;
using Tomboy.Tags;

namespace Tomboy.Sync
{
	/// <summary>
	/// Abstract class that performs tests of the Tomboy.SyncManager class (and thus tests the full syncing logic). 
	/// We want to test different implementation of SyncManager (i.e. Filesytsem sync, Web sync with Snowy/Rainy).
	/// This class holds tests, that are not dependant on implemenation details. There should be test classes deriving
	/// (see FilesystemSyncManagerTests) and extend/override these testcases and perform implemenatation specifc tests.
	/// </summary>
	public abstract class AbstractSyncManagerTestsBase
	{
		// our scenarios always involve a server, and up to 2 clients
		protected ISyncServer syncServer;
		protected ISyncClient syncClientOne;
		protected ISyncClient syncClientTwo;

		// the clients are always using local disk storage
		// as backend
		protected Engine clientEngineOne;
		protected IStorage clientStorageOne;
		protected SyncManifest clientManifestOne;
		
		protected Engine clientEngineTwo;
		protected IStorage clientStorageTwo;
		protected SyncManifest clientManifestTwo;

		protected string clientStorageDirOne;
		protected string clientStorageDirTwo;

		protected IList<Note> sampleNotes;

		[SetUp]
		public void SetUp ()
		{
			var current_dir = Directory.GetCurrentDirectory ();

			clientStorageDirOne = Path.Combine (current_dir, "../../syncclient_one/");
			clientStorageDirTwo = Path.Combine (current_dir, "../../syncclient_two/");
			
			// make sure we start from empty data store directories
			CleanupClientDirectoryOne ();
			CleanupClientDirectoryTwo ();
			
			InitClientOne ();
			InitClientTwo ();

			CreateSomeSampleNotes (clientEngineOne);

		}

		[TearDown]
		public void TearDown ()
		{
			CleanupClientDirectoryOne ();
			CleanupClientDirectoryTwo ();
		}

		protected virtual void InitClientOne ()
		{
			clientStorageOne = new DiskStorage (clientStorageDirOne);
			clientEngineOne = new Engine (clientStorageOne);
			clientManifestOne = new SyncManifest ();
			syncClientOne = new FilesystemSyncClient (clientEngineOne, clientManifestOne);
		}
		protected virtual void InitClientTwo ()
		{
			clientManifestTwo = new SyncManifest ();
			clientStorageTwo = new DiskStorage (clientStorageDirTwo);
			clientEngineTwo = new Engine (clientStorageTwo);
			syncClientTwo = new FilesystemSyncClient (clientEngineTwo, clientManifestTwo);
		}

		// forces re-readin from disk, and will make sure a client does not hold
		// Notes which are equal by reference as the server when using FilesystemSync
		protected void ClearClientOne (bool reset = false)
		{
			if (reset) {
				clientManifestOne = new SyncManifest ();
				CleanupClientDirectoryOne ();
			}
			
			clientStorageOne = new DiskStorage (clientStorageDirOne);
			clientEngineOne = new Engine (clientStorageOne);
			syncClientOne = new FilesystemSyncClient (clientEngineOne, clientManifestOne);
		}

		protected void ClearClientTwo (bool reset = false)
		{
			if (reset) {
				clientManifestTwo = new SyncManifest ();
				CleanupClientDirectoryTwo ();
			}
			clientStorageTwo = new DiskStorage (clientStorageDirTwo);
			clientEngineTwo = new Engine (clientStorageTwo);
			syncClientTwo = new FilesystemSyncClient (clientEngineTwo, clientManifestTwo);
		}
		protected abstract void ClearServer (bool reset = false);

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

		protected virtual void CreateSomeSampleNotes (Engine engine)
		{
			sampleNotes = new List<Note> ();

			var note1 = new Note () {
				Title = "Sämplé title 1!",
				Text = "** This is the text of Sämple Note 1**",
				CreateDate = DateTime.Now,
				MetadataChangeDate = DateTime.Now,
				ChangeDate = DateTime.Now
			};
			// TODO add system tags
			note1.Tags.Add ("school", new Tag ("school"));
			note1.Tags.Add ("fun", new Tag ("fun"));
			note1.Tags.Add ("shopping", new Tag ("shopping"));
			sampleNotes.Add (note1);
			
			sampleNotes.Add(new Note () {
				Title = "2nd Example",
				Text = "This is the text of the second sample note",
				CreateDate = new DateTime (1984, 04, 14, 4, 32, 0, DateTimeKind.Utc),
				ChangeDate = new DateTime (2012, 04, 14, 4, 32, 0, DateTimeKind.Utc),
				MetadataChangeDate = new DateTime (2012, 12, 12, 12, 12, 12, DateTimeKind.Utc),
			});
			
			// note that DateTime.MinValue is not an allowed timestamp for notes!
			sampleNotes.Add(new Note () {
				Title = "3rd exampel title",
				Text = "Another example note",
				CreateDate = DateTime.MinValue + new TimeSpan (1, 0, 0, 0, 0),
				ChangeDate = DateTime.MinValue + new TimeSpan (1, 0, 0, 0, 0),
				MetadataChangeDate = DateTime.MinValue + new TimeSpan (1, 0, 0, 0, 0)
			});

			// save the notes to the cient engine 
			sampleNotes.ToList ().ForEach(n => engine.SaveNote (n));
		}

		protected void FirstSyncForBothSides ()
		{
			SyncManager sync_manager = new SyncManager (this.syncClientOne, this.syncServer);
			sync_manager.DoSync ();
		}
	}

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
		[Ignore]
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
