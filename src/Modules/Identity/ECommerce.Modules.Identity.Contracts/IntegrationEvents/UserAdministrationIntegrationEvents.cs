using ECommerce.Common.Application.Messaging;
namespace ECommerce.Modules.Identity.Contracts.IntegrationEvents;
public sealed record UserRoleAssignedIntegrationEvent(Guid EventId, Guid UserId, string Role, DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
public sealed record UserRoleRemovedIntegrationEvent(Guid EventId, Guid UserId, string Role, DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
public sealed record UserDisabledIntegrationEvent(Guid EventId, Guid UserId, DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
public sealed record UserEnabledIntegrationEvent(Guid EventId, Guid UserId, DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
