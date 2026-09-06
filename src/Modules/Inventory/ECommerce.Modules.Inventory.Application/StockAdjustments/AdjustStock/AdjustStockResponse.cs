using ECommerce.Modules.Inventory.Domain.StockAdjustments;

namespace ECommerce.Modules.Inventory.Application.StockAdjustments.AdjustStock;

public sealed record AdjustStockResponse(
    Guid Id,
    Guid ProductId,
    int Quantity,
    StockAdjustmentType Type,
    StockAdjustmentReason Reason,
    string? Note,
    int AvailableQuantity,
    int ReservedQuantity,
    Guid PerformedBy,
    DateTimeOffset OccurredAtUtc);
