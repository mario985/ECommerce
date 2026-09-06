using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Modules.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContextFactory : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<PaymentsDbContext> builder = new();
        builder.UseSqlite("Data Source=../../../../data/payments.db");
        return new PaymentsDbContext(builder.Options);
    }
}
