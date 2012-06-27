//  Author:
//       jjennings <jaredljennings@gmail.com>
//  
//  Copyright (c) 2012 jjennings
// 
// This library is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation; either version 2.1 of the
// License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

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
		/// Current XML version
		/// </summary>
		public const string CURRENT_VERSION = "0.3";
		
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

	    try {
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
	    } catch (System.Xml.XmlException) {
		throw new TomboyException ("Note XML is corrupted!");	
	    }

			return note;
		}
	}
}

