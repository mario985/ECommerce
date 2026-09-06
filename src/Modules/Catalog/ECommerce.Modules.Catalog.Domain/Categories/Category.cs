using ECommerce.Common.Domain.Entities;
using ECommerce.Modules.Catalog.Domain.Categories.Events;

namespace ECommerce.Modules.Catalog.Domain.Categories;

public sealed class Category : AuditableAggregateRoot<Guid>
{
    private Category(Guid id, string name, string slug, string? description, bool isActive, DateTimeOffset createdAtUtc)
        : base(id)
    {
        Name = name;
        Slug = slug;
        Description = description;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
    }

    public string Name { get; private set; }
    public string Slug { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public static Category Create(string name, string slug, string? description, DateTimeOffset createdAtUtc)
    {
        Category category = new(Guid.NewGuid(), name.Trim(), slug.Trim().ToLowerInvariant(), description?.Trim(), true, createdAtUtc);
        category.RaiseDomainEvent(new CategoryCreatedDomainEvent(category.Id, createdAtUtc));
        return category;
    }

    public static Category Rehydrate(Guid id, string name, string slug, string? description, bool isActive, DateTimeOffset createdAtUtc, DateTimeOffset? updatedAtUtc)
    {
        Category category = new(id, name, slug, description, isActive, createdAtUtc) { UpdatedAtUtc = updatedAtUtc };
        return category;
    }

    public void Update(string name, string slug, string? description, DateTimeOffset updatedAtUtc)
    {
        Name = name.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        Description = description?.Trim();
        UpdatedAtUtc = updatedAtUtc;
        RaiseDomainEvent(new CategoryUpdatedDomainEvent(Id, updatedAtUtc));
    }

    public bool Activate(DateTimeOffset occurredAtUtc)
    {
        if (IsActive) return false;
        IsActive = true;
        UpdatedAtUtc = occurredAtUtc;
        RaiseDomainEvent(new CategoryActivatedDomainEvent(Id, occurredAtUtc));
        return true;
    }

    public bool Deactivate(DateTimeOffset occurredAtUtc)
    {
        if (!IsActive) return false;
        IsActive = false;
        UpdatedAtUtc = occurredAtUtc;
        RaiseDomainEvent(new CategoryDeactivatedDomainEvent(Id, occurredAtUtc));
        return true;
    }
}
