namespace OpenDental.Features.Providers.Models;

public sealed record ProviderIdentityModel
{
    public string PayorId { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
}