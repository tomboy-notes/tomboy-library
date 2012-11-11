//
//  (C) 2012 Timo Dörr <timo@latecrew.de>
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

namespace Tomboy.Sync.Filesystem
{
	/// <summary>
	/// Filesystem sync client. In a classic usecase, where one wants to sync the
	/// Tomboy notes to another directory on the filesystem, Tomboy would play the
	/// part of the FilesystemSyncClient (using its core IStorage).
	/// </summary>
	public class FilesystemSyncClient : ISyncClient
	{
		private SyncManifest manifest;

		/// <summary>
		/// Will create a new sync client, that uses the main Tomboy data storage 
		/// as source. Note that since the main DiskStorage is static, only one
		/// FilesystemSyncClient using static storage may exist at a time, else
		/// expect race canditions.
		/// </summary>
		public FilesystemSyncClient (SyncManifest manifest) : this (DiskStorage.Instance, manifest)
		{
		}

		/// <summary>
		/// Will create a new sync client using a custom IStorage as data backend.
		/// When using different IStorage backend, multiple instances of ISyncClient
		/// are allowed to exist simultaneously.
		/// </summary>
		public FilesystemSyncClient (IStorage storage, SyncManifest manifest)
		{
			this.manifest = manifest;
			this.Storage = storage;
			this.AssociatedServerId = "";
			this.DeletedNotes = new List<Note> ();
		}
		#region ISyncClient implementation
		public int GetRevision (Note note)
		{
			if (manifest.NoteRevisions.ContainsKey (note.Guid))
				return manifest.NoteRevisions [note.Guid];
			else
				return -1;
		}
		public void SetRevision (Note note, int revision)
		{
			manifest.NoteRevisions [note.Guid] = revision;
		}
		public void Reset ()
		{
			this.AssociatedServerId = "";
			// reset the manifest
			this.manifest.Reset ();
		}
		public IStorage Storage {
			get; private set;
		}
		public int LastSynchronizedRevision {
			get {
				return manifest.LastSyncRevision;
			}
			set {
				manifest.LastSyncRevision = value;
			}
		}
		public DateTime LastSyncDate {
			get {
				return manifest.LastSyncDate;
			}
			set {
				manifest.LastSyncDate = value;
			}
		}
		// TODO remove the dictionary and replace with IList<string>
		// the client should be able to retrieve the note title
		// via an older backup / pre-transaction-start copy of the note repo
		public IDictionary<string, string> NotesForDeletion {
			get {
				return manifest.NoteDeletions;
			}
		}
		public IList<Note> DeletedNotes {
			get;
			set;
		}
		public string AssociatedServerId {
			get {
				return manifest.ServerId;
			}
			set {
				manifest.ServerId = value;
			}
		}
		#endregion
	}
}