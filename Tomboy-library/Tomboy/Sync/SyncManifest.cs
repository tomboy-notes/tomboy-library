//
//  SyncManifest.cs
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
								manifest.LastSyncDate = DateTime.UtcNow;
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
}