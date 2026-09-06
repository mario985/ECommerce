using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Modules.Cart.Infrastructure.Persistence;

public sealed class CartDbContextFactory : IDesignTimeDbContextFactory<CartDbContext>
{
    public CartDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<CartDbContext> options =
            new DbContextOptionsBuilder<CartDbContext>()
                .UseSqlite("Data Source=../../../../data/cart.db")
                .Options;

        return new CartDbContext(options);
    }
}
