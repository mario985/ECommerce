using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Ordering.Contracts.IntegrationEvents;

public sealed record OrderAwaitingPaymentIntegrationEvent(
    Guid EventId,
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
