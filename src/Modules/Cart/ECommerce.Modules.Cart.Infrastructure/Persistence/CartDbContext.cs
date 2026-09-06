using ECommerce.Modules.Cart.Domain.Carts;
using ECommerce.Modules.Cart.Domain.Checkouts;
using Microsoft.EntityFrameworkCore;
using CartAggregate = ECommerce.Modules.Cart.Domain.Carts.Cart;

namespace ECommerce.Modules.Cart.Infrastructure.Persistence;

public sealed class CartDbContext(DbContextOptions<CartDbContext> options) : DbContext(options)
{
    public DbSet<CartAggregate> Carts => Set<CartAggregate>();

    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<CartCheckout> CartCheckouts => Set<CartCheckout>();
    public DbSet<CartCheckoutItem> CartCheckoutItems => Set<CartCheckoutItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartDbContext).Assembly);
    }
}
