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

