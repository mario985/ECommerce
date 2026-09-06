namespace ECommerce.Modules.Payments.Domain.Payments;

public enum PaymentStateTransitionOutcome
{
    Applied = 1,
    AlreadyApplied = 2,
    InvalidState = 3,
}
