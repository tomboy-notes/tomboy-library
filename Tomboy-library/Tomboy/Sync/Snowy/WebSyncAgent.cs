// 
//  WebSyncAgent.cs
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
using System.Net;
using System.Web;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using System.IO;
using System.Collections.Generic;

namespace Tomboy.Sync.Snowy
{
	public class WebSyncAgent : ISyncAgent
	{
		/// <summary>
		/// Provides a predefined set of signature algorithms that are supported officially by the protocol
		/// </summary>
		public enum SignatureType
		{
			HMACSHA1,
			PLAINTEXT,
			RSASHA1
		}

		private OAuthSession session;

		/// <summary>
		/// Gets the request token.
		/// </summary>
		/// <description>Acquire a Request Token. This essentially acts as a session identifier for the authorisation process.</description>
		/// <value>
		/// The request token.
		/// </value>
		public IToken requestToken {
			get {
				return session.GetRequestToken ();
			}
		}
		private IToken accessToken;
		protected string ServerApiRootUrl;
		private string username;
		private string verifier;


		#region public methods


		/// <summary>
		/// Begins the SSO with u1.
		/// <description>Yes currently this method is designed for Ubuntu One.
		/// Yes we this should be generic for any oAuth service or other types of SSO servcies.
		/// Also this possibly should be broken out, but for it intends to start the SSO service with U1
		/// so that we can get a request token, maybe an authorization token too.
		/// </description>
		/// </summary>
		/// <returns>
		/// <c>true</c>, if SSO with u1 was begun, <c>false</c> otherwise.
		/// </returns>
		/// <param name='username'>
		/// Username.
		/// </param>
		/// <param name='password'>
		/// Password.
		/// </param>
		public bool BeginSSOWithU1 (string username, string password)
		{
			WebRequest request = WebRequest.Create ("https://login.ubuntu.com/api/1.0/authentications?ws.op=authenticate&token_name=Tomboy on " + 
			                                        System.Environment.MachineName);
			//Using this method of U1 SSO we don't have to open a webpage to handle the handshakes.
			//Examples can be find at https://one.ubuntu.com/developer/account_admin/auth/otherplatforms
			request.Credentials = new NetworkCredential(username, password);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
			Console.WriteLine ("Response Description {0}", response.StatusDescription);

			Stream dataStream = response.GetResponseStream ();
			// Open the stream using a StreamReader for easy access.
			StreamReader reader = new StreamReader (dataStream);
			// Read the content. 
			string responseFromServer = reader.ReadToEnd ();
		// At this point we have a response from the server, which contains the pieces we need.
		// This needs to be parsed from the JSON format so we can retreive the keys.
		/*
		 * 
		 * 
		 * {
		...."name": "Ubuntu One @ $hostname [$application]",
		    "consumer_key": "$consumer_key",
		    "consumer_secret": "$consumer_secret",
		    "token": "$token",
		    "token_secret": "$token_secret"
		} 
		*/
			// Display the content.
			Console.WriteLine (responseFromServer);
			reader.Close ();
			dataStream.Close ();
			response.Close ();

			var consumerContext = new OAuthConsumerContext
			{
				ConsumerKey = "ubuntuone",
				ConsumerSecret = "hammertime",
				Realm = "Snowy"
			};

		// at this point things fall apart.
		// I'm not sure how to build the oAuth object and make the request to U1 to finish so that the tokens are stored.
		// also these tokens need to be returned to the calling Tomboy app so that they can be stored.
		// Although it does appear that we were thinking of possibly storing this in a configuration and that would be extendable by an interface
			request = WebRequest.Create ("https://one.ubuntu.com/oauth/sso-finished-so-get-tokens/");
			request.Credentials = new NetworkCredential(username, password);
			response = (HttpWebResponse)request.GetResponse ();
			Console.WriteLine ("Response Description {0}", response.StatusDescription);
			return true;
		}

		public WebSyncAgent ()
		{
		}

		public WebSyncAgent (Engine parent)
		{
			this.ParentEngine = parent;
			//Check if there are details of an existing connection stored and if so, use them
			// to set up a session, and the server root TODO: Finish it
		}

		public Engine ParentEngine {
			get;
			set;
		}

		public string UserName {
			get {
				return username;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Tomboy.Sync.Snowy.WebSyncAgent"/> is ready to sync.
		/// </summary>
		/// <value>
		/// <c>true</c> if is ready; otherwise, <c>false</c>.
		/// </value>
		public bool IsReady {
			get {
				return accessToken != null;
			}
		}
	
		//TODO All of this should be more background thread-like
		//TODO: We have like zero error handling for simple stuff like the internet not being available
		/// <summary>
		/// Sets up new connection and starts Oauth autentication, app must call this once when setting up a new sync provider.
		/// </summary>
		/// <param name='serverUrl'>
		/// Server URL.
		/// </param>
		/// <param name="callbackUrl">
		/// The Callback that the server should user. You will need to extract a OAuthverifier string from the callback paramaters and use it in FinishSettingUpWebSyncConnection
		/// </param>
		public string StartSettingUpNewWebSyncConnection (string serverUrl, string callbackUrl)
		{
			ServerApiRootUrl = serverUrl.TrimEnd ('/') + "/api/1.0/"; //Make sure traling slash is right TODO: Write a test for this, make sure it works fo rstuff like U1 Notes
			//Contact the Server to request access endpoints 
			OAuthEndPoints endpoints = RequestServiceOAuthEndPoints ();

			//Construct the session
			var consumerContext = new OAuthConsumerContext
			{
				ConsumerKey = "anyone",
				ConsumerSecret = "anyone",
				Realm = "Snowy"
			};


			//Set up session and get the request token

			this.session = new OAuthSession (consumerContext, endpoints.requestUrl, endpoints.userAuthorizeUrl, endpoints.accessUrl);
			this.session.GetRequestToken();

			//return the authorisation URL that the user has to go to verify identity TODO: Error handling would probably be smart here
			return session.GetUserAuthorizationUrlForToken (requestToken, callbackUrl);
		}

		/// <summary>
		/// Setups the Sync Server URL. This is the URL that you will be syncing Notes with
		/// </summary>
		/// <param name='serverUrl'>
		/// Server URL.
		/// </param>
		public void SetupServerUrl (string serverUrl)
		{
			ServerApiRootUrl = serverUrl.TrimEnd ('/') + "/api/1.0/"; //Make sure traling slash is right TODO: Write a test for this, make sure it works fo rstuff like U1 Notes
		}

		/// <summary>
		/// Finishes the connection setup, must only be called after the user has authenticated to the server!
		/// </summary>
		/// <returns>
		/// True if successful (that is, we've been authorised), otherwise false.
		/// </returns>
		public bool FinishSettingUpWebSyncConnection (string verifier)
		{
			//Exchange the request token for access token TODO: This needs to be errorproofed for error 401 Unauthorised
			this.accessToken = session.ExchangeRequestTokenForAccessToken (this.requestToken, verifier);

			//Get the username from the authenticated response 
			//(https://edge.tomboy-online.org/api/1.0/ -> user-ref -> api-ref
			//the user/api-refs are not available in an unauthenticated answer, so this is an implicit test of 
			//the connection being successful.

			this.username = GetAuthenticatedUserName ();

			//Store all the session details TODO
			this.verifier = verifier;
			StoreWebSyncDetails ();

			return true;
		}

		public void ClearWebSyncConnectionDetails ()
		{
			ParentEngine.SetConfigVariable ("websync_accesstoken_token", string.Empty);
			ParentEngine.SetConfigVariable ("websync_accesstoken_tokensecret", string.Empty);
			ParentEngine.SetConfigVariable ("websync_accesstoken_sessionhandle", string.Empty);
			ParentEngine.SetConfigVariable ("websync_accesstoken_verifier", string.Empty);

			ParentEngine.SetConfigVariable ("websync_username", string.Empty);
			ParentEngine.SetConfigVariable ("websync_serverrooturl", string.Empty);
		}

		public OAuthEndPoints RequestServiceOAuthEndPoints ()
		{
			string response = string.Empty;
			ServicePointManager.CertificatePolicy = new CertificateManager ();
			HttpWebRequest request = WebRequest.Create (ServerApiRootUrl) as HttpWebRequest;
			request.Method = WebRequestMethod.GET.ToString ();
			request.ServicePoint.Expect100Continue = false;

			using (var responseReader = new StreamReader (request.GetResponse ().GetResponseStream ())) {
				response = responseReader.ReadToEnd ();
			}

			return JsonParser.ParseRootLevelResponseForOAuthDetails (response);
		}

		#endregion public regions

		#region private regions

		private string GetAuthenticatedUserName ()
		{
			string response = session.Request().Get().ForUrl(ServerApiRootUrl).ToString();
			return JsonParser.ParseRootLevelResponseForUserName (response);
		}

		#endregion private regions

		void StoreWebSyncDetails ()
		{
			ParentEngine.SetConfigVariable ("websync_accesstoken_token", accessToken.Token);
			ParentEngine.SetConfigVariable ("websync_accesstoken_tokensecret", accessToken.TokenSecret);
			ParentEngine.SetConfigVariable ("websync_accesstoken_sessionhandle", accessToken.SessionHandle);
			ParentEngine.SetConfigVariable ("websync_accesstoken_verifier", this.verifier);

			ParentEngine.SetConfigVariable ("websync_username", username);
			ParentEngine.SetConfigVariable ("websync_serverrooturl", ServerApiRootUrl);
		}

		void FetchWebSyncStoredDetails ()
		{
			try {
				IToken newToken = new TokenBase
				{
					ConsumerKey = "ubuntuone",
					Realm = "Snowy",
					Token = ParentEngine.GetConfigVariable ("websync_accesstoken_token"),
					TokenSecret = ParentEngine.GetConfigVariable ("websync_accesstoken_tokensecret"),
					SessionHandle = ParentEngine.GetConfigVariable ("websync_accesstoken_sessionhandle")
				};
				accessToken = newToken;
				verifier = ParentEngine.GetConfigVariable ("websync_verifier");
				username = ParentEngine.GetConfigVariable ("websync_username");
				ServerApiRootUrl = ParentEngine.GetConfigVariable ("websync_serverrooturl");
			} catch (TomboyException ex) {
				throw new TomboyException ("Could not fetch stored details, probably there are no details stored.");
			}

			//TODO! Need to rebuild the session as well
		}

		/// <summary>
		/// Performs the sync operation, with two-way merging.
		/// </summary>
		public void PerformSync ()
		{
			throw new NotImplementedException ();
		}
	
		/// <summary>
		/// Performs a one-way sync, overwriting all server notes with local notes.
		/// </summary>
		public void CopyFromLocal ()
		{
			string changeJson = JsonParser.CreateNoteUploadJson (ParentEngine.GetNotes(), null, 0);

			string response = session.Request().Put().ForUrl(ServerApiRootUrl + username + "/notes").WithRawContentType("application/json").WithBody(changeJson).ToString();

			//DEbug
			Console.WriteLine (response);
		}
	
		/// <summary>
		/// Performs a one-way sync, overwriting all local notes with notes from the server. Make sure to check IsReady before trying sync.
		/// </summary>
		public void CopyFromRemote ()
		{
			string response = session.Request().Get().ForUrl(ServerApiRootUrl + username + "/notes?include_notes=true").ToString();
			NoteChanges changes = JsonParser.ParseCompleteNotesResponse (response);

			//Empty out all old notes
			foreach (var note in ParentEngine.GetNotes ().Values) {
				ParentEngine.DeleteNote (note);
			}

			//Put in and save our new ones
			ParentEngine.AddAndSaveNotes (changes.ChangedNotes);

		}
	}
}

