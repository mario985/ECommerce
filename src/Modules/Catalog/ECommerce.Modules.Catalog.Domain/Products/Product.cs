using ECommerce.Common.Domain.Entities;

namespace ECommerce.Modules.Catalog.Domain.Products;

public sealed class Product : AuditableAggregateRoot<Guid>
{
    private Product(
        Guid id,
        string name,
        string? description,
        Sku sku,
        Money price,
        Guid? categoryId,
        bool isActive,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc,
        string? createdBy,
        string? updatedBy)
        : base(id)
    {
        Name = name;
        Description = description;
        Sku = sku;
        Price = price;
        CategoryId = categoryId;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        CreatedBy = createdBy;
        UpdatedBy = updatedBy;
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public Sku Sku { get; private set; }

    public Money Price { get; private set; }

    public Guid? CategoryId { get; private set; }

    public bool IsActive { get; private set; }

    public static Product Create(
        string name,
        string? description,
        Sku sku,
        Money price,
        Guid? categoryId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(sku);
        ArgumentNullException.ThrowIfNull(price);

        Product product = new(
            Guid.NewGuid(),
            name.Trim(),
            description?.Trim(),
            sku,
            price,
            categoryId,
            isActive: true,
            createdAtUtc: default,
            updatedAtUtc: null,
            createdBy: null,
            updatedBy: null);

        product.RaiseDomainEvent(new ProductCreatedDomainEvent(product.Id));

        return product;
    }

    public static Product Rehydrate(
        Guid id,
        string name,
        string? description,
        Sku sku,
        Money price,
        bool isActive,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc,
        string? createdBy,
        string? updatedBy,
        Guid? categoryId = null)
    {
        return new Product(
            id,
            name,
            description,
            sku,
            price,
            categoryId,
            isActive,
            createdAtUtc,
            updatedAtUtc,
            createdBy,
            updatedBy);
    }

    public void Update(
        string name,
        string? description,
        Sku sku,
        Money price,
        DateTimeOffset updatedAtUtc,
        Guid? categoryId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(sku);
        ArgumentNullException.ThrowIfNull(price);

        Name = name.Trim();
        Description = description?.Trim();
        Sku = sku;
        Price = price;
        CategoryId = categoryId;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate(DateTimeOffset updatedAtUtc)
    {
        IsActive = false;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Activate(DateTimeOffset updatedAtUtc)
    {
        IsActive = true;
        UpdatedAtUtc = updatedAtUtc;
    }
}
