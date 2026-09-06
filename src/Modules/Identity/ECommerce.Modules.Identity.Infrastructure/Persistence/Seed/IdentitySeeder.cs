using ECommerce.Modules.Identity.Domain.Roles;
using ECommerce.Modules.Identity.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ECommerce.Modules.Identity.Infrastructure.Persistence.Seed;

internal sealed class IdentitySeeder(
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IHostEnvironment environment)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        foreach (string roleName in new[] { RoleNames.User, RoleNames.Admin })
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await roleManager.RoleExistsAsync(roleName))
            {
                continue;
            }

            IdentityResult result = await roleManager.CreateAsync(
                new IdentityRole<Guid>(roleName));

            if (!result.Succeeded)
            {
                string errors = string.Join(
                    " ",
                    result.Errors.Select(error => error.Description));
                throw new InvalidOperationException(
                    $"Could not seed Identity role '{roleName}': {errors}");
            }
        }

        await SeedDevelopmentAdminAsync(cancellationToken);
    }

    private async Task SeedDevelopmentAdminAsync(CancellationToken cancellationToken)
    {
        IConfigurationSection section = configuration.GetSection("Identity:SeedAdmin");
        if (!environment.IsDevelopment() || !section.GetValue<bool>("Enabled"))
        {
            return;
        }

        string email = section["Email"]
            ?? throw new InvalidOperationException(
                "Identity:SeedAdmin:Email is required when development Admin seeding is enabled.");
        string password = section["Password"]
            ?? throw new InvalidOperationException(
                "Identity:SeedAdmin:Password is required when development Admin seeding is enabled.");
        ApplicationUser? user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email,
                CreatedAtUtc = DateTimeOffset.UtcNow,
            };
            IdentityResult created = await userManager.CreateAsync(user, password);
            EnsureSucceeded(created, "create the development Admin user");
        }

        if (!await userManager.IsInRoleAsync(user, RoleNames.Admin))
        {
            IdentityResult assigned = await userManager.AddToRoleAsync(user, RoleNames.Admin);
            EnsureSucceeded(assigned, "assign the development Admin role");
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    private static void EnsureSucceeded(IdentityResult result, string operation)
    {
        if (result.Succeeded)
        {
            return;
        }

        string errors = string.Join(" ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"Could not {operation}: {errors}");
    }
}
