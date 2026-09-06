using ECommerce.Modules.Ordering.Domain.Orders;

namespace ECommerce.Modules.Ordering.Application.Orders.GetOrder;

public sealed record OrderResponse(
    Guid Id,
    Guid CheckoutId,
    string Status,
    IReadOnlyCollection<OrderLineResponse> Items,
    decimal TotalAmount,
    string Currency,
    DateTimeOffset CreatedAtUtc,
    Guid? PaymentId,
    DateTimeOffset? PaidAtUtc,
    DateTimeOffset? PaymentFailedAtUtc)
{
    public static OrderResponse From(Order order) => new(
        order.Id,
        order.CheckoutId,
        order.Status.ToString(),
        order.Lines.Select(OrderLineResponse.From).ToArray(),
        order.TotalAmount,
        order.Currency,
        order.CreatedAtUtc,
        order.PaymentId,
        order.PaidAtUtc,
        order.PaymentFailedAtUtc);
}
