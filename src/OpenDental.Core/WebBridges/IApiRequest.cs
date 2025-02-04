using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenDentBusiness.WebBridges;

public interface IApiRequest {
	T SendRequest<T>(string urlEndpoint,HttpMethod method,AuthenticationHeaderValue authHeaderVal,string body,HttpContentType contentType=HttpContentType.Json,HttpClient clientOverride=null,List<string> queryParameters=null,JsonSerializerSettings deserializeSettings=null);
	T SendRequest<T,U>(string urlEndpoint,HttpMethod method,AuthenticationHeaderValue authHeaderVal,U body,HttpContentType contentType=HttpContentType.Json,HttpClient clientOverride=null,List<string> queryParameters=null,JsonSerializerSettings deserializeSettings=null);
	Task<T> SendRequestAsync<T,U>(string urlEndpoint,HttpMethod method,AuthenticationHeaderValue authHeaderVal,U body,HttpContentType contentType=HttpContentType.Json,HttpClient clientOverride=null,List<string> queryParameters=null,JsonSerializerSettings deserializeSettings=null);
}