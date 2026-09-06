using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Wishlist.Application.Abstractions;
using ECommerce.Modules.Wishlist.Application.Caching;
using ECommerce.Modules.Wishlist.Domain.WishlistItems;
using MediatR;

namespace ECommerce.Modules.Wishlist.Application.RemoveWishlistItem;

public sealed class RemoveWishlistItemCommandHandler(
    IWishlistRepository repository,
    ICurrentUser currentUser,
    ICacheService cacheService,
    TimeProvider timeProvider) : IRequestHandler<RemoveWishlistItemCommand, Result>
{
    public async Task<Result> Handle(RemoveWishlistItemCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure(new Error(WishlistErrors.UnauthorizedAccessCode, WishlistErrors.UnauthorizedAccessDescription, ErrorType.Unauthorized));
        Guid userId = currentUser.UserId.Value;
        WishlistItem? item = await repository.GetByUserAndProductAsync(userId, request.ProductId, cancellationToken);
        if (item is null)
            return Result.Failure(new Error(WishlistErrors.ItemNotFoundCode, WishlistErrors.ItemNotFoundDescription, ErrorType.NotFound));
        item.Remove(timeProvider.GetUtcNow());
        await repository.RemoveAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        await WishlistCache.InvalidateAsync(cacheService, userId, cancellationToken);
        return Result.Success();
    }
}
