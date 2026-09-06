using ECommerce.Common.Application.Messaging;

namespace ECommerce.Modules.Reviews.Contracts.IntegrationEvents;

public sealed record ProductRatingChangedIntegrationEvent(
    Guid EventId,
    Guid ProductId,
    double AverageRating,
    int ReviewCount,
    DateTimeOffset OccurredAtUtc,
    string CorrelationId = "") : IIntegrationEvent;
