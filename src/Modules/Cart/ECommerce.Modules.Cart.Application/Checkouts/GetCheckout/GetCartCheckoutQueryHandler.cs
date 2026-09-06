using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Cart.Application.Abstractions;
using ECommerce.Modules.Cart.Domain.Checkouts;
using MediatR;

namespace ECommerce.Modules.Cart.Application.Checkouts.GetCheckout;

public sealed class GetCartCheckoutQueryHandler(
    ICartCheckoutRepository checkoutRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetCartCheckoutQuery, Result<CartCheckoutResponse>>
{
    public async Task<Result<CartCheckoutResponse>> Handle(
        GetCartCheckoutQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure<CartCheckoutResponse>(new Error(
                "Cart.Unauthorized", "An authenticated customer is required.", ErrorType.Unauthorized));
        }

        CartCheckout? checkout = await checkoutRepository.GetByCheckoutIdAsync(
            request.CheckoutId, cancellationToken);
        if (checkout is null)
        {
            return Result.Failure<CartCheckoutResponse>(new Error(
                CartCheckoutErrors.NotFoundCode,
                CartCheckoutErrors.NotFoundDescription,
                ErrorType.NotFound));
        }

        if (checkout.CustomerId != currentUser.UserId.Value)
        {
            return Result.Failure<CartCheckoutResponse>(new Error(
                CartCheckoutErrors.ForbiddenCode,
                CartCheckoutErrors.ForbiddenDescription,
                ErrorType.Forbidden));
        }

        return Result.Success(new CartCheckoutResponse(
            checkout.CheckoutId,
            checkout.OrderId,
            checkout.Status.ToString(),
            checkout.Items.Select(item => new CartCheckoutItemResponse(
                item.ProductId, item.ProductName, item.Sku,
                item.UnitPrice, item.Currency, item.Quantity)).ToArray(),
            checkout.CreatedAtUtc,
            checkout.CompletedAtUtc,
            checkout.FailedAtUtc));
    }
}
