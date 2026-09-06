namespace ECommerce.Modules.Wishlist.Domain.WishlistItems;

public static class WishlistErrors
{
    public const string ItemNotFoundCode = "Wishlist.ItemNotFound";
    public const string ItemNotFoundDescription = "The wishlist item could not be found.";
    public const string DuplicateItemCode = "Wishlist.DuplicateItem";
    public const string DuplicateItemDescription = "The product is already in the wishlist.";
    public const string ProductNotFoundCode = "Wishlist.ProductNotFound";
    public const string ProductNotFoundDescription = "The product could not be found.";
    public const string UnauthorizedAccessCode = "Wishlist.UnauthorizedAccess";
    public const string UnauthorizedAccessDescription = "An authenticated customer is required.";
    public const string CartOperationFailedCode = "Wishlist.CartOperationFailed";
    public const string CartOperationFailedDescription = "The product could not be added to the cart.";
    public const string InvalidProductIdCode = "Wishlist.InvalidProductId";
    public const string InvalidProductIdDescription = "A product ID is required.";
}
