using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation;
using ECommerce.Common.Presentation.Results;
using ECommerce.Modules.Reviews.Application.Reviews.ApproveReview;
using ECommerce.Modules.Reviews.Application.Reviews.CreateReview;
using ECommerce.Modules.Reviews.Application.Reviews.GetPendingReviews;
using ECommerce.Modules.Reviews.Application.Reviews.GetProductReviews;
using ECommerce.Modules.Reviews.Application.Reviews.RejectReview;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Modules.Reviews.Presentation.Reviews;

public static class ReviewEndpoints
{
    public static IEndpointRouteBuilder MapReviewEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder productReviews = endpoints.MapGroup("/api/v1/products/{productId:guid}/reviews")
            .RequireRateLimiting(RateLimitingPolicyNames.PublicRead)
            .WithTags("Reviews");
        productReviews.MapGet("/", GetProductReviewsAsync)
            .WithName("Reviews.GetProductReviews")
            .WithSummary("Get approved product reviews")
            .WithDescription("Returns approved reviews and the rating summary for a product.")
            .Produces<ProductReviewsResponse>();
        productReviews.MapPost("/", CreateReviewAsync)
            .RequireAuthorization()
            .WithName("Reviews.CreateReview")
            .WithSummary("Submit a product review")
            .WithDescription("Submits a pending review for a product purchased by the authenticated customer.")
            .Produces(StatusCodes.Status201Created);

        RouteGroupBuilder adminReviews = endpoints.MapGroup("/api/v1/admin/reviews")
            .RequireAuthorization(AuthorizationPolicyNames.AdminOnly)
            .RequireRateLimiting(RateLimitingPolicyNames.Admin)
            .WithTags("Admin Reviews");
        adminReviews.MapGet("/", GetPendingReviewsAsync)
            .WithName("ReviewsAdmin.GetPendingReviews")
            .WithSummary("Get pending reviews")
            .WithDescription("Returns reviews awaiting administrator moderation.")
            .Produces<PendingReviewsResponse>();
        adminReviews.MapPost("/{id:guid}/approve", ApproveReviewAsync)
            .WithName("ReviewsAdmin.ApproveReview")
            .WithSummary("Approve a review")
            .WithDescription("Approves a pending review and invalidates its product review cache.")
            .Produces(StatusCodes.Status204NoContent);
        adminReviews.MapPost("/{id:guid}/reject", RejectReviewAsync)
            .WithName("ReviewsAdmin.RejectReview")
            .WithSummary("Reject a review")
            .WithDescription("Rejects a pending review and invalidates its product review cache.")
            .Produces(StatusCodes.Status204NoContent);
        return endpoints;
    }

    private static async Task<IResult> CreateReviewAsync(
        Guid productId,
        CreateReviewRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<Guid> result = await sender.Send(
            new CreateReviewCommand(productId, request.Rating, request.Comment), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/v1/products/{productId}/reviews", new { Id = result.Value })
            : ApiResults.Problem(result.Error!);
    }

    private static async Task<IResult> GetProductReviewsAsync(
        Guid productId,
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20) =>
        ToHttp(await sender.Send(
            new GetProductReviewsQuery(productId, page, pageSize), cancellationToken));

    private static async Task<IResult> GetPendingReviewsAsync(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20) =>
        ToHttp(await sender.Send(new GetPendingReviewsQuery(page, pageSize), cancellationToken));

    private static Task<IResult> ApproveReviewAsync(Guid id, ISender sender, CancellationToken cancellationToken) =>
        ModerateAsync(new ApproveReviewCommand(id), sender, cancellationToken);

    private static Task<IResult> RejectReviewAsync(Guid id, ISender sender, CancellationToken cancellationToken) =>
        ModerateAsync(new RejectReviewCommand(id), sender, cancellationToken);

    private static async Task<IResult> ModerateAsync(
        IRequest<Result> command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ApiResults.Problem(result.Error!);
    }

    private static IResult ToHttp<T>(Result<T> result) =>
        result.IsSuccess ? Results.Ok(result.Value) : ApiResults.Problem(result.Error!);
}
