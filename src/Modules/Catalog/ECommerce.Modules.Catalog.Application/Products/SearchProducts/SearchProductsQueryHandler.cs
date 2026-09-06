using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Domain.Products;
using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.SearchProducts;

public sealed class SearchProductsQueryHandler(
    IProductRepository productRepository,
    ICacheService cacheService,
    CatalogSearchCacheVersion searchCacheVersion,
    CatalogCacheOptions cacheOptions)
    : IRequestHandler<SearchProductsQuery, Result<SearchProductsResponse>>
{
    public async Task<Result<SearchProductsResponse>> Handle(
        SearchProductsQuery request,
        CancellationToken cancellationToken)
    {
        int page = request.Page;
        int pageSize = request.PageSize;
        string? normalizedSku = string.IsNullOrWhiteSpace(request.Sku)
            ? null
            : Sku.Create(request.Sku).Value;

        string version = await searchCacheVersion.GetAsync(cancellationToken);
        string cacheKey = CatalogCacheKeys.Search(
            version,
            request.Query,
            normalizedSku,
            request.IsActive,
            request.CategoryId,
            request.MinPrice,
            request.MaxPrice,
            request.InStock,
            request.MinimumRating,
            request.Sort,
            request.IncludeSkuAndCategoryInTextSearch,
            page,
            pageSize);
        SearchProductsResponse? cached = await cacheService.GetAsync<SearchProductsResponse>(
            cacheKey, cancellationToken);
        if (cached is not null)
        {
            return Result.Success(cached);
        }

        AdvancedProductSearchResult result = await productRepository.SearchAsync(
            new ProductSearchCriteria(
                request.Query,
                normalizedSku,
                request.IsActive,
                request.CategoryId,
                request.MinPrice,
                request.MaxPrice,
                request.InStock,
                request.MinimumRating,
                request.Sort ?? "newest",
                page,
                pageSize,
                request.IncludeSkuAndCategoryInTextSearch),
            cancellationToken);

        ProductSearchResponse[] items = result.Items
            .Select(item => new ProductSearchResponse(
                item.Id,
                item.Name,
                item.Description,
                item.Sku,
                item.Price,
                item.Currency,
                item.CategoryName,
                item.AverageRating,
                item.ReviewCount,
                item.InStock,
                item.IsActive,
                item.CreatedAtUtc))
            .ToArray();
        int totalPages = result.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(result.TotalCount / (double)pageSize);

        SearchProductsResponse response = new(
            items,
            page,
            pageSize,
            result.TotalCount,
            totalPages);
        await cacheService.SetAsync(
            cacheKey,
            response,
            new CacheEntryOptions(cacheOptions.SearchExpiration),
            cancellationToken);
        return Result.Success(response);
    }
}
