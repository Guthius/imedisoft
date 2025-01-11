using System.Collections.Generic;

namespace Imedisoft.Core.Features.Clinics.Dtos;

public sealed record GetAllClinicsResponse
{
    public List<ClinicDto> Clinics { get; set; }
}