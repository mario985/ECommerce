using ECommerce.Api.Middleware;
using ECommerce.Api.Health;
using ECommerce.Api.Observability;
using ECommerce.Api.ExceptionHandling;
using ECommerce.Api.Extensions;
using ECommerce.Api.OpenApi;
using ECommerce.Api.RateLimiting;
using ECommerce.Api.Versioning;
using ECommerce.Common.Application.Observability;
using ECommerce.Common.Infrastructure;
using ECommerce.Modules.Cart.Infrastructure;
using ECommerce.Modules.Cart.Presentation;
using ECommerce.Modules.Catalog.Infrastructure;
using ECommerce.Modules.Catalog.Presentation;
using ECommerce.Modules.Identity.Infrastructure;
using ECommerce.Modules.Identity.Presentation;
using ECommerce.Modules.Inventory.Infrastructure;
using ECommerce.Modules.Inventory.Presentation;
using ECommerce.Modules.Ordering.Infrastructure;
using ECommerce.Modules.Ordering.Presentation;
using ECommerce.Modules.Payments.Infrastructure;
using ECommerce.Modules.Payments.Presentation;
using ECommerce.Modules.Reviews.Infrastructure;
using ECommerce.Modules.Reviews.Presentation.Reviews;
using ECommerce.Modules.Wishlist.Infrastructure;
using ECommerce.Modules.Wishlist.Presentation;
using System.Text.Json.Serialization;
using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.UseRepositoryDataDirectory(builder.Environment);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = builder.Configuration
        .GetValue<long?>("Api:MaxRequestBodyBytes") ?? 1_048_576;
});

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.With<ActivityLogEventEnricher>());

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        ICorrelationContext correlation = context.HttpContext.RequestServices
            .GetRequiredService<ICorrelationContext>();
        context.ProblemDetails.Extensions["correlationId"] = correlation.CorrelationId;
        context.ProblemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToString();
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApiRateLimiting(builder.Configuration);
builder.Services.AddConfiguredCors(builder.Configuration);
builder.Services.AddUrlApiVersioning();
builder.Services.AddApiDocumentation(builder.Configuration);

builder.Services
    .AddCommon(builder.Configuration)
    .AddIdentityModule(builder.Configuration)
    .AddCatalogModule(builder.Configuration)
    .AddInventoryModule(builder.Configuration)
    .AddCartModule(builder.Configuration)
    .AddOrderingModule(builder.Configuration)
    .AddPaymentsModule(builder.Configuration)
    .AddReviewsModule(builder.Configuration)
    .AddWishlistModule(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (context, _, exception) =>
    {
        if (exception is not null || context.Response.StatusCode >= 500)
        {
            return LogEventLevel.Error;
        }

        return context.Request.Path.StartsWithSegments("/health")
            ? LogEventLevel.Debug
            : LogEventLevel.Information;
    };
    options.EnrichDiagnosticContext = (diagnosticContext, context) =>
    {
        ICorrelationContext correlation = context.RequestServices.GetRequiredService<ICorrelationContext>();
        diagnosticContext.Set(ObservabilityConstants.CorrelationIdProperty, correlation.CorrelationId);
        diagnosticContext.Set(ObservabilityConstants.TraceIdProperty, Activity.Current?.TraceId.ToString() ?? string.Empty);
        diagnosticContext.Set(ObservabilityConstants.SpanIdProperty, Activity.Current?.SpanId.ToString() ?? string.Empty);
        string? userId = context.User.FindFirst("sub")?.Value;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            diagnosticContext.Set("UserId", userId);
        }
    };
});
app.UseExceptionHandler();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseStatusCodePages(async statusContext =>
{
    HttpContext context = statusContext.HttpContext;
    if (context.Response.HasStarted || context.Response.ContentLength.HasValue ||
        context.Response.StatusCode is not (401 or 403 or 404))
    {
        return;
    }

    int status = context.Response.StatusCode;
    string code = status switch
    {
        401 => "Api.Unauthorized",
        403 => "Api.Forbidden",
        _ => "Api.NotFound",
    };
    ProblemDetails problem = new()
    {
        Type = $"https://ecommerce/errors/api/{code[(code.IndexOf('.') + 1)..].ToLowerInvariant()}",
        Title = status switch
        {
            401 => "Authentication is required.",
            403 => "You are not authorized to perform this action.",
            _ => "The requested resource was not found.",
        },
        Status = status,
        Detail = "The request could not be completed.",
        Instance = context.Request.Path,
    };
    problem.Extensions["errorCode"] = code;
    problem.Extensions["correlationId"] = context.RequestServices
        .GetRequiredService<ICorrelationContext>().CorrelationId;
    problem.Extensions["traceId"] = Activity.Current?.TraceId.ToString();
    context.Response.ContentType = "application/problem+json";
    await context.Response.WriteAsJsonAsync(
        problem,
        options: null,
        contentType: "application/problem+json",
        cancellationToken: context.RequestAborted);
});
app.UseCors(CorsExtensions.PolicyName);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseApiDocumentation();

app.MapIdentityEndpoints();
app.MapCatalogEndpoints();
app.MapInventoryEndpoints();
app.MapCartEndpoints();
app.MapOrderingEndpoints();
app.MapPaymentsEndpoints();
app.MapReviewEndpoints();
app.MapWishlistEndpoints();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("live"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync,
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = registration => registration.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
    },
});

app.Run();

public partial class Program;
