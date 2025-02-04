namespace Imedisoft.Features.Providers.Dtos;

public sealed record ProviderNameDto
{
    public long Id { get; set; }
    public string Abbr { get; set; } = string.Empty;
}