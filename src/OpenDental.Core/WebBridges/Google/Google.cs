using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using CodeBase;
using Imedisoft.Core.Caching;
using Imedisoft.Core.Entities;
using Newtonsoft.Json;
using OpenDentBusiness.Properties;

namespace OpenDentBusiness;

public class Google
{
    private static string GetApiUrl(UrlEndpoint endpoint)
    {
        var url = "https://accounts.google.com/o/oauth2/v2/auth";

        switch (endpoint)
        {
            case UrlEndpoint.Root:
                break;

            case UrlEndpoint.AccessToken:
                url = "https://www.googleapis.com/oauth2/v4/token";
                break;

            case UrlEndpoint.RefreshToken:
                url = "https://oauth2.googleapis.com/token";
                break;
        }

        return url;
    }

    private static GoogleToken GetAccessTokenHqOrThrow(string code, bool isRefresh, string redirectUri = null, string codeVerifier = null)
    {
        var listPayloadItems = new List<PayloadItem>();
        listPayloadItems.Add(new PayloadItem(code, "Code"));
        listPayloadItems.Add(new PayloadItem(isRefresh, "IsRefreshToken"));
        if (redirectUri != null)
        {
            //The redirectUri points to a port on the user's computer. It is fine to send this to WebServiceMainHQ
            //because Google just compares it to the redirectUri that was sent with the previous auth request as an additional security measure.
            listPayloadItems.Add(new PayloadItem(redirectUri, "RedirectUri"));
        }

        if (codeVerifier != null)
        {
            listPayloadItems.Add(new PayloadItem(codeVerifier, "CodeVerifier"));
        }

        var officeData = PayloadHelper.CreatePayload(PayloadHelper.CreatePayloadContent(listPayloadItems), eServiceCode.OAuth);
            
        var result = WebServiceMainHQProxy.GetWebServiceMainHQInstance().GetGoogleAccessToken(officeData);
            
        return JsonConvert.DeserializeObject<GoogleToken>(result);
    }

    public static string MakeRefreshAccessTokenRequest(string refreshToken)
    {
        var response = GetAccessTokenHqOrThrow(refreshToken, true);
                
        return response.AccessToken;
    }

    private enum UrlEndpoint
    {
        Root,
        AccessToken,
        RefreshToken,
    }
        
    public class AuthorizationRequest
    {
        private const string CodeChallengeMethod = "S256";
        private const int MinPort = 49152;
        private const int MaxPort = 65535;

        private HttpListener _listener;
        private string _state;
        private string _codeVerifier;
        private string _codeChallenge;
        private string _url;
        
        public void StartListener()
        {
            if (_listener is {IsListening: true})
            {
                return;
            }

            for (var i = MinPort; i < MaxPort; i++)
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://{IPAddress.Loopback}:{i}/");
                    
                try
                {
                    _listener.Start();
                        
                    return;
                }
                catch
                {
                    // ignored
                }
            }

            throw new ODException($"Could not find an available port for the HttpListener. Ports {MinPort} to {MaxPort} were tried.");
        }
            
        public GoogleToken MakeAccessTokenRequest(string emailAddress)
        {
            if (_listener is not {IsListening: true})
            {
                throw new ODException("An attempt to request tokens was made before starting the HttpListener.");
            }

            _state = RandomDataBase64Url(32);
            _codeVerifier = RandomDataBase64Url(32);
            _codeChallenge = Base64UrlencodeNoPadding(Sha256(_codeVerifier));
            BuildAuthorizationUrl(emailAddress);
            Process.Start(_url);
            var code = "";
            var context = _listener.GetContext();
            SendListenerResponse(context);
            code = GetAuthCodeFromContextOrThrow(context);
            var token = GetAccessTokenHqOrThrow(code, isRefresh: false, GetRedirectUri(), _codeVerifier);
            return token;
        }
            
        public void CloseListener()
        {
            if (_listener == null)
            {
                return;
            }
                
            _listener.Close();
            _listener = null;
        }
            
        private static string RandomDataBase64Url(uint length)
        {
            var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64UrlencodeNoPadding(bytes);
        }
            
        private static string Base64UrlencodeNoPadding(byte[] buffer)
        {
            var base64 = Convert.ToBase64String(buffer);
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            base64 = base64.Replace("=", "");
            return base64;
        }
            
        private byte[] Sha256(string inputStirng)
        {
            var bytes = Encoding.ASCII.GetBytes(inputStirng);
            var sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }

        private void BuildAuthorizationUrl(string emailAddress)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(GetApiUrl(UrlEndpoint.Root));
            stringBuilder.Append("?");
            //This will throw if user doesn't have OD reg key. Includes the OD client ID for google and all other params that never change.
            stringBuilder.Append(GetQueryParamsFromHQ());
            stringBuilder.Append("&login_hint=" + emailAddress);
            stringBuilder.Append("&state=" + _state);
            stringBuilder.Append("&code_challenge=" + _codeChallenge);
            stringBuilder.Append("&code_challenge_method=" + CodeChallengeMethod);
            stringBuilder.Append("&redirect_uri=" + Uri.EscapeDataString(GetRedirectUri()));
            _url = stringBuilder.ToString();
        }

        private string GetQueryParamsFromHQ()
        {
            var response = WebServiceMainHQProxy.GetWebServiceMainHQInstance().BuildOAuthUrl(PrefC.GetString(PrefName.RegistrationKey), OAuthApplicationNames.GoogleLoopbackIpAddressFlow.ToString());
            if (response.Trim().First() != '<')
            {
                return JsonConvert.DeserializeObject(response).ToString();
            }
                
            var xmlDocument = new XmlDocument();
                
            xmlDocument.LoadXml(response);
                
            var errorNode = xmlDocument.SelectSingleNode("//Error");
            if (errorNode == null)
            {
                throw new Exception("No error message returned from server.");
            }

            throw new Exception(errorNode.InnerText);
        }

        private string GetRedirectUri()
        {
            var redirectUri = _listener.Prefixes.AsEnumerable().FirstOrDefault();
            
            return redirectUri ?? "";
        }

        private void SendListenerResponse(HttpListenerContext context)
        {
            var bytes = Encoding.UTF8.GetBytes(Resources.GoogleAuthCodeResponseHtml);
            
            context.Response.ContentLength64 = bytes.Length;
            context.Response.OutputStream.Write(bytes, 0, bytes.Length);
            context.Response.OutputStream.Close();
        }

        private string GetAuthCodeFromContextOrThrow(HttpListenerContext context)
        {
            // Checks for errors.
            var error = context.Request.QueryString.Get("error");
            var authCode = context.Request.QueryString.Get("code");
            var incomingState = context.Request.QueryString.Get("state");
            if (error.IsNullOrEmpty())
            {
                if (authCode.IsNullOrEmpty() || incomingState.IsNullOrEmpty())
                {
                    error = $"Malformed authorization response. {context.Request.QueryString}";
                }
                //Compares the receieved state to the expected value, to ensure that
                //Open Dental made the request which resulted in authorization.
                else if (incomingState != _state)
                {
                    error = $"Received request with invalid state ({incomingState})";
                }
            }

            if (!error.IsNullOrEmpty())
            {
                throw new ODException(error);
            }

            return authCode;
        }
    }
}
    
public class GoogleToken(string access, string refresh, string error = "")
{
    public string AccessToken = access;
    public string RefreshToken = refresh;
    public string ErrorMessage = error;
}