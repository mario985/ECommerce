using ECommerce.Modules.Catalog.Domain.Categories;

namespace ECommerce.Modules.Catalog.Infrastructure.MongoDb;

internal static class CategoryDocumentMapper
{
    public static CategoryDocument ToDocument(Category category) => new()
    {
        Id = category.Id, Name = category.Name, Slug = category.Slug, Description = category.Description,
        IsActive = category.IsActive, CreatedAtUtc = category.CreatedAtUtc, UpdatedAtUtc = category.UpdatedAtUtc,
    };

    public static Category ToCategory(CategoryDocument document) => Category.Rehydrate(
        document.Id, document.Name, document.Slug, document.Description, document.IsActive,
        document.CreatedAtUtc, document.UpdatedAtUtc);
}
