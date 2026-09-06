using ECommerce.Modules.Inventory.Domain.Reservations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class InventoryReservationConfiguration
    : IEntityTypeConfiguration<InventoryReservation>
{
    public void Configure(EntityTypeBuilder<InventoryReservation> builder)
    {
        builder.ToTable("InventoryReservations");

        builder.HasKey(reservation => reservation.Id);
        builder.Property(reservation => reservation.Id)
            .ValueGeneratedNever();

        builder.Property(reservation => reservation.StockItemId)
            .IsRequired();
        builder.Property(reservation => reservation.ProductId)
            .IsRequired();
        builder.Property(reservation => reservation.Quantity)
            .IsRequired();
        builder.Property(reservation => reservation.Status)
            .HasConversion<int>()
            .IsRequired();
        builder.Property(reservation => reservation.CreatedAtUtc)
            .IsRequired();
        builder.Property(reservation => reservation.OrderId);

        builder.HasIndex(reservation => reservation.ProductId);
        builder.HasIndex(reservation => reservation.OrderId);
        builder.HasIndex(reservation => new { reservation.OrderId, reservation.ProductId })
            .IsUnique()
            .HasFilter("OrderId IS NOT NULL");
        builder.HasIndex(reservation => reservation.Status);
    }
}
