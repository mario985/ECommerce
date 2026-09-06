using ECommerce.Modules.Ordering.Domain.Orders;

namespace ECommerce.Modules.Ordering.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> ExistsByCheckoutIdAsync(Guid checkoutId, CancellationToken cancellationToken);

    Task AddAsync(Order order, CancellationToken cancellationToken);

    Task<(IReadOnlyCollection<Order> Orders, int TotalCount)> GetHistoryAsync(
        Guid customerId,
        OrderStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
