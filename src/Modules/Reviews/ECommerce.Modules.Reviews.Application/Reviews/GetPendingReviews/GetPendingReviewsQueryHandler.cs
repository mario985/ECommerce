using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Reviews.Application.Abstractions;
using ECommerce.Modules.Reviews.Domain.Reviews;
using MediatR;

namespace ECommerce.Modules.Reviews.Application.Reviews.GetPendingReviews;

public sealed class GetPendingReviewsQueryHandler(IReviewRepository repository)
    : IRequestHandler<GetPendingReviewsQuery, Result<PendingReviewsResponse>>
{
    public async Task<Result<PendingReviewsResponse>> Handle(GetPendingReviewsQuery request, CancellationToken cancellationToken)
    {
        (IReadOnlyCollection<Review> reviews, int totalCount) = await repository.GetPendingAsync(
            request.Page, request.PageSize, cancellationToken);
        return Result.Success(new PendingReviewsResponse(
            reviews.Select(review => new PendingReviewResponse(
                review.Id, review.ProductId, review.Rating, review.Comment, review.CreatedAtUtc)).ToArray(),
            request.Page,
            request.PageSize,
            totalCount,
            totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize)));
    }
}
