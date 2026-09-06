using System.Threading;
using ECommerce.Common.Application.Observability;

namespace ECommerce.Common.Infrastructure.Observability;

internal sealed class CorrelationContext(ICorrelationIdGenerator generator) : ICorrelationContext
{
    private static readonly AsyncLocal<Holder?> Current = new();

    public string CorrelationId
    {
        get
        {
            if (Current.Value is null)
            {
                Current.Value = new Holder(generator.Create());
            }

            return Current.Value.Value;
        }
    }

    public IDisposable Push(string correlationId)
    {
        Holder? previous = Current.Value;
        string value = CorrelationIdGenerator.IsValid(correlationId)
            ? correlationId
            : generator.Create();
        Current.Value = new Holder(value);
        return new RestoreScope(previous);
    }

    private sealed record Holder(string Value);

    private sealed class RestoreScope(Holder? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Current.Value = previous;
            _disposed = true;
        }
    }
}
