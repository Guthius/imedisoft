using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenDentBusiness.WebBridges;

namespace OpenDentBusiness;

public class APIRequest : IApiRequest
{
    public static IApiRequest Inst { get; } = new APIRequest();

    public T SendRequest<T>(string urlEndpoint, HttpMethod method, AuthenticationHeaderValue authHeaderVal, string body, HttpContentType contentType, HttpClient clientOverride = null, List<string> queryParameters = null, JsonSerializerSettings deserializeSettings = null)
    {
        return Task.Run(async () => await SendRequestAsync<T, string>(urlEndpoint, method, authHeaderVal, body, contentType, clientOverride, queryParameters, deserializeSettings))
            .GetAwaiter()
            .GetResult();
    }
        
    public TResult SendRequest<TResult, TBody>(string urlEndpoint, HttpMethod method, AuthenticationHeaderValue authHeaderVal, TBody body, HttpContentType contentType, HttpClient clientOverride = null, List<string> queryParameters = null, JsonSerializerSettings deserializeSettings = null)
    {
        return Task.Run(async () => await SendRequestAsync<TResult, TBody>(urlEndpoint, method, authHeaderVal, body, contentType, clientOverride, queryParameters, deserializeSettings))
            .GetAwaiter()
            .GetResult();
    }

    public async Task<TResult> SendRequestAsync<TResult, TBody>(string urlEndpoint, HttpMethod method, AuthenticationHeaderValue authHeaderVal, TBody body, HttpContentType contentType = HttpContentType.Json, HttpClient clientOverride = null, List<string> queryParameters = null, JsonSerializerSettings deserializeSettings = null)
    {
        var res = "";
        var response = new HttpResponseMessage();
        var client = clientOverride ?? new HttpClient();
        var disposeClient = clientOverride == null; // Dispose client only if it was created here
        if (queryParameters != null)
        {
            urlEndpoint += "?" + string.Join("&", queryParameters);
        }

        try
        {
            using (var request = new HttpRequestMessage(method, urlEndpoint))
            {
                if (authHeaderVal != null)
                {
                    request.Headers.Authorization = authHeaderVal;
                }

                if (method != HttpMethod.Get)
                {
                    switch (body)
                    {
                        case string bodyStr:
                            switch (contentType)
                            {
                                case HttpContentType.Json:
                                    request.Content = new StringContent(bodyStr, Encoding.UTF8, "application/json");
                                    break;
                                
                                case HttpContentType.UrlEncoded:
                                {
                                    var dictKeyValuePairs = JsonConvert
                                        .DeserializeObject<Dictionary<string, object>>(bodyStr)
                                        .ToDictionary(
                                            x => x.Key, 
                                            x => x.Value == null ? "" : x.Value.ToString());
                                    
                                    request.Content = new FormUrlEncodedContent(dictKeyValuePairs.Select(x => x));
                                    break;
                                }
                            }

                            break;
                        
                        case MultipartFormDataContent bodyMultipart:
                            request.Content = bodyMultipart;
                            break;
                        
                        default:
                            throw new Exception("APIRequest content type invalid.");
                    }
                }

                response = await client.SendAsync(request);
            }

            using (var streamReader = new StreamReader(await response.Content.ReadAsStreamAsync()))
            {
                res = await streamReader.ReadToEndAsync();
            }

            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<TResult>(res, deserializeSettings);
        }
        catch (HttpRequestException hre)
        {
            var errorMsg = hre.Message + (string.IsNullOrWhiteSpace(res) ? "" : "\r\nRaw response:\r\n" + res);
            var errorJson = new ApiResponseError {Message = errorMsg, Response = res, ResponseStatus = (int) response.StatusCode};
            
            throw new HttpRequestException(JsonConvert.SerializeObject(errorJson), hre);
        }
        finally
        {
            if (disposeClient)
            {
                client.Dispose();
            }
        }
    }
}

public class ApiResponseError
{
    public string Message;
    public string Response;
    public int ResponseStatus;
}

public enum HttpContentType
{
    Json,
    UrlEncoded,
    Multipart
}