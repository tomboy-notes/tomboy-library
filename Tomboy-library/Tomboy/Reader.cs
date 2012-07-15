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
using System.Linq;
using System.Xml;
using System.Xml.Xsl;
using System.Text;
using System.IO;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;

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
		private XNamespace ns = "http://beatniksoftware.com/tomboy";
		private XslCompiledTransform xslTransform;
		private Assembly _assembly;
		private const string _style_sheet_name = "Tomboy.Tomboy.note_stylesheet.xsl";
		//TODO: Compile statements for Platform types
		private string _style_sheet_location = "";

		public Reader ()
		{
			/* The order of the following methods matter */
			GetAssembly ();
			LoadPaths ();
			LoadXSL ();
			/* end of orderness */
		}

		/// <summary>
		/// Loads the XSL Stylesheets for transformation later
		/// </summary>
		private void LoadXSL ()
		{
			CopyXSLT ();
			if (xslTransform == null) {
				Console.WriteLine ("creating Transform");
				xslTransform = new XslCompiledTransform (true);
				xslTransform.Load (Path.Combine (_style_sheet_location, _style_sheet_name));
			}
		}

		private void GetAssembly ()
		{
			try {
				_assembly = Assembly.GetExecutingAssembly ();
			} catch {
				Console.WriteLine ("Error accessing resources!");
			}	
		}

		private void LoadPaths ()
		{
			_style_sheet_location = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "Library", "Caches", "Tomboy");
		}

		/// <summary>
		/// Copies a stream from one location to another..
		/// </summary>
		/// <param name='input'>
		/// Input.
		/// </param>
		/// <param name='output'>
		/// Output.
		/// </param>
		private void CopyStream (Stream input, Stream output)
		{
			// Insert null checking here for production
			byte[] buffer = new byte[8192];

			int bytesRead;
			while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0) {
				output.Write (buffer, 0, bytesRead);
			}
		}

		/// <summary>
		/// Copies the XSL to the correct location
		/// </summary>
		private void CopyXSLT ()
		{
			if (!Directory.Exists (_style_sheet_location))
				Directory.CreateDirectory (_style_sheet_location);

			/* Only copy the file if it doesn't exist
			 * This allows someone to override the default
			 * It also allows someone to rebuild if corrupt
			 */
			if (!File.Exists (Path.Combine (_style_sheet_location, _style_sheet_name))) {
				using (Stream input = _assembly.GetManifestResourceStream(_style_sheet_name))
				using (Stream output = File.Create(Path.Combine (_style_sheet_location, _style_sheet_name)))
					CopyStream (input, output);
			}
		}

		/// <summary>
		/// Parses the string from a Note to the DateTime Value to be stored in the Note class
		/// </summary>
		/// <returns>
		/// The string.
		/// </returns>
		/// <param name='xdoc'>
		/// Xdoc.
		/// </param>
		/// <param name='attributeName'>
		/// Attribute name.
		/// </param>
		private DateTime ParseString (XDocument xdoc, string attributeName)
		{
			if (xdoc.Descendants (ns + attributeName).FirstOrDefault () == null)
				return DateTime.Now;

			XNode xnode = xdoc.Descendants (ns + attributeName).FirstOrDefault ().FirstNode;

			DateTime date;
			if (DateTime.TryParse (xnode.ToString (), out date))
				return date;
			else
				return DateTime.Now;
		}

		public Note Read (XDocument xdoc, string uri)
		{
			StringBuilder sb = new StringBuilder ();
			StringWriter stringWriter = new StringWriter (sb);
			XmlTextWriter xmlTextWriter = new XmlTextWriter (stringWriter);
			Note note = new Note (uri);

			// For debug purposes
//			IEnumerable<XElement> 
//			 from el in xdoc.Elements()
//			select el;
//			foreach (XElement e in childList)
//				Console.WriteLine("element {0}", e);

			note.Title = xdoc.Descendants (ns + "title").FirstOrDefault ().FirstNode.ToString ();
			note.ChangeDate = ParseString (xdoc, "last-change-date");
			note.MetadataChangeDate = ParseString (xdoc, "last-metadata-change-date");
			note.CreateDate = ParseString (xdoc, "create-date");
			XmlReader reader = xdoc.CreateReader ();

			try {
				while (reader.Read ()) {
					switch (reader.NodeType) {
					case XmlNodeType.Element:
						switch (reader.Name) {
						case "text":
							xslTransform.Transform(reader, null,xmlTextWriter);
							note.Text = sb.ToString ();
							break;
						}
						break;
					}
				}

			} catch (System.Xml.XmlException e) {
				//throw new TomboyException ("Note XML is corrupted!");	
				Console.Write ("exception {0}", e);
			}

			return note;
		}
	}
}