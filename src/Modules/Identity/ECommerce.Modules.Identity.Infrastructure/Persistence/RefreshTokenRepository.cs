using ECommerce.Modules.Identity.Application.Abstractions;
using ECommerce.Modules.Identity.Domain.RefreshTokens;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Identity.Infrastructure.Persistence;

internal sealed class RefreshTokenRepository(IdentityDbContext dbContext)
    : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken)
    {
        return dbContext.RefreshTokens.SingleOrDefaultAsync(
            refreshToken => refreshToken.Token == token,
            cancellationToken);
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken)
    {
        await dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
