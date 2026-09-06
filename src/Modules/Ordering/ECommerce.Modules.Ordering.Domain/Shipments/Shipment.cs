using ECommerce.Common.Domain.Entities;
using ECommerce.Modules.Ordering.Domain.Shipments.Events;

namespace ECommerce.Modules.Ordering.Domain.Shipments;

public sealed class Shipment : AuditableAggregateRoot<Guid>
{
    public const int MaximumCarrierLength = 100;
    public const int MaximumTrackingNumberLength = 100;

    private Shipment()
    {
        Carrier = string.Empty;
        TrackingNumber = string.Empty;
    }

    private Shipment(
        Guid id,
        Guid orderId,
        string carrier,
        string trackingNumber,
        DateTimeOffset createdAtUtc) : base(id)
    {
        OrderId = orderId;
        Carrier = carrier;
        TrackingNumber = trackingNumber;
        Status = ShipmentStatus.Pending;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid OrderId { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public string TrackingNumber { get; private set; }
    public string Carrier { get; private set; }
    public DateTimeOffset? ShippedAtUtc { get; private set; }
    public DateTimeOffset? DeliveredAtUtc { get; private set; }

    public static Shipment Create(
        Guid orderId,
        string carrier,
        string trackingNumber,
        DateTimeOffset createdAtUtc)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("An order ID is required.", nameof(orderId));
        }

        string normalizedCarrier = NormalizeRequired(
            carrier, MaximumCarrierLength, "A valid carrier is required.", nameof(carrier));
        string normalizedTrackingNumber = NormalizeRequired(
            trackingNumber,
            MaximumTrackingNumberLength,
            "A valid tracking number is required.",
            nameof(trackingNumber));

        Shipment shipment = new(
            Guid.NewGuid(), orderId, normalizedCarrier, normalizedTrackingNumber, createdAtUtc);
        shipment.RaiseDomainEvent(new ShipmentCreatedDomainEvent(
            shipment.Id, orderId, createdAtUtc));
        return shipment;
    }

    public ShipmentTransitionOutcome StartPreparing(DateTimeOffset occurredAtUtc) =>
        TransitionTo(
            ShipmentStatus.Preparing,
            [ShipmentStatus.Pending],
            occurredAtUtc,
            () => new ShipmentPreparedDomainEvent(Id, OrderId, occurredAtUtc));

    public ShipmentTransitionOutcome MarkAsShipped(DateTimeOffset occurredAtUtc) =>
        TransitionTo(
            ShipmentStatus.Shipped,
            [ShipmentStatus.Preparing],
            occurredAtUtc,
            () =>
            {
                ShippedAtUtc = occurredAtUtc;
                return new ShipmentShippedDomainEvent(Id, OrderId, occurredAtUtc);
            });

    public ShipmentTransitionOutcome MarkAsOutForDelivery(DateTimeOffset occurredAtUtc) =>
        TransitionTo(
            ShipmentStatus.OutForDelivery,
            [ShipmentStatus.Shipped],
            occurredAtUtc,
            () => new ShipmentOutForDeliveryDomainEvent(Id, OrderId, occurredAtUtc));

    public ShipmentTransitionOutcome MarkAsDelivered(DateTimeOffset occurredAtUtc) =>
        TransitionTo(
            ShipmentStatus.Delivered,
            [ShipmentStatus.OutForDelivery],
            occurredAtUtc,
            () =>
            {
                DeliveredAtUtc = occurredAtUtc;
                return new ShipmentDeliveredDomainEvent(Id, OrderId, occurredAtUtc);
            });

    public ShipmentTransitionOutcome Cancel(DateTimeOffset occurredAtUtc) =>
        TransitionTo(
            ShipmentStatus.Cancelled,
            [ShipmentStatus.Pending, ShipmentStatus.Preparing],
            occurredAtUtc,
            () => new ShipmentCancelledDomainEvent(Id, OrderId, occurredAtUtc));

    private ShipmentTransitionOutcome TransitionTo(
        ShipmentStatus target,
        IReadOnlyCollection<ShipmentStatus> allowedCurrentStatuses,
        DateTimeOffset occurredAtUtc,
        Func<ECommerce.Common.Domain.Events.IDomainEvent> createEvent)
    {
        if (Status == target)
        {
            return ShipmentTransitionOutcome.AlreadyApplied;
        }

        if (Status == ShipmentStatus.Delivered)
        {
            return ShipmentTransitionOutcome.AlreadyDelivered;
        }

        if (!allowedCurrentStatuses.Contains(Status))
        {
            return ShipmentTransitionOutcome.InvalidState;
        }

        Status = target;
        UpdatedAtUtc = occurredAtUtc;
        RaiseDomainEvent(createEvent());
        return ShipmentTransitionOutcome.Applied;
    }

    private static string NormalizeRequired(
        string value,
        int maximumLength,
        string message,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(message, parameterName);
        }

        string normalized = value.Trim();
        if (normalized.Length > maximumLength)
        {
            throw new ArgumentException(message, parameterName);
        }

        return normalized;
    }
}
