//
// OAuthBase.cs
//  
// Author:
//       Timo Dörr <timo@latecrew.de>
//       Bojan Rajkovic <bojanr@brandeis.edu>
//       Shannon Whitley <swhitley@whitleymedia.com>
//       Eran Sandler <http://eran.sandler.co.il/>
//       Sandy Armstrong <sanfordarmstrong@gmail.com>
// 
// Copyright (c) 2009 Bojan Rajkovic
// Copyright (c) 2014 Timo Dörr
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tomboy.OAuth
{
	/// <summary>
	/// Provides a base class for OAuth authentication and signing.
	/// </summary>
	public abstract class OAuthBase
	{
		private const string OAuthVersion = "1.0";

		//
		// List of know and used oauth parameters' names
		//
		private const string OAuthConsumerKeyKey = "oauth_consumer_key";
		private const string OAuthCallbackKey = "oauth_callback";
		private const string OAuthVersionKey = "oauth_version";
		private const string OAuthSignatureMethodKey = "oauth_signature_method";
		private const string OAuthSignatureKey = "oauth_signature";
		private const string OAuthTimestampKey = "oauth_timestamp";
		private const string OAuthNonceKey = "oauth_nonce";
		private const string OAuthTokenKey = "oauth_token";
		private const string OAuthTokenSecretKey = "oauth_token_secret";
		private const string OAuthVerifierKey = "oauth_verifier";

		private const string HMACSHA1SignatureType = "HMAC-SHA1";
		private const string PlainTextSignatureType = "PLAINTEXT";
		private const string RSASHA1SignatureType = "RSA-SHA1";

		private Random random = new Random ();

		private string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";

		/// <summary>
		/// URL encodes a string using OAuth's encoding scheme (slightly different from HttpUtility's UrlEncode).
		/// </summary>
		/// <param name="value">The string to URL encode.</param>
		/// <returns>An URL encoded string.</returns>
		private string UrlEncode (string value)
		{
			var result = new StringBuilder ();

			foreach (char symbol in value) {
				if (unreservedChars.IndexOf(symbol) != -1) result.Append(symbol);
				else result.Append('%' + String.Format("{0:X2}", (int)symbol));
			}

			return result.ToString();
		}

		/// <summary>
		/// Internal function to cut out all non oauth query string parameters.
		/// </summary>
		/// <param name="parameters">The query string part of the URL.</param>
		/// <returns>A list of QueryParameter each containing the parameter name and value.</returns>
		private IEnumerable<IQueryParameter<string>> GetQueryParameters (string parameters)
		{
			return CreateQueryParametersIterator (parameters);
		}

		private IEnumerable<IQueryParameter<string>> CreateQueryParametersIterator (string parameters)
		{
			if (parameters == null) throw new ArgumentNullException ("parameters");
			var parameterDictionary = System.Web.HttpUtility.ParseQueryString (parameters).ToDictionary ();

			foreach (var kvp in parameterDictionary)
				yield return new QueryParameter<string> (kvp.Key, kvp.Value, s => string.IsNullOrEmpty (s));
		}

		/// <summary>
		/// Generate the signature base that is used to produce the signature
		/// </summary>
		/// <param name="url">The full URL that needs to be signed including its non OAuth URL parameters.</param>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="token">The token, if available. If not available pass null or an empty string.</param>
		/// <param name="tokenSecret">The token secret, if available. If not available pass null or an empty string.</param>
		/// <param name="verifier">The callback verifier, if available. If not available pass null or an empty string.</param>
		/// <param name="httpMethod">The HTTP method used. Must be a valid HTTP method verb (POST,GET,PUT, etc)</param>
		/// <param name="signatureType">The signature type. To use the default values use <see cref="SignatureType">SignatureType</see>.</param>
		/// <returns>The signature base.</returns>
		private string GenerateSignatureBase (Uri url, string consumerKey, IOAuthToken token, string verifier,
						      RequestMethod method, TimeSpan timeStamp, string nonce, SignatureType signatureType, string callbackUrl,
						      out string normalizedUrl,
			out List<IQueryParameter<string>> parameters)
		{
			token.Token = token.Token ?? string.Empty;
			token.Secret = token.Secret ?? string.Empty;
			verifier = verifier ?? String.Empty;

			if (consumerKey == null) throw new ArgumentNullException ("consumerKey");

			var signatureString = string.Empty;

			switch (signatureType) {
				case SignatureType.HMACSHA1:
					signatureString = "HMAC-SHA1";
					break;
				case SignatureType.RSASHA1:
					signatureString = "RSA-SHA1";
					break;
				case SignatureType.PLAINTEXT:
					signatureString = SignatureType.PLAINTEXT.ToString ();
					break;
			}

			parameters = GetQueryParameters (url.Query).Concat (new List<IQueryParameter<string>> {
				new QueryParameter<string> (OAuthVersionKey, OAuthVersion, s => string.IsNullOrEmpty (s)),
				new QueryParameter<string> (OAuthTimestampKey, ((long)timeStamp.TotalSeconds).ToString (), s => string.IsNullOrEmpty (s)),
				new QueryParameter<string> (OAuthSignatureMethodKey, signatureString, s => string.IsNullOrEmpty (s)),
				new QueryParameter<string> (OAuthNonceKey, nonce, s => string.IsNullOrEmpty (s)),
				new QueryParameter<string> (OAuthConsumerKeyKey, consumerKey, s => string.IsNullOrEmpty (s))
			}).ToList ();

			if (!string.IsNullOrEmpty (token.Token)) parameters.Add (new QueryParameter<string> (OAuthTokenKey, token.Token, s => string.IsNullOrEmpty (s)));
			if (!string.IsNullOrEmpty (verifier)) parameters.Add (new QueryParameter<string> (OAuthVerifierKey, verifier, s => string.IsNullOrEmpty (s)));
			if (!string.IsNullOrEmpty (callbackUrl)) parameters.Add (new QueryParameter<string> (OAuthCallbackKey, UrlEncode (callbackUrl), s => string.IsNullOrEmpty (s)));

			normalizedUrl = string.Format ("{0}://{1}", url.Scheme, url.Host);
			if (!((url.Scheme == "http" && url.Port == 80) || (url.Scheme == "https" && url.Port == 443))) normalizedUrl += ":" + url.Port;
			normalizedUrl += url.AbsolutePath;

			parameters.Sort ();
			string normalizedRequestParameters = parameters.NormalizeRequestParameters ();

			var signatureBase = new StringBuilder ();
			signatureBase.AppendFormat("{0}&", method.ToString ());
			signatureBase.AppendFormat("{0}&", UrlEncode (normalizedUrl));
			signatureBase.AppendFormat("{0}", UrlEncode (normalizedRequestParameters));

			return signatureBase.ToString ();
		}

		/// <summary>
		/// Generates a signature using the HMAC-SHA1 algorithm
		/// </summary>
		/// <param name="url">The full URL that needs to be signed including its non-OAuth URL parameters.</param>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer seceret.</param>
		/// <param name="token">The token, if available. If not available pass null or an empty string.</param>
		/// <param name="tokenSecret">The token secret, if available. If not, pass null or an empty string.</param>
		/// <param name="verifier">The callback verifier, if available. If not, pass null or an empty string.</param>
		/// <param name="httpMethod">The HTTP method used. Must be valid HTTP method verb (POST, GET, PUT, etc).</param>
		/// <returns>A Base64 string of the hash value.</returns>
		protected string GenerateSignature (Uri url, string consumerKey, string consumerSecret, IOAuthToken token,
		                                    string verifier, RequestMethod method, TimeSpan timeStamp, string nonce,
		                                    string callbackUrl, out string normalizedUrl,
		                                    out List<IQueryParameter<string>> parameters)
		{
			// TODO use HMACSHA1 instead of PLAINTEXT
			// for non-ssl connection.
			return GenerateSignature (url, consumerKey, consumerSecret, token, verifier, method, timeStamp, nonce,
			                          SignatureType.PLAINTEXT, callbackUrl, out normalizedUrl, out parameters);
		}

		/// <summary>
		/// Generates a signature using the specified signature type.
		/// </summary>
		/// <param name="url">The full URL that needs to be signed including its non-OAuth URL parameters.</param>
		/// <param name="consumerKey">The consumer key.</param>
		/// <param name="consumerSecret">The consumer seceret.</param>
		/// <param name="token">The token, if available. If not available pass null or an empty string.</param>
		/// <param name="tokenSecret">The token secret, if available. If not, pass null or an empty string.</param>
		/// <param name="verifier">The callback verifier, if available. If not, pass null or an empty string.</param>
		/// <param name="httpMethod">The HTTP method used. Must be a valid HTTP method verb (POST,GET,PUT, etc).</param>
		/// <param name="signatureType">The type of signature to use.</param>
		/// <returns>A Base64 string of the hash value.</returns>
		private string GenerateSignature (Uri url, string consumerKey, string consumerSecret, IOAuthToken token,
		                                  string verifier, RequestMethod method, TimeSpan timeStamp, string nonce, SignatureType signatureType,
		                                  string callbackUrl, out string normalizedUrl, out List<IQueryParameter<string>> parameters)
		{
			normalizedUrl = null;
			parameters = null;

			switch (signatureType)
			{
				case SignatureType.PLAINTEXT:
					var signature = UrlEncode (string.Format ("{0}&{1}", consumerSecret, token.Secret));
					GenerateSignatureBase (url, consumerKey, token, verifier, method, timeStamp, nonce, SignatureType.PLAINTEXT,
						callbackUrl, out normalizedUrl, out parameters);
					return signature;
				case SignatureType.HMACSHA1:
//					string signatureBase = GenerateSignatureBase (url, consumerKey, token, verifier, method,
//					                                              timeStamp, nonce, SignatureType.HMACSHA1, callbackUrl,
//					                                              out normalizedUrl, out parameters);
//
//					var hmacsha1 = new HMACSHA1 ();
//					hmacsha1.Key = Encoding.ASCII.GetBytes (string.Format ("{0}&{1}",
//						UrlEncode (consumerSecret),
//						string.IsNullOrEmpty (token.Secret) ? "" : UrlEncode(token.Secret)));
//
//					var hashedSignature = GenerateSignatureUsingHash (signatureBase, hmacsha1);
//
////					log.LogDebug ("HMAC-SHA1 encoded signature {0} of consumer secret and token secret.", hashedSignature);
//					return hashedSignature;
					throw new NotImplementedException ();
			case SignatureType.RSASHA1:
					throw new NotImplementedException ();
				default:
					throw new ArgumentException ("Unknown signature type", "signatureType");
			}
		}

		/// <summary>
		/// Generate the timestamp for the signature.
		/// </summary>
		/// <returns>A string timestamp.</returns>
		protected TimeSpan GenerateTimeStamp ()
		{
//			log.LogDebug ("Generating time stamp.");
			// Default implementation of UNIX time of the current UTC time
			return DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
		}

		/// <summary>
		/// Generate a nonce.
		/// </summary>
		/// <returns>A random nonce string.</returns>
		protected virtual string GenerateNonce()
		{
//			log.LogDebug ("Generating nonce.");
			// Just a simple implementation of a random number between 123400 and 9999999
			return random.Next (123400, 9999999).ToString ();
		}
	}
}