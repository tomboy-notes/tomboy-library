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

