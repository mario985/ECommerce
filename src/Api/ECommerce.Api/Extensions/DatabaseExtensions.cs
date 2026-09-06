using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ECommerce.Api.Extensions;

internal static class DatabaseExtensions
{
    private static readonly string[] DatabaseNames =
    [
        "Identity",
        "Inventory",
        "Cart",
        "Ordering",
        "Payments",
        "Wishlist"
    ];

    public static void UseRepositoryDataDirectory(
        this IConfiguration configuration,
        IHostEnvironment environment)
    {
        string dataDirectory = FindDataDirectory(environment.ContentRootPath);
        Directory.CreateDirectory(dataDirectory);

        foreach (string databaseName in DatabaseNames)
        {
            string? connectionString = configuration.GetConnectionString(databaseName);
            const string prefix = "Data Source=";

            if (connectionString is null ||
                !connectionString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string source = connectionString[prefix.Length..].Trim();
            if (Path.IsPathRooted(source))
            {
                continue;
            }

            string databasePath = Path.Combine(dataDirectory, Path.GetFileName(source));
            configuration[$"ConnectionStrings:{databaseName}"] = $"{prefix}{databasePath}";
        }
    }

    private static string FindDataDirectory(string contentRootPath)
    {
        foreach (string startPath in new[] { contentRootPath, AppContext.BaseDirectory })
        {
            DirectoryInfo? directory = new(startPath);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ECommerce.sln")))
                {
                    return Path.Combine(directory.FullName, "data");
                }

                directory = directory.Parent;
            }
        }

        return Path.Combine(contentRootPath, "data");
    }
}
