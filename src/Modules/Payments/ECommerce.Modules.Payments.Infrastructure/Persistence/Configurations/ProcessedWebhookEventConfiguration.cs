using ECommerce.Modules.Payments.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Payments.Infrastructure.Persistence.Configurations;

internal sealed class ProcessedWebhookEventConfiguration
    : IEntityTypeConfiguration<ProcessedWebhookEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedWebhookEvent> builder)
    {
        builder.ToTable("ProcessedWebhookEvents");
        builder.HasKey(webhookEvent => webhookEvent.Id);
        builder.Property(webhookEvent => webhookEvent.Id).ValueGeneratedNever();
        builder.Property(webhookEvent => webhookEvent.StripeEventId)
            .HasMaxLength(255)
            .IsRequired();
        builder.HasIndex(webhookEvent => webhookEvent.StripeEventId).IsUnique();
        builder.Property(webhookEvent => webhookEvent.EventType)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(webhookEvent => webhookEvent.ProviderPaymentIntentId)
            .HasMaxLength(255)
            .IsRequired();
        builder.Property(webhookEvent => webhookEvent.ReceivedAtUtc)
            .HasConversion(
                value => value.UtcTicks,
                value => new DateTimeOffset(value, TimeSpan.Zero))
            .IsRequired();
        builder.Property(webhookEvent => webhookEvent.ProcessedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);
        builder.Property(webhookEvent => webhookEvent.Status)
            .HasConversion<int>()
            .IsRequired();
        builder.HasIndex(webhookEvent => webhookEvent.Status);
        builder.Property(webhookEvent => webhookEvent.LastError).HasMaxLength(500);
    }
}
