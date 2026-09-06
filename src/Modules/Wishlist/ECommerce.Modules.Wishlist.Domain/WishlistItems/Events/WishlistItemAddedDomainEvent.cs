using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Wishlist.Domain.WishlistItems.Events;

public sealed record WishlistItemAddedDomainEvent(Guid WishlistItemId, Guid UserId, Guid ProductId, DateTimeOffset OccurredAtUtc) : IDomainEvent;
