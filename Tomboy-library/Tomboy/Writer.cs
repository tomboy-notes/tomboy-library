//  
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
	/// Note to XML writer.
	/// </summary>
	public class Writer
	{
		private const string CURRENT_VERSION = "0.3";
		// NOTE: If this changes from a standard format, make sure to update
		//       XML parsing to have a DateTime.TryParseExact
		public const string DATE_TIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";
		
		/// <summary>
		/// Write the specified Note to the provided XML object
		/// </summary>
		/// <param name='xml'>
		/// Xml.
		/// </param>
		/// <param name='note'>
		/// Note.
		/// </param>
		public static void Write (XmlWriter xml, Note note)
		{
			xml.WriteStartDocument ();
			xml.WriteStartElement (null, "note", "http://beatniksoftware.com/tomboy");
			xml.WriteAttributeString(null,
			                         "version",
			                         null,
			                         CURRENT_VERSION);
			xml.WriteAttributeString("xmlns",
			                         "link",
			                         null,
			                         "http://beatniksoftware.com/tomboy/link");
			xml.WriteAttributeString("xmlns",
			                         "size",
			                         null,
			                         "http://beatniksoftware.com/tomboy/size");

			xml.WriteStartElement (null, "title", null);
			xml.WriteString (note.Title);
			xml.WriteEndElement ();

			xml.WriteStartElement (null, "text", null);
			xml.WriteAttributeString ("xml", "space", null, "preserve");
			// Insert <note-content> blob...
			xml.WriteRaw (note.Text);
			xml.WriteEndElement ();

			xml.WriteStartElement (null, "last-change-date", null);
			xml.WriteString (
			        XmlConvert.ToString (note.ChangeDate, DATE_TIME_FORMAT));
			xml.WriteEndElement ();

			xml.WriteStartElement (null, "last-metadata-change-date", null);
			xml.WriteString (
			        XmlConvert.ToString (note.MetadataChangeDate, DATE_TIME_FORMAT));
			xml.WriteEndElement ();

			if (note.CreateDate != DateTime.MinValue) {
				xml.WriteStartElement (null, "create-date", null);
				xml.WriteString (
				        XmlConvert.ToString (note.CreateDate, DATE_TIME_FORMAT));
				xml.WriteEndElement ();
			}

			xml.WriteStartElement (null, "x", null);
			xml.WriteString (note.X.ToString ());
			xml.WriteEndElement ();

			xml.WriteStartElement (null, "y", null);
			xml.WriteString (note.Y.ToString ());
			xml.WriteEndElement ();

			if (note.Tags.Count > 0) {
				xml.WriteStartElement (null, "tags", null);
				foreach (Tag tag in note.Tags.Values) {
					xml.WriteStartElement (null, "tag", null);
					xml.WriteString (tag.Name);
					xml.WriteEndElement ();
				}
				xml.WriteEndElement ();
			}
			
			xml.WriteEndElement (); // Note
			xml.WriteEndDocument ();
		}
	}
}

