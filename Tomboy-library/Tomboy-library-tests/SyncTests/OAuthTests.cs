//
//  OAuthTests.cs
//
//  Author:
//       Timo Dörr <timo@latecrew.de>
//
//  Copyright (c) 2014 Timo Dörr
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
using Tomboy.Sync.Web;
using System.Security.Cryptography.X509Certificates;

namespace Tomboy
{
	public class DummyCertificateManager : ICertificatePolicy
	{

		public bool CheckValidationResult (ServicePoint sp, 
						   X509Certificate certificate,
						   WebRequest request,
						   int error)

		{
			return true;
		}
	}

	// please enable this test ONLY when debugging/testing OAuth and consider
	// using your own SyncServer instance (i.e. Rainy) to keep load on the public
	// demo server low.
	[Ignore]
	[TestFixture ()]
	public class OAuthTests
	{
		string serverUrl = "https://rpi.orion.latecrew.de/";
		string testUser = "testuser";
		string testPass ="testpass";
		JsonServiceClient client;

		[SetUp]
		public void Setup ()
		{
			ServicePointManager.CertificatePolicy = new DummyCertificateManager ();
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
		private IOAuthToken TokenExchange ()
		{
			return WebSyncServer.PerformFastTokenExchange (serverUrl, testUser, testPass);
		}
		
		[Test ()]
		public void SimpleAuthenticatedRequest ()
		{
			client = new JsonServiceClient (serverUrl);
			var oauth = new OAuthConnection (serverUrl);
			var access_token = TokenExchange ();
			client.LocalHttpWebRequestFilter += webservice_request => {
				var auth_header = OAuthConnection.GenerateAuthorizationHeader (access_token, webservice_request.RequestUri, RequestMethod.GET, "");
				webservice_request.Headers ["Authorization"] = auth_header;
			};
			var resp = client.Get<UserResponse> (serverUrl + "/api/1.0/" + testUser);
		}
	}
}

