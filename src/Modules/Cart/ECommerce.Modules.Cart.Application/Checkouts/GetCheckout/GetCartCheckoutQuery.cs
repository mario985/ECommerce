using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Cart.Application.Checkouts.GetCheckout;

public sealed record GetCartCheckoutQuery(Guid CheckoutId)
    : IRequest<Result<CartCheckoutResponse>>;
