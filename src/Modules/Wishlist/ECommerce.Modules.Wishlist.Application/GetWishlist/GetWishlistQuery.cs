using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Wishlist.Application.GetWishlist;

public sealed record GetWishlistQuery : IRequest<Result<WishlistResponse>>;
