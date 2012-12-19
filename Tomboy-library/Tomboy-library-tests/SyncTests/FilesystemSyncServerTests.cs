using System;
using NUnit.Framework;
using Tomboy;
using Tomboy.Sync.Filesystem;
using System.IO;
using Tomboy.Sync;

namespace Tomboy.Sync
{
	[TestFixture()]
	public class FilesystemSyncServerTests : AbstractSyncServerTests
	{
		private Engine serverEngine;
		private IStorage serverStorage;
		private string serverStorageDir;

		private SyncManifest manifest;

		[SetUp]
		public void SetUp ()
		{
			var current_dir = Directory.GetCurrentDirectory ();
			serverStorageDir = Path.Combine (current_dir, "../../syncserver/");
			                      
			serverStorage = new DiskStorage ();
			serverStorage.SetPath (serverStorageDir);

			serverEngine = new Engine (serverStorage);

			manifest = new SyncManifest ();
			syncServer = new FilesystemSyncServer (serverEngine, manifest);

			CreateSomeSampleNotes ();
		}

		[TearDown]
		public void TearDown ()
		{
			Directory.Delete (serverStorageDir, true);
		}

		// ... tests are derived from base class.
		// only add implementation specific tests tests not covered by base class
	}
}

