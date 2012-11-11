// 
//  ApiRootResponse.cs
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
namespace Tomboy.Sync.Snowy
{
    public struct OAuthEndPoints
    {
		public string requestUrl;
		/// <summary>
		/// The user authorize URL.
		/// </summary>
		/// <description>User Authorises Access
		/// At this point, the user needs to agree to provide your web site access to their data. 
		/// This is achieved by redirecting the user to the User Authorization URL end point with the request token included in the URL
		/// </description>
		public string userAuthorizeUrl;
		public string accessUrl;
    }
}

