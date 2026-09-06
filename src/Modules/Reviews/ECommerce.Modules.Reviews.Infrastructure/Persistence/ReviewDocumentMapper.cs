using ECommerce.Modules.Reviews.Domain.Reviews;

namespace ECommerce.Modules.Reviews.Infrastructure.Persistence;

internal static class ReviewDocumentMapper
{
    public static ReviewDocument ToDocument(Review review) => new()
    {
        Id = review.Id,
        ProductId = review.ProductId,
        CustomerId = review.CustomerId,
        Rating = review.Rating,
        Comment = review.Comment,
        Status = review.Status,
        CreatedAtUtc = review.CreatedAtUtc,
        UpdatedAtUtc = review.UpdatedAtUtc,
    };

    public static Review ToReview(ReviewDocument document) => Review.Rehydrate(
        document.Id,
        document.ProductId,
        document.CustomerId,
        document.Rating,
        document.Comment,
        document.Status,
        document.CreatedAtUtc,
        document.UpdatedAtUtc);
}
