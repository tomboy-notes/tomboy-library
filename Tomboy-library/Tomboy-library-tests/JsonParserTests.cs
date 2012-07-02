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
		public void ParseNotesResponse_GoodResponse_ReturnsCorrectNotes ()
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
						+ "}"
						+ "\t], "
						+ "\t\"latest-sync-revision\": 2"
						+ "}";
			Dictionary<string, Note> notes = JsonParser.ParseCompleteNotesResponse (noteresponse);

			Assert.AreEqual (2, notes.Count);

			Note testNote = notes["note://tomboy/c70f70f5-f080-4333-8a37-34213fdc8c5e"];
			Assert.AreEqual ("A Note", testNote.Title);
			Assert.AreEqual ("Describe your new note here: this note has some content. I believe.", testNote.Text);
			Assert.AreEqual (DateTime.Parse ("2012-06-27T20:05:33Z"), testNote.ChangeDate);
			Assert.IsTrue (testNote.Tags.ContainsKey ("system:notebook:Tomboy etc."));
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
		public void CreateNoteUploadJson_ProperNotesDictionary_ReturnsCorrectJson ()
		{

		}

	}
}

