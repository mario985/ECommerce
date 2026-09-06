namespace ECommerce.Modules.Reviews.Domain.Reviews;

public static class ReviewErrors
{
    public const string NotFoundCode = "Review.NotFound";
    public const string InvalidRatingCode = "Review.InvalidRating";
    public const string InvalidCommentCode = "Review.InvalidComment";
    public const string AlreadyApprovedCode = "Review.AlreadyApproved";
    public const string AlreadyRejectedCode = "Review.AlreadyRejected";
    public const string DuplicateReviewCode = "Review.DuplicateReview";
    public const string ProductNotFoundCode = "Review.ProductNotFound";
    public const string CustomerNotEligibleCode = "Review.CustomerNotEligible";
}
