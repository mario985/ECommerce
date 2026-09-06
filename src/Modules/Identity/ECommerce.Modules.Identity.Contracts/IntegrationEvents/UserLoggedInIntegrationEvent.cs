using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Identity.Contracts.IntegrationEvents;

public sealed record UserLoggedInIntegrationEvent(
    Guid EventId,
    Guid UserId,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
