using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Windows.Storage.Streams;
using Imedisoft.Core.Features.Providers.Dtos;
using Imedisoft.Features.Providers.Dtos;
using Newtonsoft.Json;
using HttpClient = System.Net.Http.HttpClient;

namespace Imedisoft.Features.Providers;

public static class ProviderService
{
    private const string ServerUri = "https://localhost:7002";

    public static List<ProviderDto> GetAll()
    {
        using var httpClient = new HttpClient();

        httpClient.BaseAddress = new Uri(ServerUri);

        var response = httpClient.GetAsync("/providers").Result;

        response.EnsureSuccessStatusCode();

        var providerDtos = JsonConvert.DeserializeObject<List<ProviderDto>>(response.Content.ReadAsStringAsync().Result);

        return providerDtos;
    }

    public static ProviderDto Update(long providerId, UpdateProviderRequest request)
    {
        var requestJson = JsonConvert.SerializeObject(request);
        var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        using var httpClient = new HttpClient();

        httpClient.BaseAddress = new Uri(ServerUri);

        var response = httpClient.PostAsync("/providers/" + providerId, requestContent).Result;

        response.EnsureSuccessStatusCode();
        
        var providerDto = JsonConvert.DeserializeObject<ProviderDto>(response.Content.ReadAsStringAsync().Result);

        return providerDto;
    }
}