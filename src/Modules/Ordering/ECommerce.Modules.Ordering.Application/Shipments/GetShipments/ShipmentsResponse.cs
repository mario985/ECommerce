namespace ECommerce.Modules.Ordering.Application.Shipments.GetShipments;

public sealed record ShipmentsResponse(
    IReadOnlyCollection<ShipmentResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
