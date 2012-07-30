// 
//  JsonParserTests.cs
//  
//  Author:
//       Robert Nordan <rpvn@robpvn.net>
// 
//  Copyright (c) 2012 Robert Nordan
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
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Tomboy.Sync.Snowy;

namespace Tomboy
{
	[TestFixture()]
	public class JsonParserTests
	{
		[Test()]
		public void ParseRootLevelResponseForOAuthDetails_GoodResponse_ReturnsCorrectData ()
		{
			string response = "{"
					+ "\t\"oauth_access_token_url\": \"https://edge.tomboy-online.org/oauth/access_token/\"," 
					+ "\t\"api-version\": \"1.0\"," 
					+ "\t\"oauth_request_token_url\": \"https://edge.tomboy-online.org/oauth/request_token/\"," 
					+ "\t\"oauth_authorize_url\": \"https://edge.tomboy-online.org/oauth/authenticate/\"," 
	//				+ "\t\"user-ref\": {"
	//				+ "\t\t\"href\": \"https://edge.tomboy-online.org/rpvn/\"," 
	//				+ "\t\t\"api-ref\": \"https://edge.tomboy-online.org/api/1.0/rpvn/\""
	//				+ "}"
					+ "}";
	
			OAuthEndPoints parsed = JsonParser.ParseRootLevelResponseForOAuthDetails (response);
			Assert.AreEqual ("https://edge.tomboy-online.org/oauth/access_token/", parsed.accessUrl);
			Assert.AreEqual ("https://edge.tomboy-online.org/oauth/request_token/", parsed.requestUrl);
			Assert.AreEqual ("https://edge.tomboy-online.org/oauth/authenticate/", parsed.userAuthorizeUrl);
		}

		[Test()]
		public void ParseRootLevelResponseForUserName_GoodResponse_ReturnsCorrectData ()
		{
			string response = "{"
					+ "\t\"oauth_access_token_url\": \"https://edge.tomboy-online.org/oauth/access_token/\"," 
					+ "\t\"api-version\": \"1.0\"," 
					+ "\t\"oauth_request_token_url\": \"https://edge.tomboy-online.org/oauth/request_token/\"," 
					+ "\t\"oauth_authorize_url\": \"https://edge.tomboy-online.org/oauth/authenticate/\"," 
					+ "\t\"user-ref\": {"
					+ "\t\t\"href\": \"https://edge.tomboy-online.org/tomboyusername/\"," 
					+ "\t\t\"api-ref\": \"https://edge.tomboy-online.org/api/1.0/tomboyusername/\""
					+ "}"
					+ "}";
	
			string parsed = JsonParser.ParseRootLevelResponseForUserName (response);
			Assert.AreEqual ("tomboyusername", parsed);
		}

		[Test()]
		public void ParseNotesResponse_GoodResponseNoDeletions_ReturnsCorrectNotes ()
		{
			// A typical response if you have for exaple used the paramater since¤455 in your request,
			// representing the changes since that revision.
			string noteresponse = "{"
						+ "\t\"notes\": ["
						+ "{"
						+ "\"note-content\": \"Describe your new note here: this note has some content. I believe.\", "
						+ "\"open-on-startup\": false, "
						+ "\"last-metadata-change-date\": \"2012-06-27T20:05:33Z\", "
						+ "\"title\": \"A Note\", "
						+ "\"tags\": ["
						+ "\"system:notebook:Tomboy etc.\""
						+ "], "
						+ "\"create-date\": \"2012-06-27T20:04:06Z\", "
						+ "\"last-sync-revision\": 2, "
						+ "\"last-change-date\": \"2012-06-27T20:05:33Z\", "
						+ "\"guid\": \"c70f70f5-f080-4333-8a37-34213fdc8c5e\", "
						+ "\"pinned\": false"
						+ "},"
						+ "{"
						+ "\"note-content\": \"Tomboy is way cool.\", "
						+ "\"open-on-startup\": false, "
						+ "\"last-metadata-change-date\": \"2012-06-27T20:05:33Z\", "
						+ "\"title\": \"Another Note\", "
						+ "\"tags\": ["
						+ "\"system:notebook:Tomboy etc.\""
						+ "], "
						+ "\"create-date\": \"2012-06-27T20:06:06Z\", "
						+ "\"last-sync-revision\": 2, "
						+ "\"last-change-date\": \"2012-06-27T20:09:33Z\", "
						+ "\"guid\": \"c70f70f5-f080-4333-8a37-hjfkdskd\", "
						+ "\"pinned\": false"
						+ "}"
						+ "\t], "
						+ "\t\"latest-sync-revision\": 456"
						+ "}";
			NoteChanges notes = JsonParser.ParseCompleteNotesResponse (noteresponse);
			Dictionary<string, Note> changed = notes.ChangedNotes;

			Assert.AreEqual (2, changed.Count);

			Note testNote = changed["note://tomboy/c70f70f5-f080-4333-8a37-34213fdc8c5e"];
			Assert.AreEqual ("A Note", testNote.Title);
			Assert.AreEqual ("Describe your new note here: this note has some content. I believe.", testNote.Text);
			Assert.AreEqual (DateTimeOffset.Parse ("2012-06-27T20:05:33Z").DateTime, testNote.ChangeDate);
			Assert.IsTrue (testNote.Tags.ContainsKey ("system:notebook:Tomboy etc."));

			List<string> deleted = notes.DeletedNoteGuids;
			Assert.AreEqual (0, deleted.Count);

			Assert.AreEqual (456, notes.SyncRevision);
		}

		[Test()]
		public void ParseNotesResponse_GoodResponseSomeDeletions_ReturnsCorrectNotes ()
		{
			string noteresponse = "{"
						+ "\t\"notes\": ["
						+ "{"
						+ "\"note-content\": \"Describe your new note here: this note has some content. I believe.\", "
						+ "\"open-on-startup\": false, "
						+ "\"last-metadata-change-date\": \"2012-06-27T20:05:33Z\", "
						+ "\"title\": \"A Note\", "
						+ "\"tags\": ["
						+ "\"system:notebook:Tomboy etc.\""
						+ "], "
						+ "\"create-date\": \"2012-06-27T20:04:06Z\", "
						+ "\"last-sync-revision\": 2, "
						+ "\"last-change-date\": \"2012-06-27T20:05:33Z\", "
						+ "\"guid\": \"c70f70f5-f080-4333-8a37-34213fdc8c5e\", "
						+ "\"pinned\": false"
						+ "},"
						+ "{"
						+ "\"note-content\": \"Tomboy is way cool.\", "
						+ "\"open-on-startup\": false, "
						+ "\"last-metadata-change-date\": \"2012-06-27T20:05:33Z\", "
						+ "\"title\": \"Another Note\", "
						+ "\"tags\": ["
						+ "\"system:notebook:Tomboy etc.\""
						+ "], "
						+ "\"create-date\": \"2012-06-27T20:06:06Z\", "
						+ "\"last-sync-revision\": 2, "
						+ "\"last-change-date\": \"2012-06-27T20:09:33Z\", "
						+ "\"guid\": \"c70f70f5-f080-4333-8a37-hjfkdskd\", "
						+ "\"pinned\": false"
						+ "},"
						+ "{"
                        			+ "\"guid\": \"0bc7b1ef-264f-4aa9-8746-d0f87e9b0176\","
                        			+ "\"command\": \"delete\""
                				+ "}"
						+ "\t], "
						+ "\t\"latest-sync-revision\": 456"
						+ "}";
			NoteChanges notes = JsonParser.ParseCompleteNotesResponse (noteresponse);
			Dictionary<string, Note> changed = notes.ChangedNotes;

			Assert.AreEqual (2, changed.Count);

			Note testNote = changed["note://tomboy/c70f70f5-f080-4333-8a37-34213fdc8c5e"];
			Assert.AreEqual ("A Note", testNote.Title);
			Assert.AreEqual ("Describe your new note here: this note has some content. I believe.", testNote.Text);
			Assert.AreEqual (DateTimeOffset.Parse ("2012-06-27T20:05:33Z").DateTime, testNote.ChangeDate);
			Assert.IsTrue (testNote.Tags.ContainsKey ("system:notebook:Tomboy etc."));

			List<string> deleted = notes.DeletedNoteGuids;
			Assert.AreEqual (1, deleted.Count);
			string deletedGuid = deleted[0];
			Assert.AreEqual ("0bc7b1ef-264f-4aa9-8746-d0f87e9b0176", deletedGuid);

			Assert.AreEqual (456, notes.SyncRevision);
		}

		[Test()]
		public void ParseUserInfo_GoodResponse_ReturnsCorrectInfo ()
		{

			string userResponse = "{"
						+ "\t\"user-name\": \"tomboyusername\"," 
						+ "\t\"last-name\": \"\"," 
						+ "\t\"notes-ref\": {"
						+ "\t\t\"href\": \"https://edge.tomboy-online.org/tomboyusername/notes/\", "
						+ "\t\t\"api-ref\": \"https://edge.tomboy-online.org/api/1.0/tomboyusername/notes/\""
						+ "\t}," 
						+ "\t\"current-sync-guid\": \"f87e0381-7492-43e9-a6d7-f5e0e38c6aec\", "
						+ "\t\"first-name\": \"\", "
						+ "\t\"latest-sync-revision\": 2"
						+ "}";

			UserInfo parsed = JsonParser.ParseUserInfoResponse (userResponse);

			Assert.AreEqual ("tomboyusername", parsed.username);
			Assert.AreEqual (2, parsed.latestSyncRevision);
			Assert.AreEqual ("f87e0381-7492-43e9-a6d7-f5e0e38c6aec", parsed.currentSyncGuid);
			Assert.AreEqual ("", parsed.firstname);
			Assert.AreEqual ("", parsed.lastname);
		}

		[Test()]
		public void CreateNoteUploadJson_ProperNotesDictionarySomeToBeDeleted_ReturnsCorrectJson ()
		{
//			string expectedJson =  //No point in being totally identical?
//				+ "{"
//				+ "\"latest-sync-revision\": 456,"
//				+ "\"note-changes\": [{"
//				+ "\"guid\": \"002e91a2-2e34-4e2d-bf88-21def49a7705\","
//				+ "\"title\": \"New Note 6\","
//				+ "\"note-content\": \"Describe your note <b>here</b>.\","
//				+ "\"note-content-version\": 0.1,"
//				+ "\"last-change-date\": \"2009-04-19T21:29:23.2197340-07:00\","
//				+ "\"last-metadata-change-date\": \"2009-04-19T21:29:23.2197340-07:00\","
//				+ "\"create-date\": \"2008-03-06T13:44:46.4342680-08:00\","
//				+ "\"open-on-startup\": false,"
//				+ "\"pinned\": false,"
//				+ "\"tags\": [\"tag1\", \"tag2\", \"tag3\", \"system:notebook:biology\"]"
//				+ "}, {"
//				+ "\"guid\": \"0bc7b1ef-264f-4aa9-8746-d0f87e9b0176\","
//				+ "\"command\": \"delete\""
//				+ "}]"
//				+ "}";

			Dictionary<string, Note> toUpload = new Dictionary<string, Note> ();

			Note note1 = new Note ("note://002e91a2-2e34-4e2d-bf88-21def49a770");
			note1.Title = "New Note 6";
			note1.Text = "Describe your note <b>here</b>.";
			note1.ChangeDate = DateTime.Parse ("2009-04-19T21:29:23.2197340-07:00");
			note1.MetadataChangeDate = DateTime.Parse ("2009-04-19T21:29:23.2197340-07:00");
			note1.CreateDate = DateTime.Parse ("2008-03-06T13:44:46.4342680-08:00");
			note1.OpenOnStartup = "false";
			note1.Tags.Add ("tag1", new Tags.Tag ("tag1"));
			note1.Tags.Add ("tag2", new Tags.Tag ("tag2"));
			note1.Tags.Add ("system:notebook:biology", new Tags.Tag ("system:notebook:biology"));

			toUpload.Add (note1.Uri, note1);

			Dictionary<string, Note> toDelete = new Dictionary<string, Note> ();

			//Don't need different content as long as it has a different guid
			Note note2 = new Note ("note://0bc7b1ef-264f-4aa9-8746-d0f87e9b0176");
			note2.Title = "New Note 6";
			note2.Text = "Describe your note <b>here</b>.";
			note2.ChangeDate = DateTime.Parse ("2009-04-19T21:29:23.2197340-07:00");
			note2.MetadataChangeDate = DateTime.Parse ("2009-04-19T21:29:23.2197340-07:00");
			note2.CreateDate = DateTime.Parse ("2008-03-06T13:44:46.4342680-08:00");
			note2.OpenOnStartup = "false";
			note2.Tags.Add ("tag1", new Tags.Tag ("tag1"));
			note2.Tags.Add ("tag2", new Tags.Tag ("tag2"));
			note2.Tags.Add ("system:notebook:biology", new Tags.Tag ("system:notebook:biology"));

			toDelete.Add (note2.Uri, note2);

			string result = JsonParser.CreateNoteUploadJson (toUpload, toDelete, 456);

			//Slightly iffy to use the same kind of lib for verifying as we do for decomposing from server?
			JObject json = JObject.Parse (result);

			Assert.AreEqual (456, (int) json["latest-sync-revision"]);

			JArray notes = (JArray) json["note-changes"];
			Assert.IsNotNull (notes);
			Assert.AreEqual (2, notes.Count);

			JObject deletedNote = (from JObject note in notes
				where (string) note["guid"] == "0bc7b1ef-264f-4aa9-8746-d0f87e9b0176"
				select note).First ();

			Assert.AreEqual ("delete", (string) deletedNote["command"]);
			Assert.AreNotEqual("New Note 6", (string) deletedNote["title"]);

			JObject updatedNote = (from JObject note in notes
				where (string) note["guid"] == "002e91a2-2e34-4e2d-bf88-21def49a770"
				select note).First ();

			Assert.AreEqual ("New Note 6", (string) updatedNote["title"]);
			Assert.AreEqual ("Describe your note <b>here</b>.", (string) updatedNote["note-content"]);
			Assert.AreEqual (note1.CreateDate, (DateTime) updatedNote["create-date"]);
			Assert.AreEqual (note1.ChangeDate, (DateTime) updatedNote["last-change-date"]);
			Assert.AreEqual (note1.MetadataChangeDate, (DateTime) updatedNote["last-metadata-change-date"]);

		}
	}
}

