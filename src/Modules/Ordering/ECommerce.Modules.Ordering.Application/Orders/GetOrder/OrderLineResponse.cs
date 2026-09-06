using ECommerce.Modules.Ordering.Domain.Orders;

namespace ECommerce.Modules.Ordering.Application.Orders.GetOrder;

public sealed record OrderLineResponse(
    Guid ProductId,
    string ProductName,
    string Sku,
    decimal UnitPrice,
    string Currency,
    int Quantity,
    decimal LineTotal)
{
    public static OrderLineResponse From(OrderLine line) => new(
        line.ProductId,
        line.ProductName,
        line.Sku,
        line.UnitPrice,
        line.Currency,
        line.Quantity,
        line.LineTotal);
}
