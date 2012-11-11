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
using System.Linq;

namespace Tomboy.Sync
{
	// TODO maybe have this a static class?
	public class SyncManager
	{
		private ISyncClient client;
		private ISyncServer server;
		private IList<Note> clientNotes;

		public SyncManager (ISyncClient client, ISyncServer server)
		{
			this.client = client;
			this.server = server;
			this.clientNotes = client.Storage.GetNotes ().Select (kvp => kvp.Value).ToList ();
		}

		/// <summary>
		/// If the server has been wiped or reinitialized by another client
		/// for some reason, our local manifest is inaccurate and could misguide
		/// sync into erroneously deleting local notes, etc.  We reset the client
		/// to prevent this situation.
		/// </summary>
		private void AssertClientServerRelationship ()
		{
			string serverId = server.Id;
			if (client.AssociatedServerId != serverId) {
				client.Reset ();
				client.AssociatedServerId = serverId;
			}
		}

		/// <summary>
		/// Asserts the client notes are saved. This will force any open note, that has
		/// unsaved changes, to be saved before the sync process.
		/// TODO: The user might want to be notified before and decide whether to save
		/// or discard changes to a note.
		/// </summary>
		/// <param name='affectedNotes'>
		/// List of notes that are affected by the synchronization process and might change
		/// or be deleted.
		/// </param>
		private void AssertClientNotesAreSaved (IList<Note> affected_notes)
		{
			// all notes that are affected by the sync must be saved before the sync
			// else the user might lose changes of an unsaved note
			foreach (Note note in affected_notes) {
				client.Storage.SaveNote (note);
			}
		}

		/// <summary>
		/// Check if syncing between client and server is necessary after all.
		/// </summary>
		/// <returns>
		/// <c>true</c>, if syncing needs to be done, <c>false</c> otherwise.
		/// </returns>
		private bool NeedSyncing ()
		{
			bool revisionsAreEqual = client.LastSynchronizedRevision == server.LatestRevision;
			bool clientSyncedBefore = client.LastSynchronizedRevision > -1;
			bool clientWantsNotesDeleted = client.NotesForDeletion.Count > 0;

			if (revisionsAreEqual && clientSyncedBefore && !clientWantsNotesDeleted)
				return false;
			else
				return true;
		}

		private void UpdateLocalNotesNewOrModifiedByServer (List<Note> new_or_modified_notes)
		{
			foreach (Note note in new_or_modified_notes) {

				bool note_already_exists_locally = clientNotes.Contains (note);

				bool note_unchanged_since_last_sync = false;
				if (note_already_exists_locally) {
					Note local_note = clientNotes.Where (n => note.Guid == n.Guid).First ();
					note_unchanged_since_last_sync = local_note.MetadataChangeDate <= client.LastSyncDate;
				}

				if (!note_already_exists_locally) {
					// note does not exist on the client, import it
					client.Storage.SaveNote (note);
				} else if (note_unchanged_since_last_sync) {
					// note has not been changed locally since the last  sync,
					// but is newer on the server - so update it with the server version
					client.Storage.SaveNote (note);
				} else {
					// note exists and was modified since the last sync - oops
					// TODO conflict resolution
				}
			}
		}

		/// <summary>
		/// Deletes the client notes locally, which have been deleted by server. If a note
		/// has local changes and the server wants it deleted, we don't delete it.
		/// </summary>
		/// <param name='serverDeletedNotes'>
		/// List of notes that the server has deleted and thus should be deleted
		/// by the client, too.
		/// </param>
		private void DeleteClientNotesDeletedByServer (IList<Note> server_notes)
		{
			foreach (Note note in clientNotes) {
				bool notes_has_local_changes = client.GetRevision (note) > -1;
				bool server_wants_notes_deleted = !server_notes.Contains (note);

				// TODO this makes not much sense ?! if the note has local changes
				// don't delete it!
				if (notes_has_local_changes && server_wants_notes_deleted) {
					client.Storage.DeleteNote (note);
					client.DeletedNotes.Add (note);
				}
			}
			// update our internal clientNotes list since notes might have been deleted now
			// TODO when introducing transactions, is this necessary / right?
			clientNotes = client.Storage.GetNotes ().Values.ToList ();
		}

		/// <summary>
		/// Deletes the notes on the server which are not present on the the client.
		/// </summary>
		private void DeleteServerNotesNotPresentOnClient (IList<Note> server_notes)
		{
			List<Note> server_delete_notes = new List<Note> ();
			
			foreach (Note note in server_notes) {
				if (!clientNotes.Contains (note)) {
					server_delete_notes.Add (note);
				}
			}
			server.DeleteNotes (server_delete_notes);
		}

		private List<Note> FindLocallyModifiedNotes ()
		{
			List<Note> new_or_modified_notes = new List<Note> ();
			foreach (Note note in clientNotes) {

				bool notes_is_new = client.GetRevision (note) == -1;

				// note was already on server, but got modified on the client since
				// last sync
				bool local_note_changed_since_last_sync =
					client.GetRevision (note) <= client.LastSynchronizedRevision
					&& note.MetadataChangeDate > client.LastSyncDate;

				if (notes_is_new || local_note_changed_since_last_sync) {
					new_or_modified_notes.Add (note);
				}
			}
			return new_or_modified_notes;
		}

		private void UploadNewOrModifiedNotesToServer (List<Note> new_or_modified_notes)
		{
			if (new_or_modified_notes.Count > 0) {
				server.UploadNotes (new_or_modified_notes);
			}
		}
		private void SetNotesToNewRevision (List<Note> new_or_modified_notes, int revision)
		{
			foreach (Note note in new_or_modified_notes) {
				client.SetRevision (note, revision);
			}
		}

		/// <summary>
		/// Performs the main syncing process. Here is where the magic happens.
		/// </summary>
		public void DoSync ()
		{
			// perform backups of either storage before doing anything
			//local.Store.Backup ();
			//remote.Store.Backup ();

			AssertClientServerRelationship ();

			if (!NeedSyncing ())
				return;

			server.BeginSyncTransaction ();

			IList<Note> serverUpdatedNotes = server.GetNoteUpdatesSince (client.LastSynchronizedRevision);
			AssertClientNotesAreSaved (serverUpdatedNotes);

			var new_revision = server.LatestRevision + 1;

			// get all notes, but without the content 
			IList<Note> server_notes = server.GetAllNotes (false);

			// TODO transaction begin

			// delete notes that are present in the client store but not on the server anymore
			DeleteClientNotesDeletedByServer (server_notes);

			// Look through all the notes modified on the client...
			List<Note> new_or_modified_notes = FindLocallyModifiedNotes ();

			// ...and upload the modified notes to the server
			UploadNewOrModifiedNotesToServer (new_or_modified_notes);

			// TODO transaction commit

			SetNotesToNewRevision (new_or_modified_notes, new_revision);

			// every server note, that does not exist on the client
			// should be deleted from the server
			DeleteServerNotesNotPresentOnClient (server_notes);

			server.CommitSyncTransaction ();

			// set revisions to new state
			client.LastSynchronizedRevision = new_revision;
			client.LastSyncDate = DateTime.Now;
		}
	}
}