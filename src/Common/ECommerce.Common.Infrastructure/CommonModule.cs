using ECommerce.Common.Application.Messaging;
using ECommerce.Common.Application.Behaviors;
using ECommerce.Common.Application.Observability;
using ECommerce.Common.Infrastructure.Caching.Redis;
using ECommerce.Common.Infrastructure.Observability;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace ECommerce.Common.Infrastructure;

public static class CommonModule
{
    public static IServiceCollection AddCommon(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IIntegrationEventPublisher, MediatRIntegrationEventPublisher>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddRedisCaching(configuration);
        services.AddECommerceObservability(configuration);

        return services;
    }

    private sealed class MediatRIntegrationEventPublisher(
        IPublisher publisher,
        ICorrelationContext correlationContext,
        ILogger<MediatRIntegrationEventPublisher> logger)
        : IIntegrationEventPublisher
    {
        public async Task PublishAsync<TEvent>(
            TEvent integrationEvent,
            CancellationToken cancellationToken)
            where TEvent : IIntegrationEvent
        {
            string correlationId = CorrelationIdGenerator.IsValid(integrationEvent.CorrelationId)
                ? integrationEvent.CorrelationId
                : correlationContext.CorrelationId;
            if (!CorrelationIdGenerator.IsValid(integrationEvent.CorrelationId))
            {
                PropertyInfo property = typeof(TEvent).GetProperty(nameof(IIntegrationEvent.CorrelationId))
                    ?? throw new InvalidOperationException(
                        $"Integration event '{typeof(TEvent).Name}' has no correlation property.");
                property.SetValue(integrationEvent, correlationId);
            }
            using IDisposable correlationScope = correlationContext.Push(correlationId);
            using IDisposable? logScope = logger.BeginScope(new Dictionary<string, object>
            {
                [ObservabilityConstants.CorrelationIdProperty] = correlationId,
                ["IntegrationEventId"] = integrationEvent.EventId,
                ["IntegrationEventType"] = typeof(TEvent).Name,
            });
            using System.Diagnostics.Activity? activity = CommerceActivitySources.Messaging.StartActivity(
                $"IntegrationEvent {typeof(TEvent).Name}",
                System.Diagnostics.ActivityKind.Producer);
            activity?.SetTag("messaging.message.id", integrationEvent.EventId);
            activity?.SetTag("messaging.message.type", typeof(TEvent).FullName);
            activity?.SetTag("ecommerce.correlation.id", correlationId);
            await publisher.Publish(integrationEvent, cancellationToken);
        }
    }
}
