using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Identity.Application.Abstractions;
using ECommerce.Modules.Identity.Domain.Roles;
using ECommerce.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Modules.Identity.Infrastructure.Identity;

internal sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager)
    : IIdentityService
{
    public async Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return DuplicateEmail();
        }

        ApplicationUser applicationUser = new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = email,
            CreatedAtUtc = DateTimeOffset.UtcNow,
        };

        IdentityResult creationResult = await userManager.CreateAsync(applicationUser, password);
        if (!creationResult.Succeeded)
        {
            return MapCreationFailure(creationResult);
        }

        IdentityResult roleResult = await userManager.AddToRoleAsync(applicationUser, RoleNames.User);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(applicationUser);
            return IdentityFailure<Guid>("Identity.RoleAssignmentFailed", roleResult);
        }

        return Result.Success(applicationUser.Id);
    }

    public async Task<Result<AuthenticatedUser>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? applicationUser = await userManager.FindByEmailAsync(email);
        if (applicationUser is null || applicationUser.IsDisabled)
        {
            return InvalidCredentials();
        }

        SignInResult signInResult = await signInManager.CheckPasswordSignInAsync(
            applicationUser,
            password,
            lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            return InvalidCredentials();
        }

        return Result.Success(new AuthenticatedUser(
            applicationUser.Id,
            applicationUser.Email ?? email));
    }

    public async Task<IReadOnlyCollection<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser applicationUser = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException($"Authenticated user '{userId}' no longer exists.");

        IList<string> roles = await userManager.GetRolesAsync(applicationUser);
        return roles.ToArray();
    }

    public async Task<Result<AuthenticatedUser>> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? applicationUser = await userManager.FindByIdAsync(userId.ToString());
        if (applicationUser is null || applicationUser.IsDisabled)
        {
            return Result.Failure<AuthenticatedUser>(new Error(
                UserErrors.UserNotFoundCode,
                UserErrors.UserNotFoundDescription,
                ErrorType.NotFound));
        }

        return Result.Success(new AuthenticatedUser(
            applicationUser.Id,
            applicationUser.Email ?? applicationUser.UserName ?? string.Empty));
    }

    private static Result<Guid> MapCreationFailure(IdentityResult identityResult)
    {
        if (identityResult.Errors.Any(error =>
                error.Code is "DuplicateEmail" or "DuplicateUserName"))
        {
            return DuplicateEmail();
        }

        return IdentityFailure<Guid>("Identity.InvalidRegistration", identityResult);
    }

    private static Result<Guid> DuplicateEmail()
    {
        return Result.Failure<Guid>(new Error(
            UserErrors.DuplicateEmailCode,
            UserErrors.DuplicateEmailDescription,
            ErrorType.Conflict));
    }

    private static Result<AuthenticatedUser> InvalidCredentials()
    {
        return Result.Failure<AuthenticatedUser>(new Error(
            UserErrors.InvalidCredentialsCode,
            UserErrors.InvalidCredentialsDescription,
            ErrorType.Unauthorized));
    }

    private static Result<TValue> IdentityFailure<TValue>(
        string code,
        IdentityResult identityResult)
    {
        string description = string.Join(
            " ",
            identityResult.Errors.Select(error => error.Description));

        return Result.Failure<TValue>(new Error(code, description, ErrorType.Validation));
    }
}
