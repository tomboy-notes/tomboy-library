using System;
using NUnit.Framework;
using Tomboy.Sync;
using System.IO;
using Tomboy.Sync.Filesystem;
using System.Linq;
using System.Collections.Generic;

namespace Tomboy.Sync
{
	/// <summary>
	/// Abstract class that performs tests of the Tomboy.SyncManager class (and thus tests the full syncing logic). 
	/// We want to test different implementation of SyncManager (i.e. Filesytsem sync, Web sync with Snowy/Rainy).
	/// This class holds tests, that are not dependant on implemenation details. There should be test classes deriving
	/// (see FilesystemSyncManagerTests) and extend/override these testcases and perform implemenatation specifc tests.
	/// </summary>
	public abstract partial class AbstractSyncManagerTests 
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

		// forces re-readin from disk, and will make sure a client does not hold
		// Notes which are equal by reference as the server when using FilesystemSync
		protected void ClearClientOne (bool reset = false)
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

		protected void ClearClientTwo (bool reset = false)
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
			
			sampleNotes.Add(new Note () {
				Title = "Sämplé title 1!",
				Text = "** This is the text of Sämple Note 1**",
				CreateDate = DateTime.Now,
				MetadataChangeDate = DateTime.Now,
				ChangeDate = DateTime.Now
			});
			
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

		[Test]
		public void FirstSyncForBothSides ()
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
			ClearServer (reset: false);
		}

		[Test]
		public void ClientHasAllNotesAfterFirstSync ()
		{
			FirstSyncForBothSides ();

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

			FirstSyncForBothSides ();

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
			FirstSyncForBothSides ();

			var server_notes = syncServer.GetAllNotes (true);
			foreach (var note in clientEngineOne.GetNotes ().Values) {
				var note_on_server = server_notes.Single (n => n == note);
				Assert.AreEqual (note_on_server.Text, note.Text);
			}
		}

		[Test]
		public void ClientSyncsToNewServer()
		{
			FirstSyncForBothSides ();
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
			FirstSyncForBothSides ();
				
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
			FirstSyncForBothSides ();
			
			Assert.AreEqual (1024, clientEngineOne.GetNotes ().Count);
			Assert.AreEqual (1024, syncServer.UploadedNotes.Count);
		}

		[Test]
		public void GetNoteUpdateSinceTest ()
		{
			FirstSyncForBothSides ();

			var notes = syncServer.GetNoteUpdatesSince (-1);

			var client_notes = clientEngineOne.GetNotes ().Values;
			client_notes.ToList ().ForEach(n=> { Assert.That (notes.Contains (n)); });

			notes = syncServer.GetNoteUpdatesSince (0);
			Assert.AreEqual (0, notes.Count);
		}
	}
}
