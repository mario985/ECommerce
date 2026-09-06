using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Payments.Domain.Payments.Events;

public sealed record PaymentIntentCreatedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    Guid CustomerId,
    string ProviderPaymentIntentId) : IDomainEvent;
