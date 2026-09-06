using ECommerce.Common.Domain.Entities;

namespace ECommerce.Modules.Inventory.Domain.Reservations;

public sealed class InventoryReservation : Entity<Guid>
{
    private InventoryReservation()
    {
    }

    internal InventoryReservation(
        Guid stockItemId,
        Guid productId,
        int quantity,
        DateTimeOffset createdAtUtc,
        Guid? orderId)
        : base(Guid.NewGuid())
    {
        StockItemId = stockItemId;
        ProductId = productId;
        Quantity = quantity;
        Status = InventoryReservationStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        OrderId = orderId;
    }

    public Guid StockItemId { get; private set; }

    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    public InventoryReservationStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public Guid? OrderId { get; private set; }

    internal void Release()
    {
        EnsureCanTransitionTo(InventoryReservationStatus.Released);
        Status = InventoryReservationStatus.Released;
    }

    internal void Confirm()
    {
        EnsureCanTransitionTo(InventoryReservationStatus.Confirmed);
        Status = InventoryReservationStatus.Confirmed;
    }

    internal void EnsureCanTransitionTo(InventoryReservationStatus targetStatus)
    {
        if (Status != InventoryReservationStatus.Pending)
        {
            throw new InvalidReservationTransitionException(Id, Status, targetStatus);
        }
    }
}
