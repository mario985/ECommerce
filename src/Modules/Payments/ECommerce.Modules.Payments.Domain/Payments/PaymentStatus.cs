namespace ECommerce.Modules.Payments.Domain.Payments;

public enum PaymentStatus
{
    Pending = 1,
    RequiresPaymentMethod = 2,
    Processing = 3,
    Succeeded = 4,
    Failed = 5,
    Cancelled = 6,
}
