using ECommerce.Modules.Cart.Domain.Checkouts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Cart.Infrastructure.Persistence.Configurations;

internal sealed class CartCheckoutConfiguration : IEntityTypeConfiguration<CartCheckout>
{
    public void Configure(EntityTypeBuilder<CartCheckout> builder)
    {
        builder.ToTable("CartCheckouts");
        builder.HasKey(checkout => checkout.Id);
        builder.Property(checkout => checkout.Id).ValueGeneratedNever();
        builder.Property(checkout => checkout.CheckoutId).IsRequired();
        builder.HasIndex(checkout => checkout.CheckoutId).IsUnique();
        builder.Property(checkout => checkout.CartId).IsRequired();
        builder.HasIndex(checkout => checkout.CartId);
        builder.Property(checkout => checkout.CustomerId).IsRequired();
        builder.HasIndex(checkout => checkout.CustomerId);
        builder.HasIndex(checkout => checkout.OrderId);
        builder.Property(checkout => checkout.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(checkout => checkout.Status);
        builder.HasIndex(checkout => checkout.CreatedAtUtc);
        builder.HasIndex(checkout => checkout.CartId).IsUnique()
            .HasFilter("Status = 1");
        builder.Property(checkout => checkout.CreatedAtUtc).IsRequired();
        builder.Property(checkout => checkout.CreatedBy).HasMaxLength(100);
        builder.Property(checkout => checkout.UpdatedBy).HasMaxLength(100);
        builder.Ignore(checkout => checkout.DomainEvents);
        builder.HasMany(checkout => checkout.Items).WithOne()
            .HasForeignKey("CartCheckoutId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(checkout => checkout.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
