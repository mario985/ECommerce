using ECommerce.Modules.Reviews.Application.Abstractions;
using ECommerce.Modules.Reviews.Application.Caching;
using ECommerce.Modules.Reviews.Application.Reviews.CreateReview;
using ECommerce.Modules.Reviews.Infrastructure.Caching;
using ECommerce.Modules.Reviews.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ECommerce.Modules.Reviews.Infrastructure;

public static class ReviewsModule
{
    public static IServiceCollection AddReviewsModule(this IServiceCollection services, IConfiguration configuration)
    {
        ReviewCacheOptions cacheOptions = configuration.GetSection(ReviewCacheOptions.SectionName)
            .Get<ReviewCacheOptions>() ?? new ReviewCacheOptions();
        if (cacheOptions.ApprovedReviewsExpirationMinutes <= 0)
            throw new InvalidOperationException("Reviews cache expiration must be positive.");
        services.AddSingleton(cacheOptions);

        services.AddOptions<ReviewsMongoDbOptions>()
            .Bind(configuration.GetSection(ReviewsMongoDbOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.ConnectionString), "ReviewsMongoDb:ConnectionString is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.DatabaseName), "ReviewsMongoDb:DatabaseName is required.")
            .ValidateOnStart();
        services.AddSingleton(serviceProvider =>
        {
            ReviewsMongoDbOptions options = serviceProvider.GetRequiredService<IOptions<ReviewsMongoDbOptions>>().Value;
            MongoClient client = new(options.ConnectionString);
            return client.GetDatabase(options.DatabaseName).GetCollection<ReviewDocument>(options.ReviewsCollectionName);
        });
        services.AddScoped<IReviewRepository, MongoReviewRepository>();
        services.AddScoped<IReviewCacheInvalidator, ReviewCacheInvalidator>();
        services.AddHostedService<ReviewsMongoDbInitializer>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssemblyContaining<CreateReviewValidator>();
        services.AddMediatR(options => options.RegisterServicesFromAssembly(typeof(CreateReviewCommandHandler).Assembly));
        return services;
    }
}
