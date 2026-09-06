using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Cart.Application.Carts.GetCart;
using MediatR;

namespace ECommerce.Modules.Cart.Application.Carts.AddItem;

public sealed record AddCartItemCommand(
    Guid ProductId,
    int Quantity) : IRequest<Result<CartResponse>>;
