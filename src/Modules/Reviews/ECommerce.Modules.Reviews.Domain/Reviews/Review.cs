using ECommerce.Common.Domain.Entities;
using ECommerce.Modules.Reviews.Domain.Reviews.Events;

namespace ECommerce.Modules.Reviews.Domain.Reviews;

public sealed class Review : AuditableAggregateRoot<Guid>
{
    public const int MaximumCommentLength = 2000;

    private Review(
        Guid id,
        Guid productId,
        Guid customerId,
        int rating,
        string comment,
        ReviewStatus status,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc) : base(id)
    {
        ProductId = productId;
        CustomerId = customerId;
        Rating = rating;
        Comment = comment;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        CreatedBy = customerId.ToString();
    }

    public Guid ProductId { get; private set; }
    public Guid CustomerId { get; private set; }
    public int Rating { get; private set; }
    public string Comment { get; private set; }
    public ReviewStatus Status { get; private set; }

    public static Review Create(
        Guid productId,
        Guid customerId,
        int rating,
        string comment,
        DateTimeOffset createdAtUtc)
    {
        if (productId == Guid.Empty) throw new ArgumentException("A product ID is required.", nameof(productId));
        if (customerId == Guid.Empty) throw new ArgumentException("A customer ID is required.", nameof(customerId));
        if (rating is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
        ArgumentException.ThrowIfNullOrWhiteSpace(comment);
        string normalizedComment = comment.Trim();
        if (normalizedComment.Length > MaximumCommentLength) throw new ArgumentException("The review comment is too long.", nameof(comment));

        Review review = new(
            Guid.NewGuid(), productId, customerId, rating, normalizedComment,
            ReviewStatus.Pending, createdAtUtc, null);
        review.RaiseDomainEvent(new ReviewCreatedDomainEvent(
            review.Id, productId, customerId, createdAtUtc));
        return review;
    }

    public static Review Rehydrate(
        Guid id,
        Guid productId,
        Guid customerId,
        int rating,
        string comment,
        ReviewStatus status,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc) =>
        new(id, productId, customerId, rating, comment, status, createdAtUtc, updatedAtUtc);

    public ReviewModerationOutcome Approve(DateTimeOffset occurredAtUtc)
    {
        if (Status == ReviewStatus.Approved) return ReviewModerationOutcome.AlreadyApplied;
        if (Status != ReviewStatus.Pending) return ReviewModerationOutcome.InvalidState;
        Status = ReviewStatus.Approved;
        MarkUpdated(occurredAtUtc);
        RaiseDomainEvent(new ReviewApprovedDomainEvent(Id, ProductId, CustomerId, occurredAtUtc));
        return ReviewModerationOutcome.Applied;
    }

    public ReviewModerationOutcome Reject(DateTimeOffset occurredAtUtc)
    {
        if (Status == ReviewStatus.Rejected) return ReviewModerationOutcome.AlreadyApplied;
        if (Status != ReviewStatus.Pending) return ReviewModerationOutcome.InvalidState;
        Status = ReviewStatus.Rejected;
        MarkUpdated(occurredAtUtc);
        RaiseDomainEvent(new ReviewRejectedDomainEvent(Id, ProductId, CustomerId, occurredAtUtc));
        return ReviewModerationOutcome.Applied;
    }

    private void MarkUpdated(DateTimeOffset occurredAtUtc)
    {
        UpdatedAtUtc = occurredAtUtc;
        UpdatedBy = null;
    }
}
