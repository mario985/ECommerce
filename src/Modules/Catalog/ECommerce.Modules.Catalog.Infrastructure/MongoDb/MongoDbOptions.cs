namespace ECommerce.Modules.Catalog.Infrastructure.MongoDb;

public sealed class MongoDbOptions
{
    public const string SectionName = "MongoDb";

    public string ConnectionString { get; init; } = string.Empty;

    public string DatabaseName { get; init; } = string.Empty;

    public string ProductsCollectionName { get; init; } = string.Empty;

    public string CategoriesCollectionName { get; init; } = "categories";
}
