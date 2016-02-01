# SDKDemo
This repository is used to provide project solution for OAuth demo of ringCentral.

RingCentral Authorization Code Demo in C# SDK

Overview
This document is used to assist developers with the way how to integrate authorization code grant type of C# SDK with your client applications.

Client application integration
1.	Request authorization code
a)	Create request url
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

b)	Let url redirect page by calling authUrl method of SDK
public ActionResult Index()
     {
            return Redirect(Platform.AuthUrl(AuthCodeParams));
     }

2.	User login and consent

Enter the provided phone number and password, and click login button
- Username (phone number): 16505496100
- Password: rcEngineering@1!

3.	Retrieve authorization code
 
If user hits authorize button, the page will redirect to your previous redirectUrl address, then you will have chance to grab code and state values from browser url.
string code = Request.QueryString["code"];
string state = Request.QueryString["state"];

4.	Exchange code for token
With the 2 parameters and redirectUri, you are able to retrieve access tokens and others by using following code.
var response = Platform.AuthorizeByAuthCode("authorization_code", code, initialRedirectUri);

5.	Get access token
The response contains following parameters that is required to map with conrresponding dictionary keys.

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
 

