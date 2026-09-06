namespace ECommerce.Modules.Inventory.Domain.StockItems;

public sealed record StockItemSearchResult(
    IReadOnlyCollection<StockItem> Items,
    long TotalCount);
