using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ECommerce.Modules.Catalog.Application.Caching;

public static class CatalogCacheKeys
{
    public const string ActiveCategories = "catalog:categories:active";
    public const string SearchVersion = "catalog:search:version";

    public static string Product(Guid productId) =>
        $"catalog:product:{productId:D}";

    public static string ProductContract(Guid productId) =>
        $"catalog:product-contract:{productId:D}";

    public static string Search(
        string version,
        string? search,
        string? sku,
        bool? isActive,
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        bool? inStock,
        double? minimumRating,
        string? sort,
        bool includeSkuAndCategoryInTextSearch,
        int page,
        int pageSize)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        string normalized = string.Join('|',
            NormalizeSearch(search),
            NormalizeSku(sku),
            isActive?.ToString().ToLowerInvariant() ?? string.Empty,
            categoryId?.ToString("D") ?? string.Empty,
            minPrice?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            maxPrice?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            inStock?.ToString().ToLowerInvariant() ?? string.Empty,
            minimumRating?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            NormalizeSearch(sort ?? "newest"),
            includeSkuAndCategoryInTextSearch.ToString().ToLowerInvariant(),
            page.ToString(CultureInfo.InvariantCulture),
            pageSize.ToString(CultureInfo.InvariantCulture));
        string hash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(normalized))).ToLowerInvariant();
        return $"catalog:search:{version}:{hash}";
    }

    public static string Search(
        string version,
        string? search,
        string? sku,
        bool? isActive,
        int page,
        int pageSize) => Search(
            version,
            search,
            sku,
            isActive,
            categoryId: null,
            minPrice: null,
            maxPrice: null,
            inStock: null,
            minimumRating: null,
            sort: "name",
            includeSkuAndCategoryInTextSearch: false,
            page,
            pageSize);

    private static string NormalizeSearch(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();

    private static string NormalizeSku(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
}
