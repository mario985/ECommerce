using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Ordering.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository(OrderingDbContext dbContext) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Orders
            .Include(order => order.Lines)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByCheckoutIdAsync(
        Guid checkoutId,
        CancellationToken cancellationToken)
    {
        return dbContext.Orders.AnyAsync(
            order => order.CheckoutId == checkoutId,
            cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        await dbContext.Orders.AddAsync(order, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Order> Orders, int TotalCount)> GetHistoryAsync(
        Guid customerId,
        OrderStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Order> query = dbContext.Orders
            .AsNoTracking()
            .Include(order => order.Lines)
            .Where(order => order.CustomerId == customerId);
        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);
        int skip = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        Order[] orders = await query
            .OrderByDescending(order => order.CreatedAtUtc)
            .ThenByDescending(order => order.Id)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
        return (orders, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
