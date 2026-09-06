namespace ECommerce.Modules.Inventory.Application.Reservations;

public sealed class ReservationConflictException(
    string message,
    Exception? innerException = null)
    : Exception(message, innerException);
