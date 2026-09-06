using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Cart.Application.Carts.ClearCart;

public sealed record ClearCartCommand : IRequest<Result>;
