namespace ECommerce.Common.Application.Caching;

public sealed record CacheEntryOptions(TimeSpan? AbsoluteExpirationRelativeToNow = null);
