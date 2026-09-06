using MediatR;

namespace ECommerce.Modules.Inventory.Application.Reservations.ReleaseInventoryReservation;

public sealed record ReleaseInventoryReservationCommand(Guid ReservationId) : IRequest;
