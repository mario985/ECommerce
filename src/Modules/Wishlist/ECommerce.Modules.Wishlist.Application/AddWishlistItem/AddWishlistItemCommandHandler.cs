using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Catalog.Contracts.Products;
using ECommerce.Modules.Wishlist.Application.Abstractions;
using ECommerce.Modules.Wishlist.Application.Caching;
using ECommerce.Modules.Wishlist.Domain.WishlistItems;
using MediatR;

namespace ECommerce.Modules.Wishlist.Application.AddWishlistItem;

public sealed class AddWishlistItemCommandHandler(
    IWishlistRepository repository,
    IProductCatalogReader catalogReader,
    ICurrentUser currentUser,
    ICacheService cacheService,
    TimeProvider timeProvider) : IRequestHandler<AddWishlistItemCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(AddWishlistItemCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Failure(WishlistErrors.UnauthorizedAccessCode, WishlistErrors.UnauthorizedAccessDescription, ErrorType.Unauthorized);
        ProductCatalogResponse? product = await catalogReader.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
            return Failure(WishlistErrors.ProductNotFoundCode, WishlistErrors.ProductNotFoundDescription, ErrorType.NotFound);
        Guid userId = currentUser.UserId.Value;
        if (await repository.ExistsAsync(userId, request.ProductId, cancellationToken))
            return Failure(WishlistErrors.DuplicateItemCode, WishlistErrors.DuplicateItemDescription, ErrorType.Conflict);
        WishlistItem item = WishlistItem.Create(userId, product.Id, timeProvider.GetUtcNow());
        await repository.AddAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await WishlistCache.InvalidateAsync(cacheService, userId, cancellationToken);
        return Result.Success(item.Id);
    }

    private static Result<Guid> Failure(string code, string description, ErrorType type) =>
        Result.Failure<Guid>(new Error(code, description, type));
}
