using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Wishlist.Application.MoveWishlistItemToCart;

public sealed record MoveWishlistItemToCartCommand(Guid ProductId) : IRequest<Result>;
