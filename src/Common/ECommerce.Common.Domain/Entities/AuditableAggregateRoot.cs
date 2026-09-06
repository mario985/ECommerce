namespace ECommerce.Common.Domain.Entities;

public abstract class AuditableAggregateRoot<TId> : AggregateRoot<TId>
    where TId : notnull
{
    protected AuditableAggregateRoot()
    {
    }

    protected AuditableAggregateRoot(TId id)
        : base(id)
    {
    }

    public DateTimeOffset CreatedAtUtc { get; protected set; }

    public DateTimeOffset? UpdatedAtUtc { get; protected set; }

    public string? CreatedBy { get; protected set; }

    public string? UpdatedBy { get; protected set; }
}
