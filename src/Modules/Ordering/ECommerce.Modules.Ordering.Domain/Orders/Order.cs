using ECommerce.Common.Domain.Entities;
using ECommerce.Modules.Ordering.Domain.Orders.Events;

namespace ECommerce.Modules.Ordering.Domain.Orders;

public sealed class Order : AuditableAggregateRoot<Guid>
{
    private readonly List<OrderLine> _lines = [];

    private Order()
    {
        Currency = string.Empty;
    }

    private Order(Guid id, Guid checkoutId, Guid customerId, DateTimeOffset createdAtUtc)
        : base(id)
    {
        CheckoutId = checkoutId;
        CustomerId = customerId;
        Status = OrderStatus.PendingInventory;
        CreatedAtUtc = createdAtUtc;
        CreatedBy = customerId.ToString();
        Currency = string.Empty;
    }

    public Guid CheckoutId { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();
    public decimal TotalAmount => _lines.Sum(line => line.LineTotal);
    public string Currency { get; private set; }
    public Guid? PaymentId { get; private set; }
    public DateTimeOffset? PaidAtUtc { get; private set; }
    public DateTimeOffset? PaymentFailedAtUtc { get; private set; }

    public static Order Create(
        Guid checkoutId,
        Guid customerId,
        IReadOnlyCollection<OrderItemSnapshot> items,
        DateTimeOffset createdAtUtc)
    {
        if (checkoutId == Guid.Empty)
        {
            throw new ArgumentException("A checkout ID is required.", nameof(checkoutId));
        }

        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("A customer ID is required.", nameof(customerId));
        }

        ArgumentNullException.ThrowIfNull(items);
        if (items.Count == 0)
        {
            throw new ArgumentException(OrderErrors.EmptyDescription, nameof(items));
        }

        if (items.Select(item => item.ProductId).Distinct().Count() != items.Count)
        {
            throw new ArgumentException("An order cannot contain duplicate products.", nameof(items));
        }

        Order order = new(Guid.NewGuid(), checkoutId, customerId, createdAtUtc);
        foreach (OrderItemSnapshot item in items)
        {
            OrderLine line = new(
                item.ProductId,
                item.ProductName,
                item.Sku,
                item.UnitPrice,
                item.Currency,
                item.Quantity);
            if (order._lines.Count > 0 &&
                !string.Equals(order.Currency, line.Currency, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(OrderErrors.CurrencyMismatchDescription, nameof(items));
            }

            order.Currency = line.Currency;
            order._lines.Add(line);
        }

        order.RaiseDomainEvent(new OrderCreatedDomainEvent(order.Id, checkoutId, customerId));
        return order;
    }

    public OrderTransitionOutcome MarkInventoryReserved(DateTimeOffset updatedAtUtc)
    {
        if (Status == OrderStatus.AwaitingPayment)
        {
            return OrderTransitionOutcome.AlreadyApplied;
        }

        if (Status != OrderStatus.PendingInventory)
        {
            return OrderTransitionOutcome.InvalidState;
        }

        Status = OrderStatus.AwaitingPayment;
        MarkUpdated(updatedAtUtc);
        RaiseDomainEvent(new OrderInventoryReservedDomainEvent(Id, CheckoutId, CustomerId));
        return OrderTransitionOutcome.Applied;
    }

    public OrderTransitionOutcome MarkInventoryReservationFailed(DateTimeOffset updatedAtUtc)
    {
        if (Status == OrderStatus.InventoryFailed)
        {
            return OrderTransitionOutcome.AlreadyApplied;
        }

        if (Status != OrderStatus.PendingInventory)
        {
            return OrderTransitionOutcome.InvalidState;
        }

        Status = OrderStatus.InventoryFailed;
        MarkUpdated(updatedAtUtc);
        RaiseDomainEvent(new OrderInventoryReservationFailedDomainEvent(Id, CheckoutId, CustomerId));
        return OrderTransitionOutcome.Applied;
    }

    public OrderTransitionOutcome MarkPaid(Guid paymentId, DateTimeOffset paidAtUtc)
    {
        if (paymentId == Guid.Empty)
        {
            throw new ArgumentException("A payment ID is required.", nameof(paymentId));
        }

        if (Status == OrderStatus.Paid && PaymentId == paymentId)
        {
            return OrderTransitionOutcome.AlreadyApplied;
        }

        if (Status != OrderStatus.AwaitingPayment)
        {
            return OrderTransitionOutcome.InvalidState;
        }

        Status = OrderStatus.Paid;
        PaymentId = paymentId;
        PaidAtUtc = paidAtUtc;
        MarkUpdated(paidAtUtc);
        RaiseDomainEvent(new OrderPaidDomainEvent(Id, CheckoutId, CustomerId, paymentId));
        return OrderTransitionOutcome.Applied;
    }

    public OrderTransitionOutcome MarkPaymentFailed(DateTimeOffset failedAtUtc)
    {
        if (Status == OrderStatus.PaymentFailed)
        {
            return OrderTransitionOutcome.AlreadyApplied;
        }

        if (Status != OrderStatus.AwaitingPayment)
        {
            return OrderTransitionOutcome.InvalidState;
        }

        Status = OrderStatus.PaymentFailed;
        PaymentFailedAtUtc = failedAtUtc;
        MarkUpdated(failedAtUtc);
        RaiseDomainEvent(new OrderPaymentFailedDomainEvent(Id, CheckoutId, CustomerId));
        return OrderTransitionOutcome.Applied;
    }

    private void MarkUpdated(DateTimeOffset updatedAtUtc)
    {
        UpdatedAtUtc = updatedAtUtc;
        UpdatedBy = CustomerId.ToString();
    }
}
