using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;

namespace Tomboy
{
	public class DiskStorage : IStorage
	{
		
		static DiskStorage instance = null;
		static readonly object lock_ = new object();

		protected DiskStorage ()
		{
		}

		public static DiskStorage Instance
		{
			get
			{
				lock (lock_)
				{
					if (instance == null)
						instance = new DiskStorage ();
					return instance;
				}
			}
			set {
				lock (lock_)
				{
					instance = value;
				}
			}
		}		
		
		public List<Note> GetNotes ()
		{
			return null;
		}
				
		public static Note Read (string read_file, string uri)
		{
			return Instance.ReadFile (read_file, uri);
		}

		public virtual Note ReadFile (string read_file, string uri)
		{
			Note note;
			string version;
			using (var xml = new XmlTextReader (new StreamReader (read_file, System.Text.Encoding.UTF8)) {Namespaces = false})
				note = Read (xml, uri, out version);

			return note;
		}

		public virtual Note Read (XmlTextReader xml, string uri)
		{
			string version; // discarded
			Note note = Read (xml, uri, out version);
			return note;
		}

		private Note Read (XmlTextReader xml, string uri, out string version)
		{
			Note note = new Note (uri);
			DateTime date;
			int num;
			version = String.Empty;

			while (xml.Read ()) {
				switch (xml.NodeType) {
				case XmlNodeType.Element:
					switch (xml.Name) {
					case "note":
						version = xml.GetAttribute ("version");
						break;
					case "title":
						note.Title = xml.ReadString ();
						break;
					case "text":
						// <text> is just a wrapper around <note-content>
						// NOTE: Use .text here to avoid triggering a save.
						note.Text = xml.ReadInnerXml ();
						break;
					case "last-change-date":
						if (DateTime.TryParse (xml.ReadString (), out date))
							note.ChangeDate = date;
						else
							note.ChangeDate = DateTime.Now;
						break;
					case "last-metadata-change-date":
						if (DateTime.TryParse (xml.ReadString (), out date))
							note.MetadataChangeDate = date;
						else
							note.MetadataChangeDate = DateTime.Now;
						break;
					case "create-date":
						if (DateTime.TryParse (xml.ReadString (), out date))
							note.CreateDate = date;
						else
							note.CreateDate = DateTime.Now;
						break;
					case "x":
						if (int.TryParse (xml.ReadString (), out num))
							note.X = num;
						break;
					case "y":
						if (int.TryParse (xml.ReadString (), out num))
							note.Y = num;
						break;
					}
					break;
				}
			}

			return note;
		}		
	}
}