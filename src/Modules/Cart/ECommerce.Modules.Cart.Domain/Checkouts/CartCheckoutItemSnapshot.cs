namespace ECommerce.Modules.Cart.Domain.Checkouts;

public sealed record CartCheckoutItemSnapshot(
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    string Currency,
    int Quantity);
