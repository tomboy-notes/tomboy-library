//
//  Author:
//       Timo Dörr <timo@latecrew.de>
//
//  Copyright (c) 2012-2014 Timo Dörr
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
using System;
using NUnit.Framework;
using System.Xml;
using Tomboy.Sync;
using System.Text;
using System.IO;
using Tomboy.Tags;
using System.Linq;

namespace Tomboy.Sync
{
	[TestFixture()]
	public class SyncManifestTests
	{
		SyncManifest sampleManifest;
		string tmpFilePath;
		string tmpSampleManifestPath;
	
		[SetUp]
		public void Setup ()
		{
			var manifest = new SyncManifest ();
			manifest.LastSyncDate = DateTime.UtcNow - new TimeSpan (1, 0, 0);
			manifest.LastSyncRevision = 2;

			manifest.NoteRevisions.Add ("1234-5678-9012-3456", 4);
			manifest.NoteRevisions.Add ("1111-2222-3333-4444", 9293);
			manifest.NoteRevisions.Add ("6666-2222-3333-4444", 17);
			
			manifest.NoteDeletions.Add ("1111-11111-1111-1111", "Deleted note 1");
			manifest.NoteDeletions.Add ("1111-11111-2222-2222", "Gelöschte Notiz 2");

			sampleManifest = manifest;
			
			tmpFilePath = Path.GetTempFileName ();
			Console.WriteLine ("using tmpFile: {0}", tmpFilePath);
		
			tmpSampleManifestPath = Path.GetTempFileName ();
			Console.WriteLine ("using tmpSampleManifestPath: {0}", tmpSampleManifestPath);
				
			using (var fs = File.OpenWrite (tmpSampleManifestPath)) {
				SyncManifest.Write (sampleManifest, fs);
			}
		}
		[TearDown]
		public void TearDown ()
		{
			if (!string.IsNullOrEmpty (tmpFilePath) && File.Exists (tmpFilePath)) {
				File.Delete (tmpFilePath);
			}
		}
		void VerifyManifestsAreTheSame (SyncManifest expected, SyncManifest actual)
		{
			Assert.AreEqual (expected.LastSyncDate, actual.LastSyncDate);
			Assert.AreEqual (expected.LastSyncRevision, actual.LastSyncRevision);

			Assert.AreEqual (expected.NoteRevisions.Count, actual.NoteRevisions.Count);

			foreach (var kvp in expected.NoteRevisions) {
				Assert.That (actual.NoteRevisions.ContainsKey (kvp.Key));
				Assert.That (actual.NoteRevisions [kvp.Key] == kvp.Value);
			}
			foreach (var kvp in expected.NoteDeletions) {
				Assert.That (actual.NoteDeletions.ContainsKey (kvp.Key));
				Assert.That (actual.NoteDeletions[kvp.Key] == kvp.Value);
			}
		}
		
		[Test()]
		public void ReadWriteSyncManifest ()
		{
			using (var fs = File.OpenWrite (tmpFilePath)) {
				SyncManifest.Write (sampleManifest, fs);
			}
			// re-read in the results
			SyncManifest manifest;
			using (var fs = File.OpenRead (tmpFilePath)) {
				 manifest = SyncManifest.Read (fs);
			}
			
			VerifyManifestsAreTheSame (sampleManifest, manifest);
		}
		[Test]
		public void ReadSyncManifest ()
		{
			SyncManifest manifest;
			using (var fs = File.OpenRead ("../../test_manifest/sample_manifest.xml")) {	
				 manifest = SyncManifest.Read (fs);
			}
			
			Assert.AreEqual (2, manifest.LastSyncRevision);
			Assert.AreEqual (DateTime.Parse ("2014-02-19T13:48:43.8263650+00:00").ToUniversalTime (), manifest.LastSyncDate);
			Assert.AreEqual ("1111-2222-3333-4444", manifest.ServerId);
			Assert.AreEqual (3, manifest.NoteRevisions.Count ());
			Assert.AreEqual (1, manifest.NoteRevisions.Where (kvp => kvp.Key == "1234-5678-9012-3456" && kvp.Value == 4).Count ());
			Assert.AreEqual (1, manifest.NoteRevisions.Where (kvp => kvp.Key == "1111-2222-3333-4444" && kvp.Value == 9293).Count ());
			Assert.AreEqual (1, manifest.NoteRevisions.Where (kvp => kvp.Key == "6666-2222-3333-4444" && kvp.Value == 17).Count ());

			Assert.AreEqual (2, manifest.NoteDeletions.Count ());
			Assert.AreEqual (1, manifest.NoteDeletions.Where (kvp => kvp.Key == "1111-11111-1111-1111" && kvp.Value == "Deleted note 1").Count ());
			Assert.AreEqual (1, manifest.NoteDeletions.Where (kvp => kvp.Key == "1111-11111-2222-2222" && kvp.Value == "Gelöschte Notiz 2").Count ());
		}
		[Test]
		public void ReadSyncManifestAsString ()
		{
			string xml_manifest;
			using (var fs = File.OpenRead (tmpSampleManifestPath)) {	
				using (var streamReader = new StreamReader (fs, Encoding.UTF8)) {
					xml_manifest = streamReader.ReadToEnd ();	
				}
			}
			SyncManifest manifest = SyncManifest.Read ((string) xml_manifest);
			VerifyManifestsAreTheSame (sampleManifest, manifest);
		}
		[Test]
		public void ReadSyncManifestWithoutLastSyncDateSucceeds ()
		{
			using (var fs = File.OpenRead ("../../test_manifest/sample_manifest_without_lastsyncdate.xml")) {
				var manifest = SyncManifest.Read (fs);
				Assert.AreEqual (DateTime.MinValue, manifest.LastSyncDate);
			}
		}
		[Test]
		public void WriteSyncManifestToString ()
		{ 
			string xml_manifest = SyncManifest.Write (sampleManifest);
			using (var fs = File.OpenRead (tmpSampleManifestPath)) {	
				using (var streamReader = new StreamReader (fs, Encoding.UTF8)) {
					string xml_manifest_expected = streamReader.ReadToEnd ();	
					Assert.AreEqual (xml_manifest_expected, xml_manifest);
				}
			}
			
		}
	}
}
