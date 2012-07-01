// 
//  OAuthTester.cs
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
using Tomboy;
using Tomboy.Sync.Snowy;
namespace OAuthSetUpTester
{
	public class OAuthTester
	{
		public OAuthTester ()
		{
		}

		public static void  Main (string[] args)
		{
			Console.WriteLine ("Manual OAuth tester for Tomboy-lib (Can't be automated due to user interaction being needed.)");
			Console.WriteLine ("Starting setup process...");

			Engine engine = new Engine (new DummyStorage ());

			WebSyncAgent agent = new WebSyncAgent (engine);
			engine.SyncAgent = agent;

			string authUrl = agent.StartSettingUpNewWebSyncConnection ("https://edge.tomboy-online.org/", "http://projects.gnome.org/tomboy/");

			Console.WriteLine ("Please visit the link provided, find the OAuth_verifier info from the callback URL, then come back and enter it:");
			Console.WriteLine (authUrl);
			string verifier = Console.ReadLine ();

			bool finished = agent.FinishSettingUpWebSyncConnection (verifier);

			if (finished)
				Console.WriteLine ("Success! Username is " + agent.UserName);
			else
				Console.WriteLine ("Failure! Something went wrong.");

			Console.WriteLine ("Press return to finish");

			Console.ReadLine ();

		}
	}
}

