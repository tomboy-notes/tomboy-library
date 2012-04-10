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
using System.Text;
using System.IO;
using System.Xml;

namespace Tomboy
{
	/// <summary>
	/// Xml decoder for Note file
	/// </summary>
	public class XmlDecoder
	{

		static StringBuilder builder;

		static XmlDecoder ()
		{
			builder = new StringBuilder ();
		}

		public static string Decode (string source)
		{
			StringReader reader = new StringReader (source);
			XmlTextReader xml = new XmlTextReader (reader);
			xml.Namespaces = false;

			while (xml.Read ()) {
				switch (xml.NodeType) {
				case XmlNodeType.Text:
				case XmlNodeType.Whitespace:
					builder.Append (xml.Value);
					break;
				}
			}

			xml.Close ();

			string val = builder.ToString ();
			builder.Length = 0;
			return val;
		}
	}
}

