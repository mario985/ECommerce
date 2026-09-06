namespace ECommerce.Modules.Wishlist.Application.GetWishlist;

public sealed record WishlistResponse(IReadOnlyList<WishlistItemResponse> Items);

public sealed record WishlistItemResponse(
    Guid ProductId,
    string Name,
    decimal Price,
    string Currency,
    DateTimeOffset AddedAtUtc);
