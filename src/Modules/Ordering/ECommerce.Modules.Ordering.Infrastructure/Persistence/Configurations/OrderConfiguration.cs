using ECommerce.Modules.Ordering.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Ordering.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedNever();

        builder.Property(order => order.CheckoutId).IsRequired();
        builder.HasIndex(order => order.CheckoutId).IsUnique();
        builder.Property(order => order.CustomerId).IsRequired();
        builder.HasIndex(order => order.CustomerId);
        builder.Property(order => order.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(order => order.Status);
        builder.Property(order => order.Currency).HasMaxLength(3).IsRequired();
        builder.Property(order => order.PaymentId);
        builder.HasIndex(order => order.PaymentId).IsUnique();
        builder.Property(order => order.PaidAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);
        builder.Property(order => order.PaymentFailedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);

        builder.Property(order => order.CreatedAtUtc)
            .HasConversion(
                value => value.UtcTicks,
                value => new DateTimeOffset(value, TimeSpan.Zero))
            .IsRequired();
        builder.HasIndex(order => order.CreatedAtUtc);
        builder.Property(order => order.UpdatedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue
                    ? new DateTimeOffset(value.Value, TimeSpan.Zero)
                    : null);
        builder.Property(order => order.CreatedBy).HasMaxLength(100);
        builder.Property(order => order.UpdatedBy).HasMaxLength(100);

        builder.Ignore(order => order.TotalAmount);
        builder.Ignore(order => order.DomainEvents);

        builder.HasMany(order => order.Lines)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(order => order.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
