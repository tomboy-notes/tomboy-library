using System;
using System.Xml;

namespace Tomboy
{
	/// <summary>
	/// Reader is responsible for consuming Notes in XML format
	/// and returning the Note as a object.
	/// </summary>
	public class Reader
	{
		/// <summary>
		/// Read the specified xml and uri.
		/// </summary>
		/// <description>XML is the raw Note XML for each note in the system.</description>
		/// <description>uri is in the format of //tomboy:NoteHash</description>
		/// <param name='xml'>
		/// Xml.
		/// </param>
		/// <param name='uri'>
		/// URI.
		/// </param>
		public static Note Read (XmlTextReader xml, string uri)
		{
			Note note = new Note (uri);
			DateTime date;
			int num;
			string version = String.Empty;

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

