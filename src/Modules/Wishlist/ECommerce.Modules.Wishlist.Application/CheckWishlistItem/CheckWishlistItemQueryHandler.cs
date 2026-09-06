using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Wishlist.Application.Abstractions;
using ECommerce.Modules.Wishlist.Domain.WishlistItems;
using MediatR;

namespace ECommerce.Modules.Wishlist.Application.CheckWishlistItem;

public sealed class CheckWishlistItemQueryHandler(IWishlistRepository repository, ICurrentUser currentUser)
    : IRequestHandler<CheckWishlistItemQuery, Result<CheckWishlistItemResponse>>
{
    public async Task<Result<CheckWishlistItemResponse>> Handle(CheckWishlistItemQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure<CheckWishlistItemResponse>(new Error(WishlistErrors.UnauthorizedAccessCode, WishlistErrors.UnauthorizedAccessDescription, ErrorType.Unauthorized));
        bool exists = await repository.ExistsAsync(currentUser.UserId.Value, request.ProductId, cancellationToken);
        return Result.Success(new CheckWishlistItemResponse(exists));
    }
}
