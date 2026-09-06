using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Wishlist.Application.AddWishlistItem;

public sealed record AddWishlistItemCommand(Guid ProductId) : IRequest<Result<Guid>>;
