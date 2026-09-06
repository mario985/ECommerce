using ECommerce.Modules.Cart.Application.Abstractions;
using ECommerce.Modules.Cart.Application;
using ECommerce.Modules.Cart.Contracts;
using ECommerce.Modules.Cart.Application.Carts.AddItem;
using ECommerce.Modules.Cart.Application.Carts.Checkout;
using ECommerce.Modules.Cart.Application.Carts.GetCart;
using ECommerce.Modules.Cart.Application.Carts.RemoveItem;
using ECommerce.Modules.Cart.Application.Carts.UpdateQuantity;
using ECommerce.Modules.Cart.Infrastructure.Persistence;
using ECommerce.Modules.Cart.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FluentValidation;

namespace ECommerce.Modules.Cart.Infrastructure;

public static class CartModule
{
    public static IServiceCollection AddCartModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Cart")
            ?? throw new InvalidOperationException("Connection string 'Cart' is required.");

        services.AddDbContext<CartDbContext>(options => options.UseSqlite(connectionString));
        services.AddHealthChecks().AddDbContextCheck<CartDbContext>(
            "cart-sqlite",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready", "database"]);
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartItemAdder, CartItemAdder>();
        services.AddScoped<ICartCheckoutRepository, CartCheckoutRepository>();
        services.AddHostedService<CartDbInitializer>();
        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<AddCartItemValidator>();
        services.AddScoped<RemoveCartItemValidator>();
        services.AddScoped<UpdateCartItemQuantityValidator>();
        services.AddValidatorsFromAssemblyContaining<AddCartItemValidator>();
        services.AddScoped<CartCheckoutRequestedDomainEventHandler>();
        services.AddMediatR(mediatRConfiguration =>
        {
            mediatRConfiguration.RegisterServicesFromAssembly(typeof(GetCartQueryHandler).Assembly);
        });

        return services;
    }
}
