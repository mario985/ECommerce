using ECommerce.Common.Application.Caching;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Domain.Products;
using MediatR;

namespace ECommerce.Modules.Catalog.Application.Products.GetProductById;

public sealed class GetProductByIdQueryHandler(
    IProductRepository productRepository,
    ICacheService cacheService,
    CatalogCacheOptions cacheOptions)
    : IRequestHandler<GetProductByIdQuery, ProductResponse?>
{
    public async Task<ProductResponse?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        string cacheKey = CatalogCacheKeys.Product(request.ProductId);
        ProductResponse? cached = await cacheService.GetAsync<ProductResponse>(
            cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        Product? product = await productRepository.GetByIdAsync(
            request.ProductId,
            cancellationToken);

        if (product is null)
        {
            return null;
        }

        ProductResponse response = ProductResponse.FromProduct(product);
        await cacheService.SetAsync(
            cacheKey,
            response,
            new CacheEntryOptions(cacheOptions.ProductExpiration),
            cancellationToken);
        return response;
    }
}
