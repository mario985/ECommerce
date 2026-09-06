using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace ECommerce.Modules.Catalog.Infrastructure.MongoDb;

public sealed class MongoDbInitializer(
    IMongoCollection<ProductDocument> productsCollection,
    IMongoCollection<CategoryDocument>? categoriesCollection = null) : IHostedService
{
    public const string UniqueSkuIndexName = "ux_products_sku";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        CreateIndexModel<ProductDocument> uniqueSkuIndex = new(
            Builders<ProductDocument>.IndexKeys.Ascending(document => document.Sku),
            new CreateIndexOptions
            {
                Name = UniqueSkuIndexName,
                Unique = true,
            });
        CreateIndexModel<ProductDocument>[] searchIndexes =
        [
            new(
                Builders<ProductDocument>.IndexKeys
                    .Ascending(document => document.Name)
                    .Ascending(document => document.Sku),
                new CreateIndexOptions { Name = "ix_products_name_sku" }),
            new(
                Builders<ProductDocument>.IndexKeys
                    .Ascending(document => document.CategoryId)
                    .Ascending(document => document.PriceAmount),
                new CreateIndexOptions { Name = "ix_products_category_price" }),
            new(
                Builders<ProductDocument>.IndexKeys.Descending(document => document.CreatedAtUtc),
                new CreateIndexOptions { Name = "ix_products_created" }),
            new(
                Builders<ProductDocument>.IndexKeys
                    .Descending(document => document.AverageRating)
                    .Descending(document => document.ReviewCount),
                new CreateIndexOptions { Name = "ix_products_rating_popularity" }),
            new(
                Builders<ProductDocument>.IndexKeys.Ascending(document => document.InStock),
                new CreateIndexOptions { Name = "ix_products_in_stock" }),
        ];

        CreateIndexModel<CategoryDocument> uniqueCategoryNameIndex = new(
            Builders<CategoryDocument>.IndexKeys.Ascending(document => document.Name),
            new CreateIndexOptions { Name = "ux_categories_name", Unique = true });
        CreateIndexModel<CategoryDocument> uniqueCategorySlugIndex = new(
            Builders<CategoryDocument>.IndexKeys.Ascending(document => document.Slug),
            new CreateIndexOptions { Name = "ux_categories_slug", Unique = true });
        Task productIndex = productsCollection.Indexes.CreateManyAsync(
            [uniqueSkuIndex, .. searchIndexes],
            cancellationToken);
        if (categoriesCollection is null)
        {
            return productIndex;
        }

        return Task.WhenAll(
            productIndex,
            categoriesCollection.Indexes.CreateManyAsync(
                [uniqueCategoryNameIndex, uniqueCategorySlugIndex], cancellationToken));
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
