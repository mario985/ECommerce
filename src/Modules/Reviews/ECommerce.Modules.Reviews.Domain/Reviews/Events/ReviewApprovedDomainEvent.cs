using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Reviews.Domain.Reviews.Events;

public sealed record ReviewApprovedDomainEvent(
    Guid ReviewId,
    Guid ProductId,
    Guid CustomerId,
    DateTimeOffset OccurredAtUtc) : IDomainEvent;
