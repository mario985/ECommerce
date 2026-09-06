namespace ECommerce.Modules.Inventory.Domain.Reservations;

public sealed class InvalidReservationTransitionException(
    Guid reservationId,
    InventoryReservationStatus status,
    InventoryReservationStatus targetStatus)
    : Exception(
        $"Reservation '{reservationId}' cannot transition from {status} to {targetStatus}.")
{
    public Guid ReservationId { get; } = reservationId;

    public InventoryReservationStatus Status { get; } = status;

    public InventoryReservationStatus TargetStatus { get; } = targetStatus;
}
