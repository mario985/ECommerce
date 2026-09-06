using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Catalog.Domain.Categories.Events;

public sealed record CategoryUpdatedDomainEvent(Guid CategoryId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
