namespace ECommerce.Modules.Catalog.Presentation.Categories;
public sealed record CreateCategoryRequest(string Name, string Slug, string? Description);
public sealed record UpdateCategoryRequest(string Name, string Slug, string? Description);
