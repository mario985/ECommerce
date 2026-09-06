using ECommerce.Common.Application.Caching;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Reviews.Application.Abstractions;
using ECommerce.Modules.Reviews.Application.Caching;
using ECommerce.Modules.Reviews.Domain.Reviews;
using MediatR;

namespace ECommerce.Modules.Reviews.Application.Reviews.GetProductReviews;

public sealed class GetProductReviewsQueryHandler(
    IReviewRepository repository,
    ICacheService cacheService,
    ReviewCacheOptions cacheOptions) : IRequestHandler<GetProductReviewsQuery, Result<ProductReviewsResponse>>
{
    public async Task<Result<ProductReviewsResponse>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        string versionKey = ReviewCacheKeys.ProductVersion(request.ProductId);
        string? version = await cacheService.GetAsync<string>(versionKey, cancellationToken);
        if (!Guid.TryParseExact(version, "N", out _))
        {
            version = Guid.NewGuid().ToString("N");
            await cacheService.SetAsync(versionKey, version, new CacheEntryOptions(), cancellationToken);
        }

        string cacheKey = ReviewCacheKeys.ProductReviews(request.ProductId, version, request.Page, request.PageSize);
        ProductReviewsResponse? cached = await cacheService.GetAsync<ProductReviewsResponse>(cacheKey, cancellationToken);
        if (cached is not null) return Result.Success(cached);

        (IReadOnlyCollection<Review> reviews, int totalCount, double averageRating) =
            await repository.GetApprovedByProductAsync(
                request.ProductId, request.Page, request.PageSize, cancellationToken);
        ProductReviewsResponse response = new(
            Math.Round(averageRating, 2),
            totalCount,
            reviews.Select(review => new ProductReviewResponse(
                review.Rating, review.Comment, review.CreatedAtUtc)).ToArray(),
            request.Page,
            request.PageSize,
            totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)request.PageSize));
        await cacheService.SetAsync(
            cacheKey, response, new CacheEntryOptions(cacheOptions.ApprovedReviewsExpiration), cancellationToken);
        return Result.Success(response);
    }
}
