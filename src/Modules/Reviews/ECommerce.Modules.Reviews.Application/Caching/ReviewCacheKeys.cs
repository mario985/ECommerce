using System.Globalization;

namespace ECommerce.Modules.Reviews.Application.Caching;

public static class ReviewCacheKeys
{
    public static string ProductVersion(Guid productId) => $"reviews:product:{productId:D}:version";
    public static string ProductReviews(Guid productId, string version, int page, int pageSize) =>
        $"reviews:product:{productId:D}:{version}:{page.ToString(CultureInfo.InvariantCulture)}:{pageSize.ToString(CultureInfo.InvariantCulture)}";
}
