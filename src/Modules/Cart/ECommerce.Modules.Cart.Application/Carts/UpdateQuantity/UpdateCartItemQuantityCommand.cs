using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Cart.Application.Carts.GetCart;
using MediatR;

namespace ECommerce.Modules.Cart.Application.Carts.UpdateQuantity;

public sealed record UpdateCartItemQuantityCommand(
    Guid ProductId,
    int Quantity) : IRequest<Result<CartResponse>>;
