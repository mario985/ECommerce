using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Wishlist.Application.RemoveWishlistItem;

public sealed record RemoveWishlistItemCommand(Guid ProductId) : IRequest<Result>;
