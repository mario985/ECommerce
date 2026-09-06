namespace ECommerce.Modules.Inventory.Domain.StockItems;

public interface IInventoryAdminRepository
{
    Task<StockItemSearchResult> SearchAsync(
        string? search,
        string? sku,
        bool? lowStock,
        int lowStockThreshold,
        int? availableQuantityLessThan,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
