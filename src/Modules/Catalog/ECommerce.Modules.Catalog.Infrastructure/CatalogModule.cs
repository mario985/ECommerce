using ECommerce.Modules.Catalog.Application.Products.CreateProduct;
using ECommerce.Modules.Catalog.Application.Caching;
using ECommerce.Modules.Catalog.Contracts.Products;
using ECommerce.Modules.Catalog.Domain.Products;
using ECommerce.Modules.Catalog.Domain.Categories;
using ECommerce.Modules.Catalog.Infrastructure.MongoDb;
using ECommerce.Modules.Catalog.Infrastructure.Caching;
using ECommerce.Modules.Catalog.Infrastructure.Health;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using FluentValidation;

namespace ECommerce.Modules.Catalog.Infrastructure;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        CatalogCacheOptions cacheOptions = configuration
            .GetSection(CatalogCacheOptions.SectionName)
            .Get<CatalogCacheOptions>() ?? new CatalogCacheOptions();
        if (cacheOptions.ProductExpirationMinutes <= 0 || cacheOptions.SearchExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("Catalog cache expiration values must be positive.");
        }

        services.AddSingleton(cacheOptions);
        services.AddSingleton<CatalogSearchCacheVersion>();
        services
            .AddOptions<MongoDbOptions>()
            .Bind(configuration.GetSection(MongoDbOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                $"{MongoDbOptions.SectionName}:ConnectionString is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DatabaseName),
                $"{MongoDbOptions.SectionName}:DatabaseName is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ProductsCollectionName),
                $"{MongoDbOptions.SectionName}:ProductsCollectionName is required.")
            .ValidateOnStart();

        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            MongoDbOptions options = serviceProvider
                .GetRequiredService<IOptions<MongoDbOptions>>()
                .Value;

            return new MongoClient(options.ConnectionString);
        });

        services.AddSingleton(serviceProvider =>
        {
            MongoDbOptions options = serviceProvider
                .GetRequiredService<IOptions<MongoDbOptions>>()
                .Value;

            return serviceProvider
                .GetRequiredService<IMongoClient>()
                .GetDatabase(options.DatabaseName);
        });

        services.AddSingleton(serviceProvider =>
        {
            MongoDbOptions options = serviceProvider
                .GetRequiredService<IOptions<MongoDbOptions>>()
                .Value;

            return serviceProvider
                .GetRequiredService<IMongoDatabase>()
                .GetCollection<ProductDocument>(options.ProductsCollectionName);
        });
        services.AddSingleton(serviceProvider =>
        {
            MongoDbOptions options = serviceProvider.GetRequiredService<IOptions<MongoDbOptions>>().Value;
            return serviceProvider.GetRequiredService<IMongoDatabase>().GetCollection<CategoryDocument>(options.CategoriesCollectionName);
        });

        services.AddScoped<IProductRepository, MongoProductRepository>();
        services.AddScoped<ICategoryRepository, MongoCategoryRepository>();
        services.AddScoped<MongoProductCatalogReader>();
        services.AddScoped<IProductCatalogReader, CachedProductCatalogReader>();
        services.AddScoped<ICatalogCacheInvalidator, CatalogCacheInvalidator>();
        services.AddSingleton<MongoDbHealthCheck>();
        services.AddValidatorsFromAssemblyContaining<CreateProductCommandHandler>();
        services.AddHealthChecks().AddCheck<MongoDbHealthCheck>(
            "catalog-mongodb",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready", "database"],
            timeout: TimeSpan.FromSeconds(2));
        services.AddHostedService<MongoDbInitializer>();

        services.AddMediatR(mediatRConfiguration =>
        {
            mediatRConfiguration.RegisterServicesFromAssembly(
                typeof(CreateProductCommandHandler).Assembly);
        });

        return services;
    }
}
