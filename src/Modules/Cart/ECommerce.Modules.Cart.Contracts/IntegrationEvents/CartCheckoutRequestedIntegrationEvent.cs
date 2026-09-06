using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Cart.Contracts.IntegrationEvents;

public sealed record CartCheckoutRequestedIntegrationEvent(
    Guid EventId,
    Guid CheckoutId,
    Guid CartId,
    Guid CustomerId,
    IReadOnlyCollection<CartCheckoutItem> Items,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
