using ECommerce.Common.Domain.Entities;
using ECommerce.Modules.Wishlist.Domain.WishlistItems.Events;

namespace ECommerce.Modules.Wishlist.Domain.WishlistItems;

public sealed class WishlistItem : AggregateRoot<Guid>
{
    private WishlistItem() { }

    private WishlistItem(Guid id, Guid userId, Guid productId, DateTimeOffset createdAtUtc)
        : base(id)
    {
        UserId = userId;
        ProductId = productId;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid UserId { get; private set; }
    public Guid ProductId { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static WishlistItem Create(Guid userId, Guid productId, DateTimeOffset createdAtUtc)
    {
        if (userId == Guid.Empty) throw new ArgumentException("A user ID is required.", nameof(userId));
        if (productId == Guid.Empty) throw new ArgumentException("A product ID is required.", nameof(productId));
        WishlistItem item = new(Guid.NewGuid(), userId, productId, createdAtUtc);
        item.RaiseDomainEvent(new WishlistItemAddedDomainEvent(item.Id, userId, productId, createdAtUtc));
        return item;
    }

    public void Remove(DateTimeOffset removedAtUtc) =>
        RaiseDomainEvent(new WishlistItemRemovedDomainEvent(Id, UserId, ProductId, removedAtUtc));
}
