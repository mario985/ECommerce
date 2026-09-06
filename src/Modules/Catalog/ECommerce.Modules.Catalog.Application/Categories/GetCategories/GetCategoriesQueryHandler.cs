using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Domain.Categories;
using MediatR;
namespace ECommerce.Modules.Catalog.Application.Categories.GetCategories;
public sealed class GetCategoriesQueryHandler(ICategoryRepository repository, ICacheService cache, CatalogCacheOptions options) : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyCollection<CategoryResponse>>>
{
    public async Task<Result<IReadOnlyCollection<CategoryResponse>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        if (request.ActiveOnly)
        {
            IReadOnlyCollection<CategoryResponse>? cached = await cache.GetAsync<IReadOnlyCollection<CategoryResponse>>(CatalogCacheKeys.ActiveCategories, cancellationToken);
            if (cached is not null) return Result.Success(cached);
        }
        IReadOnlyCollection<CategoryResponse> result = (await repository.GetAllAsync(request.ActiveOnly, cancellationToken)).Select(CategoryResponse.From).ToArray();
        if (request.ActiveOnly) await cache.SetAsync(CatalogCacheKeys.ActiveCategories, result, new CacheEntryOptions(options.CategoryExpiration), cancellationToken);
        return Result.Success(result);
    }
}
