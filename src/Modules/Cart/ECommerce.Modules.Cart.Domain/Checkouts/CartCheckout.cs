using ECommerce.Common.Domain.Entities;
using ECommerce.Modules.Cart.Domain.Checkouts.Events;

namespace ECommerce.Modules.Cart.Domain.Checkouts;

public sealed class CartCheckout : AuditableAggregateRoot<Guid>
{
    private readonly List<CartCheckoutItem> _items = [];

    private CartCheckout()
    {
    }

    private CartCheckout(Guid checkoutId, Guid cartId, Guid customerId, DateTimeOffset createdAtUtc)
        : base(Guid.NewGuid())
    {
        CheckoutId = checkoutId;
        CartId = cartId;
        CustomerId = customerId;
        Status = CartCheckoutStatus.Pending;
        CreatedAtUtc = createdAtUtc;
        CreatedBy = customerId.ToString();
    }

    public Guid CheckoutId { get; private set; }
    public Guid CartId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid? OrderId { get; private set; }
    public CartCheckoutStatus Status { get; private set; }
    public IReadOnlyCollection<CartCheckoutItem> Items => _items.AsReadOnly();
    public DateTimeOffset? CompletedAtUtc { get; private set; }
    public DateTimeOffset? FailedAtUtc { get; private set; }

    public static CartCheckout Create(
        Guid checkoutId,
        Guid cartId,
        Guid customerId,
        IReadOnlyCollection<CartCheckoutItemSnapshot> items,
        DateTimeOffset createdAtUtc)
    {
        if (checkoutId == Guid.Empty) throw new ArgumentException("A checkout ID is required.", nameof(checkoutId));
        if (cartId == Guid.Empty) throw new ArgumentException("A cart ID is required.", nameof(cartId));
        if (customerId == Guid.Empty) throw new ArgumentException("A customer ID is required.", nameof(customerId));
        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0) throw new ArgumentException("A checkout requires at least one item.", nameof(items));

        CartCheckout checkout = new(checkoutId, cartId, customerId, createdAtUtc);
        foreach (CartCheckoutItemSnapshot item in items)
        {
            checkout._items.Add(new CartCheckoutItem(
                item.ProductId, item.ProductName, item.Sku, item.UnitPrice,
                item.Currency, item.Quantity));
        }

        checkout.RaiseDomainEvent(new CartCheckoutCreatedDomainEvent(checkoutId, cartId, customerId));
        return checkout;
    }

    public CartCheckoutTransitionOutcome MarkCompleted(Guid orderId, DateTimeOffset completedAtUtc)
    {
        if (orderId == Guid.Empty) throw new ArgumentException("An order ID is required.", nameof(orderId));
        if (Status == CartCheckoutStatus.Completed && OrderId == orderId)
            return CartCheckoutTransitionOutcome.AlreadyApplied;
        if (Status != CartCheckoutStatus.Pending)
            return CartCheckoutTransitionOutcome.InvalidState;

        OrderId = orderId;
        Status = CartCheckoutStatus.Completed;
        CompletedAtUtc = completedAtUtc;
        UpdatedAtUtc = completedAtUtc;
        UpdatedBy = CustomerId.ToString();
        RaiseDomainEvent(new CartCheckoutPaidDomainEvent(CheckoutId, CartId, CustomerId, orderId));
        return CartCheckoutTransitionOutcome.Applied;
    }

    public CartCheckoutTransitionOutcome MarkFailed(Guid orderId, DateTimeOffset failedAtUtc)
    {
        if (orderId == Guid.Empty) throw new ArgumentException("An order ID is required.", nameof(orderId));
        if (Status == CartCheckoutStatus.Failed && OrderId == orderId)
            return CartCheckoutTransitionOutcome.AlreadyApplied;
        if (Status != CartCheckoutStatus.Pending)
            return CartCheckoutTransitionOutcome.InvalidState;

        OrderId = orderId;
        Status = CartCheckoutStatus.Failed;
        FailedAtUtc = failedAtUtc;
        UpdatedAtUtc = failedAtUtc;
        UpdatedBy = CustomerId.ToString();
        RaiseDomainEvent(new CartCheckoutFailedDomainEvent(CheckoutId, CartId, CustomerId, orderId));
        return CartCheckoutTransitionOutcome.Applied;
    }
}
