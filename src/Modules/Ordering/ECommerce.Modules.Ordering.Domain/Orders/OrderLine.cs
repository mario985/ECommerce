using ECommerce.Common.Domain.Entities;

namespace ECommerce.Modules.Ordering.Domain.Orders;

public sealed class OrderLine : Entity<Guid>
{
    private OrderLine()
    {
        ProductName = string.Empty;
        Sku = string.Empty;
        Currency = string.Empty;
    }

    internal OrderLine(
        Guid productId,
        string productName,
        string sku,
        decimal unitPrice,
        string currency,
        int quantity)
        : base(Guid.NewGuid())
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

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

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
}
