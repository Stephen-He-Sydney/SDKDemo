using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RingCentral.SDK;

namespace RingCentralDemo.Models
{
    public class SDKModel
    {
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public string ApiEndPoint { get; set; }
        public string AppName { get; set; }
        public string AppVersion { get; set; }

        public SDKModel()
        {
            AppKey = "8GtalMrJRA2JnozdtdWTlg";
            AppSecret = "uJlm9m4NRa-EYG00l1YbxAKtyxdR0JSVeVINxNFpb0wg";
            ApiEndPoint = "https://platform.devtest.ringcentral.com";
            AppName = "C Sharp Test Suite";
            AppVersion = "1.0.0";
        }

        public SDK GenerateSDK()
        {
            return new SDK(this.AppKey, this.AppSecret, this.ApiEndPoint, this.AppName, this.AppVersion);
        }
    }
}