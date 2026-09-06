using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Modules.Wishlist.Infrastructure.Persistence;

public sealed class WishlistDbContextFactory : IDesignTimeDbContextFactory<WishlistDbContext>
{
    public WishlistDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<WishlistDbContext> options = new DbContextOptionsBuilder<WishlistDbContext>().UseSqlite("Data Source=../../../../data/wishlist.db").Options;
        return new WishlistDbContext(options);
    }
}
