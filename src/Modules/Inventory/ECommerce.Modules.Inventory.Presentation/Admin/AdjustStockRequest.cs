using ECommerce.Modules.Inventory.Domain.StockAdjustments;

namespace ECommerce.Modules.Inventory.Presentation.Admin;

public sealed record AdjustStockRequest(
    int Quantity,
    StockAdjustmentType Type,
    StockAdjustmentReason Reason,
    string? Note);
