using ECommerce.Modules.Wishlist.Application.Abstractions;
using ECommerce.Modules.Wishlist.Application.AddWishlistItem;
using ECommerce.Modules.Wishlist.Application.GetWishlist;
using ECommerce.Modules.Wishlist.Infrastructure.Persistence;
using ECommerce.Modules.Wishlist.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECommerce.Modules.Wishlist.Infrastructure;

public static class WishlistModule
{
    public static IServiceCollection AddWishlistModule(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Wishlist") ?? throw new InvalidOperationException("Connection string 'Wishlist' is required.");
        WishlistCacheOptions cacheOptions = configuration.GetSection(WishlistCacheOptions.SectionName).Get<WishlistCacheOptions>() ?? new();
        if (cacheOptions.ExpirationMinutes <= 0) throw new InvalidOperationException("WishlistCache:ExpirationMinutes must be positive.");
        services.AddSingleton(cacheOptions);
        services.AddDbContext<WishlistDbContext>(options => options.UseSqlite(connectionString));
        services.AddHealthChecks().AddDbContextCheck<WishlistDbContext>("wishlist-sqlite", failureStatus: HealthStatus.Unhealthy, tags: ["ready", "database"]);
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddHostedService<WishlistDbInitializer>();
        services.AddValidatorsFromAssemblyContaining<AddWishlistItemValidator>();
        services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(AddWishlistItemCommandHandler).Assembly));
        return services;
    }
}
