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
using System.Collections.Generic;
using Tomboy.Sync.Web.DTO;
using System.Linq;
using ServiceStack.ServiceClient.Web;
using Tomboy.OAuth;
using System.Net;
using System.Net.Http;

namespace Tomboy.Sync.Web
{
	/// <summary>
	/// OAuth authorization callback. Will provide the callbackUrl that is sent back by the server. Usually, this url
	/// has to be opened in a browser to present it to the user. The user will perform authentication, and if succesfull,
	/// be redirected to the callbackUrl previously provided (NOT the callbackurl passed as parameter to this delegate).
	/// 
	/// This is the pattern that you basically should follow when implementing this delegate:
	/// 0. Setup a HttpListener or similiar on localhost, that you control and generate a link to that server - should be done
	///    BEFORE this delegate is called. This url is passed to <see cref="PerformTokenExchange"/>.
	/// 1. Once the callback fires, open a browser and point to the supplied callbackUrl argument (NOT the url to your HttpListener)
	/// 2. After the user successfully authenticated, he will be redirected to your HttpListener's url, and will have a ?oauth_verifier
	///    append to the query string of the url
	/// 3. You make sure to extract that oauth_verifier value and return it in this callback
	/// </summary>
	public delegate string OAuthAuthorizationCallback (string callbackUrl);
	
	/// <summary>
	/// Proxy class (as in the proxy design pattern) that encapsules communication with a Tomboy sync 
	/// server (like Rainy or Snowy).
	/// </summary>
	public class WebSyncServer : ISyncServer
	{
		private string rootApiUrl;
		private string userServiceUrl;
		private string notesServiceUrl;

		private IOAuthToken accessToken;

		/// <summary>
		/// Initializes a new instance of the <see cref="Tomboy.Sync.Web.WebSyncServer"/> class.
		/// </summary>
		/// <param name="serverUrl">Server URL (without the api/1.0 part).</param>
		/// <param name="accessToken">An pre-obtained access token for OAuth.
		/// Use <see cref="PerformTokenExchange"/> or <see cref="PerformNonInteractiveTokenExchange"/>.
		/// </param>
		public WebSyncServer (string serverUrl, IOAuthToken accessToken)
		{
			rootApiUrl = serverUrl + "/api/1.0";
			this.accessToken = accessToken;

			this.DeletedServerNotes = new List<string> ();
			this.UploadedNotes = new List<Note> ();
		}
		public static IOAuthToken PerformTokenExchange (string serverUrl, string oauthCallbackUrl, OAuthAuthorizationCallback callback)
		{
			var oauth_connection = new OAuthConnection (serverUrl);

			string link_to_open_for_user = "";
			link_to_open_for_user = oauth_connection.GetAuthorizationUrl (oauthCallbackUrl);

			string verifier = callback (link_to_open_for_user);
			bool result = oauth_connection.GetAccessAfterAuthorization (verifier);
			if (result == false)
				throw new UnauthorizedAccessException ();

			var token = oauth_connection.AccessToken;
			return token;
		}
		/// <summary>
		/// Performs the fast token exchange. IMPORTANT: Fast Exchange is not covered by the Tomboy API
		/// specification and is UNSTANDARDIZED API some server expose (currently only Rainy).
		/// </summary>
		/// <description>>
		/// For fast (or direct) token exchange, the username and password is appended to the server url
		/// i.e: If a regular sync server's url is http://localhost:8080/, the username and password
		/// are appended to the url: http://localhost:8080/username/password/. The server detects this
		/// and does not direct the client to a login page, but directly grants a valid OAuth token.
		/// this is ideal for programmatic logins without user interaction (i.e. unit tests).
		/// </description>
		/// <returns>An AccessToken that can be used for all further requests.</returns>
		/// <param name="serverUrl">Server URL (without username or password appended).</param>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		public static IOAuthToken PerformFastTokenExchange (string serverUrl, string username, string password)
		{
			OAuthAuthorizationCallback cb = (callbackUrl) => {
				var http_handler = new HttpClientHandler () {
					AllowAutoRedirect = false
				};
				var http_client = new HttpClient (http_handler);
				var request = http_client.GetAsync (callbackUrl);
				
				// point to browser
				// the oauth_verifier we need, is part of the querystring in the (redirection)
	                        // 'Location:' header
				string location = request.Result.Headers.GetValues ("Location").Single ();
	                        var query = string.Join ("", location.Split ('?').Skip (1));
				var oauth_data = System.Web.HttpUtility.ParseQueryString (query);
				string oauth_verifier = oauth_data["oauth_verifier"];

				// get the oauth_verifier somehow and return
				return oauth_verifier;
			};

			// fictional url as we don't need to be callbacked
			string cburl = "http://localhost:56894/tomboy-fast-token-exchange/";

			// add + "" to break the reference and force value copy
			string fast_url = serverUrl + "";
			if (!serverUrl.EndsWith ("/"))
				fast_url += "/";
			fast_url += username + "/" + password + "/";
			return PerformTokenExchange (fast_url, cburl , cb);
		}
		private JsonServiceClient GetJsonClient ()
		{
			var restClient = new JsonServiceClient ();
			restClient.SetAccessToken (this.accessToken);

			return restClient;
		}

		private void Connect ()
		{
			var restClient = GetJsonClient ();

			// with the first connection we find out the OAuth urls
			var api_response = restClient.Get<ApiResponse> (rootApiUrl);

			// the server tells us the address of the user webservice
			this.userServiceUrl = api_response.UserRef.ApiRef;

			if (api_response.ApiVersion != "1.0") {
				throw new NotImplementedException ("unknown ApiVersion: " + api_response.ApiVersion);
			}

			var user_response = restClient.Get<UserResponse> (this.userServiceUrl);
			this.notesServiceUrl = user_response.NotesRef.ApiRef;

			this.LatestRevision = user_response.LatestSyncRevision;
			this.Id = user_response.CurrentSyncGuid;

		}

		#region ISyncServer implementation

		public bool BeginSyncTransaction ()
		{
			this.UploadedNotes = new List<Note> ();
			this.DeletedServerNotes = new List<string> ();

			Connect ();
			return true;
		}

		public bool CommitSyncTransaction ()
		{
			bool notes_were_deleted_or_uploaded =
				DeletedServerNotes.Count > 0 || UploadedNotes.Count > 0;

			if (notes_were_deleted_or_uploaded)
				this.LatestRevision++;

			return true;
		}

		public bool CancelSyncTransaction ()
		{
			// TODO
			return true;
		}

		public IList<Note> GetAllNotes (bool include_note_content)
		{
			var restClient = GetJsonClient ();

			string url;
			url = this.notesServiceUrl + "?include_notes=" + include_note_content.ToString ();
			var response = restClient.Get<GetNotesResponse> (url);

			return response.Notes.ToTomboyNotes ();
		}

		public IList<Note> GetNoteUpdatesSince (long revision)
		{
			var restClient = GetJsonClient ();

			// we have to add the ?since parameter to our uri
			var notes_request_url = notesServiceUrl + "?since=" + revision;

			var response = restClient.Get<GetNotesResponse> (notes_request_url);

			return response.Notes.ToTomboyNotes ();
		}

		public void DeleteNotes (IList<string> delete_note_guids)
		{
			var restClient = GetJsonClient ();

			// to delete notes, we call PutNotes and set the command to 'delete'
			var request = new PutNotesRequest ();

			request.LatestSyncRevision = (int) this.LatestRevision; 

			request.Notes = new List<DTONote> ();
			foreach (string delete_guid in delete_note_guids) {
				request.Notes.Add (new DTONote () {
					Guid = delete_guid,
					Command = "delete"
				});
				DeletedServerNotes.Add (delete_guid);
			}

			restClient.Put<PutNotesRequest> (notesServiceUrl, request);
//			restClient.Put<PutNotesRequest> ("http://127.0.0.1:8090/johndoe/notes/", request);
		}

		public void UploadNotes (IList<Note> notes)
		{
			var restClient = GetJsonClient ();

			var request = new PutNotesRequest ();
			//request.LatestSyncRevision = this.LatestRevision;
			request.Notes = notes.ToDTONotes ();

			restClient.Put<GetNotesResponse> (notesServiceUrl, request);

			// TODO if conflicts arise, this may be different
			UploadedNotes = notes;
		}

		public bool UpdatesAvailableSince (int revision)
		{
			throw new NotImplementedException ();
		}

		public IList<string> DeletedServerNotes {
			get; private set;
		}

		public IList<Note> UploadedNotes {
			get; private set;
		}

		public long LatestRevision {
			get ; private set;
		}

		public DateTime LastSyncDate {
			get; private set;
		}

		public string Id {
			get; private set;
		}

		#endregion
	}
}

