using System.Diagnostics;
using ECommerce.Common.Application.Observability;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    ICorrelationContext correlationContext) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        #pragma warning disable CA1848
        logger.LogError(
            exception,
            "Unhandled API exception. CorrelationId={CorrelationId} TraceId={TraceId} RequestPath={RequestPath}",
            correlationContext.CorrelationId,
            Activity.Current?.TraceId.ToString(),
            httpContext.Request.Path.Value);
        #pragma warning restore CA1848

        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        ProblemDetails problem = new()
        {
            Type = "https://ecommerce/errors/api/unexpected",
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "The server could not complete the request.",
            Instance = httpContext.Request.Path,
        };
        problem.Extensions["errorCode"] = "Api.UnexpectedError";
        problem.Extensions["correlationId"] = correlationContext.CorrelationId;
        problem.Extensions["traceId"] = Activity.Current?.TraceId.ToString();

        httpContext.Response.StatusCode = problem.Status!.Value;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);
        return true;
    }
}
