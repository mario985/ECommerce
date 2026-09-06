using ECommerce.Modules.Catalog.Domain.Products;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace ECommerce.Modules.Catalog.Infrastructure.MongoDb;

public sealed class MongoProductRepository(
    IMongoCollection<ProductDocument> productsCollection,
    IMongoCollection<CategoryDocument>? categoriesCollection = null) : IProductRepository
{
    public Task<bool> ExistsByCategoryIdAsync(Guid categoryId, bool activeOnly, CancellationToken cancellationToken)
    {
        FilterDefinition<ProductDocument> filter = Builders<ProductDocument>.Filter.Eq(x => x.CategoryId, categoryId);
        if (activeOnly) filter &= Builders<ProductDocument>.Filter.Eq(x => x.IsActive, true);
        return productsCollection.Find(filter).AnyAsync(cancellationToken);
    }
    public async Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        ProductDocument? document = await productsCollection
            .Find(storedDocument => storedDocument.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document is null ? null : ProductDocumentMapper.ToProduct(document);
    }

    public Task<bool> ExistsBySkuAsync(
        Sku sku,
        Guid? excludingProductId,
        CancellationToken cancellationToken)
    {
        FilterDefinition<ProductDocument> filter =
            Builders<ProductDocument>.Filter.Eq(document => document.Sku, sku.Value);

        if (excludingProductId.HasValue)
        {
            filter &= Builders<ProductDocument>.Filter.Ne(
                document => document.Id,
                excludingProductId.Value);
        }

        return productsCollection
            .Find(filter)
            .AnyAsync(cancellationToken);
    }

    public async Task<ProductSearchResult> SearchAsync(
        string? search,
        string? sku,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        FilterDefinitionBuilder<ProductDocument> filters =
            Builders<ProductDocument>.Filter;
        List<FilterDefinition<ProductDocument>> filterParts = [];

        if (!string.IsNullOrWhiteSpace(search))
        {
            BsonRegularExpression expression = new(
                Regex.Escape(search.Trim()),
                "i");
            filterParts.Add(filters.Or(
                filters.Regex(document => document.Name, expression),
                filters.Regex(document => document.Description, expression)));
        }

        if (!string.IsNullOrWhiteSpace(sku))
        {
            filterParts.Add(filters.Eq(document => document.Sku, sku));
        }

        if (isActive.HasValue)
        {
            filterParts.Add(filters.Eq(
                document => document.IsActive,
                isActive.Value));
        }

        FilterDefinition<ProductDocument> filter = filterParts.Count == 0
            ? filters.Empty
            : filters.And(filterParts);
        long requestedSkip = (long)(page - 1) * pageSize;
        int skip = requestedSkip > int.MaxValue ? int.MaxValue : (int)requestedSkip;

        long totalCount = await productsCollection.CountDocumentsAsync(
            filter,
            cancellationToken: cancellationToken);
        List<ProductDocument> documents = await productsCollection
            .Find(filter)
            .SortBy(document => document.Name)
            .ThenBy(document => document.Id)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        Product[] products = documents
            .Select(ProductDocumentMapper.ToProduct)
            .ToArray();

        return new ProductSearchResult(products, totalCount);
    }

    public async Task<AdvancedProductSearchResult> SearchAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        List<BsonDocument> stages = [];
        if (categoriesCollection is not null)
        {
            stages.Add(new BsonDocument("$lookup", new BsonDocument
            {
                ["from"] = categoriesCollection.CollectionNamespace.CollectionName,
                ["localField"] = nameof(ProductDocument.CategoryId),
                ["foreignField"] = "_id",
                ["as"] = "category",
            }));
            stages.Add(new BsonDocument("$set", new BsonDocument(
                "CategoryName",
                new BsonDocument("$arrayElemAt", new BsonArray { "$category.Name", 0 }))));
        }

        BsonDocument match = BuildAdvancedMatch(criteria, categoriesCollection is not null);
        if (match.ElementCount > 0)
        {
            stages.Add(new BsonDocument("$match", match));
        }

        List<BsonDocument> countStages = [.. stages, new("$count", "count")];
        BsonDocument? countDocument = await productsCollection
            .Aggregate<BsonDocument>(countStages, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);
        long totalCount = countDocument?.GetValue("count", 0).ToInt64() ?? 0;

        long requestedSkip = (long)(criteria.Page - 1) * criteria.PageSize;
        int skip = requestedSkip > int.MaxValue ? int.MaxValue : (int)requestedSkip;
        List<BsonDocument> pageStages =
        [
            .. stages,
            new("$sort", BuildSort(criteria.Sort)),
            new("$skip", skip),
            new("$limit", criteria.PageSize),
        ];
        List<BsonDocument> documents = await productsCollection
            .Aggregate<BsonDocument>(pageStages, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);

        ProductSearchItem[] items = documents.Select(ToSearchItem).ToArray();
        return new AdvancedProductSearchResult(items, totalCount);
    }

    public Task UpdateAvailabilityAsync(
        Guid productId,
        bool inStock,
        CancellationToken cancellationToken) =>
        productsCollection.UpdateOneAsync(
            document => document.Id == productId,
            Builders<ProductDocument>.Update.Set(document => document.InStock, inStock),
            cancellationToken: cancellationToken);

    public Task UpdateRatingAsync(
        Guid productId,
        double averageRating,
        int reviewCount,
        CancellationToken cancellationToken) =>
        productsCollection.UpdateOneAsync(
            document => document.Id == productId,
            Builders<ProductDocument>.Update
                .Set(document => document.AverageRating, averageRating)
                .Set(document => document.ReviewCount, reviewCount),
            cancellationToken: cancellationToken);

    public Task AddAsync(
        Product product,
        CancellationToken cancellationToken)
    {
        ProductDocument document = ProductDocumentMapper.ToDocument(product);

        return productsCollection.InsertOneAsync(
            document,
            cancellationToken: cancellationToken);
    }

    public Task UpdateAsync(
        Product product,
        CancellationToken cancellationToken)
    {
        UpdateDefinition<ProductDocument> update = Builders<ProductDocument>.Update
            .Set(document => document.Name, product.Name)
            .Set(document => document.Description, product.Description)
            .Set(document => document.CategoryId, product.CategoryId)
            .Set(document => document.Sku, product.Sku.Value)
            .Set(document => document.PriceAmount, product.Price.Amount)
            .Set(document => document.PriceCurrency, product.Price.Currency)
            .Set(document => document.IsActive, product.IsActive)
            .Set(document => document.CreatedAtUtc, product.CreatedAtUtc)
            .Set(document => document.UpdatedAtUtc, product.UpdatedAtUtc)
            .Set(document => document.CreatedBy, product.CreatedBy)
            .Set(document => document.UpdatedBy, product.UpdatedBy);

        return productsCollection.UpdateOneAsync(
            storedDocument => storedDocument.Id == product.Id,
            update,
            cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return productsCollection.DeleteOneAsync(
            storedDocument => storedDocument.Id == id,
            cancellationToken);
    }

    private static BsonDocument BuildAdvancedMatch(
        ProductSearchCriteria criteria,
        bool categoryLookupAvailable)
    {
        BsonDocument match = [];
        if (criteria.IsActive.HasValue)
        {
            match[nameof(ProductDocument.IsActive)] = criteria.IsActive.Value;
        }
        if (criteria.CategoryId.HasValue)
        {
            match[nameof(ProductDocument.CategoryId)] = new BsonBinaryData(
                criteria.CategoryId.Value,
                GuidRepresentation.Standard);
        }
        if (criteria.MinPrice.HasValue || criteria.MaxPrice.HasValue)
        {
            BsonDocument price = [];
            if (criteria.MinPrice.HasValue) price["$gte"] = criteria.MinPrice.Value;
            if (criteria.MaxPrice.HasValue) price["$lte"] = criteria.MaxPrice.Value;
            match[nameof(ProductDocument.PriceAmount)] = price;
        }
        if (criteria.InStock.HasValue)
        {
            match[nameof(ProductDocument.InStock)] = criteria.InStock.Value;
        }
        if (criteria.MinimumRating.HasValue)
        {
            match[nameof(ProductDocument.AverageRating)] = new BsonDocument(
                "$gte", criteria.MinimumRating.Value);
        }
        if (!string.IsNullOrWhiteSpace(criteria.Sku))
        {
            match[nameof(ProductDocument.Sku)] = criteria.Sku;
        }
        if (!string.IsNullOrWhiteSpace(criteria.Query))
        {
            BsonRegularExpression regex = new(Regex.Escape(criteria.Query.Trim()), "i");
            BsonArray alternatives =
            [
                new BsonDocument(nameof(ProductDocument.Name), regex),
                new BsonDocument(nameof(ProductDocument.Description), regex),
            ];
            if (criteria.IncludeSkuAndCategoryInTextSearch)
            {
                alternatives.Add(new BsonDocument(nameof(ProductDocument.Sku), regex));
            }
            if (categoryLookupAvailable && criteria.IncludeSkuAndCategoryInTextSearch)
            {
                alternatives.Add(new BsonDocument("CategoryName", regex));
            }
            match["$or"] = alternatives;
        }
        return match;
    }

    private static BsonDocument BuildSort(string sort) => sort.ToLowerInvariant() switch
    {
        "price-asc" => new BsonDocument
        {
            [nameof(ProductDocument.PriceAmount)] = 1,
            ["_id"] = 1,
        },
        "price-desc" => new BsonDocument
        {
            [nameof(ProductDocument.PriceAmount)] = -1,
            ["_id"] = 1,
        },
        "rating-desc" => new BsonDocument
        {
            [nameof(ProductDocument.AverageRating)] = -1,
            [nameof(ProductDocument.ReviewCount)] = -1,
            ["_id"] = 1,
        },
        "popularity" => new BsonDocument
        {
            [nameof(ProductDocument.ReviewCount)] = -1,
            [nameof(ProductDocument.AverageRating)] = -1,
            ["_id"] = 1,
        },
        "name" => new BsonDocument
        {
            [nameof(ProductDocument.Name)] = 1,
            ["_id"] = 1,
        },
        _ => new BsonDocument
        {
            [nameof(ProductDocument.CreatedAtUtc)] = -1,
            ["_id"] = -1,
        },
    };

    private static ProductSearchItem ToSearchItem(BsonDocument document)
    {
        string? categoryName = document.TryGetValue("CategoryName", out BsonValue? value) &&
                               !value.IsBsonNull
            ? value.AsString
            : null;
        document.Remove("category");
        document.Remove("CategoryName");
        ProductDocument product = MongoDB.Bson.Serialization.BsonSerializer
            .Deserialize<ProductDocument>(document);
        return new ProductSearchItem(
            product.Id,
            product.Name,
            product.Description,
            product.Sku,
            product.PriceAmount,
            product.PriceCurrency,
            categoryName,
            Math.Round(product.AverageRating, 2),
            product.ReviewCount,
            product.InStock,
            product.IsActive,
            product.CreatedAtUtc);
    }
}
