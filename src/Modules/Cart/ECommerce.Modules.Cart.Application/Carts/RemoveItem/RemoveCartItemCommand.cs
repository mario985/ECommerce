using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Cart.Application.Carts.RemoveItem;

public sealed record RemoveCartItemCommand(Guid ProductId) : IRequest<Result>;
