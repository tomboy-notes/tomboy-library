using System;
using System.Collections.Generic;

namespace Tomboy.Sync.Web
{
	// TODO this class is most likely not need, we can recycle FileSystemSyncClient instead!
	public class WebSyncClient : ISyncClient
	{
		private SyncManifest manifest;

		/// <summary>
		/// Will create a new sync client, that uses the main Tomboy data storage 
		/// as source. Note that since the main DiskStorage is static, only one
		/// WebSyncClient using static storage may exist at a time, else
		/// expect race canditions.
		/// </summary>
		public WebSyncClient (SyncManifest manifest) : this (new Engine (DiskStorage.Instance), manifest)
		{
		}
		
		/// <summary>
		/// Will create a new sync client using a custom IStorage as data backend.
		/// When using different IStorage backend, multiple instances of WebSyncClient
		/// are allowed to exist simultaneously.
		/// </summary>
		public WebSyncClient (Engine engine, SyncManifest manifest)
		{
			this.manifest = manifest;
			this.Engine = engine;
			
			this.DeletedNotes = new List<Note> ();
			
		}

		#region ISyncClient implementation

		public void Reset ()
		{
			throw new NotImplementedException ();
		}

		public Engine Engine {
			get;
			// TODO remove set
			set;
		}

		public int LastSynchronizedRevision {
			get;
			set;
		}

		public DateTime LastSyncDate {
			get;
			set;
		}

		public System.Collections.Generic.IDictionary<string, string> NotesForDeletion {
			get;
			// TODO remove set
			set;
		}

		public System.Collections.Generic.IList<Note> DeletedNotes {
			get;
			// TODO remove set
			set;
		}

		public string AssociatedServerId {
			get;
			set;
		}

		#endregion
	}
}

