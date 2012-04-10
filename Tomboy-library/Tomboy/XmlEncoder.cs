//  Author:
//       jjennings <jaredljennings@gmail.com>
//  
//  Copyright (c) 2012 jjennings
//  Robert Nordan
//  Alex Graveley (original author)
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
using System.Text;
using System.IO;

namespace Tomboy
{
	/// <summary>
	/// Xml encoding of the Note file
	/// </summary>
	public class XmlEncoder
	{

		static StringBuilder builder;
		static StringWriter writer;
		static XmlTextWriter xml;
		static XmlWriterSettings documentSettings;
		static XmlWriterSettings fragmentSettings;

		static XmlEncoder ()
		{
			documentSettings = new XmlWriterSettings ();
			documentSettings.NewLineChars = "\n";
			documentSettings.Indent = true;

			fragmentSettings = new XmlWriterSettings ();
			fragmentSettings.NewLineChars = "\n";
			fragmentSettings.Indent = true;
			fragmentSettings.ConformanceLevel = ConformanceLevel.Fragment;

			builder = new StringBuilder ();
			writer = new StringWriter (builder);
			xml = new XmlTextWriter (writer);
		}

		public static string Encode (string source)
		{
			xml.WriteString (source);

			string val = builder.ToString ();
			builder.Length = 0;
			return val;
		}

		public static XmlWriterSettings DocumentSettings
		{
			get { return documentSettings; }
		}

		public static XmlWriterSettings FragmentSettings
		{
			get { return fragmentSettings; }
		}
	}
}

