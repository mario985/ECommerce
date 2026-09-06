using ECommerce.Common.Application.Errors;
using ECommerce.Common.Application.Observability;
using ECommerce.Modules.Ordering.Contracts.IntegrationEvents;
using ECommerce.Modules.Payments.Application.Abstractions;
using ECommerce.Modules.Payments.Domain.Payments;
using ECommerce.Modules.Payments.Domain.Payments.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Payments.Application.IntegrationEvents;

#pragma warning disable CA1711 // Integration event handler is the architecture's explicit terminology.
public sealed class OrderAwaitingPaymentIntegrationEventHandler(
    IPaymentRepository paymentRepository,
    IPaymentGateway paymentGateway,
    PaymentIntentCreatedDomainEventHandler domainEventHandler,
    TimeProvider timeProvider,
    ILogger<OrderAwaitingPaymentIntegrationEventHandler> logger)
    : INotificationHandler<OrderAwaitingPaymentIntegrationEvent>
{
    public async Task Handle(
        OrderAwaitingPaymentIntegrationEvent notification,
        CancellationToken cancellationToken)
    {
        using System.Diagnostics.Activity? activity = CommerceActivitySources.Payments.StartActivity("Payments.CreatePaymentIntent");
        activity?.SetTag("ecommerce.order.id", notification.OrderId);
        activity?.SetTag("ecommerce.customer.id", notification.CustomerId);
        PaymentLog.OrderAwaitingPaymentReceived(
            logger, notification.OrderId, notification.CustomerId);

        if (!IsValid(notification))
        {
            PaymentLog.InvalidEventRejected(logger, notification.OrderId, notification.CustomerId);
            return;
        }

        Payment? payment = await paymentRepository.GetByOrderIdAsync(
            notification.OrderId,
            cancellationToken);
        if (payment?.ProviderPaymentIntentId is not null)
        {
            PaymentLog.DuplicateIgnored(
                logger, payment.Id, notification.OrderId, notification.CustomerId);
            return;
        }

        if (payment is null)
        {
            payment = Payment.Create(
                notification.OrderId,
                notification.CustomerId,
                notification.TotalAmount,
                notification.Currency,
                timeProvider.GetUtcNow(),
                notification.CorrelationId);
            await paymentRepository.AddAsync(payment, cancellationToken);
            await paymentRepository.SaveChangesAsync(cancellationToken);
            PaymentLog.Created(logger, payment.Id, payment.OrderId, payment.CustomerId);
            payment.ClearDomainEvents();
        }

        PaymentLog.IntentCreationStarted(logger, payment.Id, payment.OrderId, payment.CustomerId);
        Result<CreatePaymentIntentResult> gatewayResult =
            await paymentGateway.CreatePaymentIntentAsync(
                new CreatePaymentIntentRequest(
                    payment.Id,
                    payment.OrderId,
                    payment.CustomerId,
                    payment.Amount,
                    payment.Currency,
                    $"payment-intent-order-{payment.OrderId:N}"),
                cancellationToken);

        if (gatewayResult.IsFailure)
        {
            Error error = gatewayResult.Error!;
            payment.MarkIntentCreationFailed(
                error.Code,
                error.Description,
                timeProvider.GetUtcNow());
            await paymentRepository.SaveChangesAsync(cancellationToken);
            payment.ClearDomainEvents();
            PaymentLog.IntentCreationFailed(
                logger, error.Code, payment.Id, payment.OrderId, payment.CustomerId);
            return;
        }

        CreatePaymentIntentResult gatewayPayment = gatewayResult.Value;
        PaymentIntentUpdateOutcome outcome = payment.SetPaymentIntent(
            gatewayPayment.ProviderPaymentIntentId,
            gatewayPayment.ClientSecret,
            timeProvider.GetUtcNow());
        if (outcome != PaymentIntentUpdateOutcome.Applied)
        {
            PaymentLog.DuplicateIgnored(
                logger, payment.Id, payment.OrderId, payment.CustomerId);
            return;
        }

        await paymentRepository.SaveChangesAsync(cancellationToken);
        PaymentIntentCreatedDomainEvent domainEvent = payment.DomainEvents
            .OfType<PaymentIntentCreatedDomainEvent>()
            .Single();
        payment.ClearDomainEvents();
        await domainEventHandler.HandleAsync(domainEvent, payment, cancellationToken);
        activity?.SetTag("ecommerce.payment.id", payment.Id);
        PaymentLog.IntentCreated(
            logger,
            gatewayPayment.ProviderPaymentIntentId,
            payment.Id,
            payment.OrderId,
            payment.CustomerId);
    }

    private static bool IsValid(OrderAwaitingPaymentIntegrationEvent notification) =>
        notification.OrderId != Guid.Empty &&
        notification.CustomerId != Guid.Empty &&
        notification.TotalAmount > 0 &&
        !string.IsNullOrWhiteSpace(notification.Currency) &&
        notification.Currency.Trim().Length == 3;
}
#pragma warning restore CA1711
