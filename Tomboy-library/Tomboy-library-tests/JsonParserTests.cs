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

	}
}

