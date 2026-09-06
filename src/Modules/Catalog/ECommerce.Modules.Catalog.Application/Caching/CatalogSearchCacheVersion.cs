using ECommerce.Common.Application.Caching;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Catalog.Application.Caching;

public sealed partial class CatalogSearchCacheVersion(
    ICacheService cacheService,
    ILogger<CatalogSearchCacheVersion> logger)
{
    public async Task<string> GetAsync(CancellationToken cancellationToken)
    {
        string? version = await cacheService.GetAsync<string>(
            CatalogCacheKeys.SearchVersion, cancellationToken);
        if (Guid.TryParseExact(version, "N", out _))
        {
            return version;
        }

        return await ChangeAsync(cancellationToken);
    }

    public async Task<string> ChangeAsync(CancellationToken cancellationToken)
    {
        string version = Guid.NewGuid().ToString("N");
        await cacheService.SetAsync(
            CatalogCacheKeys.SearchVersion,
            version,
            new CacheEntryOptions(),
            cancellationToken);
        VersionChanged(logger, version);
        return version;
    }

    [LoggerMessage(8101, LogLevel.Information,
        "Catalog search cache version changed to {SearchVersion}")]
    private static partial void VersionChanged(ILogger logger, string searchVersion);
}
