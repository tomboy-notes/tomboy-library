// Permission is hereby granted, free of charge, to any person obtaining 
// a copy of this software and associated documentation files (the 
// "Software"), to deal in the Software without restriction, including 
// without limitation the rights to use, copy, modify, merge, publish, 
// distribute, sublicense, and/or sell copies of the Software, and to 
// permit persons to whom the Software is furnished to do so, subject to 
// the following conditions: 
//  
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software. 
//  
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION 
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 
// 
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com) 
// Copyright (c) 2014 Timo Dörr <timo@latecrew.de>
// 
// Authors: 
//      Sandy Armstrong <sanfordarmstrong@gmail.com>
//      Timo Dörr <timo@latecrew.de>
// Based on code from:
//      Bojan Rajkovic <bojanr@brandeis.edu>
//      Shannon Whitley <swhitley@whitleymedia.com>
//      Eran Sandler <http://eran.sandler.co.il/>
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Tomboy;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using ServiceStack.ServiceClient.Web;
using Tomboy.Sync.Web.DTO;
using ServiceStack;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Tomboy.OAuth
{
	public class OAuthConnection : OAuthBase
	{
		bool useNonInteractiveAuth;
	
		string rootUrl;
		public string RootUrl {
			get { return rootUrl;}
			set {
				if (!Uri.IsWellFormedUriString (value, UriKind.Absolute))
					throw new ArgumentException ("rootUrl not a valid URI");
				rootUrl = value;
				if (!rootUrl.EndsWith ("/"))
					rootUrl += "/";
			}
		}
				
		/// <summary>
		/// Will hold the access token after successfull authorization
		/// </summary>
		/// <value>The access token.</value>
		public IOAuthToken AccessToken { get; set; }
		private IOAuthToken RequestToken;

		private ApiResponse apiRoot;
		public const string ConsumerKey = "anyone";
		public const string ConsumerSecret = "anyone";
		public const string Realm = "Snowy";

		public string CallbackUrl { get; private set; }
		string verifier;

		
		public OAuthConnection (string rootUrl, bool useNonInteractiveAuth = false)
		{
			this.useNonInteractiveAuth = useNonInteractiveAuth;
			this.rootUrl = rootUrl;
		}

		#region Public Authorization Methods
		
		void GetRootApiRef ()
		{
			var rest_client = new JsonServiceClient ();
			apiRoot = rest_client.Get<ApiResponse> (rootUrl+ "api/1.0/");
		}
		
		public string GetAuthorizationUrl (string callbackUrl)
		{
			this.CallbackUrl = callbackUrl;
			GetRootApiRef ();
			
			var response = Post (apiRoot.OAuthRequestTokenUrl, null, string.Empty);

			if (response.Length == 0) {
				throw new Exception ();
			}

			// Response contains token and token secret.  We only need the token until we're authorized.
			var qs = System.Web.HttpUtility.ParseQueryString (response);
			if (string.IsNullOrEmpty (qs ["oauth_token"])) {
				throw new Exception ("Error reading oauth_token");
			}
			this.RequestToken = new OAuthToken {
				Token = qs ["oauth_token"],
				Secret = qs ["oauth_token_secret"]
			};
			var link = string.Format ("{0}?oauth_token={1}&oauth_callback={2}", apiRoot.OAuthAuthorizeUrl, RequestToken.Token, Uri.EscapeUriString (CallbackUrl));
			return link;
		}

		public bool GetAccessAfterAuthorization (string verifier)
		{
			this.verifier = verifier;
			if (RequestToken == null)
				throw new Exception ("RequestToken");

			var response = Post (apiRoot.OAuthAccessTokenUrl, null, string.Empty);

			if (response.Length == 0) {
				throw new Exception ("received empty response for OAuth authorization");
			}
			//Store the Token and Token Secret
			var qs = System.Web.HttpUtility.ParseQueryString (response);
			AccessToken = new OAuthToken ();
			if (!string.IsNullOrEmpty (qs ["oauth_token"]))
				AccessToken.Token = qs ["oauth_token"];
			if (!string.IsNullOrEmpty (qs ["oauth_token_secret"]))
				AccessToken.Secret = qs ["oauth_token_secret"];
			return true;
		}
		#endregion

		public string Get (string uri, IDictionary<string, string> queryParameters)
		{
			return WebRequest (RequestMethod.GET,
			                   BuildUri (uri, queryParameters),
			                   null);
		}
		
		public string Delete (string uri, IDictionary<string, string> queryParameters)
		{
			return WebRequest (RequestMethod.DELETE,
			                   BuildUri (uri, queryParameters),
			                   null);
		}
		
		public string Put (string uri, IDictionary<string, string> queryParameters, string putValue)
		{
			return WebRequest (RequestMethod.PUT,
			                   BuildUri (uri, queryParameters),
			                   putValue);
		}
		
		public string Post (string uri, IDictionary<string, string> queryParameters, string postValue)
		{
			return WebRequest (RequestMethod.POST,
			                   BuildUri (uri, queryParameters),
			                   postValue);
		}


		#region Private Methods
//		/// <summary>
//		/// Submit a web request using OAuth, asynchronously.
//		/// </summary>
//		/// <param name="method">GET or POST.</param>
//		/// <param name="url">The full URL, including the query string.</param>
//		/// <param name="postData">Data to post (query string format), if POST methods.</param>
//		/// <param name="callback">The callback to call with the web request data when the asynchronous web request finishes.</param>
//		/// <returns>The return value of QueueUserWorkItem.</returns>
//		public bool AsyncWebRequest (RequestMethod method, string url, string postData, Action<string> callback)
//		{
//			return ThreadPool.QueueUserWorkItem (new WaitCallback (delegate {
//				callback (WebRequest (method, url, postData));
//			}));
//		}

		/// <summary>
		/// Submit a web request using OAuth.
		/// </summary>
		/// <param name="method">GET or POST.</param>
		/// <param name="url">The full URL, including the query string.</param>
		/// <param name="postData">Data to post (query string format), if POST methods.</param>
		/// <returns>The web server response.</returns>
		private string WebRequest (RequestMethod method, string url, string postData)
		{
			Uri uri = new Uri (url);

			var nonce = GenerateNonce ();
			var timeStamp = GenerateTimeStamp ();

			var outUrl = string.Empty;
			List<IQueryParameter<string>> parameters = null;

			
			string callbackUrl = string.Empty;
			if (url.StartsWith (apiRoot.OAuthRequestTokenUrl)) {
				callbackUrl = CallbackUrl;
			}

			IOAuthToken token = new OAuthToken { Token = "", Secret = "" };
			// for the access token exchange we need to supply the request token
			if (url.StartsWith (apiRoot.OAuthAccessTokenUrl)) {
				token = this.RequestToken;
				callbackUrl = CallbackUrl;
			} else if (this.AccessToken != null) {
				token = this.AccessToken;
			}
			var sig = GenerateSignature (uri, ConsumerKey, ConsumerSecret, token, verifier, method,
			                             timeStamp, nonce, callbackUrl, out outUrl, out parameters);


			parameters.Add (new QueryParameter<string> ("oauth_signature",
				Uri.EscapeUriString (sig),
			        s => string.IsNullOrEmpty (s))
			);
			parameters.Sort ();

			var ret = MakeWebRequest (method, url, parameters, postData);

			return ret;
		}
		public static string GenerateAuthorizationHeader (IOAuthToken accessToken, Uri uri, RequestMethod method, string postData)
		{
			var oauth = new OAuthConnection (uri.ToString());
			oauth.AccessToken = accessToken;
			return oauth.GenerateAuthorizationHeader (uri, method, postData);
		}
		
		public string GenerateAuthorizationHeader (Uri uri, RequestMethod method, string postData)
		{
			List<IQueryParameter<string>> parameters = null;
			string outUrl = "";
			var nonce = GenerateNonce ();
			var timeStamp = GenerateTimeStamp ();
			
			if (AccessToken == null)
				throw new Exception ("AccessToken not set");
		
			var sig = GenerateSignature (uri, ConsumerKey, ConsumerSecret, AccessToken, verifier, method,
			                             timeStamp, nonce, "", out outUrl, out parameters);
			
			var headerParams =
				parameters.Implode (",", q => string.Format ("{0}=\"{1}\"", q.Name, q.Value));
			var auth_header = String.Format ("OAuth realm=\"{0}\",{1}", Realm, headerParams);
			return auth_header;
		}
		/// <summary>
		/// Wraps a web request into a convenient package.
		/// </summary>
		/// <param name="method">HTTP method of the request.</param>
		/// <param name="url">Full URL to the web resource.</param>
		/// <param name="postData">Data to post in query string format.</param>
		/// <returns>The web server response.</returns>
		private string MakeWebRequest (RequestMethod method,
		                               string url,
		                               List<IQueryParameter<string>> parameters,
		                               string postData)
		{
			var responseData = string.Empty;

			// TODO: Set UserAgent, Timeout, KeepAlive, Proxy?
			var http_client = new HttpClient ();
			var message = new HttpRequestMessage ();
			message.RequestUri = new Uri (url);
			switch (method) {
			case RequestMethod.GET:
				message.Method = System.Net.Http.HttpMethod.Get;
				break;
			case RequestMethod.POST:
				message.Method = System.Net.Http.HttpMethod.Post;
				break;
			default: throw new NotImplementedException ();
			}
			
			var headerParams =
				parameters.Implode (",", q => string.Format ("{0}=\"{1}\"", q.Name, q.Value));
			string authorization_header = String.Format ("OAuth realm=\"{0}\",{1}",
				Realm, headerParams);
			
			message.Headers.Add ("Authorization", authorization_header);

			if (postData == null) {
				postData = string.Empty;
			}

			HttpResponseMessage resp;
			if (method == RequestMethod.PUT ||
			     method == RequestMethod.POST) {
				var content = new StringContent (postData, Encoding.UTF8, "application/json");
				message.Content = content;
			}
			resp = http_client.SendAsync (message).Result;

			return resp.Content.ReadAsStringAsync ().Result;
		}

		private string BuildUri (string baseUri, IDictionary<string, string> queryParameters)
		{
			StringBuilder urlBuilder = new StringBuilder (baseUri);	// TODO: Capacity?
			urlBuilder.Append ("?");
			if (queryParameters != null) {
				foreach (var param in queryParameters) {
					urlBuilder.Append (param.Key);
					urlBuilder.Append ("=");
					urlBuilder.Append (param.Value);
					urlBuilder.Append ("&");
				}
			}
			// Get rid of trailing ? or &
			urlBuilder.Remove (urlBuilder.Length - 1, 1);
			return urlBuilder.ToString ();
		}
		#endregion
	}
}