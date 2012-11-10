//
//  Sync.cs
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
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Tomboy.Sync
{
	public class SyncManifest
	{
		private DateTime last_sync_date = DateTime.MinValue;
		private int last_sync_rev = -1;
		private string server_id = Guid.NewGuid ().ToString ();
		private IDictionary<string, int> note_revisions = new Dictionary<string, int> ();
		private IDictionary<string, string> note_deletions = new Dictionary<string, string> ();

		public DateTime LastSyncDate {
			get {
				return last_sync_date;
			}
			set {
				last_sync_date = value;
			}
		}

		public int LastSyncRevision {
			get {
				return last_sync_rev;
			}
			set {
				last_sync_rev = value;
			}
		}

		public string ServerId {
			get {
				return server_id;
			}
			set {
				server_id = value;
			}
		}

		public IDictionary<string, int> NoteRevisions {
			get {
				return note_revisions;
			}
		}

		public IDictionary<string, string> NoteDeletions {
			get {
				return note_deletions;
			}
		}

		public SyncManifest ()
		{
		}

		public void Reset ()
		{
			note_revisions = new Dictionary<string, int> ();
			note_deletions = new Dictionary<string, string> ();
			server_id = String.Empty;
			last_sync_date = DateTime.MinValue;
			last_sync_rev = -1;
		}
		#region Xml serialization
		private const string CURRENT_VERSION = "0.3";
		/// <summary>
		/// Write the specified manifest to an XmlWriter.
		/// </summary>
		public static void Write (XmlWriter xml, SyncManifest manifest)
		{
			xml.WriteStartDocument ();
			xml.WriteStartElement (null, "manifest", "http://beatniksoftware.com/tomboy");
			xml.WriteAttributeString (null,
			                         "version",
			                         null,
			                         CURRENT_VERSION);

			if (manifest.LastSyncDate > DateTime.MinValue) {
				xml.WriteStartElement (null, "last-sync-date", null);
				xml.WriteString (
					XmlConvert.ToString (manifest.LastSyncDate, Writer.DATE_TIME_FORMAT));
				xml.WriteEndElement ();
			}

			xml.WriteStartElement (null, "last-sync-rev", null);
			xml.WriteString (manifest.LastSyncRevision.ToString ());
			xml.WriteEndElement ();

			xml.WriteStartElement (null, "server-id", null);
			xml.WriteString (manifest.ServerId);
			xml.WriteEndElement ();

			WriteNoteRevisions (xml, manifest);
			WriteNoteDeletions (xml, manifest);

			xml.WriteEndDocument ();
		}
		private static void WriteNoteRevisions (XmlWriter xml, SyncManifest manifest)
		{
			xml.WriteStartElement (null, "note-revisions", null);
			foreach (var revision in manifest.NoteRevisions) {
				xml.WriteStartElement (null, "note", null);
				xml.WriteAttributeString ("guid", revision.Key);
				xml.WriteAttributeString ("latest-revision", revision.Value.ToString ());
				xml.WriteEndElement ();
			}
			xml.WriteEndElement ();
		}
		private static void WriteNoteDeletions (XmlWriter xml, SyncManifest manifest)
		{
			xml.WriteStartElement (null, "note-deletions", null);
			foreach (var deletion in manifest.NoteDeletions) {
				xml.WriteStartElement (null, "note", null);
				xml.WriteAttributeString ("guid", deletion.Key);
				xml.WriteAttributeString ("title", deletion.Value);
				xml.WriteEndElement ();
			}
			xml.WriteEndElement ();
		}
		public static SyncManifest Read (XmlTextReader xml)
		{
			SyncManifest manifest = new SyncManifest ();
			string version = String.Empty;
			
			try {
				while (xml.Read ()) {
					switch (xml.NodeType) {
					case XmlNodeType.Element:
						switch (xml.Name) {
						case "manifest":
							version = xml.GetAttribute ("version");
							break;
						case "server-id":
							// <text> is just a wrapper around <note-content>
							// NOTE: Use .text here to avoid triggering a save.
							manifest.ServerId = xml.ReadString ();
							break;
						case "last-sync-date":
							DateTime date;
							if (DateTime.TryParse (xml.ReadString (), out date))
								manifest.LastSyncDate = date;
							else
								manifest.LastSyncDate = DateTime.Now;
							break;
						case "last-sync-rev":
							int num;
							if (int.TryParse (xml.ReadString (), out num))
								manifest.LastSyncRevision = num;
							break;
						case "note-revisions":
							xml.ReadToDescendant ("note");
							do {
								var guid = xml.GetAttribute ("guid");
								int rev = int.Parse (xml.GetAttribute ("latest-revision"));
								manifest.NoteRevisions.Add (guid, rev);
							} while (xml.ReadToNextSibling ("note"));
							break;
						case "note-deletions":
							xml.ReadToDescendant ("note");
							do {
								var guid = xml.GetAttribute ("guid");
								string title = xml.GetAttribute ("title");
								manifest.NoteDeletions.Add (guid, title);
							} while (xml.ReadToNextSibling ("note"));
							break;
						}
						break;
	
							
					}
				}
			} catch (XmlException) {
				//TODO: Log the error
				return null;
			}
			return manifest;
		}
		#endregion Xml serialization
	}

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
		private void AssertClientNotesAreSaved (IList<Note> affectedNotes)
		{
			// all notes that are affected by the sync must be saved before the sync
			// else the user might lose changes of an unsaved note
			foreach (Note note in affectedNotes) {
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

		private void UpdateLocalNotesNewOrModifiedByServer (List<Note> newOrModifiedNotes)
		{
			foreach (Note note in newOrModifiedNotes) {
				bool noteAlreadyExistsLocally = clientNotes.Contains (note);

				bool noteUnchangedSinceLastSync = false;
				if (noteAlreadyExistsLocally) {
					Note localNote = clientNotes.Where (n => note.Guid == n.Guid).First ();
					noteUnchangedSinceLastSync = localNote.MetadataChangeDate <= client.LastSyncDate;
				}

				if (!noteAlreadyExistsLocally) {
					// note does not exist on the client, import it
					client.Storage.SaveNote (note);
				} else if (noteUnchangedSinceLastSync) {
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
		/// List of Guid's of notes that the server has deleted and thus should be deleted
		/// by the client, too.
		/// </param>
		private void DeleteClientNotesDeletedByServer (IList<Note> serverNotes)
		{
			foreach (Note note in clientNotes) {
				bool noteHasNoLocalChanges = client.GetRevision (note) > -1;
				bool serverWantsNoteDeleted = !serverNotes.Contains (note);

				if (noteHasNoLocalChanges && serverWantsNoteDeleted) {
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
		private void DeleteServerNotesNotPresentOnClient (IList<Note> serverNotes)
		{
			List<Note> serverDeleteNotes = new List<Note> ();
			
			foreach (Note note in serverNotes) {
				if (!clientNotes.Contains (note)) {
					serverDeleteNotes.Add (note);
				}
			}
			server.DeleteNotes (serverDeleteNotes);
		}

		private List<Note> FindLocallyModifiedNotes ()
		{
			List<Note> newOrModifiedNotes = new List<Note> ();
			foreach (Note note in clientNotes) {

				bool noteIsNewNote = client.GetRevision (note) == -1;

				// note was already on server, but got modified on the client since
				// last sync
				bool localNoteChangedSinceLastSync =
					client.GetRevision (note) <= client.LastSynchronizedRevision
					&& note.MetadataChangeDate > client.LastSyncDate;

				if (noteIsNewNote || localNoteChangedSinceLastSync) {
					newOrModifiedNotes.Add (note);
				}
			}
			return newOrModifiedNotes;
		}

		private void UploadNewOrModifiedNotesToServer (List<Note> newOrModifiedNotes)
		{
			if (newOrModifiedNotes.Count > 0) {
				server.UploadNotes (newOrModifiedNotes);
			}
		}
		private void SetNotesToNewRevision (List<Note> newOrModifiedNotes, int revision)
		{
			foreach (Note note in newOrModifiedNotes) {
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

			var newRevision = server.LatestRevision + 1;

			// get all notes, but without the content 
			IList<Note> serverNotes = server.GetAllNotes (false);

			// TODO transaction begin

			// delete notes that are present in the client store but not on the server anymore
			DeleteClientNotesDeletedByServer (serverNotes);

			// Look through all the notes modified on the client...
			List<Note> newOrModifiedNotes = FindLocallyModifiedNotes ();

			// ...and upload the modified notes to the server
			UploadNewOrModifiedNotesToServer (newOrModifiedNotes);

			// TODO transaction commit

			SetNotesToNewRevision (newOrModifiedNotes, newRevision);

			// every server note, that does not exist on the client
			// should be deleted from the server
			DeleteServerNotesNotPresentOnClient (serverNotes);

			server.CommitSyncTransaction ();

			// set revisions to new state
			client.LastSynchronizedRevision = newRevision;
			client.LastSyncDate = DateTime.Now;
		}
	}
	public class SyncException : Exception
	{
		public SyncException (string message) : base (message)
		{
		}
	}
}