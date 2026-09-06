using ECommerce.Common.Domain.Entities;

namespace ECommerce.Modules.Cart.Domain.Carts;

public sealed class CartItem : Entity<Guid>
{
    private CartItem()
    {
        ProductName = string.Empty;
        Sku = string.Empty;
        Currency = string.Empty;
    }

    internal CartItem(
        Guid id,
        Guid productId,
        string productName,
        string sku,
        decimal unitPrice,
        string currency,
        int quantity)
        : base(id)
    {
        Validate(productId, productName, sku, unitPrice, currency, quantity);
        ProductId = productId;
        ProductName = productName.Trim();
        Sku = sku.Trim();
        UnitPrice = unitPrice;
        Currency = currency.Trim().ToUpperInvariant();
        Quantity = quantity;
    }

    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string Sku { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; }
    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;

    internal void IncreaseQuantity(int quantity)
    {
        EnsurePositiveQuantity(quantity);
        Quantity = checked(Quantity + quantity);
    }

    internal bool UpdateQuantity(int quantity)
    {
        EnsurePositiveQuantity(quantity);
        if (Quantity == quantity)
        {
            return false;
        }

        Quantity = quantity;
        return true;
    }

    internal void ReduceQuantity(int quantity)
    {
        EnsurePositiveQuantity(quantity);
        if (quantity >= Quantity)
        {
            throw new InvalidOperationException("A retained cart item must have a positive quantity.");
        }

        Quantity -= quantity;
    }

    internal bool RefreshSnapshot(
        string productName,
        string sku,
        decimal unitPrice,
        string currency)
    {
        Validate(ProductId, productName, sku, unitPrice, currency, Quantity);
        string normalizedName = productName.Trim();
        string normalizedSku = sku.Trim();
        string normalizedCurrency = currency.Trim().ToUpperInvariant();
        if (ProductName == normalizedName &&
            Sku == normalizedSku &&
            UnitPrice == unitPrice &&
            Currency == normalizedCurrency)
        {
            return false;
        }

        ProductName = normalizedName;
        Sku = normalizedSku;
        UnitPrice = unitPrice;
        Currency = normalizedCurrency;
        return true;
    }

    private static void Validate(
        Guid productId,
        string productName,
        string sku,
        decimal unitPrice,
        string currency,
        int quantity)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("A product ID is required.", nameof(productId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(productName);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);
        if (unitPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        }

        EnsurePositiveQuantity(quantity);
    }

    private static void EnsurePositiveQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }
    }
}
