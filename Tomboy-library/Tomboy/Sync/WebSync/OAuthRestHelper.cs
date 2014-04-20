//
//  OAuthRestHelper.cs
//
//  Author:
//       Timo Dörr <timo@latecrew.de>
//
//  Copyright (c) 2013-2014 Timo Dörr
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
using System.Linq;
using System.IO;
using ServiceStack.ServiceClient.Web;
using Tomboy.Sync.Web.DTO;
using Tomboy.OAuth;

namespace Tomboy.Sync.Web
{
	// proxy class, that provides connecticity to a remote web synchronization server like snowy/rainy/ubuntu1
	public static class OAuthRestHelper
	{
		// helper extension method to sign each JSON request with OAuth
		public static void SetAccessToken (this JsonServiceClient client, IOAuthToken access_token)
		{
			// we use a request filter to add the required OAuth header
			client.LocalHttpWebRequestFilter += webservice_request => {
				
				RequestMethod method = RequestMethod.GET;
				switch (webservice_request.Method) {
				case "GET":
					method = RequestMethod.GET; break;
				case "POST":
					method = RequestMethod.POST; break;
				case "DELETE":
					method = RequestMethod.DELETE; break;
				case "PUT":
					method = RequestMethod.PUT; break;
				}
				
				var auth_header = OAuthConnection.GenerateAuthorizationHeader (access_token,
					webservice_request.RequestUri, method, null);
				
				webservice_request.Headers ["Authorization"] = auth_header;
			};
		}
	}
}
