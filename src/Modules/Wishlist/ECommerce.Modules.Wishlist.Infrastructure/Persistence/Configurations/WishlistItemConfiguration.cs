using ECommerce.Modules.Wishlist.Domain.WishlistItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Wishlist.Infrastructure.Persistence.Configurations;

internal sealed class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("wishlist_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.UserId).IsRequired();
        builder.Property(item => item.ProductId).IsRequired();
        builder.Property(item => item.CreatedAtUtc).IsRequired();
        builder.HasIndex(item => item.UserId);
        builder.HasIndex(item => new { item.UserId, item.ProductId }).IsUnique();
        builder.Ignore(item => item.DomainEvents);
    }
}
