namespace ECommerce.Common.Application.Observability;

public interface ICorrelationIdGenerator
{
    string Create();
}
