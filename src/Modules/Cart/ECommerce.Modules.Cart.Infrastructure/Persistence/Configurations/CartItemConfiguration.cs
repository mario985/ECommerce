using ECommerce.Modules.Cart.Domain.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Cart.Infrastructure.Persistence.Configurations;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();

        builder.Property<Guid>("CartId").IsRequired();
        builder.Property(item => item.ProductId).IsRequired();
        builder.HasIndex("CartId", nameof(CartItem.ProductId)).IsUnique();

        builder.Property(item => item.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Sku).HasMaxLength(100).IsRequired();
        builder.Property(item => item.UnitPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(item => item.Currency).HasMaxLength(3).IsRequired();
        builder.Property(item => item.Quantity).IsRequired();

        builder.Ignore(item => item.LineTotal);
    }
}
