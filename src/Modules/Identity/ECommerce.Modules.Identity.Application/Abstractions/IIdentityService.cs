using ECommerce.Common.Application.Errors;

namespace ECommerce.Modules.Identity.Application.Abstractions;

public interface IIdentityService
{
    Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<Result<AuthenticatedUser>> ValidateCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task<Result<AuthenticatedUser>> GetUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> GetRolesAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
