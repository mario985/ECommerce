using ECommerce.Common.Application.Errors;
using ECommerce.Common.Presentation.Results;
using ECommerce.Modules.Identity.Application.Users.GetCurrentUser;
using ECommerce.Modules.Identity.Domain.Roles;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ECommerce.Modules.Identity.Presentation.Users;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/me", GetCurrentUserAsync).WithName("Identity.GetCurrentUser").WithTags("Identity").RequireAuthorization();
        group.MapGet("/admin/check", () => Results.Ok(new { Status = "authorized" }))
            .RequireAuthorization(policy => policy.RequireRole(RoleNames.Admin));
        return group;
    }

    private static async Task<IResult> GetCurrentUserAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        Result<CurrentUserResponse> result = await sender.Send(
            new GetCurrentUserQuery(),
            cancellationToken);

        return result.IsFailure
            ? ApiResults.Problem(result.Error!)
            : Results.Ok(result.Value);
    }
}
