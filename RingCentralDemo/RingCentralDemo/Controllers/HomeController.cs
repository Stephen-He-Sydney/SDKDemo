using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RingCentral.SDK;
using RingCentralDemo.Helpers;
using RingCentralDemo.Models;

namespace RingCentralDemo.Controllers
{
    public class HomeController : Controller
    {
        private Platform Platform;
        private Dictionary<string, string> AuthCodeParams;

        public HomeController()
        {
            Platform = new SDKModel().GenerateSDK().GetPlatform();

            AuthCodeParams = new Dictionary<string, string>
              {
                  {"response_type", "code" },
                  {"client_id", "8GtalMrJRA2JnozdtdWTlg" }, 
                  {"redirect_uri", "http://localhost:3000/redirect" }, 
                  {"prompt", "login consent" }, 
                  {"state", "statebyStephenHe" },
              };
            SessionHelper.Set("state", AuthCodeParams["state"]);
            SessionHelper.Set("redirect_uri", AuthCodeParams["redirect_uri"]);
        }

        public ActionResult Index()
        {
            return Redirect(Platform.AuthUrl(AuthCodeParams));
        }
    }
}