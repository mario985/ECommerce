using ECommerce.Common.Application.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ECommerce.Common.Infrastructure.Observability;

internal static class ObservabilityServiceExtensions
{
    public static IServiceCollection AddECommerceObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ObservabilityOptions options = configuration
            .GetSection(ObservabilityOptions.SectionName)
            .Get<ObservabilityOptions>() ?? new ObservabilityOptions();

        services.AddSingleton<ICorrelationIdGenerator, CorrelationIdGenerator>();
        services.AddSingleton<ICorrelationContext, CorrelationContext>();
        services.AddHealthChecks().AddCheck(
            "self",
            () => HealthCheckResult.Healthy("The process is responsive."),
            tags: ["live"]);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(options.ServiceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(
                        CommerceActivitySources.CartName,
                        CommerceActivitySources.OrderingName,
                        CommerceActivitySources.InventoryName,
                        CommerceActivitySources.PaymentsName,
                        CommerceActivitySources.MessagingName)
                    .AddAspNetCoreInstrumentation(instrumentation =>
                    {
                        instrumentation.Filter = context =>
                            !context.Request.Path.StartsWithSegments("/health/live");
                    })
                    .AddHttpClientInstrumentation();

                if (options.ConsoleExporterEnabled)
                {
                    tracing.AddConsoleExporter();
                }

                if (options.OtlpExporterEnabled)
                {
                    tracing.AddOtlpExporter(exporter =>
                    {
                        if (Uri.TryCreate(options.OtlpEndpoint, UriKind.Absolute, out Uri? endpoint))
                        {
                            exporter.Endpoint = endpoint;
                        }
                    });
                }
            });

        return services;
    }
}
