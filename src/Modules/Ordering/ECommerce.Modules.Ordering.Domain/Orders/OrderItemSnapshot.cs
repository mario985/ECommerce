namespace ECommerce.Modules.Ordering.Domain.Orders;

public sealed record OrderItemSnapshot(
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    string Currency,
    int Quantity);
