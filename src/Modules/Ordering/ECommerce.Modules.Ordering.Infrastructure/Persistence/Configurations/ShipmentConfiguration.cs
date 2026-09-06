using ECommerce.Modules.Ordering.Domain.Shipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Ordering.Infrastructure.Persistence.Configurations;

internal sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");
        builder.HasKey(shipment => shipment.Id);
        builder.Property(shipment => shipment.Id).ValueGeneratedNever();

        builder.Property(shipment => shipment.OrderId).IsRequired();
        builder.HasIndex(shipment => shipment.OrderId).IsUnique();
        builder.Property(shipment => shipment.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(shipment => shipment.Status);
        builder.Property(shipment => shipment.TrackingNumber)
            .HasMaxLength(Shipment.MaximumTrackingNumberLength)
            .IsRequired();
        builder.Property(shipment => shipment.Carrier)
            .HasMaxLength(Shipment.MaximumCarrierLength)
            .IsRequired();

        builder.Property(shipment => shipment.CreatedAtUtc)
            .HasConversion(
                value => value.UtcTicks,
                value => new DateTimeOffset(value, TimeSpan.Zero))
            .IsRequired();
        builder.HasIndex(shipment => shipment.CreatedAtUtc);
        builder.Property(shipment => shipment.UpdatedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);
        builder.Property(shipment => shipment.ShippedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);
        builder.Property(shipment => shipment.DeliveredAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);
        builder.Property(shipment => shipment.CreatedBy).HasMaxLength(100);
        builder.Property(shipment => shipment.UpdatedBy).HasMaxLength(100);
        builder.Ignore(shipment => shipment.DomainEvents);
    }
}
