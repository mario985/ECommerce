using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Catalog.Domain.Categories.Events;

public sealed record CategoryDeactivatedDomainEvent(Guid CategoryId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
