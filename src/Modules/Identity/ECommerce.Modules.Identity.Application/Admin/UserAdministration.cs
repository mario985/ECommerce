using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Identity.Application.Abstractions;
using ECommerce.Modules.Identity.Domain.Roles;
using ECommerce.Modules.Identity.Domain.Users;
using ECommerce.Modules.Identity.Domain.Users.Events;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Identity.Application.Admin;

public sealed record UserSummaryResponse(Guid Id, string Email, UserStatus Status, IReadOnlyCollection<string> Roles, DateTimeOffset CreatedAtUtc);
public sealed record UserSearchResponse(IReadOnlyCollection<UserSummaryResponse> Items, int Page, int PageSize, long TotalCount, int TotalPages);
public sealed record SearchUsersQuery(string? Search, string? Role, UserStatus? Status, int Page = 1, int PageSize = 20) : IRequest<Result<UserSearchResponse>>;
public sealed record GetAdminUserQuery(Guid UserId) : IRequest<Result<UserSummaryResponse>>;
public sealed record AssignRoleCommand(Guid UserId, string Role) : IRequest<Result>;
public sealed record RemoveRoleCommand(Guid UserId, string Role) : IRequest<Result>;
public sealed record DisableUserCommand(Guid UserId) : IRequest<Result>;
public sealed record EnableUserCommand(Guid UserId) : IRequest<Result>;

public sealed class SearchUsersValidator : AbstractValidator<SearchUsersQuery>
{
    public SearchUsersValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x.Role).Must(Supported).When(x => !string.IsNullOrWhiteSpace(x.Role));
    }

    internal static bool Supported(string? role) => role is not null && (role.Equals(RoleNames.User, StringComparison.OrdinalIgnoreCase) || role.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase));
}
public sealed class RoleValidator : AbstractValidator<string>
{
    public RoleValidator() => RuleFor(x => x).NotEmpty().MaximumLength(50).Must(SearchUsersValidator.Supported);
}

public sealed class SearchUsersQueryHandler(IIdentityAdministrationService service, SearchUsersValidator validator) : IRequestHandler<SearchUsersQuery, Result<UserSearchResponse>>
{
    public async Task<Result<UserSearchResponse>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        if (!(await validator.ValidateAsync(request, cancellationToken)).IsValid)
        {
            return Fail<UserSearchResponse>(UserErrors.InvalidRoleCode, "The user search filters are invalid.", ErrorType.Validation);
        }

        AdminUserSearchData data = await service.SearchAsync(request.Search, Normalize(request.Role), request.Status, request.Page, request.PageSize, cancellationToken);
        var items = data.Items.Select(Map).ToArray();
        return Result.Success(new UserSearchResponse(items, request.Page, request.PageSize, data.TotalCount, data.TotalCount == 0 ? 0 : (int)Math.Ceiling(data.TotalCount / (double)request.PageSize)));
    }
    internal static UserSummaryResponse Map(AdminUserData x) => new(x.Id, x.Email, x.Status, x.Roles, x.CreatedAtUtc);
    internal static string? Normalize(string? role) => string.IsNullOrWhiteSpace(role) ? null : role.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase) ? RoleNames.Admin : RoleNames.User;
    internal static Result<T> Fail<T>(string code, string text, ErrorType type) => Result.Failure<T>(new Error(code, text, type));
}
public sealed class GetAdminUserQueryHandler(IIdentityAdministrationService service) : IRequestHandler<GetAdminUserQuery, Result<UserSummaryResponse>>
{
    public async Task<Result<UserSummaryResponse>> Handle(GetAdminUserQuery request, CancellationToken cancellationToken)
    {
        var user = await service.GetAsync(request.UserId, cancellationToken);
        return user is null
            ? SearchUsersQueryHandler.Fail<UserSummaryResponse>(UserErrors.UserNotFoundCode, UserErrors.UserNotFoundDescription, ErrorType.NotFound)
            : Result.Success(SearchUsersQueryHandler.Map(user));
    }
}

public sealed partial class UserAdministrationCommandHandler(IIdentityAdministrationService service, ICurrentUser currentUser, RoleValidator roleValidator, TimeProvider clock, UserAdministrationDomainEventHandler domainEventHandler, ILogger<UserAdministrationCommandHandler> logger) :
    IRequestHandler<AssignRoleCommand, Result>, IRequestHandler<RemoveRoleCommand, Result>, IRequestHandler<DisableUserCommand, Result>, IRequestHandler<EnableUserCommand, Result>
{
    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        if (!(await roleValidator.ValidateAsync(request.Role, cancellationToken)).IsValid)
        {
            return BadRole();
        }

        string role = SearchUsersQueryHandler.Normalize(request.Role)!;
        var result = await service.AssignRoleAsync(request.UserId, role, cancellationToken);
        if (result.IsFailure)
        {
            Rejected(logger, currentUser.UserId, request.UserId, role, result.Error!.Code);
            return Result.Failure(result.Error!);
        }

        if (result.Value)
        {
            await PublishDomainEventAsync("assigned", request.UserId, role, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        if (!(await roleValidator.ValidateAsync(request.Role, cancellationToken)).IsValid)
        {
            return BadRole();
        }

        string role = SearchUsersQueryHandler.Normalize(request.Role)!;
        var result = await service.RemoveRoleAsync(currentUser.UserId!.Value, request.UserId, role, cancellationToken);
        if (result.IsFailure)
        {
            Rejected(logger, currentUser.UserId, request.UserId, role, result.Error!.Code);
            return Result.Failure(result.Error!);
        }

        if (result.Value)
        {
            await PublishDomainEventAsync("removed", request.UserId, role, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> Handle(DisableUserCommand request, CancellationToken cancellationToken)
    {
        var result = await service.DisableAsync(currentUser.UserId!.Value, request.UserId, clock.GetUtcNow(), cancellationToken);
        if (result.IsFailure)
        {
            Rejected(logger, currentUser.UserId, request.UserId, null, result.Error!.Code);
            return Result.Failure(result.Error!);
        }

        if (result.Value)
        {
            await PublishDomainEventAsync("disabled", request.UserId, null, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> Handle(EnableUserCommand request, CancellationToken cancellationToken)
    {
        var result = await service.EnableAsync(request.UserId, cancellationToken);
        if (result.IsFailure)
        {
            Rejected(logger, currentUser.UserId, request.UserId, null, result.Error!.Code);
            return Result.Failure(result.Error!);
        }

        if (result.Value)
        {
            await PublishDomainEventAsync("enabled", request.UserId, null, cancellationToken);
        }

        return Result.Success();
    }

    private async Task PublishDomainEventAsync(string action, Guid id, string? role, CancellationToken cancellationToken)
    {
        AdminUserData? data = await service.GetAsync(id, cancellationToken);
        if (data is null)
        {
            throw new InvalidOperationException($"User '{id}' disappeared after an Identity administration operation.");
        }

        DateTimeOffset now = clock.GetUtcNow();
        User user = User.Rehydrate(id, data.Email);
        switch (action)
        {
            case "assigned": user.RecordRoleAssigned(role!, now); break;
            case "removed": user.RecordRoleRemoved(role!, now); break;
            case "disabled": user.RecordDisabled(now); break;
            default: user.RecordEnabled(now); break;
        }

        await domainEventHandler.HandleAsync(user.DomainEvents.Single(), cancellationToken);
        user.ClearDomainEvents();
        Changed(logger, action, currentUser.UserId, id, role);
    }
    private static Result BadRole() => Result.Failure(new Error(UserErrors.InvalidRoleCode, "The role is not supported.", ErrorType.Validation));
    [LoggerMessage(4101, LogLevel.Information, "Identity Admin {Action}; AdminUserId={AdminUserId}, TargetUserId={TargetUserId}, Role={Role}")] static partial void Changed(ILogger logger, string action, Guid? adminUserId, Guid targetUserId, string? role);
    [LoggerMessage(4102, LogLevel.Warning, "Identity Admin operation rejected; AdminUserId={AdminUserId}, TargetUserId={TargetUserId}, Role={Role}, ErrorCode={ErrorCode}")] static partial void Rejected(ILogger logger, Guid? adminUserId, Guid targetUserId, string? role, string errorCode);
}
