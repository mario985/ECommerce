namespace ECommerce.Modules.Ordering.Contracts.IntegrationEvents;

public sealed record OrderReservationItem(Guid ProductId, int Quantity);
