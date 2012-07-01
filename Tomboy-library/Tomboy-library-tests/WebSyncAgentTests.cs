// 
//  WebSyncAgentTests.cs
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

namespace Tomboy.Sync.Snowy
{
		[TestFixture()]
		public class WebSyncAgentTests : WebSyncAgent
		{
	
		[Test()]
		public void RequestServiceOAuthEndPoints_GoodConnection_GetsEndPoints ()
		{
				//This test actually is dependant on JsonParser (Which is tested individually), not great form, but the important thing is
				// checking that it connects correctly.
				this.serverRootUrl = "https://edge.tomboy-online.org/api/1.0/";
				OAuthEndPoints endPoints =  RequestServiceOAuthEndPoints ();
	
				Assert.AreEqual ("https://edge.tomboy-online.org/oauth/access_token/", endPoints.accessUrl);
				Assert.AreEqual ("https://edge.tomboy-online.org/oauth/request_token/", endPoints.requestUrl);
				Assert.AreEqual ("https://edge.tomboy-online.org/oauth/authenticate/", endPoints.userAuthorizeUrl);
		}

		[Test()]
		[ExpectedException (TomboyException)]
		public void FetchWebSyncStoredDetails_NoStoredDetails_ThrowsException ()
		{
			throw new NotImplementedException ();
		}

		[Test()]
		[ExpectedException (TomboyException)]
		public void PerformWebSync_NoConnectionSetUp_ThrowsException ()
		{
			throw new NotImplementedException ();
		}

		[Test()]
		[ExpectedException (TomboyException)]
		public void CopyFromLocal_NoConnectionSetUp_ThrowsException ()
		{
			throw new NotImplementedException ();
		}

		[Test()]
		[ExpectedException (TomboyException)]
		public void CopyFromRemote_NoConnectionSetUp_ThrowsException ()
		{
			throw new NotImplementedException ();
		}


	}
}

