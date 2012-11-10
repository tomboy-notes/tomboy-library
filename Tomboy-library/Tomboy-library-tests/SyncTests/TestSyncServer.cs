//
//  TestSyncServer.cs
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

namespace Tomboylibrarytests
{
	public class TestSyncServer : ISyncServer
	{
		private IStorage storage;
		public SyncManifest manifest;
		public TestSyncServer (IStorage storage, SyncManifest manifest)
		{
			this.storage = storage;
			this.manifest = manifest;
			this.Id = Guid.NewGuid ().ToString ();
		}

		#region ISyncServer implementation
		public IList<Note> GetAllNotes (bool includeNoteContent)
		{
			var notes = storage.GetNotes ().Select (kvp => kvp.Value).ToList ();
			if (!includeNoteContent) {
				notes.ForEach (note => note.Text = "");
			}
			return notes;
		}

		public IList<Note> GetNoteUpdatesSince (int revision)
		{
			var allNotes = GetAllNotes (true);
			var changedNotes = from Note note in allNotes
				where (manifest.NoteRevisions.ContainsKey (note.Guid)
				&& manifest.NoteRevisions [note.Guid] > revision)
				select note;
			return changedNotes.ToList ();
		}

		public void DeleteNotes (IList<Note> deleteNotes)
		{
			deleteNotes.ToList ().ForEach (note => {
				storage.DeleteNote (note);
				this.DeletedServerNotes.Add (note);
			});
		}

		public void UploadNotes (IList<Note> notes)
		{
			notes.ToList ().ForEach (note => {
				storage.SaveNote (note);
			});
		}

		public bool UpdatesAvailableSince (int revision)
		{
			return GetNoteUpdatesSince (revision).Count > 0;
		}

		public IList<Note> DeletedServerNotes {
			get; private set;
		}

		public int LatestRevision {
			get {
				return manifest.LastSyncRevision;
			}
		}

		public string Id {
			get; private set;
		}

		#endregion
	}

	[TestFixture]
	public class SyncServerTests
	{
		private TestSyncServer syncServer;
		private TomboySyncClient syncClient;
		private IStorage serverStorage;
		private SyncManifest serverManifest;
		private IStorage clientStorage;

		[SetUp]
		public void SetUp ()
		{
			// setup a sample server
			serverStorage = new DiskStorage ();
			serverStorage.SetPath ("../../syncserver/");
			serverManifest = new SyncManifest ();

			// setup a sample client
			clientStorage = new DiskStorage ();
			clientStorage.SetPath ("../../syncclient/");

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

			syncServer = new TestSyncServer (serverStorage, serverManifest);
			syncClient = new TomboySyncClient ("../../syncclient/", clientStorage);

		}
		[TearDown]
		private void TearDown ()
		{
			return;
			// delete the test storage
			if (Directory.Exists ("../../syncserver/"))
				Directory.Delete ("../../syncserver");
			if (Directory.Exists ("../../syncclient/"))
				Directory.Delete ("../../syncclient");
		}

		[Test]
		public void FirstSyncForBothSides ()
		{
			SyncManager syncManager = new SyncManager (this.syncClient, this.syncServer);
			syncManager.DoSync ();
		}
	}
}

