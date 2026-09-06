namespace ECommerce.Modules.Payments.Domain.Payments;

public enum PaymentIntentUpdateOutcome
{
    Applied = 1,
    AlreadyCreated = 2,
    InvalidProviderReference = 3,
    InvalidClientSecret = 4,
}
