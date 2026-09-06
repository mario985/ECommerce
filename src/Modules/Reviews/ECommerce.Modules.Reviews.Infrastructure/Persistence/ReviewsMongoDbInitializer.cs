using ECommerce.Modules.Reviews.Domain.Reviews;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace ECommerce.Modules.Reviews.Infrastructure.Persistence;

internal sealed class ReviewsMongoDbInitializer(IMongoCollection<ReviewDocument> reviews) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        CreateIndexModel<ReviewDocument>[] indexes =
        [
            new(
                Builders<ReviewDocument>.IndexKeys
                    .Ascending(item => item.CustomerId)
                    .Ascending(item => item.ProductId),
                new CreateIndexOptions { Name = "ux_reviews_customer_product", Unique = true }),
            new(
                Builders<ReviewDocument>.IndexKeys
                    .Ascending(item => item.ProductId)
                    .Ascending(item => item.Status)
                    .Descending(item => item.CreatedAtUtc),
                new CreateIndexOptions { Name = "ix_reviews_product_status_created" }),
            new(
                Builders<ReviewDocument>.IndexKeys.Ascending(item => item.Status),
                new CreateIndexOptions<ReviewDocument>
                {
                    Name = "ix_reviews_pending",
                    PartialFilterExpression = Builders<ReviewDocument>.Filter.Eq(
                        item => item.Status, ReviewStatus.Pending),
                }),
        ];
        return reviews.Indexes.CreateManyAsync(indexes, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
