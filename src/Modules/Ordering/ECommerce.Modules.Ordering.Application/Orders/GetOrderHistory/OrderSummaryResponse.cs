using ECommerce.Modules.Ordering.Domain.Orders;

namespace ECommerce.Modules.Ordering.Application.Orders.GetOrderHistory;

public sealed record OrderSummaryResponse(
    Guid Id,
    Guid CheckoutId,
    string Status,
    int ItemCount,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PaidAtUtc,
    DateTimeOffset? PaymentFailedAtUtc)
{
    public static OrderSummaryResponse From(Order order) => new(
        order.Id,
        order.CheckoutId,
        order.Status.ToString(),
        order.Lines.Count,
        order.TotalAmount,
        order.Currency,
        order.CreatedAtUtc,
        order.PaidAtUtc,
        order.PaymentFailedAtUtc);
}
