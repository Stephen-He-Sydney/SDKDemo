﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using RingCentral.SDK.Http;
using RingCentral.ServiceModel;

namespace RingCentral.SDK
{
    public class Platform
    {
        private const string AccessTokenTtl = "3600"; // 60 minutes
        private const string RefreshTokenTtl = "36000"; // 10 hours
        private const string RefreshTokenTtlRemember = "604800"; // 1 week
        private const string TokenEndpoint = "/restapi/oauth/token";
        private const string RevokeEndpoint = "/restapi/oauth/revoke";
        private const string AuthEndpoint = "/restapi/oauth/authorize";
        private const string UrlPrefix = "restapi";

        public HttpClient _client { private get; set; }
        protected Auth Auth;

        private Object thisLock = new Object();

        //public Platform(string appKey, string appSecret, string apiEndPoint)
        //{
        //    AppKey = appKey;
        //    AppSecret = appSecret;
        //    ApiEndpoint = apiEndPoint;
        //    Auth = new Auth();
        //    _client = new HttpClient { BaseAddress = new Uri(ApiEndpoint) };
        //}

        public Platform(string appKey, string appSecret, string apiEndPoint, string appName, string appVersion)
        {
            AppKey = appKey;
            AppSecret = appSecret;
            ApiEndpoint = apiEndPoint;
            ApiVersion = appVersion;
            Auth = new Auth();
            _client = new HttpClient { BaseAddress = new Uri(ApiEndpoint) };
            SetUserAgentHeader(appName, appVersion);
        }

        private string AppKey { get; set; }
        private string AppSecret { get; set; }
        private string ApiEndpoint { get; set; }
        private string ApiVersion { get; set; }

        /// <summary>
        ///  Generates a OAuth 2.0 request for the authorization code grant type 
        /// </summary>
        /// <param name="authCodeParams">Parameters of requesting authCode</param>
        public string AuthUrl(Dictionary<string, string> authCodeParams)
        {
            var authRequest = new Request(AuthEndpoint, authCodeParams.ToList()).GetUrl();

            return CreateUrl(authRequest, new Options());
        }

        /// <summary>
        ///  Generates a OAuth 2.0 URL for the authorization code grant type 
        /// </summary>
        /// <param name="authCodeParams">Parameters of requesting authCode</param>
        public string CreateUrl(string path, Options options)
        {
            path = path ?? string.Empty;
            options = options ?? new Options();

            StringBuilder builtUrl = new StringBuilder();
            bool hasHttp = path.StartsWith("http://") || path.Contains("https://");

            if (options.addServer && !hasHttp) builtUrl.Append(ApiEndpoint);

            if (!path.Contains(UrlPrefix) && !hasHttp) builtUrl.Append(UrlPrefix).Append("/").Append(ApiVersion);

            builtUrl.Append(path);

            if (!string.IsNullOrEmpty(options.addMethod) || options.addToken) builtUrl.Append(UrlPrefix.Contains("?")?"&":"?");

            if (!string.IsNullOrEmpty(options.addMethod)) builtUrl.Append("_method=").Append(options.addMethod);

            if (options.addToken) builtUrl.Append(!string.IsNullOrEmpty(options.addMethod) ? "&" : "").Append("access_token=").Append(Auth.GetAccessToken());

            return builtUrl.ToString();
        }

        /// <summary>
        ///     Method to generate Access Token and Refresh Token by authorization code grant type
        /// </summary>
        /// <param name="authCodeParams">Parameters of requesting authorization code</param>
        /// <returns>string response of Authenticate result.</returns>
        public Response AuthorizeByAuthCode(string grant_type, string code, string redirect_uri)
        {
            var body = new Dictionary<string, string>()
            {
                {"grant_type", grant_type},
                {"code", code},
                {"redirect_uri", redirect_uri},
                {"access_token_ttl", AccessTokenTtl},
                {"refresh_token_ttl", RefreshTokenTtl}
            };

            var request = new Request(TokenEndpoint, body);
            var result = AuthCall(request);

            Auth.SetData(result.GetJson());

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Auth.GetAccessToken());

            return result;
        }

        /// <summary>
        ///     Method to generate Access Token and Refresh Token to establish an authenticated session
        /// </summary>
        /// <param name="userName">Login of RingCentral user</param>
        /// <param name="password">Password of the RingCentral User</param>
        /// <param name="extension">Optional: Extension number to login</param>
        /// <param name="isRemember">If set to true, refresh token TTL will be one week, otherwise it's 10 hours</param>
        /// <returns>string response of Authenticate result.</returns>
        public Response Authorize(string userName, string extension, string password, bool isRemember)
        {
            var body = new Dictionary<string, string>
                       {
                           {"username", userName},
                           {"password", Uri.EscapeUriString(password)},
                           {"extension", extension},
                           {"grant_type", "password"},
                           {"access_token_ttl", AccessTokenTtl},
                           {"refresh_token_ttl", isRemember ? RefreshTokenTtlRemember : RefreshTokenTtl}
                       };

            var request = new Request(TokenEndpoint, body);
            var result = AuthCall(request);

            Auth.SetRemember(isRemember);
            Auth.SetData(result.GetJson());

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Auth.GetAccessToken());

            return result;
        }

        /// <summary>
        ///     Refreshes expired Access token during valid lifetime of Refresh Token
        /// </summary>
        /// <returns>string response of Refresh result</returns>
        public Response Refresh()
        {
            if (!Auth.IsRefreshTokenValid()) throw new Exception("Refresh Token has Expired");

            var body = new Dictionary<string, string>
                       {
                           {"grant_type", "refresh_token"},
                           {"refresh_token", Auth.GetRefreshToken()},
                           {"access_token_ttl", AccessTokenTtl},
                           {"refresh_token_ttl", Auth.IsRemember() ? RefreshTokenTtlRemember : RefreshTokenTtl}
                       };

            var request = new Request(TokenEndpoint, body);
            var result = AuthCall(request);

            Auth.SetData(result.GetJson());

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Auth.GetAccessToken());

            return result;
        }

        /// <summary>
        ///     Revokes the already granted access to stop application activity
        /// </summary>
        /// <returns>string response of Revoke result</returns>
        public Response Logout()
        {
            var body = new Dictionary<string, string>
                       {
                           {"token", Auth.GetAccessToken()}
                       };

            Auth.Reset();

            var request = new Request(RevokeEndpoint, body);

            return AuthCall(request);
        }

        /// <summary>
        ///     Authentication, Refresh and Revoke requests all require an Authentication Header Value of "Basic".  This is a
        ///     special method to handle those requests.
        /// </summary>
        /// <param name="request">
        ///     A Request object with a url and a dictionary of key value pairs (<c>Authenticate</c>,
        ///     <c>Refresh</c>, <c>Revoke</c>)
        /// </param>
        /// <returns>Response object</returns>
        private Response AuthCall(Request request)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetApiKey());

            var result = _client.PostAsync(request.GetUrl(), request.GetHttpContent()).Result;

            return new Response(result);
        }

        /// <summary>
        ///     Gets the auth data set on authorization
        /// </summary>
        /// <returns>Dictionary of auth data</returns>
        public Dictionary<string, string> GetAuthData()
        {
            return Auth.GetData();
        }

        /// <summary>
        ///     Gets the auth data set on authorization
        /// </summary>
        /// <returns>Dictionary of auth data</returns>
        public void SetAuthData(Dictionary<string, string> authData)
        {
            Auth.SetData(authData);
        }

        public Response Get(Request request)
        {
            return ApiCall("GET", request);
        }

        public Response Post(Request request)
        {
            return ApiCall("POST", request);
        }

        public Response Delete(Request request)
        {
            return ApiCall("DELETE", request);
        }

        public Response Put(Request request)
        {
            return ApiCall("PUT", request);
        }

        private Response ApiCall(string method, Request request)
        {
            if (!IsAuthorized()) throw new Exception("Access has Expired");

            HttpRequestMessage requestMessage = new HttpRequestMessage();

            requestMessage.Content = request.GetHttpContent();
            requestMessage.Method = request.GetHttpMethod(method);
            requestMessage.RequestUri = request.GetUri();

            request.GetXhttpOverRideHeader(requestMessage);

            return new Response(_client.SendAsync(requestMessage).Result);
        }


        /// <summary>
        ///     Gets the API key by encoding the AppKey and AppSecret with Encoding.UTF8.GetBytes
        /// </summary>
        /// <returns>The Api Key</returns>
        private string GetApiKey()
        {
            var byteArray = Encoding.UTF8.GetBytes(AppKey + ":" + AppSecret);
            return Convert.ToBase64String(byteArray);
        }

        /// <summary>
        ///     You also may supply custom AppName:AppVersion in the form of a header with your application codename and version. These parameters
        ///     are optional but they will help a lot to identify your application in API logs and speed up any potential troubleshooting.
        ///     Allowed characters for AppName:AppVersion are- letters, digits, hyphen, dot and underscore.
        /// </summary>
        /// <param name="header">The value of the User-Agent header</param>
        private void SetUserAgentHeader(string appName, string appVersion)
        {
            var agentString = String.Empty;

            #region Set UA String
            if (!string.IsNullOrEmpty(appName))
            {
                agentString += appName;
                if (!string.IsNullOrEmpty(appVersion))
                {
                    agentString += "_" + appVersion;
                }
            }

            //if (!string.IsNullOrEmpty(osName) && !string.IsNullOrEmpty(osVersion))
            //{
            //    agentString += "." + osName + "/" + osVersion;
            //}

            //if (!string.IsNullOrEmpty(clrVersion))
            //{
            //    agentString += ".CLR/" + clrVersion;
            //}

            if (string.IsNullOrEmpty(agentString))
            {
                agentString += "RCCSSDK_" + SDK.VERSION;
            }
            else
            {
                agentString += ".RCCSSDK_" + SDK.VERSION;
            }
            #endregion

            Regex r = new Regex("(?:[^a-z0-9-_. ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            var ua = r.Replace(agentString, String.Empty);

            _client.DefaultRequestHeaders.Add("User-Agent", ua);
            _client.DefaultRequestHeaders.Add("RC-User-Agent", ua);
        }

        /// <summary>
        ///     Determines if Access is valid and returns the boolean result.  If access is not valid but refresh token is valid
        ///     then a refresh is issued.
        /// </summary>
        /// <returns>boolean value of access authorization</returns>
        public bool IsAuthorized()
        {
            if (Auth.IsAccessTokenValid())
            {
                return true;
            }

            if (Auth.IsRefreshTokenValid())
            {
                //obtain a mutual-exclusion lock for the thisLock object, execute statement and then release the lock.
                lock (thisLock)
                {
                    Refresh();
                    return true;
                }
            }

            return false;
        }
    }
}