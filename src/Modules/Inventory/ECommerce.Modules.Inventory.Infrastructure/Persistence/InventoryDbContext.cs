using ECommerce.Modules.Inventory.Domain.Reservations;
using ECommerce.Modules.Inventory.Domain.StockAdjustments;
using ECommerce.Modules.Inventory.Domain.StockItems;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Inventory.Infrastructure.Persistence;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options)
    : DbContext(options)
{
    public DbSet<StockItem> StockItems => Set<StockItem>();

    public DbSet<InventoryReservation> InventoryReservations =>
        Set<InventoryReservation>();

    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
