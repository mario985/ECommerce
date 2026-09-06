using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Cart.Application.Carts.Checkout;

public sealed record CheckoutCartCommand : IRequest<Result<CheckoutResponse>>;
