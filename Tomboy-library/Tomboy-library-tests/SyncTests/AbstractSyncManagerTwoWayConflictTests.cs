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
using NUnit.Framework;
using System.Linq;
using Tomboy.Sync;

namespace Tomboy.Sync
{
	public partial class AbstractSyncManagerTests
	{
		// TODO 
		// conflict resolution requires more work (think about GUI interaction)
		// implement this and re-enable those unit tests

		[Ignore]
		[Test]
		public void TwoWayConflictTitleAlreadyExists ()
		{
			// provoke a conflict by creating two notes with the same title
			// on two sync client, and then sync afterwards
			clientEngineOne.SaveNote (NoteCreator.NewNote ("Conflict Title", "this note originates from clientOne"));
			clientEngineTwo.SaveNote (NoteCreator.NewNote ("Conflict Title", "this note originates from clientTwo"));

			// sync clientOne with server
			FirstSyncForBothSidesTest ();

			var server_notes = syncServer.GetAllNotes (true);
			Assert.AreEqual (4, server_notes.Count);

			// sync clientTwo with server
			new SyncManager (syncClientTwo, syncServer).DoSync ();

			// TODO Assert the right exception is thrown / have conflict resolution in place
		}

		[Ignore]
		[Test]
		public void TwoWayConflictLocalAndRemoteChanges ()
		{
			// initial sync
			FirstSyncForBothSidesTest ();
			
			// sync with second client
			new SyncManager (syncClientTwo, syncServer).DoSync ();
			
			// modify a note on the first client
			var modified_note = clientEngineOne.GetNotes ().First ().Value;
			modified_note.Text = "This note has changed.";
			clientEngineOne.SaveNote (modified_note);

			ClearClientOne (reset: false);
			ClearClientTwo (reset: false);
			
			// modify the same note on the second client
			var second_modified_note = clientEngineTwo.GetNotes ().Values.Where (n => n == modified_note).First ();
			
			// make sure we do not have the same reference
			Assert.IsFalse (ReferenceEquals (second_modified_note, modified_note));
			
			second_modified_note.Text = "This note changed on the second client, too!";
			clientEngineTwo.SaveNote(second_modified_note);
			
			// sync the first client again
			// this should go well
			ClearServer (reset: false);
			ClearClientOne (reset: false);
			ClearClientTwo (reset: false);
			
			new SyncManager (syncClientOne, syncServer).DoSync ();
		
			var server_modified_note = syncServer.GetAllNotes (true)
				.First (n => n == modified_note);
			
			// check that the note got updated
			Assert.AreEqual ("This note has changed.", server_modified_note.Text);

			var server_notes = syncServer.GetAllNotes (true);
			var client_one_notes = clientEngineOne.GetNotes ().Values;
			var client_two_notes = clientEngineTwo.GetNotes ().Values;
			
			// now sync the second client again
			// there should now be a note conflict!
			ClearServer (reset: false);
			ClearClientTwo (reset: false);

			server_notes = syncServer.GetAllNotes (true);
			client_one_notes = clientEngineOne.GetNotes ().Values;
			client_two_notes = clientEngineTwo.GetNotes ().Values;

			var c1_rev = clientManifestOne.LastSyncRevision;
			var c2_rev = clientManifestTwo.LastSyncRevision;

			new SyncManager (syncClientTwo, syncServer).DoSync ();
		}
	}
}