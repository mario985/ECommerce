using ECommerce.Modules.Ordering.Application.Abstractions;
using ECommerce.Modules.Ordering.Application.IntegrationEvents;
using ECommerce.Modules.Ordering.Application.Orders.GetOrderHistory;
using ECommerce.Modules.Ordering.Infrastructure.Persistence;
using ECommerce.Modules.Ordering.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FluentValidation;
using ECommerce.Modules.Ordering.Contracts.Reviews;
using ECommerce.Modules.Ordering.Application.Caching;

namespace ECommerce.Modules.Ordering.Infrastructure;

public static class OrderingModule
{
    public static IServiceCollection AddOrderingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Ordering")
            ?? throw new InvalidOperationException("Connection string 'Ordering' is required.");

        services.AddDbContext<OrderingDbContext>(options => options.UseSqlite(connectionString));
        services.AddHealthChecks().AddDbContextCheck<OrderingDbContext>(
            "ordering-sqlite",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready", "database"]);
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IOrderReviewEligibilityReader, OrderReviewEligibilityReader>();
        services.AddHostedService<OrderingDbInitializer>();
        services.TryAddSingleton(TimeProvider.System);

        ShipmentTrackingCacheOptions trackingCacheOptions = configuration
            .GetSection(ShipmentTrackingCacheOptions.SectionName)
            .Get<ShipmentTrackingCacheOptions>() ?? new ShipmentTrackingCacheOptions();
        if (trackingCacheOptions.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("Shipment tracking cache expiration must be positive.");
        }

        services.AddSingleton(trackingCacheOptions);

        services.AddScoped<GetOrderHistoryValidator>();
        services.AddScoped<CartCheckoutRequestedValidator>();
        services.AddScoped<OrderCreatedDomainEventHandler>();
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(
                typeof(CartCheckoutRequestedIntegrationEventHandler).Assembly);
        });
        services.AddValidatorsFromAssemblyContaining<GetOrderHistoryValidator>();

        return services;
    }
}
