using ECommerce.Modules.Cart.Domain.Carts;
using CartAggregate = ECommerce.Modules.Cart.Domain.Carts.Cart;

namespace ECommerce.Modules.Cart.Application.Carts.GetCart;

public sealed record CartResponse(
    Guid? Id,
    IReadOnlyCollection<CartItemResponse> Items,
    decimal Total,
    string? Currency)
{
    public static CartResponse Empty() => new(null, [], 0m, null);

    public static CartResponse From(CartAggregate cart)
    {
        CartItemResponse[] items = cart.Items
            .Select(ToResponse)
            .ToArray();

        return new CartResponse(cart.Id, items, cart.Total, cart.Currency);
    }

    private static CartItemResponse ToResponse(CartItem item) => new(
        item.ProductId,
        item.ProductName,
        item.Sku,
        item.UnitPrice,
        item.Currency,
        item.Quantity,
        item.LineTotal);
}
