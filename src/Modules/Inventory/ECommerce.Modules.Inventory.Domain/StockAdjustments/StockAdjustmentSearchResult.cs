namespace ECommerce.Modules.Inventory.Domain.StockAdjustments;

public sealed record StockAdjustmentSearchResult(
    IReadOnlyCollection<StockAdjustment> Items,
    long TotalCount);
