using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Modules.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContextFactory : IDesignTimeDbContextFactory<OrderingDbContext>
{
    public OrderingDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<OrderingDbContext> options =
            new DbContextOptionsBuilder<OrderingDbContext>()
                .UseSqlite("Data Source=../../../../data/ordering.db")
                .Options;
        return new OrderingDbContext(options);
    }
}
