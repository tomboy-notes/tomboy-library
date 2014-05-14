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
	
}
