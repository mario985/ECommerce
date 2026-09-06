using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Catalog.Contracts.Products;
using ECommerce.Modules.Ordering.Contracts.Reviews;
using ECommerce.Modules.Reviews.Application.Abstractions;
using ECommerce.Modules.Reviews.Domain.Reviews;
using MediatR;

namespace ECommerce.Modules.Reviews.Application.Reviews.CreateReview;

public sealed class CreateReviewCommandHandler(
    IReviewRepository reviewRepository,
    IProductCatalogReader productCatalogReader,
    IOrderReviewEligibilityReader orderReviewEligibilityReader,
    ICurrentUser currentUser,
    TimeProvider timeProvider) : IRequestHandler<CreateReviewCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure<Guid>(ReviewApplicationErrors.Unauthorized);
        if (request.Rating is < 1 or > 5)
            return Result.Failure<Guid>(ReviewApplicationErrors.InvalidRating);
        if (string.IsNullOrWhiteSpace(request.Comment) || request.Comment.Trim().Length > Review.MaximumCommentLength)
            return Result.Failure<Guid>(ReviewApplicationErrors.InvalidComment);

        ProductCatalogResponse? product = await productCatalogReader.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null) return Result.Failure<Guid>(ReviewApplicationErrors.ProductNotFound);

        Guid customerId = currentUser.UserId.Value;
        bool eligible = await orderReviewEligibilityReader.HasPurchasedProductAsync(
            customerId, request.ProductId, cancellationToken);
        if (!eligible) return Result.Failure<Guid>(ReviewApplicationErrors.CustomerNotEligible);
        if (await reviewRepository.ExistsByCustomerAndProductAsync(customerId, request.ProductId, cancellationToken))
            return Result.Failure<Guid>(ReviewApplicationErrors.DuplicateReview);

        Review review = Review.Create(
            request.ProductId, customerId, request.Rating, request.Comment, timeProvider.GetUtcNow());
        await reviewRepository.AddAsync(review, cancellationToken);
        return Result.Success(review.Id);
    }
}
