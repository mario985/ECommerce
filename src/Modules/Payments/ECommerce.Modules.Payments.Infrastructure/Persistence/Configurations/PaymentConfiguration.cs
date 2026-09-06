using ECommerce.Modules.Payments.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Payments.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Id).ValueGeneratedNever();
        builder.Property(payment => payment.OrderId).IsRequired();
        builder.HasIndex(payment => payment.OrderId).IsUnique();
        builder.Property(payment => payment.CustomerId).IsRequired();
        builder.HasIndex(payment => payment.CustomerId);
        builder.Property(payment => payment.Amount).HasPrecision(18, 3).IsRequired();
        builder.Property(payment => payment.Currency).HasMaxLength(3).IsRequired();
        builder.Property(payment => payment.CorrelationId)
            .HasMaxLength(128)
            .IsRequired()
            .HasDefaultValue(string.Empty);
        builder.Property(payment => payment.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(payment => payment.Status);
        builder.Property(payment => payment.Provider).HasConversion<int>().IsRequired();
        builder.Property(payment => payment.ProviderPaymentIntentId).HasMaxLength(255);
        builder.HasIndex(payment => payment.ProviderPaymentIntentId)
            .IsUnique()
            .HasFilter("ProviderPaymentIntentId IS NOT NULL");
        builder.Property(payment => payment.ClientSecret).HasMaxLength(500);
        builder.Property(payment => payment.CreatedAtUtc)
            .HasConversion(
                value => value.UtcTicks,
                value => new DateTimeOffset(value, TimeSpan.Zero))
            .IsRequired();
        builder.HasIndex(payment => payment.CreatedAtUtc);
        builder.Property(payment => payment.UpdatedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);
        builder.Property(payment => payment.CreatedBy).HasMaxLength(100);
        builder.Property(payment => payment.UpdatedBy).HasMaxLength(100);
        builder.Ignore(payment => payment.DomainEvents);
        builder.HasMany(payment => payment.Attempts)
            .WithOne()
            .HasForeignKey(attempt => attempt.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(payment => payment.Attempts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
