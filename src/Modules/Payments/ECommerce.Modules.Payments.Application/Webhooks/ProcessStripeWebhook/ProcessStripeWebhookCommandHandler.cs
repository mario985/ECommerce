using ECommerce.Common.Application.Errors;
using ECommerce.Common.Application.Observability;
using ECommerce.Modules.Payments.Application.Abstractions;
using ECommerce.Modules.Payments.Application.EventHandlers;
using ECommerce.Modules.Payments.Domain.Payments;
using ECommerce.Modules.Payments.Domain.Payments.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Payments.Application.Webhooks.ProcessStripeWebhook;

public sealed class ProcessStripeWebhookCommandHandler(
    IPaymentWebhookVerifier webhookVerifier,
    IWebhookEventRepository webhookEventRepository,
    IPaymentRepository paymentRepository,
    PaymentSucceededDomainEventHandler succeededEventHandler,
    PaymentFailedDomainEventHandler failedEventHandler,
    ICorrelationContext correlationContext,
    TimeProvider timeProvider,
    ILogger<ProcessStripeWebhookCommandHandler> logger)
    : IRequestHandler<ProcessStripeWebhookCommand, Result<StripeWebhookResult>>
{
    private const string ProcessingEvent = "payment_intent.processing";
    private const string SucceededEvent = "payment_intent.succeeded";
    private const string FailedEvent = "payment_intent.payment_failed";

    public async Task<Result<StripeWebhookResult>> Handle(
        ProcessStripeWebhookCommand request,
        CancellationToken cancellationToken)
    {
        using System.Diagnostics.Activity? activity = CommerceActivitySources.Payments.StartActivity("Payments.ProcessStripeWebhook");
        WebhookLog.Received(logger);
        Result<PaymentWebhookEvent> verification = webhookVerifier.Verify(
            request.Payload,
            request.Signature);
        if (verification.IsFailure)
        {
            return Result.Failure<StripeWebhookResult>(verification.Error!);
        }

        PaymentWebhookEvent webhookEvent = verification.Value;
        WebhookLog.Verified(
            logger, webhookEvent.EventId, webhookEvent.ProviderPaymentIntentId);
        WebhookDeliveryStatus? existingStatus = await webhookEventRepository.GetStatusAsync(
            webhookEvent.EventId,
            cancellationToken);
        if (existingStatus is WebhookDeliveryStatus.Processed or WebhookDeliveryStatus.Ignored)
        {
            WebhookLog.DuplicateSkipped(logger, webhookEvent.EventId);
            return Success("Duplicate", webhookEvent.EventId);
        }

        bool retryingIncompleteDelivery = existingStatus is
            WebhookDeliveryStatus.Processing or WebhookDeliveryStatus.Failed;
        if (existingStatus is null &&
            !await webhookEventRepository.TryAddProcessingAsync(
                webhookEvent,
                timeProvider.GetUtcNow(),
                cancellationToken))
        {
            WebhookLog.DuplicateSkipped(logger, webhookEvent.EventId);
            return Success("Duplicate", webhookEvent.EventId);
        }

        try
        {
            if (!IsSupported(webhookEvent.EventType))
            {
                await IgnoreAsync(webhookEvent, cancellationToken);
                return Success("Ignored", webhookEvent.EventId);
            }

            Payment? payment = await paymentRepository.GetByProviderPaymentIntentIdAsync(
                webhookEvent.ProviderPaymentIntentId,
                cancellationToken);
            if (payment is null)
            {
                WebhookLog.UnknownPayment(
                    logger, webhookEvent.EventId, webhookEvent.ProviderPaymentIntentId);
                await IgnoreAsync(webhookEvent, cancellationToken);
                return Success("Ignored", webhookEvent.EventId);
            }

            using IDisposable businessCorrelation = correlationContext.Push(payment.CorrelationId);
            using IDisposable? businessLogScope = logger.BeginScope(new Dictionary<string, object>
            {
                [ObservabilityConstants.CorrelationIdProperty] = payment.CorrelationId,
                ["PaymentId"] = payment.Id,
                ["OrderId"] = payment.OrderId,
                ["StripeEventId"] = webhookEvent.EventId,
            });
            activity?.SetTag("ecommerce.payment.id", payment.Id);
            activity?.SetTag("ecommerce.order.id", payment.OrderId);
            activity?.SetTag("ecommerce.payment.status", payment.Status.ToString());
            activity?.SetTag("ecommerce.correlation.id", payment.CorrelationId);
            WebhookLog.BusinessCorrelationRecovered(
                logger,
                webhookEvent.EventId,
                payment.Id,
                payment.OrderId);

            PaymentStateTransitionOutcome transition = ApplyTransition(payment, webhookEvent);
            if (transition == PaymentStateTransitionOutcome.InvalidState)
            {
                await IgnoreAsync(webhookEvent, cancellationToken);
                return Success("Ignored", webhookEvent.EventId);
            }

            if (transition == PaymentStateTransitionOutcome.Applied)
            {
                await paymentRepository.SaveChangesAsync(cancellationToken);
            }

            if (transition == PaymentStateTransitionOutcome.Applied || retryingIncompleteDelivery)
            {
                await PublishOutcomeAsync(payment, webhookEvent, cancellationToken);
            }

            payment.ClearDomainEvents();
            await webhookEventRepository.MarkProcessedAsync(
                webhookEvent.EventId,
                timeProvider.GetUtcNow(),
                cancellationToken);
            LogOutcome(payment, webhookEvent);
            return Success("Processed", webhookEvent.EventId);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            WebhookLog.ProcessingFailed(logger, webhookEvent.EventId, exception);
            try
            {
                await webhookEventRepository.MarkFailedAsync(
                    webhookEvent.EventId,
                    PaymentErrors.WebhookProcessingFailedDescription,
                    timeProvider.GetUtcNow(),
                    cancellationToken);
            }
            catch (Exception persistenceException) when (
                persistenceException is not OperationCanceledException)
            {
                WebhookLog.ProcessingFailed(logger, webhookEvent.EventId, persistenceException);
            }

            return Result.Failure<StripeWebhookResult>(new Error(
                PaymentErrors.WebhookProcessingFailedCode,
                PaymentErrors.WebhookProcessingFailedDescription,
                ErrorType.Failure));
        }
    }

    private static bool IsSupported(string eventType) =>
        eventType is ProcessingEvent or SucceededEvent or FailedEvent;

    private static PaymentStateTransitionOutcome ApplyTransition(
        Payment payment,
        PaymentWebhookEvent webhookEvent) => webhookEvent.EventType switch
        {
            ProcessingEvent => payment.MarkProcessing(webhookEvent.OccurredAtUtc),
            SucceededEvent => payment.MarkSucceeded(webhookEvent.OccurredAtUtc),
            FailedEvent => payment.MarkFailed(
                webhookEvent.FailureCode,
                webhookEvent.FailureMessage,
                webhookEvent.OccurredAtUtc),
            _ => PaymentStateTransitionOutcome.InvalidState,
        };

    private async Task PublishOutcomeAsync(
        Payment payment,
        PaymentWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        if (webhookEvent.EventType == SucceededEvent)
        {
            PaymentSucceededDomainEvent domainEvent = payment.DomainEvents
                .OfType<PaymentSucceededDomainEvent>()
                .SingleOrDefault() ?? new PaymentSucceededDomainEvent(
                    payment.Id, payment.OrderId, payment.CustomerId);
            await succeededEventHandler.HandleAsync(
                domainEvent,
                payment,
                webhookEvent.OccurredAtUtc,
                cancellationToken);
        }
        else if (webhookEvent.EventType == FailedEvent)
        {
            PaymentFailedDomainEvent domainEvent = payment.DomainEvents
                .OfType<PaymentFailedDomainEvent>()
                .SingleOrDefault() ?? new PaymentFailedDomainEvent(
                    payment.Id,
                    payment.OrderId,
                    payment.CustomerId,
                    webhookEvent.FailureCode);
            await failedEventHandler.HandleAsync(
                domainEvent,
                webhookEvent.OccurredAtUtc,
                payment.CorrelationId,
                cancellationToken);
        }
    }

    private async Task IgnoreAsync(
        PaymentWebhookEvent webhookEvent,
        CancellationToken cancellationToken)
    {
        await webhookEventRepository.MarkIgnoredAsync(
            webhookEvent.EventId,
            timeProvider.GetUtcNow(),
            cancellationToken);
        WebhookLog.Ignored(logger, webhookEvent.EventId, webhookEvent.EventType);
    }

    private void LogOutcome(Payment payment, PaymentWebhookEvent webhookEvent)
    {
        switch (webhookEvent.EventType)
        {
            case ProcessingEvent:
                WebhookLog.Processing(
                    logger,
                    webhookEvent.ProviderPaymentIntentId,
                    payment.Id,
                    payment.OrderId);
                break;
            case SucceededEvent:
                WebhookLog.Succeeded(logger, payment.Id, payment.OrderId, webhookEvent.EventId);
                break;
            case FailedEvent:
                WebhookLog.Failed(
                    logger,
                    payment.Id,
                    payment.OrderId,
                    webhookEvent.EventId,
                    webhookEvent.FailureCode);
                break;
        }
    }

    private static Result<StripeWebhookResult> Success(string outcome, string eventId) =>
        Result.Success(new StripeWebhookResult(outcome, eventId));
}
