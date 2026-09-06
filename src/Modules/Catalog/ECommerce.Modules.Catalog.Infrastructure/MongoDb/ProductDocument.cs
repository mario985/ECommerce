using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ECommerce.Modules.Catalog.Infrastructure.MongoDb;

public sealed class ProductDocument
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid? CategoryId { get; init; }

    public string Sku { get; init; } = string.Empty;

    public decimal PriceAmount { get; init; }

    public string PriceCurrency { get; init; } = string.Empty;

    public bool IsActive { get; init; }

    public bool InStock { get; init; }

    public double AverageRating { get; init; }

    public int ReviewCount { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset? UpdatedAtUtc { get; init; }

    public string? CreatedBy { get; init; }

    public string? UpdatedBy { get; init; }
}
