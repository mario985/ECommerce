using ECommerce.Modules.Wishlist.Domain.WishlistItems;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Wishlist.Infrastructure.Persistence;

public sealed class WishlistDbContext(DbContextOptions<WishlistDbContext> options) : DbContext(options)
{
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(typeof(WishlistDbContext).Assembly);
}
