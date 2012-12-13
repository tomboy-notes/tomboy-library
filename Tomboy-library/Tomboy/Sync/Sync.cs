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
		private IList<Note> serverNotesMetadata;

		public SyncManager (ISyncClient client, ISyncServer server)
		{
			this.client = client;
			this.server = server;
		}

		private bool LocalNoteIsNewOrChangedSinceLastSync (Note local_note)
		{
			if (client.LastSynchronizedRevision == -1)
				// this is the first sync, there cannot be local changes
				return true;

			if (local_note.MetadataChangeDate > client.LastSyncDate)
				// note was changed since the last sync
				return true;

			// in other cases, the note is not considered new or changed
			return false;
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
			var notes_to_save = new List<Note> ();
			foreach (Note note in clientNotes) {
				// important: use the notes exists locally, since affected_notes
				// may hold notes not yet imported
				if (affected_notes.Contains (note)) {
					notes_to_save.Add (note);
				}
			}
			foreach (var client_note in notes_to_save)
				client.Engine.SaveNote (client_note, false);
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

			bool clientHasNoteChangesSinceLastSync = FindLocallyModifiedNotes ().Count > 0;

			if (clientHasNoteChangesSinceLastSync || clientWantsNotesDeleted)
				return true;

			else if (revisionsAreEqual && clientSyncedBefore)
				return false;

			// all other cases we need syncing
			return true;
		}

		private void UpdateLocalNotesNewOrModifiedByServer (List<Note> new_or_modified_notes)
		{
			foreach (Note note in new_or_modified_notes) {

				bool note_already_exists_locally = client.Engine.GetNotes().Values.Contains (note);

				bool note_unchanged_since_last_sync = false;
				if (note_already_exists_locally) {
					Note local_note = client.Engine.GetNotes ().Values.Where (n => note.Guid == n.Guid).First ();
					note_unchanged_since_last_sync = local_note.MetadataChangeDate <= client.LastSyncDate;
				}

				if (!note_already_exists_locally) {
					// note does not exist on the client, import it
					client.Engine.SaveNote (note, false);
				} else if (note_unchanged_since_last_sync) {
					// note has not been changed locally since the last  sync,
					// but is newer on the server - so update it with the server version
					client.Engine.SaveNote (note, false);
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
				bool note_has_changes = LocalNoteIsNewOrChangedSinceLastSync (note);
				bool server_wants_note_deleted = !server_notes.Contains (note);

				// we don't delete a new note at this point, since we are about to upload
				// it later in the sync process to the server
				if (server_wants_note_deleted && !note_has_changes) {
					client.Engine.DeleteNote (note);
					client.DeletedNotes.Add (note);
				}
			}
		}

		/// <summary>
		/// Deletes the notes on the server which are not present on the the client.
		/// </summary>
		private void DeleteServerNotesNotPresentOnClient (IList<Note> server_notes)
		{
			// the client has a list of note guids that the client deleted
			// since the last sync. We just delete all notes in that list.
			server.DeleteNotes (client.NotesForDeletion.Keys.ToList ());
		}

		private List<Note> FindLocallyModifiedNotes ()
		{
			List<Note> new_or_modified_notes = new List<Note> ();
			foreach (Note note in clientNotes) {

				bool note_has_changes = LocalNoteIsNewOrChangedSinceLastSync (note);

				// remember: we dont increase a note revision in the manifest
				// if we edit the note locally - we can only use the 
				// metadata change date to find out what notes got modified

				// TODO OLD CODE FROM TOMBOY
				// this doesn't seem right, as there will be never this situation
				// client.GetRevision (note) <= client.LastSynchronizedRevision
				bool local_note_changed_since_last_sync =
					 note.MetadataChangeDate > client.LastSyncDate;

				if (note_has_changes || local_note_changed_since_last_sync) {
					new_or_modified_notes.Add (note);
				}
			}
			return new_or_modified_notes;
		}

		private void UpdateClientNotesWithServerVersion (IList<Note> server_updated_notes)
		{
			// assertions: server_updated_notes consist of notes, that have all higher revision
			// than our local notes. So every note should be saved and replace local notes.
			// Exception: If a local note was changed since last sync, or a note with the same 
			// title (but not Guid) exists.

			// do not directly save the notes, as this would
			// modifiy our clientNotes array, too (this is due to engine internals)
			// instead mark the notes for saving and save at the end of the function
			var notes_marked_for_saving_on_client = new List<Note> ();

			foreach (var new_or_updated_note in server_updated_notes) {

				// two different notes (= have two different Guids) are not allowed
				// to have the same note title => TODO conflict resolution
				bool title_already_exists = (from Note client_note in clientNotes
				                             where client_note.Title == new_or_updated_note.Title
				                             && client_note != new_or_updated_note
				                             select client_note).Count () > 0;
				if (title_already_exists) {
					// TODO conflict resolutions
					throw new NotImplementedException ("TODO conflict resolution: title already exists");
				}

				bool first_time_sync = client.LastSynchronizedRevision == -1;
				bool note_has_changes = LocalNoteIsNewOrChangedSinceLastSync (new_or_updated_note); 
				if (note_has_changes && !first_time_sync) {
					// fatal: the note has changes on the server (so was changed by another client)
					// AND has local changes. Either way, we lose data.
					// TODO conflict resolution
					throw new NotImplementedException ("TODO conflict resolution: note has changes on client AND server");
				}
				// if conflicts resolved, save the note
				notes_marked_for_saving_on_client.Add (new_or_updated_note);
			}

			// actually perform the import / save
			foreach (var note in notes_marked_for_saving_on_client) {
				client.Engine.SaveNote (note, false);
			}

		}
		private void UploadNewOrModifiedNotesToServer (List<Note> new_or_modified_notes)
		{
			if (new_or_modified_notes.Count > 0) {
				server.UploadNotes (new_or_modified_notes);
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
			this.clientNotes = client.Engine.GetNotes ().Values.ToList ();

			server.BeginSyncTransaction ();

			AssertClientServerRelationship ();

			if (!NeedSyncing ()) {
				server.CancelSyncTransaction ();
				return;
			}

			serverNotesMetadata = server.GetAllNotes (false);

			IList<Note> server_updated_notes = server.GetNoteUpdatesSince (client.LastSynchronizedRevision);
			// make sure all affected notes are saved, as there might be unsaved note windows open
			// with notes that are to be replaced
			AssertClientNotesAreSaved (server_updated_notes);
			// update all notes with changes from the server
			UpdateClientNotesWithServerVersion (server_updated_notes);

			long new_revision = server.LatestRevision + 1;

			// delete notes that are present in the client store but not on the server anymore
			DeleteClientNotesDeletedByServer (serverNotesMetadata);

			// Look through all the notes modified on the client...
			List<Note> new_or_modified_notes = FindLocallyModifiedNotes ();

			// ...and upload the modified notes to the server
			UploadNewOrModifiedNotesToServer (new_or_modified_notes);

			// every server note, that does not exist on the client
			// should be deleted from the server
			DeleteServerNotesNotPresentOnClient (serverNotesMetadata);

			server.CommitSyncTransaction ();

			// set revisions to new state
			client.LastSynchronizedRevision = new_revision;
			client.LastSyncDate = DateTime.UtcNow;
		}
	}
}