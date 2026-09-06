namespace ECommerce.Modules.Ordering.Domain.Orders;

public static class OrderErrors
{
    public const string NotFoundCode = "Order.NotFound";
    public const string NotFoundDescription = "The order could not be found.";
    public const string EmptyCode = "Order.Empty";
    public const string EmptyDescription = "An order must contain at least one item.";
    public const string InvalidStateCode = "Order.InvalidState";
    public const string InvalidStateDescription = "The order cannot make the requested state transition.";
    public const string CurrencyMismatchCode = "Order.CurrencyMismatch";
    public const string CurrencyMismatchDescription = "All order lines must use the same currency.";
    public const string CheckoutAlreadyProcessedCode = "Order.CheckoutAlreadyProcessed";
    public const string CheckoutAlreadyProcessedDescription = "The checkout has already been processed.";
    public const string ForbiddenCode = "Order.Forbidden";
    public const string ForbiddenDescription = "The order belongs to another customer.";
    public const string UnauthorizedCode = "Order.Unauthorized";
    public const string UnauthorizedDescription = "An authenticated customer is required.";
    public const string InvalidHistoryCode = "Order.InvalidHistory";
    public const string InvalidHistoryDescription = "Order history parameters are invalid.";
    public const string InvalidCheckoutCode = "Order.InvalidCheckout";
    public const string InvalidCheckoutDescription = "The checkout snapshot is invalid.";
}
