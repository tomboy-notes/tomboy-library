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
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using Tomboy.Xml;

namespace Tomboy.Sync
{
	public class SyncManifest
	{
		private DateTime lastSyncDate;
		public DateTime LastSyncDate {
			get {
				return lastSyncDate;
			}
			set {
				lastSyncDate = value.ToUniversalTime ();
			}
		}

		public long LastSyncRevision {
			get; set;
		}

		public string ServerId {
			get; set;
		}

		/// <summary>
		/// Gets the note revisions. Note that only a synchronization server needs these
		/// revisions. A client should not access those. A note revision will ALWAYS be less
		/// or equal to LastSyncRevision.
		/// </summary>
		public IDictionary<string, long> NoteRevisions {
			get ; private set;
		}

		/// <summary>
		/// Keep track of notes deleted since the last sync.
		/// </summary>
		/// <value>
		/// The key of the Dictionary are the note Guids, while the values are the note title
		/// (we don't have access to the original note object, since it got deleted).
		/// </value>
		public IDictionary<string, string> NoteDeletions {
			get; private set;
		}

		public SyncManifest ()
		{
			Reset ();
		}

		public void Reset ()
		{
			NoteRevisions = new Dictionary<string, long> ();
			NoteDeletions = new Dictionary<string, string> ();
			ServerId = "";
			LastSyncDate = DateTime.MinValue;
			LastSyncRevision = -1;
		}
		#region Xml serialization
		private const string CURRENT_VERSION = "0.3";

		/// <summary>
		/// Write the specified manifest to an ouput stream.
		/// </summary>
		public static void Write (SyncManifest manifest, Stream output)
		{
			var xdoc = new XDocument ();
			xdoc.Add (new XElement ("manifest",
				new XAttribute ("version", CURRENT_VERSION),
				new XElement ("last-sync-date", manifest.LastSyncDate.ToString (XmlSettings.DATE_TIME_FORMAT)),
				new XElement ("last-sync-rev", manifest.LastSyncRevision),
				new XElement ("server-id", manifest.ServerId)
				)
			);
			
			xdoc.Element ("manifest").Add (new XElement ("note-revisions",
				manifest.NoteRevisions.Select (r => {
					return new XElement ("note",
						new XAttribute ("guid", r.Key),
						new XAttribute ("latest-revision", r.Value)
					);
				})
			));
				
			xdoc.Element ("manifest").Add (new XElement ("note-deletions",
				manifest.NoteDeletions.Select (d => {
					return new XElement ("note",
						new XAttribute ("guid", d.Key),
						new XAttribute ("title", d.Value)
					);
				})
			));
		
			// this has to be performed at last..	
			xdoc.Root.SetDefaultXmlNamespace ("http://beatniksoftware.com/tomboy");		

			using (var writer = XmlWriter.Create (output, XmlSettings.DocumentSettings)) {
				xdoc.WriteTo (writer);
			}
		}
		/// <summary>
		/// Returns a XML string representation of the SyncManifest.
		/// </summary>
		/// <param name="manifest">Manifest.</param>
		public static string Write (SyncManifest manifest)
		{
			using (var ms = new MemoryStream ()) {
				using (var writer = new StreamWriter (ms, Encoding.UTF8)) {
					SyncManifest.Write (manifest, ms);	
					ms.Position = 0;
					using (var reader = new StreamReader (ms, Encoding.UTF8)) {
						return reader.ReadToEnd();
					}
				}
			}
		}
		public static SyncManifest Read (Stream stream)
		{
			SyncManifest manifest = new SyncManifest ();
			
			try {
				var xdoc = XDocument.Load (stream);
				var elements = xdoc.Root.Elements ().ToList<XElement> ();
	
				string version = 
					(from el in xdoc.Elements() where el.Name.LocalName == "manifest"
					 select el.Attribute ("version").Value).Single ();
				if (version != CURRENT_VERSION)
					throw new TomboyException ("Syncmanifest is of unknown version");
	
				manifest.LastSyncDate =
					(from  el in elements where el.Name.LocalName == "last-sync-date"
					select DateTime.Parse (el.Value)).FirstOrDefault ();
	
				manifest.ServerId = 
					(from el in elements where el.Name.LocalName == "server-id"
					 select el.Value).Single();
	
				manifest.LastSyncRevision =
					(from el in elements where el.Name.LocalName == "last-sync-rev"
					 select long.Parse (el.Value)).Single ();
	
				var notes_for_deletion = 
					from el in elements where el.Name.LocalName == "note-deletions"
					from note in el.Elements ()  
					let guid = (string) note.Attribute ("guid").Value 
					let title = (string) note.Attribute ("title").Value
					select new KeyValuePair<string, string> (guid, title);
	
				foreach (var kvp in notes_for_deletion)
					manifest.NoteDeletions.Add (kvp);
	
				var notes_revisions =
					from el in elements where el.Name.LocalName == "note-revisions"
					from note in el.Elements()
					let guid = (string) note.Attribute ("guid").Value
					let revision = long.Parse (note.Attribute ("latest-revision").Value)
					select new KeyValuePair<string, long> (guid, revision);
					
				foreach (var kvp in notes_revisions)
					manifest.NoteRevisions.Add (kvp);
	
				return manifest;
			}
			catch (Exception e) {
				// TODO handle exception
				throw e;
			}
		}
		public static SyncManifest Read (string xmlstring)
		{
			using (var memstream = new MemoryStream ()) {	
				using (var streamwriter = new StreamWriter (memstream, Encoding.UTF8)) {
					streamwriter.Write (xmlstring);
					streamwriter.Flush ();
					memstream.Position = 0;
					return Read (memstream);
				}
			}
		}
		#endregion Xml serialization
	}
}