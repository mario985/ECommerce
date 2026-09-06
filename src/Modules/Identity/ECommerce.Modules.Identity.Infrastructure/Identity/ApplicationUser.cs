using Microsoft.AspNetCore.Identity;

namespace ECommerce.Modules.Identity.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public bool IsDisabled { get; set; }
    public DateTimeOffset? DisabledAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
