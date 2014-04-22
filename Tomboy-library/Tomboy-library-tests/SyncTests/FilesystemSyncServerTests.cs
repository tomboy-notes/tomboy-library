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
			                      
			serverStorage = new DiskStorage (serverStorageDir);

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

