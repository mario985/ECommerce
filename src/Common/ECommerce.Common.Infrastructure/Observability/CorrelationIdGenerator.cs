using ECommerce.Common.Application.Observability;

namespace ECommerce.Common.Infrastructure.Observability;

public sealed class CorrelationIdGenerator : ICorrelationIdGenerator
{
    public string Create() => Guid.NewGuid().ToString("N");

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > ObservabilityConstants.MaximumCorrelationIdLength)
        {
            return false;
        }

        return value.All(character => character is >= '!' and <= '~');
    }
}
