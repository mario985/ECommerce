using ECommerce.Modules.Inventory.Domain.StockAdjustments;

namespace ECommerce.Modules.Inventory.Application.StockAdjustments.GetStockAdjustmentHistory;

public sealed record StockAdjustmentResponse(
    Guid Id,
    int Quantity,
    StockAdjustmentType Type,
    StockAdjustmentReason Reason,
    string? Note,
    Guid PerformedBy,
    DateTimeOffset OccurredAtUtc);

public sealed record StockAdjustmentHistoryResponse(
    IReadOnlyCollection<StockAdjustmentResponse> Items,
    int Page,
    int PageSize,
    long TotalCount,
    int TotalPages);
