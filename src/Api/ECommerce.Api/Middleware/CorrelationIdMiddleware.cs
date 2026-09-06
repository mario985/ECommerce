using ECommerce.Common.Application.Observability;
using ECommerce.Common.Infrastructure.Observability;
using Serilog.Context;

namespace ECommerce.Api.Middleware;

public sealed class CorrelationIdMiddleware(
    RequestDelegate next,
    ICorrelationContext correlationContext,
    ICorrelationIdGenerator correlationIdGenerator)
{
    public async Task InvokeAsync(HttpContext context)
    {
        string? supplied = context.Request.Headers[ObservabilityConstants.CorrelationHeaderName]
            .FirstOrDefault();
        string correlationId = CorrelationIdGenerator.IsValid(supplied)
            ? supplied!
            : correlationIdGenerator.Create();

        using IDisposable correlationScope = correlationContext.Push(correlationId);
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[ObservabilityConstants.CorrelationHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using IDisposable correlationLog = LogContext.PushProperty(
            ObservabilityConstants.CorrelationIdProperty,
            correlationId);

        await next(context);
    }
}
