namespace ECommerce.Modules.Catalog.Domain.Products;

public sealed record ProductSearchResult(
    IReadOnlyCollection<Product> Items,
    long TotalCount);

public sealed record ProductSearchCriteria(
    string? Query,
    string? Sku,
    bool? IsActive,
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    bool? InStock,
    double? MinimumRating,
    string Sort,
    int Page,
    int PageSize,
    bool IncludeSkuAndCategoryInTextSearch = true);

public sealed record ProductSearchItem(
    Guid Id,
    string Name,
    string? Description,
    string Sku,
    decimal Price,
    string Currency,
    string? CategoryName,
    double AverageRating,
    int ReviewCount,
    bool InStock,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);

public sealed record AdvancedProductSearchResult(
    IReadOnlyCollection<ProductSearchItem> Items,
    long TotalCount);
