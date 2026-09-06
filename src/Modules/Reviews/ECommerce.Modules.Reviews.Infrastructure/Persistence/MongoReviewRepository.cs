using ECommerce.Modules.Reviews.Application.Abstractions;
using ECommerce.Modules.Reviews.Domain.Reviews;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ECommerce.Modules.Reviews.Infrastructure.Persistence;

public sealed class MongoReviewRepository(IMongoCollection<ReviewDocument> reviews) : IReviewRepository
{
    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        ReviewDocument? document = await reviews.Find(item => item.Id == id).FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ReviewDocumentMapper.ToReview(document);
    }

    public Task<bool> ExistsByCustomerAndProductAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken) =>
        reviews.Find(item => item.CustomerId == customerId && item.ProductId == productId)
            .AnyAsync(cancellationToken);

    public async Task<(IReadOnlyCollection<Review> Reviews, int TotalCount, double AverageRating)> GetApprovedByProductAsync(
        Guid productId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        FilterDefinition<ReviewDocument> filter = Builders<ReviewDocument>.Filter.And(
            Builders<ReviewDocument>.Filter.Eq(item => item.ProductId, productId),
            Builders<ReviewDocument>.Filter.Eq(item => item.Status, ReviewStatus.Approved));
        int totalCount = checked((int)await reviews.CountDocumentsAsync(filter, cancellationToken: cancellationToken));
        List<ReviewDocument> documents = await reviews.Find(filter)
            .SortByDescending(item => item.CreatedAtUtc)
            .ThenByDescending(item => item.Id)
            .Skip(checked((page - 1) * pageSize))
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
        List<int> ratings = totalCount == 0
            ? []
            : await reviews.Find(filter)
                .Project(item => item.Rating)
                .ToListAsync(cancellationToken);
        double averageRating = ratings.Count == 0 ? 0 : ratings.Average();
        return (documents.Select(ReviewDocumentMapper.ToReview).ToArray(), totalCount, averageRating);
    }

    public async Task<(IReadOnlyCollection<Review> Reviews, int TotalCount)> GetPendingAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        FilterDefinition<ReviewDocument> filter = Builders<ReviewDocument>.Filter.Eq(
            item => item.Status, ReviewStatus.Pending);
        int totalCount = checked((int)await reviews.CountDocumentsAsync(filter, cancellationToken: cancellationToken));
        List<ReviewDocument> documents = await reviews.Find(filter)
            .SortBy(item => item.CreatedAtUtc)
            .ThenBy(item => item.Id)
            .Skip(checked((page - 1) * pageSize))
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
        return (documents.Select(ReviewDocumentMapper.ToReview).ToArray(), totalCount);
    }

    public async Task<(int ReviewCount, double AverageRating)> GetRatingSummaryAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        BsonDocument[] pipeline =
        [
            new("$match", new BsonDocument
            {
                [nameof(ReviewDocument.ProductId)] = new MongoDB.Bson.BsonBinaryData(
                    productId, MongoDB.Bson.GuidRepresentation.Standard),
                [nameof(ReviewDocument.Status)] = ReviewStatus.Approved.ToString(),
            }),
            new("$group", new BsonDocument
            {
                ["_id"] = BsonNull.Value,
                ["reviewCount"] = new BsonDocument("$sum", 1),
                ["averageRating"] = new BsonDocument("$avg", $"${nameof(ReviewDocument.Rating)}"),
            }),
        ];
        BsonDocument? summary = await reviews.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);
        return summary is null
            ? (0, 0)
            : (summary["reviewCount"].AsInt32, summary["averageRating"].ToDouble());
    }

    public Task AddAsync(Review review, CancellationToken cancellationToken) =>
        reviews.InsertOneAsync(ReviewDocumentMapper.ToDocument(review), cancellationToken: cancellationToken);

    public Task UpdateAsync(Review review, CancellationToken cancellationToken) =>
        reviews.ReplaceOneAsync(
            item => item.Id == review.Id,
            ReviewDocumentMapper.ToDocument(review),
            cancellationToken: cancellationToken);
}
