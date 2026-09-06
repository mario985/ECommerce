using ECommerce.Modules.Inventory.Domain.StockItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("StockItems");

        builder.HasKey(stockItem => stockItem.Id);

        builder.Property(stockItem => stockItem.ProductId)
            .IsRequired();
        builder.HasIndex(stockItem => stockItem.ProductId)
            .IsUnique();

        builder.Property(stockItem => stockItem.Sku)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(stockItem => stockItem.AvailableQuantity)
            .IsRequired();
        builder.Property(stockItem => stockItem.ReservedQuantity)
            .IsRequired();

        builder.HasMany(stockItem => stockItem.Reservations)
            .WithOne()
            .HasForeignKey(reservation => reservation.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(stockItem => stockItem.Reservations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
