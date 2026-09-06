using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Ordering.Contracts.IntegrationEvents;

public sealed record OrderPaymentFailedIntegrationEvent(
    Guid EventId,
    Guid OrderId,
    Guid CheckoutId,
    Guid CustomerId,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
