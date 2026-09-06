namespace ECommerce.Modules.Ordering.Application.Orders.GetOrderHistory;

public sealed record OrderHistoryResponse(
    IReadOnlyCollection<OrderSummaryResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
