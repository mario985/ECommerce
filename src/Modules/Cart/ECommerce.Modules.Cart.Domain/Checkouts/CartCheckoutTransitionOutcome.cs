namespace ECommerce.Modules.Cart.Domain.Checkouts;

public enum CartCheckoutTransitionOutcome
{
    Applied = 1,
    AlreadyApplied = 2,
    InvalidState = 3,
}
