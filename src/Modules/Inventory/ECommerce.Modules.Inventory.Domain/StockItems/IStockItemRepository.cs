namespace ECommerce.Modules.Inventory.Domain.StockItems;

public interface IStockItemRepository
{
    Task<StockItem?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task<StockItem?> GetByReservationIdAsync(
        Guid reservationId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<StockItem>> GetByOrderIdAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task<bool> ExistsByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task AddAsync(
        StockItem stockItem,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        StockItem stockItem,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
