namespace Imedisoft.Features.Providers.Dtos;

public sealed record ProviderSpecialtyDto
{
    public long Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string TaxonomyCode { get; set; } = string.Empty;
}