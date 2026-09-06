namespace ECommerce.Modules.Reviews.Application.Caching;

public sealed class ReviewCacheOptions
{
    public const string SectionName = "ReviewsCache";
    public int ApprovedReviewsExpirationMinutes { get; init; } = 5;
    public TimeSpan ApprovedReviewsExpiration => TimeSpan.FromMinutes(ApprovedReviewsExpirationMinutes);
}
