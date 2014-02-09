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

		/// <summary>
		/// Write API provided to clients to use for saving the settings
		/// </summary>
		/// <param name="settings">Settings.</param>
		public static void Write(SettingsSync settings){
			Init();
			var xmlSettings = new XmlWriterSettings ();
			xmlSettings.Indent = true;
			xmlSettings.IndentChars = "\t";
			XmlWriter writer = XmlWriter.Create (settingsFile, xmlSettings);
			Write (writer, settings);
			writer.Close ();
		}

		public static void CreateSettings() {
			Init();
			System.IO.File.Create(settingsFile);
		}

		/// <summary>
		/// Write the specified xml and settings.
		/// </summary>
		/// <param name="xml">Xml.</param>
		/// <param name="settings">Settings.</param>
		private static void Write(XmlWriter xml, SettingsSync settings){
			xml.WriteStartDocument ();
			xml.WriteStartElement (null, "settings", null);

			xml.WriteStartElement (null, "sync-url", null);
			xml.WriteString (settings.syncURL);
			xml.WriteEndElement ();

			xml.WriteStartElement (null, "auto-sync", null);
			string temp = "";
			temp = (settings.autoSync == true) ? temp = "True" : temp = "False";
			xml.WriteString (temp);
			xml.WriteEndElement ();

			xml.WriteEndElement ();

		}

		/// <summary>
		/// Reads from the Settings.xml file and returns all the settings
		/// </summary>
		public static SettingsSync Read(){
			Init();
			if (System.IO.File.Exists(settingsFile))
			{
				XmlReader reader = XmlTextReader.Create(settingsFile);
				SettingsSync settings = Read(reader);
				reader.Close();
				return settings;
			}
			else
			{
				SettingsSync settings = new SettingsSync();
				settings.autoSync = true;
				settings.syncURL = "";
				return settings;
			}
		}

		/// <summary>
		/// Read the specified reader.
		/// </summary>
		/// <param name="reader">Reader.</param>
		private static SettingsSync Read(XmlReader reader){
			SettingsSync settings = new SettingsSync ();

			try{
				while(reader.Read ()){
					switch(reader.NodeType){
						case XmlNodeType.Element:
							switch(reader.Name){
								case "settings": 
									break;
								case "sync-url": 
									settings.syncURL = reader.ReadString ();
									break;
								case "auto-sync":
									string temp = reader.ReadString ();
									settings.autoSync = (temp.Equals ("True")) ? settings.autoSync = true : settings.autoSync = false;
									break;
							}
							break;
					}
				}
			}catch(XmlException e){
				//Console.Write(e.ToString);
			}

			return settings;
		}

	}

}
