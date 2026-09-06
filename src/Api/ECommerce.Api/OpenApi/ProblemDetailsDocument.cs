using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.OpenApi;

/// <summary>
/// Documents the standard ProblemDetails payload returned by the API.
/// </summary>
public class ApiProblemDetailsDocument : ProblemDetails
{
    /// <summary>Stable, client-consumable API error code.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>Request correlation identifier.</summary>
    public string? CorrelationId { get; init; }

    /// <summary>Distributed tracing identifier.</summary>
    public string? TraceId { get; init; }
}

/// <summary>
/// Documents the validation ProblemDetails payload returned for invalid requests.
/// </summary>
public sealed class ApiValidationProblemDetailsDocument : ApiProblemDetailsDocument
{
    /// <summary>Validation messages grouped by request property.</summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }
}

/// <summary>
/// Documents the pagination envelope shared by collection endpoints.
/// </summary>
public sealed class ApiPagedResponseDocument
{
    /// <summary>Page items. The item shape is specific to the endpoint.</summary>
    public IReadOnlyCollection<object> Items { get; init; } = [];

    /// <summary>One-based page number.</summary>
    public int Page { get; init; } = 1;

    /// <summary>Number of items requested per page.</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>Total number of matching items.</summary>
    public long TotalCount { get; init; }

    /// <summary>Total number of available pages.</summary>
    public int TotalPages { get; init; }
}
