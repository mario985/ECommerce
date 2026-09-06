using ECommerce.Common.Application.Errors;
using MediatR;

namespace ECommerce.Modules.Reviews.Application.Reviews.GetProductReviews;

public sealed record GetProductReviewsQuery(Guid ProductId, int Page = 1, int PageSize = 20)
    : IRequest<Result<ProductReviewsResponse>>;

public sealed record ProductReviewResponse(int Rating, string Comment, DateTimeOffset CreatedAtUtc);

public sealed record ProductReviewsResponse(
    double AverageRating,
    int TotalReviews,
    IReadOnlyCollection<ProductReviewResponse> Items,
    int Page,
    int PageSize,
    int TotalPages);
