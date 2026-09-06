using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ECommerce.Modules.Reviews.Domain.Reviews;

namespace ECommerce.Modules.Reviews.Infrastructure.Persistence;

public sealed class ReviewDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; init; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid ProductId { get; init; }
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid CustomerId { get; init; }
    public int Rating { get; init; }
    public string Comment { get; init; } = string.Empty;
    [BsonRepresentation(BsonType.String)]
    public ReviewStatus Status { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    public DateTimeOffset? UpdatedAtUtc { get; init; }
}
