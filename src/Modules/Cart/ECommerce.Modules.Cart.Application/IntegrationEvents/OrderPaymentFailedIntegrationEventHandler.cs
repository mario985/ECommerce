using ECommerce.Modules.Cart.Application.Abstractions;
using ECommerce.Modules.Cart.Application.Carts;
using ECommerce.Modules.Cart.Domain.Checkouts;
using ECommerce.Modules.Ordering.Contracts.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Cart.Application.IntegrationEvents;

#pragma warning disable CA1711
public sealed class OrderPaymentFailedIntegrationEventHandler(
    ICartCheckoutRepository checkoutRepository,
    ILogger<OrderPaymentFailedIntegrationEventHandler> logger)
    : INotificationHandler<OrderPaymentFailedIntegrationEvent>
{
    public async Task Handle(
        OrderPaymentFailedIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        CartLog.PaymentFailureReceived(
            logger, notification.CheckoutId, notification.OrderId, notification.CustomerId);
        CartCheckout? checkout = await checkoutRepository.GetByCheckoutIdAsync(
            notification.CheckoutId, cancellationToken);
        if (checkout is null || checkout.CustomerId != notification.CustomerId)
        {
            CartLog.FinalizationCheckoutMissing(
                logger, notification.CheckoutId, notification.OrderId, notification.CustomerId);
            return;
        }

        CartCheckoutTransitionOutcome outcome = checkout.MarkFailed(
            notification.OrderId, notification.OccurredAtUtc);
        if (outcome != CartCheckoutTransitionOutcome.Applied)
        {
            CartLog.DuplicateFinalizationIgnored(
                logger, checkout.CheckoutId, notification.OrderId, checkout.Status.ToString());
            return;
        }

        await checkoutRepository.SaveChangesAsync(cancellationToken);
        CartLog.CheckoutFailed(
            logger, checkout.CheckoutId, checkout.CartId, notification.OrderId, checkout.CustomerId);
    }
}
#pragma warning restore CA1711
