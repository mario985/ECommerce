using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Identity.Domain.Users.Events;

public sealed record UserRegisteredDomainEvent(Guid UserId, string Email) : IDomainEvent;
