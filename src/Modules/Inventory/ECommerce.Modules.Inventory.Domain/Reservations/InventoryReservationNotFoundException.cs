namespace ECommerce.Modules.Inventory.Domain.Reservations;

public sealed class InventoryReservationNotFoundException(Guid reservationId)
    : Exception($"Reservation '{reservationId}' does not belong to this stock item.")
{
    public Guid ReservationId { get; } = reservationId;
}
