using System.Diagnostics;
using ECommerce.Common.Application.Observability;
using Serilog.Core;
using Serilog.Events;

namespace ECommerce.Api.Observability;

public sealed class ActivityLogEventEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        Activity? activity = Activity.Current;
        if (activity is null)
        {
            return;
        }

        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(
            ObservabilityConstants.TraceIdProperty,
            activity.TraceId.ToString()));
        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(
            ObservabilityConstants.SpanIdProperty,
            activity.SpanId.ToString()));
    }
}
