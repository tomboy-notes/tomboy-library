using System;
using Tomboy.Sync.Web;
using Tomboy.OAuth;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;

namespace Tomboy.Sync.Web.Developer
{
	/// <summary>
	/// Web sync server developer extensions. Encapsulate features that are not part of the officla Tomboy REST API
	/// spec but rather unofficial additions indented for developer and administration use. May not be supported on all
	/// server implementations.
	/// </summary>
	public class DeveloperServiceClient 
	{
		private IOAuthToken accessToken;
		private string serverBaseUrl;

		public DeveloperServiceClient (string serverUrl, IOAuthToken accessToken)
		{
			serverBaseUrl = serverUrl;
			this.accessToken = accessToken;
		}
		public void ClearAllNotes (string username)
		{
			var client = GetJsonClient ();
			client.Get<IReturnVoid> ("/api/1.0/" + username + "/notes/clear");
		}

		private JsonServiceClient GetJsonClient ()
		{
			var client = new JsonServiceClient (serverBaseUrl);
			client.SetAccessToken (accessToken);

			return client;
		}
	}
}

