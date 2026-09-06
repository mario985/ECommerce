namespace ECommerce.Modules.Cart.Domain.Carts;

public static class CartErrors
{
    public const string NotFoundCode = "Cart.NotFound";
    public const string NotFoundDescription = "The cart could not be found.";
    public const string ItemNotFoundCode = "Cart.ItemNotFound";
    public const string ItemNotFoundDescription = "The cart item could not be found.";
    public const string EmptyCode = "Cart.Empty";
    public const string EmptyDescription = "The cart is empty and cannot be checked out.";
    public const string InvalidCheckoutCode = "Cart.InvalidCheckout";
    public const string InvalidCheckoutDescription = "The cart is not valid for checkout.";
    public const string InvalidQuantityCode = "Cart.InvalidQuantity";
    public const string InvalidQuantityDescription = "Cart item quantity must be greater than zero.";
    public const string InvalidProductIdCode = "Cart.InvalidProductId";
    public const string InvalidProductIdDescription = "A product ID is required.";
    public const string ProductNotFoundCode = "Cart.ProductNotFound";
    public const string ProductNotFoundDescription = "The product could not be found.";
    public const string ProductInactiveCode = "Cart.ProductInactive";
    public const string ProductInactiveDescription = "The product is not active.";
    public const string CurrencyMismatchCode = "Cart.CurrencyMismatch";
    public const string CurrencyMismatchDescription = "All cart items must use the same currency.";
    public const string UnauthorizedCode = "Cart.Unauthorized";
    public const string UnauthorizedDescription = "An authenticated customer is required.";
}
