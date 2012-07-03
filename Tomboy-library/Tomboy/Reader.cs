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
using System.Xml.Xsl;
using System.Text;
using System.IO;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Reflection;

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
		private XslCompiledTransform xslTransform;
		private Assembly _assembly;
		private const string _style_sheet_name = "Tomboy.Tomboy.note_stylesheet.xsl";
		//TODO: Compile statements for Platform types
		private string _style_sheet_location = "/Library/Caches/tomboy";

		public Reader ()
		{
			/* The order of the following methods matter */
			GetAssembly ();
			//LoadPaths ();
			CopyXSLT ();
			LoadXSL ();
			/* end of orderness */
		}

		/// <summary>
		/// Loads the XSL Stylesheets for transformation later
		/// </summary>
		private void LoadXSL ()
		{
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
			//_style_sheet_location = Path.Combine (Environment.GetEnvironmentVariable ("Caches"), "tomboy");
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
		/// Strings the replacements which are / will be OS dependent.
		/// </summary>
		/// <param name='stringBuilder'>
		/// String builder.
		/// </param>
		private void StringReplacements (StringBuilder stringBuilder)
		{
			/* replace NewLines with <br /> to support WebKit# */
			stringBuilder.Replace (System.Environment.NewLine, "<br />");
		}

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
		public Note Read (XmlTextReader xml, string uri)
		{
			Note note = new Note (uri);
			DateTime date;
			int num;
			string version = String.Empty;
			// used for Note text
			StringBuilder buffer = new StringBuilder();
			StringWriter writer = new StringWriter (buffer);

			try {
				while (xml.Read ()) {
					switch (xml.NodeType) {
					case XmlNodeType.Element:
						Console.WriteLine ("Element {0}", xml.Name);
						switch (xml.Name) {
						case "note":
							version = xml.GetAttribute ("version");
							break;
						case "title":
							note.Title = xml.ReadString ();
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
						case "text":
							xslTransform.Transform(xml, null,writer);
							StringReplacements (buffer);
							note.Text = buffer.ToString ();
							break;

						case "open-on-startup":
							note.OpenOnStartup  = xml.ReadString ();
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