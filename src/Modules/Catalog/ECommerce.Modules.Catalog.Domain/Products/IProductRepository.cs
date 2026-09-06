namespace ECommerce.Modules.Catalog.Domain.Products;

public interface IProductRepository
{
    Task<bool> ExistsByCategoryIdAsync(Guid categoryId, bool activeOnly, CancellationToken cancellationToken) => Task.FromResult(false);
    Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ExistsBySkuAsync(
        Sku sku,
        Guid? excludingProductId,
        CancellationToken cancellationToken);

    Task<ProductSearchResult> SearchAsync(
        string? search,
        string? sku,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<AdvancedProductSearchResult> SearchAsync(
        ProductSearchCriteria criteria,
        CancellationToken cancellationToken) =>
        Task.FromResult(new AdvancedProductSearchResult([], 0));

    Task UpdateAvailabilityAsync(
        Guid productId,
        bool inStock,
        CancellationToken cancellationToken) => Task.CompletedTask;

    Task UpdateRatingAsync(
        Guid productId,
        double averageRating,
        int reviewCount,
        CancellationToken cancellationToken) => Task.CompletedTask;

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        Product product,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken);
}
