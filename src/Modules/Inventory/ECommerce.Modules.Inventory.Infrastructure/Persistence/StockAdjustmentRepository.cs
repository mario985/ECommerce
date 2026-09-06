using ECommerce.Modules.Inventory.Domain.StockAdjustments;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Inventory.Infrastructure.Persistence;

public sealed class StockAdjustmentRepository(InventoryDbContext dbContext)
    : IStockAdjustmentRepository
{
    public async Task AddAsync(
        StockAdjustment adjustment,
        CancellationToken cancellationToken)
    {
        await dbContext.StockAdjustments.AddAsync(adjustment, cancellationToken);
    }

    public async Task<StockAdjustmentSearchResult> SearchAsync(
        Guid productId,
        StockAdjustmentType? type,
        StockAdjustmentReason? reason,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<StockAdjustment> query = dbContext.StockAdjustments
            .AsNoTracking()
            .Where(adjustment => adjustment.ProductId == productId);
        if (type.HasValue)
        {
            query = query.Where(adjustment => adjustment.Type == type.Value);
        }
        if (reason.HasValue)
        {
            query = query.Where(adjustment => adjustment.Reason == reason.Value);
        }
        if (fromUtc.HasValue)
        {
            DateTimeOffset normalizedFromUtc = fromUtc.Value.ToUniversalTime();
            query = query.Where(adjustment => adjustment.OccurredAtUtc >= normalizedFromUtc);
        }
        if (toUtc.HasValue)
        {
            DateTimeOffset normalizedToUtc = toUtc.Value.ToUniversalTime();
            query = query.Where(adjustment => adjustment.OccurredAtUtc <= normalizedToUtc);
        }

        long count = await query.LongCountAsync(cancellationToken);
        StockAdjustment[] items = await query
            .OrderByDescending(adjustment => adjustment.OccurredAtUtc)
            .ThenByDescending(adjustment => adjustment.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
        return new StockAdjustmentSearchResult(items, count);
    }
}
