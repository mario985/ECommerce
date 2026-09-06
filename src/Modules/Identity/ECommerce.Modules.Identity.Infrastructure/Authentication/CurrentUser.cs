using System.IdentityModel.Tokens.Jwt;
using ECommerce.Common.Application.Authentication;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Modules.Identity.Infrastructure.Authentication;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            string? subject = httpContextAccessor.HttpContext?.User
                .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(subject, out Guid userId) ? userId : null;
        }
    }

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public string? Email => httpContextAccessor.HttpContext?.User
        .FindFirst(JwtRegisteredClaimNames.Email)?.Value;

    public IReadOnlyCollection<string> Roles => httpContextAccessor.HttpContext?.User
        .FindAll("role")
        .Select(claim => claim.Value)
        .ToArray() ?? [];
}
