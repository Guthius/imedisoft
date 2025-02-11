namespace Imedisoft.Core.Features.Providers.Dtos;

public sealed record ProviderClinicDto
{
    public long? ClinicId { get; set; }
    public string DeaNumber { get; set; } = string.Empty;
    public string StateLicense { get; set; } = string.Empty;
    public string StateRxId { get; set; } = string.Empty;
    public string StateWhereLicensed { get; set; } = string.Empty;
}