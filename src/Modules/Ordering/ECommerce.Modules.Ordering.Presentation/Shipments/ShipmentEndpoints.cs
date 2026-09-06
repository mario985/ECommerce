using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation;
using ECommerce.Common.Presentation.Results;
using ECommerce.Modules.Ordering.Application.Shipments;
using ECommerce.Modules.Ordering.Application.Shipments.CreateShipment;
using ECommerce.Modules.Ordering.Application.Shipments.GetOrderTracking;
using ECommerce.Modules.Ordering.Application.Shipments.GetShipments;
using ECommerce.Modules.Ordering.Application.Shipments.UpdateShipmentStatus;
using ECommerce.Modules.Ordering.Domain.Shipments;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Modules.Ordering.Presentation.Shipments;

public static class ShipmentEndpoints
{
    public static IEndpointRouteBuilder MapShipmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder adminOrders = endpoints.MapGroup("/api/v1/admin/orders")
            .RequireAuthorization(AuthorizationPolicyNames.AdminOnly)
            .RequireRateLimiting(RateLimitingPolicyNames.Admin)
            .WithTags("Admin Shipments");
        adminOrders.MapPost("/{orderId:guid}/shipment", CreateShipmentAsync)
            .WithName("OrderingAdmin.CreateShipment")
            .WithSummary("Create an order shipment")
            .WithDescription("Creates a pending shipment for a paid order with confirmed inventory reservation.")
            .Produces<ShipmentResponse>(StatusCodes.Status201Created);

        RouteGroupBuilder adminShipments = endpoints.MapGroup("/api/v1/admin/shipments")
            .RequireAuthorization(AuthorizationPolicyNames.AdminOnly)
            .RequireRateLimiting(RateLimitingPolicyNames.Admin)
            .WithTags("Admin Shipments");
        adminShipments.MapPut("/{id:guid}/status", UpdateStatusAsync)
            .WithName("OrderingAdmin.UpdateShipmentStatus")
            .WithSummary("Update shipment status")
            .WithDescription("Moves a shipment through its controlled fulfillment lifecycle.")
            .Produces<ShipmentResponse>();
        adminShipments.MapGet("/", GetShipmentsAsync)
            .WithName("OrderingAdmin.GetShipments")
            .WithSummary("Get shipments")
            .WithDescription("Returns a paged administrative shipment list with optional status filtering.")
            .Produces<ShipmentsResponse>();
        return endpoints;
    }

    public static RouteGroupBuilder MapShipmentTrackingRoute(this RouteGroupBuilder orders)
    {
        orders.MapGet("/{orderId:guid}/tracking", GetOrderTrackingAsync)
            .WithName("Ordering.GetOrderTracking")
            .WithTags("Order Tracking")
            .WithSummary("Get order tracking")
            .WithDescription("Returns shipment tracking for an order owned by the authenticated customer.")
            .Produces<OrderTrackingResponse>();
        return orders;
    }

    private static async Task<IResult> CreateShipmentAsync(
        Guid orderId,
        CreateShipmentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<ShipmentResponse> result = await sender.Send(
            new CreateShipmentCommand(orderId, request.Carrier, request.TrackingNumber),
            cancellationToken);
        return result.IsSuccess
            ? Results.Created("/api/v1/admin/shipments", result.Value)
            : ApiResults.Problem(result.Error!);
    }

    private static async Task<IResult> UpdateStatusAsync(
        Guid id,
        UpdateShipmentStatusRequest request,
        ISender sender,
        CancellationToken cancellationToken) =>
        ToHttp(await sender.Send(
            new UpdateShipmentStatusCommand(id, request.Status), cancellationToken));

    private static async Task<IResult> GetShipmentsAsync(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20,
        ShipmentStatus? status = null) =>
        ToHttp(await sender.Send(
            new GetShipmentsQuery(page, pageSize, status), cancellationToken));

    private static async Task<IResult> GetOrderTrackingAsync(
        Guid orderId,
        ISender sender,
        CancellationToken cancellationToken) =>
        ToHttp(await sender.Send(new GetOrderTrackingQuery(orderId), cancellationToken));

    private static IResult ToHttp<T>(Result<T> result) =>
        result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result.Error!);
}
