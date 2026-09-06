using MediatR;

namespace ECommerce.Modules.Inventory.Application.Reservations.ConfirmInventoryReservation;

public sealed record ConfirmInventoryReservationCommand(Guid ReservationId) : IRequest;
