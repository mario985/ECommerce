using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Cart.Application.Carts.GetCart;

public sealed record GetCartQuery : IRequest<Result<CartResponse>>;
