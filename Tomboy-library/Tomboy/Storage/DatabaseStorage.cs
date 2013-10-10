//
//  DatabaseStorage.cs
//
//  Author:
//       Timo Dörr <timo@latecrew.de>
//
//  Copyright (c) 2013 Timo Dörr 
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
using System.Data;
using System.Linq;
using ServiceStack.OrmLite;
using Tomboy;
using Tomboy.Db;
using Tomboy.Sync;
using Tomboy.Sync.Web.DTO;

namespace Rainy.Db
{
	public class DbStorage : IStorage, IDisposable
	{
		public readonly string Username;
		public readonly bool UseHistory;
		protected IDbConnection db;
		protected IDbConnectionFactory connFactory;
		protected SyncManifest Manifest;
		protected IDbTransaction trans;

		public DbStorage (IDbConnectionFactory factory, string username, SyncManifest manifest, bool use_history = true)
		{

			if (factory == null)
				throw new ArgumentNullException ("factory");
			this.connFactory = factory;

			if (string.IsNullOrEmpty (username))
				this.Username = "default";
			else
				this.Username = username;

			this.Manifest = manifest;

			this.UseHistory = use_history;
			db = factory.OpenDbConnection ();

			// start everything as a transaction
			trans = db.BeginTransaction ();

		}
		public virtual Dictionary<string, Note> GetNotes ()
		{
			// TODO remove double copying
			var notes = GetDBNotes ();

			if (notes.Any (n => n.IsEncypted))
				throw new Exception ("Found encrypted note but using non-encryption backend that can't decrypt.");

			var dict = notes.ToDictionary (n => n.Guid, n => n.ToDTONote ().ToTomboyNote ());
			return dict;
		}
		protected List<DBNote> GetDBNotes ()
		{
			var db_notes = db.Select<DBNote> (dbn => dbn.Username == this.Username);

			return db_notes;
		}
		public void SetPath (string path)
		{
			throw new NotImplementedException ();
		}
		public void SetBackupPath (string path)
		{
			throw new NotImplementedException ();
		}
		public virtual void SaveNote (Note note)
		{
			var db_note = note.ToDTONote ().ToDBNote (this.Username);
			SaveDBNote (db_note);
		}
		protected void SaveDBNote (DBNote db_note)
		{
			// archive any previously existing note into its own table
			// TODO: evaluate how we could re-use the same DBNote table, which will save us
			// a select + reinsert operation
//			if (UseHistory) {
//				var old_note = db.FirstOrDefault<DBNote> (n => n.CompoundPrimaryKey == db_note.CompoundPrimaryKey);
//				if (old_note != null) {
//					var archived_note = new DBArchivedNote ().PopulateWith (old_note);
//					// set the last known revision
//
//					if (Manifest.NoteRevisions.Keys.Contains (db_note.Guid)) {
//						archived_note.LastSyncRevision = Manifest.NoteRevisions[db_note.Guid];
//					}
//					db.Insert<DBArchivedNote> (archived_note);
//				}
//			}

			// unforunately, we can't know if that note already exist
			// so we delete any previous incarnations of that note and
			// re-insert
			db.Delete<DBNote> (n => n.CompoundPrimaryKey == db_note.CompoundPrimaryKey);
			db.Insert (db_note);
		}

		public void DeleteNote (Note note)
		{
			var dbNote = note.ToDTONote ().ToDBNote (this.Username);

//			if (UseHistory) {
//				var archived_note = new DBArchivedNote ().PopulateWith(dbNote);
//				if (Manifest.NoteRevisions.ContainsKey (note.Guid)) {
//					archived_note.LastSyncRevision = Manifest.NoteRevisions[note.Guid];
//				}
//				var stored_note = db.FirstOrDefault<DBArchivedNote> (n => n.CompoundPrimaryKey == archived_note.CompoundPrimaryKey);
//				// if that revision already exists, do not store
//				if (stored_note == null)
//					db.Insert<DBArchivedNote> (archived_note);
//			}

			db.Delete<DBNote> (n => n.CompoundPrimaryKey == dbNote.CompoundPrimaryKey);
		}
		public void SetConfigVariable (string key, string value)
		{
			throw new System.NotImplementedException ();
		}
		public string GetConfigVariable (string key)
		{
			throw new System.NotImplementedException ();
		}
		public string Backup ()
		{
			throw new System.NotImplementedException ();
		}

		public void Dispose ()
		{
			trans.Commit ();
			trans.Dispose ();

			db.Dispose ();
		}
	}
}
