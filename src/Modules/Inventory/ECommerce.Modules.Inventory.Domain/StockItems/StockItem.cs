using ECommerce.Common.Domain.Entities;
using ECommerce.Modules.Inventory.Domain.Reservations;
using ECommerce.Modules.Inventory.Domain.StockItems.Events;

namespace ECommerce.Modules.Inventory.Domain.StockItems;

public sealed class StockItem : AggregateRoot<Guid>
{
    private readonly List<InventoryReservation> _reservations = [];

    private StockItem()
    {
    }

    private StockItem(Guid id, Guid productId, string sku)
        : base(id)
    {
        ProductId = productId;
        Sku = sku;
    }

    public Guid ProductId { get; private set; }

    public string Sku { get; private set; } = null!;

    public int AvailableQuantity { get; private set; }

    public int ReservedQuantity { get; private set; }

    public IReadOnlyCollection<InventoryReservation> Reservations =>
        _reservations.AsReadOnly();

    public static StockItem Create(Guid productId, string sku)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product ID cannot be empty.", nameof(productId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sku);

        return new StockItem(Guid.NewGuid(), productId, sku.Trim().ToUpperInvariant());
    }

    public void IncreaseStock(int quantity)
    {
        EnsurePositiveQuantity(quantity);
        AvailableQuantity = checked(AvailableQuantity + quantity);
        RaiseDomainEvent(new StockIncreasedDomainEvent(Id, ProductId, quantity));
    }

    public void DecreaseStock(int quantity)
    {
        EnsurePositiveQuantity(quantity);

        if (quantity > AvailableQuantity)
        {
            throw new InsufficientAvailableStockException(AvailableQuantity, quantity);
        }

        AvailableQuantity -= quantity;
        RaiseDomainEvent(new StockDecreasedDomainEvent(Id, ProductId, quantity));
    }

    public InventoryReservation Reserve(
        int quantity,
        DateTimeOffset createdAtUtc,
        Guid? orderId = null)
    {
        EnsurePositiveQuantity(quantity);

        if (quantity > AvailableQuantity)
        {
            throw new InsufficientAvailableStockException(AvailableQuantity, quantity);
        }

        InventoryReservation reservation = new(
            Id,
            ProductId,
            quantity,
            createdAtUtc,
            orderId);

        AvailableQuantity -= quantity;
        ReservedQuantity = checked(ReservedQuantity + quantity);
        _reservations.Add(reservation);

        return reservation;
    }

    public InventoryReservation ReleaseReservation(Guid reservationId)
    {
        InventoryReservation reservation = GetReservation(reservationId);
        reservation.EnsureCanTransitionTo(InventoryReservationStatus.Released);
        int availableQuantity = checked(AvailableQuantity + reservation.Quantity);

        EnsureReservedQuantityCanBeRemoved(reservation.Quantity);
        reservation.Release();

        AvailableQuantity = availableQuantity;
        ReservedQuantity -= reservation.Quantity;

        return reservation;
    }

    public InventoryReservation ConfirmReservation(Guid reservationId)
    {
        InventoryReservation reservation = GetReservation(reservationId);
        reservation.EnsureCanTransitionTo(InventoryReservationStatus.Confirmed);

        EnsureReservedQuantityCanBeRemoved(reservation.Quantity);
        reservation.Confirm();

        ReservedQuantity -= reservation.Quantity;

        return reservation;
    }

    public int ConfirmReservationsForOrder(Guid orderId)
    {
        InventoryReservation[] reservations = ReservationsForOrder(orderId);
        if (reservations.Length == 0 ||
            reservations.All(reservation => reservation.Status == InventoryReservationStatus.Confirmed))
        {
            return 0;
        }

        if (reservations.Any(reservation => reservation.Status != InventoryReservationStatus.Pending))
        {
            throw new InvalidOperationException(
                "An Order reservation cannot be confirmed from its current state.");
        }

        int quantity = reservations.Sum(reservation => reservation.Quantity);
        EnsureReservedQuantityCanBeRemoved(quantity);
        foreach (InventoryReservation reservation in reservations)
        {
            reservation.Confirm();
        }

        ReservedQuantity -= quantity;
        return reservations.Length;
    }

    public int ReleaseReservationsForOrder(Guid orderId)
    {
        InventoryReservation[] reservations = ReservationsForOrder(orderId);
        if (reservations.Length == 0 ||
            reservations.All(reservation => reservation.Status == InventoryReservationStatus.Released))
        {
            return 0;
        }

        if (reservations.Any(reservation => reservation.Status != InventoryReservationStatus.Pending))
        {
            throw new InvalidOperationException(
                "An Order reservation cannot be released from its current state.");
        }

        int quantity = reservations.Sum(reservation => reservation.Quantity);
        EnsureReservedQuantityCanBeRemoved(quantity);
        int availableQuantity = checked(AvailableQuantity + quantity);
        foreach (InventoryReservation reservation in reservations)
        {
            reservation.Release();
        }

        AvailableQuantity = availableQuantity;
        ReservedQuantity -= quantity;
        return reservations.Length;
    }

    private InventoryReservation[] ReservationsForOrder(Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("An order ID is required.", nameof(orderId));
        }

        return _reservations
            .Where(reservation => reservation.OrderId == orderId)
            .ToArray();
    }

    private InventoryReservation GetReservation(Guid reservationId)
    {
        return _reservations.SingleOrDefault(
                reservation => reservation.Id == reservationId)
            ?? throw new InventoryReservationNotFoundException(reservationId);
    }

    private void EnsureReservedQuantityCanBeRemoved(int quantity)
    {
        if (quantity > ReservedQuantity)
        {
            throw new InvalidOperationException(
                "Reserved stock quantity is inconsistent with its reservation.");
        }
    }

    private static void EnsurePositiveQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                quantity,
                "Quantity must be greater than zero.");
        }
    }
}
