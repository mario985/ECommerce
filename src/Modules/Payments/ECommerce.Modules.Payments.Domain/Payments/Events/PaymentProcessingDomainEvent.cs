using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Payments.Domain.Payments.Events;

public sealed record PaymentProcessingDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId) : IDomainEvent;
