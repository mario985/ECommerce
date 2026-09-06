using ECommerce.Modules.Inventory.Domain.StockItems;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Inventory.Infrastructure.Persistence;

public sealed class StockItemRepository(InventoryDbContext dbContext)
    : IStockItemRepository, IInventoryAdminRepository
{
    public Task<StockItem?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return dbContext.StockItems.SingleOrDefaultAsync(
            stockItem => stockItem.ProductId == productId,
            cancellationToken);
    }

    public Task<StockItem?> GetByReservationIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken)
    {
        return dbContext.StockItems
            .Include(stockItem => stockItem.Reservations)
            .SingleOrDefaultAsync(
                stockItem => stockItem.Reservations.Any(
                    reservation => reservation.Id == reservationId),
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<StockItem>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await dbContext.StockItems
            .Include(stockItem => stockItem.Reservations)
            .Where(stockItem => stockItem.Reservations.Any(
                reservation => reservation.OrderId == orderId))
            .ToArrayAsync(cancellationToken);
    }

    public Task<bool> ExistsByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return dbContext.StockItems.AnyAsync(
            stockItem => stockItem.ProductId == productId,
            cancellationToken);
    }

    public async Task<StockItemSearchResult> SearchAsync(
        string? search,
        string? sku,
        bool? lowStock,
        int lowStockThreshold,
        int? availableQuantityLessThan,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<StockItem> query = dbContext.StockItems.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            string term = search.Trim();
            query = query.Where(item => EF.Functions.Like(item.Sku, $"%{term}%"));
        }
        if (!string.IsNullOrWhiteSpace(sku))
        {
            string normalizedSku = sku.Trim().ToUpperInvariant();
            query = query.Where(item => item.Sku == normalizedSku);
        }
        if (lowStock.HasValue)
        {
            query = lowStock.Value
                ? query.Where(item => item.AvailableQuantity <= lowStockThreshold)
                : query.Where(item => item.AvailableQuantity > lowStockThreshold);
        }
        if (availableQuantityLessThan.HasValue)
        {
            query = query.Where(item =>
                item.AvailableQuantity < availableQuantityLessThan.Value);
        }

        long count = await query.LongCountAsync(cancellationToken);
        StockItem[] items = await query
            .OrderBy(item => item.Sku)
            .ThenBy(item => item.ProductId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
        return new StockItemSearchResult(items, count);
    }

    public async Task AddAsync(
        StockItem stockItem,
        CancellationToken cancellationToken)
    {
        await dbContext.StockItems.AddAsync(stockItem, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task UpdateAsync(
        StockItem stockItem,
        CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
