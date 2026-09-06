using ECommerce.Common.Application.Errors;
using ECommerce.Common.Application.Messaging;
using ECommerce.Modules.Reviews.Application.Abstractions;
using ECommerce.Modules.Reviews.Application.Caching;
using ECommerce.Modules.Reviews.Contracts.IntegrationEvents;
using ECommerce.Modules.Reviews.Domain.Reviews;
using MediatR;

namespace ECommerce.Modules.Reviews.Application.Reviews.RejectReview;

public sealed class RejectReviewCommandHandler(
    IReviewRepository repository,
    IReviewCacheInvalidator cacheInvalidator,
    IIntegrationEventPublisher integrationEventPublisher,
    TimeProvider timeProvider) : IRequestHandler<RejectReviewCommand, Result>
{
    public async Task<Result> Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        Review? review = await repository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review is null) return Result.Failure(ReviewApplicationErrors.NotFound);
        ReviewModerationOutcome outcome = review.Reject(timeProvider.GetUtcNow());
        if (outcome == ReviewModerationOutcome.InvalidState)
            return Result.Failure(ReviewApplicationErrors.AlreadyApproved);
        if (outcome == ReviewModerationOutcome.AlreadyApplied) return Result.Success();
        await repository.UpdateAsync(review, cancellationToken);
        await cacheInvalidator.InvalidateProductAsync(review.ProductId, cancellationToken);
        (int reviewCount, double averageRating) = await repository.GetRatingSummaryAsync(
            review.ProductId, cancellationToken);
        await integrationEventPublisher.PublishAsync(
            new ProductRatingChangedIntegrationEvent(
                Guid.NewGuid(), review.ProductId, averageRating, reviewCount,
                timeProvider.GetUtcNow()),
            cancellationToken);
        return Result.Success();
    }
}
