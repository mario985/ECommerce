using ECommerce.Modules.Cart.Domain.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CartAggregate = ECommerce.Modules.Cart.Domain.Carts.Cart;

namespace ECommerce.Modules.Cart.Infrastructure.Persistence.Configurations;

internal sealed class CartConfiguration : IEntityTypeConfiguration<CartAggregate>
{
    public void Configure(EntityTypeBuilder<CartAggregate> builder)
    {
        builder.ToTable("Carts");
        builder.HasKey(cart => cart.Id);
        builder.Property(cart => cart.Id).ValueGeneratedNever();

        builder.Property(cart => cart.CustomerId).IsRequired();
        builder.HasIndex(cart => cart.CustomerId).IsUnique();

        builder.Property(cart => cart.CreatedAtUtc).IsRequired();
        builder.Property(cart => cart.UpdatedAtUtc);
        builder.Property(cart => cart.CreatedBy).HasMaxLength(100);
        builder.Property(cart => cart.UpdatedBy).HasMaxLength(100);

        builder.Ignore(cart => cart.Total);
        builder.Ignore(cart => cart.Currency);
        builder.Ignore(cart => cart.DomainEvents);

        builder.HasMany(cart => cart.Items)
            .WithOne()
            .HasForeignKey("CartId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(cart => cart.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
