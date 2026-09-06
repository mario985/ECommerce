namespace ECommerce.Modules.Catalog.Domain.Categories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Category?> GetBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Category>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, Guid? excludingCategoryId, CancellationToken cancellationToken);
    Task<bool> ExistsBySlugAsync(string slug, Guid? excludingCategoryId, CancellationToken cancellationToken);
    Task AddAsync(Category category, CancellationToken cancellationToken);
    Task UpdateAsync(Category category, CancellationToken cancellationToken);
}
