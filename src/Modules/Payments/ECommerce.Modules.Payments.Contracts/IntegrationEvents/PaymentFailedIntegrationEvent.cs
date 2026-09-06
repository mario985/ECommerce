using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Payments.Contracts.IntegrationEvents;

public sealed record PaymentFailedIntegrationEvent(
    Guid EventId,
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    string? FailureCode,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
