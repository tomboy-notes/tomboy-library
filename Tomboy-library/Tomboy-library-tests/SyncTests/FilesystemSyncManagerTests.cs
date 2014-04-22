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

namespace Tomboy.Sync
{

	[TestFixture]
	public partial class FilesystemSyncManagerTests : AbstractSyncManagerTests
	{
		private Engine serverEngine;
		private IStorage serverStorage;
		private SyncManifest serverManifest;

		private string serverStorageDir;

		[SetUp]
		public new void SetUp ()
		{
			var current_dir = Directory.GetCurrentDirectory ();
			serverStorageDir = Path.Combine (current_dir, "../../syncserver/");

			// make sure we start from empty data store directories
			CleanupServerDirectory ();

			InitServer ();

		}
		[TearDown]
		public new void TearDown ()
		{
			CleanupServerDirectory ();
		}

		private void InitServer ()
		{
			serverStorage = new DiskStorage (serverStorageDir);
			serverEngine = new Engine (serverStorage);
			serverManifest = new SyncManifest ();
			syncServer = new FilesystemSyncServer (serverEngine, serverManifest);
		}

		private void CleanupServerDirectory ()
		{
			// delete the test storage
			if (Directory.Exists (serverStorageDir))
				Directory.Delete (serverStorageDir, true);
		}

		protected override void ClearServer (bool reset = false)
		{
			if (reset) {
				serverManifest = new SyncManifest ();
				CleanupServerDirectory ();
			}
			serverStorage = new DiskStorage (serverStorageDir);
			serverEngine = new Engine (serverStorage);
			syncServer = new FilesystemSyncServer (serverEngine, serverManifest);
		}

		[Test]
		public new void FirstSyncForBothSides ()
		{
			Assert.That (string.IsNullOrEmpty (clientManifestOne.ServerId));

			base.FirstSyncForBothSidesTest ();

			var local_notes = clientEngineOne.GetNotes ().Values;
			var server_notes = serverEngine.GetNotes ().Values;

			// make sure each local note exists on the server
			foreach (var note in local_notes) {
				Assert.That (server_notes.Contains (note));
			}

			// manifest Ids should be equal and not empty
			Assert.AreEqual (clientManifestOne.ServerId, serverManifest.ServerId);
			Assert.That (!string.IsNullOrEmpty (clientManifestOne.ServerId));

			// manifest revisions should be 0
			Assert.AreEqual (0, clientManifestOne.LastSyncRevision);
			Assert.AreEqual (0, serverManifest.LastSyncRevision);

		}

		[Test]
		public new void NoteDatesAfterSync ()
		{
			base.NoteDatesAfterSync ();

			// make sure the server engine dates match the note dates
			var stored_notes = clientEngineOne.GetNotes ().Values;
			var server_stored_notes = serverEngine.GetNotes ().Values;

			foreach (var note in stored_notes) {

				var server_stored_note = server_stored_notes.Single (n => n.Guid == note.Guid);
				Assert.AreEqual (note.ChangeDate, server_stored_note.ChangeDate);
				Assert.AreEqual (note.MetadataChangeDate, server_stored_note.MetadataChangeDate);
				Assert.AreEqual (note.CreateDate, server_stored_note.CreateDate);

			}
		}

		[Test]
		public new void MakeSureTextIsSynced ()
		{
			base.MakeSureTextIsSynced ();

			// re-read server notes from disk
			ClearServer (reset: false);

			// make sure the written-out notes carries the text, too
			var server_notes = serverEngine.GetNotes ().Values;
			foreach (var note in clientEngineOne.GetNotes ().Values) {
				var note_on_server = server_notes.First (n => n == note);
				Assert.AreEqual (note_on_server.Text, note.Text);
			}
		}

		[Test]
		public new void ClientSyncsToNewServer()
		{
			base.ClientSyncsToNewServer ();

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
		public new void ClientDeletesNotesAfterFirstSync ()
		{
			base.ClientDeletesNotesAfterFirstSync ();

			//  server engine now holds a total of two notes
			Assert.AreEqual (2, serverEngine.GetNotes ().Count);

			// all notes on the client and the server should be equal
			var client_notes = clientEngineOne.GetNotes ().Values;
			var server_notes = serverEngine.GetNotes ().Values;
			var intersection = client_notes.Intersect (server_notes);
			Assert.AreEqual (2, intersection.Count ());
		}

		[Test]
		public new void TwoWaySyncFetchOnlyRevisions ()
		{
			base.TwoWaySyncFetchOnlyRevisions ();

			Assert.AreEqual (1, serverManifest.LastSyncRevision);

			foreach (var rev in serverManifest.NoteRevisions.Values)
				Assert.LessOrEqual (rev, 1);

		}
	}
}
