namespace ECommerce.Modules.Reviews.Infrastructure.Persistence;

public sealed class ReviewsMongoDbOptions
{
    public const string SectionName = "ReviewsMongoDb";
    public string ConnectionString { get; init; } = "mongodb://localhost:27017";
    public string DatabaseName { get; init; } = "ECommerceReviews";
    public string ReviewsCollectionName { get; init; } = "reviews";
}
