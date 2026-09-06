using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation.Results;
using ECommerce.Modules.Cart.Application.Carts.AddItem;
using ECommerce.Modules.Cart.Application.Carts.Checkout;
using ECommerce.Modules.Cart.Application.Carts.ClearCart;
using ECommerce.Modules.Cart.Application.Carts.GetCart;
using ECommerce.Modules.Cart.Application.Carts.RemoveItem;
using ECommerce.Modules.Cart.Application.Carts.UpdateQuantity;
using ECommerce.Modules.Cart.Application.Checkouts.GetCheckout;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Modules.Cart.Presentation.Carts;

public static class CartEndpoints
{
    public static RouteGroupBuilder MapCartRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("", GetCartAsync).WithName("Cart.GetCart").WithTags("Cart");
        group.MapPost("/items", AddItemAsync).WithName("Cart.AddItem").WithTags("Cart");
        group.MapPut("/items/{productId:guid}", UpdateQuantityAsync).WithName("Cart.UpdateItem").WithTags("Cart");
        group.MapDelete("/items/{productId:guid}", RemoveItemAsync).WithName("Cart.RemoveItem").WithTags("Cart");
        group.MapDelete("", ClearCartAsync).WithName("Cart.ClearCart").WithTags("Cart");
        group.MapPost("/checkout", CheckoutAsync).WithName("Cart.Checkout").WithTags("Cart");
        group.MapGet("/checkouts/{checkoutId:guid}", GetCheckoutAsync).WithName("Cart.GetCheckout").WithTags("Cart");
        return group;
    }

    private static async Task<IResult> GetCartAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<CartResponse> result = await sender.Send(new GetCartQuery(), cancellationToken);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.Ok(result.Value);
    }

    private static async Task<IResult> AddItemAsync(
        AddCartItemRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<CartResponse> result = await sender.Send(
            new AddCartItemCommand(request.ProductId, request.Quantity),
            cancellationToken);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateQuantityAsync(
        Guid productId,
        UpdateCartItemQuantityRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<CartResponse> result = await sender.Send(
            new UpdateCartItemQuantityCommand(productId, request.Quantity),
            cancellationToken);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.Ok(result.Value);
    }

    private static async Task<IResult> RemoveItemAsync(
        Guid productId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(new RemoveCartItemCommand(productId), cancellationToken);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.NoContent();
    }

    private static async Task<IResult> ClearCartAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(new ClearCartCommand(), cancellationToken);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.NoContent();
    }

    private static async Task<IResult> CheckoutAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<CheckoutResponse> result = await sender.Send(
            new CheckoutCartCommand(),
            cancellationToken);
        return result.IsFailure
            ? ApiResults.Problem(result.Error!)
            : Results.Accepted(value: result.Value);
    }

    private static async Task<IResult> GetCheckoutAsync(
        Guid checkoutId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<CartCheckoutResponse> result = await sender.Send(
            new GetCartCheckoutQuery(checkoutId), cancellationToken);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.Ok(result.Value);
    }
}
