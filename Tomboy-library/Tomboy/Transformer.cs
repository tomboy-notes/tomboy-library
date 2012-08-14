//
//  Transformer.cs
//
//  Author:
//       Jared Jennings <jjennings@gnome.org>
//
//  Copyright (c) 2012 Jared Jennings 2012
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
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
	/// This class is responsible for transforming Note objects to Tomboy formats.
	/// Use this whenever the your application uses a different format than Tomboy.
	/// </summary>
	/// 
	public class Transformer
	{
		private XNamespace ns = "http://beatniksoftware.com/tomboy";
		private XslCompiledTransform xslTransformTo;
		private XslCompiledTransform xslTransformFrom;
		private Assembly _assembly;
		private const string _xsl_transform_to = "Tomboy.Tomboy.transform_to_note.xsl";
		private const string _xsl_transform_from = "Tomboy.Tomboy.transform_from_note.xsl";
		private string _style_sheet_location = "";

		public Transformer ()
		{
			/* The order of the following methods matter */
			GetAssembly ();
			LoadPaths ();
			CopyXSLT (_xsl_transform_to);
			LoadXSL ();
			/* end of orderness */
		}

		/// <summary>
		/// Transforms to standard Tomboy formats
		/// </summary>
		/// <param name='note'>
		/// Note.
		/// </param>
		public void TransformTo (Note note)
		{
			StringBuilder sb = new StringBuilder ();
			StringWriter stringWriter = new StringWriter (sb);
			XmlTextWriter xmlTextWriter = new XmlTextWriter (stringWriter);

			StringReader stringReader = new StringReader (note.Text);
			XmlTextReader xmlTextReader = new XmlTextReader (stringReader);
			XmlReader reader = xmlTextReader;

			try {
				while (reader.Read ()) {
					switch (reader.NodeType) {
					case XmlNodeType.Element:
						Console.WriteLine ("Element Name {0}", reader.Name);
						switch (reader.Name) {
						case "text":
							xslTransformTo.Transform(reader, null,xmlTextWriter);
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
		}

		/// <summary>
		/// Loads the XSL Stylesheets for transformation later
		/// </summary>
		private void LoadXSL ()
		{
			if (xslTransformTo == null) {
				xslTransformTo = new XslCompiledTransform (true);
				xslTransformTo.Load (Path.Combine (_style_sheet_location, _xsl_transform_to));
			}
			if (xslTransformFrom == null) {
				xslTransformFrom = new XslCompiledTransform (true);
				xslTransformFrom.Load (Path.Combine (_style_sheet_location, _xsl_transform_from));
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
		private void CopyXSLT (string xsl_file_name)
		{
			if (!Directory.Exists (_style_sheet_location))
				Directory.CreateDirectory (_style_sheet_location);

			/* Only copy the file if it doesn't exist
			 * This allows someone to override the default
			 * It also allows someone to rebuild if corrupt
			 */
			if (!File.Exists (Path.Combine (_style_sheet_location, xsl_file_name))) {
				Console.WriteLine ("deploying default Transform {0}", xsl_file_name);
				using (Stream input = _assembly.GetManifestResourceStream(xsl_file_name))
				using (Stream output = File.Create(Path.Combine (_style_sheet_location, xsl_file_name)))
					CopyStream (input, output);
			}
		}
	}
}

