using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Reviews.Domain.Reviews;

namespace ECommerce.Modules.Reviews.Application.Reviews;

public static class ReviewApplicationErrors
{
    public static readonly Error NotFound = new(ReviewErrors.NotFoundCode, "The requested review was not found.", ErrorType.NotFound);
    public static readonly Error InvalidRating = new(ReviewErrors.InvalidRatingCode, "Rating must be between 1 and 5.", ErrorType.Validation);
    public static readonly Error InvalidComment = new(ReviewErrors.InvalidCommentCode, "A review comment between 1 and 2000 characters is required.", ErrorType.Validation);
    public static readonly Error AlreadyApproved = new(ReviewErrors.AlreadyApprovedCode, "An approved review cannot be rejected.", ErrorType.Conflict);
    public static readonly Error AlreadyRejected = new(ReviewErrors.AlreadyRejectedCode, "A rejected review cannot be approved.", ErrorType.Conflict);
    public static readonly Error DuplicateReview = new(ReviewErrors.DuplicateReviewCode, "The customer has already reviewed this product.", ErrorType.Conflict);
    public static readonly Error ProductNotFound = new(ReviewErrors.ProductNotFoundCode, "The requested product was not found.", ErrorType.NotFound);
    public static readonly Error CustomerNotEligible = new(ReviewErrors.CustomerNotEligibleCode, "The customer has not purchased this product in a paid order.", ErrorType.Conflict);
    public static readonly Error Unauthorized = new("Review.Unauthorized", "Authentication is required to create a review.", ErrorType.Unauthorized);
}
