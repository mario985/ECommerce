using ECommerce.Modules.Cart.Application.Abstractions;
using ECommerce.Common.Application.Observability;
using ECommerce.Modules.Cart.Application.Carts;
using ECommerce.Modules.Cart.Domain.Checkouts;
using ECommerce.Modules.Ordering.Contracts.IntegrationEvents;
using MediatR;
using Microsoft.Extensions.Logging;
using CartAggregate = ECommerce.Modules.Cart.Domain.Carts.Cart;

namespace ECommerce.Modules.Cart.Application.IntegrationEvents;

#pragma warning disable CA1711
public sealed class OrderPaidIntegrationEventHandler(
    ICartCheckoutRepository checkoutRepository,
    ICartRepository cartRepository,
    ILogger<OrderPaidIntegrationEventHandler> logger)
    : INotificationHandler<OrderPaidIntegrationEvent>
{
    public async Task Handle(OrderPaidIntegrationEvent notification, CancellationToken cancellationToken)
    {
        using System.Diagnostics.Activity? activity = CommerceActivitySources.Cart.StartActivity("Cart.ReconcileCheckout");
        activity?.SetTag("ecommerce.checkout.id", notification.CheckoutId);
        activity?.SetTag("ecommerce.order.id", notification.OrderId);
        CartLog.OrderPaidReceived(
            logger, notification.CheckoutId, notification.OrderId, notification.CustomerId);
        CartCheckout? checkout = await checkoutRepository.GetByCheckoutIdAsync(
            notification.CheckoutId, cancellationToken);
        if (checkout is null || checkout.CustomerId != notification.CustomerId)
        {
            CartLog.FinalizationCheckoutMissing(
                logger, notification.CheckoutId, notification.OrderId, notification.CustomerId);
            return;
        }

        if (checkout.Status != CartCheckoutStatus.Pending)
        {
            CartLog.DuplicateFinalizationIgnored(
                logger, checkout.CheckoutId, notification.OrderId, checkout.Status.ToString());
            return;
        }

        CartAggregate? cart = await cartRepository.GetByIdAsync(checkout.CartId, cancellationToken);
        if (cart is null)
        {
            CartLog.FinalizationCheckoutMissing(
                logger, checkout.CheckoutId, notification.OrderId, notification.CustomerId);
            return;
        }

        CartLog.ReconciliationStarted(
            logger, checkout.CheckoutId, cart.Id, notification.OrderId, checkout.CustomerId);
        foreach (CartCheckoutItem item in checkout.Items)
        {
            if (cart.FindItem(item.ProductId) is not null)
            {
                CartLog.PurchasedQuantityRemoved(
                    logger, checkout.CheckoutId, cart.Id, item.ProductId,
                    Math.Min(item.Quantity, cart.FindItem(item.ProductId)!.Quantity));
            }
        }

        cart.ReconcileCompletedCheckout(
            checkout.CheckoutId,
            checkout.Items.Select(item => new CartCheckoutItemSnapshot(
                item.ProductId, item.ProductName, item.Sku, item.UnitPrice,
                item.Currency, item.Quantity)).ToArray(),
            notification.OccurredAtUtc);
        CartCheckoutTransitionOutcome outcome = checkout.MarkCompleted(
            notification.OrderId, notification.OccurredAtUtc);
        if (outcome != CartCheckoutTransitionOutcome.Applied)
        {
            return;
        }

        await checkoutRepository.SaveChangesAsync(cancellationToken);
        CartLog.CheckoutCompleted(
            logger, checkout.CheckoutId, cart.Id, notification.OrderId, checkout.CustomerId);
        activity?.SetTag("ecommerce.cart.id", cart.Id);
    }
}
#pragma warning restore CA1711
