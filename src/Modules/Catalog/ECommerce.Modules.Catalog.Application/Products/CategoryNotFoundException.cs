namespace ECommerce.Modules.Catalog.Application.Products;
public sealed class CategoryNotFoundException(Guid categoryId) : Exception($"Category '{categoryId}' was not found.")
{
    public Guid CategoryId { get; } = categoryId;
}
