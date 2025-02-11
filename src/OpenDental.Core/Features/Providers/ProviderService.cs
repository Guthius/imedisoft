using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Imedisoft.Core.Features.Providers.Dtos;
using LanguageExt;
using LanguageExt.Common;
using Newtonsoft.Json;
using HttpClient = System.Net.Http.HttpClient;

namespace Imedisoft.Core.Features.Providers;

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

    public static List<ProviderSpecialtyDto> GetSpecialties()
    {
        using var httpClient = new HttpClient();

        httpClient.BaseAddress = new Uri(ServerUri);

        var response = httpClient.GetAsync("/providers/specialties").Result;

        response.EnsureSuccessStatusCode();

        var providerSpecialtyDtos = JsonConvert.DeserializeObject<List<ProviderSpecialtyDto>>(response.Content.ReadAsStringAsync().Result);

        return providerSpecialtyDtos;
    }

    public static Either<Error, ProviderDto> Create(CreateProviderRequest request)
    {
        var requestJson = JsonConvert.SerializeObject(request);
        var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        using var httpClient = new HttpClient();

        httpClient.BaseAddress = new Uri(ServerUri);

        var response = httpClient.PostAsync("/providers", requestContent).Result;
        if (!response.IsSuccessStatusCode)
        {
            return GetError(response);
        }

        var providerDto = JsonConvert.DeserializeObject<ProviderDto>(response.Content.ReadAsStringAsync().Result);

        return providerDto;
    }

    public static Either<Error, ProviderDto> Update(long providerId, UpdateProviderRequest request)
    {
        var requestJson = JsonConvert.SerializeObject(request);
        var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        using var httpClient = new HttpClient();

        httpClient.BaseAddress = new Uri(ServerUri);

        var response = httpClient.PutAsync("/providers/" + providerId, requestContent).Result;
        if (!response.IsSuccessStatusCode)
        {
            return GetError(response);
        }

        var providerDto = JsonConvert.DeserializeObject<ProviderDto>(response.Content.ReadAsStringAsync().Result);

        return providerDto;
    }

    private static Error GetError(HttpResponseMessage response)
    {
        var errorJson = response.Content.ReadAsStringAsync().Result;
        var errorDto = JsonConvert.DeserializeObject<ErrorDto>(errorJson);

        return Error.New(errorDto.Message);
    }

    private sealed record ErrorDto
    {
        public string Message { get; set; } = string.Empty;
    }
}