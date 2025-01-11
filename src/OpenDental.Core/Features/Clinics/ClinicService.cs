using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Imedisoft.Core.Features.Clinics.Dtos;
using Newtonsoft.Json;

namespace Imedisoft.Core.Features.Clinics;

public static class ClinicService
{
    private const string ServerUri = "https://localhost:7002";

    public static async Task<ClinicDto> GetById(long id)
    {
        using var httpClient = new HttpClient();

        httpClient.BaseAddress = new Uri(ServerUri);

        var response = await httpClient.GetAsync("/clinics/" + id);

        response.EnsureSuccessStatusCode();

        return JsonConvert.DeserializeObject<ClinicDto>(await response.Content.ReadAsStringAsync());
    }

    public static List<ClinicDto> GetAll()
    {
        using var httpClient = new HttpClient();

        httpClient.BaseAddress = new Uri(ServerUri);

        var response = httpClient.GetAsync("/clinics").Result;

        response.EnsureSuccessStatusCode();

        var data = JsonConvert.DeserializeObject<GetAllClinicsResponse>(response.Content.ReadAsStringAsync().Result);

        return data.Clinics;
    }
}