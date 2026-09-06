using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Catalog.Contracts.Products;
using ECommerce.Modules.Wishlist.Application.Abstractions;
using ECommerce.Modules.Wishlist.Application.Caching;
using ECommerce.Modules.Wishlist.Domain.WishlistItems;
using MediatR;

namespace ECommerce.Modules.Wishlist.Application.GetWishlist;

public sealed class GetWishlistQueryHandler(
    IWishlistRepository repository,
    IProductCatalogReader catalogReader,
    ICurrentUser currentUser,
    ICacheService cacheService,
    WishlistCacheOptions cacheOptions) : IRequestHandler<GetWishlistQuery, Result<WishlistResponse>>
{
    public async Task<Result<WishlistResponse>> Handle(GetWishlistQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure<WishlistResponse>(new Error(WishlistErrors.UnauthorizedAccessCode, WishlistErrors.UnauthorizedAccessDescription, ErrorType.Unauthorized));
        Guid userId = currentUser.UserId.Value;
        string key = WishlistCache.ForUser(userId);
        WishlistResponse? cached = await cacheService.GetAsync<WishlistResponse>(key, cancellationToken);
        if (cached is not null) return Result.Success(cached);
        List<WishlistItemResponse> items = [];
        foreach (var item in await repository.GetUserWishlistAsync(userId, cancellationToken))
        {
            ProductCatalogResponse? product = await catalogReader.GetByIdAsync(item.ProductId, cancellationToken);
            if (product is not null)
                items.Add(new(product.Id, product.Name, product.Price, product.Currency, item.CreatedAtUtc));
        }
        WishlistResponse response = new(items);
        await cacheService.SetAsync(key, response, new CacheEntryOptions(cacheOptions.Expiration), cancellationToken);
        return Result.Success(response);
    }
}

public sealed class WishlistCacheOptions
{
    public const string SectionName = "WishlistCache";
    public int ExpirationMinutes { get; init; } = 10;
    public TimeSpan Expiration => TimeSpan.FromMinutes(ExpirationMinutes);
}
