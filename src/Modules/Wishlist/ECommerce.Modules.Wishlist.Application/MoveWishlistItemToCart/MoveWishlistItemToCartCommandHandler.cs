using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Cart.Contracts;
using ECommerce.Modules.Wishlist.Application.Abstractions;
using ECommerce.Modules.Wishlist.Application.Caching;
using ECommerce.Modules.Wishlist.Domain.WishlistItems;
using MediatR;

namespace ECommerce.Modules.Wishlist.Application.MoveWishlistItemToCart;

public sealed class MoveWishlistItemToCartCommandHandler(
    IWishlistRepository repository,
    ICartItemAdder cartItemAdder,
    ICurrentUser currentUser,
    ICacheService cacheService,
    TimeProvider timeProvider) : IRequestHandler<MoveWishlistItemToCartCommand, Result>
{
    public async Task<Result> Handle(MoveWishlistItemToCartCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure(new Error(WishlistErrors.UnauthorizedAccessCode, WishlistErrors.UnauthorizedAccessDescription, ErrorType.Unauthorized));
        Guid userId = currentUser.UserId.Value;
        WishlistItem? item = await repository.GetByUserAndProductAsync(userId, request.ProductId, cancellationToken);
        if (item is null)
            return Result.Failure(new Error(WishlistErrors.ItemNotFoundCode, WishlistErrors.ItemNotFoundDescription, ErrorType.NotFound));
        Result cartResult = await cartItemAdder.AddItemAsync(item.ProductId, 1, cancellationToken);
        if (cartResult.IsFailure)
            return Result.Failure(new Error(WishlistErrors.CartOperationFailedCode, WishlistErrors.CartOperationFailedDescription, cartResult.Error!.Type));
        item.Remove(timeProvider.GetUtcNow());
        await repository.RemoveAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await WishlistCache.InvalidateAsync(cacheService, userId, cancellationToken);
        return Result.Success();
    }
}
