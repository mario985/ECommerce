using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Identity.Application.Abstractions;
using ECommerce.Modules.Identity.Domain.Users;
using ECommerce.Modules.Identity.Domain.Users.Events;
using FluentValidation.Results;
using MediatR;

namespace ECommerce.Modules.Identity.Application.Authentication.Register;

public sealed class RegisterUserCommandHandler(
    IIdentityService identityService,
    RegisterUserValidator validator,
    UserRegisteredDomainEventHandler domainEventHandler)
    : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<RegisterUserResponse>(new Error(
                "Identity.RegistrationValidation",
                string.Join(" ", validationResult.Errors.Select(error => error.ErrorMessage)),
                ErrorType.Validation));
        }

        string email = request.Email.Trim();
        Result<Guid> creationResult = await identityService.CreateUserAsync(
            email,
            request.Password,
            cancellationToken);

        if (creationResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(creationResult.Error!);
        }

        User user = User.Register(creationResult.Value, email);
        UserRegisteredDomainEvent domainEvent = user.DomainEvents
            .OfType<UserRegisteredDomainEvent>()
            .Single();

        await domainEventHandler.HandleAsync(domainEvent, cancellationToken);
        user.ClearDomainEvents();

        return Result.Success(new RegisterUserResponse(user.Id, user.Email));
    }
}
