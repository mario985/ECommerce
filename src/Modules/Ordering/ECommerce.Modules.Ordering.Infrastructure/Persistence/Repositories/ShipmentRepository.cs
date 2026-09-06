using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Domain.Shipments;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Ordering.Infrastructure.Persistence.Repositories;

internal sealed class ShipmentRepository(OrderingDbContext dbContext) : IShipmentRepository
{
    public Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Shipments.SingleOrDefaultAsync(
            shipment => shipment.Id == id, cancellationToken);

    public Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        dbContext.Shipments.SingleOrDefaultAsync(
            shipment => shipment.OrderId == orderId, cancellationToken);

    public Task<bool> ExistsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken) =>
        dbContext.Shipments.AnyAsync(
            shipment => shipment.OrderId == orderId, cancellationToken);

    public async Task AddAsync(Shipment shipment, CancellationToken cancellationToken)
    {
        await dbContext.Shipments.AddAsync(shipment, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Shipment> Shipments, int TotalCount)> GetShipmentsAsync(
        ShipmentStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        IQueryable<Shipment> query = dbContext.Shipments.AsNoTracking();
        if (status.HasValue)
        {
            query = query.Where(shipment => shipment.Status == status.Value);
        }

        int totalCount = await query.CountAsync(cancellationToken);
        int skip = (int)Math.Min((long)(page - 1) * pageSize, int.MaxValue);
        Shipment[] shipments = await query
            .OrderByDescending(shipment => shipment.CreatedAtUtc)
            .ThenByDescending(shipment => shipment.Id)
            .Skip(skip)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);
        return (shipments, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
