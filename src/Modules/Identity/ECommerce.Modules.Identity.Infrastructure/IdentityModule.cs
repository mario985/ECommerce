using System.Text;
using ECommerce.Common.Application.Authentication;
using ECommerce.Modules.Identity.Application.Abstractions;
using ECommerce.Modules.Identity.Application.Authentication.Login;
using ECommerce.Modules.Identity.Application.Authentication.Logout;
using ECommerce.Modules.Identity.Application.Authentication.RefreshToken;
using ECommerce.Modules.Identity.Application.Authentication.Register;
using ECommerce.Modules.Identity.Application.Admin;
using ECommerce.Modules.Identity.Infrastructure.Authentication;
using ECommerce.Modules.Identity.Infrastructure.Identity;
using ECommerce.Modules.Identity.Infrastructure.Persistence;
using ECommerce.Modules.Identity.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;

namespace ECommerce.Modules.Identity.Infrastructure;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Identity")
            ?? throw new InvalidOperationException("Connection string 'Identity' is required.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddHealthChecks().AddDbContextCheck<IdentityDbContext>(
            "identity-sqlite",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready", "database"]);

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<IdentityDbContext>();

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => options.Key.Length >= 32, "Jwt:Key must contain at least 32 characters.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
            .Validate(options => options.ExpirationMinutes > 0, "Jwt:ExpirationMinutes must be positive.")
            .Validate(
                options => options.RefreshTokenExpirationDays > 0,
                "Jwt:RefreshTokenExpirationDays must be positive.")
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<Microsoft.Extensions.Options.IOptions<JwtOptions>>((options, jwtOptions) =>
            {
                JwtOptions jwt = jwtOptions.Value;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = "email",
                    RoleClaimType = "role",
                };
            });
        services.AddAuthorization(options =>
            options.AddPolicy(
                AuthorizationPolicyNames.AdminOnly,
                policy => policy.RequireRole(ECommerce.Modules.Identity.Domain.Roles.RoleNames.Admin)));
        services.AddHttpContextAccessor();

        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IIdentityAdministrationService, IdentityAdministrationService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<RefreshTokenGenerator>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IdentitySeeder>();
        services.AddHostedService<IdentityDbInitializer>();

        services.AddScoped<RegisterUserValidator>();
        services.AddScoped<LoginUserValidator>();
        services.AddScoped<RefreshTokenValidator>();
        services.AddScoped<LogoutValidator>();
        services.AddValidatorsFromAssemblyContaining<RegisterUserCommandHandler>();
        services.AddScoped<SearchUsersValidator>();
        services.AddScoped<RoleValidator>();
        services.AddScoped<UserAdministrationDomainEventHandler>();
        services.AddScoped<UserRegisteredDomainEventHandler>();
        services.AddScoped<UserLoggedInDomainEventHandler>();
        services.AddScoped<RefreshTokenIssuedDomainEventHandler>();
        services.AddScoped<RefreshTokenRevokedDomainEventHandler>();

        services.AddMediatR(mediatRConfiguration =>
        {
            mediatRConfiguration.RegisterServicesFromAssembly(
                typeof(RegisterUserCommandHandler).Assembly);
        });

        return services;
    }
}
