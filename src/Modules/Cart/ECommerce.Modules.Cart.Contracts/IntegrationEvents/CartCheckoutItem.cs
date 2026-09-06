namespace ECommerce.Modules.Cart.Contracts.IntegrationEvents;

public sealed record CartCheckoutItem(
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    string Currency,
    int Quantity);
