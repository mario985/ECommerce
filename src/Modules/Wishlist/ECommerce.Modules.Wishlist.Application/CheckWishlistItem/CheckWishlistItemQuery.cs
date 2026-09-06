using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Wishlist.Application.CheckWishlistItem;

public sealed record CheckWishlistItemQuery(Guid ProductId) : IRequest<Result<CheckWishlistItemResponse>>;
public sealed record CheckWishlistItemResponse(bool IsFavorite);
