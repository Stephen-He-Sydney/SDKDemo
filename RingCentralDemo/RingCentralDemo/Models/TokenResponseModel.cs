using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RingCentralDemo.Models
{
    public class TokenResponseModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ExpireIn { get; set; }
        public string RefreshToken_ExpireIn { get; set; }
        public string TokenType { get; set; }
        public string Scope { get; set; }
        public string OwnerId { get; set; }
        public string EndpointId { get; set; }
    }
}