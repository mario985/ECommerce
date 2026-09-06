using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Payments.Contracts.IntegrationEvents;

public sealed record PaymentIntentCreatedIntegrationEvent(
    Guid EventId,
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
