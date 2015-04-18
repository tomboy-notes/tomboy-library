//
// SettingsSync.cs
//
// Author:
//       Rashid Khan <rashood.khan@gmail.com>
//
// Copyright (c) 2014 Rashid Khan
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Xml;
using Tomboy.Tags;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using Tomboy.Xml;

namespace Tomboy.Sync
{

	public class SettingsSync
	{
		/// <summary>
		/// Settings Directory : ~/.tomboy/
		/// Settings File 	   : ~/.tomboy/Settings.xml
		/// </summary>
		private static string homeDir;
		private static string settingsDir;
		public static string settingsFile{
			get; set;
		}

		/// <summary>
		/// Sync URL defined by the User
		/// </summary>
		/// <value>The sync UR.</value>
		public string syncURL{
			get; set;
		}

		/// <summary>
		/// Specifies if the user wants to sync automatically.
		/// </summary>
		/// <value><c>true</c> if auto sync; otherwise, <c>false</c>.</value>
		public bool autoSync{
			get; set;
		}

		public string webSyncURL {
			get;
			set;
		}

		public string token {
			get;
			set;
		}

		public string secret {
			get;
			set;
		}

		/// <summary>
		/// Constructor for the SettingsSync.
		/// Initializes all the variables, i.e. creates the settings.xml
		/// </summary>
		public SettingsSync(){
			Init();
		}

		private static void Init(){
			homeDir = System.Environment.GetEnvironmentVariable ("HOME");
			settingsDir = System.IO.Path.Combine (homeDir,".tomboy");
			System.IO.Directory.CreateDirectory (settingsDir);
			settingsFile = System.IO.Path.Combine (settingsDir, "Settings.xml");
		}

		public static void Write(SettingsSync settings) {
			Init();
			using (var fs = File.OpenWrite (settingsFile)) {
				WriteNew (settings, fs);
				fs.Close ();
			}
		}

		public static void CreateSettings() {
			Init();
			if (!File.Exists (settingsFile))
				System.IO.File.Create(settingsFile);
		}

		private static void WriteNew( SettingsSync settings, Stream output) {
			var xdoc = new XDocument ();

			xdoc.Add (new XElement("settings"));

			xdoc.Root.Add (
				new XElement("sync-url",settings.syncURL),
				new XElement ("web-sync-url", settings.webSyncURL),
				new XElement ("token",settings.token),
				new XElement ("secret", settings.secret)
			);

			using (var writer = XmlWriter.Create (output, XmlSettings.DocumentSettings)) {
				xdoc.WriteTo (writer);
			}
		}

		private static SettingsSync ReadNew(Stream stream) {
			SettingsSync settings = new SettingsSync();

			try {
				var xdoc = XDocument.Load (stream, LoadOptions.PreserveWhitespace);
				var elements = xdoc.Root.Elements ();

				settings.syncURL = (from el in elements where el.Name.LocalName == "sync-url" select el.Value).FirstOrDefault ();

				settings.webSyncURL = (from el in elements where el.Name.LocalName == "web-sync-url" select el.Value).FirstOrDefault ();

				settings.token = (from el in elements where el.Name.LocalName == "token" select el.Value).FirstOrDefault ();

				settings.secret = (from el in elements where el.Name.LocalName == "secret" select el.Value).FirstOrDefault ();


			}catch( Exception e) {
			}

			return settings;
		}

		public static SettingsSync ReadFile(){
			Init();
			if (File.Exists (settingsFile))
				using (var fs = File.OpenRead (settingsFile)) {
					SettingsSync settings = ReadNew (fs);
					fs.Close ();
					return settings;
				}
			else
				return null;
		}
	}

}
