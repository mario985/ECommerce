namespace ECommerce.Common.Application.Observability;

public static class ObservabilityConstants
{
    public const string CorrelationHeaderName = "X-Correlation-ID";
    public const int MaximumCorrelationIdLength = 128;
    public const string CorrelationIdProperty = "CorrelationId";
    public const string TraceIdProperty = "TraceId";
    public const string SpanIdProperty = "SpanId";
}
