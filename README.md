#  SDK Demo

## OAuth 2.0 Authorization Code Grant Type For RingCentral C# SDK 

### Table of contents

1. [Overview](#overview)

2. [Client application integration](#client-application-integration)
  1. [Request authorization code](#request-authorization-code)
  2. [User login and consent](#user-login-and-consent)
  3. [Retrieve authorization code](#retrieve-authorization-code)
  4. [Exchange code for token](#exchange-code-for-token)
  5. [Get access token](#get-access-token)


### Overview
This document is used to assist developers with the way how to integrate authorization code grant type of C# SDK with your client applications.

### Client Application Integration


###### Step One
###### Request Authorization Code
###### a)	Create request URL 
```cs
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

```


###### b)	Redirect Url calling authUrl method
```cs
 public ActionResult Index()
 {
        return Redirect(Platform.AuthUrl(AuthCodeParams));
 }

```



###### Step Two
###### User Login and Consent
![alt text](http://ringcentral.github.io/images/rng_3leg-oauth_side-by-side_640x551.png "3-legged OAuth")

Enter the provided phone number and password, and click login button
- Username (phone number): 16505496100
- Password: rcEngineering@1!


###### Step Three
###### Retrieve Authorization Code
If the authorize button above is hit, the code and state will be delivered after redirecting page.
```cs
string code = Request.QueryString["code"];
string state = Request.QueryString["state"];

```


###### Step Four
###### Exchange Code for Token
With the 2 parameters and redirectUri provided before, you are able to retrieve access tokens and others by using following code.
```cs
var response = Platform.AuthorizeByAuthCode("authorization_code", code, initialRedirectUri);

```

###### Step Five
###### Get Access Token
The response contains following parameters that is required to map with corresponding dictionary keys.
```cs
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


```
