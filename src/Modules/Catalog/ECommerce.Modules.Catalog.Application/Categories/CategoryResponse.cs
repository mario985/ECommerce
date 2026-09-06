using ECommerce.Modules.Catalog.Domain.Categories;

namespace ECommerce.Modules.Catalog.Application.Categories;

public record CategoryResponse(Guid Id, string Name, string Slug, string? Description, bool IsActive)
{
    public static CategoryResponse From(Category c) => new(c.Id, c.Name, c.Slug, c.Description, c.IsActive);
}
