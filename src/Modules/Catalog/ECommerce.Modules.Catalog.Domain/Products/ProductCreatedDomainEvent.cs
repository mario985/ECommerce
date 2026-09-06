using ECommerce.Common.Domain.Events;

namespace ECommerce.Modules.Catalog.Domain.Products;

public sealed record ProductCreatedDomainEvent(Guid ProductId) : IDomainEvent;
