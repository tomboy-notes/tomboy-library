using NUnit.Framework;
using Tomboy.Sync.Web;
using Tomboy.OAuth;
using System.Net;
using ServiceStack.ServiceClient.Web;
using System.Linq;

namespace Tomboy
{

	[TestFixture]
	public class WebSyncServerTests : Tomboy.Sync.AbstractSyncManagerTests
	{
		IOAuthToken accessToken;
		string serverBaseUrl = "https://rpi.orion.latecrew.de/";

		public IOAuthToken TokenExchange (string fastAuthUri)
		{
			var oauth = new OAuthConnection (fastAuthUri);
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
		[SetUp]
		public new void SetUp ()
		{
			ServicePointManager.CertificatePolicy = new DummyCertificateManager ();
			if (accessToken == null) {
				accessToken = TokenExchange (serverBaseUrl + "/emma/oH7Lda/");
			}
			syncServer = new WebSyncServer (serverBaseUrl, accessToken);
			
		}

		[TearDown]
		public new void TearDown ()
		{
		}

		protected override void ClearServer (bool reset = false)
		{
			return;
		}
	}
}
	