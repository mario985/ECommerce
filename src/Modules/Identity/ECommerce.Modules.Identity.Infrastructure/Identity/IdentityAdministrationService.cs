using ECommerce.Common.Application.Errors;
using ECommerce.Modules.Identity.Application.Abstractions;
using ECommerce.Modules.Identity.Domain.RefreshTokens;
using ECommerce.Modules.Identity.Domain.Roles;
using ECommerce.Modules.Identity.Domain.Users;
using ECommerce.Modules.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Identity.Infrastructure.Identity;

internal sealed class IdentityAdministrationService(IdentityDbContext db, UserManager<ApplicationUser> users) : IIdentityAdministrationService
{
    public async Task<AdminUserSearchData> SearchAsync(string? search, string? role, UserStatus? status, int page, int pageSize, CancellationToken ct)
    {
        IQueryable<ApplicationUser> q = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search)) { string term = search.Trim(); q = q.Where(x => x.Email != null && EF.Functions.Like(x.Email, $"%{term}%")); }
        if (status.HasValue) q = status == UserStatus.Disabled ? q.Where(x => x.IsDisabled) : q.Where(x => !x.IsDisabled);
        if (role is not null) q = q.Where(x => db.UserRoles.Any(ur => ur.UserId == x.Id && db.Roles.Any(r => r.Id == ur.RoleId && r.Name == role)));
        long count = await q.LongCountAsync(ct);
        ApplicationUser[] pageUsers = await q.OrderBy(x => x.Email).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToArrayAsync(ct);
        Guid[] ids = pageUsers.Select(x => x.Id).ToArray();
        var roleRows = await (from ur in db.UserRoles where ids.Contains(ur.UserId) join r in db.Roles on ur.RoleId equals r.Id select new { ur.UserId, r.Name }).ToArrayAsync(ct);
        return new(pageUsers.Select(x => Map(x, roleRows.Where(y => y.UserId == x.Id).Select(y => y.Name!).ToArray())).ToArray(), count);
    }
    public async Task<AdminUserData?> GetAsync(Guid id, CancellationToken ct) { ApplicationUser? u = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct); if (u is null) return null; string[] roles = await (from ur in db.UserRoles where ur.UserId == id join r in db.Roles on ur.RoleId equals r.Id select r.Name!).ToArrayAsync(ct); return Map(u, roles); }
    public async Task<Result<bool>> AssignRoleAsync(Guid id, string role, CancellationToken ct) { ApplicationUser? u = await users.FindByIdAsync(id.ToString()); if (u is null) return Fail(UserErrors.UserNotFoundCode, UserErrors.UserNotFoundDescription, ErrorType.NotFound); if (!await db.Roles.AnyAsync(x => x.Name == role, ct)) return Fail(UserErrors.RoleNotFoundCode, "The role was not found.", ErrorType.NotFound); if (await users.IsInRoleAsync(u, role)) return Result.Success(false); IdentityResult result = await users.AddToRoleAsync(u, role); return result.Succeeded ? Result.Success(true) : Fail(UserErrors.InvalidRoleCode, "The role could not be assigned.", ErrorType.Validation); }
    public async Task<Result<bool>> RemoveRoleAsync(Guid actorId, Guid id, string role, CancellationToken ct) { await using var tx = await db.Database.BeginTransactionAsync(ct); ApplicationUser? u = await users.FindByIdAsync(id.ToString()); if (u is null) return Fail(UserErrors.UserNotFoundCode, UserErrors.UserNotFoundDescription, ErrorType.NotFound); if (!await users.IsInRoleAsync(u, role)) return Result.Success(false); if (role == RoleNames.Admin && actorId == id) return Fail(UserErrors.CannotRemoveOwnAdminRoleCode, "An administrator cannot remove their own Admin role.", ErrorType.Conflict); IList<string> existing = await users.GetRolesAsync(u); if (!u.IsDisabled && existing.Count == 1) return Fail(UserErrors.CannotRemoveLastRoleCode, "An enabled user must retain an application role.", ErrorType.Conflict); if (role == RoleNames.Admin && !u.IsDisabled && await ActiveAdminCount(ct) <= 1) return Fail(UserErrors.CannotRemoveFinalAdminCode, "At least one active administrator must remain.", ErrorType.Conflict); IdentityResult result = await users.RemoveFromRoleAsync(u, role); if (!result.Succeeded) return Fail(UserErrors.InvalidRoleCode, "The role could not be removed.", ErrorType.Validation); await tx.CommitAsync(ct); return Result.Success(true); }
    public async Task<Result<bool>> DisableAsync(Guid actorId, Guid id, DateTimeOffset now, CancellationToken ct) { if (actorId == id) return Fail(UserErrors.CannotDisableSelfCode, "An administrator cannot disable their own account.", ErrorType.Conflict); await using var tx = await db.Database.BeginTransactionAsync(ct); ApplicationUser? u = await db.Users.SingleOrDefaultAsync(x => x.Id == id, ct); if (u is null) return Fail(UserErrors.UserNotFoundCode, UserErrors.UserNotFoundDescription, ErrorType.NotFound); if (u.IsDisabled) return Result.Success(false); bool admin = await db.UserRoles.AnyAsync(ur => ur.UserId == id && db.Roles.Any(r => r.Id == ur.RoleId && r.Name == RoleNames.Admin), ct); if (admin && await ActiveAdminCount(ct) <= 1) return Fail(UserErrors.CannotRemoveFinalAdminCode, "At least one active administrator must remain.", ErrorType.Conflict); u.IsDisabled = true; u.DisabledAtUtc = now; RefreshToken[] tokens = await db.RefreshTokens.Where(x => x.UserId == id && x.RevokedAtUtc == null).ToArrayAsync(ct); foreach (var token in tokens) if (token.GetStatus(now) == RefreshTokenStatus.Active) token.Revoke(now); await db.SaveChangesAsync(ct); await tx.CommitAsync(ct); return Result.Success(true); }
    public async Task<Result<bool>> EnableAsync(Guid id, CancellationToken ct) { ApplicationUser? u = await db.Users.SingleOrDefaultAsync(x => x.Id == id, ct); if (u is null) return Fail(UserErrors.UserNotFoundCode, UserErrors.UserNotFoundDescription, ErrorType.NotFound); if (!u.IsDisabled) return Result.Success(false); u.IsDisabled = false; u.DisabledAtUtc = null; await db.SaveChangesAsync(ct); return Result.Success(true); }
    private Task<int> ActiveAdminCount(CancellationToken ct) => (from u in db.Users where !u.IsDisabled join ur in db.UserRoles on u.Id equals ur.UserId join r in db.Roles on ur.RoleId equals r.Id where r.Name == RoleNames.Admin select u.Id).Distinct().CountAsync(ct);
    private static AdminUserData Map(ApplicationUser u, IReadOnlyCollection<string> roles) => new(u.Id, u.Email ?? u.UserName ?? string.Empty, u.IsDisabled ? UserStatus.Disabled : UserStatus.Active, roles, u.CreatedAtUtc);
    private static Result<bool> Fail(string code, string text, ErrorType type) => Result.Failure<bool>(new Error(code, text, type));
}
