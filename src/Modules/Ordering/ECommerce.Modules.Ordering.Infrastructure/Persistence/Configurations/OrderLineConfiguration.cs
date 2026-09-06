using ECommerce.Modules.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Ordering.Infrastructure.Persistence.Configurations;

internal sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");
        builder.HasKey(line => line.Id);
        builder.Property(line => line.Id).ValueGeneratedNever();

        builder.Property<Guid>("OrderId").IsRequired();
        builder.Property(line => line.ProductId).IsRequired();
        builder.HasIndex("OrderId", nameof(OrderLine.ProductId)).IsUnique();
        builder.Property(line => line.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(line => line.Sku).HasMaxLength(100).IsRequired();
        builder.Property(line => line.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(line => line.Currency).HasMaxLength(3).IsRequired();
        builder.Property(line => line.Quantity).IsRequired();
        builder.Ignore(line => line.LineTotal);
    }
}
