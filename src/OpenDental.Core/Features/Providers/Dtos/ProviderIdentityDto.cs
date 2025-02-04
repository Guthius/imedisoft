namespace Imedisoft.Features.Providers.Dtos;

public sealed record ProviderIdentityDto
{
    public string PayorId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}