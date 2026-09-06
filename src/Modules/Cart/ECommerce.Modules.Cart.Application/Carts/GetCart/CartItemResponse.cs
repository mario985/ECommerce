namespace ECommerce.Modules.Cart.Application.Carts.GetCart;

public sealed record CartItemResponse(
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal);
