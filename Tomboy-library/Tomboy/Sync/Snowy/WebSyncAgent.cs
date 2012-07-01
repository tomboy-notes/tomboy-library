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

namespace Tomboy.Sync.Snowy
{
	public class WebSyncAgent : ISyncAgent
	{
		private OAuthSession session;
		private IToken requestToken;
		private IToken accessToken;
		protected string serverRootUrl;
		private string username;

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
	
		//TODO All of this should be more background thread-like
		//TODO: We have like zero error handling for simple stuff like the internet not being available
		/// <summary>
		/// Sets up new connection and starts Oauth autentication, app must call this once when setting up a new sync provider.
		/// </summary>
		/// <param name='serverUrl'>
		/// Server URL.
		/// </param>
		public string StartSettingUpNewConnection (string serverUrl)
		{
			serverRootUrl = serverUrl.TrimEnd ('/') + "/api/1.0/"; //Make sure traling slash is right TODO: Write a test for this, make sure it works fo rstuff like U1 Notes
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
			this.requestToken = session.GetRequestToken();

			//return the authorisation URL that the user has to go to verify identity TODO: Error handling would probably be smart here
			return session.GetUserAuthorizationUrlForToken (requestToken);
		}

		/// <summary>
		/// Finishes the connection setup, must only be called after the user has authenticated to the server!
		/// </summary>
		/// <returns>
		/// True if successful (that is, we've been authorised), otherwise false.
		/// </returns>
		public bool FinishSettingUpConnection ()
		{
			//Exchange the request token for access token
			this.accessToken = session.ExchangeRequestTokenForAccessToken (this.requestToken);

			//Get the username from the authenticated response 
			//(https://edge.tomboy-online.org/api/1.0/ -> user-ref -> api-ref
			//the user/api-refs are not available in an unauthenticated answer, so this is an implicit test of 
			//the connection being successful.

			this.username = GetAuthenticatedUserName ();

			//Store all the session details TODO

			StoreWebSyncDetails ();

			return true;
		}

		public void ClearWebSyncDetails ()
		{
		}

		protected OAuthEndPoints RequestServiceOAuthEndPoints ()
		{
			string response = GetRootNodeResponse ();
			return JsonParser.ParseRootLevelResponseForOAuthDetails (response);
		}

		private string GetAuthenticatedUserName ()
		{
			string response = GetRootNodeResponse ();
			return JsonParser.ParseRootLevelResponseForUserName (response);
		}

		private string GetRootNodeResponse ()
		{
			string response = string.Empty;
			ServicePointManager.CertificatePolicy = new CertificateManager ();
			HttpWebRequest request = WebRequest.Create (this.serverRootUrl) as HttpWebRequest;
			request.Method = "GET";
			request.ServicePoint.Expect100Continue = false;
			using (var responseReader = new StreamReader (request.GetResponse ().GetResponseStream ())) {
				response = responseReader.ReadToEnd ();
			}

			return response;
		}

		void StoreWebSyncDetails ()
		{
			ParentEngine.
		}

		void FetchWebSyncStoredDetails ()
		{
			throw new NotImplementedException ();
		}

		/// <summary>
		/// Performs the sync operation, with two-way merging.
		/// </summary>
		public void PerformSync ()
		{
		}
	
		/// <summary>
		/// Performs a one-way sync, overwriting all server notes with local notes.
		/// </summary>
		public void CopyFromLocal ()
		{
		}
	
		/// <summary>
		/// Performs a one-way sync, overwriting all local notes with notes from the server.
		/// </summary>
		public void CopyFromRemote ()
		{
		}
	}
}

