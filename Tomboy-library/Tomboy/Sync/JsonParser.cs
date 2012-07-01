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
using Newtonsoft.Json.Linq;
namespace Tomboy.Sync
{
    public class JsonParser
    {
	public JsonParser ()
	{
	}

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

			string user = (string)json["user-ref"]["href"];

			user = user.TrimEnd ('/');

			string[] userArray = user.Split ('/');
			return userArray[userArray.Length -1];
		}
    }
}

