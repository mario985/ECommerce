using ECommerce.Common.Application.Errors;
using ECommerce.Common.Application.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ECommerce.Common.Presentation.Results;

public static class ApiResults
{
    public static IResult Validation(
        string description,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        Problem(new Error("Api.Validation", description, ErrorType.Validation, errors));

    public static IResult InvalidPagination() => Validation(
        "Pagination parameters are invalid.",
        new Dictionary<string, string[]>
        {
            ["page"] = ["Page must be greater than or equal to 1."],
            ["pageSize"] = ["PageSize must be between 1 and 100."],
        });

    public static IResult Problem(Error error)
    {
        int statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError,
        };

        return new ProblemResult(error, statusCode);
    }

    private sealed class ProblemResult(Error error, int statusCode) : IResult
    {
        public async Task ExecuteAsync(HttpContext httpContext)
        {
            ProblemDetails problem = new()
            {
                Type = $"https://ecommerce/errors/{error.Code.Replace('.', '/').ToLowerInvariant()}",
                Title = error.Description,
                Status = statusCode,
                Detail = error.Description,
                Instance = httpContext.Request.Path,
            };
            problem.Extensions["errorCode"] = error.Code;
            problem.Extensions["correlationId"] =
                (httpContext.RequestServices.GetService(typeof(ICorrelationContext)) as ICorrelationContext)?.CorrelationId
                ?? httpContext.Request.Headers[ObservabilityConstants.CorrelationHeaderName].ToString();
            problem.Extensions["traceId"] = Activity.Current?.TraceId.ToString();
            if (error.ValidationErrors is not null)
            {
                problem.Extensions["errors"] = error.ValidationErrors;
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(
                problem,
                options: null,
                contentType: "application/problem+json",
                cancellationToken: httpContext.RequestAborted);
        }
    }
}
