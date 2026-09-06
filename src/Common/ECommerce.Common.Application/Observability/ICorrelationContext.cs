namespace ECommerce.Common.Application.Observability;

public interface ICorrelationContext
{
    string CorrelationId { get; }

    IDisposable Push(string correlationId);
}
