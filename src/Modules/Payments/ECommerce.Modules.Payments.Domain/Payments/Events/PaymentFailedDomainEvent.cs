using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Payments.Domain.Payments.Events;

public sealed record PaymentFailedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    string? FailureCode) : IDomainEvent;
