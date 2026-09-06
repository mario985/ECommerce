using System.Threading.RateLimiting;
using ECommerce.Common.Application.Observability;
using ECommerce.Common.Presentation;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace ECommerce.Api.RateLimiting;

public sealed class RateLimitWindowOptions
{
    public int PermitLimit { get; set; } = 120;
    public int WindowSeconds { get; set; } = 60;
}

public static partial class RateLimitingExtensions
{
    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        RateLimitWindowOptions authentication = Read(configuration, RateLimitingPolicyNames.Authentication, 10);
        RateLimitWindowOptions publicRead = Read(configuration, RateLimitingPolicyNames.PublicRead, 120);
        RateLimitWindowOptions authenticated = Read(configuration, RateLimitingPolicyNames.Authenticated, 120);
        RateLimitWindowOptions admin = Read(configuration, RateLimitingPolicyNames.Admin, 180);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                RateLimitRejected(
                    context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                        .CreateLogger("Api.RateLimiting"),
                    context.HttpContext.Request.Path,
                    context.HttpContext.RequestServices.GetRequiredService<ICorrelationContext>().CorrelationId,
                    context.HttpContext.User.FindFirst("sub")?.Value);
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                }

                ProblemDetails problem = new()
                {
                    Type = "https://ecommerce/errors/api/rate-limit-exceeded",
                    Title = "Too many requests.",
                    Status = StatusCodes.Status429TooManyRequests,
                    Detail = "The request limit for this API policy was exceeded.",
                    Instance = context.HttpContext.Request.Path,
                };
                problem.Extensions["errorCode"] = "Api.RateLimitExceeded";
                problem.Extensions["correlationId"] = context.HttpContext.RequestServices
                    .GetRequiredService<ICorrelationContext>().CorrelationId;
                context.HttpContext.Response.ContentType = "application/problem+json";
                await context.HttpContext.Response.WriteAsJsonAsync(
                    problem,
                    options: null,
                    contentType: "application/problem+json",
                    cancellationToken: cancellationToken);
            };
            options.AddPolicy(RateLimitingPolicyNames.Authentication,
                context => FixedWindow(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", authentication));
            options.AddPolicy(RateLimitingPolicyNames.PublicRead,
                context => FixedWindow(context.Connection.RemoteIpAddress?.ToString() ?? "unknown", publicRead));
            options.AddPolicy(RateLimitingPolicyNames.Authenticated,
                context => FixedWindow(
                    context.User.FindFirst("sub")?.Value
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown",
                    authenticated));
            options.AddPolicy(RateLimitingPolicyNames.Admin,
                context => FixedWindow(
                    context.User.FindFirst("sub")?.Value
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown",
                    admin));
        });
        return services;
    }

    private static RateLimitWindowOptions Read(
        IConfiguration configuration,
        string name,
        int defaultPermitLimit) =>
        configuration.GetSection($"RateLimiting:{name}").Get<RateLimitWindowOptions>()
        ?? new RateLimitWindowOptions { PermitLimit = defaultPermitLimit };

    private static RateLimitPartition<string> FixedWindow(
        string partition,
        RateLimitWindowOptions options) =>
        RateLimitPartition.GetFixedWindowLimiter(
            partition,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = Math.Max(1, options.PermitLimit),
                Window = TimeSpan.FromSeconds(Math.Max(1, options.WindowSeconds)),
                QueueLimit = 0,
                AutoReplenishment = true,
            });

    [LoggerMessage(8201, LogLevel.Warning,
        "API rate limit rejected request. RequestPath={RequestPath} CorrelationId={CorrelationId} UserId={UserId}")]
    private static partial void RateLimitRejected(
        ILogger logger,
        string requestPath,
        string correlationId,
        string? userId);
}
