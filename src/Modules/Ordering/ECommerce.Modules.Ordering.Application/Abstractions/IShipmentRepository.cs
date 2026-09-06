using ECommerce.Modules.Ordering.Domain.Shipments;

namespace ECommerce.Modules.Ordering.Application.Abstractions;

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Shipment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task<bool> ExistsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task AddAsync(Shipment shipment, CancellationToken cancellationToken);

    Task<(IReadOnlyCollection<Shipment> Shipments, int TotalCount)> GetShipmentsAsync(
        ShipmentStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
