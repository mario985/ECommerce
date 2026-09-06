using ECommerce.Common.Domain.Entities;

namespace ECommerce.Modules.Inventory.Domain.StockAdjustments;

public sealed class StockAdjustment : Entity<Guid>
{
    private StockAdjustment()
    {
    }

    private StockAdjustment(
        Guid id,
        Guid stockItemId,
        Guid productId,
        int quantity,
        StockAdjustmentType type,
        StockAdjustmentReason reason,
        string? note,
        Guid performedBy,
        DateTimeOffset occurredAtUtc)
        : base(id)
    {
        StockItemId = stockItemId;
        ProductId = productId;
        Quantity = quantity;
        Type = type;
        Reason = reason;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        PerformedBy = performedBy;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
    }

    public Guid StockItemId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public StockAdjustmentType Type { get; private set; }
    public StockAdjustmentReason Reason { get; private set; }
    public string? Note { get; private set; }
    public Guid PerformedBy { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }

    public static StockAdjustment Create(
        Guid stockItemId,
        Guid productId,
        int quantity,
        StockAdjustmentType type,
        StockAdjustmentReason reason,
        string? note,
        Guid performedBy,
        DateTimeOffset occurredAtUtc)
    {
        if (stockItemId == Guid.Empty || productId == Guid.Empty || performedBy == Guid.Empty)
        {
            throw new ArgumentException("Stock item, product, and administrator IDs are required.");
        }

        if (quantity <= 0 || !Enum.IsDefined(type) || !Enum.IsDefined(reason))
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Adjustment values are invalid.");
        }

        return new StockAdjustment(
            Guid.NewGuid(), stockItemId, productId, quantity, type, reason,
            note, performedBy, occurredAtUtc);
    }
}
