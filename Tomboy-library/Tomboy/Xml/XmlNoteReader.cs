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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Tomboy.Tags;
using System.Text;

namespace Tomboy.Xml
{
	/// <summary>
	/// Reader is responsible for consuming Notes in XML format
	/// and returning the Note as a object.
	/// </summary>
	public static class XmlNoteReader
	{
		/// <summary>
		/// Current XML version
		/// </summary>
		public const string CURRENT_VERSION = "0.3";
		
		/// <summary>
		/// Read and parses a Stream that should contains valid XML encoded note.
		/// </summary>
		/// <param name='stream'>
		/// A stream that contains XML.
		/// </param>
		/// <param name='uri'>
		/// URI.
		/// </param>
		// TODO uri must go elsewhere or be removed
		public static Note Read (Stream stream, string uri)
		{
			Note note = new Note (uri);
			string version = String.Empty;
			
			try {
				var xdoc = XDocument.Load (stream, LoadOptions.PreserveWhitespace);
				var elements = xdoc.Root.Elements ();
				
				version =
					(from el in xdoc.Elements () where el.Name.LocalName == "note"
					select el.Attribute ("version").Value).Single ();
				if (version != CURRENT_VERSION)
					throw new Exception ("Version missmatch");
				
				note.Title =
					(from el in elements where el.Name.LocalName == "title"
					select el.Value).Single ();
			
				// <text><note-content> is tricky as it contains tags that might be XML interpreted
				var text_node = xdoc.Root.Elements().Where (n => n.Name.LocalName == "text").Elements ().First ();
				// remove namespace from embedded tags
				foreach (var desc in text_node.Descendants ()) {
					desc.Name = desc.Name.LocalName;
				}	
				// read the inner text as pure string, don't interpret tags
				using (var reader = text_node.CreateReader ()) {
					reader.MoveToContent ();
					note.Text = reader.ReadInnerXml ();
				}
				
				var tags =
					from el in elements where el.Name.LocalName == "tags"
					from tag in el.Elements ()
					select tag.Value;
					
				foreach (string tag in tags)
					note.Tags.Add (tag, new Tag (tag));
				
				note.CreateDate =
					(from el in elements where el.Name.LocalName == "create-date"
					select DateTime.Parse (el.Value)).Single ();
				
				note.ChangeDate =
					(from el in elements where el.Name.LocalName == "last-change-date"
					select DateTime.Parse (el.Value)).Single ();
				
				note.MetadataChangeDate =
					(from el in elements where el.Name.LocalName == "last-metadata-change-date"
					select DateTime.Parse (el.Value)).Single ();
				
				note.Width =
					(from el in elements where el.Name.LocalName == "width"
					select int.Parse (el.Value)).FirstOrDefault ();
				
				note.Height =
					(from el in elements where el.Name.LocalName == "height"
					select int.Parse (el.Value)).FirstOrDefault ();
				
				note.X =
					(from el in elements where el.Name.LocalName == "x"
					select int.Parse (el.Value)).FirstOrDefault ();
				
				note.Y =
					(from el in elements where el.Name.LocalName == "y"
					select int.Parse (el.Value)).FirstOrDefault ();
			}
			catch (Exception e) {
				throw e;
			}
			return note;
		}
		public static Note Read (string xmlstring, string uri)
		{
			using (var memstream = new MemoryStream ()) {	
				using (var streamwriter = new StreamWriter (memstream, Encoding.UTF8)) {
					streamwriter.Write (xmlstring);
					streamwriter.Flush ();
					memstream.Position = 0;
					return Read (memstream, uri);
				}
			}
		}
	}
}