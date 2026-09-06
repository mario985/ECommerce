using ECommerce.Modules.Identity.Domain.RefreshTokens;
using ECommerce.Modules.Identity.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(refreshToken => refreshToken.Id);

        builder.Property(refreshToken => refreshToken.Token)
            .HasMaxLength(512)
            .IsRequired();
        builder.Property(refreshToken => refreshToken.ReplacedByToken)
            .HasMaxLength(512);
        builder.Property(refreshToken => refreshToken.ExpiresAtUtc).IsRequired();
        builder.Property(refreshToken => refreshToken.CreatedAtUtc).IsRequired();

        builder.HasIndex(refreshToken => refreshToken.Token).IsUnique();
        builder.HasIndex(refreshToken => refreshToken.UserId);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(refreshToken => refreshToken.DomainEvents);
    }
}
