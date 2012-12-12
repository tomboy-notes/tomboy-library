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

using System.Collections.Generic;
using System.Linq;
using Tomboy;
using Tomboy.Sync;
using System;

namespace Tomboy.Sync.Filesystem
{
	/// <summary>
	/// Filesystem sync server. In a classic usecase where one would like to backup/sync
	/// notes from Tomboy to another folder (i.e. a USB stick), then the FilesystemSyncServer
	/// represents the sync target (i.e. a folder on that particular USB stick).
	/// </summary>
	public class FilesystemSyncServer : ISyncServer
	{
		private Engine engine;
		private SyncManifest manifest;
		private long newRevision;

		public FilesystemSyncServer (Engine engine, SyncManifest manifest)
		{
			this.engine = engine;
			this.manifest = manifest;

			// if not server id is set, set a new one
			if (string.IsNullOrEmpty (this.Id)) {
				this.Id = Guid.NewGuid ().ToString ();
			}
			newRevision = this.LatestRevision + 1;

			this.UploadedNotes = new List<Note> ();
			this.DeletedServerNotes = new List<string> ();
		}

		#region ISyncServer implementation

		public bool BeginSyncTransaction ()
		{
			// TODO
			return true;
		}

		public bool CommitSyncTransaction ()
		{
			// TODO
			this.LatestRevision = this.LatestRevision + 1;

			// required for testing, as we will always reuse the same object instance
			newRevision++;
			return true;
		}

		public bool CancelSyncTransaction ()
		{
			// TODO
			return true;
		}

		public IList<Note> GetAllNotes (bool include_note_content)
		{
			var notes = engine.GetNotes ().Select (kvp => kvp.Value).ToList ();
			if (!include_note_content) {
				notes.ForEach (note => note.Text = "");
			}
			return notes;
		}

		public IList<Note> GetNoteUpdatesSince (long revision)
		{
			var allNotes = GetAllNotes (true);
			var changedNotes = from Note note in allNotes
				where (manifest.NoteRevisions.ContainsKey (note.Guid)
				&& manifest.NoteRevisions [note.Guid] > revision)
				select note;
			return changedNotes.ToList ();
		}

		public void DeleteNotes (IList<string> deleteNotesGuids)
		{
			// select the notes by Guid that we want to delete
			var notes_to_delete = engine.GetNotes ().Values
				.Where (n => deleteNotesGuids.Contains (n.Guid));
			// delete those selected notes from our local store
			notes_to_delete.ToList ().ForEach (note => {
				engine.DeleteNote (note);
				this.DeletedServerNotes.Add (note.Guid);
			});
		}

		public void UploadNotes (IList<Note> notes)
		{
			notes.ToList ().ForEach (note => {
				engine.SaveNote (note);
				UploadedNotes.Add (note);
				// set the note to the new revision
				manifest.NoteRevisions [note.Guid] = newRevision;
			});
		}

		public IList<Note> UploadedNotes { get; private set; }

		public bool UpdatesAvailableSince (int revision)
		{
			return GetNoteUpdatesSince (revision).Count > 0;
		}

		public IList<string> DeletedServerNotes {
			get; private set;
		}

		public long LatestRevision {
			get {
				return manifest.LastSyncRevision;
			}
			private set {
				manifest.LastSyncRevision = value;
			}
		}

		public string Id {
			get {
				return manifest.ServerId;
			}
			private set {
				manifest.ServerId = value;
			}
		}
		#endregion
	}
}