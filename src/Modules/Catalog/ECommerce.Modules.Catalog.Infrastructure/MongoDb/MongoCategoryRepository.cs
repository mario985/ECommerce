using ECommerce.Modules.Catalog.Domain.Categories;
using MongoDB.Driver;
#pragma warning disable CA1862

namespace ECommerce.Modules.Catalog.Infrastructure.MongoDb;

public sealed class MongoCategoryRepository(IMongoCollection<CategoryDocument> categories) : ICategoryRepository
{
    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        (await categories.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken)) is { } d ? CategoryDocumentMapper.ToCategory(d) : null;

    public async Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken) =>
        (await categories.Find(x => x.Slug == slug.Trim().ToLowerInvariant()).FirstOrDefaultAsync(cancellationToken)) is { } d ? CategoryDocumentMapper.ToCategory(d) : null;

    public async Task<IReadOnlyCollection<Category>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken)
    {
        var filter = activeOnly ? Builders<CategoryDocument>.Filter.Eq(x => x.IsActive, true) : Builders<CategoryDocument>.Filter.Empty;
        return (await categories.Find(filter).SortBy(x => x.Name).ToListAsync(cancellationToken)).Select(CategoryDocumentMapper.ToCategory).ToArray();
    }

    public Task<bool> ExistsByNameAsync(string name, Guid? excludingCategoryId, CancellationToken cancellationToken) => ExistsAsync(x => x.Name == name.Trim(), excludingCategoryId, cancellationToken);
    public Task<bool> ExistsBySlugAsync(string slug, Guid? excludingCategoryId, CancellationToken cancellationToken) => ExistsAsync(x => x.Slug == slug.Trim().ToLowerInvariant(), excludingCategoryId, cancellationToken);
    private Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<CategoryDocument, bool>> match, Guid? excluding, CancellationToken cancellationToken)
    {
        var filter = Builders<CategoryDocument>.Filter.Where(match);
        if (excluding.HasValue) filter &= Builders<CategoryDocument>.Filter.Ne(x => x.Id, excluding.Value);
        return categories.Find(filter).AnyAsync(cancellationToken);
    }
    public Task AddAsync(Category category, CancellationToken cancellationToken) => categories.InsertOneAsync(CategoryDocumentMapper.ToDocument(category), cancellationToken: cancellationToken);
    public Task UpdateAsync(Category category, CancellationToken cancellationToken) => categories.ReplaceOneAsync(x => x.Id == category.Id, CategoryDocumentMapper.ToDocument(category), cancellationToken: cancellationToken);
}
#pragma warning restore CA1862
