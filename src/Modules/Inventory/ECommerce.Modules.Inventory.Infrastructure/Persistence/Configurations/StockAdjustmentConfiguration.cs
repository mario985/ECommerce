using ECommerce.Modules.Inventory.Application.StockAdjustments.AdjustStock;
using ECommerce.Modules.Inventory.Domain.StockAdjustments;
using ECommerce.Modules.Inventory.Domain.StockItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockAdjustmentConfiguration
    : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("StockAdjustments");
        builder.HasKey(adjustment => adjustment.Id);
        builder.Property(adjustment => adjustment.Quantity).IsRequired();
        builder.Property(adjustment => adjustment.Type).IsRequired();
        builder.Property(adjustment => adjustment.Reason).IsRequired();
        builder.Property(adjustment => adjustment.Note)
            .HasMaxLength(AdjustStockValidator.MaximumNoteLength);
        builder.Property(adjustment => adjustment.PerformedBy).IsRequired();
        builder.Property(adjustment => adjustment.OccurredAtUtc)
            .HasConversion<long>()
            .IsRequired();

        builder.HasIndex(adjustment => new
        {
            adjustment.ProductId,
            adjustment.OccurredAtUtc,
        });
        builder.HasIndex(adjustment => adjustment.StockItemId);
        builder.HasIndex(adjustment => adjustment.PerformedBy);
        builder.HasIndex(adjustment => new
        {
            adjustment.ProductId,
            adjustment.Type,
            adjustment.Reason,
        });

        builder.HasOne<StockItem>()
            .WithMany()
            .HasForeignKey(adjustment => adjustment.StockItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
