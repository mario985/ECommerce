using ECommerce.Modules.Inventory.Application.Reservations.ReserveOrderInventory;
using ECommerce.Modules.Inventory.Application;
using ECommerce.Modules.Inventory.Application.Admin.SearchInventory;
using ECommerce.Modules.Inventory.Application.StockAdjustments.AdjustStock;
using ECommerce.Modules.Inventory.Application.StockAdjustments.GetStockAdjustmentHistory;
using ECommerce.Modules.Inventory.Domain.StockAdjustments;
using ECommerce.Modules.Inventory.Application.StockItems.GetStock;
using ECommerce.Modules.Inventory.Domain.StockItems;
using ECommerce.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FluentValidation;

namespace ECommerce.Modules.Inventory.Infrastructure;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Inventory")
            ?? throw new InvalidOperationException(
                "Connection string 'Inventory' is required.");

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddHealthChecks().AddDbContextCheck<InventoryDbContext>(
            "inventory-sqlite",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready", "database"]);
        services.AddScoped<IStockItemRepository, StockItemRepository>();
        services.AddScoped<IInventoryAdminRepository, StockItemRepository>();
        services.AddScoped<IStockAdjustmentRepository, StockAdjustmentRepository>();
        int lowStockThreshold = configuration.GetValue<int?>(
            $"{InventoryOptions.SectionName}:LowStockThreshold") ?? 10;
        if (lowStockThreshold < 0)
        {
            throw new InvalidOperationException(
                "Inventory:LowStockThreshold must be nonnegative.");
        }
        services.AddSingleton(new InventoryOptions
        {
            LowStockThreshold = lowStockThreshold,
        });
        services.AddHostedService<InventoryDbInitializer>();
        services.AddScoped<OrderInventoryReservationRequestedValidator>();
        services.AddScoped<AdjustStockValidator>();
        services.AddValidatorsFromAssemblyContaining<AdjustStockValidator>();
        services.AddScoped<GetStockAdjustmentHistoryValidator>();
        services.AddScoped<SearchInventoryValidator>();

        services.AddMediatR(mediatRConfiguration =>
        {
            mediatRConfiguration.RegisterServicesFromAssembly(
                typeof(GetStockQueryHandler).Assembly);
        });

        return services;
    }
}
