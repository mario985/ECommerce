using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.SearchProducts;

public sealed record SearchProductsQuery(
    string? Query = null,
    Guid? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? InStock = null,
    double? MinimumRating = null,
    string? Sort = null,
    int Page = 1,
    int PageSize = 20,
    string? Sku = null,
    bool? IsActive = true,
    bool IncludeSkuAndCategoryInTextSearch = true) : IRequest<Result<SearchProductsResponse>>
{
    public SearchProductsQuery(
        string? search,
        string? sku,
        bool? isActive,
        int page = 1,
        int pageSize = 20)
        : this(
            search,
            Sort: "name",
            Page: page,
            PageSize: pageSize,
            Sku: sku,
            IsActive: isActive,
            IncludeSkuAndCategoryInTextSearch: false)
    {
    }
}

public sealed record SearchProductsResponse(
    IReadOnlyCollection<ProductSearchResponse> Items,
    int Page,
    int PageSize,
    long TotalCount,
    int TotalPages);

public sealed record ProductSearchResponse(
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
