using ECommerce.Modules.Payments.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Payments.Infrastructure.Persistence.Configurations;

internal sealed class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("PaymentAttempts");
        builder.HasKey(attempt => attempt.Id);
        builder.Property(attempt => attempt.Id).ValueGeneratedNever();
        builder.Property(attempt => attempt.PaymentId).IsRequired();
        builder.HasIndex(attempt => attempt.PaymentId);
        builder.Property(attempt => attempt.ProviderReference).HasMaxLength(255);
        builder.Property(attempt => attempt.Status).HasConversion<int>().IsRequired();
        builder.Property(attempt => attempt.CreatedAtUtc)
            .HasConversion(
                value => value.UtcTicks,
                value => new DateTimeOffset(value, TimeSpan.Zero))
            .IsRequired();
        builder.Property(attempt => attempt.FailureCode).HasMaxLength(100);
        builder.Property(attempt => attempt.FailureMessage).HasMaxLength(500);
    }
}
