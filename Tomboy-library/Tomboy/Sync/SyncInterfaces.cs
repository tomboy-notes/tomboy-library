//
//  Copyright (c) ?? - 2012 Sandy Armstrong and others
//  Copyright (c) 2012 Timo Dörr <timo@latecrew.de>
//
//  File contents partially taken from Tomboy source
//	at http://git.gnome.org/browse/tomboy/tree/Tomboy/Synchronization/SyncManager.cs 
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

namespace Tomboy.Sync
{
	public interface ISyncServer
	{
		// TODO we dont have transaction in the DiskStorage yet
		// make those actually usedful
		bool BeginSyncTransaction ();
		bool CommitSyncTransaction ();
		bool CancelSyncTransaction ();

		// gets all notes on the server. implementation may chose to
		// not include the full note body (i.e. when transferred over the
		// network and would be slow). I.e, the REST API 1.0 supports
		// not to get the note content by adding the ?include_note=false
		// parameter. Getting all notes along with their body is NOT
		// necessary in most cases as the
		/// <see cref='GetNoteUpdatesSince'/> can be used to only get
		// those notes, that needs to be updated.
		IList<Note> GetAllNotes (bool include_note_content);

		// get notes that have changed since a specific revision
		// always includes the full note with its content.
		IList<Note> GetNoteUpdatesSince (long revision);

		// perform deletion of the notes
		void DeleteNotes (IList<string> deleteNotesGuids);

		// list of notes that were deleted from the server
		// because the client deleted them earlier. Note that since the notes
		// are deleted, we do not have the note content anymore, and work 
		// only with the note GUIDs.
		//
		// This value should be set in the implementation of DeleteNotes ()
		// and contain the list of notes deleted via DeleteNotes ()
		IList<string> DeletedServerNotes { get; }

		// uploads a list of notes to the server for updating
		void UploadNotes (IList<Note> notes);

		// notes that were uploaded by the client because the client
		// had the newer version of the note
		IList<Note> UploadedNotes { get; }

		// the global sync revision the server is on
		long LatestRevision { get; } // NOTE: Only reliable during a transaction

		DateTime LastSyncDate { get; }

		// get the Id of the sync the client associated with
		// note that multiple clients can refer to the same Id
		string Id { get; }

		// whether there needs synchronisation to be done
		// this can be changed notes, as well as whether notes
		// marked for deletion are available
		bool UpdatesAvailableSince (int revision);
	}

	public interface ISyncClient
	{
		// the undelying IStorage of the client
		Engine Engine { get; }

		// the global revision the client is on
		long LastSynchronizedRevision { get; set; }

		// date of the last sync
		DateTime LastSyncDate { get; set; }

		// notes that should be deleted in the sync, because the client
		// deleted them since last sync
		IDictionary<string, string> NotesForDeletion { get; }

		// notes that have been deleted from the client storage 
		// after the sync, because the server did not have a copy of those
		IList<Note> DeletedNotes { get; }

		// unassociate a client and a server, reseting
		// all sync information between the two
		void Reset ();

		// get the Id of the server that the client is 
		// associated with. Note that multiple clients
		// may refer to the same ServerId, which means
		// that they all sync against the same repository
		string AssociatedServerId { get; set; }
	}
}