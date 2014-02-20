//
//  XmlUtil.cs
//
//  Author:
//       jjennings <jaredljennings@gmail.com>
//       Timo Dörr <timo@latecrew.de>
//  
//  Copyright (c) 2012 jjennings
//  Copyright (c) 2014 Timo Dörr
//
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
using System.Xml;
using System.Xml.Linq;

namespace Tomboy.Xml
{
	public static class XmlHelper
	{
		public static void SetDefaultXmlNamespace(this XElement xelem, XNamespace xmlns)
		{
			if (xelem.Name.NamespaceName == string.Empty)
				xelem.Name = xmlns + xelem.Name.LocalName;
			foreach (var e in xelem.Elements())
				e.SetDefaultXmlNamespace (xmlns);
		}
	}

	public static class XmlSettings
	{
		// NOTE: If this changes from a standard format, make sure to update
		//       XML parsing to have a DateTime.TryParseExact
		public static string DATE_TIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";
	
		static XmlWriterSettings documentSettings;
		static XmlWriterSettings fragmentSettings;

		static XmlSettings ()
		{
			documentSettings = new XmlWriterSettings ();
			documentSettings.NewLineChars = "\n";
			documentSettings.Indent = true;
			documentSettings.CheckCharacters = false;
			documentSettings.IndentChars = "\t";

			fragmentSettings = new XmlWriterSettings ();
			fragmentSettings.NewLineChars = "\n";
			fragmentSettings.Indent = true;
			fragmentSettings.CheckCharacters = false;
			fragmentSettings.ConformanceLevel = ConformanceLevel.Fragment;
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