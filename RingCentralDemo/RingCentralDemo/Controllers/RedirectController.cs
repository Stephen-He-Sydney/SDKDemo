using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RingCentral.SDK;
using RingCentralDemo.Helpers;
using RingCentralDemo.Models;
using Newtonsoft.Json.Linq;

namespace RingCentralDemo.Controllers
{
    public class RedirectController : Controller
    {
        private Platform Platform;

        public ActionResult Index()
        {
            string initialState = SessionHelper.Get("state").ToString();
            string initialRedirectUri = SessionHelper.Get("redirect_uri").ToString();

            string code = Request.QueryString["code"];
            string state = Request.QueryString["state"];
            TokenResponseModel tokenResponseModel = null;

            if (state.Trim() == initialState.Trim())
            {
                Platform = new SDKModel().GenerateSDK().GetPlatform();
                var response = Platform.AuthorizeByAuthCode("authorization_code", code, initialRedirectUri);

                JToken token = JObject.Parse(response.GetBody());
                tokenResponseModel = new TokenResponseModel()
                {
                    AccessToken = (string)token.SelectToken("access_token"),
                    ExpireIn = (string)token.SelectToken("expires_in"),
                    RefreshToken = (string)token.SelectToken("refresh_token"),
                    RefreshToken_ExpireIn = (string)token.SelectToken("refresh_token_expires_in"),
                    Scope = (string)token.SelectToken("scope"),
                    TokenType = (string)token.SelectToken("token_type"),
                    OwnerId = (string)token.SelectToken("owner_id"),
                    EndpointId = (string)token.SelectToken("endpoint_id")
                }; 
            }

            return View(tokenResponseModel != null ? tokenResponseModel : null);
        }
    }
}