using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation.Results;
using ECommerce.Modules.Wishlist.Application.AddWishlistItem;
using ECommerce.Modules.Wishlist.Application.CheckWishlistItem;
using ECommerce.Modules.Wishlist.Application.GetWishlist;
using ECommerce.Modules.Wishlist.Application.MoveWishlistItemToCart;
using ECommerce.Modules.Wishlist.Application.RemoveWishlistItem;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Modules.Wishlist.Presentation;

public static class WishlistEndpoints
{
    public static IEndpointRouteBuilder MapWishlistEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/api/v1/wishlist")
            .RequireAuthorization()
            .WithTags("Wishlist");
        group.MapPost("/items/{productId:guid}", AddAsync).WithName("Wishlist.AddItem").WithSummary("Add a product to the wishlist").Produces<Guid>(StatusCodes.Status201Created).ProducesProblem(409);
        group.MapDelete("/items/{productId:guid}", RemoveAsync).WithName("Wishlist.RemoveItem").WithSummary("Remove a product from the wishlist").Produces(StatusCodes.Status204NoContent);
        group.MapGet("", GetAsync).WithName("Wishlist.Get").WithSummary("Get the current customer's wishlist").Produces<WishlistResponse>();
        group.MapGet("/items/{productId:guid}/exists", ExistsAsync).WithName("Wishlist.Exists").WithSummary("Check whether a product is favorited").Produces<CheckWishlistItemResponse>();
        group.MapPost("/items/{productId:guid}/cart", MoveToCartAsync).WithName("Wishlist.MoveToCart").WithSummary("Move a wishlist item to the cart").Produces(StatusCodes.Status204NoContent);
        return endpoints;
    }

    private static async Task<IResult> AddAsync(Guid productId, ISender sender, CancellationToken token)
    {
        Result<Guid> result = await sender.Send(new AddWishlistItemCommand(productId), token);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.Created($"/api/v1/wishlist/items/{productId}", new { id = result.Value, productId });
    }
    private static async Task<IResult> RemoveAsync(Guid productId, ISender sender, CancellationToken token)
    {
        Result result = await sender.Send(new RemoveWishlistItemCommand(productId), token);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.NoContent();
    }
    private static async Task<IResult> GetAsync(ISender sender, CancellationToken token)
    {
        Result<WishlistResponse> result = await sender.Send(new GetWishlistQuery(), token);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.Ok(result.Value);
    }
    private static async Task<IResult> ExistsAsync(Guid productId, ISender sender, CancellationToken token)
    {
        Result<CheckWishlistItemResponse> result = await sender.Send(new CheckWishlistItemQuery(productId), token);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.Ok(result.Value);
    }
    private static async Task<IResult> MoveToCartAsync(Guid productId, ISender sender, CancellationToken token)
    {
        Result result = await sender.Send(new MoveWishlistItemToCartCommand(productId), token);
        return result.IsFailure ? ApiResults.Problem(result.Error!) : Results.NoContent();
    }
}
