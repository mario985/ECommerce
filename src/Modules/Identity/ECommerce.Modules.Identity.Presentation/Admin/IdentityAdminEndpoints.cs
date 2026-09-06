using ECommerce.Common.Application.Authentication;
using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation.Results;
using ECommerce.Modules.Identity.Application.Admin;
using ECommerce.Modules.Identity.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Modules.Identity.Presentation.Admin;
public sealed record AssignRoleRequest(string Role);
public static class IdentityAdminEndpoints
{
    public static RouteGroupBuilder MapIdentityAdminEndpoints(this RouteGroupBuilder group)
    {
        group.RequireAuthorization(AuthorizationPolicyNames.AdminOnly);
        group.MapGet("/users", Search).WithName("IdentityAdmin.SearchUsers").WithTags("Identity Admin");
        group.MapGet("/users/{userId:guid}", Get).WithName("IdentityAdmin.GetUser").WithTags("Identity Admin");
        group.MapPost("/users/{userId:guid}/roles", Assign).WithName("IdentityAdmin.AssignRole").WithTags("Identity Admin");
        group.MapDelete("/users/{userId:guid}/roles/{role}", Remove).WithName("IdentityAdmin.RemoveRole").WithTags("Identity Admin");
        group.MapPost("/users/{userId:guid}/disable", Disable).WithName("IdentityAdmin.DisableUser").WithTags("Identity Admin");
        group.MapPost("/users/{userId:guid}/enable", Enable).WithName("IdentityAdmin.EnableUser").WithTags("Identity Admin");
        return group;
    }
    private static async Task<IResult> Search(ISender s, CancellationToken ct, string? search = null, string? role = null, UserStatus? status = null, int page = 1, int pageSize = 20) => Map(await s.Send(new SearchUsersQuery(search, role, status, page, pageSize), ct));
    private static async Task<IResult> Get(Guid userId, ISender s, CancellationToken ct) => Map(await s.Send(new GetAdminUserQuery(userId), ct));
    private static async Task<IResult> Assign(Guid userId, AssignRoleRequest request, ISender s, CancellationToken ct) => MapNoContent(await s.Send(new AssignRoleCommand(userId, request.Role), ct));
    private static async Task<IResult> Remove(Guid userId, string role, ISender s, CancellationToken ct) => MapNoContent(await s.Send(new RemoveRoleCommand(userId, role), ct));
    private static async Task<IResult> Disable(Guid userId, ISender s, CancellationToken ct) => MapNoContent(await s.Send(new DisableUserCommand(userId), ct));
    private static async Task<IResult> Enable(Guid userId, ISender s, CancellationToken ct) => MapNoContent(await s.Send(new EnableUserCommand(userId), ct));
    private static IResult Map<T>(Result<T> r) => r.IsFailure ? ApiResults.Problem(r.Error!) : Results.Ok(r.Value);
    private static IResult MapNoContent(Result r) => r.IsFailure ? ApiResults.Problem(r.Error!) : Results.NoContent();
}
