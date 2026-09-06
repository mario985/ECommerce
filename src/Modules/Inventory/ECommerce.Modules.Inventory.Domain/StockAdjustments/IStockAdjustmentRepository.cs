namespace ECommerce.Modules.Inventory.Domain.StockAdjustments;

public interface IStockAdjustmentRepository
{
    Task AddAsync(StockAdjustment adjustment, CancellationToken cancellationToken);

    Task<StockAdjustmentSearchResult> SearchAsync(
        Guid productId,
        StockAdjustmentType? type,
        StockAdjustmentReason? reason,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
