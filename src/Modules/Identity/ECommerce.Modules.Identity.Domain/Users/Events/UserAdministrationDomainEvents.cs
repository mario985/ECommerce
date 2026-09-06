using ECommerce.Common.Domain.Events;
namespace ECommerce.Modules.Identity.Domain.Users.Events;
public sealed record UserRoleAssignedDomainEvent(Guid UserId, string Role, DateTimeOffset OccurredAtUtc) : IDomainEvent;
public sealed record UserRoleRemovedDomainEvent(Guid UserId, string Role, DateTimeOffset OccurredAtUtc) : IDomainEvent;
public sealed record UserDisabledDomainEvent(Guid UserId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
public sealed record UserEnabledDomainEvent(Guid UserId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
