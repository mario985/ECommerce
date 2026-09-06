using ECommerce.Modules.Identity.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(user => user.CreatedAtUtc).IsRequired();
        builder.Property(user => user.IsDisabled).IsRequired().HasDefaultValue(false);
        builder.Property(user => user.DisabledAtUtc);
    }
}
