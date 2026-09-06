using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Identity.Contracts.IntegrationEvents;

public sealed record UserRegisteredIntegrationEvent(
    Guid EventId,
    Guid UserId,
    string Email,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
