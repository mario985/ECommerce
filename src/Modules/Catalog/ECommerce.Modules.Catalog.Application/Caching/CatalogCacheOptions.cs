namespace ECommerce.Modules.Catalog.Application.Caching;

public sealed class CatalogCacheOptions
{
    public const string SectionName = "CatalogCache";

    public int ProductExpirationMinutes { get; init; } = 10;
    public int SearchExpirationMinutes { get; init; } = 2;
    public int CategoryExpirationMinutes { get; init; } = 2;

    public TimeSpan ProductExpiration => TimeSpan.FromMinutes(ProductExpirationMinutes);
    public TimeSpan SearchExpiration => TimeSpan.FromMinutes(SearchExpirationMinutes);
    public TimeSpan CategoryExpiration => TimeSpan.FromMinutes(CategoryExpirationMinutes);
}
