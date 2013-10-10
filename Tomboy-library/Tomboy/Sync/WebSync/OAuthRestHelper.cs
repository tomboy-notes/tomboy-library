//
//  OAuthRestHelper.cs
//
//  Author:
//       td <>
//
//  Copyright (c) 2013 td
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
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Consumer;
using System.Linq;
using System.IO;
using ServiceStack.ServiceClient.Web;

namespace Tomboy.Sync.Web
{
	// proxy class, that provides connecticity to a remote web synchronization server like snowy/rainy/ubuntu1

	public static class OAuthRestHelper
	{
		// helper extension method to sign each JSON request with OAuth
		public static void SetAccessToken (this JsonServiceClient client, IToken access_token)
		{
			// we use a request filter to add the required OAuth header
			client.LocalHttpWebRequestFilter += webservice_request => {
				
				OAuthConsumerContext consumer_context = new OAuthConsumerContext ();
				
				consumer_context.SignatureMethod = "HMAC-SHA1";
				consumer_context.ConsumerKey = access_token.ConsumerKey;
				consumer_context.ConsumerSecret = "anyone";
				consumer_context.UseHeaderForOAuthParameters = true;
				
				// the OAuth process creates a signature, which uses several data from
				// the web request like method, hostname, headers etc.
				OAuthContext request_context = new OAuthContext ();
				request_context.Headers = webservice_request.Headers;
				request_context.RequestMethod = webservice_request.Method;
				request_context.RawUri = webservice_request.RequestUri;
				
				// now create the signature for that context
				consumer_context.SignContextWithToken (request_context, access_token);
				
				// BUG TODO the oauth_token is not included when generating the header,
				// this is a bug ing DevDefined.OAuth. We add it manually as a workaround
				request_context.AuthorizationHeaderParameters.Add ("oauth_token", access_token.Token);
				
				string oauth_header = request_context.GenerateOAuthParametersForHeader ();
				
				webservice_request.Headers.Add ("Authorization", oauth_header);
				
			};
		}
	}
}
