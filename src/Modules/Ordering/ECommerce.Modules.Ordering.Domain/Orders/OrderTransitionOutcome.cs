namespace ECommerce.Modules.Ordering.Domain.Orders;

public enum OrderTransitionOutcome
{
    Applied = 1,
    AlreadyApplied = 2,
    InvalidState = 3,
}
