using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Identity.Domain.Users;
namespace ECommerce.Modules.Identity.Application.Abstractions;
public sealed record AdminUserData(Guid Id, string Email, UserStatus Status, IReadOnlyCollection<string> Roles, DateTimeOffset CreatedAtUtc);
public sealed record AdminUserSearchData(IReadOnlyCollection<AdminUserData> Items, long TotalCount);
public interface IIdentityAdministrationService
{
    Task<AdminUserSearchData> SearchAsync(string? search, string? role, UserStatus? status, int page, int pageSize, CancellationToken cancellationToken);
    Task<AdminUserData?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<bool>> AssignRoleAsync(Guid id, string role, CancellationToken cancellationToken);
    Task<Result<bool>> RemoveRoleAsync(Guid actorId, Guid id, string role, CancellationToken cancellationToken);
    Task<Result<bool>> DisableAsync(Guid actorId, Guid id, DateTimeOffset utcNow, CancellationToken cancellationToken);
    Task<Result<bool>> EnableAsync(Guid id, CancellationToken cancellationToken);
}
