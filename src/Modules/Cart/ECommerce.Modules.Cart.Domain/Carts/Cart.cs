using ECommerce.Common.Domain.Entities;
using ECommerce.Modules.Cart.Domain.Carts.Events;
using ECommerce.Modules.Cart.Domain.Checkouts;

namespace ECommerce.Modules.Cart.Domain.Carts;

public sealed class Cart : AuditableAggregateRoot<Guid>
{
    private readonly List<CartItem> _items = [];

    private Cart()
    {
    }

    private Cart(Guid id, Guid customerId, DateTimeOffset createdAtUtc)
        : base(id)
    {
        CustomerId = customerId;
        CreatedAtUtc = createdAtUtc;
        CreatedBy = customerId.ToString();
    }

    public Guid CustomerId { get; private set; }
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    public decimal Total => _items.Sum(item => item.LineTotal);
    public string? Currency => _items.Count == 0 ? null : _items[0].Currency;

    public static Cart Create(Guid customerId, DateTimeOffset createdAtUtc)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("A customer ID is required.", nameof(customerId));
        }

        Cart cart = new(Guid.NewGuid(), customerId, createdAtUtc);
        cart.RaiseDomainEvent(new CartCreatedDomainEvent(cart.Id, customerId));
        return cart;
    }

    public CartItem? FindItem(Guid productId) =>
        _items.SingleOrDefault(item => item.ProductId == productId);

    public void AddItem(
        Guid productId,
        string productName,
        string sku,
        decimal unitPrice,
        string currency,
        int quantity,
        DateTimeOffset changedAtUtc)
    {
        CartItem? existingItem = FindItem(productId);
        if (existingItem is not null)
        {
            EnsureCurrencyMatches(currency);
            existingItem.IncreaseQuantity(quantity);
            MarkUpdated(changedAtUtc);
            RaiseDomainEvent(new CartItemQuantityChangedDomainEvent(
                Id, CustomerId, productId, existingItem.Quantity));
            return;
        }

        EnsureCurrencyMatches(currency);
        CartItem item = new(
            Guid.NewGuid(), productId, productName, sku, unitPrice, currency, quantity);
        _items.Add(item);
        MarkUpdated(changedAtUtc);
        RaiseDomainEvent(new CartItemAddedDomainEvent(
            Id, CustomerId, productId, quantity));
    }

    public void UpdateItemQuantity(
        Guid productId,
        int quantity,
        DateTimeOffset changedAtUtc)
    {
        CartItem item = FindItem(productId)
            ?? throw new InvalidOperationException("The cart item does not exist.");

        if (!item.UpdateQuantity(quantity))
        {
            return;
        }

        MarkUpdated(changedAtUtc);
        RaiseDomainEvent(new CartItemQuantityChangedDomainEvent(
            Id, CustomerId, productId, quantity));
    }

    public bool RemoveItem(Guid productId, DateTimeOffset changedAtUtc)
    {
        CartItem? item = FindItem(productId);
        if (item is null)
        {
            return false;
        }

        _items.Remove(item);
        MarkUpdated(changedAtUtc);
        RaiseDomainEvent(new CartItemRemovedDomainEvent(Id, CustomerId, productId));
        return true;
    }

    public bool Clear(DateTimeOffset changedAtUtc)
    {
        if (_items.Count == 0)
        {
            return false;
        }

        _items.Clear();
        MarkUpdated(changedAtUtc);
        RaiseDomainEvent(new CartClearedDomainEvent(Id, CustomerId));
        return true;
    }

    public bool RemovePurchasedQuantity(
        Guid productId,
        int quantity,
        Guid checkoutId,
        DateTimeOffset changedAtUtc)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        CartItem? item = FindItem(productId);
        if (item is null)
        {
            return false;
        }

        int removedQuantity = Math.Min(item.Quantity, quantity);
        if (item.Quantity <= quantity)
        {
            _items.Remove(item);
        }
        else
        {
            item.ReduceQuantity(quantity);
        }

        MarkUpdated(changedAtUtc);
        RaiseDomainEvent(new CartItemPurchasedQuantityRemovedDomainEvent(
            Id, CustomerId, checkoutId, productId, removedQuantity));
        return true;
    }

    public void ReconcileCompletedCheckout(
        Guid checkoutId,
        IReadOnlyCollection<CartCheckoutItemSnapshot> checkedOutItems,
        DateTimeOffset changedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(checkedOutItems);
        foreach (CartCheckoutItemSnapshot item in checkedOutItems)
        {
            RemovePurchasedQuantity(item.ProductId, item.Quantity, checkoutId, changedAtUtc);
        }

        RaiseDomainEvent(new CartCheckoutCompletedDomainEvent(Id, CustomerId, checkoutId));
    }

    public void RefreshItemSnapshot(
        Guid productId,
        string productName,
        string sku,
        decimal unitPrice,
        string currency,
        DateTimeOffset changedAtUtc)
    {
        CartItem item = FindItem(productId)
            ?? throw new InvalidOperationException("The cart item does not exist.");

        EnsureCurrencyMatches(currency);
        if (item.RefreshSnapshot(productName, sku, unitPrice, currency))
        {
            MarkUpdated(changedAtUtc);
        }
    }

    public bool RequestCheckout(Guid checkoutId)
    {
        if (checkoutId == Guid.Empty)
        {
            throw new ArgumentException("A checkout ID is required.", nameof(checkoutId));
        }

        if (_items.Count == 0)
        {
            return false;
        }

        RaiseDomainEvent(new CartCheckoutRequestedDomainEvent(checkoutId, Id, CustomerId));
        return true;
    }

    private void EnsureCurrencyMatches(string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        if (Currency is not null &&
            !string.Equals(Currency, currency.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(CartErrors.CurrencyMismatchDescription);
        }
    }

    private void MarkUpdated(DateTimeOffset changedAtUtc)
    {
        UpdatedAtUtc = changedAtUtc;
        UpdatedBy = CustomerId.ToString();
    }
}
