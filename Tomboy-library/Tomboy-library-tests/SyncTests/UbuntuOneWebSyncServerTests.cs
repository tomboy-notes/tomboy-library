using System;
using NUnit.Framework;
using DevDefined.OAuth.Framework;
using DevDefined.OAuth.Storage.Basic;
using Tomboy.Sync.Web;
using System.Linq;
using Tomboy.Sync;

namespace Tomboy
{
	[TestFixture]

	// UbuntuOne currently broken
	// although GetAllNotes () works, DeleteNotes() does not
	// and we need this to reset U1 notes to zero before each test
	[Ignore]
	public class UbuntuOneWebSyncServerTests : AbstractSyncServerTests
	{
		protected IToken GetAccessToken ()
		{
			// access tokens can be retrieved with gconf once tomboy is setup for syncing
			// use those paths:
			// /apps/tomboy/sync/tomboyweb/oauth_token
			// /apps/tomboy/sync/tomboyweb/oauth_token_secret

			IToken access_token = new AccessToken ();
			access_token.ConsumerKey = "anyone";
			access_token.Token = "zqkX2sJ0DN2xS2wp7Vjb";
			access_token.TokenSecret = "zjhRkTWWFSJCQdZgr61thWD7qDz7z3t7LT3F9mQ7Hxk0cDV0hqF11xcRR38dLVJxX1Qb3lxCcRN5nwXt";

			return access_token;
		}

		[SetUp]
		public void SetUp ()
		{

			var uri = "https://one.ubuntu.com/notes/";

			this.syncServer = new WebSyncServer (uri, GetAccessToken ());

			// delete all notes on the server before every test
			syncServer.BeginSyncTransaction ();
			var notes = syncServer.GetAllNotes (false);
			syncServer.DeleteNotes (notes.Select (n => n.Guid).ToList ());

			notes = syncServer.GetAllNotes (false);
			Assert.AreEqual (0, notes.Count);
		}
		[TearDown]
		public new void TearDown ()
		{
		}
	}
}

