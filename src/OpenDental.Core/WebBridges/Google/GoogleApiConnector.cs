using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using OpenDentBusiness.Email;
using GmailApi = Google.Apis.Gmail.v1;

namespace OpenDentBusiness;

public class GoogleApiConnector
{
    public static GmailApi.GmailService CreateGmailService(BasicEmailAddress emailAddress)
    {
        var credential = new ODGoogleUserCredential
        {
            AccessToken = emailAddress.AccessToken,
        };
        
        var baseService = new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        };
        
        return new GmailApi.GmailService(baseService);
    }

    public class ODGoogleUserCredential : IConfigurableHttpClientInitializer, IHttpExecuteInterceptor
    {
        public string AccessToken { get; set; }

        public void Initialize(ConfigurableHttpClient httpClient)
        {
            httpClient.MessageHandler.Credential = this;
        }

        public Task InterceptAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var bearerTokenInterceptor = new BearerToken.AuthorizationHeaderAccessMethod();

            bearerTokenInterceptor.Intercept(request, AccessToken);
            
            return Task.CompletedTask;
        }
    }
}