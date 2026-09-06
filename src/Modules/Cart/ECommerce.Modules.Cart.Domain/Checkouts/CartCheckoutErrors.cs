namespace ECommerce.Modules.Cart.Domain.Checkouts;

public static class CartCheckoutErrors
{
    public const string NotFoundCode = "CartCheckout.NotFound";
    public const string NotFoundDescription = "The cart checkout could not be found.";
    public const string AlreadyPendingCode = "CartCheckout.AlreadyPending";
    public const string AlreadyPendingDescription = "The cart already has a pending checkout.";
    public const string AlreadyCompletedCode = "CartCheckout.AlreadyCompleted";
    public const string AlreadyCompletedDescription = "The cart checkout is already completed.";
    public const string InvalidStateCode = "CartCheckout.InvalidState";
    public const string InvalidStateDescription = "The cart checkout is not in a valid state for this operation.";
    public const string ForbiddenCode = "CartCheckout.Forbidden";
    public const string ForbiddenDescription = "The cart checkout belongs to another customer.";
}
