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
		// From original Tomboy.NoteArchiver
		// NOTE: If this changes from a standard format, make sure to update
		//       XML parsing to have a DateTime.TryParseExact
		private const string DATE_TIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";

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

		public static NoteChanges  ParseCompleteNotesResponse (string response)
		{
			Dictionary<string, Note> changed = new Dictionary<string, Note> ();
			List<string> deleted = new List<string> ();

			JObject json = JObject.Parse (response);

			foreach (var noteJson in json["notes"]) {
				if (noteJson["command"] == null) {
					Note newNote = new Note ("note://tomboy/" + noteJson["guid"]);
					newNote.ChangeDate = (DateTime) noteJson["last-change-date"];
					newNote.CreateDate = (DateTime) noteJson["create-date"];
					newNote.MetadataChangeDate = (DateTime) noteJson["last-metadata-change-date"];
					newNote.Text = (string) noteJson["note-content"];
					newNote.Title = (string) noteJson["title"];
					
					foreach (var tagJson in noteJson["tags"]) {
						newNote.Tags.Add (tagJson.ToString (), new Tags.Tag (tagJson.ToString ()));
					}
					changed.Add (newNote.Uri, newNote);
				} else {
					deleted.Add ((string) noteJson["guid"]);
				}
			}
			NoteChanges toRet = new NoteChanges ();

			toRet.SyncRevision = (int) json["latest-sync-revision"];
			toRet.ChangedNotes = changed;
			toRet.DeletedNoteGuids = deleted;

			return toRet; //TODO
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

		/// <summary>
		/// Creates the note upload json.
		/// </summary>
		/// <returns>
		/// The note upload json.
		/// </returns>
		/// <param name='changedNotes'>
		///  A dict of changed notes that are to have their changes uploaded
		/// </param>
		/// <param name='deletedNotes'>
		/// A dict of notes that have been deleted locally and should be removed from the server.
		/// </param>
		public static string CreateNoteUploadJson (Dictionary<string, Note> changedNotes, Dictionary<string, Note> deletedNotes, int revision)
		{
			JObject json = new JObject ();
			json.Add ("latest-sync-revision", revision);
			json.Add ("note-changes", new JArray ());

			foreach (Note note in changedNotes.Values) {
				JObject noteJson = new JObject ();
				noteJson.Add ("title", note.Title);
				noteJson.Add ("guid", note.Uri.Replace ("note://", ""));
				noteJson.Add ("note-content", note.Text);
				noteJson.Add ("last-change-date", note.ChangeDate.ToString (DATE_TIME_FORMAT));
				noteJson.Add ("last-metadata-change-date", note.MetadataChangeDate.ToString (DATE_TIME_FORMAT));
				noteJson.Add ("create-date", note.CreateDate.ToString (DATE_TIME_FORMAT));

				noteJson.Add ("tags", new JArray ());
				foreach (Tags.Tag tag in note.Tags.Values) {
					((JArray) noteJson["tags"]).Add (tag.NormalizedName);
				}


				((JArray) json["note-changes"]).Add (noteJson);
			}

			foreach (Note note in deletedNotes.Values) {
				JObject noteJson = new JObject ();
				noteJson.Add ("guid", note.Uri.Replace ("note://", ""));
				noteJson.Add ("command", "delete");
				((JArray) json["note-changes"]).Add (noteJson);
			}

			return json.ToString ();
		}

		#endregion Encoders
	}
}

