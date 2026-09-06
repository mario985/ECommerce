using ECommerce.Modules.Payments.Application.Abstractions;
using ECommerce.Modules.Payments.Application.IntegrationEvents;
using ECommerce.Modules.Payments.Application.EventHandlers;
using ECommerce.Modules.Payments.Infrastructure.Persistence;
using ECommerce.Modules.Payments.Infrastructure.Persistence.Repositories;
using ECommerce.Modules.Payments.Infrastructure.Stripe;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FluentValidation;

namespace ECommerce.Modules.Payments.Infrastructure;

public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        StripeOptions stripeOptions = configuration
            .GetSection(StripeOptions.SectionName)
            .Get<StripeOptions>() ?? new StripeOptions();
        if (!stripeOptions.Enabled)
        {
            return services;
        }

        string connectionString = configuration.GetConnectionString("Payments")
            ?? throw new InvalidOperationException("Connection string 'Payments' is required.");

        services.AddOptions<StripeOptions>()
            .Bind(configuration.GetSection(StripeOptions.SectionName))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.SecretKey),
                "Stripe:SecretKey is required when Stripe payments are enabled.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.WebhookSecret),
                "Stripe:WebhookSecret is required when Stripe payments are enabled.")
            .ValidateOnStart();
        services.AddDbContext<PaymentsDbContext>(options => options.UseSqlite(connectionString));
        services.AddHealthChecks().AddDbContextCheck<PaymentsDbContext>(
            "payments-sqlite",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready", "database"]);
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddSingleton<IStripeAmountConverter, StripeAmountConverter>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        services.AddScoped<IPaymentWebhookVerifier, StripeWebhookVerifier>();
        services.AddScoped<IWebhookEventRepository, WebhookEventRepository>();
        services.AddScoped<PaymentIntentCreatedDomainEventHandler>();
        services.AddScoped<PaymentSucceededDomainEventHandler>();
        services.AddScoped<PaymentFailedDomainEventHandler>();
        services.AddHostedService<PaymentsDbInitializer>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(
                typeof(OrderAwaitingPaymentIntegrationEventHandler).Assembly);
        });
        return services;
    }
}
