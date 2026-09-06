namespace ECommerce.Common.Infrastructure.Observability;

public sealed class ObservabilityOptions
{
    public const string SectionName = "OpenTelemetry";

    public string ServiceName { get; init; } = "ECommerce.Api";
    public bool ConsoleExporterEnabled { get; init; }
    public bool OtlpExporterEnabled { get; init; }
    public string OtlpEndpoint { get; init; } = string.Empty;
}
