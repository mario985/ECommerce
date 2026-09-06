namespace ECommerce.Modules.Inventory.Application.Reservations;

public sealed class ReservationNotFoundException(Guid reservationId)
    : Exception($"Inventory reservation '{reservationId}' was not found.")
{
    public Guid ReservationId { get; } = reservationId;
}
