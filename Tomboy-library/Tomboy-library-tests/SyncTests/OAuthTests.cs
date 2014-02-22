//
//  OAuthTests.cs
//
//  Author:
//       td <>
//
//  Copyright (c) 2014 td
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
using NUnit.Framework;
using System;
using ServiceStack.ServiceClient.Web;
using Tomboy.Sync.Web.DTO;
using System.Net;
using Tomboy.OAuth;
using System.Linq;

namespace Tomboy
{
	[TestFixture ()]
	public class OAuthTests
	{
		string serverBaseUrl = "https://rpi.orion.latecrew.de/";
		string serverFastAuthUrl;
		JsonServiceClient client;

		[SetUp]
		public void Setup ()
		{
			ServicePointManager.CertificatePolicy = new DummyCertificateManager ();
			serverFastAuthUrl = serverBaseUrl + "testuser/testpass/";
		}
		
		[TearDown]
		public void TearDown ()
		{
		}
		
		[Test ()]
		public void TestTokenExchange ()
		{
			var token = TokenExchange ();
			Assert.That (token.Token.Length > 20);
			Assert.That (token.Secret.Length > 12);	
		}
		public IOAuthToken TokenExchange ()
		{
			var oauth = new OAuthConnection (serverFastAuthUrl);
			var client = new JsonServiceClient ();
			
		
			string callback_url = "https://localhost:8081/";
		
			string link_to_open_for_user = "";
			link_to_open_for_user = oauth.GetAuthorizationUrl (callback_url);
			
			// so, we don't want to present the user an url in the unit test
			HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create (link_to_open_for_user);
			req.AllowAutoRedirect = false;
			// the oauth_verifier we need, is part of the querystring in the (redirection)
                        // 'Location:' header
                        string location = ((HttpWebResponse)req.GetResponse ()).Headers ["Location"];
                        var query = string.Join ("", location.Split ('?').Skip (1));
			var oauth_data = System.Web.HttpUtility.ParseQueryString (query);
			string verifier = oauth_data["oauth_verifier"];
			
			bool result = oauth.GetAccessAfterAuthorization (verifier);
			var token = oauth.AccessToken;

			
			return token;
		}
		
		[Test ()]
		public void SimpleAuthenticatedRequest ()
		{
			client = new JsonServiceClient (serverBaseUrl);
			var oauth = new OAuthConnection (serverBaseUrl);
			var access_token = TokenExchange ();
			client.LocalHttpWebRequestFilter += webservice_request => {
				var auth_header = OAuthConnection.GenerateAuthorizationHeader (access_token, webservice_request.RequestUri, RequestMethod.GET, "");
				webservice_request.Headers.Add ("Authorization", auth_header);
			};
			var resp = client.Get<UserResponse> (serverBaseUrl + "/api/1.0/testuser");
		}
	}
}

