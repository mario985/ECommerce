namespace ECommerce.Modules.Inventory.Application.Reservations;

public sealed class InvalidReservationRequestException(
    string message,
    Exception? innerException = null)
    : Exception(message, innerException);
