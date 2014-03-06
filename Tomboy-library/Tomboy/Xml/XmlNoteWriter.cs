//  
//  Author:
//       jjennings <jaredljennings@gmail.com>
//       Timo Dörr <timo@latecrew.de>
//  
//  Copyright (c) 2012 jjennings
//  Copyright (c) 2014 Timo Dörr
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
using Tomboy.Tags;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Text;

namespace Tomboy.Xml
{
	/// <summary>
	/// Note to XML writer.
	/// </summary>
	public static class XmlNoteWriter
	{
		public const string CURRENT_VERSION = "0.3";

		
		/// <summary>
		/// Write the specified Note to the provided XML object
		/// </summary>
		/// <param name='xml'>
		/// Xml.
		/// </param>
		/// <param name='note'>
		/// Note.
		/// </param>
		public static void Write (Note note, Stream output)
		{
			// we need this dummy tag that introduces our custom namespaces to avoid exception in XElement.Parse()
			string dummy_tag_open = "<dummy xmlns:link=\"http://beatniksoftware.com/tomboy/link\" xmlns:size=\"http://beatniksoftware.com/tomboy/size\"  >";

			var xdoc = new XDocument ();
			
			// add the root node with namespaces
			xdoc.Add (new XElement ("note",
				new XAttribute ("version", XmlNoteReader.CURRENT_VERSION),
				new XAttribute (XNamespace.Xmlns + "link", "http://beatniksoftware.com/tomboy/link"),
				new XAttribute (XNamespace.Xmlns + "size", "http://beatniksoftware.com/tomboy/size")
			));
			
			xdoc.Root.Add(
				new XElement ("title", note.Title),
				new XElement ("text",
					new XAttribute (XNamespace.Xml + "space", "preserve"),
					new XElement ("note-content",
						new XAttribute ("version", "0.1"),
						XElement.Parse(dummy_tag_open + note.Text + "</dummy>").Nodes()
					)
				),
				new XElement ("create-date", note.CreateDate),
				new XElement ("last-change-date", note.ChangeDate),
				new XElement ("last-metadata-change-date", note.ChangeDate),
				new XElement ("width", note.Width),
				new XElement ("height", note.Height),
				new XElement ("x", note.X),
				new XElement ("y", note.Y)
			);
			
			xdoc.Element ("note").Add (new XElement ("tags",
				note.Tags.Keys.Select (k => new XElement ("tag", note.Tags[k].Name))
			));
						
			using (var writer = XmlWriter.Create (output, XmlSettings.DocumentSettings)) {
                                xdoc.WriteTo (writer);
                        }
		}
		public static string Write (Note note)
		{
			using (var ms = new MemoryStream ()) {
				using (var writer = new StreamWriter (ms, Encoding.UTF8)) {
					Write (note, ms);	
					ms.Position = 0;
					using (var reader = new StreamReader (ms, Encoding.UTF8)) {
						return reader.ReadToEnd();
					}
				}
			}
		}
	}
}
