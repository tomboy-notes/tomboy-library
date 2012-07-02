// 
//  JsonParser.cs
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
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
namespace Tomboy.Sync.Snowy
{
	public class JsonParser
	{
		public JsonParser ()
		{
		}

		#region Decoders
		public static OAuthEndPoints ParseRootLevelResponseForOAuthDetails (string response) 
		{
			OAuthEndPoints toRet = new OAuthEndPoints ();

			JObject json = JObject.Parse (response);

			toRet.accessUrl = (string)json["oauth_access_token_url"];
			toRet.requestUrl = (string)json["oauth_request_token_url"];
			toRet.userAuthorizeUrl = (string)json["oauth_authorize_url"];

			return toRet;
		}

		public static string ParseRootLevelResponseForUserName (string response) 
		{
			JObject json = JObject.Parse (response);

			string user = (string) json["user-ref"]["href"];

			user = user.TrimEnd ('/');

			string[] userArray = user.Split ('/');
			return userArray[userArray.Length -1];
		}

		public static Dictionary<string, Note>  ParseCompleteNotesResponse (string response)
		{
			Dictionary<string, Note> toRet = new Dictionary<string, Note> ();
			JObject json = JObject.Parse (response);

			foreach (var noteJson in json["notes"]) {
				Note newNote = new Note ("note://tomboy/" + noteJson["guid"]);
				newNote.ChangeDate = DateTime.Parse ((string) noteJson["last-change-date"]);
				newNote.CreateDate = DateTime.Parse ((string) noteJson["create-date"]);
				newNote.MetadataChangeDate = DateTime.Parse ((string) noteJson["last-metadata-change-date"]);
				newNote.Text = (string) noteJson["note-content"];
				newNote.Title = (string) noteJson["title"];

				foreach (var tagJson in noteJson["tags"]) {
					newNote.Tags.Add (tagJson.ToString (), new Tags.Tag (tagJson.ToString ()));
				}
				toRet.Add (newNote.Uri, newNote);
			}

			return toRet;
		}

		public static UserInfo ParseUserInfoResponse (string response)
		{
			UserInfo toRet = new UserInfo ();

			JObject json = JObject.Parse (response);

			toRet.latestSyncRevision = (int) json["latest-sync-revision"];
			toRet.firstname = (string) json["first-name"];
			toRet.lastname = (string) json["last-name"];
			toRet.currentSyncGuid = (string) json["current-sync-guid"];
			toRet.username = (string) json["user-name"];

			return toRet;
		}
		#endregion Decoders

		#region Encoders

		public static string CreateNoteUploadJson (Dictionary<string, Note> notes)
		{
			throw new NotImplementedException ();
		}

		#endregion Encoders
	}
}

